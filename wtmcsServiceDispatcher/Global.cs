using System;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Global objects.
    /// </summary>
    internal static class Global
    {
        /// <summary>
        /// The buildings.
        /// </summary>
        public static Buildings Buildings = null;

        /// <summary>
        /// The current frame.
        /// </summary>
        public static uint CurrentFrame = 0;

        /// <summary>
        /// Force mod targets.
        /// </summary>
        public static bool ForceTarget = true;

        /// <summary>
        /// Wether updates should be framed or complete.
        /// </summary>
        public static bool FramedUpdates = false;

        /// <summary>
        /// The garbage truck dispatcher.
        /// </summary>
        public static GarbageTruckDispatcher GarbageTruckDispatcher = null;

        /// <summary>
        /// The hearse dispatcher.
        /// </summary>
        public static HearseDispatcher HearseDispatcher = null;

        /// <summary>
        /// A level is loaded.
        /// </summary>
        public static bool LevelLoaded = false;

        /// <summary>
        /// The minimum object update interval.
        /// </summary>
        public static uint ObjectUpdateInterval = 120;

        /// <summary>
        /// Pretend to handle services (find vehicles for target, but don't actually assign them).
        /// </summary>
        public static bool PretendToHandleStuff = false;

        /// <summary>
        /// The minimum recheck interval for handled targets.
        /// </summary>
        public static uint RecheckHandledInterval = PretendToHandleStuff ? 2400u : 240u;

        /// <summary>
        /// The minimum recheck interval for targets.
        /// </summary>
        public static uint RecheckInterval = PretendToHandleStuff ? 600u : 60u;

        /// <summary>
        /// The service building information priority comparer.
        /// </summary>
        public static Buildings.ServiceBuildingInfo.PriorityComparer ServiceBuildingInfoPriorityComparer = null;

        /// <summary>
        /// The settings.
        /// </summary>
        public static Settings Settings = null;

        /// <summary>
        /// The target building information priority comparer.
        /// </summary>
        public static Buildings.TargetBuildingInfo.PriorityComparer TargetBuildingInfoPriorityComparer = null;

        /// <summary>
        /// The vehicles.
        /// </summary>
        public static Vehicles Vehicles = null;

        /// <summary>
        /// Initializes the settings.
        /// </summary>
        public static void InitSettings()
        {
            if (Settings == null)
            {
                try
                {
                    try
                    {
                        Settings = Settings.Load();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(typeof(Global), "InitSettings", ex);
                        Settings = new Settings();
                    }

                    Settings.LogSettings();

                    if (Settings.LoadedVersion < Settings.Version)
                    {
                        Settings.Save();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(typeof(Global), "InitSettings", ex);
                }
            }
        }
    }
}