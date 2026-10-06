using UnityEngine;

namespace SilentLedger
{
    /// <summary>App-wide settings applied once at startup, before the first scene loads.</summary>
    public static class GameBootstrap
    {
        /// <summary>The design target; Android otherwise defaults to 30 FPS.</summary>
        const int TargetFrameRate = 60;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Configure()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = TargetFrameRate;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
}
