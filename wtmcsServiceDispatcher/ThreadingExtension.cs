using ICities;
using System;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// The actual doer.
    /// </summary>
    public class ThreadingExtension : ThreadingExtensionBase
    {
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

                Global.CurrentFrame = simulationFrame;

                // Do vehicle based stuff.
                if (Global.Vehicles != null)
                {
                    // Update vehicles.
                    Global.Vehicles.Update();
                }

                // Do bulding based stuff.
                if (Global.Buildings != null)
                {
                    // Update buildings.
                    Global.Buildings.Update();

                    // Dispatch hearses.
                    if (Global.HearseDispatcher != null)
                    {
                        Global.HearseDispatcher.Dispatch();
                    }

                    // Dispatch garbage trucks;
                    if (Global.GarbageTruckDispatcher != null)
                    {
                        Global.GarbageTruckDispatcher.Dispatch();
                    }

                    if (Log.LogDebugLists && Global.CurrentFrame - this.lastDebugListLog >= 1800)
                    {
                        this.lastDebugListLog = Global.CurrentFrame;

                        Global.Buildings.DebugListLogBuildings();
                        Log.FlushBuffer();
                    }
                }

                if (!this.started || (Global.CurrentFrame - Log.LastFlush >= 600))
                {
                    Log.FlushBuffer();
                }

                this.started = true;
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnUpdate", ex);
                this.isBroken = true;
            }
            finally
            {
                base.OnUpdate(realTimeDelta, simulationTimeDelta);
            }
        }
    }
}
