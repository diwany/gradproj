# Flowchart Specification

> **Project:** AI-Assisted Virtual Reality Museum for Cultural Heritage Education — A Real-Time Speech-Enabled Cultural Heritage Experience for Meta Quest and Desktop Simulator Platforms
>
> **Purpose:** Complete flowchart specification document, derived from the actual implementation in `PROJECT_REPORT.md`, ready to be converted into professional flowcharts in Visio, Draw.io, Lucidchart, StarUML, or Mermaid.
>
> Every step, decision, loop, and error path is grounded in the documented behaviour of the codebase (`GazeArtifactDetector`, `LobbyController`, `GuideOrchestrator`, `RealtimeClient`, `MicCapture`, `StreamingAudioPlayer`, `TourGuideAgent`, `AmplitudeJawFlap`, `MuseumDatabase`, `SessionState`, `OpenAIConfig`). No invented functionality.

---

## Phase 1 — System Workflow Analysis

### 1.1 Catalogue of distinct workflows

| Workflow | Scope | Subsystems involved |
|---|---|---|
| Master system workflow | Application start → exit | All |
| User registration workflow | Visitor enters form → record persisted → museum loaded | LobbyController, MuseumDatabase, SessionState, OpenAIConfig |
| Museum navigation (VR) workflow | Continuous-locomotion / snap-turn / teleport during the tour | XR Origin (XRI 3.0.8) |
| Artifact recognition (gaze) workflow | Frame loop in `GazeArtifactDetector` | GazeArtifactDetector, ArtifactInfo |
| AI guide narration workflow | OnArtifactIdentified → conversation.item.create + response.create → audio playback | GuideOrchestrator, RealtimeClient, StreamingAudioPlayer |
| Realtime voice (PTT) workflow | Push-to-Talk capture → API → response | MicCapture, RealtimeClient, GuideOrchestrator, StreamingAudioPlayer |
| Tour guide avatar workflow | NavMesh follow + distance maintenance + animator + jaw-flap | TourGuideAgent, NavMeshAgent, AmplitudeJawFlap |
| Database (SQLite) workflow | Open → CreateTable → Insert → Dispose | MuseumDatabase, gilzoide.sqlite-net |
| Scene transition workflow | Lobby → Museum via LoadSceneMode.Single | LobbyController, SessionState |

### 1.2 Short descriptions

- **Master system workflow** sequences the entire application lifetime from process launch, through the lobby configuration, into the museum experience, and back out to process exit.
- **User registration** drives the lobby form's data path: visitor input → validation → SQLite insert → in-memory session seeding → single-mode scene load.
- **Museum navigation** delivers VR locomotion: continuous motion on the left thumbstick, 30° snap-turn on the right thumbstick, teleport via the left thumbstick forward push and release with a comfort vignette.
- **Artifact recognition** runs every frame and applies four sequential gates: a 2 m ray-distance gate, a bounds-centre gate, a 1.5 s dwell-time gate, and a 30 s per-artifact cooldown gate.
- **AI guide narration** receives an identified-artifact event, composes a system-role context message, ships it to the Realtime API, and asks for a 1–2 sentence narration response.
- **Realtime voice (PTT)** runs the upstream mic capture pipeline (24 kHz PCM16 base64 chunks of 40 ms = 960 samples) and the downstream audio-delta consumption pipeline (30 s ring buffer with a 200 ms pre-buffer).
- **Tour guide avatar** keeps the Mixamo Pharaoh humanoid at a fixed 1.8 m social distance from the visitor by recomputing the desired position every 0.25 s and pathfinding the NavMeshAgent toward it.
- **Database workflow** opens the SQLite connection in `ReadWrite | Create | FullMutex` mode and inserts one row per visitor session.
- **Scene transition** uses `LoadSceneMode.Single` (not additive) to avoid the dual-XR-Origin collision; `SessionState` survives via `DontDestroyOnLoad`.

---

## Phase 2 — Master System Flowchart

### 2.1 Sequence

| Step | Name | Purpose | Inputs | Outputs | Next | Error / Branch |
|---|---|---|---|---|---|---|
| S0 | Start | Process launch | OS process invocation | — | S1 | — |
| S1 | Resolve Config Path | Locate `config.json` | `Application.dataPath`, `%APPDATA%` | Absolute path | S2 | — |
| S2 | Load Configuration | Read JSON | Path | API key string OR error | S3 (ok) / S2a (fail) | S2a → display "Config missing" status; disable Submit |
| S3 | Initialize Unity Engine | Engine bootstrap | Manifest, scenes | Loaded engine | S4 | — |
| S4 | Load Lobby Scene | Build-index 0 | Scenes/Lobby.unity | Lobby scene active | S5 | — |
| S5 | Instantiate LobbyController | `Awake`/`Start` lifecycle | Lobby scene objects | DB connection, populated form | S6 | — |
| S6 | Display Registration Form | Render world-space canvas | Form fields | Form visible to visitor | S7 | — |
| S7 | Wait for Visitor Input | Block on form interaction | Name, Age, Country, Submit | Validated event | S8 | — |
| S8 | Validate Inputs | Non-empty name + API key present | Form values, `SessionState.OpenAiApiKey` | Pass/fail | S9 (pass) / S7 (fail) | S7 with status update |
| S9 | Construct VisitorRecord | Build POCO | Name, Age, CountryCode, CountryName, StartedAtUnixSeconds | `VisitorRecord` instance | S10 | — |
| S10 | Store Visitor Record | SQLite insert | `VisitorRecord` | New row in `museum.db`; returned `Id` | S11 | On exception → status "Database error", re-enable Submit, S7 |
| S11 | Seed SessionState | Cross-scene singleton write | Visitor, API key | Populated `SessionState` | S12 | — |
| S12 | Transition to Museum | `LoadSceneAsync(Single)` | Scene name | Museum scene active, Lobby destroyed | S13 | — |
| S13 | Initialize Museum | Scene `Awake`/`Start` | Scene objects | XR Origin, NavMesh, Tour Guide, 42 artifacts | S14 | — |
| S14 | Open Realtime Session | WebSocket connect to OpenAI | API key, model URL | Open WebSocket | S15 | On connect failure → `enabled = false`, log; museum runs silent |
| S15 | Configure Session | `session.update` JSON | Visitor name, English-lock system prompt | Server-side session configured | S16 | — |
| S16 | Send Opening Greeting | `response.create` with greeting instruction | — | Greeting audio stream starts | S17 | — |
| S17 | Begin Museum Experience | Enter steady-state loop | XR pose stream, audio streams | Running tour | S18 | — |
| S18 | Steady-state Loop | Each frame: navigation + gaze + PTT + animator + audio | All inputs | Updated scene, audio, animation | S18 (loop) | Branch to S19 on application quit |
| S19 | Application Quit | OS-level shutdown signal | — | OnDestroy chain | S20 | — |
| S20 | Disconnect Realtime | `_client.DisconnectAsync` | Open WebSocket | Closed socket | S21 | — |
| S21 | Close Database | `_db.Dispose()` | Open connection | Closed connection | S22 | — |
| S22 | End | Process exit | — | — | — | — |

### 2.2 Steady-state loop S18 expanded

S18 runs every Unity frame and performs, in this order:

1. Drain inbound WebSocket events (`_incoming` queue).
2. Poll Push-to-Talk input (`HandlePushToTalkInput`).
3. Periodically emit a debug-stats line every `micStatsIntervalSeconds`.
4. `GazeArtifactDetector.Update` runs the four-gate gaze pipeline.
5. `TourGuideAgent.Update` updates the desired position, NavMeshAgent destination, facing, and animator booleans.
6. `MicCapture.Update` polls the microphone (samples streamed only when gated through).
7. `StreamingAudioPlayer` audio thread independently reads from the ring buffer as the audio device demands samples.
8. `AmplitudeJawFlap.LateUpdate` reads `AudioSource.GetOutputData` and flips `IsSpeaking`.
9. `TourGuideAgent.LateUpdate` re-snaps the hip bone's local XZ to its rest pose (the runtime fix for Mixamo walk drift).

---

## Phase 3 — User Registration Flowchart

```
[Start]
  │
  ▼
[Open Lobby scene]
  │
  ▼
[Construct LobbyController]
  │
  ▼
[LobbyController.Start] ─────────────────┐
  ├── SessionState.GetOrCreate()         │
  ├── new MuseumDatabase()                │
  ├── PopulateCountries()                 │
  │    └── Sort, pin "Other" at bottom    │
  │    └── countryDropdown.value = idx(Egypt) │
  ├── SetupAgeSlider()                    │
  │    └── min=5, max=99, default=25       │
  └── LoadAndDisplayKey()                  │
       ├── OpenAIConfig.TryLoadKey         │
       ├── If success → status "OpenAI key loaded" (green)
       └── If failure → status reason (red); _session.OpenAiApiKey = null
  │
  ▼
[Wait for input loop]
  │
  ▼
[Name field changed]──────────┐
[Age slider changed]──────────┤
[Country dropdown changed]────┤
  │                            │
  ▼                            │
[UpdateSubmitInteractable] ────┘
  │
  ├── hasName = !IsNullOrWhiteSpace(nameField.text)
  ├── hasKey  = !IsNullOrEmpty(_session.OpenAiApiKey)
  └── submitButton.interactable = hasName AND hasKey
  │
  ▼
[Submit pressed?] ── NO ──► loop back to [Wait for input loop]
  │ YES
  ▼
[Disable Submit button]
  │
  ▼
[ResolveCountry()]
  │
  ▼
[Construct VisitorRecord {Name, Age, CountryCode, CountryName, StartedAtUnixSeconds}]
  │
  ▼
[Try: _db.InsertVisitor(record)]
  │
  ├── Exception?  YES ──► [Status: "Database error. See Console."] ─► [Re-enable Submit] ─► loop
  │                NO
  ▼
[_session.Visitor = record]
  │
  ▼
[LoadSceneAsync(museumSceneName, LoadSceneMode.Single)]
  │
  ▼
[End — Lobby destroyed; SessionState survives]
```

### 3.1 Decision conditions

| Decision | Expression | True branch | False branch |
|---|---|---|---|
| Name valid | `!string.IsNullOrWhiteSpace(nameField.text)` | Proceed to enable Submit | Show "Enter your name to begin." |
| Key valid | `!string.IsNullOrEmpty(_session.OpenAiApiKey)` | Proceed to enable Submit | Show key-status text in red |
| Submit interactable | `hasName && hasKey` | Allow Submit click | Submit grayed out |
| DB insert success | No `SQLiteException` thrown | Proceed to session seeding | Show "Database error" |

---

## Phase 4 — Artifact Recognition Flowchart

The gaze loop is implemented in `GazeArtifactDetector.Update` and runs every Unity frame.

```
[Frame Start]
  │
  ▼
[Acquire gazeOrigin]
  ├── If null → gazeOrigin = Camera.main.transform
  └── If still null → return (no XR camera yet)
  │
  ▼
[Cast Ray]
  └── Physics.Raycast(gazeOrigin.position, gazeOrigin.forward,
                       out hit, maxDistance = 2 m, raycastMask,
                       QueryTriggerInteraction.Ignore)
  │
  ▼
[Did ray hit anything?] ── NO ──► [Reset _current = null; _dwellElapsed = 0] ──► return
  │ YES
  ▼
[hit.collider.GetComponentInParent<ArtifactInfo>()]
  │
  ▼
[Is hitArtifact != null?] ── NO ──► [Reset] ──► return
  │ YES
  ▼
[ComputeArtifactBounds(hitArtifact)]
  │
  ▼
[Distance(gazeOrigin, bounds.center) > maxDistance (2 m)?]
  ├── YES → [hitArtifact = null] → [Reset] → return
  └── NO  ↓
  ▼
[IsOnCooldown(hitArtifact)?]
  └── _cooldownUntil[artifact] > Time.time?
  │
  ├── YES → [Reset _current and _dwellElapsed] ──► return
  └── NO  ↓
  ▼
[hitArtifact != _current?]
  ├── YES → [_current = hitArtifact; _dwellElapsed = 0] ──► return
  └── NO  ↓
  ▼
[_dwellElapsed += Time.deltaTime]
  │
  ▼
[_dwellElapsed >= dwellSeconds (1.5 s)?] ── NO ──► return
  │ YES
  ▼
[_cooldownUntil[hitArtifact] = Time.time + cooldownSeconds (30 s)]
  │
  ▼
[_dwellElapsed = 0; _current = null]
  │
  ▼
[Log "[GazeArtifactDetector] Identified '<name>'..."]
  │
  ▼
[Invoke OnArtifactIdentified(hitArtifact, hit)]
  │
  ▼
[Return to main loop]
```

### 4.1 Branches and error cases

| Gate | Pass condition | Failure action |
|---|---|---|
| Camera available | `Camera.main != null` | Return without modifying state |
| Ray hits something | `Physics.Raycast` returns true | Reset `_current` and `_dwellElapsed` |
| Hit carries ArtifactInfo | `GetComponentInParent<ArtifactInfo>() != null` | Reset state |
| Bounds-centre in range | `Distance(origin, bounds.center) ≤ 2 m` | Reset state (suppresses cross-room clip-through) |
| Not on cooldown | `_cooldownUntil[a] ≤ Time.time` (or absent) | Reset state |
| Same target as last frame | `hitArtifact == _current` | Switch `_current`, reset dwell |
| Dwell complete | `_dwellElapsed ≥ 1.5 s` | Defer until threshold met |

The loop never throws under normal operation; it is purely a state machine over per-frame inputs.

---

## Phase 5 — AI Guide Flowchart

Runs in `GuideOrchestrator.OnArtifactIdentified`.

```
[OnArtifactIdentified(artifact, hit) event received]
  │
  ▼
[Is _client null OR !_client.IsOpen?]
  ├── YES → return (silently drop event; WebSocket not ready)
  └── NO  ↓
  ▼
[_modelSpeaking OR Time.time < _modelSpeakingUntil?]
  ├── YES → [Log "Skipped narration of '<name>' — guide is currently speaking"] → return
  └── NO  ↓
  ▼
[Compose context string:
   "The visitor is now looking at: <displayName>.
    Era: <era>.
    Background: <description>
    Give a short, evocative 1-2 sentence introduction now,
    then invite a question."]
  │
  ▼
[Send conversation.item.create JSON
   {
     "type": "conversation.item.create",
     "item": {
       "type": "message",
       "role": "system",
       "content": [{ "type": "input_text", "text": <context> }]
     }
   }]
  │
  ▼
[_lastResponseSource = "GAZE:<displayName>"]
  │
  ▼
[Send response.create JSON  { "type": "response.create" }]
  │
  ▼
[Return — event handled]
  │
  ▼
[Asynchronously: WebSocket receive loop delivers response.created]
  │
  ▼
[Handle("response.created"):
   _responseCreatedCount++;
   _modelSpeaking = true;
   Send input_audio_buffer.clear]
  │
  ▼
[Receive response.audio.delta frames continuously]
   └── For each: StreamingAudioPlayer.EnqueueBase64Pcm16
  │
  ▼
[Receive response.audio.done — log only]
  │
  ▼
[Receive response.done]
   ├── _modelSpeaking = false
   ├── _modelSpeakingUntil = Time.time + postSpeechMuteGrace (0.4 s)
   ├── Send input_audio_buffer.clear (drain echo)
   └── Reset _audioDeltaCount
  │
  ▼
[Return to monitoring state — visitor can now ask follow-up via PTT]
```

### 5.1 Decision points

| Decision | Condition | True | False |
|---|---|---|---|
| Client ready | `_client != null && _client.IsOpen` | Proceed | Drop event |
| Guide currently speaking | `_modelSpeaking || Time.time < _modelSpeakingUntil` | Skip (no self-interrupt) | Compose and send |

---

## Phase 6 — Realtime Voice Communication Flowchart

### 6.1 Upstream (visitor speaks → API)

```
[Frame Update]
  │
  ▼
[HandlePushToTalkInput]
  │
  ▼
[inputMode == PushToTalk?] ── NO ──► return
  │ YES
  ▼
[_client open?] ── NO ──► return
  │ YES
  ▼
[IsPushToTalkHeld()]
   ├── Keyboard.current[T].isPressed?  → return true
   ├── XR controller primaryButton (Y/B) on any controller? → return true
   └── otherwise → false
  │
  ▼
[Edge detection: held vs _pttWasHeld]
   ├── held AND !_pttWasHeld → PTT just pressed
   │      ├── If _modelSpeaking → Send response.cancel; log
   │      └── Log "PTT pressed — recording"
   ├── !held AND _pttWasHeld → PTT just released
   │      ├── Send input_audio_buffer.commit
   │      ├── _lastResponseSource = "PUSH_TO_TALK"
   │      ├── Send response.create  { "modalities": ["audio","text"] }
   │      └── Log "PTT released — committed input and requested response"
   └── _pttWasHeld = held
  │
  ▼
[MicCapture.Update] (independent path each frame)
  ├── Microphone.GetPosition / GetData
  ├── Linear resample to 24 kHz
  ├── Buffer until ≥ 960 samples
  ├── Convert float → short → byte (PCM16 little-endian)
  ├── Convert.ToBase64String
  └── Raise OnAudioChunkBase64 event
  │
  ▼
[GuideOrchestrator.SendMicChunk(base64)]
  │
  ▼
[Gate 1: _client null OR !IsOpen?]    YES → drop
  ▼
[Gate 2: inputMode == GazeOnly?]       YES → _micChunksSkippedNoVad++; drop
  ▼
[Gate 3: PushToTalk AND !IsPushToTalkHeld?] YES → _micChunksSkippedNoVad++; drop
  ▼
[Gate 4: muteMicWhileModelSpeaks AND
        (_modelSpeaking OR Time.time < _modelSpeakingUntil)?]
        YES → _micChunksSkippedMute++; drop
  ▼
[Send input_audio_buffer.append { audio: base64 }]
[_micChunksSent++]
```

### 6.2 Downstream (API → visitor hears narration)

```
[Background receive loop: RealtimeClient.ReceiveLoop]
  │
  ▼
[Accumulate WebSocket message fragments until EndOfMessage]
  │
  ▼
[JObject.Parse(raw)]
  ├── parse exception → log warning → continue
  └── ok ↓
  │
  ▼
[Push JObject onto _incoming concurrent queue]
  │
  ▼ (main thread)
[GuideOrchestrator.Update drains _incoming]
  │
  ▼
[Handle(evt) switch on evt["type"]]
  │
  ├── "session.created" / "session.updated" → log VAD type + threshold
  │
  ├── "response.created"
  │      ├── _responseCreatedCount++
  │      ├── _modelSpeaking = true
  │      ├── Send input_audio_buffer.clear
  │      └── Log source (_lastResponseSource)
  │
  ├── "response.audio.delta"
  │      ├── _audioDeltaCount++
  │      ├── _modelSpeaking = true
  │      └── StreamingAudioPlayer.EnqueueBase64Pcm16(evt["delta"])
  │
  ├── "response.audio.done" → log only
  │
  ├── "response.done"
  │      ├── _modelSpeaking = false
  │      ├── _modelSpeakingUntil = Time.time + postSpeechMuteGrace
  │      ├── Send input_audio_buffer.clear
  │      └── _audioDeltaCount = 0
  │
  ├── "input_audio_buffer.speech_started" / "speech_stopped" → log
  │
  ├── "input_audio_buffer.committed"
  │      └── _lastResponseSource = "VAD-AUTO-COMMIT"
  │
  ├── "conversation.item.created" → verbose log role+type
  │
  ├── "error" → LogError full payload
  │
  └── default → verbose log
  │
  ▼
[StreamingAudioPlayer.EnqueueBase64Pcm16]
  │
  ▼
[Convert.FromBase64String]
  ├── exception → return
  └── ok ↓
  ▼
[sampleCount = bytes.Length / 2]
  │
  ▼
[Acquire ring lock]
  │
  ▼
[Free = ring.Length - _available]
  │
  ▼
[sampleCount > Free?]
  ├── YES → Log "Ring overflow — dropping X oldest samples"
  │         _readIdx = (_readIdx + drop) % ring.Length
  │         _available -= drop
  └── NO ↓
  ▼
[Decode each sample (LE short) and write into _ring at _writeIdx]
[_available += sampleCount]
[Release lock]
  │
  ▼
[Audio thread: OnPcmRead(float[] data)]
  │
  ▼
[Acquire ring lock]
  │
  ▼
[!_draining?]
  ├── _available < 4800 (200 ms prebuffer)?
  │      YES → write zeros; return
  ├── otherwise → _draining = true
  │
  ▼
[take = min(needed, _available); copy from _ring to data; _available -= take]
  │
  ▼
[take < needed?] (under-run)
  ├── YES → fade _lastSample × 0.92 per sample over remainder
  │         _draining = false; _lastSample = 0
  └── NO  → return
  │
  ▼
[Release lock; samples render on AudioSource at avatar head bone]
```

### 6.3 Error handling and recovery

| Failure | Detection | Recovery |
|---|---|---|
| WebSocket connect fails | `await _socket.ConnectAsync` throws | `OnClose("Connect failed: ...")`; `enabled = false`; museum continues silent |
| Server-side close | `result.MessageType == Close` in `ReceiveLoop` | `OnClose("Server closed: ...")`; the orchestrator logs warning |
| JSON parse error on inbound frame | `JObject.Parse` throws | `Debug.LogWarning`; the receive loop continues |
| Sender exception | `SendAsync` throws | `OnClose("Send loop error: ...")` |
| Ring buffer overflow | `sampleCount > free` in `EnqueueBase64Pcm16` | `Debug.LogWarning`; drop oldest; continue |
| Audio under-run | `take < needed` in `OnPcmRead` | Decay-fade from `_lastSample`; reset to pre-buffer state |

---

## Phase 7 — Tour Guide Avatar Flowchart

`TourGuideAgent.Update` runs every frame on the Mixamo avatar; `LateUpdate` runs the hip-bone XZ lock.

```
[Frame Start]
  │
  ▼
[player null?]
  ├── YES → set player = Camera.main.transform (if any) → if still null, return
  └── NO ↓
  ▼
[fromPlayerToAgent = transform.position - player.position; .y = 0]
[planarDist = fromPlayerToAgent.magnitude]
  │
  ▼
[planarDist > 0.05?]
  ├── YES → dirFromPlayer = fromPlayerToAgent / planarDist
  └── NO  → dirFromPlayer = transform.forward (.y=0; fallback Vector3.forward)
  │
  ▼
[desiredPos = player.position + dirFromPlayer * stoppingDistance (1.8 m); .y = transform.y]
[distToDesired = Distance(transform.position, desiredPos)]
  │
  ▼
[Time.time >= _nextRepathTime AND _agent.isOnNavMesh?]
  └── _nextRepathTime = Time.time + 0.25
  ▼
[distToDesired > 0.4 (hysteresis)?]
  ├── YES → _agent.isStopped = false; _agent.SetDestination(desiredPos)
  └── NO  → _agent.isStopped = true
  │
  ▼
[Face the player]
  └── toPlayer = -fromPlayerToAgent; if sqrMagnitude > 0.0001:
       faceTarget = LookRotation(toPlayer)
       transform.rotation = Slerp(transform.rotation, faceTarget, dt * 4)
  │
  ▼
[Animator != null?]
  ├── moving = _agent.velocity.sqrMagnitude > 0.04
  ├── animator.SetBool("IsMoving", moving)
  └── animator.SetBool("IsSpeaking", isSpeaking)
  │
  ▼
[End Update — return]
```

```
[LateUpdate]
  │
  ▼
[_hipBone null?]   YES → return
  │ NO
  ▼
[p = _hipBone.localPosition]
[p.x = _hipRestLocalPos.x]
[p.z = _hipRestLocalPos.z]   ← walk-drift fix; Y is left free
[_hipBone.localPosition = p]
  │
  ▼
[End LateUpdate]
```

### 7.1 State summary

| Animator state | Trigger | Visible behaviour |
|---|---|---|
| Idle (default) | `IsMoving == false && IsSpeaking == false` | Standing still |
| Walk | `IsMoving == true && IsSpeaking == false` | Walking cycle |
| Talk | `IsSpeaking == true` (from any state) | Talking gestures (overrides Walk) |

Transitions:
- Idle → Walk on `IsMoving = true` (duration 0.15 s).
- Walk → Idle on `IsMoving = false` (duration 0.15 s).
- Idle / Walk → Talk on `IsSpeaking = true`.
- Talk → Idle on `IsSpeaking = false`; if `IsMoving = true` the next frame, transitions Idle → Walk.

---

## Phase 8 — Database Flowchart

```
[LobbyController.Start]
  │
  ▼
[new MuseumDatabase(path = null)]
  │
  ▼
[DbPath = path ?? DefaultDbPath()]
   └── Portable path first (<exe>/MuseumVR/)
   └── Otherwise %APPDATA%/MuseumVR/museum.db
  │
  ▼
[Directory.CreateDirectory(Path.GetDirectoryName(DbPath))]
  │
  ▼
[new SQLiteConnection(DbPath, ReadWrite | Create | FullMutex)]
   ├── On exception → propagate to LobbyController; lobby surfaces error
   └── On success → _conn assigned
  │
  ▼
[_conn.CreateTable<VisitorRecord>()]
   └── Schema-aware no-op if table already exists
  │
  ▼
[Database ready — Submit may be pressed]
  │
  ▼
[InsertVisitor(record)]
   ├── If record.StartedAtUnixSeconds == 0:
   │     record.StartedAtUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
   ├── _conn.Insert(record)  ← assigns auto-increment Id
   └── return record.Id
  │
  ▼
[Returned to LobbyController.OnSubmit]
  │
  ▼
[Database remains open for the rest of the lobby lifetime]
  │
  ▼
[LobbyController.OnDestroy fires when the lobby scene is unloaded]
   └── _db?.Dispose()  ← closes the SQLiteConnection
  │
  ▼
[Museum scene runs without a database connection
 (the production design persists only the visitor row at lobby submit)]
```

### 8.1 Error branches

| Error | Cause | Resolution |
|---|---|---|
| Directory creation fails | Path is read-only | OS-level exception propagates; lobby surfaces "Database error" |
| `new SQLiteConnection` throws | File corruption, missing native library | Lobby surfaces "Database error" |
| `Insert` throws | Schema mismatch (developer error) | Lobby catches the exception in `OnSubmit`, restores Submit |
| `Dispose` exception | Already-disposed connection | Suppressed silently (idempotent) |

---

## Phase 9 — Scene Transition Flowchart

```
[Lobby active; Submit pressed; record persisted to D1; _session.Visitor populated]
  │
  ▼
[Read museumSceneName ("Museum" by default)]
  │
  ▼
[loadAdditively == false (production setting)?]
  ├── YES → SceneManager.LoadSceneAsync(museumSceneName, LoadSceneMode.Single)
  └── NO  → (additive path — disabled in production; would cause dual XR Origin)
  │
  ▼
[Lobby scene begins unload]
  │
  ▼
[OnDestroy chain on Lobby GameObjects]
   ├── LobbyController.OnDestroy → _db?.Dispose()
   └── Lobby XR Origin destroyed
  │
  ▼
[SessionState GameObject SURVIVES (DontDestroyOnLoad)]
   └── Carries Visitor and OpenAiApiKey forward
  │
  ▼
[Museum scene loaded; Awake / OnEnable / Start chain]
   ├── XR Origin (museum) instantiated
   ├── 42 ArtifactInfo GameObjects active
   ├── NavMesh present (baked at edit time)
   ├── Tour Guide GameObject Awake/Start
   ├── GazeArtifactDetector Awake — picks up Camera.main
   ├── ArtifactLabelSpawner subscribes to OnArtifactIdentified
   └── GuideOrchestrator.Start:
        ├── Locate MicCapture, StreamingAudioPlayer
        ├── Read SessionState.OpenAiApiKey
        │    (if null, fall back to OpenAIConfig.TryLoadKey directly)
        ├── Subscribe to GazeArtifactDetector.OnArtifactIdentified
        ├── Construct RealtimeClient
        ├── await _client.ConnectAsync(apiKey, url)
        ├── ConfigureSession() — send session.update with English-lock prompt
        └── Send opening greeting response.create
  │
  ▼
[Museum scene active; visitor is in VR; experience begins]
```

### 9.1 Decision and exception summary

| Decision | True branch | False branch |
|---|---|---|
| `loadAdditively` | Risky; momentarily two XR Origins (rejected for production) | **Default**: clean single-mode swap |
| API key available in SessionState | Use it directly | Fall back to `OpenAIConfig.TryLoadKey` |
| `ConnectAsync` succeeds | `OnOpen` fires; ConfigureSession + opening greeting | Log error; `enabled = false`; museum runs without guide |

---

## Phase 10 — ABET Documentation Output

### Flowchart Analysis and Process Description

This section documents the system's behavioural flows for direct inclusion in the project report. Each flowchart corresponds to a discrete subsystem and is anchored to a specific class or method in the implementation. Together they describe the application's control flow from start to exit and detail every decision, loop, and error path that affects user-visible behaviour.

#### Figure 6.X — Main System Flowchart

**Purpose.** Sequence the entire application lifetime from process launch to process exit.
**Inputs.** Operating-system process invocation, the `config.json` file on disk, the Lobby and Museum scenes in Build Settings.
**Outputs.** A complete museum tour delivered to the visitor and a row persisted to SQLite.
**Decision Nodes.** Configuration loaded (success / fail); form validation (pass / fail); database insert (success / exception); WebSocket connect (success / fail).
**Loop Structures.** The steady-state museum loop (S18) is the application's frame-rate inner loop.
**Exceptional Cases.** Missing or malformed `config.json`, SQLite insert exception, Realtime API connection failure. Each is captured by an explicit branch in the master flowchart.
**System States.** Configuring, Lobby active, Validating, Inserting, Transitioning, Museum active, Speaking, Shutting down.
**Transition Conditions.** Detailed per-step in §2.1 above.

#### Figure 6.X — Visitor Registration Flowchart

**Purpose.** Define the data path from form open to visitor record persisted and session populated.
**Inputs.** Visitor name, age, country selection, Submit event.
**Outputs.** Inserted SQLite row; populated `SessionState`; scene transition initiated.
**Decision Nodes.** Name valid; API key valid; Submit interactable; database insert success.
**Loop Structures.** The wait-for-input loop, broken only by a successful Submit press.
**Exceptional Cases.** Empty name, missing API key, database exception. The empty-name and missing-key cases keep the Submit button disabled. The database exception re-enables Submit and shows a textual error.
**System States.** Form-open, Waiting-for-input, Validating, Inserting, Seeding session, Transitioning.

#### Figure 6.X — Artifact Recognition Flowchart

**Purpose.** Identify which catalogued artifact the visitor is looking at.
**Inputs.** Head transform each frame.
**Outputs.** `OnArtifactIdentified` event carrying the artifact and the raycast hit.
**Decision Nodes.** Ray hit any collider; collider parent has `ArtifactInfo`; bounds centre within 2 m; not on cooldown; same target as last frame; dwell threshold met.
**Loop Structures.** Per-frame `Update` loop; the dwell timer accumulates inside this loop.
**Exceptional Cases.** Camera not yet available at scene start (returned silently). Wall-clipped rays that hit distant artifacts are rejected by the bounds-centre gate. The 30 s cooldown prevents re-firing on the same artifact within one museum-walk-by.
**System States.** Idle, Tracking, Dwelling, Identified, Cooldown.

#### Figure 6.X — AI Guide Workflow

**Purpose.** Translate a gaze identification into a real-time narrated description.
**Inputs.** `OnArtifactIdentified` event with `ArtifactInfo`, current speaking state.
**Outputs.** `conversation.item.create` and `response.create` JSON frames sent to OpenAI.
**Decision Nodes.** WebSocket open; guide currently speaking.
**Loop Structures.** None; this workflow is event-driven and one-shot per identification.
**Exceptional Cases.** A new identification arriving during in-flight narration is dropped to prevent self-interruption; if the WebSocket is not open the event is silently dropped (the museum simply has no guide for that visit).
**System States.** Ready, Composing, Sent, Speaking.

#### Figure 6.X — OpenAI Realtime Communication Workflow

**Purpose.** Bidirectional, low-latency audio streaming with the conversational model.
**Inputs.** Visitor's microphone audio; PTT held-button events; inbound `response.audio.delta` frames.
**Outputs.** `input_audio_buffer.append`, `input_audio_buffer.commit`, `response.create`, `response.cancel`, `input_audio_buffer.clear` events; rendered audio samples at the head-bone AudioSource.
**Decision Nodes.** Input mode (`GazeOnly` / `PushToTalk` / `ServerVad`); PTT held; mute-while-speaking gate; ring overflow; audio under-run.
**Loop Structures.** The send loop, the receive loop, the audio thread's `OnPcmRead`, and the main thread's event drain — four concurrent loops cooperating through a concurrent queue and a lock-protected ring buffer.
**Exceptional Cases.** Connect failure, server close, JSON parse error, ring overflow, audio under-run — each enumerated in §6.3.

#### Figure 6.X — Tour Guide Avatar Workflow

**Purpose.** Maintain a 1.8 m social distance from the visitor and synchronise mouth animation with the AI's audio output.
**Inputs.** Player transform, NavMeshAgent state, `isSpeaking` flag, `AudioSource.GetOutputData`.
**Outputs.** NavMeshAgent destination, animator booleans `IsMoving` / `IsSpeaking`, jaw bone rotation, hip-bone XZ lock.
**Decision Nodes.** Player within hysteresis band (0.4 m of the 1.8 m desired distance); facing-range eligibility; jaw bone present.
**Loop Structures.** Per-frame `Update` repath every 0.25 s; per-frame `LateUpdate` hip lock.
**Exceptional Cases.** Player teleported to within 5 cm of the agent (degenerate direction case) — handled by falling back to the agent's forward vector. Missing jaw bone (most Mixamo characters) — handled by skipping the bone rotation while still flipping `IsSpeaking`.

#### Figure 6.X — Database Management Workflow

**Purpose.** Persist exactly one row per visitor session into the local SQLite database.
**Inputs.** `VisitorRecord` instance from `LobbyController.OnSubmit`.
**Outputs.** Persisted row in `museum.db`.
**Decision Nodes.** Portable path available; insert success.
**Loop Structures.** None — a strict open-create-insert-dispose sequence per session.
**Exceptional Cases.** Path inaccessible (caught at construction); insert exception (caught in `OnSubmit`, restores Submit button).

#### Figure 6.X — Scene Transition Workflow

**Purpose.** Move from Lobby to Museum in single-scene mode while preserving the in-memory session.
**Inputs.** Submit press; populated `SessionState`; `museumSceneName`.
**Outputs.** Lobby unloaded; Museum loaded; `SessionState` survives.
**Decision Nodes.** `loadAdditively` flag (production: false); Realtime API connect success.
**Loop Structures.** None.
**Exceptional Cases.** Realtime API connect failure leaves the museum running without a guide; the visitor still sees the environment, the artifacts, and the labels. This degradation is intentional — the museum is browsable even when the AI is unavailable.

#### Design Rationale

The flowcharts above describe an event-driven, multi-loop runtime in which the per-frame Unity update loop coordinates four logically independent state machines: the gaze detector (`GazeArtifactDetector`), the embodied agent (`TourGuideAgent`), the orchestrator (`GuideOrchestrator`), and the audio engine (`StreamingAudioPlayer`). Cross-loop communication is constrained to well-defined boundaries: the gaze detector raises an event consumed by both the label spawner and the orchestrator; the orchestrator's incoming events are marshalled across thread boundaries through a concurrent queue; the audio engine communicates with the main thread only through the lock-protected ring buffer. This architecture is the reason the flowcharts above can be drawn cleanly per-subsystem without introducing implicit cross-cutting concerns.

---

**End of Flowchart Specification.**
