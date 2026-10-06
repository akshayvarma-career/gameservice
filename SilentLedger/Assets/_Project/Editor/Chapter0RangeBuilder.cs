using SilentLedger.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static SilentLedger.EditorTools.GreyboxKit;

namespace SilentLedger.EditorTools
{
    /// <summary>
    /// Builds the grey-box test range (sniper range, armory crate, kill house) used to try the
    /// player and weapons. Re-run it to regenerate the scene from code.
    /// </summary>
    public static class Chapter0RangeBuilder
    {
        const string ScenePath = Root + "/Scenes/Ch0_TestRange.unity";

        [MenuItem("Silent Ledger/Build Test Range")]
        public static string Build()
        {
            Init();
            var scene = NewScene();

            var level = new GameObject("Level").transform;
            BuildGroundAndBounds(level);
            BuildRange(level);
            BuildArmory(level);
            BuildKillHouse(level);

            PlayerRigBuilder.Build(new Vector3(0f, 0f, -10f), 0f);
            PlayerRigBuilder.AddEventSystem();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            return $"Built {ScenePath}";
        }

        static void BuildGroundAndBounds(Transform level)
        {
            // Base area around the origin; the range runs north (+z) to 430 m.
            Box("Ground", level, new Vector3(0f, -0.5f, 200f), new Vector3(80f, 1f, 480f), Ground);
            Box("Bound_W", level, new Vector3(-40f, 0.6f, 200f), new Vector3(0.5f, 1.2f, 480f), Wall);
            Box("Bound_E", level, new Vector3(40f, 0.6f, 200f), new Vector3(0.5f, 1.2f, 480f), Wall);
            Box("Bound_S", level, new Vector3(0f, 0.6f, -40f), new Vector3(80f, 1.2f, 0.5f), Wall);
            Box("Bound_N", level, new Vector3(0f, 2f, 440f), new Vector3(80f, 4f, 0.5f), Wall);
        }

        static void BuildRange(Transform level)
        {
            var range = Group("SniperRange", level);
            var bench = Box("FiringBench", range, new Vector3(-12f, 0.5f, 2f), new Vector3(10f, 1f, 0.6f), Wall);
            Info(bench, "Range briefing",
                "Sniper range. Aim down sights and hit the targets at 100, 200 and 400 m. Warm up on the close ones first.");

            float[] lanes = { -16f, -12f, -8f };
            float[] distances = { 100f, 200f, 400f };
            float[] scales = { 1.2f, 1.8f, 3f };
            for (int i = 0; i < lanes.Length; i++)
            {
                SpawnTarget($"Warmup_{i + 1}", range, new Vector3(lanes[i], 0f, 20f), 0f, 1f, false);
                SpawnTarget($"Lane{i + 1}_{distances[i]}m", range, new Vector3(lanes[i], 0f, 2f + distances[i]), 0f, scales[i], false);
            }
            foreach (float x in new[] { -18f, -14f, -10f, -6f })
                Box("LaneDivider", range, new Vector3(x, 0.3f, 15f), new Vector3(0.15f, 0.6f, 24f), Dark);
        }

        static void BuildArmory(Transform level)
        {
            var armory = Group("Armory", level);
            var crate = Box("AmmoCrate", armory, new Vector3(-4f, 0.45f, -6f), new Vector3(1.2f, 0.9f, 0.8f), Accent);
            crate.AddComponent<AmmoCrate>();
            Box("WeaponRack", armory, new Vector3(-6f, 1f, -6.5f), new Vector3(2f, 2f, 0.3f), Metal);
        }

        static void BuildKillHouse(Transform level)
        {
            var house = Group("KillHouse", level);

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

            var sign = Box("KillHouseSign", house, new Vector3(12.5f, 1f, -1f), new Vector3(0.2f, 2f, 1.2f), Accent);
            Info(sign, "Drill briefing",
                "Kill house: clear every room. Red targets are hostile. Blue targets are civilians, so don't shoot them.");

            SpawnTarget("Hostile_W1", house, new Vector3(17.5f, 0f, 5f), 90f, 1f, false);
            SpawnTarget("Civilian_W", house, new Vector3(16f, 0f, -4f), 90f, 1f, true);
            SpawnTarget("Hostile_SE1", house, new Vector3(24.5f, 0f, -4f), 90f, 1f, false);
            SpawnTarget("Hostile_SE2", house, new Vector3(23f, 0f, -1.5f), 90f, 1f, false);
            SpawnTarget("Hostile_NE1", house, new Vector3(24.5f, 0f, 6.5f), 0f, 1f, false);
            SpawnTarget("Civilian_NE", house, new Vector3(21.5f, 0f, 6.5f), 0f, 1f, true);
        }
    }
}
