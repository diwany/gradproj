# Project Schedule and Gantt Chart Specification

> **Project:** AI-Assisted Virtual Reality Museum for Cultural Heritage Education — A Real-Time Speech-Enabled Cultural Heritage Experience for Meta Quest and Desktop Simulator Platforms
>
> **Time-box:** Week 1 of October 2025 → last week of June 2026 (≈ 9 months, ≈ 39 weeks)
> **Team size assumption:** 4–6 senior-year engineering students
> **Constraints respected:** Academic semester breaks, midterm and finals windows, iterative development, integration buffers, documentation throughout.

---

## Phase 1 — Project Analysis

### 1.1 Major project phases (derived from the report)

| # | Phase | Justification (drawn from `PROJECT_REPORT.md`) |
|---|---|---|
| 1 | Initiation & Proposal | Required by AAST for any senior project; defines scope, supervisor, and roles. |
| 2 | Literature Review | Surveyed cultural-heritage VR, real-time conversational AI, gaze interaction, embodied agents (report §2). |
| 3 | Requirements Engineering | 12 functional and 9 non-functional requirements were defined upfront (report §4.1.1, §4.1.2). |
| 4 | System Design | Architecture, subsystem boundaries, data stores, state machines (report §4.2, §4.3). |
| 5 | Environment Setup (Phase 0) | HDRP VR settings, OpenXR, XR Interaction Toolkit, packages (report Phase 0). |
| 6 | VR Foundation (Phase 1) | XR Origin, Meta Quest profiles, museum scene baseline (report Phase 1). |
| 7 | Locomotion & Interaction (Phase 2) | Teleport, snap-turn, controller rays (report Phase 2). |
| 8 | Artifact Recognition (Phase 3) | Gaze detector + 42-artifact catalogue + label spawner; inert ML pipeline (report Phase 3). |
| 9 | Lobby & Persistence (Phase 4) | World-space form, SQLite, SessionState, OpenAIConfig (report Phase 4). |
| 10 | Voice Subsystem (Phase 5) | RealtimeClient, MicCapture, StreamingAudioPlayer, GuideOrchestrator (report Phase 5). |
| 11 | Avatar & NavMesh (Phase 6) | TourGuideAgent, Mixamo wire-up, MaskMap bake, walk-drift fix (report Phase 6). |
| 12 | Build, Deploy, Optimise (Phase 7) | Portable production build pipeline, HDRP optimisation (report Phase 7). |
| 13 | System Integration | Cross-subsystem connection, scene-transition fix, anti-echo guard. |
| 14 | Testing & Evaluation | Frame-rate profile, audio latency breakdown, identification accuracy, reproducibility (report §5). |
| 15 | Documentation | ABET report sections, references, appendices. |
| 16 | Presentation & Demo | Final demo at the project committee. |

### 1.2 Critical dependencies

- **Voice subsystem (Phase 5)** depends on Lobby & Persistence (Phase 4) because the API key is read from configuration and Sessionsstate carries it.
- **Avatar & NavMesh (Phase 6)** depends on the Voice subsystem (Phase 5) because the head-bone AudioSource and the `IsSpeaking` flag wire-up require both audio and animator to be live.
- **Artifact Recognition (Phase 3)** is independent of the Voice and Avatar work and was derisked early.
- **Integration** cannot begin until at least one of Voice or Avatar is functional.
- **Testing** runs continuously after each subsystem milestone.
- **Documentation** runs throughout, with intensification before each submission.

### 1.3 Risk areas

- Mixamo / Unity Humanoid integration (walk-drift) — buffered with a dedicated debug week in Phase 6.
- HDRP / Mixamo material compatibility — buffered with extra material-fix iteration.
- Realtime API behaviour drift — buffered by versioned debug logging in Phase 5.
- Quest Link hardware variability — addressed during integration on multiple machines.

---

## Phase 2 — Project Breakdown Structure (WBS)

### Level 1
1. Project Initiation
2. Research
3. System Design
4. VR Development
5. AI Development
6. Database Development
7. Integration
8. Testing
9. Documentation
10. Presentation Preparation
11. Deployment

### Level 2 and Level 3 (derived from documentation)

```
1. Project Initiation
   1.1 Project proposal
   1.2 Supervisor assignment
   1.3 Team formation and role distribution
   1.4 Tooling baseline (Unity Hub, Git, Discord, asset accounts)

2. Research
   2.1 Cultural-heritage VR literature review
   2.2 Real-time conversational AI architectures
   2.3 Gaze-based interaction in VR
   2.4 Embodied agents and Mixamo workflows
   2.5 Synthesis and positioning section

3. System Design
   3.1 Functional + non-functional requirements
   3.2 Architecture decomposition
   3.3 Data store and data flow design
   3.4 State machine and activity modelling
   3.5 Editor-automation strategy
   3.6 Project schedule and milestones

4. VR Development
   4.1 Phase 0 — Project setup + HDRP VR settings
   4.2 Phase 1 — XR Origin in Museum scene; Quest interaction profiles
   4.3 Phase 2 — Teleport, snap-turn, controller rays, vignette
   4.4 Phase 3 — Gaze artifact identification + label prefab
       4.4.1 GazeArtifactDetector (raycast + dwell + cooldown)
       4.4.2 Bounds-centre verification
       4.4.3 42-artifact catalogue authoring
       4.4.4 ArtifactInfoAutoFill utility
       4.4.5 ArtifactLabelSpawner + ArtifactLabel.prefab
   4.5 Inert ML pipeline (parallel research deliverable)

5. AI Development
   5.1 OpenAI account setup, key handling
   5.2 RealtimeClient (WebSocket transport)
   5.3 MicCapture (resample + PCM16 + base64)
   5.4 StreamingAudioPlayer (30 s ring + 200 ms prebuffer)
   5.5 GuideOrchestrator (3 input modes + event handlers)
   5.6 System prompt + English language lock
   5.7 Avatar embodiment
       5.7.1 NavMesh bake + placeholder body
       5.7.2 Distance-keeping behaviour
       5.7.3 Mixamo wire-up
       5.7.4 Walk-drift hip XZ lock
       5.7.5 Materials + MaskMap bake
       5.7.6 AmplitudeJawFlap

6. Database Development
   6.1 SQLite via gilzoide.sqlite-net (OpenUPM)
   6.2 VisitorRecord schema
   6.3 MuseumDatabase wrapper
   6.4 Portable path resolution
   6.5 SessionState singleton (DontDestroyOnLoad)

7. Integration
   7.1 Lobby → Museum scene transition
   7.2 Single-mode load (dual XR Origin fix)
   7.3 GuideOrchestrator ↔ GazeArtifactDetector wiring
   7.4 AmplitudeJawFlap ↔ TourGuideAgent ↔ Animator
   7.5 Cross-subsystem smoke tests
   7.6 Push-to-Talk on Quest controllers

8. Testing
   8.1 Subsystem unit tests
   8.2 Gaze identification on all 42 artifacts
   8.3 Audio latency end-to-end measurement
   8.4 Frame-time profiling
   8.5 OpenAI cost per visit measurement
   8.6 Reproducibility on a fresh Windows PC
   8.7 User-acceptance walkthroughs

9. Documentation
   9.1 Progress logs
   9.2 Editor menu inventory
   9.3 Artifact catalogue (42 entries)
   9.4 ABET report chapters 1–7
   9.5 IEEE research paper
   9.6 Appendices (source listings, menu reference, build, catalogue)
   9.7 Diagrams (DFD, flowchart, UML activity, UML state, Gantt)

10. Presentation Preparation
   10.1 Slide deck
   10.2 Live demo rehearsal
   10.3 Poster
   10.4 Backup demo recording

11. Deployment
   11.1 Portable production build pipeline
   11.2 README authoring
   11.3 USB demo packaging
   11.4 Demo machine provisioning
```

---

## Phase 3 — Detailed Task Schedule

The schedule numbers weeks from **W1 = Week of 2025-09-29** (first Monday in October) through **W39 = Week of 2026-06-22** (final week before the AAST submission window).

Each task lists start and end weeks, dependencies, deliverables, and the team role responsible.

| Task ID | Task Name | Description | Start | End | Dur (wks) | Dependencies | Deliverable | Responsible Role |
|---|---|---|---|---|---|---|---|---|
| T01 | Project proposal | Define scope, supervisor signs | W1 | W2 | 2 | — | Approved proposal | All |
| T02 | Team role distribution | Assign Tech Lead, AI Lead, VR Lead, DB Lead, Docs Lead, QA Lead | W1 | W2 | 2 | T01 | RACI matrix | All |
| T03 | Tooling baseline | Unity Hub, Git repo, Discord, OpenAI account | W2 | W3 | 2 | T01 | Repo + CI dummy | Tech Lead |
| T04 | Literature review — VR heritage | Survey Smithsonian, British Museum, Louvre, AK Studio Art | W2 | W5 | 4 | T01 | Notes + summary | Docs Lead |
| T05 | Literature review — Real-time AI | OpenAI Realtime API, prior STT+LLM+TTS pipelines | W2 | W5 | 4 | T01 | Notes | AI Lead |
| T06 | Literature review — Gaze in VR | Tanriverdi & Jacob, Pfeuffer | W3 | W5 | 3 | T01 | Notes | VR Lead |
| T07 | Literature review — Embodied agents | Cassell et al.; Mixamo workflow | W3 | W5 | 3 | T01 | Notes | AI Lead |
| T08 | Requirements engineering | 12 functional + 9 non-functional reqs | W4 | W6 | 3 | T01 | Requirements doc | Tech Lead |
| T09 | System architecture design | Subsystem decomposition, data stores | W5 | W7 | 3 | T08 | Architecture doc | Tech Lead + AI Lead |
| T10 | Data flow + state design | DFD/Activity/State drafts | W6 | W8 | 3 | T09 | UML drafts | Docs Lead |
| T11 | Schedule + milestones | This Gantt chart's source schedule | W6 | W7 | 2 | T08 | Schedule doc | Tech Lead |
| T12 | Phase 0 — Package install + HDRP VR settings | Unity 6, HDRP, XR plug-ins, AI Navigation, Newtonsoft, sqlite-net via OpenUPM | W6 | W8 | 3 | T03 | Compiling project | VR Lead |
| T13 | Phase 1 — VR foundation | XR Origin, Meta Quest profiles, Museum.unity from AK Studio Art | W7 | W9 | 3 | T12 | Quest-tracked museum scene | VR Lead |
| T14 | Phase 2 — Locomotion | Teleport, snap-turn, controller rays | W9 | W11 | 3 | T13 | Locomotion working | VR Lead |
| T15 | Phase 3.1 — Gaze detector | GazeArtifactDetector + bounds-centre + 30 s cooldown | W10 | W13 | 4 | T13 | Detector firing | VR Lead |
| T16 | Phase 3.2 — Label spawner + prefab | ArtifactLabelSpawner + world-space label | W11 | W13 | 3 | T15 | Labels visible | VR Lead |
| T17 | Phase 3.3 — Artifact catalogue (42 entries) | ArtifactInfoAutoFill dictionary | W12 | W14 | 3 | — | 42 entries authored | Docs Lead + VR Lead |
| T18 | Phase 3.4 — Inert ML pipeline | InferenceEngineArtifactClassifier, ArtifactCaptureCamera, RecognitionPipeline | W12 | W16 | 5 | T12 | Code in repo (inert) | AI Lead |
| T19 | Midterm break / exams (Fall semester) | Reduced velocity | W13 | W14 | 2 | — | — | All |
| T20 | Phase 4.1 — SQLite + VisitorRecord schema | gilzoide.sqlite-net, schema, OpenAIConfig | W15 | W17 | 3 | T12 | DB wrapper functional | DB Lead |
| T21 | Phase 4.2 — Lobby scene generation | World-space form, hieroglyph backdrop, country list | W15 | W18 | 4 | T20 | Lobby scene runs | VR Lead + DB Lead |
| T22 | Phase 4.3 — SessionState + cross-scene | DontDestroyOnLoad singleton | W17 | W19 | 3 | T20 | Cross-scene preserved | DB Lead |
| T23 | Phase 5.1 — RealtimeClient | WebSocket, JSON framing, OnEvent/OnOpen/OnClose | W17 | W19 | 3 | T20 | Connect to API | AI Lead |
| T24 | Phase 5.2 — MicCapture | Resample to 24 kHz, PCM16, base64 chunks | W18 | W20 | 3 | T23 | Mic chunks streaming | AI Lead |
| T25 | Phase 5.3 — StreamingAudioPlayer | 30 s ring buffer, 200 ms prebuffer, PCMReaderCallback | W19 | W21 | 3 | T23 | Audio playback stable | AI Lead |
| T26 | Phase 5.4 — GuideOrchestrator | Three input modes, event handlers, English lock prompt | W20 | W23 | 4 | T23, T24, T25 | Conversational guide live | AI Lead |
| T27 | Fall semester final exams | Reduced velocity | W14 | W16 | 3 (overlap) | — | — | All |
| T28 | Winter break (Jan) — limited progress | Light tasks only (docs, planning) | W17 | W18 | 2 (overlap) | — | — | All |
| T29 | Phase 6.1 — NavMesh + placeholder body | Bake NavMesh, capsule placeholder + TourGuideAgent | W22 | W23 | 2 | T26 | Capsule follows player | VR Lead |
| T30 | Phase 6.2 — Distance-keeping behaviour | Desired-position formula, hysteresis | W23 | W24 | 2 | T29 | Smooth follow | VR Lead |
| T31 | Phase 6.3 — Mixamo wire-up | MixamoWireUp, Animator controller, audio reparent | W23 | W25 | 3 | T29 | Avatar in scene | AI Lead |
| T32 | Phase 6.4 — Walk-drift hip XZ lock | TourGuideAgent.LateUpdate fix | W24 | W25 | 2 | T31 | No drift visible | AI Lead |
| T33 | Phase 6.5 — Materials + MaskMap bake | MixamoMaterialFix + MixamoMaskMapBaker | W24 | W26 | 3 | T31 | Avatar textured | VR Lead |
| T34 | Phase 6.6 — AmplitudeJawFlap | RMS-driven IsSpeaking + jaw bone rotation | W25 | W26 | 2 | T31, T26 | Mouth animates with audio | AI Lead |
| T35 | Integration block A — Lobby↔Museum, voice↔gaze | Single-mode scene transition; PTT on Quest | W26 | W28 | 3 | T22, T26, T32 | E2E walkthrough works | Tech Lead |
| T36 | Anti-echo guard + mute grace | muteMicWhileModelSpeaks + 0.4 s grace | W26 | W27 | 2 | T26 | Echo loops eliminated | AI Lead |
| T37 | Hieroglyph backdrop + form polish | Add Egyptian wall texture, expand country list | W27 | W28 | 2 | T21 | Lobby visually polished | VR Lead |
| T38 | Phase 7.1 — Build automation | PortablePackageBuilder, README, pre-flight checks | W28 | W30 | 3 | T35 | Portable build produced | Tech Lead |
| T39 | Spring semester midterm break / exams | Reduced velocity | W28 | W29 | 2 | — | — | All |
| T40 | Testing — gaze on 42 artifacts | Per-artifact pass/fail count | W29 | W30 | 2 | T35 | Test report | QA Lead |
| T41 | Testing — audio latency breakdown | Per-stage timing measurement | W29 | W31 | 3 | T35 | Latency table | QA Lead |
| T42 | Testing — frame profile | 10-min tour CPU/GPU profile | W30 | W31 | 2 | T35 | Frame-time chart | QA Lead |
| T43 | Testing — OpenAI cost per visit | Usage logs analysis | W30 | W31 | 2 | T35 | Cost figure | QA Lead |
| T44 | Reproducibility test (fresh PC) | USB demo on a clean Windows machine with Quest Link | W30 | W31 | 2 | T38 | Pass/fail report | QA Lead |
| T45 | Bug-fix iteration 1 | Address findings from T40–T44 | W31 | W32 | 2 | T40–T44 | Bug log closed | All |
| T46 | Subsystem hardening — voice | Handle reconnect, error events | W31 | W32 | 2 | T26 | Robust voice path | AI Lead |
| T47 | Subsystem hardening — avatar | Diagnose & Repair Avatar Materials utility | W31 | W32 | 2 | T33 | Diagnostic tool | VR Lead |
| T48 | User-acceptance testing — round 1 | 5 external users | W32 | W33 | 2 | T45 | UAT feedback | QA Lead + Docs Lead |
| T49 | Bug-fix iteration 2 | Address UAT findings | W33 | W34 | 2 | T48 | Bug log closed | All |
| T50 | Documentation — ABET chapters 1–4 | Intro, lit review, terminology, proposed model | W6 | W30 | 25 (rolling) | T08 | Draft chapters | Docs Lead |
| T51 | Documentation — Chapter 5 (eval) | Simulation and performance evaluation | W30 | W34 | 5 | T40–T44 | Chapter 5 final | Docs Lead + QA Lead |
| T52 | Documentation — Chapter 6 (business model) | Canvas + analysis | W31 | W34 | 4 | T10 | Chapter 6 final | Docs Lead |
| T53 | Documentation — Chapter 7 + appendices | Conclusion, source listings, menus, catalogue | W33 | W36 | 4 | All chapters | Final report assembly | Docs Lead |
| T54 | IEEE research paper | Compressed version for journal/conf submission | W30 | W36 | 7 | T50, T51 | Paper draft | Docs Lead |
| T55 | Diagrams — DFD / Activity / State / Flow / Gantt | Generation + rendering in Visio / draw.io | W33 | W35 | 3 | T10 | Final figures | Docs Lead |
| T56 | Spring final exams (light period) | Reduced velocity | W35 | W36 | 2 | — | — | All |
| T57 | Final integration freeze | No new features; only fixes | W36 | W36 | 1 | All dev | Frozen build | Tech Lead |
| T58 | Performance optimisation pass | Lighting bake, LODs (deferred from Phase 7) | W36 | W37 | 2 | T57 | ≥ 72 fps confirmed | VR Lead |
| T59 | Presentation slides | Deck + speaker notes | W36 | W37 | 2 | T53 | Slide deck | Docs Lead + Tech Lead |
| T60 | Demo rehearsal | Three full run-throughs | W37 | W38 | 2 | T58, T59 | Polished demo | All |
| T61 | Backup demo recording | Pre-recorded fallback video | W37 | W38 | 2 | T58 | Recording in repo | QA Lead |
| T62 | Poster | A0/A1 academic poster | W37 | W38 | 2 | T53 | Poster PDF | Docs Lead |
| T63 | Demo machine provisioning | USB + Quest + cables ready | W37 | W38 | 2 | T38 | Demo kit | Tech Lead |
| T64 | Final report submission | AAST submission window | W38 | W39 | 2 | T53, T54 | Report submitted | All |
| T65 | Project defence | Committee presentation + Q&A | W39 | W39 | 1 | T64 | Final defence | All |

---

## Phase 4 — Month-by-Month Timeline

### October 2025 (W1–W4)
- **Major activities:** Proposal approval, team formation, tooling baseline, kick-off of literature review across four areas, requirements engineering start.
- **Deliverables:** Approved proposal; team RACI; Unity project skeleton in Git; lit-review notes draft.
- **Milestones:** **M1 — Project Proposal Approved** (W2).
- **Expected progress:** Team operational, baseline reading complete, requirements draft circulating.

### November 2025 (W5–W8)
- **Major activities:** Architecture finalised; Phase 0 (HDRP VR settings + packages); Phase 1 (XR Origin in Museum scene); start Phase 2 (locomotion).
- **Deliverables:** Architecture document; compiling Unity project; XR-tracked museum environment.
- **Milestones:** **M2 — Literature Review Completed** (W6); **M3 — System Design Approved** (W8).

### December 2025 (W9–W12)
- **Major activities:** Phase 2 complete (teleport + snap-turn); start Phase 3 (gaze detector); begin 42-artifact cataloguing; start parallel inert ML branch.
- **Deliverables:** Locomotion working in headset; gaze detector firing on test artifacts.
- **Milestones:** **M4 — VR Museum Prototype Completed** (W12).
- **Note:** Fall semester midterms (mid-December) reduce velocity briefly.

### January 2026 (W13–W17)
- **Major activities:** Phase 3 hardening (bounds-centre, cooldown); finalise 42-artifact catalogue; begin Phase 4 (SQLite + lobby scene); winter break — slower pace.
- **Deliverables:** All 42 artifacts identifiable on dwell; Lobby form rendering; SQLite wrapper functional.
- **Milestones:** **M5 — Artifact Recognition Functional** (W14); **M6 — Database Functional** (W17).

### February 2026 (W18–W21)
- **Major activities:** Phase 4 complete (SessionState + cross-scene); Phase 5 underway (RealtimeClient + MicCapture + StreamingAudioPlayer); finalise English-locked system prompt.
- **Deliverables:** End-to-end lobby submission; conversational voice path live.
- **Milestones:** *(none formally — preparatory)*.

### March 2026 (W22–W26)
- **Major activities:** Phase 5 complete (orchestrator + three input modes); Phase 6 (NavMesh, Mixamo wire-up, materials, MaskMap, hip XZ lock, jaw flap).
- **Deliverables:** AI guide live in museum; Mixamo Pharaoh follows visitor and animates correctly.
- **Milestones:** **M7 — AI Tour Guide Integrated** (W26).

### April 2026 (W27–W31)
- **Major activities:** Integration; anti-echo guard; hieroglyph backdrop polish; build automation; testing block A (gaze, latency, frame profile, cost, reproducibility); spring midterm windows.
- **Deliverables:** Production-grade portable build; complete test report.
- **Milestones:** **M8 — Full System Integration** (W28); **M9 — System Testing Completed** (W31).

### May 2026 (W32–W35)
- **Major activities:** UAT round 1; bug-fix iteration 2; documentation push (Chapters 5–7 + appendices + IEEE paper); diagram generation.
- **Deliverables:** Final report draft; IEEE paper draft; figure pack rendered.
- **Milestones:** **M10 — Final Documentation Completed** (W35).

### June 2026 (W36–W39)
- **Major activities:** Final integration freeze; optimisation pass (lighting bake + LODs); presentation slides; demo rehearsal; backup recording; poster; demo machine provisioning; report submission; defence.
- **Deliverables:** Final demo, polished slide deck, A1 poster, submitted report, IEEE paper, USB demo kit.
- **Milestones:** **M11 — Final Demonstration Ready** (W37); **M12 — Project Submission** (W39).

---

## Phase 5 — Milestones

| ID | Milestone | Target Week | Calendar Date (approx.) | Dependency |
|---|---|---|---|---|
| M1 | Project Proposal Approved | W2 | 2025-10-13 | T01 |
| M2 | Literature Review Completed | W6 | 2025-11-10 | T04–T07 |
| M3 | System Design Approved | W8 | 2025-11-24 | T08–T11 |
| M4 | VR Museum Prototype Completed | W12 | 2025-12-22 | T12–T14 |
| M5 | Artifact Recognition Functional | W14 | 2026-01-05 | T15–T17 |
| M6 | Database & Lobby Functional | W17 | 2026-01-26 | T20–T22 |
| M7 | AI Tour Guide Integrated | W26 | 2026-03-30 | T23–T34 |
| M8 | Full System Integration | W28 | 2026-04-13 | T35–T37 |
| M9 | System Testing Completed | W31 | 2026-05-04 | T40–T45 |
| M10 | Final Documentation Completed | W35 | 2026-06-01 | T50–T55 |
| M11 | Final Demonstration Ready | W37 | 2026-06-15 | T58–T63 |
| M12 | Project Submission | W39 | 2026-06-29 | T64, T65 |

---

## Phase 6 — Critical Path Analysis

### 6.1 Critical path

**T01 → T08 → T09 → T12 → T13 → T15 → T20 → T22 → T23 → T26 → T31 → T34 → T35 → T40–T45 → T48 → T49 → T53 → T57 → T58 → T59 → T60 → T64 → T65**

These tasks are critical because each subsequent task cannot meaningfully begin until the prior is complete (architecture before code; voice transport before orchestrator; orchestrator before integration; integration before testing; testing before final documentation; documentation before submission).

### 6.2 Why these tasks form the critical path

- **T09 → T12:** Architecture decisions (HDRP, OpenXR, sqlite-net) drive the package install order.
- **T20 → T23:** API key handling and SessionState must be available before any Realtime API connection can be made meaningful (the orchestrator reads the key from `SessionState`).
- **T26 → T31:** The avatar's `AudioSource` is parented to the head bone produced by Mixamo wire-up; without a wired Mixamo avatar there is no head bone to parent to.
- **T35 → testing block:** Integration must precede measurement.
- **T57 → submission:** Feature freeze precedes all polish, optimisation, and final report assembly.

### 6.3 Buffers

- W19 (December midterm dip), W17–W18 (winter break), W28–W29 (spring midterm dip), W35–W36 (final-exam dip) are explicit reduced-velocity windows in the schedule.
- Bug-fix iterations T45 (1 wk) and T49 (1 wk) absorb late-discovered issues.
- T46 (voice hardening) and T47 (avatar diagnostic) are explicit buffer tasks added in April.

### 6.4 Risk areas

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| Mixamo walk drift | High | Medium | Allocated explicit week (T32) + runtime safety net |
| HDRP / Mixamo materials pink | Medium | Medium | Allocated T33 + post-build diagnostic utility |
| Realtime API behaviour change | Low | High | Versioned debug logging; the `RealtimeClient` interface abstracts the dependency |
| Server-VAD echo loops | High | High | Default to Push-To-Talk; T36 anti-echo guard week |
| Quest Link hardware variance | Medium | Medium | Multi-machine testing in T44 |
| Late-stage scope creep | Medium | High | Hard feature freeze at T57 |

---

## Phase 7 — Resource Allocation

| Role | Responsibilities | Lead Tasks |
|---|---|---|
| **Tech Lead** | Architecture, integration, build pipeline, repository hygiene | T03, T08, T09, T11, T35, T38, T57, T63 |
| **VR Development Lead** | XR Origin, locomotion, gaze detector, lobby scene, materials, optimisation | T12, T13, T14, T15, T16, T21, T33, T37, T47, T58 |
| **AI / Voice Lead** | Realtime API integration, mic capture, audio player, orchestrator, avatar embodiment | T05, T07, T23, T24, T25, T26, T31, T32, T34, T36, T46 |
| **Database / Persistence Lead** | SQLite wrapper, schema, SessionState, OpenAIConfig | T20, T22 |
| **QA / Testing Lead** | Test plans, instrumentation, UAT coordination, demo recording | T40, T41, T42, T43, T44, T48, T61 |
| **Documentation Lead** | ABET report, IEEE paper, diagrams, poster, slide deck | T04, T17, T50, T51, T52, T53, T54, T55, T59, T62 |

All members participate in proposal, requirements, integration freeze, demo rehearsal, and the final defence.

---

## Phase 8 — Testing & Validation Schedule

| Test type | Window | Owner | Description |
|---|---|---|---|
| Subsystem unit tests | Throughout | Each lead | Per-subsystem smoke tests as subsystems land |
| Gaze identification (42 artifacts) | W29–W30 | QA Lead | Walk-up test for every catalogued artifact, three runs each |
| Audio latency end-to-end | W29–W31 | QA Lead | Stage-by-stage timing instrumented in `GuideOrchestrator` |
| Frame-time profile | W30–W31 | QA Lead | XR Profiler over a 10-minute representative tour |
| OpenAI cost per visit | W30–W31 | QA Lead | OpenAI usage dashboard correlated with session length |
| Reproducibility (fresh PC) | W30–W31 | Tech Lead + QA Lead | Run portable build on a clean Windows machine |
| Bug-fix iteration 1 | W31–W32 | All | Address findings from test block A |
| Voice hardening | W31–W32 | AI Lead | Error events, reconnect-on-close, retry semantics |
| Avatar diagnostic | W31–W32 | VR Lead | `Diagnose & Repair Avatar Materials` utility |
| UAT round 1 | W32–W33 | QA Lead + Docs Lead | 5 external users in the lab |
| Bug-fix iteration 2 | W33–W34 | All | Address UAT findings |
| Final integration freeze | W36 | Tech Lead | No new features; fix-only |
| Optimisation pass | W36–W37 | VR Lead | Lighting bake, LODs, ≥ 72 fps confirmed |

---

## Phase 9 — Documentation Schedule

| Document | Window | Owner |
|---|---|---|
| Project proposal | W1–W2 | All |
| Progress logs (rolling) | W1–W39 | Docs Lead |
| Research notes by area | W2–W6 | All four leads |
| Architecture document | W5–W8 | Tech Lead |
| Requirements document | W4–W6 | Tech Lead |
| Schedule (this document) | W6–W7 | Tech Lead |
| ABET Chapters 1–4 (rolling) | W6–W30 | Docs Lead |
| Editor menu inventory | W22–W30 | All leads |
| Artifact catalogue document | W12–W14 | Docs Lead + VR Lead |
| ABET Chapter 5 (evaluation) | W30–W34 | Docs Lead + QA Lead |
| ABET Chapter 6 (business model) | W31–W34 | Docs Lead |
| ABET Chapter 7 (conclusion) + appendices | W33–W36 | Docs Lead |
| Final figures (DFD, UML, Gantt) | W33–W35 | Docs Lead |
| IEEE research paper | W30–W36 | Docs Lead |
| Presentation slide deck | W36–W37 | Docs Lead + Tech Lead |
| Poster (A0/A1) | W37–W38 | Docs Lead |
| Submission package | W38–W39 | All |

---

## Phase 10 — Final Gantt Chart Specification

### 10.1 Gantt chart table (ready for Microsoft Project / Excel / draw.io import)

| Task ID | Task Name | Start Week | End Week | Duration (wks) | Dependencies | Milestone | Team Responsible |
|---|---|---|---|---|---|---|---|
| T01 | Project proposal | W1 | W2 | 2 | — | M1 (W2) | All |
| T02 | Team role distribution | W1 | W2 | 2 | T01 | — | All |
| T03 | Tooling baseline | W2 | W3 | 2 | T01 | — | Tech Lead |
| T04 | Lit review — VR heritage | W2 | W5 | 4 | T01 | — | Docs Lead |
| T05 | Lit review — Real-time AI | W2 | W5 | 4 | T01 | — | AI Lead |
| T06 | Lit review — Gaze in VR | W3 | W5 | 3 | T01 | — | VR Lead |
| T07 | Lit review — Embodied agents | W3 | W5 | 3 | T01 | — | AI Lead |
| T08 | Requirements | W4 | W6 | 3 | T01 | — | Tech Lead |
| T09 | Architecture design | W5 | W7 | 3 | T08 | — | Tech Lead + AI Lead |
| T10 | DFD/Activity/State drafts | W6 | W8 | 3 | T09 | — | Docs Lead |
| T11 | Schedule + milestones | W6 | W7 | 2 | T08 | — | Tech Lead |
| T12 | Phase 0 — HDRP VR settings + packages | W6 | W8 | 3 | T03 | — | VR Lead |
| T13 | Phase 1 — VR foundation | W7 | W9 | 3 | T12 | — | VR Lead |
| T14 | Phase 2 — Locomotion | W9 | W11 | 3 | T13 | — | VR Lead |
| T15 | Phase 3.1 — Gaze detector | W10 | W13 | 4 | T13 | — | VR Lead |
| T16 | Phase 3.2 — Label spawner | W11 | W13 | 3 | T15 | — | VR Lead |
| T17 | Phase 3.3 — Catalogue (42) | W12 | W14 | 3 | — | M5 (W14) | Docs + VR Lead |
| T18 | Phase 3.4 — Inert ML pipeline | W12 | W16 | 5 | T12 | — | AI Lead |
| T20 | Phase 4.1 — SQLite + schema | W15 | W17 | 3 | T12 | — | DB Lead |
| T21 | Phase 4.2 — Lobby scene | W15 | W18 | 4 | T20 | — | VR + DB Lead |
| T22 | Phase 4.3 — SessionState | W17 | W19 | 3 | T20 | M6 (W17) | DB Lead |
| T23 | Phase 5.1 — RealtimeClient | W17 | W19 | 3 | T20 | — | AI Lead |
| T24 | Phase 5.2 — MicCapture | W18 | W20 | 3 | T23 | — | AI Lead |
| T25 | Phase 5.3 — StreamingAudioPlayer | W19 | W21 | 3 | T23 | — | AI Lead |
| T26 | Phase 5.4 — GuideOrchestrator | W20 | W23 | 4 | T23, T24, T25 | — | AI Lead |
| T29 | Phase 6.1 — NavMesh + placeholder | W22 | W23 | 2 | T26 | — | VR Lead |
| T30 | Phase 6.2 — Distance-keeping | W23 | W24 | 2 | T29 | — | VR Lead |
| T31 | Phase 6.3 — Mixamo wire-up | W23 | W25 | 3 | T29 | — | AI Lead |
| T32 | Phase 6.4 — Hip XZ lock | W24 | W25 | 2 | T31 | — | AI Lead |
| T33 | Phase 6.5 — Materials + MaskMap | W24 | W26 | 3 | T31 | — | VR Lead |
| T34 | Phase 6.6 — JawFlap | W25 | W26 | 2 | T31, T26 | M7 (W26) | AI Lead |
| T35 | Integration block A | W26 | W28 | 3 | T22, T26, T32 | M8 (W28) | Tech Lead |
| T36 | Anti-echo guard | W26 | W27 | 2 | T26 | — | AI Lead |
| T37 | Lobby polish | W27 | W28 | 2 | T21 | — | VR Lead |
| T38 | Phase 7.1 — Build automation | W28 | W30 | 3 | T35 | — | Tech Lead |
| T40 | Testing — gaze (42) | W29 | W30 | 2 | T35 | — | QA Lead |
| T41 | Testing — audio latency | W29 | W31 | 3 | T35 | — | QA Lead |
| T42 | Testing — frame profile | W30 | W31 | 2 | T35 | — | QA Lead |
| T43 | Testing — OpenAI cost | W30 | W31 | 2 | T35 | — | QA Lead |
| T44 | Reproducibility test | W30 | W31 | 2 | T38 | — | Tech + QA |
| T45 | Bug-fix iteration 1 | W31 | W32 | 2 | T40–T44 | M9 (W31) | All |
| T46 | Voice hardening | W31 | W32 | 2 | T26 | — | AI Lead |
| T47 | Avatar diagnostic | W31 | W32 | 2 | T33 | — | VR Lead |
| T48 | UAT round 1 | W32 | W33 | 2 | T45 | — | QA + Docs Lead |
| T49 | Bug-fix iteration 2 | W33 | W34 | 2 | T48 | — | All |
| T50 | Docs — Chapters 1–4 | W6 | W30 | 25 (rolling) | T08 | — | Docs Lead |
| T51 | Docs — Chapter 5 | W30 | W34 | 5 | T40–T44 | — | Docs + QA |
| T52 | Docs — Chapter 6 | W31 | W34 | 4 | T10 | — | Docs Lead |
| T53 | Docs — Chapter 7 + appendices | W33 | W36 | 4 | T50–T52 | M10 (W35) | Docs Lead |
| T54 | IEEE paper | W30 | W36 | 7 | T50, T51 | — | Docs Lead |
| T55 | Final diagrams | W33 | W35 | 3 | T10 | — | Docs Lead |
| T57 | Final integration freeze | W36 | W36 | 1 | All dev | — | Tech Lead |
| T58 | Optimisation pass | W36 | W37 | 2 | T57 | — | VR Lead |
| T59 | Presentation slides | W36 | W37 | 2 | T53 | — | Docs + Tech |
| T60 | Demo rehearsal | W37 | W38 | 2 | T58, T59 | M11 (W37) | All |
| T61 | Backup demo recording | W37 | W38 | 2 | T58 | — | QA Lead |
| T62 | Poster | W37 | W38 | 2 | T53 | — | Docs Lead |
| T63 | Demo machine provisioning | W37 | W38 | 2 | T38 | — | Tech Lead |
| T64 | Final report submission | W38 | W39 | 2 | T53, T54 | M12 (W39) | All |
| T65 | Project defence | W39 | W39 | 1 | T64 | — | All |

---

## Phase 11 — Project Schedule and Gantt Chart Analysis (ABET-style)

### Project Planning Approach

The project was planned using a **phase-gated incremental development model** following the seven-phase decomposition documented in the project report (Phase 0 — Project Setup through Phase 7 — Build and Distribution). Each phase establishes a verifiable deliverable that gates subsequent phases. The planning approach intentionally derisks the highest-uncertainty subsystems early: the real-time AI integration (Phase 5) and the Mixamo avatar integration (Phase 6.3–6.4) are introduced only after the lower-risk Phases 0–4 have produced a stable foundation. This phase-gated approach is consistent with the "verify before advancing" practice the project report identifies as a key engineering lesson.

### Phase Breakdown

The schedule covers nine months from the first week of October 2025 to the last week of June 2026. The nine months are partitioned into four implementation blocks:

- **October–November 2025 (W1–W8):** Initiation, research, requirements, and architecture.
- **December 2025–January 2026 (W9–W17):** VR foundation, locomotion, gaze identification, and the lobby + persistence subsystem.
- **February–March 2026 (W18–W26):** Real-time voice subsystem and embodied tour guide avatar.
- **April–June 2026 (W27–W39):** Integration, testing, documentation, presentation, and submission.

This partition aligns with the AAST academic calendar's natural rhythm: research and design before the winter break, voice and avatar work during the spring term, and intensive integration and documentation in the final two months.

### Milestones

Twelve milestones (M1–M12) anchor the schedule. M1–M3 confirm scope and architecture; M4–M7 confirm subsystem completion; M8 confirms integration; M9 confirms testing; M10–M12 confirm documentation, demo readiness, and submission. Each milestone has explicit task dependencies recorded in §5.

### Resource Allocation

The team is structured around six roles (Tech Lead, VR Lead, AI Lead, DB Lead, QA Lead, Docs Lead). The distribution reflects the project's actual implementation: subsystems requiring tight knowledge of one technology stack (HDRP/XR for VR; WebSockets and PCM streaming for voice; SQLite for persistence) are owned by a single role to limit context-switching cost. Cross-cutting tasks (proposal, integration freeze, demo rehearsal, defence) involve the whole team.

### Risk Management

Three classes of risk dominate the schedule and are explicitly buffered: (1) **technology integration risks** — Mixamo walk drift, HDRP/Mixamo material incompatibility — are absorbed by dedicated tasks (T32, T33) with their own week-long allocations; (2) **external dependency risks** — Realtime API behaviour change, Quest Link hardware variance — are mitigated by versioned debug logging and multi-machine reproducibility testing (T44); and (3) **schedule risks** from academic constraints — midterm and final exam windows — are explicitly marked as reduced-velocity periods with no critical-path deliverables scheduled inside them.

### Schedule Justification

The 39-week timeline is justified by the implementation complexity documented in the report. The voice subsystem alone (Phases 5.1–5.4) consumes seven weeks (T23–T26) due to the WebSocket transport, the 24 kHz resampling pipeline, the 30-second ring buffer with 200 ms pre-buffer, and the three-input-mode orchestrator. The Mixamo avatar pipeline (T29–T34) consumes another five weeks due to material conversion, MaskMap baking, walk-drift fix, and jaw-flap integration. Cumulative testing (T40–T49) consumes another five weeks, including UAT and two bug-fix iterations. The final-month sequence (T57–T65) is deliberately overspecified with feature-freeze, optimisation, slide deck, rehearsal, backup recording, poster, demo provisioning, and submission as discrete tasks, mirroring the polished delivery the project's ABET defence requires.

### Critical Path

The critical path runs through architecture (T09), Phase 0 environment setup (T12), gaze detector (T15), lobby + database (T20–T22), voice transport and orchestrator (T23, T26), Mixamo wire-up and walk-drift fix (T31, T32), integration block A (T35), testing block (T40–T45), UAT and second bug-fix (T48–T49), final documentation (T53), feature freeze (T57), optimisation (T58), presentation prep (T59–T60), and submission/defence (T64–T65). Any slip in any task on this critical chain propagates one-for-one into the submission date. The buffers identified in §6.3 — explicit bug-fix iterations and the hardening windows — exist precisely to absorb such slippage without endangering M12.

### Final Note on the Gantt Output

The task table in §10.1 is in a form directly importable into Microsoft Project (CSV with Task ID, Start, End, Predecessors), Excel (column-per-week shaded cell layout), draw.io (Gantt template), or Lucidchart's Gantt template. The milestone column converts to diamond markers in any standard Gantt-chart tool. The combination of weekly granularity, explicit dependencies, role assignments, and milestone anchors makes the schedule sufficient for both project execution and final report inclusion as an ABET-compliant figure.

---

**End of Project Schedule and Gantt Chart Specification.**
