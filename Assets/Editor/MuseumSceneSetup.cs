using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Museum.EditorTools
{
    public static class MuseumSceneSetup
    {
        const string MenuPath = "Tools/Museum/Set Up Museum Scene";

        const string SourceScene = "Assets/AK Studio Art/Egyptian Museum VR/Scenes/Demo.unity";
        const string TargetSceneFolder = "Assets/Scenes";
        const string TargetScene = "Assets/Scenes/Museum.unity";
        const string XrOriginPrefab = "Assets/Samples/XR Interaction Toolkit/3.0.8/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";

        [MenuItem(MenuPath)]
        public static void SetUpMuseumScene()
        {
            if (!File.Exists(SourceScene))
            {
                Fail($"Source scene not found:\n{SourceScene}");
                return;
            }
            if (!File.Exists(XrOriginPrefab))
            {
                Fail($"XR Origin prefab not found at:\n{XrOriginPrefab}\n\n" +
                     "Run Tools > Museum > Import XR Starter Assets first, then retry.");
                return;
            }

            if (!AssetDatabase.IsValidFolder(TargetSceneFolder))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            if (!File.Exists(TargetScene))
            {
                if (!AssetDatabase.CopyAsset(SourceScene, TargetScene))
                {
                    Fail($"Failed to copy scene:\n{SourceScene} -> {TargetScene}");
                    return;
                }
                AssetDatabase.Refresh();
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var scene = EditorSceneManager.OpenScene(TargetScene, OpenSceneMode.Single);

            int removed = 0;
            Vector3 spawnPos = Vector3.zero;
            bool spawnPosFound = false;
            foreach (var cam in CollectNonXrCameras(scene))
            {
                if (!spawnPosFound)
                {
                    var t = cam.transform;
                    spawnPos = new Vector3(t.position.x, 0f, t.position.z);
                    spawnPosFound = true;
                }
                Object.DestroyImmediate(cam.gameObject);
                removed++;
            }
            if (removed > 0)
                Debug.Log($"[Museum Scene Setup] Removed {removed} non-XR camera GameObject(s). Spawn floor pos: {spawnPos}");

            var existingOrigin = Object.FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (existingOrigin != null)
            {
                Debug.Log("[Museum Scene Setup] XR Origin already in scene; reusing.");
            }
            else
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(XrOriginPrefab);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                instance.transform.position = spawnPos;
                instance.transform.rotation = Quaternion.identity;
                Debug.Log($"[Museum Scene Setup] Spawned XR Origin at {spawnPos}");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            AddSceneToBuildSettings(TargetScene);

            EditorUtility.DisplayDialog(
                "Museum Scene Ready",
                $"Museum.unity is set up:\n" +
                $"- {(removed > 0 ? $"Removed {removed} non-XR camera(s)" : "No non-XR cameras present")}\n" +
                "- XR Origin (XR Rig) present\n" +
                "- Scene added to Build Settings\n\n" +
                "Connect a Meta Quest via Quest Link, press Play. You should see the museum and your hands.",
                "OK");
        }

        static List<Camera> CollectNonXrCameras(Scene scene)
        {
            var result = new List<Camera>();
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var cam in root.GetComponentsInChildren<Camera>(true))
                {
                    if (cam.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>(true) == null)
                        result.Add(cam);
                }
            }
            return result;
        }

        static void AddSceneToBuildSettings(string scenePath)
        {
            var current = EditorBuildSettings.scenes;
            foreach (var s in current)
                if (s.path == scenePath) return;

            var list = new List<EditorBuildSettingsScene>(current);
            list.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = list.ToArray();
        }

        static void Fail(string msg)
        {
            Debug.LogError("[Museum Scene Setup] " + msg);
            EditorUtility.DisplayDialog("Museum Scene Setup", msg, "OK");
        }
    }
}
