# Museum VR (Egyptian Museum Tour)

Museum VR is a Unity-based VR graduation project that delivers an interactive Egyptian museum tour.
Visitors enter through a lobby form, explore the museum in VR, look at artifacts to trigger narration,
and talk to a realtime AI guide using push-to-talk.

## Highlights

- Unity 6 + HDRP VR project built for Meta Quest via PCVR (Quest Link).
- Gaze-based artifact identification with dwell time and cooldown.
- Realtime AI guide narration and Q&A using OpenAI Realtime API.
- Push-to-talk interaction (keyboard and VR controller primary buttons).
- Visitor session capture (name, age, country) persisted to SQLite.
- Mixamo-based guide avatar pipeline with NavMesh follow system.

## Visitor Flow

1. Start in the Lobby scene.
2. Enter visitor details (name, age, country) and begin tour.
3. Load into museum scene.
4. Look at an artifact to trigger 1-2 sentence narration.
5. Hold push-to-talk to ask follow-up questions.

## Tech Stack

- Engine: Unity 6000.4.5f1 LTS
- Rendering: HDRP 17.4
- VR: OpenXR + XR Interaction Toolkit 3.x
- Input: Unity Input System
- AI Voice: OpenAI Realtime API (WebSocket)
- Database: SQLite (gilzoide.sqlite-net)
- JSON: Newtonsoft.Json

## Repository Structure

- `Assets/`
	- Runtime gameplay scripts, scenes, prefabs, models, materials, and audio/voice systems.
- `Packages/`
	- Unity package manifests and dependencies.
- `ProjectSettings/`
	- Unity project configuration.
- `Assets/Scripts/Runtime/Voice/`
	- Core realtime voice pipeline (`GuideOrchestrator`, `RealtimeClient`, `MicCapture`, `StreamingAudioPlayer`).
- `Assets/Scripts/Runtime/Artifact/`
	- Gaze detection, artifact metadata, and label spawning.
- `Assets/Scripts/Runtime/Lobby/`
	- Lobby form and visitor/session logic.

## Prerequisites

- Unity Hub + Unity Editor `6000.4.5f1` (or compatible Unity 6 LTS setup).
- Meta Quest headset configured for PCVR with Link/Air Link.
- OpenAI API key with Realtime API access.

## Configuration

Create `config.json` with your OpenAI key.

Expected format:

```json
{
	"openai_api_key": "sk-..."
}
```

The app reads config from these locations:

1. `<exe folder>/MuseumVR/config.json`
2. `%APPDATA%/MuseumVR/config.json` (fallback)

SQLite visitor data is stored in:

1. `<exe folder>/MuseumVR/museum.db`
2. `%APPDATA%/MuseumVR/museum.db` (fallback)

## Open and Run

1. Open this folder in Unity Hub.
2. Open `Assets/Scenes/Lobby.unity`.
3. Enter Play mode.
4. Fill visitor form and start tour.

## Controls

- Artifact narration trigger: Look at an artifact for dwell duration.
- Push-to-talk:
	- Desktop: Hold `T`
	- VR: Hold primary button (`Y` / `B`)

## Voice Modes

`GuideOrchestrator` supports:

- `GazeOnly`: narration on gaze only, mic off.
- `PushToTalk`: mic streamed only while button/key is held (recommended).
- `ServerVad`: always-on mic with server-side speech detection.

## Notes

- The production path uses gaze-based identification and realtime voice.
- ML classifier-related scripts are intentionally kept in the project but not wired into the active runtime flow.
- Generated Unity folders (`Library`, `Temp`, `Build`, etc.) are excluded from source control.

## Useful Documents

- `PROJECT_OVERVIEW.txt`
- `PROJECT_REPORT.md`
- `SUMMARY.md`
- `progress.md`

## License

This project is maintained as an academic graduation project repository.
