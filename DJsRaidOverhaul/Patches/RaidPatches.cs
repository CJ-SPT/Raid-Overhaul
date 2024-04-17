using EFT;
using EFT.UI;
using TMPro;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Collections.Generic;
using HarmonyLib;
using Comfort.Common;
using Newtonsoft.Json;
using UnityEngine;
using EFT.UI.Map;
using EFT.Weather;
using EFT.Interactive;
using EFT.UI.Matchmaker;
using EFT.Communications;
using EFT.UI.BattleTimer;
using Aki.Common.Http;
using Aki.Custom.Airdrops;
using Aki.Custom.BTR.Utils;
using Aki.Custom.Airdrops.Utils;
using Aki.Custom.Airdrops.Models;
using Aki.Reflection.Patching;
using DJsRaidOverhaul.Helpers;
using DJsRaidOverhaul.Controllers;
using EFT.InputSystem;
using EFT.UI.Gestures;

namespace DJsRaidOverhaul.Patches
{
    public struct RaidTime
    {
        internal static bool inverted = false;

        private static DateTime inverseTime
        {
            get
            {
                DateTime result = DateTime.Now.AddHours(12);
                return result.Day > DateTime.Now.Day
                       ? result.AddDays(-1)
                       : result.Day < DateTime.Now.Day
                       ? result.AddDays(1) : result;
            }
        }

        public static DateTime GetCurrTime() => DateTime.Now;
        public static DateTime GetInverseTime() => inverseTime;
        public static DateTime GetDateTime() => inverted ? GetInverseTime() : GetCurrTime();
    }

    public class GameWorldPatch : ModulePatch
    {

        protected override MethodBase GetTargetMethod() => typeof(GameWorld).GetMethod("OnGameStarted", BindingFlags.Instance | BindingFlags.Public);

        [PatchPostfix]
        static void Postfix(GameWorld __instance)
        {
            DateTime time = RaidTime.GetDateTime();
            __instance.GameDateTime.Reset(time, time, 1);
        }
    }

    public class GlobalsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(TarkovApplication).GetMethod("Start", BindingFlags.Instance | BindingFlags.Public);

        [PatchPostfix]
        static async void Postfix(TarkovApplication __instance)
        {
            while (__instance.GetClientBackEndSession() == null || __instance.GetClientBackEndSession().BackEndConfig == null)
                await Task.Yield();

            BackendConfigSettingsClass globals = __instance.GetClientBackEndSession().BackEndConfig.Config;
            globals.AllowSelectEntryPoint = true;
        }
    }

    public class EnableEntryPointPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(EntryPointView).GetMethod("Show", BindingFlags.Instance | BindingFlags.Public);

        [PatchPrefix]
        static void Prefix(ref bool allowSelection) => allowSelection = true;
    }

    public class UIPanelPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(LocationConditionsPanel).GetMethod("method_0", BindingFlags.Instance | BindingFlags.Public);

        [PatchPostfix]
        static void Postfix(ref TextMeshProUGUI ____currentPhaseTime, ref TextMeshProUGUI ____nextPhaseTime)
        {
            try { ____nextPhaseTime.text = RaidTime.GetInverseTime().ToString("HH:mm:ss"); }
            catch (Exception) { }
            finally { ____currentPhaseTime.text = RaidTime.GetCurrTime().ToString("HH:mm:ss"); }
        }
    }

    public class TimerUIPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(TimerPanel).GetMethod("SetTimerText", BindingFlags.Instance | BindingFlags.Public);

        [PatchPrefix]
        static void Prefix(ref TimeSpan timeSpan) => timeSpan = new TimeSpan(RaidTime.GetDateTime().Ticks);
    }

    public class ExitTimerUIPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(MainTimerPanel).GetMethod("UpdateTimer", BindingFlags.Instance | BindingFlags.Public);

        [PatchTranspiler]
        static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
        {
            int shift = 0;

            instructions.ExecuteForEach((inst) =>
            {
                if (shift == 2)
                    inst.opcode = OpCodes.Ret;
                if (shift >= 3)
                    inst.opcode = OpCodes.Nop;
                shift++;
            });

            return instructions;
        }
    }

    public class EventExfilPatch : ModulePatch
    {
        internal static bool IsLockdown = false;

        internal static bool awaitDrop = false;

        protected override MethodBase GetTargetMethod() => typeof(ExfiltrationRequirement).GetMethod("Met", BindingFlags.Instance | BindingFlags.Public);

        [PatchPostfix]
        static void Postfix(Player player, ref bool __result)
        {
            if (player.IsYourPlayer)
            {
                if (IsLockdown)
                {
                    NotificationManagerClass.DisplayMessageNotification("Cannot extract during a lockdown", ENotificationDurationType.Long, ENotificationIconType.Alert);
                }
            }
            __result = true;
        }
    }

    public class WeatherControllerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(WeatherController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.Public);

        [PatchPostfix]
        static void Postfix(ref WeatherController __instance) => __instance.WindController.CloudWindMultiplier = 1;
    }

    public class WatchPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(Watch).GetProperty("DateTime_0", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(true);

        [PatchPostfix]
        static void Postfix(ref DateTime __result)
        {
            __result = RaidTime.GetDateTime();
        }
    }

    public class RandomizeDefaultStatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPrefix]
        public static void PatchPrefix()
        {
            DoorController.RandomizeDefaultDoors();
            DoorController.RandomizeLampState();
        }
    }

    public class RigPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(PreloaderUI).GetMethod("InitConsole");
        }

        [PatchPostfix]
        public static void Postfix(PreloaderUI __instance)
        {
            LayoutLoader.LoadRigLayouts();
        }
    }

    public class AirdropBoxPatch : ModulePatch
    {
        internal static bool isExtractCrate = false;
        private static JsonConverter[] _defaultJsonConverters;

        protected override MethodBase GetTargetMethod() { return typeof(AirdropsManager).GetMethod("BuildLootContainer", BindingFlags.Instance | BindingFlags.NonPublic); }

        [PatchPrefix]
        static bool Prefix(ref ItemFactoryUtil ___factory, ref AirdropParametersModel ___airdropParameters, AirdropBox ___airdropBox)
        {
            if (!isExtractCrate) return true;
            BuildCrate(___airdropBox);
            ___airdropParameters.AirdropAvailable = true;
            return false;
        }

        [PatchPostfix]
        static void Postfix(ref AirdropBox ___airdropBox, ref AirdropParametersModel ___airdropParameters)
        {
            if (!isExtractCrate) return;
            AwaitThenGetBox(___airdropParameters, ___airdropBox.container);
        }

        static void BuildCrate(AirdropBox airdrop)
        {
            var itemCrate = Singleton<ItemFactory>.Instance.CreateItem("exfilcratecontainer", "6223349b3136504a544d1608", null);
            LootItem.CreateLootContainer(airdrop.container, itemCrate, "Heavy crate", Singleton<GameWorld>.Instance);
        }

        static async void AwaitThenGetBox(AirdropParametersModel param, LootableContainer box)
        {
            if (!isExtractCrate) return;
            isExtractCrate = false;

            while (Vector3.Distance(box.transform.position, param.RandomAirdropPoint) > 3f)
            {
                await Task.Yield();
            }

            NotificationManagerClass.DisplayMessageNotification($"The extract crate has landed, secure your loot while you can", ENotificationDurationType.Long, ENotificationIconType.Default);

            EventExfilPatch.awaitDrop = true;

            await Task.Delay(150000);

            NotificationManagerClass.DisplayMessageNotification("The extract crate is locked, and any gear within it is now secured and will be returned to your stash at the end of the raid, if possible.", ENotificationDurationType.Long, ENotificationIconType.Default);

            typeof(LootableContainer).GetMethod("Lock", BindingFlags.Instance | BindingFlags.Public).Invoke(box, null);

            sendExfilBox(box);

            EventExfilPatch.awaitDrop = false;
        }

        static void sendExfilBox(LootableContainer airdropBox)
        {
            var exfilCrateItems = Singleton<ItemFactory>.Instance.TreeToFlatItems(airdropBox.ItemOwner.MainStorage[0].Items);

            RequestHandler.PutJson("/singleplayer/traderServices/itemDelivery", new
            {
                items = exfilCrateItems,
                traderId = Utils.ReqID
            }.ToJson(_defaultJsonConverters));
        }
    }
}