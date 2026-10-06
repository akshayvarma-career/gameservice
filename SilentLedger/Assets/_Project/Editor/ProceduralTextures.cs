using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SilentLedger.EditorTools
{
    /// <summary>
    /// Seamless placeholder textures generated from noise. Surface textures (concrete, tarmac,
    /// metal) are near-white detail maps that the material colour tints; terrain textures carry
    /// their own colour. Replace any PNG with a real texture of the same name.
    /// </summary>
    public static class ProceduralTextures
    {
        const string Folder = GreyboxKit.Root + "/Generated/Textures";
        const int Size = 256;

        public static Texture2D Concrete() => Get("concrete", (u, v) =>
        {
            float n = Fbm(u, v, 8f, 4, 1) * 0.12f + Fbm(u, v, 2f, 2, 2) * 0.08f;
            float speck = Hash(u, v, 3) > 0.985f ? -0.18f : 0f;
            return Grey(0.86f + n + speck);
        });

        public static Texture2D Tarmac() => Get("tarmac", (u, v) =>
        {
            float grain = (Hash(u, v, 4) - 0.5f) * 0.18f;
            float stone = Hash(u, v, 5) > 0.97f ? 0.15f : 0f;
            return Grey(0.85f + grain + stone + Fbm(u, v, 3f, 3, 6) * 0.08f);
        });

        public static Texture2D MetalPanel() => Get("metal_panel", (u, v) =>
        {
            float seamU = Mathf.Abs(Frac(u * 2f) - 0.5f) > 0.492f ? -0.25f : 0f;
            float seamV = Mathf.Abs(Frac(v * 1f) - 0.5f) > 0.494f ? -0.25f : 0f;
            float streak = Fbm(u * 0.3f, v * 6f, 6f, 3, 7) * 0.06f;
            return Grey(0.9f + Mathf.Min(seamU, seamV) + streak + (Hash(u, v, 8) - 0.5f) * 0.03f);
        });

        // Terrain textures: URP Terrain/Lit reads albedo alpha as smoothness, so alpha stays low
        // (rough ground). With solid alpha the valley floor renders like a mirror.

        public static Texture2D DryGrass() => Get("terrain_dry_grass", (u, v) =>
        {
            float n = Fbm(u, v, 6f, 4, 10);
            float blade = (Hash(u, v, 11) - 0.5f) * 0.12f;
            var dry = new Color(0.55f, 0.52f, 0.33f);
            var green = new Color(0.38f, 0.43f, 0.25f);
            return Color.Lerp(green, dry, Mathf.Clamp01(0.5f + n * 1.6f)) * (1f + blade);
        }, smoothness: 0.05f);

        public static Texture2D Rock() => Get("terrain_rock", (u, v) =>
        {
            float n = Fbm(u, v, 4f, 5, 12);
            float strata = Mathf.Sin((v + n * 0.3f) * Mathf.PI * 12f) * 0.04f;
            return new Color(0.46f, 0.45f, 0.44f) * (1f + n * 0.7f + strata);
        }, smoothness: 0.12f);

        public static Texture2D Dirt() => Get("terrain_dirt", (u, v) =>
        {
            float n = Fbm(u, v, 5f, 4, 13);
            float pebble = Hash(u, v, 14) > 0.97f ? 0.2f : 0f;
            return new Color(0.42f, 0.35f, 0.27f) * (1f + n * 0.5f + pebble);
        }, smoothness: 0.05f);

        public static Texture2D Snow() => Get("terrain_snow", (u, v) =>
            new Color(0.93f, 0.95f, 0.98f) * (1f + Fbm(u, v, 3f, 3, 15) * 0.08f), smoothness: 0.3f);

        // ---------------------------------------------------------------- generation

        /// <param name="smoothness">Stored in alpha; null writes an opaque RGB texture.</param>
        static Texture2D Get(string name, Func<float, float, Color> pixel, float? smoothness = null)
        {
            string path = $"{Folder}/{name}.png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null) return existing;

            GreyboxKit.EnsureFolder(GreyboxKit.Root + "/Generated", "Textures");
            var format = smoothness.HasValue ? TextureFormat.RGBA32 : TextureFormat.RGB24;
            var texture = new Texture2D(Size, Size, format, false);
            var pixels = new Color[Size * Size];
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                var c = pixel(x / (float)Size, y / (float)Size);
                pixels[y * Size + x] = new Color(Mathf.Clamp01(c.r), Mathf.Clamp01(c.g), Mathf.Clamp01(c.b), smoothness ?? 1f);
            }
            texture.SetPixels(pixels);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.alphaSource = smoothness.HasValue ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
            importer.alphaIsTransparency = false;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.mipmapEnabled = true;
            importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 2;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Color Grey(float value) => new Color(value, value, value);

        static float Frac(float x) => x - Mathf.Floor(x);

        /// <summary>Tileable value hash per texel (wraps at the texture edge).</summary>
        static float Hash(float u, float v, int seed)
        {
            int x = Mathf.FloorToInt(Frac(u) * Size), y = Mathf.FloorToInt(Frac(v) * Size);
            uint h = (uint)(x * 374761393 + y * 668265263 + seed * 1442695041);
            h = (h ^ (h >> 13)) * 1274126177;
            return (h ^ (h >> 16)) / (float)uint.MaxValue;
        }

        /// <summary>Seamless fractal noise in roughly -0.5..0.5, periodic over the 0..1 tile.</summary>
        static float Fbm(float u, float v, float frequency, int octaves, int seed)
        {
            float sum = 0f, amplitude = 0.5f;
            for (int o = 0; o < octaves; o++)
            {
                sum += amplitude * (TileNoise(u, v, frequency, seed + o * 31) - 0.5f);
                frequency *= 2f;
                amplitude *= 0.5f;
            }
            return sum;
        }

        /// <summary>Perlin noise made seamless by blending the four wrapped samples.</summary>
        static float TileNoise(float u, float v, float frequency, int seed)
        {
            float ox = seed * 17.13f, oy = seed * 9.71f;
            float x = u * frequency, y = v * frequency, w = frequency;
            float a = Mathf.PerlinNoise(ox + x, oy + y);
            float b = Mathf.PerlinNoise(ox + x - w, oy + y);
            float c = Mathf.PerlinNoise(ox + x, oy + y - w);
            float d = Mathf.PerlinNoise(ox + x - w, oy + y - w);
            float wx = 1f - u, wy = 1f - v;
            return a * wx * wy + b * u * wy + c * wx * v + d * u * v;
        }
    }
}
