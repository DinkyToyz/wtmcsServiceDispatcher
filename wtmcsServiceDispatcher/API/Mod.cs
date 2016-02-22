using System;
using System.Collections.Generic;
using System.Linq;
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

                if (this.targetBuildingChecks == null)
                {
                    this.targetBuildingChecks = new Dictionary<byte, string>();
                    foreach (Settings.BuildingCheckOrder checks in Enum.GetValues(typeof(Settings.BuildingCheckOrder)))
                    {
                        if (Log.LogALot || Library.IsDebugBuild)
                            Log.Debug(this, "OnSettingsUI", "Init", "BuildingCheckOrder", (byte)checks, checks, Settings.GetBuildingCheckOrderName(checks));

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

                // Add general dispatch group.
                UIHelperBase dispatchGroup = helper.AddGroup("Central Services Dispatch");

                dispatchGroup.AddCheckbox(
                    "Limit building ranges",
                    Global.Settings.RangeLimit,
                    value =>
                    {
                        if (Global.Settings.RangeLimit != value)
                        {
                            Global.BuildingUpdateNeeded = true;
                            Global.Settings.RangeLimit = value;
                            Global.Settings.Save();
                        }
                    });

                dispatchGroup.AddExtendedSlider(
                    "Range modifier",
                    0.1f,
                    10.0f,
                    0.1f,
                    Global.Settings.RangeModifier,
                    true,
                    "F1",
                    value =>
                    {
                        if (Global.Settings.RangeModifier != value)
                        {
                            Global.BuildingUpdateNeeded = true;
                            Global.Settings.RangeModifier = value;
                            Global.Settings.Save();
                        }
                    });

                dispatchGroup.AddExtendedSlider(
                    "Range minimum",
                    0f,
                    100000000f,
                    1f,
                    Global.Settings.RangeMinimum,
                    false,
                    value =>
                    {
                        if (Global.Settings.RangeMinimum != value)
                        {
                            Global.BuildingUpdateNeeded = true;
                            Global.Settings.RangeMinimum = value;
                            Global.Settings.Save();
                        }
                    });

                dispatchGroup.AddExtendedSlider(
                    "Range maximum",
                    0f,
                    100000000f,
                    1f,
                    Global.Settings.RangeMaximum,
                    false,
                    value =>
                    {
                        if (Global.Settings.RangeMaximum != value)
                        {
                            Global.BuildingUpdateNeeded = true;
                            Global.Settings.RangeMaximum = value;
                            Global.Settings.Save();
                        }
                    });

                // Add hearse group.
                UIHelperBase hearseGroup = helper.AddGroup("Hearses");

                hearseGroup.AddCheckbox(
                    "Dispatch hearses",
                    Global.Settings.DispatchHearses,
                    value =>
                    {
                        if (Global.Settings.DispatchHearses != value)
                        {
                            Global.Settings.DispatchHearses = value;
                            Global.Settings.Save();
                            Global.ReInitializeHandlers();
                        }
                    });

                hearseGroup.AddCheckbox(
                    "Dispatch hearses by district",
                    Global.Settings.DispatchHearsesByDistrict,
                    value =>
                    {
                        if (Global.Settings.DispatchHearsesByDistrict != value)
                        {
                            Global.BuildingUpdateNeeded = true;
                            Global.Settings.DispatchHearsesByDistrict = value;
                            Global.Settings.Save();
                        }
                    });

                hearseGroup.AddCheckbox(
                    "Dispatch hearses by building range",
                    Global.Settings.DispatchHearsesByRange,
                    value =>
                    {
                        if (Global.Settings.DispatchHearsesByRange != value)
                        {
                            Global.BuildingUpdateNeeded = true;
                            Global.Settings.DispatchHearsesByRange = value;
                            Global.Settings.Save();
                        }
                    });

                hearseGroup.AddCheckbox(
                    "Pass through hearses",
                    Global.Settings.RemoveHearsesFromGrid,
                    value =>
                    {
                        if (Global.Settings.RemoveHearsesFromGrid != value)
                        {
                            Global.Settings.RemoveHearsesFromGrid = value;
                            Global.Settings.Save();
                            Global.ReInitializeHandlers();
                        }
                    });

                hearseGroup.AddDropdown(
                    "Send out spare hearses when",
                    this.vehicleCreationOptions.OrderBy(vco => vco.Key).Select(vco => vco.Value).ToArray(),
                    (int)Global.Settings.CreateSpareHearses,
                    value =>
                    {
                        if (Log.LogALot || Library.IsDebugBuild)
                            Log.Debug(this, "OnSettingsUI", "Set", "CreateSpareHearses", value);

                        foreach (Settings.SpareVehiclesCreation option in Enum.GetValues(typeof(Settings.SpareVehiclesCreation)))
                        {
                            if ((byte)option == value)
                            {
                                if (Global.Settings.CreateSpareHearses != option)
                                {
                                    if (Log.LogALot || Library.IsDebugBuild)
                                        Log.Debug(this, "OnSettingsUI", "Set", "CreateSpareHearses", value, option);

                                    Global.Settings.CreateSpareHearses = option;
                                    if (Global.HearseDispatcher != null)
                                    {
                                        Global.HearseDispatcher.ReInitialize();
                                    }
                                    Global.Settings.Save();
                                }

                                break;
                            }
                        }
                    });

                hearseGroup.AddDropdown(
                    "Hearse dispatch strategy",
                    this.targetBuildingChecks.OrderBy(bco => bco.Key).Select(bco => bco.Value).ToArray(),
                    (int)Global.Settings.DeathChecksPreset,
                    value =>
                    {
                        if (Log.LogALot || Library.IsDebugBuild)
                            Log.Debug(this, "OnSettingsUI", "Set", "DeathChecksPreset", value);

                        foreach (Settings.BuildingCheckOrder checks in Enum.GetValues(typeof(Settings.BuildingCheckOrder)))
                        {
                            if ((byte)checks == value)
                            {
                                if (Global.Settings.DeathChecksPreset != checks)
                                {
                                    if (Log.LogALot || Library.IsDebugBuild)
                                        Log.Debug(this, "OnSettingsUI", "Set", "DeathChecksPreset", value, checks);

                                    Global.Settings.DeathChecksPreset = checks;
                                    if (Global.HearseDispatcher != null)
                                    {
                                        Global.HearseDispatcher.ReInitialize();
                                    }
                                    Global.Settings.Save();
                                }
                                break;
                            }
                        }
                    });

                // Add garbage group.
                UIHelperBase garbageGroup = helper.AddGroup("Garbage Trucks");

                garbageGroup.AddCheckbox(
                    "Dispatch garbage trucks",
                    Global.Settings.DispatchGarbageTrucks,
                    value =>
                    {
                        if (Global.Settings.DispatchGarbageTrucks != value)
                        {
                            Global.Settings.DispatchGarbageTrucks = value;
                            Global.Settings.Save();
                            Global.ReInitializeHandlers();
                        }
                    });

                garbageGroup.AddCheckbox(
                    "Dispatch garbage trucks by district",
                    Global.Settings.DispatchGarbageTrucksByDistrict,
                    value =>
                    {
                        if (Global.Settings.DispatchGarbageTrucksByDistrict != value)
                        {
                            Global.BuildingUpdateNeeded = true;
                            Global.Settings.DispatchGarbageTrucksByDistrict = value;
                            Global.Settings.Save();
                        }
                    });

                garbageGroup.AddCheckbox(
                    "Dispatch garbage trucks by building range",
                    Global.Settings.DispatchGarbageTrucksByRange,
                    value =>
                    {
                        if (Global.Settings.DispatchGarbageTrucksByRange != value)
                        {
                            Global.BuildingUpdateNeeded = true;
                            Global.Settings.DispatchGarbageTrucksByRange = value;
                            Global.Settings.Save();
                        }
                    });

                if (Detours.CanDetour(Detours.Methods.GarbageTruckAI_TryCollectGarbage))
                {
                    garbageGroup.AddCheckbox(
                        "Prioritize assigned buildings",
                        Global.Settings.LimitOpportunisticGarbageCollection,
                        value =>
                        {
                            if (Global.Settings.LimitOpportunisticGarbageCollection != value)
                            {
                                Detours.InitNeeded = true;
                                Global.Settings.LimitOpportunisticGarbageCollection = value;
                                Global.Settings.Save();
                            }
                        });
                }
                else
                {
                    UIComponent limitOpportunisticGarbageCollectionCheckBox = garbageGroup.AddCheckbox(
                        "Prioritize assigned buildings",
                        false,
                        value =>
                        {
                        }) as UIComponent;
                    limitOpportunisticGarbageCollectionCheckBox.Disable();
                }

                garbageGroup.AddDropdown(
                    "Send out spare garbage trucks when",
                    this.vehicleCreationOptions.OrderBy(vco => vco.Key).Select(vco => vco.Value).ToArray(),
                    (int)Global.Settings.CreateSpareGarbageTrucks,
                    value =>
                    {
                        if (Log.LogALot || Library.IsDebugBuild)
                            Log.Debug(this, "OnSettingsUI", "Set", "CreateSpareGarbageTrucks", value);

                        foreach (Settings.SpareVehiclesCreation option in Enum.GetValues(typeof(Settings.SpareVehiclesCreation)))
                        {
                            if ((byte)option == value)
                            {
                                if (Global.Settings.CreateSpareGarbageTrucks != option)
                                {
                                    if (Log.LogALot || Library.IsDebugBuild)
                                        Log.Debug(this, "OnSettingsUI", "Set", "CreateSpareGarbageTrucks", value, option);

                                    Global.Settings.CreateSpareGarbageTrucks = option;
                                    if (Global.GarbageTruckDispatcher != null)
                                    {
                                        Global.GarbageTruckDispatcher.ReInitialize();
                                    }
                                    Global.Settings.Save();
                                }
                                break;
                            }
                        }
                    });

                garbageGroup.AddDropdown(
                    "Garbage truck dispatch strategy",
                    this.targetBuildingChecks.OrderBy(bco => bco.Key).Select(bco => bco.Value).ToArray(),
                    (int)Global.Settings.GarbageChecksPreset,
                    value =>
                    {
                        if (Log.LogALot || Library.IsDebugBuild)
                            Log.Debug(this, "OnSettingsUI", "Set", "GarbageChecksPreset", value);

                        foreach (Settings.BuildingCheckOrder checks in Enum.GetValues(typeof(Settings.BuildingCheckOrder)))
                        {
                            if ((byte)checks == value)
                            {
                                if (Global.Settings.GarbageChecksPreset != checks)
                                {
                                    if (Log.LogALot || Library.IsDebugBuild)
                                        Log.Debug(this, "OnSettingsUI", "Set", "GarbageChecksPreset", value, checks);

                                    Global.Settings.GarbageChecksPreset = checks;
                                    if (Global.GarbageTruckDispatcher != null)
                                    {
                                        Global.GarbageTruckDispatcher.ReInitialize();
                                    }
                                    Global.Settings.Save();
                                }
                                break;
                            }
                        }
                    });

                garbageGroup.AddExtendedSlider(
                    "Garbage patrol limit",
                    1.0f,
                    5000.0f,
                    1.0f,
                    Global.Settings.MinimumGarbageForPatrol,
                    false,
                    value =>
                    {
                        if (Global.Settings.MinimumGarbageForPatrol != (ushort)value)
                        {
                            Global.BuildingUpdateNeeded = true;
                            Global.Settings.MinimumGarbageForPatrol = (ushort)value;
                            Global.Settings.Save();
                        }
                    });

                garbageGroup.AddExtendedSlider(
                    "Garbage dispatch limit",
                    1.0f,
                    5000.0f,
                    1.0f,
                    Global.Settings.MinimumGarbageForDispatch,
                    false,
                    value =>
                    {
                        if (Global.Settings.MinimumGarbageForDispatch != (ushort)value)
                        {
                            Global.BuildingUpdateNeeded = true;
                            Global.Settings.MinimumGarbageForDispatch = (ushort)value;
                            Global.Settings.Save();
                        }
                    });

                // Add bulldoze group.
                UIHelperBase bulldozeGroup = helper.AddGroup("Bulldozers");

                if (BulldozeHelper.CanBulldoze)
                {
                    bulldozeGroup.AddCheckbox(
                        "Dispatch bulldozers",
                        Global.Settings.AutoBulldozeBuildings,
                        value =>
                        {
                            if (Global.Settings.AutoBulldozeBuildings != value)
                            {
                                Global.Settings.AutoBulldozeBuildings = value;
                                Global.Settings.Save();
                                Global.ReInitializeHandlers();
                            }
                        });
                }
                else
                {
                    UIComponent dispatchWreckingCrewsCheckBox = bulldozeGroup.AddCheckbox(
                        "Dispatch bulldozers",
                        false,
                        value =>
                        {
                        }) as UIComponent;
                }

                bulldozeGroup.AddExtendedSlider(
                    "Bulldozer delay",
                    0.0f,
                    60.0f * 24.0f,
                    0.01f,
                    (float)Global.Settings.AutoBulldozeBuildingsDelay,
                    true,
                    value =>
                    {
                        if (Global.Settings.AutoBulldozeBuildingsDelay != (double)value)
                        {
                            Global.BuildingUpdateNeeded = true;
                            Global.Settings.AutoBulldozeBuildingsDelay = (double)value;
                            Global.Settings.Save();
                        }
                    });

                // Add logging group.
                UIHelperBase logGroup = helper.AddGroup("Logging (not saved in settings)");

                UIComponent debugLog = null;
                UIComponent devLog = null;
                UIComponent objectLog = null;
                UIComponent namesLog = null;
                UIComponent fileLog = null;

                debugLog = logGroup.AddCheckbox(
                    "Debug log",
                    Log.LogDebug,
                    value =>
                    {
                        if (!this.updatingLogControls)
                        {
                            Log.LogDebug = value;
                            UpdateLogControls(debugLog, devLog, objectLog, namesLog, fileLog);
                        }
                    }) as UIComponent;

                devLog = logGroup.AddCheckbox(
                    "Developer log",
                    Log.LogALot,
                    value =>
                    {
                        if (!this.updatingLogControls)
                        {
                            Log.LogALot = value;
                            UpdateLogControls(debugLog, devLog, objectLog, namesLog, fileLog);
                        }
                    }) as UIComponent;

                objectLog = logGroup.AddCheckbox(
                    "Object list log",
                    Log.LogDebugLists,
                    value =>
                    {
                        if (!this.updatingLogControls)
                        {
                            Log.LogDebugLists = value;
                            UpdateLogControls(debugLog, devLog, objectLog, namesLog, fileLog);
                        }
                    }) as UIComponent;

                namesLog = logGroup.AddCheckbox(
                    "Log names",
                    Log.LogNames,
                    value =>
                    {
                        if (!this.updatingLogControls)
                        {
                            Log.LogNames = value;
                            UpdateLogControls(debugLog, devLog, objectLog, namesLog, fileLog);
                        }
                    }) as UIComponent;

                fileLog = logGroup.AddCheckbox(
                    "Log to file",
                    Log.LogToFile,
                    value =>
                    {
                        if (!this.updatingLogControls)
                        {
                            Log.LogToFile = value;
                            UpdateLogControls(debugLog, devLog, objectLog, namesLog, fileLog);
                        }
                    }) as UIComponent;

                Log.FlushBuffer();
            }
            catch (System.Exception ex)
            {
                Log.Error(this, "OnSettingsUI", ex);
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