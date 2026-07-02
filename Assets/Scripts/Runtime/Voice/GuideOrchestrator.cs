using System.Collections.Concurrent;
using System.Threading.Tasks;
using Museum.Artifact;
using Museum.Config;
using Museum.Session;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Museum.Voice
{
    /// <summary>
    /// Top-level Realtime API tour guide. Owns the WebSocket session, drains events on the main
    /// thread, configures the session (system prompt with visitor name, ServerVAD), and pushes
    /// per-artifact context whenever the gaze detector identifies one.
    /// </summary>
    public class GuideOrchestrator : MonoBehaviour
    {
        [Header("Wiring")]
        public GazeArtifactDetector gazeDetector;

        [Header("Session")]
        [Tooltip("Realtime model. Default is gpt-4o-mini-realtime-preview — cheap and widely available.\nAlternatives:\n  gpt-realtime  (GA, higher quality, more expensive)\n  gpt-4o-realtime-preview  (older preview alias)\n  gpt-4o-realtime-preview-2024-12-17  (specific snapshot)")]
        public string model = "gpt-4o-mini-realtime-preview";

        [Tooltip("OpenAI voice. Wide options: alloy, ash, ballad, coral, echo, sage, shimmer, verse, marin, cedar.")]
        public string voice = "alloy";

        [Tooltip("System instructions appended to the visitor-aware preamble.")]
        [TextArea(4, 12)]
        public string baseInstructions =
            "Always respond in English regardless of the visitor's name, country of origin, or any other context. " +
            "Do not switch languages even if the visitor's name or country might suggest a different language. " +
            "You are a knowledgeable, warm museum tour guide in an Egyptian museum. " +
            "Speak conversationally and concisely. Pause for the visitor's questions. " +
            "When the system tells you the visitor is looking at an artifact, give a 1-2 sentence " +
            "introduction (name, era, why it matters), then invite a follow-up. " +
            "If the visitor asks a question unrelated to the current artifact, answer briefly and steer " +
            "them gently back to the museum.";

        public enum InputMode
        {
            GazeOnly,        // mic never streams. Guide narrates only on gaze identification.
            PushToTalk,      // mic streams only while pttKey is held. Manual commit on release. No Server VAD.
            ServerVad        // mic streams continuously. OpenAI auto-detects speech. Most natural but echo-prone.
        }

        [Header("Behavior")]
        [Tooltip("How the visitor talks to the guide:\n  GazeOnly — guide only narrates artifacts you look at\n  PushToTalk — hold a key to ask questions (recommended)\n  ServerVad — mic always live (can fire on background noise / echo)")]
        public InputMode inputMode = InputMode.PushToTalk;

        [Tooltip("Hold this key to talk in PushToTalk mode. Default T (keyboard). VR also responds to either controller's primary button (Y/B) automatically — no Inspector setup needed.")]
        public Key pushToTalkKey = Key.T;

        [Tooltip("VAD speech-detection threshold. Higher = less sensitive (fewer false positives from background noise / echo).")]
        [Range(0.3f, 0.95f)] public float vadThreshold = 0.8f;

        [Tooltip("How long the player must be silent before VAD commits (ms). Higher = more tolerant of pauses mid-sentence.")]
        public int vadSilenceDurationMs = 800;

        [Tooltip("Mute mic input while the model is speaking, to prevent echo from re-triggering VAD. Eliminates 'guide cancels itself mid-sentence'.")]
        public bool muteMicWhileModelSpeaks = true;

        [Tooltip("After the model finishes, keep the mic muted for this extra time (seconds) so the audio buffer drains without echo.")]
        public float postSpeechMuteGrace = 0.4f;

        [Header("Debug")]
        [Tooltip("Log every Realtime API event to the console with timing. Verbose; turn off in production.")]
        public bool verboseLogging = true;

        [Tooltip("Log mic chunk send/skip stats every N seconds (0 = off).")]
        public float micStatsIntervalSeconds = 5f;

        RealtimeClient _client;
        MicCapture _mic;
        StreamingAudioPlayer _player;
        readonly ConcurrentQueue<JObject> _incoming = new ConcurrentQueue<JObject>();
        bool _sessionConfigured;
        bool _modelSpeaking;
        float _modelSpeakingUntil; // mic stays muted until Time.time exceeds this

        // Debug counters
        int _micChunksSent;
        int _micChunksSkippedMute;
        int _micChunksSkippedNoVad;
        int _audioDeltaCount;
        float _nextStatsLogTime;
        string _lastResponseSource = "unknown";
        int _responseCreatedCount;

        async void Start()
        {
            _mic = GetComponentInChildren<MicCapture>(true) ?? Object.FindAnyObjectByType<MicCapture>(FindObjectsInactive.Include);
            _player = GetComponentInChildren<StreamingAudioPlayer>(true) ?? Object.FindAnyObjectByType<StreamingAudioPlayer>(FindObjectsInactive.Include);
            if (gazeDetector == null) gazeDetector = Object.FindAnyObjectByType<GazeArtifactDetector>();
            if (_mic == null || _player == null)
            {
                Debug.LogError($"[GuideOrchestrator] Missing dependencies. mic={_mic} player={_player}. Disabling.");
                enabled = false;
                return;
            }

            var session = SessionState.GetOrCreate();
            if (string.IsNullOrEmpty(session.OpenAiApiKey))
            {
                if (OpenAIConfig.TryLoadKey(out var key, out var err))
                {
                    session.OpenAiApiKey = key;
                    Debug.Log("[GuideOrchestrator] Loaded OpenAI key directly from config.json (Lobby was skipped).");
                }
                else
                {
                    Debug.LogWarning("[GuideOrchestrator] " + err + "\nTour guide disabled.");
                    enabled = false;
                    return;
                }
            }

            if (gazeDetector != null)
                gazeDetector.OnArtifactIdentified += OnArtifactIdentified;

            _client = new RealtimeClient();
            _client.OnEvent += evt => _incoming.Enqueue(evt);
            _client.OnOpen += () => Debug.Log("[GuideOrchestrator] Realtime WebSocket open.");
            _client.OnClose += reason => Debug.LogWarning("[GuideOrchestrator] Realtime closed: " + reason);

            _mic.OnAudioChunkBase64 += SendMicChunk;
            _mic.OnError += msg => Debug.LogWarning("[GuideOrchestrator] Mic: " + msg);

            var trimmedModel = (model ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(trimmedModel))
            {
                Debug.LogError("[GuideOrchestrator] Model field is empty. Set it on the Tour Guide GameObject in Inspector.");
                enabled = false;
                return;
            }
            var url = $"wss://api.openai.com/v1/realtime?model={trimmedModel}";
            try { await _client.ConnectAsync(session.OpenAiApiKey, url); }
            catch (System.Exception e)
            {
                Debug.LogError($"[GuideOrchestrator] Realtime connect failed: {e.Message}");
                enabled = false;
                return;
            }

            ConfigureSession();
            if (inputMode == InputMode.GazeOnly)
                Debug.Log("[GuideOrchestrator] Mode: GazeOnly. Mic disabled. Guide narrates only on gaze.");
            else if (inputMode == InputMode.PushToTalk)
            {
                _mic.StartCapture();
                Debug.Log($"[GuideOrchestrator] Mode: PushToTalk. Hold '{pushToTalkKey}' to talk. Mic captures continuously but is only streamed while held.");
            }
            else
            {
                _mic.StartCapture();
                Debug.Log("[GuideOrchestrator] Mode: ServerVad. Mic is always live. Background noise can trigger spurious responses.");
            }
        }

        void OnDestroy()
        {
            if (gazeDetector != null) gazeDetector.OnArtifactIdentified -= OnArtifactIdentified;
            if (_mic != null) _mic.StopCapture();
            if (_client != null) Task.Run(() => _client.DisconnectAsync());
        }

        void Update()
        {
            while (_incoming.TryDequeue(out var evt))
                Handle(evt);

            HandlePushToTalkInput();

            if (micStatsIntervalSeconds > 0f && Time.time >= _nextStatsLogTime)
            {
                _nextStatsLogTime = Time.time + micStatsIntervalSeconds;
                Debug.Log($"[GuideOrchestrator stats] mode={inputMode} pttHeld={(inputMode == InputMode.PushToTalk && IsPushToTalkHeld())} sent={_micChunksSent} muted={_micChunksSkippedMute} gated={_micChunksSkippedNoVad} audioDeltas={_audioDeltaCount} responsesCreated={_responseCreatedCount} modelSpeaking={_modelSpeaking} bufferedSec={(_player != null ? _player.BufferedSeconds : 0f):F2}");
            }
        }

        void ConfigureSession()
        {
            if (_sessionConfigured) return;
            _sessionConfigured = true;

            var visitor = SessionState.Instance?.Visitor;
            string visitorPreamble = visitor != null
                ? $"The visitor's name is {visitor.Name}, age {visitor.Age}, from {visitor.CountryName}. Greet them by name on the first response."
                : "Greet the visitor warmly.";

            var instructions = visitorPreamble + "\n" + baseInstructions;

            var update = new JObject
            {
                ["type"] = "session.update",
                ["session"] = new JObject
                {
                    ["type"] = "realtime",
                    ["instructions"] = instructions,
                    ["output_modalities"] = new JArray("audio"),
                    ["audio"] = new JObject
                    {
                        ["input"] = new JObject
                        {
                            ["format"] = new JObject
                            {
                                ["type"] = "audio/pcm",
                                ["rate"] = 24000
                            },
                            ["turn_detection"] = (inputMode == InputMode.ServerVad)
                                ? new JObject
                                {
                                    ["type"] = "server_vad",
                                    ["threshold"] = vadThreshold,
                                    ["prefix_padding_ms"] = 300,
                                    ["silence_duration_ms"] = vadSilenceDurationMs
                                }
                                : null  // GazeOnly + PushToTalk both disable Server VAD; we control turn-taking manually
                        },
                        ["output"] = new JObject
                        {
                            ["format"] = new JObject
                            {
                                ["type"] = "audio/pcm",
                                ["rate"] = 24000
                            },
                            ["voice"] = voice
                        }
                    }
                }
            };
            _client.Send(update);

            _lastResponseSource = "OPENING_GREETING";
            _client.Send(new JObject
            {
                ["type"] = "response.create",
                ["response"] = new JObject
                {
                    ["instructions"] = "Speak in English only. Greet the visitor warmly and briefly invite them to look around."
                }
            });
        }

        void OnArtifactIdentified(ArtifactInfo artifact, RaycastHit hit)
        {
            if (_client == null || !_client.IsOpen) return;

            // Don't interrupt the guide mid-narration. The gaze detector's per-artifact 30s cooldown
            // means a brief glance at the same one later won't re-fire; a *different* artifact
            // simply waits for the current speech to finish.
            if (_modelSpeaking || Time.time < _modelSpeakingUntil)
            {
                Debug.Log($"[GuideOrchestrator] Skipped narration of '{artifact.displayName}' — guide is currently speaking.");
                return;
            }

            string context =
                $"The visitor is now looking at: {artifact.displayName}. " +
                $"Era: {artifact.era}. " +
                $"Background: {artifact.description} " +
                $"Give a short, evocative 1-2 sentence introduction now, then invite a question.";

            _client.Send(new JObject
            {
                ["type"] = "conversation.item.create",
                ["item"] = new JObject
                {
                    ["type"] = "message",
                    ["role"] = "system",
                    ["content"] = new JArray(
                        new JObject { ["type"] = "input_text", ["text"] = context }
                    )
                }
            });

            _lastResponseSource = $"GAZE:{artifact.displayName}";
            _client.Send(new JObject { ["type"] = "response.create" });
        }

        void SendMicChunk(string base64Pcm16)
        {
            if (_client == null || !_client.IsOpen) return;
            if (inputMode == InputMode.GazeOnly) { _micChunksSkippedNoVad++; return; }
            if (inputMode == InputMode.PushToTalk && !IsPushToTalkHeld()) { _micChunksSkippedNoVad++; return; }
            if (muteMicWhileModelSpeaks && (_modelSpeaking || Time.time < _modelSpeakingUntil)) { _micChunksSkippedMute++; return; }
            _micChunksSent++;
            _client.Send(new JObject
            {
                ["type"] = "input_audio_buffer.append",
                ["audio"] = base64Pcm16
            });
        }

        bool _pttWasHeld;
        readonly System.Collections.Generic.List<UnityEngine.XR.InputDevice> _xrDevices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();

        bool IsPushToTalkHeld()
        {
            // Keyboard fallback for desktop dev / Quest Link with hardware keyboard handy.
            if (Keyboard.current != null && Keyboard.current[pushToTalkKey].isPressed) return true;

            // VR: hold either controller's primary button (Y on left, B on right) to talk.
            // Cheaper than going through XRI's action map and means PTT works in any scene
            // without per-scene InputAction wiring.
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(
                UnityEngine.XR.InputDeviceCharacteristics.Controller, _xrDevices);
            for (int i = 0; i < _xrDevices.Count; i++)
            {
                if (_xrDevices[i].TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool primary) && primary)
                    return true;
            }
            return false;
        }

        void HandlePushToTalkInput()
        {
            if (inputMode != InputMode.PushToTalk) return;
            if (_client == null || !_client.IsOpen) return;
            bool held = IsPushToTalkHeld();
            if (held && !_pttWasHeld)
            {
                if (_modelSpeaking)
                {
                    // Interrupt the guide so the visitor can ask a question.
                    _client.Send(new JObject { ["type"] = "response.cancel" });
                    Debug.Log("[GuideOrchestrator] PTT pressed mid-narration — cancelled current response.");
                }
                Debug.Log("[GuideOrchestrator] PTT pressed — recording.");
            }
            else if (!held && _pttWasHeld)
            {
                _client.Send(new JObject { ["type"] = "input_audio_buffer.commit" });
                _lastResponseSource = "PUSH_TO_TALK";
                _client.Send(new JObject
                {
                    ["type"] = "response.create",
                    ["response"] = new JObject()
                });
                Debug.Log("[GuideOrchestrator] PTT released — committed input and requested response.");
            }
            _pttWasHeld = held;
        }

        void Handle(JObject evt)
        {
            var type = (string)evt["type"];
            switch (type)
            {
                case "response.created":
                    _responseCreatedCount++;
                    _modelSpeaking = true;
                    Debug.Log($"[Realtime] response.created #{_responseCreatedCount}  source={_lastResponseSource}");
                    _lastResponseSource = "vad-or-server"; // reset; future explicit triggers overwrite before this fires again
                    // Wipe any pending mic chunks server-side so they can't trigger spurious follow-up responses.
                    _client.Send(new JObject { ["type"] = "input_audio_buffer.clear" });
                    break;

                case "response.output_audio.delta":
                    _audioDeltaCount++;
                    _modelSpeaking = true;
                    var b64 = (string)evt["delta"];
                    if (!string.IsNullOrEmpty(b64)) _player.EnqueueBase64Pcm16(b64);
                    break;

                case "response.output_audio.done":
                    if (verboseLogging) Debug.Log("[Realtime] response.output_audio.done");
                    break;

                case "response.output_text.delta":
                    break;

                case "response.output_text.done":
                    break;

                case "response.output_audio_transcript.delta":
                    break;

                case "response.output_audio_transcript.done":
                    break;

                case "response.done":
                    _modelSpeaking = false;
                    _modelSpeakingUntil = Time.time + postSpeechMuteGrace;
                    if (verboseLogging) Debug.Log($"[Realtime] response.done  status={evt["response"]?["status"]}  audioDeltasInThisResponse={_audioDeltaCount}");
                    _audioDeltaCount = 0;
                    // Drain any mic chunks captured during the response (echo from speakers, etc.) that were buffered server-side.
                    _client.Send(new JObject { ["type"] = "input_audio_buffer.clear" });
                    break;

                case "input_audio_buffer.speech_started":
                    if (verboseLogging) Debug.Log("[Realtime] input_audio_buffer.speech_started — VAD detected speech in mic input");
                    break;

                case "input_audio_buffer.speech_stopped":
                    if (verboseLogging) Debug.Log("[Realtime] input_audio_buffer.speech_stopped — VAD detected end of speech");
                    break;

                case "input_audio_buffer.committed":
                    if (verboseLogging) Debug.Log($"[Realtime] input_audio_buffer.committed — item_id={evt["item_id"]} (this triggers response.create from VAD)");
                    _lastResponseSource = "VAD-AUTO-COMMIT";
                    break;

                case "conversation.item.created":
                    if (verboseLogging)
                    {
                        var role = (string)evt["item"]?["role"] ?? "?";
                        var itemType = (string)evt["item"]?["type"] ?? "?";
                        Debug.Log($"[Realtime] conversation.item.created  role={role} type={itemType}");
                    }
                    break;

                case "rate_limits.updated":
                case "response.output_item.added":
                case "response.output_item.done":
                case "response.content_part.added":
                case "response.content_part.done":
                    // Noisy: skip unless verboseLogging
                    break;

                case "error":
                    Debug.LogError($"[Realtime] error: {evt["error"]}");
                    break;

                case "session.created":
                case "session.updated":
                    Debug.Log($"[Realtime] {type}  vad={evt["session"]?["turn_detection"]?["type"] ?? "<null>"} threshold={evt["session"]?["turn_detection"]?["threshold"]}");
                    break;

                default:
                    if (verboseLogging) Debug.Log($"[Realtime] {type}");
                    break;
            }
        }
    }
}
