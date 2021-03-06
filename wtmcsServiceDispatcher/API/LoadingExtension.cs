﻿using ICities;
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
            Log.Debug(this, "OnCreated");
            Global.Initialize();
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
                    // Load settings and initialize stuff.
                    Global.Initialize();

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
                    this.DeInitialize();
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
                Detours.LogCounts();
                this.DeInitialize();
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
                this.DeInitialize();
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
        /// Uninitializes data.
        /// </summary>
        private void DeInitialize()
        {
            Global.LevelLoaded = false;

            Log.Info(this, "DeInitialize");

            Global.DeInitialize();
            Detours.Dispose();
            Global.DisposeHandlers();
        }
    }
}