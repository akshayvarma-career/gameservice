using SilentLedger.Interaction;
using SilentLedger.Mission;
using SilentLedger.Weapons;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static SilentLedger.EditorTools.GreyboxKit;

namespace SilentLedger.EditorTools
{
    /// <summary>
    /// Free, procedural environment art for a level: mountain terrain around a flat valley,
    /// pines, a dawn sky with fog, trilight ambient, a mobile-friendly post-processing volume,
    /// and static flags so the level batches well on phones.
    /// </summary>
    public static class EnvironmentBuilder
    {
        const string GeneratedFolder = Root + "/Generated";

        public struct Valley
        {
            /// <summary>Flat, buildable rectangle in world XZ (min x, min z, max x, max z).</summary>
            public Rect flat;
            /// <summary>Distance over which the floor rises into the mountains.</summary>
            public float rise;
        }

        // ---------------------------------------------------------------- terrain

        /// <summary>
        /// Terrain covering origin..origin+size, flat (height 0) inside the valley and rising into
        /// snow-capped ridges outside it, painted grass, dirt, rock and snow.
        /// </summary>
        public static Terrain BuildTerrain(Transform parent, string name, Vector3 origin, Vector3 size, Valley valley, int seed)
        {
            EnsureFolder(Root, "Generated");
            EnsureFolder(GeneratedFolder, "Terrain");
            string dataPath = $"{GeneratedFolder}/Terrain/{name}_TerrainData.asset";
            AssetDatabase.DeleteAsset(dataPath);

            const int heightRes = 257;
            var data = new TerrainData { heightmapResolution = heightRes, alphamapResolution = 256 };
            data.size = size;

            var heights = new float[heightRes, heightRes];
            for (int zi = 0; zi < heightRes; zi++)
            for (int xi = 0; xi < heightRes; xi++)
            {
                float x = origin.x + xi / (float)(heightRes - 1) * size.x;
                float z = origin.z + zi / (float)(heightRes - 1) * size.z;
                heights[zi, xi] = Height(x, z, valley, seed) / size.y;
            }
            data.SetHeights(0, 0, heights);

            data.terrainLayers = new[]
            {
                Layer("DryGrass", ProceduralTextures.DryGrass(), 8f),
                Layer("Dirt", ProceduralTextures.Dirt(), 6f),
                Layer("Rock", ProceduralTextures.Rock(), 14f),
                Layer("Snow", ProceduralTextures.Snow(), 12f),
            };
            Paint(data, origin, valley, seed);
            AssetDatabase.CreateAsset(data, dataPath);

            var go = Terrain.CreateTerrainGameObject(data);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = origin;
            var terrain = go.GetComponent<Terrain>();
            terrain.materialTemplate = TerrainMaterial();
            terrain.heightmapPixelError = 8f;       // mobile: fewer triangles
            terrain.basemapDistance = 250f;
            terrain.drawInstanced = true;
            terrain.shadowCastingMode = ShadowCastingMode.Off;
            terrain.treeDistance = 0f;
            terrain.detailObjectDistance = 0f;
            return terrain;
        }

        /// <summary>World-space height: 0 in the valley, ridged mountains beyond it.</summary>
        static float Height(float x, float z, Valley valley, int seed)
        {
            float dx = Mathf.Max(valley.flat.xMin - x, 0f, x - valley.flat.xMax);
            float dz = Mathf.Max(valley.flat.yMin - z, 0f, z - valley.flat.yMax);
            float distance = Mathf.Sqrt(dx * dx + dz * dz);
            if (distance <= 0f) return 0f;

            float mask = Mathf.SmoothStep(0f, 1f, distance / valley.rise);
            float ridged = 0f, amplitude = 1f, frequency = 0.004f;
            for (int o = 0; o < 5; o++)
            {
                float n = 1f - Mathf.Abs(Mathf.PerlinNoise(seed + x * frequency, seed * 0.7f + z * frequency) * 2f - 1f);
                ridged += n * n * amplitude;
                amplitude *= 0.5f;
                frequency *= 2.1f;
            }
            float foothills = distance * 0.12f;
            return mask * (ridged * 75f + Mathf.Min(foothills, 40f));
        }

        static void Paint(TerrainData data, Vector3 origin, Valley valley, int seed)
        {
            int res = data.alphamapResolution;
            var maps = new float[res, res, 4];
            float snowLine = data.size.y * 0.55f;
            for (int zi = 0; zi < res; zi++)
            for (int xi = 0; xi < res; xi++)
            {
                float u = xi / (float)(res - 1), v = zi / (float)(res - 1);
                float h = data.GetInterpolatedHeight(u, v);
                float steep = data.GetSteepness(u, v);
                float x = origin.x + u * data.size.x, z = origin.z + v * data.size.z;

                float snow = Mathf.SmoothStep(0f, 1f, (h - snowLine) / 15f) * (1f - Mathf.SmoothStep(0f, 1f, (steep - 35f) / 15f));
                float rock = Mathf.SmoothStep(0f, 1f, (steep - 22f) / 14f) * (1f - snow);
                float dirtNoise = Mathf.PerlinNoise(seed + x * 0.03f, z * 0.03f);
                float dirt = Mathf.SmoothStep(0f, 1f, (dirtNoise - 0.55f) / 0.15f) * (1f - rock - snow);
                float grass = Mathf.Max(0f, 1f - rock - snow - dirt);

                float sum = grass + dirt + rock + snow;
                maps[zi, xi, 0] = grass / sum;
                maps[zi, xi, 1] = dirt / sum;
                maps[zi, xi, 2] = rock / sum;
                maps[zi, xi, 3] = snow / sum;
            }
            data.SetAlphamaps(0, 0, maps);
        }

        static TerrainLayer Layer(string name, Texture2D texture, float tile)
        {
            string path = $"{GeneratedFolder}/Terrain/Layer_{name}.terrainlayer";
            var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
            if (layer == null)
            {
                layer = new TerrainLayer();
                AssetDatabase.CreateAsset(layer, path);
            }
            layer.diffuseTexture = texture;
            layer.tileSize = new Vector2(tile, tile);
            layer.smoothness = 0f;
            layer.metallic = 0f;
            EditorUtility.SetDirty(layer);
            return layer;
        }

        static Material TerrainMaterial()
        {
            string path = $"{MaterialFolder}/Terrain_Lit.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"));
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        /// <summary>Scatters pines on the lower slopes, below the snow line.</summary>
        public static void ScatterPines(Transform parent, Terrain terrain, int count, int seed)
        {
            var trunk = Mat("Pine_Trunk", new Color(0.3f, 0.22f, 0.15f));
            var needles = Mat("Pine_Needles", new Color(0.16f, 0.26f, 0.17f));
            var mesh = GreyboxMeshes.Pine();
            var group = Group("Pines", parent);
            var data = terrain.terrainData;
            var rng = new System.Random(seed);
            int placed = 0;
            for (int attempt = 0; attempt < count * 20 && placed < count; attempt++)
            {
                float u = (float)rng.NextDouble(), v = (float)rng.NextDouble();
                float h = data.GetInterpolatedHeight(u, v);
                if (h < 3f || h > data.size.y * 0.45f || data.GetSteepness(u, v) > 30f) continue;

                var pine = new GameObject("Pine");
                pine.transform.SetParent(group, false);
                pine.transform.position = terrain.transform.position + new Vector3(u * data.size.x, h - 0.2f, v * data.size.z);
                pine.transform.rotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
                pine.transform.localScale = Vector3.one * (0.8f + (float)rng.NextDouble() * 0.7f);
                pine.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = pine.AddComponent<MeshRenderer>();
                renderer.sharedMaterials = new[] { trunk, needles };
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                placed++;
            }
        }

        // ---------------------------------------------------------------- atmosphere

        /// <summary>Procedural dawn sky, linear fog and trilight ambient matched to the sun.</summary>
        public static void DawnAtmosphere(Light sun)
        {
            string skyPath = $"{MaterialFolder}/Sky_Dawn.mat";
            var sky = AssetDatabase.LoadAssetAtPath<Material>(skyPath);
            if (sky == null)
            {
                sky = new Material(Shader.Find("Skybox/Procedural"));
                AssetDatabase.CreateAsset(sky, skyPath);
            }
            sky.SetFloat("_SunSize", 0.035f);
            sky.SetFloat("_AtmosphereThickness", 1.25f);
            sky.SetColor("_SkyTint", new Color(0.55f, 0.6f, 0.72f));
            sky.SetColor("_GroundColor", new Color(0.35f, 0.33f, 0.31f));
            sky.SetFloat("_Exposure", 1.15f);
            EditorUtility.SetDirty(sky);

            RenderSettings.skybox = sky;
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.58f, 0.64f, 0.75f);
            RenderSettings.ambientEquatorColor = new Color(0.62f, 0.58f, 0.52f);
            RenderSettings.ambientGroundColor = new Color(0.26f, 0.24f, 0.21f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.7f, 0.72f, 0.76f);
            RenderSettings.fogStartDistance = 120f;
            RenderSettings.fogEndDistance = 950f;
        }

        /// <summary>
        /// Global post-processing tuned for mobile: neutral tonemapping, warm dawn grade, subtle
        /// bloom (for lamps and the sun) and a light vignette. No grain, blur or depth of field.
        /// </summary>
        public static Volume DawnPostProcessing(string profileName)
        {
            string path = $"{Root}/Settings/{profileName}.asset";
            EnsureFolder(Root, "Settings");
            AssetDatabase.DeleteAsset(path);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);

            var tonemapping = AddOverride<Tonemapping>(profile);
            tonemapping.mode.Override(TonemappingMode.Neutral);

            var grade = AddOverride<ColorAdjustments>(profile);
            grade.postExposure.Override(0.15f);
            grade.contrast.Override(12f);
            grade.saturation.Override(-6f);
            grade.colorFilter.Override(new Color(1f, 0.97f, 0.93f));

            var whiteBalance = AddOverride<WhiteBalance>(profile);
            whiteBalance.temperature.Override(8f);

            var bloom = AddOverride<Bloom>(profile);
            bloom.threshold.Override(1f);
            bloom.intensity.Override(0.3f);
            bloom.scatter.Override(0.6f);
            bloom.highQualityFiltering.Override(false);

            var vignette = AddOverride<Vignette>(profile);
            vignette.intensity.Override(0.22f);
            vignette.smoothness.Override(0.4f);

            EditorUtility.SetDirty(profile);

            var go = new GameObject("PostProcessing") { layer = 0 };
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;
            return volume;
        }

        static T AddOverride<T>(VolumeProfile profile) where T : VolumeComponent
        {
            var component = profile.Add<T>();
            component.name = typeof(T).Name;
            component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(component, profile); // components must be sub-assets to persist
            return component;
        }

        // ---------------------------------------------------------------- batching

        /// <summary>
        /// Marks everything under root static (batching, occlusion, navigation), except things
        /// that move: doors, targets, characters.
        /// </summary>
        public static void MarkStatic(Transform root)
        {
            if (root.GetComponent<Door>() != null || root.GetComponent<Target>() != null
                || root.GetComponent<SquadMember>() != null || root.GetComponent<NavMeshAgent>() != null)
                return;
            GameObjectUtility.SetStaticEditorFlags(root.gameObject,
                StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic
                | StaticEditorFlags.ReflectionProbeStatic);
            foreach (Transform child in root) MarkStatic(child);
        }
    }
}
