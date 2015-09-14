using ColossalFramework;
using System;
using System.Collections.Generic;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Vehicle data.
    /// </summary>
    internal class VehicleKeeper
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
        /// Initializes a new instance of the <see cref="VehicleKeeper"/> class.
        /// </summary>
        public VehicleKeeper()
        {
            Log.Debug(this, "Constructed");
        }

        /// <summary>
        /// Updates data.
        /// </summary>
        /// <exception cref="System.Exception">Update frame loop counter to high.</exception>
        public void Update()
        {
            // Get and categorize vehicles.
            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            // First update?
            if (this.isInitialized)
            {
                // Data is initialized. Just check buildings for this frame.
                uint endFrame = this.GetFrameEnd();

                uint counter = 0;
                while (this.vehicleFrame != endFrame)
                {
                    if (counter > 256)
                    {
                        throw new Exception("Update frame loop counter to high");
                    }
                    counter++;

                    this.vehicleFrame = this.GetFrameNext(this.vehicleFrame + 1);
                    FrameBoundaries bounds = this.GetFrameBoundaries(this.vehicleFrame);

                    this.HandleVehicles(ref vehicles, buildings, bounds.FirstId, bounds.LastId);
                }
            }
            else
            {
                // Data is not initialized. Check all vehicles.
                this.HandleVehicles(ref vehicles, buildings, 0, vehicles.Length);

                this.vehicleFrame = this.GetFrameEnd();
                this.isInitialized = Global.FramedUpdates;
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

            return new FrameBoundaries(frame * 1024, ((frame + 1) * 1024) - 1);
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
        /// <param name="buildings">The buildings.</param>
        /// <param name="firstVehicleId">The first vehicle identifier.</param>
        /// <param name="lastVehicleId">The last vehicle identifier.</param>
        private void HandleVehicles(ref Vehicle[] vehicles, Building[] buildings, ushort firstVehicleId, int lastVehicleId)
        {
            for (ushort id = firstVehicleId; id < lastVehicleId; id++)
            {
                if (vehicles[id].m_leadingVehicle != 0 || vehicles[id].m_cargoParent != 0 || vehicles[id].Info == null || (vehicles[id].m_flags & Vehicle.Flags.Spawned) == Vehicle.Flags.None)
                {
                    if (this.removedFromGrid != null && this.removedFromGrid.Contains(id))
                    {
                        this.removedFromGrid.Remove(id);
                    }
                }
                else
                {
                    if ((vehicles[id].m_flags & Vehicle.Flags.TransferToSource) != Vehicle.Flags.None && (vehicles[id].m_flags & (Vehicle.Flags.TransferToTarget | Vehicle.Flags.Arriving | Vehicle.Flags.Stopped)) == Vehicle.Flags.None &&
                        vehicles[id].m_targetBuilding != 0 && vehicles[id].m_targetBuilding != vehicles[id].m_sourceBuilding && (buildings[vehicles[id].m_sourceBuilding].m_flags & Building.Flags.Downgrading) == Building.Flags.None)
                    {
                        if (Global.HearseDispatcher != null && vehicles[id].m_transferType == Global.HearseDispatcher.TransferType)
                        {
                            Global.HearseDispatcher.CheckVehicleTarget(id, ref vehicles[id]);
                        }

                        if (Global.GarbageTruckDispatcher != null && vehicles[id].m_transferType == Global.GarbageTruckDispatcher.TransferType)
                        {
                            Global.GarbageTruckDispatcher.CheckVehicleTarget(id, ref vehicles[id]);
                        }
                    }

                    if (this.removedFromGrid != null)
                    {
                        if ((vehicles[id].m_flags & Vehicle.Flags.Stopped) == Vehicle.Flags.None)
                        {
                            if (this.removedFromGrid.Contains(id))
                            {
                                if (Log.LogToFile) Log.Debug(this, "HandleVehicles", "Moving", id, vehicles[id].m_targetBuilding, vehicles[id].Info.name, VehicleHelper.GetVehicleName(id), vehicles[id].m_flags);

                                this.removedFromGrid.Remove(id);
                            }
                        }
                        else if (((Global.Settings.RemoveHearsesFromGrid && vehicles[id].Info.m_vehicleAI is HearseAI) /* ||
                                  (Global.Settings.RemoveGarbageTrucksFromGrid && vehicles[id].Info.m_vehicleAI is GarbageTruckAI)*/) &&
                                 !this.removedFromGrid.Contains(id))
                        {
                            if (Log.LogToFile) Log.Debug(this, "HandleVehicles", "RemoveFromGrid", id, vehicles[id].m_targetBuilding, vehicles[id].Info.name, VehicleHelper.GetVehicleName(id), vehicles[id].m_flags);

                            Singleton<VehicleManager>.instance.RemoveFromGrid(id, ref vehicles[id], false);
                            this.removedFromGrid.Add(id);
                        }
                    }
                }
            }
        }
    }
}
