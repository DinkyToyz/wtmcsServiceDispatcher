using System.IO;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WhatThe.Mods.CitiesSkylines.ServiceDispatcher;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.SerializableSettings
{
    /// <summary>
    /// Serializable settings class.
    /// </summary>
    [Serializable]
    class Version0
    {
        /// <summary>
        /// The default assignment compatibility mode. 
        /// </summary>
        [NonSerialized]
        public const ServiceDispatcherSettings.ModCompatibilityMode DefaultAssignmentCompatibilityMode = ServiceDispatcherSettings.ModCompatibilityMode.UseInstanciatedClassMethods;

        /// <summary>
        /// The default creation compatibility mode.
        /// </summary>
        [NonSerialized]
        public const ServiceDispatcherSettings.ModCompatibilityMode DefaultCreationCompatibilityMode = ServiceDispatcherSettings.ModCompatibilityMode.UseInstanciatedClassMethods;

        /// <summary>
        /// The SetTarget call compatibility mode.
        /// </summary>
        public ServiceDispatcherSettings.ModCompatibilityMode AssignmentCompatibilityMode = DefaultAssignmentCompatibilityMode;

        /// <summary>
        /// Automatic bulldoze of abandoned buildings.
        /// </summary>
        public bool AutoBulldozeBuildings = false;

        /// <summary>
        /// The automatic bulldoze buildings delay.
        /// </summary>
        public double AutoBulldozeBuildingsDelaySeconds = 5.0 * 60.0;

        /// <summary>
        /// Automatic cemetery emptying.
        /// </summary>
        public bool AutoEmptyCemeteries = false;

        /// <summary>
        /// The automatic empty cemetery start level percent.
        /// </summary>
        public uint AutoEmptyCemeteryStartLevelPercent = 95u;

        /// <summary>
        /// The automatic empty cemetery stop level percent.
        /// </summary>
        public uint AutoEmptyCemeteryStopLevelPercent = 5u;

        /// <summary>
        /// Automatic landfill emptying.
        /// </summary>
        public bool AutoEmptyLandfills = false;

        /// <summary>
        /// The automatic empty landfill start level percent.
        /// </summary>
        public uint AutoEmptyLandfillStartLevelPercent = 95u;

        /// <summary>
        /// The automatic empty landfill stop level percent.
        /// </summary>
        public uint AutoEmptyLandfillStopLevelPercent = 5u;

        /// <summary>
        /// Whether transfer manager offer blocking is allowed or not.
        /// </summary>
        public bool BlockTransferManagerOffers = true;

        /// <summary>
        /// The possible building checks.
        /// </summary>
        public ServiceDispatcherSettings.BuildingCheckParameters[] BuildingChecksPossible = null;

        /// <summary>
        /// The possible building checks presets.
        /// </summary>
        public ServiceDispatcherSettings.BuildingChecksPresetInfo[] BuildingChecksPresets = null;

        /// <summary>
        /// When to create spare ambulances.
        /// </summary>
        public ServiceDispatcherSettings.SpareVehiclesCreation CreateSpareAmbulances = ServiceDispatcherSettings.SpareVehiclesCreation.WhenBuildingIsCloser;

        /// <summary>
        /// When to create spare garbage trucks.
        /// </summary>
        public ServiceDispatcherSettings.SpareVehiclesCreation CreateSpareGarbageTrucks = ServiceDispatcherSettings.SpareVehiclesCreation.WhenBuildingIsCloser;

        /// <summary>
        /// When to create spare hearses.
        /// </summary>
        public ServiceDispatcherSettings.SpareVehiclesCreation CreateSpareHearses = ServiceDispatcherSettings.SpareVehiclesCreation.WhenBuildingIsCloser;

        /// <summary>
        /// The CreateVehicle call compatibility mode.
        /// </summary>
        public ServiceDispatcherSettings.ModCompatibilityMode CreationCompatibilityMode = DefaultCreationCompatibilityMode;

        /// <summary>
        /// The current dead people building checks.
        /// </summary>
        public ServiceDispatcherSettings.BuildingCheckParameters[] DeathChecksCurrent = null;

        /// <summary>
        /// The custom hearse building checks.
        /// </summary>
        public ServiceDispatcherSettings.BuildingCheckParameters[] DeathChecksCustom = null;

        /// <summary>
        /// The dead people building checks presets.
        /// </summary>
        public ServiceDispatcherSettings.BuildingCheckOrder DeathChecksPreset = ServiceDispatcherSettings.BuildingCheckOrder.InRange;

        /// <summary>
        /// Whether ambulances should be handled or not.
        /// </summary>
        public Boolean DispatchAmbulances = true;

        /// <summary>
        /// Whether ambulances dispatchers should care about districts or not.
        /// </summary>
        public bool DispatchAmbulancesByDistrict = false;

        /// <summary>
        /// Limit ambulances service buildings by range.
        /// </summary>
        public bool DispatchAmbulancesByRange = true;

        /// <summary>
        /// Whether the dispatch should be limited by district or not.
        /// </summary>
        public Boolean DispatchByDistrict = false;

        /// <summary>
        /// Limit service building range for target buildings without problems.
        /// </summary>
        public bool DispatchByRange = false;

        /// <summary>
        /// Whether garbage trucks should be handled or not.
        /// </summary>
        public Boolean DispatchGarbageTrucks = true;

        /// <summary>
        /// Whether garbage truck dispatchers should care about districts or not.
        /// </summary>
        public bool DispatchGarbageTrucksByDistrict = false;

        /// <summary>
        /// Limit garbage service buildings by range.
        /// </summary>
        public bool DispatchGarbageTrucksByRange = true;

        /// <summary>
        /// Whether hearses should be handled or not.
        /// </summary>
        public Boolean DispatchHearses = true;

        /// <summary>
        /// Whether hearse dispatchers should care about districts or not.
        /// </summary>
        public bool DispatchHearsesByDistrict = false;

        /// <summary>
        /// Limit hearse service buildings by range.
        /// </summary>
        public bool DispatchHearsesByRange = true;

        /// <summary>
        /// The current garbage building checks.
        /// </summary>
        public ServiceDispatcherSettings.BuildingCheckParameters[] GarbageChecksCurrent = null;

        /// <summary>
        /// The custom garbage building checks.
        /// </summary>
        public ServiceDispatcherSettings.BuildingCheckParameters[] GarbageChecksCustom = null;

        /// <summary>
        /// The dirty building checks presets.
        /// </summary>
        public ServiceDispatcherSettings.BuildingCheckOrder GarbageChecksPreset = ServiceDispatcherSettings.BuildingCheckOrder.InRange;

        /// <summary>
        /// Limit too the closest service buildings when igoring range for hearses.
        /// </summary>
        public byte IgnoreRangeUseClosestDeathCareBuilding = 0;

        /// <summary>
        /// Limit too the closest service buildings when igoring range fro garbage trucks.
        /// </summary>
        public byte IgnoreRangeUseClosestGarbageBuilding = 0;

        /// <summary>
        /// Limit too the closest service buildings when igoring range ambulances.
        /// </summary>
        public byte IgnoreRangeUseClosestHealthCareBuilding = 0;

        /// <summary>
        /// Limit opportunistic garbage collection.
        /// </summary>
        public bool LimitOpportunisticGarbageCollection = true;

        /// <summary>
        /// The minimum amount of garbage to dispatch a truck for.
        /// </summary>
        public ushort MinimumGarbageForDispatch = 1500;

        /// <summary>
        /// The minimum amount of garbage to direct a patrolling truck for.
        /// </summary>
        public ushort MinimumGarbageForPatrol = 200;

        /// <summary>
        /// Limit building ranges.
        /// </summary>
        public bool RangeLimit = true;

        /// <summary>
        /// The maximum range (when limiting building ranges).
        /// </summary>
        public float RangeMaximum = 10000000;

        /// <summary>
        /// The minimum range (when limiting building ranges).
        /// </summary>
        public float RangeMinimum = 10000;

        /// <summary>
        /// The range modifier.
        /// </summary>
        public float RangeModifier = 1.0f;

        /// <summary>
        /// Whether code overrides are allowed or not.
        /// </summary>
        public ServiceDispatcherSettings.Allowance ReflectionAllowance = ServiceDispatcherSettings.Allowance.Default;

        /// <summary>
        /// Whether stopped ambulances should be removed from grid or not.
        /// </summary>
        public bool RemoveAmbulancesFromGrid = false;

        /// <summary>
        /// Whether stopped garbage trucks should be removed from grid or not.
        /// </summary>
        public bool RemoveGarbageTrucksFromGrid = false;

        /// <summary>
        /// Whether stopped hearses should be removed from grid or not.
        /// </summary>
        public bool RemoveHearsesFromGrid = false;

        /// <summary>
        /// Automatic removal of stuck vehicles.
        /// </summary>
        public bool RemoveStuckVehicles = false;

        /// <summary>
        /// The automatic vehicle removal delay.
        /// </summary>
        public double RemoveStuckVehiclesDelaySeconds = 5.0 * 60.0;

        /// <summary>
        /// The save count.
        /// </summary>
        public uint SaveCount = 0;

        /// <summary>
        /// The current sick people building checks.
        /// </summary>
        public ServiceDispatcherSettings.BuildingCheckParameters[] SickChecksCurrent = null;

        /// <summary>
        /// The custom ambulance building checks.
        /// </summary>
        public ServiceDispatcherSettings.BuildingCheckParameters[] SickChecksCustom = null;

        /// <summary>
        /// The sick people building checks presets.
        /// </summary>
        public ServiceDispatcherSettings.BuildingCheckOrder SickChecksPreset = ServiceDispatcherSettings.BuildingCheckOrder.InRange;

        /// <summary>
        /// The settings version.
        /// </summary>
        public int Version = 0;

        /// <summary>
        /// Loads settings from the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The settings.</returns>
        public static Settings Load(string fileName)
        {
            Log.Debug(typeof(Version0), "Load", "Begin");

            try
            {
                if (File.Exists(fileName))
                {
                    Log.Info(typeof(Settings), "Load", fileName);

                    using (FileStream file = File.OpenRead(fileName))
                    {
                        XmlSerializer ser = new XmlSerializer(typeof(Version0), ServiceDispatcherSettings.XmlRoot);
                        ServiceDispatcherSettings cfg = ser.Deserialize(file) as ServiceDispatcherSettings;
                        if (cfg != null)
                        {
                            Log.Debug(typeof(Settings), "Load", "Loaded");

                            if (cfg.Version < 4)
                            {
                                cfg.AssignmentCompatibilityMode = ServiceDispatcherSettings.DefaultAssignmentCompatibilityMode;
                                cfg.CreationCompatibilityMode = ServiceDispatcherSettings.DefaultCreationCompatibilityMode;

                                if (cfg.Version < 3)
                                {
                                    if (cfg.Version < 2)
                                    {
                                        cfg.DispatchHearsesByDistrict = cfg.DispatchByDistrict;
                                        cfg.DispatchHearsesByRange = cfg.DispatchByRange;
                                        cfg.DispatchGarbageTrucksByDistrict = cfg.DispatchByDistrict;
                                        cfg.DispatchGarbageTrucksByRange = cfg.DispatchByRange;
                                        cfg.DispatchAmbulancesByDistrict = cfg.DispatchByDistrict;
                                        cfg.DispatchAmbulancesByRange = cfg.DispatchByRange;
                                    }

                                    if (cfg.MinimumGarbageForDispatch >= 2000)
                                    {
                                        cfg.MinimumGarbageForPatrol = 200;
                                    }
                                    else if (cfg.MinimumGarbageForDispatch >= 300)
                                    {
                                        cfg.MinimumGarbageForPatrol = 150;
                                    }
                                    else if (cfg.MinimumGarbageForDispatch >= 100)
                                    {
                                        cfg.MinimumGarbageForPatrol = 100;
                                    }
                                    else
                                    {
                                        cfg.MinimumGarbageForPatrol = cfg.MinimumGarbageForDispatch;
                                    }
                                }
                            }

                            Settings sets = new Settings(cfg);

                            Log.Debug(typeof(Settings), "Load", "End");

                            return sets;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Settings), "Load", ex);
            }

            Log.Debug(typeof(Settings), "Load", "End");
            return new Settings();
        }

        /// <summary>
        /// Saves settings to the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="settings">The settings.</param>
        public static void Save(string fileName, Settings settings)
        {
            Log.Debug(typeof(Version0), "Save", "Begin");

            if (Log.LogALot || Library.IsDebugBuild)
            {
                settings.LogSettings();
            }

            try
            {
                string filePath = Path.GetDirectoryName(Path.GetFullPath(fileName));
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                Log.Info(typeof(Version0), "Save", fileName);

                using (FileStream file = File.Create(fileName))
                {
                    ServiceDispatcherSettings cfg = new ServiceDispatcherSettings();

                    cfg.DispatchByDistrict = settings.DeathCare.DispatchByDistrict && settings.Garbage.DispatchByDistrict && settings.HealthCare.DispatchByDistrict;
                    cfg.DispatchByRange = settings.DeathCare.DispatchByRange || settings.Garbage.DispatchByRange || settings.HealthCare.DispatchByRange;
                    cfg.RangeModifier = settings.RangeModifier;
                    cfg.RangeLimit = settings.RangeLimit;
                    cfg.RangeMaximum = settings.RangeMaximum;
                    cfg.RangeMinimum = settings.RangeMinimum;
                    cfg.ReflectionAllowance = settings.ReflectionAllowance;
                    cfg.BlockTransferManagerOffers = settings.BlockTransferManagerOffers;
                    cfg.AssignmentCompatibilityMode = settings.AssignmentCompatibilityMode;
                    cfg.CreationCompatibilityMode = settings.CreationCompatibilityMode;

                    cfg.DispatchHearses = settings.DeathCare.DispatchVehicles;
                    cfg.DispatchHearsesByDistrict = settings.DeathCare.DispatchByDistrict;
                    cfg.DispatchHearsesByRange = settings.DeathCare.DispatchByRange;
                    cfg.RemoveHearsesFromGrid = settings.DeathCare.RemoveFromGrid;
                    cfg.CreateSpareHearses = settings.DeathCare.CreateSpares;
                    cfg.DeathChecksPreset = settings.DeathCare.ChecksPreset;
                    cfg.DeathChecksCustom = settings.DeathCare.ChecksCustom;
                    cfg.DeathChecksCurrent = settings.DeathCare.ChecksParameters;
                    cfg.IgnoreRangeUseClosestDeathCareBuilding = settings.DeathCare.IgnoreRangeUseClosestBuildings;
                    cfg.AutoEmptyCemeteries = settings.DeathCare.AutoEmpty;
                    cfg.AutoEmptyCemeteryStartLevelPercent = settings.DeathCare.AutoEmptyStartLevelPercent;
                    cfg.AutoEmptyCemeteryStopLevelPercent = settings.DeathCare.AutoEmptyStopLevelPercent;

                    cfg.DispatchAmbulances = settings.HealthCare.DispatchVehicles;
                    cfg.DispatchAmbulancesByDistrict = settings.HealthCare.DispatchByDistrict;
                    cfg.DispatchAmbulancesByRange = settings.HealthCare.DispatchByRange;
                    cfg.RemoveAmbulancesFromGrid = settings.HealthCare.RemoveFromGrid;
                    cfg.CreateSpareAmbulances = settings.HealthCare.CreateSpares;
                    cfg.SickChecksPreset = settings.HealthCare.ChecksPreset;
                    cfg.SickChecksCustom = settings.HealthCare.ChecksCustom;
                    cfg.SickChecksCurrent = settings.HealthCare.ChecksParameters;
                    cfg.IgnoreRangeUseClosestHealthCareBuilding = settings.HealthCare.IgnoreRangeUseClosestBuildings;

                    cfg.DispatchGarbageTrucks = settings.Garbage.DispatchVehicles;
                    cfg.DispatchGarbageTrucksByDistrict = settings.Garbage.DispatchByDistrict;
                    cfg.DispatchGarbageTrucksByRange = settings.Garbage.DispatchByRange;
                    cfg.LimitOpportunisticGarbageCollection = settings.Garbage.LimitOpportunisticCollection;
                    cfg.CreateSpareGarbageTrucks = settings.Garbage.CreateSpares;
                    cfg.MinimumGarbageForDispatch = settings.Garbage.MinimumAmountForDispatch;
                    cfg.MinimumGarbageForPatrol = settings.Garbage.MinimumAmountForPatrol;
                    cfg.GarbageChecksPreset = settings.Garbage.ChecksPreset;
                    cfg.GarbageChecksCustom = settings.Garbage.ChecksCustom;
                    cfg.GarbageChecksCurrent = settings.Garbage.ChecksParameters;
                    cfg.IgnoreRangeUseClosestGarbageBuilding = settings.Garbage.IgnoreRangeUseClosestBuildings;
                    cfg.AutoEmptyLandfills = settings.Garbage.AutoEmpty;
                    cfg.AutoEmptyLandfillStartLevelPercent = settings.Garbage.AutoEmptyStartLevelPercent;
                    cfg.AutoEmptyLandfillStopLevelPercent = settings.Garbage.AutoEmptyStopLevelPercent;

                    cfg.AutoBulldozeBuildings = settings.WreckingCrews.DispatchVehicles;
                    cfg.AutoBulldozeBuildingsDelaySeconds = settings.WreckingCrews.DelaySeconds;

                    cfg.RemoveStuckVehicles = settings.RecoveryCrews.DispatchVehicles;
                    cfg.RemoveStuckVehiclesDelaySeconds = settings.RecoveryCrews.DelaySeconds;

                    cfg.BuildingChecksPresets = (Enum.GetValues(typeof(ServiceDispatcherSettings.BuildingCheckOrder)) as ServiceDispatcherSettings.BuildingCheckOrder[]).Where(bco => bco != ServiceDispatcherSettings.BuildingCheckOrder.Custom).Select(bco => new ServiceDispatcherSettings.BuildingChecksPresetInfo(bco)).ToArray();
                    cfg.BuildingChecksPossible = (Enum.GetValues(typeof(ServiceDispatcherSettings.BuildingCheckParameters)) as ServiceDispatcherSettings.BuildingCheckParameters[]).Where(bcp => bcp != ServiceDispatcherSettings.BuildingCheckParameters.Undefined).ToArray();

                    cfg.Version = settings.Version;
                    cfg.SaveCount = settings.SaveCount;

                    XmlSerializer ser = new XmlSerializer(typeof(Version0), ServiceDispatcherSettings.XmlRoot);
                    ser.Serialize(file, cfg);
                    file.Flush();
                    file.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Version0), "Save", ex);
            }

            Log.Debug(typeof(Version0), "Save", "End");
        }
    }
}
