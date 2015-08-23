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
        /// Called when gane updates.
        /// </summary>
        /// <param name="realTimeDelta">The real time delta.</param>
        /// <param name="simulationTimeDelta">The simulation time delta.</param>
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if (isBroken)
            {
                return;
            }

            try
            {
                if (this.threadingManager.simulationPaused)
                {
                    return;
                }

                if (Global.CurrentFrame == this.threadingManager.simulationFrame)
                {
                    return;
                }

                if (Global.CurrentFrame == 0 && Log.LogDebugLists)
                {
                    Vehicles.DebugListLog();
                    Buildings.DebugListLog();
                }

                Global.CurrentFrame = this.threadingManager.simulationFrame;

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

                    ////// Dispatch garbage trucks;
                    ////if (Global.GarbageTruckDispatcher != null)
                    ////{
                    ////    Global.GarbageTruckDispatcher.Dispatch();
                    ////}
                }

                if (!started || (Global.CurrentFrame - Log.LastFlush >= 600))
                {
                    Log.FlushBuffer();
                }

                started = true;
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnUpdate", ex);
                isBroken = true;
            }
            finally
            {
                base.OnUpdate(realTimeDelta, simulationTimeDelta);
            }
        }
    }
}