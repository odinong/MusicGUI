using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MusicPlayer.MainMenuPatches
{
    public class Loader
    {
        private static void Init()
        {
            Load = new GameObject();
            Load.AddComponent<HarmonyPatches>();
            Load.AddComponent<PluginInfo>();
            Load.AddComponent<Plugin>();
            UnityEngine.Object.DontDestroyOnLoad(Load);
        }
        private static void Unload()
        {
            UnityEngine.Object.Destroy(Load);
        }
        private static GameObject Load;
    }
}
