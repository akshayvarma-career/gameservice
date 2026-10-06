using System;
using SilentLedger.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SilentLedger.UI
{
    /// <summary>
    /// On-screen button with press, release, tap and long-press events. With a look target set,
    /// dragging a finger on the button also turns the view (used by Fire, so players aim while shooting).
    /// </summary>
    public class TouchButton : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IDragHandler, IInitializePotentialDragHandler
    {
        const int NoPointer = int.MinValue;

        [SerializeField] Graphic graphic;
        [Tooltip("If set, dragging on this button also turns the view.")]
        [SerializeField] PlayerIntent lookIntent;
        [SerializeField] float longPressSeconds = 0.4f;
        [SerializeField] Color idleColor = new Color(1f, 1f, 1f, 0.22f);
        [SerializeField] Color pressedColor = new Color(1f, 1f, 1f, 0.45f);
        [SerializeField] Color latchedColor = new Color(1f, 0.75f, 0.3f, 0.45f);

        public event Action Pressed;
        public event Action Released;
        /// <summary>Raised on release if the press did not become a long press.</summary>
        public event Action Tapped;
        public event Action LongPressed;

        public bool IsHeld { get; private set; }

        int pointerId = NoPointer;
        float downTime;
        bool longPressFired;
        bool latched;

        public void OnInitializePotentialDrag(PointerEventData eventData) => eventData.useDragThreshold = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (IsHeld) return;
            pointerId = eventData.pointerId;
            IsHeld = true;
            downTime = Time.unscaledTime;
            longPressFired = false;
            Refresh();
            Pressed?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId == pointerId) Release(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (lookIntent != null && eventData.pointerId == pointerId)
                lookIntent.AddTouchLook(eventData.delta);
        }

        void Update()
        {
            if (!IsHeld || longPressFired || LongPressed == null) return;
            if (Time.unscaledTime - downTime < longPressSeconds) return;
            longPressFired = true;
            LongPressed.Invoke();
        }

        void OnDisable()
        {
            if (IsHeld) Release(false);
        }

        /// <summary>Shows the button as switched on, for toggles such as aim.</summary>
        public void SetLatched(bool value)
        {
            if (latched == value) return;
            latched = value;
            Refresh();
        }

        void Release(bool allowTap)
        {
            pointerId = NoPointer;
            IsHeld = false;
            Refresh();
            Released?.Invoke();
            if (allowTap && !longPressFired) Tapped?.Invoke();
        }

        void Refresh()
        {
            if (graphic != null)
                graphic.color = IsHeld ? pressedColor : latched ? latchedColor : idleColor;
            transform.localScale = Vector3.one * (IsHeld ? 0.93f : 1f);
        }
    }
}
