using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Museum.Artifact;

namespace Museum.EditorTools
{
    public static class ArtifactClassMapping
    {
        const string AssignFromLabelsMenu = "Tools/Museum/Phase 3/Assign Class Indices From Labels File";
        const string WirePipelineMenu = "Tools/Museum/Phase 3/Wire ML Recognition Pipeline";

        [MenuItem(AssignFromLabelsMenu)]
        public static void AssignClassIndicesFromLabelsFile()
        {
            var path = EditorUtility.OpenFilePanel(
                "Select labels file (one displayName per line, line N = class N)",
                Application.dataPath, "txt,csv,json");
            if (string.IsNullOrEmpty(path)) return;

            string[] rawLines = File.ReadAllLines(path);
            var lines = new List<string>();
            foreach (var l in rawLines)
            {
                var trimmed = l.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;
                lines.Add(trimmed);
            }
            if (lines.Count == 0)
            {
                EditorUtility.DisplayDialog("Class Index Assignment", "Labels file is empty.", "OK");
                return;
            }

            var infos = Object.FindObjectsByType<ArtifactInfo>(FindObjectsInactive.Include);
            var infoByName = new Dictionary<string, ArtifactInfo>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var info in infos)
            {
                var key = string.IsNullOrEmpty(info.displayName) ? info.gameObject.name : info.displayName;
                if (infoByName.ContainsKey(key))
                    Debug.LogWarning($"[Class Mapping] Duplicate displayName '{key}' — only one will receive a classIndex.");
                infoByName[key] = info;
            }

            int matched = 0, unmatchedLabels = 0;
            var report = new System.Text.StringBuilder();
            for (int i = 0; i < lines.Count; i++)
            {
                if (infoByName.TryGetValue(lines[i], out var info))
                {
                    Undo.RecordObject(info, "Assign Class Index");
                    info.classIndex = i;
                    EditorUtility.SetDirty(info);
                    matched++;
                    report.AppendLine($"  {i,4} -> {info.displayName}");
                }
                else
                {
                    unmatchedLabels++;
                    report.AppendLine($"  {i,4} ! NOT FOUND IN SCENE: \"{lines[i]}\"");
                }
            }

            int unassigned = 0;
            var assigned = new HashSet<ArtifactInfo>();
            foreach (var info in infos) if (info.classIndex >= 0) assigned.Add(info);
            foreach (var info in infos)
                if (info.classIndex < 0) { unassigned++; report.AppendLine($"  ??? NO LABEL FOR: \"{info.displayName}\""); }

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log($"[Class Mapping] {path}\n{report}");
            EditorUtility.DisplayDialog("Class Index Assignment",
                $"Labels: {lines.Count}\nMatched: {matched}\nLabels with no matching artifact: {unmatchedLabels}\nArtifacts with no matching label: {unassigned}\n\nFull report in Console.\nScene saved.",
                "OK");
        }

        [MenuItem(WirePipelineMenu)]
        public static void WirePipeline()
        {
            var detector = Object.FindAnyObjectByType<GazeArtifactDetector>();
            if (detector == null)
            {
                EditorUtility.DisplayDialog("Wire ML Pipeline",
                    "No GazeArtifactDetector in scene. Run Set Up Gaze Detection first.", "OK");
                return;
            }

            var captureCam = Object.FindAnyObjectByType<ArtifactCaptureCamera>();
            if (captureCam == null)
            {
                EditorUtility.DisplayDialog("Wire ML Pipeline",
                    "No ArtifactCaptureCamera in scene. Run Add Artifact Capture Camera first.", "OK");
                return;
            }

            var classifier = Object.FindAnyObjectByType<InferenceEngineArtifactClassifier>();
            if (classifier == null)
            {
                classifier = Undo.AddComponent<InferenceEngineArtifactClassifier>(captureCam.gameObject);
                Debug.Log("[Wire ML Pipeline] Added InferenceEngineArtifactClassifier on capture camera. Drag your ONNX ModelAsset onto its 'modelAsset' field.");
            }

            var existingSpawner = detector.GetComponent<ArtifactLabelSpawner>();
            ArtifactLabel labelPrefab = existingSpawner != null ? existingSpawner.labelPrefab : null;
            if (existingSpawner != null)
            {
                Undo.DestroyObjectImmediate(existingSpawner);
                Debug.Log("[Wire ML Pipeline] Replaced ArtifactLabelSpawner with ArtifactRecognitionPipeline.");
            }

            var pipeline = detector.GetComponent<ArtifactRecognitionPipeline>();
            if (pipeline == null) pipeline = Undo.AddComponent<ArtifactRecognitionPipeline>(detector.gameObject);
            pipeline.captureCamera = captureCam;
            pipeline.classifier = classifier;
            if (pipeline.labelPrefab == null) pipeline.labelPrefab = labelPrefab;

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Selection.activeGameObject = pipeline.gameObject;
            EditorGUIUtility.PingObject(pipeline);

            EditorUtility.DisplayDialog("Wire ML Pipeline",
                "Pipeline wired:\n" +
                $"- GazeArtifactDetector: {detector.gameObject.name}\n" +
                $"- ArtifactCaptureCamera: {captureCam.gameObject.name}\n" +
                $"- Classifier: {(classifier.modelAsset != null ? classifier.modelAsset.name : "NEEDS MODELASSET")}\n" +
                $"- Label prefab: {(pipeline.labelPrefab != null ? pipeline.labelPrefab.name : "NEEDS PREFAB")}\n\n" +
                "Next: drop your ONNX file into Unity (it imports as ModelAsset), drag it onto the classifier's modelAsset field, run Assign Class Indices From Labels File with your labels.txt, press Play.",
                "OK");
        }
    }
}
