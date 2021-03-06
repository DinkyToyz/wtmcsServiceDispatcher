﻿using System;
using System.IO;

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
        /// Indicates whether development experiments are enabled.
        /// </summary>
        public static readonly bool EnableDevExperiments = FileSystem.Exists(".enable.experiments.dev");

        /// <summary>
        /// Indicates whether experiments are enabled.
        /// </summary>
        public static readonly bool EnableExperiments = FileSystem.Exists(".enable.experiments");

        /// <summary>
        /// The ambulance dispatcher.
        /// </summary>
        public static Dispatcher AmbulanceDispatcher = null;

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
        public static uint CurrentFrame = 0u;

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
            Global.GarbageTruckDispatcher = null;
            Global.HearseDispatcher = null;
            Global.AmbulanceDispatcher = null;
            Global.ServiceBuildingInfoPriorityComparer = null;
            Global.TargetBuildingInfoPriorityComparer = null;
            Global.Buildings = null;
            Global.Vehicles = null;
            Global.ServiceProblems = null;
        }

        /// <summary>
        /// Dumps the data.
        /// </summary>
        /// <param name="objectNamePlural">The object name plural.</param>
        /// <param name="openDumpedFile">if set to <c>true</c> open dumped file after creation.</param>
        /// <param name="requireLoadedLevel">if set to <c>true</c> require a loaded level before dumping.</param>
        /// <param name="lister">The object lister.</param>
        public static void DumpData(string objectNamePlural, bool openDumpedFile, bool requireLoadedLevel, Func<string[]> lister)
        {
            if (requireLoadedLevel && !LevelLoaded)
            {
                return;
            }

            bool logNames = Log.LogNames;
            Log.LogNames = true;

            try
            {
                string[] lines = lister();

                if (lines != null)
                {
                    DumpData(objectNamePlural, openDumpedFile, lines);
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Global), "DumpData", ex, objectNamePlural, openDumpedFile, requireLoadedLevel);
            }
            finally
            {
                Log.LogNames = logNames;
            }
        }

        /// <summary>
        /// Dumps the data.
        /// </summary>
        /// <param name="objectNamePlural">The object name in plural.</param>
        /// <param name="openDumpedFile">if set to <c>true</c> open dumped file after creation.</param>
        /// <param name="lines">The lines.</param>
        /// <exception cref="InvalidDataException">No objects</exception>
        public static void DumpData(string objectNamePlural, bool openDumpedFile, string[] lines)
        {
            try
            {
                if (lines.Length == 0)
                {
                    throw new InvalidDataException("No objects");
                }

                string filePathName = FileSystem.FilePathName("." + objectNamePlural + ".txt");

                using (StreamWriter dumpFile = new StreamWriter(filePathName, false))
                {
                    dumpFile.Write(String.Join("\n", lines).ConformNewlines());
                    dumpFile.WriteLine();
                    dumpFile.Close();
                }

                if (openDumpedFile)
                {
                    FileSystem.OpenFile(filePathName);
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Global), "DumpData", ex, objectNamePlural, openDumpedFile);
            }
        }

        /// <summary>
        /// Gets the service settings.
        /// </summary>
        /// <param name="dispatcherType">Type of the dispatcher.</param>
        /// <returns>The service settings.</returns>
        public static StandardServiceSettings GetServiceSettings(Dispatcher.DispatcherTypes dispatcherType)
        {
            switch (dispatcherType)
            {
                case Dispatcher.DispatcherTypes.GarbageTruckDispatcher:
                    return Global.Settings.Garbage;

                case Dispatcher.DispatcherTypes.HearseDispatcher:
                    return Global.Settings.DeathCare;

                case Dispatcher.DispatcherTypes.AmbulanceDispatcher:
                    return Global.Settings.HealthCare;

                case Dispatcher.DispatcherTypes.None:
                    throw new ArgumentNullException("Dispathcher type 'None' can not have settings");

                default:
                    throw new ArgumentException("No settings for dispatcher type: " + dispatcherType.ToString());
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

                    // Initialize ambulance objects.
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

                BuildingUpdateNeeded = true;
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

                        if (Global.Buildings != null)
                        {
                            Global.Buildings.DebugListLogBuildings();
                        }

                        if (Global.Vehicles != null)
                        {
                            Global.Vehicles.DebugListLogVehicles();
                        }

                        TransferManagerHelper.DebugListLog();
                        flush = true;
                    }

                    if (Global.ServiceProblems != null)
                    {
                        Global.ServiceProblems.DebugListLogServiceProblems();
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