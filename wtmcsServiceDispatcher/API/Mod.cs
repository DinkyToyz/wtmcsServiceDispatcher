using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        /// The allowances.
        /// </summary>
        private Dictionary<byte, string> modCompatibilityModes = null;

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
                UIHelperBase cemeteryGroup = this.CreateEmptiableServiceBuildingGroup(helper, Global.Settings.DeathCare, true);

                // Add garbage group.
                UIHelperBase garbageGroup = this.CreateServiceGroup(helper, Global.Settings.Garbage, Global.GarbageTruckDispatcher, true);

                // Add landfill group.
                UIHelperBase landfillGroup = this.CreateEmptiableServiceBuildingGroup(helper, Global.Settings.Garbage, true);

                // Add ambulance group.
                if (Global.EnableDevExperiments || Global.Settings.HealthCare.DispatchVehicles)
                {
                    UIHelperBase ambulanceGroup = this.CreateServiceGroup(helper, Global.Settings.HealthCare, Global.AmbulanceDispatcher, true);
                }

                // Add bulldoze and recovery group.
                UIHelperBase wreckingRecoveryGroup = this.CreateWreckingRecoveryGroup(helper);

                // Add compatibility group.
                UIHelperBase compatibilityGroup = this.CreateCompatibilityGroup(helper);

                // Add logging group.
                UIHelperBase logGroup = this.CreateLogGroup(helper);

                // Add miscellaneous group.
                UIHelperBase miscellaneousGroup = this.CreateMiscellaneousGroup(helper);

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
        private void AddHiddenServiceControls(UIHelperBase group, HiddenServiceSettings settings, bool canService)
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
        /// Creates the compatibility group.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <returns>The group.</returns>
        private UIHelperBase CreateCompatibilityGroup(UIHelperBase helper)
        {
            try
            {
                UIHelperBase group = helper.AddGroup("Compatibility");

                if (Settings.AboveAboveMaxTestedGameVersion)
                {
                    group.AddInformationalText("Note:", "The mod has not been tested with this version of the game.");
                }

                group.AddDropdown(
                    "Assigment compatibility mode",
                    this.modCompatibilityModes.OrderBy(a => a.Key).Select(compatibilityMode => compatibilityMode.Value).ToArray(),
                    (int)Global.Settings.AssignmentCompatibilityMode,
                    value =>
                    {
                        try
                        {
                            foreach (ServiceDispatcherSettings.ModCompatibilityMode compatibilityMode in Enum.GetValues(typeof(ServiceDispatcherSettings.ModCompatibilityMode)))
                            {
                                if ((byte)compatibilityMode == value)
                                {
                                    if (compatibilityMode != Global.Settings.AssignmentCompatibilityMode)
                                    {
                                        Global.Settings.AssignmentCompatibilityMode = compatibilityMode;
                                        Global.Settings.Save();
                                    }

                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "CreateCompatibilityGroup", ex, "AssigmentCompatibilityMode", value);
                        }
                    });

                group.AddDropdown(
                    "Creation compatibility mode",
                    this.modCompatibilityModes.OrderBy(a => a.Key).Select(compatibilityMode => compatibilityMode.Value).ToArray(),
                    (int)Global.Settings.CreationCompatibilityMode,
                    value =>
                    {
                        try
                        {
                            foreach (ServiceDispatcherSettings.ModCompatibilityMode compatibilityMode in Enum.GetValues(typeof(ServiceDispatcherSettings.ModCompatibilityMode)))
                            {
                                if ((byte)compatibilityMode == value)
                                {
                                    if (compatibilityMode != Global.Settings.CreationCompatibilityMode)
                                    {
                                        Global.Settings.CreationCompatibilityMode = compatibilityMode;
                                        Global.Settings.Save();
                                    }

                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "CreateCompatibilityGroup", ex, "CreationCompatibilityMode", value);
                        }
                    });

                group.AddDropdown(
                        "Allow Code Overrides",
                        this.allowances.OrderBy(a => a.Key).Select(allowances => allowances.Value).ToArray(),
                        (int)Global.Settings.ReflectionAllowance,
                        value =>
                        {
                            try
                            {
                                foreach (ServiceDispatcherSettings.Allowance allowance in Enum.GetValues(typeof(ServiceDispatcherSettings.Allowance)))
                                {
                                    if ((byte)allowance == value)
                                    {
                                        if (allowance != Global.Settings.ReflectionAllowance)
                                        {
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
                                Log.Error(this, "CreateCompatibilityGroup", ex, "ReflectionAllowance", value);
                            }
                        });

                group.AddCheckbox(
                    "Restrict transfer manager",
                    Global.Settings.BlockTransferManagerOffers,
                    value =>
                    {
                        try
                        {
                            if (Global.Settings.BlockTransferManagerOffers != value)
                            {
                                Global.Settings.BlockTransferManagerOffers = value;
                                Global.Settings.Save();
                                Global.ReInitializeHandlers();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "CreateCompatibilityGroup", ex, "BlockTransferManagerOffers", value);
                        }
                    });

                return group;
            }
            catch (Exception ex)
            {
                Log.Error(this, "CreateCompatibilityGroup", ex);
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
        private UIHelperBase CreateEmptiableServiceBuildingGroup(UIHelperBase helper, StandardServiceSettings settings, bool canService)
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
        /// Creates the miscellaneous group.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <returns>The group.</returns>
        private UIHelperBase CreateMiscellaneousGroup(UIHelperBase helper)
        {
            try
            {
                UIHelperBase group = helper.AddGroup("Miscellaneous");

                try
                {
                    group.AddInformationalText("Config Path:", FileSystem.FilePath);

                    string version = null;
                    Assembly modAss = this.GetType().Assembly;
                    if (modAss != null)
                    {
                        version = modAss.GetName().Version.ToString() + ", " + version + " ";
                    }

                    version += "(" + AssemblyInfo.PreBuildStamps.DateTime.ToString("yyyy-MM-dd HH:mm") + ")";

                    group.AddInformationalText("Mod Version:", version);
                }
                catch
                {
                }

                group.AddInformationalText("Note:", "Dumping is only possible when a city is loaded.");

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
                Log.Error(this, "CreateMiscellaneousGroup", ex);
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
        private UIHelperBase CreateServiceGroup(UIHelperBase helper, StandardServiceSettings settings, Dispatcher dispatcher, bool canService)
        {
            try
            {
                UIHelperBase group = helper.AddGroup(settings.VehicleNamePlural);
                InformationalText currentStrategyInformationalText = null;

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

                if (settings.CanLimitOpportunisticCollection)
                {
                    group.AddCheckbox(
                        "Prioritize assigned buildings",
                        settings.LimitOpportunisticCollection && settings.OpportunisticCollectionLimitDetourAllowed,
                        value =>
                        {
                            try
                            {
                                if (settings.OpportunisticCollectionLimitDetourAllowed)
                                {
                                    if (settings.LimitOpportunisticCollection != value)
                                    {
                                        settings.LimitOpportunisticCollection = value;
                                        Detours.InitNeeded = true;
                                        Global.Settings.Save();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error(this, "OnSettingsUI", ex, "GarbageGroup", "LimitOpportunisticGarbageCollection", value);
                                Log.Error(this, "CreateServiceGroup", ex, settings.VehicleNamePlural, "LimitOpportunisticCollection");
                            }
                        });
                }

                group.AddDropdown(
                    "Send out spare " + settings.VehicleNamePlural.ToLower() + " when",
                    this.vehicleCreationOptions.OrderBy(vco => vco.Key).Select(vco => vco.Value).ToArray(),
                    (int)settings.CreateSpares,
                    value =>
                    {
                        try
                        {
                            foreach (ServiceDispatcherSettings.SpareVehiclesCreation option in Enum.GetValues(typeof(ServiceDispatcherSettings.SpareVehiclesCreation)))
                            {
                                if ((byte)option == value)
                                {
                                    if (settings.CreateSpares != option)
                                    {
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
                            foreach (ServiceDispatcherSettings.BuildingCheckOrder checks in Enum.GetValues(typeof(ServiceDispatcherSettings.BuildingCheckOrder)))
                            {
                                if ((byte)checks == value)
                                {
                                    if (settings.ChecksPreset != checks)
                                    {
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

                                    if (currentStrategyInformationalText != null)
                                    {
                                        currentStrategyInformationalText.Text = settings.ChecksParametersString;
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

                //if (Global.EnableDevExperiments)
                //{
                //    currentStrategyInformationalText = group.AddInformationalText(
                //        "Current dispatch strategy",
                //        settings.ChecksParametersString);
                //}

                group.AddExtendedSlider(
                    "Closest services to use when ignoring range",
                    0,
                    99,
                    1,
                    settings.IgnoreRangeUseClosestBuildings,
                    false,
                    true,
                    value =>
                    {
                        try
                        {
                            byte buildings = (byte)value;

                            if (buildings != settings.IgnoreRangeUseClosestBuildings)
                            {
                                settings.IgnoreRangeUseClosestBuildings = buildings;
                                Global.Settings.Save();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(this, "CreateServiceGroup", ex, settings.VehicleNamePlural, "IgnoreRangeUseClosestBuildings", value);
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

                bool updatingCustomStrategy = true;
                InformationalText customStrategyWarningLabel = null;
                UITextField customStrategyTextField = null;

                customStrategyTextField = (UITextField)group.AddTextfield(
                    "Custom dispatch strategy",
                    settings.ChecksCustomString,
                    value => { },
                    value =>
                    {
                        if (!updatingCustomStrategy)
                        {
                            try
                            {
                                updatingCustomStrategy = true;

                                string oldValue = settings.ChecksCustomString;
                                settings.ChecksCustomString = value;

                                if (oldValue != settings.ChecksCustomString)
                                {
                                    if (settings.ChecksPreset == ServiceDispatcherSettings.BuildingCheckOrder.Custom && dispatcher != null)
                                    {
                                        try
                                        {
                                            dispatcher.ReInitialize();
                                        }
                                        catch (Exception ex)
                                        {
                                            Log.Error(this, "CreateServiceGroup", ex, settings.ChecksCustomString, "ChecksCustom", "ReInitializeDispatcher");
                                        }
                                    }

                                    Global.Settings.Save();
                                }

                                if (settings.ChecksCustomStringHadError)
                                {
                                    customStrategyWarningLabel.Text = settings.ChecksCustomStringErrorMessage;
                                    customStrategyWarningLabel.Show();
                                    customStrategyWarningLabel.Enable();
                                }
                                else
                                {
                                    customStrategyWarningLabel.Disable();
                                    customStrategyWarningLabel.Hide();
                                    customStrategyWarningLabel.Text = "";
                                    customStrategyTextField.text = settings.ChecksCustomString;
                                }

                                if (currentStrategyInformationalText != null && settings.ChecksPreset == ServiceDispatcherSettings.BuildingCheckOrder.Custom)
                                {
                                    currentStrategyInformationalText.Text = settings.ChecksParametersString;
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error(this, "CreateServiceGroup", ex, settings.VehicleNamePlural, "ChecksCustom", value);
                            }
                            finally
                            {
                                updatingCustomStrategy = false;
                            }
                        }
                    });

                customStrategyWarningLabel = group.AddInformationalText("Warning", "");
                customStrategyWarningLabel.Disable();
                customStrategyWarningLabel.Hide();

                updatingCustomStrategy = false;
                customStrategyTextField.width *= 2;

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
                foreach (ServiceDispatcherSettings.BuildingCheckOrder checks in Enum.GetValues(typeof(ServiceDispatcherSettings.BuildingCheckOrder)))
                {
                    this.targetBuildingChecks.Add((byte)checks, Settings.GetBuildingCheckOrderName(checks));
                }
            }

            if (this.vehicleCreationOptions == null)
            {
                this.vehicleCreationOptions = new Dictionary<byte, string>();
                foreach (ServiceDispatcherSettings.SpareVehiclesCreation option in Enum.GetValues(typeof(ServiceDispatcherSettings.SpareVehiclesCreation)))
                {
                    this.vehicleCreationOptions.Add((byte)option, Settings.GetSpareVehiclesCreationName(option));
                }
            }

            if (this.allowances == null)
            {
                this.allowances = new Dictionary<byte, string>();
                foreach (ServiceDispatcherSettings.Allowance allowance in Enum.GetValues(typeof(ServiceDispatcherSettings.Allowance)))
                {
                    this.allowances.Add((byte)allowance, Settings.GetAllowanceName(allowance));
                }
            }

            if (this.modCompatibilityModes == null)
            {
                this.modCompatibilityModes = new Dictionary<byte, string>();
                foreach (ServiceDispatcherSettings.ModCompatibilityMode compatibilityMode in Enum.GetValues(typeof(ServiceDispatcherSettings.ModCompatibilityMode)))
                {
                    this.modCompatibilityModes.Add((byte)compatibilityMode, Settings.GetModCompatibilityModeName(compatibilityMode));
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