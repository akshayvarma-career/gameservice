using SilentLedger.Audio;
using SilentLedger.Interaction;
using SilentLedger.Mission;
using SilentLedger.Player;
using SilentLedger.UI;
using SilentLedger.Weapons;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static SilentLedger.EditorTools.GreyboxKit;

namespace SilentLedger.EditorTools
{
    /// <summary>
    /// Builds the PlayerRig: first-person player, weapons, touch HUD and mission HUD.
    /// Saves it as a prefab and leaves the instance in the open scene.
    /// </summary>
    public static class PlayerRigBuilder
    {
        public const string PrefabPath = Root + "/Prefabs/PlayerRig.prefab";

        public struct Rig
        {
            public GameObject root;
            public Transform player;
            public PlayerIntent intent;
            public WeaponHolder weapons;
            public TouchHud touchHud;
            public MissionHud missionHud;
        }

        static Font font;
        static Sprite circle;
        static readonly Vector2 Half = new Vector2(0.5f, 0.5f);

        public static Rig Build(Vector3 spawn, float yaw)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            var rig = new Rig { root = new GameObject("PlayerRig") };

            var player = new GameObject("Player") { layer = IgnoreRaycastLayer };
            player.transform.SetParent(rig.root.transform, false);
            player.transform.SetPositionAndRotation(spawn, Quaternion.Euler(0f, yaw, 0f));
            rig.player = player.transform;

            var body = player.AddComponent<CharacterController>();
            body.height = 1.8f;
            body.radius = 0.35f;
            body.center = new Vector3(0f, 0.9f, 0f);
            body.stepOffset = 0.3f;

            rig.intent = player.AddComponent<PlayerIntent>();
            var controller = player.AddComponent<PlayerController>();
            var look = player.AddComponent<PlayerLook>();
            var keyboard = player.AddComponent<KeyboardMouseInput>();

            var cameraRoot = Group("CameraRoot", player.transform, new Vector3(0f, 1.65f, 0f));
            var cameraGo = new GameObject("Main Camera") { tag = "MainCamera", layer = IgnoreRaycastLayer };
            cameraGo.transform.SetParent(cameraRoot, false);
            var cam = cameraGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.03f;
            cam.farClipPlane = 1000f;
            cam.fieldOfView = 70f;
            cameraGo.AddComponent<AudioListener>();
            var cameraData = cameraGo.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = true;
            cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing; // cheap on mobile

            rig.weapons = cameraGo.AddComponent<WeaponHolder>();
            var interactor = cameraGo.AddComponent<Interactor>();
            var rifle = BuildRifle(cameraGo.transform);
            var pistol = BuildPistol(cameraGo.transform);

            Set(controller, "intent", rig.intent);
            Set(controller, "cameraRoot", cameraRoot);
            Set(look, "intent", rig.intent);
            Set(look, "viewCamera", cam);
            Set(keyboard, "intent", rig.intent);
            Set(rig.weapons, "intent", rig.intent);
            Set(rig.weapons, "look", look);
            Set(rig.weapons, "controller", controller);
            Set(rig.weapons, "viewCamera", cam);
            Set(rig.weapons, "hitMask", Physics.DefaultRaycastLayers & ~(1 << SquadLayer)); // no friendly fire
            SetList(rig.weapons, "weapons", rifle, pistol);
            Set(interactor, "intent", rig.intent);

            BuildAudio(rig.root.transform);
            rifle.shotSounds = SfxGenerator.Clips("rifle_shot_");
            pistol.shotSounds = SfxGenerator.Clips("pistol_shot_");
            Set(player.AddComponent<Footsteps>(), "controller", controller);
            Set(cameraGo.AddComponent<WeaponAudio>(), "weapons", rig.weapons);

            BuildHud(ref rig, interactor);
            PrefabUtility.SaveAsPrefabAssetAndConnect(rig.root, PrefabPath, InteractionMode.AutomatedAction);
            return rig;
        }

        public static void AddEventSystem()
        {
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        // ---------------------------------------------------------------- audio

        static void BuildAudio(Transform parent)
        {
            var library = SfxGenerator.EnsureGenerated();
            var audio = Group("Audio", parent);
            var voices2D = new AudioSource[8];
            var voices3D = new AudioSource[12];
            for (int i = 0; i < voices2D.Length; i++) voices2D[i] = Voice(audio, $"Voice2D_{i}", spatial: false);
            for (int i = 0; i < voices3D.Length; i++) voices3D[i] = Voice(audio, $"Voice3D_{i}", spatial: true);

            var sfx = audio.gameObject.AddComponent<Sfx>();
            Set(sfx, "library", library);
            SetList(sfx, "voices2D", voices2D);
            SetList(sfx, "voices3D", voices3D);
        }

        static AudioSource Voice(Transform parent, string name, bool spatial)
        {
            var source = Group(name, parent).gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = spatial ? 1f : 0f;
            source.dopplerLevel = 0f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.minDistance = 1.5f;
            source.maxDistance = 60f;
            return source;
        }

        // ---------------------------------------------------------------- weapons

        static Weapon BuildRifle(Transform cam)
        {
            var weapon = ViewModelRoot("Rifle", cam);
            weapon.stats = new WeaponStats
            {
                displayName = "Rifle", damage = 34f, roundsPerMinute = 600f, automatic = true,
                magazineSize = 30, reserveAmmo = 120, reloadSeconds = 2.2f, range = 500f,
                hipSpread = 2.5f, aimSpread = 0.3f, recoilPitch = 0.9f, recoilYaw = 0.25f, aimFov = 45f,
            };
            if (AttachModel(weapon, ArtSetup.RiflePrefab))
            {
                // Model origin is the line of sight, so aiming just drops it a hair below centre.
                weapon.hipPosition = new Vector3(0.13f, -0.1f, 0.36f);
                weapon.aimPosition = new Vector3(0f, -0.022f, 0.42f);
            }
            else
            {
                ViewModelPart("Body", weapon.transform, new Vector3(0f, 0f, 0.1f), new Vector3(0.06f, 0.09f, 0.55f), Dark);
                ViewModelPart("Magazine", weapon.transform, new Vector3(0f, -0.1f, 0.06f), new Vector3(0.045f, 0.14f, 0.07f), Metal);
                ViewModelPart("Stock", weapon.transform, new Vector3(0f, -0.02f, -0.24f), new Vector3(0.05f, 0.08f, 0.18f), Dark);
                ViewModelPart("Sight", weapon.transform, new Vector3(0f, 0.06f, 0f), new Vector3(0.025f, 0.03f, 0.08f), Metal);
                // Aim pose keeps the top of the sight just below the line of sight.
                weapon.hipPosition = new Vector3(0.2f, -0.22f, 0.45f);
                weapon.aimPosition = new Vector3(0f, -0.09f, 0.38f);
            }
            weapon.transform.localPosition = weapon.hipPosition;
            return weapon;
        }

        static Weapon BuildPistol(Transform cam)
        {
            var weapon = ViewModelRoot("Pistol", cam);
            weapon.stats = new WeaponStats
            {
                displayName = "Pistol", damage = 40f, roundsPerMinute = 300f, automatic = false,
                magazineSize = 12, reserveAmmo = 48, reloadSeconds = 1.5f, range = 100f,
                hipSpread = 2f, aimSpread = 0.5f, recoilPitch = 1.6f, recoilYaw = 0.2f, aimFov = 55f,
            };
            if (AttachModel(weapon, ArtSetup.PistolPrefab))
            {
                weapon.hipPosition = new Vector3(0.12f, -0.095f, 0.3f);
                weapon.aimPosition = new Vector3(0f, -0.008f, 0.32f);
            }
            else
            {
                ViewModelPart("Slide", weapon.transform, new Vector3(0f, 0f, 0.04f), new Vector3(0.04f, 0.05f, 0.18f), Dark);
                ViewModelPart("Grip", weapon.transform, new Vector3(0f, -0.07f, -0.02f), new Vector3(0.035f, 0.1f, 0.05f), Metal);
                ViewModelPart("Sight", weapon.transform, new Vector3(0f, 0.03f, 0.1f), new Vector3(0.01f, 0.012f, 0.012f), Metal);
                weapon.hipPosition = new Vector3(0.15f, -0.18f, 0.32f);
                weapon.aimPosition = new Vector3(0f, -0.045f, 0.3f);
            }
            weapon.transform.localPosition = weapon.hipPosition;
            weapon.gameObject.SetActive(false);
            return weapon;
        }

        /// <summary>Puts the imported gun model under the weapon, if it has been set up.</summary>
        static bool AttachModel(Weapon weapon, string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) return false;
            PrefabUtility.InstantiatePrefab(prefab, weapon.transform);
            return true;
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

        static void BuildHud(ref Rig rig, Interactor interactor)
        {
            var canvasGo = new GameObject("TouchHUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))
                { layer = UILayer };
            canvasGo.transform.SetParent(rig.root.transform, false);
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 1f;
            var canvas = canvasGo.transform;
            var intent = rig.intent;

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

            // Mission HUD: objective (top left), subtitles (bottom centre), banner, waypoint.
            var objective = Label("Objective", canvas, "", 30, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(40f, -30f), new Vector2(900f, 120f));
            var subtitle = Label("Subtitle", canvas, "", 32, TextAnchor.LowerCenter, new Vector2(0.5f, 0f), new Vector2(-60f, 70f), new Vector2(1000f, 140f));
            var banner = Label("Banner", canvas, "", 64, TextAnchor.MiddleCenter, Half, new Vector2(0f, 220f), new Vector2(1400f, 200f));
            var bannerGroup = banner.gameObject.AddComponent<CanvasGroup>();
            bannerGroup.blocksRaycasts = false;
            var waypoint = Rect("Waypoint", canvas, Half, Half, Vector2.zero, new Vector2(30f, 30f));
            var icon = Rect("Icon", waypoint, Half, Half, Vector2.zero, new Vector2(22f, 22f));
            icon.localRotation = Quaternion.Euler(0f, 0f, 45f);
            Img(icon, null, new Color(0.95f, 0.64f, 0.23f), raycast: false);
            var distance = Label("Distance", waypoint, "", 22, TextAnchor.UpperCenter, Half, new Vector2(0f, -20f), new Vector2(160f, 30f));
            var stats = Label("DevStats", canvas, "", 22, TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(300f, 30f));
            Set(stats.gameObject.AddComponent<DevStats>(), "label", stats);

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

            rig.touchHud = canvasGo.AddComponent<TouchHud>();
            var hud = rig.touchHud;
            Set(hud, "intent", intent);
            Set(hud, "weapons", rig.weapons);
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

            rig.missionHud = canvasGo.AddComponent<MissionHud>();
            var mission = rig.missionHud;
            Set(mission, "canvasRect", (RectTransform)canvas);
            Set(mission, "objectiveText", objective);
            Set(mission, "subtitleText", subtitle);
            Set(mission, "bannerText", banner);
            Set(mission, "bannerGroup", bannerGroup);
            Set(mission, "waypoint", waypoint);
            Set(mission, "waypointDistance", distance);
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
            string a = alignment.ToString();
            rect.pivot = new Vector2(a.Contains("Right") ? 1f : a.Contains("Left") ? 0f : 0.5f,
                a.StartsWith("Upper") ? 1f : a.StartsWith("Lower") ? 0f : 0.5f);
            var label = rect.gameObject.AddComponent<Text>();
            label.font = font;
            label.text = text;
            label.fontSize = size;
            label.fontStyle = FontStyle.Bold;
            label.alignment = alignment;
            label.color = Color.white;
            label.raycastTarget = false;
            label.supportRichText = true;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            var shadow = rect.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            return label;
        }
    }
}
