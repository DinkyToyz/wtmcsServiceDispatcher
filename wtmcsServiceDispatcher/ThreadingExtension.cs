﻿using System;
using ICities;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// The actual doer.
    /// </summary>
    public class ThreadingExtension : ThreadingExtensionBase
    {
        /// <summary>
        /// The maximum exception count before mod is considered broken.
        /// </summary>
        private static readonly int MaxExceptionCount = 1;

        /// <summary>
        /// The on update exception count.
        /// </summary>
        private int exceptionCount = 0;

        /// <summary>
        /// The mod is broken.
        /// </summary>
        private bool isBroken = false;

        /// <summary>
        /// The last debug list log stamp.
        /// </summary>
        private uint lastDebugListLog = 0;

        /// <summary>
        /// The game has started.
        /// </summary>
        private bool started = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadingExtension"/> class.
        /// </summary>
        public ThreadingExtension()
            : base()
        {
            Log.Debug(this, "Constructed");
        }

        /// <summary>
        /// Called when doer is created.
        /// </summary>
        /// <param name="threading">The threading.</param>
        public override void OnCreated(IThreading threading)
        {
            Log.Debug(this, "OnCreated", "Base");
            Log.FlushBuffer();
            base.OnCreated(threading);
        }

        /// <summary>
        /// Called when doer is released.
        /// </summary>
        public override void OnReleased()
        {
            Log.Debug(this, "OnReleased", "Base");
            Log.FlushBuffer();
            base.OnReleased();
        }

        /// <summary>
        /// Called when game updates.
        /// </summary>
        /// <param name="realTimeDelta">The real time delta.</param>
        /// <param name="simulationTimeDelta">The simulation time delta.</param>
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if (this.isBroken)
            {
                return;
            }

            try
            {
                if (this.threadingManager.simulationPaused)
                {
                    return;
                }

                uint simulationFrame = this.threadingManager.simulationFrame;

                if (Global.CurrentFrame == simulationFrame)
                {
                    return;
                }

                if (Global.CurrentFrame == 0 && Log.LogDebugLists)
                {
                    VehicleHelper.DebugListLog();
                    BuildingHelper.DebugListLog();
                }

                if (Global.CurrentFrame == 0 && simulationFrame > 0)
                {
                    this.lastDebugListLog = simulationFrame;
                }

                if (this.started && Global.DetourInitNeeded)
                {
                    Global.InitDetours();
                }

                Global.CurrentFrame = simulationFrame;

                if (Global.Settings.DispatchGarbageTrucks || Global.Settings.DispatchHearses)
                {
                    // Do vehicle based stuff.
                    if (Global.Vehicles != null)
                    {
                        // Update vehicles.
                        Global.Vehicles.Update();
                    }

                    // Do building based stuff.
                    if (Global.Buildings != null)
                    {
                        // Update buildings.
                        Global.Buildings.Update();

                        // Dispatch hearses.
                        if (Global.Settings.DispatchHearses && Global.HearseDispatcher != null)
                        {
                            Global.HearseDispatcher.Dispatch();
                        }

                        // Dispatch garbage trucks;
                        if (Global.Settings.DispatchGarbageTrucks && Global.GarbageTruckDispatcher != null)
                        {
                            Global.GarbageTruckDispatcher.Dispatch();
                        }
                    }
                }

                if (Log.LogDebugLists && Global.CurrentFrame - this.lastDebugListLog >= 1800)
                {
                    this.lastDebugListLog = Global.CurrentFrame;

                    Global.Buildings.DebugListLogBuildings();
                    Log.FlushBuffer();
                }
                else if (!this.started || (Global.CurrentFrame - Log.LastFlush >= 600))
                {
                    Log.FlushBuffer();
                }

                this.started = true;
                if (this.exceptionCount > 0)
                {
                    this.exceptionCount--;
                }
            }
            catch (Exception ex)
            {
                this.exceptionCount++;
                if (this.exceptionCount > MaxExceptionCount)
                {
                    Global.RevertDetours();

                    this.isBroken = true;
                }

                Log.Error(this, "OnUpdate", ex);
            }
            finally
            {
                base.OnUpdate(realTimeDelta, simulationTimeDelta);
            }
        }
    }
}