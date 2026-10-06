using SilentLedger.Interaction;
using SilentLedger.Player;
using SilentLedger.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace SilentLedger.UI
{
    /// <summary>
    /// Connects the on-screen buttons to the player and shows ammo, prompts, messages and hit markers.
    /// </summary>
    public class TouchHud : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] PlayerIntent intent;
        [SerializeField] WeaponHolder weapons;
        [SerializeField] Interactor interactor;

        [Header("Buttons")]
        [SerializeField] TouchButton fireButton;
        [SerializeField] TouchButton leftFireButton;
        [SerializeField] TouchButton aimButton;
        [SerializeField] TouchButton reloadButton;
        [SerializeField] TouchButton swapButton;
        [SerializeField] TouchButton crouchButton;
        [SerializeField] TouchButton interactButton;

        [Header("Readouts")]
        [SerializeField] Text ammoText;
        [SerializeField] Text weaponText;
        [SerializeField] Text promptText;
        [SerializeField] Text messageText;
        [SerializeField] CanvasGroup hitMarker;
        [SerializeField] Graphic[] hitMarkerGraphics;

        static readonly Color HitColor = Color.white;
        static readonly Color KillColor = new Color(1f, 0.3f, 0.25f);
        static readonly Color CivilianColor = new Color(0.45f, 0.75f, 1f);

        float messageHideTime;
        int civilianDownedFrame = -1;

        void OnEnable()
        {
            fireButton.Pressed += intent.RequestFire;
            leftFireButton.Pressed += intent.RequestFire;
            aimButton.Pressed += intent.AimPressed;
            aimButton.Released += intent.AimReleased;
            reloadButton.Pressed += intent.RequestReload;
            swapButton.Pressed += intent.RequestSwap;
            interactButton.Pressed += intent.RequestInteract;
            crouchButton.Tapped += intent.RequestCrouch;
            crouchButton.LongPressed += intent.RequestProne;

            weapons.Changed += RefreshWeapon;
            weapons.Hit += ShowHitMarker;
            interactor.MessageShown += ShowMessage;
            Target.AnyDowned += OnTargetDowned;
        }

        void OnDisable()
        {
            fireButton.Pressed -= intent.RequestFire;
            leftFireButton.Pressed -= intent.RequestFire;
            aimButton.Pressed -= intent.AimPressed;
            aimButton.Released -= intent.AimReleased;
            reloadButton.Pressed -= intent.RequestReload;
            swapButton.Pressed -= intent.RequestSwap;
            interactButton.Pressed -= intent.RequestInteract;
            crouchButton.Tapped -= intent.RequestCrouch;
            crouchButton.LongPressed -= intent.RequestProne;

            weapons.Changed -= RefreshWeapon;
            weapons.Hit -= ShowHitMarker;
            interactor.MessageShown -= ShowMessage;
            Target.AnyDowned -= OnTargetDowned;
        }

        void Start()
        {
            RefreshWeapon();
            hitMarker.alpha = 0f;
            messageText.text = "";
        }

        void Update()
        {
            intent.TouchFireHeld = fireButton.IsHeld || leftFireButton.IsHeld;
            aimButton.SetLatched(intent.Aiming);

            var current = interactor.Current;
            bool canUse = current != null;
            if (interactButton.gameObject.activeSelf != canUse) interactButton.gameObject.SetActive(canUse);
            promptText.text = canUse ? current.Prompt : "";

            hitMarker.alpha = Mathf.MoveTowards(hitMarker.alpha, 0f, Time.deltaTime * 4f);
            if (messageText.text.Length > 0 && Time.time >= messageHideTime) messageText.text = "";
        }

        void RefreshWeapon()
        {
            var weapon = weapons.Current;
            weaponText.text = weapon.stats.displayName;
            ammoText.text = weapons.IsReloading
                ? "RELOADING"
                : $"{weapon.AmmoInMagazine} / {weapon.ReserveAmmo}";
        }

        void ShowHitMarker(bool downed)
        {
            // Target.AnyDowned fires just before WeaponHolder.Hit for the same shot.
            bool civilian = downed && civilianDownedFrame == Time.frameCount;
            var color = civilian ? CivilianColor : downed ? KillColor : HitColor;
            foreach (var g in hitMarkerGraphics) g.color = color;
            hitMarker.alpha = 1f;
        }

        void OnTargetDowned(Target target)
        {
            if (!target.IsCivilian) return;
            civilianDownedFrame = Time.frameCount;
            ShowMessage("Civilian hit. Check your targets.", 2.5f);
        }

        void ShowMessage(string message, float seconds)
        {
            messageText.text = message;
            messageHideTime = Time.time + seconds;
        }
    }
}
