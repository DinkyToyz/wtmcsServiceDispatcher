using ColossalFramework;
using System;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Vehicle data.
    /// </summary>
    internal class VehicleKeeper : IHandlerPart
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
        /// Initializes a new instance of the <see cref="VehicleKeeper"/> class.
        /// </summary>
        public VehicleKeeper()
        {
            this.ReInitialize();
            Log.Debug(this, "Constructed");
        }

        /// <summary>
        /// Re-initialize the part.
        /// </summary>
        public void ReInitialize()
        { }

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

                    this.bucketeer = new Bucketeer(this.bucketMask, this.bucketFactor);
                    this.bucket = this.bucketeer.GetEnd();
                }

                this.HandleVehicles();
            }

            if (Global.VehicleUpdateNeeded)
            {
                if (Global.Services != null)
                {
                    Global.Services.UpdateAllVehicles();
                }

                Global.VehicleUpdateNeeded = false;
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
            if (Global.Services != null)
            {
                Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
                Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

                for (ushort id = firstVehicleId; id < lastVehicleId; id++)
                {
                    // Is the vehicle?
                    if (vehicles[id].m_leadingVehicle != 0 || vehicles[id].m_cargoParent != 0 || vehicles[id].Info == null || (vehicles[id].m_flags & VehicleHelper.VehicleExists) == ~VehicleHelper.VehicleAll)
                    {
                        Global.Services.RemoveVehicle(id);
                    }
                    else
                    {
                        // Check target assignments for service vehicles.
                        if ((vehicles[id].m_flags & Vehicle.Flags.TransferToSource) != ~VehicleHelper.VehicleAll &&
                            (vehicles[id].m_flags & (Vehicle.Flags.TransferToTarget | Vehicle.Flags.Arriving | Vehicle.Flags.Stopped)) == ~VehicleHelper.VehicleAll &&
                            (vehicles[id].m_flags & VehicleHelper.VehicleUnavailable) == ~VehicleHelper.VehicleAll &&
                            vehicles[id].m_targetBuilding != vehicles[id].m_sourceBuilding && (buildings[vehicles[id].m_sourceBuilding].m_flags & Building.Flags.Downgrading) == Building.Flags.None)
                        {
                            Global.Services.CheckVehicleTarget(id, ref vehicles[id]);
                        }

                        // Check vehicle.
                        Global.Services.CheckVehicle(id, ref vehicles[id]);
                    }
                }
            }
        }
    }
}