using BepInEx;
using BepInEx.Logging;
using LegionPreLoader.Helpers;

namespace LegionPreLoader
{
    [BepInPlugin(ClientInfo.ROPreLoadGUID, ClientInfo.ROPreLoadName, ClientInfo.PluginVersion)]
    public class LegionPreLoader : BaseUnityPlugin
    {
        public static LegionPreLoader Instance { get; private set; }
        public static ManualLogSource Log;

        public void Awake()
        {
            LegionUtils.LoadJsonConfig();
            Instance = this;
        }
    }
}