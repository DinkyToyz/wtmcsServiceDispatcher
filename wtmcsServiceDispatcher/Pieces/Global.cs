using System;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Global objects.
    /// </summary>
    internal static class Global
    {
        /// <summary>
        /// The ambulance dispatcher.
        /// </summary>
        public static Dispatcher AmbulanceDispatcher = null;

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
        /// The wait path stuck delay.
        /// </summary>
        public static uint CheckFlagStuckDelay = 600u;

        /// <summary>
        /// The minimum class check interval.
        /// </summary>
        public static uint ClassCheckInterval = 240u;

        /// <summary>
        /// The clean transfer offers delay.
        /// </summary>
        public static uint CleanTransferOffersDelay = 120u;

        /// <summary>
        /// The current frame.
        /// </summary>
        public static uint CurrentFrame = 0u;

        /// <summary>
        /// The de-assign confused delay.
        /// </summary>
        public static uint DeAssignConfusedDelay = 240u;

        /// <summary>
        /// The demand update delay.
        /// </summary>
        public static uint DemandLingerDelay = 0u;

        /// <summary>
        /// Indicates whether development experiments are enabled.
        /// </summary>
        public static bool EnableDevExperiments = FileSystem.Exists(".enable.experiments.dev");

        /// <summary>
        /// Indicates whether experiments are enabled.
        /// </summary>
        public static bool EnableExperiments = FileSystem.Exists(".enable.experiments");

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
        /// The problem linger delay.
        /// </summary>
        public static uint ProblemLingerDelay = 3600u;

        /// <summary>
        /// The problems.
        /// </summary>
        public static ProblemKeeper Problems = null;

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
        public static uint TargetLingerDelay = 240u;

        /// <summary>
        /// The transfer offers needs cleaning.
        /// </summary>
        public static bool TransferOffersCleaningNeeded = false;

        /// <summary>
        /// The vehicles.
        /// </summary>
        public static VehicleKeeper Vehicles = null;

        /// <summary>
        /// Gets a value indicating whether to clean ambulance service offers.
        /// </summary>
        /// <value>
        ///   <c>True</c> if ambulance service offers should be cleaned; otherwise, <c>false</c>.
        /// </value>
        public static bool CleanAmbulanceTransferOffers
        {
            get
            {
                return Global.Settings.HealthCare.DispatchVehicles && Global.Settings.HealthCare.CreateSpares != ServiceDispatcherSettings.SpareVehiclesCreation.Never;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to clean garbage truck service offers.
        /// </summary>
        /// <value>
        ///   <c>True</c> if garbage truck service offers should be cleaned; otherwise, <c>false</c>.
        /// </value>
        public static bool CleanGarbageTruckTransferOffers
        {
            get
            {
                return Global.Settings.Garbage.DispatchVehicles && Global.Settings.Garbage.CreateSpares != ServiceDispatcherSettings.SpareVehiclesCreation.Never;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to clean hearse service offers.
        /// </summary>
        /// <value>
        ///   <c>True</c> if hearse service offers should be cleaned; otherwise, <c>false</c>.
        /// </value>
        public static bool CleanHearseTransferOffers
        {
            get
            {
                return Global.Settings.DeathCare.DispatchVehicles && Global.Settings.DeathCare.CreateSpares != ServiceDispatcherSettings.SpareVehiclesCreation.Never;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to clean service offers.
        /// </summary>
        /// <value>
        ///   <c>True</c> if service offers should be cleaned; otherwise, <c>false</c>.
        /// </value>
        public static bool CleanTransferOffers
        {
            get
            {
                return (Global.Settings.DeathCare.DispatchVehicles && Global.Settings.DeathCare.CreateSpares != ServiceDispatcherSettings.SpareVehiclesCreation.Never) ||
                       (Global.Settings.Garbage.DispatchVehicles && Global.Settings.Garbage.CreateSpares != ServiceDispatcherSettings.SpareVehiclesCreation.Never) ||
                       (Global.Settings.HealthCare.DispatchVehicles && Global.Settings.HealthCare.CreateSpares != ServiceDispatcherSettings.SpareVehiclesCreation.Never);
            }
        }

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
            Global.AmbulanceDispatcher = null;
            Global.ServiceBuildingInfoPriorityComparer = null;
            Global.TargetBuildingInfoPriorityComparer = null;
            Global.Buildings = null;
            Global.Vehicles = null;
            Global.Problems = null;
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
        /// Re-initializes the ambulance dispatcher.
        /// </summary>
        public static void ReInitializeAmbulanceDispatcher()
        {
            try
            {
                AmbulanceDispatcher.ReInitialize();
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Global), "ReInitializeAmbulanceDispatcher", ex);
            }
        }

        /// <summary>
        /// Re-initializes the garbage truck dispatcher.
        /// </summary>
        public static void ReInitializeGarbageTruckDispatcher()
        {
            try
            {
                GarbageTruckDispatcher.ReInitialize();
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Global), "ReInitializeGarbageTruckDispatcher", ex);
            }
        }

        /// <summary>
        /// Initializes the dispatchers.
        /// </summary>
        public static void ReInitializeHandlers()
        {
            // Initialize dispatch objects.
            try
            {
                if (Settings.DeathCare.DispatchVehicles || Settings.Garbage.DispatchVehicles || Settings.HealthCare.DispatchVehicles)
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

                    if (Problems == null)
                    {
                        Problems = new ProblemKeeper();
                    }
                    else
                    {
                        Problems.ReInitialize();
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
                    if (Settings.DeathCare.DispatchVehicles)
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
                    if (Settings.Garbage.DispatchVehicles)
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

                    // Initialize hearse objects.
                    if (Settings.HealthCare.DispatchVehicles)
                    {
                        if (AmbulanceDispatcher == null)
                        {
                            AmbulanceDispatcher = new Dispatcher(Dispatcher.DispatcherTypes.AmbulanceDispatcher);
                        }
                        else
                        {
                            AmbulanceDispatcher.ReInitialize();
                        }
                    }
                }

                // Initialize vehicle objects.
                if (Settings.DeathCare.DispatchVehicles || Settings.Garbage.DispatchVehicles || Settings.HealthCare.DispatchVehicles ||
                    Settings.DeathCare.RemoveFromGrid || Settings.HealthCare.RemoveFromGrid)
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
            catch (Exception ex)
            {
                Log.Error(typeof(Global), "ReInitializeHandlers", ex);
            }
        }

        /// <summary>
        /// Re-initializes the hearse dispatcher.
        /// </summary>
        public static void ReInitializeHearseDispatcher()
        {
            try
            {
                HearseDispatcher.ReInitialize();
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Global), "ReInitializeHearseDispatcher", ex);
            }
        }
    }
}