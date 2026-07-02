using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Museum.EditorTools
{
    public static class HdrpSampleMaterialFix
    {
        const string MenuPath = "Tools/Museum/Fix Sample Materials for HDRP";
        const string TargetFolder = "Assets/Samples";

        [MenuItem(MenuPath)]
        public static void FixMaterials()
        {
            var hdrpLit = Shader.Find("HDRP/Lit");
            var hdrpUnlit = Shader.Find("HDRP/Unlit");
            if (hdrpLit == null)
            {
                EditorUtility.DisplayDialog("Sample Material Fix", "HDRP/Lit shader not found. Is HDRP installed?", "OK");
                return;
            }

            var guids = AssetDatabase.FindAssets("t:Material", new[] { TargetFolder });
            int converted = 0, skippedHdrp = 0, skippedNoShader = 0;
            var report = new List<string>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) continue;
                if (mat.shader == null) { skippedNoShader++; continue; }

                var name = mat.shader.name;
                if (name.StartsWith("HDRP/") || name.StartsWith("Shader Graphs/HDRP")) { skippedHdrp++; continue; }

                Color baseColor = ExtractBaseColor(mat);
                Texture mainTex = ExtractMainTex(mat);
                bool isUnlitSource = name.Contains("Unlit") || name.Contains("Particle") || name.EndsWith("/UI") || name == "UI/Default";

                mat.shader = isUnlitSource ? (hdrpUnlit ?? hdrpLit) : hdrpLit;

                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
                if (mainTex != null && mat.HasProperty("_BaseColorMap")) mat.SetTexture("_BaseColorMap", mainTex);

                EditorUtility.SetDirty(mat);
                converted++;
                report.Add($"  {path}\n    {name} -> {mat.shader.name}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string msg = $"Converted {converted} material(s) to HDRP shaders.\n" +
                         $"Already HDRP: {skippedHdrp}\nNo shader: {skippedNoShader}\n";
            if (converted > 0)
            {
                msg += "\nDetails (also in Console):\n" + string.Join("\n", report.GetRange(0, Mathf.Min(report.Count, 8)));
                if (report.Count > 8) msg += $"\n... and {report.Count - 8} more.";
            }
            Debug.Log("[Museum HDRP Material Fix]\n" + string.Join("\n", report));
            EditorUtility.DisplayDialog("Sample Material Fix", msg, "OK");
        }

        static Color ExtractBaseColor(Material mat)
        {
            if (mat.HasProperty("_BaseColor")) return mat.GetColor("_BaseColor");
            if (mat.HasProperty("_Color")) return mat.GetColor("_Color");
            if (mat.HasProperty("_TintColor")) return mat.GetColor("_TintColor");
            return Color.white;
        }

        static Texture ExtractMainTex(Material mat)
        {
            if (mat.HasProperty("_BaseColorMap")) { var t = mat.GetTexture("_BaseColorMap"); if (t != null) return t; }
            if (mat.HasProperty("_BaseMap")) { var t = mat.GetTexture("_BaseMap"); if (t != null) return t; }
            if (mat.HasProperty("_MainTex")) { var t = mat.GetTexture("_MainTex"); if (t != null) return t; }
            return null;
        }
    }
}
