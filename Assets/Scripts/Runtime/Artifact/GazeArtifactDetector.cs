using System;
using System.Collections.Generic;
using UnityEngine;

namespace Museum.Artifact
{
    /// <summary>
    /// Casts a ray from the head each frame, looking for ArtifactInfo components within range.
    /// When the player holds gaze on the same artifact for `dwellSeconds`, raises
    /// OnArtifactIdentified once. Each artifact is then suppressed for `cooldownSeconds`.
    /// </summary>
    [DisallowMultipleComponent]
    public class GazeArtifactDetector : MonoBehaviour
    {
        [Header("Ray")]
        [Tooltip("Camera/transform the gaze ray originates from. Defaults to the XR Origin's main camera at runtime.")]
        public Transform gazeOrigin;

        [Tooltip("Maximum distance from gaze origin to artifact for identification to fire. Keep tight (~2m) so the ray can't reach across rooms — museum walls in the asset pack don't all have colliders.")]
        public float maxDistance = 2f;

        [Tooltip("Layers the gaze raycast considers. Set to include artifact colliders + walls so gaze can be blocked by obstacles.")]
        public LayerMask raycastMask = ~0;

        [Header("Dwell")]
        [Tooltip("Seconds the player must continuously look at the same artifact before it identifies.")]
        public float dwellSeconds = 1.5f;

        [Tooltip("Per-artifact cooldown after identification before it can re-trigger.")]
        public float cooldownSeconds = 30f;

        public event Action<ArtifactInfo, RaycastHit> OnArtifactIdentified;

        ArtifactInfo _current;
        float _dwellElapsed;
        readonly Dictionary<ArtifactInfo, float> _cooldownUntil = new Dictionary<ArtifactInfo, float>();

        public ArtifactInfo CurrentTarget => _current;
        public float CurrentDwellProgress01 => _current != null ? Mathf.Clamp01(_dwellElapsed / dwellSeconds) : 0f;

        void Awake()
        {
            if (gazeOrigin == null && Camera.main != null)
                gazeOrigin = Camera.main.transform;
        }

        void Update()
        {
            if (gazeOrigin == null)
            {
                if (Camera.main == null) return;
                gazeOrigin = Camera.main.transform;
            }

            ArtifactInfo hitArtifact = null;
            RaycastHit hit = default;

            if (Physics.Raycast(gazeOrigin.position, gazeOrigin.forward, out hit, maxDistance, raycastMask, QueryTriggerInteraction.Ignore))
            {
                hitArtifact = hit.collider.GetComponentInParent<ArtifactInfo>();
            }

            // Reject artifacts whose actual bounds-center is beyond range — colliders can extend a long way past maxDistance
            // and museum walls without colliders can let the ray clip into artifacts in other rooms.
            if (hitArtifact != null)
            {
                Bounds artifactBounds = ComputeArtifactBounds(hitArtifact);
                float centerDist = Vector3.Distance(gazeOrigin.position, artifactBounds.center);
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
                Debug.Log($"[GazeArtifactDetector] Identified '{hitArtifact.displayName}' — raycast hit at {hit.distance:F2}m, bounds center {Vector3.Distance(gazeOrigin.position, ComputeArtifactBounds(hitArtifact).center):F2}m");
                OnArtifactIdentified?.Invoke(hitArtifact, hit);
            }
        }

        static Bounds ComputeArtifactBounds(ArtifactInfo artifact)
        {
            var renderers = artifact.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return new Bounds(artifact.transform.position, Vector3.one * 0.1f);
            var b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            return b;
        }

        bool IsOnCooldown(ArtifactInfo artifact)
        {
            return _cooldownUntil.TryGetValue(artifact, out var until) && Time.time < until;
        }

        void OnDrawGizmosSelected()
        {
            if (gazeOrigin == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(gazeOrigin.position, gazeOrigin.forward * maxDistance);
        }
    }
}
