﻿using System;

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
        public static BuildingKeeper Buildings = null;

        /// <summary>
        /// A global building update is needed.
        /// </summary>
        public static bool BuildingUpdateNeeded = false;

        /// <summary>
        /// The current frame.
        /// </summary>
        public static uint CurrentFrame = 0;

        /// <summary>
        /// Whether updates should be framed or complete.
        /// </summary>
        public static bool FramedUpdates = true;

        /// <summary>
        /// The garbage truck dispatcher.
        /// </summary>
        public static Dispatcher GarbageTruckDispatcher = null;

        /// <summary>
        /// The hearse dispatcher.
        /// </summary>
        public static Dispatcher HearseDispatcher = null;

        /// <summary>
        /// A level is loaded.
        /// </summary>
        public static bool LevelLoaded = false;

        /// <summary>
        /// The method detours.
        /// </summary>
        public static MethodDetours MethodDetours = null;

        /// <summary>
        /// The minimum object update interval.
        /// </summary>
        public static uint ObjectUpdateInterval = 120;

        /// <summary>
        /// The minimum recheck interval for handled targets.
        /// </summary>
        public static uint RecheckHandledInterval = 240u;

        /// <summary>
        /// The minimum recheck interval for targets.
        /// </summary>
        public static uint RecheckInterval = 60u;

        /// <summary>
        /// The service building information priority comparer.
        /// </summary>
        public static ServiceBuildingInfo.PriorityComparer ServiceBuildingInfoPriorityComparer = null;

        /// <summary>
        /// The settings.
        /// </summary>
        public static Settings Settings = null;

        /// <summary>
        /// The target building information priority comparer.
        /// </summary>
        public static TargetBuildingInfo.PriorityComparer TargetBuildingInfoPriorityComparer = null;

        /// <summary>
        /// The vehicles.
        /// </summary>
        public static VehicleKeeper Vehicles = null;

        /// <summary>
        /// Initializes the dispatchers.
        /// </summary>
        public static void InitHandlers()
        {
            // Initialize dispatch objects.
            if (Settings.DispatchHearses || Settings.DispatchGarbageTrucks)
            {
                if (Buildings == null)
                {
                    Buildings = new BuildingKeeper();
                }

                if (TargetBuildingInfoPriorityComparer == null)
                {
                    TargetBuildingInfoPriorityComparer = new TargetBuildingInfo.PriorityComparer();
                }

                if (ServiceBuildingInfoPriorityComparer == null)
                {
                    ServiceBuildingInfoPriorityComparer = new ServiceBuildingInfo.PriorityComparer();
                }

                // Initialize hearse objects.
                if (Settings.DispatchHearses && HearseDispatcher == null)
                {
                    HearseDispatcher = new Dispatcher(Dispatcher.DispatcherTypes.HearseDispatcher);
                }

                // Initialize garbage truck objects.
                if (Settings.DispatchGarbageTrucks)
                {
                    if (GarbageTruckDispatcher == null)
                    {
                        GarbageTruckDispatcher = new Dispatcher(Dispatcher.DispatcherTypes.GarbageTruckDispatcher);
                    }

                    if (Settings.LimitOpportunisticGarbageCollection)
                    {
                        MethodDetours.Detour_GarbageTruckAI_TryCollectGarbage();
                    }
                    else
                    {
                        MethodDetours.Revert_GarbageTruckAI_TryCollectGarbage();
                    }
                }
                else
                {
                    MethodDetours.Revert_GarbageTruckAI_TryCollectGarbage();
                }
            }
            else
            {
                MethodDetours.Revert();
            }

            // Initialize vehicle objects.
            if ((Settings.DispatchHearses || Settings.DispatchGarbageTrucks || Settings.RemoveHearsesFromGrid /* || Settings.RemoveGarbageTrucksFromGrid */) && Vehicles == null)
            {
                Vehicles = new VehicleKeeper();
            }
        }

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
