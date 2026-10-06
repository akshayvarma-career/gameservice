using UnityEngine;

namespace SilentLedger.Interaction
{
    /// <summary>Shows a briefing message when used, such as a tour stop or a range instruction.</summary>
    public class InfoPoint : MonoBehaviour, IInteractable
    {
        [SerializeField] string prompt = "Read";
        [SerializeField, TextArea] string message = "";
        [SerializeField] float seconds = 6f;

        public string Prompt => prompt;
        public bool CanInteract => true;

        public void Interact(Interactor interactor) => interactor.ShowMessage(message, seconds);
    }
}
