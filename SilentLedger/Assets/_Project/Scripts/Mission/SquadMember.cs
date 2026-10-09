using System;
using SilentLedger.Interaction;
using SilentLedger.Player;
using UnityEngine;
using UnityEngine.AI;

namespace SilentLedger.Mission
{
    /// <summary>
    /// A named character the player can talk to when the mission allows it. Turns to face the
    /// player when close, and can follow a target if it has a NavMeshAgent.
    /// </summary>
    public class SquadMember : MonoBehaviour, IInteractable
    {
        [SerializeField] string displayName = "Squadmate";
        [SerializeField] Color color = Color.white;
        [Tooltip("Turns to face the player within this range. 0 never turns (Bishop doesn't look up).")]
        [SerializeField] float faceRange = 6f;
        [SerializeField] float followDistance = 2.5f;
        [Tooltip("How much further than followDistance the target must get before the follower sets off again.")]
        [SerializeField] float restartMargin = 1f;

        public string DisplayName => displayName;
        public Color Color => color;

        /// <summary>When true the player sees a "Talk to" prompt; using it raises <see cref="Talked"/>.</summary>
        public bool Talkable { get; set; }
        public event Action Talked;

        /// <summary>Set to make this character follow; clear to stop where it is.</summary>
        public Transform FollowTarget { get; set; }

        public string Prompt => $"Talk to {displayName}";
        public bool CanInteract => Talkable;

        Transform player;
        NavMeshAgent agent;
        bool following;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            // Set even while the agent is disabled (riding in a vehicle); otherwise it stays 0 and the
            // follower walks right into the player and keeps shuffling there.
            if (agent != null) agent.stoppingDistance = followDistance;
        }

        void Start()
        {
            var controller = FindAnyObjectByType<PlayerController>();
            if (controller != null) player = controller.transform;
            if (agent != null && agent.enabled) // disabled while riding in a vehicle
            {
                // Start on the ground under our feet, not whatever NavMesh is nearest (e.g. a vehicle roof).
                if (NavMesh.SamplePosition(transform.position, out var hit, 1.5f, NavMesh.AllAreas))
                    agent.Warp(hit.position);
                else
                    Debug.LogWarning($"{displayName} has no NavMesh within 1.5 m of {transform.position}", this);
            }
        }

        public void Interact(Interactor interactor)
        {
            Talkable = false;
            Talked?.Invoke();
        }

        void Update()
        {
            bool moving = false;
            if (agent != null && agent.isOnNavMesh)
            {
                UpdateFollow();
                moving = agent.velocity.sqrMagnitude > 0.05f;
            }

            if (moving || player == null || faceRange <= 0f) return;
            Vector3 toPlayer = player.position - transform.position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude > faceRange * faceRange || toPlayer.sqrMagnitude < 0.01f) return;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toPlayer),
                1f - Mathf.Exp(-5f * Time.deltaTime));
        }

        /// <summary>
        /// Follows with a dead band: sets off only once the target is more than followDistance +
        /// restartMargin away, and stops (clearing the path) once within followDistance, so small
        /// player movements don't keep the follower shuffling.
        /// </summary>
        void UpdateFollow()
        {
            if (FollowTarget == null)
            {
                if (agent.hasPath) agent.ResetPath();
                following = false;
                return;
            }

            Vector3 offset = FollowTarget.position - transform.position;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (!following && distance > followDistance + restartMargin) following = true;
            if (following && distance <= followDistance) following = false;

            if (following) agent.SetDestination(FollowTarget.position);
            else if (agent.hasPath) agent.ResetPath();
        }
    }
}
