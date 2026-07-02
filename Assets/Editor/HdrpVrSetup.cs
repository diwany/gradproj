using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Museum.EditorTools
{
    public static class HdrpVrSetup
    {
        const string MenuPath = "Tools/Museum/Apply HDRP VR Settings";

        [MenuItem(MenuPath)]
        public static void ApplyHdrpVrSettings()
        {
            var assets = FindAllHdrpAssets();
            if (assets.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "Museum HDRP VR Setup",
                    "No HDRenderPipelineAsset assets found in the project.",
                    "OK");
                return;
            }

            int modified = 0;
            foreach (var asset in assets)
            {
                if (ApplyVrSettings(asset))
                {
                    EditorUtility.SetDirty(asset);
                    modified++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Museum HDRP VR Setup",
                $"Configured {modified} HDRP asset(s) for VR.\n\n" +
                "Disabled: Raytracing, SSR, SSGI, Volumetric Clouds.\n" +
                "Set: Lit Shader Mode = Forward Only.\n\n" +
                "Volumetric Fog left enabled at the asset level; tune per-volume in scene profiles.\n" +
                "Screen Space Shadows: toggle manually in HDRP Asset Inspector if visible (this HDRP version doesn't expose it via API).\n" +
                "XR Single-Pass Instanced is configured under Project Settings > XR Plug-in Management > OpenXR (Phase 1).",
                "OK");
        }

        static List<HDRenderPipelineAsset> FindAllHdrpAssets()
        {
            var result = new List<HDRenderPipelineAsset>();
            var guids = AssetDatabase.FindAssets("t:HDRenderPipelineAsset");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<HDRenderPipelineAsset>(path);
                if (asset != null)
                    result.Add(asset);
            }
            return result;
        }

        static bool ApplyVrSettings(HDRenderPipelineAsset asset)
        {
            var settings = asset.currentPlatformRenderPipelineSettings;
            bool changed = false;

            if (settings.supportRayTracing) { settings.supportRayTracing = false; changed = true; }
            if (settings.supportSSR) { settings.supportSSR = false; changed = true; }
            if (settings.supportSSGI) { settings.supportSSGI = false; changed = true; }
            if (settings.supportVolumetricClouds) { settings.supportVolumetricClouds = false; changed = true; }

            if (settings.supportedLitShaderMode != RenderPipelineSettings.SupportedLitShaderMode.ForwardOnly)
            {
                settings.supportedLitShaderMode = RenderPipelineSettings.SupportedLitShaderMode.ForwardOnly;
                changed = true;
            }

            if (changed)
                asset.currentPlatformRenderPipelineSettings = settings;

            Debug.Log($"[Museum HDRP VR Setup] {AssetDatabase.GetAssetPath(asset)} — changed: {changed}");
            return changed;
        }
    }
}
