using System;
using UnityEngine;

namespace SilentLedger.Weapons
{
    [Serializable]
    public class WeaponStats
    {
        public string displayName = "Rifle";
        public float damage = 34f;
        public float roundsPerMinute = 600f;
        public bool automatic = true;
        public int magazineSize = 30;
        public int reserveAmmo = 120;
        public float reloadSeconds = 2.2f;
        public float range = 500f;
        [Tooltip("Cone half-angle in degrees.")] public float hipSpread = 2.5f;
        [Tooltip("Cone half-angle in degrees.")] public float aimSpread = 0.3f;
        [Tooltip("Degrees of upward kick per shot.")] public float recoilPitch = 0.9f;
        [Tooltip("Maximum sideways kick per shot, in degrees.")] public float recoilYaw = 0.25f;
        public float aimFov = 45f;
    }

    /// <summary>A weapon's stats, view-model poses and live ammo count. Driven by WeaponHolder.</summary>
    public class Weapon : MonoBehaviour
    {
        public WeaponStats stats = new WeaponStats();
        [Tooltip("View-model position relative to the camera when firing from the hip.")]
        public Vector3 hipPosition = new Vector3(0.2f, -0.22f, 0.45f);
        [Tooltip("View-model position relative to the camera when aiming down sights.")]
        public Vector3 aimPosition = new Vector3(0f, -0.09f, 0.38f);

        public int AmmoInMagazine { get; set; }
        public int ReserveAmmo { get; set; }

        public void ResetAmmo()
        {
            AmmoInMagazine = stats.magazineSize;
            ReserveAmmo = stats.reserveAmmo;
        }
    }
}
