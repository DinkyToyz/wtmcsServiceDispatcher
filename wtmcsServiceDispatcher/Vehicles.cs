using ColossalFramework;
using System;

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
            if (Log.LogALot && Library.IsDebugBuild) Log.Debug(this, "Update", "Begin");

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

                    CategorizeVehicles(ref vehicles, bounds.FirstId, bounds.LastId);
                }
            }
            else
            {
                // Data is not initialized. Check all vehicles.
                if (Log.LogALot && Library.IsDebugBuild) Log.Debug(this, "OnUpdate", "Intialize");

                CategorizeVehicles(ref vehicles, 0, vehicles.Length);

                vehicleFrame = GetFrameEnd();
                isInitialized = true;
            }

            if (Log.LogALot && Library.IsDebugBuild) Log.Debug(this, "Update", "End");
        }

        /// <summary>
        /// Categorizes the vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        private void CategorizeVehicle(ushort vehicleId, ref Vehicle vehicle)
        {
        }

        /// <summary>
        /// Categorizes the vehicles.
        /// </summary>
        /// <param name="vehicles">The vehicles.</param>
        /// <param name="firstVehicleId">The first vehicle identifier.</param>
        /// <param name="lastVehicleId">The last vehicle identifier.</param>
        private void CategorizeVehicles(ref Vehicle[] vehicles, ushort firstVehicleId, int lastVehicleId)
        {
            for (ushort id = firstVehicleId; id < lastVehicleId; id++)
            {
                if (vehicles[id].m_leadingVehicle != 0 || vehicles[id].m_cargoParent != 0 || vehicles[id].Info == null || (vehicles[id].m_flags & Vehicle.Flags.Spawned) == Vehicle.Flags.None)
                {
                }
                else
                {
                    CategorizeVehicle(id, ref vehicles[id]);
                }
            }
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
    }
}