using UnityEngine;
using UnityEngine.InputSystem;

namespace SilentLedger.Player
{
    /// <summary>
    /// Keyboard and mouse controls for testing in the Editor. The keyboard always works; the mouse
    /// turns the view only after F1 locks the cursor (Esc unlocks), so the touch UI stays clickable.
    /// WASD move, Shift sprint, F1 lock mouse, LMB fire, RMB aim, R reload, Q swap,
    /// C crouch, Z prone, E interact.
    /// </summary>
    public class KeyboardMouseInput : MonoBehaviour
    {
        [SerializeField] PlayerIntent intent;
        [SerializeField] float mouseSensitivity = 0.08f;

        // Below the sprint threshold, so holding W walks and Shift+W sprints.
        const float WalkDeflection = 0.89f;

        bool mouseLocked;

        void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            Vector2 move = Vector2.zero;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed) move.y += 1f;
                if (keyboard.sKey.isPressed) move.y -= 1f;
                if (keyboard.dKey.isPressed) move.x += 1f;
                if (keyboard.aKey.isPressed) move.x -= 1f;
                move = move.normalized * (keyboard.leftShiftKey.isPressed ? 1f : WalkDeflection);

                if (keyboard.rKey.wasPressedThisFrame) intent.RequestReload();
                if (keyboard.qKey.wasPressedThisFrame) intent.RequestSwap();
                if (keyboard.cKey.wasPressedThisFrame) intent.RequestCrouch();
                if (keyboard.zKey.wasPressedThisFrame) intent.RequestProne();
                if (keyboard.eKey.wasPressedThisFrame) intent.RequestInteract();
                if (keyboard.f1Key.wasPressedThisFrame) SetMouseLocked(!mouseLocked);
                if (keyboard.escapeKey.wasPressedThisFrame) SetMouseLocked(false);
            }
            intent.KeyMove = move;

            bool fire = false;
            if (mouseLocked && mouse != null)
            {
                intent.AddLook(mouse.delta.ReadValue() * mouseSensitivity);
                fire = mouse.leftButton.isPressed;
                if (mouse.rightButton.wasPressedThisFrame) intent.AimPressed();
                if (mouse.rightButton.wasReleasedThisFrame) intent.AimReleased();
            }
            intent.MouseFireHeld = fire;
        }

        void OnDisable() => SetMouseLocked(false);

        void SetMouseLocked(bool locked)
        {
            mouseLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
