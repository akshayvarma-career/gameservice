using SilentLedger.Player;
using UnityEngine;

namespace SilentLedger.Audio
{
    /// <summary>
    /// The player's footsteps, timed by distance travelled. Strides and volume follow the stance:
    /// sprinting is loud, crouching quiet, prone almost silent. Also plays a rustle on stance change.
    /// </summary>
    public class Footsteps : MonoBehaviour
    {
        [SerializeField] PlayerController controller;

        [Header("Stride (m between steps)")]
        [SerializeField] float walkStride = 1.9f;
        [SerializeField] float sprintStride = 2.6f;
        [SerializeField] float crouchStride = 1.2f;
        [SerializeField] float proneStride = 0.9f;

        [Header("Volume")]
        [SerializeField, Range(0f, 1f)] float walkVolume = 0.35f;
        [SerializeField, Range(0f, 1f)] float sprintVolume = 0.5f;
        [SerializeField, Range(0f, 1f)] float crouchVolume = 0.18f;
        [SerializeField, Range(0f, 1f)] float proneVolume = 0.1f;

        float travelled;
        PlayerController.Stance lastStance;

        void Start() => lastStance = controller.CurrentStance;

        void Update()
        {
            var library = Sfx.Library;
            if (library == null) return;

            if (controller.CurrentStance != lastStance)
            {
                lastStance = controller.CurrentStance;
                Sfx.Play2D(library.stanceChange, 0.5f, 0.1f);
            }

            var (stride, volume) = StepFor(controller);
            if (!controller.IsGrounded || controller.Speed < 0.5f)
            {
                travelled = stride * 0.6f; // the first step lands soon after starting to move
                return;
            }

            travelled += controller.Speed * Time.deltaTime;
            if (travelled < stride) return;
            travelled = 0f;
            Sfx.Play2D(library.footstepsConcrete, volume, 0.08f);
        }

        (float stride, float volume) StepFor(PlayerController player)
        {
            if (player.IsSprinting) return (sprintStride, sprintVolume);
            return player.CurrentStance switch
            {
                PlayerController.Stance.Crouching => (crouchStride, crouchVolume),
                PlayerController.Stance.Prone => (proneStride, proneVolume),
                _ => (walkStride, walkVolume),
            };
        }
    }
}
