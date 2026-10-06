using System;
using UnityEngine;

namespace SilentLedger.Weapons
{
    /// <summary>A training target that falls back when shot and pops up again after a delay.</summary>
    public class Target : MonoBehaviour, IDamageable
    {
        [SerializeField] Transform pivot;
        [SerializeField] float health = 50f;
        [Tooltip("Seconds before the target stands back up. 0 keeps it down.")]
        [SerializeField] float resetSeconds = 3f;
        [Tooltip("Civilian targets must not be shot.")]
        [SerializeField] bool civilian;

        /// <summary>Raised whenever any target is taken down.</summary>
        public static event Action<Target> AnyDowned;

        float currentHealth;
        float downedAt;
        float angle;

        public bool IsCivilian => civilian;
        public bool IsDown { get; private set; }

        void Awake() => currentHealth = health;

        public bool ApplyDamage(float amount, Vector3 point, Vector3 direction)
        {
            if (IsDown) return false;
            currentHealth -= amount;
            if (currentHealth > 0f) return false;

            IsDown = true;
            downedAt = Time.time;
            AnyDowned?.Invoke(this);
            return true;
        }

        void Update()
        {
            if (IsDown && resetSeconds > 0f && Time.time >= downedAt + resetSeconds)
            {
                IsDown = false;
                currentHealth = health;
            }

            float targetAngle = IsDown ? 90f : 0f;
            if (Mathf.Approximately(angle, targetAngle)) return;
            angle = Mathf.MoveTowards(angle, targetAngle, 600f * Time.deltaTime);
            pivot.localRotation = Quaternion.Euler(angle, 0f, 0f);
        }
    }
}
