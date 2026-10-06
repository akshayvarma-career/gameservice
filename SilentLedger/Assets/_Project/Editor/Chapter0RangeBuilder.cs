using System;
using SilentLedger.Interaction;
using SilentLedger.Player;
using SilentLedger.UI;
using SilentLedger.Weapons;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SilentLedger.EditorTools
{
    /// <summary>
    /// Builds the grey-box Chapter 0 test range (sniper range, armory crate, kill house) and the
    /// PlayerRig prefab (player, weapons, touch HUD). Re-run it to regenerate both from code.
    /// </summary>
    public static class Chapter0RangeBuilder
    {
        const string Root = "Assets/_Project";
        const string ScenePath = Root + "/Scenes/Ch0_TestRange.unity";
        const string PrefabPath = Root + "/Prefabs/PlayerRig.prefab";
        const string MaterialFolder = Root + "/Materials";
        const int IgnoreRaycastLayer = 2;
        const int UILayer = 5;

        static Material ground, wall, accent, dark, enemy, civilian, metal;
        static Font font;
        static Sprite circle;

        [MenuItem("Silent Ledger/Build Chapter 0 Test Range")]
        public static string Build()
        {
            EnsureFolder(Root, "Scenes");
            EnsureFolder(Root, "Prefabs");
            EnsureFolder(Root, "Materials");
            CreateMaterials();
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            foreach (var go in scene.GetRootGameObjects())
                if (go.GetComponent<Camera>() != null) Object.DestroyImmediate(go);

            var level = new GameObject("Level").transform;
            BuildGroundAndBounds(level);
            BuildRange(level);
            BuildArmory(level);
            BuildKillHouse(level);

            var rig = BuildPlayerRig(new Vector3(0f, 0f, -10f));
            PrefabUtility.SaveAsPrefabAssetAndConnect(rig, PrefabPath, InteractionMode.AutomatedAction);

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            return $"Built {ScenePath} and {PrefabPath}";
        }

        // ---------------------------------------------------------------- level

        static void BuildGroundAndBounds(Transform level)
        {
            // Base area around the origin; the range runs north (+z) to 430 m.
            Box("Ground", level, new Vector3(0f, -0.5f, 200f), new Vector3(80f, 1f, 480f), ground);
            Box("Bound_W", level, new Vector3(-40f, 0.6f, 200f), new Vector3(0.5f, 1.2f, 480f), wall);
            Box("Bound_E", level, new Vector3(40f, 0.6f, 200f), new Vector3(0.5f, 1.2f, 480f), wall);
            Box("Bound_S", level, new Vector3(0f, 0.6f, -40f), new Vector3(80f, 1.2f, 0.5f), wall);
            Box("Bound_N", level, new Vector3(0f, 2f, 440f), new Vector3(80f, 4f, 0.5f), wall);
        }

        static void BuildRange(Transform level)
        {
            var range = new GameObject("SniperRange").transform;
            range.SetParent(level, false);

            var bench = Box("FiringBench", range, new Vector3(-12f, 0.5f, 2f), new Vector3(10f, 1f, 0.6f), wall);
            var info = bench.AddComponent<InfoPoint>();
            Set(info, "prompt", "Range briefing");
            Set(info, "message",
                "Sniper range. Aim down sights and hit the targets at 100, 200 and 400 m. Warm up on the close ones first.");

            float[] lanes = { -16f, -12f, -8f };
            float[] distances = { 100f, 200f, 400f };
            float[] scales = { 1.2f, 1.8f, 3f };
            for (int i = 0; i < lanes.Length; i++)
            {
                SpawnTarget($"Warmup_{i + 1}", range, new Vector3(lanes[i], 0f, 20f), 0f, 1f, false);
                SpawnTarget($"Lane{i + 1}_{distances[i]}m", range, new Vector3(lanes[i], 0f, 2f + distances[i]),
                    0f, scales[i], false);
            }
            foreach (float x in new[] { -18f, -14f, -10f, -6f })
                Box("LaneDivider", range, new Vector3(x, 0.3f, 15f), new Vector3(0.15f, 0.6f, 24f), dark);
        }

        static void BuildArmory(Transform level)
        {
            var armory = new GameObject("Armory").transform;
            armory.SetParent(level, false);
            var crate = Box("AmmoCrate", armory, new Vector3(-4f, 0.45f, -6f), new Vector3(1.2f, 0.9f, 0.8f), accent);
            crate.AddComponent<AmmoCrate>();
            Box("WeaponRack", armory, new Vector3(-6f, 1f, -6.5f), new Vector3(2f, 2f, 0.3f), metal);
        }

        static void BuildKillHouse(Transform level)
        {
            var house = new GameObject("KillHouse").transform;
            house.SetParent(level, false);

            // Shell: x 14..26, z -6..8, entrance gap on the west wall.
            WallAlongZ(house, 14f, -6f, 8f, (0f, 1.4f));
            WallAlongZ(house, 26f, -6f, 8f);
            WallAlongX(house, -6f, 14f, 26f);
            WallAlongX(house, 8f, 14f, 26f);
            // Interior: west room | east rooms split north/south, each behind a door.
            WallAlongZ(house, 20f, -6f, 8f, (-2.6f, -1.2f));
            WallAlongX(house, 1f, 20f, 26f, (22.4f, 23.8f));
            SpawnDoor(house, new Vector3(20f, 0f, -2.6f), -90f);
            SpawnDoor(house, new Vector3(22.4f, 0f, 1f), 0f);

            var sign = Box("KillHouseSign", house, new Vector3(12.5f, 1f, -1f), new Vector3(0.2f, 2f, 1.2f), accent);
            var info = sign.AddComponent<InfoPoint>();
            Set(info, "prompt", "Drill briefing");
            Set(info, "message",
                "Kill house: clear every room. Red targets are hostile. Blue targets are civilians, so don't shoot them.");

            SpawnTarget("Hostile_W1", house, new Vector3(17.5f, 0f, 5f), 90f, 1f, false);
            SpawnTarget("Civilian_W", house, new Vector3(16f, 0f, -4f), 90f, 1f, true);
            SpawnTarget("Hostile_SE1", house, new Vector3(24.5f, 0f, -4f), 90f, 1f, false);
            SpawnTarget("Hostile_SE2", house, new Vector3(23f, 0f, -1.5f), 90f, 1f, false);
            SpawnTarget("Hostile_NE1", house, new Vector3(24.5f, 0f, 6.5f), 0f, 1f, false);
            SpawnTarget("Civilian_NE", house, new Vector3(21.5f, 0f, 6.5f), 0f, 1f, true);
        }

        static void WallAlongZ(Transform parent, float x, float z0, float z1, params (float from, float to)[] gaps) =>
            Wall(parent, z0, z1, gaps, (mid, len, y, h) =>
                Box("Wall", parent, new Vector3(x, y, mid), new Vector3(0.2f, h, len), wall));

        static void WallAlongX(Transform parent, float z, float x0, float x1, params (float from, float to)[] gaps) =>
            Wall(parent, x0, x1, gaps, (mid, len, y, h) =>
                Box("Wall", parent, new Vector3(mid, y, z), new Vector3(len, h, 0.2f), wall));

        /// <summary>Lays wall segments from start to end, leaving door gaps with a lintel above.</summary>
        static void Wall(Transform parent, float start, float end, (float from, float to)[] gaps,
            Action<float, float, float, float> place)
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

        static void SpawnDoor(Transform parent, Vector3 hinge, float yaw)
        {
            var pivot = new GameObject("Door").transform;
            pivot.SetParent(parent, false);
            pivot.localPosition = hinge;
            pivot.localRotation = Quaternion.Euler(0f, yaw, 0f);
            Box("Panel", pivot, new Vector3(0.69f, 1.1f, 0f), new Vector3(1.36f, 2.2f, 0.08f), metal);
            pivot.gameObject.AddComponent<Door>();
        }

        static void SpawnTarget(string name, Transform parent, Vector3 position, float yaw, float scale, bool isCivilian)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.localPosition = position;
            root.localRotation = Quaternion.Euler(0f, yaw, 0f);

            Box("Stand", root, new Vector3(0f, 0.05f, 0f), new Vector3(0.8f * scale, 0.1f, 0.4f * scale), dark);
            var pivot = new GameObject("Pivot").transform;
            pivot.SetParent(root, false);
            pivot.localPosition = new Vector3(0f, 0.1f, 0f);
            var mat = isCivilian ? civilian : enemy;
            Box("Body", pivot, new Vector3(0f, 0.6f * scale, 0f), new Vector3(0.55f * scale, 1.2f * scale, 0.06f), mat);
            Box("Head", pivot, new Vector3(0f, 1.38f * scale, 0f), new Vector3(0.3f * scale, 0.32f * scale, 0.06f), mat);

            var target = root.gameObject.AddComponent<Target>();
            Set(target, "pivot", pivot);
            Set(target, "civilian", isCivilian);
        }

        // ---------------------------------------------------------------- player

        static GameObject BuildPlayerRig(Vector3 spawn)
        {
            var rig = new GameObject("PlayerRig");

            var player = new GameObject("Player");
            player.layer = IgnoreRaycastLayer;
            player.transform.SetParent(rig.transform, false);
            player.transform.position = spawn;

            var body = player.AddComponent<CharacterController>();
            body.height = 1.8f;
            body.radius = 0.35f;
            body.center = new Vector3(0f, 0.9f, 0f);
            body.stepOffset = 0.3f;

            var intent = player.AddComponent<PlayerIntent>();
            var controller = player.AddComponent<PlayerController>();
            var look = player.AddComponent<PlayerLook>();
            var keyboard = player.AddComponent<KeyboardMouseInput>();

            var cameraRoot = new GameObject("CameraRoot").transform;
            cameraRoot.SetParent(player.transform, false);
            cameraRoot.localPosition = new Vector3(0f, 1.65f, 0f);

            var cameraGo = new GameObject("Main Camera") { tag = "MainCamera", layer = IgnoreRaycastLayer };
            cameraGo.transform.SetParent(cameraRoot, false);
            var cam = cameraGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.03f;
            cam.farClipPlane = 800f;
            cam.fieldOfView = 70f;
            cameraGo.AddComponent<AudioListener>();

            var holder = cameraGo.AddComponent<WeaponHolder>();
            var interactor = cameraGo.AddComponent<Interactor>();
            var rifle = BuildRifle(cameraGo.transform);
            var pistol = BuildPistol(cameraGo.transform);

            Set(controller, "intent", intent);
            Set(controller, "cameraRoot", cameraRoot);
            Set(look, "intent", intent);
            Set(look, "viewCamera", cam);
            Set(keyboard, "intent", intent);
            Set(holder, "intent", intent);
            Set(holder, "look", look);
            Set(holder, "controller", controller);
            Set(holder, "viewCamera", cam);
            SetList(holder, "weapons", rifle, pistol);
            Set(interactor, "intent", intent);

            BuildHud(rig.transform, intent, holder, interactor);
            return rig;
        }

        static Weapon BuildRifle(Transform cam)
        {
            var weapon = ViewModelRoot("Rifle", cam);
            ViewModelPart("Body", weapon.transform, new Vector3(0f, 0f, 0.1f), new Vector3(0.06f, 0.09f, 0.55f), dark);
            ViewModelPart("Magazine", weapon.transform, new Vector3(0f, -0.1f, 0.06f), new Vector3(0.045f, 0.14f, 0.07f), metal);
            ViewModelPart("Stock", weapon.transform, new Vector3(0f, -0.02f, -0.24f), new Vector3(0.05f, 0.08f, 0.18f), dark);
            ViewModelPart("Sight", weapon.transform, new Vector3(0f, 0.06f, 0f), new Vector3(0.025f, 0.03f, 0.08f), metal);
            weapon.stats = new WeaponStats
            {
                displayName = "Rifle", damage = 34f, roundsPerMinute = 600f, automatic = true,
                magazineSize = 30, reserveAmmo = 120, reloadSeconds = 2.2f, range = 500f,
                hipSpread = 2.5f, aimSpread = 0.3f, recoilPitch = 0.9f, recoilYaw = 0.25f, aimFov = 45f,
            };
            // Aim pose keeps the top of the sight just below the line of sight.
            weapon.hipPosition = new Vector3(0.2f, -0.22f, 0.45f);
            weapon.aimPosition = new Vector3(0f, -0.09f, 0.38f);
            weapon.transform.localPosition = weapon.hipPosition;
            return weapon;
        }

        static Weapon BuildPistol(Transform cam)
        {
            var weapon = ViewModelRoot("Pistol", cam);
            ViewModelPart("Slide", weapon.transform, new Vector3(0f, 0f, 0.04f), new Vector3(0.04f, 0.05f, 0.18f), dark);
            ViewModelPart("Grip", weapon.transform, new Vector3(0f, -0.07f, -0.02f), new Vector3(0.035f, 0.1f, 0.05f), metal);
            ViewModelPart("Sight", weapon.transform, new Vector3(0f, 0.03f, 0.1f), new Vector3(0.01f, 0.012f, 0.012f), metal);
            weapon.stats = new WeaponStats
            {
                displayName = "Pistol", damage = 40f, roundsPerMinute = 300f, automatic = false,
                magazineSize = 12, reserveAmmo = 48, reloadSeconds = 1.5f, range = 100f,
                hipSpread = 2f, aimSpread = 0.5f, recoilPitch = 1.6f, recoilYaw = 0.2f, aimFov = 55f,
            };
            weapon.hipPosition = new Vector3(0.15f, -0.18f, 0.32f);
            weapon.aimPosition = new Vector3(0f, -0.045f, 0.3f);
            weapon.transform.localPosition = weapon.hipPosition;
            weapon.gameObject.SetActive(false);
            return weapon;
        }

        static Weapon ViewModelRoot(string name, Transform cam)
        {
            var go = new GameObject(name) { layer = IgnoreRaycastLayer };
            go.transform.SetParent(cam, false);
            return go.AddComponent<Weapon>();
        }

        static void ViewModelPart(string name, Transform parent, Vector3 position, Vector3 size, Material material)
        {
            var part = Box(name, parent, position, size, material, collider: false);
            part.layer = IgnoreRaycastLayer;
            part.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        }

        // ---------------------------------------------------------------- HUD

        static void BuildHud(Transform parent, PlayerIntent intent, WeaponHolder holder, Interactor interactor)
        {
            var canvasGo = new GameObject("TouchHUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))
                { layer = UILayer };
            canvasGo.transform.SetParent(parent, false);
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 1f;
            var canvas = canvasGo.transform;

            // Touch zones: left 42% moves, the rest looks. Buttons sit on top and take their own touches.
            var movePad = Rect("MovePad", canvas, Vector2.zero, new Vector2(0.42f, 1f), Vector2.zero, Vector2.zero);
            Img(movePad, null, Color.clear);
            var stickBase = Rect("StickBase", movePad, Vector2.zero, Vector2.zero, new Vector2(280f, 260f), new Vector2(240f, 240f));
            var baseImage = Img(stickBase, circle, new Color(1f, 1f, 1f, 0.25f), raycast: false);
            var knob = Rect("Knob", stickBase, Half, Half, Vector2.zero, new Vector2(110f, 110f));
            var knobImage = Img(knob, circle, new Color(1f, 1f, 1f, 0.6f), raycast: false);
            var joystick = movePad.gameObject.AddComponent<FloatingJoystick>();
            Set(joystick, "intent", intent);
            Set(joystick, "stickBase", stickBase);
            Set(joystick, "knob", knob);
            Set(joystick, "baseGraphic", baseImage);
            Set(joystick, "knobGraphic", knobImage);

            var lookPad = Rect("LookPad", canvas, new Vector2(0.42f, 0f), Vector2.one, Vector2.zero, Vector2.zero);
            Img(lookPad, null, Color.clear);
            Set(lookPad.gameObject.AddComponent<LookPad>(), "intent", intent);

            // Crosshair and hit marker.
            var crosshair = Rect("Crosshair", canvas, Half, Half, Vector2.zero, Vector2.zero);
            Img(Rect("Dot", crosshair, Half, Half, Vector2.zero, new Vector2(4f, 4f)), null, Color.white, raycast: false);
            foreach (var dir in new[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right })
            {
                var size = dir.x != 0f ? new Vector2(14f, 3f) : new Vector2(3f, 14f);
                Img(Rect("Bar", crosshair, Half, Half, dir * 18f, size), null, new Color(1f, 1f, 1f, 0.85f), raycast: false);
            }
            var hitMarker = Rect("HitMarker", canvas, Half, Half, Vector2.zero, new Vector2(40f, 40f));
            var hitGroup = hitMarker.gameObject.AddComponent<CanvasGroup>();
            hitGroup.blocksRaycasts = false;
            var hitGraphics = new Graphic[4];
            for (int i = 0; i < 4; i++)
            {
                float angle = 45f + 90f * i;
                var dir = Quaternion.Euler(0f, 0f, angle) * Vector3.up;
                var bar = Rect("Tick", hitMarker, Half, Half, (Vector2)dir * 22f, new Vector2(3f, 12f));
                bar.localRotation = Quaternion.Euler(0f, 0f, angle);
                hitGraphics[i] = Img(bar, null, Color.white, raycast: false);
            }

            // Readouts.
            var ammo = Label("Ammo", canvas, "30 / 120", 44, TextAnchor.UpperRight, Vector2.one, new Vector2(-40f, -30f), new Vector2(500f, 60f));
            var weaponName = Label("WeaponName", canvas, "Rifle", 28, TextAnchor.UpperRight, Vector2.one, new Vector2(-40f, -90f), new Vector2(500f, 40f));
            var prompt = Label("Prompt", canvas, "", 32, TextAnchor.MiddleCenter, Half, new Vector2(0f, -110f), new Vector2(800f, 50f));
            var message = Label("Message", canvas, "", 30, TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1200f, 120f));

            // Buttons, positions from the bottom-right corner unless noted.
            var bottomRight = new Vector2(1f, 0f);
            var fire = Button("Fire", canvas, "FIRE", bottomRight, new Vector2(-250f, 250f), 220f, intent);
            var leftFire = Button("LeftFire", canvas, "FIRE", Vector2.zero, new Vector2(170f, 640f), 130f, null);
            var aim = Button("Aim", canvas, "AIM", bottomRight, new Vector2(-510f, 290f), 150f, null);
            var reload = Button("Reload", canvas, "RELOAD", bottomRight, new Vector2(-260f, 500f), 120f, null);
            var swap = Button("Swap", canvas, "SWAP", bottomRight, new Vector2(-440f, 520f), 110f, null);
            var crouch = Button("Crouch", canvas, "CROUCH", bottomRight, new Vector2(-95f, 95f), 125f, null);
            var use = Button("Use", canvas, "USE", bottomRight, new Vector2(-110f, 720f), 140f, null);
            use.gameObject.SetActive(false);

            var hud = canvasGo.AddComponent<TouchHud>();
            Set(hud, "intent", intent);
            Set(hud, "weapons", holder);
            Set(hud, "interactor", interactor);
            Set(hud, "fireButton", fire);
            Set(hud, "leftFireButton", leftFire);
            Set(hud, "aimButton", aim);
            Set(hud, "reloadButton", reload);
            Set(hud, "swapButton", swap);
            Set(hud, "crouchButton", crouch);
            Set(hud, "interactButton", use);
            Set(hud, "ammoText", ammo);
            Set(hud, "weaponText", weaponName);
            Set(hud, "promptText", prompt);
            Set(hud, "messageText", message);
            Set(hud, "hitMarker", hitGroup);
            SetList(hud, "hitMarkerGraphics", hitGraphics);
        }

        static TouchButton Button(string name, Transform canvas, string label, Vector2 anchor, Vector2 position,
            float diameter, PlayerIntent lookIntent)
        {
            var rect = Rect(name, canvas, anchor, anchor, position, new Vector2(diameter, diameter));
            var image = Img(rect, circle, new Color(1f, 1f, 1f, 0.22f));
            Label("Label", rect, label, Mathf.RoundToInt(diameter * 0.17f), TextAnchor.MiddleCenter, Half, Vector2.zero,
                new Vector2(diameter, diameter));
            var button = rect.gameObject.AddComponent<TouchButton>();
            Set(button, "graphic", image);
            if (lookIntent != null) Set(button, "lookIntent", lookIntent);
            return button;
        }

        static readonly Vector2 Half = new Vector2(0.5f, 0.5f);

        static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = UILayer };
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        static Image Img(RectTransform rect, Sprite sprite, Color color, bool raycast = true)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        static Text Label(string name, Transform parent, string text, int size, TextAnchor alignment, Vector2 anchor,
            Vector2 position, Vector2 box)
        {
            var rect = Rect(name, parent, anchor, anchor, position, box);
            rect.pivot = new Vector2(alignment.ToString().Contains("Right") ? 1f : alignment.ToString().Contains("Left") ? 0f : 0.5f,
                alignment.ToString().StartsWith("Upper") ? 1f : alignment.ToString().StartsWith("Lower") ? 0f : 0.5f);
            var label = rect.gameObject.AddComponent<Text>();
            label.font = font;
            label.text = text;
            label.fontSize = size;
            label.fontStyle = FontStyle.Bold;
            label.alignment = alignment;
            label.color = Color.white;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            var shadow = rect.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            return label;
        }

        // ---------------------------------------------------------------- helpers

        static GameObject Box(string name, Transform parent, Vector3 position, Vector3 size, Material material,
            bool collider = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        static void CreateMaterials()
        {
            ground = Mat("Grey_Ground", new Color(0.36f, 0.37f, 0.35f));
            wall = Mat("Grey_Wall", new Color(0.62f, 0.62f, 0.6f));
            dark = Mat("Grey_Dark", new Color(0.15f, 0.15f, 0.16f));
            metal = Mat("Grey_Metal", new Color(0.32f, 0.34f, 0.38f));
            accent = Mat("Accent_Orange", new Color(0.95f, 0.55f, 0.15f));
            enemy = Mat("Target_Hostile", new Color(0.8f, 0.2f, 0.18f));
            civilian = Mat("Target_Civilian", new Color(0.2f, 0.45f, 0.85f));
        }

        static Material Mat(string name, Color color)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", 0.15f);
            EditorUtility.SetDirty(material);
            return material;
        }

        static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent))
            {
                int slash = parent.LastIndexOf('/');
                EnsureFolder(parent.Substring(0, slash), parent.Substring(slash + 1));
            }
            if (!AssetDatabase.IsValidFolder($"{parent}/{child}"))
                AssetDatabase.CreateFolder(parent, child);
        }

        static void Set(Object target, string field, object value)
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
                default: throw new ArgumentException($"Unsupported value type {value?.GetType()}");
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetList(Object target, string field, params Object[] values)
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
