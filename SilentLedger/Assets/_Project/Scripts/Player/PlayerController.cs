using UnityEngine;

namespace SilentLedger.Player
{
    /// <summary>
    /// First-person movement on a CharacterController: walk, sprint (stick pushed to the edge),
    /// crouch and prone, with smooth height changes and a headroom check before standing up.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public enum Stance { Standing, Crouching, Prone }

        [SerializeField] PlayerIntent intent;
        [SerializeField] Transform cameraRoot;

        [Header("Speed (m/s)")]
        [SerializeField] float walkSpeed = 4f;
        [SerializeField] float sprintSpeed = 6.5f;
        [SerializeField] float crouchSpeed = 2f;
        [SerializeField] float proneSpeed = 0.9f;
        [SerializeField, Range(0.1f, 1f)] float aimSpeedMultiplier = 0.6f;
        [SerializeField] float acceleration = 14f;

        [Tooltip("Stick deflection (0..1) at which pushing forward turns into a sprint.")]
        [SerializeField, Range(0.5f, 1f)] float sprintThreshold = 0.9f;

        [Header("Height (m)")]
        [SerializeField] float standHeight = 1.8f;
        [SerializeField] float crouchHeight = 1.2f;
        [SerializeField] float proneHeight = 0.6f;
        [SerializeField] float eyeBelowTop = 0.15f;
        [SerializeField] float heightSharpness = 10f;

        [SerializeField] float gravity = -20f;

        CharacterController body;
        Vector3 horizontalVelocity;
        float verticalVelocity;

        public Stance CurrentStance { get; private set; } = Stance.Standing;
        public bool IsSprinting { get; private set; }
        public bool IsMoving => horizontalVelocity.sqrMagnitude > 0.1f;
        /// <summary>Horizontal speed in m/s.</summary>
        public float Speed => horizontalVelocity.magnitude;
        public bool IsGrounded => body.isGrounded;

        void Awake() => body = GetComponent<CharacterController>();

        void Update()
        {
            float dt = Time.deltaTime;
            UpdateStance();

            Vector2 move = intent.Move;
            bool wantSprint = move.magnitude >= sprintThreshold && move.y > 0.5f
                              && !intent.Aiming && !intent.FireHeld;
            if (wantSprint && CurrentStance != Stance.Standing && !TrySetStance(Stance.Standing))
                wantSprint = false;
            IsSprinting = wantSprint;

            Vector3 wish = transform.right * move.x + transform.forward * move.y;
            if (IsSprinting)
                wish = wish.normalized * sprintSpeed;
            else
                wish = Vector3.ClampMagnitude(wish / sprintThreshold, 1f) * StanceSpeed();
            if (intent.Aiming) wish *= aimSpeedMultiplier;

            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, wish, acceleration * dt);
            if (body.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            verticalVelocity += gravity * dt;

            body.Move((horizontalVelocity + Vector3.up * verticalVelocity) * dt);
            UpdateHeight(dt);
        }

        void UpdateStance()
        {
            if (intent.ConsumeProne())
            {
                if (CurrentStance == Stance.Prone) TrySetStance(Stance.Standing);
                else CurrentStance = Stance.Prone;
            }

            if (intent.ConsumeCrouch())
            {
                switch (CurrentStance)
                {
                    case Stance.Standing: CurrentStance = Stance.Crouching; break;
                    case Stance.Crouching: TrySetStance(Stance.Standing); break;
                    case Stance.Prone: TrySetStance(Stance.Crouching); break;
                }
            }
        }

        bool TrySetStance(Stance stance)
        {
            if (!HasHeadroom(HeightFor(stance))) return false;
            CurrentStance = stance;
            return true;
        }

        bool HasHeadroom(float targetHeight)
        {
            float current = body.height;
            if (targetHeight <= current + 0.01f) return true;
            Vector3 origin = transform.position + Vector3.up * (current - body.radius);
            return !Physics.SphereCast(origin, body.radius * 0.95f, Vector3.up, out _,
                targetHeight - current, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        }

        void UpdateHeight(float dt)
        {
            float h = Mathf.Lerp(body.height, HeightFor(CurrentStance), 1f - Mathf.Exp(-heightSharpness * dt));
            body.height = h;
            body.center = new Vector3(0f, h * 0.5f, 0f);
            cameraRoot.localPosition = new Vector3(0f, h - eyeBelowTop, 0f);
        }

        float HeightFor(Stance stance) => stance switch
        {
            Stance.Crouching => crouchHeight,
            Stance.Prone => proneHeight,
            _ => standHeight,
        };

        float StanceSpeed() => CurrentStance switch
        {
            Stance.Crouching => crouchSpeed,
            Stance.Prone => proneSpeed,
            _ => walkSpeed,
        };
    }
}
