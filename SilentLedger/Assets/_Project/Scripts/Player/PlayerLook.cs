using UnityEngine;

namespace SilentLedger.Player
{
    /// <summary>
    /// Turns the body (yaw) and camera (pitch) from look input, slows look while aiming,
    /// applies recoil that mostly recovers, and blends the field of view for aiming.
    /// </summary>
    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] PlayerIntent intent;
        [SerializeField] Camera viewCamera;

        [SerializeField] float hipFov = 70f;
        [SerializeField, Range(0.1f, 1f)] float aimLookMultiplier = 0.55f;
        [SerializeField] float minPitch = -85f;
        [SerializeField] float maxPitch = 85f;
        [SerializeField] float fovSharpness = 12f;
        [SerializeField] float recoilRecovery = 7f;
        [Tooltip("Share of each recoil kick that stays as a permanent climb.")]
        [SerializeField, Range(0f, 1f)] float recoilClimb = 0.3f;

        float pitch;
        Vector2 recoil;

        /// <summary>Field of view while aiming; set by the current weapon.</summary>
        public float AimFov { get; set; } = 50f;

        void Start()
        {
            pitch = Mathf.DeltaAngle(0f, viewCamera.transform.localEulerAngles.x);
            viewCamera.fieldOfView = hipFov;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            Vector2 look = intent.ConsumeLook();
            if (intent.Aiming) look *= aimLookMultiplier;

            transform.Rotate(0f, look.x, 0f, Space.Self);
            pitch = Mathf.Clamp(pitch - look.y, minPitch, maxPitch);

            recoil = Vector2.Lerp(recoil, Vector2.zero, 1f - Mathf.Exp(-recoilRecovery * dt));
            viewCamera.transform.localRotation =
                Quaternion.Euler(Mathf.Clamp(pitch - recoil.x, minPitch, maxPitch), recoil.y, 0f);

            float targetFov = intent.Aiming ? AimFov : hipFov;
            viewCamera.fieldOfView = Mathf.Lerp(viewCamera.fieldOfView, targetFov, 1f - Mathf.Exp(-fovSharpness * dt));
        }

        public void AddRecoil(float pitchUp, float yaw)
        {
            recoil += new Vector2(pitchUp * (1f - recoilClimb), yaw);
            pitch = Mathf.Clamp(pitch - pitchUp * recoilClimb, minPitch, maxPitch);
        }
    }
}
