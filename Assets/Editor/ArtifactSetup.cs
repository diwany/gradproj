using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using Museum.Artifact;

namespace Museum.EditorTools
{
    public static class ArtifactSetup
    {
        const string TagSelectedMenu = "Tools/Museum/Phase 3/Tag Selected as Artifact";
        const string SetupGazeMenu = "Tools/Museum/Phase 3/Set Up Gaze Detection in Active Scene";

        const string LabelPrefabPath = "Assets/Prefabs/UI/ArtifactLabel.prefab";

        [MenuItem(TagSelectedMenu)]
        public static void TagSelectedAsArtifact()
        {
            var selected = Selection.gameObjects;
            if (selected == null || selected.Length == 0)
            {
                EditorUtility.DisplayDialog("Tag Artifact",
                    "Select one or more GameObjects in the Hierarchy first.\n\n" +
                    "Each selected GameObject will get an ArtifactInfo + a Collider so the gaze raycast can detect it.\n\n" +
                    "After tagging, fill in displayName / era / description in the Inspector for each.",
                    "OK");
                return;
            }

            int added = 0, skipped = 0, collidersAdded = 0;
            foreach (var go in selected)
            {
                if (go.GetComponent<ArtifactInfo>() != null) { skipped++; continue; }
                Undo.RecordObject(go, "Tag As Artifact");
                var info = Undo.AddComponent<ArtifactInfo>(go);
                if (string.IsNullOrEmpty(info.displayName))
                    info.displayName = go.name;
                added++;

                if (go.GetComponentInChildren<Collider>(true) == null)
                {
                    if (go.GetComponent<MeshFilter>() != null)
                        Undo.AddComponent<MeshCollider>(go);
                    else
                        Undo.AddComponent<BoxCollider>(go);
                    collidersAdded++;
                }
            }
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            EditorUtility.DisplayDialog("Tag Artifact",
                $"Tagged {added} GameObject(s) as Artifact.\nSkipped (already tagged): {skipped}\nColliders added where missing: {collidersAdded}\n\nScene saved.\n\n" +
                "Now fill in displayName, era, and description in the Inspector — or use Tools > Museum > Phase 3 > Auto-Fill Artifact Info to populate them all from a generated mapping.",
                "OK");
        }

        const string TightenGazeMenu = "Tools/Museum/Phase 3/Tighten Gaze Range to 2m";

        [MenuItem(TightenGazeMenu)]
        public static void TightenGazeRange()
        {
            var detector = Object.FindAnyObjectByType<GazeArtifactDetector>();
            if (detector == null)
            {
                EditorUtility.DisplayDialog("Tighten Gaze Range", "No GazeArtifactDetector in active scene. Run Set Up Gaze Detection first.", "OK");
                return;
            }
            UnityEditor.Undo.RecordObject(detector, "Tighten Gaze Range");
            detector.maxDistance = 2f;
            UnityEditor.EditorUtility.SetDirty(detector);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            EditorUtility.DisplayDialog("Tighten Gaze Range",
                $"GazeArtifactDetector.maxDistance set to 2m on '{detector.gameObject.name}'.\n\n" +
                "Now you have to be within 2m of an artifact for the ray to reach it — the through-wall ghost-firing should stop.",
                "OK");
        }

        const string DumpMenu = "Tools/Museum/Phase 3/Dump Tagged Artifact Names";
        const string DumpPath = "Assets/Editor/_artifact_names.txt";

        [MenuItem(DumpMenu)]
        public static void DumpTaggedNames()
        {
            var infos = Object.FindObjectsByType<ArtifactInfo>(FindObjectsInactive.Include);
            if (infos.Length == 0)
            {
                EditorUtility.DisplayDialog("Dump Artifact Names",
                    "No ArtifactInfo components in the active scene. Tag some GameObjects first via Phase 3 > Tag Selected as Artifact.",
                    "OK");
                return;
            }
            System.Array.Sort(infos, (a, b) => string.Compare(a.gameObject.name, b.gameObject.name, System.StringComparison.OrdinalIgnoreCase));
            var lines = new List<string>();
            foreach (var info in infos)
            {
                var path = GetPath(info.gameObject);
                var pos = info.transform.position;
                lines.Add($"{info.gameObject.name}\t{path}\t{pos.x:F1}\t{pos.y:F1}\t{pos.z:F1}");
            }
            var content = $"# {infos.Length} ArtifactInfo components — name<TAB>scene-path<TAB>x<TAB>y<TAB>z\n" + string.Join("\n", lines);
            File.WriteAllText(DumpPath, content);
            AssetDatabase.Refresh();
            Debug.Log("[Museum] Wrote artifact dump:\n" + content);
            EditorUtility.DisplayDialog("Dump Artifact Names",
                $"Wrote {infos.Length} entries to:\n{DumpPath}\n\nFull list also in Console — copy from there to extend the artifact catalog in ArtifactInfoAutoFill.cs.",
                "OK");
        }

        static string GetPath(GameObject go)
        {
            var t = go.transform;
            var sb = new System.Text.StringBuilder(go.name);
            while (t.parent != null) { t = t.parent; sb.Insert(0, t.name + "/"); }
            return sb.ToString();
        }

        [MenuItem(SetupGazeMenu)]
        public static void SetUpGazeDetection()
        {
            var origin = Object.FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (origin == null)
            {
                EditorUtility.DisplayDialog("Set Up Gaze Detection",
                    "No XR Origin found in active scene. Open Museum.unity first.", "OK");
                return;
            }

            var labelPrefab = LoadOrCreateLabelPrefab();
            if (labelPrefab == null) return;

            var existingDetector = Object.FindAnyObjectByType<GazeArtifactDetector>();
            GameObject host;
            if (existingDetector != null)
            {
                host = existingDetector.gameObject;
            }
            else
            {
                host = new GameObject("Gaze Artifact Detection");
                host.transform.SetParent(origin.transform, false);
                Undo.RegisterCreatedObjectUndo(host, "Create Gaze Detection");
            }

            var detector = host.GetComponent<GazeArtifactDetector>() ?? Undo.AddComponent<GazeArtifactDetector>(host);
            if (origin.Camera != null)
                detector.gazeOrigin = origin.Camera.transform;

            var spawner = host.GetComponent<ArtifactLabelSpawner>() ?? Undo.AddComponent<ArtifactLabelSpawner>(host);
            spawner.labelPrefab = labelPrefab.GetComponent<ArtifactLabel>();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            Selection.activeGameObject = host;
            EditorGUIUtility.PingObject(host);

            EditorUtility.DisplayDialog("Set Up Gaze Detection",
                $"Gaze detection is wired:\n" +
                $"- Host: {host.name} (under XR Origin)\n" +
                $"- gazeOrigin = XR camera\n" +
                $"- maxDistance = {detector.maxDistance}m, dwell = {detector.dwellSeconds}s, cooldown = {detector.cooldownSeconds}s\n" +
                $"- Label prefab: {AssetDatabase.GetAssetPath(labelPrefab)}\n\n" +
                "Tag artifacts with Tools > Museum > Phase 3 > Tag Selected as Artifact, then press Play and stare at one for 1.5s.",
                "OK");
        }

        const string RegenerateLabelMenu = "Tools/Museum/Phase 3/Regenerate Label Prefab";

        [MenuItem(RegenerateLabelMenu)]
        public static void RegenerateLabelPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(LabelPrefabPath) != null)
                AssetDatabase.DeleteAsset(LabelPrefabPath);
            var prefab = LoadOrCreateLabelPrefab();
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("Regenerate Label Prefab", "Failed.", "OK");
                return;
            }

            // Re-wire any existing ArtifactLabelSpawner in the active scene; deleting the old prefab broke their reference.
            int rewired = 0;
            var label = prefab.GetComponent<ArtifactLabel>();
            foreach (var spawner in Object.FindObjectsByType<ArtifactLabelSpawner>(FindObjectsInactive.Include))
            {
                Undo.RecordObject(spawner, "Rewire Label Prefab");
                spawner.labelPrefab = label;
                EditorUtility.SetDirty(spawner);
                rewired++;
            }
            if (rewired > 0)
            {
                var scene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            EditorUtility.DisplayDialog("Regenerate Label Prefab",
                $"Recreated:\n{LabelPrefabPath}\n\nRe-wired {rewired} ArtifactLabelSpawner(s) in scene.",
                "OK");
        }

        static GameObject LoadOrCreateLabelPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(LabelPrefabPath);
            if (existing != null) return existing;

            var folder = Path.GetDirectoryName(LabelPrefabPath).Replace('\\', '/');
            EnsureFolder(folder);

            // Museum aesthetic: dark stone card with gold border, warm-white name + gold era.
            var stone = new Color(0.09f, 0.07f, 0.05f, 0.92f);   // deep dark brown
            var gold = new Color(0.92f, 0.74f, 0.36f, 1.0f);     // antique gold
            var nameColor = new Color(0.98f, 0.96f, 0.90f, 1.0f); // warm cream-white
            var eraColor = new Color(0.92f, 0.74f, 0.36f, 1.0f);  // gold
            var shadow = new Color(0f, 0f, 0f, 0.65f);            // drop shadow

            var root = new GameObject("ArtifactLabel");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0.7f, 0.22f);
            // Pixel-density scale: 1 unit = 1 meter, so set localScale to 1 and let RectTransform sizeDelta express size in meters.
            root.transform.localScale = Vector3.one;
            // Smaller scale on the canvas reference unit so TMP fontSizes feel natural (default TMP sizes assume pixels).
            canvas.GetComponent<RectTransform>().localScale = Vector3.one;

            var cg = root.AddComponent<CanvasGroup>();

            // Drop shadow plate (slightly larger, behind everything)
            AddImage(root.transform, "Shadow", shadow,
                anchorMin: new Vector2(0f, 0f), anchorMax: new Vector2(1f, 1f),
                offsetMin: new Vector2(-0.02f, -0.025f), offsetMax: new Vector2(0.02f, 0.015f));

            // Gold border: just a slightly larger gold rect under the cream rect
            AddImage(root.transform, "Border", gold,
                anchorMin: new Vector2(0f, 0f), anchorMax: new Vector2(1f, 1f),
                offsetMin: new Vector2(-0.006f, -0.006f), offsetMax: new Vector2(0.006f, 0.006f));

            // Dark stone card
            AddImage(root.transform, "Card", stone,
                anchorMin: new Vector2(0f, 0f), anchorMax: new Vector2(1f, 1f),
                offsetMin: Vector2.zero, offsetMax: Vector2.zero);

            // Thin gold separator between name and era
            AddImage(root.transform, "Separator", gold,
                anchorMin: new Vector2(0.10f, 0.42f), anchorMax: new Vector2(0.90f, 0.45f),
                offsetMin: Vector2.zero, offsetMax: Vector2.zero);

            // Name (top half) — large warm-white text
            var nameTmp = AddTmp(root.transform, "Name", "Artifact Name",
                fontSize: 0.045f, color: nameColor, alignment: TextAlignmentOptions.Center,
                fontStyle: FontStyles.Bold,
                anchorMin: new Vector2(0.04f, 0.46f), anchorMax: new Vector2(0.96f, 0.96f));

            // Era (bottom half) — gold italic
            var eraTmp = AddTmp(root.transform, "Era", "Era",
                fontSize: 0.028f, color: eraColor,
                alignment: TextAlignmentOptions.Center,
                fontStyle: FontStyles.Italic,
                anchorMin: new Vector2(0.04f, 0.06f), anchorMax: new Vector2(0.96f, 0.40f));

            var label = root.AddComponent<ArtifactLabel>();
            label.nameLabel = nameTmp;
            label.eraLabel = eraTmp;
            label.canvasGroup = cg;

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, LabelPrefabPath);
            Object.DestroyImmediate(root);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return prefab;
        }

        static void AddImage(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = color;
            img.raycastTarget = false;
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = offsetMin;
            r.offsetMax = offsetMax;
        }

        static TextMeshProUGUI AddTmp(Transform parent, string name, string text, float fontSize, Color color, TextAlignmentOptions alignment, FontStyles fontStyle, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = alignment;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = fontStyle;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            return tmp;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            string cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = cur + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }
    }
}
