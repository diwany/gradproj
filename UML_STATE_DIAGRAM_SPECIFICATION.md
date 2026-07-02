# UML State Diagram Specification

> **Project:** AI-Assisted Virtual Reality Museum for Cultural Heritage Education — A Real-Time Speech-Enabled Cultural Heritage Experience for Meta Quest and Desktop Simulator Platforms
>
> **Purpose:** Complete UML State Machine Diagram specification, derived from the actual implementation in `PROJECT_REPORT.md`, ready for direct conversion into professional UML State Diagrams in StarUML, Visual Paradigm, Enterprise Architect, Draw.io, Lucidchart, PlantUML, or Visio.

---

## Phase 1 — State Analysis

The following components in the implementation exhibit explicit state-based behaviour and deserve their own UML state machine.

| Component | Class / Asset | Purpose | Possible States (summary) |
|---|---|---|---|
| Application | `LobbyController` + `GuideOrchestrator` (cross-scene) | Top-level application lifecycle | Starting, Lobby, Validating, Inserting, Museum, Speaking, Shutting Down |
| Visitor Session | `SessionState` + `MuseumDatabase` | Lifecycle of a single visit | Not Created, Collecting, Validating, Saving, Created, Active, Closed |
| Gaze Detector | `GazeArtifactDetector` | Per-frame gaze pipeline | Idle, Tracking, Dwelling, Identified, Cooldown |
| AI Guide | `GuideOrchestrator` | Realtime API event-driven behaviour | Idle, Waiting for Artifact, Sending, Speaking, Listening for Question, Question in Flight |
| Realtime Voice Connection | `RealtimeClient` | WebSocket lifecycle | Disconnected, Connecting, Connected, Closing, Errored |
| Tour Guide Avatar | `TourGuideAgent` Animator | Animator state machine | Init, Idle, Walk, Talk |
| Database | `MuseumDatabase` | SQLite connection lifecycle | Closed, Opening, Ready, Inserting, Disposed |
| Realtime Audio Player | `StreamingAudioPlayer` | Audio thread streaming state | Prebuffering, Draining, Underrun (transient) |

Each is detailed below.

---

## Phase 2 — Main Application State Diagram

### 2.1 States

| State | Description | Entry Actions | Exit Actions | Internal Activities |
|---|---|---|---|---|
| Application Starting | Process running, engine bootstrap, Lobby scene loading | Acquire `Application.dataPath`; resolve `config.json` | — | Engine initialisation |
| Loading Configuration | Read JSON file | `File.ReadAllText` | Trim key | Validate non-empty key |
| Lobby Active | Lobby scene visible; form interactive | `PopulateCountries`, `LoadAndDisplayKey`, `SetupAgeSlider` | — | Listen to form events |
| Visitor Registration | Visitor typing / selecting | Enable inputs | — | Update Submit interactability |
| Validating Input | Form validation | — | — | `UpdateSubmitInteractable` |
| Creating Session | Build `VisitorRecord`, insert, populate `SessionState` | `_db.InsertVisitor` | — | Possibly catch exception |
| Museum Loading | Scene transition in progress | `LoadSceneAsync(Single)` | — | Unload Lobby, load Museum |
| Museum Active | Steady-state experience running | Connect Realtime API, configure session, send greeting | — | Per-frame loop, gaze, voice, animator |
| AI Interaction Active | Guide currently speaking or processing a request | `_modelSpeaking = true` | `_modelSpeaking = false`, set `_modelSpeakingUntil` | Stream audio, mute mic, drive jaw, drive `IsSpeaking` |
| Tour Ending | Quit signal received | — | — | Stop subsystems |
| Application Shutdown | Closing connection + DB | `_client.DisconnectAsync`, `_db.Dispose` | — | Cleanup |

### 2.2 Transitions

| Source | Target | Trigger | Guard | Action |
|---|---|---|---|---|
| Application Starting | Loading Configuration | engine ready | — | — |
| Loading Configuration | Lobby Active | config loaded | — | `_session.OpenAiApiKey = key` |
| Loading Configuration | Lobby Active (degraded) | config missing/invalid | — | Show key error in status |
| Lobby Active | Visitor Registration | form first interaction | — | — |
| Visitor Registration | Validating Input | any field changed | — | — |
| Validating Input | Visitor Registration | invalid | — | Keep Submit disabled |
| Validating Input | Creating Session | Submit pressed | [`hasName && hasKey`] | Disable Submit |
| Creating Session | Museum Loading | insert succeeded | — | `_session.Visitor = record` |
| Creating Session | Visitor Registration | insert exception | — | Show error, re-enable Submit |
| Museum Loading | Museum Active | scene loaded | — | Open WebSocket, configure session, send greeting |
| Museum Active | AI Interaction Active | `response.created` received | — | `_modelSpeaking = true` |
| AI Interaction Active | Museum Active | `response.done` received | — | Schedule mute grace |
| Museum Active | Tour Ending | OS quit signal | — | — |
| AI Interaction Active | Tour Ending | OS quit signal | — | — |
| Tour Ending | Application Shutdown | — | — | Cleanup |
| Application Shutdown | (final) | cleanup done | — | Process exit |

### 2.3 Hierarchical organisation

- **Composite state: Museum Active** contains the parallel substates: Gaze Pipeline Running, Audio Pipeline Running, Avatar Following.
- **Composite state: AI Interaction Active** is a sub-state of Museum Active; entering it does not leave Museum Active. The visitor can still navigate and look at other artifacts while the guide speaks (those events are filtered by the speaking gate).

---

## Phase 3 — Visitor Session State Diagram

### 3.1 States

| State | Entry Actions | Exit Actions | Events |
|---|---|---|---|
| Session Not Created | — | — | — |
| Collecting User Information | Open form, populate countries, default Egypt | — | onValueChanged events |
| Validating Input | — | — | Recompute `hasName && hasKey` |
| Saving Visitor Data | `_db.InsertVisitor` | — | DB exception handler |
| Creating Session | `_session.Visitor = record` | — | — |
| Session Active | `DontDestroyOnLoad` already set | — | Museum scene runs |
| Session Ending | — | — | OS quit signal |
| Session Closed | — | — | — |

### 3.2 Transitions

| Source | Target | Trigger | Guard | Action |
|---|---|---|---|---|
| Session Not Created | Collecting User Information | Lobby loads | — | — |
| Collecting User Information | Validating Input | Submit pressed | — | Disable Submit |
| Validating Input | Saving Visitor Data | inputs valid | [`hasName && hasKey`] | Construct `VisitorRecord` |
| Validating Input | Collecting User Information | inputs invalid | — | Show status |
| Saving Visitor Data | Creating Session | row inserted | — | Return `Id` |
| Saving Visitor Data | Collecting User Information | insert exception | — | Re-enable Submit; show "Database error" |
| Creating Session | Session Active | `_session.Visitor = record` | — | Trigger scene load |
| Session Active | Session Ending | quit | — | — |
| Session Ending | Session Closed | cleanup done | — | — |

---

## Phase 4 — Artifact Recognition State Diagram

### 4.1 States

| State | Entry Action | Exit Action | Internal Action |
|---|---|---|---|
| Idle | Reset `_current = null`, `_dwellElapsed = 0` | — | Wait for ray hit |
| Scanning | — | — | Each frame: cast ray, resolve ArtifactInfo |
| Artifact Candidate Detected | — | — | Compute bounds, distance |
| Dwell Timer Running | — | — | Accumulate `_dwellElapsed += Time.deltaTime` |
| Artifact Verification | — | — | Confirm same `_current` |
| Cooldown Check | — | — | Lookup `_cooldownUntil[a]` |
| Artifact Recognized | Set `_cooldownUntil[a] = Time.time + 30`, reset dwell, fire event | Clear `_current` | — |
| Narration Triggered | — | — | Pass control to AI Guide subsystem |
| Cooldown Active | — | — | Suppress further fires on this artifact for 30 s |

### 4.2 Transitions

| Source | Target | Trigger | Guard |
|---|---|---|---|
| Idle | Scanning | each frame `Update` | — |
| Scanning | Artifact Candidate Detected | ray hit + `ArtifactInfo` resolved | [bounds.center ≤ 2 m] |
| Scanning | Idle | ray miss OR no `ArtifactInfo` OR out of range | — |
| Artifact Candidate Detected | Cooldown Check | always | — |
| Cooldown Check | Idle | on cooldown | [`_cooldownUntil[a] > Time.time`] |
| Cooldown Check | Dwell Timer Running | not on cooldown AND same as `_current` | [same target] |
| Cooldown Check | Artifact Verification | not on cooldown AND new target | [different target] |
| Artifact Verification | Dwell Timer Running | always (after `_current` set) | — |
| Dwell Timer Running | Artifact Recognized | `_dwellElapsed >= 1.5 s` | — |
| Dwell Timer Running | Idle | target changed | — |
| Artifact Recognized | Narration Triggered | OnArtifactIdentified fires | — |
| Narration Triggered | Cooldown Active | event consumed | — |
| Cooldown Active | Idle | 30 s elapsed (per artifact) | — |

### 4.3 Constraint guards

- 2-metre detection: `Distance(gazeOrigin, bounds.center) ≤ 2`.
- 1.5-second dwell: `_dwellElapsed ≥ 1.5`.
- 30-second cooldown: `_cooldownUntil[artifact] > Time.time` blocks re-entry.
- Failure transitions: target loss, target change, or cooldown all return to Idle.
- Recovery transitions: missing `Camera.main` returns silently to Idle.

---

## Phase 5 — AI Guide State Diagram

### 5.1 States

| State | Entry Action | Exit Action | Internal Action |
|---|---|---|---|
| Idle | `_modelSpeaking = false` | — | Wait for events |
| Waiting For Artifact | — | — | Subscribed to OnArtifactIdentified |
| Preparing Context | — | — | Compose system message |
| Generating Prompt | — | — | Format JSON payload |
| Sending Request | — | — | `_client.Send(conversation.item.create)`; `_client.Send(response.create)` |
| Waiting For Response | — | — | Wait for `response.created` |
| Receiving Audio Stream | `_modelSpeaking = true`; clear input buffer | — | Enqueue audio deltas |
| Speaking | — | — | Drive `IsSpeaking` via jaw flap |
| Waiting For Question | — | — | Monitor PTT button |
| Processing Question | — | — | Stream PCM16; commit on release; request response |
| Continuing Conversation | — | — | Same as Waiting For Response |
| Ending Conversation | `_modelSpeaking = false`; `_modelSpeakingUntil = Time.time + 0.4` | — | Clear input buffer |

### 5.2 Transitions

| Source | Target | Trigger | Guard | Timeout | Error |
|---|---|---|---|---|---|
| Idle | Waiting For Artifact | session.created received | — | — | — |
| Waiting For Artifact | Preparing Context | OnArtifactIdentified | [client open AND not speaking] | — | drop if not open |
| Preparing Context | Generating Prompt | always | — | — | — |
| Generating Prompt | Sending Request | always | — | — | — |
| Sending Request | Waiting For Response | frame sent | — | — | — |
| Waiting For Response | Receiving Audio Stream | response.created | — | infinite | — |
| Receiving Audio Stream | Speaking | first response.audio.delta enqueued | — | — | ring overflow logs warning |
| Speaking | Speaking | further response.audio.delta | — | — | — |
| Speaking | Ending Conversation | response.done | — | — | — |
| Ending Conversation | Waiting For Question | grace timer set | — | — | — |
| Waiting For Question | Processing Question | PTT pressed | — | — | — |
| Processing Question | Sending Request | PTT released | — | — | — |
| Processing Question | Waiting For Response | response.create sent | — | — | — |
| Any | Ending Conversation | error event | — | — | logged |

### 5.3 Guards and timeouts

- Self-interruption guard: `[!_modelSpeaking && Time.time >= _modelSpeakingUntil]` required before sending a new narration.
- Mute grace: 0.4 s after `response.done`.
- Error handling: an `error` event from the API does not change state directly; it is logged. The next state transition is driven by subsequent valid events or by user PTT input.

---

## Phase 6 — Realtime Voice Communication State Diagram

### 6.1 States

| State | Entry Action | Exit Action | Internal Action |
|---|---|---|---|
| Disconnected | `_socket = null` | — | — |
| Connecting | New `ClientWebSocket`; set Authorization + OpenAI-Beta headers | — | `_socket.ConnectAsync` |
| Connected | Raise `OnOpen`; start send + receive loops | — | Send/receive JSON frames |
| Session Configured | — | — | `session.update` sent; `session.created/updated` received |
| Listening | — | — | Mic gated by input mode; PTT pressed releases stream |
| Push-To-Talk Active | — | — | Mic streams chunks |
| Capturing Audio | — | — | `Microphone.GetData` polling |
| Encoding Audio | — | — | Resample, PCM16, base64 |
| Sending Audio | — | — | `input_audio_buffer.append` |
| Waiting For Response | `input_audio_buffer.commit`; `response.create` | — | — |
| Receiving Audio | `_modelSpeaking = true` | `_modelSpeaking = false` | Decode deltas into ring buffer |
| Buffering Audio | — | — | Audio thread waits until ≥ 200 ms pre-buffer |
| Playing Audio | `_draining = true` | `_draining = false` (on under-run) | `OnPcmRead` consumes samples |
| Conversation Complete | Schedule mute grace | — | — |
| Connection Error | `OnClose(reason)` | — | Log |
| Reconnecting | (not implemented automatically) | — | Operator/dev triggers |
| Closing | `_socket.CloseAsync(NormalClosure)` | — | — |

### 6.2 Transitions and recovery

| Source | Target | Trigger | Failure recovery |
|---|---|---|---|
| Disconnected | Connecting | `ConnectAsync` invoked | — |
| Connecting | Connected | `_socket.ConnectAsync` completes | On exception → Connection Error |
| Connected | Session Configured | `session.created` received | — |
| Session Configured | Listening | greeting `response.done` received | — |
| Listening | Push-To-Talk Active | PTT pressed | — |
| Push-To-Talk Active | Capturing Audio | mic event | — |
| Capturing Audio | Encoding Audio | chunk full | — |
| Encoding Audio | Sending Audio | base64 ready | gate by mute grace |
| Sending Audio | Listening | chunk sent (loop within PTT) | — |
| Push-To-Talk Active | Waiting For Response | PTT released | `commit + response.create` |
| Waiting For Response | Receiving Audio | `response.created` | — |
| Receiving Audio | Buffering Audio | first delta + below pre-buffer | — |
| Buffering Audio | Playing Audio | pre-buffer reached | — |
| Playing Audio | Playing Audio | further deltas | overflow → drop oldest |
| Playing Audio | Buffering Audio | under-run | decay-fade to silence |
| Playing Audio | Conversation Complete | `response.done` | — |
| Conversation Complete | Listening | grace expires | — |
| Connected (any sub) | Connection Error | exception | — |
| Connection Error | Disconnected | cleanup | manual reconnect |
| Connected | Closing | `DisconnectAsync` | — |
| Closing | Disconnected | close complete | — |

### 6.3 Timeout behaviour

- No hard-coded reconnect timer in the current implementation; on `OnClose` the orchestrator logs a warning and the museum continues silent.
- Audio pre-buffer is 4800 samples = 200 ms; reached deterministically when enough deltas have arrived.
- Post-speech mute grace is `postSpeechMuteGrace = 0.4 s`.

---

## Phase 7 — Tour Guide Avatar State Diagram

### 7.1 States

| State | Entry Action | Animation | NavMesh Behaviour | Transitions |
|---|---|---|---|---|
| Initialization | `Awake` caches `_agent`, `_hipBone`; `Start` resolves player | — | `stoppingDistance = 0.3`, `autoBraking = true` | → Idle when player resolved |
| Idle | `IsMoving = false`, `IsSpeaking = false` | Idle animation | `isStopped = true` | → Walk on distToDesired > 0.4; → Talk on `IsSpeaking` |
| Following Visitor | (computed via desired position) | — | `SetDestination(desiredPos)` | — |
| Walking | `IsMoving = true` | Walk animation | NavMeshAgent moves | → Idle when within hysteresis; → Talk on speaking |
| Stopping | `isStopped = true` | Walk → Idle transition over 0.15 s | — | → Idle on velocity ≈ 0 |
| Speaking | `IsSpeaking = true` | Talk animation | continues NavMesh (Talk overlays) | → Idle when `IsSpeaking = false` |
| Returning To Follow | `isStopped = false` | — | new destination | → Walking |
| Disabled | `OnDisable` | — | `isStopped = true` | — |

### 7.2 Transitions and synchronisation

| Source | Target | Guard | Sync |
|---|---|---|---|
| Init | Idle | `Camera.main` resolved | — |
| Idle | Walking | `distToDesired > 0.4` | repath every 0.25 s |
| Walking | Stopping | `distToDesired ≤ 0.4` | `isStopped = true` |
| Stopping | Idle | velocity ≈ 0 | animator transition 0.15 s |
| Idle / Walking | Speaking | `isSpeaking = true` from `AmplitudeJawFlap` | — |
| Speaking | Idle | `isSpeaking = false` | — |
| Any | Disabled | scene unload | — |

### 7.3 Animator detail

The Animator Controller has three states (Idle, Walk, Talk) and two booleans (`IsMoving`, `IsSpeaking`). Transitions use `hasExitTime = false`, `duration = 0.15 s`, and `canTransitionToSelf = false`.

### 7.4 Hip-bone XZ lock (composite of LateUpdate)

A composite-state add-on to all visible states: in every `LateUpdate`, after the Animator writes bone poses, the hip bone's local X and Z are re-snapped to the rest pose. Y is left free. This is the runtime fix for Mixamo's walk-cycle drift documented in the report.

---

## Phase 8 — Database State Diagram

### 8.1 States

| State | Entry Action | Exit Action | Internal Action |
|---|---|---|---|
| Database Closed | — | — | — |
| Database Opening | `DefaultDbPath()`; `Directory.CreateDirectory` | — | `new SQLiteConnection(path, ReadWrite | Create | FullMutex)` |
| Database Ready | `_conn.CreateTable<VisitorRecord>()` | — | — |
| Inserting | `_conn.Insert(record)` | — | Update `Id` |
| Error | log | — | — |
| Recovering | (not automatic; manual or via lobby retry) | — | — |
| Disposed | `_conn.Dispose()` | — | — |

### 8.2 Transitions

| Source | Target | Trigger | Guard |
|---|---|---|---|
| Closed | Opening | `new MuseumDatabase()` | — |
| Opening | Ready | connection opened successfully | — |
| Opening | Error | `SQLiteException` thrown | — |
| Ready | Inserting | `InsertVisitor(record)` | — |
| Inserting | Ready | insert succeeded | — |
| Inserting | Error | insert exception | — |
| Error | Recovering | user-driven retry | — |
| Recovering | Inserting | retry succeeds | — |
| Ready | Disposed | `LobbyController.OnDestroy` → `_db.Dispose()` | — |
| Error | Disposed | scene unload | — |

---

## Phase 9 — Event Catalog

| Event | Source | Target Component | Current State | Next State | Description |
|---|---|---|---|---|---|
| ApplicationStarted | OS | Application | (start) | Application Starting | Process invoked |
| ConfigLoaded | OpenAIConfig | Application | Loading Configuration | Lobby Active | API key successfully read |
| ConfigMissing | OpenAIConfig | Application | Loading Configuration | Lobby Active (degraded) | key missing |
| FormFieldChanged | LobbyController | Application | Visitor Registration | Validating Input | Any field event |
| ValidationPassed | LobbyController | Application | Validating Input | Creating Session | name + key present |
| SubmitPressed | LobbyController | Application | Validating Input | Saving Visitor Data | Begin Tour click |
| DBInsertSuccess | MuseumDatabase | Application | Saving Visitor Data | Creating Session | row persisted |
| DBInsertFailed | MuseumDatabase | Application | Saving Visitor Data | Collecting User Information | exception |
| SceneLoadComplete | Unity | Application | Museum Loading | Museum Active | Museum scene active |
| WebSocketOpen | RealtimeClient | Realtime Voice | Connecting | Connected | `OnOpen` |
| SessionConfigured | RealtimeClient | Realtime Voice | Connected | Session Configured | `session.created` |
| OpeningGreetingDone | RealtimeClient | Realtime Voice | Session Configured | Listening | `response.done` for greeting |
| GazeArtifactIdentified | GazeArtifactDetector | AI Guide | Waiting For Artifact | Preparing Context | OnArtifactIdentified |
| ResponseCreated | RealtimeClient | AI Guide | Sending Request | Receiving Audio Stream | `response.created` |
| AudioDeltaReceived | RealtimeClient | Realtime Voice | Receiving Audio / Buffering | Playing Audio | `response.audio.delta` |
| ResponseDone | RealtimeClient | AI Guide | Speaking | Ending Conversation | `response.done` |
| PTTPressed | InputSystem | Realtime Voice | Listening | Push-To-Talk Active | T key or Y/B button |
| PTTReleased | InputSystem | Realtime Voice | Push-To-Talk Active | Waiting For Response | end-of-press edge |
| ArtifactCooldownExpired | GazeArtifactDetector | Gaze Detector | Cooldown Active | Idle | 30 s elapsed |
| DwellThresholdReached | GazeArtifactDetector | Gaze Detector | Dwell Timer Running | Artifact Recognized | `_dwellElapsed ≥ 1.5` |
| QuitRequested | OS | Application | Museum Active | Tour Ending | OS close |
| WebSocketClosed | RealtimeClient | Realtime Voice | (any) | Disconnected | server close OR client close |
| Error | RealtimeClient | (logged) | (no transition) | (no transition) | logged only |
| RingOverflow | StreamingAudioPlayer | Realtime Audio Player | Playing Audio | Playing Audio (warning) | dropped oldest |
| Underrun | StreamingAudioPlayer | Realtime Audio Player | Playing Audio | Buffering Audio | decay-fade |

---

## Phase 10 — Guard Condition Analysis

| Guard | Purpose | Source State | Target State |
|---|---|---|---|
| `[hasName && hasKey]` | Form fully valid | Validating Input | Saving Visitor Data |
| `[!_modelSpeaking && Time.time >= _modelSpeakingUntil]` | Anti-self-interrupt | Waiting For Artifact | Preparing Context |
| `[Distance(origin, bounds.center) ≤ 2 m]` | Anti-clip-through | Scanning | Artifact Candidate Detected |
| `[_dwellElapsed ≥ 1.5 s]` | Dwell threshold | Dwell Timer Running | Artifact Recognized |
| `[_cooldownUntil[a] ≤ Time.time]` | Per-artifact cooldown free | Cooldown Check | Dwell Timer Running |
| `[Microphone.devices.Length > 0]` | Mic permission granted | Idle (orchestrator) | Listening |
| `[response.audio.delta != null]` | Inbound audio frame | Buffering Audio | Playing Audio |
| `[_available >= prebufferSamples]` | Pre-buffer satisfied | Buffering Audio | Playing Audio |
| `[SessionState.Visitor != null]` | Session exists | Museum Loading | Museum Active |
| `[OS-quit signal received]` | User requested exit | Museum Active | Tour Ending |
| `[inputMode == PushToTalk]` | Input mode gate | Listening | Push-To-Talk Active |
| `[loadAdditively == false]` | Single-mode scene transition | Creating Session | Museum Loading |
| `[distToDesired > 0.4 m]` | Hysteresis exit | Idle (avatar) | Walking |
| `[distToDesired ≤ 0.4 m]` | Hysteresis enter | Walking | Stopping |
| `[rms > silenceThreshold]` | Speaking detection | Idle (avatar) | Speaking |

---

## Phase 11 — Composite State Analysis

### 11.1 Composite: Museum Active

```
[Museum Active]
   ├── Substate: Gaze Pipeline Running
   │      └── (Idle ↔ Scanning ↔ Dwelling ↔ Recognized ↔ Cooldown)
   ├── Substate: AI Interaction Active     (orthogonal region)
   │      └── (Idle ↔ Sending ↔ Receiving ↔ Speaking ↔ Ending)
   ├── Substate: Avatar Following            (orthogonal region)
   │      └── (Init ↔ Idle ↔ Walking ↔ Stopping ↔ Speaking ↔ Disabled)
   └── Substate: Audio Pipeline              (orthogonal region)
          └── (Prebuffering ↔ Draining ↔ Underrun)
```

All four substates run **concurrently** (UML orthogonal regions). They are forked on entry to Museum Active and joined on exit.

### 11.2 Composite: Realtime Voice Connection

```
[Connected]
   ├── Listening
   ├── Push-To-Talk Active
   │      ├── Capturing Audio
   │      ├── Encoding Audio
   │      └── Sending Audio
   ├── Waiting For Response
   ├── Receiving Audio
   ├── Playing Audio
   └── Conversation Complete
```

`Connected` is a single composite that gates all its substates; any error returns to `Connection Error` and then `Disconnected`.

### 11.3 Composite: Visitor Session

`Session Active` is a composite that exists for the entire museum lifetime; it contains the visit's running state but the system does not formally substate it further.

---

## Phase 12 — ABET Documentation Output

### State Diagram Analysis

This section presents the behavioural model of the system using UML State Machine Diagrams. Seven state machines are specified, corresponding to the seven components in the implementation that exhibit explicit state-based behaviour: the application itself, the visitor session, the artifact-recognition pipeline, the AI guide orchestrator, the Realtime voice connection, the tour-guide avatar's animator, and the SQLite database.

#### Figure X.X — Main Application State Diagram

**Purpose.** Capture the application's top-level lifecycle from process start to process exit.
**States.** Application Starting, Loading Configuration, Lobby Active, Visitor Registration, Validating Input, Creating Session, Museum Loading, Museum Active, AI Interaction Active, Tour Ending, Application Shutdown.
**Entry/Exit Actions.** Specified per state in §2.1. Entry to `Museum Active` opens the WebSocket and sends the English-locked session prompt; exit from `AI Interaction Active` schedules the 0.4 s mute grace.
**Events.** Engine ready; config loaded; submit pressed; insert success/failure; scene loaded; response.created; response.done; OS quit.
**Guards.** Form validity gate; anti-self-interrupt gate; single-mode-load enforcement.
**Internal Activities.** Per-frame loops in Museum Active.
**Transitions.** Specified per row in §2.2.
**Final State.** Application Shutdown → process exit.
**Design Rationale.** The transition from Lobby Active to Museum Active is hard-gated through Creating Session because the rest of the application depends on `SessionState.Visitor` being populated.

#### Figure X.X — Visitor Session State Diagram

**Purpose.** Track the lifecycle of one visitor session from form open to database commit and session disposal.
**States.** Session Not Created → Collecting → Validating → Saving → Created → Active → Closed.
**Events.** Form field changed; Submit pressed; insert success/failure; quit.
**Guards.** Validation gate; database success gate.
**Final State.** Session Closed when the application exits.
**Design Rationale.** The Validating state allows the lobby to round-trip between Collecting and Validating freely as the user edits, but only the explicit Submit press progresses toward Saving.

#### Figure X.X — Artifact Recognition State Diagram

**Purpose.** Model the per-frame gaze pipeline as a state machine with deterministic transitions on guard evaluation.
**States.** Idle, Scanning, Artifact Candidate Detected, Cooldown Check, Dwell Timer Running, Artifact Verification, Artifact Recognized, Narration Triggered, Cooldown Active.
**Constraint Guards.** 2 m distance gate, 1.5 s dwell threshold, 30 s per-artifact cooldown.
**Loop Behaviour.** Every Unity frame the detector either remains in Idle/Scanning or progresses through the dwell pipeline; on identification it raises an event and returns to Cooldown Active for that specific artifact for 30 s.
**Failure / Recovery Transitions.** Loss of target, cooldown, or out-of-range conditions transition cleanly back to Idle without throwing.

#### Figure X.X — AI Tour Guide State Diagram

**Purpose.** Capture the conversational lifecycle of the AI guide subsystem.
**States.** Idle, Waiting For Artifact, Preparing Context, Generating Prompt, Sending Request, Waiting For Response, Receiving Audio Stream, Speaking, Waiting For Question, Processing Question, Continuing Conversation, Ending Conversation.
**Internal Processing.** Speaking state drives `_modelSpeaking = true` and clears the server-side input buffer to prevent echo loops.
**Timeout Conditions.** Post-speech mute grace of 0.4 s; no API-side timeout enforced by the client (the API may issue `error` events, which are logged).
**Error Conditions.** WebSocket closure transitions to Idle by way of `OnClose` callback.

#### Figure X.X — Realtime Voice Communication State Diagram

**Purpose.** Model the WebSocket connection lifecycle and the bidirectional audio sub-states.
**States.** Disconnected, Connecting, Connected (composite), Session Configured, Listening, Push-To-Talk Active (composite), Capturing Audio, Encoding Audio, Sending Audio, Waiting For Response, Receiving Audio, Buffering Audio, Playing Audio, Conversation Complete, Connection Error, Closing.
**Failure Recovery.** On `OnClose` the connection transitions through Connection Error to Disconnected; the museum continues to render but the guide is silent until a manual reconnect (not implemented automatically in the production build).
**Timeout Behaviour.** Audio pre-buffer is 200 ms (4 800 samples at 24 kHz); under-run triggers a decay-to-silence transient.

#### Figure X.X — Tour Guide Avatar State Diagram

**Purpose.** Specify the Mixamo avatar's Animator state machine plus the NavMesh-driven behaviour, including the hip-bone XZ-lock that runs in `LateUpdate` as a runtime safety net.
**States.** Initialization, Idle, Walking, Stopping, Speaking, Returning To Follow, Disabled.
**Animation States.** Idle / Walk / Talk inside the Animator Controller with two boolean parameters.
**NavMesh Behaviour.** `stoppingDistance = 0.3` on the agent; the orchestrator computes a separate `stoppingDistance = 1.8 m` social-distance target through the `desiredPos` formula; the 0.4 m hysteresis prevents oscillation at the boundary.
**Synchronisation with Audio.** `IsSpeaking` is driven by `AmplitudeJawFlap.LateUpdate` reading `AudioSource.GetOutputData` and computing an RMS amplitude against `silenceThreshold`.

#### Figure X.X — Database State Diagram

**Purpose.** Model the SQLite connection lifecycle.
**States.** Database Closed, Opening, Ready, Inserting, Error, Recovering, Disposed.
**Recovery Logic.** Insert failure transitions to Error; the lobby's exception handler re-enables Submit, allowing the user to retry without re-typing.
**Final State.** Disposed (on `LobbyController.OnDestroy`).

#### Design Rationale

The state-machine decomposition above respects two architectural commitments documented in the report. First, **state-bearing components are localised**: each state machine is owned by a single class, eliminating ambiguity about who advances the state. Second, **failure modes are first-class states**: Connection Error in the Realtime Voice machine, Error in the Database machine, and the cooldown-and-rejection paths in the Gaze Detector machine are all explicit states rather than implicit exception handlers. This makes the failure semantics auditable in the diagrams and the report. The four-region composite `Museum Active` is the diagrammatic representation of the project's central architectural property: four cooperating, independently advancing state machines (gaze, AI, avatar, audio) running concurrently on a single Unity main loop.

---

**End of UML State Diagram Specification.**
