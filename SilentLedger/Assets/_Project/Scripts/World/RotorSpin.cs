using UnityEngine;

namespace SilentLedger.World
{
    /// <summary>
    /// Spins a rotor around a local axis. Either winds down from the start (a helicopter that has
    /// just landed) or holds full speed until <see cref="BeginSpinDown"/> is called.
    /// </summary>
    public class RotorSpin : MonoBehaviour
    {
        [SerializeField] Vector3 localAxis = Vector3.forward;
        [SerializeField] float startRpm = 300f;
        [SerializeField] float idleRpm;
        [Tooltip("Seconds to wind down from the starting speed to idle.")]
        [SerializeField] float spinDownSeconds = 25f;
        [Tooltip("Off: hold full speed until BeginSpinDown (e.g. while flying).")]
        [SerializeField] bool spinDownOnStart = true;

        float elapsed;
        bool spinningDown;

        /// <summary>Current speed as a fraction of the starting speed (0..1).</summary>
        public float Throttle { get; private set; } = 1f;

        void Start() => spinningDown = spinDownOnStart;

        public void BeginSpinDown()
        {
            spinningDown = true;
            elapsed = 0f;
        }

        void Update()
        {
            float rpm = startRpm;
            if (spinningDown)
            {
                elapsed += Time.deltaTime;
                float t = spinDownSeconds > 0f ? Mathf.Clamp01(elapsed / spinDownSeconds) : 1f;
                rpm = Mathf.Lerp(idleRpm, startRpm, (1f - t) * (1f - t)); // fast at first, then a slow coast
            }
            Throttle = startRpm > 0f ? rpm / startRpm : 0f;
            if (rpm <= 0.01f) return;
            transform.Rotate(localAxis, rpm * 6f * Time.deltaTime, Space.Self);
        }
    }
}
