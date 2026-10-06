using SilentLedger.Player;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SilentLedger.UI
{
    /// <summary>Turns the view when a finger drags anywhere on the right side of the screen.</summary>
    public class LookPad : MonoBehaviour,
        IPointerDownHandler, IDragHandler, IPointerUpHandler, IInitializePotentialDragHandler
    {
        const int NoPointer = int.MinValue;

        [SerializeField] PlayerIntent intent;

        int pointerId = NoPointer;

        public void OnInitializePotentialDrag(PointerEventData eventData) => eventData.useDragThreshold = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (pointerId == NoPointer) pointerId = eventData.pointerId;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId == pointerId) intent.AddTouchLook(eventData.delta);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId == pointerId) pointerId = NoPointer;
        }

        void OnDisable() => pointerId = NoPointer;
    }
}
