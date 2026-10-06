using SilentLedger.Audio;
using UnityEngine;

namespace SilentLedger.Interaction
{
    /// <summary>A hinged door that swings open or closed when used. Sits on the hinge pivot.</summary>
    public class Door : MonoBehaviour, IInteractable
    {
        [SerializeField] float openAngle = 100f;
        [SerializeField] float degreesPerSecond = 240f;

        Quaternion closedRotation;
        float angle;
        bool open;

        public string Prompt => open ? "Close door" : "Open door";
        public bool CanInteract => true;

        void Awake() => closedRotation = transform.localRotation;

        public void Interact(Interactor interactor)
        {
            open = !open;
            var library = Sfx.Library;
            if (library != null)
                Sfx.PlayAt(open ? library.doorOpen : library.doorClose, transform.position + Vector3.up * 1.2f, 0.8f);
        }

        void Update()
        {
            float target = open ? openAngle : 0f;
            if (Mathf.Approximately(angle, target)) return;
            angle = Mathf.MoveTowards(angle, target, degreesPerSecond * Time.deltaTime);
            transform.localRotation = closedRotation * Quaternion.Euler(0f, angle, 0f);
        }
    }
}
