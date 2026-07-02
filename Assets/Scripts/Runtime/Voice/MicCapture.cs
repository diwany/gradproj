using System;
using UnityEngine;

namespace Museum.Voice
{
    /// <summary>
    /// Captures from the default microphone, downsamples to 24 kHz mono, encodes as PCM16,
    /// and emits ~40ms chunks as base64 strings ready for Realtime API's input_audio_buffer.append.
    /// </summary>
    [DisallowMultipleComponent]
    public class MicCapture : MonoBehaviour
    {
        public const int TargetSampleRate = 24000;
        const int ChunkMs = 40;
        const int RingBufferSeconds = 2; // mic clip length

        [Tooltip("Microphone device. Empty = system default.")]
        public string deviceName = "";

        [Tooltip("Skip silence below this normalized amplitude. Set 0 to send everything (recommended; ServerVAD on the API side handles silence).")]
        [Range(0f, 0.2f)] public float silenceThreshold = 0f;

        public event Action<string> OnAudioChunkBase64;
        public event Action<string> OnError;

        AudioClip _clip;
        int _deviceRate;
        int _lastReadSample;
        float[] _readBuffer;
        float _resampleAccum;
        // Holding tank of resampled 24 kHz floats waiting to be packed into chunks
        float[] _outBuffer;
        int _outFill;
        int _chunkSampleCount;

        public bool IsRunning { get; private set; }

        public void StartCapture()
        {
            if (IsRunning) return;
            if (Microphone.devices == null || Microphone.devices.Length == 0)
            {
                OnError?.Invoke("No microphone devices available. On Windows: Settings > Privacy > Microphone > Allow desktop apps.");
                return;
            }

            var name = string.IsNullOrEmpty(deviceName) ? null : deviceName;
            // Prefer device's native min rate; fall back to 48000 / 44100.
            Microphone.GetDeviceCaps(name, out var minFreq, out var maxFreq);
            int rate = 48000;
            if (maxFreq != 0 && (rate < minFreq || rate > maxFreq))
                rate = Mathf.Clamp(rate, minFreq, maxFreq);
            if (minFreq == 0 && maxFreq == 0) rate = 48000;

            _clip = Microphone.Start(name, true, RingBufferSeconds, rate);
            if (_clip == null)
            {
                OnError?.Invoke($"Microphone.Start failed (device='{name ?? "default"}', rate={rate}).");
                return;
            }
            _deviceRate = rate;
            _lastReadSample = 0;
            _resampleAccum = 0f;
            _readBuffer = new float[Mathf.Max(rate / 4, 4096)]; // up to ~250ms read each frame
            _chunkSampleCount = (TargetSampleRate * ChunkMs) / 1000; // 960
            _outBuffer = new float[_chunkSampleCount * 4];
            _outFill = 0;
            IsRunning = true;
        }

        public void StopCapture()
        {
            if (!IsRunning) return;
            try { Microphone.End(string.IsNullOrEmpty(deviceName) ? null : deviceName); } catch { }
            _clip = null;
            IsRunning = false;
        }

        void Update()
        {
            if (!IsRunning || _clip == null) return;

            int writeHead = Microphone.GetPosition(string.IsNullOrEmpty(deviceName) ? null : deviceName);
            int available = writeHead - _lastReadSample;
            if (available < 0) available += _clip.samples; // wrapped around
            if (available <= 0) return;

            // Cap per-frame read to avoid glitches if Update is delayed.
            available = Mathf.Min(available, _readBuffer.Length);

            // Read samples from the ring buffer starting at _lastReadSample.
            _clip.GetData(_readBuffer, _lastReadSample);
            _lastReadSample = (_lastReadSample + available) % _clip.samples;

            // Downsample device rate -> 24 kHz with linear interpolation.
            // Use accumulator pattern: step = deviceRate / 24000.
            float step = (float)_deviceRate / TargetSampleRate;
            for (int i = 0; i < available; i++)
            {
                _resampleAccum += 1f;
                while (_resampleAccum >= step)
                {
                    _resampleAccum -= step;
                    if (_outFill >= _outBuffer.Length)
                        FlushChunks();
                    _outBuffer[_outFill++] = _readBuffer[i];
                }
            }
            FlushChunks();
        }

        void FlushChunks()
        {
            while (_outFill >= _chunkSampleCount)
            {
                if (silenceThreshold > 0f && IsSilent(_outBuffer, _chunkSampleCount, silenceThreshold))
                {
                    Array.Copy(_outBuffer, _chunkSampleCount, _outBuffer, 0, _outFill - _chunkSampleCount);
                    _outFill -= _chunkSampleCount;
                    continue;
                }

                var bytes = new byte[_chunkSampleCount * 2];
                for (int i = 0; i < _chunkSampleCount; i++)
                {
                    var s = Mathf.Clamp(_outBuffer[i], -1f, 1f);
                    short v = (short)Mathf.RoundToInt(s * short.MaxValue);
                    bytes[i * 2] = (byte)(v & 0xFF);
                    bytes[i * 2 + 1] = (byte)((v >> 8) & 0xFF);
                }
                var base64 = Convert.ToBase64String(bytes);
                OnAudioChunkBase64?.Invoke(base64);

                Array.Copy(_outBuffer, _chunkSampleCount, _outBuffer, 0, _outFill - _chunkSampleCount);
                _outFill -= _chunkSampleCount;
            }
        }

        static bool IsSilent(float[] buffer, int count, float threshold)
        {
            float sum = 0f;
            for (int i = 0; i < count; i++) sum += Mathf.Abs(buffer[i]);
            return (sum / count) < threshold;
        }

        void OnDisable()
        {
            StopCapture();
        }
    }
}
