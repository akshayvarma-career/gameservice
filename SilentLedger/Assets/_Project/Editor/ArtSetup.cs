using SilentLedger.World;
using UnityEditor;
using UnityEngine;

namespace SilentLedger.EditorTools
{
    /// <summary>
    /// Turns imported art into game-ready prefabs: mobile texture settings, a URP material,
    /// simple colliders and moving parts. Re-run after replacing a model or texture.
    /// </summary>
    public static class ArtSetup
    {
        const string HelicopterFolder = GreyboxKit.Root + "/Art/Vehicles/Helicopter";
        public const string HelicopterPrefab = GreyboxKit.Root + "/Prefabs/Helicopter.prefab";

        [MenuItem("Silent Ledger/Art/Set Up Helicopter")]
        public static string SetupHelicopter()
        {
            string model = $"{HelicopterFolder}/Helicopter_SiteHollow.fbx";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(model) == null) return $"Missing {model}";

            ConfigureModel(model);
            var baseColor = ConfigureTexture($"{HelicopterFolder}/T_Helicopter_BaseColor.png", TextureKind.Color, 2048);
            var normal = ConfigureTexture($"{HelicopterFolder}/T_Helicopter_Normal.png", TextureKind.Normal, 1024);
            var metallic = ConfigureTexture($"{HelicopterFolder}/T_Helicopter_MetallicSmoothness.png", TextureKind.Data, 1024);
            var occlusion = ConfigureTexture($"{HelicopterFolder}/T_Helicopter_AO.png", TextureKind.Data, 1024);
            var material = LitMaterial($"{HelicopterFolder}/M_Helicopter.mat", baseColor, normal, metallic, occlusion);

            // Prefab: model instance with the material, two box colliders and spinning rotors.
            var root = new GameObject("Helicopter");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(model), root.transform);
            foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>(true))
                renderer.sharedMaterial = material;

            // The FBX is Z-up (root rotated -90 on X): mesh-local -Y is the nose, +Z is up.
            var body = instance.transform;
            AddBox(body, new Vector3(0f, -3.1f, 2.0f), new Vector3(2.8f, 9.3f, 2.8f));   // cabin
            AddBox(body, new Vector3(0f, 4.6f, 2.6f), new Vector3(0.9f, 6.4f, 1.1f));    // tail boom

            Spin(body.Find("Helicopter_MainRotor"), Vector3.forward, 300f);
            Spin(body.Find("Helicopter_TailRotor"), Vector3.right, 900f);

            PrefabUtility.SaveAsPrefabAsset(root, HelicopterPrefab);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            return $"Built {HelicopterPrefab}";
        }

        // ---------------------------------------------------------------- first-person weapons

        /// <summary>Asset Store guns (Low Poly Guns Collection) live here; the folder is git-ignored.</summary>
        public const string GunsFolder = GreyboxKit.Root + "/ThirdParty/LowPolyGuns";
        public const string RiflePrefab = GunsFolder + "/Weapon_Rifle.prefab";
        public const string PistolPrefab = GunsFolder + "/Weapon_Pistol.prefab";

        [MenuItem("Silent Ledger/Art/Set Up Weapons")]
        public static string SetupWeapons()
        {
            // M4-style carbine and polymer pistol, scaled to real length.
            string rifle = BuildWeaponPrefab("assault4", RiflePrefab, length: 0.84f, rearZ: -0.3f);
            string pistol = BuildWeaponPrefab("pistol3", PistolPrefab, length: 0.19f, rearZ: -0.06f);
            AssetDatabase.SaveAssets();
            return $"{rifle}; {pistol}";
        }

        /// <summary>
        /// A view-model prefab whose origin is on the line of sight: the top of the gun sits at
        /// y = 0, centred in x, the muzzle pointing +z, and the rear of the gun at rearZ.
        /// </summary>
        static string BuildWeaponPrefab(string name, string prefabPath, float length, float rearZ)
        {
            string folder = $"{GunsFolder}/{name}";
            string model = $"{folder}/{name}.fbx";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(model) == null) return $"Missing {model}";

            var importer = (ModelImporter)AssetImporter.GetAtPath(model);
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importAnimation = false;
            importer.isReadable = false;
            importer.SaveAndReimport();

            var baseColor = ConfigureTexture($"{folder}/{name}_diffuse.png", TextureKind.Color, 1024);
            var normal = ConfigureTexture($"{folder}/{name}_normal.png", TextureKind.Normal, 1024);
            var material = AssetDatabase.LoadAssetAtPath<Material>($"{folder}/M_{name}.mat");
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, $"{folder}/M_{name}.mat");
            }
            material.SetTexture("_BaseMap", baseColor);
            material.SetTexture("_BumpMap", normal);
            material.EnableKeyword("_NORMALMAP");
            material.SetFloat("_Metallic", 0.4f);
            material.SetFloat("_Smoothness", 0.45f);
            EditorUtility.SetDirty(material);

            var root = new GameObject(System.IO.Path.GetFileNameWithoutExtension(prefabPath));
            var body = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(model), root.transform);
            foreach (var renderer in body.GetComponentsInChildren<MeshRenderer>(true))
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            // Scale to real length, then move so the sight line is the origin.
            var bounds = WorldBounds(body);
            body.transform.localScale *= length / bounds.size.z;
            bounds = WorldBounds(body);
            body.transform.localPosition -= new Vector3(bounds.center.x, bounds.max.y, bounds.min.z - rearZ);
            GreyboxKit.SetLayerRecursively(root, GreyboxKit.IgnoreRaycastLayer);

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            return $"Built {prefabPath}";
        }

        static Bounds WorldBounds(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            return bounds;
        }

        static void AddBox(Transform parent, Vector3 center, Vector3 size)
        {
            var box = parent.gameObject.AddComponent<BoxCollider>();
            box.center = center;
            box.size = size;
        }

        static void Spin(Transform rotor, Vector3 axis, float rpm)
        {
            if (rotor == null) return;
            var spin = rotor.gameObject.AddComponent<RotorSpin>();
            var so = new SerializedObject(spin);
            so.FindProperty("localAxis").vector3Value = axis;
            so.FindProperty("startRpm").floatValue = rpm;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------------------------------------------------------------- importers

        static void ConfigureModel(string path)
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            importer.materialImportMode = ModelImporterMaterialImportMode.None; // we assign our own URP material
            importer.importAnimation = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.isReadable = false;
            importer.meshCompression = ModelImporterMeshCompression.Medium;
            importer.SaveAndReimport();
        }

        enum TextureKind { Color, Normal, Data }

        static Texture2D ConfigureTexture(string path, TextureKind kind, int androidMaxSize)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = kind == TextureKind.Normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = kind == TextureKind.Color;
            importer.mipmapEnabled = true;
            importer.anisoLevel = 2;
            var android = importer.GetPlatformTextureSettings("Android");
            android.overridden = true;
            android.maxTextureSize = androidMaxSize;
            android.format = TextureImporterFormat.ASTC_6x6;
            importer.SetPlatformTextureSettings(android);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Material LitMaterial(string path, Texture2D baseColor, Texture2D normal, Texture2D metallicSmoothness, Texture2D occlusion)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetTexture("_BaseMap", baseColor);
            material.SetColor("_BaseColor", Color.white);
            material.SetTexture("_BumpMap", normal);
            material.EnableKeyword("_NORMALMAP");
            material.SetTexture("_MetallicGlossMap", metallicSmoothness);
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.SetFloat("_SmoothnessTextureChannel", 0f); // smoothness in the metallic map's alpha
            material.SetFloat("_Smoothness", 1f);
            material.SetTexture("_OcclusionMap", occlusion);
            material.EnableKeyword("_OCCLUSIONMAP");
            material.SetFloat("_OcclusionStrength", 1f);
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
