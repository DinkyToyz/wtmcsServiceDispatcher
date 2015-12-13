﻿using System;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Global objects.
    /// </summary>
    internal static class Global
    {
        /// <summary>
        /// Whether updates should be framed or complete.
        /// </summary>
        public static bool BucketedUpdates = true;

        /// <summary>
        /// The buildings.
        /// </summary>
        public static BuildingKeeper Buildings = null;

        /// <summary>
        /// A global building update is needed.
        /// </summary>
        public static bool BuildingUpdateNeeded = false;

        /// <summary>
        /// The minimum capacity update interval.
        /// </summary>
        public static uint CapacityUpdateInterval = 30u;

        /// <summary>
        /// The minimum class check interval.
        /// </summary>
        public static uint ClassCheckInterval = 240;

        /// <summary>
        /// The current frame.
        /// </summary>
        public static uint CurrentFrame = 0;

        /// <summary>
        /// The demand update delay.
        /// </summary>
        public static uint DemandLingerDelay = 0u;

        /// <summary>
        /// The target de-assign delay.
        /// </summary>
        public static uint TargetLingerDelay = 480u;

        /// <summary>
        /// A detour reinitialize is needed.
        /// </summary>
        public static bool DetourInitNeeded = true;

        /// <summary>
        /// The GarbageTruckAI.TryCollectGarbage detours.
        /// </summary>
        public static GarbageTruckAITryCollectGarbageDetour GarbageTruckAITryCollectGarbageDetour = null;

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
        /// The minimum object update interval.
        /// </summary>
        public static uint ObjectUpdateInterval = 120u;

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
        /// Adds a garbage truck class to detours.
        /// </summary>
        /// <param name="originalClass">The original class.</param>
        public static void AddGarbageTruckClass(Type originalClass)
        {
            GarbageTruckAITryCollectGarbageDetour.AddClass(originalClass);
        }

        /// <summary>
        /// Disposes all detours.
        /// </summary>
        public static void DisposeDetours()
        {
            if (GarbageTruckAITryCollectGarbageDetour != null)
            {
                GarbageTruckAITryCollectGarbageDetour.Dispose();
            }
        }

        /// <summary>
        /// Disposes the dispatchers.
        /// </summary>
        public static void DisposeHandlers()
        {
            Global.GarbageTruckDispatcher = null;
            Global.HearseDispatcher = null;
            Global.ServiceBuildingInfoPriorityComparer = null;
            Global.TargetBuildingInfoPriorityComparer = null;
            Global.Buildings = null;
            Global.Vehicles = null;
        }

        /// <summary>
        /// Initializes the method detours.
        /// </summary>
        public static void InitializeDetours()
        {
            if (Settings.DispatchGarbageTrucks && Settings.LimitOpportunisticGarbageCollection)
            {
                GarbageTruckAITryCollectGarbageDetour.Detour();
            }
            else
            {
                GarbageTruckAITryCollectGarbageDetour.Revert();
            }

            DetourInitNeeded = false;
        }

        /// <summary>
        /// Initializes the dispatchers.
        /// </summary>
        public static void ReInitializeHandlers()
        {
            // Initialize dispatch objects.
            if (Settings.DispatchHearses || Settings.DispatchGarbageTrucks)
            {
                // Initialize buildings.
                if (Buildings == null)
                {
                    Buildings = new BuildingKeeper();
                }
                else
                {
                    Buildings.ReInitialize();
                }

                if (TargetBuildingInfoPriorityComparer == null)
                {
                    TargetBuildingInfoPriorityComparer = new TargetBuildingInfo.PriorityComparer();
                }
                else
                {
                    TargetBuildingInfoPriorityComparer.ReInitialize();
                }

                if (ServiceBuildingInfoPriorityComparer == null)
                {
                    ServiceBuildingInfoPriorityComparer = new ServiceBuildingInfo.PriorityComparer();
                }
                else
                {
                    ServiceBuildingInfoPriorityComparer.ReInitialize();
                }

                // Initialize hearse objects.
                if (Settings.DispatchHearses)
                {
                    if (HearseDispatcher == null)
                    {
                        HearseDispatcher = new Dispatcher(Dispatcher.DispatcherTypes.HearseDispatcher);
                    }
                    else
                    {
                        HearseDispatcher.ReInitialize();
                    }
                }

                // Initialize garbage truck objects.
                if (Settings.DispatchGarbageTrucks)
                {
                    if (GarbageTruckDispatcher == null)
                    {
                        GarbageTruckDispatcher = new Dispatcher(Dispatcher.DispatcherTypes.GarbageTruckDispatcher);
                    }
                    else
                    {
                        GarbageTruckDispatcher.ReInitialize();
                    }
                }
            }

            // Initialize vehicle objects.
            if (Settings.DispatchHearses || Settings.DispatchGarbageTrucks || Settings.RemoveHearsesFromGrid /* || Settings.RemoveGarbageTrucksFromGrid */)
            {
                if (Vehicles == null)
                {
                    Vehicles = new VehicleKeeper();
                }
                else
                {
                    Vehicles.ReInitialize();
                }
            }

            BuildingUpdateNeeded = true;
        }

        /// <summary>
        /// Initializes the settings.
        /// </summary>
        public static void InitializeSettings()
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

        /// <summary>
        /// Reverts all detours.
        /// </summary>
        public static void RevertDetours()
        {
            if (GarbageTruckAITryCollectGarbageDetour != null)
            {
                GarbageTruckAITryCollectGarbageDetour.Revert();
            }
        }
    }
}