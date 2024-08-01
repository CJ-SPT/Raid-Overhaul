using System;
using UnityEngine;
using BepInEx.Configuration;

namespace RaidOverhaul.Helpers
{
    public static class DJConfig
    {
        [Flags]
        public enum RaidEvents
        {
            Damage = 1,
            Blackout = 2,
            ArmorRepair = 4,
            Heal = 8,
            Airdrop = 16,
            Skill = 32,
            Metabolism = 64,
            Malfunction = 128,
            Trader = 256,
            Berserk = 512,
            Weight = 1024,
            NoJokesHere = 2048,
            ShoppingSpree = 4096,
            ExfilLockdown = 8192,

        All = Damage | Blackout | ArmorRepair | Heal | Airdrop | Skill | Metabolism | Malfunction | Trader | Berserk | Weight | NoJokesHere | ShoppingSpree | ExfilLockdown,
        }

        [Flags]
        public enum DoorEvents
        {
            PowerOn = 1,
            DoorUnlock = 2,
            KDoorUnlock = 4,

        All = PowerOn | DoorUnlock | KDoorUnlock,
        }
        
        public static ConfigEntry<bool> CleanBodiesAsap { get; set; }
        public static ConfigEntry<bool> DropBackPack;
        public static ConfigEntry<bool> EnableClean;
        public static ConfigEntry<int> TimeToClean;
        public static ConfigEntry<int> DistToClean;
        public static ConfigEntry<float> DropBackPackChance;

        public static ConfigEntry<bool> Deafness;

        public static ConfigEntry<bool> EnableEvents;
        public static ConfigEntry<bool> EnableDoorEvents;
        public static ConfigEntry<bool> EnableRaidStartEvents;

        public static ConfigEntry<RaidEvents> RandomEventsToEnable;

        public static ConfigEntry<DoorEvents> DoorEventsToEnable;

        public static void BindConfig(ConfigFile cfg)
        {
            #region Core Events

            EnableRaidStartEvents = cfg.Bind(
                "1. Core Events  (Changing Any Of These Options Requires Restart)",
                "Raid Start Events",
                true,
                new ConfigDescription("Dictates whether you allow the Door and Light randomization events to run on raid start or not.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = false, Order = 3 }));

            EnableEvents = cfg.Bind(
                "1. Core Events  (Changing Any Of These Options Requires Restart)",
                "Dynamic Events",
                true,
                new ConfigDescription("Dictates whether the dynamic event timer should increment and allow events to run or not.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = false, Order = 2 }));

            EnableDoorEvents = cfg.Bind(
                "1. Core Events  (Changing Any Of These Options Requires Restart)",
                "Dynamic Door Events",
                true,
                new ConfigDescription("Dictates whether the dynamic event timer should increment and allow door events to run or not.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = false, Order = 1 }));

            #endregion

            #region Random Events

            RandomEventsToEnable = cfg.Bind(
               "2. Random Events",
               "Events List",
                RaidEvents.All,
                new ConfigDescription("Disable/Enable any of the random events that occur throughout your raids.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = false, Order = 1 }));

            #endregion

            #region Door Events

            DoorEventsToEnable = cfg.Bind(
                "3. Door Events",
                "Door Events List",
                DoorEvents.All,
                new ConfigDescription("Disable/Enable any of the door/power switch events that occur throughout your raids.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = false, Order = 1 }));

            #endregion

            #region Body Clean Up

            CleanBodiesAsap = cfg.Bind(
                "4. Body Cleanup Configs",
                "Maid Service",
                true,
                new ConfigDescription("Clean bodies immediately. For when you go on too much of a killing spree.",
                null,
                new ConfigurationManagerAttributes {Order = 4, CustomDrawer = MaidService}));

            EnableClean = cfg.Bind(
                "4. Body Cleanup Configs",
                "Enable Clean",
                true,
                new ConfigDescription("Enable body cleanup event.\nThis requires a restart to take effect after enabling or disabling!",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = false, Order = 3 }));

            TimeToClean = cfg.Bind(
                "4. Body Cleanup Configs",
                "Time to Clean",
                15,
                new ConfigDescription("The time to clean bodies calculated in minutes.",
                new AcceptableValueRange<int>(1, 60),
                new ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = false, Order = 2 }));

            DistToClean = cfg.Bind(
                "4. Body Cleanup Configs",
                "Distance to Clean",
                50,
                new ConfigDescription("How far away bodies should be for cleanup.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = false, Order = 1 }));

            #endregion

            #region Bot Drop

            DropBackPack = cfg.Bind(
                "5. Bot Drop Configs",
                "Drop Backpack",
                true,
                new ConfigDescription("Enable bots dropping backpacks on death.\nThis requires a restart to take effect after enabling or disabling!",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = true, Order = 2 }));

            DropBackPackChance = cfg.Bind(
                "5. Bot Drop Configs",
                "Backpack Drop Chance",
                0.3f,
                new ConfigDescription("Chance of bots dropping a backpack on death.",
                new AcceptableValueRange<float>(0f, 1f),
                new ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = true, Order = 1 }));

            #endregion

            #region Deafness

            Deafness = cfg.Bind(
                "6. Deafness",
                "Enable",
                false,
                new ConfigDescription("Enable deafness changes. Make sure you have your ear protection on.\nThis requires a restart to take effect after enabling or disabling!",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = false, Order = 1 }));

            #endregion
        }

        public static void MaidService(ConfigEntryBase entry)
        {
            bool button = GUILayout.Button("Maid Service", GUILayout.ExpandWidth(true));
            if (button)
            {
                Patches.BodyCleanup.MaidServiceRun();
            }
        }
    }
}