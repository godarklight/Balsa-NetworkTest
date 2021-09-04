using System;
using UnityEngine;
using BalsaCore;

namespace NetworkTestKSP
{
    [BalsaAddon]
    public class Loader
    {
        private static bool loaded = false;
        private static GameObject go;
        private static MonoBehaviour mod;

        //Game start
        [BalsaAddonInit]
        public static void BalsaInit()
        {
            if (!loaded)
            {
                loaded = true;
                go = new GameObject();
                mod = go.AddComponent<NetworkTestMain>();
            }
        }

        //Game exit
        [BalsaAddonFinalize]
        public static void BalsaFinalize()
        {
        }
    }
}
