using UnityEngine;

namespace SilentLedger.Weapons
{
    public interface IDamageable
    {
        /// <summary>Applies damage and returns true if this hit took the target down.</summary>
        bool ApplyDamage(float amount, Vector3 point, Vector3 direction);
    }
}
