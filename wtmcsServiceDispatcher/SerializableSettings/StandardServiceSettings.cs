using System;
using System.Linq;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Settings for normal services.
    /// </summary>
    public class StandardServiceSettings : ServiceSettings
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
        /// The checks custom string error message;
        /// </summary>
        private string checksCustomStringError = null;

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
                this.ChecksCustom = ServiceDispatcherSettings.BuildingChecksPresetInfo.ToArray(value, out this.checksCustomStringError);
            }
        }

        /// <summary>
        /// Gets the custom checks string error message.
        /// </summary>
        /// <value>
        /// The custom checks string error message.
        /// </value>
        public string ChecksCustomStringErrorMessage => this.checksCustomStringError;

        /// <summary>
        /// Gets a value indicating whether there were errors on custom checks string.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the custom checks string had error; otherwise, <c>false</c>.
        /// </value>
        public bool ChecksCustomStringHadError => this.checksCustomStringError != null;

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
            Log.Debug(this, "LogSettings", this.VehicleNamePlural, this.MaterialName, "ChecksPreset", (byte)this.ChecksPreset, this.ChecksPreset, Settings.GetBuildingCheckOrderName(this.ChecksPreset));
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