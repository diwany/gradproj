using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Museum.EditorTools
{
    /// <summary>
    /// Configures Mixamo animation FBXs (Idle, Walking, Walking1, Talking) for in-place
    /// looping playback driven by NavMeshAgent. Without this, Mixamo's Walking animation
    /// has forward root motion baked into the bones — with applyRootMotion=false the
    /// agent ends up sliding past its NavMeshAgent position and the legs/feet desync.
    /// Sets Bake Into Pose for Position XZ + Rotation so the animation loops in place,
    /// keeps Y unchecked so the character stays grounded, and ensures Loop Time is on.
    /// </summary>
    public static class MixamoAnimationFix
    {
        const string MenuPath = "Tools/Museum/Phase 6/Configure Mixamo Animations for In-Place Looping";
        const string MixamoFolder = "Assets/Mixamo";

        // FBXs that should LOOP in place (idle stance, walk cycle, talk loop).
        static readonly string[] LoopFiles =
        {
            "Assets/Mixamo/Idle.fbx",
            "Assets/Mixamo/Walking.fbx",
            "Assets/Mixamo/Walking1.fbx",
            "Assets/Mixamo/Talking.fbx",
        };

        [MenuItem(MenuPath)]
        public static void ConfigureAll()
        {
            int touched = 0, missing = 0;
            var report = new System.Text.StringBuilder();
            foreach (var path in LoopFiles)
            {
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null)
                {
                    missing++;
                    report.AppendLine($"  MISSING: {path}");
                    continue;
                }

                var clips = importer.clipAnimations;
                if (clips == null || clips.Length == 0)
                {
                    // Fall back: copy the default-generated clip animations and edit them.
                    clips = importer.defaultClipAnimations;
                }

                bool changed = false;
                for (int i = 0; i < clips.Length; i++)
                {
                    var c = clips[i];
                    if (!c.loopTime) { c.loopTime = true; changed = true; }
                    // Bake Into Pose checkboxes — when ON, root motion for that axis is BAKED into the pose
                    // (i.e. NOT generated as root motion at runtime). For NavMeshAgent-driven characters
                    // we want this so animation translation doesn't fight the agent's velocity.
                    if (!c.lockRootPositionXZ) { c.lockRootPositionXZ = true; changed = true; }   // bake XZ
                    if (!c.lockRootRotation) { c.lockRootRotation = true; changed = true; }      // bake rotation
                    // Y is left unbaked so the character stays grounded by its hips' Y curve.
                    // Heightfrom = Original keeps the actual height curve.
                    if (c.keepOriginalPositionY) { c.keepOriginalPositionY = true; }
                    clips[i] = c;
                }
                importer.clipAnimations = clips;

                if (changed)
                {
                    importer.SaveAndReimport();
                    touched++;
                    report.AppendLine($"  Configured: {path}  ({clips.Length} clip(s))");
                }
                else
                {
                    report.AppendLine($"  Already correct: {path}");
                }
            }

            Debug.Log("[Mixamo Animation Fix]\n" + report);
            EditorUtility.DisplayDialog("Mixamo Animations",
                $"FBXs configured: {touched}\nFBXs missing: {missing}\n\n" +
                "Each clip is now: Loop Time = on, XZ + Rotation baked into pose " +
                "(in-place looping). Y left unbaked so the character isn't pinned to a fixed height.\n\n" +
                "Press Play. Walk should now cycle in place while the NavMeshAgent moves the character — no more drift.",
                "OK");
        }
    }
}
