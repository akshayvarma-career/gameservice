using System;
using SilentLedger.Player;
using UnityEngine;

namespace SilentLedger.Interaction
{
    /// <summary>
    /// Picks the usable object within reach that is closest to where the player is facing, so a
    /// waist-high crate works without looking down at it. The HUD shows the Use button only while
    /// <see cref="Current"/> is set.
    /// </summary>
    public class Interactor : MonoBehaviour
    {
        [SerializeField] PlayerIntent intent;
        [SerializeField] float reach = 2.2f;
        [Tooltip("How far off the facing direction (horizontally) an object can be and still be picked.")]
        [SerializeField, Range(10f, 90f)] float maxAngle = 50f;
        [SerializeField] LayerMask mask = Physics.DefaultRaycastLayers;

        /// <summary>Raised with a message and how many seconds to show it.</summary>
        public event Action<string, float> MessageShown;

        public IInteractable Current { get; private set; }

        readonly Collider[] nearby = new Collider[16];

        void Update()
        {
            Current = FindBest();
            if (intent.ConsumeInteract() && Current != null)
                Current.Interact(this);
        }

        IInteractable FindBest()
        {
            Vector3 origin = transform.position;
            Vector3 facing = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            float minDot = Mathf.Cos(maxAngle * Mathf.Deg2Rad);

            int count = Physics.OverlapSphereNonAlloc(origin, reach, nearby, mask, QueryTriggerInteraction.Collide);
            IInteractable best = null;
            float bestDot = minDot;
            for (int i = 0; i < count; i++)
            {
                var candidate = nearby[i].GetComponentInParent<IInteractable>();
                if (candidate == null || !candidate.CanInteract) continue;

                Vector3 toward = Vector3.ProjectOnPlane(ClosestPoint(nearby[i], origin) - origin, Vector3.up);
                float dot = toward.sqrMagnitude < 0.01f ? 1f : Vector3.Dot(facing, toward.normalized);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    best = candidate;
                }
            }
            return best;
        }

        static Vector3 ClosestPoint(Collider collider, Vector3 point) =>
            collider is MeshCollider { convex: false } ? collider.bounds.center : collider.ClosestPoint(point);

        public void ShowMessage(string message, float seconds = 4f) => MessageShown?.Invoke(message, seconds);
    }
}
