using Mono.Cecil;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace LegionPreLoader.Helpers
{
    public static class LegionUtils
    {
        public static JObject config;
        public static string serverModPath = Path.Combine(Environment.CurrentDirectory, "user", "mods", "RaidOverhaul", "config");
        public static bool EnableCustomBoss = true;
        public static bool RemoveFromSwag = false;
        public static bool EnableCustomItems = true;
        public static bool EnableTimeChanges = true;
        public static bool EnableWatchAnimations = true;
        public static bool BackupProfile = true;


        public static void AddEnumValue(ref TypeDefinition type, string name, object value)
        {
            const FieldAttributes defaultEnumFieldAttributes = FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault;

            type.Fields.Add(new FieldDefinition(name, defaultEnumFieldAttributes, type) { Constant = value });
        }

        public static void LoadJsonConfig()
        {
            try
            {
                config = JObject.Parse(File.ReadAllText(serverModPath + "/config.json"));

                if (config["EnableCustomBoss"] != null)
                {
                    EnableCustomBoss = (bool)config["EnableCustomBoss"];
                }
                if (config["RemoveFromSwag"] != null)
                {
                    RemoveFromSwag = (bool)config["RemoveFromSwag"];
                }
                if (config["EnableCustomItems"] != null)
                {
                    EnableCustomItems = (bool)config["EnableCustomItems"];
                }
                if (config["EnableTimeChanges"] != null)
                {
                    EnableTimeChanges = (bool)config["EnableTimeChanges"];
                }
                if (config["EnableWatchAnimations"] != null)
                {
                    EnableWatchAnimations = (bool)config["EnableWatchAnimations"];
                }
                if (config["BackupProfile"] != null)
                {
                    BackupProfile = (bool)config["BackupProfile"];
                }

                LegionPreLoader.Log.LogInfo("Server config loaded");
            }
            catch (FileNotFoundException) { 
                LegionPreLoader.Log.LogError("Couldn't find server config. Make sure you've installed the mod correctly. "); 
            }
            catch (Exception x) { 
                LegionPreLoader.Log.LogError("Couldn't read server config. Error => " + x.Message); 
            }
        }
    }

    public static class LegionEnums
    {
        public const string BossLegionName = "bosslegion";
        public const int BossLegionValue = 199;
    }
}