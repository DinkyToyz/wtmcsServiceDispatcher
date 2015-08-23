using ICities;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private Dictionary<byte, string> TargetBuildingChecks = null;

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
        /// <exception cref="System.NotImplementedException"></exception>
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

                if (TargetBuildingChecks == null)
                {
                    TargetBuildingChecks = new Dictionary<byte, string>();
                    foreach (Settings.BuildingCheckOrder checks in Enum.GetValues(typeof(Settings.BuildingCheckOrder)))
                    {
                        if (Log.LogALot || Library.IsDebugBuild) Log.Debug(this, "OnSettingsUI", "Init", "BuildingCheckOrder", (byte)checks, checks, Settings.GetBuildingCheckOrderName(checks));
                        TargetBuildingChecks.Add((byte)checks, Settings.GetBuildingCheckOrderName(checks));
                    }
                }

                // Add gernal dispatch group.
                UIHelperBase dispatchGroup = helper.AddGroup("Central Services Dispatch");
                dispatchGroup.AddCheckbox("Dispatch by district", Global.Settings.DispatchByDistrict, value => { Global.Settings.DispatchByDistrict = value; Global.Settings.Save(); });
                dispatchGroup.AddCheckbox("Limit by building range", Global.Settings.LimitRange, value => { Global.Settings.LimitRange = value; Global.Settings.Save(); });
                dispatchGroup.AddSlider("Service Building range modifier (0.1 - 10)", 0.1f, 10.0f, 0.1f, Global.Settings.RangeModifier, value => { Global.Settings.RangeModifier = value; Global.Settings.Save(); });
                dispatchGroup.AddDropdown("Target Building Checks", TargetBuildingChecks.OrderBy(bco => bco.Key).Select(bco => bco.Value).ToArray(), (int)Global.Settings.BuildingChecksPreset,
                    value =>
                    {
                        if (Log.LogALot || Library.IsDebugBuild) Log.Debug(this, "OnSettingsUI", "Set", "BuildingCheckOrder", value);

                        foreach (Settings.BuildingCheckOrder checks in Enum.GetValues(typeof(Settings.BuildingCheckOrder)))
                        {
                            if ((byte)checks == value)
                            {
                                if (Log.LogALot || Library.IsDebugBuild) Log.Debug(this, "OnSettingsUI", "Set", "BuildingCheckOrder", value, checks);

                                Global.Settings.BuildingChecksPreset = checks;
                                Global.InitBuildingChecks();
                                Global.Settings.Save();
                                break;
                            }
                        }
                    });

                // Add hearse group.
                UIHelperBase hearseGroup = helper.AddGroup("Hearses");
                hearseGroup.AddCheckbox("Dispatch hearses", Global.Settings.DispatchHearses, value => { Global.Settings.DispatchHearses = value; Global.Settings.Save(); });
                hearseGroup.AddCheckbox("Pass through hearses", Global.Settings.RemoveHearsesFromGrid, value => { Global.Settings.RemoveHearsesFromGrid = value; Global.Settings.Save(); });

                // Add garbage group.
                UIHelperBase garbageGroup = helper.AddGroup("Garbage Trucks");
                hearseGroup.AddCheckbox("Dispatch garbage trucks", Global.Settings.DispatchGarbageTrucks, value => { Global.Settings.DispatchGarbageTrucks = value; Global.Settings.Save(); });
                hearseGroup.AddCheckbox("Pass through garbage trucks", Global.Settings.RemoveGarbageTrucksFromGrid, value => { Global.Settings.RemoveGarbageTrucksFromGrid = value; Global.Settings.Save(); });
            }
            catch (System.Exception ex)
            {
                Log.Error(this, "OnSettingsUI", ex);
            }
        }
    }
}