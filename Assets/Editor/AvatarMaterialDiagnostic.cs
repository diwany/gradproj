using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Museum.EditorTools
{
    /// <summary>
    /// Diagnoses and repairs the Tour Guide Avatar's mesh renderers:
    /// reports each material slot, replaces null/error-shader materials with lambert2.mat,
    /// and re-wires BaseColor / Normal / Mask textures from Assets/Mixamo/Textures by
    /// suffix matching. Also kills duplicate "Tour Guide Avatar" GameObjects so we end up
    /// with exactly one.
    /// </summary>
    public static class AvatarMaterialDiagnostic
    {
        const string MenuPath = "Tools/Museum/Phase 6/Diagnose & Repair Avatar Materials";
        const string MaterialsFolder = "Assets/Mixamo/Materials";
        const string MixamoFolder = "Assets/Mixamo";

        [MenuItem(MenuPath)]
        public static void Run()
        {
            var report = new List<string>();
            var hdrpLit = Shader.Find("HDRP/Lit");

            // 1. Collect avatar roots (anything named "Tour Guide Avatar").
            var avatars = GameObject.FindObjectsByType<Transform>(FindObjectsInactive.Include)
                .Where(t => t.name == "Tour Guide Avatar")
                .Select(t => t.gameObject)
                .ToList();

            if (avatars.Count == 0)
            {
                Done("No 'Tour Guide Avatar' in the active scene. Run Wire Mixamo Avatar first.", report);
                return;
            }

            // 2. Drop duplicates (keep the first one with at least one SkinnedMeshRenderer).
            GameObject keep = avatars.FirstOrDefault(a => a.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length > 0)
                            ?? avatars[0];
            foreach (var dup in avatars)
            {
                if (dup != keep)
                {
                    report.Add($"  Destroyed duplicate avatar: {GetPath(dup.transform)}");
                    Undo.DestroyObjectImmediate(dup);
                }
            }
            report.Add($"  Kept avatar: {GetPath(keep.transform)}");

            // 3. Pick the canonical material — prefer Mixamo lambert2 if it exists, otherwise the first .mat in Assets/Mixamo/Materials/.
            Material fallbackMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsFolder}/lambert2.mat");
            if (fallbackMat == null)
            {
                var anyGuid = AssetDatabase.FindAssets("t:Material", new[] { MaterialsFolder }).FirstOrDefault();
                if (anyGuid != null) fallbackMat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(anyGuid));
            }

            // 4. Walk every renderer under the avatar and report / repair.
            var skinned = keep.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var nonSkinned = keep.GetComponentsInChildren<MeshRenderer>(true);
            int slotsRepaired = 0;
            int texturesRewired = 0;

            // Build texture lookup once (recursive across Mixamo).
            var texLookup = new Dictionary<string, Texture>();
            foreach (var g in AssetDatabase.FindAssets("t:Texture", new[] { MixamoFolder }))
            {
                var tp = AssetDatabase.GUIDToAssetPath(g);
                var t = AssetDatabase.LoadAssetAtPath<Texture>(tp);
                if (t != null) texLookup[t.name] = t;
            }

            foreach (var smr in skinned)
            {
                report.Add($"\n  SkinnedMeshRenderer: {GetPath(smr.transform)}  mesh={(smr.sharedMesh != null ? smr.sharedMesh.name : "<null>")}");
                slotsRepaired += InspectAndRepair(smr, fallbackMat, hdrpLit, texLookup, report, ref texturesRewired);
            }
            foreach (var mr in nonSkinned)
            {
                report.Add($"\n  MeshRenderer: {GetPath(mr.transform)}");
                slotsRepaired += InspectAndRepair(mr, fallbackMat, hdrpLit, texLookup, report, ref texturesRewired);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();

            string summary =
                $"Avatars in scene: {avatars.Count} (kept 1, removed {avatars.Count - 1})\n" +
                $"SkinnedMeshRenderers: {skinned.Length}\n" +
                $"MeshRenderers: {nonSkinned.Length}\n" +
                $"Material slots repaired: {slotsRepaired}\n" +
                $"Texture slots re-wired: {texturesRewired}\n\n" +
                "Full report (also in Console):\n" +
                string.Join("\n", report.Take(20));
            Done(summary, report);
        }

        static int InspectAndRepair(Renderer renderer, Material fallback, Shader hdrpLit,
            Dictionary<string, Texture> texLookup, List<string> report, ref int texRewires)
        {
            var mats = renderer.sharedMaterials;
            int repaired = 0;
            for (int i = 0; i < mats.Length; i++)
            {
                var mat = mats[i];
                if (mat == null)
                {
                    if (fallback != null)
                    {
                        report.Add($"    [{i}] <NULL> -> assigned {fallback.name}");
                        mats[i] = fallback;
                        repaired++;
                    }
                    else
                    {
                        report.Add($"    [{i}] <NULL> and no fallback material in {MaterialsFolder}");
                    }
                    continue;
                }

                string shaderName = mat.shader != null ? mat.shader.name : "<no shader>";
                bool hidden = shaderName.StartsWith("Hidden/InternalErrorShader") || shaderName == "<no shader>";

                if (hidden && fallback != null)
                {
                    report.Add($"    [{i}] {mat.name} (BROKEN shader '{shaderName}') -> assigned {fallback.name}");
                    mats[i] = fallback;
                    repaired++;
                    mat = fallback;
                    shaderName = mat.shader != null ? mat.shader.name : "<no shader>";
                }

                // Convert any non-HDRP shader on the assigned material.
                if (hdrpLit != null && mat.shader != null && !mat.shader.name.StartsWith("HDRP/"))
                {
                    Texture priorBase = TryGet(mat, "_BaseColorMap", "_BaseMap", "_MainTex");
                    Texture priorNormal = TryGet(mat, "_NormalMap", "_BumpMap");
                    mat.shader = hdrpLit;
                    if (priorBase != null && mat.HasProperty("_BaseColorMap")) mat.SetTexture("_BaseColorMap", priorBase);
                    if (priorNormal != null && mat.HasProperty("_NormalMap")) mat.SetTexture("_NormalMap", priorNormal);
                }
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);

                Texture currentBase = mat.HasProperty("_BaseColorMap") ? mat.GetTexture("_BaseColorMap") : null;
                Texture currentNormal = mat.HasProperty("_NormalMap") ? mat.GetTexture("_NormalMap") : null;
                Texture currentMask = mat.HasProperty("_MaskMap") ? mat.GetTexture("_MaskMap") : null;

                Texture pickedBase = currentBase, pickedNormal = currentNormal, pickedMask = currentMask;
                ClassifyAll(texLookup, ref pickedBase, ref pickedNormal, ref pickedMask);

                bool changed = false;
                if (currentBase == null && pickedBase != null && mat.HasProperty("_BaseColorMap"))
                {
                    mat.SetTexture("_BaseColorMap", pickedBase);
                    texRewires++;
                    changed = true;
                }
                if (currentNormal == null && pickedNormal != null && mat.HasProperty("_NormalMap"))
                {
                    EnsureNormalImport(pickedNormal);
                    mat.SetTexture("_NormalMap", pickedNormal);
                    texRewires++;
                    changed = true;
                }
                if (currentMask == null && pickedMask != null && mat.HasProperty("_MaskMap"))
                {
                    mat.SetTexture("_MaskMap", pickedMask);
                    texRewires++;
                    changed = true;
                }

                report.Add(
                    $"    [{i}] {mat.name}  shader={shaderName}\n" +
                    $"        Base={Name(pickedBase)}  Normal={Name(pickedNormal)}  Mask={Name(pickedMask)}");

                if (changed) EditorUtility.SetDirty(mat);
            }
            renderer.sharedMaterials = mats;
            EditorUtility.SetDirty(renderer);
            return repaired;
        }

        static void ClassifyAll(Dictionary<string, Texture> lookup,
            ref Texture baseColor, ref Texture normal, ref Texture mask)
        {
            // Pick the first texture in the lookup matching each role; gentle "contains" matching.
            foreach (var kv in lookup)
            {
                var lname = kv.Key.ToLowerInvariant();
                if (baseColor == null && (
                    lname.Contains("basecolor") || lname.Contains("base_color") ||
                    lname.Contains("diffuse") || lname.Contains("albedo") || lname.EndsWith("_b")))
                {
                    baseColor = kv.Value;
                }
                else if (normal == null && (lname.Contains("normal") || lname.EndsWith("_n")))
                {
                    normal = kv.Value;
                }
                else if (mask == null && (
                    lname.Contains("metallic") || lname.Contains("metalness") ||
                    lname.Contains("mask") || lname.EndsWith("_m")))
                {
                    mask = kv.Value;
                }
            }
        }

        static Texture TryGet(Material mat, params string[] props)
        {
            foreach (var p in props)
            {
                if (mat.HasProperty(p))
                {
                    var t = mat.GetTexture(p);
                    if (t != null) return t;
                }
            }
            return null;
        }

        static void EnsureNormalImport(Texture tex)
        {
            var path = AssetDatabase.GetAssetPath(tex);
            if (string.IsNullOrEmpty(path)) return;
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) return;
            if (imp.textureType == TextureImporterType.NormalMap) return;
            imp.textureType = TextureImporterType.NormalMap;
            imp.SaveAndReimport();
        }

        static string Name(Texture t) => t != null ? t.name : "<none>";

        static string GetPath(Transform t)
        {
            var path = t.name;
            while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
            return path;
        }

        static void Done(string summary, List<string> report)
        {
            Debug.Log("[Avatar Material Diagnostic]\n" + string.Join("\n", report));
            EditorUtility.DisplayDialog("Avatar Materials", summary, "OK");
        }
    }
}
