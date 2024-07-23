using BepInEx;
using LegionPreLoader.Helpers;

namespace LegionPreLoader
{
    [BepInPlugin(ClientInfo.ROPreLoadGUID, ClientInfo.ROPreLoadName, ClientInfo.PluginVersion)]
    public class LegionPreLoader : BaseUnityPlugin
    {
        public static LegionPreLoader Instance { get; private set; }

        public void Awake()
        {
            Instance = this;
        }
    }
}