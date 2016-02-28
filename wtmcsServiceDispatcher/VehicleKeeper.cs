﻿using System;
using System.Collections.Generic;
using ColossalFramework;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Vehicle data.
    /// </summary>
    internal class VehicleKeeper
    {
        /// <summary>
        /// The current/last update bucket.
        /// </summary>
        private uint bucket;

        /// <summary>
        /// The vehicle object bucket manager.
        /// </summary>
        private Bucketeer bucketeer;

        /// <summary>
        /// The bucket factor.
        /// </summary>
        private uint bucketFactor = 1024;

        /// <summary>
        /// The bucket mask.
        /// </summary>
        private uint bucketMask = 15;

        /// <summary>
        /// The vehicles that have been removed from grid.
        /// </summary>
        private HashSet<ushort> removedFromGrid = new HashSet<ushort>();

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
        /// <exception cref="System.Exception">Update bucket loop counter to high.</exception>
        public void Update()
        {
            // Get and categorize vehicles.

            // First update?
            if (this.bucketeer != null)
            {
                // Data is initialized. Just check buildings for this frame.
                uint endBucket = this.bucketeer.GetEnd();

                uint counter = 0;
                while (this.bucket != endBucket)
                {
                    if (counter > 256)
                    {
                        throw new Exception("Update bucket loop counter to high");
                    }
                    counter++;

                    this.bucket = this.bucketeer.GetNext(this.bucket + 1);
                    Bucketeer.Boundaries bounds = this.bucketeer.GetBoundaries(this.bucket);

                    this.HandleVehicles(bounds.FirstId, bounds.LastId);
                }
            }
            else
            {
                if (Global.BucketedUpdates)
                {
                    uint length = (uint)Singleton<VehicleManager>.instance.m_vehicles.m_buffer.Length;
                    this.bucketFactor = length / (this.bucketMask + 1);
                    Log.Debug(this, "Update", "bucketFactor", length, this.bucketMask, this.bucketFactor);

                    this.bucketeer = new Bucketeer(this.bucketMask, this.bucketFactor);
                    this.bucket = this.bucketeer.GetEnd();
                }

                this.HandleVehicles();
            }
        }

        /// <summary>
        /// Categorizes the vehicles.
        /// </summary>
        private void HandleVehicles()
        {
            this.HandleVehicles(0, Singleton<VehicleManager>.instance.m_vehicles.m_buffer.Length);
        }

        /// <summary>
        /// Categorizes the vehicles.
        /// </summary>
        /// <param name="firstVehicleId">The first vehicle identifier.</param>
        /// <param name="lastVehicleId">The last vehicle identifier.</param>
        private void HandleVehicles(ushort firstVehicleId, int lastVehicleId)
        {
            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            for (ushort id = firstVehicleId; id < lastVehicleId; id++)
            {
                if (vehicles[id].m_leadingVehicle != 0 || vehicles[id].m_cargoParent != 0 || vehicles[id].Info == null || (vehicles[id].m_flags & VehicleHelper.VehicleExists) == Vehicle.Flags.None)
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
                                if (Log.LogToFile)
                                {
                                    Log.Debug(this, "HandleVehicles", "Moving", id, vehicles[id].m_targetBuilding, vehicles[id].Info.name, VehicleHelper.GetVehicleName(id), vehicles[id].m_flags);
                                }

                                this.removedFromGrid.Remove(id);
                            }
                        }
                        else if ((Global.Settings.RemoveHearsesFromGrid && vehicles[id].Info.m_vehicleAI is HearseAI) &&
                                 !this.removedFromGrid.Contains(id))
                        {
                            if (Log.LogToFile)
                            {
                                Log.Debug(this, "HandleVehicles", "RemoveFromGrid", id, vehicles[id].m_targetBuilding, vehicles[id].Info.name, VehicleHelper.GetVehicleName(id), vehicles[id].m_flags);
                            }

                            Singleton<VehicleManager>.instance.RemoveFromGrid(id, ref vehicles[id], false);
                            this.removedFromGrid.Add(id);
                        }
                    }
                }
            }
        }
    }
}