using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;
using BepInEx.Logging;
using Newtonsoft.Json;
using SPT.Common.Http;
using EFT.Weather;
using EFT.InventoryLogic;
using UnityEngine;

namespace RaidOverhaul.Helpers
{
    public static class Utils
    {
        public static FieldInfo FogField;
        public static FieldInfo LighteningThunderField;
        public static FieldInfo RainField;
        public static FieldInfo TemperatureField;

        public static readonly List<string> Traders = new List<string> {
            "54cb50c76803fa8b248b4571",     //Prapor
            "54cb57776803fa99248b456e",     //Therapist 
            "579dc571d53a0658a154fbec",     //Fence
            "58330581ace78e27b8b10cee",     //Skier 
            "5935c25fb3acc3127c3d8cd9",     //Peacekeeper 
            "5a7c2eca46aef81a7ca2145d",     //Mechanic 
            "5ac3b934156ae10c4430e83c",     //Ragman 
            "5c0647fdd443bc2504c2d371",     //Jaeger 
            "Requisitions"                  //Req Shop
        };

        public static readonly string ReqID = "Requisitions";
        public static readonly string skeletonKey = "66a2fc926af26cc365283f23";
        public static readonly string vipKeycard = "66a2fc9886fbd5d38c5ca2a6";

        public static T Get<T>(string url) {
            var req = RequestHandler.GetJson(url);

            if (string.IsNullOrEmpty(req)) {
                throw new InvalidOperationException("The response from the server is null or empty.");
            }

            return JsonConvert.DeserializeObject<T>(req);
        }
        public static void LogToServerConsole(string message) {
            Plugin.Log.Log( LogLevel.Info, message);
            RequestHandler.GetJson("/RaidOverhaul/LogToServer/" + message);
        }

        public static EquipmentSlot[] _armbandFAS = {
            EquipmentSlot.Pockets,
            EquipmentSlot.TacticalVest,
            EquipmentSlot.ArmBand
        };

        public static EquipmentSlot[] _armbandAAS = {
            EquipmentSlot.TacticalVest,
            EquipmentSlot.ArmorVest,
            EquipmentSlot.Headwear,
            EquipmentSlot.FaceCover,
            EquipmentSlot.Eyewear,
            EquipmentSlot.ArmBand
        };

        public static void LoadLegionSettings()
        {
            if (File.Exists(Plugin.legionJsonPath)) {
                try
                {
                    string legionSettingsJson = File.ReadAllText(Plugin.legionJsonPath);
                    Plugin.legionText = new TextAsset(legionSettingsJson);
                    Debug.Log("Legion settings loaded successfully");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error loading Legion settings from {Plugin.legionJsonPath}: {ex.Message}");
                }
            }
            else {
                Debug.LogError($"Legion settings file not found at {Plugin.legionJsonPath}");
            }
        }

        public static void GetWeatherFields()
        {
            FogField = AccessTools.Field(typeof(WeatherDebug), "Fog");
            LighteningThunderField = AccessTools.Field(typeof(WeatherDebug), "LightningThunderProbability");
            RainField = AccessTools.Field(typeof(WeatherDebug), "Rain");
            TemperatureField = AccessTools.Field(typeof(WeatherDebug), "Temperature");
        }
    }
}
