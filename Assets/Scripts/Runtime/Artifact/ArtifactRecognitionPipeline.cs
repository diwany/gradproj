using System.Collections.Generic;
using UnityEngine;

namespace Museum.Artifact
{
    /// <summary>
    /// Full pipeline: on gaze identification, render capture camera, run ML classifier,
    /// look up the predicted class in the registry of ArtifactInfo, spawn the floating label.
    /// Falls back to the gaze-detected artifact when the ML model is not loaded or
    /// confidence is below threshold.
    /// </summary>
    [RequireComponent(typeof(GazeArtifactDetector))]
    public class ArtifactRecognitionPipeline : MonoBehaviour
    {
        [Header("References")]
        public ArtifactCaptureCamera captureCamera;
        public InferenceEngineArtifactClassifier classifier;
        public ArtifactLabel labelPrefab;

        [Header("Confidence")]
        [Tooltip("Below this softmax score, the ML prediction is ignored and the gaze-hit artifact is used instead.")]
        [Range(0f, 1f)] public float minConfidence = 0.3f;

        [Tooltip("Log a warning when the ML class differs from the gaze-hit artifact's classIndex (helps catch mislabelled training data).")]
        public bool logMismatches = true;

        [Header("Label placement")]
        public float spawnYOffset = 0.2f;
        public float fallbackHeight = 1.7f;

        GazeArtifactDetector _detector;
        Dictionary<int, ArtifactInfo> _byClassIndex;

        void Awake()
        {
            _detector = GetComponent<GazeArtifactDetector>();
            BuildRegistry();
        }

        void OnEnable()
        {
            if (_detector != null) _detector.OnArtifactIdentified += HandleIdentified;
        }

        void OnDisable()
        {
            if (_detector != null) _detector.OnArtifactIdentified -= HandleIdentified;
        }

        void BuildRegistry()
        {
            _byClassIndex = new Dictionary<int, ArtifactInfo>();
            foreach (var info in Object.FindObjectsByType<ArtifactInfo>(FindObjectsInactive.Include))
            {
                if (info.classIndex < 0) continue;
                if (_byClassIndex.ContainsKey(info.classIndex))
                    Debug.LogWarning($"[ArtifactRecognitionPipeline] Duplicate classIndex {info.classIndex}: '{_byClassIndex[info.classIndex].displayName}' and '{info.displayName}'. Last one wins.");
                _byClassIndex[info.classIndex] = info;
            }
        }

        void HandleIdentified(ArtifactInfo gazedArtifact, RaycastHit hit)
        {
            ArtifactInfo target = gazedArtifact;

            if (classifier != null && classifier.IsReady && captureCamera != null)
            {
                var rt = captureCamera.CaptureNow();
                if (rt != null && classifier.TryClassify(rt, out var prediction))
                {
                    if (prediction.confidence >= minConfidence && _byClassIndex.TryGetValue(prediction.classIndex, out var predicted))
                    {
                        if (logMismatches && predicted != gazedArtifact && gazedArtifact.classIndex >= 0)
                            Debug.LogWarning($"[ArtifactRecognitionPipeline] ML mismatch: gaze='{gazedArtifact.displayName}' (idx {gazedArtifact.classIndex}), ML='{predicted.displayName}' (idx {prediction.classIndex}, conf={prediction.confidence:F2}). Trusting ML.");
                        target = predicted;
                    }
                    else if (logMismatches)
                    {
                        Debug.Log($"[ArtifactRecognitionPipeline] ML low-confidence ({prediction.confidence:F2}) on '{gazedArtifact.displayName}'. Falling back to gaze.");
                    }
                }
            }

            SpawnLabel(target);
        }

        void SpawnLabel(ArtifactInfo artifact)
        {
            if (labelPrefab == null || artifact == null) return;
            var pos = ComputeLabelPosition(artifact);
            var label = Instantiate(labelPrefab, pos, Quaternion.identity);
            label.Show(artifact.displayName, artifact.era, _detector.gazeOrigin);
        }

        Vector3 ComputeLabelPosition(ArtifactInfo artifact)
        {
            var renderers = artifact.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return artifact.transform.position + Vector3.up * fallbackHeight;
            var b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            return new Vector3(b.center.x, b.max.y + spawnYOffset, b.center.z);
        }
    }
}
