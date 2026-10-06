using System;
using SilentLedger.Interaction;
using UnityEngine;

namespace SilentLedger.Mission
{
    /// <summary>An object the mission switches on when the player should use it, such as a loadout bench.</summary>
    public class MissionInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] string prompt = "Use";

        public bool Active { get; set; }
        public event Action Used;

        public string Prompt => prompt;
        public bool CanInteract => Active;

        public void Interact(Interactor interactor)
        {
            Active = false;
            Used?.Invoke();
        }
    }
}
