using UnityEngine;
using RaidOverhaul.Helpers;

namespace RaidOverhaul.Controllers
{
    public class ConfigController : MonoBehaviour
    {
        public static ServerConfigs ServerConfig = new ServerConfigs();
        public static EventsConfig EventConfig = new EventsConfig();
        public static SeasonalConfig SeasonConfig = new SeasonalConfig();
    }
}