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
        /// The hearse vehicles.
        /// </summary>
        private Dictionary<ushort, ServiceVehicleInfo> hearseVehicles = new Dictionary<ushort, ServiceVehicleInfo>();

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
        /// Gets the hearse vehicles.
        /// </summary>
        /// <value>
        /// The hearse vehicles.
        /// </value>
        public IEnumerable<ServiceVehicleInfo> HearseVehicles
        {
            get
            {
                return hearseVehicles.Values;
            }
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
                    Types.FrameBoundaries bounds = GetFrameBoundaries(vehicleFrame);

                    CategorizeVehicles(vehicles, bounds.First, bounds.Last);
                }
            }
            else
            {
                // Data is not initialized. Check all vehicles.
                if (Log.LogALot && Library.IsDebugBuild) Log.Debug(this, "OnUpdate", "Intialize");

                CategorizeVehicles(vehicles, 0, vehicles.Length);

                vehicleFrame = GetFrameEnd();
                isInitialized = true;
            }

            if (Log.LogALot && Library.IsDebugBuild) Log.Debug(this, "Update", "End");
        }

        private void CategorizeVehicles(Vehicle[] vehicles, ushort first, int last)
        {
            for (ushort id = first; id < last; id++)
            {
                if (vehicles[id].m_leadingVehicle != 0
                    || vehicles[id].m_cargoParent != 0
                    || vehicles[id].Info == null
                    || vehicles[id].Info.m_vehicleAI is PoliceCarAI
                    //|| (vehicles[id].m_flags & Vehicle.Flags.Spawned) == Vehicle.Flags.None
                    )
                {
                    if (Global.Settings.HandleHearses)
                    {
                        hearseVehicles.Remove(id);
                    }
                }
                else
                {
                    CategorizeVehicle(id, vehicles[id]);
                }
            }
        }

        /// <summary>
        /// Categorizes the vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        private void CategorizeVehicle(ushort vehicleId, Vehicle vehicle)
        {
            if (Log.LogALot && Library.IsDebugBuild) Log.Debug(this, "CategorizeVehicle", vehicleId, vehicle.Info.name, vehicle.Info.m_vehicleAI.GetType(), vehicle.m_targetBuilding, (vehicle.m_flags & Vehicle.Flags.Spawned));

            if (Global.Settings.HandleHearses)
            {
                // Check free hearses.
                if (vehicle.Info.m_vehicleAI is HearseAI && vehicle.m_targetBuilding == 0)
                {
                    if (!hearseVehicles.ContainsKey(vehicleId))
                    {
                        if (Log.LogALot && Library.IsDebugBuild) Log.Debug(this, "CategorizeVehicle", "Free Hearse", vehicleId);

                        hearseVehicles[vehicleId] = new ServiceVehicleInfo(vehicleId, vehicle);
                    }
                    else
                    {
                        hearseVehicles[vehicleId].SetInfo(vehicle);
                    }
                }
                else if (hearseVehicles.ContainsKey(vehicleId))
                {
                    if (Log.LogALot && Library.IsDebugBuild) Log.Debug(this, "CategorizeVehicle", "Not Free Hearse", vehicleId);

                    hearseVehicles.Remove(vehicleId);
                }
            }
        }

        /// <summary>
        /// Gets the frame boundaries.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>The frame boundaries.</returns>
        private Types.FrameBoundaries GetFrameBoundaries(uint frame)
        {
            frame = frame & 15;

            return new Types.FrameBoundaries(frame * 1024, (frame + 1) * 1024 - 1);
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
        /// Info about service vehicle.
        /// </summary>
        public class ServiceVehicleInfo
        {
            /// <summary>
            /// The position.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// The source building.
            /// </summary>
            public ushort SourceBuilding = 0;

            /// <summary>
            /// The vehicle identifier.
            /// </summary>
            public ushort VehicleId = 0;

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceVehicleInfo"/> class.
            /// </summary>
            public ServiceVehicleInfo()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceVehicleInfo"/> class.
            /// </summary>
            /// <param name="vehicleId">The vehicle identifier.</param>
            /// <param name="vehicle">The vehicle.</param>
            public ServiceVehicleInfo(ushort vehicleId, Vehicle vehicle)
            {
                this.VehicleId = vehicleId;
                this.SourceBuilding = vehicle.m_sourceBuilding;
                this.Position = vehicle.GetLastFramePosition();
            }

            /// <summary>
            /// Sets the information.
            /// </summary>
            /// <param name="vehicle">The vehicle.</param>
            public void SetInfo(Vehicle vehicle)
            {
                this.SourceBuilding = vehicle.m_sourceBuilding;
                this.Position = vehicle.GetLastFramePosition();
            }
        }
    }
}