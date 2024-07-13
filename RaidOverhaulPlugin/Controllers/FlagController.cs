using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;
using RaidOverhaul.Helpers;

using static RaidOverhaul.Plugin;

namespace RaidOverhaul.Controllers
{
    public class FlagController : MonoBehaviour
    {
        public static JObject flagConfig;
        public static bool TraderEventStatus = false;

        public static void LoadFlagConfig()
        {
            flagConfig = JObject.Parse(File.ReadAllText(pluginPath + "/Resources" + "/Flags.json"));

            if (flagConfig["TraderEventHasRun"] != null)
            TraderEventStatus = (bool)flagConfig["TraderEventHasRun"];
        }

        public static void ResetTraderLL()
        {
            string filePath = Path.Combine(pluginPath, "Resources", "Flags");
            filePath += ".json";

            StreamWriter streamWriter = new StreamWriter(filePath);

            if (TraderEventStatus)
            {
                var Traders = Utils.Traders;
                
                foreach (var Trader in Traders)
                {
                    {
                        Session.Profile.TradersInfo[Trader].SetStanding(Session.Profile.TradersInfo[Trader].Standing - 1);
                    }
                }
            }
        }
    }
}