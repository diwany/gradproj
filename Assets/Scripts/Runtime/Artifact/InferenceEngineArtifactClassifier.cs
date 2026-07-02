using System;
using System.Collections.Generic;
using Unity.InferenceEngine;
using UnityEngine;

namespace Museum.Artifact
{
    /// <summary>
    /// Loads an ONNX image classifier and runs inference on a single texture per request.
    /// The model is expected to:
    /// - Accept input shape NCHW = [1, 3, 224, 224]  (configurable via inputResolution)
    /// - Have ImageNet normalization (mean/std) baked into the ONNX graph as a preprocessing layer
    ///   (Inference Engine 2.6 dropped the C#-side mean/std setters; do this in PyTorch before export)
    /// - Output a softmax (or logits — argmax behaves the same) over N classes
    /// </summary>
    [DisallowMultipleComponent]
    public class InferenceEngineArtifactClassifier : MonoBehaviour, IDisposable
    {
        [Header("Model")]
        [Tooltip("Drag your imported .onnx (ModelAsset) here.")]
        public ModelAsset modelAsset;

        [Tooltip("Backend. GPUCompute is fastest in-Editor; falls back to CPU if compute shaders unavailable.")]
        public BackendType backend = BackendType.GPUCompute;

        [Header("Input shape")]
        [Tooltip("Image side length the model expects. Standard ImageNet classifiers use 224.")]
        public int inputResolution = 224;

        Worker _worker;
        Tensor<float> _input;
        bool _ready;

        public bool IsReady => _ready;

        void Awake()
        {
            TryLoad();
        }

        void OnDestroy() => Dispose();

        public void Dispose()
        {
            _worker?.Dispose();
            _worker = null;
            _input?.Dispose();
            _input = null;
            _ready = false;
        }

        public bool TryLoad()
        {
            Dispose();
            if (modelAsset == null)
            {
                Debug.LogWarning("[InferenceEngineArtifactClassifier] No ModelAsset assigned. Classifier disabled.");
                return false;
            }
            try
            {
                var model = ModelLoader.Load(modelAsset);
                _worker = new Worker(model, backend);
                _input = new Tensor<float>(new TensorShape(1, 3, inputResolution, inputResolution));
                _ready = true;
                Debug.Log($"[InferenceEngineArtifactClassifier] Loaded {modelAsset.name}; backend={backend}; input=1x3x{inputResolution}x{inputResolution}.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[InferenceEngineArtifactClassifier] Failed to load model: {e}");
                Dispose();
                return false;
            }
        }

        public struct Prediction
        {
            public int classIndex;
            public float confidence;
            public float[] allScores;
        }

        /// <summary>
        /// Runs synchronous inference on the given texture. Returns top-1 class + softmax-style score.
        /// Texture is resampled to inputResolution x inputResolution by Inference Engine.
        /// </summary>
        public bool TryClassify(Texture inputTexture, out Prediction prediction)
        {
            prediction = default;
            if (!_ready || _worker == null || _input == null)
            {
                if (!TryLoad()) return false;
            }
            if (inputTexture == null) return false;

            var transform = new TextureTransform()
                .SetCoordOrigin(CoordOrigin.TopLeft)
                .SetTensorLayout(TensorLayout.NCHW);

            TextureConverter.ToTensor(inputTexture, _input, transform);
            _worker.Schedule(_input);

            var output = _worker.PeekOutput() as Tensor<float>;
            if (output == null)
            {
                Debug.LogError("[InferenceEngineArtifactClassifier] PeekOutput returned null or non-float tensor.");
                return false;
            }
            output.CompleteAllPendingOperations();
            var data = output.DownloadToArray();
            if (data == null || data.Length == 0) return false;

            int topIndex = 0;
            float topScore = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                if (data[i] > topScore) { topScore = data[i]; topIndex = i; }
            }

            prediction = new Prediction
            {
                classIndex = topIndex,
                confidence = topScore,
                allScores = data
            };
            return true;
        }
    }
}
