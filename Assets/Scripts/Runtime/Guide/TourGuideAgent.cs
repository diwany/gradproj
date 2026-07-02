using UnityEngine;
using UnityEngine.AI;

namespace Museum.Guide
{
    /// <summary>
    /// NavMeshAgent that follows the XR camera around. Optional Animator parameter wiring drives
    /// IsMoving / IsSpeaking. Place this on a GameObject parented to a placeholder capsule or the
    /// Mixamo avatar root.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class TourGuideAgent : MonoBehaviour
    {
        [Header("Following")]
        [Tooltip("Player target. Auto-assigned to the active Camera at runtime if left empty.")]
        public Transform player;

        [Tooltip("How close (meters) the guide should stay to the player. The agent actively maintains this distance — backs up if you approach, follows if you move away. 1.8m feels companion-close in VR without overlap.")]
        public float stoppingDistance = 1.8f;

        [Tooltip("How often to refresh the destination (seconds). Prevents per-frame NavMesh queries.")]
        public float repathInterval = 0.25f;

        [Header("Animator")]
        [Tooltip("Optional Animator. Leave empty if running on a placeholder without animations.")]
        public Animator animator;
        public string isMovingParam = "IsMoving";
        public string isSpeakingParam = "IsSpeaking";

        [Header("Speech state")]
        [Tooltip("Set externally (e.g., by AmplitudeJawFlap or GuideOrchestrator). Drives the IsSpeaking animator parameter.")]
        public bool isSpeaking;

        NavMeshAgent _agent;
        float _nextRepathTime;
        int _isMovingHash;
        int _isSpeakingHash;

        // Mixamo-walk safety net: animations import with XZ translation baked into the hip bone,
        // so even with applyRootMotion=false the visible body drifts forward each frame and snaps
        // back at loop end ("walks toward you and teleports back"). We lock the hip's local XZ to
        // its rest value each LateUpdate, leaving Y free for natural up/down bobbing.
        Transform _hipBone;
        Vector3 _hipRestLocalPos;

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            // We compute the destination ourselves to be at `stoppingDistance` from the player
            // already, so the NavMeshAgent's own stoppingDistance must be small — otherwise it
            // stops short of our destination by another full stoppingDistance and ends up far away.
            _agent.stoppingDistance = 0.3f;
            _agent.autoBraking = true;
            _isMovingHash = Animator.StringToHash(isMovingParam);
            _isSpeakingHash = Animator.StringToHash(isSpeakingParam);
        }

        void Start()
        {
            if (player == null)
            {
                var cam = Camera.main;
                if (cam != null) player = cam.transform;
            }

            if (animator != null && animator.isHuman)
            {
                _hipBone = animator.GetBoneTransform(HumanBodyBones.Hips);
                if (_hipBone != null) _hipRestLocalPos = _hipBone.localPosition;
            }
        }

        void Update()
        {
            if (player == null)
            {
                if (Camera.main != null) player = Camera.main.transform;
                if (player == null) return;
            }

            var fromPlayerToAgent = transform.position - player.position;
            fromPlayerToAgent.y = 0f;
            float planarDist = fromPlayerToAgent.magnitude;

            // Compute the desired position: a point at `stoppingDistance` from the player, on
            // whatever side the agent is currently on. As the player moves, this point follows them
            // — when the player walks toward the agent, the desired position moves "behind" the
            // agent and the agent backs up. Solves "player walks into stationary agent" by making
            // the agent always actively maintain distance.
            Vector3 dirFromPlayer;
            if (planarDist > 0.05f)
            {
                dirFromPlayer = fromPlayerToAgent / planarDist;
            }
            else
            {
                // Player is on top of the agent (e.g., teleported in). Pick the agent's facing
                // so they step away forward, not into a wall behind them.
                dirFromPlayer = transform.forward;
                dirFromPlayer.y = 0f;
                if (dirFromPlayer.sqrMagnitude < 0.001f) dirFromPlayer = Vector3.forward;
                dirFromPlayer.Normalize();
            }
            var desiredPos = player.position + dirFromPlayer * stoppingDistance;
            desiredPos.y = transform.position.y;
            float distToDesired = Vector3.Distance(transform.position, desiredPos);

            if (Time.time >= _nextRepathTime)
            {
                _nextRepathTime = Time.time + repathInterval;
                if (_agent.isOnNavMesh)
                {
                    // Hysteresis: only commit to moving if we're meaningfully off-target.
                    if (distToDesired > 0.4f)
                    {
                        _agent.isStopped = false;
                        _agent.SetDestination(desiredPos);
                    }
                    else
                    {
                        _agent.isStopped = true;
                    }
                }
            }

            // Always face the player when the player is anywhere near.
            var toPlayer = -fromPlayerToAgent;
            if (toPlayer.sqrMagnitude > 0.0001f)
            {
                var faceTarget = Quaternion.LookRotation(toPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, faceTarget, Time.deltaTime * 4f);
            }

            if (animator != null)
            {
                bool moving = _agent.velocity.sqrMagnitude > 0.04f;
                animator.SetBool(_isMovingHash, moving);
                animator.SetBool(_isSpeakingHash, isSpeaking);
            }
        }

        // Runs after the Animator has written bone poses for the frame. Re-snap the hip's local XZ
        // back to its rest value so any forward/lateral drift that the animation tries to introduce
        // is undone. Y is left free so the natural walk-cycle bob is preserved.
        void LateUpdate()
        {
            if (_hipBone == null) return;
            var p = _hipBone.localPosition;
            p.x = _hipRestLocalPos.x;
            p.z = _hipRestLocalPos.z;
            _hipBone.localPosition = p;
        }
    }
}
