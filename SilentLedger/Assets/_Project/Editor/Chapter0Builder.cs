using System.Collections.Generic;
using SilentLedger.Audio;
using SilentLedger.Interaction;
using SilentLedger.Mission;
using SilentLedger.Weapons;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using static SilentLedger.EditorTools.GreyboxKit;

namespace SilentLedger.EditorTools
{
    /// <summary>
    /// Builds Chapter 0 · First Light at Site Hollow, LANTERN's base in a decommissioned mountain
    /// airbase at dawn: helipad, sniper range, armory, ops room, briefing room and kill house,
    /// with the squad at their stations and the 0.1 Arrival mission wired up.
    /// </summary>
    public static class Chapter0Builder
    {
        const string ScenePath = Root + "/Scenes/Ch0_FirstLight.unity";
        const string RangeScenePath = Root + "/Scenes/Ch0_TestRange.unity";
        const string NavMeshPath = Root + "/Navigation/Ch0_FirstLight_NavMesh.asset";

        [MenuItem("Silent Ledger/Build Chapter 0 First Light")]
        public static string Build()
        {
            Init();
            var scene = NewScene();
            SetDawnLight();

            var level = new GameObject("Level").transform;
            BuildTerrain(level);
            BuildHelipad(level);
            var rangeTargets = BuildSniperRange(level, out var bishopSpot);
            var bench = BuildArmory(level, out var okaforSpot);
            BuildOpsRoom(level, out var tamsinSpot);
            BuildBriefingRoom(level, out var vargaSpot);
            var killHouseEntrance = BuildKillHouse(level);
            DressBase(level);
            EnvironmentBuilder.MarkStatic(level);
            BakeNavMesh(level);

            EnvironmentBuilder.DawnAtmosphere(Sun());
            EnvironmentBuilder.DawnPostProcessing("Ch0_Dawn_Profile");

            var squad = Group("Squad", null);
            var tony = Character(squad, "Tony", new Color(0.55f, 0.8f, 0.35f), new Vector3(2.4f, 0f, 9f), 180f, 6f, follower: true);
            Box("Bag", squad, new Vector3(3.2f, 0.2f, 8.2f), new Vector3(0.6f, 0.4f, 0.35f), Dark);
            var bishop = Character(squad, "Bishop", new Color(0.55f, 0.62f, 0.75f), bishopSpot, 0f, 0f);
            var okafor = Character(squad, "Okafor", new Color(0.9f, 0.55f, 0.3f), okaforSpot, -90f, 6f);
            var tamsin = Character(squad, "Tamsin", new Color(0.75f, 0.5f, 0.95f), tamsinSpot, -90f, 6f);
            var varga = Character(squad, "Col. Varga", new Color(0.92f, 0.9f, 0.82f), vargaSpot, -90f, 8f);

            var rig = PlayerRigBuilder.Build(new Vector3(0f, 0f, -6f), 0f);
            Set(rig.weapons, "startingWeaponCount", 1); // the pistol comes from the armory
            PlayerRigBuilder.AddEventSystem();

            var director = new GameObject("Chapter0Director").AddComponent<Chapter0Arrival>();
            Set(director, "intent", rig.intent);
            Set(director, "weapons", rig.weapons);
            Set(director, "touchHud", rig.touchHud);
            Set(director, "hud", rig.missionHud);
            Set(director, "player", rig.player);
            Set(director, "tony", tony);
            Set(director, "bishop", bishop);
            Set(director, "okafor", okafor);
            Set(director, "tamsin", tamsin);
            Set(director, "varga", varga);
            Set(director, "loadoutBench", bench);
            Set(director, "killHouseEntrance", killHouseEntrance);
            SetList(director, "rangeTargets", rangeTargets.ToArray());

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene(RangeScenePath, true),
            };
            AssetDatabase.SaveAssets();
            return $"Built {ScenePath}";
        }

        static void SetDawnLight()
        {
            var sun = Sun();
            // Early-morning warmth, but high enough that the ground reads clearly on a phone screen.
            sun.transform.rotation = Quaternion.Euler(28f, -40f, 0f);
            sun.color = new Color(1f, 0.85f, 0.7f);
            sun.intensity = 1.3f;
        }

        // ---------------------------------------------------------------- level

        static void BuildTerrain(Transform level)
        {
            // Site Hollow sits in a flat mountain valley; the sniper range runs north up it to 400 m.
            var valley = new EnvironmentBuilder.Valley { flat = Rect.MinMaxRect(-75f, -45f, 75f, 450f), rise = 120f };
            var terrain = EnvironmentBuilder.BuildTerrain(level, "Ch0_Terrain", new Vector3(-400f, 0f, -300f),
                new Vector3(800f, 180f, 900f), valley, seed: 7);
            EnvironmentBuilder.ScatterPines(level, terrain, 140, seed: 7);

            Box("Apron", level, new Vector3(5f, 0.005f, 0f), new Vector3(60f, 0.01f, 50f), Tarmac, collider: false);
            Box("Hangar", level, new Vector3(0f, 6f, 36f), new Vector3(36f, 12f, 8f), Metal);
            Box("HangarDoors", level, new Vector3(0f, 4.5f, 31.9f), new Vector3(24f, 9f, 0.2f), Dark);
        }

        /// <summary>Fence, floodlights, road, barriers, sandbags, crates, drums, mast and signs.</summary>
        static void DressBase(Transform level)
        {
            var props = Group("Props", level);
            var lamp = Mat("Lamp_Emissive", new Color(1f, 0.95f, 0.85f), emission: new Color(4f, 3.6f, 3f));
            var redLamp = Mat("Lamp_Red", new Color(1f, 0.2f, 0.15f), emission: new Color(4f, 0.4f, 0.3f));
            var sandbag = Mat("Sandbag", new Color(0.6f, 0.54f, 0.4f), ProceduralTextures.Concrete());
            var crate = Mat("Crate_Olive", new Color(0.36f, 0.38f, 0.26f), ProceduralTextures.MetalPanel());
            var drum = Mat("Drum_Red", new Color(0.5f, 0.17f, 0.12f), ProceduralTextures.MetalPanel(), smoothness: 0.3f);
            var paint = Mat("Paint_Yellow", new Color(0.9f, 0.75f, 0.2f));

            // Perimeter fence with a gate on the south side; the range strip stays open to the north.
            var fence = Group("Fence", props);
            FenceRun(fence, new Vector3(-60f, 0f, -35f), new Vector3(-6f, 0f, -35f));
            FenceRun(fence, new Vector3(6f, 0f, -35f), new Vector3(62f, 0f, -35f));
            FenceRun(fence, new Vector3(62f, 0f, -35f), new Vector3(62f, 0f, 72f));
            FenceRun(fence, new Vector3(-28f, 0f, 72f), new Vector3(62f, 0f, 72f));
            FenceRun(fence, new Vector3(-60f, 0f, -35f), new Vector3(-60f, 0f, 72f));

            // Access road and barriers from the gate to the apron.
            Box("Road", props, new Vector3(0f, 0.006f, -31f), new Vector3(9f, 0.01f, 14f), Tarmac, collider: false);
            for (int i = 0; i < 5; i++)
            {
                Box("Barrier", props, new Vector3(-5.5f, 0.4f, -34f + i * 2.6f), new Vector3(0.6f, 0.8f, 2.2f), Wall);
                Box("Barrier", props, new Vector3(5.5f, 0.4f, -34f + i * 2.6f), new Vector3(0.6f, 0.8f, 2.2f), Wall);
            }

            // Apron paint: taxi line from the helipad to the hangar, and an apron edge line.
            Box("TaxiLine", props, new Vector3(0f, 0.012f, 24.5f), new Vector3(0.3f, 0.005f, 12f), paint, collider: false);
            Box("EdgeLine", props, new Vector3(-24.6f, 0.012f, 0f), new Vector3(0.25f, 0.005f, 50f), paint, collider: false);

            // Floodlight towers facing the middle of the base.
            foreach (var spot in new[] { new Vector3(-55f, 0f, -30f), new Vector3(55f, 0f, -30f), new Vector3(55f, 0f, 66f), new Vector3(-24f, 0f, 66f) })
            {
                var tower = Group("Floodlight", props, spot);
                var toCentre = new Vector3(5f, 0f, 15f) - spot;
                tower.rotation = Quaternion.LookRotation(new Vector3(toCentre.x, 0f, toCentre.z));
                Box("Pole", tower, new Vector3(0f, 5f, 0f), new Vector3(0.3f, 10f, 0.3f), Metal);
                Box("Head", tower, new Vector3(0f, 10.1f, 0.2f), new Vector3(1.6f, 0.7f, 0.4f), Dark, collider: false);
                Box("Lamp", tower, new Vector3(0f, 10.1f, 0.42f), new Vector3(1.4f, 0.5f, 0.04f), lamp, collider: false);
            }

            // Radio mast behind the ops room, red warning lamp on top.
            Box("RadioMast", props, new Vector3(31f, 9f, -20f), new Vector3(0.4f, 18f, 0.4f), Metal);
            Box("MastLamp", props, new Vector3(31f, 18.2f, -20f), new Vector3(0.35f, 0.35f, 0.35f), redLamp, collider: false);

            // Sandbag walls either side of the range bench.
            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -49.5f : -30.5f;
                Box("Sandbags", props, new Vector3(x, 0.45f, 2f), new Vector3(2.4f, 0.9f, 0.9f), sandbag);
                Box("Sandbags", props, new Vector3(x, 1.1f, 2f), new Vector3(2f, 0.4f, 0.8f), sandbag);
            }

            // Crates and fuel drums by the hangar.
            foreach (var c in new[] { new Vector3(13f, 0.6f, 26f), new Vector3(14.3f, 0.6f, 26.2f), new Vector3(13.6f, 1.8f, 26.1f), new Vector3(-15f, 0.6f, 25f), new Vector3(-16.3f, 0.6f, 24.6f) })
                Box("Crate", props, c, new Vector3(1.2f, 1.2f, 1.2f), crate);
            for (int i = 0; i < 5; i++)
            {
                var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                barrel.name = "Drum";
                barrel.transform.SetParent(props, false);
                barrel.transform.localPosition = new Vector3(-12f + (i % 3) * 0.7f, 0.45f, 27f + (i / 3) * 0.7f);
                barrel.transform.localScale = new Vector3(0.6f, 0.45f, 0.6f);
                barrel.GetComponent<Renderer>().sharedMaterial = drum;
            }

            // Windsock by the helipad.
            Box("WindsockPole", props, new Vector3(9f, 2f, 18f), new Vector3(0.1f, 4f, 0.1f), Metal);
            var sock = Box("Windsock", props, new Vector3(9.55f, 3.8f, 18f), new Vector3(1.1f, 0.35f, 0.35f), Accent, collider: false);
            sock.transform.localRotation = Quaternion.Euler(0f, 20f, -12f);

            // Door signs.
            foreach (var z in new[] { 1f, -13f })
                Box("DoorSign", props, new Vector3(17.85f, 2.65f, z), new Vector3(0.05f, 0.35f, 1.6f), Accent, collider: false);
            Box("DoorSign", props, new Vector3(31.85f, 2.65f, 1f), new Vector3(0.05f, 0.35f, 1.6f), Accent, collider: false);
        }

        /// <summary>A straight fence: posts every 4 m, two rails, and one collider for the whole run.</summary>
        static void FenceRun(Transform parent, Vector3 from, Vector3 to)
        {
            var run = Group("FenceRun", parent, (from + to) * 0.5f);
            var along = to - from;
            float length = along.magnitude;
            run.rotation = Quaternion.LookRotation(along.normalized);
            int posts = Mathf.Max(2, Mathf.RoundToInt(length / 4f) + 1);
            for (int i = 0; i < posts; i++)
            {
                float z = -length / 2f + length * i / (posts - 1);
                Box("Post", run, new Vector3(0f, 1.1f, z), new Vector3(0.1f, 2.2f, 0.1f), Metal, collider: false);
            }
            Box("RailTop", run, new Vector3(0f, 2.1f, 0f), new Vector3(0.05f, 0.05f, length), Metal, collider: false);
            Box("RailMid", run, new Vector3(0f, 1.1f, 0f), new Vector3(0.05f, 0.05f, length), Metal, collider: false);
            run.gameObject.AddComponent<BoxCollider>().size = new Vector3(0.2f, 2.2f, length);
        }

        static void BuildHelipad(Transform level)
        {
            var pad = Group("Helipad", level, new Vector3(0f, 0f, 12f));
            Box("Pad", pad, new Vector3(0f, 0.02f, 0f), new Vector3(14f, 0.02f, 14f), Dark, collider: false);
            Box("H_Left", pad, new Vector3(-1.5f, 0.035f, 0f), new Vector3(0.6f, 0.01f, 5f), Accent, collider: false);
            Box("H_Right", pad, new Vector3(1.5f, 0.035f, 0f), new Vector3(0.6f, 0.01f, 5f), Accent, collider: false);
            Box("H_Bar", pad, new Vector3(0f, 0.035f, 0f), new Vector3(3f, 0.01f, 0.6f), Accent, collider: false);

            var heli = Group("Helicopter", pad, new Vector3(-0.5f, 0f, 1f));
            Box("Fuselage", heli, new Vector3(0f, 1.4f, 0f), new Vector3(2.4f, 2.2f, 5f), Metal);
            Box("Cockpit", heli, new Vector3(0f, 1.3f, 3.2f), new Vector3(2.2f, 1.8f, 1.5f), Glass);
            Box("TailBoom", heli, new Vector3(0f, 1.9f, -4.4f), new Vector3(0.5f, 0.5f, 4.5f), Metal);
            Box("TailFin", heli, new Vector3(0f, 2.6f, -6.4f), new Vector3(0.2f, 1.4f, 0.9f), Metal);
            Box("Rotor", heli, new Vector3(0f, 2.65f, 0f), new Vector3(11f, 0.06f, 0.35f), Dark, collider: false);
            Box("Skid_L", heli, new Vector3(-1.1f, 0.15f, 0f), new Vector3(0.15f, 0.15f, 5f), Dark);
            Box("Skid_R", heli, new Vector3(1.1f, 0.15f, 0f), new Vector3(0.15f, 0.15f, 5f), Dark);
        }

        static List<Target> BuildSniperRange(Transform level, out Vector3 bishopSpot)
        {
            var range = Group("SniperRange", level, new Vector3(-40f, 0f, 0f));
            Box("FiringBench", range, new Vector3(0f, 0.5f, 2f), new Vector3(16f, 1f, 0.8f), Wall);
            bishopSpot = range.position + new Vector3(4.5f, 0f, 0.9f);

            var targets = new List<Target>();
            float[] lanes = { -5f, 0f, 5f };
            float[] distances = { 100f, 200f, 400f };
            float[] scales = { 1.2f, 1.8f, 3f };
            for (int i = 0; i < lanes.Length; i++)
            {
                targets.Add(SpawnTarget($"Lane{i + 1}_Near", range, new Vector3(lanes[i], 0f, 25f), 0f, 1f, false));
                targets.Add(SpawnTarget($"Lane{i + 1}_{distances[i]}m", range, new Vector3(lanes[i], 0f, 2f + distances[i]), 0f, scales[i], false));
            }
            foreach (float x in new[] { -7.5f, -2.5f, 2.5f, 7.5f })
                Box("LaneDivider", range, new Vector3(x, 0.3f, 16f), new Vector3(0.15f, 0.6f, 26f), Dark);
            return targets;
        }

        static MissionInteractable BuildArmory(Transform level, out Vector3 okaforSpot)
        {
            var armory = Room("Armory", level, 18f, -4f, 28f, 6f, 'W');
            Box("WeaponRack", armory, new Vector3(27.6f, 1f, 1f), new Vector3(0.3f, 2f, 6f), Metal);
            var crate = Box("AmmoCrate", armory, new Vector3(20f, 0.45f, 5f), new Vector3(1.2f, 0.9f, 0.8f), Accent);
            crate.AddComponent<AmmoCrate>();
            var bench = Box("LoadoutBench", armory, new Vector3(23.5f, 0.45f, 5.2f), new Vector3(3f, 0.9f, 0.8f), Wall);
            var interactable = bench.AddComponent<MissionInteractable>();
            Set(interactable, "prompt", "Take loadout");
            okaforSpot = new Vector3(25f, 0f, 0f);
            return interactable;
        }

        static void BuildOpsRoom(Transform level, out Vector3 tamsinSpot)
        {
            var ops = Room("OpsRoom", level, 18f, -18f, 28f, -8f, 'W');
            Box("ScreenWall", ops, new Vector3(27.75f, 1.7f, -13f), new Vector3(0.1f, 1.6f, 7f), Glass);
            Box("MapTable", ops, new Vector3(23f, 0.5f, -13f), new Vector3(2.6f, 1f, 1.6f), Dark);
            Box("MapTop", ops, new Vector3(23f, 1.01f, -13f), new Vector3(2.4f, 0.02f, 1.4f), Glass, collider: false);
            tamsinSpot = new Vector3(25.8f, 0f, -11f);
        }

        static void BuildBriefingRoom(Transform level, out Vector3 vargaSpot)
        {
            var briefing = Room("BriefingRoom", level, 32f, -4f, 42f, 6f, 'W');
            Box("Screen", briefing, new Vector3(41.75f, 1.7f, 1f), new Vector3(0.1f, 1.4f, 3.5f), Glass);
            Box("Lectern", briefing, new Vector3(39.5f, 0.55f, 3f), new Vector3(0.6f, 1.1f, 0.6f), Dark);
            for (int row = 0; row < 2; row++)
            for (int seat = 0; seat < 3; seat++)
                Box("Chair", briefing, new Vector3(35f + row * 1.6f, 0.25f, -1f + seat * 1.6f), new Vector3(0.6f, 0.5f, 0.6f), Metal);
            vargaSpot = new Vector3(40.3f, 0f, 1f);
        }

        /// <summary>The kill house for 0.2. Returns a marker just outside its entrance.</summary>
        static Transform BuildKillHouse(Transform level)
        {
            var house = Group("KillHouse", level, new Vector3(16f, 0f, 46f));

            WallAlongZ(house, 14f, -6f, 8f, (0f, 1.4f));
            WallAlongZ(house, 26f, -6f, 8f);
            WallAlongX(house, -6f, 14f, 26f);
            WallAlongX(house, 8f, 14f, 26f);
            WallAlongZ(house, 20f, -6f, 8f, (-2.6f, -1.2f));
            WallAlongX(house, 1f, 20f, 26f, (22.4f, 23.8f));
            SpawnDoor(house, new Vector3(20f, 0f, -2.6f), -90f);
            SpawnDoor(house, new Vector3(22.4f, 0f, 1f), 0f);

            SpawnTarget("Hostile_W1", house, new Vector3(17.5f, 0f, 5f), 90f, 1f, false);
            SpawnTarget("Civilian_W", house, new Vector3(16f, 0f, -4f), 90f, 1f, true);
            SpawnTarget("Hostile_SE1", house, new Vector3(24.5f, 0f, -4f), 90f, 1f, false);
            SpawnTarget("Hostile_SE2", house, new Vector3(23f, 0f, -1.5f), 90f, 1f, false);
            SpawnTarget("Hostile_NE1", house, new Vector3(24.5f, 0f, 6.5f), 0f, 1f, false);
            SpawnTarget("Civilian_NE", house, new Vector3(21.5f, 0f, 6.5f), 0f, 1f, true);

            return Group("Entrance", house, new Vector3(12f, 0f, 0.7f));
        }

        static void BakeNavMesh(Transform level)
        {
            var surface = level.gameObject.AddComponent<NavMeshSurface>();
            // Only the base itself: the terrain is far larger than anywhere squadmates walk.
            // Keep the volume taller than the terrain: a shorter one silently drops the terrain
            // and leaves the valley floor without NavMesh (Tony then snaps onto the helicopter roof).
            surface.collectObjects = CollectObjects.Volume;
            surface.center = new Vector3(0f, 95f, 18f);
            surface.size = new Vector3(150f, 200f, 120f);
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = ~((1 << IgnoreRaycastLayer) | (1 << UILayer) | (1 << SquadLayer));
            surface.BuildNavMesh();
            AssetDatabase.DeleteAsset(NavMeshPath);
            AssetDatabase.CreateAsset(surface.navMeshData, NavMeshPath);
        }

        // ---------------------------------------------------------------- characters

        static SquadMember Character(Transform parent, string name, Color color, Vector3 position, float yaw,
            float faceRange, bool follower = false)
        {
            var root = Group(name, parent, position);
            root.localRotation = Quaternion.Euler(0f, yaw, 0f);

            var material = Mat($"Char_{name.Replace(". ", "_").Replace(" ", "_")}", color);
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.transform.localScale = new Vector3(0.6f, 0.9f, 0.6f);
            body.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(body.GetComponent<Collider>());
            Box("Visor", root, new Vector3(0f, 1.55f, 0.24f), new Vector3(0.36f, 0.12f, 0.12f), Dark, collider: false);

            var capsule = root.gameObject.AddComponent<CapsuleCollider>();
            capsule.height = 1.8f;
            capsule.radius = 0.3f;
            capsule.center = new Vector3(0f, 0.9f, 0f);

            if (follower)
            {
                var agent = root.gameObject.AddComponent<NavMeshAgent>();
                agent.speed = 5f;
                agent.angularSpeed = 540f;
                agent.acceleration = 20f;
                agent.radius = 0.35f;
                agent.height = 1.8f;
                root.gameObject.AddComponent<AgentFootsteps>();
            }

            var member = root.gameObject.AddComponent<SquadMember>();
            Set(member, "displayName", name);
            Set(member, "color", color);
            Set(member, "faceRange", faceRange);
            SetLayerRecursively(root.gameObject, SquadLayer);
            return member;
        }
    }
}
