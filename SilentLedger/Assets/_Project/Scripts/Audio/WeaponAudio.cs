using System.Collections;
using SilentLedger.Weapons;
using UnityEngine;

namespace SilentLedger.Audio
{
    /// <summary>
    /// Sounds for the player's weapons: shots, dry fire, a three-part reload timed to the
    /// weapon's reload length, swaps, and bullet impacts (a metal ping on targets).
    /// </summary>
    public class WeaponAudio : MonoBehaviour
    {
        [SerializeField] WeaponHolder weapons;

        Coroutine reloadSounds;

        void OnEnable()
        {
            weapons.Fired += OnFired;
            weapons.DryFired += OnDryFired;
            weapons.ReloadStarted += OnReloadStarted;
            weapons.Swapped += OnSwapped;
            weapons.Impact += OnImpact;
        }

        void OnDisable()
        {
            weapons.Fired -= OnFired;
            weapons.DryFired -= OnDryFired;
            weapons.ReloadStarted -= OnReloadStarted;
            weapons.Swapped -= OnSwapped;
            weapons.Impact -= OnImpact;
        }

        void OnFired()
        {
            var weapon = weapons.Current;
            Sfx.Play2D(weapon.shotSounds, weapon.shotVolume, 0.04f);
        }

        void OnDryFired()
        {
            if (Sfx.Library != null) Sfx.Play2D(Sfx.Library.dryFire, 0.6f);
        }

        void OnReloadStarted()
        {
            StopReloadSounds();
            reloadSounds = StartCoroutine(PlayReload(weapons.Current.stats.reloadSeconds));
        }

        void OnSwapped()
        {
            StopReloadSounds();
            if (Sfx.Library != null) Sfx.Play2D(Sfx.Library.weaponSwap, 0.5f);
        }

        void OnImpact(Vector3 point, Vector3 normal, bool damageable)
        {
            var library = Sfx.Library;
            if (library == null) return;
            if (damageable) Sfx.PlayAt(library.targetHit, point, 0.8f);
            else Sfx.PlayAt(library.impactConcrete, point, 0.6f, 0.12f);
        }

        IEnumerator PlayReload(float seconds)
        {
            var library = Sfx.Library;
            if (library == null) yield break;
            yield return new WaitForSeconds(seconds * 0.15f);
            Sfx.Play2D(library.magazineOut, 0.6f);
            yield return new WaitForSeconds(seconds * 0.45f);
            Sfx.Play2D(library.magazineIn, 0.7f);
            yield return new WaitForSeconds(seconds * 0.25f);
            Sfx.Play2D(library.boltRack, 0.6f);
            reloadSounds = null;
        }

        void StopReloadSounds()
        {
            if (reloadSounds == null) return;
            StopCoroutine(reloadSounds);
            reloadSounds = null;
        }
    }
}
