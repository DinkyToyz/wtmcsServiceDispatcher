using ICities;
using System;
using System.Collections.Generic;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// The loader.
    /// </summary>
    public class LoadingExtension : LoadingExtensionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadingExtension"/> class.
        /// </summary>
        public LoadingExtension()
            : base()
        {
            Log.Debug(this, "Constructed");
        }

        /// <summary>
        /// Called when mod is created.
        /// </summary>
        /// <param name="loading">The loading.</param>
        public override void OnCreated(ILoading loading)
        {
            Log.Debug(this, "OnCreated", "Base");
            Log.FlushBuffer();
            base.OnCreated(loading);
        }

        /// <summary>
        /// Called when level has been loaded.
        /// </summary>
        /// <param name="mode">The mode.</param>
        public override void OnLevelLoaded(LoadMode mode)
        {
            Log.Debug(this, "OnLevelLoaded", "Begin");

            try
            {
                if (mode == LoadMode.NewGame || mode == LoadMode.LoadGame)
                {
                    // Load settings.
                    Global.InitSettings();

                    // Initialize building checks.
                    switch (Global.Settings.BuildingChecks)
                    {
                        case Settings.BuildingCheckOrder.InRangeFirst:
                            Global.BuldingCheckParameters = Dispatcher.BuldingCheckParameters.InRangeFirst;
                            break;

                        case Settings.BuildingCheckOrder.ProblematicFirst:
                            Global.BuldingCheckParameters = Dispatcher.BuldingCheckParameters.ProblematicFirst;
                            break;

                        case Settings.BuildingCheckOrder.ForgottenFirst:
                            Global.BuldingCheckParameters = Dispatcher.BuldingCheckParameters.ForgottenFirst;
                            break;

                        case Settings.BuildingCheckOrder.Custom:
                            List<Dispatcher.BuldingCheckParameters> pars = new List<Dispatcher.BuldingCheckParameters>();

                            foreach (Settings.BuildingCheckParameters par in Global.Settings.BuildingChecksCustom)
                            {
                                switch (par)
                                {
                                    case Settings.BuildingCheckParameters.InRange:
                                        pars.Add(Dispatcher.BuldingCheckParameters.InRange);
                                        break;

                                    case Settings.BuildingCheckParameters.ProblematicInRange:
                                        pars.Add(Dispatcher.BuldingCheckParameters.ProblematicInRange);
                                        break;

                                    case Settings.BuildingCheckParameters.ProblematicIgnoreRange:
                                        pars.Add(Dispatcher.BuldingCheckParameters.ProblematicIgnoreRange);
                                        break;

                                    case Settings.BuildingCheckParameters.ForgottenIgnoreRange:
                                        pars.Add(Dispatcher.BuldingCheckParameters.ForgottenIgnoreRange);
                                        break;
                                }
                            }

                            Global.BuldingCheckParameters = pars.ToArray();
                            break;
                    }

                    // Initialize dispatch objects.
                    if (Global.Settings.DispatchHearses)
                    {
                        Global.Buildings = new Buildings();
                        Global.TargetBuildingInfoPriorityComparer = new Buildings.TargetBuildingInfo.PriorityComparer();
                        Global.ServiceBuildingInfoPriorityComparer = new Buildings.ServiceBuildingInfo.PriorityComparer();

                        // Initialize hearse objects.
                        if (Global.Settings.DispatchHearses)
                        {
                            Global.HearseDispatcher = new HearseDispatcher();
                        }
                    }

                    // Initialize vehicle objects.
                    if (Global.Settings.RemoveHearsesFromGrid)
                    {
                        Global.Vehicles = new Vehicles();
                    }

                    Global.LevelLoaded = true;
                    Global.CurrentFrame = 0;

                    Log.Info(this, "OnLevelLoaded", "Initialized");
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnLevelLoaded", ex);
                try
                {
                    DeInitialize();
                }
                catch (Exception exnull)
                {
                    Log.Error(this, "OnLevelLoaded", exnull);
                }
            }
            finally
            {
                base.OnLevelLoaded(mode);
            }

            Log.Debug(this, "OnLevelLoaded", "End");
            Log.FlushBuffer();
        }

        /// <summary>
        /// Called when level is unloading.
        /// </summary>
        public override void OnLevelUnloading()
        {
            Log.Debug(this, "OnLevelUnloading", "End");

            try
            {
                DeInitialize();
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnLevelUnloading", ex);
            }
            finally
            {
                Log.Debug(this, "OnLevelUnloading", "Base");
                base.OnLevelUnloading();
            }

            Log.Debug(this, "OnLevelUnloading", "End");
            Log.FlushBuffer();
        }

        /// <summary>
        /// Called when mod is released.
        /// </summary>
        public override void OnReleased()
        {
            Log.Debug(this, "OnReleased", "Begin");

            try
            {
                DeInitialize();
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnReleased", ex);
            }
            finally
            {
                Log.Debug(this, "OnReleased", "Base");
                base.OnReleased();
            }

            Log.Debug(this, "OnReleased", "End");
            Log.FlushBuffer();
        }

        /// <summary>
        /// Deinitializes data.
        /// </summary>
        private void DeInitialize()
        {
            Global.LevelLoaded = false;

            Log.Info(this, "DeInitialize");

            Global.HearseDispatcher = null;
            Global.ServiceBuildingInfoPriorityComparer = null;
            Global.TargetBuildingInfoPriorityComparer = null;
            Global.Buildings = null;
        }
    }
}