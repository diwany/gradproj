using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Museum.Guide;
using Museum.Voice;

namespace Museum.EditorTools
{
    /// <summary>
    /// Phase 6.4 — replace the placeholder capsule body with a Mixamo humanoid avatar:
    /// builds an Animator Controller (Idle/Walk/Talk), wires NavMeshAgent + TourGuideAgent,
    /// reparents the streaming AudioSource onto the head bone, and hooks AmplitudeJawFlap
    /// to a jaw bone if one exists (most Mixamo characters don't have one — the Talk
    /// animation alone reads as "speaking" via IsSpeaking).
    /// </summary>
    public static class MixamoWireUp
    {
        const string MenuPath = "Tools/Museum/Phase 6/Wire Mixamo Avatar";

        const string GuideFbxPath = "Assets/Mixamo/Guide.fbx";
        const string IdleFbxPath = "Assets/Mixamo/Idle.fbx";
        const string WalkingFbxPath = "Assets/Mixamo/Walking1.fbx";
        const string TalkingFbxPath = "Assets/Mixamo/Talking.fbx";

        const string AnimationFolder = "Assets/Animation";
        const string ControllerPath = "Assets/Animation/TourGuide.controller";

        [MenuItem(MenuPath)]
        public static void WireUp()
        {
            var guideFbx = AssetDatabase.LoadAssetAtPath<GameObject>(GuideFbxPath);
            if (guideFbx == null)
            {
                Fail($"Missing {GuideFbxPath}. Drop your Mixamo character FBX there and set Rig=Humanoid.");
                return;
            }

            var idleClip = LoadClip(IdleFbxPath);
            var walkClip = LoadClip(WalkingFbxPath);
            var talkClip = LoadClip(TalkingFbxPath);
            if (idleClip == null) { Fail($"No AnimationClip in {IdleFbxPath}."); return; }
            if (walkClip == null) { Fail($"No AnimationClip in {WalkingFbxPath}."); return; }
            if (talkClip == null) { Fail($"No AnimationClip in {TalkingFbxPath}."); return; }

            EnsureLooping(idleClip);
            EnsureLooping(walkClip);

            var voiceHost = Object.FindAnyObjectByType<GuideOrchestrator>();
            if (voiceHost == null)
            {
                Fail("No Tour Guide GameObject in scene. Run Tools > Museum > Phase 5 > Add Tour Guide first.");
                return;
            }
            var tourGuide = voiceHost.gameObject;

            // Capture the placeholder's position (or any prior avatar's position) so the swap is in-place,
            // then destroy the entire body GameObject.
            Vector3 bodyLocalPos = new Vector3(2f, 0f, 2f);
            var existingAgents = tourGuide.GetComponentsInChildren<TourGuideAgent>();
            foreach (var existing in existingAgents)
            {
                bodyLocalPos = existing.transform.localPosition;
                bodyLocalPos.y = 0f;
                Undo.DestroyObjectImmediate(existing.gameObject);
            }

            EnsureFolder(AnimationFolder);
            var controller = BuildAnimatorController(idleClip, walkClip, talkClip);

            var avatar = (GameObject)PrefabUtility.InstantiatePrefab(guideFbx);
            if (avatar == null)
            {
                Fail($"Failed to instantiate {GuideFbxPath}. Is the FBX importable as a prefab? Check the Inspector.");
                return;
            }
            Undo.RegisterCreatedObjectUndo(avatar, "Wire Mixamo Avatar");
            avatar.name = "Tour Guide Avatar";
            avatar.transform.SetParent(tourGuide.transform, false);
            avatar.transform.localPosition = bodyLocalPos;
            avatar.transform.localRotation = Quaternion.identity;

            var animator = avatar.GetComponent<Animator>();
            if (animator == null)
            {
                Fail("Avatar has no Animator. Re-import Guide.fbx with Rig > Animation Type = Humanoid.");
                Object.DestroyImmediate(avatar);
                return;
            }
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            var nav = avatar.GetComponent<NavMeshAgent>();
            if (nav == null) nav = avatar.AddComponent<NavMeshAgent>();
            if (nav == null)
            {
                Fail("Failed to add NavMeshAgent to the avatar. Ensure the AI Navigation module is enabled in Package Manager.");
                Object.DestroyImmediate(avatar);
                return;
            }
            nav.height = 1.8f;
            nav.radius = 0.35f;
            nav.speed = 2.2f;
            nav.acceleration = 12f;
            nav.angularSpeed = 360f;
            nav.stoppingDistance = 0.3f;

            var guideAgent = avatar.AddComponent<TourGuideAgent>();
            guideAgent.animator = animator;
            guideAgent.stoppingDistance = 1.8f;

            var headBone = animator.GetBoneTransform(HumanBodyBones.Head);
            Transform audioParent = headBone != null ? headBone : avatar.transform;

            var voiceGo = new GameObject("VoiceSource");
            voiceGo.transform.SetParent(audioParent, false);
            voiceGo.transform.localPosition = Vector3.zero;
            var streaming = voiceGo.AddComponent<StreamingAudioPlayer>();
            var bodySource = voiceGo.GetComponent<AudioSource>();
            bodySource.spatialBlend = 0.85f;
            bodySource.minDistance = 1f;
            bodySource.maxDistance = 25f;

            var jawFlap = avatar.AddComponent<AmplitudeJawFlap>();
            jawFlap.audioSource = bodySource;
            jawFlap.tourGuideAgent = guideAgent;
            jawFlap.jawBone = FindJawBone(animator);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            Selection.activeGameObject = avatar;
            EditorGUIUtility.PingObject(avatar);

            string jawMsg = jawFlap.jawBone != null
                ? $"jaw bone wired ({jawFlap.jawBone.name})"
                : "no jaw bone found — IsSpeaking flag alone drives the Talk animation. Most Mixamo rigs lack a jaw bone; this is normal.";
            string headMsg = headBone != null ? "head bone" : "AVATAR ROOT (no Humanoid head bone — check Avatar config)";

            EditorUtility.DisplayDialog("Mixamo Wire-Up",
                "Avatar wired:\n" +
                "- Animator: TourGuide.controller (Idle/Walk/Talk + IsMoving/IsSpeaking)\n" +
                "- Root motion disabled\n" +
                "- NavMeshAgent + TourGuideAgent on avatar root\n" +
                $"- StreamingAudioPlayer reparented to {headMsg}\n" +
                $"- AmplitudeJawFlap: {jawMsg}\n\n" +
                "Press Play in Museum.unity. Avatar should idle, walk to you, and switch to Talk while the Realtime API speaks.\n\n" +
                "If the avatar floats above the floor or is wildly large/small, re-import Guide.fbx with Scale Factor 0.01 (most Mixamo FBX) and re-run this menu.",
                "OK");
        }

        static AnimationClip LoadClip(string fbxPath)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            return assets.OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview__"));
        }

        static void EnsureLooping(AnimationClip clip)
        {
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            if (!settings.loopTime)
            {
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                EditorUtility.SetDirty(clip);
            }
        }

        static AnimatorController BuildAnimatorController(AnimationClip idle, AnimationClip walk, AnimationClip talk)
        {
            // Overwrites if it exists.
            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            // Strip default parameters/states that CreateAnimatorControllerAtPath might add (it doesn't, but be safe).
            while (controller.parameters.Length > 0) controller.RemoveParameter(0);

            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsSpeaking", AnimatorControllerParameterType.Bool);

            var sm = controller.layers[0].stateMachine;
            // Clear any default states.
            foreach (var s in sm.states.ToArray()) sm.RemoveState(s.state);

            var idleState = sm.AddState("Idle");
            idleState.motion = idle;
            var walkState = sm.AddState("Walk");
            walkState.motion = walk;
            var talkState = sm.AddState("Talk");
            talkState.motion = talk;
            sm.defaultState = idleState;

            AddTransition(idleState, walkState, "IsMoving", true);
            AddTransition(walkState, idleState, "IsMoving", false);
            AddTransition(idleState, talkState, "IsSpeaking", true);
            AddTransition(walkState, talkState, "IsSpeaking", true);
            AddTransition(talkState, idleState, "IsSpeaking", false);

            return controller;
        }

        static void AddTransition(AnimatorState from, AnimatorState to, string param, bool value)
        {
            var t = from.AddTransition(to);
            t.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, param);
            t.hasExitTime = false;
            t.duration = 0.15f;
            t.canTransitionToSelf = false;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace("\\", "/");
            var folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }

        static Transform FindJawBone(Animator animator)
        {
            var jaw = animator.GetBoneTransform(HumanBodyBones.Jaw);
            if (jaw != null) return jaw;
            return FindByNameContains(animator.transform, "jaw");
        }

        static Transform FindByNameContains(Transform root, string substring)
        {
            substring = substring.ToLowerInvariant();
            if (root.name.ToLowerInvariant().Contains(substring)) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindByNameContains(root.GetChild(i), substring);
                if (found != null) return found;
            }
            return null;
        }

        static void Fail(string msg)
        {
            EditorUtility.DisplayDialog("Mixamo Wire-Up", msg, "OK");
            Debug.LogError("[MixamoWireUp] " + msg);
        }
    }
}
