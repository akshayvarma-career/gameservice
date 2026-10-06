using UnityEngine;

namespace SilentLedger.Audio
{
    /// <summary>
    /// Plays one-shot sound effects through a fixed pool of AudioSources: 2D voices for the
    /// player's own sounds and UI, 3D voices for sounds out in the world. The voices are created
    /// in the scene by the rig builder, so player builds never strip the AudioSource class.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class Sfx : MonoBehaviour
    {
        [SerializeField] SfxLibrary library;
        [SerializeField] AudioSource[] voices2D;
        [SerializeField] AudioSource[] voices3D;

        public static Sfx Instance { get; private set; }
        public static SfxLibrary Library => Instance != null ? Instance.library : null;

        int next2D;
        int next3D;

        void Awake() => Instance = this;

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Plays a random clip from the set without position (first person or UI).</summary>
        public static void Play2D(AudioClip[] clips, float volume = 1f, float pitchJitter = 0.04f)
        {
            if (Instance == null) return;
            Instance.Play(Instance.voices2D, ref Instance.next2D, clips, volume, pitchJitter, null);
        }

        /// <summary>Plays a random clip from the set at a point in the world.</summary>
        public static void PlayAt(AudioClip[] clips, Vector3 position, float volume = 1f, float pitchJitter = 0.06f)
        {
            if (Instance == null) return;
            Instance.Play(Instance.voices3D, ref Instance.next3D, clips, volume, pitchJitter, position);
        }

        void Play(AudioSource[] voices, ref int next, AudioClip[] clips, float volume, float pitchJitter, Vector3? position)
        {
            if (clips == null || clips.Length == 0 || voices.Length == 0) return;
            var voice = voices[next];
            next = (next + 1) % voices.Length;
            if (position.HasValue) voice.transform.position = position.Value;
            voice.clip = clips[Random.Range(0, clips.Length)];
            voice.volume = volume;
            voice.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            voice.Play();
        }
    }
}
