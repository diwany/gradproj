using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Museum.Artifact;

namespace Museum.EditorTools
{
    public static class ArtifactCaptureSetup
    {
        const string AddCaptureCameraMenu = "Tools/Museum/Phase 3/Add Artifact Capture Camera";
        const string ToggleDebugSaverMenu = "Tools/Museum/Phase 3/Toggle Debug Capture Saver";

        [MenuItem(AddCaptureCameraMenu)]
        public static void AddCaptureCamera()
        {
            var origin = Object.FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (origin == null)
            {
                EditorUtility.DisplayDialog("Capture Camera", "No XR Origin in active scene. Open Museum.unity first.", "OK");
                return;
            }

            var headCamera = origin.Camera;
            if (headCamera == null)
            {
                EditorUtility.DisplayDialog("Capture Camera", "XR Origin has no Camera assigned.", "OK");
                return;
            }

            var existing = Object.FindAnyObjectByType<ArtifactCaptureCamera>();
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing.gameObject);
                EditorUtility.DisplayDialog("Capture Camera",
                    $"Already in scene at:\n{GetPath(existing.gameObject)}", "OK");
                return;
            }

            var go = new GameObject("Artifact Capture Camera");
            Undo.RegisterCreatedObjectUndo(go, "Add Capture Camera");
            go.transform.SetParent(headCamera.transform, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            var cam = go.AddComponent<Camera>();
            cam.enabled = false;

            var capture = go.AddComponent<ArtifactCaptureCamera>();
            capture.cullingMask = headCamera.cullingMask;

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);

            EditorUtility.DisplayDialog("Capture Camera",
                $"Added 'Artifact Capture Camera' under {headCamera.name}.\n" +
                $"Resolution: {capture.resolution}x{capture.resolution}\n" +
                $"FOV: {capture.captureFov}°\n\n" +
                "Optionally enable the debug saver via Tools > Museum > Phase 3 > Toggle Debug Capture Saver to confirm framing.",
                "OK");
        }

        [MenuItem(ToggleDebugSaverMenu)]
        public static void ToggleDebugSaver()
        {
            var detector = Object.FindAnyObjectByType<GazeArtifactDetector>();
            if (detector == null)
            {
                EditorUtility.DisplayDialog("Debug Capture Saver", "No GazeArtifactDetector in scene.", "OK");
                return;
            }

            var saver = detector.GetComponent<DebugArtifactCaptureSaver>();
            if (saver != null)
            {
                Undo.DestroyObjectImmediate(saver);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                EditorUtility.DisplayDialog("Debug Capture Saver", "Removed debug saver. PNGs will no longer be written.", "OK");
                return;
            }

            saver = Undo.AddComponent<DebugArtifactCaptureSaver>(detector.gameObject);
            saver.captureCamera = Object.FindAnyObjectByType<ArtifactCaptureCamera>();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            EditorUtility.DisplayDialog("Debug Capture Saver",
                "Added. On every gaze-identification, a 224x224 PNG is saved to:\n" +
                $"{System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName, "_DebugCaptures")}\n\n" +
                "Use this to verify the capture camera frames the artifact well before training/converting your ML model. Toggle off when done.",
                "OK");
        }

        static string GetPath(GameObject go)
        {
            var t = go.transform;
            var sb = new System.Text.StringBuilder(go.name);
            while (t.parent != null) { t = t.parent; sb.Insert(0, t.name + "/"); }
            return sb.ToString();
        }
    }
}
