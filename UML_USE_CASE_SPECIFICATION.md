# UML Use Case Diagram Specification

> **Project:** AI-Assisted Virtual Reality Museum for Cultural Heritage Education — A Real-Time Speech-Enabled Cultural Heritage Experience for Meta Quest and Desktop Simulator Platforms
>
> **Purpose:** Complete UML Use Case Specification document, derived from the actual implementation in `PROJECT_REPORT.md`, ready for direct conversion into professional UML Use Case Diagrams in StarUML, Enterprise Architect, Visual Paradigm, Lucidchart, Draw.io, PlantUML, or Visio.

---

## Phase 1 — System Requirements Analysis

### 1.1 Functional requirements (extracted from the project report)

| ID | Functional Requirement |
|---|---|
| F1 | Present a 3D Egyptian museum environment in stereoscopic VR with full 6-DoF head tracking. |
| F2 | Provide locomotion through the museum via both teleportation and smooth thumbstick movement. |
| F3 | Collect the visitor's name, age, and country in a VR-native lobby form; validate inputs; persist locally. |
| F4 | Identify which of 42 catalogued artifacts the visitor is looking at, with no explicit user input, on a 1.5 s dwell threshold. |
| F5 | On identification, display a floating world-space label with name and era. |
| F6 | On identification, push the artifact's name/era/description to the AI guide and request a 1–2 sentence spoken narration. |
| F7 | Allow the visitor to interrupt the guide and ask a follow-up question via the Y / B controller button or the keyboard T key. |
| F8 | Present a 3D animated humanoid character (Mixamo Pharaoh) that follows the visitor at a comfortable 1.8 m social distance. |
| F9 | Animate the character through an Idle / Walk / Talk state machine, synchronised with the AI's audio output. |
| F10 | Preserve session state (visitor identity, API key, optional Realtime connection) across the Lobby → Museum scene transition. |
| F11 | Produce a portable production build deployable on a USB drive with no per-machine setup required. |

### 1.2 User goals

- Provide identity once at the lobby and proceed to the museum.
- Walk through the museum and look at artifacts without any explicit "identify this" gesture.
- Hear contextually relevant narration begin automatically when an artifact is in view.
- Ask follow-up questions verbally and receive spoken answers.
- See a believable embodied guide accompanying the visit.
- Leave the museum at the end of the experience.

### 1.3 System services (the application's externally-visible behaviour)

| Service | Subsystem |
|---|---|
| Render the lobby form | `LobbyController`, `LobbySceneSetup` |
| Persist visitor identity | `MuseumDatabase` → SQLite `museum.db` |
| Manage cross-scene state | `SessionState` (DontDestroyOnLoad) |
| Read configuration | `OpenAIConfig` |
| Render the museum environment in stereo VR | Unity HDRP + OpenXR + XR Origin |
| Provide VR locomotion | XR Interaction Toolkit Starter Assets |
| Identify artifacts | `GazeArtifactDetector` + `ArtifactInfo` |
| Display floating labels | `ArtifactLabelSpawner` |
| Connect to OpenAI Realtime API | `RealtimeClient` |
| Capture microphone audio | `MicCapture` |
| Stream and play received audio | `StreamingAudioPlayer` (30 s ring + 200 ms pre-buffer) |
| Orchestrate AI conversation | `GuideOrchestrator` |
| Follow the visitor with the avatar | `TourGuideAgent` + NavMeshAgent |
| Animate avatar (jaw, walk, talk) | `AmplitudeJawFlap` + Animator Controller |
| Build the production package | `PortablePackageBuilder` |

### 1.4 External systems

- **OpenAI Realtime API** (`wss://api.openai.com/v1/realtime?model=gpt-4o-mini-realtime-preview`) — speech-to-speech inference and audio generation, accessed over a persistent WebSocket.
- **Meta Quest Hardware + Meta Quest Link runtime** — provides head/hand tracking and controller input through OpenXR.
- **Microphone** and **Speakers/Headphones** — OS-mediated audio I/O devices.
- **Local filesystem** — hosts the SQLite database file and the `config.json` configuration file.

### 1.5 Human actors

- **Visitor** — the human wearing the Meta Quest headset, the primary user of the system.
- **Operator (Deployment Operator)** — the human responsible for installing the build and provisioning the OpenAI API key file prior to deployment.

### 1.6 Non-human actors

- **OpenAI Realtime API** — the external conversational AI service.
- **Meta Quest Hardware** — the headset and controllers as a system actor providing pose and button events through OpenXR.
- **Local filesystem** — present to acknowledge data-store interactions but typically not drawn as a UML actor.

### 1.7 System boundary

The UML system boundary encloses everything the Unity application owns at runtime: the Lobby and Museum scenes, all subsystems documented in §1.3, and the in-process SQLite database. The OpenAI Realtime API, the Meta Quest hardware (treated as a system actor at the OpenXR API boundary), and the Operator-deposited configuration file are explicitly outside the boundary.

---

## Phase 2 — Actor Identification

### A1: Visitor

| Attribute | Detail |
|---|---|
| Description | The human wearing the Meta Quest headset; the primary consumer of the museum experience. |
| Responsibilities | Provide identity at the lobby; navigate the museum; direct attention through head pose; hold the push-to-talk button to ask questions. |
| Goals | Learn about Egyptian artifacts in an immersive setting; receive personalised narration and follow-up answers. |
| Interactions With System | Lobby form (name, age, country, Submit), continuous and teleport locomotion, head gaze toward artifacts, push-to-talk audio, speakers receiving narration. |
| Type | **Primary actor.** Appears in every Use Case Diagram in the report. |

### A2: Operator

| Attribute | Detail |
|---|---|
| Description | The deployment / installation operator (curator, IT staff, museum administrator). Not present at runtime once the configuration file is in place. |
| Responsibilities | Deposit `config.json` containing the OpenAI API key alongside the executable; install the build folder on the target machine; connect a Meta Quest. |
| Goals | Make the application runnable on the target PC without per-user setup. |
| Interactions With System | One-time filesystem-mediated configuration before runtime. |
| Type | **Primary actor of administrative use cases.** Appears in the Session Management Use Case Diagram. |

### A3: OpenAI Realtime API

| Attribute | Detail |
|---|---|
| Description | External cloud service hosting the `gpt-4o-mini-realtime-preview` model. |
| Responsibilities | Receive session configuration and audio inputs; produce conversational audio responses; emit JSON control events. |
| Goals | Provide low-latency speech-to-speech inference. |
| Interactions With System | Bidirectional WebSocket exchange of JSON events (`session.update`, `input_audio_buffer.*`, `conversation.item.create`, `response.create`, `response.audio.delta`, `response.done`, `error`). |
| Type | **Secondary (system) actor.** Appears in the Main and AI Tour Guide Use Case Diagrams. |

### A4: Meta Quest Hardware

| Attribute | Detail |
|---|---|
| Description | The Meta Quest 2 / 3 / Pro headset and its two motion controllers, exposed to the application through the OpenXR runtime via Meta Quest Link. |
| Responsibilities | Track head and controller 6-DoF pose; emit button events; render the application's stereo frames; pass through audio to the visitor's headphones. |
| Goals | Faithful interactive presentation of the VR experience to the visitor. |
| Interactions With System | Continuous pose stream and button events; rendered frames in return. |
| Type | **Secondary (system) actor.** Appears at the boundary of every interactive use case. |

### A5: System Administrator (optional / future scope)

| Attribute | Detail |
|---|---|
| Description | A hypothetical post-deployment maintainer who rotates the API key, ships hot-fix builds, and reviews visitor records. |
| Responsibilities | Maintain the deployed installation. |
| Goals | Keep the experience operational and the database accurate over time. |
| Interactions With System | Filesystem (config rotation, log review) and the SQLite file. |
| Type | **Primary actor of administrative use cases.** Optionally appears in the Session Management diagram; not required in the minimal academic figure. |

### Actors explicitly **excluded** from the production-edition diagram

- **XR Device Simulator user.** The simulator code path was removed in the production edition; the actor is therefore not drawn.
- **Desktop user (mouse + keyboard).** Same — the production deliverable targets Meta Quest only.

These two actors may be reintroduced in any future product edition that re-enables the simulator path; the architecture supports it but the present deployment does not include it.

---

## Phase 3 — Main System Use Case Diagram

The following use cases are drawn at the system level. Identifier ranges:

- UC-01 to UC-19: Visitor-facing.
- UC-20 to UC-29: AI guide-facing.
- UC-30 to UC-39: Administrative and infrastructural.

| ID | Name | Goal | Primary Actor | Secondary Actors | Preconditions | Postconditions | Basic Flow | Alternative Flow | Exceptions |
|---|---|---|---|---|---|---|---|---|---|
| UC-01 | Launch Application | Start the executable | Visitor (or Operator) | Local filesystem | Build folder present on disk; Meta Quest connected | Application is running; Lobby scene loaded | 1. OS invokes `MuseumVR.exe`. 2. Engine bootstraps. 3. Configuration loaded. 4. Lobby scene rendered. | — | If config missing, lobby displays a red status message and the Submit button remains disabled. |
| UC-02 | Configure System | Load OpenAI key + database location | Operator (one-time); Application (runtime) | Local filesystem | `config.json` present at portable or %APPDATA% path | API key cached in `SessionState`; database path resolved | 1. `OpenAIConfig.GetConfigPath()` resolves portable-first. 2. `TryLoadKey` reads JSON. 3. Key trimmed and validated. 4. `_session.OpenAiApiKey = key`. | Fall back to %APPDATA% path if portable not present. | File missing or `openai_api_key` empty → status message; degraded mode. |
| UC-03 | Register Visitor | Capture name, age, country | Visitor | Local filesystem (via UC-04) | Lobby active; valid API key | New row in `museum.db`; `SessionState.Visitor` populated | 1. Visitor types name. 2. Adjusts age slider. 3. Selects country (default: Egypt). 4. Presses Submit. 5. Record inserted; session populated. | Visitor can re-edit any field before pressing Submit. | Database insert exception → re-enable Submit, status "Database error". |
| UC-04 | Validate Input | Ensure name + key present | Application | — | Lobby active | Submit interactability flag set | 1. `hasName = !IsNullOrWhiteSpace(name)`. 2. `hasKey = !IsNullOrEmpty(apiKey)`. 3. `submitButton.interactable = hasName && hasKey`. | — | — |
| UC-05 | Store Visitor Record | Persist visitor row | Application | SQLite | `VisitorRecord` constructed | New row with auto-incremented `Id` | 1. `_db.InsertVisitor(record)` opens connection (if needed). 2. `_conn.Insert(record)`. 3. Returns `record.Id`. | — | `SQLiteException` → propagated to UC-03. |
| UC-06 | Create Session | Seed cross-scene singleton | Application | — | `VisitorRecord` inserted | `SessionState.Visitor` populated, marked DontDestroyOnLoad | 1. `SessionState.GetOrCreate()`. 2. `_session.Visitor = record`. | — | — |
| UC-07 | Transition To Museum | Load Museum.unity | Application | — | Session created | Museum scene active, Lobby destroyed | 1. `SceneManager.LoadSceneAsync(museumSceneName, LoadSceneMode.Single)`. 2. Lobby destroyed. 3. SessionState survives. | — | Scene-load failure → engine-level error. |
| UC-08 | Open Realtime Connection | Connect WebSocket | Application | OpenAI Realtime API | Museum scene active; API key valid | WebSocket open and configured | 1. `new RealtimeClient()`. 2. `_client.ConnectAsync(apiKey, url)`. 3. `ConfigureSession()` sends `session.update`. 4. `response.create` greeting requested. | — | Connect throws → museum runs silent; `enabled = false`. |
| UC-09 | Navigate Museum | Move through the environment | Visitor | Meta Quest Hardware | Museum scene active | Visitor's XR Origin transform updates | 1. Left thumbstick → continuous locomotion. 2. Left thumbstick forward + release → teleport with comfort vignette. 3. Right thumbstick → 30° snap turn. | — | Teleport ray misses → no destination set. |
| UC-10 | Observe Artifact | Direct head gaze at artifact | Visitor | Meta Quest Hardware | Visitor within 2 m of artifact | Gaze detector accumulates dwell | 1. Visitor turns head toward artifact. 2. Forward ray hits collider with `ArtifactInfo`. 3. Bounds-centre check passes. 4. Dwell timer starts. | Looking away resets timer. | Ray clips through wall → bounds-centre check rejects. |
| UC-11 | Identify Artifact | Fire identification event after dwell + cooldown | Application | — | Dwell ≥ 1.5 s; not on cooldown | `OnArtifactIdentified` event raised | 1. `_dwellElapsed ≥ 1.5`. 2. `_cooldownUntil[artifact] = Time.time + 30`. 3. Event invoked. | — | Artifact on cooldown → suppressed silently. |
| UC-12 | Display Artifact Label | Spawn world-space label with name + era | Application | — | Identification event received | Label visible in scene | 1. Compute bounds-top. 2. Instantiate `ArtifactLabel.prefab`. 3. Bind `displayName`, `era`. 4. Auto-fade after configured duration. | — | — |
| UC-13 | Receive AI Narration | Hear spoken description of identified artifact | Visitor | OpenAI Realtime API | Identification event; WebSocket open; guide not currently speaking | Audio narration played at avatar head bone | 1. `GuideOrchestrator` composes context. 2. Sends `conversation.item.create` + `response.create`. 3. Receives `response.audio.delta` stream. 4. Ring buffer decoded; AudioSource plays. | — | Guide currently speaking → narration skipped (anti-self-interrupt). |
| UC-14 | Ask Question | Hold push-to-talk to speak | Visitor | Meta Quest Hardware | WebSocket open | Audio chunks streamed; question committed | 1. Visitor holds Y / B / T. 2. `MicCapture` polls mic. 3. Resample to 24 kHz PCM16 base64. 4. Chunks sent as `input_audio_buffer.append`. 5. On release, `input_audio_buffer.commit` + `response.create`. | If model currently speaking when PTT pressed, send `response.cancel`. | Mic permission denied → no audio captured. |
| UC-15 | Receive AI Response | Hear AI's answer | Visitor | OpenAI Realtime API | Question committed | Audio response played | Same flow as UC-13 streaming. | — | Server-side error event → logged; visitor hears no response. |
| UC-16 | Interact With Tour Guide | See the embodied guide following | Visitor | — | Museum scene active | Avatar maintains 1.8 m social distance, animates Idle/Walk/Talk | 1. `TourGuideAgent.Update` computes `desiredPos`. 2. NavMeshAgent pathfinds. 3. Animator booleans update. 4. `LateUpdate` re-snaps hip XZ. | If player teleports onto agent, fallback direction = transform.forward. | NavMesh missing → agent stationary; logged. |
| UC-17 | Continue Tour | Walk to next artifact | Visitor | Meta Quest Hardware | Previous narration ended (or visitor walks away) | Loop back to UC-09 | 1. Visitor moves. 2. New gaze targets become eligible. | — | — |
| UC-18 | Exit Museum | Quit the application | Visitor | OS | Museum active | Application begins shutdown | 1. Visitor closes via OS shortcut. 2. `OnDestroy` chain begins. | — | — |
| UC-19 | Terminate Session | Clean up WebSocket + DB | Application | OpenAI Realtime API; SQLite | Quit signal received | Connection closed; DB disposed | 1. `_client.DisconnectAsync`. 2. `_db.Dispose()` (already disposed on Lobby unload). 3. Process exits. | — | DisconnectAsync exception → swallowed and logged. |
| UC-20 | Generate Artifact Narration | AI produces narration | OpenAI Realtime API | Application | Context message + `response.create` received | Audio stream produced | Server-side inference. | — | Error event returned. |
| UC-21 | Answer Visitor Question | AI produces follow-up answer | OpenAI Realtime API | Application | Committed audio question + `response.create` received | Audio stream produced | Same. | — | Same. |
| UC-22 | Stream Voice Response | Decode + play audio | Application | — | `response.audio.delta` events arriving | Audio rendered at head bone | 1. Decode base64. 2. Enqueue to ring buffer. 3. `OnPcmRead` callback consumes. | Under-run → decay-fade. | Overflow → drop oldest with warning. |
| UC-23 | Animate Guide Avatar | Drive `IsSpeaking` from audio amplitude | Application | — | Audio playing | Mouth/body animates | 1. `AmplitudeJawFlap.LateUpdate` reads `GetOutputData`. 2. RMS computed. 3. `IsSpeaking = rms > silenceThreshold`. 4. If jaw bone present, rotate. | — | No jaw bone → still flips `IsSpeaking`. |
| UC-24 | Maintain Conversation Context | Preserve session-level context | Application | OpenAI Realtime API | Session open | Server retains context across responses | 1. `session.update` set once. 2. New `conversation.item.create` messages append to context. | — | Session reset on reconnect. |
| UC-25 | Terminate Conversation | End response cycle | Application | OpenAI Realtime API | `response.done` received | `_modelSpeaking = false`; 0.4 s mute grace scheduled | 1. Handle `response.done`. 2. Clear input buffer. 3. Reset counters. | — | — |
| UC-30 | Deploy Build | Operator installs the build | Operator | Local filesystem | Production build folder available | Build present on target machine | 1. Operator copies `MuseumVR-Production/` folder to target. 2. Confirms `MuseumVR/config.json` exists. 3. Connects Meta Quest. | — | Missing config → operator must add file before runtime. |
| UC-31 | Provision API Key | Place `config.json` next to executable | Operator | Local filesystem | Build present | Key readable by application | 1. Operator writes `{ "openai_api_key": "sk-..." }`. | Key may also live at %APPDATA% fallback. | Invalid JSON → application logs error and refuses to start the guide. |
| UC-32 | Inspect Visitor Records | Open SQLite file externally | Operator (post-tour) | SQLite | Database file present | Records readable | 1. Operator opens `museum.db` in a SQLite browser. | — | File locked while application running → wait until exit. |

---

## Phase 4 — Visitor Use Case Analysis (Detailed)

### UC-01a — Register Visitor (Detailed)

- **Objective:** Capture and persist the visitor's identity.
- **Trigger:** Lobby form rendered; visitor focuses the name field.
- **Primary Actor:** Visitor.
- **Preconditions:** Configuration loaded; SQLite available.
- **Postconditions:** New row persisted; `SessionState.Visitor` populated; Submit button enters disabled-then-clicked state.
- **Main Success Scenario:**
  1. Visitor types name into the TMP_InputField.
  2. Visitor sets age via the slider (range 5–99, default 25).
  3. Visitor selects country via the dropdown (default: Egypt).
  4. Visitor presses **Begin Tour**.
  5. `LobbyController.OnSubmit` disables Submit, constructs `VisitorRecord`, calls `_db.InsertVisitor`.
  6. `_session.Visitor = record`.
  7. `SceneManager.LoadSceneAsync(museumSceneName, LoadSceneMode.Single)`.
- **Alternative Scenarios:**
  - Visitor edits fields multiple times before Submit; each change re-evaluates `UpdateSubmitInteractable`.
  - API key not loaded → Submit grayed out; visitor cannot proceed until Operator fixes configuration.
- **Exception Scenarios:**
  - `SQLiteException` during insert → status text "Database error", Submit re-enabled, visitor can retry.

### UC-02a — Start Museum Experience

- **Objective:** Trigger the transition into the museum and the conversational guide.
- **Trigger:** Submit completed, record inserted.
- **Primary Actor:** Visitor (implicit).
- **Preconditions:** UC-01 succeeded.
- **Postconditions:** Museum scene loaded; WebSocket connecting; greeting requested.
- **Main Success Scenario:**
  1. `LoadSceneAsync(Single)` completes.
  2. Museum scene Awake/Start chain runs.
  3. `GuideOrchestrator.Start` reads `SessionState.OpenAiApiKey`, constructs `RealtimeClient`, connects.
  4. `ConfigureSession` sends `session.update`.
  5. Opening greeting `response.create` issued.
- **Alternative Scenarios:** API key missing at runtime → fall back to `OpenAIConfig.TryLoadKey`.
- **Exception Scenarios:** Connect failure → `enabled = false`; museum runs without guide.

### UC-03a — Navigate Museum

- **Objective:** Move through the museum environment.
- **Trigger:** Visitor uses thumbsticks.
- **Primary Actor:** Visitor.
- **Preconditions:** Museum active; XR Origin functional.
- **Postconditions:** Visitor position updated.
- **Main Success Scenario:** Continuous locomotion, snap-turn, teleport as documented.
- **Alternative Scenarios:** Teleport ray hits a non-Teleport-area surface → teleport rejected.
- **Exception Scenarios:** Controller disconnected → no input.

### UC-04a — Observe Artifact

- **Objective:** Direct attention toward an artifact for identification.
- **Trigger:** Visitor turns head toward an artifact within 2 m.
- **Primary Actor:** Visitor.
- **Preconditions:** Visitor inside museum; not on cooldown for that artifact.
- **Postconditions:** Dwell timer accumulates.
- **Main Success Scenario:** Ray hits artifact's collider; bounds-centre passes; same target across frames; dwell accrues.
- **Alternative Scenarios:** Visitor looks away → dwell resets.
- **Exception Scenarios:** Ray hits wall first → no identification.

### UC-05a — Trigger Artifact Recognition

- **Objective:** Fire identification event after dwell threshold.
- **Trigger:** `_dwellElapsed ≥ 1.5 s`.
- **Primary Actor:** Application (on visitor's behalf).
- **Preconditions:** UC-04 active.
- **Postconditions:** `OnArtifactIdentified` raised.
- **Main Success Scenario:** Set cooldown timestamp; reset dwell; invoke event; both label and orchestrator handle.

### UC-06a — Listen To Narration

- **Objective:** Hear the AI's spoken description.
- **Trigger:** `response.audio.delta` events begin arriving.
- **Primary Actor:** Visitor.
- **Preconditions:** WebSocket open; `response.create` issued from UC-05.
- **Postconditions:** Visitor hears narration through 3D-positioned AudioSource at avatar head bone.
- **Main Success Scenario:** Audio streamed through ring buffer; played by audio thread; mouth animates.
- **Alternative Scenarios:** Visitor walks away during narration; audio continues to play.
- **Exception Scenarios:** Under-run → decay-to-silence; overflow → oldest dropped + console warning.

### UC-07a — Ask Question

- **Objective:** Pose a follow-up question via PTT.
- **Trigger:** Visitor holds Y / B (controller) or T (keyboard).
- **Primary Actor:** Visitor.
- **Preconditions:** WebSocket open.
- **Postconditions:** Audio question committed; `response.create` issued.
- **Main Success Scenario:** Mic chunks streamed while held; on release, commit + response request.
- **Alternative Scenarios:** PTT pressed while guide speaking → `response.cancel` sent first.
- **Exception Scenarios:** Mic permission denied → no audio captured; visitor sees no error in VR.

### UC-08a — Receive AI Response

Same flow as UC-06a but triggered by UC-07.

### UC-09a — Continue Tour

- **Objective:** Walk to the next artifact, repeat the loop.
- **Trigger:** Visitor moves and looks elsewhere.
- **Primary Actor:** Visitor.
- **Postconditions:** Loop returns to UC-04.

### UC-10a — End Tour

- **Objective:** Exit the application.
- **Trigger:** OS-level quit.
- **Primary Actor:** Visitor.
- **Postconditions:** WebSocket closed; database disposed; process exits.

---

## Phase 5 — Artifact Recognition Use Cases

### UC-G1 — Cast Gaze Ray

- **Purpose:** Determine which collider the visitor's head is currently pointed at.
- **Actor:** Application.
- **Inputs:** Head transform (`Camera.main.transform`).
- **Outputs:** `RaycastHit` or none.
- **Dependencies:** Unity Physics.
- **Associated Use Cases:** UC-G2, UC-G3.

### UC-G2 — Verify Distance / Bounds Centre

- **Purpose:** Reject hits whose actual bounds centre is beyond 2 m.
- **Actor:** Application.
- **Inputs:** Hit collider, `ArtifactInfo`.
- **Outputs:** Pass/fail boolean.
- **Dependencies:** Renderer bounds encapsulation.

### UC-G3 — Check Dwell Time

- **Purpose:** Accumulate dwell across consecutive frames on the same target.
- **Actor:** Application.
- **Inputs:** Per-frame `Time.deltaTime`.
- **Outputs:** Updated `_dwellElapsed`; threshold-reached event when ≥ 1.5 s.

### UC-G4 — Check Cooldown

- **Purpose:** Prevent re-identification of an artifact within 30 s of its previous identification.
- **Actor:** Application.
- **Inputs:** `ArtifactInfo`, current `Time.time`, `_cooldownUntil` dictionary.
- **Outputs:** Pass/fail boolean.

### UC-G5 — Retrieve Artifact Metadata

- **Purpose:** Read `displayName`, `era`, `description` from the identified `ArtifactInfo`.
- **Actor:** Application.
- **Inputs:** `ArtifactInfo` reference.
- **Outputs:** Three strings.
- **Associated Use Cases:** UC-12 (label), UC-13 (narration).

### UC-G6 — Display Artifact Label

- **Purpose:** Render the world-space label at the artifact's bounds-top.
- **Actor:** Application.
- **Inputs:** Metadata + bounds-top position.
- **Outputs:** Instantiated label prefab.

### UC-G7 — Initiate Narration

- **Purpose:** Hand off the identified artifact to the AI Guide for narration.
- **Actor:** Application.
- **Inputs:** `ArtifactInfo`.
- **Outputs:** `conversation.item.create` + `response.create` JSON.

---

## Phase 6 — AI Tour Guide Use Cases

### UC-AI1 — Generate Artifact Narration

- **Goal:** Produce a 1–2 sentence spoken introduction for the identified artifact.
- **Actors:** OpenAI Realtime API (primary); Application (secondary).
- **Preconditions:** Session configured with English-locking system prompt; `_modelSpeaking == false`.
- **Postconditions:** Audio stream produced and consumed by client.
- **Normal Flow:** Application sends context + `response.create`; API streams `response.audio.delta` + `response.done`.
- **Alternative Flow:** API may include text channel in parallel (currently logged only).
- **Exceptions:** `error` event from API → logged, no narration delivered.

### UC-AI2 — Answer Visitor Question

- **Goal:** Provide an answer to the visitor's PTT question.
- **Actors:** OpenAI Realtime API; Application.
- **Preconditions:** `input_audio_buffer.commit` + `response.create` received by the API.
- **Postconditions:** Audio response delivered.
- **Normal Flow:** Same as UC-AI1.
- **Exceptions:** Empty audio buffer → API may return error or empty response.

### UC-AI3 — Maintain Conversation Context

- **Goal:** Keep session-level memory so follow-up questions are coherent with earlier narration.
- **Actors:** OpenAI Realtime API.
- **Preconditions:** Session active.
- **Postconditions:** Server retains conversation items across responses.
- **Normal Flow:** Server appends each `conversation.item.create` to context.

### UC-AI4 — Stream Voice Response

- **Goal:** Deliver low-latency audio playback.
- **Actors:** Application.
- **Preconditions:** `response.audio.delta` events arriving.
- **Postconditions:** Audible playback at the avatar's head bone.
- **Normal Flow:** Decode → ring buffer → audio thread → AudioSource.
- **Exceptions:** Under-run / overflow handled with logging + graceful degradation.

### UC-AI5 — Animate Guide Avatar

- **Goal:** Synchronise mouth and body animation with audio output.
- **Actors:** Application.
- **Preconditions:** AudioSource active.
- **Postconditions:** Animator `IsSpeaking` reflects audio amplitude.
- **Normal Flow:** `AmplitudeJawFlap.LateUpdate` reads `GetOutputData`, computes RMS, drives flag.

### UC-AI6 — Continue Discussion

- **Goal:** Allow turn-taking after a response.
- **Actors:** Visitor; OpenAI Realtime API.
- **Preconditions:** `response.done` received; mute grace expired.
- **Postconditions:** Ready for next PTT or next gaze identification.

### UC-AI7 — Terminate Conversation

- **Goal:** Reset speaking state and clear server-side input buffer at end of response.
- **Actors:** Application.
- **Preconditions:** `response.done` received.
- **Postconditions:** `_modelSpeaking = false`; `_modelSpeakingUntil = Time.time + 0.4`; `input_audio_buffer.clear` sent.

---

## Phase 7 — Database and Session Management Use Cases

### UC-DB1 — Open Database

- **Trigger:** `LobbyController.Start`.
- **Flow:** Resolve path (portable-first) → ensure directory → `new SQLiteConnection(path, ReadWrite | Create | FullMutex)` → `CreateTable<VisitorRecord>()`.
- **Outputs:** Open connection.
- **Dependencies:** `gilzoide.sqlite-net`.
- **Exception Handling:** Connection failure propagates to LobbyController; lobby surfaces "Database error".

### UC-DB2 — Insert Visitor Record

- **Trigger:** Submit pressed in lobby.
- **Flow:** Construct `VisitorRecord` → `_db.InsertVisitor(record)` → return `Id`.
- **Outputs:** Auto-incremented `Id`; persisted row.
- **Exception Handling:** Caught in `OnSubmit`; Submit re-enabled.

### UC-DB3 — Dispose Database

- **Trigger:** `LobbyController.OnDestroy` (when Lobby unloads).
- **Flow:** `_db?.Dispose()` closes connection.
- **Outputs:** Closed connection.

### UC-S1 — Create Session State

- **Trigger:** `LobbyController.Start`.
- **Flow:** `SessionState.GetOrCreate()` returns singleton or creates new DontDestroyOnLoad GameObject.
- **Outputs:** Live singleton.

### UC-S2 — Populate Session State

- **Trigger:** Submit succeeded.
- **Flow:** `_session.Visitor = record`; `_session.OpenAiApiKey = key` (already set earlier).
- **Outputs:** Populated session fields.

### UC-S3 — Preserve Session Across Scene Transition

- **Trigger:** Lobby → Museum load.
- **Flow:** GameObject marked DontDestroyOnLoad survives `LoadSceneMode.Single`.
- **Outputs:** Museum scripts read same singleton.

### UC-S4 — Read Session State in Museum

- **Trigger:** `GuideOrchestrator.Start`.
- **Flow:** Read `SessionState.Instance.OpenAiApiKey`; fall back to `OpenAIConfig.TryLoadKey` if missing.
- **Outputs:** Resolved API key for WebSocket auth and visitor name for system prompt.

### UC-S5 — Load Configuration

- **Trigger:** Application startup.
- **Flow:** Portable path → fallback to %APPDATA% → `File.ReadAllText` → `JsonConvert.DeserializeObject<ConfigFile>` → trim/validate.
- **Outputs:** API key string or error.
- **Exception Handling:** Missing/invalid → status message; degraded mode.

---

## Phase 8 — Include / Extend Relationship Analysis

### 8.1 `<<include>>` relationships

`<<include>>` is used when the base use case **always** uses the included behaviour.

| Base Use Case | Included Use Case | Justification |
|---|---|---|
| UC-03 Register Visitor | UC-04 Validate Input | Every registration goes through input validation. |
| UC-03 Register Visitor | UC-05 Store Visitor Record | Every successful registration produces a persisted row. |
| UC-03 Register Visitor | UC-06 Create Session | Every registration seeds the cross-scene session. |
| UC-03 Register Visitor | UC-07 Transition To Museum | Every successful registration transitions to the museum. |
| UC-01 Launch Application | UC-02 Configure System | Every launch loads configuration. |
| UC-01 Launch Application | UC-S1 Create Session State | The singleton is always created at start. |
| UC-08 Open Realtime Connection | UC-S4 Read Session State (key) | Always reads the key. |
| UC-08 Open Realtime Connection | UC-24 Maintain Conversation Context | Session-level configuration always sent. |
| UC-11 Identify Artifact | UC-G3 Check Dwell Time | Always part of the identification pipeline. |
| UC-11 Identify Artifact | UC-G4 Check Cooldown | Always evaluated. |
| UC-11 Identify Artifact | UC-G5 Retrieve Artifact Metadata | Always read on identification. |
| UC-13 Receive AI Narration | UC-22 Stream Voice Response | All narrations are streamed. |
| UC-13 Receive AI Narration | UC-23 Animate Guide Avatar | All narrations drive avatar animation. |
| UC-14 Ask Question | UC-22 Stream Voice Response (downstream of answer) | All answers stream the same way. |
| UC-14 Ask Question | UC-AI2 Answer Visitor Question | Every committed question requests an answer. |
| UC-22 Stream Voice Response | UC-25 Terminate Conversation | Every response cycle ends with `response.done` handling. |

### 8.2 `<<extend>>` relationships

`<<extend>>` is used when the extending behaviour is optional or conditional.

| Extended Use Case | Extending Use Case | Justification |
|---|---|---|
| UC-13 Receive AI Narration | UC-14 Ask Question | The visitor may optionally ask a follow-up. |
| UC-14 Ask Question | UC-AI6 Continue Discussion | The visitor may ask successive questions. |
| UC-11 Identify Artifact | UC-12 Display Artifact Label | The label is part of identification, but conceptually a separable visual output. (Could also be modelled as `<<include>>`; both are defensible — see §12.) |
| UC-09 Navigate Museum | UC-10 Observe Artifact | Observation is optional during navigation. |
| UC-01 Launch Application | UC-30 Deploy Build | Deployment is an Operator-side extension prior to launch. |
| UC-02 Configure System | UC-31 Provision API Key | Provisioning is one-time and optional per launch. |
| UC-19 Terminate Session | UC-DB3 Dispose Database | Database disposal is one termination action among several. |
| UC-13 Receive AI Narration | UC-AI3 Maintain Conversation Context | Context retention extends every narration. |
| UC-G1 Cast Gaze Ray | UC-G2 Verify Distance / Bounds Centre | The verification extends the basic ray cast with a robustness gate. |

### 8.3 Generalisation relationships

| Parent Use Case | Specialised Use Cases | Justification |
|---|---|---|
| Stream Voice Response (abstract) | UC-13 Receive AI Narration; UC-15 Receive AI Response | Both specialise the same underlying audio-streaming behaviour. |
| Configure System (abstract) | UC-02 Application configuration; UC-31 Operator provisioning | Different responsibility splits between Application and Operator. |

---

## Phase 9 — System Boundary Analysis

### 9.1 Inside the boundary (VR Museum System)

- All Unity GameObjects in both scenes.
- All MonoBehaviours in `Museum.<area>` namespaces (`Lobby`, `Persistence`, `Session`, `Config`, `Artifact`, `Voice`, `Guide`, `Util`).
- The in-process SQLite engine.
- The in-memory ring buffer and cooldown map.
- Both scenes (Lobby and Museum) plus the world-space lobby canvas.
- The Mixamo avatar and its NavMesh.

### 9.2 Outside the boundary

- The OpenAI Realtime API endpoint and the inference model running there.
- The Meta Quest Hardware and the Meta Quest Link runtime.
- The OS audio drivers, microphone, and speakers.
- The local filesystem holding `config.json` and `museum.db`.
- The Operator's workstation environment.

### 9.3 External-system interactions at the boundary

| Boundary | Protocol | Direction |
|---|---|---|
| Application ↔ OpenAI Realtime API | WebSocket over TLS, JSON frames | Bidirectional |
| Application ↔ Meta Quest Hardware | OpenXR runtime | Bidirectional (pose in, frames out) |
| Application ↔ Microphone | `UnityEngine.Microphone` | Inbound |
| Application ↔ Speakers | `UnityEngine.AudioSource` | Outbound |
| Application ↔ Filesystem | `System.IO.File` + SQLite | Bidirectional |

### 9.4 Final UML boundary description

In the UML Use Case Diagram, the system boundary is drawn as a labelled rectangle titled **"VR Museum System"** with the Visitor and Operator (and optionally the System Administrator) on the left, and the OpenAI Realtime API and Meta Quest Hardware on the right. Connecting lines (association edges) cross the boundary only between actors and their owned use cases — never directly between actors. Include and extend relationships are drawn only between use cases inside the boundary.

---

## Phase 10 — Use Case Prioritisation

| Tier | Definition | Use Cases |
|---|---|---|
| **Core (Must-Have)** | Without these, the visitor cannot complete a meaningful museum visit. | UC-01, UC-02, UC-03, UC-04, UC-05, UC-06, UC-07, UC-08, UC-09, UC-10, UC-11, UC-12, UC-13, UC-22, UC-23, UC-25, UC-G1, UC-G3, UC-G4, UC-G5, UC-AI1, UC-AI4, UC-AI5, UC-S1, UC-S2, UC-S3, UC-S4, UC-S5, UC-DB1, UC-DB2, UC-DB3 |
| **Supporting (Should-Have)** | Augment the core experience but the system is still functional without them. | UC-14, UC-15, UC-16, UC-17, UC-18, UC-19, UC-21, UC-24, UC-AI2, UC-AI3, UC-AI6, UC-AI7, UC-G2 (verification), UC-G6, UC-G7 |
| **Administrative** | Performed by the Operator (or future System Administrator) outside the runtime user-experience loop. | UC-30, UC-31, UC-32 |
| **Optional / Future Scope** | Considered but not required for the production-edition deliverable. | A non-VR (desktop) experience; a multi-user shared tour; on-device ML recognition replacing the OpenAI dependency; standalone-Quest Android build. |

---

## Phase 11 — Use Case Matrix

| ID | Name | Primary Actor | Secondary Actor | Priority | Complexity | Dependencies | Expected Outcome |
|---|---|---|---|---|---|---|---|
| UC-01 | Launch Application | Visitor | Local filesystem | Core | Low | UC-02 | Lobby visible |
| UC-02 | Configure System | Application | Filesystem | Core | Low | — | API key cached |
| UC-03 | Register Visitor | Visitor | SQLite | Core | Medium | UC-04, UC-05, UC-06 | Row persisted; session created |
| UC-04 | Validate Input | Application | — | Core | Low | UC-02 | Submit interactable |
| UC-05 | Store Visitor Record | Application | SQLite | Core | Low | UC-DB1 | Persisted row |
| UC-06 | Create Session | Application | — | Core | Low | UC-S1 | SessionState populated |
| UC-07 | Transition To Museum | Application | — | Core | Low | UC-06 | Museum scene loaded |
| UC-08 | Open Realtime Connection | Application | OpenAI Realtime API | Core | High | UC-S4 | WebSocket open |
| UC-09 | Navigate Museum | Visitor | Meta Quest Hardware | Core | Medium | UC-07 | XR Origin moves |
| UC-10 | Observe Artifact | Visitor | Meta Quest Hardware | Core | Low | UC-09 | Dwell accrues |
| UC-11 | Identify Artifact | Application | — | Core | Medium | UC-G1..UC-G5 | OnArtifactIdentified |
| UC-12 | Display Artifact Label | Application | — | Core | Low | UC-11 | Label visible |
| UC-13 | Receive AI Narration | Visitor | OpenAI Realtime API | Core | High | UC-11, UC-08, UC-22 | Audio plays |
| UC-14 | Ask Question | Visitor | Meta Quest Hardware; OpenAI | Supporting | High | UC-08 | Question committed |
| UC-15 | Receive AI Response | Visitor | OpenAI Realtime API | Supporting | High | UC-14, UC-22 | Audio answer plays |
| UC-16 | Interact With Tour Guide | Visitor | — | Supporting | Medium | UC-07 | Avatar follows |
| UC-17 | Continue Tour | Visitor | — | Supporting | Low | UC-09 | Loop |
| UC-18 | Exit Museum | Visitor | OS | Supporting | Low | UC-09 | Quit signal |
| UC-19 | Terminate Session | Application | — | Supporting | Low | UC-18 | Clean shutdown |
| UC-20 | Generate Artifact Narration | OpenAI Realtime API | Application | Core | High | UC-08 | Audio stream produced |
| UC-21 | Answer Visitor Question | OpenAI Realtime API | Application | Supporting | High | UC-14 | Audio stream produced |
| UC-22 | Stream Voice Response | Application | — | Core | High | UC-08 | Audio rendered |
| UC-23 | Animate Guide Avatar | Application | — | Core | Medium | UC-22 | Mouth/body animated |
| UC-24 | Maintain Conversation Context | Application | OpenAI Realtime API | Supporting | Low | UC-08 | Server retains context |
| UC-25 | Terminate Conversation | Application | OpenAI Realtime API | Core | Low | UC-22 | Mute grace scheduled |
| UC-30 | Deploy Build | Operator | Filesystem | Administrative | Low | — | Build present on target |
| UC-31 | Provision API Key | Operator | Filesystem | Administrative | Low | UC-30 | Key file in place |
| UC-32 | Inspect Visitor Records | Operator | SQLite | Administrative | Low | UC-DB1, UC-DB2 | Records reviewed |
| UC-G1 | Cast Gaze Ray | Application | — | Core | Low | UC-09 | RaycastHit |
| UC-G2 | Verify Distance / Bounds | Application | — | Supporting | Low | UC-G1 | Pass/fail |
| UC-G3 | Check Dwell Time | Application | — | Core | Low | UC-G1 | Dwell elapsed |
| UC-G4 | Check Cooldown | Application | — | Core | Low | UC-G3 | Pass/fail |
| UC-G5 | Retrieve Artifact Metadata | Application | — | Core | Low | UC-G4 | Strings returned |
| UC-G6 | Display Artifact Label | Application | — | Supporting | Low | UC-G5 | Label spawned |
| UC-G7 | Initiate Narration | Application | OpenAI Realtime API | Core | Medium | UC-G5 | conversation.item.create + response.create sent |
| UC-AI1 | Generate Artifact Narration | OpenAI Realtime API | Application | Core | High | UC-G7 | Narration audio |
| UC-AI2 | Answer Visitor Question | OpenAI Realtime API | Application | Supporting | High | UC-14 | Answer audio |
| UC-AI3 | Maintain Conversation Context | OpenAI Realtime API | — | Supporting | Low | UC-08 | Persistent context |
| UC-AI4 | Stream Voice Response | Application | — | Core | High | UC-22 | Audio rendered |
| UC-AI5 | Animate Guide Avatar | Application | — | Core | Medium | UC-22 | IsSpeaking flag |
| UC-AI6 | Continue Discussion | Visitor; Application | OpenAI Realtime API | Supporting | Medium | UC-25 | Next turn possible |
| UC-AI7 | Terminate Conversation | Application | — | Core | Low | UC-22 | response.done handled |
| UC-DB1 | Open Database | Application | SQLite | Core | Low | UC-02 | Connection open |
| UC-DB2 | Insert Visitor Record | Application | SQLite | Core | Low | UC-DB1 | Row inserted |
| UC-DB3 | Dispose Database | Application | SQLite | Core | Low | UC-DB1 | Connection closed |
| UC-S1 | Create Session State | Application | — | Core | Low | UC-01 | Singleton ready |
| UC-S2 | Populate Session State | Application | — | Core | Low | UC-05 | Visitor cached |
| UC-S3 | Preserve Session Across Transition | Application | — | Core | Low | UC-S1 | Singleton survives |
| UC-S4 | Read Session State in Museum | Application | — | Core | Low | UC-S2 | Key + Visitor read |
| UC-S5 | Load Configuration | Application | Filesystem | Core | Low | UC-31 | Key cached |

---

## Phase 12 — ABET Documentation Output

### Use Case Diagram Analysis

This section presents the system's user-facing services as a structured set of UML Use Case Diagrams. The analysis identifies five actors (Visitor and Operator as human actors; OpenAI Realtime API and Meta Quest Hardware as secondary system actors; and a future System Administrator listed for completeness), enumerates fifty use cases (UC-01 through UC-32 plus the gaze, AI, database, and session families), and specifies their interrelationships through `<<include>>`, `<<extend>>`, and generalisation links. Four diagrams are recommended for inclusion in the report: a master Main System Use Case Diagram, and three focused diagrams covering Visitor Interaction, AI Tour Guide, and Session Management. Each diagram is documented below with purpose, actors, contained use cases, and design rationale.

#### Figure X.X — Main System Use Case Diagram

**Purpose.** A single overview showing the system boundary, the two human actors (Visitor and Operator), the two secondary system actors (OpenAI Realtime API and Meta Quest Hardware), and the high-level service offerings of the application.
**Actors.** Visitor (primary); Operator (administrative); OpenAI Realtime API (secondary system actor); Meta Quest Hardware (secondary system actor).
**Use Cases.** UC-01 Launch Application; UC-02 Configure System; UC-03 Register Visitor; UC-09 Navigate Museum; UC-10 Observe Artifact; UC-11 Identify Artifact; UC-12 Display Artifact Label; UC-13 Receive AI Narration; UC-14 Ask Question; UC-15 Receive AI Response; UC-16 Interact With Tour Guide; UC-18 Exit Museum; UC-30 Deploy Build; UC-31 Provision API Key.
**Preconditions.** Build present on target; API key file readable; Meta Quest connected.
**Postconditions.** Visitor completes a tour and exits cleanly; visitor record persisted.
**Basic Flow.** Operator deploys and provisions; Visitor launches, registers, navigates, observes, identifies, listens, optionally asks, exits.
**Alternative Flow.** Visitor may register and immediately exit without observing any artifact (a degenerate but valid path).
**Exception Flow.** Connect failure → museum runs without guide; database failure → registration retry.
**Relationships.** UC-01 `<<include>>` UC-02; UC-03 `<<include>>` UC-04, UC-05, UC-06, UC-07; UC-13 `<<extend>>` UC-14; UC-11 `<<extend>>` UC-12; UC-30 precedes UC-31 (sequence association, not include/extend).
**Design Rationale.** The Main diagram is deliberately summary in nature; it surfaces only those use cases whose presence on the diagram aids reader comprehension and omits gaze sub-pipeline mechanics (UC-G1–UC-G5) and audio streaming internals (UC-22–UC-25). Those internals are deferred to the focused diagrams.

#### Figure X.X — Visitor Interaction Use Case Diagram

**Purpose.** Detail the visitor's path through the experience, from registration to exit.
**Actors.** Visitor (primary); Meta Quest Hardware; SQLite (secondary, implicit).
**Use Cases.** UC-01 Launch Application; UC-03 Register Visitor; UC-04 Validate Input; UC-05 Store Visitor Record; UC-06 Create Session; UC-07 Transition To Museum; UC-09 Navigate Museum; UC-10 Observe Artifact; UC-11 Identify Artifact; UC-12 Display Artifact Label; UC-13 Receive AI Narration; UC-14 Ask Question; UC-15 Receive AI Response; UC-17 Continue Tour; UC-18 Exit Museum.
**Relationships.** All `<<include>>` and `<<extend>>` arrows from UC-03 onward and the gaze-pipeline includes are visible here.
**Design Rationale.** This diagram is the one most likely to be reproduced in the report's Chapter 4 (Proposed Model) as a self-contained illustration of the visitor's experience.

#### Figure X.X — AI Tour Guide Use Case Diagram

**Purpose.** Show the conversational subsystem in isolation.
**Actors.** OpenAI Realtime API (primary system actor); Visitor (secondary actor delivering audio input and consuming audio output); Application.
**Use Cases.** UC-08 Open Realtime Connection; UC-20 Generate Artifact Narration; UC-21 Answer Visitor Question; UC-22 Stream Voice Response; UC-23 Animate Guide Avatar; UC-24 Maintain Conversation Context; UC-25 Terminate Conversation; UC-AI6 Continue Discussion; UC-AI7 Terminate Conversation; UC-G7 Initiate Narration (cross-boundary include from Visitor Interaction).
**Relationships.** UC-22 `<<include>>` UC-23 and UC-25; UC-13 `<<extend>>` UC-14 (depicted with a cross-diagram reference); UC-AI1 and UC-AI2 generalised by an abstract "Generate Audio Response" if a generalisation node is desired.
**Design Rationale.** Isolating the AI subsystem in its own diagram makes the WebSocket protocol's event-driven shape visible and prevents the Main diagram from becoming cluttered with network-level details.

#### Figure X.X — Session Management Use Case Diagram

**Purpose.** Show the lifecycle and ownership of the visitor record, the API key, and the cross-scene session singleton.
**Actors.** Operator; Application; SQLite.
**Use Cases.** UC-02 Configure System; UC-31 Provision API Key; UC-S5 Load Configuration; UC-S1 Create Session State; UC-DB1 Open Database; UC-DB2 Insert Visitor Record; UC-S2 Populate Session State; UC-S3 Preserve Session Across Transition; UC-S4 Read Session State in Museum; UC-DB3 Dispose Database; UC-32 Inspect Visitor Records.
**Relationships.** `<<include>>` arrows from each lifecycle owner to its constituent steps; UC-32 is an administrative `<<extend>>` after the tour concludes.
**Design Rationale.** This diagram makes the responsibility split between Operator (configuration provisioning) and Application (runtime use of that configuration) explicit, and shows the single writer / multiple reader pattern around `SessionState`.

#### Final Recommendation

For an ABET-compliant report, four use case diagrams are recommended:

1. **Main System Use Case Diagram** — placed near the top of Chapter 4 to orient the reader.
2. **Visitor Interaction Use Case Diagram** — placed in Chapter 4 alongside the visitor-experience walkthrough.
3. **AI Tour Guide Use Case Diagram** — placed in Chapter 4 alongside the voice-pipeline discussion (around §4.4 Phase 5).
4. **Session Management Use Case Diagram** — placed in Chapter 4 alongside the lobby/persistence discussion (around §4.4 Phase 4 / Phase 8).

Use cases internal to the gaze pipeline (UC-G1–UC-G5) should remain visible only on the Visitor Interaction diagram as a single grouped cluster behind UC-11; this prevents the Main diagram from being overwhelmed by per-frame decisional sub-cases. Similarly, the SQLite and SessionState lifecycle use cases (UC-DB1–UC-DB3 and UC-S1–UC-S5) should appear only on the Session Management diagram, with their existence summarised under UC-03 and UC-08 on the other diagrams.

#### Design Rationale (overall)

The decomposition above respects three principles consistent with the implementation. **First**, every use case maps to at least one runtime artefact in the codebase (`LobbyController.OnSubmit`, `GazeArtifactDetector.Update`, `GuideOrchestrator.OnArtifactIdentified`, etc.) — there are no aspirational or invented use cases. **Second**, the `<<include>>` relationships reflect actual control-flow dependencies in the code rather than mere thematic grouping; for example, UC-03 `<<include>>` UC-05 is justified because the lobby's submit handler unconditionally invokes the database insert. **Third**, the boundary between Application and Operator is drawn at the configuration file: everything Application performs at runtime is inside the system; everything Operator performs is outside the system and asynchronous to the runtime experience. This boundary is the cleanest separation available given the implementation's portable-file-based configuration strategy.

---

**End of UML Use Case Specification.**
