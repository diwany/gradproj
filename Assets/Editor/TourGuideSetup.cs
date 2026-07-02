using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Museum.Artifact;
using Museum.Util;
using Museum.Voice;

namespace Museum.EditorTools
{
    public static class TourGuideSetup
    {
        const string MenuPath = "Tools/Museum/Phase 5/Add Tour Guide to Active Scene";
        const string ResetModelMenu = "Tools/Museum/Phase 5/Reset Tour Guide Model to Default";
        const string GazeOnlyMenu = "Tools/Museum/Phase 5/Set Tour Guide to Gaze-Only Mode";
        const string DefaultModel = "gpt-4o-mini-realtime-preview";

        [MenuItem(GazeOnlyMenu)]
        public static void SetGazeOnly() => SetMode(GuideOrchestrator.InputMode.GazeOnly, "Gaze-Only Mode",
            "Mic disabled. Guide narrates only when you look at an artifact for 1.5s. Visitor cannot ask voice questions in this mode.");

        const string PttMenu = "Tools/Museum/Phase 5/Set Tour Guide to Push-To-Talk Mode";
        [MenuItem(PttMenu)]
        public static void SetPushToTalk() => SetMode(GuideOrchestrator.InputMode.PushToTalk, "Push-To-Talk Mode",
            "Visitor holds the PTT key (default T) to ask questions. Guide narrates artifacts on gaze. No background-noise false fires.");

        const string VadMenu = "Tools/Museum/Phase 5/Set Tour Guide to Server-VAD Mode";
        [MenuItem(VadMenu)]
        public static void SetServerVad() => SetMode(GuideOrchestrator.InputMode.ServerVad, "Server-VAD Mode",
            "Mic always live. OpenAI auto-detects when visitor stops speaking. Most natural but vulnerable to echo loops and background noise.");

        static void SetMode(GuideOrchestrator.InputMode mode, string title, string description)
        {
            var orchestrator = Object.FindAnyObjectByType<GuideOrchestrator>();
            if (orchestrator == null)
            {
                EditorUtility.DisplayDialog(title, "No Tour Guide in active scene.", "OK");
                return;
            }
            UnityEditor.Undo.RecordObject(orchestrator, "Set Input Mode");
            orchestrator.inputMode = mode;
            UnityEditor.EditorUtility.SetDirty(orchestrator);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            EditorUtility.DisplayDialog(title, description, "OK");
        }

        [MenuItem(ResetModelMenu)]
        public static void ResetModelToDefault()
        {
            var orchestrator = Object.FindAnyObjectByType<GuideOrchestrator>();
            if (orchestrator == null)
            {
                EditorUtility.DisplayDialog("Reset Model", "No Tour Guide in active scene.", "OK");
                return;
            }
            UnityEditor.Undo.RecordObject(orchestrator, "Reset Model");
            orchestrator.model = DefaultModel;
            UnityEditor.EditorUtility.SetDirty(orchestrator);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            EditorUtility.DisplayDialog("Reset Model",
                $"Model field on '{orchestrator.gameObject.name}' is now: {DefaultModel}",
                "OK");
        }

        [MenuItem(MenuPath)]
        public static void AddTourGuide()
        {
            var existing = Object.FindAnyObjectByType<GuideOrchestrator>();
            if (existing != null)
            {
                if (existing.GetComponent<SingletonAudioListener>() == null)
                    Undo.AddComponent<SingletonAudioListener>(existing.gameObject);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing.gameObject);
                EditorUtility.DisplayDialog("Tour Guide",
                    "Tour Guide already in scene. Ensured SingletonAudioListener is attached so the duplicate listener gets suppressed at runtime.",
                    "OK");
                return;
            }

            var go = new GameObject("Tour Guide");
            Undo.RegisterCreatedObjectUndo(go, "Add Tour Guide");

            var audio = go.AddComponent<AudioSource>();
            audio.spatialBlend = 0f;
            audio.loop = true;
            audio.playOnAwake = false;

            var player = go.AddComponent<StreamingAudioPlayer>();
            var mic = go.AddComponent<MicCapture>();
            var orchestrator = go.AddComponent<GuideOrchestrator>();
            go.AddComponent<SingletonAudioListener>();

            var gaze = Object.FindAnyObjectByType<GazeArtifactDetector>();
            if (gaze != null) orchestrator.gazeDetector = gaze;

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);

            EditorUtility.DisplayDialog("Tour Guide",
                "Tour Guide added with:\n" +
                "- AudioSource (2D for now; Phase 6 will reparent to the avatar's head bone)\n" +
                "- StreamingAudioPlayer (24 kHz mono PCM16 ring buffer)\n" +
                "- MicCapture (downsamples device rate to 24 kHz, ~40ms PCM16 chunks)\n" +
                $"- GuideOrchestrator (gaze detector wired: {(gaze != null ? "yes" : "no")})\n\n" +
                "On Play in the Museum scene: opens a Realtime API session using the API key loaded by the lobby, " +
                "greets the visitor by name, and pushes per-artifact context whenever the gaze detector identifies one.\n\n" +
                "Requires a working microphone and an OpenAI account with gpt-4o-realtime access. " +
                "If the API key is missing, the orchestrator self-disables with a console warning.",
                "OK");
        }
    }
}
