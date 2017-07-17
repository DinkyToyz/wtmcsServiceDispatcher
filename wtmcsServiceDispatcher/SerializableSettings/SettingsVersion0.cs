using System;
using System.Linq;
using WhatThe.Mods.CitiesSkylines.ServiceDispatcher;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.SerializableSettings
{
    /// <summary>
    /// Serializable settings class, version 0-5.
    /// </summary>
    [Serializable]
    public class SettingsVersion0 : ISerializableSettings
    {
        /// <summary>
        /// The SetTarget call compatibility mode.
        /// </summary>
        public ServiceDispatcherSettings.ModCompatibilityMode AssignmentCompatibilityMode = ServiceDispatcherSettings.DefaultAssignmentCompatibilityMode;

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
        public ServiceDispatcherSettings.ModCompatibilityMode CreationCompatibilityMode = ServiceDispatcherSettings.DefaultCreationCompatibilityMode;

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
        /// Gets the current version.
        /// </summary>
        /// <value>
        /// The current version.
        /// </value>
        public static int CurrentVersion => 5;

        /// <summary>
        /// Gets the loaded version.
        /// </summary>
        /// <value>
        /// The loaded version.
        /// </value>
        public int LoadedVersion => this.Version;

        /// <summary>
        /// Gets the maximum version.
        /// </summary>
        /// <value>
        /// The maximum version.
        /// </value>
        public int MaxVersion => 5;

        /// <summary>
        /// Gets the minimum version.
        /// </summary>
        /// <value>
        /// The minimum version.
        /// </value>
        public int MinVersion => 0;

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <returns>
        /// The settings.
        /// </returns>
        public Settings GetSettings()
        {
            Settings settings = new Settings(false);

            settings.loadedVersion = this.Version;
            settings.SaveCount = this.SaveCount;

            settings.RangeModifier = this.RangeModifier;
            settings.RangeLimit = this.RangeLimit;
            settings.RangeMaximum = this.RangeMaximum;
            settings.RangeMinimum = this.RangeMinimum;
            settings.ReflectionAllowance = this.ReflectionAllowance;
            settings.BlockTransferManagerOffers = this.BlockTransferManagerOffers;
            settings.AssignmentCompatibilityMode = this.AssignmentCompatibilityMode;
            settings.CreationCompatibilityMode = this.CreationCompatibilityMode;

            settings.DeathCare.DispatchVehicles = this.DispatchHearses;
            settings.DeathCare.DispatchByDistrict = this.DispatchHearsesByDistrict;
            settings.DeathCare.DispatchByRange = this.DispatchHearsesByRange;
            settings.DeathCare.CreateSpares = this.CreateSpareHearses;
            settings.DeathCare.RemoveFromGrid = this.RemoveHearsesFromGrid;
            settings.DeathCare.ChecksCustom = this.DeathChecksCustom;
            settings.DeathCare.ChecksPreset = this.DeathChecksPreset;
            settings.DeathCare.IgnoreRangeUseClosestBuildings = this.IgnoreRangeUseClosestDeathCareBuilding;
            settings.DeathCare.AutoEmpty = this.AutoEmptyCemeteries;
            settings.DeathCare.AutoEmptyStartLevelPercent = this.AutoEmptyCemeteryStartLevelPercent;
            settings.DeathCare.AutoEmptyStopLevelPercent = this.AutoEmptyCemeteryStopLevelPercent;

            settings.HealthCare.DispatchVehicles = this.DispatchAmbulances;
            settings.HealthCare.DispatchByDistrict = this.DispatchAmbulancesByDistrict;
            settings.HealthCare.DispatchByRange = this.DispatchAmbulancesByRange;
            settings.HealthCare.CreateSpares = this.CreateSpareAmbulances;
            settings.HealthCare.RemoveFromGrid = this.RemoveAmbulancesFromGrid;
            settings.HealthCare.ChecksCustom = this.SickChecksCustom;
            settings.HealthCare.ChecksPreset = this.SickChecksPreset;
            settings.HealthCare.IgnoreRangeUseClosestBuildings = this.IgnoreRangeUseClosestHealthCareBuilding;

            settings.Garbage.DispatchVehicles = this.DispatchGarbageTrucks;
            settings.Garbage.DispatchByDistrict = this.DispatchGarbageTrucksByDistrict;
            settings.Garbage.DispatchByRange = this.DispatchGarbageTrucksByRange;
            settings.Garbage.LimitOpportunisticCollection = this.LimitOpportunisticGarbageCollection;
            settings.Garbage.CreateSpares = this.CreateSpareGarbageTrucks;
            settings.Garbage.MinimumAmountForDispatch = this.MinimumGarbageForDispatch;
            settings.Garbage.MinimumAmountForPatrol = this.MinimumGarbageForPatrol;
            settings.Garbage.ChecksCustom = this.GarbageChecksCustom;
            settings.Garbage.ChecksPreset = this.GarbageChecksPreset;
            settings.Garbage.IgnoreRangeUseClosestBuildings = this.IgnoreRangeUseClosestGarbageBuilding;
            settings.Garbage.AutoEmpty = this.AutoEmptyLandfills;
            settings.Garbage.AutoEmptyStartLevelPercent = this.AutoEmptyLandfillStartLevelPercent;
            settings.Garbage.AutoEmptyStopLevelPercent = this.AutoEmptyLandfillStopLevelPercent;

            settings.WreckingCrews.DispatchVehicles = this.AutoBulldozeBuildings;
            settings.WreckingCrews.DelaySeconds = this.AutoBulldozeBuildingsDelaySeconds;

            settings.RecoveryCrews.DispatchVehicles = this.RemoveStuckVehicles;
            settings.RecoveryCrews.DelaySeconds = this.RemoveStuckVehiclesDelaySeconds;

            settings.Initialize();

            return settings;
        }

        private ServiceDispatcherSettings.SpareVehiclesCreation FixBrokenSpareVehiclesCreation(ServiceDispatcherSettings.SpareVehiclesCreation loadedSpareVehiclesCreation)
        {
            // Make the best of it, as we can't really make it right...
            switch(loadedSpareVehiclesCreation)
            {
                case ServiceDispatcherSettings.SpareVehiclesCreation.Never: return ServiceDispatcherSettings.SpareVehiclesCreation.WhenNoFree;
                case ServiceDispatcherSettings.SpareVehiclesCreation.WhenNoFree: return ServiceDispatcherSettings.SpareVehiclesCreation.WhenBuildingIsCloser;
                case ServiceDispatcherSettings.SpareVehiclesCreation.WhenBuildingIsCloser: return ServiceDispatcherSettings.SpareVehiclesCreation.WhenBuildingIsCloser;
                default: return ServiceDispatcherSettings.SpareVehiclesCreation.WhenNoFree;
            }
        }

        /// <summary>
        /// Initializes the settings.
        /// </summary>
        public void Initialize()
        {
            if (this.Version < 5)
            {
                this.CreateSpareAmbulances = FixBrokenSpareVehiclesCreation(this.CreateSpareAmbulances);
                this.CreateSpareGarbageTrucks = FixBrokenSpareVehiclesCreation(this.CreateSpareGarbageTrucks);
                this.CreateSpareHearses = FixBrokenSpareVehiclesCreation(this.CreateSpareHearses);

                if (this.Version < 4)
                {
                    this.AssignmentCompatibilityMode = ServiceDispatcherSettings.DefaultAssignmentCompatibilityMode;
                    this.CreationCompatibilityMode = ServiceDispatcherSettings.DefaultCreationCompatibilityMode;

                    if (this.Version < 3)
                    {
                        if (this.Version < 2)
                        {
                            this.DispatchHearsesByDistrict = this.DispatchByDistrict;
                            this.DispatchHearsesByRange = this.DispatchByRange;
                            this.DispatchGarbageTrucksByDistrict = this.DispatchByDistrict;
                            this.DispatchGarbageTrucksByRange = this.DispatchByRange;
                            this.DispatchAmbulancesByDistrict = this.DispatchByDistrict;
                            this.DispatchAmbulancesByRange = this.DispatchByRange;
                        }

                        if (this.MinimumGarbageForDispatch >= 2000)
                        {
                            this.MinimumGarbageForPatrol = 200;
                        }
                        else if (this.MinimumGarbageForDispatch >= 300)
                        {
                            this.MinimumGarbageForPatrol = 150;
                        }
                        else if (this.MinimumGarbageForDispatch >= 100)
                        {
                            this.MinimumGarbageForPatrol = 100;
                        }
                        else
                        {
                            this.MinimumGarbageForPatrol = this.MinimumGarbageForDispatch;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public void SetSettings(Settings settings)
        {
            this.DispatchByDistrict = settings.DeathCare.DispatchByDistrict && settings.Garbage.DispatchByDistrict && settings.HealthCare.DispatchByDistrict;
            this.DispatchByRange = settings.DeathCare.DispatchByRange || settings.Garbage.DispatchByRange || settings.HealthCare.DispatchByRange;
            this.RangeModifier = settings.RangeModifier;
            this.RangeLimit = settings.RangeLimit;
            this.RangeMaximum = settings.RangeMaximum;
            this.RangeMinimum = settings.RangeMinimum;
            this.ReflectionAllowance = settings.ReflectionAllowance;
            this.BlockTransferManagerOffers = settings.BlockTransferManagerOffers;
            this.AssignmentCompatibilityMode = settings.AssignmentCompatibilityMode;
            this.CreationCompatibilityMode = settings.CreationCompatibilityMode;

            this.DispatchHearses = settings.DeathCare.DispatchVehicles;
            this.DispatchHearsesByDistrict = settings.DeathCare.DispatchByDistrict;
            this.DispatchHearsesByRange = settings.DeathCare.DispatchByRange;
            this.RemoveHearsesFromGrid = settings.DeathCare.RemoveFromGrid;
            this.CreateSpareHearses = settings.DeathCare.CreateSpares;
            this.DeathChecksPreset = settings.DeathCare.ChecksPreset;
            this.DeathChecksCustom = settings.DeathCare.ChecksCustom;
            this.DeathChecksCurrent = settings.DeathCare.ChecksParameters;
            this.IgnoreRangeUseClosestDeathCareBuilding = settings.DeathCare.IgnoreRangeUseClosestBuildings;
            this.AutoEmptyCemeteries = settings.DeathCare.AutoEmpty;
            this.AutoEmptyCemeteryStartLevelPercent = settings.DeathCare.AutoEmptyStartLevelPercent;
            this.AutoEmptyCemeteryStopLevelPercent = settings.DeathCare.AutoEmptyStopLevelPercent;

            this.DispatchAmbulances = settings.HealthCare.DispatchVehicles;
            this.DispatchAmbulancesByDistrict = settings.HealthCare.DispatchByDistrict;
            this.DispatchAmbulancesByRange = settings.HealthCare.DispatchByRange;
            this.RemoveAmbulancesFromGrid = settings.HealthCare.RemoveFromGrid;
            this.CreateSpareAmbulances = settings.HealthCare.CreateSpares;
            this.SickChecksPreset = settings.HealthCare.ChecksPreset;
            this.SickChecksCustom = settings.HealthCare.ChecksCustom;
            this.SickChecksCurrent = settings.HealthCare.ChecksParameters;
            this.IgnoreRangeUseClosestHealthCareBuilding = settings.HealthCare.IgnoreRangeUseClosestBuildings;

            this.DispatchGarbageTrucks = settings.Garbage.DispatchVehicles;
            this.DispatchGarbageTrucksByDistrict = settings.Garbage.DispatchByDistrict;
            this.DispatchGarbageTrucksByRange = settings.Garbage.DispatchByRange;
            this.LimitOpportunisticGarbageCollection = settings.Garbage.LimitOpportunisticCollection;
            this.CreateSpareGarbageTrucks = settings.Garbage.CreateSpares;
            this.MinimumGarbageForDispatch = settings.Garbage.MinimumAmountForDispatch;
            this.MinimumGarbageForPatrol = settings.Garbage.MinimumAmountForPatrol;
            this.GarbageChecksPreset = settings.Garbage.ChecksPreset;
            this.GarbageChecksCustom = settings.Garbage.ChecksCustom;
            this.GarbageChecksCurrent = settings.Garbage.ChecksParameters;
            this.IgnoreRangeUseClosestGarbageBuilding = settings.Garbage.IgnoreRangeUseClosestBuildings;
            this.AutoEmptyLandfills = settings.Garbage.AutoEmpty;
            this.AutoEmptyLandfillStartLevelPercent = settings.Garbage.AutoEmptyStartLevelPercent;
            this.AutoEmptyLandfillStopLevelPercent = settings.Garbage.AutoEmptyStopLevelPercent;

            this.AutoBulldozeBuildings = settings.WreckingCrews.DispatchVehicles;
            this.AutoBulldozeBuildingsDelaySeconds = settings.WreckingCrews.DelaySeconds;

            this.RemoveStuckVehicles = settings.RecoveryCrews.DispatchVehicles;
            this.RemoveStuckVehiclesDelaySeconds = settings.RecoveryCrews.DelaySeconds;

            this.BuildingChecksPresets = (Enum.GetValues(typeof(ServiceDispatcherSettings.BuildingCheckOrder)) as ServiceDispatcherSettings.BuildingCheckOrder[]).WhereSelectToArray(bco => bco != ServiceDispatcherSettings.BuildingCheckOrder.Custom, bco => new ServiceDispatcherSettings.BuildingChecksPresetInfo(bco));
            this.BuildingChecksPossible = (Enum.GetValues(typeof(ServiceDispatcherSettings.BuildingCheckParameters)) as ServiceDispatcherSettings.BuildingCheckParameters[]).WhereToArray(bcp => bcp != ServiceDispatcherSettings.BuildingCheckParameters.Undefined);

            this.Version = CurrentVersion;
            this.SaveCount = settings.SaveCount;
        }
    }
}