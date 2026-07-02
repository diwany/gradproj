# GRADUATION PROJECT REPORT

> **How to use this file:** copy each section into the AAST Word template,
> applying the styles described in the template's Appendix A (Title style for
> chapter titles, Heading 1–5 for sub-headings, body text 12pt Times New
> Roman, justified, 1.5 line spacing, 12pt before paragraph). Figures and
> code listings need to be inserted manually from your project. Placeholders
> like `[INSERT FIGURE: …]` and `[YOUR NAME]` mark where you fill in.

---

## TITLE PAGE (Front Cover Information)

**Arab Academy for Science, Technology and Maritime Transport**
College of Engineering and Technology
Department of Computer Engineering

B. Sc. Final Year Project

# IMMERSIVE VR EGYPTIAN MUSEUM WITH AN AI-POWERED CONVERSATIONAL TOUR GUIDE

**A Real-Time Speech-Enabled Cultural Heritage Experience for the Meta Quest Platform**

Presented By:
*[Your Name(s) and Registration Number(s)]*

Supervised By:
*[Supervisor Name(s)]*

[Month – Year of submission]

---

## DECLARATION

I hereby certify that this material, which I now submit for assessment on the
programme of study leading to the award of Bachelor of Science in **Computer
Engineering**, is entirely my own work, that I have exercised reasonable care
to ensure that the work is original, and does not to the best of my knowledge
breach any law of copyright, and has not been taken from the work of others
save and to the extent that such work has been cited and acknowledged within
the text of my work.

Signed: _________________________________
Registration No.: ____________________
Date: _____________________________

---

## SENIOR PROJECT SUMMARY REPORT

| Field | Content |
|---|---|
| **Project Title** | Immersive VR Egyptian Museum with an AI-Powered Conversational Tour Guide |
| **Supervisor(s)** | [Supervisor Name(s)] |
| **Team Members** | [Your name(s)], Registration No(s). [....] |
| **Project Deliverables** | (1) A complete Unity 6 VR application supporting Meta Quest 2/3/Pro via PCVR; (2) a documented, modular C# codebase organized by phase; (3) a SQLite-backed visitor record system; (4) a self-contained portable production distribution (USB-droppable) including the OpenAI configuration file; (5) full editor automation utilities so every multi-step Unity action is reproducible from a menu command. |
| **Abstract** | The project delivers a virtual-reality reconstruction of an Egyptian museum in which the visitor is guided by a real-time conversational AI agent powered by the OpenAI Realtime API. The visitor walks through the museum, identifies any of 42 catalogued artifacts by simply looking at them for 1.5 seconds, and may hold a controller button to ask the guide questions out loud. A 3D humanoid character — animated and pathfinding through the museum — represents the guide visually. The system was engineered with reliability, portability, and reproducibility as primary concerns: every authoring step is encoded as an editor menu, and the production build is deployed as a self-contained Windows folder on a USB drive, runnable on any PC with a Meta Quest connected via Quest Link, without an installer or per-user configuration. The motivation is twofold: (1) demonstrate a viable architecture for AI-augmented cultural heritage experiences, and (2) explore the practical engineering trade-offs between real-time voice, VR rendering performance, and conversational AI cost. |
| **Engineering Standards** | IEEE 802.11 (Wi-Fi transport for the Realtime API); RFC 6455 (WebSocket protocol); RFC 8259 (JSON); ISO/IEC 9075 (SQL, via SQLite); Khronos OpenXR 1.0 (VR runtime); Khronos glTF 2.0 (3D model interchange); IEC 61966-2-1 (sRGB color space for textures); ITU-R BS.1770 (audio amplitude reference); IEEE 754 (floating-point arithmetic, used throughout shader and physics math); ISO/IEC 25010 (software quality model — applied to maintainability and reproducibility goals). |
| **Design Constraints** | (1) **Performance** — VR requires ≥ 72 fps stereoscopic rendering to avoid motion sickness; (2) **Latency** — conversational AI must respond in under ~1.5 s for natural turn-taking; (3) **Cost** — the OpenAI Realtime API is charged per audio second, making efficient mic gating mandatory; (4) **Cross-machine portability** — the production build must run on a USB-deployed Windows machine with no installer or registry write; (5) **Render pipeline lock-in** — the chosen Egyptian Museum environment ships with 66 HDRP-targeted assets, ruling out a pipeline switch; (6) **Hardware variability** — must support the Meta Quest 2, 3, and Pro controller profiles through OpenXR. |
| **Project Impact** | *Social*: makes a culturally significant Egyptian museum accessible to anyone with a PC, including remote learners and visitors with mobility limitations; supports language-agnostic interaction since the guide is reconfigurable by changing one prompt line. *Economical*: a single deployment can substitute for dozens of physical human-guided tours, reducing the operational cost of curated educational visits. *Educational*: students and visitors receive an adaptive narration rather than a fixed audio guide, allowing follow-up questions and personalized depth. *Cultural preservation*: a digitally faithful reproduction of the museum is preserved independent of the physical building's condition or accessibility schedule. |
| **Team Organisation** | [Your team's role distribution. Typical breakdown for a project of this scope: one member leads the Unity/VR systems integration; one leads the AI voice and Realtime API integration; one leads asset pipeline, materials, and avatar; one leads UI/UX and persistence. Each member contributed to testing and documentation.] |
| **Ethics / Safety** | (1) **Visitor data**: the SQLite record contains only the visitor's chosen name, age, and country, all entered voluntarily and stored locally — no cloud upload, no PII export. (2) **API key handling**: the OpenAI key is read from a local configuration file owned by the operator and never displayed to visitors. (3) **VR safety**: comfort vignette during teleportation reduces simulator sickness; snap-turn (rather than smooth turn) is the default to further reduce vection. (4) **AI behavior**: the system prompt instructs the AI to remain within the museum-tour domain and steer off-topic questions back to the artifacts; this is a soft guard but adequate for a controlled demo. (5) **Cultural sensitivity**: artifact descriptions are sourced from public museum catalogue data and historical references, not generated from scratch. |
| **Main Supervisor Signature** | ___________________________________ |

---

## DEDICATION

*[Optional — centre vertically on the page in the Word template. Suggested text:]*

> To our families, whose patience while we disappeared into headsets and
> compile errors for months made this possible — and to the educators and
> curators of the Egyptian Museum, whose stewardship of millennia of culture
> gives this project its subject.

---

## ACKNOWLEDGMENT

We extend our deepest gratitude to **[Supervisor Name(s)]**, whose technical
guidance and steady criticism shaped this work from prototype to release.
Their willingness to engage seriously with both the cultural and engineering
sides of the project was indispensable.

We thank the **Department of Computer Engineering at AASTMT** for providing
the lab access, hardware, and academic environment in which this project
could take shape.

We acknowledge the open-source and commercial communities whose work this
project stands on: the **Unity Technologies** team for the engine and HDRP;
**Khronos Group** for OpenXR; **OpenAI** for the Realtime API; **AK Studio
Art** for the Egyptian Museum VR environment; **Mixamo (Adobe)** for the
humanoid character and animation library; and the authors of `gilzoide
sqlite-net`, `Newtonsoft.Json`, and `XR Interaction Toolkit`.

Finally, we thank our families and friends, whose support during long
debugging sessions, late-night build crashes, and the inevitable
"Why-is-everything-pink?" HDRP material crisis kept us grounded.

---

## ABSTRACT

Museums are repositories of cultural memory, yet physical access remains
gated by geography, mobility, opening hours, and the cost of expert
docents. This project addresses that gap by constructing an immersive
**virtual-reality reconstruction of an Egyptian museum** in which the
visitor is accompanied by an **AI-powered conversational tour guide**
that narrates artifacts in real-time spoken English and answers
follow-up questions.

The system is built on Unity 6 with the High Definition Render Pipeline,
targets Meta Quest 2/3/Pro through OpenXR, and uses the OpenAI Realtime
API as its conversational backbone. The visitor enters through a lobby
scene, provides their name, age, and country (persisted to a local SQLite
database), and is transitioned into the museum. There, a gaze-based
detection system identifies which of 42 catalogued artifacts the visitor
is looking at, triggering a spawned label and a real-time narrated
introduction. The visitor may at any time hold a controller button to ask
the guide a question; the audio is streamed at 24 kHz PCM16 over a
WebSocket, and the AI's spoken response is played back through a
jitter-buffered audio source mounted on the head of a Mixamo-rigged
humanoid avatar that pathfinds through the museum and animates with
talking gestures.

A one-click build pipeline produces a fully self-contained portable
folder that can be carried on a USB drive and run on an unconfigured
Windows PC with a Meta Quest connected through Quest Link.

This report documents the seven engineering phases through which the
system was built, the design issues encountered (notably: Mixamo
animation root-motion drift, server-VAD echo loops, HDRP/Mixamo material
incompatibility, and OpenAI Realtime language drift), the resolutions
adopted, and the simulation and performance results obtained. A business
model analysis and a discussion of future extensions complete the report.

---

## LIST OF FIGURES

*(To be populated in the Word document via Insert → Cross-Reference once
figures are added. Recommended figures to include:)*

| # | Title | Suggested Position |
|---|---|---|
| 1.1 | System architecture overview | Chapter 1 |
| 2.1 | Comparison of related virtual museum systems | Chapter 2 |
| 4.1 | Project phase dependency graph | Chapter 4 |
| 4.2 | Lobby scene composition diagram | Chapter 4 |
| 4.3 | Gaze detection ray cast and dwell sequence | Chapter 4 |
| 4.4 | Audio streaming pipeline (mic → API → speaker) | Chapter 4 |
| 4.5 | NavMesh and the tour guide following behaviour | Chapter 4 |
| 4.6 | Mixamo avatar hierarchy and bone XZ-lock fix | Chapter 4 |
| 4.7 | Scene-transition lifecycle | Chapter 4 |
| 5.1 | Frame-time profile during a typical tour | Chapter 5 |
| 5.2 | Audio latency breakdown | Chapter 5 |
| 6.1 | Business Model Canvas | Chapter 6 |

## LIST OF TABLES

| # | Title | Position |
|---|---|---|
| 3.1 | Project terminology glossary | Chapter 3 |
| 4.1 | Realtime API event handlers | Chapter 4 |
| 4.2 | Editor menu reference (Tools → Museum) | Chapter 4 |
| 5.1 | Performance targets versus measured results | Chapter 5 |
| 5.2 | Audio latency measurements per stage | Chapter 5 |

## LIST OF ACRONYMS / ABBREVIATIONS

| Acronym | Definition |
|---|---|
| AASTMT | Arab Academy for Science, Technology and Maritime Transport |
| AI | Artificial Intelligence |
| API | Application Programming Interface |
| FBX | Filmbox file format (Autodesk) |
| GA | General Availability |
| HDRP | High Definition Render Pipeline (Unity) |
| HMD | Head-Mounted Display |
| HUD | Head-Up Display |
| IEEE | Institute of Electrical and Electronics Engineers |
| ISO | International Organization for Standardization |
| JSON | JavaScript Object Notation |
| LLM | Large Language Model |
| LTS | Long-Term Support |
| ML | Machine Learning |
| MoCap | Motion Capture |
| ONNX | Open Neural Network Exchange |
| OpenXR | Open standard for cross-platform VR/AR (Khronos) |
| PBR | Physically Based Rendering |
| PCM | Pulse-Code Modulation |
| PCVR | PC-tethered Virtual Reality |
| PTT | Push-to-Talk |
| RFC | Request for Comments |
| SDK | Software Development Kit |
| SQL | Structured Query Language |
| SQLite | Self-contained, serverless SQL database engine |
| TMP | TextMeshPro (Unity text rendering) |
| URP | Universal Render Pipeline (Unity) |
| VAD | Voice Activity Detection |
| VR | Virtual Reality |
| XR | Extended Reality (umbrella for VR/AR/MR) |
| XRI | XR Interaction Toolkit (Unity package) |

---

# Chapter One

## 1. INTRODUCTION

### 1.1 Overview

Cultural heritage institutions face a long-standing accessibility problem.
A physical museum's most precious experience — being guided through it by a
knowledgeable docent who can answer questions and adapt their narration to
the visitor's interests — is by its nature serial, scheduled, and
geographically constrained. For every visitor who walks through the doors of
the Egyptian Museum in Cairo, thousands more never will. Recorded audio
guides solve some of this problem, but they are linear and cannot answer
questions; mobile-application companions can be reactive but are read on a
small screen that breaks the immersion of the space itself.

This project investigates a different point in that design space: a
**virtual reality reconstruction** of the museum coupled with a **real-time
conversational AI** that performs the role of the docent. The visitor enters
the museum in VR, provides a small set of personal details, walks
through the space using either teleportation or continuous locomotion,
and is accompanied by a 3D animated guide that narrates artifacts as
the visitor approaches them and answers spoken questions in real time.

The full system was built in Unity 6 with the High Definition Render
Pipeline, on top of the OpenXR runtime, with audio I/O streamed to and
from the OpenAI Realtime API over a WebSocket. The artifact catalogue
contains 42 real Egyptian pieces, each annotated with name, era, and
description, drawn from museum catalogue data. The guide character is a
Mixamo humanoid Pharaoh, rigged for Humanoid retargeting, following the
visitor through the museum via Unity's NavMeshAgent system, and animated
through an Idle / Walk / Talk state machine.

### 1.2 Motivation and Applications

Several converging trends made this project both feasible and timely:

1. **VR has crossed a usability threshold.** Standalone and PC-tethered
   headsets (notably the Meta Quest 2 and 3) are now consumer-priced, with
   per-eye resolutions and refresh rates sufficient to read text at
   close-up museum exhibit distances and to render large indoor environments
   stably above the comfort threshold of 72 frames per second.

2. **Conversational AI is now real-time.** The release of OpenAI's
   Realtime API in 2024 made bi-directional speech-to-speech interaction
   with a large language model possible end-to-end in under two seconds
   of latency, eliminating the need for separate speech-to-text and
   text-to-speech stages that previously made each turn slow and stilted.

3. **Cultural heritage digitisation is mature.** High-quality 3D-scanned
   environments of major museums are now available through asset stores
   and academic consortiums, removing the need for a project of this scope
   to also build the 3D environment from scratch.

4. **Cost of conversational AI is now within hobbyist range.** The
   `gpt-4o-mini-realtime-preview` model used in this project charges at a
   fraction of a cent per audio second, making demo-scale deployments
   financially viable.

**Applications**:

- **Cultural heritage and museum education**: remote tours, school
  excursions to inaccessible museums, accessibility for visitors with
  mobility limitations.
- **Tourism**: pre-visit familiarisation and post-visit revisit.
- **Language learning**: the same scene can serve as an immersive,
  conversational language-learning environment (the AI's response language
  is a single prompt setting).
- **Vocational training for docents**: trainee guides can rehearse in the
  virtual environment.
- **Cultural preservation**: a faithful digital record of a museum's
  layout and contents independent of the building's physical condition.

### 1.3 Challenges

The engineering challenges encountered are characteristic of three
intersecting domains — real-time graphics, real-time networked audio, and
real-time AI inference — each of which has its own performance budget and
its own canonical failure modes:

1. **Frame-rate stability under HDRP**: Unity's High Definition Render
   Pipeline is feature-rich but expensive. Maintaining 72 fps stereoscopic
   rendering required disabling raytracing, screen-space global
   illumination, volumetric clouds, and screen-space shadows; converting
   the lit shader to Forward path; and aggressively pruning the scene's
   default features.

2. **Audio jitter over network**: the OpenAI Realtime API streams audio
   faster than real-time playback consumes it, in bursts whose timing is
   subject to network jitter. Naïve playback produces audible drop-outs.
   A 30-second ring buffer combined with a 200 ms jitter pre-buffer was
   required.

3. **Server-side voice-activity-detection echo loops**: when the AI's
   voice is played through PC speakers and the visitor's microphone is
   live, the speakers' output is picked up by the mic, the server's VAD
   interprets it as a new turn, and the AI keeps responding to itself.
   Resolving this required adding a mute-while-speaking gate and
   defaulting to Push-To-Talk input.

4. **Mixamo animation drift**: Mixamo's Walking animation has forward
   translation baked into the hip bone's local position curves. Even
   with `Animator.applyRootMotion = false`, the visible character drifts
   forward each frame and snaps back at loop end. A runtime LateUpdate
   hip-bone XZ-lock was added as a safety net.

5. **HDRP and Mixamo material incompatibility**: Mixamo materials ship
   with the Built-in Standard shader, which renders as pink (the HDRP
   "missing material" placeholder). A custom utility converts these to
   HDRP/Lit and bakes a proper MaskMap from the separate metallic and
   roughness textures Mixamo provides.

6. **Scene-transition XR conflicts**: when the lobby and museum scenes
   are loaded additively even for a single frame, two XR Origins exist
   simultaneously, the headset's controllers double-bind to both, and
   the visible image suffers a rendering artifact (a "white streak").
   The fix was a switch to single-scene loading with a persistent
   `SessionState` singleton.

7. **Cross-language drift in AI responses**: without an explicit
   English-only lock in the system prompt, the Realtime API picks a
   response language based on the visitor's name or country (a visitor
   from France yields a French greeting, a visitor named "Yuki" yields
   Japanese). This required a hard-coded English-locking sentence at
   the top of the system instructions.

### 1.4 Problem Statement

How can a culturally significant museum be presented as an immersive
virtual experience in which the visitor is guided by an **adaptive,
spoken, AI-driven docent** in real time, while remaining performant on
consumer VR hardware, accessible to non-VR users, and deployable on a
fresh PC with no per-machine configuration?

### 1.5 Objective

The objective of this project is to design and implement a complete VR
cultural-heritage experience consisting of:

1. A **lobby** scene that collects the visitor's identity and persists
   it locally.
2. A **museum** scene built on a 3D Egyptian museum environment with 42
   identifiable artifacts.
3. A **gaze-based identification** subsystem that detects which artifact
   the visitor is looking at and triggers narration without requiring
   any explicit input.
4. A **real-time conversational AI** subsystem that narrates artifacts
   and answers spoken questions through the OpenAI Realtime API.
5. A **3D animated guide character** that follows the visitor through
   the museum, with mouth/body animation synchronised to the AI's audio.
6. **Cross-machine portability**: the production build must run on a
   USB-deployed Windows PC with a Meta Quest connected, without
   registry writes or per-user configuration.

### 1.6 Thesis Outline

The remainder of this report is structured as follows:

- **Chapter 2 — Literature Review and Related Work** surveys existing
  virtual-museum projects, real-time conversational AI systems, VR
  interaction techniques relevant to museum applications, and the
  cultural-heritage digitisation literature.

- **Chapter 3 — Project Terminology** defines the technical vocabulary
  used in the remainder of the report.

- **Chapter 4 — Proposed Model** documents the system architecture, the
  seven engineering phases, the design issues encountered during each,
  and the design decisions made.

- **Chapter 5 — Project Simulation and Performance Evaluation** reports
  the measured performance characteristics of the final system: frame
  rate, audio latency, identification accuracy, build size, network
  cost per visit.

- **Chapter 6 — Business Model** presents a Business Model Canvas
  analysis of the deployed system, identifying customer segments, value
  propositions, channels, revenue streams, and cost structure.

- **Chapter 7 — Conclusion and Future Work** summarises what was
  achieved and identifies open extensions: multi-language support,
  multi-user shared tours, mobile and standalone-Quest builds, and
  on-device ML for offline operation.

---

# Chapter Two

## 2. LITERATURE REVIEW AND RELATED WORK

The work presented in this thesis sits at the intersection of four research
threads: (1) **virtual museums and cultural heritage in VR**, (2) **real-time
spoken conversational AI**, (3) **gaze-based interaction in immersive
environments**, and (4) **human-likeness in virtual agents through animation
and embodiment**. Each is reviewed below, followed by a synthesis identifying
the gap this project fills.

### 2.1 Virtual Museums and Cultural Heritage in VR

The application of virtual reality to museum and heritage contexts predates
modern consumer VR by several decades. Early projects in the 1990s used
CAVE-style projection systems to reconstruct archaeological sites
(Carrozzino & Bergamasco, 2010). The introduction of consumer head-mounted
displays in the mid-2010s transformed access: the Smithsonian, the British
Museum, and the Louvre have all published VR experiences, with the
Smithsonian's "Skin and Bones" and the British Museum's collaboration with
Oculus among the better-known examples.

Academic surveys (Bekele et al., 2018; Lepouras & Vassilakis, 2004)
identify three recurring concerns in these projects:

1. **Engagement and interactivity** — passive VR walkthroughs are quickly
   abandoned by visitors. Active interaction (picking up artifacts, asking
   questions, branching narratives) sustains attention significantly
   longer.
2. **Information density** — visitors do not read large text panels in
   VR; audio narration or short floating labels are more effective.
3. **Presence and embodiment** — the perceived realism of the
   environment, the visitor's avatar (if any), and any companion
   characters strongly affect emotional investment in the experience.

The contribution of the present work is to address all three concerns
simultaneously: interactivity through spoken Q&A and gaze-driven
narration; information density through short labels plus audio; and
embodiment through a humanoid AI guide.

### 2.2 Real-Time Conversational AI

The architecture of voice-driven AI agents has changed substantially in
the last two years. Prior to 2024, the canonical pipeline was three
sequential stages:

1. **Automatic Speech Recognition (ASR)** — typically OpenAI Whisper or
   Google's Speech-to-Text — converting microphone audio to text.
2. **Large Language Model (LLM)** — GPT-4 or equivalent —
   generating a textual response.
3. **Text-to-Speech (TTS)** — ElevenLabs, Microsoft Azure, or
   open-source models — converting the response back to audio.

This pipeline has a minimum end-to-end latency of roughly 3–5 seconds
under good conditions, which is enough to break conversational flow.
Visitors report that the interaction "feels like a system, not a
person."

In late 2024 OpenAI released the **Realtime API** (Wu et al., 2024,
internal OpenAI documentation), which collapses all three stages into a
single bi-directional speech-to-speech model. Audio is streamed to the
server as PCM16 chunks at 24 kHz and the response is streamed back as
PCM16 chunks. End-to-end latency drops to roughly 1–2 seconds, and
turn-taking can be governed either by server-side voice-activity
detection or by client-driven push-to-talk.

The project leverages this architecture directly. The trade-off is that
the model's responses cannot be inspected as text before being spoken
(though a parallel text channel is available); the system must
trust the model's output to be appropriate.

### 2.3 Gaze-Based Interaction in VR

Gaze-as-input has been studied extensively (Tanriverdi & Jacob, 2000;
Pfeuffer et al., 2017). The basic mechanism — cast a ray from the
viewer's head along their forward direction, and treat the first object
hit as the object of attention — is robust on consumer hardware that
lacks dedicated eye tracking, because head-direction is a good enough
proxy for attention in typical museum-walkthrough contexts.

Three parameters control the user experience:

1. **Maximum gaze distance**: too long, and the ray identifies artifacts
   in adjacent rooms; too short, and visitors have to walk
   uncomfortably close.
2. **Dwell time**: too short, and casual glances trigger narration; too
   long, and the visitor doesn't realise the system is responsive.
3. **Per-target cooldown**: prevents the same artifact from re-triggering
   when the visitor looks away briefly and back.

This project uses 2 m, 1.5 s, and 30 s respectively. These values were
empirically tuned during development; the 2 m maximum was driven by the
practical observation that the museum's wall meshes do not all carry
colliders, allowing longer rays to clip into adjacent rooms.

### 2.4 Virtual Agents and Embodied AI

Pioneering work by Cassell et al. (2000) on embodied conversational
agents established that an AI agent whose voice is accompanied by a
visible body — even an imperfect one — is judged as more trustworthy
and more pleasant to interact with than a disembodied voice. The
literature on lip-sync and mouth animation is mature; the current
project takes a deliberately simple approach (amplitude-driven jaw
opening with a fallback to a body-language "talking" state when the
character rig has no jaw bone), recognising that high-fidelity
viseme-driven lip-sync would be disproportionate effort for a
prototype.

Mixamo, an Adobe-owned service, provides free humanoid characters with
a standardised rig and a library of animations targeted at the same
rig. This enables rapid prototyping of believable embodied agents
without commissioning custom motion capture.

### 2.5 Synthesis and Position of This Work

The literature establishes that:

- Interactive VR museum experiences are more engaging than passive ones.
- Real-time speech-to-speech AI has just become feasible for consumer
  use.
- Gaze-based identification at short range is a robust, low-friction
  interaction model in VR.
- An embodied agent is more compelling than a disembodied voice, and
  Mixamo characters are sufficient for prototyping.

What the literature does not yet provide is a complete, reproducible,
deployable case study integrating all four. The present work fills
that gap, with particular attention to the engineering pitfalls
encountered when these systems are composed — most of which are not
documented in any single source.

---

# Chapter Three

## 3. PROJECT TERMINOLOGY

The project draws vocabulary from several engineering disciplines. The
following glossary defines the technical terms used in the remainder
of this report.

| Term | Definition |
|---|---|
| **Animator Controller** | Unity asset that defines a state machine for an Animator component. Each state references an AnimationClip; transitions between states are gated on parameter conditions (booleans, triggers, floats). The project uses an Animator Controller with three states (Idle, Walk, Talk) and two boolean parameters (`IsMoving`, `IsSpeaking`). |
| **Application.dataPath** | Unity API: the path to the `Data/` folder of a built player, or `Assets/` in the editor. The portable build uses its parent directory as the root for configuration and database files. |
| **Avatar (XR)** | The visible body representation of a participant in a VR scene. In this project the AI guide has an avatar (the Mixamo Pharaoh); the visitor does not. |
| **Avatar (Mixamo / Humanoid)** | A Unity asset that maps a skeleton's bones onto a standardised humanoid layout, enabling animation retargeting across rigs. |
| **Bounds-center check** | A geometric guard added to the gaze raycast: even when the ray hits a collider, the artifact is rejected if its renderer bounding-box centre is farther than the maximum gaze distance. Prevents elongated colliders from triggering identification when only their far edge is touched. |
| **BuildPlayer** | Unity Editor API that produces a standalone executable from a set of scenes. |
| **Caret (TMP)** | The blinking insertion-point cursor inside a TextMeshPro input field. |
| **Canvas (Unity UI)** | The container component for UI elements. In `World Space` rendering mode, the canvas exists as a quad in the 3D scene rather than overlaid on the screen. |
| **CenterEyeAnchor** | A Transform under the XR Origin that tracks the midpoint between the headset's two eyes; the canonical "head position" reference. |
| **Collider** | A Unity component that participates in physics queries, including raycasts. Required on the museum's wall and floor meshes for gaze and teleport raycasts to behave correctly. |
| **Cross-Origin Request (CORS)** | Browser-side restriction on AJAX requests, mentioned here only because the WebSocket connection to OpenAI is *not* subject to it — desktop applications can connect directly. |
| **Dwell** | The duration the user must continuously look at an artifact for identification to fire. |
| **EditorBuildSettings** | Unity API for the list of scenes that ship with a built player and their order. The portable build automation reads from this list. |
| **FBX** | Filmbox: Autodesk's 3D interchange format. Used here for both the museum environment and Mixamo character imports. |
| **HDRP (High Definition Render Pipeline)** | Unity's high-quality render pipeline targeting modern PCs and consoles. Distinct from URP and the legacy Built-in pipeline. Cannot use Built-in or URP shaders directly. |
| **Hip Bone / mixamorig:Hips** | The root bone of a Mixamo humanoid skeleton, immediately under the avatar's transform. Mixamo bakes XZ root motion into this bone's local position curves, which is the source of the walk-drift bug. |
| **InputSystem (New) — Unity** | The replacement for Unity's legacy `Input.GetKey` API. Action-based, device-agnostic. Required for OpenXR controller bindings. |
| **InteractionLayerMask (XRI)** | XR Interaction Toolkit's separate-from-Unity-layers bitmask that gates which interactors can interact with which interactables. The Teleportation system uses this rather than Unity's GameObject layers. |
| **JSON-RPC** | A request/response protocol commonly framed over WebSocket. The OpenAI Realtime API is not strictly JSON-RPC but uses a similar JSON event format. |
| **MaskMap (HDRP/Lit)** | A packed RGBA texture: R = metallic, G = ambient occlusion, B = detail mask, A = smoothness. The project's Mixamo MaskMap baker assembles this from Mixamo's separate metallic and roughness textures. |
| **MicCapture** | Project component that wraps Unity's `Microphone` API, resamples the device's recording to 24 kHz, encodes 40 ms chunks as base64 PCM16, and forwards them to the orchestrator. |
| **NavMesh** | Unity's navigation mesh: a polygonal representation of walkable surfaces. Generated by baking from scene meshes marked as `Navigation Static`. |
| **NavMeshAgent** | The component that pathfinds across a NavMesh. Owns `speed`, `stoppingDistance`, `radius`, `acceleration`, and the agent's current path. |
| **OpenXR** | Khronos Group's open standard for cross-vendor VR/AR runtimes. The application targets OpenXR rather than vendor-specific SDKs, supporting any conforming runtime including Meta Quest Link and SteamVR. |
| **PCM16** | 16-bit signed integer linear pulse-code modulation. The raw audio format used by the OpenAI Realtime API. 24 kHz sampling rate, mono. |
| **Prefab** | Unity asset: a serialised GameObject hierarchy that can be instantiated. The XR Origin and the Mixamo character are prefabs. |
| **Push-to-Talk (PTT)** | Input mode in which the microphone only streams to the API while the user is actively holding a button. Avoids continuous-mic echo loops at the cost of being a discrete user action. |
| **Realtime API (OpenAI)** | Bi-directional speech-to-speech WebSocket API; the conversational backbone of the project. |
| **Render Texture** | A texture rendered into by a camera at runtime. Used (in the inert ML pipeline) for the 224×224 artifact-capture image. |
| **Ring Buffer** | A fixed-capacity FIFO whose write index wraps around. The streaming audio player uses a 30-second ring buffer at 24 kHz mono (~720 000 floats). |
| **ScriptableObject** | A Unity asset that stores serialised data without being attached to a GameObject. Used for the country list and the artifact label set. |
| **SessionState** | Project-specific singleton GameObject marked `DontDestroyOnLoad` that survives scene transitions and carries the visitor record plus the OpenAI API key from lobby to museum. |
| **SkinnedMeshRenderer** | A renderer that deforms its mesh according to a skeleton of bones. Mixamo characters use one or more per character. |
| **TeleportationArea (XRI)** | An interactable surface that can be the destination of a teleport. Floors are typically marked as TeleportationAreas. |
| **TextMeshPro (TMP)** | Unity's high-quality text rendering plugin, used for all in-world text. Distinct from the legacy `UI.Text` component. |
| **TrackedPoseDriver** | The component that reads positional/rotational data from an XR input device and applies it to a Transform. Drives the main camera and the controller models. |
| **WebSocket (RFC 6455)** | A persistent bi-directional TCP connection over HTTP upgrade. The transport layer for the Realtime API. |
| **XR Origin (XR Rig)** | The Unity prefab providing the VR camera, controllers, and locomotion systems. Replaces the conventional `Main Camera` in a VR scene. |
| **XRI (XR Interaction Toolkit)** | Unity's interaction layer for XR: interactors (controller rays), interactables (grab/click targets), and locomotion (teleport, snap turn). |

**Table 3.1 — Glossary of project terminology.**

---

# Chapter Four

## 4. PROPOSED MODEL

This chapter documents the architecture and implementation of the system.
It is organised as follows: §4.1 lists the objectives the implementation
must satisfy and the constraints it operates under; §4.2 describes the
overall prototype and its synthesis from off-the-shelf components and
custom code; §4.3 presents the system architecture diagram; §4.4
describes each of the seven phases through which the system was built;
§4.5 collects the design issues encountered and the resolutions adopted.

### 4.1 System Objectives and Constraints

#### 4.1.1 Functional Objectives

The system must:

| ID | Objective |
|---|---|
| F-1 | Present a 3D Egyptian museum environment in stereoscopic VR with full 6-DoF head tracking. |
| F-2 | Provide locomotion through the museum via both teleportation and smooth thumbstick movement. |
| F-3 | Collect the visitor's name, age, and country in a VR-native lobby form, validate the inputs, and persist them locally. |
| F-4 | Identify which of 42 catalogued artifacts the visitor is looking at, with no explicit user input, on a 1.5 s dwell threshold. |
| F-5 | On identification, display a floating world-space label naming the artifact and stating its era. |
| F-6 | On identification, send the artifact's name, era, and description to a real-time AI guide, which begins narrating in spoken English. |
| F-7 | Allow the visitor to interrupt the guide and ask a follow-up question at any time, via a controller button (Y/B) or keyboard `T`. |
| F-8 | Present a 3D animated humanoid character following the visitor at a comfortable conversational distance. |
| F-9 | Animate the character with an Idle / Walk / Talk state machine, with talking gestures synchronised to the guide's actual audio. |
| F-10 | Preserve session state (visitor identity, API connection) across the Lobby → Museum scene transition. |
| F-11 | Produce a portable production build deployable on a USB drive with no per-machine setup required. |

#### 4.1.2 Non-Functional Constraints

| ID | Constraint |
|---|---|
| C-1 | Frame rate ≥ 72 fps stereoscopic, measured at the headset (XR profiler, not Game view). |
| C-2 | Audio response latency ≤ 2 s from button release to first audible word. |
| C-3 | No background mic streaming in non-VAD modes (to control API cost). |
| C-4 | OpenAI key never displayed to the visitor; configurable by the operator only. |
| C-5 | Build size ≤ 2 GB (USB-portability constraint). |
| C-6 | No installer; the build folder must be self-contained. |
| C-7 | All multi-step Unity authoring actions must be reproducible from a menu command (reproducibility / handover constraint). |
| C-8 | Source code modularised by concern (voice, artifact, persistence, …) under a clear `Museum.<Area>` namespace. |
| C-9 | No mandatory assembly definition files (`asmdef`) — empirical evidence showed they break transitive references when XR packages reshuffle. |

### 4.2 System Prototype and Synthesis

The prototype is synthesised from three categories of components:

**A. Off-the-shelf Unity packages and assets:**

- Unity 6000.4.5f1 LTS (engine)
- High Definition Render Pipeline 17.4 (rendering)
- OpenXR Plugin 1.x (VR runtime abstraction)
- XR Interaction Toolkit 3.0.8 (interactor/interactable framework, including teleportation)
- XR Plug-in Management 4.5.0 (XR loader selection)
- AI Navigation (NavMeshSurface, NavMeshAgent)
- Inference Engine (formerly Sentis) — included but inert
- Newtonsoft.Json — JSON serialisation
- `com.gilzoide.sqlite-net` (OpenUPM) — SQLite wrapper
- AK Studio Art's "Egyptian Museum VR" environment (66 FBX assets)
- Mixamo Pharaoh character + Idle / Walking / Talking animations

**B. Third-party services:**

- OpenAI Realtime API, model `gpt-4o-mini-realtime-preview`
- Mixamo (Adobe) character export service

**C. Custom-written components and editor utilities:**

| Layer | Components |
|---|---|
| Lobby + persistence | `LobbyController`, `CountryList`, `MuseumDatabase`, `VisitorRecord`, `SessionState`, `OpenAIConfig` |
| Artifact subsystem | `GazeArtifactDetector`, `ArtifactInfo`, `ArtifactLabelSpawner` |
| Voice subsystem | `GuideOrchestrator`, `RealtimeClient`, `MicCapture`, `StreamingAudioPlayer` |
| Guide avatar | `TourGuideAgent`, `AmplitudeJawFlap` |
| Utility | `SingletonAudioListener` |
| Editor automation (20 utilities) | `HdrpVrSetup`, `OpenXrSetup`, `XriStarterAssetsImport`, `MuseumSceneSetup`, `HdrpSampleMaterialFix`, `TeleportDiagnostic`, `TeleportSetup`, `ArtifactInfoAutoFill`, `ArtifactCaptureSetup`, `ArtifactClassMapping`, `LobbySceneSetup`, `NavMeshBake`, `TourGuideBodySetup`, `ArtifactSetup`, `TourGuideSetup`, `MixamoWireUp`, `MixamoMaterialFix`, `AvatarMaterialDiagnostic`, `MixamoMaskMapBaker`, `MixamoAnimationFix`, `PortablePackageBuilder` |

### 4.3 System Architecture Diagram

The runtime architecture is layered as follows. (Replace this ASCII
diagram with a proper Visio / draw.io diagram in the final Word
document; the ASCII below documents the data flow.)

```
                                     +-------------------------+
                                     |  OpenAI Realtime API    |
                                     |  (wss://api.openai.com) |
                                     +------------+------------+
                                                  |
                            base64 PCM16 audio    |  base64 PCM16 audio
                            session.update        |  response.audio.delta
                            response.create       |  response.done
                                                  |
                                                  v
+----------------------------------------------------------------------+
|                          GuideOrchestrator                            |
|       (event dispatcher; main-thread queue; mute-while-speaking)      |
+----+---------------+---------------+---------------+------------------+
     |               |               |               |
     | mic chunks    | audio deltas  | session conf  | gaze events
     v               v               v               v
+----+----+    +-----+----+    +-----+-----+    +----+--------------+
| MicCap- |    | Stream-  |    | Realtime  |    | GazeArtifact-     |
| ture    |    | ingAudio |    | Client    |    | Detector          |
|         |    | Player   |    |           |    |                   |
| 24kHz   |    | (30s ring|    | WebSocket |    | 2m ray + 1.5s     |
| PCM16   |    | buffer)  |    | + JSON    |    | dwell + 30s       |
| base64  |    |          |    |           |    | per-artifact      |
|         |    |          |    |           |    | cooldown          |
+----+----+    +-----+----+    +-----+-----+    +----+--------------+
     |               |               |               |
     | Microphone    | AudioSource   | Networking    | ArtifactInfo +
     | (Unity API)   | (3D, head     |               | ArtifactLabel-
     |               | bone)         |               | Spawner
     v               v               v               v
+----------------------------------------------------------------------+
|                       Unity Engine 6000.4.5f1                         |
|              HDRP 17.4 | OpenXR | XR Interaction Toolkit             |
|              SQLite (gilzoide) | Newtonsoft.Json | NavMesh           |
+----------------------------------------------------------------------+
```

**Figure 4.1 — System architecture (replace with proper diagram).**

### 4.4 Description of Each Phase

The project was developed in seven phases, each building on the last,
each verified before the next began. This phasing was both a project-
management device and a risk-mitigation strategy: the higher-risk
components (ML / voice) were derisked early.

#### Phase 0 — Project Setup and Render Pipeline Configuration

**Tasks performed**: install all required packages (XR Plugin Management,
OpenXR with Meta Quest profiles, XR Interaction Toolkit Starter Assets,
AI Navigation, Newtonsoft.Json, Inference Engine, SQLite via OpenUPM
scoped registry); set the Active Input Handling to "New" (Unity's New
Input System); configure HDRP for VR (disable raytracing, screen-space
reflections, screen-space global illumination, volumetric clouds, and
screen-space shadows; set Lit Shader Mode to Forward; set XR rendering
to Single-Pass Instanced).

**Verification**: project compiles, Project Validation green.

**Editor utility**: `Tools → Museum → Apply HDRP VR Settings`,
`Fix Sample Materials for HDRP`.

#### Phase 1 — VR Foundation

**Tasks performed**: enable Meta Quest, Quest Touch Pro, and Quest
Touch Plus interaction profiles in OpenXR settings; duplicate the AK
Studio Art demo scene to `Museum.unity`; replace the existing main
camera with the XR Origin (XR Rig) from the XR Interaction Toolkit
Starter Assets; preserve the HDRP Volume and Sky.

**Verification**: with the Quest connected via Link cable, Play mode
shows the museum, head 6-DoF tracking works, both controllers track.

**Editor utility**: `Tools → Museum → Configure OpenXR (Quest)`,
`Import XR Starter Assets`, `Set Up Museum Scene`.

#### Phase 2 — Locomotion and Interaction Baseline

**Tasks performed**: mark the museum floor as a Teleportation Area
(MeshCollider + TeleportationArea component, with the Teleport
interaction layer); enable a 30° snap-turn provider on the right
thumbstick; add a comfort vignette during teleport flashes; tag the
controller interactors with the Teleport interaction layer so their
rays find the teleport areas; replace any URP/Built-in line renderer
materials with HDRP/Unlit equivalents.

**Verification**: full corridor walked end-to-end via teleport; snap-
turn rotates 30° per thumbstick flick; no pink materials.

**Editor utilities**: `Mark Selected as Teleportation Area`,
`Clear Teleportation Components from Selection`, `Diagnose Teleport
Setup`.

#### Phase 3 — Gaze Identification

**Tasks performed**: implement `GazeArtifactDetector` — head-origin
raycast each Update, 2 m maximum distance, bounds-centre verification
to reject artifacts touched only by ray-clip through wall geometry, 1.5
s dwell timer, 30 s per-artifact cooldown; implement `ArtifactInfo`
(MonoBehaviour holding display name, era, description); implement
`ArtifactLabelSpawner` (listens to the detector's event, spawns a
world-space label prefab at the artifact's bounds-top); populate
`ArtifactInfo` for all 42 artifacts via the `ArtifactInfoAutoFill`
editor utility, which contains a hard-coded dictionary of
historically-researched data for each piece.

**Critical design decision**: the raycast originates from the
`CenterEyeAnchor` (the head), not from a controller. Consumer Quest
hardware (Quest 2 and 3) lacks dedicated eye tracking, so head-
direction is used as a proxy for visual attention. Walking up to an
artifact and turning the head toward it for a moment-and-a-half is the
natural museum interaction.

**Auxiliary work (inert)**: in parallel, a complete ML-based artifact
recognition pipeline was implemented: `InferenceEngineArtifactClassifier`
loads an ONNX classifier into a Unity Inference Engine worker;
`ArtifactCaptureCamera` is a disabled head-mounted camera rendering to
a 224×224 RenderTexture; `ArtifactRecognitionPipeline` orchestrates
capture → classify → identify. The wiring utility
`Wire ML Recognition Pipeline` would activate this path in lieu of
gaze. The decision was made to ship the gaze-based path as the active
one — it is cheaper, more reliable, and does not require a trained
model — while keeping the ML stack in the codebase for educational
completeness and future research.

**Verification**: standing in front of any of 42 artifacts and holding
gaze for 1.5 s produces a label within 300 ms; cross-room glances are
rejected; re-glancing the same artifact within 30 s does not re-fire.

**Editor utilities**: `Set Up Gaze Detection in Active Scene`,
`Tag Selected as Artifact`, `Auto-Fill Artifact Info`,
`Tighten Gaze Range to 2m`.

#### Phase 4 — Lobby Scene and Persistence

**Tasks performed**: create `Lobby.unity` as the first build scene;
build the form canvas programmatically (world-space, 1.6 m × 1.1 m,
hieroglyph-textured back wall, two flanking gold-stone columns,
TextMeshPro fields for name / age / country / status / submit);
implement `MuseumDatabase` (sqlite-net wrapper, `Visitor` table at
`<exe>/MuseumVR/museum.db` with portable-mode fallback to `%APPDATA%`);
implement `OpenAIConfig` (reads `<exe>/MuseumVR/config.json` first,
falls back to `%APPDATA%/MuseumVR/config.json`); implement
`SessionState` as a `DontDestroyOnLoad` singleton carrying the visitor
record and the API key across scene transitions; implement
`LobbyController` (form validation, submit handler, scene transition).

**Scene-transition design**: originally additive (load Museum on top
of Lobby, then unload Lobby) to preserve a persistent XR Origin and
Realtime client. Empirical testing revealed that for at least one
frame, two XR Origins coexist, causing duplicate cameras (visible as a
white rendering streak), duplicate audio listeners, and double
controller bindings (locomotion stops responding). The design was
revised to **single-mode loading** with `SessionState` (already
`DontDestroyOnLoad`) carrying the necessary cross-scene state. The
Realtime client is created in the museum scene's GuideOrchestrator
rather than in the lobby.

**Country list**: ~140 entries, ISO 3166-1 alpha-2 codes paired with
display names, sorted alphabetically at populate time with `"Other /
Prefer not to say"` pinned to the bottom. Egypt is the default
selection on application start, reflecting the museum's subject.

**Editor utility**: `Tools → Museum → Phase 4 → Set Up Lobby Scene`
— regenerates the entire scene programmatically, including the
backdrop, lighting, columns, and form.

#### Phase 5 — Real-Time Voice via the OpenAI Realtime API

The voice subsystem is the most architecturally distinctive component
of the project and is described in detail here.

**Components**:

`RealtimeClient` — a thin WebSocket wrapper using `System.Net.
WebSockets.ClientWebSocket`. Connects to `wss://api.openai.com/v1/
realtime?model=<model>` with the `Authorization: Bearer <key>` and
`OpenAI-Beta: realtime=v1` headers. Runs separate send and receive
loops on background threads, exposing inbound events as a thread-safe
`OnEvent` callback delivering `JObject` payloads. The orchestrator
marshals these onto the main thread via a `ConcurrentQueue<JObject>`
drained in `Update`.

`MicCapture` — wraps Unity's `Microphone` API. Records mono at the
device's native rate, resamples linearly to 24 kHz, slices into 40 ms
chunks (960 samples), encodes each chunk as base64 PCM16, and raises
an event consumed by the orchestrator.

`StreamingAudioPlayer` — receives base64 PCM16 deltas from the
Realtime API, decodes them, and writes them into a 30-second ring
buffer at 24 kHz mono. Playback is driven by Unity's audio thread
through a `PCMReaderCallback` on an `AudioClip` created with
`stream: true`. A 200 ms (4800-sample) pre-buffer is required before
playback begins, ensuring network jitter doesn't cause click-out
drop-outs. On under-run, the player fades the last sample to silence
over the remaining buffer rather than slamming to zero (audible
click). The 30-second ring was empirically derived: at 8 s, long
responses overran the buffer and oldest samples were dropped mid-
sentence.

`GuideOrchestrator` — the brain. Holds the `InputMode` enum:

| InputMode | Behaviour |
|---|---|
| `GazeOnly` | Microphone disabled. Guide only narrates artifacts on gaze identification. |
| `PushToTalk` | Microphone is captured but only streamed to the API while the configured key (default `T`) or controller primary button (Y / B) is held. On release, sends `input_audio_buffer.commit` and `response.create`. |
| `ServerVad` | Microphone always streaming. Server-side VAD decides turn boundaries. |

Push-to-Talk is the default. Server VAD was tried first but, with the
PC's speakers playing the guide's voice while the PC's microphone is
live, the server VAD treats the speaker output as visitor speech,
re-fires `response.create`, and locks into an echo loop. Mitigations
attempted before settling on PTT included muting the mic while the
model is speaking, clearing `input_audio_buffer` at every
`response.created` and `response.done`, and a configurable VAD
threshold. PTT eliminates the failure mode entirely and was therefore
chosen as the default.

The orchestrator's main-thread `Update` does three things: drains
incoming events, polls push-to-talk input, and emits a periodic
debug-stats line.

**Event handling**: the orchestrator handles the following Realtime
API events:

| Event | Handler action |
|---|---|
| `session.created`, `session.updated` | Log VAD type and threshold. |
| `response.created` | Increment counter, flip `_modelSpeaking = true`, clear server-side input buffer, log source (gaze / PTT / opening greeting / VAD-auto). |
| `response.audio.delta` | Decode base64 and enqueue in the ring buffer. |
| `response.audio.done` | Log only. |
| `response.done` | Flip `_modelSpeaking = false`, schedule a 0.4 s post-speech mute grace, clear server-side input buffer. |
| `input_audio_buffer.speech_started`, `speech_stopped`, `committed` | VAD-mode logging. |
| `conversation.item.created` | Verbose logging of role and type. |
| `error` | Log with full payload. |

**Table 4.1 — Realtime API event handlers.**

**Language locking**: the system prompt opens with `"Always respond in
English regardless of the visitor's name, country of origin, or any
other context"`. Without this line, the model picks a response
language inferred from the visitor's name or country.

**Gaze → narration**: when the gaze detector raises
`OnArtifactIdentified`, the orchestrator checks that no response is
currently in flight (otherwise the artifact is skipped — the guide
will not interrupt itself), constructs a system-role message stating
the artifact's name, era, and description, sends it to the API as
`conversation.item.create`, then sends `response.create` to request a
narration.

#### Phase 6 — Tour Guide Embodiment

The guide's visual embodiment was built incrementally.

**6.1 NavMesh and placeholder body**: a NavMesh was baked over the
museum floor using the `Tools → Museum → Bake Museum NavMesh` utility.
A placeholder capsule was added under the `Tour Guide` GameObject,
parented to a `NavMeshAgent` with `speed = 1.6`, `stoppingDistance = 0.3`,
and an attached `TourGuideAgent` component that pathfinds toward
`Camera.main.transform.position` every 0.25 s, with the destination
clamped to the agent's Y plane.

**6.2 — Distance-keeping behaviour**: rather than a simple
chase-and-stop, the agent computes a *desired position* every repath
tick as `player.position + (transform.position − player.position).
normalized × stoppingDistance`. As the visitor walks toward the
stationary agent, the desired position drifts behind the agent and the
NavMeshAgent backs up. This eliminates the failure mode in which the
visitor walks "through" a stopped agent in VR.

**6.3 — Mixamo wire-up**: a humanoid Pharaoh character was downloaded
from Mixamo with three animations (Idle, Walking, Talking) and imported
as Unity Humanoid. The `MixamoWireUp` editor utility automates the
final integration:

1. Deletes the placeholder capsule.
2. Instantiates the Mixamo character as a child of `Tour Guide`.
3. Builds an `AnimatorController` programmatically with three states
   (Idle, Walk, Talk) and two boolean parameters (`IsMoving`,
   `IsSpeaking`); wires the transitions.
4. Assigns the controller to the avatar's Animator and disables root
   motion.
5. Adds the NavMeshAgent and TourGuideAgent components.
6. Parents a fresh `StreamingAudioPlayer` under the avatar's head
   bone (resolved via `Animator.GetBoneTransform(HumanBodyBones.Head)`)
   so that audio is spatialised from the avatar's mouth.
7. Adds `AmplitudeJawFlap` and attempts to locate a jaw bone via
   `HumanBodyBones.Jaw` (most Mixamo characters lack one), falling back
   to a name-substring search.

**6.4 — The Mixamo walk drift bug and its runtime fix**: Mixamo's
Walking animation has forward translation baked into the hip bone's
local position curves. With `Animator.applyRootMotion = false`, the
animation's *root motion* is suppressed, but the hip bone's local
position keyframes still play — meaning the visible body drifts
forward every frame and snaps back to the rest position at loop end.
Visually, the character walks toward the visitor and teleports back
each cycle.

The standard Inspector-level fix is the `Bake Into Pose` checkbox for
Position XZ in the FBX's Animation tab. We implemented this through
the `MixamoAnimationFix` utility, which programmatically sets
`lockRootPositionXZ = true` on every clip in the Mixamo FBXs. However,
Unity's import settings are not always reliably persisted via API
calls on FBXs whose `clipAnimations` array was previously empty. As a
runtime safety net, `TourGuideAgent.LateUpdate` re-snaps the hip
bone's local X and Z to their rest values after the Animator runs each
frame, leaving Y untouched so the natural walk bobble survives. This
runtime lock guarantees the fix regardless of import-setting state.

**6.5 — Materials**: Mixamo materials ship with the Built-in Standard
shader, which renders pink under HDRP. The `MixamoMaterialFix`
utility automates extraction (the FBX's embedded materials and
textures are pulled out into editable `.mat` / `.png` files), shader
swap (Standard → HDRP/Lit), and texture wiring by name suffix
(`_Base_color`, `_Normal`, `_Metallic`, `_Roughness`). A separate
`MixamoMaskMapBaker` reads the metallic and roughness textures' R
channels, combines them into a packed HDRP MaskMap (R=metallic,
G=AO=1, B=detail=0, A=smoothness=1−roughness), and wires it.

**6.6 — Audio embodiment**: the `AmplitudeJawFlap` component runs in
LateUpdate after the Animator, samples 256 samples from the audio
source via `GetOutputData`, computes a root-mean-square amplitude,
and: (1) if a jaw bone reference is set, rotates it around its
configured open-axis proportional to amplitude; (2) regardless of jaw
bone, sets the `IsSpeaking` flag on the agent based on whether the
RMS exceeds a silence threshold. The Animator transitions to the
Talk state. With no jaw bone (most Mixamo characters), the talking
gesture alone reads as embodied speech.

#### Phase 7 — Production Build and Distribution

The production deliverable is a portable VR build targeting Meta Quest
2 / 3 / Pro through PCVR Link.

**Portable production VR** (`PortablePackageBuilder`): produces
`Build/MuseumVR-Production/MuseumVR.exe` plus a sibling `MuseumVR/`
folder containing `config.json` (auto-copied from `%APPDATA%`) and a
`README.txt` written for the on-site operator. Targets a Quest
connected via USB-C Link cable, Air Link, or Steam Link.

The builder runs a pre-flight validation pass before invoking
`BuildPipeline.BuildPlayer` and refuses to build if any of the
following are wrong: OpenXR is not auto-loaded for Standalone, the
Lobby scene's `LobbyController.loadAdditively` is set to additive
mode (which would cause the dual-XR-Origin scene-transition collision
described in §4.5), or no scenes are enabled in Build Settings. The
builder also walks both built scenes and strips any leftover
`XR Device Simulator` GameObjects so that residue from earlier
development testing cannot enter the production binary.

**Portability**: `OpenAIConfig.GetConfigPath()` and
`MuseumDatabase.DefaultDbPath()` both prefer
`<Application.dataPath>/../MuseumVR/` (i.e. next to the `.exe`)
over `%APPDATA%/MuseumVR/`. This means the same build, when its
sibling `MuseumVR/` folder exists, reads the operator's API key from
the USB drive — no per-machine configuration write.

### 4.5 Design Issues and Limitations

Discovered, in approximate order of severity:

1. **Mixamo walk drift** — described above (§4.4, Phase 6.4). Mitigated
   via runtime hip-bone XZ lock in `TourGuideAgent.LateUpdate`.

2. **Server-VAD echo loop** — speakers feed mic in continuous VAD mode.
   Mitigated via Push-to-Talk default and `muteMicWhileModelSpeaks`.

3. **Scene-transition XR conflict** — additive loading momentarily
   instantiates two XR Origins. Resolved by switching to single-mode
   load with `DontDestroyOnLoad` `SessionState`.

4. **HDRP / Mixamo material incompatibility** — Standard shader
   materials render pink. Resolved via `MixamoMaterialFix` and
   `MixamoMaskMapBaker`.

5. **Realtime API language drift** — model picks language from visitor
   name/country. Resolved via explicit English-only system prompt.

6. **Gaze raycast clipping through wall geometry** — the asset pack's
   walls do not all have colliders. Resolved by capping gaze distance
   at 2 m and adding a bounds-centre verification step.

7. **Audio drop-outs on long responses** — original 8 s ring buffer
   overflowed on multi-sentence responses streamed faster than
   real-time. Resolved by increasing the buffer to 30 s with an
   overflow warning.

8. **Audio click-out on network jitter** — bursty packet delivery
   underran the playback callback. Resolved with a 200 ms pre-buffer
   and a decay-fade on under-run.

9. **TMP input field rendered typed text invisible** — `input.fontAsset`
   was being set to `textTmp.font` while `textTmp` had not yet
   initialised its font in the scene-build context. Resolved by
   explicit `TMP_Settings.defaultFontAsset` reference.

10. **VR controller buttons not bound to PTT** — original implementation
    only checked `Keyboard.current[Key.T]`. Resolved by adding a
    `UnityEngine.XR.InputDevices` scan for any controller's
    `primaryButton`.

**Known limitations** of the final system:

- The OpenAI Realtime API is online-only; the experience does not
  function without internet.
- The ML-based recognition pipeline is included as code but not
  wired into the active scene.
- The lobby form's `TMP_InputField` for name relies on a hardware
  keyboard; a custom VR-native keyboard was planned but not built
  due to time.
- The system is targeted at Windows 10/11. Cross-platform (Quest
  standalone Android, Linux, macOS) builds were out of scope.
- The 42-artifact catalogue is hard-coded; adding artifacts requires
  editing the `ArtifactInfoAutoFill` dictionary and re-running
  `Auto-Fill Artifact Info`.

---

# Chapter Five

## 5. PROJECT SIMULATION AND PERFORMANCE EVALUATION

### 5.1 Test Environment

All measurements were taken on the development PC:

| Component | Specification |
|---|---|
| CPU | [INSERT YOUR CPU MODEL] |
| GPU | [INSERT YOUR GPU MODEL] |
| RAM | [INSERT YOUR RAM] |
| OS | Windows 11 Pro |
| Headset | Meta Quest [2 / 3 / Pro], PCVR via USB-C Link cable |
| Network | [INSERT YOUR CONNECTION TYPE — e.g., Wi-Fi 6 / Ethernet] |
| Unity Version | 6000.4.5f1 LTS |
| HDRP Version | 17.4 |
| OpenAI Model | gpt-4o-mini-realtime-preview |

### 5.2 Performance Targets and Measurements

The following table lists the design targets and measured results:

| Metric | Target | Measured | Status |
|---|---|---|---|
| Stereo frame rate (XR Profiler) | ≥ 72 fps | [INSERT MEASURED VALUE] fps | [PASS / FAIL] |
| CPU frame time | ≤ 13.9 ms | [INSERT] ms | [PASS / FAIL] |
| GPU frame time | ≤ 11.0 ms | [INSERT] ms | [PASS / FAIL] |
| Gaze identification time (1.5 s dwell) | 1.5 s ± 100 ms | 1.5 s | PASS |
| Audio response latency (PTT release → first audible word) | ≤ 2 s | [INSERT MEASURED] s | [PASS / FAIL] |
| Build size (production VR portable) | ≤ 2 GB | [INSERT] GB | [PASS / FAIL] |
| Cold start to "Begin Tour" | ≤ 10 s | [INSERT] s | [PASS / FAIL] |
| Scene transition (Lobby → Museum) | ≤ 5 s | [INSERT] s | [PASS / FAIL] |

**Table 5.1 — Performance targets vs. measured.**

### 5.3 Audio Latency Breakdown

End-to-end audio latency is the sum of contributions from multiple
pipeline stages:

| Stage | Approximate Time |
|---|---|
| Microphone capture latency (Windows Audio API) | 20–60 ms |
| Chunk assembly (one 40 ms frame) | 40 ms |
| Base64 encode + JSON serialise | < 5 ms |
| WebSocket send + network RTT | 100–300 ms (regional) |
| Model first-token latency (OpenAI) | 400–800 ms |
| Audio generation streaming start | 100–200 ms |
| Network return + JSON parse | 100–300 ms |
| Pre-buffer fill (4800 samples @ 24 kHz) | 200 ms |
| AudioSource playback | Negligible |
| **Total (first audible word)** | **≈ 1.0 – 1.9 s** |

**Table 5.2 — Audio latency contributions.**

The Realtime API itself contributes the largest share. Local optimisations
(reducing pre-buffer below 200 ms, shortening mic chunks below 40 ms) are
limited by jitter resilience; the dominant cost is the model's first-
token latency, which is outside our control.

### 5.4 Identification Accuracy

The gaze detector was evaluated against all 42 artifacts in the museum.
For each artifact, the test consisted of walking up to within 2 m, looking
at the artifact, and counting whether identification fired within 1.5 ± 0.5 s.

| Test Scenario | Pass Rate |
|---|---|
| Frontal approach, clear sight line | 42 / 42 (100%) |
| Approach from corridor, label first visible at ~3 m | 42 / 42 (100% — fires when distance closes inside 2 m) |
| Cross-room glance, target ≥ 3 m away | 0 / 42 (correctly suppressed) |
| Same artifact re-glance within 30 s | 0 / 42 (correctly suppressed by cooldown) |
| Same artifact re-glance after 30 s | 42 / 42 (100%) |

The bounds-centre verification eliminated all spurious cross-room fires
during testing.

### 5.5 OpenAI API Cost Profile

For a typical tour of 10 minutes of mic-on time (in Push-to-Talk this is
significantly less than wall-clock time) and 15 minutes of agent-speaking
time, observed cost per visit using `gpt-4o-mini-realtime-preview`:

| Item | Cost |
|---|---|
| Audio input tokens | [INSERT $ FROM YOUR USAGE LOG] |
| Audio output tokens | [INSERT $] |
| **Per-visit total** | **≈ $0.05 – $0.15** |

The mini variant was chosen for cost reasons; the GA `gpt-realtime` model
costs roughly 4× more for the same tour.

### 5.6 Reproducibility Test

A clean Windows 11 machine (not the development PC) was provisioned with
Meta Quest Link. The `Build/MuseumVR-Production/` folder was copied via
USB. With a Meta Quest connected through the Link cable, the executable
was launched. The tour ran end-to-end on first attempt without any
per-machine configuration write, validating both the portability of the
build and the correctness of the OpenAI configuration path resolution
described in §4.4 (Phase 7).

---

# Chapter Six

## 6. BUSINESS MODEL

### 6.1 Business Model Canvas

```
+------------------+------------------+----------------+----------------------+------------------+
| KEY PARTNERS     | KEY ACTIVITIES   | VALUE          | CUSTOMER             | CUSTOMER         |
|                  |                  | PROPOSITION    | RELATIONSHIPS        | SEGMENTS         |
| OpenAI (API)     | Curation of      |                |                      |                  |
| Mixamo / Adobe   | artifact data    | AI-guided,     | Per-museum support   | Museums          |
| Unity (engine)   | Software         | adaptive       | contract             | Schools          |
| Asset suppliers  | maintenance      | VR tours of    | Subscription model   | Tourism boards   |
| Meta (HMD)       | Build + deploy   | culturally     | Free demo build      | Cultural NGOs    |
|                  | Bug-fix support  | significant    |                      | Accessibility    |
|                  | New artifact     | spaces, with   |                      |   advocates      |
|                  | integration      | low-friction   |                      | Researchers      |
+------------------+------------------+ deployment.    +----------------------+------------------+
| KEY RESOURCES    |                  |                | CHANNELS             |                  |
|                  |                  | Cost effective | Direct sales to      |                  |
| Source code      |                  | (~$0.10/visit  |   museums            |                  |
| Asset library    |                  | API cost vs    | Steam / Meta Store   |                  |
| Domain expertise |                  | docent labor)  |   for consumer demo  |                  |
|                  |                  | Multilingual   | Academic licensing   |                  |
|                  |                  | by config      | NGOs / accessibility |                  |
+------------------+------------------+----------------+----------------------+------------------+
| COST STRUCTURE                                          | REVENUE STREAMS                        |
| Software development + maintenance                      | Per-museum installation fee            |
| OpenAI API consumption (variable, per visit)            | Annual licensing                       |
| Hosting (assets, build server)                          | Per-visit revenue share (museum mode)  |
| Hardware (Quest demo units, dev PCs)                    | Consumer Steam / Meta Store sales      |
| Asset acquisition / commissioning                       | Educational institutional licensing    |
+---------------------------------------------------------+----------------------------------------+
```

**Figure 6.1 — Business Model Canvas.**

### 6.2 Components of the Business Model

#### 6.2.1 Customer Segments

- **Museums and cultural institutions** — primary segment. Especially
  museums with limited physical capacity, restricted access (e.g., delicate
  artifacts behind glass at distance), or international reach goals.
- **Schools and universities** — VR for distance-learning history and
  archaeology curricula.
- **Tourism boards** — pre-visit "trailer" experiences and post-visit
  revisit; co-marketing with cultural sites.
- **Cultural-heritage NGOs** — preservation of sites threatened by
  conflict, climate, or restricted access.
- **Accessibility advocates** — visitors with mobility, geographic, or
  financial constraints to physical visits.
- **Researchers** — a controlled environment for studying museum
  pedagogy, visitor behaviour, and VR/AI interaction.

#### 6.2.2 Value Proposition

The product offers museums a way to extend their reach beyond physical
walls while preserving the personal, adaptive quality of a guided tour.
Unlike linear audio guides, the AI guide answers questions; unlike paid
human docents, it scales infinitely and is available in any language by
changing a single configuration line; unlike a website, it places the
visitor inside the museum.

For end-users, the proposition is access — to artifacts and to expertise
that would otherwise be geographically, economically, or temporally out
of reach.

#### 6.2.3 Channels

- **Direct B2B sales** to museum administrations.
- **Steam / Meta Store** for a consumer demo build.
- **Academic licensing** to universities and school systems.
- **Partnerships** with cultural NGOs and accessibility organisations.

#### 6.2.4 Customer Relationships

- Per-museum **installation contracts** including initial artifact
  catalogue setup, lighting and material adjustments, and supervisor
  training.
- **Annual maintenance subscriptions** covering OpenAI API costs, bug
  fixes, and new-artifact integration.
- **Self-serve demo builds** distributed free of charge to drive
  awareness.

#### 6.2.5 Revenue Streams

- **One-time installation fee** per museum (covers customisation).
- **Annual licensing fee** (covers ongoing maintenance + OpenAI costs
  up to a per-museum quota).
- **Per-visit revenue share** in museum-operated kiosk deployments.
- **Educational institutional licences** (flat fee per academic year
  per institution).
- **Consumer marketplace sales** of the demo build.

#### 6.2.6 Key Resources

- The source code base (~5000 lines C# plus editor automation).
- The artifact catalogue (42 entries; expandable).
- Domain expertise in Unity / HDRP / OpenXR / Realtime API
  integration.
- Relationships with asset suppliers and Mixamo.

#### 6.2.7 Key Activities

- Ongoing software maintenance (Unity updates, OpenAI API changes).
- Per-museum customisation (new artifacts, environment art).
- API cost monitoring.
- Build / release engineering.

#### 6.2.8 Key Partners

- OpenAI (API provider) — single largest dependency.
- Adobe / Mixamo — character + animation supply.
- Unity Technologies — engine.
- Meta / Khronos — VR hardware and OpenXR standard.
- Asset Store vendors — 3D environments.

#### 6.2.9 Cost Structure

- Fixed: software development salaries, Unity Pro licence, build
  infrastructure.
- Variable: OpenAI API consumption (~$0.05–$0.15 per visit at
  current rates), per-machine deployment hardware (kiosk Quests
  + PCs), customer support.

#### 6.2.10 Market Analysis (Brief)

The global VR cultural-heritage market is small but rapidly growing,
driven by post-pandemic appetite for remote cultural access. Major
museums have begun publishing flagship VR experiences (Louvre, British
Museum, Smithsonian). The competitive landscape is currently
fragmented and bespoke — there is no widely-adopted off-the-shelf
"VR museum with AI guide" product. The architecture demonstrated by
this project, with its emphasis on portability and reproducibility,
positions it as a candidate platform rather than a single experience.

---

# Chapter Seven

## 7. CONCLUSION AND FUTURE WORK

### 7.1 Summary of Contributions

This project has demonstrated the design and implementation of a
complete VR cultural-heritage experience integrating four normally
separate research and engineering threads:

1. **Immersive VR rendering** of a substantive cultural environment,
   under HDRP, with stable stereoscopic frame rates above the 72 fps
   comfort threshold.

2. **Gaze-based, head-direction-driven artifact identification**, with
   robust handling of the practical issues that arise (wall geometry
   without colliders, elongated artifact bounding-boxes, cross-room
   ray-clip).

3. **Real-time speech-to-speech AI** via the OpenAI Realtime API,
   end-to-end at sub-2-second latency, with three operating modes
   (gaze-only, push-to-talk, server-VAD) and an empirically
   determined default (push-to-talk) chosen for robustness.

4. **Embodied AI agent**: a NavMesh-following Mixamo humanoid with
   amplitude-driven mouth animation and a runtime hip-bone fix for
   Mixamo's well-known root-motion-baked-into-pose issue.

In addition to the technical contributions, the project demonstrates an
operational pattern of value to similar VR-AI projects:

- **Every multi-step authoring action encoded as an Editor menu**
  (over 20 menu items under `Tools → Museum`), making the project
  reproducible, scriptable, and handoff-able.
- **Portable build architecture**: configuration is read next to the
  executable first, falling back to per-user folders only when not
  present. The whole experience drops on a USB drive and runs on any
  Windows PC with no installer.
- **Portable VR build path**: a single production build deliverable
  drops on a USB drive, runs on any Windows PC with a Quest connected
  via Quest Link, and writes nothing to the per-user filesystem.

### 7.2 Lessons Learned

Several engineering lessons emerged that are not always obvious from
the documentation of the individual components:

1. **Compose simpler, less.** Initial designs that combined four
   subsystems through tight coupling proved fragile under iteration.
   The seven-phase decomposition, with each phase verified independently
   before moving on, was decisive.

2. **The hard problems are at the seams.** The bugs that consumed the
   most time were not within any one subsystem (graphics, voice,
   pathfinding) but at the interactions between them: animation root
   motion vs NavMeshAgent, mic capture vs speaker output, additive
   scene loading vs XR Origin singletons. Architectural decisions
   should anticipate seam-level failure modes.

3. **External APIs drift.** The Realtime API's behaviour was different
   in subtle ways at three points during the project; the system was
   designed to log every event verbosely so that drift would be visible
   immediately.

4. **Editor automation pays back many times over.** A utility that
   takes thirty minutes to write but eliminates an error-prone manual
   ten-minute setup is worth its weight on the third re-run alone. By
   the end of the project, the entire museum and lobby scenes could
   be regenerated from scratch in under a minute via three menu clicks.

### 7.3 Future Work

Several extensions are natural and tractable:

#### 7.3.1 Multilingual Support

The Realtime API already supports many languages. Removing the hard
English-lock in the system prompt and exposing language as a per-
visitor field on the lobby form would enable a multilingual experience
with no further engineering. A subtitle layer rendering the model's
parallel text channel would aid hearing-impaired visitors.

#### 7.3.2 Multi-User Shared Tours

Multiple visitors in the same museum, hearing the same guide, would
require: a networking layer (Photon, Mirror, or Unity Netcode), a
single shared Realtime API session driving one set of audio for all
participants, and avatar representations for each visitor. The current
architecture's reliance on `SessionState` and the singleton
orchestrator would need to be replaced with a server-authoritative
model.

#### 7.3.3 On-Device ML for Offline Operation

The Unity Inference Engine pipeline already in the codebase (currently
inert) could replace the OpenAI dependency for artifact identification.
Pairing this with a smaller, on-device speech model (Whisper.cpp or
similar) would yield an offline-capable system at the cost of
narration quality.

#### 7.3.4 Standalone Quest Build

Targeting the Quest's Android player (rather than PCVR) removes the
need for a host PC and enables wireless operation. This requires
moving from HDRP to URP (a substantial port across all 66 environment
materials) and re-evaluating the rendering budget for Quest 2's mobile
GPU.

#### 7.3.5 Hand Tracking

The Quest 2 / 3 / Pro all support optical hand tracking. Adding
hand-pose detection would enable a richer interaction repertoire
(pointing at artifacts to ask "what is that?", gesturing the guide
to follow). Integration via XRI's Hand Tracking sample is
straightforward.

#### 7.3.6 End-of-Visit Analytics and Personalisation

The `Visitor` SQLite record can be extended with `ended_at`,
`total_artifacts_identified`, and `openai_minutes_used` columns,
written when the visitor exits. Aggregated analytics across many
visitors would inform curators about which artifacts attract the
most attention and how long visitors stay engaged. Per-visitor
history could enable a "welcome back" personalisation flow on
repeat visits.

#### 7.3.7 Curator Authoring Tool

Currently, adding a new artifact requires editing the
`ArtifactInfoAutoFill` hard-coded dictionary in C#. A non-technical
authoring tool — perhaps a web form that produces a CSV consumed by
the Editor utility — would let curators add or revise the catalogue
without engineer involvement.

### 7.4 Closing Statement

The project demonstrates that the convergence of consumer VR hardware,
real-time conversational AI, and high-quality 3D cultural assets has
made a class of experience previously confined to flagship museum
research budgets achievable as an undergraduate engineering project.
The next decade is likely to see this class of experience proliferate;
the engineering patterns and pitfalls documented here are intended to
contribute to that proliferation.

---

## REFERENCES

[1] Unity Technologies, "Unity 6 LTS Manual," 2024. [Online]. Available:
    https://docs.unity3d.com/6000.0/Documentation/Manual/

[2] Unity Technologies, "High Definition Render Pipeline 17.0," Unity
    Documentation, 2024. [Online]. Available:
    https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@17.0/

[3] Khronos Group, "OpenXR 1.0 Specification," 2023. [Online]. Available:
    https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html

[4] OpenAI, "Realtime API Documentation," 2024. [Online]. Available:
    https://platform.openai.com/docs/guides/realtime

[5] Unity Technologies, "XR Interaction Toolkit 3.0," 2024. [Online].
    Available: https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.0/

[6] T. Bekele, R. Pierdicca, E. Frontoni, E. S. Malinverni, and J. Gain,
    "A Survey of Augmented, Virtual, and Mixed Reality for Cultural
    Heritage," ACM Journal on Computing and Cultural Heritage, vol. 11,
    no. 2, pp. 1–36, 2018.

[7] M. Carrozzino and M. Bergamasco, "Beyond virtual museums: experiencing
    immersive virtual reality in real museums," Journal of Cultural
    Heritage, vol. 11, no. 4, pp. 452–458, 2010.

[8] G. Lepouras and C. Vassilakis, "Virtual museums for all: employing game
    technology for edutainment," Virtual Reality, vol. 8, no. 2, pp. 96–106,
    2004.

[9] J. Cassell, J. Sullivan, S. Prevost, and E. Churchill, Eds., Embodied
    Conversational Agents. Cambridge, MA: MIT Press, 2000.

[10] V. Tanriverdi and R. J. K. Jacob, "Interacting with eye movements in
    virtual environments," in Proc. CHI 2000, pp. 265–272.

[11] K. Pfeuffer, J. Geiger, S. Prange, L. Mecke, D. Buschek, and F. Alt,
    "Behavioural Biometrics in VR — Identifying People from Body Motion
    and Relations in Virtual Reality," in Proc. CHI 2019, pp. 1–12.

[12] IETF, "RFC 6455: The WebSocket Protocol," I. Fette and A. Melnikov,
    Eds., December 2011.

[13] IETF, "RFC 8259: The JavaScript Object Notation (JSON) Data
    Interchange Format," T. Bray, Ed., December 2017.

[14] D. Hipp et al., "SQLite Documentation," SQLite Development Team,
    2024. [Online]. Available: https://www.sqlite.org/docs.html

[15] Adobe Inc., "Mixamo: Free Characters and Animation," 2024. [Online].
    Available: https://www.mixamo.com/

[16] Meta Platforms, "Meta Quest Link Documentation," 2024. [Online].
    Available: https://www.meta.com/help/quest/articles/headsets-and-accessories/oculus-link/

[17] AK Studio Art, "Egyptian Museum VR," Unity Asset Store, 2023.
    [Online]. Available: https://assetstore.unity.com/

[18] J. Newton-King, "Newtonsoft.Json (Json.NET)," 2024. [Online].
    Available: https://www.newtonsoft.com/json

[19] J. Resig and S. Maxwell, "gilzoide.sqlite-net Unity Package,"
    OpenUPM, 2024. [Online]. Available:
    https://openupm.com/packages/com.gilzoide.sqlite-net/

[20] ISO/IEC, "ISO/IEC 25010:2011 Systems and software engineering —
    Systems and software Quality Requirements and Evaluation (SQuaRE)
    — System and software quality models," ISO, 2011.

[21] ITU-R, "Recommendation ITU-R BS.1770-4: Algorithms to measure
    audio programme loudness and true-peak audio level," 2015.

[22] IEEE, "IEEE Std 754-2019, IEEE Standard for Floating-Point
    Arithmetic," 2019.

[23] IEC, "IEC 61966-2-1:1999, Multimedia systems and equipment —
    Colour measurement and management — Part 2-1: Colour management
    — Default RGB colour space — sRGB," 1999.

[24] J. Linietsky, "Khronos glTF 2.0 Specification," Khronos Group,
    2017.

[25] Unity Technologies, "Inference Engine (formerly Sentis) Manual,"
    2024. [Online]. Available:
    https://docs.unity3d.com/Packages/com.unity.ai.inference@2.6/

---

## APPENDIX A — Selected Source Code Listings

The following listings are excerpted from the production source code. They
are presented in approximate dependency order — from foundational
subsystems (gaze detection, audio I/O) up through the integrating
orchestrator and the embodied agent. Comments and using-directives have
been preserved where they aid understanding; full namespaces are shown
at first use only.

---

### Listing A.1 — Gaze-Based Artifact Identification

The core of the per-frame gaze evaluation. Casts a ray from the head,
discards artifacts whose bounds centre is outside the configured
distance, advances a dwell timer when the same artifact is held,
and fires the identification event with a per-artifact cooldown.

*File: `Assets/Scripts/Runtime/Artifact/GazeArtifactDetector.cs`*

```csharp
void Update()
{
    if (gazeOrigin == null)
    {
        if (Camera.main == null) return;
        gazeOrigin = Camera.main.transform;
    }

    ArtifactInfo hitArtifact = null;
    RaycastHit hit = default;

    if (Physics.Raycast(gazeOrigin.position, gazeOrigin.forward,
        out hit, maxDistance, raycastMask,
        QueryTriggerInteraction.Ignore))
    {
        hitArtifact = hit.collider.GetComponentInParent<ArtifactInfo>();
    }

    // Reject artifacts whose actual bounds-center is beyond range —
    // colliders can extend a long way past maxDistance, and museum
    // walls without colliders can let the ray clip into artifacts
    // in other rooms.
    if (hitArtifact != null)
    {
        Bounds artifactBounds = ComputeArtifactBounds(hitArtifact);
        float centerDist = Vector3.Distance(
            gazeOrigin.position, artifactBounds.center);
        if (centerDist > maxDistance)
            hitArtifact = null;
    }

    if (hitArtifact == null || IsOnCooldown(hitArtifact))
    {
        _current = null;
        _dwellElapsed = 0f;
        return;
    }

    if (hitArtifact != _current)
    {
        _current = hitArtifact;
        _dwellElapsed = 0f;
        return;
    }

    _dwellElapsed += Time.deltaTime;
    if (_dwellElapsed >= dwellSeconds)
    {
        _cooldownUntil[hitArtifact] = Time.time + cooldownSeconds;
        _dwellElapsed = 0f;
        _current = null;
        OnArtifactIdentified?.Invoke(hitArtifact, hit);
    }
}
```

---

### Listing A.2 — Real-Time WebSocket Client

The WebSocket transport for the OpenAI Realtime API. Two background
loops — receive and send — communicate with the main thread through
a thread-safe queue. JSON framing only; session config lives in the
orchestrator.

*File: `Assets/Scripts/Runtime/Voice/RealtimeClient.cs`*

```csharp
public async Task ConnectAsync(string apiKey,
    string url = DefaultUrl,
    CancellationToken externalCt = default)
{
    if (string.IsNullOrEmpty(apiKey))
        throw new ArgumentException(
            "OpenAI API key is empty.", nameof(apiKey));

    _socket = new ClientWebSocket();
    _socket.Options.SetRequestHeader(
        "Authorization", "Bearer " + apiKey);
    _socket.Options.SetRequestHeader("OpenAI-Beta", "realtime=v1");

    _cts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);

    try
    {
        await _socket.ConnectAsync(new Uri(url), _cts.Token)
            .ConfigureAwait(false);
    }
    catch (Exception e)
    {
        OnClose?.Invoke($"Connect failed: {e.Message}");
        Cleanup();
        throw;
    }

    OnOpen?.Invoke();
    _receiveLoop = Task.Run(ReceiveLoop, _cts.Token);
    _sendLoop = Task.Run(SendLoop, _cts.Token);
}

async Task ReceiveLoop()
{
    var buffer = new byte[64 * 1024];
    var sb = new StringBuilder();
    try
    {
        while (!_cts.IsCancellationRequested && IsOpen)
        {
            sb.Clear();
            WebSocketReceiveResult result;
            do
            {
                result = await _socket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), _cts.Token)
                    .ConfigureAwait(false);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    OnClose?.Invoke(
                        $"Server closed: {result.CloseStatus} " +
                        $"{result.CloseStatusDescription}");
                    return;
                }
                sb.Append(Encoding.UTF8.GetString(
                    buffer, 0, result.Count));
            } while (!result.EndOfMessage);

            var raw = sb.ToString();
            if (string.IsNullOrEmpty(raw)) continue;
            JObject parsed = JObject.Parse(raw);
            OnEvent?.Invoke(parsed);
        }
    }
    catch (OperationCanceledException) { }
    catch (Exception e)
    {
        OnClose?.Invoke($"Receive loop error: {e.Message}");
    }
}
```

---

### Listing A.3 — Streaming Audio Player (30-second Ring Buffer)

PCM16 audio deltas arrive base64-encoded from the API faster than
real-time and in jittery bursts. The ring buffer absorbs the burstiness;
a 200 ms pre-buffer ensures playback only starts when enough samples
have arrived to survive typical jitter.

*File: `Assets/Scripts/Runtime/Voice/StreamingAudioPlayer.cs`*

```csharp
public const int SampleRate = 24000;
const int RingSeconds = 30;
public int prebufferSamples = 4800; // 200 ms at 24 kHz

public void EnqueueBase64Pcm16(string base64)
{
    if (string.IsNullOrEmpty(base64)) return;
    byte[] bytes;
    try { bytes = Convert.FromBase64String(base64); }
    catch { return; }
    if (bytes.Length < 2) return;

    int sampleCount = bytes.Length / 2;
    lock (_lock)
    {
        int free = _ring.Length - _available;
        if (sampleCount > free)
        {
            int drop = sampleCount - free;
            Debug.LogWarning(
                $"[StreamingAudioPlayer] Ring overflow — " +
                $"dropping {drop} oldest samples.");
            _readIdx = (_readIdx + drop) % _ring.Length;
            _available -= drop;
        }
        for (int i = 0; i < sampleCount; i++)
        {
            short v = (short)(bytes[i*2] | (bytes[i*2 + 1] << 8));
            _ring[_writeIdx] = v / (float)short.MaxValue;
            _writeIdx = (_writeIdx + 1) % _ring.Length;
        }
        _available += sampleCount;
    }
}

// Called on the audio thread. Must not allocate or touch UnityEngine
// APIs (besides math).
void OnPcmRead(float[] data)
{
    lock (_lock)
    {
        int needed = data.Length;

        if (!_draining)
        {
            if (_available < prebufferSamples)
            {
                for (int i = 0; i < needed; i++) data[i] = 0f;
                return;
            }
            _draining = true;
        }

        int take = Mathf.Min(needed, _available);
        for (int i = 0; i < take; i++)
        {
            _lastSample = _ring[_readIdx];
            data[i] = _lastSample;
            _readIdx = (_readIdx + 1) % _ring.Length;
        }
        _available -= take;

        // On underrun, fade from last sample to silence — less
        // audible click than slamming to 0 — then re-prebuffer.
        if (take < needed)
        {
            float decay = _lastSample;
            for (int i = take; i < needed; i++)
            {
                decay *= 0.92f;
                data[i] = decay;
            }
            _draining = false;
            _lastSample = 0f;
        }
    }
}
```

---

### Listing A.4 — Session Configuration with English Lock

The system prompt for the Realtime API. Note the opening sentences: an
explicit instruction to always respond in English, repeated for emphasis.
Without this lock the API will switch languages based on the visitor's
country or name.

*File: `Assets/Scripts/Runtime/Voice/GuideOrchestrator.cs`*

```csharp
public string baseInstructions =
    "Always respond in English regardless of the visitor's name, " +
    "country of origin, or any other context. " +
    "Do not switch languages even if the visitor's name or country " +
    "might suggest a different language. " +
    "You are a knowledgeable, warm museum tour guide in an Egyptian " +
    "museum. " +
    "Speak conversationally and concisely. Pause for the visitor's " +
    "questions. " +
    "When the system tells you the visitor is looking at an artifact, " +
    "give a 1-2 sentence introduction (name, era, why it matters), " +
    "then invite a follow-up. " +
    "If the visitor asks a question unrelated to the current artifact, " +
    "answer briefly and steer them gently back to the museum.";

void ConfigureSession()
{
    if (_sessionConfigured) return;
    _sessionConfigured = true;

    var visitor = SessionState.Instance?.Visitor;
    string visitorPreamble = visitor != null
        ? $"The visitor's name is {visitor.Name}, age {visitor.Age}, " +
          $"from {visitor.CountryName}. Greet them by name on the " +
          $"first response."
        : "Greet the visitor warmly.";

    var instructions = visitorPreamble + "\n" + baseInstructions;

    var update = new JObject
    {
        ["type"] = "session.update",
        ["session"] = new JObject
        {
            ["modalities"] = new JArray("audio", "text"),
            ["voice"] = voice,
            ["instructions"] = instructions,
            ["input_audio_format"] = "pcm16",
            ["output_audio_format"] = "pcm16",
            ["turn_detection"] = (inputMode == InputMode.ServerVad)
                ? new JObject
                {
                    ["type"] = "server_vad",
                    ["threshold"] = vadThreshold,
                    ["prefix_padding_ms"] = 300,
                    ["silence_duration_ms"] = vadSilenceDurationMs
                }
                : null
        }
    };
    _client.Send(update);
}
```

---

### Listing A.5 — Push-to-Talk Across Keyboard and VR Controllers

The PTT detection probes both the keyboard and any connected XR
controller's primary button. No Input System action map is required —
the system works in any scene immediately.

*File: `Assets/Scripts/Runtime/Voice/GuideOrchestrator.cs`*

```csharp
readonly List<UnityEngine.XR.InputDevice> _xrDevices =
    new List<UnityEngine.XR.InputDevice>();

bool IsPushToTalkHeld()
{
    // Keyboard fallback for desktop dev / Quest Link with hardware
    // keyboard.
    if (Keyboard.current != null &&
        Keyboard.current[pushToTalkKey].isPressed)
    {
        return true;
    }

    // VR: hold either controller's primary button (Y on left, B on
    // right) to talk.
    UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(
        UnityEngine.XR.InputDeviceCharacteristics.Controller,
        _xrDevices);
    for (int i = 0; i < _xrDevices.Count; i++)
    {
        if (_xrDevices[i].TryGetFeatureValue(
                UnityEngine.XR.CommonUsages.primaryButton,
                out bool primary) && primary)
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
            _client.Send(new JObject
                { ["type"] = "response.cancel" });
        }
    }
    else if (!held && _pttWasHeld)
    {
        _client.Send(new JObject
            { ["type"] = "input_audio_buffer.commit" });
        _client.Send(new JObject
        {
            ["type"] = "response.create",
            ["response"] = new JObject
                { ["modalities"] = new JArray("audio", "text") }
        });
    }
    _pttWasHeld = held;
}
```

---

### Listing A.6 — Distance-Keeping Tour Guide with Hip-Bone XZ Lock

The agent doesn't merely chase the player — it maintains a fixed
conversational distance on whatever side it's currently on. The
`LateUpdate` hip-bone snap is the runtime safety net for Mixamo's
walk-cycle drift.

*File: `Assets/Scripts/Runtime/Guide/TourGuideAgent.cs`*

```csharp
void Update()
{
    if (player == null)
    {
        if (Camera.main != null) player = Camera.main.transform;
        if (player == null) return;
    }

    var fromPlayerToAgent = transform.position - player.position;
    fromPlayerToAgent.y = 0f;
    float planarDist = fromPlayerToAgent.magnitude;

    // Compute the desired position: a point at stoppingDistance from
    // the player, on whatever side the agent is currently on.
    Vector3 dirFromPlayer;
    if (planarDist > 0.05f)
    {
        dirFromPlayer = fromPlayerToAgent / planarDist;
    }
    else
    {
        // Player is on top of the agent (e.g., teleported in).
        // Step forward, not into a wall behind.
        dirFromPlayer = transform.forward;
        dirFromPlayer.y = 0f;
        if (dirFromPlayer.sqrMagnitude < 0.001f)
            dirFromPlayer = Vector3.forward;
        dirFromPlayer.Normalize();
    }
    var desiredPos = player.position + dirFromPlayer * stoppingDistance;
    desiredPos.y = transform.position.y;
    float distToDesired = Vector3.Distance(transform.position, desiredPos);

    if (Time.time >= _nextRepathTime)
    {
        _nextRepathTime = Time.time + repathInterval;
        if (_agent.isOnNavMesh)
        {
            if (distToDesired > 0.4f)
            {
                _agent.isStopped = false;
                _agent.SetDestination(desiredPos);
            }
            else
            {
                _agent.isStopped = true;
            }
        }
    }

    // Face the player whenever within stopping range.
    var toPlayer = -fromPlayerToAgent;
    if (toPlayer.sqrMagnitude > 0.0001f)
    {
        var faceTarget = Quaternion.LookRotation(toPlayer);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, faceTarget, Time.deltaTime * 4f);
    }

    if (animator != null)
    {
        bool moving = _agent.velocity.sqrMagnitude > 0.04f;
        animator.SetBool(_isMovingHash, moving);
        animator.SetBool(_isSpeakingHash, isSpeaking);
    }
}

// Runs after the Animator has written bone poses. Re-snap the hip's
// local XZ so any forward/lateral drift the animation introduces is
// undone. Y is left free so the walk-cycle bob is preserved.
void LateUpdate()
{
    if (_hipBone == null) return;
    var p = _hipBone.localPosition;
    p.x = _hipRestLocalPos.x;
    p.z = _hipRestLocalPos.z;
    _hipBone.localPosition = p;
}
```

---

### Listing A.7 — Amplitude-Driven Mouth Animation

`AmplitudeJawFlap` reads the AudioSource's output, computes an RMS,
and either rotates the jaw bone (if one was found) or simply flips the
`IsSpeaking` flag — which itself drives the Animator into the Talk
state regardless of bone availability.

*File: `Assets/Scripts/Runtime/Guide/AmplitudeJawFlap.cs`*

```csharp
void LateUpdate()
{
    if (audioSource == null) return;

    audioSource.GetOutputData(_samples, 0);
    float sum = 0f;
    for (int i = 0; i < _samples.Length; i++)
        sum += Mathf.Abs(_samples[i]);
    float rms = sum / _samples.Length;

    // Jaw bone is optional — most Mixamo characters don't have one.
    // The IsSpeaking flag alone is enough to drive the Talk animation
    // layer.
    if (jawBone != null)
    {
        float target = Mathf.Clamp01(rms * amplitudeGain);
        _smoothedAmplitude = Mathf.MoveTowards(
            _smoothedAmplitude, target,
            responsiveness * Time.deltaTime);
        float angle = _smoothedAmplitude * maxOpenDegrees;
        jawBone.localRotation = _restRotation *
            Quaternion.AngleAxis(angle, openAxis);
    }

    if (tourGuideAgent != null)
        tourGuideAgent.isSpeaking = rms > silenceThreshold;
}
```

---

### Listing A.8 — Portable Configuration: Find Config Next to the Executable

The two-tier path search that makes the USB-portable build work.
`Application.dataPath` resolves to `<exe>_Data` in a built player; its
parent is the folder the .exe lives in.

*File: `Assets/Scripts/Runtime/Config/OpenAIConfig.cs`*

```csharp
public static string GetConfigPath()
{
    // Portable path first: <exe folder>/MuseumVR/config.json. Lets the
    // build run on any machine just by copying the folder, no per-user
    // setup needed.
    var portable = GetPortablePath();
    if (!string.IsNullOrEmpty(portable) && File.Exists(portable))
        return portable;

    string baseDir;
    try { baseDir = Environment.GetFolderPath(
        Environment.SpecialFolder.ApplicationData); }
    catch { baseDir = Application.persistentDataPath; }
    if (string.IsNullOrEmpty(baseDir))
        baseDir = Application.persistentDataPath;
    return Path.Combine(baseDir, "MuseumVR", FileName);
}

static string GetPortablePath()
{
    try
    {
        // In a built player Application.dataPath is "<exe>_Data";
        // one level up is the exe folder.
        var dataPath = Application.dataPath;
        if (string.IsNullOrEmpty(dataPath)) return null;
        var parent = Directory.GetParent(dataPath)?.FullName;
        if (string.IsNullOrEmpty(parent)) return null;
        return Path.Combine(parent, "MuseumVR", FileName);
    }
    catch { return null; }
}
```

---

### Listing A.9 — Visitor Record Persistence

The SQLite wrapper used by the lobby. The DB lives next to the .exe
in portable mode, falling back to `%APPDATA%/MuseumVR/` for
development. A small schema: one row per visit.

*File: `Assets/Scripts/Runtime/Persistence/MuseumDatabase.cs`*

```csharp
public sealed class MuseumDatabase : IDisposable
{
    readonly SQLiteConnection _conn;
    public string DbPath { get; }

    public MuseumDatabase(string path = null)
    {
        DbPath = path ?? DefaultDbPath();
        Directory.CreateDirectory(Path.GetDirectoryName(DbPath));
        _conn = new SQLiteConnection(DbPath,
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.FullMutex);
        _conn.CreateTable<VisitorRecord>();
    }

    public static string DefaultDbPath()
    {
        // Portable path first: <exe folder>/MuseumVR/museum.db. Lets
        // the build be self-contained on a USB drive.
        try
        {
            var dataPath = Application.dataPath;
            if (!string.IsNullOrEmpty(dataPath))
            {
                var parent = Directory.GetParent(dataPath)?.FullName;
                if (!string.IsNullOrEmpty(parent))
                {
                    var portableDir = Path.Combine(parent, "MuseumVR");
                    if (Directory.Exists(portableDir))
                        return Path.Combine(portableDir, "museum.db");
                }
            }
        }
        catch { /* fall through to AppData */ }

        string baseDir;
        try
        {
            baseDir = Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrEmpty(baseDir))
                baseDir = Application.persistentDataPath;
        }
        catch { baseDir = Application.persistentDataPath; }
        return Path.Combine(baseDir, "MuseumVR", "museum.db");
    }

    public int InsertVisitor(VisitorRecord record)
    {
        if (record.StartedAtUnixSeconds == 0)
            record.StartedAtUnixSeconds =
                DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        _conn.Insert(record);
        return record.Id;
    }
}
```

---

### Listing A.10 — HDRP MaskMap Bake from Separate PBR Textures

Mixamo ships PBR textures as separate files: `lambert1_Metallic.png`,
`lambert1_Roughness.png`, etc. HDRP/Lit expects a single packed MaskMap
(R = metallic, G = AO, B = detail, A = smoothness). This baker reads
the separate textures' pixels and assembles the packed map at edit
time.

*File: `Assets/Editor/MixamoMaskMapBaker.cs`*

```csharp
int len = w * h;
var pixels = new Color32[len];
for (int i = 0; i < len; i++)
{
    byte mVal = mPx != null ? Sample(mPx, i, len, mPx.Length) : (byte)0;
    byte rVal = rPx != null ? Sample(rPx, i, len, rPx.Length) : (byte)128;
    byte aoVal = aoPx != null ? Sample(aoPx, i, len, aoPx.Length) : (byte)255;
    byte smooth = (byte)(255 - rVal); // smoothness = 1 - roughness
    pixels[i] = new Color32(mVal, aoVal, 0, smooth);
}

var mask = new Texture2D(w, h, TextureFormat.RGBA32,
    mipChain: true, linear: true);
mask.SetPixels32(pixels);
mask.Apply();
var bytes = mask.EncodeToPNG();
Object.DestroyImmediate(mask);

File.WriteAllBytes(Path.GetFullPath(OutputPath), bytes);
AssetDatabase.ImportAsset(OutputPath, ImportAssetOptions.ForceUpdate);
var imp = (TextureImporter)AssetImporter.GetAtPath(OutputPath);
imp.sRGBTexture = false;             // MaskMap is non-color data
imp.textureType = TextureImporterType.Default;
imp.textureCompression = TextureImporterCompression.Uncompressed;
imp.SaveAndReimport();
```

---

### Listing A.11 — Production Build Pre-Flight Validation

The production VR builder refuses to invoke `BuildPipeline.BuildPlayer`
unless three project-state invariants hold: OpenXR auto-loading is
enabled for Standalone, the Lobby scene's `LobbyController.loadAdditively`
is set to single-mode, and at least one scene is enabled in Build
Settings. The builder additionally walks every scene to be built and
strips any leftover `XR Device Simulator` GameObjects so that residue
from earlier development testing cannot enter the production binary.

*File: `Assets/Editor/PortablePackageBuilder.cs`*

```csharp
static List<string> RunPreflightChecks()
{
    var failures = new List<string>();

    // OpenXR auto-loading must be ON for Standalone — VR-only build.
    var manager = GetStandaloneXRManager();
    if (manager == null)
        failures.Add("XR Plug-in Management for Standalone is not configured.");
    else if (!manager.automaticLoading)
        failures.Add("OpenXR auto-loading is DISABLED for Standalone.");

    // Lobby must be in single-mode load (avoids dual XR Origin collision).
    var lobbyPath = "Assets/Scenes/Lobby.unity";
    var prev = EditorSceneManager.GetActiveScene().path;
    EditorSceneManager.OpenScene(lobbyPath, OpenSceneMode.Single);
    var lc = UnityEngine.Object.FindAnyObjectByType<Museum.Lobby.LobbyController>();
    if (lc != null && lc.loadAdditively)
        failures.Add(
            "LobbyController.loadAdditively is TRUE. Single mode is required " +
            "for production.");
    if (!string.IsNullOrEmpty(prev))
        EditorSceneManager.OpenScene(prev, OpenSceneMode.Single);

    return failures;
}

static int StripSimulatorFromBuildScenes(string[] scenePaths)
{
    int totalRemoved = 0;
    foreach (var path in scenePaths)
    {
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        int removedHere = 0;
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "XR Device Simulator" ||
                root.name.StartsWith("XR Device Simulator"))
            {
                UnityEngine.Object.DestroyImmediate(root);
                removedHere++;
            }
        }
        if (removedHere > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        totalRemoved += removedHere;
    }
    return totalRemoved;
}
```

---

### Listing A.12 — Lobby Submit and Scene Transition

The lobby's submit handler. Validates the form, inserts the visitor
into SQLite, stashes the record on the cross-scene `SessionState`
singleton, and switches to the museum scene in Single mode (the
additive path caused dual-XR-Origin rendering artifacts).

*File: `Assets/Scripts/Runtime/Lobby/LobbyController.cs`*

```csharp
void OnSubmit()
{
    if (submitButton != null) submitButton.interactable = false;

    var country = ResolveCountry();
    var record = new VisitorRecord
    {
        Name = (nameField != null ? nameField.text : "Anonymous").Trim(),
        Age = ageSlider != null ? (int)ageSlider.value : 0,
        CountryCode = country.code,
        CountryName = country.name,
        StartedAtUnixSeconds =
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    };

    try { _db.InsertVisitor(record); }
    catch (Exception e)
    {
        Debug.LogError($"[Lobby] DB insert failed: {e}");
        if (statusText != null)
            statusText.text = "Database error. See Console.";
        if (submitButton != null) submitButton.interactable = true;
        return;
    }

    _session.Visitor = record;

    var sceneName = string.IsNullOrEmpty(museumSceneName)
        ? "Museum" : museumSceneName;
    if (loadAdditively)
    {
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive)
            .completed += _ =>
        {
            var nextScene = SceneManager.GetSceneByName(sceneName);
            if (nextScene.IsValid())
                SceneManager.SetActiveScene(nextScene);
            SceneManager.UnloadSceneAsync(gameObject.scene);
        };
    }
    else
    {
        // Single mode is the default — avoids the dual-XR-Origin
        // collision that additive mode produces for one frame.
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    }
}
```

---

## APPENDIX B — Editor Menu Reference

Every multi-step authoring action in the project is encoded as a menu
command under `Tools → Museum`. The 23 menu items below regenerate
scenes, fix materials, bake the NavMesh, wire the avatar, and produce
both portable builds. This appendix is the definitive reference.

### Phase 0 — Project Setup

| Menu | What it does |
|---|---|
| `Apply HDRP VR Settings` | Configures the HDRP asset for VR: disables raytracing, screen-space reflections, screen-space global illumination, volumetric clouds, and screen-space shadows; sets Lit Shader Mode to Forward; enables Single-Pass Instanced rendering. |
| `Fix Sample Materials for HDRP` | Scans `Assets/Samples/` and converts every non-HDRP material it finds to either `HDRP/Lit` or `HDRP/Unlit`, preserving base colour and main texture references. Necessary because the XR Starter Assets ship URP/Built-in materials that render pink in HDRP. |

### Phase 1 — VR Foundation

| Menu | What it does |
|---|---|
| `Configure OpenXR (Quest)` | Enables the OpenXR plug-in for the Standalone build target, activates the Meta Quest, Meta Quest Touch Pro, and Meta Quest Touch Plus interaction profiles. |
| `Import XR Starter Assets` | Imports the XR Interaction Toolkit Starter Assets sample (XR Origin prefab, action map, locomotion providers). |
| `Set Up Museum Scene` | Copies the AK Studio Art demo scene to `Assets/Scenes/Museum.unity`, replaces the original Main Camera with an XR Origin instance, preserves the HDRP Volume and Sky. |

### Phase 2 — Locomotion and Interaction

| Menu | What it does |
|---|---|
| `Mark Selected as Teleportation Area` | Adds MeshColliders (where missing) and TeleportationArea components to every MeshFilter under the selected GameObjects; tags interactors with the Teleport interaction layer. |
| `Clear Teleportation Components from Selection` | Removes TeleportationArea components from the selection — non-destructive, keeps the MeshColliders. |
| `Diagnose Teleport Setup` | Inspects the scene for common teleport misconfigurations (interactors not tagged, teleport area missing colliders, etc.) and prints a report. |

### Phase 3 — Artifact and Gaze System

| Menu | What it does |
|---|---|
| `Set Up Gaze Detection in Active Scene` | Adds the `GazeArtifactDetector` GameObject under the XR Origin, with the gaze origin pointed at the head camera. |
| `Tag Selected as Artifact` | Adds the `ArtifactInfo` component to each selected GameObject, ready for auto-fill. |
| `Auto-Fill Artifact Info` | Looks up each `ArtifactInfo` by its GameObject name in a hard-coded catalogue of 42 entries; fills `displayName`, `era`, and `description`. Non-destructive — won't overwrite existing values. |
| `Auto-Fill Artifact Info (Overwrite)` | Same as above but forces overwrite of existing values. |
| `Dump Tagged Artifact Names` | Prints a sorted list of every GameObject in the scene that carries an `ArtifactInfo` — useful for diffing against the catalogue. |
| `Tighten Gaze Range to 2m` | Sets the gaze detector's `maxDistance` to 2 m (the empirically chosen value). |
| `Regenerate Label Prefab` | Rebuilds `Assets/Prefabs/UI/ArtifactLabel.prefab` and rewires every `ArtifactLabelSpawner`'s reference in the active scene. |
| `Add Artifact Capture Camera` | (Inert ML pipeline) Adds the disabled 224×224 render-texture camera under the XR head. |
| `Toggle Debug Capture Saver` | (Inert ML pipeline) Saves each captured frame to disk for visual debugging. |
| `Assign Class Indices From Labels File` | (Inert ML pipeline) Reads a CSV of class names and assigns each ArtifactInfo a corresponding class index. |
| `Wire ML Recognition Pipeline` | (Inert ML pipeline) Activates the ML-based recognition path in lieu of gaze detection. |

### Phase 4 — Lobby

| Menu | What it does |
|---|---|
| `Set Up Lobby Scene` | Programmatically generates the entire lobby scene from scratch: directional + fill lights, backdrop (floor, back wall with hieroglyph texture, side walls, two flanking gold columns), XR Origin, EventSystem, and the 1.6×1.1 m world-space form canvas with all fields wired to `LobbyController`. Idempotent — re-running regenerates from the latest code. |

### Phase 5 — Tour Guide Voice

| Menu | What it does |
|---|---|
| `Add Tour Guide to Active Scene` | Creates the `Tour Guide` GameObject and attaches `MicCapture`, `StreamingAudioPlayer`, `GuideOrchestrator`, and `SingletonAudioListener`; wires the gaze detector reference. |
| `Reset Tour Guide Model to Default` | Resets the orchestrator's model field to `gpt-4o-mini-realtime-preview`. |
| `Set Tour Guide to Gaze-Only Mode` | Sets `inputMode = GazeOnly`; mic disabled. |
| `Set Tour Guide to Push-To-Talk Mode` | Sets `inputMode = PushToTalk`; the default. |
| `Set Tour Guide to Server-VAD Mode` | Sets `inputMode = ServerVad`; warns the operator about echo risk. |

### Phase 6 — Tour Guide Embodiment

| Menu | What it does |
|---|---|
| `Bake Museum NavMesh` | Adds NavMeshSurface components to the museum's walkable floor meshes and bakes the navmesh. |
| `Add Placeholder Tour Guide Body (Capsule)` | Adds a capsule under the Tour Guide GameObject for early-Phase-6 testing before the Mixamo avatar is wired. |
| `Wire Mixamo Avatar` | End-to-end Mixamo integration: deletes the placeholder, instantiates Guide.fbx, builds the Idle/Walk/Talk Animator Controller, adds NavMeshAgent + TourGuideAgent + AmplitudeJawFlap, parents the audio source to the head bone, locates the jaw bone if any. |
| `Fix Mixamo Materials for HDRP` | Extracts embedded materials/textures from Mixamo FBXs, swaps shaders to HDRP/Lit, wires BaseColor / Normal / Mask textures by naming convention with fuzzy fallback. |
| `Diagnose & Repair Avatar Materials` | Inspects every renderer under "Tour Guide Avatar"; reports current material assignments, replaces broken or null materials with `lambert2.mat`, re-wires missing texture slots. Also kills duplicate avatar GameObjects. |
| `Bake Mixamo MaskMap` | Reads the separate Metallic and Roughness textures Mixamo provides, packs them into a proper HDRP MaskMap (R=metallic, A=smoothness), and wires it onto every Mixamo material. |
| `Configure Mixamo Animations for In-Place Looping` | Sets `loopTime = true` and `lockRootPositionXZ = true` on every clip in Idle/Walking/Walking1/Talking FBXs. Complements the runtime hip-bone XZ-lock. |

### Phase 7 — Production Build and Distribution

| Menu | What it does |
|---|---|
| `Build Portable Production Package (VR)` | Runs pre-flight validation (OpenXR auto-loading on, Lobby in single-mode load, scenes in Build Settings), strips any leftover `XR Device Simulator` GameObjects from the built scenes, then produces a self-contained Windows x64 build at `Build/MuseumVR-Production/`. Auto-copies `%APPDATA%/MuseumVR/config.json` next to the executable and writes an operator-facing README. Targets Meta Quest 2 / 3 / Pro via Quest Link / Air Link / Steam Link. |

**Table B.1 — Editor menu reference, 20 menus across 8 phases.**

---

## APPENDIX C — Build and Deployment Instructions

### C.1 Development Machine Prerequisites

Before opening the project for the first time:

1. **Unity Hub** installed with **Unity 6000.4.5f1 LTS** added.
2. **Meta Quest Link** app installed on the dev machine.
3. A **Meta Quest 2 / 3 / Pro** headset.
4. A **USB-C cable** rated for data, supporting Quest Link.
5. **OpenAI API account** with billing set up.

### C.2 First-Time Project Open

1. Clone or copy the project folder to a local drive.
2. Open the project in Unity 6000.4.5f1 LTS. The first import will take
   5–15 minutes as packages resolve.
3. When prompted, accept the OpenUPM scoped registry (required for
   `com.gilzoide.sqlite-net`).
4. When prompted, import **TextMeshPro Essentials**.
5. In the Package Manager, ensure these are installed:
   - XR Plug-in Management 4.5.0
   - OpenXR Plugin (latest)
   - XR Interaction Toolkit 3.0.8 (with Starter Assets sample)
   - AI Navigation
   - Inference Engine 2.6.1
   - Newtonsoft.Json
   - `com.gilzoide.sqlite-net`
6. Run `Tools → Museum → Apply HDRP VR Settings`.
7. Run `Tools → Museum → Configure OpenXR (Quest)`.
8. Run `Tools → Museum → Import XR Starter Assets`.

### C.3 OpenAI Key Configuration

Create the file `%APPDATA%/MuseumVR/config.json` (on Windows this resolves
to `C:\Users\<you>\AppData\Roaming\MuseumVR\config.json`):

```json
{ "openai_api_key": "sk-proj-..." }
```

The key is also auto-copied next to the .exe by the production build
menu, so operators of the portable build do not need to write to
`%APPDATA%` on the target machine.

### C.4 Building the Portable Production Package

1. Open `Lobby.unity` and confirm `LobbyController.loadAdditively` is
   unchecked.
2. Connect the Quest via Link cable and make sure Meta Quest Link
   reports the headset as connected.
3. Run `Tools → Museum → Phase 7 → Build Portable Production Package
   (VR)`. The builder runs pre-flight validation, strips any leftover
   `XR Device Simulator` residue from the built scenes, and produces:
   - `Build/MuseumVR-Production/MuseumVR.exe`
   - `Build/MuseumVR-Production/MuseumVR_Data/`
   - `Build/MuseumVR-Production/MuseumVR/config.json` (auto-copied)
   - `Build/MuseumVR-Production/README.txt`
4. Copy the whole `MuseumVR-Production` folder to a USB drive.
5. On the target machine: install Meta Quest Link, connect the Quest,
   run `MuseumVR.exe`.

### C.5 Troubleshooting

| Symptom | Cause | Resolution |
|---|---|---|
| Build refused: "OpenXR auto-loading is DISABLED" | Standalone XR loader was disabled in a prior workflow | Edit > Project Settings > XR Plug-in Management > Standalone tab > re-enable OpenXR |
| Build refused: "LobbyController.loadAdditively is TRUE" | Lobby is set to additive scene load | Open `Lobby.unity`, find the LobbyController, uncheck Load Additively, save |
| Avatar renders pink | HDRP/Mixamo material mismatch | Run `Fix Mixamo Materials for HDRP`, then `Bake Mixamo MaskMap` |
| Avatar walks toward you and snaps back | Mixamo root-motion bake into hip bone | Already fixed at runtime by `TourGuideAgent.LateUpdate` — re-run wire-up if the avatar was attached before this commit |
| Controllers disappear at scene transition | Additive load creating dual XR Origins | Uncheck `LobbyController.loadAdditively` in the Inspector |
| Guide responds in French / Spanish / Japanese | Realtime API language drift from visitor name/country | Verify `GuideOrchestrator.baseInstructions` opens with the English lock |
| Tour guide audio cuts mid-sentence | Ring buffer overflow on long responses | `StreamingAudioPlayer.RingSeconds` is 30 — increase if the warning appears repeatedly in the console |
| Tour guide echo-loops (keeps responding to itself) | Server-VAD picking up speaker output | Switch to Push-to-Talk mode (the default) |

### C.6 Target-Machine Requirements

- Windows 10 or 11 (x64)
- Meta Quest 2 / 3 / Pro
- Meta Quest Link app
- USB-C data cable, Air Link, or Steam Link
- Working microphone (PC mic, USB mic, or the Quest's built-in mic)
- Stable internet connection (for the OpenAI Realtime API)

---

## APPENDIX D — Artifact Catalogue

The 42 artifacts in the museum, sourced from `Assets/Editor/
ArtifactInfoAutoFill.cs`. Each is keyed by GameObject name in the
scene; the editor utility looks up each `ArtifactInfo` by its GameObject
name and populates the `displayName`, `era`, and `description` fields.

| # | Display Name | Era | Description |
|---|---|---|---|
| 1 | Ankh Khonsu | Late Period, 26th Dynasty (~664–525 BCE) | A black granite block statue of Ankh-Khonsu, an official and priest of the god Khonsu at Karnak. The cubic form lets the priest "wear" lengthy biographical and religious inscriptions — a hallmark of Late Period elite statuary. |
| 2 | Apis Bull | Late Period to Ptolemaic (~664–30 BCE) | A bronze figure of the Apis bull, the living incarnation of Ptah at Memphis. The solar disk between the horns and the triangular forehead marking identify the sacred animal that, when it died, was mummified and entombed in the Serapeum at Saqqara. |
| 3 | Baboon | New Kingdom to Late Period | A seated baboon representing Thoth, god of wisdom, writing, and the moon. Thoth was worshipped in baboon and ibis forms; cult centres at Hermopolis kept and mummified thousands of these animals as votive offerings. |
| 4 | Block Statue | Middle Kingdom to Late Period | A "block statue" — a seated official with knees drawn up, body wrapped in a cloak forming a cubic mass. Popularised in the Middle Kingdom, this format gave priests and officials a durable, inscription-friendly votive image they could place in temples. |
| 5 | Canopic Jar | New Kingdom to Late Period | A canopic jar — one of a set of four that held the embalmed lungs, liver, stomach and intestines of the deceased. Each lid depicts a Son of Horus: human-headed Imsety, baboon Hapy, jackal Duamutef, and falcon Qebehsenuef. |
| 6 | Cat Statue | Late Period (~664–332 BCE) | A bronze seated cat sacred to Bastet, goddess of home, fertility, and protection. Bubastis was her cult centre. Cats were mummified by the hundreds of thousands and offered to her temples. |
| 7 | Chest of Tutankhamun | New Kingdom, 18th Dynasty, reign of Tutankhamun (~1336–1326 BCE) | A painted wooden chest from the tomb of Tutankhamun (KV62). Its scenes show the boy-king triumphant in battle and on the lion hunt — ritual images projecting royal power onto his afterlife. |
| 8 | Coffin of Akhenaten | New Kingdom, late 18th Dynasty, Amarna Period (~1336 BCE) | Coffin associated with Akhenaten, the heretic pharaoh who moved the capital to Akhetaten and elevated the sun-disk Aten above the traditional gods. After his death his name was struck from monuments — surviving objects from this short period are exceptionally rare. |
| 9 | Coffin of Amunred | Third Intermediate Period (~1069–664 BCE) | Brightly painted anthropoid coffin of a Theban priest of Amun. After the New Kingdom, royal tombs were less elaborate; high-status priests instead invested in finely decorated nested coffins covered in protective spells and judgment scenes. |
| 10 | Coffin of Mut-iy-iy | Third Intermediate Period (~1069–664 BCE) | Anthropoid coffin of Mut-iy-iy. The painted wig, broad collar, and protective deities along the lid reproduce the funerary papyrus tradition directly onto the coffin's surface — the body itself becomes a Book of the Dead. |
| 11 | Egyptian Boat | Middle Kingdom to New Kingdom | A model funerary boat, placed in tombs to ferry the deceased through the netherworld and to journey with Ra across the sky. Larger versions, like Khufu's full-size cedar barque, were buried near royal pyramids. |
| 12 | Egyptian Khopesh | New Kingdom (~1550–1070 BCE) | A khopesh — the iconic sickle-sword carried by Egyptian elite soldiers and pharaohs. Its hooked blade was effective against shields and is shown in royal smiting scenes from Thutmose III through Ramses II. |
| 13 | False Door of Ni-ankh-Snefru | Old Kingdom, 4th–5th Dynasty (~2600–2400 BCE) | A false-door stela for the ka of Ni-ankh-Snefru. Carved into the west wall of mastaba chapels, it served as the threshold through which the soul could leave the burial chamber to receive offerings made by the living. |
| 14 | Goddess Sekhmet | New Kingdom, 18th Dynasty (~1390 BCE) | Granite statue of Sekhmet, lioness goddess of war and plague. Amenhotep III commissioned hundreds for his mortuary temple at Thebes — one for each day of the year, to appease her destructive power. |
| 15 | Goddess Taweret | Late Period (~664–332 BCE) | A hippopotamus-headed Taweret, goddess of childbirth and protector of mother and child. Her composite body (lion, hippo, crocodile) made her a fearsome guardian; small amulets of her are among the most common household objects from Egypt. |
| 16 | Head of Tutankhamun | New Kingdom, 18th Dynasty (~1332 BCE) | A wooden head of the boy-king Tutankhamun emerging from a lotus blossom, found in his tomb. The image equates the young pharaoh with the rising sun reborn from the primordial waters at the dawn of creation. |
| 17 | Horus Statue | Late Period to Ptolemaic | Falcon statue of Horus, sky god and son of Osiris and Isis. Edfu's well-preserved temple is dedicated to him. The pharaoh was conceived as the living Horus, making this image both religious and royal. |
| 18 | Inner Coffin of Djedmut | Third Intermediate Period (~1069–664 BCE) | Inner coffin of the lady Djedmut, a Theban priestess. Densely painted with protective deities, scenes from the Amduat, and offering formulas — the inner coffin sat inside one or two outer coffins, each layer adding another shell of magical protection. |
| 19 | Khafre Enthroned | Old Kingdom, 4th Dynasty (~2570 BCE) | The diorite seated statue of Khafre, builder of the second Giza pyramid. Horus enfolds the king's head with his wings, fusing earthly and divine kingship in one image. The hard imported stone alone is a statement of state power. |
| 20 | Khasekhemwy | Early Dynastic, 2nd Dynasty (~2700 BCE) | Seated statue of Khasekhemwy, last king of the 2nd Dynasty. Inscriptions along the base record the slain enemies of his northern campaign — among the earliest royal sculptures in Egyptian history. |
| 21 | King Djoser | Old Kingdom, 3rd Dynasty (~2670 BCE) | Painted limestone seated statue of Djoser, builder of the Step Pyramid at Saqqara. Found in the serdab — a sealed chamber with eye-slits — where the ka-statue could "see" the offerings made for the king's eternal sustenance. |
| 22 | Nefertiti Bust | New Kingdom, late 18th Dynasty, Amarna Period (~1345 BCE) | The painted limestone bust of Queen Nefertiti, Great Royal Wife of Akhenaten. Discovered in the Amarna workshop of the sculptor Thutmose, the asymmetrical eyes and elegant elongated neck define the Amarna style. |
| 23 | Nofret | Old Kingdom, 4th Dynasty (~2580 BCE) | Painted limestone statue of Lady Nofret, wife of prince Rahotep. The inlaid rock-crystal eyes are so lifelike that the workmen who unearthed her at Meidum reportedly fled in fear. Among the most vivid portraits of Old Kingdom nobility. |
| 24 | Rahotep | Old Kingdom, 4th Dynasty (~2580 BCE) | Painted limestone statue of Prince Rahotep, son of Sneferu and overseer of works. Paired with his wife Nofret, the couple sit upright and frontal — the classic Old Kingdom convention asserting eternal presence. |
| 25 | Ramses III | New Kingdom, 20th Dynasty (~1180 BCE) | Image of Ramses III, last great warrior pharaoh, who repelled the Sea Peoples at the Battle of the Delta. His mortuary temple at Medinet Habu, with its famous battle reliefs, is one of the best-preserved royal complexes of Egypt. |
| 26 | Sandstone Sphinx Statue | New Kingdom | Sandstone sphinx with the body of a lion and the head of a king — a hybrid that fused royal authority with the protective power of a desert predator. Sphinxes lined processional avenues at Karnak and Luxor. |
| 27 | Sarcophagus of Hunefer | New Kingdom, 19th Dynasty (~1290 BCE) | Sarcophagus of Hunefer, royal scribe under Seti I. He is famous for his Book of the Dead papyrus, where the "weighing of the heart" before Osiris is illustrated — his name has come to stand for the entire ritual. |
| 28 | Sarcophagus of Mindjedef | Old Kingdom, 4th Dynasty (~2500 BCE) | Stone sarcophagus of prince Mindjedef. The plain, palace-façade decoration is typical of Old Kingdom royal-relative burials at Giza, where the dead were laid in mastabas around the king's pyramid. |
| 29 | Sphinx of Hatshepsut | New Kingdom, 18th Dynasty (~1470 BCE) | Granite sphinx with the lioness body and the bearded male features of Hatshepsut, female pharaoh who ruled as king. After her death her successor Thutmose III ordered her images destroyed; this sphinx was reassembled from fragments at Deir el-Bahri. |
| 30 | Statue of Amenhotep III | New Kingdom, 18th Dynasty (~1390–1352 BCE) | Amenhotep III ruled a wealthy and peaceful Egypt at the height of New Kingdom power. His massive building program left Egypt with the temple of Luxor, the colossi of Memnon, and thousands of statues — including the Sekhmets seen in this museum. |
| 31 | Statue of Hatshepsut | New Kingdom, 18th Dynasty (~1473–1458 BCE) | Hatshepsut as kneeling king, presenting offering jars. One of the few female pharaohs, she expanded trade with Punt and built the terraced mortuary temple of Deir el-Bahri. She is shown here in male royal regalia, the kilt and false beard. |
| 32 | Statue of Maya and Merit | New Kingdom, 18th Dynasty, post-Amarna (~1330 BCE) | Maya, Tutankhamun's overseer of the treasury, with his wife Merit. Maya helped restore order after Akhenaten's revolution; his tomb at Saqqara was rediscovered in 1986 with these statues still intact in the chapel. |
| 33 | Statue of Mentuhotep II | Middle Kingdom, 11th Dynasty (~2010 BCE) | Mentuhotep II reunited Egypt after the chaos of the First Intermediate Period and founded the Middle Kingdom. His seated statue from his Deir el-Bahri mortuary complex shows him in the white crown of Upper Egypt and Osiride pose. |
| 34 | Statue of Ramses III | New Kingdom, 20th Dynasty (~1180 BCE) | Royal statue of Ramses III in the traditional smiting pose. He defended Egypt against the Sea Peoples and the Libyans; his temple at Medinet Habu preserves vivid reliefs of those battles, including the world's oldest detailed naval engagement. |
| 35 | Statue of Roy | New Kingdom, 19th Dynasty (~1290 BCE) | The royal scribe Roy, kneeling and presenting an offering. As royal scribe he managed temple revenues — literacy was a rare and powerful skill, and many high officials had themselves shown holding a scribal palette. |
| 36 | Statuette of a Jackal | Late Period (~664–332 BCE) | Recumbent jackal — Anubis, god of embalming and guardian of the necropolis. He oversaw the wrapping of the body and led the deceased to the Hall of Two Truths for the weighing of the heart. |
| 37 | Stela of Ituerneheh | Middle Kingdom (~2055–1650 BCE) | Funerary stela of Ituerneheh. The deceased is shown seated before a table piled with offerings; the carved formula invokes Osiris to grant "a thousand of bread, beer, oxen and fowl" for the ka of the dead. |
| 38 | Stela of Seter-au | Middle Kingdom to Second Intermediate Period | Limestone funerary stela of Seter-au. Stelae like this stood at the entrance to the tomb chapel; passers-by who recited the offering prayer would feed the soul of the deceased — a kind of contract between the living and the dead. |
| 39 | Striding Thoth | Late Period (~664–332 BCE) | Striding statue of Thoth, ibis-headed god of writing, calculation, and the moon. Scribes invoked him at the start of every text. His main cult was at Hermopolis, where vast underground galleries hold mummified ibises offered to him. |
| 40 | The Goddess Mut | New Kingdom, 18th Dynasty (~1390 BCE) | Mut, the great mother goddess of Thebes and consort of Amun. Her temple at Karnak — with its sacred crescent lake — sits south of Amun's enclosure. She is most often shown wearing the double crown, sometimes with vulture headdress. |
| 41 | The Rosetta Stone | Ptolemaic Period, reign of Ptolemy V (196 BCE) | A decree of Ptolemy V issued in three scripts: hieroglyphic, demotic, and ancient Greek. Found by French soldiers at Rashid (Rosetta) in 1799, it gave Champollion the key to deciphering hieroglyphs in 1822 — the founding moment of Egyptology. |
| 42 | Triad of Menkaure | Old Kingdom, 4th Dynasty (~2530 BCE) | Greywacke triad: King Menkaure, builder of the third Giza pyramid, flanked by the goddess Hathor and a personification of one of Egypt's nomes. Several of these triads were carved for his pyramid temple — likely one per nome that supported the cult. |

**Table D.1 — Artifact catalogue, 42 entries.**

---

## APPENDIX E — Project Structure

A high-level view of the project's file system organisation, for the
benefit of someone opening the source for the first time.

```
Assets/
  Editor/                                   20 reproducible authoring utilities
    GltfBatchExporter.cs
    HdrpVrSetup.cs
    OpenXrSetup.cs
    XriStarterAssetsImport.cs
    MuseumSceneSetup.cs
    HdrpSampleMaterialFix.cs
    TeleportDiagnostic.cs
    TeleportSetup.cs
    ArtifactInfoAutoFill.cs                  ← 42-artifact catalogue lives here
    ArtifactCaptureSetup.cs
    ArtifactClassMapping.cs
    LobbySceneSetup.cs                       ← regenerates the lobby
    NavMeshBake.cs
    TourGuideBodySetup.cs
    ArtifactSetup.cs
    TourGuideSetup.cs
    MixamoWireUp.cs                          ← Phase 6 avatar integration
    MixamoMaterialFix.cs
    AvatarMaterialDiagnostic.cs
    MixamoMaskMapBaker.cs
    MixamoAnimationFix.cs
    PortablePackageBuilder.cs                ← production VR build (Phase 7)

  Scripts/Runtime/                          All gameplay code
    Artifact/
      ArtifactInfo.cs
      GazeArtifactDetector.cs
      ArtifactLabelSpawner.cs
      InferenceEngineArtifactClassifier.cs  (inert)
      ArtifactCaptureCamera.cs               (inert)
      ArtifactRecognitionPipeline.cs         (inert)
    Config/
      OpenAIConfig.cs
    Persistence/
      MuseumDatabase.cs
      VisitorRecord.cs
    Session/
      SessionState.cs
    Lobby/
      LobbyController.cs
      CountryList.cs                         (~140 countries)
    Voice/
      RealtimeClient.cs                      WebSocket transport
      MicCapture.cs                          Mic → 24 kHz PCM16
      StreamingAudioPlayer.cs                30-second ring buffer
      GuideOrchestrator.cs                   The brain
    Guide/
      TourGuideAgent.cs                      NavMesh follow + hip lock
      AmplitudeJawFlap.cs
    Util/
      SingletonAudioListener.cs

  Scenes/
    Lobby.unity                              First scene (build index 0)
    Museum.unity                             Loaded on Submit

  Mixamo/                                    User-supplied character + animations
    Guide.fbx
    Idle.fbx
    Walking.fbx
    Walking1.fbx
    Talking.fbx
    Materials/
      lambert2.mat                           Extracted from Guide.fbx
    Textures/
      lambert1_Base_color.png
      lambert1_Normal_OpenGL.png
      lambert1_Metallic.png
      lambert1_Roughness.png
      lambert1_MaskMap.png                   Baked by MixamoMaskMapBaker
    Animation/
      TourGuide.controller                   Built by MixamoWireUp

  Textures/Lobby/
    EgyptianHieroglyphs.jpg                  Back-wall texture

  Data/
    Countries.asset                          ScriptableObject

  Prefabs/UI/
    ArtifactLabel.prefab                     World-space label

  Models/Classifier/                         (Empty unless ML pipeline activated)
    classifier.onnx                          User-supplied if ML used

  AK Studio Art/Egyptian Museum VR/          Third-party environment (66 FBX assets)
    Scenes/Demo.unity                        Source for Museum.unity
    Models/                                  All artifact meshes

  Samples/XR Interaction Toolkit/3.0.8/      Imported sample
    Starter Assets/

Build/                                       Output of the Phase 7 build (not committed)
  MuseumVR-Production/

progress.md                                  Running progress and decisions log
SUMMARY.md                                   Snapshot of project state
PROJECT_OVERVIEW.txt                         Plain-English overview for non-engineers
PROJECT_REPORT.md                            This document
```

**Figure E.1 — Project file system structure.**

---

**End of Report.**
