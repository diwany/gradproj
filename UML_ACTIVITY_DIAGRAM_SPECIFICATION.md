# UML Activity Diagram Specification

> **Project:** AI-Assisted Virtual Reality Museum for Cultural Heritage Education — A Real-Time Speech-Enabled Cultural Heritage Experience for Meta Quest and Desktop Simulator Platforms
>
> **Purpose:** Complete UML Activity Diagram specification, derived from the actual implementation in `PROJECT_REPORT.md`, ready for direct conversion into professional UML Activity Diagrams in StarUML, Visual Paradigm, Draw.io, Lucidchart, Enterprise Architect, PlantUML, or Visio.

---

## Phase 1 — System Behavior Analysis

### 1.1 Primary actor

- **Visitor** — the human user wearing the Meta Quest headset who provides identity at the lobby, navigates the museum, and converses with the AI guide.

### 1.2 Secondary actors

- **Operator** — provisions the OpenAI API key file before deployment; not present at runtime.
- **OpenAI Realtime API** — external cloud service (`gpt-4o-mini-realtime-preview`) reached over a WebSocket.

### 1.3 System components

| Component | Class / Asset | Role |
|---|---|---|
| Lobby UI | `LobbyController`, `Lobby.unity`, world-space canvas | Identity capture |
| Database | `MuseumDatabase`, `VisitorRecord` | SQLite persistence |
| Session | `SessionState` (DontDestroyOnLoad) | Cross-scene singleton |
| Configuration | `OpenAIConfig` | API key loading |
| Gaze | `GazeArtifactDetector`, `ArtifactInfo` | Artifact identification |
| Labels | `ArtifactLabelSpawner`, `ArtifactLabel.prefab` | Floating identification labels |
| Orchestrator | `GuideOrchestrator` | Realtime API event handling |
| Transport | `RealtimeClient` | WebSocket transport |
| Mic | `MicCapture` | 24 kHz PCM16 base64 chunks |
| Audio | `StreamingAudioPlayer` | 30 s ring buffer, head-bone AudioSource |
| Avatar | `TourGuideAgent`, `AmplitudeJawFlap` | NavMesh follow, mouth animation |

### 1.4 Activity classes

- **User activities** — form completion, locomotion, head-pointing at artifacts, holding the PTT button, vocal questions.
- **System activities** — render frames, validate form, persist record, transition scene, raycast each frame, send/receive JSON over WebSocket, buffer audio, drive animator.
- **AI activities** — server-side speech-to-speech inference, audio generation, language-locked English narration.
- **Database activities** — open connection, ensure schema, insert record, dispose.
- **Concurrent activities** — gaze evaluation, audio capture, audio playback, animator update, NavMesh pursuit, WebSocket I/O.

### 1.5 Decision points (master list)

- Name field non-empty?
- API key loaded?
- Submit interactable?
- Database insert success?
- WebSocket open?
- Model currently speaking?
- Input mode (`GazeOnly` / `PushToTalk` / `ServerVad`)?
- PTT held?
- Mic mute-grace active?
- Ray hits collider?
- Hit carries `ArtifactInfo`?
- Bounds-centre within 2 m?
- Same artifact as previous frame?
- Dwell ≥ 1.5 s?
- On cooldown?
- Within hysteresis band of desired 1.8 m position?
- Audio under-run?
- Ring overflow?
- Jaw bone present on avatar?

### 1.6 Synchronisation points

- The `_incoming` concurrent queue synchronises the WebSocket receive loop (background thread) with `GuideOrchestrator.Update` (main thread).
- The ring-buffer lock synchronises `EnqueueBase64Pcm16` (main thread) with `OnPcmRead` (audio thread).
- `LateUpdate` ordering implicitly synchronises the Animator's bone writes with `TourGuideAgent`'s hip XZ-lock and `AmplitudeJawFlap`'s jaw rotation.
- `SessionState`'s `DontDestroyOnLoad` mark synchronises the Lobby → Museum scene-transition boundary.

### 1.7 Termination conditions

- The application terminates on OS-level close signal, propagating through Unity's `OnDestroy` chain to close the WebSocket and dispose the database connection.

---

## Phase 2 — Actor Identification (Swimlanes)

The Activity Diagrams use the following swimlanes.

### SL-1: Visitor

| Attribute | Detail |
|---|---|
| Purpose | Provide identity, navigate, gaze at artifacts, speak via PTT. |
| Responsibilities | Form input, locomotion, gaze direction, push-to-talk operation. |
| Interactions | XR Origin, Lobby form, Push-to-talk button, Microphone. |
| Inputs (from system) | Rendered visuals, narration audio, floating labels. |
| Outputs (to system) | Form values, controller input, head pose, audio. |

### SL-2: VR Museum System

| Attribute | Detail |
|---|---|
| Purpose | Unity-side application logic outside the specialised subsystems below. |
| Responsibilities | Scene transitions, frame rendering, XR Origin updates, locomotion processing. |
| Interactions | Lobby form, XR Origin, audio mixer. |
| Inputs | Controller state, scene events. |
| Outputs | Rendered frames, scene transitions. |

### SL-3: Artifact Recognition System

| Attribute | Detail |
|---|---|
| Purpose | Decide which artifact the visitor is looking at and fire identification events. |
| Responsibilities | Each-frame ray cast, bounds-centre verification, dwell timer, cooldown map. |
| Interactions | Head transform, `ArtifactInfo` components, label spawner, orchestrator. |
| Inputs | Head transform, per-frame deltaTime. |
| Outputs | `OnArtifactIdentified(ArtifactInfo, RaycastHit)` event. |

### SL-4: AI Tour Guide

| Attribute | Detail |
|---|---|
| Purpose | Translate gaze events into Realtime API conversation; orchestrate narration and follow-up Q&A. |
| Responsibilities | Compose context messages, send `response.create`, handle inbound events. |
| Interactions | Gaze detector, RealtimeClient, MicCapture, StreamingAudioPlayer, TourGuideAgent. |
| Inputs | Gaze events, WebSocket inbound events, PTT state. |
| Outputs | WebSocket outbound events, audio playback, animator flags. |

### SL-5: OpenAI Realtime API

| Attribute | Detail |
|---|---|
| Purpose | External service: speech-to-speech inference and audio generation. |
| Responsibilities | Accept audio + system messages; produce audio responses with English narration. |
| Interactions | RealtimeClient (WebSocket). |
| Inputs | `session.update`, `input_audio_buffer.append/commit/clear`, `conversation.item.create`, `response.create/cancel`. |
| Outputs | `session.created/updated`, `response.created/audio.delta/audio.done/done`, `input_audio_buffer.committed`, `error`. |

### SL-6: Database

| Attribute | Detail |
|---|---|
| Purpose | Persist visitor records to SQLite. |
| Responsibilities | Open file, create schema, insert rows, dispose. |
| Interactions | `LobbyController.OnSubmit`. |
| Inputs | `VisitorRecord`. |
| Outputs | Persisted row, auto-incremented `Id`. |

### SL-7: Tour Guide Avatar

| Attribute | Detail |
|---|---|
| Purpose | Mixamo humanoid following the visitor with NavMeshAgent and synchronising mouth/body animation with the AI's audio. |
| Responsibilities | Compute desired 1.8 m position, drive NavMeshAgent, animate Idle/Walk/Talk, hip XZ lock. |
| Interactions | Player transform, NavMesh, Animator, AudioSource (head bone). |
| Inputs | Player position, `isSpeaking` flag, `AudioSource.GetOutputData`. |
| Outputs | Avatar transform, animator booleans, jaw rotation. |

### SL-8: Session Manager

| Attribute | Detail |
|---|---|
| Purpose | Hold visitor record + API key across scene transitions. |
| Responsibilities | Maintain singleton lifetime; provide cross-scene reads/writes. |
| Interactions | `LobbyController` (write), `GuideOrchestrator` (read). |
| Inputs | `VisitorRecord`, API key string. |
| Outputs | Same, exposed as properties. |

---

## Phase 3 — Main Activity Diagram

Master end-to-end Activity Diagram for the entire user experience.

### 3.1 Activity table

| ID | Activity | Swimlane | Purpose | Inputs | Outputs | Preconditions | Postconditions | Control Flow Next |
|---|---|---|---|---|---|---|---|---|
| A01 | Launch Application | SL-2 | Process start | OS invocation | Engine bootstrapped | OS executes binary | Engine loop running | A02 |
| A02 | Resolve Configuration Path | SL-2 | Locate config.json | `Application.dataPath`, `%APPDATA%` | Absolute path | Engine running | Path known | A03 |
| A03 | Load API Key | SL-8 | Read JSON | Path | API key string OR failure | File present | `SessionState.OpenAiApiKey` populated or null | A04 |
| A04 | Initialize Lobby Scene | SL-2 | Load Lobby.unity | Build index 0 | Lobby scene loaded | None | World-space canvas active | A05 |
| A05 | Display Form | SL-2 | Render UI | Form fields | UI visible | Canvas active | Visitor sees form | A06 |
| A06 | Enter Visitor Information | SL-1 | Visitor types and selects | Keyboard, dropdown, slider | Field values | Form visible | Form populated | A07 |
| A07 | Validate Input | SL-2 | Check non-empty name + API key | Field values, `SessionState.OpenAiApiKey` | Pass/fail | Submit clicked | Validation outcome | DECISION D-A07 |
| A08 | Store Visitor Record | SL-6 | SQLite insert | `VisitorRecord` | Persisted row, returned `Id` | Validation passed | Row in `museum.db` | DECISION D-A08 |
| A09 | Seed Session State | SL-8 | Singleton write | `VisitorRecord`, API key | Populated `SessionState` | Insert successful | Visitor + key available across scenes | A10 |
| A10 | Transition to Museum | SL-2 | Single-mode scene load | Scene name | Museum loaded, Lobby destroyed | Session seeded | Museum scene active | A11 |
| A11 | Initialize Museum | SL-2 | Scene Awake/Start chain | Scene objects | XR Origin + 42 artifacts + Tour Guide ready | Museum scene loaded | Subsystems live | A12 (parallel fork) |
| A12 | Open Realtime Session | SL-4 → SL-5 | WebSocket connect | API key, URL | Open connection | API key valid | `IsOpen == true` | A13 |
| A13 | Configure Session | SL-4 → SL-5 | Send `session.update` | English-lock prompt, voice, modalities | Server-side config | Connection open | Session configured | A14 |
| A14 | Send Opening Greeting | SL-4 → SL-5 | `response.create` with greeting prompt | — | Greeting audio stream | Session configured | First narration starts | A15 |
| A15 | Begin Museum Steady State | SL-2 + SL-3 + SL-4 + SL-7 (concurrent) | Enter main loop | All inputs | Live experience | All subsystems initialised | Continuous behaviour | A16 (parallel) |
| A16 | Monitor Visitor (gaze + PTT + locomotion) | SL-2 + SL-3 | Per-frame loop | Head pose, button state, mic | Identification events; PTT events; updated XR Origin | Steady state | Loop | A17 (event-driven branch) |
| A17 | Recognise Artifact | SL-3 | Gate pipeline | Head transform | `OnArtifactIdentified` | Visitor within 2 m of artifact for 1.5 s | Identification event fired | A18 parallel with A19 |
| A18 | Spawn Label | SL-2 | World-space label | Event payload | Label visible | Event fired | Visitor sees label | back to A16 |
| A19 | Narrate Artifact | SL-4 → SL-5 → SL-7 | `conversation.item.create` + `response.create`; receive audio; play back | Event payload | Audio stream | Event fired and `_modelSpeaking == false` | Audio playing at head bone | A20 |
| A20 | Receive Audio Stream | SL-5 → SL-4 → SL-7 | Decode deltas into ring buffer | `response.audio.delta` events | Buffered audio | Stream started | Audio in ring | A21 |
| A21 | Play Narration | SL-7 → SL-1 | Audio thread reads ring; AudioSource renders | Ring buffer samples | Audible voice | Pre-buffer reached | Visitor hears guide | back to A16 |
| A22 | Wait for Visitor Question | SL-4 | Monitor PTT button | Button state | Held / released events | None | Held → A23 | A23 on press |
| A23 | Capture Visitor Audio | SL-1 → SL-2 → SL-4 → SL-5 | Stream PCM16 base64 chunks | Microphone | `input_audio_buffer.append` events | PTT held | API receives audio | A24 on release |
| A24 | Commit Question | SL-4 → SL-5 | `input_audio_buffer.commit` + `response.create` | — | Server begins response | PTT released | Server processing | A25 |
| A25 | Stream Response Audio | SL-5 → SL-4 → SL-7 | Same as A20–A21 | `response.audio.delta` | Audible answer | `response.created` received | Visitor hears reply | back to A16 |
| A26 | Continue Exploration | SL-1 | Visitor moves on | Locomotion input | New gaze targets | Audio finished | Loop | back to A16 |
| A27 | Exit Application | SL-1 → SL-2 | OS quit signal | — | OnDestroy chain | Visitor requests quit | Shutdown in progress | A28 |
| A28 | Disconnect Realtime | SL-4 → SL-5 | `_client.DisconnectAsync()` | — | Closed WebSocket | Quit requested | Connection closed | A29 |
| A29 | Dispose Database | SL-6 | `_conn.Dispose()` | — | Closed SQLite | Quit requested | Database closed | A30 |
| A30 | End | SL-2 | Process exit | — | — | Cleanup complete | Process terminated | — |

### 3.2 Decision nodes (master)

| Decision ID | Condition | True Branch | False Branch |
|---|---|---|---|
| D-A07 | Name and API key both valid? | A08 (Store Visitor Record) | back to A06 (visitor edits) |
| D-A08 | `_db.InsertVisitor` returned without exception? | A09 (Seed Session State) | A06 with status "Database error" |
| D-Stream | `_modelSpeaking == true` when new gaze event fires? | Skip narration; log; return | A19 (begin narration) |

### 3.3 Concurrency (Fork at A11/A15)

After A15 the application's main thread enters a steady-state in which four parallel activities run concurrently and rejoin only on quit:
- Gaze pipeline (`GazeArtifactDetector.Update`)
- Voice send/receive (`MicCapture` + `RealtimeClient` + `GuideOrchestrator.Update`)
- Avatar pursuit (`TourGuideAgent.Update` and `LateUpdate`)
- Audio thread (`StreamingAudioPlayer.OnPcmRead`)

A Join Node sits at A27 (Exit Application): all parallel activities terminate on the quit signal.

---

## Phase 4 — Visitor Registration Activity Diagram

### 4.1 Activities

| ID | Activity | Swimlane | Description |
|---|---|---|---|
| R01 | Initial Node | — | Visitor opens application |
| R02 | Resolve Config Path | SL-8 | `OpenAIConfig.GetConfigPath()` |
| R03 | Load Config | SL-8 | `OpenAIConfig.TryLoadKey` |
| R04 | Construct DB | SL-6 | `new MuseumDatabase()` |
| R05 | Populate Country List | SL-2 | `PopulateCountries()` (Egypt default) |
| R06 | Display Form | SL-2 | Render world-space canvas |
| R07 | Enter Name | SL-1 | Visitor types into TMP_InputField |
| R08 | Enter Age | SL-1 | Visitor moves slider (5–99) |
| R09 | Select Country | SL-1 | Visitor opens dropdown |
| R10 | Validation Activity | SL-2 | `UpdateSubmitInteractable` |
| R11 | Submit Action | SL-1 | Press Begin Tour |
| R12 | Resolve Country | SL-2 | Map dropdown index to `Country` |
| R13 | Construct VisitorRecord | SL-2 | Populate fields |
| R14 | Insert Visitor | SL-6 | `_db.InsertVisitor(record)` |
| R15 | Set SessionState.Visitor | SL-8 | `_session.Visitor = record` |
| R16 | Load Museum Scene | SL-2 | `SceneManager.LoadSceneAsync(Single)` |
| R17 | Final Node | — | Lobby destroyed |

### 4.2 Decision nodes

| ID | Condition | True | False |
|---|---|---|---|
| DR-1 | `hasName && hasKey`? | R11 enabled | Submit disabled; loop R07–R10 |
| DR-2 | Insert without exception? | R15 | Log error, re-enable Submit, loop |
| DR-3 | `loadAdditively` flag | (rejected design) | R16 (single-mode load) |

### 4.3 Object flows

- `VisitorRecord` flows R13 → R14 → R15.
- `SessionState` is mutated R15.
- Inserted `Id` flows R14 → R15 (logged, not used downstream).

### 4.4 Preconditions & postconditions

- **Preconditions:** `config.json` exists; Lobby scene is build-index 0; SQLite native library available.
- **Postconditions:** One new row in `D1`; `SessionState.Visitor` and `SessionState.OpenAiApiKey` populated; Museum scene active; Lobby destroyed.

---

## Phase 5 — Artifact Recognition Activity Diagram

### 5.1 Activities

| ID | Activity | Swimlane |
|---|---|---|
| G01 | Initial Node — Frame Tick | SL-3 |
| G02 | Acquire Gaze Origin | SL-3 |
| G03 | Cast Ray (forward, 2 m, mask, ignore triggers) | SL-3 |
| G04 | Resolve Collider Parent → ArtifactInfo | SL-3 |
| G05 | Compute Encapsulated Bounds | SL-3 |
| G06 | Bounds-Centre Verification | SL-3 |
| G07 | Cooldown Lookup | SL-3 |
| G08 | Track Target Continuity | SL-3 |
| G09 | Accumulate Dwell | SL-3 |
| G10 | Test Dwell Threshold | SL-3 |
| G11 | Commit Identification (set cooldown, reset dwell, log) | SL-3 |
| G12 | Invoke OnArtifactIdentified | SL-3 |
| G13 | Final Node — Return | SL-3 |

### 5.2 Decision guards

| ID | Guard |
|---|---|
| DG-1 | [Camera available] |
| DG-2 | [Ray hit collider] |
| DG-3 | [Collider parent has ArtifactInfo] |
| DG-4 | [bounds.center within 2 m] |
| DG-5 | [Not on cooldown] |
| DG-6 | [Same artifact as last frame] |
| DG-7 | [dwellElapsed ≥ 1.5 s] |

### 5.3 Object flows

- `ArtifactInfo` flows G04 → G06 → G07 → G08 → G11 → G12.
- `RaycastHit` flows G03 → G12.
- `_cooldownUntil[artifact]` is updated at G11.

### 5.4 Loops and termination

- Loop: every Unity frame; no diagram-level termination.
- Internal early-return paths exit via DG-1, DG-2, DG-3, DG-4, DG-5 directly to G13.

---

## Phase 6 — AI Tour Guide Activity Diagram

### 6.1 Activities

| ID | Activity | Swimlane |
|---|---|---|
| T01 | Initial Node — OnArtifactIdentified | SL-4 |
| T02 | Check WebSocket State | SL-4 |
| T03 | Check Speaking Gate | SL-4 |
| T04 | Retrieve Artifact Information | SL-3 → SL-4 |
| T05 | Prepare AI Context | SL-4 |
| T06 | Generate Prompt (system message) | SL-4 |
| T07 | Send `conversation.item.create` | SL-4 → SL-5 |
| T08 | Send `response.create` | SL-4 → SL-5 |
| T09 | Receive `response.created` | SL-5 → SL-4 |
| T10 | Set `_modelSpeaking = true` | SL-4 |
| T11 | Clear server-side input_audio_buffer | SL-4 → SL-5 |
| T12 | Stream `response.audio.delta` events | SL-5 → SL-4 → SL-7 |
| T13 | Decode + Enqueue to Ring Buffer | SL-7 |
| T14 | Audio thread plays via AudioSource | SL-7 → SL-1 |
| T15 | Drive Animator (`IsSpeaking = true`) | SL-7 |
| T16 | Receive `response.done` | SL-5 → SL-4 |
| T17 | Set `_modelSpeaking = false`, schedule grace | SL-4 |
| T18 | Clear input_audio_buffer (drain echo) | SL-4 → SL-5 |
| T19 | Wait for Visitor Question (PTT) | SL-4 |
| T20 | Decision: continue or end conversation | SL-4 |
| T21 | Return to Monitoring State | SL-4 |
| T22 | Final Node | SL-4 |

### 6.2 Control flow guards

| ID | Guard |
|---|---|
| DT-1 | [Client open] (else drop) |
| DT-2 | [Not currently speaking] (else log "skipped narration" and drop) |
| DT-3 | [Visitor pressed PTT] (during T19 → continue) |
| DT-4 | [No PTT and visitor left within 30 s] (timeout → return to monitoring) |

### 6.3 Object flows

- `ArtifactInfo` → System message (T05–T06).
- JSON frame → Realtime API (T07, T08, T11, T18).
- Audio delta → Ring buffer → AudioSource → speakers (T12 → T13 → T14).
- `_modelSpeaking` and `_modelSpeakingUntil` mutate at T10 and T17.

---

## Phase 7 — Realtime Voice Processing Activity Diagram

### 7.1 Activities (with explicit fork/join)

| ID | Activity | Swimlane |
|---|---|---|
| V01 | Initial Node — Steady State | SL-4 |
| V02 | **Fork** — three concurrent flows: F1 mic capture, F2 outbound dispatch, F3 inbound dispatch | SL-4 |
| F1.1 | Microphone Poll | SL-2 |
| F1.2 | Linear Resample to 24 kHz | SL-2 |
| F1.3 | Slice into 40 ms Window (960 samples) | SL-2 |
| F1.4 | Convert to PCM16 (short[] → byte[]) | SL-2 |
| F1.5 | Base64 Encode | SL-2 |
| F1.6 | Raise `OnAudioChunkBase64` event | SL-2 → SL-4 |
| F2.1 | Detect PTT State (`IsPushToTalkHeld`) | SL-4 |
| F2.2 | Gate by input mode + mute grace | SL-4 |
| F2.3 | Send `input_audio_buffer.append` | SL-4 → SL-5 |
| F2.4 | On PTT release: send `input_audio_buffer.commit` + `response.create` | SL-4 → SL-5 |
| F3.1 | Background WebSocket receive | SL-5 → SL-4 |
| F3.2 | Parse JSON | SL-4 |
| F3.3 | Enqueue to `_incoming` queue | SL-4 |
| F3.4 | Main-thread drain (each frame) | SL-4 |
| F3.5 | Dispatch by event type (switch) | SL-4 |
| F3.6 | If `response.audio.delta` → decode → ring buffer | SL-7 |
| F3.7 | Audio thread reads ring → AudioSource → speakers | SL-7 → SL-1 |
| F3.8 | If `response.done` → reset speaking flag, grace timer, clear input buffer | SL-4 |
| V03 | **Join** — converges on quit signal | SL-4 |
| V04 | Disconnect WebSocket | SL-4 → SL-5 |
| V05 | Final Node | SL-4 |

### 7.2 Parallel activity matrix

| Concurrent activity | Runs on | Synchronisation with main thread |
|---|---|---|
| Mic capture | Main thread (frame-rate) | Direct (Unity API) |
| WebSocket send loop | Background `Task` | `_outgoing` concurrent queue, dequeued by send loop |
| WebSocket receive loop | Background `Task` | `_incoming` concurrent queue, drained by `Update` |
| Audio playback | Unity audio thread | Ring-buffer `lock` |
| Animator update | Main thread | Implicit (same frame) |
| NavMeshAgent | Main thread | Implicit |

### 7.3 Decision and exception flows

- **Decision V-Mode** at F2.2 — three branches by input mode; only `PushToTalk` (held) or `ServerVad` send audio.
- **Decision V-Mute** at F2.2 — `muteMicWhileModelSpeaks && (_modelSpeaking || Time.time < _modelSpeakingUntil)` short-circuits send.
- **Exception V-Overflow** at F3.6 — `sampleCount > free` triggers a logged drop of oldest samples.
- **Exception V-Underrun** at F3.7 — `take < needed` triggers a decay-to-silence fade and reverts to pre-buffer state.
- **Exception V-NetClose** — server-initiated close logs warning; orchestrator continues without guide.

---

## Phase 8 — Tour Guide Avatar Activity Diagram

### 8.1 Activities

| ID | Activity | Swimlane |
|---|---|---|
| W01 | Initial Node — Awake | SL-7 |
| W02 | Get NavMeshAgent | SL-7 |
| W03 | Set `_agent.stoppingDistance = 0.3`, autoBraking = true | SL-7 |
| W04 | Cache `_hipBone` and `_hipRestLocalPos` | SL-7 |
| W05 | Start — Locate `Camera.main` as player | SL-7 |
| W06 | Frame Tick — Update | SL-7 |
| W07 | Compute `fromPlayerToAgent` and `planarDist` | SL-7 |
| W08 | Compute `dirFromPlayer` (with degenerate-case fallback) | SL-7 |
| W09 | Compute `desiredPos` and `distToDesired` | SL-7 |
| W10 | Decide: `Time.time >= _nextRepathTime`? | SL-7 |
| W11 | Decide: `distToDesired > 0.4`? | SL-7 |
| W12 | If yes → `_agent.isStopped = false; SetDestination(desiredPos)` | SL-7 |
| W13 | Else → `_agent.isStopped = true` | SL-7 |
| W14 | Slerp facing toward player | SL-7 |
| W15 | Set `IsMoving` and `IsSpeaking` on Animator | SL-7 |
| W16 | LateUpdate — hip-bone XZ lock | SL-7 |
| W17 | LateUpdate (jaw) — `AmplitudeJawFlap` reads `AudioSource.GetOutputData` | SL-7 |
| W18 | Compute RMS amplitude | SL-7 |
| W19 | Decide: jaw bone present? | SL-7 |
| W20 | If yes → rotate jaw smoothed by amplitude | SL-7 |
| W21 | Set `isSpeaking` on `TourGuideAgent` based on RMS > silenceThreshold | SL-7 |
| W22 | Final Node (per-frame) | SL-7 |

### 8.2 Animator state transitions (as guards)

| Source | Target | Guard |
|---|---|---|
| Idle | Walk | [IsMoving == true] |
| Walk | Idle | [IsMoving == false] |
| Idle / Walk | Talk | [IsSpeaking == true] |
| Talk | Idle | [IsSpeaking == false] |

---

## Phase 9 — Parallel Activity Analysis

| Concurrent activity | Trigger | Execution context | Synchronisation method | Termination condition | Fork node | Join node |
|---|---|---|---|---|---|---|
| Gaze evaluation | Frame tick | Main thread | None (single-threaded read) | Application quit | After A15 | At A27 |
| Tour guide pursuit | Frame tick | Main thread | None (single-threaded write) | Application quit | After A15 | At A27 |
| Audio capture | Frame tick (Unity API) | Main thread | OS audio driver | Mic disabled / quit | After A15 | At A27 |
| WebSocket send loop | Steady | Background `Task` | `_outgoing` concurrent queue | CancellationToken cancel | After A12 | At A28 |
| WebSocket receive loop | Steady | Background `Task` | `_incoming` concurrent queue | Server close OR cancel | After A12 | At A28 |
| Audio playback callback | Audio device | Unity audio thread | Ring-buffer `lock` | `AudioClip` destroyed | After A11 | At A27 |
| Animator + Avatar | Frame tick | Main thread (LateUpdate ordering) | Implicit per-frame ordering | Quit | After A11 | At A27 |

---

## Phase 10 — Decision Node Analysis

| Decision | Condition | True | False | Result |
|---|---|---|---|---|
| Name valid? | `!string.IsNullOrWhiteSpace(name)` | Submit interactable progresses | Submit grayed out | Lobby gating |
| API key valid? | `!string.IsNullOrEmpty(_session.OpenAiApiKey)` | Submit interactable progresses | Status shows reason | Lobby gating |
| Submit interactable? | Both above true | Submit click possible | Submit grayed out | Lobby gating |
| DB insert succeeded? | No exception | Continue to session seeding | Status "Database error" | Persistence outcome |
| WebSocket open? | `_client.IsOpen` | Send frames | Drop event | API guard |
| Model currently speaking? | `_modelSpeaking \|\| Time.time < _modelSpeakingUntil` | Skip self-interrupt | Send | Anti-echo guard |
| PTT held? | Keyboard T OR XR primaryButton on any controller | Stream audio (if mode permits) | Halt mic | Voice gating |
| Input mode | enum {GazeOnly, PushToTalk, ServerVad} | Branch accordingly | — | Audio routing |
| Ray hit collider? | `Physics.Raycast` returns true | Continue to artifact lookup | Reset state | Gaze gating |
| Collider has ArtifactInfo? | `GetComponentInParent<ArtifactInfo>() != null` | Continue | Reset | Gaze gating |
| Bounds-centre within 2 m? | `Distance(origin, bounds.center) ≤ 2 m` | Continue | Reset | Anti-clip guard |
| On cooldown? | `_cooldownUntil[a] > Time.time` | Reset state | Continue | 30 s suppression |
| Same target as previous frame? | `hit == _current` | Accumulate dwell | Reset dwell, set new `_current` | Continuity tracking |
| Dwell ≥ 1.5 s? | `_dwellElapsed >= 1.5 s` | Fire identification | Continue | Dwell completion |
| Within 0.4 m of desired position? | `distToDesired ≤ 0.4` | `isStopped = true` | Set destination | Hysteresis on follow |
| Ring overflow? | `sampleCount > free` | Drop oldest, log | Write normally | Audio buffer protection |
| Audio under-run? | `take < needed` | Decay-fade, reset pre-buffer | Continue normally | Click-out prevention |
| Jaw bone present? | `_hipBone != null` (avatar config) | Rotate jaw | Skip rotation | Mixamo character variability |
| `loadAdditively`? | Public field (production: false) | Additive (rejected) | Single (production) | Scene transition mode |

---

## Phase 11 — Object Flow Analysis

| Object | Source Activity | Destination Activity | Purpose |
|---|---|---|---|
| Name string | R07 | R13 | Identity capture |
| Age integer | R08 | R13 | Identity capture |
| Country selection | R09 | R12 → R13 | Identity capture |
| `VisitorRecord` instance | R13 | R14, R15 | Persisted + cached |
| Auto-increment `Id` | R14 | R15 | (logged) |
| API key string | A03 | A09, A13 | Realtime auth |
| `SessionState` | R15 | A13 | Visitor name in system prompt |
| Head transform | per-frame | G03, W07 | Gaze + follow |
| `ArtifactInfo` reference | G04 | G06–G12, T04–T06 | Identification + narration |
| `RaycastHit` | G03 | G12 (event payload) | Hit metadata |
| System message JSON | T06 | T07 | Context push |
| `response.create` JSON | T08 | SL-5 | Response request |
| `input_audio_buffer.append` JSON | F2.3 | SL-5 | Question audio |
| `input_audio_buffer.commit` + `response.create` JSON | F2.4 | SL-5 | Question turn close |
| `response.audio.delta` JSON | SL-5 | F3.5 | Audio stream |
| Decoded PCM samples | F3.6 | F3.7 | Playback |
| Audio output buffer | F3.7 | EE-6 (speakers) | Visitor hears |
| `_modelSpeaking` flag | T10, T17 | F2.2, T03 | Anti-echo, anti-interrupt |
| `IsMoving` boolean | W15 | Animator | Walk state |
| `IsSpeaking` boolean | W21 | Animator | Talk state |
| Hip rest position | W04 | W16 | Walk-drift safety net |
| Jaw bone rest rotation | (init) | W20 | Mouth animation |

---

## Phase 12 — ABET Documentation Output

### Activity Diagram Analysis

This section presents the system's behavioural specification at the granularity of UML Activity Diagrams. Eight swimlanes (Visitor; VR Museum System; Artifact Recognition System; AI Tour Guide; OpenAI Realtime API; Database; Tour Guide Avatar; Session Manager) partition the responsibilities of the application along the natural seams identified by the implementation. Six top-level activity diagrams describe, respectively, the master end-to-end experience, visitor registration, gaze-based artifact recognition, AI guide narration and Q&A, real-time voice processing, and the tour-guide avatar's behaviour. Two further structural analyses — parallel activity decomposition and decision node catalogue — make the system's concurrency model and decision logic explicit.

#### Figure X.X — Main System Activity Diagram

**Purpose.** Capture the system's entire user-visible lifecycle in a single diagram, exposing the lobby-to-museum transition, the concurrent fork that begins steady-state behaviour, and the deterministic shutdown sequence.
**Actors.** Visitor (primary); Operator (pre-deployment file); OpenAI Realtime API (cloud service).
**Activities.** Thirty top-level activities (A01–A30) organised across all eight swimlanes.
**Decision Nodes.** D-A07 (form valid); D-A08 (insert success).
**Fork/Join Nodes.** Fork at A15 (after museum subsystems initialise); Join at A27 (Application Exit) where all parallel activities terminate.
**Object Flows.** `VisitorRecord` from A06 → A08 → A09; API key from A03 → A09 → A13; `OnArtifactIdentified` event from A17 to both A18 and A19.
**Preconditions.** `config.json` present; build settings include Lobby and Museum scenes.
**Postconditions.** One row inserted into `museum.db`; the visitor has completed and exited a tour.
**Control Flow.** Strictly sequential through A01–A15; then concurrent through A16–A26; then sequential A27–A30.
**Concurrency.** Four parallel activities run concurrently between A15 and A27: gaze evaluation, voice send/receive, avatar pursuit, audio playback.

#### Figure X.X — Visitor Registration Activity Diagram

**Purpose.** Detail the lobby's data path from form display to scene transition.
**Actors.** Visitor; Database; Session Manager.
**Activities.** Seventeen (R01–R17).
**Decision Nodes.** DR-1 (Submit interactability); DR-2 (insert success).
**Object Flows.** Form values → `VisitorRecord` → SQLite row + `SessionState.Visitor`.
**Preconditions.** Configuration loaded successfully.
**Postconditions.** `SessionState` populated; Museum scene loaded; Lobby destroyed.
**Concurrency.** None — strictly sequential within the lobby's lifetime.

#### Figure X.X — Artifact Recognition Activity Diagram

**Purpose.** Show the per-frame gaze decision pipeline including the bounds-centre verification step that prevents cross-room wall-clip identifications.
**Actors.** Artifact Recognition System (the entire flow is internal).
**Activities.** Thirteen (G01–G13).
**Decision Nodes.** Seven sequential guards (DG-1 through DG-7).
**Object Flows.** `ArtifactInfo` reference along the pipeline; updated cooldown timestamp into `_cooldownUntil`.
**Preconditions.** XR Origin's main camera is available.
**Postconditions.** Either no event fired (no qualifying artifact this frame) or `OnArtifactIdentified` raised exactly once.
**Concurrency.** None within this diagram; runs concurrently with the other steady-state activities documented in the master diagram.

#### Figure X.X — AI Tour Guide Activity Diagram

**Purpose.** Describe the orchestration of narration in response to a gaze identification, including the anti-self-interruption guard.
**Actors.** AI Tour Guide; OpenAI Realtime API; Tour Guide Avatar.
**Activities.** Twenty-two (T01–T22).
**Decision Nodes.** DT-1 (client open); DT-2 (not currently speaking); DT-3 (PTT pressed); DT-4 (timeout).
**Object Flows.** Identification event → context string → system message → narration audio.
**Preconditions.** WebSocket established and configured with English-locked system prompt.
**Postconditions.** Audio playback completed and the system returns to monitoring state.

#### Figure X.X — Realtime Voice Processing Activity Diagram

**Purpose.** Specify the three concurrent flows (mic capture, outbound dispatch, inbound dispatch) and the fork/join structure that synchronises them.
**Actors.** Visitor; AI Tour Guide; OpenAI Realtime API; Tour Guide Avatar.
**Activities.** Eighteen including explicit fork and join (V01–V05 with subflows F1.1–F3.8).
**Decision Nodes.** V-Mode (input mode), V-Mute (anti-echo gate); plus exception branches V-Overflow and V-Underrun.
**Fork/Join Nodes.** Fork at V02; Join at V03.
**Object Flows.** PCM16 base64 chunks upstream; decoded samples downstream; flags `_modelSpeaking` and `_modelSpeakingUntil` mediate between the two directions.

#### Figure X.X — Tour Guide Avatar Activity Diagram

**Purpose.** Detail per-frame avatar behaviour: distance maintenance via desired position recomputation, animator state, and the runtime walk-drift fix.
**Actors.** Tour Guide Avatar.
**Activities.** Twenty-two (W01–W22).
**Decision Nodes.** Repath timer ready; hysteresis band; jaw bone presence; animator state guards.
**Object Flows.** Player transform → `desiredPos`; `IsSpeaking` from `AmplitudeJawFlap` to `TourGuideAgent` to the animator.
**Concurrency.** `Update`, `LateUpdate`, and `AmplitudeJawFlap.LateUpdate` chain deterministically per Unity's execution ordering.

#### Design Rationale

The activity diagrams above respect three design properties documented in the project report. First, **single-writer ownership of mutable state**: only `LobbyController` writes to D1; only `SessionState` writes to D3; only `EnqueueBase64Pcm16` writes to D5; only `GazeArtifactDetector.Update` writes to D6. This invariant is preserved by keeping the writers in distinct swimlanes and never crossing object-flow arrows between writers. Second, **explicit thread-boundary synchronisation**: the diagrams introduce queue and lock objects at the precise junctures where the background `Task`-based send and receive loops meet the main-thread orchestrator, and where the audio thread meets the main thread. Third, **degradation rather than failure**: when the API is unreachable or returns an error, the activity diagram routes the system back to monitoring state rather than terminating; the visitor can still walk the museum, see labels, and use the lobby's persistence, while the guide is silent.

---

**End of UML Activity Diagram Specification.**
