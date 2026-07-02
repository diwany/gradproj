using UnityEngine;

namespace Museum.Artifact
{
    /// <summary>
    /// Disabled camera parented to the XR head. On demand, renders a single 224x224 frame
    /// to a RenderTexture for ML classification. Avoid calling every frame — Render() is
    /// synchronous and blocks the main thread.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class ArtifactCaptureCamera : MonoBehaviour
    {
        [Tooltip("Output resolution. Standard ImageNet/ResNet input is 224x224.")]
        public int resolution = 224;

        [Tooltip("Field of view used for the capture. Narrower than the player's full FOV so the artifact dominates the frame — closer to what an ML classifier was trained on.")]
        [Range(10f, 110f)] public float captureFov = 35f;

        [Tooltip("Layers the capture camera will render. Should include artifacts but exclude UI / XR rig / debug gizmos.")]
        public LayerMask cullingMask = ~0;

        Camera _camera;
        RenderTexture _renderTexture;
        Texture2D _readback;

        public RenderTexture RenderTexture => _renderTexture;
        public int Resolution => resolution;

        void Awake()
        {
            _camera = GetComponent<Camera>();
            _camera.enabled = false; // we drive Render() manually
            _camera.fieldOfView = captureFov;
            _camera.cullingMask = cullingMask;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = Color.black;
            _camera.nearClipPlane = 0.05f;
            _camera.farClipPlane = 50f;

            _renderTexture = new RenderTexture(resolution, resolution, 16, RenderTextureFormat.ARGB32)
            {
                name = "ArtifactCapture_RT",
                useMipMap = false,
                autoGenerateMips = false,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            _renderTexture.Create();
            _camera.targetTexture = _renderTexture;
        }

        void OnDestroy()
        {
            if (_camera != null) _camera.targetTexture = null;
            if (_renderTexture != null) _renderTexture.Release();
            if (_readback != null) Destroy(_readback);
        }

        /// <summary>
        /// Render a single frame from the current head pose into the RenderTexture.
        /// Returns the RenderTexture (alive until next CaptureNow / OnDestroy).
        /// </summary>
        public RenderTexture CaptureNow()
        {
            if (_camera == null) return null;
            _camera.fieldOfView = captureFov;
            _camera.cullingMask = cullingMask;
            _camera.Render();
            return _renderTexture;
        }

        /// <summary>
        /// Same as CaptureNow but also reads pixels back to a Texture2D for inspection or
        /// for ML pipelines that want a Texture2D rather than a RenderTexture.
        /// </summary>
        public Texture2D CaptureNowReadback()
        {
            CaptureNow();
            if (_readback == null || _readback.width != resolution || _readback.height != resolution)
            {
                if (_readback != null) Destroy(_readback);
                _readback = new Texture2D(resolution, resolution, TextureFormat.RGB24, false, false);
            }
            var prev = RenderTexture.active;
            RenderTexture.active = _renderTexture;
            _readback.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0, false);
            _readback.Apply(false, false);
            RenderTexture.active = prev;
            return _readback;
        }
    }
}
