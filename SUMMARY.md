# Museum VR — Session Summary

## What it is

Unity 6 / HDRP VR Egyptian Museum for Meta Quest via PCVR. Player wears the headset, fills out a lobby form (name / age / country), walks through the museum, gazes at any of 42 tagged artifacts within 2 m for 1.5 s — a label appears with name + era, and the OpenAI Realtime API tour guide narrates. Visitor holds **T** to ask the guide a question (push-to-talk).

## Built and shipped

- **Phase 0 — Project setup**: HDRP VR settings (no raytracing/SSR/SSGI/volumetric clouds, Forward path), New Input System, all packages installed (XR Interaction Toolkit, OpenXR, XR Plugin Management, Inference Engine, AI Navigation, Newtonsoft.Json, gilzoide/sqlite-net via OpenUPM)
- **Phase 1 — VR foundation**: OpenXR with all three Meta Quest controller profiles, XR Origin in Museum scene
- **Phase 2 — Locomotion**: teleportation areas marked, snap turn from XRI Starter Assets, comfort vignette
- **Phase 3 — Gaze identification**: 2 m raycast + bounds-center check + 1.5 s dwell + 30 s per-artifact cooldown, 42 Egyptian artifacts cataloged with researched era + description, dark-stone/gold-trim world-space labels
- **Phase 3 (ML, inert)**: `InferenceEngineArtifactClassifier`, `ArtifactRecognitionPipeline`, `ArtifactCaptureCamera`, `Wire ML Recognition Pipeline` menu — included for show but **not wired** into the active scene
- **Phase 4 — Lobby**: scene with VR world-space form, SQLite (`%APPDATA%/MuseumVR/museum.db`), OpenAI key loaded from `%APPDATA%/MuseumVR/config.json`, additive scene load to Museum on submit
- **Phase 5 — Realtime API voice**: `RealtimeClient` (WebSocket), `MicCapture` (24 kHz PCM16 base64), `StreamingAudioPlayer` (30 s ring buffer + jitter prebuffer), `GuideOrchestrator` with three input modes
- **Phase 6.1 — Tour guide pathfinding**: NavMesh bake utility, `TourGuideAgent` (NavMeshAgent follow + animator), `AmplitudeJawFlap`, placeholder capsule body utility

## Active fix awaiting verification

Ring buffer in `StreamingAudioPlayer.cs` raised from 8 s → 30 s. Fixes mid-sentence audio cutoffs caused by the Realtime API streaming audio faster than realtime playback consumes it.

## Key locked decisions

| Decision | Rationale |
|---|---|
| Default model: **`gpt-4o-mini-realtime-preview`** | $5 OpenAI account doesn't have GA access for `gpt-realtime` / `gpt-realtime-mini` |
| Default mode: **PushToTalk** (key `T`) | Server VAD created uncontrollable echo loops with PC speakers + PC mic |
| **ML stack included but inert** | User explicitly: "keep all the ML files but just for show" |
| **HDRP stays** (no URP) | Switching would require re-converting all 66 FBX materials |
| **No asmdefs** | Every attempt broke transitive references when packages reshuffled |
| All multi-step Unity actions are **editor menus** under `Tools → Museum → Phase N` | Reproducible, scriptable, scene-portable |
| **Sentis renamed to Inference Engine** in Unity 6 | Package id `com.unity.ai.inference`, namespace `Unity.InferenceEngine` |
| Gaze `maxDistance = 2 m` + bounds-center check | Museum walls without colliders let raycasts punch through; 4 m caused random firings |
| Mute mic + clear `input_audio_buffer` at response start AND end | Defeats echo loops in ServerVad mode |
| OpenAI key from `config.json`, not lobby form | Operator owns the key; visitors never see it |

## Awaiting from user

1. Confirm PTT + 30 s buffer make voice clean end-to-end
2. Bake the NavMesh + add the placeholder capsule to verify pathfinding
3. Download a Mixamo character + Idle / Walking / Talking animations, import as Humanoid

## Next steps

- **Phase 6.4** — Mixamo avatar wire-up utility (Animator state machine, bone references, reparent AudioSource to head bone, wire jaw bone)
- **Phase 7** — HDRP optimization: bake GI, LOD groups on heaviest FBX, profile XR mode for ≥ 72 fps, end-of-visit DB write, error/empty states

## Reference docs

- [`PROJECT_REPORT.md`](PROJECT_REPORT.md) — full graduation report with architecture, phases, evaluation, appendices
- [`PROJECT_OVERVIEW.txt`](PROJECT_OVERVIEW.txt) — plain-English overview for non-engineers
- [`progress.md`](progress.md) — full phase status, runtime config, decisions log
