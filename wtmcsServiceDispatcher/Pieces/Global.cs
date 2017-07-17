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
        public const bool BucketedUpdates = true;

        /// <summary>
        /// The minimum capacity update interval.
        /// </summary>
        public const uint CapacityUpdateInterval = 30u;

        /// <summary>
        /// The wait path stuck delay.
        /// </summary>
        public const uint CheckFlagStuckDelay = 600u;

        /// <summary>
        /// The minimum class check interval.
        /// </summary>
        public const uint ClassCheckInterval = 240u;

        /// <summary>
        /// The clean transfer offers delay.
        /// </summary>
        public const uint CleanTransferOffersDelay = 120u;

        /// <summary>
        /// The de-assign confused delay.
        /// </summary>
        public const uint DeAssignConfusedDelay = 240u;

        /// <summary>
        /// The debug list log delay.
        /// </summary>
        public const uint DebugListLogDelay = 1800u;

        /// <summary>
        /// The demand update delay.
        /// </summary>
        public const uint DemandLingerDelay = 0u;

        /// <summary>
        /// The log flush delay.
        /// </summary>
        public const uint LogFlushDelay = 600u;

        /// <summary>
        /// The minimum object update interval.
        /// </summary>
        public const uint ObjectUpdateInterval = 120u;

        /// <summary>
        /// The problem linger delay.
        /// </summary>
        public const uint ProblemLingerDelay = 240u;

        /// <summary>
        /// The problem cleaning delay.
        /// </summary>
        public const uint ProblemUpdateDelay = 960u;

        /// <summary>
        /// The minimum recheck interval for handled targets.
        /// </summary>
        public const uint RecheckHandledInterval = 240u;

        /// <summary>
        /// The minimum recheck interval for targets.
        /// </summary>
        public const uint RecheckInterval = 60u;

        /// <summary>
        /// The target de-assign delay.
        /// </summary>
        public const uint TargetLingerDelay = 240u;

        /// <summary>
        /// The buildings.
        /// </summary>
        public static BuildingKeeper Buildings = null;

        /// <summary>
        /// A global building update is needed.
        /// </summary>
        public static bool BuildingUpdateNeeded = false;

        /// <summary>
        /// A global vehicle update is needed.
        /// </summary>
        public static bool VehicleUpdateNeeded = false;

        /// <summary>
        /// The current frame.
        /// </summary>
        public static uint CurrentFrame = 0u;

        /// <summary>
        /// The dispatch services.
        /// </summary>
        public static DispatchServiceKeeper Services = null;

        /// <summary>
        /// Indicates whether development experiments are enabled.
        /// </summary>
        public static bool EnableDevExperiments = FileSystem.Exists(".enable.experiments.dev");

        /// <summary>
        /// Indicates whether experiments are enabled.
        /// </summary>
        public static bool EnableExperiments = FileSystem.Exists(".enable.experiments");

        /// <summary>
        /// A level is loaded.
        /// </summary>
        public static bool LevelLoaded = false;

        /// <summary>
        /// The service building information priority comparer.
        /// </summary>
        public static ServiceBuildingInfo.PriorityComparer ServiceBuildingInfoPriorityComparer = null;

        /// <summary>
        /// The service problems.
        /// </summary>
        public static ServiceProblemKeeper ServiceProblems = null;

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
        /// The transfer offers needs cleaning.
        /// </summary>
        public static bool TransferOffersCleaningNeeded = false;

        /// <summary>
        /// The vehicles.
        /// </summary>
        public static VehicleKeeper Vehicles = null;

        /// <summary>
        /// De-initializes the mod.
        /// </summary>
        public static void DeInitialize()
        {
            LogDebugLists(false, true);

            DeInitializeHelpers();
        }

        /// <summary>
        /// De-initializes the helpers.
        /// </summary>
        public static void DeInitializeHelpers()
        {
            TransferManagerHelper.DeInitialize();
            BulldozeHelper.DeInitialize();
        }

        /// <summary>
        /// Disposes the dispatchers.
        /// </summary>
        public static void DisposeHandlers()
        {
            ServiceBuildingInfoPriorityComparer = null;
            TargetBuildingInfoPriorityComparer = null;
            Buildings = null;
            Vehicles = null;
            ServiceProblems = null;
            Services = null;
        }

        /// <summary>
        /// Gets the service settings.
        /// </summary>
        /// <param name="serviceType">Type of the dispatcher.</param>
        /// <returns>The service settings.</returns>
        public static StandardServiceSettings GetServiceSettings(ServiceHelper.ServiceType serviceType)
        {
            switch (serviceType)
            {
                case ServiceHelper.ServiceType.GarbageTruckDispatcher:
                    return Settings.Garbage;

                case ServiceHelper.ServiceType.HearseDispatcher:
                    return Settings.DeathCare;

                case ServiceHelper.ServiceType.AmbulanceDispatcher:
                    return Settings.HealthCare;

                case ServiceHelper.ServiceType.None:
                    throw new ArgumentNullException("Dispathcher type 'None' can not have settings");

                default:
                    throw new ArgumentException("No settings for dispatcher type: " + serviceType.ToString());
            }
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
            LogDebugLists(true, false);
        }

        /// <summary>
        /// Initializes the helpers.
        /// </summary>
        public static void InitializeHelpers()
        {
            TransferManagerHelper.DeInitialize();
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

                    if (Settings.LoadedVersion < ServiceDispatcherSettings.CurrentVersion)
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
        /// Logs the debug lists.
        /// </summary>
        public static void LogDebugLists()
        {
            LogDebugLists(false, false);
        }

        /// <summary>
        /// Initializes the dispatchers.
        /// </summary>
        public static void ReInitializeHandlers()
        {
            // Initialize dispatch objects.
            try
            {
                if (Settings.DispatchAnyVehicles || Settings.AutoEmptyAnyBuildings)
                {
                    if (Services == null)
                    {
                        Services = new DispatchServiceKeeper();
                    }
                    else
                    {
                        Services.ReInitialize();
                    }
                }
                else
                {
                    Services = null;
                }

                if (Settings.DispatchAnyVehicles || Settings.AutoEmptyAnyBuildings || Settings.WreckingCrews.DispatchVehicles)
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
                }
                else
                {
                    Buildings = null;
                }

                if (Settings.DispatchAnyVehicles)
                {
                    if (TargetBuildingInfoPriorityComparer == null)
                    {
                        TargetBuildingInfoPriorityComparer = new TargetBuildingInfo.PriorityComparer();
                    }

                    if (ServiceBuildingInfoPriorityComparer == null)
                    {
                        ServiceBuildingInfoPriorityComparer = new ServiceBuildingInfo.PriorityComparer();
                    }

                    // Initialize problem keeper.
                    if (ServiceProblems == null)
                    {
                        ServiceProblems = new ServiceProblemKeeper();
                    }
                    else
                    {
                        ServiceProblems.ReInitialize();
                    }
                }
                else
                {
                    ServiceProblems = null;
                    TargetBuildingInfoPriorityComparer = null;
                    ServiceBuildingInfoPriorityComparer = null;
                }

                // Initialize vehicle objects.
                if (Settings.DispatchAnyVehicles || Settings.DeathCare.RemoveFromGrid || Settings.HealthCare.RemoveFromGrid || Settings.RecoveryCrews.DispatchVehicles)
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
                else
                {
                    Vehicles = null;
                }

                BuildingUpdateNeeded = true;
                VehicleUpdateNeeded = true;
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Global), "ReInitializeHandlers", ex);
            }
        }

        /// <summary>
        /// Logs the debug lists.
        /// </summary>
        /// <param name="initializing">if set to <c>true</c> level is loading.</param>
        /// <param name="deInitializing">if set to <c>true</c> level is unloading.</param>
        private static void LogDebugLists(bool initializing, bool deInitializing)
        {
            try
            {
                bool flush = false;

                if (initializing)
                {
                    if (Log.LogDebugLists)
                    {
                        Log.Debug(typeof(Global), "LogDebugLists", "Initializing");
                    }
                }
                else if (deInitializing)
                {
                    if (Log.LogDebugLists)
                    {
                        Log.Debug(typeof(Global), "LogDebugLists", "DeInitializing");
                    }
                }
                else if (CurrentFrame == 0)
                {
                    if (Log.LogDebugLists)
                    {
                        Log.Debug(typeof(Global), "LogDebugLists", "Started");

                        Detours.LogInfo();
                        TransferManagerHelper.LogInfo();
                        VehicleHelper.DebugListLog();
                        BuildingHelper.DebugListLog();
                        TransferManagerHelper.DebugListLog();

                        flush = true;
                    }
                }
                else if (CurrentFrame > 0)
                {
                    if (Log.LogDebugLists)
                    {
                        Log.Debug(typeof(Global), "LogDebugLists", "Running");

                        if (Services != null)
                        {
                            Services.DebugListLogBuildings();
                        }

                        if (Buildings != null)
                        {
                            Buildings.DebugListLogBuildings();
                        }

                        if (Vehicles != null)
                        {
                            Vehicles.DebugListLogVehicles();
                        }

                        TransferManagerHelper.DebugListLog();
                        flush = true;
                    }

                    if (ServiceProblems != null)
                    {
                        ServiceProblems.DebugListLogServiceProblems();
                        flush = true;
                    }
                }

                if (flush)
                {
                    Log.FlushBuffer();
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Global), "LogDebugLists", ex);
            }
        }
    }
}