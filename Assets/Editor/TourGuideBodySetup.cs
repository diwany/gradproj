using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Museum.Guide;
using Museum.Voice;

namespace Museum.EditorTools
{
    public static class TourGuideBodySetup
    {
        const string PlaceholderMenu = "Tools/Museum/Phase 6/Add Placeholder Tour Guide Body (Capsule)";

        [MenuItem(PlaceholderMenu)]
        public static void AddPlaceholderBody()
        {
            // Find or create the parent Tour Guide GameObject (Phase 5 already added it for voice).
            var voiceHost = Object.FindAnyObjectByType<GuideOrchestrator>();
            if (voiceHost == null)
            {
                EditorUtility.DisplayDialog("Tour Guide Body",
                    "No Tour Guide GameObject in scene. Run Tools > Museum > Phase 5 > Add Tour Guide to Active Scene first.",
                    "OK");
                return;
            }

            var tourGuide = voiceHost.gameObject;

            var existingAgent = tourGuide.GetComponentInChildren<TourGuideAgent>();
            if (existingAgent != null)
            {
                Selection.activeGameObject = existingAgent.gameObject;
                EditorGUIUtility.PingObject(existingAgent);
                EditorUtility.DisplayDialog("Tour Guide Body", "A TourGuideAgent already exists in the scene.", "OK");
                return;
            }

            // Build a capsule placeholder under the Tour Guide GameObject.
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Undo.RegisterCreatedObjectUndo(capsule, "Placeholder Tour Guide Body");
            capsule.name = "Placeholder Body";
            capsule.transform.SetParent(tourGuide.transform, false);
            capsule.transform.localPosition = new Vector3(2f, 1f, 2f);
            capsule.transform.localScale = new Vector3(0.6f, 0.9f, 0.6f);
            // No collision needed for the capsule — NavMeshAgent handles avoidance against itself; mesh collider would interfere.
            var col = capsule.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            var agent = capsule.AddComponent<NavMeshAgent>();
            agent.height = 1.8f;
            agent.radius = 0.35f;
            agent.speed = 1.6f;
            agent.acceleration = 8f;
            agent.angularSpeed = 240f;
            agent.stoppingDistance = 1.5f;

            var guide = capsule.AddComponent<TourGuideAgent>();

            // Move the audio source from the Tour Guide root onto the capsule (so audio comes from the body, not world origin).
            var rootSource = tourGuide.GetComponent<AudioSource>();
            if (rootSource != null)
            {
                var bodySource = capsule.AddComponent<AudioSource>();
                bodySource.clip = rootSource.clip;
                bodySource.spatialBlend = 0.85f; // mostly 3D so it feels positioned
                bodySource.minDistance = 1f;
                bodySource.maxDistance = 25f;
                bodySource.loop = rootSource.loop;
                bodySource.playOnAwake = false;

                var streamingPlayer = tourGuide.GetComponent<StreamingAudioPlayer>();
                if (streamingPlayer != null)
                {
                    Object.DestroyImmediate(streamingPlayer);
                    var newPlayer = capsule.AddComponent<StreamingAudioPlayer>();
                    Debug.Log("[Tour Guide Body] Moved StreamingAudioPlayer onto the placeholder body for spatial audio.");
                }
                Object.DestroyImmediate(rootSource);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            Selection.activeGameObject = capsule;
            EditorGUIUtility.PingObject(capsule);

            EditorUtility.DisplayDialog("Tour Guide Body",
                "Placeholder capsule body added under Tour Guide.\n\n" +
                "- NavMeshAgent: speed 1.6, stopping 1.5m\n" +
                "- TourGuideAgent: follows Camera.main\n" +
                "- AudioSource: 3D-spatialized, 1m–25m falloff\n\n" +
                "Make sure you've baked the NavMesh first (Tools > Museum > Phase 6 > Bake Museum NavMesh).\n\n" +
                "Press Play — the capsule should pathfind to you. When you're ready for the real avatar, " +
                "import a Mixamo FBX as Humanoid, drag it under this Tour Guide GameObject, and we'll write the " +
                "avatar wire-up utility to swap the capsule out.",
                "OK");
        }
    }
}
