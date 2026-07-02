using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityGLTF;

public static class GltfBatchExporter
{
    private const string OutputRoot = "C:/Users/K/Documents/project/assets/glb";
    private const string SourceRoot = "Assets/AK Studio Art";

    [MenuItem("Tools/Export AK Studio Art to glTF")]
    public static void ExportAll()
    {
        Directory.CreateDirectory(OutputRoot);

        var prefabPaths = AssetDatabase.FindAssets("t:Prefab", new[] { SourceRoot })
            .Select(AssetDatabase.GUIDToAssetPath)
            .OrderBy(p => p)
            .ToArray();

        Debug.Log($"[GltfBatchExporter] Found {prefabPaths.Length} prefabs under {SourceRoot}.");

        int success = 0, failure = 0;
        var failures = new System.Collections.Generic.List<string>();

        for (int i = 0; i < prefabPaths.Length; i++)
        {
            var path = prefabPaths[i];
            GameObject instance = null;
            try
            {
                instance = PrefabUtility.LoadPrefabContents(path);
                if (instance == null)
                {
                    Debug.LogWarning($"[GltfBatchExporter] [{i+1}/{prefabPaths.Length}] Skip (null): {path}");
                    failure++;
                    failures.Add(path + " (null)");
                    continue;
                }

                var rel = path.Substring(SourceRoot.Length).TrimStart('/');
                var relDir = Path.GetDirectoryName(rel)?.Replace('\\', '/') ?? "";
                var outDir = string.IsNullOrEmpty(relDir) ? OutputRoot : (OutputRoot + "/" + relDir);
                Directory.CreateDirectory(outDir);

                var fileName = Path.GetFileNameWithoutExtension(path);
                var exportOpts = new ExportContext { TexturePathRetriever = AssetDatabase.GetAssetPath };
                var exporter = new GLTFSceneExporter(new[] { instance.transform }, exportOpts);
                exporter.SaveGLB(outDir, fileName);

                success++;
                Debug.Log($"[GltfBatchExporter] [{i+1}/{prefabPaths.Length}] OK: {fileName} -> {outDir}");
            }
            catch (Exception e)
            {
                failure++;
                failures.Add(path + " : " + e.Message);
                Debug.LogError($"[GltfBatchExporter] [{i+1}/{prefabPaths.Length}] FAIL {path}: {e}");
            }
            finally
            {
                if (instance != null)
                {
                    PrefabUtility.UnloadPrefabContents(instance);
                }
            }
        }

        Debug.Log($"[GltfBatchExporter] DONE. Success={success}, Failure={failure}");
        if (failures.Count > 0)
        {
            Debug.LogWarning("[GltfBatchExporter] Failed items:\n  " + string.Join("\n  ", failures));
        }

        File.WriteAllText(
            Path.Combine(OutputRoot, "_export_summary.txt"),
            $"Exported {success}/{prefabPaths.Length} prefabs at {DateTime.Now:O}\n" +
            (failures.Count > 0 ? ("Failures:\n" + string.Join("\n", failures)) : "No failures."));
    }

    [MenuItem("Tools/Export Demo Scenes to glTF")]
    public static void ExportScene()
    {
        var scenes = new (string path, string outName)[]
        {
            ("Assets/AK Studio Art/Egyptian Museum VR/Scenes/Demo.unity", "EgyptianMuseumVR_Demo"),
            ("Assets/AK Studio Art/Egypt Pharoah Bust/Scenes/Demo.unity", "EgyptPharoahBust_Demo"),
        };

        var outDir = OutputRoot + "/Scenes";
        Directory.CreateDirectory(outDir);

        foreach (var (path, outName) in scenes)
        {
            try
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                var roots = scene.GetRootGameObjects();
                int totalGo = 0, totalMeshFilters = 0, totalRenderers = 0;
                foreach (var root in roots)
                {
                    totalGo += root.GetComponentsInChildren<Transform>(true).Length;
                    totalMeshFilters += root.GetComponentsInChildren<MeshFilter>(true).Length;
                    totalRenderers += root.GetComponentsInChildren<Renderer>(true).Length;
                }
                Debug.Log($"[GltfBatchExporter] Scene '{path}' loaded: {roots.Length} roots, {totalGo} transforms, {totalMeshFilters} MeshFilters, {totalRenderers} Renderers");

                var transforms = roots.Select(r => r.transform).ToArray();
                var exportOpts = new ExportContext { TexturePathRetriever = AssetDatabase.GetAssetPath };
                var exporter = new GLTFSceneExporter(transforms, exportOpts);
                exporter.SaveGLB(outDir, outName);

                var resultPath = Path.Combine(outDir, outName + ".glb");
                var size = File.Exists(resultPath) ? new FileInfo(resultPath).Length : 0;
                Debug.Log($"[GltfBatchExporter] Scene exported to {resultPath} ({size:N0} bytes)");
            }
            catch (Exception e)
            {
                Debug.LogError($"[GltfBatchExporter] FAIL exporting {path}: {e}");
            }
        }
    }
}
