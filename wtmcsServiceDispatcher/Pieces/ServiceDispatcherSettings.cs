using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Serializable settings class.
    /// </summary>
    [Serializable]
    public class ServiceDispatcherSettings
    {
        /// <summary>
        /// Gets the XML root.
        /// </summary>
        /// <value>
        /// The XML root.
        /// </value>
        public static XmlRootAttribute XmlRoot => new XmlRootAttribute("ServiceDispatcherSettings");

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
        public BuildingCheckParameters[] BuildingChecksPossible = null;

        /// <summary>
        /// The possible building checks presets.
        /// </summary>
        public BuildingChecksPresetInfo[] BuildingChecksPresets = null;

        /// <summary>
        /// When to create spare ambulances.
        /// </summary>
        public SpareVehiclesCreation CreateSpareAmbulances = SpareVehiclesCreation.WhenBuildingIsCloser;

        /// <summary>
        /// When to create spare garbage trucks.
        /// </summary>
        public SpareVehiclesCreation CreateSpareGarbageTrucks = SpareVehiclesCreation.WhenBuildingIsCloser;

        /// <summary>
        /// When to create spare hearses.
        /// </summary>
        public SpareVehiclesCreation CreateSpareHearses = SpareVehiclesCreation.WhenBuildingIsCloser;

        /// <summary>
        /// The CreateVehicle call compatibility mode.
        /// </summary>
        public ServiceDispatcherSettings.ModCompatibilityMode CreationCompatibilityMode = DefaultCreationCompatibilityMode;

        /// <summary>
        /// The current dead people building checks.
        /// </summary>
        public BuildingCheckParameters[] DeathChecksCurrent = null;

        /// <summary>
        /// The custom hearse building checks.
        /// </summary>
        public BuildingCheckParameters[] DeathChecksCustom = null;

        /// <summary>
        /// The dead people building checks presets.
        /// </summary>
        public BuildingCheckOrder DeathChecksPreset = BuildingCheckOrder.InRange;

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
        public BuildingCheckParameters[] GarbageChecksCurrent = null;

        /// <summary>
        /// The custom garbage building checks.
        /// </summary>
        public BuildingCheckParameters[] GarbageChecksCustom = null;

        /// <summary>
        /// The dirty building checks presets.
        /// </summary>
        public BuildingCheckOrder GarbageChecksPreset = BuildingCheckOrder.InRange;

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
        public Allowance ReflectionAllowance = Allowance.Default;

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
        public BuildingCheckParameters[] SickChecksCurrent = null;

        /// <summary>
        /// The custom ambulance building checks.
        /// </summary>
        public BuildingCheckParameters[] SickChecksCustom = null;

        /// <summary>
        /// The sick people building checks presets.
        /// </summary>
        public BuildingCheckOrder SickChecksPreset = BuildingCheckOrder.InRange;

        /// <summary>
        /// The settings version.
        /// </summary>
        public int Version = 0;

        /// <summary>
        /// Whether something is allowed or not.
        /// </summary>
        public enum Allowance
        {
            /// <summary>
            /// The default rules applies.
            /// </summary>
            Default = 0,

            /// <summary>
            /// Never allowed.
            /// </summary>
            Never = 1,

            /// <summary>
            /// Always allowed.
            /// </summary>
            Always = 2
        }

        /// <summary>
        /// Order of building checks.
        /// </summary>
        public enum BuildingCheckOrder
        {
            /// <summary>
            /// Custom order.
            /// </summary>
            Custom = 0,

            /// <summary>
            /// 1, in range; 2, forgotten.
            /// </summary>
            InRange = 1,

            /// <summary>
            /// Straight order.
            /// </summary>
            FirstFirst = 2,

            /// <summary>
            /// 1, in range; 2, problematic out of range.
            /// </summary>
            InRangeFirst = 3,

            /// <summary>
            /// 1, problematic in range; 2, problematic; 3, in range.
            /// </summary>
            ProblematicFirst = 4,

            /// <summary>
            /// 1, very problematic in range; 2, very problematic; 3, in range; 4, problematic out of range.
            /// </summary>
            VeryProblematicFirst = 5,

            /// <summary>
            /// 1, forgotten in range; 2, forgotten out of range; 3, in range; 4, problematic out of range.
            /// </summary>
            ForgottenFirst = 6
        }

        /// <summary>
        /// Building check parameters.
        /// </summary>
        public enum BuildingCheckParameters
        {
            /// <summary>
            /// Custom parameters.
            /// </summary>
            Undefined = 0,

            /// <summary>
            /// Any buildings.
            /// </summary>
            Any = 1,

            /// <summary>
            /// Buildings in range.
            /// </summary>
            InRange = 2,

            /// <summary>
            /// Problematic buildings in range.
            /// </summary>
            ProblematicInRange = 3,

            /// <summary>
            /// Problematic buildings in or out of range.
            /// </summary>
            ProblematicIgnoreRange = 4,

            /// <summary>
            /// Problematic buildings in range.
            /// </summary>
            VeryProblematicInRange = 5,

            /// <summary>
            /// Problematic buildings in or out of range.
            /// </summary>
            VeryProblematicIgnoreRange = 6,

            /// <summary>
            /// Forgotten buildings in range.
            /// </summary>
            ForgottenInRange = 7,

            /// <summary>
            /// Forgotten buildings in or out of range.
            /// </summary>
            ForgottenIgnoreRange = 8
        }

        /// <summary>
        /// Setting for mod compatibility for object method calls.
        /// </summary>
        public enum ModCompatibilityMode
        {
            /// <summary>
            /// Use CSD custom code instead of object methods.
            /// </summary>
            UseCustomCode = 0,

            /// <summary>
            /// Cast object as original class before calling method.
            /// </summary>
            UseOriginalClassMethods = 1,

            /// <summary>
            /// Call method on instantiated object in the normal way.
            /// </summary>
            UseInstanciatedClassMethods = 2
        }

        /// <summary>
        /// Options for when to create spare vehicles.
        /// </summary>
        public enum SpareVehiclesCreation
        {
            /// <summary>
            /// Never create spare vehicles.
            /// </summary>
            Never = 1,

            /// <summary>
            /// Create spare vehicles when service building has no free vehicles.
            /// </summary>
            WhenNoFree = 2,

            /// <summary>
            /// Create spare vehicles when service building is closer to target than all free vehicles.
            /// </summary>
            WhenBuildingIsCloser = 3
        }

        /// <summary>
        /// Building checks preset information.
        /// </summary>
        public class BuildingChecksPresetInfo
        {
            /// <summary>
            /// The building checks.
            /// </summary>
            public BuildingCheckParameters[] BuildingChecks = null;

            /// <summary>
            /// The description.
            /// </summary>
            public string Description;

            /// <summary>
            /// The identifier.
            /// </summary>
            public BuildingCheckOrder Identifier;

            /// <summary>
            /// The name.
            /// </summary>
            public string Name;

            /// <summary>
            /// Initializes a new instance of the <see cref="BuildingChecksPresetInfo"/> class.
            /// </summary>
            public BuildingChecksPresetInfo()
            {
                this.Identifier = ServiceDispatcherSettings.BuildingCheckOrder.Custom;
                this.BuildingChecks = new BuildingCheckParameters[] { BuildingCheckParameters.Any };
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="BuildingChecksPresetInfo" /> class.
            /// </summary>
            /// <param name="buildingCheckOrder">The building check order preset.</param>
            public BuildingChecksPresetInfo(BuildingCheckOrder buildingCheckOrder)
            {
                this.Identifier = buildingCheckOrder;
                this.Name = Settings.GetBuildingCheckOrderName(buildingCheckOrder);
                this.Description = Settings.GetBuildingCheckOrderDescription(buildingCheckOrder);
                this.BuildingChecks = Settings.GetBuildingChecksParameters(this.Identifier);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="BuildingChecksPresetInfo"/> class.
            /// </summary>
            /// <param name="buildingChecks">The building checks.</param>
            public BuildingChecksPresetInfo(string buildingChecks)
            {
                this.Identifier = ServiceDispatcherSettings.BuildingCheckOrder.Custom;
                this.BuildingChecks = ToArray(buildingChecks);
            }

            /// <summary>
            /// Converts string to building check array.
            /// </summary>
            /// <param name="buildingChecks">The building checks.</param>
            /// <returns>The building checks in an array.</returns>
            public static BuildingCheckParameters[] ToArray(string buildingChecks)
            {
                List<BuildingCheckParameters> checks = new List<BuildingCheckParameters>();

                foreach (string check in buildingChecks.Split(',', ';', ':', '/', ' ', '\t', '\r', '\n'))
                {
                    string checkLow = check.ToLower();
                    foreach (BuildingCheckParameters parameter in Enum.GetValues(typeof(BuildingCheckParameters)))
                    {
                        if (parameter.ToString().ToLower() == checkLow)
                        {
                            checks.Add(parameter);
                        }
                    }
                }

                return checks.ToArray();
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents the specified checks.
            /// </summary>
            /// <param name="buildingChecks">The building checks.</param>
            /// <returns>
            /// A <see cref="System.String" /> that represents the checks.
            /// </returns>
            public static string ToString(BuildingCheckParameters[] buildingChecks)
            {
                return buildingChecks == null
                    ? ""
                    : String.Join(", ", buildingChecks.WhereSelect(c => c != BuildingCheckParameters.Undefined, c => c.ToString()).ToArray());
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return ToString(this.BuildingChecks);
            }
        }
    }
}