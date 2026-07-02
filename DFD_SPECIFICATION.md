# Data Flow Diagram (DFD) Specification

> **Project:** AI-Assisted Virtual Reality Museum for Cultural Heritage Education — A Real-Time Speech-Enabled Cultural Heritage Experience for Meta Quest and Desktop Simulator Platforms
>
> **Purpose:** Complete DFD specification document, derived from the actual implementation in `PROJECT_REPORT.md`, ready to be converted into professional Level-0, Level-1, and Level-2 Data Flow Diagrams in Visio, Draw.io, Lucidchart, StarUML, or Microsoft Visio.

---

## Phase 1 — System Understanding

### 1.1 System Purpose

The system delivers a real-time, AI-narrated virtual-reality tour of an Egyptian museum environment. The visitor enters a lobby scene, supplies their identity, transitions into a 3D museum, walks among 42 catalogued artifacts, has each artifact identified automatically by gaze, and is accompanied by a humanoid AI tour guide that narrates each artifact and answers spoken follow-up questions in real time via the OpenAI Realtime API.

### 1.2 Main System Functions

| F-# | Function | Responsible Subsystem |
|---|---|---|
| F1 | Visitor identity collection and validation | `LobbyController` |
| F2 | Visitor record persistence | `MuseumDatabase` (SQLite) |
| F3 | Cross-scene state preservation | `SessionState` (DontDestroyOnLoad) |
| F4 | OpenAI API key retrieval | `OpenAIConfig` |
| F5 | Real-time speech-to-speech with the AI | `RealtimeClient`, `MicCapture`, `StreamingAudioPlayer`, `GuideOrchestrator` |
| F6 | Gaze-based artifact identification | `GazeArtifactDetector` |
| F7 | Floating artifact label display | `ArtifactLabelSpawner`, `ArtifactInfo` |
| F8 | Embodied AI guide following the visitor | `TourGuideAgent`, NavMeshAgent, `AmplitudeJawFlap` |
| F9 | Scene transition (Lobby → Museum) | `LobbyController` + `SessionState` |

### 1.3 Internal Subsystems

- **Lobby Subsystem:** Form UI, validation, persistence dispatch, scene transition.
- **Persistence Subsystem:** SQLite database accessed through the `MuseumDatabase` wrapper.
- **Session Subsystem:** Cross-scene singleton holding the active `VisitorRecord` and OpenAI key.
- **Gaze Subsystem:** Head-direction raycast, bounds-centre verification, dwell timer, per-artifact cooldown map.
- **Label Subsystem:** World-space TextMeshPro labels spawned above identified artifacts.
- **Voice Subsystem:** WebSocket transport, 24 kHz PCM16 microphone capture, streaming audio decoder with 30 s ring buffer, orchestrator handling Realtime API events.
- **Embodied Guide Subsystem:** Mixamo humanoid avatar with NavMeshAgent pursuit, hip-bone XZ lock, amplitude-driven mouth animation, Idle/Walk/Talk animator controller.

### 1.4 External Entities

| External Entity | Role |
|---|---|
| **Visitor** | The human user who interacts with the system through VR controllers, head movement, and voice. |
| **Operator** | The deployment operator who provides the OpenAI API key file before the application runs. |
| **OpenAI Realtime API** | External cloud-hosted speech-to-speech model. |
| **Microphone (XR / PC)** | Audio input device that captures the visitor's voice. |
| **Speakers / Headphones** | Audio output devices that render the guide's voice. |
| **Meta Quest Hardware** | Tracked HMD plus left and right controllers providing head/hand pose and button input. |

### 1.5 Data Stores (anticipated)

- D1 — Visitor Records (SQLite, `museum.db`)
- D2 — Artifact Catalogue (in-memory, populated from `ArtifactInfo` components in the Museum scene; the canonical catalogue is hard-coded in `Assets/Editor/ArtifactInfoAutoFill.cs`)
- D3 — Session State (in-memory, `DontDestroyOnLoad` singleton)
- D4 — Configuration Store (filesystem JSON, `MuseumVR/config.json`)
- D5 — Audio Ring Buffer (in-memory, 30 s capacity, `StreamingAudioPlayer`)
- D6 — Per-Artifact Cooldown Map (in-memory, `GazeArtifactDetector._cooldownUntil`)

### 1.6 Real-Time Data Streams

- Microphone PCM16 (24 kHz mono, 40 ms chunks of 960 samples each, base64 encoded over WebSocket).
- Realtime API audio deltas (base64 PCM16 returned over WebSocket and decoded into the ring buffer).
- Realtime API JSON control events (session.created, session.updated, response.created, response.audio.delta, response.audio.done, response.done, input_audio_buffer.committed, error, etc.).
- HMD head pose (60–90 Hz, via OpenXR).
- Controller pose and button state (via OpenXR / `UnityEngine.XR.InputDevices`).

---

## Phase 2 — Context Diagram Analysis

The following entities are confirmed **true DFD external entities** (they sit outside the system boundary and exchange data with the system):

### EE-1: Visitor

| Attribute | Detail |
|---|---|
| Role | Primary actor; provides identity, navigates the museum, asks the guide questions, listens to narration. |
| Data Sent To System | Visitor name (string); Age (integer 5–99); Country selection (ISO 3166-1 alpha-2 code); Submit event; Head pose; Controller pose; Trigger/grip/primary button events; Microphone audio (PCM16); Push-to-talk button hold events. |
| Data Received From System | Lobby form UI rendering; Validation status messages; Museum scene rendering; Floating artifact labels (name + era); Spatial audio narration; Spatial audio response to questions; Animated tour guide visual following behaviour. |
| Interaction Frequency | Continuous during the entire session (head pose at frame rate ≈ 90 Hz, audio at 24 kHz, controller polling at frame rate). |
| Interaction Type | Bidirectional, real-time, multi-modal. |

### EE-2: Operator

| Attribute | Detail |
|---|---|
| Role | Provides the OpenAI API key and deploys the application on the target machine. |
| Data Sent To System | `config.json` file containing `openai_api_key`. |
| Data Received From System | Operator-facing `README.txt` produced by the build; runtime log output. |
| Interaction Frequency | One-time pre-deployment configuration plus occasional key rotation. |
| Interaction Type | File-based, asynchronous. |

### EE-3: OpenAI Realtime API

| Attribute | Detail |
|---|---|
| Role | Cloud-hosted speech-to-speech conversational AI model (`gpt-4o-mini-realtime-preview`). |
| Data Sent To System | `session.created`, `session.updated`, `response.created`, `response.audio.delta` (base64 PCM16 chunks), `response.audio.done`, `response.done`, `input_audio_buffer.committed`, `error` events as JSON over WebSocket. |
| Data Received From System | `session.update` (configuration including instructions, voice, modalities, audio format, turn detection); `input_audio_buffer.append` (base64 PCM16 from microphone); `input_audio_buffer.commit`; `input_audio_buffer.clear`; `conversation.item.create` (system messages describing identified artifacts); `response.create`; `response.cancel`. |
| Interaction Frequency | Persistent WebSocket connection for the entire museum session; bidirectional streaming throughout. |
| Interaction Type | WebSocket (RFC 6455), JSON-framed, real-time streaming, full-duplex. |

### EE-4: Meta Quest Hardware (HMD + Controllers)

| Attribute | Detail |
|---|---|
| Role | Provides 6-DoF head and hand tracking and button state through the OpenXR runtime. |
| Data Sent To System | Head position and orientation; left and right controller position and orientation; controller button state (trigger, grip, primary Y/B, secondary X/A, thumbstick analog values). |
| Data Received From System | Stereoscopic rendered frames; haptic feedback (not currently used); audio output stream. |
| Interaction Frequency | Continuous at frame rate (≥ 72 fps target). |
| Interaction Type | Hardware-mediated through OpenXR. |

### EE-5: Microphone

| Attribute | Detail |
|---|---|
| Role | Audio input capture device (PC microphone, USB microphone, or Quest built-in mic depending on configuration). |
| Data Sent To System | Mono audio samples at the device's native sample rate. |
| Data Received From System | Activation / deactivation control via `Microphone.Start` and `Microphone.End`. |
| Interaction Frequency | Continuous while mic is active (Push-to-Talk only streams during the held-button interval). |
| Interaction Type | OS audio driver mediated. |

### EE-6: Speakers / Headphones

| Attribute | Detail |
|---|---|
| Role | Audio output device rendering the guide's spatialised voice. |
| Data Sent To System | None. |
| Data Received From System | PCM audio samples generated by Unity's `AudioSource` attached to the avatar's head bone. |
| Interaction Frequency | Continuous during narration playback. |
| Interaction Type | Hardware-mediated output. |

### Entities that are **internal processes**, not external entities

- **Artifact Knowledge Base** — internal data store D2, populated at scene-build time by `ArtifactInfoAutoFill`. Not external.
- **SQLite Database** — internal data store D1. The DBMS engine is linked into the application via `gilzoide.sqlite-net`; the file lives next to the executable. Not external.
- **Museum Administrator** — does not interact with the running system; replaced by the Operator entity above for the actual deployment-time interaction.

---

## Phase 3 — Level 0 DFD Specification

The system at Level 0 is decomposed into eight major processes. Process numbering matches the prompt's expected list, refined and verified against the documentation.

### P1 — User Registration & Session Initialization

| Field | Content |
|---|---|
| **Purpose** | Render the lobby form, collect the visitor's name, age, and country, validate the input, persist the record to SQLite, and seed the cross-scene session state. |
| **Inputs** | Form input (name, age, country) from Visitor (EE-1); Submit event from Visitor (EE-1); `config.json` content from Configuration Store (D4). |
| **Outputs** | New `VisitorRecord` row written to Visitor Records (D1); `SessionState.Visitor` populated (D3); `SessionState.OpenAiApiKey` populated (D3); Validation status text to Visitor (EE-1); Scene-load command to Unity engine. |
| **Connected Data Stores** | D1 (write), D3 (write), D4 (read). |
| **Connected External Entities** | Visitor (EE-1). |
| **Processing Description** | The lobby form is generated by `LobbySceneSetup` and bound to `LobbyController`. `LobbyController.Start` constructs a `MuseumDatabase` instance and reads the OpenAI key through `OpenAIConfig.TryLoadKey`. The Submit button becomes interactable only when (a) the name field is non-empty and (b) the API key is present. On submit, `OnSubmit()` resolves the selected country against the sorted country list, constructs a `VisitorRecord`, calls `MuseumDatabase.InsertVisitor`, writes the record to `SessionState.Instance.Visitor`, and triggers `SceneManager.LoadSceneAsync(museumSceneName, LoadSceneMode.Single)`. |

### P2 — Museum Navigation

| Field | Content |
|---|---|
| **Purpose** | Allow the visitor to move through the museum environment via teleportation, snap turn, and continuous locomotion driven by the XR Origin (XR Rig). |
| **Inputs** | Head pose, controller pose, thumbstick analog values, controller button events from Meta Quest Hardware (EE-4). |
| **Outputs** | Updated XR Origin world transform; teleport reticle visualisation; stereoscopic rendered frames to EE-4. |
| **Connected Data Stores** | None (read-only access to scene geometry). |
| **Connected External Entities** | Meta Quest Hardware (EE-4); Visitor (EE-1). |
| **Processing Description** | Continuous-locomotion via the left thumbstick is provided by the XR Interaction Toolkit's `ContinuousMoveProvider`. Teleportation is delivered by the `TeleportationProvider` operating against `TeleportationArea` components on floor meshes. Snap-turn (30°) on the right thumbstick is delivered by `SnapTurnProvider`. A comfort vignette darkens the screen edges during teleportation. |

### P3 — Artifact Identification

| Field | Content |
|---|---|
| **Purpose** | Identify which of the 42 catalogued artifacts the visitor's head is currently directed at, applying distance, bounds-centre, dwell, and cooldown constraints. |
| **Inputs** | Head pose from EE-4 (via the XR Origin's CenterEyeAnchor camera transform); `ArtifactInfo` components on scene GameObjects; current frame's `Time.deltaTime`. |
| **Outputs** | `OnArtifactIdentified` event carrying the identified `ArtifactInfo` and the raycast hit, delivered to subscribers (`ArtifactLabelSpawner`, `GuideOrchestrator`). |
| **Connected Data Stores** | D2 (read — artifact metadata); D6 (read and write — per-artifact cooldown timestamps). |
| **Connected External Entities** | None (transitively serves the Visitor through the label and narration). |
| **Processing Description** | Each frame `GazeArtifactDetector.Update` casts a forward ray from the head, length capped at `maxDistance = 2 m`. If the ray hits a collider whose parent carries an `ArtifactInfo`, a bounds-centre check rejects artifacts whose bounding-box centre exceeds 2 m. The cooldown map suppresses any artifact within 30 s of its previous identification. On a continuous match the dwell timer accumulates `Time.deltaTime`; when it reaches 1.5 s, the cooldown timestamp is set, the dwell timer is reset, and `OnArtifactIdentified` is raised. |

### P4 — AI Guide Interaction

| Field | Content |
|---|---|
| **Purpose** | Compose the system-prompt context for an identified artifact, push it to the OpenAI Realtime API, and request a narration response. |
| **Inputs** | `OnArtifactIdentified` event from P3, carrying `ArtifactInfo`; visitor identity from D3; current `_modelSpeaking` flag and post-speech grace timer. |
| **Outputs** | `conversation.item.create` system message and `response.create` JSON event sent to OpenAI Realtime API (EE-3). |
| **Connected Data Stores** | D2 (read), D3 (read), D5 (write — indirectly, via response audio that lands in the ring buffer). |
| **Connected External Entities** | OpenAI Realtime API (EE-3). |
| **Processing Description** | `GuideOrchestrator.OnArtifactIdentified` checks whether a previous response is still in flight (`_modelSpeaking` or within `postSpeechMuteGrace`); if so the narration request is skipped (the guide will not interrupt itself). Otherwise it constructs a system-role message containing the artifact's `displayName`, `era`, and `description`, sends it via `_client.Send(...)` as `conversation.item.create`, then sends a `response.create` event. The `_lastResponseSource` is tagged for diagnostic logging. |

### P5 — Voice Processing (Push-to-Talk Question Handling and Streaming Audio Playback)

| Field | Content |
|---|---|
| **Purpose** | Bidirectionally stream audio: forward push-to-talk microphone audio to the API as base64 PCM16 chunks; receive base64 PCM16 deltas from the API, decode them into a ring buffer, and play them through an `AudioSource` mounted on the avatar's head bone. |
| **Inputs** | Push-to-talk button events from Meta Quest Hardware (EE-4); raw audio samples from Microphone (EE-5); incoming JSON events including `response.audio.delta` from OpenAI Realtime API (EE-3). |
| **Outputs** | `input_audio_buffer.append`, `input_audio_buffer.commit`, `response.create`, `response.cancel`, and `input_audio_buffer.clear` events to OpenAI Realtime API (EE-3); PCM samples to Speakers / Headphones (EE-6). |
| **Connected Data Stores** | D5 (write — incoming audio deltas; read — playback consumption via the audio thread's `PCMReaderCallback`). |
| **Connected External Entities** | OpenAI Realtime API (EE-3); Microphone (EE-5); Speakers / Headphones (EE-6); Meta Quest Hardware (EE-4). |
| **Processing Description** | The mic capture thread polls `Microphone.GetData`, linearly resamples to 24 kHz, builds 40 ms (960-sample) PCM16 chunks, and encodes each to base64. The orchestrator forwards each chunk to the Realtime API only when (a) `inputMode = PushToTalk` and the PTT key/button is held, or (b) `inputMode = ServerVad`. On button release, the orchestrator commits the input buffer and issues `response.create`. Incoming `response.audio.delta` events are decoded by `StreamingAudioPlayer.EnqueueBase64Pcm16` into the 30-second ring buffer; the audio thread's `PCMReaderCallback` consumes samples for playback, with a 200 ms pre-buffer requirement and a decay-to-silence fade on under-run. |

### P6 — Visitor Data Management

| Field | Content |
|---|---|
| **Purpose** | Maintain the SQLite visitor record table including connection lifecycle, schema creation, and insert operations. |
| **Inputs** | `VisitorRecord` instance from P1; database file path from `MuseumDatabase.DefaultDbPath()` (which prefers a folder next to the executable for portability). |
| **Outputs** | Persisted SQLite row in D1; returned auto-incremented `Id` to P1. |
| **Connected Data Stores** | D1 (write). |
| **Connected External Entities** | None (the SQLite engine is linked-in, not external). |
| **Processing Description** | `MuseumDatabase.MuseumDatabase(path)` ensures the parent directory exists, opens an `SQLiteConnection` with `ReadWrite | Create | FullMutex` flags, and creates the `VisitorRecord` table if needed. `InsertVisitor(record)` stamps `StartedAtUnixSeconds` if zero and calls `_conn.Insert(record)`. The database file location follows the portable-first resolution rule: `<exe>/MuseumVR/museum.db` if the portable folder exists, otherwise `%APPDATA%/MuseumVR/museum.db`. |

### P7 — Information Presentation

| Field | Content |
|---|---|
| **Purpose** | Render world-space floating labels above identified artifacts so the visitor can read the artifact's name and era. |
| **Inputs** | `OnArtifactIdentified` event from P3. |
| **Outputs** | Instantiated label prefab in the museum scene; updated TextMeshPro text fields. |
| **Connected Data Stores** | D2 (read — `displayName`, `era`). |
| **Connected External Entities** | Visitor (EE-1) — via stereoscopic rendering of the label. |
| **Processing Description** | `ArtifactLabelSpawner` subscribes to `GazeArtifactDetector.OnArtifactIdentified`. On firing, it computes the artifact's renderer-encapsulated bounding box, spawns the `ArtifactLabel.prefab` at the bounds-top position, sets the label's `displayName` and `era` fields, and auto-fades the label after a configurable duration. |

### P8 — System Configuration & Session Management

| Field | Content |
|---|---|
| **Purpose** | Load the OpenAI API key from disk, instantiate the `SessionState` singleton, and preserve it across the Lobby → Museum scene transition. |
| **Inputs** | Filesystem read of `config.json` from D4; scene-load events from Unity engine. |
| **Outputs** | Populated `SessionState.OpenAiApiKey` in D3; established `SessionState` GameObject marked `DontDestroyOnLoad`. |
| **Connected Data Stores** | D3 (write), D4 (read). |
| **Connected External Entities** | Operator (EE-2) — indirectly, through the file deposited on disk. |
| **Processing Description** | `OpenAIConfig.GetConfigPath()` resolves the portable path first (`<exe>/MuseumVR/config.json`) then falls back to `%APPDATA%/MuseumVR/config.json`. The JSON file is deserialised through Newtonsoft.Json. `SessionState.GetOrCreate()` either returns the existing singleton or creates a new GameObject marked `DontDestroyOnLoad`. The Museum scene's `GuideOrchestrator.Start` retrieves the key from `SessionState` and falls back to a direct `OpenAIConfig` read if the lobby was bypassed. |

---

## Phase 4 — Data Store Analysis

### D1 — Visitor Records (SQLite, `museum.db`)

| Attribute | Detail |
|---|---|
| **Store ID** | D1 |
| **Store Name** | Visitor Records |
| **Physical Format** | SQLite database file, default location `<exe>/MuseumVR/museum.db` (portable) or `%APPDATA%/MuseumVR/museum.db` (per-user fallback). |
| **Schema** | Table `VisitorRecord` with columns `Id INTEGER PRIMARY KEY AUTOINCREMENT`, `Name TEXT`, `Age INTEGER`, `CountryCode TEXT(2)`, `CountryName TEXT`, `StartedAtUnixSeconds INTEGER`. |
| **Read Operations** | `MuseumDatabase.GetVisitor(int id)`, `MuseumDatabase.VisitorCount()`. |
| **Write Operations** | `MuseumDatabase.InsertVisitor(VisitorRecord)`, `MuseumDatabase.UpdateVisitor(VisitorRecord)`. |
| **Processes Using It** | P1 (write), P6 (write), P8 (read at session start). |

### D2 — Artifact Catalogue

| Attribute | Detail |
|---|---|
| **Store ID** | D2 |
| **Store Name** | Artifact Catalogue |
| **Physical Format** | In-memory, hydrated from `ArtifactInfo` components attached to GameObjects in the Museum scene. The canonical content is hard-coded as a `Dictionary<string, Entry>` in `Assets/Editor/ArtifactInfoAutoFill.cs` and applied at edit time by the `Auto-Fill Artifact Info` menu. |
| **Schema** | Each `ArtifactInfo` exposes `displayName: string`, `era: string`, `description: string`. There are exactly 42 entries. |
| **Read Operations** | Read each frame by `GazeArtifactDetector.Update` (lookup via `GetComponentInParent<ArtifactInfo>()`) and by `ArtifactLabelSpawner` and `GuideOrchestrator.OnArtifactIdentified` on identification events. |
| **Write Operations** | Edit-time only (no runtime writes). |
| **Processes Using It** | P3 (read), P4 (read), P7 (read). |

### D3 — Session State

| Attribute | Detail |
|---|---|
| **Store ID** | D3 |
| **Store Name** | Session State |
| **Physical Format** | In-memory `SessionState` singleton GameObject marked `DontDestroyOnLoad`. |
| **Schema** | `Visitor: VisitorRecord`, `OpenAiApiKey: string`. |
| **Read Operations** | `GuideOrchestrator.ConfigureSession` (visitor name for the system prompt); `LobbyController.OnSubmit` (set Visitor). |
| **Write Operations** | `LobbyController.OnSubmit` (Visitor), `LobbyController.LoadAndDisplayKey` (OpenAiApiKey), `GuideOrchestrator.Start` (fallback OpenAiApiKey read). |
| **Processes Using It** | P1 (write), P4 (read), P5 (read indirectly), P8 (read and write). |

### D4 — Configuration Store

| Attribute | Detail |
|---|---|
| **Store ID** | D4 |
| **Store Name** | Configuration Store |
| **Physical Format** | JSON file `MuseumVR/config.json` (portable, next to the executable; fallback `%APPDATA%/MuseumVR/config.json`). |
| **Schema** | `{ "openai_api_key": "sk-..." }`. |
| **Read Operations** | `OpenAIConfig.TryLoadKey`. |
| **Write Operations** | None at runtime; the Operator writes the file before deployment, and the production build automatically copies it next to the executable. |
| **Processes Using It** | P1 (read at lobby start), P8 (read). |

### D5 — Audio Ring Buffer

| Attribute | Detail |
|---|---|
| **Store ID** | D5 |
| **Store Name** | Streaming Audio Ring Buffer |
| **Physical Format** | In-memory `float[]` of size `24000 × 30 = 720,000` samples (30 seconds of mono PCM at 24 kHz). |
| **Schema** | Circular FIFO with `_readIdx`, `_writeIdx`, `_available`, `_lastSample`, `_draining` fields, protected by a `lock` object. |
| **Read Operations** | Audio thread `OnPcmRead(float[] data)` callback driven by the `AudioClip` `PCMReaderCallback`. |
| **Write Operations** | Main thread `EnqueueBase64Pcm16(string base64)` decodes incoming deltas and writes them. |
| **Processes Using It** | P5 (both read and write). |

### D6 — Per-Artifact Cooldown Map

| Attribute | Detail |
|---|---|
| **Store ID** | D6 |
| **Store Name** | Per-Artifact Cooldown Map |
| **Physical Format** | In-memory `Dictionary<ArtifactInfo, float>` mapping artifact to a `Time.time + 30 s` expiry. |
| **Schema** | Key: `ArtifactInfo` reference; Value: float Unity time stamp. |
| **Read Operations** | `GazeArtifactDetector.IsOnCooldown(ArtifactInfo)` each frame. |
| **Write Operations** | `GazeArtifactDetector.Update` when dwell completes. |
| **Processes Using It** | P3 (both read and write). |

---

## Phase 5 — Level 1 DFD Decomposition

### P1 → Level 1 (User Registration & Session Initialization)

| Sub-process | Inputs | Outputs | Data Stores | Processing Logic |
|---|---|---|---|---|
| P1.1 Load Configuration | `config.json` file path | API key string | D4 (read) | `OpenAIConfig.TryLoadKey` opens the JSON file with `File.ReadAllText`, deserialises via `JsonConvert.DeserializeObject<ConfigFile>`, validates non-empty `OpenAiApiKey`. Returns success/failure and the trimmed key. |
| P1.2 Display Form and Render Country List | Country catalogue (~140 entries from `CountryList.Default()`) | Form UI in the scene | None | `LobbyController.PopulateCountries` constructs a sorted list, pins `"Other / Prefer not to say"` at the bottom, selects Egypt by default, and populates the TMP dropdown. |
| P1.3 Input Validation | Form field values (Name string, Age integer, Country selection) | Validation status | None | `LobbyController.UpdateSubmitInteractable` checks `hasName = !string.IsNullOrWhiteSpace(nameField.text)` and `hasKey = !string.IsNullOrEmpty(_session.OpenAiApiKey)`. Submit becomes interactable only when both hold. Status text shows `"Enter your name to begin."` or `"OpenAI key missing — see Status above."` as appropriate. |
| P1.4 Visitor Record Creation | Validated form values | `VisitorRecord` instance | None | `OnSubmit` constructs `new VisitorRecord { Name, Age, CountryCode, CountryName, StartedAtUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds() }`. |
| P1.5 Database Storage | `VisitorRecord` instance | Persisted row, returned `Id` | D1 (write) | Delegates to P6 (`MuseumDatabase.InsertVisitor`). On exception the lobby surfaces an error and re-enables Submit. |
| P1.6 Session Creation | `VisitorRecord` reference, API key string | `SessionState.Visitor`, `SessionState.OpenAiApiKey` | D3 (write) | `SessionState.Instance.Visitor = record;` (singleton was created in `Start`). |
| P1.7 Museum Scene Transition | Museum scene name | Loaded museum scene; unloaded lobby scene | None | `SceneManager.LoadSceneAsync(museumSceneName, LoadSceneMode.Single)`. Single mode is mandatory to avoid the dual-XR-Origin collision documented in the report. |

### P2 → Level 1 (Museum Navigation)

| Sub-process | Inputs | Outputs | Data Stores | Processing Logic |
|---|---|---|---|---|
| P2.1 Read Controller Input | Thumbstick analog values, button states | Locomotion intent vectors | None | XR Interaction Toolkit's Input Action references read controller state through OpenXR. |
| P2.2 Apply Continuous Locomotion | Left thumbstick vector, head forward | Updated XR Origin position | None | `ContinuousMoveProvider` translates the XR Origin in the head's forward/right direction at configured speed. |
| P2.3 Apply Snap Turn | Right thumbstick X | Rotated XR Origin yaw | None | `SnapTurnProvider` applies discrete 30° rotations when the right thumbstick crosses the deadzone. |
| P2.4 Process Teleport Request | Left thumbstick forward push + release; teleport ray hit on `TeleportationArea` | Teleport destination | None | `TeleportationProvider` warps the XR Origin to the resolved hit point; a comfort vignette fades during the warp. |

### P3 → Level 1 (Artifact Identification) — *expanded further in Phase 6*

| Sub-process | Inputs | Outputs | Data Stores | Processing Logic |
|---|---|---|---|---|
| P3.1 Acquire Gaze Origin | XR camera transform | Origin position and forward vector | None | `gazeOrigin = Camera.main.transform` (lazy-initialised in `Awake`). |
| P3.2 Cast Ray | Origin, forward, `maxDistance = 2 m`, `raycastMask` | `RaycastHit` or none | None | `Physics.Raycast(...)` with `QueryTriggerInteraction.Ignore`. |
| P3.3 Resolve Hit to ArtifactInfo | Hit collider | `ArtifactInfo` reference or null | D2 (read) | `hit.collider.GetComponentInParent<ArtifactInfo>()`. |
| P3.4 Verify Bounds Centre | `ArtifactInfo`, gaze origin | Pass/fail boolean | D2 (read renderers) | `ComputeArtifactBounds(...)` encapsulates all child renderer bounds; reject if `Distance(origin, bounds.center) > maxDistance`. |
| P3.5 Check Cooldown | `ArtifactInfo`, current `Time.time` | Pass/fail boolean | D6 (read) | `IsOnCooldown` returns true while `_cooldownUntil[artifact] > Time.time`. |
| P3.6 Update Dwell Timer | Current target, previous target | Accumulated dwell seconds | None | If target changed, reset `_dwellElapsed = 0`; otherwise `_dwellElapsed += Time.deltaTime`. |
| P3.7 Fire Identification Event | Identified `ArtifactInfo`, `RaycastHit` | `OnArtifactIdentified` event | D6 (write) | Set `_cooldownUntil[artifact] = Time.time + 30`; reset dwell; invoke event. |

### P4 → Level 1 (AI Guide Interaction)

| Sub-process | Inputs | Outputs | Data Stores | Processing Logic |
|---|---|---|---|---|
| P4.1 Receive Identification Event | `ArtifactInfo`, `RaycastHit` | Event payload | D2 (read) | Handler `GuideOrchestrator.OnArtifactIdentified`. |
| P4.2 Check Speaking Gate | `_modelSpeaking`, `_modelSpeakingUntil` | Proceed / skip flag | None | If the model is currently producing audio, the request is suppressed to prevent self-interruption. |
| P4.3 Build Context Message | `ArtifactInfo.displayName`, `era`, `description` | JSON `conversation.item.create` payload | D2 (read) | String concatenation into the required input_text message format. |
| P4.4 Send Context | JSON payload | WebSocket frame to EE-3 | None | `_client.Send(JObject)`. |
| P4.5 Request Response | JSON `response.create` payload | WebSocket frame to EE-3 | None | `_client.Send(...)` tagged with `_lastResponseSource = "GAZE:<displayName>"`. |

### P5 → Level 1 (Voice Processing) — *expanded further in Phase 6*

| Sub-process | Inputs | Outputs | Data Stores | Processing Logic |
|---|---|---|---|---|
| P5.1 Detect Push-to-Talk State | Keyboard.current[T], controller primary buttons | Held boolean | None | `IsPushToTalkHeld()` checks both. |
| P5.2 Capture Microphone Samples | Native-rate mono samples | Resampled 24 kHz PCM16 chunk | None | `MicCapture.Update` reads from `Microphone.GetData`, resamples linearly. |
| P5.3 Encode Chunk | 40 ms PCM16 frame | Base64 string | None | `Convert.ToBase64String`. |
| P5.4 Stream Chunk | Base64 string | `input_audio_buffer.append` event | None | `_client.Send`, gated on input mode and speaking grace. |
| P5.5 Handle PTT Release | Release event | `input_audio_buffer.commit` + `response.create` events | None | `HandlePushToTalkInput`. |
| P5.6 Receive Audio Delta | `response.audio.delta` event from EE-3 | Decoded PCM samples in D5 | D5 (write) | `StreamingAudioPlayer.EnqueueBase64Pcm16`. |
| P5.7 Render Audio | Ring buffer samples | PCM stream to EE-6 | D5 (read) | `OnPcmRead` audio thread callback. |
| P5.8 Drive Animation | Audio amplitude | `IsSpeaking` animator flag, optional jaw rotation | None | `AmplitudeJawFlap.LateUpdate`. |

### P6 → Level 1 (Visitor Data Management)

| Sub-process | Inputs | Outputs | Data Stores | Processing Logic |
|---|---|---|---|---|
| P6.1 Resolve DB Path | Application.dataPath, %APPDATA% | Absolute path | None | `DefaultDbPath()` portable-first resolution. |
| P6.2 Open Connection | Path | `SQLiteConnection` | None | `new SQLiteConnection(path, ReadWrite | Create | FullMutex)`. |
| P6.3 Ensure Schema | Connection | Table created if absent | D1 (write) | `_conn.CreateTable<VisitorRecord>()`. |
| P6.4 Insert Record | `VisitorRecord` | Inserted row, generated `Id` | D1 (write) | `_conn.Insert(record)`. |
| P6.5 Dispose | None | Connection closed | None | `Dispose()` on application exit. |

### P7 → Level 1 (Information Presentation)

| Sub-process | Inputs | Outputs | Data Stores | Processing Logic |
|---|---|---|---|---|
| P7.1 Compute Spawn Position | `ArtifactInfo`, encapsulated bounds | World-space label position | None | Bounds-top with a configurable offset. |
| P7.2 Instantiate Label | Prefab reference | New label GameObject | None | `Instantiate(labelPrefab, position, Quaternion.identity)`. |
| P7.3 Bind Text | `displayName`, `era` | TMP text content | D2 (read) | Assign `text` fields on the label's two TMP components. |
| P7.4 Schedule Fade | Label reference | Fade coroutine | None | Auto-fade after a configurable duration. |

### P8 → Level 1 (System Configuration & Session Management)

| Sub-process | Inputs | Outputs | Data Stores | Processing Logic |
|---|---|---|---|---|
| P8.1 Resolve Config Path | dataPath, %APPDATA% | Absolute path | None | `OpenAIConfig.GetConfigPath()` portable-first. |
| P8.2 Read File | Path | JSON string | D4 (read) | `File.ReadAllText`. |
| P8.3 Deserialise | JSON string | `ConfigFile` | None | `JsonConvert.DeserializeObject<ConfigFile>`. |
| P8.4 Validate Key | `ConfigFile` | Valid key or error message | None | Reject empty/whitespace. |
| P8.5 Hydrate Session | Key, Visitor | Populated `SessionState` | D3 (write) | Singleton getter then field set. |
| P8.6 Mark DontDestroyOnLoad | Singleton GameObject | Survives scene swap | None | `DontDestroyOnLoad(gameObject)`. |

---

## Phase 6 — Level 2 DFD Decomposition

### P3 — Artifact Identification at Level 2

```
INPUT: Head transform (each frame)
   │
   ├──> P3.1 Acquire Gaze Origin
   │       └── (transform.position, transform.forward)
   │
   ├──> P3.2 Cast Ray (max 2 m, layer mask, ignore triggers)
   │       └── RaycastHit
   │
   ├──> P3.3 Resolve Hit to ArtifactInfo
   │       └── ArtifactInfo or null
   │
   ├──> P3.4 Verify Bounds Centre
   │       ├── Compute bounds of all child renderers
   │       └── Reject if distance to bounds.center > 2 m
   │
   ├──> P3.5 Check Cooldown
   │       └── _cooldownUntil[artifact] vs Time.time
   │
   ├──> P3.6 Track Target Continuity
   │       ├── If target changed → reset dwell, save new _current
   │       └── If same → accumulate dwell
   │
   ├──> P3.7 Test Dwell Threshold
   │       └── _dwellElapsed >= 1.5 s?
   │
   ├──> P3.8 Commit Identification
   │       ├── Set _cooldownUntil[artifact] = Time.time + 30
   │       ├── Reset _dwellElapsed
   │       ├── Clear _current
   │       └── Invoke OnArtifactIdentified(artifact, hit)
   │
   └──> OUTPUT: Event to ArtifactLabelSpawner and GuideOrchestrator
```

**Decision points within P3:**
- D3.A: Did the ray hit a collider? (no → skip)
- D3.B: Did the collider's parent carry `ArtifactInfo`? (no → skip)
- D3.C: Is the bounds-centre within `maxDistance`? (no → skip)
- D3.D: Is this artifact on cooldown? (yes → reset `_current` and `_dwellElapsed`, exit)
- D3.E: Is the hit artifact the same as `_current`? (no → reset dwell, set new `_current`, exit)
- D3.F: Has `_dwellElapsed` reached `dwellSeconds = 1.5 s`? (no → exit; yes → fire event)

### P4 — AI Guide System at Level 2

```
INPUT: OnArtifactIdentified(ArtifactInfo, RaycastHit)
   │
   ├──> P4.1 Check WebSocket State
   │       └── _client != null && _client.IsOpen?  no → drop event
   │
   ├──> P4.2 Check Speaking Gate
   │       └── _modelSpeaking || Time.time < _modelSpeakingUntil?
   │              yes → log "skipped narration" → drop event
   │
   ├──> P4.3 Compose System Message
   │       └── "The visitor is now looking at: {displayName}.
   │            Era: {era}. Background: {description}.
   │            Give a short evocative 1-2 sentence introduction now,
   │            then invite a question."
   │
   ├──> P4.4 Send conversation.item.create
   │       └── JSON event with role=system, content type=input_text
   │
   ├──> P4.5 Send response.create
   │       ├── Tag _lastResponseSource = "GAZE:<displayName>"
   │       └── Empty response object (uses session-level modalities)
   │
   └──> OUTPUT: Two JSON frames over WebSocket to EE-3
```

### P5 — OpenAI Realtime Voice System at Level 2 (Upstream and Downstream)

**Upstream pipeline (visitor question → API):**

```
PTT button held
   │
   ├──> P5.1 Microphone polling
   │       └── Native-rate mono samples from EE-5
   │
   ├──> P5.2 Linear resample to 24 kHz
   │       └── Float samples at 24 kHz
   │
   ├──> P5.3 Slice into 40 ms windows
   │       └── 960-sample blocks
   │
   ├──> P5.4 Convert float to PCM16
   │       └── short[] then byte[]
   │
   ├──> P5.5 Base64 encode
   │       └── String payload
   │
   ├──> P5.6 Gate by input mode and mute grace
   │       ├── GazeOnly → skip (counter ++)
   │       ├── PushToTalk && !held → skip (counter ++)
   │       └── muteMicWhileModelSpeaks && _modelSpeaking → skip (counter ++)
   │
   ├──> P5.7 Send input_audio_buffer.append
   │       └── WebSocket frame to EE-3
   │
PTT button released
   │
   ├──> P5.8 Send input_audio_buffer.commit
   │
   └──> P5.9 Send response.create
           └── Tag _lastResponseSource = "PUSH_TO_TALK"
```

**Downstream pipeline (API → visitor):**

```
WebSocket frame arrives
   │
   ├──> P5.10 ReceiveLoop parses JSON
   │       └── JObject pushed to _incoming concurrent queue
   │
   ├──> P5.11 GuideOrchestrator.Update drains queue
   │
   ├──> P5.12 Dispatch event type
   │       ├── session.created/updated → log
   │       ├── response.created → set _modelSpeaking = true,
   │       │   clear input_audio_buffer server-side
   │       ├── response.audio.delta → P5.13
   │       ├── response.audio.done → log
   │       ├── response.done → set _modelSpeaking = false,
   │       │   _modelSpeakingUntil = Time.time + 0.4,
   │       │   clear input_audio_buffer server-side
   │       ├── input_audio_buffer.speech_started/stopped/committed → log
   │       └── error → log
   │
   ├──> P5.13 StreamingAudioPlayer.EnqueueBase64Pcm16
   │       ├── Convert.FromBase64String
   │       ├── Decode short[] interleaved
   │       ├── Lock ring; check overflow; warn + drop oldest if needed
   │       └── Write into _ring at _writeIdx, update _available
   │
   ├──> P5.14 Audio thread OnPcmRead (Unity audio thread)
   │       ├── If !_draining and _available < 4800 → output silence
   │       ├── Else copy samples into output buffer
   │       └── On under-run, fade from _lastSample by ×0.92 per sample
   │
   ├──> P5.15 AudioSource (3D, head bone) renders to EE-6
   │
   └──> P5.16 AmplitudeJawFlap LateUpdate
           ├── GetOutputData(256 samples)
           ├── RMS calculation
           ├── If jawBone != null → rotate by smoothed amplitude
           └── tourGuideAgent.isSpeaking = rms > silenceThreshold
```

### P1 — Visitor Registration at Level 2

```
Application start
   │
   ├──> P1.1 LobbyController.Start
   │       ├── SessionState.GetOrCreate()
   │       ├── new MuseumDatabase()
   │       ├── PopulateCountries() — sort 140 entries, default Egypt
   │       ├── SetupAgeSlider() — range 5–99, default 25
   │       └── LoadAndDisplayKey() — read config.json → SessionState
   │
   ├──> P1.2 User interacts
   │       ├── NameField.onValueChanged → UpdateSubmitInteractable
   │       ├── AgeSlider.onValueChanged → RefreshAgeDisplay
   │       └── CountryDropdown.value updates
   │
   ├──> P1.3 Submit pressed
   │       ├── Disable Submit button
   │       ├── ResolveCountry() against _sortedCountries
   │       ├── Construct VisitorRecord (Name, Age, CountryCode, CountryName,
   │       │   StartedAtUnixSeconds)
   │       └── try { _db.InsertVisitor(record); }
   │           catch → set error status, re-enable Submit, exit
   │
   ├──> P1.4 Store in SessionState
   │       └── _session.Visitor = record
   │
   └──> P1.5 LoadSceneAsync(museumSceneName, LoadSceneMode.Single)
           └── Lobby destroyed; SessionState survives via DontDestroyOnLoad
```

---

## Phase 7 — Data Flow Catalog

The complete catalogue of data flows in the system. Format: `Flow ID | Source | Destination | Data Content | Direction | Frequency`.

| Flow ID | Source | Destination | Data Content | Direction | Frequency |
|---|---|---|---|---|---|
| F01 | Visitor (EE-1) | P1.3 Input Validation | Name (string ≤ 64 chars) | → | Once per session |
| F02 | Visitor (EE-1) | P1.3 Input Validation | Age (integer 5–99) | → | Once per session |
| F03 | Visitor (EE-1) | P1.3 Input Validation | Country selection (ISO 3166-1 alpha-2) | → | Once per session |
| F04 | Visitor (EE-1) | P1 | Submit event | → | Once per session |
| F05 | D4 Configuration Store | P1.1 / P8.2 | API key JSON | → | Once at lobby start |
| F06 | P1.4 | P1.5 | `VisitorRecord` instance | → | Once per session |
| F07 | P1.5 / P6 | D1 Visitor Records | Persisted row | → | Once per session |
| F08 | P6 | P1.5 | Auto-incremented `Id` | → | Once per session |
| F09 | P1.6 | D3 Session State | Visitor + API key | → | Once per session |
| F10 | P1.7 | Unity engine | Scene-load command | → | Once per session |
| F11 | Meta Quest (EE-4) | P2 | Head pose (position + orientation) | → | Each frame (~90 Hz) |
| F12 | Meta Quest (EE-4) | P2 | Controller pose | → | Each frame |
| F13 | Meta Quest (EE-4) | P2 | Thumbstick analog values | → | Each frame |
| F14 | Meta Quest (EE-4) | P2 | Button events (trigger, grip, Y, B) | → | Per event |
| F15 | P2 | Unity engine | Updated XR Origin transform | → | Per frame |
| F16 | P2 | Meta Quest (EE-4) | Rendered stereoscopic frames | → | Per frame |
| F17 | Meta Quest (EE-4) | P3.1 | Head transform | → | Each frame |
| F18 | D2 Artifact Catalogue | P3.3 | `ArtifactInfo` reference | → | Per gaze hit |
| F19 | D6 Cooldown Map | P3.5 | Cooldown timestamp | → | Each frame for the current target |
| F20 | P3.7 | D6 | Updated cooldown timestamp | → | Per identification (≤ once per 30 s per artifact) |
| F21 | P3.7 | P4.1 (event) | `OnArtifactIdentified(ArtifactInfo, RaycastHit)` | → | Per identification |
| F22 | P3.7 | P7.1 (event) | Same `OnArtifactIdentified` event | → | Per identification |
| F23 | D2 | P4.3 | `displayName`, `era`, `description` | → | Per identification |
| F24 | P4.4 / P4.5 | OpenAI Realtime API (EE-3) | `conversation.item.create` + `response.create` JSON frames | → | Per identification |
| F25 | Meta Quest (EE-4) | P5.1 | Primary button (Y / B) state | → | Each frame |
| F26 | Keyboard | P5.1 | T key state | → | Each frame |
| F27 | Microphone (EE-5) | P5.2 | Native-rate mono samples | → | Continuous while mic is active |
| F28 | P5.5 | P5.7 | Base64 PCM16 chunk (40 ms) | → | ~25 chunks / second when PTT held |
| F29 | P5.7 | OpenAI Realtime API (EE-3) | `input_audio_buffer.append` frame | → | ~25 / second |
| F30 | P5.8 | OpenAI Realtime API (EE-3) | `input_audio_buffer.commit` frame | → | Per PTT release |
| F31 | P5.9 | OpenAI Realtime API (EE-3) | `response.create` frame | → | Per PTT release |
| F32 | OpenAI Realtime API (EE-3) | P5.10 | `response.audio.delta` frame (base64 PCM16) | → | ~25 / second during narration |
| F33 | OpenAI Realtime API (EE-3) | P5.10 | Control events: session.*, response.*, input_audio_buffer.*, error | → | Per event |
| F34 | P5.13 | D5 Audio Ring Buffer | Decoded PCM16 samples | → | Per delta |
| F35 | D5 | P5.14 | Samples for playback | → | Each audio thread tick |
| F36 | P5.15 | Speakers / Headphones (EE-6) | Rendered PCM audio | → | Continuous during playback |
| F37 | P5.15 | P5.16 / `AmplitudeJawFlap` | Output sample buffer | → | Each `LateUpdate` |
| F38 | P5.16 | `TourGuideAgent` | `isSpeaking` boolean | → | Each frame |
| F39 | P3.7 | P7.1 | `ArtifactInfo` (event payload) | → | Per identification |
| F40 | P7.2 | Unity engine | Instantiated label GameObject | → | Per identification |
| F41 | P7.3 | Label TMP fields | `displayName`, `era` | → | Per identification |
| F42 | Operator (EE-2) | D4 | Operator-deposited `config.json` | → | Once, pre-deployment |
| F43 | Meta Quest (EE-4) | XR Origin (P2) | Tracked-pose updates for `TrackedPoseDriver` | → | Each frame |
| F44 | NavMeshAgent (within P8/TourGuideAgent) | Avatar transform | Position + rotation updates | → | Each frame |
| F45 | `TourGuideAgent` Update | `Animator` | `IsMoving`, `IsSpeaking` bools | → | Each frame |
| F46 | `TourGuideAgent` LateUpdate | Hip bone (mixamorig:Hips) | Locked local XZ position | → | Each frame |
| F47 | `AmplitudeJawFlap` LateUpdate | Jaw bone (if present) | Rotation angle | → | Each frame |
| F48 | D3 SessionState | P4.3 (system prompt) | Visitor `Name`, `Age`, `CountryName` | → | Once at session config |
| F49 | P5.10 → main thread | `_incoming` concurrent queue | JObject events | → | Per inbound event |
| F50 | `_incoming` queue | `GuideOrchestrator.Update` | Drained events | → | Each frame |

---

## Phase 8 — ABET Documentation Output

### Data Flow Diagram Analysis

This section synthesises the system's logical structure into a sequence of data flow diagrams suitable for inclusion in the project report. Four levels of diagram are specified: a *context diagram* that locates the system within its environment of external entities; a *Level-0* diagram that decomposes the system into eight high-level processes interacting through six data stores; *Level-1* decompositions for each of those processes; and *Level-2* decompositions for the four processes carrying the system's most consequential logic (artifact identification, AI guide interaction, real-time voice processing, and visitor registration).

#### Context Diagram Description

The context diagram presents the application as a single bubble surrounded by six external entities: the **Visitor** (primary actor, providing identity, head and controller input, and voice; receiving rendered visuals and spatialised audio), the **Operator** (depositing the OpenAI configuration file before runtime), the **OpenAI Realtime API** (cloud-hosted conversational AI accessed over a WebSocket connection), the **Meta Quest Hardware** (HMD and controllers exchanging tracked-pose data and button state through the OpenXR runtime), the **Microphone** (audio input device), and the **Speakers / Headphones** (audio output device). Data flows are bidirectional with the Visitor, OpenAI, and the headset; unidirectional inbound from the Operator and the Microphone; and unidirectional outbound to the Speakers. The context diagram makes explicit that the SQLite database and the artifact catalogue are *internal* to the system rather than external dependencies.

#### Level-0 DFD Description

The Level-0 decomposition resolves the system into eight major processes:

- **P1 — User Registration & Session Initialization** mediates the lobby form, validates input, persists the visitor record, and seeds the session.
- **P2 — Museum Navigation** translates controller input into locomotion (continuous, snap-turn, and teleportation).
- **P3 — Artifact Identification** runs the gaze raycast pipeline with distance, bounds-centre, dwell, and cooldown gates.
- **P4 — AI Guide Interaction** composes per-artifact system prompts and dispatches them to the Realtime API.
- **P5 — Voice Processing** handles bidirectional audio: microphone capture, encoding, transmission, reception, ring-buffered decoding, and 3D playback.
- **P6 — Visitor Data Management** wraps the SQLite persistence layer.
- **P7 — Information Presentation** spawns and binds the world-space artifact labels.
- **P8 — System Configuration & Session Management** loads the API key and manages the cross-scene `SessionState` singleton.

Six data stores connect these processes: **D1** Visitor Records (SQLite), **D2** Artifact Catalogue (in-memory, 42 entries), **D3** Session State (DontDestroyOnLoad singleton), **D4** Configuration Store (JSON file on disk), **D5** Audio Ring Buffer (30-second in-memory PCM16 FIFO), and **D6** Per-Artifact Cooldown Map (in-memory dictionary). The Level-0 diagram makes explicit that the visitor's lobby submission propagates through P1 → D1 → D3, then drives the scene transition into the museum where P3, P4, P5, and P7 collaborate continuously to deliver the guided experience.

#### Level-1 DFD Description

Each Level-0 process is further decomposed into between three and eight sub-processes documented above. The Level-1 decomposition exposes the internal control flow within each major process — for example, P1 separates configuration loading, form display, validation, record construction, database storage, session seeding, and scene transition into distinct sub-processes; P5 separates the upstream microphone pipeline from the downstream audio-delta consumption pipeline. The Level-1 diagrams are the level at which validation gates and error paths become explicit (e.g., P1.3's interactability gate, P1.5's database-exception handling, P5.6's three-condition mute gate).

#### Level-2 DFD Description

The Level-2 decomposition is applied to the four processes that bear the highest implementation complexity. Each Level-2 diagram exposes the decision points and control flow at frame-level granularity:

- **P3 (Artifact Identification)** Level-2 illustrates the six-gate filtering pipeline (ray hit → ArtifactInfo present → bounds-centre within range → not on cooldown → continuous target → dwell threshold met) leading to the identification event.
- **P4 (AI Guide Interaction)** Level-2 illustrates the WebSocket-state guard, the self-interruption-prevention gate, the system-prompt construction, and the two-frame transmission (`conversation.item.create` followed by `response.create`).
- **P5 (Voice Processing)** Level-2 separates the upstream nine-stage pipeline (mic poll → resample → slice → PCM16 → base64 → input-mode gate → mute-grace gate → WebSocket send → PTT-release commit + response) from the downstream seven-stage pipeline (WebSocket receive → JSON parse → event dispatch → base64 decode → ring-buffer write → audio-thread read → 3D audio render). The 200 ms pre-buffer requirement and the decay-to-silence under-run handler are both visible at Level-2.
- **P1 (Visitor Registration)** Level-2 illustrates the strict sequencing from lobby start through database insert to scene transition, exposing the rollback path on database insert failure.

#### Data Store Description

The system uses six data stores. **D1 (Visitor Records)** is a SQLite database resolved via a portable-first path strategy (`<exe>/MuseumVR/museum.db` or `%APPDATA%` fallback) and storing one row per visitor session. **D2 (Artifact Catalogue)** holds the 42-entry catalogue of Egyptian artifacts authored at edit time and surfaced through `ArtifactInfo` components on scene GameObjects. **D3 (Session State)** is the in-memory cross-scene singleton holding the active visitor record and the OpenAI API key. **D4 (Configuration Store)** is the operator-deposited `config.json` resolved via the same portable-first strategy as D1. **D5 (Audio Ring Buffer)** is a 30-second, 24 kHz, single-channel, lock-protected circular FIFO providing jitter resilience for the streamed audio output. **D6 (Per-Artifact Cooldown Map)** is the in-memory dictionary suppressing re-identification of an artifact within 30 seconds of its previous identification.

#### Data Flow Summary

The data-flow catalogue (Phase 7) enumerates fifty distinct flows. The hottest flows are F11–F17 (head-pose stream from the headset, at frame rate), F27–F36 (the bidirectional audio pipeline, at 24 kHz internal sample rate and ~25 frames per second at the network boundary), and F44–F47 (the avatar's animation-driving signals). The coldest flows are F01–F09 (the once-per-session lobby submission path) and F42 (the once-pre-deployment configuration drop by the Operator).

#### Design Rationale

The decomposition follows three principles validated against the report's implementation. First, **separation of concerns by subsystem**: voice, gaze, navigation, persistence, configuration, and presentation are isolated to permit independent verification and to limit the blast-radius of failures (a Realtime API outage degrades only P4 and P5; a database lock failure surfaces inside P1 and is recoverable). Second, **strict data-store ownership**: each data store has a single writer in the steady state (D1 is written only through `MuseumDatabase`; D3 is written only through `SessionState`; D5 is written only by `StreamingAudioPlayer.EnqueueBase64Pcm16` on the main thread and read only by the audio thread's `OnPcmRead`). Third, **explicit gating between asynchronous boundaries**: the WebSocket's two background threads (send loop, receive loop) communicate with the main thread exclusively through a concurrent queue, and the audio thread communicates with the main thread exclusively through the lock-protected ring buffer. The DFD makes each of these architectural commitments visible at the appropriate level of decomposition.

---

**End of DFD Specification.**
