using UnityEngine;

namespace Museum.Artifact
{
    /// <summary>
    /// Listens to a GazeArtifactDetector and instantiates an ArtifactLabel prefab
    /// above the identified artifact's bounds, billboarded toward the player.
    /// </summary>
    [RequireComponent(typeof(GazeArtifactDetector))]
    public class ArtifactLabelSpawner : MonoBehaviour
    {
        [Tooltip("World-space label prefab. Must contain an ArtifactLabel component on the root.")]
        public ArtifactLabel labelPrefab;

        [Tooltip("Vertical offset above the top of the artifact bounds where the label spawns.")]
        public float spawnYOffset = 0.2f;

        [Tooltip("Fallback offset when the artifact has no renderer bounds.")]
        public float fallbackHeight = 1.7f;

        GazeArtifactDetector _detector;

        void Awake()
        {
            _detector = GetComponent<GazeArtifactDetector>();
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
            if (labelPrefab == null)
            {
                Debug.LogWarning($"[ArtifactLabelSpawner] Identified '{artifact.displayName}' but labelPrefab is null. Run Tools > Museum > Phase 3 > Regenerate Label Prefab.");
                return;
            }
            var pos = ComputeLabelPosition(artifact);
            var label = Instantiate(labelPrefab, pos, Quaternion.identity);
            label.Show(artifact.displayName, artifact.era, _detector.gazeOrigin);
            Debug.Log($"[ArtifactLabelSpawner] Spawned label for '{artifact.displayName}' at {pos} (gaze hit {hit.point}, artifact origin {artifact.transform.position})");
        }

        Vector3 ComputeLabelPosition(ArtifactInfo artifact)
        {
            var renderers = artifact.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return artifact.transform.position + Vector3.up * fallbackHeight;

            var b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                b.Encapsulate(renderers[i].bounds);

            return new Vector3(b.center.x, b.max.y + spawnYOffset, b.center.z);
        }
    }
}
