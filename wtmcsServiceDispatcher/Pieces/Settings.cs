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
    internal class Settings
    {
        /// <summary>
        /// The death-care settings.
        /// </summary>
        public readonly ServiceSettings DeathCare = new ServiceSettings()
        {
            VehicleNamePlural = "Hearses",
            EmptiableServiceBuildingNamePlural = "Cemeteries",
            MaterialName = "Dead People",
            CanAutoEmpty = true,
            CanRemoveFromGrid = true
        };

        /// <summary>
        /// The garbage settings.
        /// </summary>
        public readonly ServiceSettings Garbage = new ServiceSettings()
        {
            VehicleNamePlural = "Garbage Trucks",
            EmptiableServiceBuildingNamePlural = "Landfills",
            MaterialName = "Garbage",
            CanAutoEmpty = true,
            CanLimitOpportunisticCollection = true,
            OpportunisticCollectionLimitDetour = Detours.Methods.GarbageTruckAI_TryCollectGarbage,
            UseMinimumAmountForDispatch = true,
            UseMinimumAmountForPatrol = true
        };

        /// <summary>
        /// The health-care settings.
        /// </summary>
        public readonly ServiceSettings HealthCare = new ServiceSettings()
        {
            VehicleNamePlural = "Ambulances",
            MaterialName = "Sick People",
            CanRemoveFromGrid = true
        };

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
        public double AutoBulldozeBuildingsDelaySeconds = 5.0 * 60.0;

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
        /// Whether code overrides are allowed or not.
        /// </summary>
        public Allowance ReflectionAllowance = Allowance.Default;

        /// <summary>
        /// Automatic removal of stuck vehicles.
        /// </summary>
        public bool RemoveStuckVehicles = false;

        /// <summary>
        /// The automatic vehicle removal delay.
        /// </summary>
        public double RemoveStuckVehiclesDelaySeconds = 5 * 60.0;

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
            { SpareVehiclesCreation.Never, "Let the game's AI decide when to send out vehicles." },
            { SpareVehiclesCreation.WhenNoFree, "Send out spare vehicles when service building has no free vehicles." },
            { SpareVehiclesCreation.WhenBuildingIsCloser, "Send out spare vehicles when service building is closer than all free vehicles." }
        };

        /// <summary>
        /// The display names for the vehicle creation options.
        /// </summary>
        private static Dictionary<SpareVehiclesCreation, string> spareVehiclesCreationNames = new Dictionary<SpareVehiclesCreation, string>()
        {
            { SpareVehiclesCreation.Never, "Game decides" },
            { SpareVehiclesCreation.WhenNoFree, "None are free" },
            { SpareVehiclesCreation.WhenBuildingIsCloser, "Building is closer" }
        };

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

                this.DeathCare.DispatchVehicles = settings.DispatchHearses;
                this.DeathCare.DispatchByDistrict = settings.DispatchHearsesByDistrict;
                this.DeathCare.DispatchByRange = settings.DispatchHearsesByRange;
                this.DeathCare.CreateSpares = settings.CreateSpareHearses;
                this.DeathCare.RemoveFromGrid = settings.RemoveHearsesFromGrid;
                this.DeathCare.ChecksCustom = settings.DeathChecksCustom;
                this.DeathCare.ChecksPreset = settings.DeathChecksPreset;

                this.HealthCare.DispatchVehicles = settings.DispatchAmbulances;
                this.HealthCare.DispatchByDistrict = settings.DispatchAmbulancesByDistrict;
                this.HealthCare.DispatchByRange = settings.DispatchAmbulancesByRange;
                this.HealthCare.CreateSpares = settings.CreateSpareAmbulances;
                this.HealthCare.RemoveFromGrid = settings.RemoveAmbulancesFromGrid;
                this.HealthCare.ChecksCustom = settings.SickChecksCustom;
                this.HealthCare.ChecksPreset = settings.SickChecksPreset;

                this.Garbage.DispatchVehicles = settings.DispatchGarbageTrucks;
                this.Garbage.DispatchByDistrict = settings.DispatchGarbageTrucksByDistrict;
                this.Garbage.DispatchByRange = settings.DispatchGarbageTrucksByRange;
                this.Garbage.LimitOpportunisticCollection = settings.LimitOpportunisticGarbageCollection;
                this.Garbage.CreateSpares = settings.CreateSpareGarbageTrucks;
                this.Garbage.MinimumAmountForDispatch = settings.MinimumGarbageForDispatch;
                this.Garbage.MinimumAmountForPatrol = settings.MinimumGarbageForPatrol;
                this.Garbage.ChecksCustom = settings.GarbageChecksCustom;
                this.Garbage.ChecksPreset = settings.GarbageChecksPreset;

                this.AutoBulldozeBuildings = settings.AutoBulldozeBuildings;
                this.AutoBulldozeBuildingsDelaySeconds = settings.AutoBulldozeBuildingsDelaySeconds;

                this.RemoveStuckVehicles = settings.RemoveStuckVehicles;
                this.RemoveStuckVehiclesDelaySeconds = settings.RemoveStuckVehiclesDelaySeconds;
            }

            this.HealthCare.DispatchVehicles = false;
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
        /// Gets a value indicating whether any dispatchers cares about districts or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if any dispatchers dispatches by district; otherwise, <c>false</c>.
        /// </value>
        public bool DispatchAnyByDistrict
        {
            get
            {
                return this.DeathCare.DispatchByDistrict || this.Garbage.DispatchByDistrict || this.HealthCare.DispatchByDistrict;
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
                return this.DeathCare.DispatchByRange || this.Garbage.DispatchByRange || this.HealthCare.DispatchByRange;
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
        public bool AllowReflection(uint minGameVersion = 0, uint maxGameVersion = uint.MaxValue)
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

            Log.Debug(this, "LogSettings", "DispatchHearses", this.DeathCare.DispatchVehicles);
            Log.Debug(this, "LogSettings", "DispatchHearsesByDistrict", this.DeathCare.DispatchByDistrict);
            Log.Debug(this, "LogSettings", "DispatchHearsesByRange", this.DeathCare.DispatchByRange);
            Log.Debug(this, "LogSettings", "RemoveHearsesFromGrid", this.DeathCare.RemoveFromGrid);
            Log.Debug(this, "LogSettings", "CreateSpareHearses", this.DeathCare.CreateSpares);
            Log.Debug(this, "LogSettings", "DeathChecks", (byte)this.DeathCare.ChecksPreset, this.DeathCare.ChecksPreset, GetBuildingCheckOrderName(this.DeathCare.ChecksPreset));
            Log.Debug(this, "LogSettings", "DeathChecksParameters", String.Join(", ", this.DeathCare.ChecksParameters.Select(bc => bc.ToString()).ToArray()));
            if (this.DeathCare.ChecksCustom != null)
            {
                Log.Debug(this, "LogSettings", "DeathChecksCustom", String.Join(", ", this.DeathCare.ChecksCustom.Select(bc => bc.ToString()).ToArray()));
            }

            Log.Debug(this, "LogSettings", "DispatchAmbulances", this.HealthCare.DispatchVehicles);
            Log.Debug(this, "LogSettings", "DispatchAmbulancesByDistrict", this.HealthCare.DispatchByDistrict);
            Log.Debug(this, "LogSettings", "DispatchAmbulancesByRange", this.HealthCare.DispatchByRange);
            Log.Debug(this, "LogSettings", "RemoveAmbulancesFromGrid", this.HealthCare.RemoveFromGrid);
            Log.Debug(this, "LogSettings", "CreateSpareAmbulances", this.HealthCare.CreateSpares);
            Log.Debug(this, "LogSettings", "SickChecks", (byte)this.HealthCare.ChecksPreset, this.HealthCare.ChecksPreset, GetBuildingCheckOrderName(this.HealthCare.ChecksPreset));
            Log.Debug(this, "LogSettings", "SickChecksParameters", String.Join(", ", this.HealthCare.ChecksParameters.Select(bc => bc.ToString()).ToArray()));
            if (this.HealthCare.ChecksCustom != null)
            {
                Log.Debug(this, "LogSettings", "SickChecksCustom", String.Join(", ", this.HealthCare.ChecksCustom.Select(bc => bc.ToString()).ToArray()));
            }

            Log.Debug(this, "LogSettings", "DispatchGarbageTrucks", this.Garbage.DispatchVehicles);
            Log.Debug(this, "LogSettings", "DispatchGarbageTrucksByDistrict", this.Garbage.DispatchByDistrict);
            Log.Debug(this, "LogSettings", "DispatchGarbageTrucksByRange", this.Garbage.DispatchByRange);
            Log.Debug(this, "LogSettings", "LimitOportunisticGarbageCollection", this.Garbage.LimitOpportunisticCollection);
            Log.Debug(this, "LogSettings", "CreateSpareGarbageTrucks", this.Garbage.CreateSpares);
            Log.Debug(this, "LogSettings", "MinimumGarbageForDispatch", this.Garbage.MinimumAmountForDispatch);
            Log.Debug(this, "LogSettings", "MinimumGarbageForPatrol", this.Garbage.MinimumAmountForPatrol);
            Log.Debug(this, "LogSettings", "GarbageChecks", (byte)this.Garbage.ChecksPreset, this.Garbage.ChecksPreset, GetBuildingCheckOrderName(this.Garbage.ChecksPreset));
            Log.Debug(this, "LogSettings", "GarbageChecksParameters", String.Join(", ", this.Garbage.ChecksParameters.Select(bc => bc.ToString()).ToArray()));
            if (this.Garbage.ChecksCustom != null)
            {
                Log.Debug(this, "LogSettings", "GarbageChecksCustom", String.Join(", ", this.Garbage.ChecksCustom.Select(bc => bc.ToString()).ToArray()));
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

                    cfg.DispatchByDistrict = this.DeathCare.DispatchByDistrict && this.Garbage.DispatchByDistrict && this.HealthCare.DispatchByDistrict;
                    cfg.DispatchByRange = this.DeathCare.DispatchByRange || this.Garbage.DispatchByRange || this.HealthCare.DispatchByRange;
                    cfg.RangeModifier = this.RangeModifier;
                    cfg.RangeLimit = this.RangeLimit;
                    cfg.RangeMaximum = this.RangeMaximum;
                    cfg.RangeMinimum = this.RangeMinimum;
                    cfg.ReflectionAllowance = this.ReflectionAllowance;

                    cfg.DispatchHearses = this.DeathCare.DispatchVehicles;
                    cfg.DispatchHearsesByDistrict = this.DeathCare.DispatchByDistrict;
                    cfg.DispatchHearsesByRange = this.DeathCare.DispatchByRange;
                    cfg.RemoveHearsesFromGrid = this.DeathCare.RemoveFromGrid;
                    cfg.CreateSpareHearses = this.DeathCare.CreateSpares;
                    cfg.DeathChecksPreset = this.DeathCare.ChecksPreset;
                    cfg.DeathChecksCustom = this.DeathCare.ChecksCustom;
                    cfg.DeathChecksCurrent = this.DeathCare.ChecksParameters;

                    cfg.DispatchAmbulances = this.HealthCare.DispatchVehicles;
                    cfg.DispatchAmbulancesByDistrict = this.HealthCare.DispatchByDistrict;
                    cfg.DispatchAmbulancesByRange = this.HealthCare.DispatchByRange;
                    cfg.RemoveAmbulancesFromGrid = this.HealthCare.RemoveFromGrid;
                    cfg.CreateSpareAmbulances = this.HealthCare.CreateSpares;
                    cfg.SickChecksPreset = this.HealthCare.ChecksPreset;
                    cfg.SickChecksCustom = this.HealthCare.ChecksCustom;
                    cfg.SickChecksCurrent = this.HealthCare.ChecksParameters;

                    cfg.DispatchGarbageTrucks = this.Garbage.DispatchVehicles;
                    cfg.DispatchGarbageTrucksByDistrict = this.Garbage.DispatchByDistrict;
                    cfg.DispatchGarbageTrucksByRange = this.Garbage.DispatchByRange;
                    cfg.LimitOpportunisticGarbageCollection = this.Garbage.LimitOpportunisticCollection;
                    cfg.CreateSpareGarbageTrucks = this.Garbage.CreateSpares;
                    cfg.MinimumGarbageForDispatch = this.Garbage.MinimumAmountForDispatch;
                    cfg.MinimumGarbageForPatrol = this.Garbage.MinimumAmountForPatrol;
                    cfg.GarbageChecksPreset = this.Garbage.ChecksPreset;
                    cfg.GarbageChecksCustom = this.Garbage.ChecksCustom;
                    cfg.GarbageChecksCurrent = this.Garbage.ChecksParameters;

                    cfg.AutoBulldozeBuildings = this.AutoBulldozeBuildings;
                    cfg.AutoBulldozeBuildingsDelaySeconds = this.AutoBulldozeBuildingsDelaySeconds;

                    cfg.RemoveStuckVehicles = this.RemoveStuckVehicles;
                    cfg.RemoveStuckVehiclesDelaySeconds = this.RemoveStuckVehiclesDelaySeconds;

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
            public double AutoBulldozeBuildingsDelaySeconds = 5.0 * 60.0;

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

        /// <summary>
        /// Settings for normal services.
        /// </summary>
        public class ServiceSettings
        {
            /// <summary>
            /// The automatic emptying start level.
            /// </summary>
            public uint AutoEmptyStartLevelPercent = 95u;

            /// <summary>
            /// The automatic empty cemetery stop level.
            /// </summary>
            public uint AutoEmptyStopLevelPercent = 5u;

            /// <summary>
            /// The custom checks parameters.
            /// </summary>
            public BuildingCheckParameters[] ChecksCustom = null;

            /// <summary>
            /// The create spares option.
            /// </summary>
            public SpareVehiclesCreation CreateSpares = SpareVehiclesCreation.WhenBuildingIsCloser;

            /// <summary>
            /// The dispatch by district toggle.
            /// </summary>
            public bool DispatchByDistrict = false;

            /// <summary>
            /// The dispatch by range toggle.
            /// </summary>
            public bool DispatchByRange = true;

            /// <summary>
            /// The dispatch toggle.
            /// </summary>
            public bool DispatchVehicles = false;

            /// <summary>
            /// The minimum amount for dispatch.
            /// </summary>
            public ushort MinimumAmountForDispatch = 1500;

            /// <summary>
            /// The minimum amount for patrol.
            /// </summary>
            public ushort MinimumAmountForPatrol = 200;

            /// <summary>
            /// The automatic empty settings value.
            /// </summary>
            private bool autoEmptyValue = false;

            /// <summary>
            /// The automatic empty possible value.
            /// </summary>
            private bool? canAutoEmptyValue = null;

            /// <summary>
            /// The opportunistic collection limit possible value.
            /// </summary>
            private bool? canLimitOpportunisticCollectionValue = false;

            /// <summary>
            /// The remove from grid possible value.
            /// </summary>
            private bool? canRemoveFromGridValue = null;

            /// <summary>
            /// The checks preset settings value.
            /// </summary>
            private BuildingCheckOrder checksPresetValue = BuildingCheckOrder.InRange;

            /// <summary>
            /// The plural name for service buildings that can be emptied value.
            /// </summary>
            private string emptiableServiceBuildingNamePluralValue = null;

            /// <summary>
            /// The opportunistic collection limit settings value.
            /// </summary>
            private bool limitOpportunisticCollectionValue = true;

            /// <summary>
            /// The material name value.
            /// </summary>
            private string materialNameValue = null;

            /// <summary>
            /// The opportunistic collection limit detour value.
            /// </summary>
            private Detours.Methods opportunisticCollectionLimitDetour = Detours.Methods.None;

            /// <summary>
            /// The remove from grid settings value.
            /// </summary>
            private bool removeFromGrid = true;

            /// <summary>
            /// The minimum amount for dispatch usability value.
            /// </summary>
            private bool? useMinimumAmountForDispatch = false;

            /// <summary>
            /// The minimum amount for patrol usability value.
            /// </summary>
            private bool? useMinimumAmountForPatrol = false;

            /// <summary>
            /// The plural vehicle name value.
            /// </summary>
            private string vehicleNamePluralValue = null;

            /// <summary>
            /// The singular vehicle name value.
            /// </summary>
            private string vehicleNameSingularValue;

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceSettings" /> class.
            /// </summary>
            public ServiceSettings()
            {
            }

            /// <summary>
            /// Gets or sets a value indicating whether automatic emptying should be done.
            /// </summary>
            /// <value>
            ///   <c>true</c> if automatic emptying is on; otherwise, <c>false</c>.
            /// </value>
            public bool AutoEmpty
            {
                get
                {
                    return this.autoEmptyValue && this.CanAutoEmpty;
                }

                set
                {
                    this.autoEmptyValue = value;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether this service can do automatic emptying.
            /// </summary>
            /// <value>
            /// <c>true</c> if this service can automatic emptying; otherwise, <c>false</c>.
            /// </value>
            /// <exception cref="System.InvalidOperationException">Write-once property modification.</exception>
            public bool CanAutoEmpty
            {
                get
                {
                    return this.canAutoEmptyValue != null && this.canAutoEmptyValue.HasValue && this.canAutoEmptyValue.Value;
                }

                set
                {
                    if (this.canAutoEmptyValue == null || !this.canAutoEmptyValue.HasValue)
                    {
                        this.canAutoEmptyValue = value;
                    }
                    else
                    {
                        throw new InvalidOperationException("Write-once property modification");
                    }
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether this service can limit opportunistic collection.
            /// </summary>
            /// <value>
            /// <c>true</c> if this service can limit opportunistic collection; otherwise, <c>false</c>.
            /// </value>
            /// <exception cref="System.InvalidOperationException">Write-once property modification.</exception>
            public bool CanLimitOpportunisticCollection
            {
                get
                {
                    return this.canLimitOpportunisticCollectionValue != null && this.canLimitOpportunisticCollectionValue.HasValue && this.canLimitOpportunisticCollectionValue.Value;
                }

                set
                {
                    if (this.canLimitOpportunisticCollectionValue == null || !this.canLimitOpportunisticCollectionValue.HasValue)
                    {
                        this.canLimitOpportunisticCollectionValue = value;
                    }
                    else
                    {
                        throw new InvalidOperationException("Write-once property modification");
                    }
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether these vehicles can be removed from grid.
            /// </summary>
            /// <value>
            /// <c>true</c> if these vehicles can be removed from grid; otherwise, <c>false</c>.
            /// </value>
            /// <exception cref="System.InvalidOperationException">Write-once property modification.</exception>
            public bool CanRemoveFromGrid
            {
                get
                {
                    return this.canRemoveFromGridValue != null && this.canRemoveFromGridValue.HasValue && this.canRemoveFromGridValue.Value;
                }

                set
                {
                    if (this.canRemoveFromGridValue == null || !this.canRemoveFromGridValue.HasValue)
                    {
                        this.canRemoveFromGridValue = value;
                    }
                    else
                    {
                        throw new InvalidOperationException("Write-once property modification");
                    }
                }
            }

            /// <summary>
            /// Gets the building checks parameters.
            /// </summary>
            /// <value>
            /// The checks parameters.
            /// </value>
            public BuildingCheckParameters[] ChecksParameters
            {
                get
                {
                    return GetBuildingChecksParameters(this.checksPresetValue, this.ChecksCustom);
                }
            }

            /// <summary>
            /// Gets or sets the building checks preset.
            /// </summary>
            /// <value>
            /// The checks preset.
            /// </value>
            public BuildingCheckOrder ChecksPreset
            {
                get
                {
                    return this.checksPresetValue;
                }

                set
                {
                    this.checksPresetValue = value;

                    if (value == BuildingCheckOrder.Custom && (this.ChecksCustom == null || this.ChecksCustom.Length == 0))
                    {
                        this.ChecksCustom = GetBuildingChecksParameters(BuildingCheckOrder.InRange);
                    }
                }
            }

            /// <summary>
            /// Gets or sets the plural name for service buildings that can be emptied.
            /// </summary>
            /// <value>
            /// The plural name for service buildings that can be emptied.
            /// </value>
            /// <exception cref="System.InvalidOperationException">Write-once property modification.</exception>
            public string EmptiableServiceBuildingNamePlural
            {
                get
                {
                    return this.emptiableServiceBuildingNamePluralValue;
                }

                set
                {
                    if (this.emptiableServiceBuildingNamePluralValue == null)
                    {
                        this.emptiableServiceBuildingNamePluralValue = value;
                    }
                    else
                    {
                        throw new InvalidOperationException("Write-once property modification");
                    }
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether to limit opportunistic collection.
            /// </summary>
            /// <value>
            /// <c>true</c> if limiting opportunistic collection; otherwise, <c>false</c>.
            /// </value>
            public bool LimitOpportunisticCollection
            {
                get
                {
                    return this.limitOpportunisticCollectionValue && this.CanLimitOpportunisticCollection;
                }

                set
                {
                    this.limitOpportunisticCollectionValue = value;
                }
            }

            /// <summary>
            /// Gets or sets the material name.
            /// </summary>
            /// <value>
            /// The name of the material.
            /// </value>
            /// <exception cref="System.InvalidOperationException">Write-once property modification.</exception>
            public string MaterialName
            {
                get
                {
                    return this.materialNameValue;
                }

                set
                {
                    if (this.materialNameValue == null)
                    {
                        this.materialNameValue = value;
                    }
                    else
                    {
                        throw new InvalidOperationException("Write-once property modification");
                    }
                }
            }

            /// <summary>
            /// Gets or sets the opportunistic collection limit detour method.
            /// </summary>
            /// <value>
            /// The opportunistic collection limit detour method.
            /// </value>
            /// <exception cref="System.InvalidOperationException">Write-once property modification.</exception>
            public Detours.Methods OpportunisticCollectionLimitDetour
            {
                get
                {
                    return this.opportunisticCollectionLimitDetour;
                }

                set
                {
                    if (this.opportunisticCollectionLimitDetour == Detours.Methods.None)
                    {
                        this.opportunisticCollectionLimitDetour = value;
                    }
                    else
                    {
                        throw new InvalidOperationException("Write-once property modification");
                    }
                }
            }

            /// <summary>
            /// Gets a value indicating whether this service patrols.
            /// </summary>
            /// <value>
            ///   <c>true</c> if service patrols; otherwise, <c>false</c>.
            /// </value>
            public bool Patrol
            {
                get
                {
                    return this.UseMinimumAmountForPatrol && this.MinimumAmountForPatrol > 0 &&
                           (!this.UseMinimumAmountForDispatch || this.MinimumAmountForPatrol < this.MinimumAmountForDispatch);
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether these vehicles should be removed from grid.
            /// </summary>
            /// <value>
            ///   <c>true</c> if vehicles should be removed from grid; otherwise, <c>false</c>.
            /// </value>
            public bool RemoveFromGrid
            {
                get
                {
                    return this.removeFromGrid && this.CanRemoveFromGrid;
                }

                set
                {
                    this.canRemoveFromGridValue = value;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether to use minimum amount for dispatch.
            /// </summary>
            /// <value>
            /// <c>true</c> if minimum amount for dispatch is used; otherwise, <c>false</c>.
            /// </value>
            /// <exception cref="System.InvalidOperationException">Write-once property modification.</exception>
            public bool UseMinimumAmountForDispatch
            {
                get
                {
                    return this.useMinimumAmountForDispatch != null && this.useMinimumAmountForDispatch.HasValue && this.useMinimumAmountForDispatch.Value;
                }

                set
                {
                    if (this.useMinimumAmountForDispatch == null || !this.useMinimumAmountForDispatch.HasValue)
                    {
                        this.useMinimumAmountForDispatch = value;
                    }
                    else
                    {
                        throw new InvalidOperationException("Write-once property modification");
                    }
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether to use minimum amount for patrol.
            /// </summary>
            /// <value>
            /// <c>true</c> if minimum amount for patrol is used; otherwise, <c>false</c>.
            /// </value>
            /// <exception cref="System.InvalidOperationException">Write-once property modification.</exception>
            public bool UseMinimumAmountForPatrol
            {
                get
                {
                    return this.useMinimumAmountForPatrol != null && this.useMinimumAmountForPatrol.HasValue && this.useMinimumAmountForPatrol.Value;
                }

                set
                {
                    if (this.useMinimumAmountForPatrol == null || !this.useMinimumAmountForPatrol.HasValue)
                    {
                        this.useMinimumAmountForPatrol = value;
                    }
                    else
                    {
                        throw new InvalidOperationException("Write-once property modification");
                    }
                }
            }

            /// <summary>
            /// Gets or sets the plural vehicle name.
            /// </summary>
            /// <value>
            /// The plural vehicle name.
            /// </value>
            /// <exception cref="System.InvalidOperationException">Write-once property modification.</exception>
            public string VehicleNamePlural
            {
                get
                {
                    if (this.vehicleNamePluralValue != null)
                    {
                        return this.vehicleNamePluralValue;
                    }
                    else if (this.VehicleNameSingular != null)
                    {
                        return this.VehicleNameSingular + "s";
                    }
                    else
                    {
                        return null;
                    }
                }

                set
                {
                    if (this.vehicleNamePluralValue == null)
                    {
                        this.vehicleNamePluralValue = value;
                    }
                    else
                    {
                        throw new InvalidOperationException("Write-once property modification");
                    }
                }
            }

            /// <summary>
            /// Gets or sets the singular vehicle name.
            /// </summary>
            /// <value>
            /// The singular vehicle name.
            /// </value>
            /// <exception cref="System.InvalidOperationException">Write-once property modification.</exception>
            public string VehicleNameSingular
            {
                get
                {
                    if (this.vehicleNameSingularValue != null)
                    {
                        return this.vehicleNameSingularValue;
                    }
                    else if (this.vehicleNamePluralValue != null && this.vehicleNamePluralValue.Length > 1)
                    {
                        return this.vehicleNamePluralValue.Substring(0, this.vehicleNamePluralValue.Length - 1);
                    }
                    else
                    {
                        return null;
                    }
                }

                set
                {
                    if (this.vehicleNameSingularValue == null)
                    {
                        this.vehicleNameSingularValue = value;
                    }
                    else
                    {
                        throw new InvalidOperationException("Write-once property modification");
                    }
                }
            }
        }
    }
}