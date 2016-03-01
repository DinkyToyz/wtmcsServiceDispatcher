using System;

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
        public static uint ClassCheckInterval = 240u;

        /// <summary>
        /// The transfer offers needs cleaning.
        /// </summary>
        public static bool TransferOffersCleaningNeeded = false;

        /// <summary>
        /// The clean transfer offers delay.
        /// </summary>
        public static uint CleanTransferOffersDelay = 120u;

        /// <summary>
        /// The current frame.
        /// </summary>
        public static uint CurrentFrame = 0u;

        /// <summary>
        /// The demand update delay.
        /// </summary>
        public static uint DemandLingerDelay = 0u;

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
        /// The delay before unused vehicles are recalled to service building.
        /// </summary>
        public static uint RecallDelay = 240u;

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
        /// The elapsed game simulation time.
        /// </summary>
        public static double SimulationTime = 0.0;

        /// <summary>
        /// The target building information priority comparer.
        /// </summary>
        public static TargetBuildingInfo.PriorityComparer TargetBuildingInfoPriorityComparer = null;

        /// <summary>
        /// The target de-assign delay.
        /// </summary>
        public static uint TargetLingerDelay = 480u;

        /// <summary>
        /// The wait path stuck delay.
        /// </summary>
        public static uint WaitPathStuckDelay = 600;

        /// <summary>
        /// The vehicles.
        /// </summary>
        public static VehicleKeeper Vehicles = null;

        /// <summary>
        /// De-initializes the mod.
        /// </summary>
        public static void DeInitialize()
        {
            DeInitializeHelpers();
        }

        /// <summary>
        /// De-initializes the helpers.
        /// </summary>
        public static void DeInitializeHelpers()
        {
            BulldozeHelper.DeInitialize();
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
        /// Initializes the mod for use.
        /// </summary>
        public static void Initialize()
        {
            InitializeSettings();
            ReInitializeHandlers();
            InitializeHelpers();
            SimulationTime = 0.0;
        }

        /// <summary>
        /// Initializes the helpers.
        /// </summary>
        public static void InitializeHelpers()
        {
            VehicleHelper.Initialize();
            BulldozeHelper.Initialize();
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
    }
}