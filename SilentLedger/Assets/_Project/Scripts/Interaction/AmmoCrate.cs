using SilentLedger.Weapons;
using UnityEngine;

namespace SilentLedger.Interaction
{
    /// <summary>Refills all of the player's weapons.</summary>
    public class AmmoCrate : MonoBehaviour, IInteractable
    {
        public string Prompt => "Resupply";
        public bool CanInteract => true;

        public void Interact(Interactor interactor)
        {
            var holder = interactor.GetComponent<WeaponHolder>();
            if (holder == null) return;
            holder.Refill();
            interactor.ShowMessage("Ammo refilled.", 2f);
        }
    }
}
