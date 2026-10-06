using UnityEngine;

namespace SilentLedger.World
{
    /// <summary>
    /// Spins a rotor around a local axis, winding down from a starting speed to an idle speed,
    /// like a helicopter that has just landed.
    /// </summary>
    public class RotorSpin : MonoBehaviour
    {
        [SerializeField] Vector3 localAxis = Vector3.forward;
        [SerializeField] float startRpm = 300f;
        [SerializeField] float idleRpm;
        [Tooltip("Seconds to wind down from the starting speed to idle.")]
        [SerializeField] float spinDownSeconds = 25f;

        float elapsed;

        void Update()
        {
            elapsed += Time.deltaTime;
            float t = spinDownSeconds > 0f ? Mathf.Clamp01(elapsed / spinDownSeconds) : 1f;
            float rpm = Mathf.Lerp(idleRpm, startRpm, (1f - t) * (1f - t)); // fast at first, then a slow coast
            if (rpm <= 0.01f) return;
            transform.Rotate(localAxis, rpm * 6f * Time.deltaTime, Space.Self);
        }
    }
}
