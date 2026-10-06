using SilentLedger.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SilentLedger.UI
{
    /// <summary>
    /// Move stick for the left side of the screen. It appears wherever the thumb lands;
    /// pushing to the edge reads as full deflection, which the controller turns into a sprint.
    /// </summary>
    public class FloatingJoystick : MonoBehaviour,
        IPointerDownHandler, IDragHandler, IPointerUpHandler, IInitializePotentialDragHandler
    {
        const int NoPointer = int.MinValue;

        [SerializeField] PlayerIntent intent;
        [SerializeField] RectTransform stickBase;
        [SerializeField] RectTransform knob;
        [SerializeField] Graphic baseGraphic;
        [SerializeField] Graphic knobGraphic;
        [Tooltip("Knob travel from the centre, in canvas units.")]
        [SerializeField] float radius = 110f;
        [SerializeField, Range(0.5f, 1f)] float sprintThreshold = 0.9f;
        [SerializeField] Color sprintTint = new Color(1f, 0.75f, 0.3f, 0.9f);

        RectTransform area;
        Vector3 restPosition;
        int pointerId = NoPointer;
        Color baseColor, knobColor;

        void Awake()
        {
            area = (RectTransform)transform;
            restPosition = stickBase.localPosition;
            baseColor = baseGraphic.color;
            knobColor = knobGraphic.color;
            SetIdle();
        }

        public void OnInitializePotentialDrag(PointerEventData eventData) => eventData.useDragThreshold = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (pointerId != NoPointer) return;
            pointerId = eventData.pointerId;
            if (TryGetLocal(eventData, out var local))
                stickBase.localPosition = local;
            knob.anchoredPosition = Vector2.zero;
            SetAlpha(1f);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != pointerId || !TryGetLocal(eventData, out var local)) return;

            Vector2 offset = Vector2.ClampMagnitude(local - (Vector2)stickBase.localPosition, radius);
            knob.anchoredPosition = offset;
            Vector2 move = offset / radius;
            intent.TouchMove = move;

            bool sprinting = move.magnitude >= sprintThreshold && move.y > 0.5f;
            knobGraphic.color = sprinting ? sprintTint : knobColor;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != pointerId) return;
            SetIdle();
        }

        void OnDisable() => SetIdle();

        void SetIdle()
        {
            pointerId = NoPointer;
            if (intent != null) intent.TouchMove = Vector2.zero;
            if (stickBase == null) return;
            stickBase.localPosition = restPosition;
            knob.anchoredPosition = Vector2.zero;
            knobGraphic.color = knobColor;
            SetAlpha(0.4f);
        }

        void SetAlpha(float scale)
        {
            baseGraphic.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * scale);
            var k = knobGraphic.color;
            knobGraphic.color = new Color(k.r, k.g, k.b, knobColor.a * scale);
        }

        bool TryGetLocal(PointerEventData eventData, out Vector2 local) =>
            RectTransformUtility.ScreenPointToLocalPointInRectangle(area, eventData.position,
                eventData.pressEventCamera, out local);
    }
}
