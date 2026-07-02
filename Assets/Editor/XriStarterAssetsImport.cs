using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace Museum.EditorTools
{
    public static class XriStarterAssetsImport
    {
        const string MenuPath = "Tools/Museum/Import XR Starter Assets";
        const string PackageName = "com.unity.xr.interaction.toolkit";
        const string SampleName = "Starter Assets";

        [MenuItem(MenuPath)]
        public static void ImportStarterAssets()
        {
            var samples = Sample.FindByPackage(PackageName, string.Empty);
            Sample target = default;
            bool found = false;
            foreach (var s in samples)
            {
                if (s.displayName == SampleName)
                {
                    target = s;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                EditorUtility.DisplayDialog(
                    "Museum XR Starter Assets",
                    $"Could not find sample \"{SampleName}\" in {PackageName}.\n\n" +
                    "Open Window > Package Manager, select XR Interaction Toolkit, expand Samples, and import \"Starter Assets\" manually.",
                    "OK");
                return;
            }

            if (target.isImported)
            {
                EditorUtility.DisplayDialog(
                    "Museum XR Starter Assets",
                    $"\"{SampleName}\" is already imported at:\n{target.importPath}\n\n" +
                    "If you want a fresh copy, delete that folder first and run this again.",
                    "OK");
                return;
            }

            bool ok = target.Import(Sample.ImportOptions.OverridePreviousImports);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Museum XR Starter Assets",
                ok
                    ? $"Imported \"{SampleName}\" to:\n{target.importPath}\n\n" +
                      "Wait for Unity to finish compiling, then run\nTools > Museum > Set Up Museum Scene"
                    : "Import failed. Check Console for details.",
                "OK");
        }
    }
}
