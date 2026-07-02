using UnityEngine;

namespace Museum.Guide
{
    /// <summary>
    /// Drives a jaw bone's local X rotation from the AudioSource's current output amplitude.
    /// Cheap stand-in for proper viseme lip-sync. Also flips a TourGuideAgent.isSpeaking flag
    /// so the Animator can layer in a "talking" gesture.
    /// </summary>
    public class AmplitudeJawFlap : MonoBehaviour
    {
        [Tooltip("AudioSource feeding the jaw motion (the streaming Realtime API output).")]
        public AudioSource audioSource;

        [Tooltip("Bone to rotate. Usually a Mixamo Head/Jaw bone with X = open angle.")]
        public Transform jawBone;

        [Tooltip("Optional: tour guide agent to flip IsSpeaking on while audio is above the noise floor.")]
        public TourGuideAgent tourGuideAgent;

        [Header("Tuning")]
        [Tooltip("Multiplier on raw amplitude. Raise if jaw barely opens.")]
        public float amplitudeGain = 8f;

        [Tooltip("Maximum open angle in degrees.")]
        public float maxOpenDegrees = 18f;

        [Tooltip("Smoothing — how fast the jaw catches up to amplitude.")]
        public float responsiveness = 18f;

        [Tooltip("Below this amplitude, treat as silence (flat jaw, IsSpeaking=false).")]
        public float silenceThreshold = 0.005f;

        [Tooltip("Local rotation axis on the jaw bone for opening. Default X works for most Mixamo rigs.")]
        public Vector3 openAxis = new Vector3(1, 0, 0);

        Quaternion _restRotation;
        float _smoothedAmplitude;
        readonly float[] _samples = new float[256];

        void Start()
        {
            if (jawBone != null) _restRotation = jawBone.localRotation;
        }

        void LateUpdate()
        {
            if (audioSource == null) return;

            audioSource.GetOutputData(_samples, 0);
            float sum = 0f;
            for (int i = 0; i < _samples.Length; i++) sum += Mathf.Abs(_samples[i]);
            float rms = sum / _samples.Length;

            // Jaw bone is optional — most Mixamo characters don't have one. The IsSpeaking flag
            // alone is enough to drive the Talk animation layer.
            if (jawBone != null)
            {
                float target = Mathf.Clamp01(rms * amplitudeGain);
                _smoothedAmplitude = Mathf.MoveTowards(_smoothedAmplitude, target, responsiveness * Time.deltaTime);
                float angle = _smoothedAmplitude * maxOpenDegrees;
                jawBone.localRotation = _restRotation * Quaternion.AngleAxis(angle, openAxis);
            }

            if (tourGuideAgent != null)
                tourGuideAgent.isSpeaking = rms > silenceThreshold;
        }
    }
}
