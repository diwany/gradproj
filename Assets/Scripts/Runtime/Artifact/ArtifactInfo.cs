using UnityEngine;

namespace Museum.Artifact
{
    /// <summary>
    /// Tag component on every artifact in the museum. Identifies the object to the gaze detector
    /// and supplies display data shown in the world-space label and read by the tour guide.
    /// </summary>
    public class ArtifactInfo : MonoBehaviour
    {
        [Tooltip("Class index in the ONNX classifier. Used to verify ML predictions match this object during dev. Set to -1 if unknown.")]
        public int classIndex = -1;

        [Tooltip("Display name shown on the floating label.")]
        public string displayName;

        [Tooltip("Era / period text shown beneath the name.")]
        public string era;

        [TextArea(3, 8)]
        [Tooltip("Long-form description used by the tour guide (Phase 5) when narrating this artifact.")]
        public string description;

        void Reset()
        {
            if (string.IsNullOrEmpty(displayName))
                displayName = gameObject.name;
        }
    }
}
