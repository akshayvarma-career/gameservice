using UnityEngine;

namespace SilentLedger.World
{
    /// <summary>
    /// Stand-in motion for an unrigged character model: a stepping bob and a forward lean while
    /// moving, and slow breathing while idle. Sits on the model (a child of the character root)
    /// and reads the root's movement. Remove once the character has a rig and animations.
    /// </summary>
    public class ProceduralCharacterMotion : MonoBehaviour
    {
        [SerializeField] float strideMetres = 1.4f;
        [SerializeField] float bobHeight = 0.035f;
        [SerializeField] float maxLeanDegrees = 6f;
        [SerializeField] float breathHeight = 0.006f;

        Transform body;
        Vector3 restPosition;
        Quaternion restRotation;
        Vector3 lastPosition;
        float phase;
        float speed;

        void Awake()
        {
            body = transform.parent;
            restPosition = transform.localPosition;
            restRotation = transform.localRotation;
        }

        void OnEnable() => lastPosition = body != null ? body.position : transform.position;

        void Update()
        {
            if (body == null) return;
            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            Vector3 delta = body.position - lastPosition;
            lastPosition = body.position;
            delta.y = 0f;
            // Teleports and vehicle rides would read as huge speeds; ignore them.
            float measured = delta.magnitude / dt > 12f ? 0f : delta.magnitude / dt;
            speed = Mathf.Lerp(speed, measured, 1f - Mathf.Exp(-8f * dt));

            float moving = Mathf.Clamp01(speed / 1.5f);
            phase += speed / strideMetres * Mathf.PI * 2f * dt;
            float bob = Mathf.Abs(Mathf.Sin(phase)) * bobHeight * moving;
            float sway = Mathf.Sin(phase) * 2f * moving;
            float lean = Mathf.Clamp(speed * 1.5f, 0f, maxLeanDegrees);
            // Breathing as a small rise and fall (not scale: Z-up exports have a sideways local Y).
            float breath = (Mathf.Sin(Time.time * 1.6f) * 0.5f + 0.5f) * breathHeight * (1f - moving);

            transform.localPosition = restPosition + Vector3.up * (bob + breath);
            transform.localRotation = Quaternion.Euler(lean, 0f, sway) * restRotation;
        }
    }
}
