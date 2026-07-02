using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace Museum.EditorTools
{
    public static class TeleportSetup
    {
        const string MarkAreaMenu = "Tools/Museum/Mark Selected as Teleportation Area";
        const string ClearMenu = "Tools/Museum/Clear Teleportation Components from Selection";
        const string TeleportLayerName = "Teleport";

        [MenuItem(MarkAreaMenu)]
        public static void MarkSelectedAsArea()
        {
            var selected = Selection.gameObjects;
            if (selected == null || selected.Length == 0)
            {
                EditorUtility.DisplayDialog("Teleportation Setup",
                    "Select one or more GameObjects in the Hierarchy first.\n" +
                    "Pick the floor / corridor / zone-floor meshes (each must have a MeshFilter or be a parent that contains them).",
                    "OK");
                return;
            }

            var areaMask = UnityEngine.XR.Interaction.Toolkit.InteractionLayerMask.GetMask("Default", TeleportLayerName);
            if (areaMask == 0)
            {
                EditorUtility.DisplayDialog("Teleportation Setup",
                    $"XRI Interaction Layer \"{TeleportLayerName}\" not found. Run Project Validation > Fix All first (Edit > Project Settings > XR Plug-in Management > Project Validation).",
                    "OK");
                return;
            }

            int areasAdded = 0;
            int collidersAdded = 0;
            int interactorsTagged = 0;
            var report = new List<string>();

            foreach (var root in selected)
            {
                var meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
                if (meshFilters.Length == 0)
                {
                    report.Add($"  SKIP {root.name} — no MeshFilter under this GameObject");
                    continue;
                }

                foreach (var mf in meshFilters)
                {
                    var go = mf.gameObject;
                    Undo.RecordObject(go, "Mark Teleportation Area");

                    if (go.GetComponent<MeshCollider>() == null)
                    {
                        var col = Undo.AddComponent<MeshCollider>(go);
                        col.sharedMesh = mf.sharedMesh;
                        collidersAdded++;
                    }

                    if (go.GetComponent<TeleportationArea>() == null)
                    {
                        var area = Undo.AddComponent<TeleportationArea>(go);
                        area.interactionLayers = areaMask;
                        areasAdded++;
                        report.Add($"  + {GetPath(go)} (TeleportationArea, Interaction Layer = Default+Teleport)");
                    }
                    else
                    {
                        report.Add($"  = {GetPath(go)} (already had TeleportationArea)");
                    }
                }
            }

            // Also tag all controller interactors with the Teleport layer so dedicated teleport interactors find the areas.
            interactorsTagged = TagInteractorsWithMask(areaMask);

            var activeScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);

            string msg = $"Added {areasAdded} TeleportationArea component(s).\n" +
                         $"Added {collidersAdded} MeshCollider(s) where missing.\n" +
                         $"Tagged {interactorsTagged} controller interactor(s) with Teleport layer.\n" +
                         "Scene saved.\n\n" +
                         (report.Count == 0 ? "" : "Details (full list in Console):\n" + string.Join("\n", report.GetRange(0, Mathf.Min(report.Count, 8))) + (report.Count > 8 ? $"\n... and {report.Count - 8} more." : ""));
            Debug.Log("[Museum Teleport Setup]\n" + string.Join("\n", report));
            EditorUtility.DisplayDialog("Teleportation Setup", msg, "OK");
        }

        [MenuItem(ClearMenu)]
        public static void ClearTeleportationFromSelection()
        {
            var selected = Selection.gameObjects;
            if (selected == null || selected.Length == 0) return;
            int removed = 0;
            foreach (var root in selected)
            {
                foreach (var area in root.GetComponentsInChildren<TeleportationArea>(true))
                {
                    Undo.DestroyObjectImmediate(area);
                    removed++;
                }
            }
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorUtility.DisplayDialog("Teleportation Setup", $"Removed {removed} TeleportationArea component(s).\n(MeshColliders left in place — undo manually if needed.)", "OK");
        }

        static int TagInteractorsWithMask(int extraMask)
        {
            int tagged = 0;
            foreach (var interactor in Object.FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>(FindObjectsInactive.Include))
            {
                int current = interactor.interactionLayers;
                int updated = current | extraMask;
                if (current != updated)
                {
                    Undo.RecordObject(interactor, "Tag Interactor With Teleport Layer");
                    interactor.interactionLayers = updated;
                    EditorUtility.SetDirty(interactor);
                    tagged++;
                }
            }
            return tagged;
        }

        static string GetPath(GameObject go)
        {
            var t = go.transform;
            var sb = new System.Text.StringBuilder(go.name);
            while (t.parent != null) { t = t.parent; sb.Insert(0, t.name + "/"); }
            return sb.ToString();
        }
    }
}
