# Progress

## Phase status

| Phase | What | Status |
|---|---|---|
| 0 | Project setup: packages, HDRP VR settings, Active Input = New | ✅ Done |
| 1 | VR foundation: OpenXR + Quest profiles, XR Origin in Museum scene | ✅ Done |
| 2 | Locomotion: teleport areas marked, snap turn from Starter Assets, vignette | ✅ Done |
| 3.1 | Gaze detection + ArtifactLabel prefab + auto-fill catalog (42 artifacts) | ✅ Done |
| 3.3 | ArtifactCaptureCamera + DebugCaptureSaver + label set | ✅ Written, **inert by design** |
| 3.4 | InferenceEngineArtifactClassifier + ArtifactRecognitionPipeline | ✅ Written, **inert by design** |
| 4 | Lobby scene + SQLite + form UI + OpenAIConfig | ✅ Done |
| 5 | RealtimeClient + MicCapture + StreamingAudioPlayer + GuideOrchestrator (3 modes) | ✅ Done |
| 6.1 | NavMesh bake utility + TourGuideAgent + AmplitudeJawFlap + placeholder body utility | ✅ Done + verified (capsule follows player) |
| 6.2 | User imports Mixamo character + animations | 🔄 In progress |
| 6.4 | Mixamo avatar wire-up utility | ⏳ Pending user import |
| 7 | HDRP optimization, baked lighting, LODs, frame budget | ⏳ Last phase |

## Last-known-good runtime config

- `Tour Guide` GameObject in `Museum.unity`
  - `GuideOrchestrator.model = "gpt-4o-mini-realtime-preview"` (after running `Reset Tour Guide Model to Default`)
  - `GuideOrchestrator.inputMode = PushToTalk` (default after recompile; or run `Set Tour Guide to Push-To-Talk Mode`)
  - `GuideOrchestrator.vadThreshold = 0.8` (only matters in ServerVad mode)
  - `GuideOrchestrator.muteMicWhileModelSpeaks = true`
  - `GuideOrchestrator.postSpeechMuteGrace = 0.4`
  - `GuideOrchestrator.verboseLogging = true`
  - `GuideOrchestrator.micStatsIntervalSeconds = 5`
  - `StreamingAudioPlayer.prebufferSamples = 4800` (200 ms)
- `Gaze Artifact Detection` (under XR Origin)
  - `GazeArtifactDetector.maxDistance = 2`
  - `GazeArtifactDetector.dwellSeconds = 1.5`
  - `GazeArtifactDetector.cooldownSeconds = 30`
- 42 artifacts in `Museum.unity` have `ArtifactInfo` populated with name/era/description

## Active issue

None. Voice path (PTT + 30s ring) and NavMesh follow (capsule placeholder) are verified end-to-end as of 2026-05-08.

## Decisions log (most recent first)

1. **Ring buffer 8s → 30s** + warn-on-drop. Realtime API can transmit a multi-second response in ~1s of network time. 8s wasn't enough.
2. **Three input modes** via `GuideOrchestrator.InputMode` enum. Default `PushToTalk` (key `T`). Server VAD created uncontrollable echo loops with PC speakers + PC mic.
3. **Greeting `modalities` fix**: API requires `["audio", "text"]`, not `["audio"]`. Was silently failing.
4. **`input_audio_buffer.clear`** sent at every `response.created` and `response.done` to wipe stale mic data on the server side.
5. **Verbose Realtime API logging** added to GuideOrchestrator — every event tagged with source (GAZE / PTT / OPENING_GREETING / VAD-AUTO-COMMIT).
6. **Gaze maxDistance 4m → 2m** + bounds-center check. Walls without colliders let rays clip into other rooms.
7. **Gaze identification rejected while guide is speaking**, so cross-artifact glances don't cancel current narration.
8. **Default model `gpt-4o-mini-realtime-preview`**. User has $5 of credit; doesn't have GA access (`gpt-realtime` / `gpt-realtime-mini`).
9. **OpenAI key from `%APPDATA%/MuseumVR/config.json`**, not lobby form. Operator owns the key.
10. **ML stack kept inert** — user wants it included for show but not running. `InferenceEngineArtifactClassifier`, `ArtifactRecognitionPipeline`, `ArtifactCaptureCamera`, `Wire ML Recognition Pipeline` menu all exist but the active path is gaze→label only.
11. **Lobby keyboard**: planned Meta XR Virtual Keyboard but it's not on the scoped registry → fell back to TMP_InputField for the name field. (Not yet wired to a VR-friendly keyboard; works with hardware keyboard during PCVR development.)
12. **No asmdefs**. Every attempt to add asmdefs broke transitive references when packages reshuffled. All scripts go to default `Assembly-CSharp` / `Assembly-CSharp-Editor`.
13. **Sentis renamed to Inference Engine** in Unity 6: package id `com.unity.ai.inference`, namespace `Unity.InferenceEngine`. The plan referred to "Sentis" but the actual package is Inference Engine.
14. **HDRP stays** — switching to URP would mean re-converting all 66 FBX materials. Optimize HDRP instead.
15. **Editor utilities, not manual setup**. Every multi-step Unity action has been encoded as a `Tools → Museum → Phase N → ...` menu item.

## Files I never want to lose track of

- `Assets/Scripts/Runtime/Voice/GuideOrchestrator.cs` — voice brain (~430 lines)
- `Assets/Scripts/Runtime/Voice/RealtimeClient.cs` — WebSocket transport
- `Assets/Scripts/Runtime/Voice/MicCapture.cs` — mic → 24 kHz PCM16 base64
- `Assets/Scripts/Runtime/Voice/StreamingAudioPlayer.cs` — base64 PCM16 → AudioSource via PCMReaderCallback (now 30 s ring)
- `Assets/Scripts/Runtime/Artifact/GazeArtifactDetector.cs` — 2 m raycast + bounds-center + 1.5 s dwell + 30 s cooldown
- `Assets/Scripts/Runtime/Artifact/ArtifactLabelSpawner.cs` — listens to gaze event, spawns world-space label
- `Assets/Scripts/Runtime/Lobby/LobbyController.cs` — form + DB insert + scene transition
- `Assets/Editor/ArtifactInfoAutoFill.cs` — hard-coded dictionary of all 42 Egyptian artifacts (era + description)
- `Assets/Editor/TourGuideSetup.cs` — Tour Guide menus including the three mode setters
- `Assets/Scenes/Museum.unity` — main scene
- `Assets/Scenes/Lobby.unity` — start scene
