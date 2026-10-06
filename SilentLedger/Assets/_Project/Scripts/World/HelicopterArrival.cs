using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace SilentLedger.World
{
    /// <summary>
    /// Flies the helicopter in over the base and lands it where it was placed in the scene.
    /// A passenger rides hidden inside and jumps out of the left door just before touchdown,
    /// dropping a bag. Pitch and bank follow the flight path; the rotor sound follows rotor speed.
    /// </summary>
    public class HelicopterArrival : MonoBehaviour
    {
        [Header("Flight")]
        [Tooltip("Where the approach starts, relative to the landing spot.")]
        [SerializeField] Vector3 approachStart = new Vector3(70f, 45f, -160f);
        [Tooltip("Bezier control point shaping the approach, relative to the landing spot.")]
        [SerializeField] Vector3 approachControl = new Vector3(0f, 20f, -55f);
        [SerializeField] float hoverHeight = 12f;
        [SerializeField] float approachSeconds = 10f;
        [SerializeField] float descentSeconds = 5f;
        [Tooltip("Degrees of pitch/bank per m/s² of acceleration.")]
        [SerializeField] float tiltPerAcceleration = 2.5f;
        [SerializeField] float maxTilt = 15f;

        [Header("Parts")]
        [SerializeField] RotorSpin[] rotors;
        [SerializeField] AudioSource rotorAudio;

        [Header("Passenger")]
        [SerializeField] Transform passenger;
        [SerializeField] Transform bag;
        [Tooltip("Left door, relative to the helicopter.")]
        [SerializeField] Vector3 doorOffset = new Vector3(-1.5f, 1f, 0.6f);
        [Tooltip("Where the passenger lands, relative to the landing spot.")]
        [SerializeField] Vector3 jumpLanding = new Vector3(-3.2f, 0f, 0.6f);
        [SerializeField] Vector3 bagLanding = new Vector3(-2.5f, 0.2f, -0.2f);
        [Tooltip("Height above the pad at which the passenger jumps (too early).")]
        [SerializeField] float jumpHeight = 1.3f;

        Vector3 landedPosition;
        Quaternion landedRotation;
        Vector3 lastPosition, lastVelocity;
        Vector2 tilt;
        bool passengerOut;

        public bool HasLanded { get; private set; }

        void Awake()
        {
            landedPosition = transform.position;
            landedRotation = transform.rotation;
        }

        /// <summary>Runs the whole arrival; finishes once the helicopter is on the ground.</summary>
        public IEnumerator Play()
        {
            BoardPassenger();
            transform.SetPositionAndRotation(landedPosition + approachStart, landedRotation);
            lastPosition = transform.position;
            lastVelocity = Vector3.zero;

            // Approach: a curve that ends hovering over the pad, slowing down as it arrives.
            Vector3 p0 = landedPosition + approachStart;
            Vector3 p1 = landedPosition + approachControl;
            Vector3 p2 = landedPosition + Vector3.up * hoverHeight;
            for (float t = 0f; t < 1f; t += Time.deltaTime / approachSeconds)
            {
                float s = 1f - (1f - t) * (1f - t); // ease out
                Vector3 position = Bezier(p0, p1, p2, s);
                Vector3 tangent = Bezier(p0, p1, p2, Mathf.Min(s + 0.01f, 1f)) - position;
                tangent.y = 0f;
                var heading = tangent.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(tangent) : landedRotation;
                // Turn onto the landing heading over the last stretch.
                heading = Quaternion.Slerp(heading, landedRotation, Mathf.SmoothStep(0f, 1f, (s - 0.75f) / 0.25f));
                Fly(position, heading);
                yield return null;
            }

            // Descent: settle straight down onto the pad.
            for (float t = 0f; t < 1f; t += Time.deltaTime / descentSeconds)
            {
                float height = Mathf.Lerp(hoverHeight, 0f, Mathf.SmoothStep(0f, 1f, t));
                Fly(landedPosition + Vector3.up * height, landedRotation);
                if (!passengerOut && height <= jumpHeight) StartCoroutine(PassengerJumps());
                yield return null;
            }

            transform.SetPositionAndRotation(landedPosition, landedRotation);
            if (!passengerOut) StartCoroutine(PassengerJumps());
            foreach (var rotor in rotors) rotor.BeginSpinDown();
            HasLanded = true;
            while (passenger != null && passenger.parent == transform) yield return null;
        }

        /// <summary>Moves to a point on the path, pitching and banking with the acceleration.</summary>
        void Fly(Vector3 position, Quaternion heading)
        {
            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            Vector3 velocity = (position - lastPosition) / dt;
            Vector3 acceleration = (velocity - lastVelocity) / dt;
            lastPosition = position;
            lastVelocity = velocity;

            Vector3 local = Quaternion.Inverse(heading) * acceleration;
            var target = new Vector2(
                Mathf.Clamp(local.z * tiltPerAcceleration, -maxTilt, maxTilt),   // speeding up: nose down
                Mathf.Clamp(-local.x * tiltPerAcceleration, -maxTilt, maxTilt)); // turning: bank into it
            tilt = Vector2.Lerp(tilt, target, 1f - Mathf.Exp(-3f * dt));
            transform.SetPositionAndRotation(position, heading * Quaternion.Euler(tilt.x, 0f, tilt.y));
        }

        void Update()
        {
            if (rotorAudio == null || rotors == null || rotors.Length == 0) return;
            float throttle = rotors[0].Throttle;
            rotorAudio.volume = throttle;
            rotorAudio.pitch = 0.55f + 0.45f * throttle;
            if (throttle <= 0.02f && rotorAudio.isPlaying) rotorAudio.Stop();
        }

        void BoardPassenger()
        {
            passengerOut = false;
            if (passenger == null) return;
            SetAgent(false);
            passenger.SetParent(transform, false);
            passenger.localPosition = doorOffset + Vector3.right * 0.8f; // seated just inside the door
            passenger.localRotation = Quaternion.Euler(0f, -90f, 0f);  // facing out of the left side
            SetVisible(passenger, false);
            if (bag != null)
            {
                bag.SetParent(passenger, false);
                bag.localPosition = new Vector3(0.3f, 0.9f, 0.2f);
                SetVisible(bag, false);
            }
        }

        IEnumerator PassengerJumps()
        {
            passengerOut = true;
            if (passenger == null) yield break;

            passenger.SetParent(null, true);
            passenger.position = transform.TransformPoint(doorOffset);
            SetVisible(passenger, true);
            if (bag != null) SetVisible(bag, true);

            // A short hop out of the door, arcing down to the ground.
            Vector3 start = passenger.position;
            Vector3 end = landedPosition + landedRotation * jumpLanding;
            var facing = Quaternion.LookRotation(Vector3.ProjectOnPlane(end - start, Vector3.up));
            passenger.rotation = facing;
            const float hopSeconds = 0.6f;
            for (float t = 0f; t < 1f; t += Time.deltaTime / hopSeconds)
            {
                passenger.position = Vector3.Lerp(start, end, t) + Vector3.up * (Mathf.Sin(t * Mathf.PI) * 0.6f);
                yield return null;
            }
            passenger.position = end;

            // The bag slips off and lands beside him.
            if (bag != null)
            {
                bag.SetParent(null, true);
                Vector3 bagStart = bag.position, bagEnd = landedPosition + landedRotation * bagLanding;
                for (float t = 0f; t < 1f; t += Time.deltaTime / 0.35f)
                {
                    bag.position = Vector3.Lerp(bagStart, bagEnd, t * t);
                    yield return null;
                }
                bag.position = bagEnd;
            }
            SetAgent(true);
        }

        void SetAgent(bool active)
        {
            var agent = passenger.GetComponent<NavMeshAgent>();
            if (agent == null) return;
            agent.enabled = active;
            if (active && NavMesh.SamplePosition(passenger.position, out var hit, 2f, NavMesh.AllAreas))
                agent.Warp(hit.position);
        }

        static void SetVisible(Transform root, bool visible)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true)) renderer.enabled = visible;
        }

        static Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, float t) =>
            Vector3.Lerp(Vector3.Lerp(a, b, t), Vector3.Lerp(b, c, t), t);
    }
}
