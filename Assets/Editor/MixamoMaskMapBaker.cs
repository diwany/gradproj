using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Museum.EditorTools
{
    /// <summary>
    /// Bakes a proper HDRP MaskMap (R=Metallic, G=AO, B=Detail, A=Smoothness) from
    /// the separate Metallic and Roughness textures Mixamo ships, then wires it into
    /// every Mixamo material. Replaces the incorrect metallic-only-as-MaskMap path
    /// the auto-fix uses by default.
    /// </summary>
    public static class MixamoMaskMapBaker
    {
        const string MenuPath = "Tools/Museum/Phase 6/Bake Mixamo MaskMap";
        const string MixamoFolder = "Assets/Mixamo";
        const string TexturesFolder = "Assets/Mixamo/Textures";
        const string OutputPath = "Assets/Mixamo/Textures/lambert1_MaskMap.png";

        [MenuItem(MenuPath)]
        public static void Bake()
        {
            // 1. Find the source textures by name suffix.
            var texGuids = AssetDatabase.FindAssets("t:Texture", new[] { MixamoFolder });
            Texture2D metallic = null, roughness = null, ao = null;
            foreach (var g in texGuids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                if (p == OutputPath) continue; // ignore prior bake output
                var t = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                if (t == null) continue;
                var lname = t.name.ToLowerInvariant();
                if (metallic == null && (lname.Contains("metallic") || lname.Contains("metalness") || lname.EndsWith("_m"))) metallic = t;
                else if (roughness == null && (lname.Contains("roughness") || lname.EndsWith("_r"))) roughness = t;
                else if (ao == null && (lname.Contains("_ao") || lname.Contains("occlusion") || lname.Contains("ambient"))) ao = t;
            }

            if (metallic == null && roughness == null)
            {
                EditorUtility.DisplayDialog("Bake MaskMap",
                    "Couldn't find a Metallic OR Roughness texture in Assets/Mixamo. Nothing to bake.", "OK");
                return;
            }

            // 2. Make sources readable so we can sample pixels.
            EnsureReadable(metallic);
            EnsureReadable(roughness);
            EnsureReadable(ao);

            int w = (metallic ?? roughness ?? ao).width;
            int h = (metallic ?? roughness ?? ao).height;

            Color32[] mPx = metallic != null ? metallic.GetPixels32() : null;
            Color32[] rPx = roughness != null ? roughness.GetPixels32() : null;
            Color32[] aoPx = ao != null ? ao.GetPixels32() : null;

            // Resize on the fly if dimensions don't match — sample by ratio. Mixamo ships matched resolutions almost always.
            int len = w * h;
            var pixels = new Color32[len];
            for (int i = 0; i < len; i++)
            {
                byte mVal = mPx != null ? Sample(mPx, i, len, mPx.Length) : (byte)0;
                byte rVal = rPx != null ? Sample(rPx, i, len, rPx.Length) : (byte)128;
                byte aoVal = aoPx != null ? Sample(aoPx, i, len, aoPx.Length) : (byte)255;
                byte smooth = (byte)(255 - rVal); // smoothness = 1 - roughness
                pixels[i] = new Color32(mVal, aoVal, 0, smooth);
            }

            var mask = new Texture2D(w, h, TextureFormat.RGBA32, mipChain: true, linear: true);
            mask.SetPixels32(pixels);
            mask.Apply();
            var bytes = mask.EncodeToPNG();
            Object.DestroyImmediate(mask);

            // 3. Write file and import as linear (non-sRGB), no compression to keep channel fidelity.
            File.WriteAllBytes(Path.GetFullPath(OutputPath), bytes);
            AssetDatabase.ImportAsset(OutputPath, ImportAssetOptions.ForceUpdate);
            var imp = (TextureImporter)AssetImporter.GetAtPath(OutputPath);
            imp.sRGBTexture = false;
            imp.textureType = TextureImporterType.Default;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.SaveAndReimport();

            var maskAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(OutputPath);

            // 4. Wire onto every Mixamo material.
            int wired = 0;
            var matGuids = AssetDatabase.FindAssets("t:Material", new[] { MixamoFolder });
            foreach (var mg in matGuids)
            {
                var mp = AssetDatabase.GUIDToAssetPath(mg);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(mp);
                if (mat == null) continue;
                if (!mat.HasProperty("_MaskMap")) continue;
                mat.SetTexture("_MaskMap", maskAsset);
                // Tell HDRP/Lit to actually sample the MaskMap (otherwise it falls back to slider values).
                mat.EnableKeyword("_MASKMAP");
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 1f);     // sliders multiply MaskMap
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 1f);
                EditorUtility.SetDirty(mat);
                wired++;
            }
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Bake MaskMap",
                $"Baked {OutputPath}\n" +
                $"  Source Metallic: {(metallic != null ? metallic.name : "<none>")}\n" +
                $"  Source Roughness: {(roughness != null ? roughness.name : "<none>")}\n" +
                $"  Source AO: {(ao != null ? ao.name : "<none, defaulted to 1.0>")}\n" +
                $"  Resolution: {w}x{h}\n\n" +
                $"Wired into {wired} material(s).\n\n" +
                "Avatar should now show correct gloss on gold accents and matte fabric.",
                "OK");
        }

        static byte Sample(Color32[] px, int i, int targetLen, int srcLen)
        {
            if (srcLen == targetLen) return px[i].r;
            // Different resolution: nearest-neighbor by linear ratio.
            int idx = (int)((long)i * srcLen / targetLen);
            return px[Mathf.Clamp(idx, 0, srcLen - 1)].r;
        }

        static void EnsureReadable(Texture2D tex)
        {
            if (tex == null) return;
            var path = AssetDatabase.GetAssetPath(tex);
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) return;
            bool dirty = false;
            if (!imp.isReadable) { imp.isReadable = true; dirty = true; }
            // Force uncompressed so channel data isn't smeared by DXT.
            if (imp.textureCompression != TextureImporterCompression.Uncompressed)
            {
                imp.textureCompression = TextureImporterCompression.Uncompressed;
                dirty = true;
            }
            if (dirty) imp.SaveAndReimport();
        }
    }
}
