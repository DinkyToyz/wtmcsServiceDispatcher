using System.Collections.Generic;

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
        public readonly StandardServiceSettings DeathCare = new StandardServiceSettings(SerializableSettings.ServiceType.DeathCare);

        /// <summary>
        /// The garbage settings.
        /// </summary>
        public readonly StandardServiceSettings Garbage = new StandardServiceSettings(SerializableSettings.ServiceType.Garbage);

        /// <summary>
        /// The health-care settings.
        /// </summary>
        public readonly StandardServiceSettings HealthCare = new StandardServiceSettings(SerializableSettings.ServiceType.HealthCare);

        /// <summary>
        /// The recovery crews settings.
        /// </summary>
        public readonly HiddenServiceSettings RecoveryCrews = new HiddenServiceSettings(SerializableSettings.ServiceType.RecoveryCrews);

        /// <summary>
        /// Whether to load settings per city or not.
        /// </summary>
        public readonly bool LoadSettingsPerCity = false;

        /// <summary>
        /// The wrecking crews settings.
        /// </summary>
        public readonly HiddenServiceSettings WreckingCrews = new HiddenServiceSettings(SerializableSettings.ServiceType.WreckingCrews);

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
        /// Gets a value indicating whether to automatically empty any buildings.
        /// </summary>
        /// <value>
        ///   <c>true</c> if automatically emptying any buildings; otherwise, <c>false</c>.
        /// </value>
        public bool AutoEmptyAnyBuildings => this.Garbage.AutoEmpty || this.DeathCare.AutoEmpty || this.HealthCare.AutoEmpty;

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
        public bool DispatchAnyVehicles => this.Garbage.DispatchVehicles || this.DeathCare.DispatchVehicles || this.HealthCare.DispatchVehicles;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Settings"/> has been loaded from save.
        /// </summary>
        /// <value>
        ///   <c>true</c> if loaded; otherwise, <c>false</c>.
        /// </value>
        public bool Loaded => this.loadedVersion != null && this.loadedVersion.HasValue && this.loadedVersion.Value >= 0;

        /// <summary>
        /// Gets the settings version in the loaded file.
        /// </summary>
        /// <value>
        /// The settings version in the loaded file.
        /// </value>
        public int LoadedVersion => (this.loadedVersion == null || !this.loadedVersion.HasValue) ? 0 : this.loadedVersion.Value;

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

            Log.Debug(this, "LogSettings", ServiceDispatcherSettings.CurrentVersion, this.LoadedVersion, this.SaveCount);
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

            this.SaveCount++;
            ServiceDispatcherSettings.Save(fileName, this);
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
    }
}