using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace Museum.EditorTools
{
    public static class NavMeshBake
    {
        const string MenuPath = "Tools/Museum/Phase 6/Bake Museum NavMesh";
        const string NavMeshGoName = "Museum NavMesh";

        [MenuItem(MenuPath)]
        public static void BakeNavMesh()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog("NavMesh Bake", "No active scene.", "OK");
                return;
            }

            // Reuse existing NavMesh container if present.
            var go = GameObject.Find(NavMeshGoName);
            if (go == null)
            {
                go = new GameObject(NavMeshGoName);
                Undo.RegisterCreatedObjectUndo(go, "Add NavMesh Surface");
            }

            var surface = go.GetComponent<NavMeshSurface>() ?? Undo.AddComponent<NavMeshSurface>(go);
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
            surface.layerMask = ~0;
            surface.defaultArea = 0; // Walkable
            surface.agentTypeID = 0;  // Humanoid (default)

            EditorUtility.SetDirty(surface);
            surface.BuildNavMesh();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);

            var hasData = surface.navMeshData != null;
            EditorUtility.DisplayDialog("NavMesh Bake",
                hasData
                    ? "NavMesh baked.\n\n" +
                      $"Container: {NavMeshGoName}\n" +
                      "Agent Type: Humanoid (default)\n" +
                      "Walkable Area: 0\n\n" +
                      "If walls are walkable or stairs unreachable: open the NavMesh Surface inspector and tweak Agent Radius / Step Height / Slope. Click Bake again."
                    : "Bake failed — no NavMesh data was generated. Check that the museum has visible MeshRenderers in this scene.",
                "OK");
        }
    }
}
