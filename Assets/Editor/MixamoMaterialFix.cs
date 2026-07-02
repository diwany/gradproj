using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Museum.EditorTools
{
    /// <summary>
    /// Mixamo FBXs ship with Built-in/Standard shaders that render untextured under HDRP.
    /// This menu extracts any embedded materials/textures, switches shaders to HDRP/Lit,
    /// and wires textures via Mixamo's "<materialName>_<channel>" naming convention
    /// (e.g. lambert1_B = BaseColor, lambert1_N = Normal, lambert1_M = Metallic, lambert1_R = Roughness).
    /// </summary>
    public static class MixamoMaterialFix
    {
        const string MenuPath = "Tools/Museum/Phase 6/Fix Mixamo Materials for HDRP";
        const string MixamoFolder = "Assets/Mixamo";
        const string MaterialsFolder = "Assets/Mixamo/Materials";
        const string TexturesFolder = "Assets/Mixamo/Textures";

        [MenuItem(MenuPath)]
        public static void FixMixamoMaterials()
        {
            var hdrpLit = Shader.Find("HDRP/Lit");
            if (hdrpLit == null)
            {
                EditorUtility.DisplayDialog("Mixamo Material Fix", "HDRP/Lit shader not found. Is HDRP installed?", "OK");
                return;
            }

            if (!AssetDatabase.IsValidFolder(MixamoFolder))
            {
                EditorUtility.DisplayDialog("Mixamo Material Fix", "Assets/Mixamo folder is missing.", "OK");
                return;
            }

            EnsureFolder(MaterialsFolder);
            EnsureFolder(TexturesFolder);

            int texturesExtracted = 0;
            int materialsExtracted = 0;
            int materialsConverted = 0;
            int normalsFixed = 0;
            var report = new List<string>();

            // 1. Extract embedded textures and materials from each FBX so we can edit them.
            var fbxGuids = AssetDatabase.FindAssets("t:Model", new[] { MixamoFolder });
            foreach (var guid in fbxGuids)
            {
                var fbxPath = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
                if (importer == null) continue;

                if (importer.ExtractTextures(TexturesFolder))
                {
                    texturesExtracted++;
                    report.Add($"  Extracted textures from {fbxPath} -> {TexturesFolder}");
                }

                // Re-import the FBX so it picks up the externalized textures.
                AssetDatabase.WriteImportSettingsIfDirty(fbxPath);
                AssetDatabase.ImportAsset(fbxPath, ImportAssetOptions.ForceUpdate);

                // Extract embedded materials into Assets/Mixamo/Materials/.
                var subAssets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
                foreach (var sub in subAssets)
                {
                    if (sub is Material embedded)
                    {
                        var matPath = $"{MaterialsFolder}/{embedded.name}.mat";
                        if (!File.Exists(matPath))
                        {
                            var err = AssetDatabase.ExtractAsset(embedded, matPath);
                            if (string.IsNullOrEmpty(err))
                            {
                                materialsExtracted++;
                                report.Add($"  Extracted material '{embedded.name}' -> {matPath}");
                            }
                            else
                            {
                                report.Add($"  Could not extract '{embedded.name}': {err}");
                            }
                        }
                    }
                }
                AssetDatabase.WriteImportSettingsIfDirty(fbxPath);
                AssetDatabase.ImportAsset(fbxPath, ImportAssetOptions.ForceUpdate);
            }
            AssetDatabase.Refresh();

            // 2. Build a texture lookup: full name -> Texture, plus prefix index (lambert1_B -> "lambert1").
            var texLookup = new Dictionary<string, Texture>();
            var textureGuids = AssetDatabase.FindAssets("t:Texture", new[] { MixamoFolder });
            foreach (var tg in textureGuids)
            {
                var tp = AssetDatabase.GUIDToAssetPath(tg);
                var tex = AssetDatabase.LoadAssetAtPath<Texture>(tp);
                if (tex != null) texLookup[tex.name] = tex;
            }

            // 3. Convert materials.
            var matGuids = AssetDatabase.FindAssets("t:Material", new[] { MixamoFolder });
            foreach (var mg in matGuids)
            {
                var matPath = AssetDatabase.GUIDToAssetPath(mg);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (mat == null) continue;

                // Try to capture textures from the previous shader before swapping.
                Texture priorBase = TryGetTexture(mat, "_BaseColorMap", "_BaseMap", "_MainTex");
                Texture priorNormal = TryGetTexture(mat, "_NormalMap", "_BumpMap");

                if (mat.shader == null || !mat.shader.name.StartsWith("HDRP/"))
                {
                    mat.shader = hdrpLit;
                    materialsConverted++;
                    report.Add($"  Shader -> HDRP/Lit on {matPath}");
                }

                Texture baseColor = priorBase;
                Texture normal = priorNormal;
                Texture metallic = null;
                Texture roughness = null;
                Texture maskCandidate = null;

                // Pass 1: textures whose name starts with the material's name (e.g. "lambert1_B").
                ClassifyTextures(texLookup, mat.name + "_", ref baseColor, ref normal, ref metallic, ref roughness, ref maskCandidate, requirePrefix: true);
                ClassifyTextures(texLookup, mat.name + " ", ref baseColor, ref normal, ref metallic, ref roughness, ref maskCandidate, requirePrefix: true);

                // Pass 2: fuzzy fallback. If we still found nothing, accept ANY texture in
                // Assets/Mixamo/ whose name ends with the channel suffix. Works for the common case
                // of a single character with one texture set whose prefix doesn't match the material.
                bool fuzzyUsed = false;
                if (baseColor == null && normal == null && metallic == null && maskCandidate == null)
                {
                    ClassifyTextures(texLookup, "", ref baseColor, ref normal, ref metallic, ref roughness, ref maskCandidate, requirePrefix: false);
                    fuzzyUsed = baseColor != null || normal != null || metallic != null;
                }

                if (baseColor != null && mat.HasProperty("_BaseColorMap"))
                {
                    mat.SetTexture("_BaseColorMap", baseColor);
                }
                // HDRP/Lit defaults _BaseColor to white if there's a BaseColorMap; force it explicitly
                // so a stray dark color from the original Standard shader doesn't multiply things to black.
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);

                if (normal != null && mat.HasProperty("_NormalMap"))
                {
                    if (EnsureNormalImport(normal)) normalsFixed++;
                    mat.SetTexture("_NormalMap", normal);
                    if (mat.HasProperty("_NormalScale")) mat.SetFloat("_NormalScale", 1f);
                }
                // Use the explicit MaskMap if Mixamo provided one, otherwise fall back to the metallic
                // texture (its R channel is the metallic input HDRP expects in the MaskMap).
                Texture maskMap = maskCandidate ?? metallic;
                if (maskMap != null && mat.HasProperty("_MaskMap"))
                {
                    mat.SetTexture("_MaskMap", maskMap);
                }
                else
                {
                    if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
                }

                if (mat.HasProperty("_Smoothness") && maskMap == null)
                    mat.SetFloat("_Smoothness", 0.25f);

                report.Add(
                    $"  Wired {matPath}\n" +
                    $"    BaseColor:{(baseColor != null ? baseColor.name : "<none>")}" +
                    $"  Normal:{(normal != null ? normal.name : "<none>")}" +
                    $"  Mask:{(maskMap != null ? maskMap.name : "<none>")}" +
                    (fuzzyUsed ? "  (fuzzy match)" : ""));

                EditorUtility.SetDirty(mat);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string msg =
                $"Textures extracted from FBX: {texturesExtracted}\n" +
                $"Materials extracted from FBX: {materialsExtracted}\n" +
                $"Materials converted to HDRP/Lit: {materialsConverted}\n" +
                $"Normal-map import flags fixed: {normalsFixed}\n\n" +
                "Per-material wire report (truncated; full version in Console):\n" +
                string.Join("\n", report.Take(8));

            Debug.Log("[Mixamo Material Fix]\n" + string.Join("\n", report));
            EditorUtility.DisplayDialog("Mixamo Material Fix", msg, "OK");
        }

        static void ClassifyTextures(Dictionary<string, Texture> lookup, string requiredPrefix,
            ref Texture baseColor, ref Texture normal, ref Texture metallic, ref Texture roughness, ref Texture mask,
            bool requirePrefix)
        {
            foreach (var kv in lookup)
            {
                var tName = kv.Key;
                if (requirePrefix && !tName.StartsWith(requiredPrefix)) continue;

                var lname = tName.ToLowerInvariant();
                var matchPart = requirePrefix
                    ? lname.Substring(requiredPrefix.Length)
                    : lname;

                if (baseColor == null && (
                    matchPart.Contains("basecolor") || matchPart.Contains("base_color") ||
                    matchPart.Contains("diffuse") || matchPart.Contains("albedo") ||
                    matchPart == "b" || matchPart.EndsWith("_b")))
                {
                    baseColor = kv.Value;
                }
                else if (normal == null && (
                    matchPart.Contains("normal") ||
                    matchPart == "n" || matchPart.EndsWith("_n")))
                {
                    normal = kv.Value;
                }
                else if (metallic == null && (
                    matchPart.Contains("metallic") || matchPart.Contains("metalness") ||
                    matchPart == "m" || matchPart.EndsWith("_m")))
                {
                    metallic = kv.Value;
                }
                else if (roughness == null && (
                    matchPart.Contains("roughness") ||
                    matchPart == "r" || matchPart.EndsWith("_r")))
                {
                    roughness = kv.Value;
                }
                else if (mask == null && (
                    matchPart.Contains("mask") || matchPart.Contains("_ao") ||
                    matchPart.Contains("occlusion")))
                {
                    mask = kv.Value;
                }
            }
        }

        static Texture TryGetTexture(Material mat, params string[] propertyNames)
        {
            foreach (var p in propertyNames)
            {
                if (mat.HasProperty(p))
                {
                    var t = mat.GetTexture(p);
                    if (t != null) return t;
                }
            }
            return null;
        }

        static bool EnsureNormalImport(Texture normal)
        {
            var path = AssetDatabase.GetAssetPath(normal);
            if (string.IsNullOrEmpty(path)) return false;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return false;
            if (importer.textureType == TextureImporterType.NormalMap) return false;
            importer.textureType = TextureImporterType.NormalMap;
            importer.SaveAndReimport();
            return true;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace("\\", "/");
            var folder = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
