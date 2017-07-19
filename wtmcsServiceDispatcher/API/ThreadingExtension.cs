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
        /// The last transfer offers clean stamp.
        /// </summary>
        private uint lastTransferOffersClean = 0;

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
            Log.Debug(this, "OnCreated");
            Log.FlushBuffer();
            base.OnCreated(threading);
        }

        /// <summary>
        /// Called when doer is released.
        /// </summary>
        public override void OnReleased()
        {
            Log.Debug(this, "OnReleased");
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

                Global.SimulationTime += simulationTimeDelta;

                uint simulationFrame = this.threadingManager.simulationFrame;

                if (Global.CurrentFrame == simulationFrame)
                {
                    return;
                }

                if (Global.CurrentFrame == 0)
                {
                    Global.LogDebugLists();
                }

                if (Global.CurrentFrame == 0 && simulationFrame > 0)
                {
                    this.lastDebugListLog = simulationFrame;
                }

                if (this.started && Detours.InitNeeded)
                {
                    Detours.Initialize();
                }

                Global.CurrentFrame = simulationFrame;

                if (Global.ServiceProblems != null && Global.CurrentFrame - Global.ServiceProblems.LastUpdate >= Global.ProblemUpdateDelay)
                {
                    Global.ServiceProblems.Update();
                }

                if (Global.Settings.DispatchAnyVehicles || Global.Settings.AutoEmptyAnyBuildings)
                {
                    // Do building stuff.
                    if (Global.Buildings != null)
                    {
                        // Update buildings.
                        Global.Buildings.Update();
                    }

                    // Do vehicle based stuff.
                    if (Global.Vehicles != null)
                    {
                        // Update vehicles.
                        Global.Vehicles.Update();
                    }

                    // Do building based stuff.
                    if (Global.Buildings != null)
                    {
                        // Dispatch hearses.
                        if (Global.Settings.DeathCare.DispatchVehicles && Global.HearseDispatcher != null)
                        {
                            Global.HearseDispatcher.Dispatch();
                        }

                        // Dispatch garbage trucks;
                        if (Global.Settings.Garbage.DispatchVehicles && Global.GarbageTruckDispatcher != null)
                        {
                            Global.GarbageTruckDispatcher.Dispatch();
                        }

                        // Dispatch ambulances.
                        if (Global.Settings.HealthCare.DispatchVehicles && Global.AmbulanceDispatcher != null)
                        {
                            Global.AmbulanceDispatcher.Dispatch();
                        }
                    }

                    if (Global.TransferOffersCleaningNeeded || Global.CurrentFrame - this.lastTransferOffersClean > Global.CleanTransferOffersDelay)
                    {
                        if (Global.CleanTransferOffers)
                        {
                            TransferManagerHelper.CleanTransferOffers();
                        }

                        this.lastTransferOffersClean = Global.CurrentFrame;
                        Global.TransferOffersCleaningNeeded = false;
                    }
                }

                if (Global.CurrentFrame - this.lastDebugListLog >= Global.DebugListLogDelay)
                {
                    this.lastDebugListLog = Global.CurrentFrame;

                    Global.LogDebugLists();
                }
                else if (!this.started || (Global.CurrentFrame - Log.LastFlush >= Global.LogFlushDelay))
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
                    try
                    {
                        Detours.Revert();
                    }
                    catch (Exception rex)
                    {
                        Log.Error(this, "OnUpdate", rex);
                    }

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