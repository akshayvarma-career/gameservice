namespace SilentLedger.Interaction
{
    public interface IInteractable
    {
        /// <summary>Short action label shown on screen, such as "Open door".</summary>
        string Prompt { get; }
        bool CanInteract { get; }
        void Interact(Interactor interactor);
    }
}
