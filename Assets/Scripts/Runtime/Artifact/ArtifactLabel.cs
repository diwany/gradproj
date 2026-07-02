using TMPro;
using UnityEngine;

namespace Museum.Artifact
{
    /// <summary>
    /// World-space label that fades and scales in, displays an artifact's name + era,
    /// then fades out. Always faces the player horizontally.
    /// </summary>
    public class ArtifactLabel : MonoBehaviour
    {
        public TMP_Text nameLabel;
        public TMP_Text eraLabel;
        public CanvasGroup canvasGroup;

        [Tooltip("Total visible time before auto-fade-out begins.")]
        public float visibleSeconds = 6f;

        [Tooltip("Fade in/out duration.")]
        public float fadeSeconds = 0.4f;

        [Tooltip("Scale at which the label is born (it scales up to 1 during the fade-in).")]
        public float startScale = 0.85f;

        Transform _faceTarget;
        float _spawnTime;
        Vector3 _baseScale;

        public void Show(string displayName, string era, Transform faceTarget)
        {
            if (nameLabel != null) nameLabel.text = displayName;
            if (eraLabel != null) eraLabel.text = era;
            _faceTarget = faceTarget;
            _spawnTime = Time.time;
            _baseScale = transform.localScale;
            transform.localScale = _baseScale * startScale;
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        void Update()
        {
            if (_faceTarget != null)
            {
                var dir = transform.position - _faceTarget.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f)
                    transform.rotation = Quaternion.LookRotation(dir);
            }

            float t = Time.time - _spawnTime;
            float fadeIn = Mathf.Clamp01(t / fadeSeconds);
            float easedIn = 1f - (1f - fadeIn) * (1f - fadeIn);
            float fadeOut = 1f - Mathf.Clamp01((t - visibleSeconds) / fadeSeconds);

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Min(easedIn, fadeOut);

            float scaleT = Mathf.Lerp(startScale, 1f, easedIn);
            transform.localScale = _baseScale / startScale * scaleT;

            if (t > visibleSeconds + fadeSeconds)
                Destroy(gameObject);
        }
    }
}
