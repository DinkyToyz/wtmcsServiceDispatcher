using ColossalFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Vehicle data.
    /// </summary>
    internal class Vehicles
    {
        /// <summary>
        /// The data is initialized.
        /// </summary>
        private bool isInitialized = false;

        /// <summary>
        /// The vehicles that have been removed from grid.
        /// </summary>
        private HashSet<ushort> removedFromGrid = new HashSet<ushort>();

        /// <summary>
        /// The current/last building frame.
        /// </summary>
        private uint vehicleFrame;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vehicles"/> class.
        /// </summary>
        public Vehicles()
        {
            Log.Debug(this, "Constructed");
        }

        /// <summary>
        /// Updates data.
        /// </summary>
        public void Update()
        {
            // Get and categorize vehicles.
            // if (Log.LogALot && Library.IsDebugBuild) Log.DevDebug(this, "Update", "Begin");

            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            // First update?
            if (isInitialized)
            {
                // Data is initialized. Just check buildings for this frame.

                uint endFrame = GetFrameEnd();

                uint counter = 0;
                while (vehicleFrame != endFrame)
                {
                    if (counter > 256)
                    {
                        throw new Exception("Update frame loop counter to high!");
                    }
                    counter++;

                    vehicleFrame = GetFrameNext(vehicleFrame + 1);
                    FrameBoundaries bounds = GetFrameBoundaries(vehicleFrame);

                    HandleVehicles(ref vehicles, bounds.FirstId, bounds.LastId);
                }
            }
            else
            {
                // Data is not initialized. Check all vehicles.
                // if (Log.LogALot && Library.IsDebugBuild) Log.DevDebug(this, "OnUpdate", "Intialize");

                HandleVehicles(ref vehicles, 0, vehicles.Length);

                vehicleFrame = GetFrameEnd();
                isInitialized = Global.FramedUpdates;
            }

            // if (Log.LogALot && Library.IsDebugBuild) Log.DevDebug(this, "Update", "End");
        }

        /// <summary>
        /// Gets the frame boundaries.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>The frame boundaries.</returns>
        private FrameBoundaries GetFrameBoundaries(uint frame)
        {
            frame = frame & 15;

            return new FrameBoundaries(frame * 1024, (frame + 1) * 1024 - 1);
        }

        /// <summary>
        /// Gets the end frame.
        /// </summary>
        /// <returns>The end frame.</returns>
        private uint GetFrameEnd()
        {
            return Singleton<SimulationManager>.instance.m_currentFrameIndex & 15;
        }

        /// <summary>
        /// Gets the next frame.
        /// </summary>
        /// <param name="frame">The current/last frame.</param>
        /// <returns>The next frame.</returns>
        private uint GetFrameNext(uint frame)
        {
            return frame & 15;
        }

        /// <summary>
        /// Categorizes the vehicles.
        /// </summary>
        /// <param name="vehicles">The vehicles.</param>
        /// <param name="firstVehicleId">The first vehicle identifier.</param>
        /// <param name="lastVehicleId">The last vehicle identifier.</param>
        private void HandleVehicles(ref Vehicle[] vehicles, ushort firstVehicleId, int lastVehicleId)
        {
            for (ushort id = firstVehicleId; id < lastVehicleId; id++)
            {
                if (vehicles[id].m_leadingVehicle != 0 || vehicles[id].m_cargoParent != 0 || vehicles[id].Info == null || (vehicles[id].m_flags & Vehicle.Flags.Spawned) == Vehicle.Flags.None)
                {
                    if (removedFromGrid != null && removedFromGrid.Contains(id))
                    {
                        removedFromGrid.Remove(id);
                    }
                }
                else
                {
                    if (removedFromGrid != null)
                    {
                        if ((vehicles[id].m_flags & Vehicle.Flags.Stopped) == Vehicle.Flags.None)
                        {
                            if (removedFromGrid.Contains(id))
                            {
                                removedFromGrid.Remove(id);
                            }
                        }
                        else if (Global.Settings.RemoveHearsesFromGrid && vehicles[id].Info.m_vehicleAI is HearseAI && !removedFromGrid.Contains(id))
                        {
                            if (Log.LogALot || Library.IsDebugBuild) Log.DevDebug(this, "HandleVehicles", "RemoveFromGrid", id, vehicles[id].Info.name);

                            Singleton<VehicleManager>.instance.RemoveFromGrid(id, ref vehicles[id], false);
                            removedFromGrid.Add(id);
                        }
                    }
                }
            }
        }

        public class ServiceVehicleInfo
        {
            /// <summary>
            /// The last seen stamp.
            /// </summary>
            public uint LastSeen;

            /// <summary>
            /// The position.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// The target
            /// </summary>
            public ushort Target;

            /// <summary>
            /// The vehicle identifier.
            /// </summary>
            public ushort VehicleId;

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceVehicleInfo"/> class.
            /// </summary>
            /// <param name="vehicleId">The vehicle identifier.</param>
            /// <param name="vehicle">The vehicle.</param>
            public ServiceVehicleInfo(ushort vehicleId, ref Vehicle vehicle)
            {
                this.VehicleId = vehicleId;
                this.LastSeen = Global.CurrentFrame;
                this.Position = vehicle.GetLastFramePosition();
                this.Target = vehicle.m_targetBuilding;
            }

            /// <summary>
            /// Updates the specified vehicle.
            /// </summary>
            /// <param name="vehicle">The vehicle.</param>
            public void Update(ref Vehicle vehicle)
            {
                this.LastSeen = Global.CurrentFrame;
                this.Position = vehicle.GetLastFramePosition();
                this.Target = vehicle.m_targetBuilding;
            }
        }
    }
}