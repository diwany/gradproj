# Figure Generation Prompts for the Graduation Report

> Twelve AI image-generation prompts, one per figure in the report's List
> of Figures, derived from a deep reading of `PROJECT_REPORT.md`. Every
> prompt is constrained to the actual implementation: Unity 6 HDRP,
> OpenXR, the OpenAI Realtime API with `gpt-4o-mini-realtime-preview`,
> the gaze raycast subsystem, the NavMesh-driven Mixamo Pharaoh guide,
> the 30-second audio ring buffer, the SQLite-backed lobby persistence,
> and the portable production VR deployment path targeting Meta Quest.
>
> **Global visual identity** (applies to all twelve figures unless
> noted): IEEE academic technical diagram, clean modern vector graphics,
> white background, sans-serif typography (Inter / Helvetica / system-
> sans), **dark navy blue (#1E3A8A)** primary, **cyan (#06B6D4)**
> accents, **light gray (#E5E7EB)** secondary surfaces, **gold (#D4A373)
> sparingly and only for Egyptian-cultural references**. Balanced
> symmetrical composition, generous whitespace, sharp lines, no skeuomorphic
> shading, no cartoon styling, no fantasy elements. 4K landscape unless
> the figure naturally fits portrait. Each prompt ends with a self-
> contained "Final AI Image Generation Prompt" that can be pasted
> directly into an image generator.

---

## Figure 1.1 — System Architecture Overview

**Purpose**
Single bird's-eye view of how the application is composed: external
services on top, the in-Unity orchestration layer in the middle, the
engine + I/O subsystems below, and the two input pathways (Quest
headset vs desktop simulator) at the bottom.

**Diagram Type**
Four-tier layered architecture diagram with vertical data-flow arrows
crossing tier boundaries.

**Visual Composition**
Four horizontal bands, top to bottom:

1. **External services** — a single rounded rectangle labelled
   "OpenAI Realtime API (wss://api.openai.com/v1/realtime)" with a
   small cloud icon to the left.
2. **Application logic** — five connected components in a single row:
   GuideOrchestrator (centre, slightly larger), with MicCapture,
   StreamingAudioPlayer, RealtimeClient, and GazeArtifactDetector
   distributed around it.
3. **Engine subsystems** — three boxes side-by-side: Unity HDRP
   renderer, OpenXR / XR Interaction Toolkit, NavMesh / NavMeshAgent.
   A fourth narrow box: SQLite persistence (Newtonsoft.Json on top).
4. **I/O pathway** — at the bottom: a Meta Quest headset icon
   labelled "PCVR via Meta Quest Link", connecting upward into the
   engine band.

**Elements to Include**
- Cloud icon + OpenAI label, with `wss://` annotation.
- 4 application-tier components named exactly as in the source:
  `GuideOrchestrator`, `MicCapture`, `StreamingAudioPlayer`,
  `RealtimeClient`, `GazeArtifactDetector`.
- 4 engine-tier blocks: `Unity HDRP 17.4`, `OpenXR + XRI 3.0.8`,
  `Unity AI Navigation`, `SQLite (gilzoide)`.
- Two device icons: Meta Quest headset; flat-panel monitor + peripherals.
- Three labelled arrow types: solid arrow = audio data, dashed arrow =
  JSON control events, thin gray arrow = engine API calls.

**Layout Structure**
Strict horizontal banding. Each band has a faint light-gray
background, vertically separated by 24-pixel gutters. Bands labelled
on the far left in vertical text: "EXTERNAL", "APPLICATION",
"ENGINE", "I/O".

**Labels and Annotation Requirements**
- Above each cross-tier arrow, a small inline label: e.g. "base64 PCM16
  audio", "JSON events", "session.update".
- Below each engine block, a small font-size version of the package
  name (e.g. `com.unity.render-pipelines.high-definition@17.4`).

**Color Guidance**
Primary dark navy blue for application-tier components. Cyan for arrow
strokes and the OpenAI box outline. Light gray fills for engine
subsystems. No gold anywhere — this is the architecture view, not the
cultural-content view.

**Camera / Viewpoint**
Pure 2D, frontal flat composition. No perspective, no shading.

**Final AI Image Generation Prompt**
> A four-tier layered system architecture diagram for a VR application,
> IEEE academic paper style, clean flat vector graphics, white background.
> Four horizontal bands, top to bottom: "EXTERNAL", "APPLICATION",
> "ENGINE", "I/O", each labelled in small vertical sans-serif text on
> the far left.
>
> Top band: a single rounded rectangle in dark navy blue outline labelled
> "OpenAI Realtime API" with the URL "wss://api.openai.com/v1/realtime"
> in smaller font underneath, with a small cloud icon to the left.
>
> Second band: five flat rectangular components in a single row,
> connected to the top OpenAI rectangle with two arrows: a solid cyan
> arrow labelled "base64 PCM16 audio" going up, and a dashed cyan arrow
> labelled "JSON events" going down. The five components left-to-right:
> "MicCapture" with a microphone icon, "RealtimeClient" with a small
> WebSocket icon, "GuideOrchestrator" (slightly larger, centred,
> emphasised), "StreamingAudioPlayer" with a small ring-buffer icon, and
> "GazeArtifactDetector" with a small eye icon. The GuideOrchestrator
> sits in the centre with thin gray connector lines to each of the four
> surrounding components.
>
> Third band: four flat rectangular engine modules in a row labelled
> "Unity HDRP 17.4", "OpenXR + XRI 3.0.8", "Unity AI Navigation",
> "SQLite (gilzoide)". Each shows a tiny version-string subtitle
> underneath. Thin light-gray arrows connect the application band down
> to these engine modules.
>
> Bottom band: a centred Meta Quest VR headset outline icon labelled
> "PCVR via Meta Quest Link" connecting upward with a cyan arrow into
> the OpenXR module above.
>
> Style: IEEE technical paper, sans-serif typography (Inter or
> Helvetica), dark navy blue (#1E3A8A) primary, cyan (#06B6D4) accents,
> light gray (#E5E7EB) backgrounds for the bands, white page
> background. No 3D, no shading, no decoration. 4K landscape, sharp
> vector lines, suitable for a thesis figure.

---

## Figure 2.1 — Comparison of Related Virtual Museum Systems

**Purpose**
Position this project against three published flagship VR museum
experiences to make the contribution clear at a glance.

**Diagram Type**
Comparison matrix (small-multiples / feature table), rendered as a clean
table-graphic rather than a literal data table.

**Visual Composition**
A 5-column × 7-row matrix:
- Columns: feature dimensions on the left, then four systems —
  Smithsonian "Skin and Bones", British Museum VR (Oculus
  collaboration), Louvre VR, **This Work**.
- Rows: 6 evaluation criteria, with column-headers row on top.

**Elements to Include**
Row headings (feature dimensions, exactly):
1. Immersive 3D environment
2. AI conversational guide (real-time)
3. Gaze-driven identification
4. Embodied 3D character guide
5. Visitor data persistence
6. Headset + non-headset path
7. Multilingual (single-config switch)

For each cell, a tasteful filled cyan dot for "supported", a hollow gray
ring for "not supported", and a half-filled cyan dot for "partial".

**Layout Structure**
Table rendered as a grid with thin gray cell borders. Column headers
band tinted very light gray. The "This Work" column has a faint dark
navy background to draw the eye. Logos / minimal pictograms for each
museum sit above each column header.

**Labels and Annotation Requirements**
- Below the matrix, a one-line caption: "Filled = supported; hollow =
  absent; half-filled = partial support".
- A small note tag on "This Work" column: "Egyptian Museum VR".

**Color Guidance**
Dark navy column headers, cyan support dots, light gray for non-
support. Single gold accent dot on "This Work" row for "Multilingual"
(the only system where it's a one-config switch).

**Camera / Viewpoint**
Pure 2D table graphic.

**Final AI Image Generation Prompt**
> A clean comparison matrix for a research thesis, IEEE academic style,
> white background. A 5-column by 7-row table. Top row is column
> headers: "Feature", "Smithsonian Skin and Bones", "British Museum
> VR", "Louvre VR", "This Work — Egyptian Museum VR". The leftmost
> column lists 7 feature names down: "Immersive 3D environment", "AI
> conversational guide (real-time)", "Gaze-driven identification",
> "Embodied 3D character guide", "Visitor data persistence", "Headset
> + non-headset path", "Multilingual (single-config switch)".
>
> Each remaining cell contains either: a filled cyan circle for
> "supported", a hollow gray ring for "not supported", or a half-filled
> cyan/gray circle for "partial". Pattern: most systems support
> "Immersive 3D environment"; only "This Work" supports the AI
> conversational guide, gaze-driven identification, headset-plus-non-
> headset, and multilingual switching; embodied character is partial
> for some; visitor data persistence is unique to "This Work".
>
> The "This Work" column has a subtle dark navy blue tinted background
> to highlight it. A single gold-coloured filled circle marks "This
> Work × Multilingual". Below the matrix is a small caption:
> "Filled circle = supported, hollow ring = absent, half-filled =
> partial support."
>
> Style: IEEE technical paper, sans-serif typography, dark navy blue
> (#1E3A8A) headers, cyan (#06B6D4) support markers, gold (#D4A373)
> accent on the multilingual cell, light gray (#E5E7EB) for non-
> support, white background, thin gray cell borders. 4K landscape,
> sharp vector lines, suitable for a thesis figure.

---

## Figure 4.1 — Project Phase Dependency Graph

**Purpose**
Show the seven-phase implementation plan as a directed dependency
graph, illustrating both serial dependencies and the parallel
"inert ML pipeline" that was implemented but never wired into the
active scene.

**Diagram Type**
Vertical directed acyclic graph (DAG) with sub-phases under Phase 6
and a branched-but-inert side track under Phase 3.

**Visual Composition**
Top-to-bottom flow. Each phase is a rounded rectangle. Phase 6
expands into sub-phases 6.1 through 6.6 in a small inner cluster.
Phase 7 splits into two build targets at the bottom.

**Elements to Include**
- **Phase 0** — Project Setup
- **Phase 1** — VR Foundation
- **Phase 2** — Locomotion + Interaction
- **Phase 3** — Gaze Identification (active path)
  - **Phase 3 ML (inert)** — ML Recognition Pipeline (sibling branch,
    rendered in faded gray with dashed border to mark "implemented but
    not wired in")
- **Phase 4** — Lobby + Persistence
- **Phase 5** — Real-Time Voice
- **Phase 6** — Tour Guide Embodiment
  - 6.1 NavMesh + Placeholder Body
  - 6.2 Distance-Keeping Behaviour
  - 6.3 Mixamo Wire-Up
  - 6.4 Walk Drift Fix (hip XZ lock)
  - 6.5 Materials + MaskMap
  - 6.6 Audio Embodiment
- **Phase 7** — Portable Production Build (VR)

**Layout Structure**
A central vertical spine of Phases 0 → 1 → 2 → 3 → 4 → 5 → 6 → 7.
Phase 3 has a side branch to "Phase 3 ML (inert)". Phase 6 is shown
expanded with a small inner box containing 6.1–6.6 in a 2×3 grid.

**Labels and Annotation Requirements**
- Solid arrows = "depends on / unlocks".
- Dashed arrow to "Phase 3 ML (inert)" labelled "implemented, not
  wired".
- Each phase node carries a one-line subtitle describing the
  verification gate (e.g. "verified: ≥ 72 fps + head tracking").

**Color Guidance**
Active spine: dark navy fill with white text. Inert ML branch: light
gray fill with dashed cyan border. Phase 6 inner cluster: cyan
borders.

**Camera / Viewpoint**
2D flat composition.

**Final AI Image Generation Prompt**
> A vertical directed acyclic graph showing the seven-phase
> implementation plan of a Unity VR project. IEEE academic style, white
> background, sans-serif typography.
>
> Central vertical spine of eight nodes top-to-bottom, each a rounded
> rectangle filled with dark navy blue (#1E3A8A) and white text:
> "Phase 0 — Project Setup", "Phase 1 — VR Foundation", "Phase 2 —
> Locomotion + Interaction", "Phase 3 — Gaze Identification (active)",
> "Phase 4 — Lobby + Persistence", "Phase 5 — Real-Time Voice", "Phase
> 6 — Tour Guide Embodiment", "Phase 7 — Portable Production Build (VR)".
> Solid dark navy arrows connect each phase to the next.
>
> From Phase 3, a horizontal dashed cyan arrow branches to the right to
> a sibling node "Phase 3 ML Pipeline (inert)" rendered in light gray
> with a dashed cyan border. The dashed arrow is labelled "implemented,
> not wired".
>
> Phase 6 is shown expanded: just below it sits a 2×3 grid of six
> smaller rounded rectangles with cyan borders labelled "6.1 NavMesh +
> Placeholder Body", "6.2 Distance-Keeping Behaviour", "6.3 Mixamo
> Wire-Up", "6.4 Hip XZ Lock", "6.5 Materials + MaskMap", "6.6 Audio
> Embodiment", connected to the Phase 6 node with a single bracket.
>
> Phase 7 is the terminal node rendered with a cyan accent stripe on
> the left edge: "Phase 7 — Portable Production Package (VR)",
> labelled "Build/MuseumVR-Production/MuseumVR.exe".
>
> Each phase node carries a tiny italic subtitle in light gray with
> its verification gate (e.g. "verified: ≥ 72 fps + head tracking",
> "verified: dwell fires in 1.5 s, no cross-room fires", "verified:
> WebSocket open and greeting received").
>
> Style: IEEE thesis figure, dark navy (#1E3A8A) primary, cyan
> (#06B6D4) accents, light gray (#E5E7EB) for the inert branch, white
> background, clean flat vector graphics, sharp lines, generous
> whitespace, 4K landscape composition.

---

## Figure 4.2 — Lobby Scene Composition Diagram

**Purpose**
Show the spatial layout of the lobby scene that the visitor first
enters — the dark stone backdrop with a hieroglyph back wall, two
flanking gold columns, the world-space form floating in front, and the
XR Origin (player) position.

**Diagram Type**
Semi-isometric VR scene diagram, with labelled callouts.

**Visual Composition**
Isometric view (≈ 30° elevation, 45° rotation). Floor as a tinted
plane. Back wall with a stylised hieroglyph pattern. Two side walls.
Two gold columns flanking the form, each with a base and capital. A
floating dark stone canvas in front showing the form layout.
A player avatar (head + body wireframe) standing at the XR Origin
position, facing the form.

**Elements to Include**
- Floor (20 × 20 m), tinted dark stone.
- Back wall (20 × 8 m) with stylised hieroglyph icon pattern.
- Left and right walls (short stubs, 5 m deep).
- Two gold columns at ±1.6 m on the X axis, 1.5 m in front of the
  back wall, each 3 m tall, with stylised square capital + base.
- World-space form canvas at (0, 1.55, 1.6) relative to the player.
  Show the form's contents: title "Welcome to the Egyptian Museum",
  subtitle, name input field, age slider, country dropdown showing
  "Egypt" selected, status text, golden "Begin Tour" button.
- Player avatar (simple wireframe) at the origin, facing the form.
- A small directional-light arrow above the scene indicating the warm
  key light.

**Layout Structure**
Scene fills the centre 70% of the canvas. Callout labels with thin
leader lines extend to the right and bottom of the scene, naming each
element. A small N/E/W/S compass and a 2 m scale bar sit in the
bottom-left.

**Labels and Annotation Requirements**
- "Floor — 20 × 20 m"
- "Back wall — 20 × 8 m, hieroglyph BaseColorMap, 2 × 1 tiling"
- "Left/Right Wall — solid stone"
- "Columns — gold (HDRP/Lit, metallic 0.7)"
- "World-space form canvas — 1.6 × 1.1 m"
- "XR Origin (visitor position)"
- "Directional Light + Fill Light"

**Color Guidance**
Dark stone walls. Cyan-tinted hieroglyph icons on the back wall (so
the back wall reads as the focal element). **Gold** for the two
columns and the "Begin Tour" button. Cream for canvas title text.

**Camera / Viewpoint**
Three-quarter isometric, slightly elevated, looking down and into the
scene.

**Final AI Image Generation Prompt**
> A semi-isometric architectural diagram of a virtual reality museum
> lobby scene, IEEE thesis figure style, white page background, clean
> flat vector graphics with subtle ambient occlusion.
>
> Viewpoint: three-quarter isometric, approximately 30 degrees
> elevation and 45 degrees rotation, looking down into the scene.
>
> Scene layout: a square floor 20 × 20 metres tinted dark stone gray
> (#1F1815). Behind the floor, a tall back wall 20 × 8 metres tinted
> warm stone (#3A2E22) with a subtle stylised pattern of Egyptian
> hieroglyph icons in cyan (#06B6D4) outlined, repeating across the
> surface as a flat texture. Two short side walls flank the floor.
>
> Two ornamental columns stand at the front of the scene, one on each
> side, approximately 3 metres tall, in matte gold (#D4A373) with
> square base plinths and square capitals. Each column has a small
> light-gray gradient suggesting metallic specular highlight.
>
> Floating in front of the columns at human eye height is a dark stone
> rectangular canvas, 1.6 metres wide by 1.1 metres tall, framed with a
> thin gold border. The canvas shows a form interface with the
> following stacked elements in cream text: a title "Welcome to the
> Egyptian Museum", a smaller italic subtitle "Tell us a little about
> yourself", labelled fields for Name (with placeholder "Your name"),
> Age (with a horizontal slider showing 25), and Country (with a
> dropdown showing "Egypt" selected), a small green status line
> "OpenAI key loaded", and a prominent gold rectangular button at the
> bottom labelled "Begin Tour" in dark text.
>
> In front of the canvas, slightly closer to the viewer, a simple
> wireframe outline of a human upper body indicates the XR Origin /
> player position, facing toward the form.
>
> Callout labels with thin gray leader lines on the right and bottom
> annotate each element: "Floor 20 × 20 m", "Back wall — hieroglyph
> BaseColorMap, 2 × 1 tiling", "Side walls — solid stone", "Columns —
> gold HDRP/Lit, metallic 0.7", "World-space canvas — 1.6 × 1.1 m",
> "XR Origin (visitor)", "Directional Light + Fill Light".
>
> A small N/E/W/S compass and a 2 m scale bar sit in the bottom-left
> corner.
>
> Style: clean academic technical diagram, dark navy (#1E3A8A) for
> labels and leader lines, cyan (#06B6D4) accents on the hieroglyph
> pattern, gold (#D4A373) for columns and the button, light gray
> (#E5E7EB) for floor accents, white page background. 4K landscape,
> sharp vector lines, no photorealism, suitable for a thesis figure.

---

## Figure 4.3 — Gaze Detection Ray Cast and Dwell Sequence

**Purpose**
Show the per-frame gaze detection mechanism: head-origin ray, the
2 m distance gate, the bounds-centre verification, the 1.5 s dwell
timer, and the 30 s per-artifact cooldown — as a single composite
figure combining a 3D inset and a state-machine sub-diagram.

**Diagram Type**
Two-panel composite: left = top-down/semi-iso 3D visualisation; right
= state machine.

**Visual Composition**
**Left panel (3D inset, ~60% width):** Visitor head silhouette in
profile, with a cyan ray extending forward. The ray crosses a 2 m
distance threshold marked as a dashed cyan arc. An artifact (a stylised
Pharaoh statue silhouette in gold) sits within the 2 m radius. A
bounding-box outline around the artifact has its centre marked with a
small target reticle. Around the artifact, a circular "dwell progress"
arc fills from 0 to 270° representing the 1.5 s timer. Above the
artifact, a small label balloon shows the dwell-progress fraction
"1.10 s / 1.50 s".

**Right panel (state machine, ~40% width):** A four-state diagram:
"No Target" → "Tracking" → "Identified" → "Cooldown (30 s)" → back to
"No Target". Each state is a rounded rectangle; arrows are labelled
with conditions: "ray hits ArtifactInfo & dist ≤ 2 m", "dwell ≥ 1.5 s",
"30 s elapsed".

**Elements to Include**
- Head silhouette / camera icon.
- Forward gaze ray (solid cyan, with arrow head).
- 2 m radial gate (dashed cyan arc).
- One artifact (Pharaoh silhouette in gold).
- Bounding-box outline around the artifact.
- Bounds-centre reticle.
- Circular dwell-progress arc.
- Dwell timer label "1.10 s / 1.50 s".
- Cooldown timer label below the artifact: "Cooldown: 0 / 30 s".
- Right-panel state machine with four nodes and labelled transitions.

**Layout Structure**
Two panels side by side, each in its own light-gray bordered card. The
3D inset's perspective is lightly isometric. The right state-machine
panel is purely 2D. A thin shared header strip above both panels reads
"Gaze Detection — 2 m / 1.5 s dwell / 30 s cooldown".

**Labels and Annotation Requirements**
- All numeric constants visible as small annotations.
- Each panel has a small title bar: "Spatial view" / "State machine".

**Color Guidance**
Cyan for the ray and timers. Dark navy for state-machine nodes and
labels. Gold for the artifact silhouette. Light gray for the bounding
box.

**Camera / Viewpoint**
Left: shallow isometric. Right: pure 2D.

**Final AI Image Generation Prompt**
> A two-panel composite technical figure illustrating the gaze-based
> artifact identification subsystem in a Unity VR project, IEEE
> academic style, white background, flat vector graphics, clean
> sans-serif typography.
>
> Shared header band across the top reads "Gaze Detection — 2 m
> distance, 1.5 s dwell, 30 s cooldown" in dark navy blue text.
>
> Left panel (60% width, "Spatial View"): a shallow-isometric scene.
> On the left, a stylised human head outline in profile facing right
> labelled "head / CenterEyeAnchor". A solid cyan arrow ray extends
> forward from the head, with an arrowhead. A dashed cyan radial arc
> crosses the ray at the 2 metre point, labelled "maxDistance = 2 m".
> Beyond the dashed arc but within the ray's reach, a stylised
> Egyptian Pharaoh statue silhouette rendered in gold (#D4A373)
> represents an artifact. Around the artifact, a thin light-gray
> rectangular bounding-box outline is drawn with a small cyan reticle
> marking the bounds-centre, labelled "bounds-centre check".
> Surrounding the artifact, a partially-filled circular arc (currently
> 270 degrees of cyan, the remainder light gray) represents the dwell
> progress, with a small floating label balloon reading "1.10 s /
> 1.50 s". Below the artifact, a smaller progress bar labelled
> "Cooldown: 0 / 30 s".
>
> Right panel (40% width, "State Machine"): four rounded-rectangle
> states arranged in a horizontal loop with directional arrows:
> "No Target" → "Tracking" → "Identified" → "Cooldown (30 s)" → back
> to "No Target". Transition arrows are labelled in small italic text:
> "ray hits ArtifactInfo and dist ≤ 2 m", "dwell ≥ 1.5 s", "30 s
> elapsed", "look away / lost". States are filled dark navy blue with
> white text.
>
> Both panels are framed in light-gray bordered cards with subtle drop
> shadows.
>
> Style: IEEE thesis figure, dark navy blue (#1E3A8A) for primary
> elements, cyan (#06B6D4) for rays, timers, and the dashed range gate,
> gold (#D4A373) only for the artifact silhouette, light gray (#E5E7EB)
> for the bounding box and incomplete progress, white background. 4K
> landscape, sharp vector lines, clean academic diagram, suitable for
> thesis printing.

---

## Figure 4.4 — Audio Streaming Pipeline

**Purpose**
Show the end-to-end real-time speech path: the visitor's microphone
through the Unity client, across the WebSocket to the OpenAI Realtime
API, and back through the streaming audio player to the avatar's
head-bone AudioSource.

**Diagram Type**
Horizontal left-to-right engineering pipeline diagram with two
parallel flows (upstream / downstream) and annotated latency at each
stage.

**Visual Composition**
A single long horizontal flow. Top track = upstream (mic → API).
Bottom track = downstream (API → ear). The two tracks share an OpenAI
Realtime API box on the right end and a "visitor" silhouette on the
left end. Between every two stages, a small latency annotation in
italic text.

**Elements to Include (upstream, left to right)**
1. Visitor head silhouette + microphone icon.
2. `MicCapture` block: "Unity Microphone → resample to 24 kHz → PCM16
   chunk (40 ms, 960 samples)".
3. Base64 encoder block.
4. `RealtimeClient` (WebSocket out).
5. **OpenAI Realtime API** centred cloud (shared with downstream).

**Elements to Include (downstream, right to left from API)**
6. `RealtimeClient` (WebSocket in).
7. Base64 decoder block.
8. `StreamingAudioPlayer` block expanded to show: 30 s ring buffer
   (visualised as a circular buffer icon) + 200 ms jitter prebuffer.
9. `AudioSource` attached to the head bone of the Mixamo avatar
   silhouette.
10. Visitor's ear icon.

**Layout Structure**
Two horizontal rails. Upstream rail tinted very light blue; downstream
rail tinted very light cyan. The OpenAI cloud sits at the right merging
both. The visitor on the left is the source and destination.

**Labels and Annotation Requirements**
- Between stages on the upstream rail: "40 ms (chunk)", "~5 ms (base64)",
  "100–300 ms (WSS + RTT)".
- Between stages on the downstream rail: "400–800 ms (first-token)",
  "100–200 ms (audio gen start)", "100–300 ms (return)", "200 ms
  (prebuffer)", "≈ 0 (playback)".
- A bold total annotation under the figure: "End-to-end first-word
  latency ≈ 1.0 – 1.9 s".

**Color Guidance**
Cyan arrows for audio data. Dark navy for component boxes. Light cyan
tint for the downstream rail to differentiate. Small gold dot inside
the avatar head-bone icon (Egyptian-cultural element).

**Camera / Viewpoint**
Pure 2D flat.

**Final AI Image Generation Prompt**
> A horizontal left-to-right real-time audio streaming pipeline
> diagram for a VR application, IEEE academic style, white background,
> flat vector graphics, sans-serif typography.
>
> The figure shows two parallel horizontal rails sharing endpoints on
> the far left (visitor) and far right (OpenAI Realtime API cloud).
> The upper rail is tinted very light blue and represents the upstream
> (microphone → API). The lower rail is tinted very light cyan and
> represents the downstream (API → ear).
>
> On the far left, a stylised visitor head silhouette with a small
> microphone icon labelled "Visitor". On the far right, a large
> rounded-rectangle cloud labelled "OpenAI Realtime API
> (gpt-4o-mini-realtime-preview)" with a small lightning icon
> indicating real-time.
>
> Upstream rail (top, left to right) shows the following components as
> rounded dark-navy rectangles with white text, connected by cyan
> arrows: "MicCapture: Microphone → resample → 24 kHz PCM16 (40 ms
> chunks)", "Base64 Encoder", "RealtimeClient (WebSocket send)". Below
> each arrow on this rail is a small italic gray latency label:
> "40 ms (chunk)", "≈ 5 ms (base64)", "100–300 ms (WSS + RTT)".
>
> Downstream rail (bottom, right to left from the API cloud) shows:
> "RealtimeClient (WebSocket recv)", "Base64 Decoder",
> "StreamingAudioPlayer — 30 s ring buffer + 200 ms prebuffer"
> (drawn with a small circular ring-buffer icon inside the box),
> "AudioSource (3D, head bone)" with a small Mixamo Pharaoh head
> silhouette and a tiny gold dot inside indicating the head-bone
> anchor, ending at the visitor's ear icon on the left.
>
> Latency labels on the downstream rail (italic, gray):
> "400–800 ms (first-token)", "100–200 ms (audio gen start)",
> "100–300 ms (return)", "200 ms (prebuffer)", "≈ 0 (playback)".
>
> Below the figure, a centred bold dark-navy annotation: "End-to-end
> first-word latency ≈ 1.0 – 1.9 s".
>
> Style: IEEE thesis figure, dark navy blue (#1E3A8A) primary boxes,
> cyan (#06B6D4) arrows and accents, light blue (#DBEAFE) tint for
> upstream rail, light cyan (#CFFAFE) tint for downstream rail, gold
> (#D4A373) only as a tiny accent inside the head-bone AudioSource,
> light gray (#E5E7EB) for latency text, white background. 4K wide
> landscape composition, sharp vector lines, clean engineering
> aesthetic.

---

## Figure 4.5 — NavMesh and Tour Guide Following Behaviour

**Purpose**
Show how the NavMeshAgent-driven tour guide maintains a fixed 1.8 m
distance from the visitor: a top-down view of the museum NavMesh, the
agent's desired-position computation, and the active-back-up behaviour
when the visitor approaches.

**Diagram Type**
Top-down floor plan + behaviour sub-sequence.

**Visual Composition**
Large top-down view of a stylised museum hall (rectangular plan with
two side galleries), with the walkable NavMesh shown as a translucent
cyan polygon mesh covering the floor. Three "frames" of the visitor /
guide are overlaid as small icons showing the three behaviour cases:
(A) visitor far, guide approaches; (B) visitor close, guide stops at
desired position; (C) visitor walks forward, guide backs up.

**Elements to Include**
- Stylised top-down museum floor plan (rectangle with two side wings).
- NavMesh polygon overlay in semi-transparent cyan.
- Three pairs of icons, each pair = (visitor head icon + Mixamo
  Pharaoh icon), each annotated with the case letter (A, B, C).
- For each pair: a thin gray line from visitor to guide labelled with
  the current distance; a dashed circle at radius 1.8 m around the
  visitor (the "desired distance"); a small arrow on the guide showing
  the path direction.
- Legend strip below: "(A) Pursuit — d > 1.8 m + 0.4 m hysteresis";
  "(B) Stopped — d ≈ 1.8 m"; "(C) Back-up — d < 1.8 m".

**Layout Structure**
Floor plan occupies the upper 70%. The three case icons are arranged
left-to-right within the plan. The bottom 30% holds the legend strip
and a small inset showing the desired-position formula:
`desiredPos = player.pos + normalize(agent.pos − player.pos) × 1.8 m`.

**Labels and Annotation Requirements**
- NavMesh polygon labelled "NavMesh (baked)".
- Each case annotated with its distance.
- Formula inset labelled "Desired position computation".

**Color Guidance**
Cyan for NavMesh and the desired-distance circle. Dark navy for the
visitor icon. Gold for the Pharaoh guide icon. Light gray for floor
plan walls.

**Camera / Viewpoint**
Pure top-down 2D.

**Final AI Image Generation Prompt**
> A top-down floor plan diagram of a virtual museum showing
> NavMesh-driven tour guide following behaviour, IEEE academic style,
> white background, clean flat vector graphics.
>
> Upper 70%: a stylised rectangular museum hall floor plan, with two
> side gallery extensions, walls drawn as thin light-gray rectangles.
> Across the walkable floor, a translucent cyan (#06B6D4 at 30%
> opacity) polygon mesh overlay represents the baked NavMesh, made of
> visible triangle/quad polygons. A small label "NavMesh (baked)"
> with a leader line in the centre of the floor.
>
> Three behaviour cases are illustrated by three pairs of icons placed
> at different locations within the floor plan, each annotated with a
> circled letter "A", "B", "C":
>
> Case A (left side, labelled "Pursuit"): a small dark navy visitor
> head icon and a gold-coloured stylised Pharaoh figurine icon
> separated by about 3.5 metres of distance, with a thin gray line
> between them labelled "d = 3.5 m". A dashed cyan circle of radius
> 1.8 m surrounds the visitor. A small cyan arrow on the guide points
> toward the visitor along the NavMesh.
>
> Case B (centre, labelled "Stopped"): visitor icon and Pharaoh icon
> separated by approximately 1.8 m, with the Pharaoh icon sitting right
> on the dashed cyan circle. The line between them is labelled
> "d ≈ 1.8 m". No motion arrow on the guide.
>
> Case C (right side, labelled "Back-up"): visitor icon and Pharaoh
> icon separated by approximately 1.2 m, with the Pharaoh icon inside
> the dashed cyan circle. A cyan arrow on the guide points AWAY from
> the visitor. Distance label "d = 1.2 m, retreating".
>
> Bottom 30%: a horizontal legend strip with three coloured labels
> matching A, B, C and brief descriptions. To the right of the legend,
> a small inset card with a monospace formula in dark navy:
> "desiredPos = player.pos + normalize(agent.pos − player.pos) × 1.8 m",
> labelled "Desired position computation".
>
> Style: IEEE thesis figure, dark navy blue (#1E3A8A) for visitor
> icons and labels, gold (#D4A373) for the Pharaoh guide icons, cyan
> (#06B6D4) for the NavMesh overlay and desired-distance circle, light
> gray (#E5E7EB) for walls and connecting lines, white background. 4K
> landscape composition, sharp vector lines, clean engineering
> aesthetic.

---

## Figure 4.6 — Mixamo Avatar Hierarchy and Bone XZ-Lock Fix

**Purpose**
Show the Mixamo humanoid skeleton hierarchy and visualise the
runtime hip-bone XZ-lock fix: a "before" panel with the hip drifting
forward during the walk loop and snapping back at loop end, and an
"after" panel where the hip's local X and Z are pinned to their rest
values while Y remains free for the natural walk bobble.

**Diagram Type**
Two side-by-side panels (BEFORE / AFTER), each combining a humanoid
skeleton wireframe and a small time-vs-position graph.

**Visual Composition**
**Left panel — BEFORE (broken)**: A humanoid skeleton in T-pose
outline, with the hip bone highlighted in red. An arrow on the hip
indicates forward drift. A small inset graph shows hip-local-Z as a
sawtooth wave (drift, snap, drift, snap).

**Right panel — AFTER (fixed)**: Same skeleton outline, hip
highlighted in green. An overlay icon shows a lock/pin. The inset
graph shows hip-local-Z as a flat line (zero); hip-local-Y still
oscillates as a smooth sine wave (the walk bobble), drawn separately.

**Elements to Include**
- Two humanoid skeletons (front view), with the same bone labels
  visible: Hips, Spine, Spine1, Spine2, Neck, Head, plus arms and
  legs. Hip bone enlarged and tagged.
- Red arrow on the BEFORE hip indicating "drift +Z then snap".
- Lock icon on the AFTER hip; a small label "LateUpdate: snap X, Z to
  rest".
- Two small inset graphs: BEFORE shows sawtooth Z, AFTER shows flat Z
  and oscillating Y.

**Layout Structure**
Equal-width panels, separated by a vertical light-gray divider with a
centred arrow label "Runtime LateUpdate fix". Skeletons centred in
each panel, graphs in the lower portion of each.

**Labels and Annotation Requirements**
- "Hip bone — mixamorig:Hips"
- "BEFORE: animation drifts hip local XZ each frame → visual snap-back
  at loop end"
- "AFTER: TourGuideAgent.LateUpdate re-snaps hip local X and Z to rest
  pose; Y is free for natural bobble"

**Color Guidance**
Red accent for BEFORE problem indicator. Green accent for AFTER fix
indicator. Dark navy for skeleton lines. Cyan for the lock icon and
graph trace.

**Camera / Viewpoint**
2D frontal view of each skeleton.

**Final AI Image Generation Prompt**
> A two-panel before/after technical figure showing a Mixamo humanoid
> skeleton and a runtime bone-locking fix, IEEE academic thesis style,
> white background, flat vector graphics.
>
> A thin vertical divider runs down the centre of the figure with a
> horizontal arrow across it labelled "Runtime LateUpdate fix".
>
> Left panel header in bold dark navy: "BEFORE — Mixamo walk drift".
> Inside, a clean front-view humanoid skeleton wireframe drawn with
> connected line segments and circular joint markers, labelled at each
> major bone: Head, Neck, Spine2, Spine1, Spine, mixamorig:Hips (with
> a small enlarged callout box), Left/Right Upper Arm, Left/Right
> Forearm, Left/Right Hand, Left/Right Upper Leg, Left/Right Lower
> Leg. The hip joint marker is enlarged and highlighted with a red
> outline. From the hip, a curved red arrow points forward (+Z),
> labelled "drifts forward each frame → snaps back at loop end".
>
> Below the skeleton in the left panel, a small inset graph titled
> "Hip local Z over time" shows a sawtooth wave: rising linearly then
> dropping vertically, repeating. X-axis is "time", Y-axis is "Z
> offset".
>
> Right panel header in bold dark navy: "AFTER — Hip XZ Lock". Same
> skeleton wireframe, with the hip joint marker now outlined in green.
> A small lock-icon overlay sits on the hip. A label above the hip
> reads "LateUpdate: hip.localPosition.x/z = rest".
>
> Below the skeleton in the right panel, two overlaid graphs share an
> axis labelled "time": a flat horizontal line (Z, zero) in green, and
> a smooth sine wave in cyan (Y bobble, ≈ 5 cm peak-to-peak), labelled
> "Z locked", "Y free (walk bobble)".
>
> Style: IEEE thesis figure, dark navy blue (#1E3A8A) for skeleton
> wireframes and labels, red (#DC2626) accent only for the BEFORE
> problem indicator, green (#16A34A) accent only for the AFTER fix
> indicator, cyan (#06B6D4) for the Y-bobble trace and lock icon,
> light gray (#E5E7EB) for graph gridlines and divider, white
> background. 4K landscape, sharp vector lines, clean engineering
> aesthetic.

---

## Figure 4.7 — Scene-Transition Lifecycle

**Purpose**
Show the lifecycle from "Lobby loaded" through "Begin Tour pressed"
to "Museum loaded", emphasising that `SessionState` (DontDestroyOnLoad)
persists across the swap and explaining why single-mode loading was
chosen over additive (which momentarily instantiated two XR Origins).

**Diagram Type**
Horizontal timeline / swim-lane diagram with two lanes:
"In-Scene Objects" and "DontDestroyOnLoad Layer".

**Visual Composition**
Three time columns left-to-right: **t₀ Lobby loaded**, **t₁ Submit
pressed**, **t₂ Museum loaded**.

Two horizontal lanes:
- **In-Scene Objects** (top): shows what GameObjects are alive at each
  stage.
- **DontDestroyOnLoad Layer** (bottom): shows `SessionState` and
  `MuseumDatabase` persisting across the whole timeline as a single
  unbroken bar.

A side-by-side comparison sub-panel on the right shows the **rejected
additive design** with two simultaneous XR Origins overlaid (annotated
"controllers vanish, white render streak").

**Elements to Include**
**t₀ — Lobby loaded**:
- Lobby Canvas + LobbyController.
- XR Origin (lobby).
- EventSystem.

**t₁ — Submit pressed**:
- SQLite insert (visualised as a database-icon flash).
- Visitor record handed to SessionState.

**t₂ — Museum loaded**:
- Museum environment, Tour Guide GameObject, Mixamo avatar, NavMesh.
- XR Origin (museum).

**DontDestroyOnLoad lane** (full width):
- SessionState (Visitor + OpenAI key).
- MuseumDatabase (SQLite connection).

**Right-side rejected-design panel**:
- "Additive load (rejected)" with two overlaid XR Origins, red ⚠ icon,
  caption "1 frame collision → broken".

**Layout Structure**
Main timeline occupies left 75%, rejected-design panel right 25%, with
a faint vertical divider.

**Labels and Annotation Requirements**
- "LoadSceneMode.Single" arrow between t₁ and t₂.
- "DontDestroyOnLoad" persistent bar across the bottom.
- Red ⚠ on the rejected design.

**Color Guidance**
Dark navy for active scene objects. Cyan for the persistent
DontDestroyOnLoad bar. Red ⚠ accent only on the rejected design.

**Camera / Viewpoint**
2D timeline / swim-lane.

**Final AI Image Generation Prompt**
> A horizontal swim-lane lifecycle diagram showing scene transition in
> a Unity VR application, IEEE academic style, white background, flat
> vector graphics, sans-serif typography.
>
> The main composition occupies the left 75% and consists of three
> labelled time columns separated by vertical gridlines: "t₀ — Lobby
> loaded", "t₁ — Submit pressed", "t₂ — Museum loaded". Two horizontal
> swim lanes span the columns: the upper lane labelled "In-Scene
> Objects" (light-gray background), the lower lane labelled
> "DontDestroyOnLoad Layer" (light-cyan background).
>
> Upper lane content:
> - In the t₀ column, three rounded dark navy boxes stacked: "Lobby
>   Canvas + LobbyController", "XR Origin (lobby)", "EventSystem".
> - In the t₁ column, a flash icon labelled "SQLite Insert" and an
>   arrow pointing down toward the lower lane labelled "Visitor record
>   → SessionState".
> - In the t₂ column, four rounded dark navy boxes: "Museum
>   Environment", "Tour Guide (GuideOrchestrator)", "Mixamo Avatar",
>   "NavMesh + Bake", and a separate "XR Origin (museum)" box.
>
> Between t₁ and t₂, a large cyan arrow spans both lanes labelled
> "SceneManager.LoadSceneAsync(LoadSceneMode.Single)".
>
> Lower lane content: a single unbroken horizontal cyan bar spanning
> all three columns labelled "SessionState (Visitor + OpenAI key) —
> DontDestroyOnLoad", and a parallel narrower bar labelled
> "MuseumDatabase (SQLite connection)".
>
> Right 25% panel (separated by a faint vertical divider) titled
> "Rejected: Additive Load": two overlapping XR Origin icons rendered
> in red outline, with a large red warning triangle "⚠" between them
> and a caption underneath in italic red text reading "1 frame
> collision — duplicate cameras, white render streak, controllers
> double-bound".
>
> Style: IEEE thesis figure, dark navy blue (#1E3A8A) for primary
> scene-object boxes and labels, cyan (#06B6D4) for the
> DontDestroyOnLoad bar and the LoadSceneMode arrow, red (#DC2626)
> only inside the rejected-design panel, light gray (#E5E7EB) for
> swim-lane backgrounds and gridlines, white page background. 4K
> landscape composition, sharp vector lines, clean academic aesthetic.

---

## Figure 5.1 — Frame-Time Profile During a Typical Tour

**Purpose**
Engineering chart showing CPU and GPU frame time over the duration of
a representative tour, against the 13.9 ms target line (72 fps), with
annotated spikes at key gameplay events.

**Diagram Type**
Multi-series line chart (engineering plot).

**Visual Composition**
A standard scientific line plot. X-axis: time in minutes (0 to 10).
Y-axis: frame time in milliseconds (0 to 20). Two traces: CPU in cyan,
GPU in dark navy. A horizontal red dashed reference line at 13.9 ms
labelled "72 fps target". Subtle gridlines. A small "Average: …" stats
box in the upper-right corner.

**Elements to Include**
- Title: "Frame-time profile during a 10-minute representative tour".
- Two line traces with clear legend.
- Reference line at 13.9 ms.
- Annotated spike markers with leader lines and labels:
  - 2:15 — "Artifact identification (label spawn + narration start)"
  - 4:30 — "Tour guide entering room (NavMesh repath)"
  - 7:50 — "Multiple artifacts in view (frustum culling spike)"
- Stats box: "Avg CPU: 9.8 ms | Avg GPU: 8.4 ms | 99th-pct: 13.1 ms".
  *(placeholder numbers — the student fills in real measurements.)*

**Layout Structure**
Standard plot framing: axes on left and bottom, title centred above.
Legend in upper-left interior. Stats in upper-right interior.

**Labels and Annotation Requirements**
- "Time (minutes)" — X-axis label.
- "Frame time (ms)" — Y-axis label.
- Gridlines at every 2.5 ms vertical and every 1 minute horizontal.

**Color Guidance**
Cyan (#06B6D4) CPU line, dark navy (#1E3A8A) GPU line, red (#DC2626)
dashed reference line. Light gray (#E5E7EB) gridlines and annotation
leader lines.

**Camera / Viewpoint**
Pure 2D chart.

**Final AI Image Generation Prompt**
> A scientific engineering line chart showing frame-time performance
> of a VR application over a representative tour, IEEE academic
> style, white background, sans-serif typography.
>
> The chart title is centred above: "Frame-time profile during a
> 10-minute representative tour". X-axis labelled "Time (minutes)"
> ranges 0 to 10 with major ticks every 1 minute. Y-axis labelled
> "Frame time (ms)" ranges 0 to 20 with major ticks every 5 ms and
> minor ticks every 2.5 ms.
>
> A horizontal red dashed reference line crosses the plot at 13.9 ms,
> labelled in red italic text at its right end: "72 fps target —
> 13.9 ms".
>
> Two line traces:
> - Cyan (#06B6D4) line labelled "CPU frame time" — varies between
>   approximately 7 and 11 ms, with occasional brief spikes to 12–13
>   ms.
> - Dark navy (#1E3A8A) line labelled "GPU frame time" — varies
>   between approximately 6 and 9 ms, with smaller spikes.
>
> Three annotated spike markers with thin gray leader lines pointing
> from the trace to a small caption box:
> - At t = 2:15, a CPU spike marker pointing to caption "Artifact
>   identification (label spawn + narration start)".
> - At t = 4:30, a combined CPU + GPU spike marker pointing to
>   caption "Tour guide entering room (NavMesh repath)".
> - At t = 7:50, a GPU-only spike marker pointing to caption
>   "Multiple artifacts in view (frustum culling spike)".
>
> Legend in upper-left interior with two coloured swatches and trace
> names. Stats box in upper-right interior with three lines in small
> dark gray monospace: "Avg CPU: 9.8 ms", "Avg GPU: 8.4 ms",
> "99th percentile: 13.1 ms".
>
> Light-gray gridlines at every major tick. Plot background is white
> with a thin dark-navy axis frame.
>
> Style: IEEE thesis chart, clean publication-quality scientific plot,
> sharp vector lines, no shading inside the lines, cyan and dark navy
> traces, red reference line, light gray (#E5E7EB) gridlines and
> annotation leaders, white background. 4K landscape composition,
> suitable for thesis printing.

---

## Figure 5.2 — Audio Latency Breakdown

**Purpose**
Visualise the contribution of each pipeline stage to end-to-end audio
latency, making clear that the dominant cost is the OpenAI model's
first-token latency.

**Diagram Type**
Horizontal stacked-bar (waterfall-style) chart with one bar for
"upstream" and one for "downstream", plus a total combined bar.

**Visual Composition**
Three horizontal stacked bars, top to bottom:
1. **Upstream** (mic → API): three segments — chunk assembly (40 ms),
   base64 + serialise (5 ms), WSS send + RTT (mid 200 ms).
2. **Downstream** (API → ear): five segments — model first-token
   (~600 ms), audio gen start (~150 ms), network return (~200 ms),
   prebuffer (200 ms), playback (~0).
3. **Total end-to-end**: a single bar showing the full ≈ 1.0–1.9 s
   range, with a marker for the typical 1.4 s mid-value.

Each segment is colour-coded and labelled inside or above the segment.

**Elements to Include**
- Y-axis: pipeline category labels.
- X-axis: time in milliseconds, 0 to 2000.
- Segment labels with stage name and contribution.
- Small swatch legend below.
- Annotation arrow pointing at the "Model first-token" segment with
  text "dominant cost — outside the project's control".

**Layout Structure**
Three full-width bars stacked vertically with consistent height.
X-axis with major ticks every 200 ms.

**Labels and Annotation Requirements**
- Each segment shows e.g. "Chunk 40 ms", "Base64 5 ms", "WSS RTT
  200 ms", "Model first-token 600 ms", "Audio gen 150 ms", "Return
  200 ms", "Prebuffer 200 ms", "Playback ≈ 0".
- Total bar's right end shows "≈ 1.0 – 1.9 s".

**Color Guidance**
A cyan-to-navy gradient palette across the segments, with the OpenAI
first-token segment in the deepest navy to make it visually dominant.

**Camera / Viewpoint**
Pure 2D chart.

**Final AI Image Generation Prompt**
> A horizontal stacked-bar latency breakdown chart for a real-time
> audio streaming pipeline, IEEE academic style, white background,
> sans-serif typography.
>
> Title centred above: "Audio latency breakdown — per-stage
> contribution to end-to-end first-word latency".
>
> X-axis labelled "Time (milliseconds)" ranges 0 to 2000 with major
> ticks every 200 ms.
>
> Three horizontal bars are stacked vertically with consistent heights
> and clear gaps between them. Each bar has its category name on the
> left:
>
> Top bar — "Upstream (mic → API)" — total length about 305 ms.
> Composed left-to-right of three coloured segments with their
> in-segment labels: a light cyan segment labelled "Chunk assembly
> 40 ms"; a slightly darker cyan segment labelled "Base64 +
> serialise 5 ms"; a medium cyan segment labelled "WebSocket send
> + RTT 100–300 ms".
>
> Middle bar — "Downstream (API → ear)" — total length about 1150 ms.
> Composed of five segments: a deep dark navy segment labelled "Model
> first-token 400–800 ms" (the longest single segment, drawn at a
> nominal 600 ms); a medium navy segment "Audio gen start 100–200 ms";
> a cyan segment "Network return 100–300 ms"; a darker cyan segment
> "Prebuffer 200 ms"; a thin segment "Playback ≈ 0".
>
> Bottom bar — "Total end-to-end" — drawn as a single full-width bar
> from 0 to a range marker, with a faint cyan-to-navy gradient fill
> and dashed range bracket at the right end labelled "≈ 1.0 – 1.9 s
> (typical ≈ 1.4 s)". A small vertical marker line drawn at 1.4 s
> labelled "typical".
>
> A curved annotation arrow points from a side note at the top-right
> to the "Model first-token" segment in the middle bar, with the
> annotation text "Dominant cost — outside the project's control
> (OpenAI inference)".
>
> Small swatch legend below the chart shows the four colour tints
> (cyan, deep cyan, navy, deep navy) with descriptions: "Client
> processing", "Network transport", "Model inference", "Audio
> generation + buffering".
>
> Style: IEEE thesis chart, dark navy (#1E3A8A) deepest segment, cyan
> (#06B6D4) lighter segments, white background, light gray (#E5E7EB)
> gridlines and axis frame, sharp vector lines, clean engineering
> aesthetic. 4K landscape composition, suitable for thesis printing.

---

## Figure 6.1 — Business Model Canvas

**Purpose**
Standard 9-block Business Model Canvas filled with the project's
strategy: customer segments (museums, schools, tourism boards,
accessibility), value proposition (AI-guided VR tours), channels,
revenue streams, cost structure, key partners (OpenAI, Mixamo,
Unity, Meta, Khronos), key activities, key resources.

**Diagram Type**
Classic 9-block Osterwalder Business Model Canvas.

**Visual Composition**
Standard canonical layout:

```
+---------------+---------------+--------------+---------------+----------------+
| KEY PARTNERS  | KEY           | VALUE        | CUSTOMER      | CUSTOMER       |
|               | ACTIVITIES    | PROPOSITION  | RELATIONSHIPS | SEGMENTS       |
|               +---------------+              +---------------+                |
|               | KEY RESOURCES |              | CHANNELS      |                |
+---------------+---------------+--------------+---------------+----------------+
| COST STRUCTURE                              | REVENUE STREAMS                 |
+---------------------------------------------+---------------------------------+
```

**Elements to Include (block by block)**

- **Key Partners**: OpenAI (Realtime API), Mixamo / Adobe, Unity
  Technologies, Meta (Quest hardware), Khronos (OpenXR), Unity Asset
  Store vendors.
- **Key Activities**: Software maintenance, per-museum customisation,
  artifact catalogue curation, build / release engineering, API cost
  monitoring.
- **Key Resources**: ~5 k lines of C# code, 42-artifact catalogue,
  domain expertise, Unity Pro licence, build server.
- **Value Proposition**: "Adaptive, spoken AI-guided VR tours of
  cultural heritage spaces; ~$0.10 / visit; multilingual by config;
  USB-portable demos."
- **Customer Relationships**: Per-museum installation contracts,
  annual maintenance subscriptions, free demo builds.
- **Customer Segments**: Museums, schools and universities, tourism
  boards, cultural-heritage NGOs, accessibility advocates, researchers.
- **Channels**: Direct B2B museum sales, Steam / Meta Store, academic
  licensing, NGO partnerships.
- **Cost Structure**: Software development salaries, OpenAI API
  consumption (variable), hosting, demo hardware, asset acquisition.
- **Revenue Streams**: One-time installation fee, annual licence,
  per-visit revenue share, consumer marketplace sales, educational
  institutional licences.

**Layout Structure**
Strict canonical Osterwalder grid. Each block has a small icon in its
top-left corner and a header in dark navy, with bullet content
beneath.

**Labels and Annotation Requirements**
- Each block: header in caps, content in concise bulleted lines.
- A small footer strip: "Business Model Canvas — Egyptian Museum VR".

**Color Guidance**
Dark navy headers, cyan icon accents, light-gray block backgrounds,
white grid lines. A single gold accent on the "Value Proposition"
block to indicate the cultural-heritage core.

**Camera / Viewpoint**
Pure 2D canvas grid.

**Final AI Image Generation Prompt**
> A standard 9-block Business Model Canvas diagram for a VR cultural-
> heritage product, IEEE / startup hybrid academic style, white
> background, clean flat vector graphics, sans-serif typography.
>
> The canvas uses the canonical Osterwalder layout: an upper row of
> five vertical columns and a lower row of two wide blocks. Top row,
> left to right: "KEY PARTNERS", a column split in two stacked blocks
> ("KEY ACTIVITIES" top, "KEY RESOURCES" bottom), "VALUE PROPOSITION"
> centre tall block, another column split in two ("CUSTOMER
> RELATIONSHIPS" top, "CHANNELS" bottom), "CUSTOMER SEGMENTS" right
> tall block. Bottom row: "COST STRUCTURE" left, "REVENUE STREAMS"
> right.
>
> Each block has a small cyan icon in its top-left corner: a
> handshake for partners, gear for activities, cube for resources, a
> light-bulb for value, chat-bubble for relationships, megaphone for
> channels, people-group for segments, downward-arrow coin for cost,
> upward-arrow coin for revenue.
>
> Block contents (small bullet text in dark gray):
>
> KEY PARTNERS: OpenAI Realtime API; Mixamo / Adobe; Unity
> Technologies; Meta (Quest hardware); Khronos (OpenXR); Asset Store
> vendors.
>
> KEY ACTIVITIES: Software maintenance; per-museum customisation;
> artifact catalogue curation; build / release engineering; API cost
> monitoring.
>
> KEY RESOURCES: ~5 000 lines C#; 42-artifact catalogue; domain
> expertise; Unity Pro licence; build server.
>
> VALUE PROPOSITION (centre, slightly larger and with a gold
> (#D4A373) thin left border accent for cultural-heritage emphasis):
> "Adaptive, spoken AI-guided VR tours of cultural heritage spaces.
> ~$0.10 / visit API cost. Multilingual by config. USB-portable
> production builds for Meta Quest via PCVR."
>
> CUSTOMER RELATIONSHIPS: Per-museum installation contracts; annual
> maintenance subscriptions; free demo builds.
>
> CHANNELS: Direct B2B museum sales; Steam / Meta Store; academic
> licensing; NGO partnerships.
>
> CUSTOMER SEGMENTS: Museums; schools and universities; tourism
> boards; cultural-heritage NGOs; accessibility advocates;
> researchers.
>
> COST STRUCTURE: Software development salaries; OpenAI API
> consumption (variable); hosting; demo hardware; asset acquisition.
>
> REVENUE STREAMS: One-time installation fee; annual licence;
> per-visit revenue share; consumer marketplace sales; educational
> institutional licences.
>
> A small bottom footer strip reads "Business Model Canvas — Egyptian
> Museum VR" in centred dark-navy italic.
>
> Style: IEEE / startup hybrid clean canvas, dark navy blue (#1E3A8A)
> block headers, cyan (#06B6D4) icon accents, gold (#D4A373) thin
> accent stripe only on the Value Proposition block, light gray
> (#E5E7EB) block fills and grid lines, white background. 4K
> landscape composition, sharp vector lines, suitable for thesis
> printing.

---

**End of figure prompts.**

> Quick tips for executing these in ChatGPT image generation:
> 1. Submit each prompt as a separate generation request so each
>    figure gets the model's full attention.
> 2. After receiving the first figure, briefly reuse the produced
>    visual style in the next prompt's leading sentence (e.g. "in the
>    same flat IEEE thesis vector style as the previous image") to
>    keep the twelve figures visually cohesive.
> 3. If an image generator struggles with embedded text, set the
>    style to "infographic with placeholder text" and overlay real
>    text afterward in PowerPoint, Word, or draw.io.
> 4. For Figures 5.1 and 5.2 you may get a cleaner result rendering
>    the chart in matplotlib / Excel / draw.io directly and using
>    these prompts only as a design specification rather than an
>    image input.
