using System;
using System.Collections.Generic;
using System.IO;
using SilentLedger.Audio;
using UnityEditor;
using UnityEngine;

namespace SilentLedger.EditorTools
{
    /// <summary>
    /// Synthesizes placeholder sound effects as WAV files, imports them with mobile-friendly
    /// settings and fills the SfxLibrary. Replace any WAV with a real recording of the same name
    /// and the game picks it up without code changes.
    /// </summary>
    public static class SfxGenerator
    {
        public const string Folder = GreyboxKit.Root + "/Audio/SFX";
        public const string LibraryPath = GreyboxKit.Root + "/Audio/SfxLibrary.asset";
        const int Rate = 44100;

        [MenuItem("Silent Ledger/Generate Placeholder SFX")]
        public static string Generate()
        {
            GreyboxKit.EnsureFolder(GreyboxKit.Root + "/Audio", "SFX");
            var written = new List<string>();

            for (int i = 0; i < 3; i++)
            {
                written.Add(Write($"rifle_shot_{i + 1:00}", Gunshot(100 + i, crackTau: 0.004f, bodyTau: 0.05f, thumpBase: 55f, thumpTau: 0.08f, tailTau: 0.22f, length: 0.6f)));
                written.Add(Write($"pistol_shot_{i + 1:00}", Gunshot(200 + i, crackTau: 0.003f, bodyTau: 0.035f, thumpBase: 85f, thumpTau: 0.05f, tailTau: 0.15f, length: 0.45f)));
                written.Add(Write($"impact_concrete_{i + 1:00}", Impact(300 + i)));
            }
            for (int i = 0; i < 6; i++)
                written.Add(Write($"footstep_concrete_{i + 1:00}", Footstep(400 + i)));
            for (int i = 0; i < 2; i++)
                written.Add(Write($"target_hit_{i + 1:00}", TargetPing(500 + i)));

            written.Add(Write("stance_change_01", Rustle(600, 0.35f)));
            written.Add(Write("dry_fire_01", DryFire(700)));
            written.Add(Write("mag_out_01", MagazineOut(800)));
            written.Add(Write("mag_in_01", MagazineIn(900)));
            written.Add(Write("bolt_rack_01", BoltRack(1000)));
            written.Add(Write("weapon_swap_01", WeaponSwap(1100)));
            written.Add(Write("door_open_01", DoorOpen(1200)));
            written.Add(Write("door_close_01", DoorClose(1300)));
            written.Add(Write("ui_hit_marker_01", Blips(new[] { (0f, 2800f) }, 0.07f, 0.015f)));
            written.Add(Write("ui_kill_marker_01", Blips(new[] { (0f, 2600f), (0.07f, 3400f) }, 0.2f, 0.02f)));
            written.Add(Write("ui_objective_01", Blips(new[] { (0f, 660f), (0.12f, 990f) }, 0.7f, 0.22f)));

            AssetDatabase.Refresh();
            foreach (var path in written) ConfigureImport(path);
            BuildLibrary();
            SetGoodLatencyDspBuffer();
            AssetDatabase.SaveAssets();
            return $"Generated {written.Count} clips in {Folder}";
        }

        /// <summary>Generates the clips if the library does not exist yet.</summary>
        public static SfxLibrary EnsureGenerated()
        {
            if (AssetDatabase.LoadAssetAtPath<SfxLibrary>(LibraryPath) == null) Generate();
            return AssetDatabase.LoadAssetAtPath<SfxLibrary>(LibraryPath);
        }

        public static AudioClip[] Clips(string prefix)
        {
            var clips = new List<AudioClip>();
            foreach (var guid in AssetDatabase.FindAssets($"{prefix} t:AudioClip", new[] { Folder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path).StartsWith(prefix))
                    clips.Add(AssetDatabase.LoadAssetAtPath<AudioClip>(path));
            }
            clips.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            return clips.ToArray();
        }

        static void BuildLibrary()
        {
            var library = AssetDatabase.LoadAssetAtPath<SfxLibrary>(LibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<SfxLibrary>();
                AssetDatabase.CreateAsset(library, LibraryPath);
            }
            library.footstepsConcrete = Clips("footstep_concrete_");
            library.stanceChange = Clips("stance_change_");
            library.dryFire = Clips("dry_fire_");
            library.magazineOut = Clips("mag_out_");
            library.magazineIn = Clips("mag_in_");
            library.boltRack = Clips("bolt_rack_");
            library.weaponSwap = Clips("weapon_swap_");
            library.impactConcrete = Clips("impact_concrete_");
            library.targetHit = Clips("target_hit_");
            library.doorOpen = Clips("door_open_");
            library.doorClose = Clips("door_close_");
            library.hitMarker = Clips("ui_hit_marker_");
            library.killMarker = Clips("ui_kill_marker_");
            library.objectiveUpdated = Clips("ui_objective_");
            EditorUtility.SetDirty(library);
        }

        // ---------------------------------------------------------------- import settings

        /// <summary>
        /// Short SFX on mobile: mono, decompressed on load (no per-play decode cost), Vorbis on
        /// disk, and a 22 kHz override on Android to halve memory.
        /// </summary>
        static void ConfigureImport(string path)
        {
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            importer.forceToMono = true;
            importer.loadInBackground = false;

            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.6f;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;

            var android = settings;
            android.sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate;
            android.sampleRateOverride = 22050;
            importer.SetOverrideSampleSettings("Android", android);
            importer.SaveAndReimport();
        }

        /// <summary>
        /// "Good latency" (512 samples): responsive gunfire without the CPU cost of "Best latency".
        /// m_DSPBufferSize is derived; the setting itself is the m_RequestedDSPBufferSize enum.
        /// </summary>
        static void SetGoodLatencyDspBuffer()
        {
            var manager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/AudioManager.asset")[0];
            var settings = new SerializedObject(manager);
            var requested = settings.FindProperty("m_RequestedDSPBufferSize");
            int goodLatency = System.Array.IndexOf(requested.enumNames, "Good latency");
            if (goodLatency < 0 || requested.enumValueIndex == goodLatency) return;
            requested.enumValueIndex = goodLatency;
            settings.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(manager);
        }

        // ---------------------------------------------------------------- sounds

        static float[] Gunshot(int seed, float crackTau, float bodyTau, float thumpBase, float thumpTau, float tailTau, float length)
        {
            var rng = new System.Random(seed);
            float jitter = 1f + ((float)rng.NextDouble() - 0.5f) * 0.12f;
            var s = new float[Samples(length)];
            var crack = HighPass(Noise(rng, s.Length), 2000f);
            var body = LowPass(Noise(rng, s.Length), 1800f * jitter);
            var tail = LowPass(LowPass(Noise(rng, s.Length), 900f), 900f);
            float phase = 0f;
            for (int i = 0; i < s.Length; i++)
            {
                float t = i / (float)Rate;
                float freq = (thumpBase + 90f * Mathf.Exp(-t / 0.03f)) * jitter;
                phase += 2f * Mathf.PI * freq / Rate;
                s[i] = crack[i] * Mathf.Exp(-t / crackTau)
                     + body[i] * 0.9f * Mathf.Exp(-t / bodyTau)
                     + Mathf.Sin(phase) * 0.9f * Mathf.Exp(-t / thumpTau)
                     + tail[i] * 0.3f * Mathf.Exp(-t / tailTau);
                s[i] = (float)Math.Tanh(s[i] * 1.8f);
            }
            return Normalize(s, 0.95f);
        }

        static float[] Footstep(int seed)
        {
            var rng = new System.Random(seed);
            float jitter = 1f + ((float)rng.NextDouble() - 0.5f) * 0.3f;
            var s = new float[Samples(0.2f)];
            var grit = LowPass(HighPass(Noise(rng, s.Length), 900f), 4000f);
            var scuff = LowPass(Noise(rng, s.Length), 2500f);
            float phase = 0f;
            for (int i = 0; i < s.Length; i++)
            {
                float t = i / (float)Rate;
                phase += 2f * Mathf.PI * (75f + 40f * Mathf.Exp(-t / 0.01f)) * jitter / Rate;
                float scuffT = t - 0.03f;
                s[i] = Mathf.Sin(phase) * 0.6f * Mathf.Exp(-t / 0.02f)
                     + grit[i] * 0.5f * Mathf.Exp(-t / 0.015f)
                     + (scuffT > 0f ? scuff[i] * 0.25f * Mathf.Exp(-scuffT / 0.025f) : 0f);
            }
            return Normalize(s, 0.6f);
        }

        static float[] Impact(int seed)
        {
            var rng = new System.Random(seed);
            var s = new float[Samples(0.3f)];
            var crack = HighPass(Noise(rng, s.Length), 2000f);
            var debris = LowPass(Noise(rng, s.Length), 5000f);
            for (int i = 0; i < s.Length; i++)
            {
                float t = i / (float)Rate;
                float sprinkle = rng.NextDouble() < 0.002 * Mathf.Exp(-t / 0.08f) ? 1f : 0f;
                s[i] = crack[i] * Mathf.Exp(-t / 0.003f)
                     + debris[i] * 0.4f * Mathf.Exp(-t / 0.05f)
                     + sprinkle * 0.5f;
            }
            return Normalize(LowPass(s, 9000f), 0.8f);
        }

        static float[] TargetPing(int seed)
        {
            var rng = new System.Random(seed);
            float jitter = 1f + ((float)rng.NextDouble() - 0.5f) * 0.06f;
            var s = Tones(0.6f, (820f * jitter, 0.25f, 0.6f), (1960f * jitter, 0.12f, 0.35f), (3150f * jitter, 0.06f, 0.25f));
            var click = HighPass(Noise(rng, s.Length), 3000f);
            for (int i = 0; i < s.Length; i++) s[i] += click[i] * Mathf.Exp(-i / (float)Rate / 0.002f);
            return Normalize(s, 0.75f);
        }

        static float[] Rustle(int seed, float length)
        {
            var rng = new System.Random(seed);
            var s = LowPass(HighPass(Noise(rng, Samples(length)), 1500f), 6000f);
            for (int i = 0; i < s.Length; i++)
            {
                float t = i / (float)s.Length;
                s[i] *= Mathf.Sin(Mathf.PI * Mathf.Pow(t, 0.6f)) * (0.6f + 0.4f * Mathf.PerlinNoise(t * 20f, seed));
            }
            return Normalize(s, 0.35f);
        }

        static float[] DryFire(int seed) =>
            Normalize(Mix(Samples(0.12f), Click(seed, 0f, 3000f, 0.002f, 1f), Tones(0.12f, (2200f, 0.02f, 0.3f))), 0.5f);

        static float[] MagazineOut(int seed)
        {
            var rng = new System.Random(seed);
            var slide = LowPass(Noise(rng, Samples(0.3f)), 3000f);
            for (int i = 0; i < slide.Length; i++)
            {
                float t = i / (float)Rate - 0.02f;
                slide[i] = t > 0f ? slide[i] * 0.4f * Mathf.Exp(-t / 0.08f) : 0f;
            }
            return Normalize(Mix(slide.Length, Click(seed, 0f, 2000f, 0.004f, 1f), slide), 0.6f);
        }

        static float[] MagazineIn(int seed) =>
            Normalize(Mix(Samples(0.25f), Click(seed, 0f, 2000f, 0.004f, 1f), Thump(0.25f, 180f, 0.015f, 0.8f),
                Tones(0.25f, (1600f, 0.05f, 0.2f), (2700f, 0.04f, 0.15f))), 0.7f);

        static float[] BoltRack(int seed) =>
            Normalize(Mix(Samples(0.4f), Click(seed, 0f, 1500f, 0.005f, 1f), Click(seed + 1, 0.12f, 1500f, 0.005f, 1f),
                Tones(0.4f, (1100f, 0.04f, 0.25f), (2300f, 0.03f, 0.15f)), Delay(Tones(0.28f, (1150f, 0.04f, 0.25f)), 0.12f, 0.4f)), 0.75f);

        static float[] WeaponSwap(int seed) =>
            Normalize(Mix(Samples(0.35f), Rustle(seed, 0.2f), Click(seed, 0.2f, 2500f, 0.003f, 0.6f)), 0.5f);

        static float[] DoorOpen(int seed)
        {
            var rng = new System.Random(seed);
            var s = new float[Samples(0.9f)];
            var grain = LowPass(Noise(rng, s.Length), 1200f);
            float phase = 0f;
            for (int i = 0; i < s.Length; i++)
            {
                float t = i / (float)Rate;
                float freq = 140f + 70f * t + 6f * Mathf.Sin(2f * Mathf.PI * 7f * t);
                phase += 2f * Mathf.PI * freq / Rate;
                float env = Mathf.Clamp01(t / 0.1f) * Mathf.Exp(-t / 0.5f);
                s[i] = (Mathf.Sin(phase) * 0.4f + Mathf.Sin(phase * 2.01f) * 0.2f) * env * (0.7f + grain[i]);
            }
            return Normalize(Mix(s.Length, s, Click(seed, 0f, 2500f, 0.003f, 0.5f)), 0.55f);
        }

        static float[] DoorClose(int seed) =>
            Normalize(Mix(Samples(0.4f), Thump(0.4f, 90f, 0.06f, 1f), Click(seed, 0.01f, 2000f, 0.004f, 0.6f)), 0.7f);

        static float[] Blips((float start, float freq)[] notes, float length, float tau)
        {
            var s = new float[Samples(length)];
            foreach (var (start, freq) in notes)
            {
                int offset = Samples(start);
                for (int i = offset; i < s.Length; i++)
                {
                    float t = (i - offset) / (float)Rate;
                    s[i] += Mathf.Sin(2f * Mathf.PI * freq * t) * Mathf.Clamp01(t / 0.005f) * Mathf.Exp(-t / tau);
                }
            }
            return Normalize(s, 0.5f);
        }

        // ---------------------------------------------------------------- building blocks

        static int Samples(float seconds) => Mathf.CeilToInt(seconds * Rate);

        static float[] Noise(System.Random rng, int count)
        {
            var s = new float[count];
            for (int i = 0; i < count; i++) s[i] = (float)(rng.NextDouble() * 2.0 - 1.0);
            return s;
        }

        static float[] Click(int seed, float start, float highPassHz, float tau, float gain)
        {
            var rng = new System.Random(seed);
            int offset = Samples(start);
            var burst = HighPass(Noise(rng, Samples(start + tau * 8f)), highPassHz);
            for (int i = 0; i < burst.Length; i++)
                burst[i] = i < offset ? 0f : burst[i] * gain * Mathf.Exp(-(i - offset) / (float)Rate / tau);
            return burst;
        }

        static float[] Thump(float length, float freq, float tau, float gain)
        {
            var s = new float[Samples(length)];
            for (int i = 0; i < s.Length; i++)
            {
                float t = i / (float)Rate;
                s[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * gain * Mathf.Exp(-t / tau);
            }
            return s;
        }

        static float[] Tones(float length, params (float freq, float tau, float gain)[] partials)
        {
            var s = new float[Samples(length)];
            for (int i = 0; i < s.Length; i++)
            {
                float t = i / (float)Rate;
                foreach (var (freq, tau, gain) in partials)
                    s[i] += Mathf.Sin(2f * Mathf.PI * freq * t) * gain * Mathf.Exp(-t / tau);
            }
            return s;
        }

        static float[] Delay(float[] source, float seconds, float gain)
        {
            int offset = Samples(seconds);
            var s = new float[source.Length + offset];
            for (int i = 0; i < source.Length; i++) s[i + offset] = source[i] * gain;
            return s;
        }

        static float[] Mix(int length, params float[][] layers)
        {
            var s = new float[length];
            foreach (var layer in layers)
                for (int i = 0; i < Mathf.Min(length, layer.Length); i++) s[i] += layer[i];
            return s;
        }

        static float[] LowPass(float[] input, float cutoff)
        {
            float a = 1f - Mathf.Exp(-2f * Mathf.PI * cutoff / Rate);
            var s = new float[input.Length];
            float y = 0f;
            for (int i = 0; i < input.Length; i++) s[i] = y += a * (input[i] - y);
            return s;
        }

        static float[] HighPass(float[] input, float cutoff)
        {
            var low = LowPass(input, cutoff);
            var s = new float[input.Length];
            for (int i = 0; i < input.Length; i++) s[i] = input[i] - low[i];
            return s;
        }

        static float[] Normalize(float[] s, float peak)
        {
            float max = 0f;
            foreach (var v in s) max = Mathf.Max(max, Mathf.Abs(v));
            if (max < 1e-6f) return s;
            float gain = peak / max;
            int fade = Mathf.Min(s.Length, Samples(0.005f));
            for (int i = 0; i < s.Length; i++)
            {
                s[i] *= gain;
                int fromEnd = s.Length - 1 - i;
                if (fromEnd < fade) s[i] *= fromEnd / (float)fade;
            }
            return s;
        }

        static string Write(string name, float[] samples)
        {
            string path = $"{Folder}/{name}.wav";
            using var writer = new BinaryWriter(File.Create(path));
            int dataBytes = samples.Length * 2;
            writer.Write("RIFF".ToCharArray());
            writer.Write(36 + dataBytes);
            writer.Write("WAVE".ToCharArray());
            writer.Write("fmt ".ToCharArray());
            writer.Write(16);
            writer.Write((short)1);          // PCM
            writer.Write((short)1);          // mono
            writer.Write(Rate);
            writer.Write(Rate * 2);          // byte rate
            writer.Write((short)2);          // block align
            writer.Write((short)16);         // bits per sample
            writer.Write("data".ToCharArray());
            writer.Write(dataBytes);
            foreach (var v in samples)
                writer.Write((short)Mathf.RoundToInt(Mathf.Clamp(v, -1f, 1f) * short.MaxValue));
            return path;
        }
    }
}
