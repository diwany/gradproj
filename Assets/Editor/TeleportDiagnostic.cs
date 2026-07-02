using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Museum.EditorTools
{
    public static class TeleportDiagnostic
    {
        const string MenuPath = "Tools/Museum/Diagnose Teleport Setup";

        [MenuItem(MenuPath)]
        public static void Diagnose()
        {
            var sb = new StringBuilder();
            int teleportXriLayerMask = UnityEngine.XR.Interaction.Toolkit.InteractionLayerMask.GetMask("Teleport");
            sb.AppendLine($"== XRI Interaction Layer 'Teleport' ==");
            sb.AppendLine(teleportXriLayerMask != 0 ? $"  Found (mask = {teleportXriLayerMask})" : "  MISSING — click Fix All on Project Validation");
            sb.AppendLine();

            sb.AppendLine("== TeleportationProvider in scene ==");
            var providers = Object.FindObjectsByType<TeleportationProvider>(FindObjectsInactive.Include);
            sb.AppendLine(providers.Length == 0 ? "  MISSING — XR Origin should have one" : $"  {providers.Length} found ({string.Join(", ", providers.Select(p => p.gameObject.name))})");
            sb.AppendLine();

            sb.AppendLine("== TeleportationArea components ==");
            var areas = Object.FindObjectsByType<TeleportationArea>(FindObjectsInactive.Include);
            if (areas.Length == 0)
            {
                sb.AppendLine("  NONE — nothing was added. Either the scene wasn't saved, or your selection had no MeshFilters under it.");
            }
            else
            {
                foreach (var a in areas)
                {
                    var col = a.GetComponent<Collider>();
                    var mc = col as MeshCollider;
                    int interactionLayerInt = a.interactionLayers;
                    string layerNames = ResolveInteractionLayers(interactionLayerInt);
                    sb.AppendLine($"  {GetPath(a.gameObject)}");
                    sb.AppendLine($"    InteractionLayerMask = {interactionLayerInt} [{layerNames}]");
                    sb.AppendLine($"    Collider = {(col == null ? "MISSING" : col.GetType().Name)}{(mc != null && mc.sharedMesh == null ? " (NO MESH)" : "")}{(col != null && col.isTrigger ? " (isTrigger=ON — teleport raycast won't hit triggers)" : "")}");
                    sb.AppendLine($"    Active in hierarchy = {a.gameObject.activeInHierarchy}");
                    sb.AppendLine($"    World bounds size = {(col != null ? col.bounds.size.ToString("F2") : "n/a")}");
                }
            }
            sb.AppendLine();

            sb.AppendLine("== Teleport Interactors (controller-side) ==");
            var rays = Object.FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor>(FindObjectsInactive.Include);
            if (rays.Length == 0)
            {
                sb.AppendLine("  No NearFarInteractor in scene. Looking for legacy XRRayInteractor...");
                var legacy = Object.FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>(FindObjectsInactive.Include);
                foreach (var r in legacy)
                    sb.AppendLine($"  XRRayInteractor: {GetPath(r.gameObject)} | InteractionLayers = {(int)r.interactionLayers} [{ResolveInteractionLayers(r.interactionLayers)}]");
            }
            else
            {
                foreach (var r in rays)
                    sb.AppendLine($"  NearFarInteractor: {GetPath(r.gameObject)} | InteractionLayers = {(int)r.interactionLayers} [{ResolveInteractionLayers(r.interactionLayers)}]");
            }

            Debug.Log("[Museum Teleport Diagnostic]\n" + sb);
            EditorUtility.DisplayDialog("Teleport Diagnostic",
                "Full report in Console. Summary:\n\n" +
                $"- Teleport XRI layer: {(teleportXriLayerMask != 0 ? "OK" : "MISSING")}\n" +
                $"- TeleportationProvider: {providers.Length} in scene\n" +
                $"- TeleportationAreas: {areas.Length}\n",
                "OK");
        }

        static string ResolveInteractionLayers(int mask)
        {
            var names = new List<string>();
            for (int i = 0; i < 32; i++)
            {
                if (((1 << i) & mask) != 0)
                {
                    var n = UnityEngine.XR.Interaction.Toolkit.InteractionLayerMask.LayerToName(i);
                    names.Add(string.IsNullOrEmpty(n) ? i.ToString() : $"{i}:{n}");
                }
            }
            return names.Count == 0 ? "none" : string.Join(", ", names);
        }

        static string GetPath(GameObject go)
        {
            var t = go.transform;
            var sb = new StringBuilder(go.name);
            while (t.parent != null) { t = t.parent; sb.Insert(0, t.name + "/"); }
            return sb.ToString();
        }
    }
}
