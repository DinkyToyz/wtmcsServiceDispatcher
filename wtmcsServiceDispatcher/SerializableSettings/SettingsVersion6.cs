using System;
using System.Linq;
using WhatThe.Mods.CitiesSkylines.ServiceDispatcher;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.SerializableSettings
{
    /// <summary>
    /// Serializable settings class, version 6.
    /// </summary>
    [Serializable]
    public class SettingsVersion6 : ISerializableSettings
    {
        /// <summary>
        /// The standard ambulance service settings.
        /// </summary>
        public StandardServiceConfig Ambulances = new StandardServiceConfig();

        /// <summary>
        /// The automatic bulldozee service settings.
        /// </summary>
        public HiddenServiceConfig AutoBulldoze = new HiddenServiceConfig();

        /// <summary>
        /// Automatic cemetery emptying.
        /// </summary>
        public AutoEmptyServiceConfig AutoEmptyCemeteries = new AutoEmptyServiceConfig();

        /// <summary>
        /// Automatic landfill emptying.
        /// </summary>
        public AutoEmptyServiceConfig AutoEmptyLandfills = new AutoEmptyServiceConfig();

        /// <summary>
        /// The compatibility configuration.
        /// </summary>
        public CompatibilityConfig Compatibility = new SettingsVersion6.CompatibilityConfig();

        /// <summary>
        /// The standard garbage truck service settings.
        /// </summary>
        public StandardServiceConfig GarbageTrucks = new StandardServiceConfig();

        /// <summary>
        /// The standard hearse service settings.
        /// </summary>
        public StandardServiceConfig Hearses = new StandardServiceConfig();

        /// <summary>
        /// The information.
        /// </summary>
        public Information Info = new Information();

        /// <summary>
        /// The building range configuration.
        /// </summary>
        public DispatchConfig Dispatch = new DispatchConfig();

        /// <summary>
        /// Automatic removal of stuck vehicles servic settings.
        /// </summary>
        public HiddenServiceConfig RemoveStuckVehicles = new HiddenServiceConfig();

        /// <summary>
        /// The save count.
        /// </summary>
        public uint SaveCount = 0;

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
        public static int CurrentVersion => 6;

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
        public int MaxVersion => 6;

        /// <summary>
        /// Gets the minimum version.
        /// </summary>
        /// <value>
        /// The minimum version.
        /// </value>
        public int MinVersion => 6;

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

            this.Compatibility.GetSettings(settings);
            this.Dispatch.GetSettings(settings);

            this.Hearses.GetSettings(settings.DeathCare);
            this.Ambulances.GetSettings(settings.HealthCare);
            this.GarbageTrucks.GetSettings(settings.Garbage);

            this.AutoEmptyCemeteries.GetSettings(settings.DeathCare);
            this.AutoEmptyLandfills.GetSettings(settings.Garbage);

            this.AutoBulldoze.GetSettings(settings.WreckingCrews);
            this.RemoveStuckVehicles.GetSettings(settings.RecoveryCrews);

            settings.Initialize();

            return settings;
        }

        /// <summary>
        /// Initializes the settings.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Sets the settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public void SetSettings(Settings settings)
        {
            this.Compatibility.SetSettings(settings);
            this.Dispatch.SetSettings(settings);

            this.Hearses.SetSettings(settings.DeathCare);
            this.Ambulances.SetSettings(settings.HealthCare);
            this.GarbageTrucks.SetSettings(settings.Garbage);

            this.AutoEmptyCemeteries.SetSettings(settings.DeathCare);
            this.AutoEmptyLandfills.SetSettings(settings.Garbage);

            this.AutoBulldoze.SetSettings(settings.WreckingCrews);
            this.RemoveStuckVehicles.SetSettings(settings.RecoveryCrews);

            this.Info.SetSettings(settings);

            this.Version = CurrentVersion;
            this.SaveCount = settings.SaveCount;
        }

        /// <summary>
        /// The auto-emptying service settings.
        /// </summary>
        [Serializable]
        public class AutoEmptyServiceConfig
        {
            /// <summary>
            /// The service is enabled.
            /// </summary>
            public bool Enabled = false;

            /// <summary>
            /// The automatic emptying start level.
            /// </summary>
            public uint StartLevelPercent = 95u;

            /// <summary>
            /// The automatic empty cemetery stop level.
            /// </summary>
            public uint StopLevelPercent = 5u;

            /// <summary>
            /// Gets the settings from the specified configuration.
            /// </summary>
            /// <param name="settings">The settings.</param>
            internal void GetSettings(StandardServiceSettings settings)
            {
                settings.DispatchVehicles = this.Enabled;
                settings.AutoEmptyStartLevelPercent = this.StartLevelPercent;
                settings.AutoEmptyStopLevelPercent = this.StopLevelPercent;
            }

            /// <summary>
            /// Sets the specified configuration for the values from the sepcified settings.
            /// </summary>
            /// <param name="settings">The settings.</param>
            internal void SetSettings(StandardServiceSettings settings)
            {
                this.Enabled = settings.DispatchVehicles;
                this.StartLevelPercent = settings.AutoEmptyStartLevelPercent;
                this.StopLevelPercent = settings.AutoEmptyStopLevelPercent;
            }
        }

        /// <summary>
        /// The compatibiloty configuration settings.
        /// </summary>
        [Serializable]
        public class CompatibilityConfig
        {
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
            /// Whether code overrides are allowed or not.
            /// </summary>
            public ServiceDispatcherSettings.Allowance ReflectionAllowance = ServiceDispatcherSettings.Allowance.Default;

            /// <summary>
            /// Gets the settings from the specified configuration.
            /// </summary>
            /// <param name="settings">The settings.</param>
            internal void GetSettings(Settings settings)
            {
                settings.ReflectionAllowance = this.ReflectionAllowance;
                settings.BlockTransferManagerOffers = this.BlockTransferManagerOffers;
                settings.AssignmentCompatibilityMode = this.AssignmentCompatibilityMode;
                settings.CreationCompatibilityMode = this.CreationCompatibilityMode;
            }

            /// <summary>
            /// Sets the specified configuration for the values from the specified settings.
            /// </summary>
            /// <param name="settings">The settings.</param>
            internal void SetSettings(Settings settings)
            {
                this.ReflectionAllowance = settings.ReflectionAllowance;
                this.BlockTransferManagerOffers = settings.BlockTransferManagerOffers;
                this.AssignmentCompatibilityMode = settings.AssignmentCompatibilityMode;
                this.CreationCompatibilityMode = settings.CreationCompatibilityMode;
            }
        }

        /// <summary>
        /// The hidden service settings.
        /// </summary>
        [Serializable]
        public class HiddenServiceConfig
        {
            /// <summary>
            /// The service delay in seconds;
            /// </summary>
            public double DelaySeconds = 5.0 * 60.0;

            /// <summary>
            /// The service is enabled.
            /// </summary>
            public bool Enabled = false;

            /// <summary>
            /// Gets the settings from the specified configuration.
            /// </summary>
            /// <param name="settings">The settings.</param>
            internal void GetSettings(HiddenServiceSettings settings)
            {
                settings.DispatchVehicles = this.Enabled;
                settings.DelaySeconds = this.DelaySeconds;
            }

            /// <summary>
            /// Sets the specified configuration for the values from the sepcified settings.
            /// </summary>
            /// <param name="settings">The settings.</param>
            internal void SetSettings(HiddenServiceSettings settings)
            {
                this.Enabled = settings.DispatchVehicles;
                this.DelaySeconds = settings.DelaySeconds;
            }
        }

        /// <summary>
        /// The settings information.
        /// </summary>
        [Serializable]
        public class Information
        {
            /// <summary>
            /// The possible building checks.
            /// </summary>
            public ServiceDispatcherSettings.BuildingCheckParameters[] BuildingChecksPossible = (Enum.GetValues(typeof(ServiceDispatcherSettings.BuildingCheckParameters)) as ServiceDispatcherSettings.BuildingCheckParameters[]).Where(bcp => bcp != ServiceDispatcherSettings.BuildingCheckParameters.Undefined).ToArray();

            /// <summary>
            /// The possible building checks presets.
            /// </summary>
            public ServiceDispatcherSettings.BuildingChecksPresetInfo[] BuildingChecksPresets = (Enum.GetValues(typeof(ServiceDispatcherSettings.BuildingCheckOrder)) as ServiceDispatcherSettings.BuildingCheckOrder[]).Where(bco => bco != ServiceDispatcherSettings.BuildingCheckOrder.Custom).Select(bco => new ServiceDispatcherSettings.BuildingChecksPresetInfo(bco)).ToArray();

            /// <summary>
            /// Sets the specified information from the specified settings.
            /// </summary>
            /// <param name="settings">The settings.</param>
            internal void SetSettings(Settings settings)
            { }
        }

        /// <summary>
        /// The range configuration settings.
        /// </summary>
        [Serializable]
        public class DispatchConfig
        {
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
            /// Gets the settings from the specified configuration.
            /// </summary>
            /// <param name="settings">The settings.</param>
            internal void GetSettings(Settings settings)
            {
                settings.RangeModifier = this.RangeModifier;
                settings.RangeLimit = this.RangeLimit;
                settings.RangeMaximum = this.RangeMaximum;
                settings.RangeMinimum = this.RangeMinimum;
            }

            /// <summary>
            /// Sets the specified configuration for the values from the specified settings.
            /// </summary>
            /// <param name="settings">The settings.</param>
            internal void SetSettings(Settings settings)
            {
                this.RangeModifier = settings.RangeModifier;
                this.RangeLimit = settings.RangeLimit;
                this.RangeMaximum = settings.RangeMaximum;
                this.RangeMinimum = settings.RangeMinimum;
            }
        }

        /// <summary>
        /// The standard service settings.
        /// </summary>
        [Serializable]
        public class StandardServiceConfig
        {
            /// <summary>
            /// The dead people building checks presets.
            /// </summary>
            public ServiceDispatcherSettings.BuildingCheckOrder ChecksPreset = ServiceDispatcherSettings.BuildingCheckOrder.InRange;

            /// <summary>
            /// When to create spare vehicles.
            /// </summary>
            public ServiceDispatcherSettings.SpareVehiclesCreation CreateSpareVehicles = ServiceDispatcherSettings.SpareVehiclesCreation.WhenBuildingIsCloser;

            /// <summary>
            /// The current dead people building checks.
            /// </summary>
            public ServiceDispatcherSettings.BuildingCheckParameters[] CurrentChecks = null;

            /// <summary>
            /// The custom hearse building checks.
            /// </summary>
            public ServiceDispatcherSettings.BuildingCheckParameters[] CustomChecks = null;

            /// <summary>
            /// Whether the dispatch should be limited by district or not.
            /// </summary>
            public Boolean DispatchByDistrict = false;

            /// <summary>
            /// Limit service building range for target buildings without problems.
            /// </summary>
            public bool DispatchByRange = false;

            /// <summary>
            /// The service is enabled.
            /// </summary>
            public bool Enabled = false;

            /// <summary>
            /// Limit too the closest service buildings when igoring range.
            /// </summary>
            public byte IgnoreRangeUseClosestBuilding = 0;

            /// <summary>
            /// Limit opportunistic collection.
            /// </summary>
            public bool LimitOpportunisticCollection = true;

            /// <summary>
            /// The minimum amount of something to dispatch a vehicle for.
            /// </summary>
            public ushort MinimumAmountForDispatch = 1500;

            /// <summary>
            /// The minimum amount of something to direct a patrolling vehicle for.
            /// </summary>
            public ushort MinimumGarbageForPatrol = 200;

            /// <summary>
            /// Whether stopped vehicles should be removed from grid or not.
            /// </summary>
            public bool RemoveVehiclesFromGrid = false;

            /// <summary>
            /// Gets the settings from the specified configuration.
            /// </summary>
            /// <param name="settings">The settings.</param>
            internal void GetSettings(StandardServiceSettings settings)
            {
                settings.DispatchVehicles = this.Enabled;
                settings.ChecksPreset = this.ChecksPreset;
                settings.ChecksCustom = this.CustomChecks;
                settings.CreateSpares = this.CreateSpareVehicles;
                settings.DispatchByDistrict = this.DispatchByDistrict;
                settings.DispatchByRange = this.DispatchByRange;
                settings.IgnoreRangeUseClosestBuildings = this.IgnoreRangeUseClosestBuilding;

                settings.RemoveFromGrid = settings.CanRemoveFromGrid && this.RemoveVehiclesFromGrid;
                settings.LimitOpportunisticCollection = settings.CanLimitOpportunisticCollection && this.LimitOpportunisticCollection;

                settings.MinimumAmountForDispatch = settings.UseMinimumAmountForDispatch ? this.MinimumAmountForDispatch : (ushort)0;
                settings.MinimumAmountForPatrol = settings.UseMinimumAmountForPatrol ? this.MinimumGarbageForPatrol : (ushort)0;
            }

            /// <summary>
            /// Sets the specified configuration for the values from the sepcified settings.
            /// </summary>
            /// <param name="settings">The settings.</param>
            internal void SetSettings(StandardServiceSettings settings)
            {
                this.Enabled = settings.DispatchVehicles;
                this.ChecksPreset = settings.ChecksPreset;
                this.CurrentChecks = settings.ChecksParameters;
                this.CustomChecks = settings.ChecksCustom;
                this.CreateSpareVehicles = settings.CreateSpares;
                this.DispatchByDistrict = settings.DispatchByDistrict;
                this.DispatchByRange = settings.DispatchByRange;
                this.IgnoreRangeUseClosestBuilding = settings.IgnoreRangeUseClosestBuildings;

                this.RemoveVehiclesFromGrid = settings.CanRemoveFromGrid && settings.RemoveFromGrid;
                this.LimitOpportunisticCollection = settings.CanLimitOpportunisticCollection && settings.LimitOpportunisticCollection;

                this.MinimumAmountForDispatch = settings.UseMinimumAmountForDispatch ? settings.MinimumAmountForDispatch : (ushort)0;
                this.MinimumGarbageForPatrol = settings.UseMinimumAmountForPatrol ? settings.MinimumAmountForPatrol : (ushort)0;
            }
        }
    }
}