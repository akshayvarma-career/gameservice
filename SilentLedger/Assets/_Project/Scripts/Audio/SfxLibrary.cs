using UnityEngine;

namespace SilentLedger.Audio
{
    /// <summary>
    /// Shared sound effects. Arrays hold variations; one is picked at random each time.
    /// Weapon shots live on each Weapon instead, since every gun sounds different.
    /// </summary>
    [CreateAssetMenu(menuName = "Silent Ledger/Sfx Library")]
    public class SfxLibrary : ScriptableObject
    {
        [Header("Movement")]
        public AudioClip[] footstepsConcrete;
        public AudioClip[] stanceChange;

        [Header("Weapons")]
        public AudioClip[] dryFire;
        public AudioClip[] magazineOut;
        public AudioClip[] magazineIn;
        public AudioClip[] boltRack;
        public AudioClip[] weaponSwap;

        [Header("World")]
        public AudioClip[] impactConcrete;
        public AudioClip[] targetHit;
        public AudioClip[] doorOpen;
        public AudioClip[] doorClose;

        [Header("UI")]
        public AudioClip[] hitMarker;
        public AudioClip[] killMarker;
        public AudioClip[] objectiveUpdated;
    }
}
