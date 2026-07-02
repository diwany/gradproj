using System;
using UnityEngine;

namespace Museum.Voice
{
    /// <summary>
    /// Plays PCM16 24 kHz mono audio streamed from the Realtime API.
    /// Uses a streaming AudioClip with PCMReaderCallback so Unity's audio thread pulls
    /// samples directly from our ring buffer — no main-thread / OnAudioFilterRead juggling.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class StreamingAudioPlayer : MonoBehaviour
    {
        public const int SampleRate = 24000;
        const int RingSeconds = 30;

        [Tooltip("Samples buffered before playback starts (and re-buffers after underrun). 4800 = 200ms at 24kHz. Higher = smoother under network jitter, longer first-word latency.")]
        public int prebufferSamples = 4800;

        AudioSource _audioSource;
        AudioClip _clip;

        readonly object _lock = new object();
        float[] _ring;
        int _readIdx;
        int _writeIdx;
        int _available;
        bool _draining; // true while we're actively pulling samples; flips to false on underrun to wait for prebuffer
        float _lastSample; // hold last real sample on underrun so DC offset doesn't click

        public int BufferedSamples
        {
            get { lock (_lock) return _available; }
        }

        public float BufferedSeconds => BufferedSamples / (float)SampleRate;

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.spatialBlend = 0f; // 2D for now; orchestrator can override when avatar lands
            _audioSource.loop = true;
            _audioSource.playOnAwake = false;

            _ring = new float[SampleRate * RingSeconds];
            _readIdx = 0;
            _writeIdx = 0;
            _available = 0;

            _clip = AudioClip.Create(
                name: "RealtimeStream",
                lengthSamples: SampleRate * 1, // 1s placeholder; PCMReaderCallback streams indefinitely
                channels: 1,
                frequency: SampleRate,
                stream: true,
                pcmreadercallback: OnPcmRead);
            _audioSource.clip = _clip;
            _audioSource.Play();
        }

        void OnDestroy()
        {
            if (_audioSource != null) _audioSource.Stop();
            if (_clip != null) Destroy(_clip);
        }

        public void EnqueueBase64Pcm16(string base64)
        {
            if (string.IsNullOrEmpty(base64)) return;
            byte[] bytes;
            try { bytes = Convert.FromBase64String(base64); }
            catch { return; }
            if (bytes.Length < 2) return;

            int sampleCount = bytes.Length / 2;
            lock (_lock)
            {
                int free = _ring.Length - _available;
                if (sampleCount > free)
                {
                    // Should never happen with 30s ring vs typical multi-second responses.
                    // If it does, log so we know to grow further. Drop oldest as last resort.
                    int drop = sampleCount - free;
                    Debug.LogWarning($"[StreamingAudioPlayer] Ring overflow — dropping {drop} oldest samples ({drop / (float)SampleRate * 1000:F0}ms). Increase RingSeconds.");
                    _readIdx = (_readIdx + drop) % _ring.Length;
                    _available -= drop;
                }
                for (int i = 0; i < sampleCount; i++)
                {
                    short v = (short)(bytes[i * 2] | (bytes[i * 2 + 1] << 8));
                    _ring[_writeIdx] = v / (float)short.MaxValue;
                    _writeIdx = (_writeIdx + 1) % _ring.Length;
                }
                _available += sampleCount;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _readIdx = 0;
                _writeIdx = 0;
                _available = 0;
                _draining = false;
                _lastSample = 0f;
            }
        }

        // Called on the audio thread. Must not allocate or touch UnityEngine APIs (besides math).
        void OnPcmRead(float[] data)
        {
            lock (_lock)
            {
                int needed = data.Length;

                // If we're not currently draining, wait until enough is buffered before starting.
                if (!_draining)
                {
                    if (_available < prebufferSamples)
                    {
                        for (int i = 0; i < needed; i++) data[i] = 0f;
                        return;
                    }
                    _draining = true;
                }

                int take = Mathf.Min(needed, _available);
                for (int i = 0; i < take; i++)
                {
                    _lastSample = _ring[_readIdx];
                    data[i] = _lastSample;
                    _readIdx = (_readIdx + 1) % _ring.Length;
                }
                _available -= take;

                // If we underrun, fade out from last sample to silence over the rest of the buffer
                // (less audible click than slamming to 0), then go back to "not draining" so we re-prebuffer.
                if (take < needed)
                {
                    float decay = _lastSample;
                    for (int i = take; i < needed; i++)
                    {
                        decay *= 0.92f;
                        data[i] = decay;
                    }
                    _draining = false;
                    _lastSample = 0f;
                }
            }
        }
    }
}
