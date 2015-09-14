﻿using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework.UI;

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
        /// The vehicle creation options strings for dropdown.
        /// </summary>
        private Dictionary<byte, string> vehicleCreationOptions = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mod"/> class.
        /// </summary>
        public Mod()
        {
            Log.NoOp();
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
                Global.InitSettings();

                if (this.targetBuildingChecks == null)
                {
                    this.targetBuildingChecks = new Dictionary<byte, string>();
                    foreach (Settings.BuildingCheckOrder checks in Enum.GetValues(typeof(Settings.BuildingCheckOrder)))
                    {
                        if (Log.LogALot || Library.IsDebugBuild) Log.Debug(this, "OnSettingsUI", "Init", "BuildingCheckOrder", (byte)checks, checks, Settings.GetBuildingCheckOrderName(checks));

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
                    "Dispatch by district", 
                    Global.Settings.DispatchByDistrict, 
                    value => 
                    { 
                        Global.Settings.DispatchByDistrict = value; 
                        Global.Settings.Save(); 
                    });

                dispatchGroup.AddCheckbox(
                    "Dispatch by building range", 
                    Global.Settings.DispatchByRange, 
                    value => 
                    { 
                        Global.Settings.DispatchByRange = value; 
                        Global.Settings.Save(); 
                    });

                dispatchGroup.AddCheckbox(
                    "Limit building ranges", 
                    Global.Settings.RangeLimit, 
                    value => 
                    { 
                        Global.Settings.RangeLimit = value; 
                        Global.Settings.Save(); 
                    });

                ////dispatchGroup.AddSlider(
                ////    "Range modifier (0.1 - 10)", 
                ////    0.1f, 
                ////    10.0f, 
                ////    0.1f, 
                ////    Global.Settings.RangeModifier, 
                ////    value => 
                ////    { 
                ////        Global.Settings.RangeModifier = value; 
                ////        Global.Settings.Save(); 
                ////    });

                ////dispatchGroup.AddSlider(
                ////    "Range minimum (0-100000000)", 
                ////    0f, 
                ////    100000000f, 
                ////    1f, 
                ////    Global.Settings.RangeMinimum, 
                ////    value => 
                ////    { 
                ////        Global.Settings.RangeMinimum = value; 
                ////        Global.Settings.Save(); 
                ////    });

                ////dispatchGroup.AddSlider(
                ////    "Range maximum (0-100000000)", 
                ////    0f, 
                ////    100000000f, 
                ////    1f, 
                ////    Global.Settings.RangeMinimum, 
                ////    value => 
                ////    { 
                ////        Global.Settings.RangeMinimum = value; 
                ////        Global.Settings.Save(); 
                ////    });

                // Add hearse group.
                UIHelperBase hearseGroup = helper.AddGroup("Hearses");

                hearseGroup.AddCheckbox(
                    "Dispatch hearses", 
                    Global.Settings.DispatchHearses, 
                    value => 
                    { 
                        Global.Settings.DispatchHearses = value; 
                        Global.Settings.Save();
                        Global.InitHandlers();
                    });

                hearseGroup.AddCheckbox(
                    "Pass through hearses", 
                    Global.Settings.RemoveHearsesFromGrid, 
                    value => 
                    { 
                        Global.Settings.RemoveHearsesFromGrid = value; 
                        Global.Settings.Save();
                        Global.InitHandlers();
                    });

                hearseGroup.AddDropdown(
                    "Send out spare hearses",
                    this.vehicleCreationOptions.OrderBy(vco => vco.Key).Select(vco => vco.Value).ToArray(),
                    (int)Global.Settings.CreateSpareHearses,
                    value =>
                    {
                        if (Log.LogALot || Library.IsDebugBuild) Log.Debug(this, "OnSettingsUI", "Set", "CreateSpareHearses", value);

                        foreach (Settings.SpareVehiclesCreation option in Enum.GetValues(typeof(Settings.SpareVehiclesCreation)))
                        {
                            if ((byte)option == value)
                            {
                                if (Log.LogALot || Library.IsDebugBuild) Log.Debug(this, "OnSettingsUI", "Set", "CreateSpareHearses", value, option);

                                Global.Settings.CreateSpareHearses = option;
                                if (Global.HearseDispatcher != null)
                                {
                                    Global.HearseDispatcher.ReInitialize();
                                }
                                Global.Settings.Save();
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
                        if (Log.LogALot || Library.IsDebugBuild) Log.Debug(this, "OnSettingsUI", "Set", "DeathChecksPreset", value);

                        foreach (Settings.BuildingCheckOrder checks in Enum.GetValues(typeof(Settings.BuildingCheckOrder)))
                        {
                            if ((byte)checks == value)
                            {
                                if (Log.LogALot || Library.IsDebugBuild) Log.Debug(this, "OnSettingsUI", "Set", "DeathChecksPreset", value, checks);

                                Global.Settings.DeathChecksPreset = checks;
                                if (Global.HearseDispatcher != null)
                                {
                                    Global.HearseDispatcher.ReInitialize();
                                }
                                Global.Settings.Save();
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
                        Global.Settings.DispatchGarbageTrucks = value; 
                        Global.Settings.Save();
                        Global.InitHandlers();
                    });

                ////garbageGroup.AddCheckbox(
                ////    "Pass through garbage trucks", 
                ////    Global.Settings.RemoveGarbageTrucksFromGrid, 
                ////    value => 
                ////    { 
                ////        Global.Settings.RemoveGarbageTrucksFromGrid = value; 
                ////        Global.Settings.Save();
                ////        Global.InitHandlers();
                ////    });

                garbageGroup.AddDropdown(
                    "Send out spare garbage trucks",
                    this.vehicleCreationOptions.OrderBy(vco => vco.Key).Select(vco => vco.Value).ToArray(),
                    (int)Global.Settings.CreateSpareGarbageTrucks,
                    value =>
                    {
                        if (Log.LogALot || Library.IsDebugBuild) Log.Debug(this, "OnSettingsUI", "Set", "CreateSpareGarbageTrucks", value);

                        foreach (Settings.SpareVehiclesCreation option in Enum.GetValues(typeof(Settings.SpareVehiclesCreation)))
                        {
                            if ((byte)option == value)
                            {
                                if (Log.LogALot || Library.IsDebugBuild) Log.Debug(this, "OnSettingsUI", "Set", "CreateSpareGarbageTrucks", value, option);

                                Global.Settings.CreateSpareGarbageTrucks = option;
                                if (Global.GarbageTruckDispatcher != null)
                                {
                                    Global.GarbageTruckDispatcher.ReInitialize();
                                }
                                Global.Settings.Save();
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
                        if (Log.LogALot || Library.IsDebugBuild) Log.Debug(this, "OnSettingsUI", "Set", "GarbageChecksPreset", value);

                        foreach (Settings.BuildingCheckOrder checks in Enum.GetValues(typeof(Settings.BuildingCheckOrder)))
                        {
                            if ((byte)checks == value)
                            {
                                if (Log.LogALot || Library.IsDebugBuild) Log.Debug(this, "OnSettingsUI", "Set", "GarbageChecksPreset", value, checks);

                                Global.Settings.GarbageChecksPreset = checks;
                                if (Global.GarbageTruckDispatcher != null)
                                {
                                    Global.GarbageTruckDispatcher.ReInitialize();
                                }
                                Global.Settings.Save();
                                break;
                            }
                        }
                    });

                UILabel minimumGarbageForDispatchLabel = null;

                object minimumGarbageForDispatchSlider =
                garbageGroup.AddSlider(
                    "Garbage amount limit (1-5000)", 
                    1.0f, 
                    5000.0f, 
                    1.0f, 
                    Global.Settings.MinimumGarbageForDispatch, 
                    value => 
                    { 
                        Global.Settings.MinimumGarbageForDispatch = (ushort)value; 
                        Global.Settings.Save(); 
                        if (minimumGarbageForDispatchLabel != null)
                        {
                            minimumGarbageForDispatchLabel.text = Global.Settings.MinimumGarbageForDispatch.ToString();
                        }
                    });

                try
                {
                    if (minimumGarbageForDispatchSlider is UISlider)
                    {
                        object panel = garbageGroup.AddSpace((int)((UISlider)minimumGarbageForDispatchSlider).height);
                        if (panel is UIPanel)
                        {
                            ((UIPanel)panel).AlignTo((UISlider)minimumGarbageForDispatchSlider, UIAlignAnchor.TopRight);
                            ((UIPanel)panel).width = ((UISlider)minimumGarbageForDispatchSlider).width;
                            minimumGarbageForDispatchLabel = ((UIPanel)panel).AddUIComponent<UILabel>();
                            minimumGarbageForDispatchLabel.autoHeight = true;
                            minimumGarbageForDispatchLabel.text = Global.Settings.MinimumGarbageForDispatch.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(this, "OnSettingsUI[SliderPanelLabelTest]", ex);
                }

                Log.FlushBuffer();
            }
            catch (System.Exception ex)
            {
                Log.Error(this, "OnSettingsUI", ex);
            }
        }
    }
}
