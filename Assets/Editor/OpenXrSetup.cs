using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;

namespace Museum.EditorTools
{
    public static class OpenXrSetup
    {
        const string MenuPath = "Tools/Museum/Configure OpenXR (Quest)";
        const string OpenXrLoader = "UnityEngine.XR.OpenXR.OpenXRLoader";

        [MenuItem(MenuPath)]
        public static void Configure()
        {
            var summary = new System.Text.StringBuilder();

            var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
            if (generalSettings == null)
            {
                XRGeneralSettingsPerBuildTarget perBuild;
                EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out perBuild);
                if (perBuild == null)
                {
                    perBuild = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                    AssetDatabase.CreateAsset(perBuild, "Assets/XR/XRGeneralSettingsPerBuildTarget.asset");
                    EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, perBuild, true);
                }
                perBuild.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.Standalone);
                generalSettings = perBuild.SettingsForBuildTarget(BuildTargetGroup.Standalone);
                summary.AppendLine("- Created XR General Settings for Standalone");
            }

            if (generalSettings.AssignedSettings == null)
            {
                var managerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
                managerSettings.name = "Standalone Providers";
                AssetDatabase.AddObjectToAsset(managerSettings, AssetDatabase.GetAssetPath(generalSettings));
                generalSettings.AssignedSettings = managerSettings;
                EditorUtility.SetDirty(generalSettings);
                summary.AppendLine("- Created XRManagerSettings");
            }

            bool loaderAssigned = XRPackageMetadataStore.AssignLoader(
                generalSettings.AssignedSettings, OpenXrLoader, BuildTargetGroup.Standalone);
            summary.AppendLine($"- OpenXR loader assigned: {loaderAssigned}");

            var openxrSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Standalone);
            if (openxrSettings == null)
            {
                summary.AppendLine("! OpenXRSettings not found for Standalone — package may still be importing.");
            }
            else
            {
                int enabled = 0;
                foreach (var f in openxrSettings.GetFeatures<OculusTouchControllerProfile>())          { f.enabled = true; enabled++; }
                foreach (var f in openxrSettings.GetFeatures<MetaQuestTouchPlusControllerProfile>())   { f.enabled = true; enabled++; }
                foreach (var f in openxrSettings.GetFeatures<MetaQuestTouchProControllerProfile>())    { f.enabled = true; enabled++; }
                EditorUtility.SetDirty(openxrSettings);
                summary.AppendLine($"- Quest interaction profiles enabled: {enabled} (Oculus Touch + Quest Touch Plus + Quest Touch Pro)");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Museum OpenXR Setup]\n" + summary);
            EditorUtility.DisplayDialog(
                "Museum OpenXR Setup",
                summary.ToString() +
                "\nVerify under: Edit > Project Settings > XR Plug-in Management > PC, Mac & Linux Standalone tab\n" +
                "OpenXR should be checked and the OpenXR sub-page should show the three Quest interaction profiles enabled.",
                "OK");
        }
    }
}
