using UnityEngine;

namespace SilentLedger.Player
{
    /// <summary>
    /// What the player wants to do this frame. Touch controls and keyboard/mouse write into it;
    /// gameplay components read it. One-shot requests are consumed by the reader, so they work
    /// regardless of script execution order.
    /// </summary>
    public class PlayerIntent : MonoBehaviour
    {
        public enum AimMode { Toggle, Hold }

        [Tooltip("Tap the aim button to toggle aiming, or hold it to aim.")]
        public AimMode aimMode = AimMode.Toggle;

        [Tooltip("Degrees of turn per pixel of finger movement, measured on a 1080p-tall screen.")]
        [Range(0.02f, 0.5f)] public float touchSensitivity = 0.13f;

        public Vector2 TouchMove { get; set; }
        public Vector2 KeyMove { get; set; }

        /// <summary>Move input, -1..1 per axis. Magnitude 1 means the stick is at its edge.</summary>
        public Vector2 Move => Vector2.ClampMagnitude(TouchMove + KeyMove, 1f);

        public bool TouchFireHeld { get; set; }
        public bool MouseFireHeld { get; set; }
        public bool FireHeld => TouchFireHeld || MouseFireHeld;

        public bool Aiming { get; private set; }

        Vector2 lookDegrees;
        bool firePressed, reload, swap, interact, crouch, prone;

        public void AddLook(Vector2 degrees) => lookDegrees += degrees;

        public void AddTouchLook(Vector2 pixelDelta) =>
            lookDegrees += pixelDelta * (touchSensitivity * 1080f / Mathf.Max(1, Screen.height));

        public Vector2 ConsumeLook()
        {
            var d = lookDegrees;
            lookDegrees = Vector2.zero;
            return d;
        }

        public void AimPressed() => Aiming = aimMode == AimMode.Toggle ? !Aiming : true;

        public void AimReleased()
        {
            if (aimMode == AimMode.Hold) Aiming = false;
        }

        public void CancelAim() => Aiming = false;

        /// <summary>
        /// A trigger pull. Recorded separately from FireHeld so a tap that is pressed and released
        /// within one frame still fires a shot.
        /// </summary>
        public void RequestFire() => firePressed = true;
        public bool ConsumeFirePressed() => Consume(ref firePressed);

        public void RequestReload() => reload = true;
        public void RequestSwap() => swap = true;
        public void RequestInteract() => interact = true;
        public void RequestCrouch() => crouch = true;
        public void RequestProne() => prone = true;

        public bool ConsumeReload() => Consume(ref reload);
        public bool ConsumeSwap() => Consume(ref swap);
        public bool ConsumeInteract() => Consume(ref interact);
        public bool ConsumeCrouch() => Consume(ref crouch);
        public bool ConsumeProne() => Consume(ref prone);

        static bool Consume(ref bool flag)
        {
            bool value = flag;
            flag = false;
            return value;
        }
    }
}
