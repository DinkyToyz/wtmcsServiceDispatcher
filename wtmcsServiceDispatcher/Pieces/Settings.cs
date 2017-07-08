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
        /// A bit above the maximum tested game version.
        /// </summary>
        public static uint AboveMaxTestedGameVersion = BuildConfig.MakeVersionNumber(1, 8, 0, BuildConfig.ReleaseType.Final, 0, BuildConfig.BuildType.Unknown);

        /// <summary>
        /// The death-care settings.
        /// </summary>
        public readonly StandardServiceSettings DeathCare = new StandardServiceSettings()
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
        public readonly StandardServiceSettings Garbage = new StandardServiceSettings()
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
        public readonly StandardServiceSettings HealthCare = new StandardServiceSettings()
        {
            VehicleNamePlural = "Ambulances",
            MaterialName = "Sick People",
            CanRemoveFromGrid = true
        };

        /// <summary>
        /// The recovery crews settings.
        /// </summary>
        public readonly HiddenServiceSettings RecoveryCrews = new HiddenServiceSettings()
        {
            VehicleNamePlural = "Recovery Services",
            VehicleNameSingular = "Recovery"
        };

        /// <summary>
        /// The settings version.
        /// </summary>
        public readonly int Version = 4;

        /// <summary>
        /// The wrecking crews settings.
        /// </summary>
        public readonly HiddenServiceSettings WreckingCrews = new HiddenServiceSettings()
        {
            VehicleNamePlural = "Bulldozers"
        };

        /// <summary>
        /// The SetTarget call compatibility mode.
        /// </summary>
        public ServiceDispatcherSettings.ModCompatibilityMode AssignmentCompatibilityMode = ServiceDispatcherSettings.DefaultAssignmentCompatibilityMode;

        /// <summary>
        /// Whether transfer manager offer blocking is allowed or not.
        /// </summary>
        public bool BlockTransferManagerOffers = true;

        /// <summary>
        /// The CreateVehicle call compatibility mode.
        /// </summary>
        public ServiceDispatcherSettings.ModCompatibilityMode CreationCompatibilityMode = ServiceDispatcherSettings.DefaultCreationCompatibilityMode;

        /// <summary>
        /// The settings version in the loaded file.
        /// </summary>
        public int? loadedVersion = null;

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
        public ServiceDispatcherSettings.Allowance ReflectionAllowance = ServiceDispatcherSettings.Allowance.Default;

        /// <summary>
        /// The save count.
        /// </summary>
        public uint SaveCount = 0;

        /// <summary>
        /// The descriptions for the building checks orders.
        /// </summary>
        private static Dictionary<ServiceDispatcherSettings.BuildingCheckOrder, string> buildingCheckOrderDescriptions = new Dictionary<ServiceDispatcherSettings.BuildingCheckOrder, string>()
        {
            { ServiceDispatcherSettings.BuildingCheckOrder.FirstFirst, "All buldings regardless of range." },
            { ServiceDispatcherSettings.BuildingCheckOrder.ForgottenFirst, "Forgotten buildings in range, followed by forgotten buildings out of range, buildings in range and finally problematic buildings in or out of range." },
            { ServiceDispatcherSettings.BuildingCheckOrder.InRange, "Buildings in range followed by forgotten buildings out of range." },
            { ServiceDispatcherSettings.BuildingCheckOrder.InRangeFirst, "Buildings in range followed by problematic buildings in or out of range." },
            { ServiceDispatcherSettings.BuildingCheckOrder.ProblematicFirst, "Problematic buildings in range followed by problematic buildings out of range and finally buildings in range." },
            { ServiceDispatcherSettings.BuildingCheckOrder.VeryProblematicFirst, "Very problematic buildings in range followed by very problematic buildings out of range, buildings in range and finally problematic buildings in or out of range." }
        };

        /// <summary>
        /// The display names for the building checks orders.
        /// </summary>
        private static Dictionary<ServiceDispatcherSettings.BuildingCheckOrder, string> buildingCheckOrderNames = new Dictionary<ServiceDispatcherSettings.BuildingCheckOrder, string>()
        {
            { ServiceDispatcherSettings.BuildingCheckOrder.Custom, "Custom" },
            { ServiceDispatcherSettings.BuildingCheckOrder.FirstFirst, "First first" },
            { ServiceDispatcherSettings.BuildingCheckOrder.ForgottenFirst, "Forgotten first" },
            { ServiceDispatcherSettings.BuildingCheckOrder.InRange, "In range" },
            { ServiceDispatcherSettings.BuildingCheckOrder.InRangeFirst, "In range first" },
            { ServiceDispatcherSettings.BuildingCheckOrder.ProblematicFirst, "Problematic first" },
            { ServiceDispatcherSettings.BuildingCheckOrder.VeryProblematicFirst, "Very problematic first" }
        };

        /// <summary>
        /// The mod compatibility mode descriptions.
        /// </summary>
        private static Dictionary<ServiceDispatcherSettings.ModCompatibilityMode, string> modCompatibilityModeDescriptions = new Dictionary<ServiceDispatcherSettings.ModCompatibilityMode, string>()
        {
            { ServiceDispatcherSettings.ModCompatibilityMode.UseCustomCode, "Uses custom assignment code. Bypasses original and modded code." },
            { ServiceDispatcherSettings.ModCompatibilityMode.UseOriginalClassMethods, "Calls the games original AI, including methods detoured by other mods" },
            { ServiceDispatcherSettings.ModCompatibilityMode.UseInstanciatedClassMethods, "Use current AI, including AI completely overriden by other mod. " }
        };

        /// <summary>
        /// The mod compatibility mode names.
        /// </summary>
        private static Dictionary<ServiceDispatcherSettings.ModCompatibilityMode, string> modCompatibilityModeNames = new Dictionary<ServiceDispatcherSettings.ModCompatibilityMode, string>()
        {
            { ServiceDispatcherSettings.ModCompatibilityMode.UseCustomCode, "Bypass AI" },
            { ServiceDispatcherSettings.ModCompatibilityMode.UseOriginalClassMethods, "Use original AI" },
            { ServiceDispatcherSettings.ModCompatibilityMode.UseInstanciatedClassMethods, "Use current AI" }
        };

        /// <summary>
        /// The descriptions for the vehicle creation options.
        /// </summary>
        private static Dictionary<ServiceDispatcherSettings.SpareVehiclesCreation, string> spareVehiclesCreationDescriptions = new Dictionary<ServiceDispatcherSettings.SpareVehiclesCreation, string>()
        {
            { ServiceDispatcherSettings.SpareVehiclesCreation.Never, "Let the game's AI decide when to send out vehicles." },
            { ServiceDispatcherSettings.SpareVehiclesCreation.WhenNoFree, "Send out spare vehicles when service building has no free vehicles." },
            { ServiceDispatcherSettings.SpareVehiclesCreation.WhenBuildingIsCloser, "Send out spare vehicles when service building is closer than all free vehicles." }
        };

        /// <summary>
        /// The display names for the vehicle creation options.
        /// </summary>
        private static Dictionary<ServiceDispatcherSettings.SpareVehiclesCreation, string> spareVehiclesCreationNames = new Dictionary<ServiceDispatcherSettings.SpareVehiclesCreation, string>()
        {
            { ServiceDispatcherSettings.SpareVehiclesCreation.Never, "Game decides" },
            { ServiceDispatcherSettings.SpareVehiclesCreation.WhenNoFree, "None are free" },
            { ServiceDispatcherSettings.SpareVehiclesCreation.WhenBuildingIsCloser, "Building is closer" }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings" /> class.
        /// </summary>
        public Settings(bool initialize = true)
        {
            this.Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        /// <param name="settings">The file settings.</param>
        public Settings(ServiceDispatcherSettings settings)
        {
            if (settings != null)
            {
                this.loadedVersion = settings.Version;
                this.SaveCount = settings.SaveCount;

                this.RangeModifier = settings.RangeModifier;
                this.RangeLimit = settings.RangeLimit;
                this.RangeMaximum = settings.RangeMaximum;
                this.RangeMinimum = settings.RangeMinimum;
                this.ReflectionAllowance = settings.ReflectionAllowance;
                this.BlockTransferManagerOffers = settings.BlockTransferManagerOffers;
                this.AssignmentCompatibilityMode = settings.AssignmentCompatibilityMode;
                this.CreationCompatibilityMode = settings.CreationCompatibilityMode;

                this.DeathCare.DispatchVehicles = settings.DispatchHearses;
                this.DeathCare.DispatchByDistrict = settings.DispatchHearsesByDistrict;
                this.DeathCare.DispatchByRange = settings.DispatchHearsesByRange;
                this.DeathCare.CreateSpares = settings.CreateSpareHearses;
                this.DeathCare.RemoveFromGrid = settings.RemoveHearsesFromGrid;
                this.DeathCare.ChecksCustom = settings.DeathChecksCustom;
                this.DeathCare.ChecksPreset = settings.DeathChecksPreset;
                this.DeathCare.IgnoreRangeUseClosestBuildings = settings.IgnoreRangeUseClosestDeathCareBuilding;
                this.DeathCare.AutoEmpty = settings.AutoEmptyCemeteries;
                this.DeathCare.AutoEmptyStartLevelPercent = settings.AutoEmptyCemeteryStartLevelPercent;
                this.DeathCare.AutoEmptyStopLevelPercent = settings.AutoEmptyCemeteryStopLevelPercent;

                this.HealthCare.DispatchVehicles = settings.DispatchAmbulances;
                this.HealthCare.DispatchByDistrict = settings.DispatchAmbulancesByDistrict;
                this.HealthCare.DispatchByRange = settings.DispatchAmbulancesByRange;
                this.HealthCare.CreateSpares = settings.CreateSpareAmbulances;
                this.HealthCare.RemoveFromGrid = settings.RemoveAmbulancesFromGrid;
                this.HealthCare.ChecksCustom = settings.SickChecksCustom;
                this.HealthCare.ChecksPreset = settings.SickChecksPreset;
                this.HealthCare.IgnoreRangeUseClosestBuildings = settings.IgnoreRangeUseClosestHealthCareBuilding;

                this.Garbage.DispatchVehicles = settings.DispatchGarbageTrucks;
                this.Garbage.DispatchByDistrict = settings.DispatchGarbageTrucksByDistrict;
                this.Garbage.DispatchByRange = settings.DispatchGarbageTrucksByRange;
                this.Garbage.LimitOpportunisticCollection = settings.LimitOpportunisticGarbageCollection;
                this.Garbage.CreateSpares = settings.CreateSpareGarbageTrucks;
                this.Garbage.MinimumAmountForDispatch = settings.MinimumGarbageForDispatch;
                this.Garbage.MinimumAmountForPatrol = settings.MinimumGarbageForPatrol;
                this.Garbage.ChecksCustom = settings.GarbageChecksCustom;
                this.Garbage.ChecksPreset = settings.GarbageChecksPreset;
                this.Garbage.IgnoreRangeUseClosestBuildings = settings.IgnoreRangeUseClosestGarbageBuilding;
                this.Garbage.AutoEmpty = settings.AutoEmptyLandfills;
                this.Garbage.AutoEmptyStartLevelPercent = settings.AutoEmptyLandfillStartLevelPercent;
                this.Garbage.AutoEmptyStopLevelPercent = settings.AutoEmptyLandfillStopLevelPercent;

                this.WreckingCrews.DispatchVehicles = settings.AutoBulldozeBuildings;
                this.WreckingCrews.DelaySeconds = settings.AutoBulldozeBuildingsDelaySeconds;

                this.RecoveryCrews.DispatchVehicles = settings.RemoveStuckVehicles;
                this.RecoveryCrews.DelaySeconds = settings.RemoveStuckVehiclesDelaySeconds;
            }

            this.Initialize();
        }

        /// <summary>
        /// Gets a value indicating whether the running game is above the version above the maximum tested game version.
        /// </summary>
        /// <value>
        /// <c>true</c> if version is to high; otherwise, <c>false</c>.
        /// </value>
        public static bool AboveAboveMaxTestedGameVersion => BuildConfig.APPLICATION_VERSION >= AboveMaxTestedGameVersion;

        /// <summary>
        /// Gets the default custom building checks parameters.
        /// </summary>
        /// <value>
        /// The default custom building checks parameters.
        /// </value>
        public static ServiceDispatcherSettings.BuildingCheckParameters[] DefaultCustomBuildingChecksParameters
        {
            get => new ServiceDispatcherSettings.BuildingCheckParameters[]
                        {
                            ServiceDispatcherSettings.BuildingCheckParameters.ForgottenInRange,
                            ServiceDispatcherSettings.BuildingCheckParameters.ForgottenIgnoreRange,
                            ServiceDispatcherSettings.BuildingCheckParameters.VeryProblematicInRange,
                            ServiceDispatcherSettings.BuildingCheckParameters.VeryProblematicIgnoreRange,
                            ServiceDispatcherSettings.BuildingCheckParameters.ProblematicInRange,
                            ServiceDispatcherSettings.BuildingCheckParameters.ProblematicIgnoreRange,
                            ServiceDispatcherSettings.BuildingCheckParameters.InRange
                        };
        }

        /// <summary>
        /// Gets the default custom building checks parameters for empty list.
        /// </summary>
        /// <value>
        /// The default custom building checks parameters for empty list.
        /// </value>
        public static ServiceDispatcherSettings.BuildingCheckParameters[] DefaultCustomBuildingChecksParametersIfEmpty
        {
            get => new ServiceDispatcherSettings.BuildingCheckParameters[]
                        {
                            ServiceDispatcherSettings.BuildingCheckParameters.Any
                        };
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
        /// Gets a value indicating whether to dispatch vehicles.
        /// </summary>
        /// <value>
        ///   <c>true</c> if dispatching vehicles; otherwise, <c>false</c>.
        /// </value>
        public bool DispatchAnyVehicles
        {
            get
            {
                return this.Garbage.DispatchVehicles || this.DeathCare.DispatchVehicles || this.HealthCare.DispatchVehicles;
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
        /// Gets the name of the allowance.
        /// </summary>
        /// <param name="allowance">The allowance.</param>
        /// <returns>The name of the allowance.</returns>
        public static string GetAllowanceName(ServiceDispatcherSettings.Allowance allowance)
        {
            return allowance.ToString();
        }

        /// <summary>
        /// Gets the display name of the building check order.
        /// </summary>
        /// <param name="checkOrder">The check order.</param>
        /// <returns>The display name.</returns>
        public static string GetBuildingCheckOrderDescription(ServiceDispatcherSettings.BuildingCheckOrder checkOrder)
        {
            string description;
            return buildingCheckOrderDescriptions.TryGetValue(checkOrder, out description) ? description : null;
        }

        /// <summary>
        /// Gets the display name of the building check order.
        /// </summary>
        /// <param name="checkOrder">The check order.</param>
        /// <returns>The display name.</returns>
        public static string GetBuildingCheckOrderName(ServiceDispatcherSettings.BuildingCheckOrder checkOrder)
        {
            string name;
            return buildingCheckOrderNames.TryGetValue(checkOrder, out name) ? name : checkOrder.ToString();
        }

        /// <summary>
        /// Gets the building checks parameters for the specified check order.
        /// </summary>
        /// <param name="buildingChecks">The building check order.</param>
        /// <param name="customBuildingCheckParameters">The custom building check parameters.</param>
        /// <returns>
        /// The building checks parameters.
        /// </returns>
        public static ServiceDispatcherSettings.BuildingCheckParameters[] GetBuildingChecksParameters(ServiceDispatcherSettings.BuildingCheckOrder buildingChecks, ServiceDispatcherSettings.BuildingCheckParameters[] customBuildingCheckParameters = null)
        {
            switch (buildingChecks)
            {
                case ServiceDispatcherSettings.BuildingCheckOrder.FirstFirst:
                    return new ServiceDispatcherSettings.BuildingCheckParameters[]
                        {
                            ServiceDispatcherSettings.BuildingCheckParameters.Any
                        };

                case ServiceDispatcherSettings.BuildingCheckOrder.InRangeFirst:
                    return new ServiceDispatcherSettings.BuildingCheckParameters[]
                        {
                            ServiceDispatcherSettings.BuildingCheckParameters.InRange,
                            ServiceDispatcherSettings.BuildingCheckParameters.ProblematicIgnoreRange
                        };

                case ServiceDispatcherSettings.BuildingCheckOrder.ProblematicFirst:
                    return new ServiceDispatcherSettings.BuildingCheckParameters[]
                        {
                            ServiceDispatcherSettings.BuildingCheckParameters.ProblematicInRange,
                            ServiceDispatcherSettings.BuildingCheckParameters.ProblematicIgnoreRange,
                            ServiceDispatcherSettings.BuildingCheckParameters.InRange
                        };

                case ServiceDispatcherSettings.BuildingCheckOrder.VeryProblematicFirst:
                    return new ServiceDispatcherSettings.BuildingCheckParameters[]
                        {
                            ServiceDispatcherSettings.BuildingCheckParameters.VeryProblematicInRange,
                            ServiceDispatcherSettings.BuildingCheckParameters.VeryProblematicIgnoreRange,
                            ServiceDispatcherSettings.BuildingCheckParameters.InRange,
                            ServiceDispatcherSettings.BuildingCheckParameters.ProblematicIgnoreRange
                        };

                case ServiceDispatcherSettings.BuildingCheckOrder.ForgottenFirst:
                    return new ServiceDispatcherSettings.BuildingCheckParameters[]
                        {
                            ServiceDispatcherSettings.BuildingCheckParameters.ForgottenInRange,
                            ServiceDispatcherSettings.BuildingCheckParameters.ForgottenIgnoreRange,
                            ServiceDispatcherSettings.BuildingCheckParameters.InRange,
                            ServiceDispatcherSettings.BuildingCheckParameters.ProblematicIgnoreRange
                        };

                case ServiceDispatcherSettings.BuildingCheckOrder.InRange:
                    return new ServiceDispatcherSettings.BuildingCheckParameters[]
                        {
                            ServiceDispatcherSettings.BuildingCheckParameters.InRange,
                            ServiceDispatcherSettings.BuildingCheckParameters.ForgottenIgnoreRange
                        };

                case ServiceDispatcherSettings.BuildingCheckOrder.Custom:
                    return (customBuildingCheckParameters != null && customBuildingCheckParameters.Length > 0)
                        ? customBuildingCheckParameters
                        : DefaultCustomBuildingChecksParameters;

                default:
                    return GetBuildingChecksParameters(ServiceDispatcherSettings.BuildingCheckOrder.FirstFirst);
            }
        }

        /// <summary>
        /// Gets the name of the mod compatibility mode.
        /// </summary>
        /// <param name="modCompatibilityMode">The mod compatibility mode.</param>
        /// <returns>The name of the mod compatibility mode.</returns>
        public static string GetModCompatibilityModeName(ServiceDispatcherSettings.ModCompatibilityMode modCompatibilityMode)
        {
            string name;
            return modCompatibilityModeNames.TryGetValue(modCompatibilityMode, out name) ? name : modCompatibilityMode.ToString();
        }

        /// <summary>
        /// Gets the display name of the vehicle creation option.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <returns>
        /// The display name.
        /// </returns>
        public static string GetSpareVehiclesCreationDescription(ServiceDispatcherSettings.SpareVehiclesCreation option)
        {
            string description;
            return spareVehiclesCreationDescriptions.TryGetValue(option, out description) ? description : null;
        }

        /// <summary>
        /// Gets the display name of the vehicle creation option.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <returns>
        /// The display name.
        /// </returns>
        public static string GetSpareVehiclesCreationName(ServiceDispatcherSettings.SpareVehiclesCreation option)
        {
            string name;
            return spareVehiclesCreationNames.TryGetValue(option, out name) ? name : option.ToString();
        }

        /// <summary>
        /// Loads settings from the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        /// The settings.
        /// </returns>
        public static Settings Load(string fileName = null)
        {
            if (fileName == null)
            {
                fileName = FilePathName;
            }

            return ServiceDispatcherSettings.Load(fileName);
        }

        /// <summary>
        /// Loads settings from the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The settings.</returns>
        public static Settings LoadLegacy(string fileName = null)
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

                            if (Global.EnableDevExperiments)
                            {
                                try
                                {
                                    ServiceDispatcherSettings.Save<SerializableSettings.SettingsVersion0>(fileName + ".Version0.1.xml", sets);
                                    Settings sets0 = ServiceDispatcherSettings.Load<SerializableSettings.SettingsVersion0>(fileName + ".Version0.1.xml");
                                    ServiceDispatcherSettings.Save<SerializableSettings.SettingsVersion0>(fileName + ".Version0.2.xml", sets0);

                                    ServiceDispatcherSettings.Save<SerializableSettings.SettingsVersion5>(fileName + ".Version5.1.xml", sets);
                                    Settings sets5 = ServiceDispatcherSettings.Load<SerializableSettings.SettingsVersion5>(fileName + ".Version5.1.xml");
                                    ServiceDispatcherSettings.Save<SerializableSettings.SettingsVersion5>(fileName + ".Version5.2.xml", sets5);
                                }
                                catch (Exception ex)
                                {
                                    Log.Error(typeof(Settings), "Load", ex, "Version0");
                                }
                            }

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
        /// Initializes this instance.
        /// </summary>
        public void Initialize()
        {
            if (!Global.EnableExperiments && !Global.EnableDevExperiments)
            {
            }

            if (!Global.EnableDevExperiments)
            {
            }

            this.HealthCare.DispatchVehicles = false;
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

            Log.Debug(this, "LogSettings", "ReflectionAllowance", this.ReflectionAllowance);
            Log.Debug(this, "LogSettings", "BlockTransferManagerOffers", this.BlockTransferManagerOffers);
            Log.Debug(this, "LogSettings", "AssignmentCompatibilityMode", this.AssignmentCompatibilityMode);
            Log.Debug(this, "LogSettings", "CreationCompatibilityMode", this.CreationCompatibilityMode);

            this.DeathCare.LogSettings();
            this.Garbage.LogSettings();
            this.HealthCare.LogSettings();

            this.RecoveryCrews.LogSettings();
            this.WreckingCrews.LogSettings();

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
            if (Log.LogALot || Library.IsDebugBuild)
            {
                this.LogSettings();
            }

            if (fileName == null)
            {
                fileName = FilePathName;
            }

            ServiceDispatcherSettings.Save(fileName, this);
        }

        /// <summary>
        /// Saves settings to the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public void SaveLegacy(string fileName = null)
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

                if (Global.EnableDevExperiments)
                {
                    ServiceDispatcherSettings.Save<SerializableSettings.SettingsVersion0>(fileName + ".Version0.xml", this);
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
                    cfg.BlockTransferManagerOffers = this.BlockTransferManagerOffers;
                    cfg.AssignmentCompatibilityMode = this.AssignmentCompatibilityMode;
                    cfg.CreationCompatibilityMode = this.CreationCompatibilityMode;

                    cfg.DispatchHearses = this.DeathCare.DispatchVehicles;
                    cfg.DispatchHearsesByDistrict = this.DeathCare.DispatchByDistrict;
                    cfg.DispatchHearsesByRange = this.DeathCare.DispatchByRange;
                    cfg.RemoveHearsesFromGrid = this.DeathCare.RemoveFromGrid;
                    cfg.CreateSpareHearses = this.DeathCare.CreateSpares;
                    cfg.DeathChecksPreset = this.DeathCare.ChecksPreset;
                    cfg.DeathChecksCustom = this.DeathCare.ChecksCustom;
                    cfg.DeathChecksCurrent = this.DeathCare.ChecksParameters;
                    cfg.IgnoreRangeUseClosestDeathCareBuilding = this.DeathCare.IgnoreRangeUseClosestBuildings;
                    cfg.AutoEmptyCemeteries = this.DeathCare.AutoEmpty;
                    cfg.AutoEmptyCemeteryStartLevelPercent = this.DeathCare.AutoEmptyStartLevelPercent;
                    cfg.AutoEmptyCemeteryStopLevelPercent = this.DeathCare.AutoEmptyStopLevelPercent;

                    cfg.DispatchAmbulances = this.HealthCare.DispatchVehicles;
                    cfg.DispatchAmbulancesByDistrict = this.HealthCare.DispatchByDistrict;
                    cfg.DispatchAmbulancesByRange = this.HealthCare.DispatchByRange;
                    cfg.RemoveAmbulancesFromGrid = this.HealthCare.RemoveFromGrid;
                    cfg.CreateSpareAmbulances = this.HealthCare.CreateSpares;
                    cfg.SickChecksPreset = this.HealthCare.ChecksPreset;
                    cfg.SickChecksCustom = this.HealthCare.ChecksCustom;
                    cfg.SickChecksCurrent = this.HealthCare.ChecksParameters;
                    cfg.IgnoreRangeUseClosestHealthCareBuilding = this.HealthCare.IgnoreRangeUseClosestBuildings;

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
                    cfg.IgnoreRangeUseClosestGarbageBuilding = this.Garbage.IgnoreRangeUseClosestBuildings;
                    cfg.AutoEmptyLandfills = this.Garbage.AutoEmpty;
                    cfg.AutoEmptyLandfillStartLevelPercent = this.Garbage.AutoEmptyStartLevelPercent;
                    cfg.AutoEmptyLandfillStopLevelPercent = this.Garbage.AutoEmptyStopLevelPercent;

                    cfg.AutoBulldozeBuildings = this.WreckingCrews.DispatchVehicles;
                    cfg.AutoBulldozeBuildingsDelaySeconds = this.WreckingCrews.DelaySeconds;

                    cfg.RemoveStuckVehicles = this.RecoveryCrews.DispatchVehicles;
                    cfg.RemoveStuckVehiclesDelaySeconds = this.RecoveryCrews.DelaySeconds;

                    cfg.BuildingChecksPresets = (Enum.GetValues(typeof(ServiceDispatcherSettings.BuildingCheckOrder)) as ServiceDispatcherSettings.BuildingCheckOrder[]).Where(bco => bco != ServiceDispatcherSettings.BuildingCheckOrder.Custom).Select(bco => new ServiceDispatcherSettings.BuildingChecksPresetInfo(bco)).ToArray();
                    cfg.BuildingChecksPossible = (Enum.GetValues(typeof(ServiceDispatcherSettings.BuildingCheckParameters)) as ServiceDispatcherSettings.BuildingCheckParameters[]).Where(bcp => bcp != ServiceDispatcherSettings.BuildingCheckParameters.Undefined).ToArray();

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
        private bool AllowanceCheck(ServiceDispatcherSettings.Allowance allowance, uint minGameVersion = 0, uint maxGameVersion = uint.MaxValue)
        {
            return allowance != ServiceDispatcherSettings.Allowance.Never &&
                   BuildConfig.APPLICATION_VERSION >= minGameVersion &&
                   (allowance == ServiceDispatcherSettings.Allowance.Always || BuildConfig.APPLICATION_VERSION < maxGameVersion);
        }

        /// <summary>
        /// Describes something's allowance.
        /// </summary>
        /// <param name="allowance">The allowance.</param>
        /// <param name="minGameVersion">The minimum game version.</param>
        /// <param name="maxGameVersion">The maximum game version.</param>
        /// <returns>The allowance description.</returns>
        private string AllowanceText(ServiceDispatcherSettings.Allowance allowance, uint minGameVersion = 0, uint maxGameVersion = uint.MaxValue)
        {
            if (allowance == ServiceDispatcherSettings.Allowance.Never)
            {
                return "No (disabled)";
            }
            else if (BuildConfig.APPLICATION_VERSION < minGameVersion)
            {
                return "No (game version too low)";
            }
            else if (allowance == ServiceDispatcherSettings.Allowance.Always)
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
        /// Settings for hidden services.
        /// </summary>
        /// <seealso cref="WhatThe.Mods.CitiesSkylines.ServiceDispatcher.Settings.ServiceSettingsBase" />
        public class HiddenServiceSettings : ServiceSettingsBase
        {
            /// <summary>
            /// The automatic delay in seconds.
            /// </summary>
            public double DelaySeconds = 5.0 * 60.0;

            /// <summary>
            /// Gets or sets the automatic delay in minutes.
            /// </summary>
            /// <value>
            /// The automatic delay in minutes.
            /// </value>
            public double DelayMinutes
            {
                get
                {
                    return this.DelaySeconds / 60.0;
                }

                set
                {
                    this.DelaySeconds = (value < 0.0) ? 0.0 : value * 60.0;
                }
            }

            /// <summary>
            /// Logs the settings.
            /// </summary>
            public override void LogSettings()
            {
                base.LogSettings();

                Log.Debug(this, "LogSettings", this.VehicleNamePlural, "DelayMinutes", this.DelayMinutes);
                Log.Debug(this, "LogSettings", this.VehicleNamePlural, "DelaySeconds", this.DelaySeconds);
            }
        }

        /// <summary>
        /// Abstract base class for service settings.
        /// </summary>
        public abstract class ServiceSettingsBase
        {
            /// <summary>
            /// The dispatch toggle.
            /// </summary>
            public bool DispatchVehicles = false;

            /// <summary>
            /// The plural vehicle name value.
            /// </summary>
            private string vehicleNamePluralValue = null;

            /// <summary>
            /// The singular vehicle name value.
            /// </summary>
            private string vehicleNameSingularValue = null;

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

            /// <summary>
            /// Logs the settings.
            /// </summary>
            public virtual void LogSettings()
            {
                Log.Debug(this, "LogSettings", this.VehicleNamePlural, "DispatchVehicles", this.DispatchVehicles);
            }
        }

        /// <summary>
        /// Settings for normal services.
        /// </summary>
        public class StandardServiceSettings : ServiceSettingsBase
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
            /// The create spares option.
            /// </summary>
            public ServiceDispatcherSettings.SpareVehiclesCreation CreateSpares = ServiceDispatcherSettings.SpareVehiclesCreation.WhenBuildingIsCloser;

            /// <summary>
            /// The dispatch by district toggle.
            /// </summary>
            public bool DispatchByDistrict = false;

            /// <summary>
            /// The dispatch by range toggle.
            /// </summary>
            public bool DispatchByRange = true;

            /// <summary>
            /// Limit too the closest service buildings when igoring range.
            /// </summary>
            public byte IgnoreRangeUseClosestBuildings = 0;

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
            private bool? canLimitOpportunisticCollectionValue = null;

            /// <summary>
            /// The remove from grid possible value.
            /// </summary>
            private bool? canRemoveFromGridValue = null;

            /// <summary>
            /// The checks preset settings value.
            /// </summary>
            private ServiceDispatcherSettings.BuildingCheckOrder checksPresetValue = ServiceDispatcherSettings.BuildingCheckOrder.InRange;

            /// <summary>
            /// The custom check parameters.
            /// </summary>
            private ServiceDispatcherSettings.BuildingCheckParameters[] customCheckParameters = Settings.DefaultCustomBuildingChecksParameters;

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
            private bool? useMinimumAmountForDispatch = null;

            /// <summary>
            /// The minimum amount for patrol usability value.
            /// </summary>
            private bool? useMinimumAmountForPatrol = null;

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
            /// Gets or sets the custom checks parameters.
            /// </summary>
            /// <value>
            /// The custom checks parameters.
            /// </value>
            public ServiceDispatcherSettings.BuildingCheckParameters[] ChecksCustom
            {
                get
                {
                    return this.customCheckParameters;
                }
                set
                {
                    if (value == null)
                    {
                        this.customCheckParameters = Settings.DefaultCustomBuildingChecksParameters;
                    }
                    else if (value.Length == 0)
                    {
                        this.customCheckParameters = Settings.DefaultCustomBuildingChecksParametersIfEmpty;
                    }
                    else
                    {
                        this.customCheckParameters = value.DistinctInOrder<ServiceDispatcherSettings.BuildingCheckParameters>().ToArray();
                    }
                }
            }

            /// <summary>
            /// Gets or sets the custom checks parameters as a string.
            /// </summary>
            /// <value>
            /// The custom checks parameters.
            /// </value>
            public string ChecksCustomString
            {
                get
                {
                    return ServiceDispatcherSettings.BuildingChecksPresetInfo.ToString(this.ChecksCustom);
                }
                set
                {
                    if (value == null)
                    {
                        this.ChecksCustom = null;
                    }
                    else if (value == String.Empty)
                    {
                        this.ChecksCustom = new ServiceDispatcherSettings.BuildingCheckParameters[0];
                    }
                    else
                    {
                        this.ChecksCustom = ServiceDispatcherSettings.BuildingChecksPresetInfo.ToArray(value);
                    }
                }
            }

            /// <summary>
            /// Gets the building checks parameters.
            /// </summary>
            /// <value>
            /// The checks parameters.
            /// </value>
            public ServiceDispatcherSettings.BuildingCheckParameters[] ChecksParameters
            {
                get
                {
                    return Settings.GetBuildingChecksParameters(this.checksPresetValue, this.ChecksCustom);
                }
            }

            /// <summary>
            /// Gets the building checks parameters as a string.
            /// </summary>
            /// <value>
            /// The building checks parameters.
            /// </value>
            public string ChecksParametersString
            {
                get
                {
                    return ServiceDispatcherSettings.BuildingChecksPresetInfo.ToString(this.ChecksParameters);
                }
            }

            /// <summary>
            /// Gets or sets the building checks preset.
            /// </summary>
            /// <value>
            /// The checks preset.
            /// </value>
            public ServiceDispatcherSettings.BuildingCheckOrder ChecksPreset
            {
                get
                {
                    return this.checksPresetValue;
                }

                set
                {
                    this.checksPresetValue = value;
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
            /// Gets a value indicating whether opportunistic collection limit detouring is allowed.
            /// </summary>
            /// <value>
            ///   <c>true</c> if opportunistic collection limit detouring is allowed; otherwise, <c>false</c>.
            /// </value>
            public bool OpportunisticCollectionLimitDetourAllowed
            {
                get
                {
                    return this.CanLimitOpportunisticCollection &&
                           (this.OpportunisticCollectionLimitDetour == Detours.Methods.None || Detours.CanDetour(this.OpportunisticCollectionLimitDetour));
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
            /// Logs the settings.
            /// </summary>
            public override void LogSettings()
            {
                base.LogSettings();

                Log.Debug(this, "LogSettings", this.VehicleNamePlural, this.MaterialName, "DispatchByDistrict", this.DispatchByDistrict);
                Log.Debug(this, "LogSettings", this.VehicleNamePlural, this.MaterialName, "DispatchByRange", this.DispatchByRange);
                Log.Debug(this, "LogSettings", this.VehicleNamePlural, this.MaterialName, "CreateSpares", this.CreateSpares);
                Log.Debug(this, "LogSettings", this.VehicleNamePlural, this.MaterialName, "ChecksPreset", (byte)this.ChecksPreset, this.ChecksPreset, GetBuildingCheckOrderName(this.ChecksPreset));
                Log.Debug(this, "LogSettings", this.VehicleNamePlural, this.MaterialName, "ChecksParameters", String.Join(", ", this.ChecksParameters.SelectToArray(bc => bc.ToString())));
                Log.Debug(this, "LogSettings", this.VehicleNamePlural, this.MaterialName, "ChecksCustom", this.ChecksCustomString);
                Log.Debug(this, "LogSettings", this.VehicleNamePlural, this.MaterialName, "IgnoreRangeUseClosestBuildings", this.IgnoreRangeUseClosestBuildings);
                Log.Debug(this, "LogSettings", this.VehicleNamePlural, this.MaterialName, "Patrol", this.Patrol);

                if (this.CanRemoveFromGrid)
                {
                    Log.Debug(this, "LogSettings", this.VehicleNamePlural, this.MaterialName, "RemoveFromGrid", this.RemoveFromGrid);
                }

                if (this.CanLimitOpportunisticCollection)
                {
                    Log.Debug(this, "LogSettings", this.VehicleNamePlural, this.MaterialName, "LimitOportunisticCollection", this.LimitOpportunisticCollection);
                }

                if (this.UseMinimumAmountForDispatch)
                {
                    Log.Debug(this, "LogSettings", this.VehicleNamePlural, this.MaterialName, "MinimumAmountForDispatch", this.MinimumAmountForDispatch);
                }

                if (this.UseMinimumAmountForPatrol)
                {
                    Log.Debug(this, "LogSettings", this.VehicleNamePlural, this.MaterialName, "MinimumAmountForPatrol", this.MinimumAmountForPatrol);
                }

                if (this.CanAutoEmpty)
                {
                    Log.Debug(this, "LogSettings", this.VehicleNamePlural, this.MaterialName, "AutoEmpty", this.AutoEmpty);
                    Log.Debug(this, "LogSettings", this.VehicleNamePlural, this.MaterialName, "AutoEmptyStartLevelPercent", this.AutoEmptyStartLevelPercent);
                    Log.Debug(this, "LogSettings", this.VehicleNamePlural, this.MaterialName, "AutoEmptyStopLevelPercent", this.AutoEmptyStopLevelPercent);
                }
            }
        }
    }
}