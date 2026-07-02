using System.IO;
using UnityEngine;

namespace Museum.Artifact
{
    /// <summary>
    /// On every artifact identification, captures the camera and saves the PNG to
    /// `_DebugCaptures/` next to the project. Used during Phase 3 to verify the capture
    /// camera is framing artifacts correctly before the ML model is wired in.
    /// Disable or remove for shipping.
    /// </summary>
    [RequireComponent(typeof(GazeArtifactDetector))]
    public class DebugArtifactCaptureSaver : MonoBehaviour
    {
        [Tooltip("Capture camera. If left empty, looks for one in children of XR Origin at runtime.")]
        public ArtifactCaptureCamera captureCamera;

        [Tooltip("Folder relative to the project root (above Assets) where PNGs are written. Created if missing.")]
        public string outputFolder = "_DebugCaptures";

        GazeArtifactDetector _detector;

        void Awake()
        {
            _detector = GetComponent<GazeArtifactDetector>();
            if (captureCamera == null)
                captureCamera = Object.FindAnyObjectByType<ArtifactCaptureCamera>();
        }

        void OnEnable()
        {
            if (_detector != null) _detector.OnArtifactIdentified += HandleIdentified;
        }

        void OnDisable()
        {
            if (_detector != null) _detector.OnArtifactIdentified -= HandleIdentified;
        }

        void HandleIdentified(ArtifactInfo artifact, RaycastHit hit)
        {
            if (captureCamera == null)
            {
                Debug.LogWarning("[DebugArtifactCaptureSaver] No ArtifactCaptureCamera in scene. Run Tools > Museum > Phase 3 > Add Artifact Capture Camera.");
                return;
            }

            var tex = captureCamera.CaptureNowReadback();
            if (tex == null) return;

            var dir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, outputFolder);
            Directory.CreateDirectory(dir);
            var safeName = string.Concat(artifact.displayName.Split(Path.GetInvalidFileNameChars()));
            var fileName = $"{System.DateTime.Now:HHmmss}_{safeName}.png";
            var path = Path.Combine(dir, fileName);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Debug.Log($"[DebugArtifactCaptureSaver] Saved capture: {path}");
        }
    }
}
