using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ColossalFramework.UI;
using ICities;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Mod interface.
    /// </summary>
    public class Mod : IUserMod
    {
        /// <summary>
        /// The allowances.
        /// </summary>
        private Dictionary<byte, string> allowances = null;

        /// <summary>
        /// The target building check strings for dropdown.
        /// </summary>
        private Dictionary<byte, string> targetBuildingChecks = null;

        /// <summary>
        /// The log controls are being updated.
        /// </summary>
        private bool updatingLogControls = false;

        /// <summary>
        /// The vehicle creation options strings for dropdown.
        /// </summary>
        private Dictionary<byte, string> vehicleCreationOptions = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mod"/> class.
        /// </summary>
        public Mod()
        {
            Log.NoOp();
            Detours.Create();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Mod"/> class.
        /// </summary>
        ~Mod()
        {
            Log.FlushBuffer();
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description
        {
            get
            {
                return Library.Description;
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get
            {
                return Library.Title;
            }
        }

        /// <summary>
        /// Called when mod is disabled.
        /// </summary>
        public void OnDisabled()
        {
            Log.Debug(this, "OnDisabled");
            Log.FlushBuffer();
        }

        /// <summary>
        /// Called when mod is enabled.
        /// </summary>
        public void OnEnabled()
        {
            Log.Debug(this, "OnEnabled");
            Log.FlushBuffer();
        }

        /// <summary>
        /// Called when initializing mod settings UI.
        /// </summary>
        /// <param name="helper">The helper.</param>
        public void OnSettingsUI(UIHelperBase helper)
        {
            Log.Debug(this, "OnSettingsUI");

            try
            {
                // Load settings.
                Global.InitializeSettings();

                this.InitializeOptions();

                // Add general dispatch group.
                UIHelperBase dispatchGroup = this.CreateDispatchGroup(helper);

                // Add hearse group.
                UIHelperBase hearseGroup = this.CreateServiceGroup(helper, Global.Settings.DeathCare, Global.HearseDispatcher, true);

                // Add cemetery group.
                if (Global.EnableExperiments || Global.Settings.DeathCare.AutoEmpty)
                {
                    UIHelperBase cemeteryGroup = this.CreateEmptiableServiceBuildingGroup(helper, Global.Settings.DeathCare, true);
                }

                // Add garbage group.
                UIHelperBase garbageGroup = this.CreateServiceGroup(helper, Global.Settings.Garbage, Global.GarbageTruckDispatcher, true);

                // Add landfill group.
                if (Global.EnableExperiments || Global.Settings.Garbage.AutoEmpty)
                {
                    UIHelperBase landfillGroup = this.CreateEmptiableServiceBuildingGroup(helper, Global.Settings.Garbage, true);
                }

                // Add ambulance group.
                if (Global.EnableExperiments || Global.Settings.HealthCare.DispatchVehicles)
                {
                    UIHelperBase ambulanceGroup = this.CreateServiceGroup(helper, Global.Settings.HealthCare, Global.AmbulanceDispatcher, true);
                }

                // Add bulldoze and recovery group.
                UIHelperBase wreckingRecoveryGroup = this.CreateWreckingRecoveryGroup(helper);

                // Add logging group.
                UIHelperBase logGroup = this.CreateLogGroup(helper);

                // Add Advanced group.
                UIHelperBase advancedGroup = this.CreateAdvancedGroup(helper);

                Log.FlushBuffer();
            }
            catch (System.Exception ex)
            {
                Log.Error(this, "OnSettingsUI", ex);
            }
        }

        /// <summary>
        /// Adds the hidden service controls.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="canService">If set to <c>true</c> this service can be enabled.</param>
        private void AddHiddenServiceControls(UIHelperBase group, Settings.HiddenServiceSettings settings, bool canService)
        {
            if (canService)
            {
                group.AddCheckbox(
                    "Dispatch " + settings.VehicleNamePlural.ToLower(),
                    settings.DispatchVehicles,
                    value =>
                    {
                        try
                        {
                            if (settings.DispatchVehicles != value)
                            {
                                settings.DispatchVehicles = value;
                                Global.Settings.Save();
                                Global.ReInitializeHandlers();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "CreateWreckingRecoveryGroup", ex, settings.VehicleNamePlural, "DispatchVehicles", value);
                        }
                    });
            }
            else
            {
                UIComponent checkBox = group.AddCheckbox(
                    "Dispatch " + settings.VehicleNamePlural.ToLower(),
                    false,
                    value =>
                    {
                    }) as UIComponent;
                checkBox.Disable();
            }

            group.AddExtendedSlider(
                settings.VehicleNameSingular + " delay",
                0.0f,
                60.0f * 24.0f,
                0.01f,
                (float)settings.DelayMinutes,
                true,
                value =>
                {
                    try
                    {
                        if (settings.DelayMinutes != (double)value)
                        {
                            settings.DelayMinutes = (double)value;
                            Global.BuildingUpdateNeeded = true;
                            Global.Settings.Save();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(this, "CreateWreckingRecoveryGroup", ex, settings.VehicleNamePlural, "DelayMinutes", value);
                    }
                });
        }

        /// <summary>
        /// Creates the advanced group.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <returns>The group.</returns>
        private UIHelperBase CreateAdvancedGroup(UIHelperBase helper)
        {
            try
            {
                UIHelperBase group = helper.AddGroup("Advanced");

                group.AddDropdown(
                    "Allow Code Overrides",
                    this.allowances.OrderBy(a => a.Key).Select(allowances => allowances.Value).ToArray(),
                    (int)Global.Settings.ReflectionAllowance,
                    value =>
                    {
                        try
                        {
                            foreach (Settings.Allowance allowance in Enum.GetValues(typeof(Settings.Allowance)))
                            {
                                if ((byte)allowance == value)
                                {
                                    if (allowance != Global.Settings.ReflectionAllowance)
                                    {
                                        if (Log.LogALot || Library.IsDebugBuild)
                                        {
                                            Log.Debug(this, "CreateAdvancedGroup", "Set", "ReflectionAllowance", value);
                                        }

                                        Global.Settings.ReflectionAllowance = allowance;
                                        Global.ReInitializeHandlers();
                                        Global.Settings.Save();
                                    }

                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "CreateAdvancedGroup", ex, "ReflectionAllowance", value);
                        }
                    });

                try
                {
                    group.AddInformationalText("Config Path:", FileSystem.FilePath);

                    Assembly modAss = this.GetType().Assembly;
                    if (modAss != null)
                    {
                        group.AddInformationalText("Mod Version:", modAss.GetName().Version.ToString());
                    }
                }
                catch
                {
                }

                group.AddButton(
                    "Dump buildings",
                    () =>
                    {
                        BuildingHelper.DumpBuildings();
                    });

                group.AddButton(
                    "Dump vehicles",
                    () =>
                    {
                        VehicleHelper.DumpVehicles();
                    });

                return group;
            }
            catch (Exception ex)
            {
                Log.Error(this, "CreateAdvancedGroup", ex);
                return null;
            }
        }

        /// <summary>
        /// Creates the dispatch group.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <returns>The group.</returns>
        private UIHelperBase CreateDispatchGroup(UIHelperBase helper)
        {
            try
            {
                UIHelperBase group = helper.AddGroup("Central Services Dispatch");

                group.AddCheckbox(
                    "Limit building ranges",
                    Global.Settings.RangeLimit,
                    value =>
                    {
                        try
                        {
                            if (Global.Settings.RangeLimit != value)
                            {
                                Global.BuildingUpdateNeeded = true;
                                Global.Settings.RangeLimit = value;
                                Global.Settings.Save();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "CreateDispatchGroup", ex, "RangeLimit", value);
                        }
                    });

                group.AddExtendedSlider(
                    "Range modifier",
                    0.1f,
                    10.0f,
                    0.1f,
                    Global.Settings.RangeModifier,
                    true,
                    "F1",
                    value =>
                    {
                        try
                        {
                            if (Global.Settings.RangeModifier != value)
                            {
                                Global.BuildingUpdateNeeded = true;
                                Global.Settings.RangeModifier = value;
                                Global.Settings.Save();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "CreateDispatchGroup", ex, "RangeModifier", value);
                        }
                    });

                group.AddExtendedSlider(
                    "Range minimum",
                    0f,
                    100000000f,
                    1f,
                    Global.Settings.RangeMinimum,
                    false,
                    value =>
                    {
                        try
                        {
                            if (Global.Settings.RangeMinimum != value)
                            {
                                Global.BuildingUpdateNeeded = true;
                                Global.Settings.RangeMinimum = value;
                                Global.Settings.Save();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "CreateDispatchGroup", ex, "RangeMinimum", value);
                        }
                    });

                group.AddExtendedSlider(
                    "Range maximum",
                    0f,
                    100000000f,
                    1f,
                    Global.Settings.RangeMaximum,
                    false,
                    value =>
                    {
                        try
                        {
                            if (Global.Settings.RangeMaximum != value)
                            {
                                Global.BuildingUpdateNeeded = true;
                                Global.Settings.RangeMaximum = value;
                                Global.Settings.Save();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "CreateDispatchGroup", ex, "RangeMaximum", value);
                        }
                    });

                return group;
            }
            catch (Exception ex)
            {
                Log.Error(this, "CreateDispatchGroup", ex);
                return null;
            }
        }

        /// <summary>
        /// Creates the group for service building that can be emptied.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="canService">If set to <c>true</c> this service can be enabled.</param>
        /// <returns>
        /// The group.
        /// </returns>
        private UIHelperBase CreateEmptiableServiceBuildingGroup(UIHelperBase helper, Settings.StandardServiceSettings settings, bool canService)
        {
            try
            {
                UIHelperBase group = helper.AddGroup(settings.EmptiableServiceBuildingNamePlural);

                if (canService)
                {
                    group.AddCheckbox(
                        "Empty " + settings.EmptiableServiceBuildingNamePlural.ToLower() + " automatically",
                        settings.AutoEmpty,
                        value =>
                        {
                            try
                            {
                                if (settings.AutoEmpty != value)
                                {
                                    settings.AutoEmpty = value;
                                    Global.Settings.Save();
                                    Global.ReInitializeHandlers();
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error(this, "CreateEmptiableServiceBuildingGroup", ex, settings.EmptiableServiceBuildingNamePlural, "AutoEmpty", value);
                            }
                        });
                }
                else
                {
                    UIComponent checkBox = group.AddCheckbox(
                        "Empty " + settings.EmptiableServiceBuildingNamePlural.ToLower() + " automatically",
                        false,
                        value =>
                        {
                        }) as UIComponent;
                    checkBox.Disable();
                }

                group.AddExtendedSlider(
                    "Start emptying at %",
                    0,
                    100,
                    1,
                    settings.AutoEmptyStartLevelPercent,
                    value =>
                    {
                        try
                        {
                            if (settings.AutoEmptyStartLevelPercent != (uint)value)
                            {
                                settings.AutoEmptyStartLevelPercent = (uint)value;
                                Global.BuildingUpdateNeeded = true;
                                Global.Settings.Save();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "CreateEmptiableServiceBuildingGroup", ex, settings.EmptiableServiceBuildingNamePlural, "AutoEmptyStartLevelPercent", value);
                        }
                    });

                group.AddExtendedSlider(
                    "Stop emptying at %",
                    0,
                    100,
                    1,
                    settings.AutoEmptyStopLevelPercent,
                    value =>
                    {
                        try
                        {
                            if (settings.AutoEmptyStopLevelPercent != (uint)value)
                            {
                                settings.AutoEmptyStopLevelPercent = (uint)value;
                                Global.BuildingUpdateNeeded = true;
                                Global.Settings.Save();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "CreateEmptiableServiceBuildingGroup", ex, settings.EmptiableServiceBuildingNamePlural, "AutoEmptyStopLevelPercent", value);
                        }
                    });

                return group;
            }
            catch (Exception ex)
            {
                Log.Error(this, "CreateEmptiableServiceBuildingGroup", ex, settings.EmptiableServiceBuildingNamePlural);
                return null;
            }
        }

        /// <summary>
        /// Creates the log group.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <returns>The group.</returns>
        private UIHelperBase CreateLogGroup(UIHelperBase helper)
        {
            try
            {
                UIHelperBase group = helper.AddGroup("Logging (not saved in settings)");

                UIComponent debugLog = null;
                UIComponent devLog = null;
                UIComponent objectLog = null;
                UIComponent namesLog = null;
                UIComponent fileLog = null;

                debugLog = group.AddCheckbox(
                    "Debug log",
                    Log.LogDebug,
                    value =>
                    {
                        try
                        {
                            if (!this.updatingLogControls)
                            {
                                Log.LogDebug = value;
                                UpdateLogControls(debugLog, devLog, objectLog, namesLog, fileLog);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "CreateLogGroup", ex, "LogDebug", value);
                        }
                    }) as UIComponent;

                devLog = group.AddCheckbox(
                    "Developer log",
                    Log.LogALot,
                    value =>
                    {
                        try
                        {
                            if (!this.updatingLogControls)
                            {
                                Log.LogALot = value;
                                UpdateLogControls(debugLog, devLog, objectLog, namesLog, fileLog);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "CreateLogGroup", ex, "LogALot", value);
                        }
                    }) as UIComponent;

                objectLog = group.AddCheckbox(
                    "Object list log",
                    Log.LogDebugLists,
                    value =>
                    {
                        try
                        {
                            if (!this.updatingLogControls)
                            {
                                Log.LogDebugLists = value;
                                UpdateLogControls(debugLog, devLog, objectLog, namesLog, fileLog);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "CreateLogGroup", ex, "LogDebugLists", value);
                        }
                    }) as UIComponent;

                namesLog = group.AddCheckbox(
                    "Log names",
                    Log.LogNames,
                    value =>
                    {
                        try
                        {
                            if (!this.updatingLogControls)
                            {
                                Log.LogNames = value;
                                UpdateLogControls(debugLog, devLog, objectLog, namesLog, fileLog);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "CreateLogGroup", ex, "LogNames", value);
                        }
                    }) as UIComponent;

                fileLog = group.AddCheckbox(
                    "Log to file",
                    Log.LogToFile,
                    value =>
                    {
                        try
                        {
                            if (!this.updatingLogControls)
                            {
                                Log.LogToFile = value;
                                UpdateLogControls(debugLog, devLog, objectLog, namesLog, fileLog);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "CreateLogGroup", ex, "LogToFile", value);
                        }
                    }) as UIComponent;

                return group;
            }
            catch (Exception ex)
            {
                Log.Error(this, "CreateLogGroup", ex);
                return null;
            }
        }

        /// <summary>
        /// Creates the service group.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <param name="canService">If set to <c>true</c> this service can be enabled.</param>
        /// <returns>
        /// The group.
        /// </returns>
        private UIHelperBase CreateServiceGroup(UIHelperBase helper, Settings.StandardServiceSettings settings, Dispatcher dispatcher, bool canService)
        {
            try
            {
                // Add cemetery group.
                UIHelperBase group = helper.AddGroup(settings.EmptiableServiceBuildingNamePlural);

                if (canService)
                {
                    group.AddCheckbox(
                        "Dispatch " + settings.VehicleNamePlural.ToLower(),
                        settings.DispatchVehicles,
                        value =>
                        {
                            try
                            {
                                if (settings.DispatchVehicles != value)
                                {
                                    settings.DispatchVehicles = value;
                                    Global.Settings.Save();
                                    Global.ReInitializeHandlers();
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error(this, "CreateServiceGroup", ex, settings.VehicleNamePlural, "DispatchVehicles", value);
                            }
                        });
                }
                else
                {
                    UIComponent checkBox = group.AddCheckbox(
                        "Dispatch " + settings.VehicleNamePlural.ToLower(),
                        false,
                        value =>
                        {
                        }) as UIComponent;
                }

                group.AddCheckbox(
                    "Dispatch by district",
                    settings.DispatchByDistrict,
                    value =>
                    {
                        try
                        {
                            if (settings.DispatchByDistrict != value)
                            {
                                settings.DispatchByDistrict = value;
                                Global.BuildingUpdateNeeded = true;
                                Global.Settings.Save();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "CreateServiceGroup", ex, settings.VehicleNamePlural, "DispatchByDistrict", value);
                        }
                    });

                group.AddCheckbox(
                    "Dispatch by building range",
                    settings.DispatchByRange,
                    value =>
                    {
                        try
                        {
                            if (settings.DispatchByRange != value)
                            {
                                settings.DispatchByRange = value;
                                Global.BuildingUpdateNeeded = true;
                                Global.Settings.Save();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "CreateServiceGroup", ex, settings.VehicleNamePlural, "DispatchByRange", value);
                        }
                    });

                if (settings.CanRemoveFromGrid)
                {
                    group.AddCheckbox(
                        "Pass through " + settings.VehicleNamePlural.ToLower(),
                        settings.RemoveFromGrid,
                        value =>
                        {
                            try
                            {
                                if (settings.RemoveFromGrid != value)
                                {
                                    settings.RemoveFromGrid = value;
                                    Global.Settings.Save();
                                    Global.ReInitializeHandlers();
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error(this, "CreateServiceGroup", ex, settings.VehicleNamePlural, "RemoveFromGrid", value);
                            }
                        });
                }

                if (settings.CanLimitOpportunisticCollection &&
                    (settings.OpportunisticCollectionLimitDetour == Detours.Methods.None || Detours.CanDetour(settings.OpportunisticCollectionLimitDetour)))
                {
                    group.AddCheckbox(
                        "Prioritize assigned buildings",
                        settings.LimitOpportunisticCollection,
                        value =>
                        {
                            try
                            {
                                if (settings.LimitOpportunisticCollection != value)
                                {
                                    settings.LimitOpportunisticCollection = value;
                                    Detours.InitNeeded = true;
                                    Global.Settings.Save();
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error(this, "OnSettingsUI", ex, "GarbageGroup", "LimitOpportunisticGarbageCollection", value);
                                Log.Error(this, "CreateServiceGroup", ex, settings.VehicleNamePlural, "LimitOpportunisticCollection");
                            }
                        });
                }
                else
                {
                    UIComponent limitOpportunisticGarbageCollectionCheckBox = group.AddCheckbox(
                        "Prioritize assigned buildings",
                        false,
                        value =>
                        {
                        }) as UIComponent;
                    limitOpportunisticGarbageCollectionCheckBox.Disable();
                }

                group.AddDropdown(
                    "Send out spare " + settings.VehicleNamePlural.ToLower() + " when",
                    this.vehicleCreationOptions.OrderBy(vco => vco.Key).Select(vco => vco.Value).ToArray(),
                    (int)settings.CreateSpares,
                    value =>
                    {
                        try
                        {
                            if (Log.LogALot || Library.IsDebugBuild)
                            {
                                Log.Debug(this, "CreateServiceGroup", "Set", settings.VehicleNamePlural, "CreateSpares", value);
                            }

                            foreach (Settings.SpareVehiclesCreation option in Enum.GetValues(typeof(Settings.SpareVehiclesCreation)))
                            {
                                if ((byte)option == value)
                                {
                                    if (settings.CreateSpares != option)
                                    {
                                        if (Log.LogALot || Library.IsDebugBuild)
                                        {
                                            Log.Debug(this, "CreateServiceGroup", "Set", settings.VehicleNamePlural, "CreateSpares", value, option);
                                        }

                                        settings.CreateSpares = option;

                                        if (dispatcher != null)
                                        {
                                            try
                                            {
                                                dispatcher.ReInitialize();
                                            }
                                            catch (Exception ex)
                                            {
                                                Log.Error(this, "CreateServiceGroup", ex, settings.VehicleNamePlural, "CreateSpares", "ReInitializeDispatcher");
                                            }
                                        }

                                        Global.Settings.Save();
                                    }

                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "CreateServiceGroup", ex, settings.VehicleNamePlural, "CreateSpares", value);
                        }
                    });

                group.AddDropdown(
                    settings.VehicleNameSingular + " dispatch strategy",
                    this.targetBuildingChecks.OrderBy(bco => bco.Key).Select(bco => bco.Value).ToArray(),
                    (int)settings.ChecksPreset,
                    value =>
                    {
                        try
                        {
                            if (Log.LogALot || Library.IsDebugBuild)
                            {
                                Log.Debug(this, "CreateServiceGroup", "Set", settings.VehicleNamePlural, "ChecksPreset", value);
                            }

                            foreach (Settings.BuildingCheckOrder checks in Enum.GetValues(typeof(Settings.BuildingCheckOrder)))
                            {
                                if ((byte)checks == value)
                                {
                                    if (settings.ChecksPreset != checks)
                                    {
                                        if (Log.LogALot || Library.IsDebugBuild)
                                        {
                                            Log.Debug(this, "CreateServiceGroup", "Set", settings.VehicleNamePlural, "ChecksPreset", value, checks);
                                        }

                                        try
                                        {
                                            settings.ChecksPreset = checks;
                                        }
                                        catch (Exception ex)
                                        {
                                            Log.Error(this, "OnSettingsUI", ex, "Set", "DeathChecksPreset", checks);
                                        }

                                        if (dispatcher != null)
                                        {
                                            try
                                            {
                                                dispatcher.ReInitialize();
                                            }
                                            catch (Exception ex)
                                            {
                                                Log.Error(this, "CreateServiceGroup", ex, settings.VehicleNamePlural, "ChecksPreset", "ReInitializeDispatcher");
                                            }
                                        }

                                        Global.Settings.Save();
                                    }

                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "CreateServiceGroup", ex, settings.VehicleNamePlural, "ChecksPreset", value);
                        }
                    });

                if (settings.UseMinimumAmountForPatrol)
                {
                    group.AddExtendedSlider(
                        settings.MaterialName + " patrol limit",
                        1.0f,
                        5000.0f,
                        1.0f,
                        settings.MinimumAmountForPatrol,
                        false,
                        value =>
                        {
                            try
                            {
                                if (settings.MinimumAmountForPatrol != (ushort)value)
                                {
                                    settings.MinimumAmountForPatrol = (ushort)value;
                                    Global.BuildingUpdateNeeded = true;
                                    Global.Settings.Save();
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error(this, "CreateServiceGroup", ex, settings.VehicleNamePlural, "MinimumAmountForPatrol", value);
                            }
                        });
                }

                if (settings.UseMinimumAmountForDispatch)
                {
                    group.AddExtendedSlider(
                        settings.MaterialName + " dispatch limit",
                        1.0f,
                        5000.0f,
                        1.0f,
                        settings.MinimumAmountForDispatch,
                        false,
                        value =>
                        {
                            try
                            {
                                if (settings.MinimumAmountForDispatch != (ushort)value)
                                {
                                    settings.MinimumAmountForDispatch = (ushort)value;
                                    Global.BuildingUpdateNeeded = true;
                                    Global.Settings.Save();
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error(this, "CreateServiceGroup", ex, settings.VehicleNamePlural, "MinimumAmountForDispatch", value);
                            }
                        });
                }

                return group;
            }
            catch (Exception ex)
            {
                Log.Error(this, "CreateServiceGroup", ex, settings.EmptiableServiceBuildingNamePlural);
                return null;
            }
        }

        /// <summary>
        /// Creates the wrecking and recovery group.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <returns>The group.</returns>
        private UIHelperBase CreateWreckingRecoveryGroup(UIHelperBase helper)
        {
            try
            {
                UIHelperBase group = helper.AddGroup("Wrecking & Recovery");

                this.AddHiddenServiceControls(group, Global.Settings.WreckingCrews, BulldozeHelper.CanBulldoze);

                group.AddInformationalText("Experimental Recovery Services:", "The recovery services are experimental, and might create more problems than they solve.");

                this.AddHiddenServiceControls(group, Global.Settings.RecoveryCrews, true);

                return group;
            }
            catch (Exception ex)
            {
                Log.Error(this, "CreateWreckingRecoveryGroup", ex);
                return null;
            }
        }

        /// <summary>
        /// Initializes the options.
        /// </summary>
        private void InitializeOptions()
        {
            if (this.targetBuildingChecks == null)
            {
                this.targetBuildingChecks = new Dictionary<byte, string>();
                foreach (Settings.BuildingCheckOrder checks in Enum.GetValues(typeof(Settings.BuildingCheckOrder)))
                {
                    if (Log.LogALot || Library.IsDebugBuild)
                    {
                        Log.Debug(this, "OnSettingsUI", "Init", "BuildingCheckOrder", (byte)checks, checks, Settings.GetBuildingCheckOrderName(checks));
                    }

                    string name = Settings.GetBuildingCheckOrderName(checks);
                    if (String.IsNullOrEmpty(name))
                    {
                        name = checks.ToString();
                    }

                    this.targetBuildingChecks.Add((byte)checks, name);
                }
            }

            if (this.vehicleCreationOptions == null)
            {
                this.vehicleCreationOptions = new Dictionary<byte, string>();
                foreach (Settings.SpareVehiclesCreation option in Enum.GetValues(typeof(Settings.SpareVehiclesCreation)))
                {
                    string name = Settings.GetSpareVehiclesCreationName(option);
                    if (String.IsNullOrEmpty(name))
                    {
                        name = option.ToString();
                    }

                    this.vehicleCreationOptions.Add((byte)option, name);
                }
            }

            if (this.allowances == null)
            {
                this.allowances = new Dictionary<byte, string>();
                foreach (Settings.Allowance allowance in Enum.GetValues(typeof(Settings.Allowance)))
                {
                    string name = Settings.GetAllowanceName(allowance);
                    if (String.IsNullOrEmpty(name))
                    {
                        name = allowance.ToString();
                    }

                    this.allowances.Add((byte)allowance, name);
                }
            }
        }

        /// <summary>
        /// Updates the log controls.
        /// </summary>
        /// <param name="debugLog">The debug log control.</param>
        /// <param name="devLog">The developer log control.</param>
        /// <param name="objectLog">The object log control.</param>
        /// <param name="namesLog">The names log control.</param>
        /// <param name="fileLog">The file log control.</param>
        private void UpdateLogControls(UIComponent debugLog, UIComponent devLog, UIComponent objectLog, UIComponent namesLog, UIComponent fileLog)
        {
            try
            {
                this.updatingLogControls = true;

                if (debugLog != null && debugLog is UICheckBox)
                {
                    ((UICheckBox)debugLog).isChecked = Log.LogDebug;
                }

                if (devLog != null && devLog is UICheckBox)
                {
                    ((UICheckBox)devLog).isChecked = Log.LogALot;
                }

                if (objectLog != null && objectLog is UICheckBox)
                {
                    ((UICheckBox)objectLog).isChecked = Log.LogDebugLists;
                }

                if (namesLog != null && namesLog is UICheckBox)
                {
                    ((UICheckBox)namesLog).isChecked = Log.LogNames;
                }

                if (fileLog != null && fileLog is UICheckBox)
                {
                    ((UICheckBox)fileLog).isChecked = Log.LogToFile;
                }
            }
            catch
            {
            }
            finally
            {
                this.updatingLogControls = false;
            }
        }
    }
}