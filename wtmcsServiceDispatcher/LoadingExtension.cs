using ICities;
using System;

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
                    Global.Settings = Settings.Load();

                    // Initialize data objects.
                    if (Global.Settings.HandleHearses)
                    {
                        Global.Buildings = new Buildings();
                        Global.Vehicles = new Vehicles();

                        // Initialize hearse objects.
                        if (Global.Settings.HandleHearses)
                        {
                            Global.HearseDispatcher = new HearseDispatcher(Global.PretendToHandleHearses);
                        }
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
                    Global.HearseDispatcher = null;
                    Global.Vehicles = null;
                    Global.Buildings = null;
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
        }

        /// <summary>
        /// Deinitializes data.
        /// </summary>
        private void DeInitialize()
        {
            Global.LevelLoaded = false;

            Log.Info(this, "DeInitialize");

            Global.HearseDispatcher = null;
            Global.Vehicles = null;
            Global.Buildings = null;
        }
    }
}