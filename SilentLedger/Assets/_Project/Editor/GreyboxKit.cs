using System;
using SilentLedger.Interaction;
using SilentLedger.Weapons;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SilentLedger.EditorTools
{
    /// <summary>Shared helpers for building grey-box levels from code.</summary>
    public static class GreyboxKit
    {
        public const string Root = "Assets/_Project";
        public const string MaterialFolder = Root + "/Materials";
        public const int IgnoreRaycastLayer = 2;
        public const int UILayer = 5;
        public const int SquadLayer = 8;

        public static Material Ground, Tarmac, Wall, Dark, Metal, Accent, Enemy, Civilian, Glass;

        /// <summary>Creates folders, the Squad layer and the shared materials. Call once per build.</summary>
        public static void Init()
        {
            EnsureFolder(Root, "Scenes");
            EnsureFolder(Root, "Prefabs");
            EnsureFolder(Root, "Materials");
            EnsureFolder(Root, "Navigation");
            EnsureLayer(SquadLayer, "Squad");

            GreyboxMeshes.ClearCache();
            Ground = Mat("Grey_Ground", new Color(0.4f, 0.4f, 0.38f), ProceduralTextures.Concrete());
            Tarmac = Mat("Grey_Tarmac", new Color(0.4f, 0.4f, 0.41f), ProceduralTextures.Tarmac());
            Wall = Mat("Grey_Wall", new Color(0.66f, 0.65f, 0.62f), ProceduralTextures.Concrete());
            Dark = Mat("Grey_Dark", new Color(0.15f, 0.15f, 0.16f));
            Metal = Mat("Grey_Metal", new Color(0.38f, 0.41f, 0.44f), ProceduralTextures.MetalPanel(), smoothness: 0.35f);
            Accent = Mat("Accent_Orange", new Color(0.95f, 0.55f, 0.15f));
            Enemy = Mat("Target_Hostile", new Color(0.8f, 0.2f, 0.18f));
            Civilian = Mat("Target_Civilian", new Color(0.2f, 0.45f, 0.85f));
            Glass = Mat("Screen_Teal", new Color(0.2f, 0.75f, 0.75f));
        }

        /// <summary>New scene with only a directional light.</summary>
        public static Scene NewScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            foreach (var go in scene.GetRootGameObjects())
                if (go.GetComponent<Camera>() != null) Object.DestroyImmediate(go);
            return scene;
        }

        public static Light Sun() => Object.FindAnyObjectByType<Light>();

        public static Transform Group(string name, Transform parent, Vector3 position = default)
        {
            var group = new GameObject(name).transform;
            group.SetParent(parent, false);
            group.localPosition = position;
            return group;
        }

        /// <summary>
        /// A box of the given size in metres. Uses a world-UV mesh (unscaled transform) so
        /// textures tile at real-world density.
        /// </summary>
        public static GameObject Box(string name, Transform parent, Vector3 position, Vector3 size, Material material,
            bool collider = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.AddComponent<MeshFilter>().sharedMesh = GreyboxMeshes.Box(size);
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            if (collider) go.AddComponent<BoxCollider>().size = size;
            return go;
        }

        // ---------------------------------------------------------------- walls and rooms

        public static void WallAlongZ(Transform parent, float x, float z0, float z1, params (float from, float to)[] gaps) =>
            LayWall(z0, z1, gaps, (mid, len, y, h) =>
                Box("Wall", parent, new Vector3(x, y, mid), new Vector3(0.2f, h, len), Wall));

        public static void WallAlongX(Transform parent, float z, float x0, float x1, params (float from, float to)[] gaps) =>
            LayWall(x0, x1, gaps, (mid, len, y, h) =>
                Box("Wall", parent, new Vector3(mid, y, z), new Vector3(len, h, 0.2f), Wall));

        /// <summary>Lays wall segments from start to end, leaving door gaps with a lintel above.</summary>
        static void LayWall(float start, float end, (float from, float to)[] gaps, Action<float, float, float, float> place)
        {
            const float height = 3f, doorHeight = 2.3f;
            float cursor = start;
            foreach (var gap in gaps)
            {
                if (gap.from > cursor) place((cursor + gap.from) / 2f, gap.from - cursor, height / 2f, height);
                place((gap.from + gap.to) / 2f, gap.to - gap.from, (doorHeight + height) / 2f, height - doorHeight);
                cursor = gap.to;
            }
            if (end > cursor) place((cursor + end) / 2f, end - cursor, height / 2f, height);
        }

        /// <summary>
        /// A rectangular room from (x0, z0) to (x1, z1) in the parent's space, with a doorway centred on
        /// one side ('N' +z, 'S' -z, 'E' +x, 'W' -x) and an optional roof.
        /// </summary>
        public static Transform Room(string name, Transform parent, float x0, float z0, float x1, float z1,
            char doorSide, float doorWidth = 1.6f, bool roof = true)
        {
            var room = Group(name, parent);
            float cx = (x0 + x1) / 2f, cz = (z0 + z1) / 2f, half = doorWidth / 2f;
            var noGap = Array.Empty<(float, float)>();
            WallAlongX(room, z0, x0, x1, doorSide == 'S' ? new[] { (cx - half, cx + half) } : noGap);
            WallAlongX(room, z1, x0, x1, doorSide == 'N' ? new[] { (cx - half, cx + half) } : noGap);
            WallAlongZ(room, x0, z0, z1, doorSide == 'W' ? new[] { (cz - half, cz + half) } : noGap);
            WallAlongZ(room, x1, z0, z1, doorSide == 'E' ? new[] { (cz - half, cz + half) } : noGap);
            if (roof)
                Box("Roof", room, new Vector3(cx, 3.1f, cz), new Vector3(x1 - x0 + 0.4f, 0.2f, z1 - z0 + 0.4f), Metal);
            return room;
        }

        public static Transform SpawnDoor(Transform parent, Vector3 hinge, float yaw, float width = 1.4f)
        {
            var pivot = Group("Door", parent, hinge);
            pivot.localRotation = Quaternion.Euler(0f, yaw, 0f);
            Box("Panel", pivot, new Vector3(width / 2f - 0.01f, 1.1f, 0f), new Vector3(width - 0.04f, 2.2f, 0.08f), Metal);
            pivot.gameObject.AddComponent<Door>();
            return pivot;
        }

        public static Target SpawnTarget(string name, Transform parent, Vector3 position, float yaw, float scale, bool isCivilian)
        {
            var root = Group(name, parent, position);
            root.localRotation = Quaternion.Euler(0f, yaw, 0f);

            Box("Stand", root, new Vector3(0f, 0.05f, 0f), new Vector3(0.8f * scale, 0.1f, 0.4f * scale), Dark);
            var pivot = Group("Pivot", root, new Vector3(0f, 0.1f, 0f));
            var material = isCivilian ? Civilian : Enemy;
            Box("Body", pivot, new Vector3(0f, 0.6f * scale, 0f), new Vector3(0.55f * scale, 1.2f * scale, 0.06f), material);
            Box("Head", pivot, new Vector3(0f, 1.38f * scale, 0f), new Vector3(0.3f * scale, 0.32f * scale, 0.06f), material);

            var target = root.gameObject.AddComponent<Target>();
            Set(target, "pivot", pivot);
            Set(target, "civilian", isCivilian);
            return target;
        }

        public static InfoPoint Info(GameObject host, string prompt, string message)
        {
            var info = host.AddComponent<InfoPoint>();
            Set(info, "prompt", prompt);
            Set(info, "message", message);
            return info;
        }

        // ---------------------------------------------------------------- assets and serialization

        public static Material Mat(string name, Color color, Texture2D baseMap = null, float smoothness = 0.15f,
            Color? emission = null)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            material.SetTexture("_BaseMap", baseMap);
            material.SetFloat("_Smoothness", smoothness);
            material.enableInstancing = true;
            if (emission.HasValue)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        public static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent))
            {
                int slash = parent.LastIndexOf('/');
                EnsureFolder(parent.Substring(0, slash), parent.Substring(slash + 1));
            }
            if (!AssetDatabase.IsValidFolder($"{parent}/{child}"))
                AssetDatabase.CreateFolder(parent, child);
        }

        static void EnsureLayer(int index, string name)
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layer = tagManager.FindProperty("layers").GetArrayElementAtIndex(index);
            if (layer.stringValue == name) return;
            if (!string.IsNullOrEmpty(layer.stringValue))
                throw new InvalidOperationException($"Layer {index} is already '{layer.stringValue}'");
            layer.stringValue = name;
            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform) SetLayerRecursively(child.gameObject, layer);
        }

        public static void Set(Object target, string field, object value)
        {
            var so = new SerializedObject(target);
            var property = so.FindProperty(field)
                           ?? throw new ArgumentException($"{target.GetType().Name} has no serialized field '{field}'");
            switch (value)
            {
                case Object o: property.objectReferenceValue = o; break;
                case bool b: property.boolValue = b; break;
                case float f: property.floatValue = f; break;
                case int i: property.intValue = i; break;
                case string s: property.stringValue = s; break;
                case Color c: property.colorValue = c; break;
                default: throw new ArgumentException($"Unsupported value type {value?.GetType()}");
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetList(Object target, string field, params Object[] values)
        {
            var so = new SerializedObject(target);
            var property = so.FindProperty(field)
                           ?? throw new ArgumentException($"{target.GetType().Name} has no serialized field '{field}'");
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
