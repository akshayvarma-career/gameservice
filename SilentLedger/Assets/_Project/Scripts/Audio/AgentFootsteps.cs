using UnityEngine;
using UnityEngine.AI;

namespace SilentLedger.Audio
{
    /// <summary>Positional footsteps for a character moved by a NavMeshAgent.</summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class AgentFootsteps : MonoBehaviour
    {
        [SerializeField] float stride = 1.9f;
        [SerializeField, Range(0f, 1f)] float volume = 0.5f;

        NavMeshAgent agent;
        float travelled;

        void Awake() => agent = GetComponent<NavMeshAgent>();

        void Update()
        {
            float speed = agent.velocity.magnitude;
            if (speed < 0.3f)
            {
                travelled = stride * 0.6f;
                return;
            }

            travelled += speed * Time.deltaTime;
            if (travelled < stride || Sfx.Library == null) return;
            travelled = 0f;
            Sfx.PlayAt(Sfx.Library.footstepsConcrete, transform.position, volume, 0.1f);
        }
    }
}
