using System;
using System.Collections.Generic;
using SilentLedger.Player;
using UnityEngine;

namespace SilentLedger.Weapons
{
    /// <summary>
    /// Fires, reloads and swaps the player's weapons. Shots are hitscan from the camera centre,
    /// with spread that tightens when aiming and widens when moving.
    /// </summary>
    public class WeaponHolder : MonoBehaviour
    {
        [SerializeField] PlayerIntent intent;
        [SerializeField] PlayerLook look;
        [SerializeField] PlayerController controller;
        [SerializeField] Camera viewCamera;
        [SerializeField] List<Weapon> weapons = new List<Weapon>();

        [SerializeField] float swapSeconds = 0.45f;
        [SerializeField] float aimSharpness = 14f;
        [SerializeField, Range(1f, 3f)] float movingSpreadMultiplier = 1.5f;
        [SerializeField] LayerMask hitMask = Physics.DefaultRaycastLayers;
        [Tooltip("How many weapons from the list the player starts with. 0 means all of them.")]
        [SerializeField] int startingWeaponCount;

        /// <summary>Raised when the weapon, its ammo or its reload state changes.</summary>
        public event Action Changed;
        /// <summary>Raised when a shot hits something damageable; true if it took the target down.</summary>
        public event Action<bool> Hit;
        /// <summary>Raised for every shot fired.</summary>
        public event Action Fired;
        /// <summary>Raised when the trigger is pulled with no ammo left at all.</summary>
        public event Action DryFired;
        /// <summary>Raised where a shot lands: point, surface normal, and whether it hit something damageable.</summary>
        public event Action<Vector3, Vector3, bool> Impact;
        /// <summary>Raised when a reload begins.</summary>
        public event Action ReloadStarted;
        /// <summary>Raised when a reload finishes.</summary>
        public event Action Reloaded;
        /// <summary>Raised when the player switches to another weapon.</summary>
        public event Action Swapped;

        int index;
        int available;
        float nextShotTime;
        float reloadEndTime = -1f;
        float swapEndTime = -1f;
        bool wasFireHeld;
        float aimBlend;
        float kick;

        public Weapon Current => weapons[index];
        public bool IsReloading => reloadEndTime >= 0f;
        public bool IsSwapping => swapEndTime >= 0f;

        void Awake()
        {
            foreach (var weapon in weapons) weapon.ResetAmmo();
            available = startingWeaponCount <= 0 ? weapons.Count : Mathf.Min(startingWeaponCount, weapons.Count);
        }

        void Start() => Equip(index);

        void Update()
        {
            float now = Time.time;

            if (intent.ConsumeSwap()) BeginSwap();
            if (IsSwapping && now >= swapEndTime) swapEndTime = -1f;

            if (intent.ConsumeReload()) TryReload();
            if (IsReloading && now >= reloadEndTime) FinishReload();

            bool held = intent.FireHeld;
            bool pressed = (held && !wasFireHeld) | intent.ConsumeFirePressed();
            wasFireHeld = held;

            bool trigger = Current.stats.automatic ? held || pressed : pressed;
            if (trigger && !IsSwapping && !IsReloading && now >= nextShotTime)
            {
                if (Current.AmmoInMagazine > 0) Fire(now);
                else if (pressed && Current.ReserveAmmo > 0) TryReload();
                else if (pressed) DryFired?.Invoke();
            }

            UpdateViewModel(Time.deltaTime);
        }

        void Fire(float now)
        {
            var stats = Current.stats;
            nextShotTime = now + 60f / stats.roundsPerMinute;
            Current.AmmoInMagazine--;

            float spread = Mathf.Lerp(stats.hipSpread, stats.aimSpread, aimBlend);
            if (controller.IsMoving) spread *= movingSpreadMultiplier;

            Transform cam = viewCamera.transform;
            Vector2 offset = UnityEngine.Random.insideUnitCircle * Mathf.Tan(spread * Mathf.Deg2Rad);
            Vector3 direction = (cam.forward + cam.right * offset.x + cam.up * offset.y).normalized;

            Fired?.Invoke();
            if (Physics.Raycast(cam.position, direction, out var hit, stats.range, hitMask, QueryTriggerInteraction.Ignore))
            {
                ImpactMarks.Spawn(hit.point, hit.normal);
                var target = hit.collider.GetComponentInParent<IDamageable>();
                Impact?.Invoke(hit.point, hit.normal, target != null);
                if (target != null) Hit?.Invoke(target.ApplyDamage(stats.damage, hit.point, direction));
            }

            float recoilScale = intent.Aiming ? 0.6f : 1f;
            look.AddRecoil(stats.recoilPitch * recoilScale,
                UnityEngine.Random.Range(-stats.recoilYaw, stats.recoilYaw) * recoilScale);
            kick = 1f;

            Changed?.Invoke();
            if (Current.AmmoInMagazine == 0) TryReload();
        }

        public void TryReload()
        {
            var weapon = Current;
            if (IsReloading || IsSwapping || weapon.ReserveAmmo == 0
                || weapon.AmmoInMagazine == weapon.stats.magazineSize) return;
            reloadEndTime = Time.time + weapon.stats.reloadSeconds;
            Changed?.Invoke();
            ReloadStarted?.Invoke();
        }

        void FinishReload()
        {
            var weapon = Current;
            int loaded = Mathf.Min(weapon.stats.magazineSize - weapon.AmmoInMagazine, weapon.ReserveAmmo);
            weapon.AmmoInMagazine += loaded;
            weapon.ReserveAmmo -= loaded;
            reloadEndTime = -1f;
            Changed?.Invoke();
            Reloaded?.Invoke();
        }

        void BeginSwap()
        {
            if (available < 2 || IsSwapping) return;
            reloadEndTime = -1f;
            Equip((index + 1) % available);
            swapEndTime = Time.time + swapSeconds;
            Swapped?.Invoke();
        }

        /// <summary>Gives the player every weapon in the list, as when picking up a loadout.</summary>
        public void UnlockAll()
        {
            available = weapons.Count;
            Changed?.Invoke();
        }

        /// <summary>Leaves at most this many rounds in the current magazine, so there is something to reload.</summary>
        public void DrainMagazine(int roundsLeft)
        {
            Current.AmmoInMagazine = Mathf.Min(Current.AmmoInMagazine, roundsLeft);
            Changed?.Invoke();
        }

        void Equip(int newIndex)
        {
            index = newIndex;
            for (int i = 0; i < weapons.Count; i++)
                weapons[i].gameObject.SetActive(i == index);
            look.AimFov = Current.stats.aimFov;
            Changed?.Invoke();
        }

        /// <summary>Fills every weapon's magazine and reserve, as at an ammo crate.</summary>
        public void Refill()
        {
            foreach (var weapon in weapons) weapon.ResetAmmo();
            reloadEndTime = -1f;
            Changed?.Invoke();
        }

        void UpdateViewModel(float dt)
        {
            bool aim = intent.Aiming && !IsSwapping && !controller.IsSprinting;
            aimBlend = Mathf.Lerp(aimBlend, aim ? 1f : 0f, 1f - Mathf.Exp(-aimSharpness * dt));
            kick = Mathf.Lerp(kick, 0f, 1f - Mathf.Exp(-20f * dt));

            float swapDrop = IsSwapping ? (swapEndTime - Time.time) / swapSeconds * 0.3f : 0f;
            float reloadDrop = IsReloading ? 0.08f : 0f;
            float sprintDrop = controller.IsSprinting ? 0.06f : 0f;

            var weapon = Current;
            weapon.transform.localPosition = Vector3.Lerp(weapon.hipPosition, weapon.aimPosition, aimBlend)
                                             + Vector3.down * (swapDrop + reloadDrop + sprintDrop)
                                             + Vector3.back * (kick * 0.04f);
        }
    }
}
