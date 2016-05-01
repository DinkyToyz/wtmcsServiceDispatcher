using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Mod settings.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// The settings version.
        /// </summary>
        public readonly int Version = 3;

        /// <summary>
        /// Automatic bulldoze of abandoned buildings.
        /// </summary>
        public bool AutoBulldozeBuildings = false;

        /// <summary>
        /// The automatic bulldoze buildings delay.
        /// </summary>
        public double AutoBulldozeBuildingsDelaySeconds = 3.0 * 60.0;

        /// <summary>
        /// When to create spare garbage trucks.
        /// </summary>
        public SpareVehiclesCreation CreateSpareGarbageTrucks = SpareVehiclesCreation.WhenBuildingIsCloser;

        /// <summary>
        /// When to create spare hearses.
        /// </summary>
        public SpareVehiclesCreation CreateSpareHearses = SpareVehiclesCreation.WhenBuildingIsCloser;

        /// <summary>
        /// Whether garbage trucks should be handled or not.
        /// </summary>
        public bool DispatchGarbageTrucks = true;

        /// <summary>
        /// Whether garbage truck dispatchers should care about districts or not.
        /// </summary>
        public bool DispatchGarbageTrucksByDistrict = false;

        /// <summary>
        /// Limit garbage service building range.
        /// </summary>
        public bool DispatchGarbageTrucksByRange = true;

        /// <summary>
        /// Whether hearses should be handled or not.
        /// </summary>
        public bool DispatchHearses = true;

        /// <summary>
        /// Whether hearse dispatchers should care about districts or not.
        /// </summary>
        public bool DispatchHearsesByDistrict = false;

        /// <summary>
        /// Limit hearse service building by range.
        /// </summary>
        public bool DispatchHearsesByRange = true;

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
        public bool RangeLimit = false;

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
        /// Call back garbage trucks to service buildings when not needed for a while.
        /// </summary>
        public bool RecallGarbageTrucks = true;

        /// <summary>
        /// Call back hearses to service buildings when not needed for a while.
        /// </summary>
        public bool RecallHearses = true;

        /// <summary>
        /// Whether code overrides are allowed or not.
        /// </summary>
        public Allowance ReflectionAllowance = Allowance.Default;

        /// <summary>
        /// Whether stopped hearses should be removed from grid or not.
        /// </summary>
        public bool RemoveHearsesFromGrid = true;

        /// <summary>
        /// Automatic removal of stuck vehicles.
        /// </summary>
        public bool RemoveStuckVehicles = false;

        /// <summary>
        /// The automatic vehicle removal delay.
        /// </summary>
        public double RemoveStuckVehiclesDelaySeconds = 60.0;

        /// <summary>
        /// The save count.
        /// </summary>
        public uint SaveCount = 0;

        /// <summary>
        /// The descriptions for the building checks orders.
        /// </summary>
        private static Dictionary<BuildingCheckOrder, string> buildingCheckOrderDescriptions = new Dictionary<BuildingCheckOrder, string>()
        {
            { BuildingCheckOrder.FirstFirst, "All buldings regardless of range." },
            { BuildingCheckOrder.ForgottenFirst, "Forgotten buildings in range, followed by forgotten buildings out of range, buildings in range and finally problematic buildings in or out of range." },
            { BuildingCheckOrder.InRange, "Buildings in range followed by forgotten buildings out of range." },
            { BuildingCheckOrder.InRangeFirst, "Buildings in range followed by problematic buildings in or out of range." },
            { BuildingCheckOrder.ProblematicFirst, "Problematic buildings in range followed by problematic buildings out of range and finally buildings in range." },
            { BuildingCheckOrder.VeryProblematicFirst, "Very problematic buildings in range followed by very problematic buildings out of range, buildings in range and finally problematic buildings in or out of range." }
        };

        /// <summary>
        /// The display names for the building checks orders.
        /// </summary>
        private static Dictionary<BuildingCheckOrder, string> buildingCheckOrderNames = new Dictionary<BuildingCheckOrder, string>()
        {
            { BuildingCheckOrder.Custom, "Custom" },
            { BuildingCheckOrder.FirstFirst, "First first" },
            { BuildingCheckOrder.ForgottenFirst, "Forgotten first" },
            { BuildingCheckOrder.InRange, "In range" },
            { BuildingCheckOrder.InRangeFirst, "In range first" },
            { BuildingCheckOrder.ProblematicFirst, "Problematic first" },
            { BuildingCheckOrder.VeryProblematicFirst, "Very problematic first" }
        };

        /// <summary>
        /// The descriptions for the vehicle creation options.
        /// </summary>
        private static Dictionary<SpareVehiclesCreation, string> spareVehiclesCreationDescriptions = new Dictionary<SpareVehiclesCreation, string>()
        {
            { SpareVehiclesCreation.Never, "Never send out spare vehicles." },
            { SpareVehiclesCreation.WhenNoFree, "Send out spare vehicles when service building has no free vehicles." },
            { SpareVehiclesCreation.WhenBuildingIsCloser, "Send out spare vehicles when service building is closer than all free vehicles." }
        };

        /// <summary>
        /// The display names for the vehicle creation options.
        /// </summary>
        private static Dictionary<SpareVehiclesCreation, string> spareVehiclesCreationNames = new Dictionary<SpareVehiclesCreation, string>()
        {
            { SpareVehiclesCreation.Never, "Never" },
            { SpareVehiclesCreation.WhenNoFree, "None are free" },
            { SpareVehiclesCreation.WhenBuildingIsCloser, "Building is closer" }
        };

        /// <summary>
        /// The custom building checks.
        /// </summary>
        private BuildingCheckParameters[] deathChecksCustom = null;

        /// <summary>
        /// The dead people building checks presets.
        /// </summary>
        private BuildingCheckOrder deathChecksPreset = BuildingCheckOrder.InRange;

        /// <summary>
        /// The custom building checks.
        /// </summary>
        private BuildingCheckParameters[] garbageChecksCustom = null;

        /// <summary>
        /// The dirty building checks presets.
        /// </summary>
        private BuildingCheckOrder garbageChecksPreset = BuildingCheckOrder.InRange;

        /// <summary>
        /// The settings version in the loaded file.
        /// </summary>
        private int? loadedVersion = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        /// <param name="settings">The file settings.</param>
        public Settings(ServiceDispatcherSettings settings = null)
        {
            if (settings != null)
            {
                this.loadedVersion = settings.Version;

                this.RangeModifier = settings.RangeModifier;
                this.RangeLimit = settings.RangeLimit;
                this.RangeMaximum = settings.RangeMaximum;
                this.RangeMinimum = settings.RangeMinimum;
                this.ReflectionAllowance = settings.ReflectionAllowance;

                this.DispatchHearses = settings.DispatchHearses;
                this.DispatchHearsesByDistrict = settings.DispatchHearsesByDistrict;
                this.DispatchHearsesByRange = settings.DispatchHearsesByRange;
                this.CreateSpareHearses = settings.CreateSpareHearses;
                this.RemoveHearsesFromGrid = settings.RemoveHearsesFromGrid;
                this.deathChecksPreset = settings.DeathChecksPreset;
                this.deathChecksCustom = settings.DeathChecksCustom;
                if (this.deathChecksPreset == BuildingCheckOrder.Custom && (this.deathChecksCustom == null || this.deathChecksCustom.Length == 0))
                {
                    this.deathChecksCustom = GetBuildingChecksParameters(BuildingCheckOrder.InRange);
                }

                this.DispatchGarbageTrucks = settings.DispatchGarbageTrucks;
                this.DispatchGarbageTrucksByDistrict = settings.DispatchGarbageTrucksByDistrict;
                this.DispatchGarbageTrucksByRange = settings.DispatchGarbageTrucksByRange;
                this.LimitOpportunisticGarbageCollection = settings.LimitOpportunisticGarbageCollection;
                this.CreateSpareGarbageTrucks = settings.CreateSpareGarbageTrucks;
                this.MinimumGarbageForDispatch = settings.MinimumGarbageForDispatch;
                this.MinimumGarbageForPatrol = settings.MinimumGarbageForPatrol;
                this.garbageChecksPreset = settings.GarbageChecksPreset;
                this.garbageChecksCustom = settings.GarbageChecksCustom;
                if (this.garbageChecksPreset == BuildingCheckOrder.Custom && (this.garbageChecksCustom == null || this.garbageChecksCustom.Length == 0))
                {
                    this.garbageChecksCustom = GetBuildingChecksParameters(BuildingCheckOrder.InRange);
                }

                this.AutoBulldozeBuildings = settings.AutoBulldozeBuildings;
                this.AutoBulldozeBuildingsDelaySeconds = settings.AutoBulldozeBuildingsDelaySeconds;
            }
        }

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
            Custom = 0,

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
        /// Gets the complete path.
        /// </summary>
        /// <value>
        /// The complete path.
        /// </value>
        public static string FilePathName
        {
            get
            {
                return FileSystem.FilePathName(".xml");
            }
        }

        /// <summary>
        /// Gets or sets the automatic bulldoze buildings delay in minutes.
        /// </summary>
        /// <value>
        /// The automatic bulldoze buildings delay in minutes.
        /// </value>
        public double AutoBulldozeBuildingsDelayMinutes
        {
            get
            {
                return this.AutoBulldozeBuildingsDelaySeconds / 60.0;
            }

            set
            {
                this.AutoBulldozeBuildingsDelaySeconds = (value < 0.0) ? 0.0 : value * 60.0;
            }
        }

        /// <summary>
        /// Gets the dead people building checks parameters.
        /// </summary>
        /// <value>
        /// The building checks parameters.
        /// </value>
        public BuildingCheckParameters[] DeathChecksParameters
        {
            get
            {
                return GetBuildingChecksParameters(this.deathChecksPreset, this.deathChecksCustom);
            }
        }

        /// <summary>
        /// Gets or sets the dead people building checks preset.
        /// </summary>
        /// <value>
        /// The building checks preset.
        /// </value>
        public BuildingCheckOrder DeathChecksPreset
        {
            get
            {
                return this.deathChecksPreset;
            }

            set
            {
                this.deathChecksPreset = value;

                if (value == BuildingCheckOrder.Custom && (this.deathChecksCustom == null || this.deathChecksCustom.Length == 0))
                {
                    this.deathChecksCustom = GetBuildingChecksParameters(BuildingCheckOrder.InRange);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether any dispatchers cares about districts or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if any dispatchers dispatches by district; otherwise, <c>false</c>.
        /// </value>
        public bool DispatchAnyByDistrict
        {
            get
            {
                return this.DispatchHearsesByDistrict || this.DispatchGarbageTrucksByDistrict;
            }
        }

        /// <summary>
        /// Gets a value indicating whether any dispatcher limits service building dispatch by range.
        /// </summary>
        /// <value>
        ///   <c>true</c> if any dispatch dispatches by range; otherwise, <c>false</c>.
        /// </value>
        public bool DispatchAnyByRange
        {
            get
            {
                return this.DispatchHearsesByRange || this.DispatchGarbageTrucksByRange;
            }
        }

        /// <summary>
        /// Gets the garbage building checks parameters.
        /// </summary>
        /// <value>
        /// The building checks parameters.
        /// </value>
        public BuildingCheckParameters[] GarbageChecksParameters
        {
            get
            {
                return GetBuildingChecksParameters(this.garbageChecksPreset, this.garbageChecksCustom);
            }
        }

        /// <summary>
        /// Gets or sets the building checks preset.
        /// </summary>
        /// <value>
        /// The building checks preset.
        /// </value>
        public BuildingCheckOrder GarbageChecksPreset
        {
            get
            {
                return this.garbageChecksPreset;
            }

            set
            {
                this.garbageChecksPreset = value;

                if (value == BuildingCheckOrder.Custom && (this.garbageChecksCustom == null || this.garbageChecksCustom.Length == 0))
                {
                    this.garbageChecksCustom = GetBuildingChecksParameters(BuildingCheckOrder.InRange);
                }
            }
        }

        /// <summary>
        /// Gets the settings version in the loaded file.
        /// </summary>
        /// <value>
        /// The settings version in the loaded file.
        /// </value>
        public int LoadedVersion
        {
            get
            {
                return (this.loadedVersion == null || !this.loadedVersion.HasValue) ? 0 : this.loadedVersion.Value;
            }
        }

        /// <summary>
        /// Gets or sets the automatic vehicle recovery delay in minutes.
        /// </summary>
        /// <value>
        /// The automatic vehicle recovery delay in minutes.
        /// </value>
        public double RemoveStuckVehiclesDelayMinutes
        {
            get
            {
                return this.RemoveStuckVehiclesDelaySeconds / 60.0;
            }

            set
            {
                this.RemoveStuckVehiclesDelaySeconds = (value < 0.0) ? 0.0 : value * 60.0;
            }
        }

        /// <summary>
        /// Gets the name of the allowance.
        /// </summary>
        /// <param name="allowance">The allowance.</param>
        /// <returns>The name of the allowance.</returns>
        public static string GetAllowanceName(Allowance allowance)
        {
            return allowance.ToString();
        }

        /// <summary>
        /// Gets the display name of the building check order.
        /// </summary>
        /// <param name="checkOrder">The check order.</param>
        /// <returns>The display name.</returns>
        public static string GetBuildingCheckOrderDescription(BuildingCheckOrder checkOrder)
        {
            return buildingCheckOrderDescriptions.ContainsKey(checkOrder) ? buildingCheckOrderDescriptions[checkOrder] : null;
        }

        /// <summary>
        /// Gets the display name of the building check order.
        /// </summary>
        /// <param name="checkOrder">The check order.</param>
        /// <returns>The display name.</returns>
        public static string GetBuildingCheckOrderName(BuildingCheckOrder checkOrder)
        {
            return buildingCheckOrderNames.ContainsKey(checkOrder) ? buildingCheckOrderNames[checkOrder] : null;
        }

        /// <summary>
        /// Gets the building checks parameters for the specified check order.
        /// </summary>
        /// <param name="buildingChecks">The building check order.</param>
        /// <param name="customBuildingCheckParameters">The custom building check parameters.</param>
        /// <returns>
        /// The building checks parameters.
        /// </returns>
        public static BuildingCheckParameters[] GetBuildingChecksParameters(BuildingCheckOrder buildingChecks, BuildingCheckParameters[] customBuildingCheckParameters = null)
        {
            switch (buildingChecks)
            {
                case BuildingCheckOrder.FirstFirst:
                    return new BuildingCheckParameters[] { BuildingCheckParameters.Any };

                case BuildingCheckOrder.InRangeFirst:
                    return new BuildingCheckParameters[] { BuildingCheckParameters.InRange, BuildingCheckParameters.ProblematicIgnoreRange };

                case BuildingCheckOrder.ProblematicFirst:
                    return new BuildingCheckParameters[] { BuildingCheckParameters.ProblematicInRange, BuildingCheckParameters.ProblematicIgnoreRange, BuildingCheckParameters.InRange };

                case BuildingCheckOrder.VeryProblematicFirst:
                    return new BuildingCheckParameters[] { BuildingCheckParameters.VeryProblematicInRange, BuildingCheckParameters.VeryProblematicIgnoreRange, BuildingCheckParameters.InRange, BuildingCheckParameters.ProblematicIgnoreRange };

                case BuildingCheckOrder.ForgottenFirst:
                    return new BuildingCheckParameters[] { BuildingCheckParameters.ForgottenInRange, BuildingCheckParameters.ForgottenIgnoreRange, BuildingCheckParameters.InRange, BuildingCheckParameters.ProblematicIgnoreRange };

                case BuildingCheckOrder.InRange:
                    return new BuildingCheckParameters[] { BuildingCheckParameters.InRange, BuildingCheckParameters.ForgottenIgnoreRange };

                case Settings.BuildingCheckOrder.Custom:
                    return (customBuildingCheckParameters != null) ? customBuildingCheckParameters : GetBuildingChecksParameters(BuildingCheckOrder.InRange);

                default:
                    return GetBuildingChecksParameters(BuildingCheckOrder.FirstFirst);
            }
        }

        /// <summary>
        /// Gets the display name of the vehicle creation option.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <returns>
        /// The display name.
        /// </returns>
        public static string GetSpareVehiclesCreationDescription(SpareVehiclesCreation option)
        {
            return spareVehiclesCreationDescriptions.ContainsKey(option) ? spareVehiclesCreationDescriptions[option] : null;
        }

        /// <summary>
        /// Gets the display name of the vehicle creation option.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <returns>
        /// The display name.
        /// </returns>
        public static string GetSpareVehiclesCreationName(SpareVehiclesCreation option)
        {
            return spareVehiclesCreationNames.ContainsKey(option) ? spareVehiclesCreationNames[option] : null;
        }

        /// <summary>
        /// Loads settings from the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The settings.</returns>
        public static Settings Load(string fileName = null)
        {
            Log.Debug(typeof(Settings), "Load", "Begin");

            try
            {
                if (fileName == null)
                {
                    fileName = FilePathName;
                }

                if (File.Exists(fileName))
                {
                    Log.Info(typeof(Settings), "Load", fileName);

                    using (FileStream file = File.OpenRead(fileName))
                    {
                        XmlSerializer ser = new XmlSerializer(typeof(ServiceDispatcherSettings));
                        ServiceDispatcherSettings cfg = ser.Deserialize(file) as ServiceDispatcherSettings;
                        if (cfg != null)
                        {
                            Log.Debug(typeof(Settings), "Load", "Loaded");

                            if (cfg.Version < 3)
                            {
                                if (cfg.Version < 2)
                                {
                                    cfg.DispatchHearsesByDistrict = cfg.DispatchByDistrict;
                                    cfg.DispatchHearsesByRange = cfg.DispatchByRange;
                                    cfg.DispatchGarbageTrucksByDistrict = cfg.DispatchByDistrict;
                                    cfg.DispatchGarbageTrucksByRange = cfg.DispatchByRange;
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
        /// Checks whether to allow reflection or not.
        /// </summary>
        /// <param name="minGameVersion">The minimum game version.</param>
        /// <param name="maxGameVersion">The maximum game version.</param>
        /// <returns>True if use of reflection is allowed.</returns>
        public bool AllowReflection(uint minGameVersion = 0, uint maxGameVersion = 0)
        {
            return this.AllowanceCheck(this.ReflectionAllowance, minGameVersion, maxGameVersion);
        }

        /// <summary>
        /// Logs the settings.
        /// </summary>
        public void LogSettings()
        {
            Log.Debug(this, "LogSettings", "RangeLimit", this.RangeLimit);
            Log.Debug(this, "LogSettings", "RangeModifier", this.RangeModifier);
            Log.Debug(this, "LogSettings", "RangeMinimum", this.RangeMinimum);
            Log.Debug(this, "LogSettings", "RangeMaximum", this.RangeMaximum);

            Log.Debug(this, "LogSettings", "DispatchHearses", this.DispatchHearses);
            Log.Debug(this, "LogSettings", "DispatchHearsesByDistrict", this.DispatchHearsesByDistrict);
            Log.Debug(this, "LogSettings", "DispatchHearsesByRange", this.DispatchHearsesByRange);
            Log.Debug(this, "LogSettings", "RemoveHearsesFromGrid", this.RemoveHearsesFromGrid);
            Log.Debug(this, "LogSettings", "CreateSpareHearses", this.CreateSpareHearses);
            Log.Debug(this, "LogSettings", "DeathChecks", (byte)this.deathChecksPreset, this.deathChecksPreset, GetBuildingCheckOrderName(this.deathChecksPreset));
            Log.Debug(this, "LogSettings", "DeathChecksParameters", String.Join(", ", this.DeathChecksParameters.Select(bc => bc.ToString()).ToArray()));
            if (this.deathChecksCustom != null)
            {
                Log.Debug(this, "LogSettings", "DeathChecksCustom", String.Join(", ", this.deathChecksCustom.Select(bc => bc.ToString()).ToArray()));
            }

            Log.Debug(this, "LogSettings", "DispatchGarbageTrucks", this.DispatchGarbageTrucks);
            Log.Debug(this, "LogSettings", "DispatchGarbageTrucksByDistrict", this.DispatchGarbageTrucksByDistrict);
            Log.Debug(this, "LogSettings", "DispatchGarbageTrucksByRange", this.DispatchGarbageTrucksByRange);
            Log.Debug(this, "LogSettings", "LimitOportunisticGarbageCollection", this.LimitOpportunisticGarbageCollection);
            Log.Debug(this, "LogSettings", "CreateSpareGarbageTrucks", this.CreateSpareGarbageTrucks);
            Log.Debug(this, "LogSettings", "MinimumGarbageForDispatch", this.MinimumGarbageForDispatch);
            Log.Debug(this, "LogSettings", "MinimumGarbageForPatrol", this.MinimumGarbageForPatrol);
            Log.Debug(this, "LogSettings", "GarbageChecks", (byte)this.garbageChecksPreset, this.garbageChecksPreset, GetBuildingCheckOrderName(this.garbageChecksPreset));
            Log.Debug(this, "LogSettings", "GarbageChecksParameters", String.Join(", ", this.GarbageChecksParameters.Select(bc => bc.ToString()).ToArray()));
            if (this.garbageChecksCustom != null)
            {
                Log.Debug(this, "LogSettings", "GarbageChecksCustom", String.Join(", ", this.garbageChecksCustom.Select(bc => bc.ToString()).ToArray()));
            }

            Log.Debug(this, "LogSettings", this.Version, this.LoadedVersion, this.SaveCount);
        }

        /// <summary>
        /// Returns text showing the refection allowance.
        /// </summary>
        /// <param name="minGameVersion">The minimum game version.</param>
        /// <param name="maxGameVersion">The maximum game version.</param>
        /// <returns>The reflection allowance description.</returns>
        public string ReflectionAllowanceText(uint minGameVersion = 0, uint maxGameVersion = 0)
        {
            return this.AllowanceText(this.ReflectionAllowance, minGameVersion, maxGameVersion);
        }

        /// <summary>
        /// Saves settings to the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public void Save(string fileName = null)
        {
            Log.Debug(this, "Save", "Begin");

            if (Log.LogALot || Library.IsDebugBuild)
            {
                this.LogSettings();
            }

            try
            {
                if (fileName == null)
                {
                    fileName = FilePathName;
                }

                string filePath = Path.GetDirectoryName(Path.GetFullPath(fileName));
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                Log.Info(this, "Save", fileName);

                this.SaveCount++;
                using (FileStream file = File.Create(fileName))
                {
                    ServiceDispatcherSettings cfg = new ServiceDispatcherSettings();

                    cfg.DispatchByDistrict = this.DispatchHearsesByDistrict && this.DispatchGarbageTrucksByDistrict;
                    cfg.DispatchByRange = this.DispatchHearsesByRange || this.DispatchGarbageTrucksByRange;
                    cfg.RangeModifier = this.RangeModifier;
                    cfg.RangeLimit = this.RangeLimit;
                    cfg.RangeMaximum = this.RangeMaximum;
                    cfg.RangeMinimum = this.RangeMinimum;
                    cfg.ReflectionAllowance = this.ReflectionAllowance;

                    cfg.DispatchHearses = this.DispatchHearses;
                    cfg.DispatchHearsesByDistrict = this.DispatchHearsesByDistrict;
                    cfg.DispatchHearsesByRange = this.DispatchHearsesByRange;
                    cfg.RemoveHearsesFromGrid = this.RemoveHearsesFromGrid;
                    cfg.CreateSpareHearses = this.CreateSpareHearses;
                    cfg.DeathChecksPreset = this.deathChecksPreset;
                    cfg.DeathChecksCustom = this.deathChecksCustom;
                    cfg.DeathChecksCurrent = this.DeathChecksParameters;

                    cfg.DispatchGarbageTrucks = this.DispatchGarbageTrucks;
                    cfg.DispatchGarbageTrucksByDistrict = this.DispatchGarbageTrucksByDistrict;
                    cfg.DispatchGarbageTrucksByRange = this.DispatchGarbageTrucksByRange;
                    cfg.LimitOpportunisticGarbageCollection = this.LimitOpportunisticGarbageCollection;
                    cfg.CreateSpareGarbageTrucks = this.CreateSpareGarbageTrucks;
                    cfg.MinimumGarbageForDispatch = this.MinimumGarbageForDispatch;
                    cfg.MinimumGarbageForPatrol = this.MinimumGarbageForPatrol;
                    cfg.GarbageChecksPreset = this.garbageChecksPreset;
                    cfg.GarbageChecksCustom = this.garbageChecksCustom;
                    cfg.GarbageChecksCurrent = this.GarbageChecksParameters;

                    cfg.AutoBulldozeBuildings = this.AutoBulldozeBuildings;
                    cfg.AutoBulldozeBuildingsDelaySeconds = this.AutoBulldozeBuildingsDelaySeconds;

                    cfg.BuildingChecksPresets = (Enum.GetValues(typeof(BuildingCheckOrder)) as BuildingCheckOrder[]).Where(bco => bco != BuildingCheckOrder.Custom).Select(bco => new ServiceDispatcherSettings.BuildingChecksPresetInfo(bco)).ToArray();
                    cfg.BuildingChecksPossible = (Enum.GetValues(typeof(BuildingCheckParameters)) as BuildingCheckParameters[]).Where(bcp => bcp != BuildingCheckParameters.Custom).ToArray();

                    cfg.Version = this.Version;
                    cfg.SaveCount = this.SaveCount;

                    XmlSerializer ser = new XmlSerializer(typeof(ServiceDispatcherSettings));
                    ser.Serialize(file, cfg);
                    file.Flush();
                    file.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "Save", ex);
            }

            Log.Debug(this, "Save", "End");
        }

        /// <summary>
        /// Check whether something is allowed.
        /// </summary>
        /// <param name="allowance">The allowance.</param>
        /// <param name="minGameVersion">The minimum game version.</param>
        /// <param name="maxGameVersion">The maximum game version.</param>
        /// <returns>True if allowed.</returns>
        private bool AllowanceCheck(Allowance allowance, uint minGameVersion = 0, uint maxGameVersion = uint.MaxValue)
        {
            return allowance != Allowance.Never &&
                   BuildConfig.APPLICATION_VERSION >= minGameVersion &&
                   (allowance == Allowance.Always || BuildConfig.APPLICATION_VERSION < maxGameVersion);
        }

        /// <summary>
        /// Describes something's allowance.
        /// </summary>
        /// <param name="allowance">The allowance.</param>
        /// <param name="minGameVersion">The minimum game version.</param>
        /// <param name="maxGameVersion">The maximum game version.</param>
        /// <returns>The allowance description.</returns>
        private string AllowanceText(Allowance allowance, uint minGameVersion = 0, uint maxGameVersion = uint.MaxValue)
        {
            if (allowance == Allowance.Never)
            {
                return "No (disabled)";
            }
            else if (BuildConfig.APPLICATION_VERSION < minGameVersion)
            {
                return "No (game version too low)";
            }
            else if (allowance == Allowance.Always)
            {
                return "Yes (overridden)";
            }
            else if (BuildConfig.APPLICATION_VERSION >= maxGameVersion)
            {
                return "No (game version too high)";
            }
            else if (minGameVersion > 0 || maxGameVersion < uint.MaxValue)
            {
                return "Yes (game version within limits)";
            }
            else
            {
                return "Yes (enabled)";
            }
        }

        /// <summary>
        /// Serializable settings class.
        /// </summary>
        [Serializable]
        public class ServiceDispatcherSettings
        {
            /// <summary>
            /// Automatic bulldoze of abandoned buildings.
            /// </summary>
            public bool AutoBulldozeBuildings = false;

            /// <summary>
            /// The automatic bulldoze buildings delay.
            /// </summary>
            public double AutoBulldozeBuildingsDelaySeconds = 10.0 * 60.0;

            /// <summary>
            /// The possible building checks.
            /// </summary>
            public BuildingCheckParameters[] BuildingChecksPossible = null;

            /// <summary>
            /// The possible building checks presets.
            /// </summary>
            public BuildingChecksPresetInfo[] BuildingChecksPresets = null;

            /// <summary>
            /// When to create spare garbage trucks.
            /// </summary>
            public SpareVehiclesCreation CreateSpareGarbageTrucks = SpareVehiclesCreation.WhenBuildingIsCloser;

            /// <summary>
            /// When to create spare hearses.
            /// </summary>
            public SpareVehiclesCreation CreateSpareHearses = SpareVehiclesCreation.WhenBuildingIsCloser;

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
            /// Whether stopped garbage trucks should be removed from grid or not.
            /// </summary>
            public bool RemoveGarbageTrucksFromGrid = false;

            /// <summary>
            /// Whether stopped hearses should be removed from grid or not.
            /// </summary>
            public bool RemoveHearsesFromGrid = false;

            /// <summary>
            /// The save count.
            /// </summary>
            public uint SaveCount = 0;

            /// <summary>
            /// The settings version.
            /// </summary>
            public int Version = 0;

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
                    this.Identifier = BuildingCheckOrder.Custom;
                    this.BuildingChecks = new BuildingCheckParameters[] { BuildingCheckParameters.Any };
                }

                /// <summary>
                /// Initializes a new instance of the <see cref="BuildingChecksPresetInfo" /> class.
                /// </summary>
                /// <param name="buildingCheckOrder">The building check order preset.</param>
                public BuildingChecksPresetInfo(BuildingCheckOrder buildingCheckOrder)
                {
                    this.Identifier = buildingCheckOrder;
                    this.Name = GetBuildingCheckOrderName(buildingCheckOrder);
                    this.Description = GetBuildingCheckOrderDescription(buildingCheckOrder);
                    this.BuildingChecks = GetBuildingChecksParameters(this.Identifier);
                }
            }
        }
    }
}