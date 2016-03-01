using ColossalFramework;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Info about a service vehicle.
    /// </summary>
    internal class ServiceVehicleInfo
    {
        /// <summary>
        /// The last target de-assign time stamp.
        /// </summary>
        private uint lastDeAssignStamp = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceVehicleInfo" /> class.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="freeToCollect">If set to <c>true</c> the vehicle is free.</param>
        public ServiceVehicleInfo(ushort vehicleId, ref Vehicle vehicle, bool freeToCollect)
        {
            this.VehicleId = vehicleId;
            this.LastAssigned = 0;

            this.Update(ref vehicle, freeToCollect, false);
        }

        /// <summary>
        /// Gets the free capacity.
        /// </summary>
        public int CapacityFree
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the used capacity in percent.
        /// </summary>
        public float CapacityUsed
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the vehicle is free.
        /// </summary>
        public bool FreeToCollect
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the vehicle is going back the the service building.
        /// </summary>
        public bool GoingBack
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the last target assigned stamp.
        /// </summary>
        public uint LastAssigned
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the last seen stamp.
        /// </summary>
        public uint LastSeen
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the position.
        /// </summary>
        public Vector3 Position
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the target.
        /// </summary>
        public ushort Target
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the vehicle identifier.
        /// </summary>
        public ushort VehicleId
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the vehicle.
        /// </summary>
        /// <value>
        /// The name of the vehicle.
        /// </value>
        public string VehicleName
        {
            get
            {
                return VehicleHelper.GetVehicleName(this.VehicleId);
            }
        }

        /// <summary>
        /// Creates the specified service building.
        /// </summary>
        /// <param name="serviceBuilding">The service building.</param>
        /// <param name="material">The material.</param>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <returns>
        /// The service vehicle.
        /// </returns>
        public static ServiceVehicleInfo Create(ServiceBuildingInfo serviceBuilding, TransferManager.TransferReason material, ushort targetBuildingId = 0)
        {
            ushort vehicleId;
            VehicleInfo info = VehicleHelper.CreateServiceVehicle(serviceBuilding, material, targetBuildingId, out vehicleId);

            if (info == null)
            {
                return null;
            }

            VehicleManager manager = Singleton<VehicleManager>.instance;

            return new ServiceVehicleInfo(vehicleId, ref manager.m_vehicles.m_buffer[vehicleId], true);
        }

        /// <summary>
        /// De-assign target from vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>True if target is or was de-assigned.</returns>
        public bool DeAssign(ref Vehicle vehicle)
        {
            this.lastDeAssignStamp = Global.CurrentFrame;

            if (VehicleHelper.CanRecallVehicle(ref vehicle) && Global.CurrentFrame - this.LastAssigned > Global.RecallDelay)
            {
                if (!(vehicle.m_targetBuilding == 0 && this.Target == 0 && this.GoingBack && (vehicle.m_flags & Vehicle.Flags.GoingBack) == Vehicle.Flags.GoingBack))
                {
                    this.Recall(ref vehicle);
                }

                return true;
            }

            if (vehicle.m_targetBuilding == 0 && this.Target == 0)
            {
                return true;
            }

            if (Global.CurrentFrame - this.LastAssigned > Global.TargetLingerDelay)
            {
                if (Log.LogALot)
                {
                    Log.DevDebug(this, "DeAssign", this.VehicleId);
                }

                this.SetTarget(0, ref vehicle);
                return true;
            }

            return this.Target == 0;
        }

        /// <summary>
        /// Recalls the vehicle to the service building.
        /// </summary>
        /// <returns>True if vehicle recalled and found path the source.</returns>
        public bool Recall()
        {
            // Get vehicle.
            return this.Recall(ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[this.VehicleId]);
        }

        /// <summary>
        /// Recalls the vehicle to the service building.
        /// </summary>
        /// <param name="vehicle">The vehicle data.</param>
        /// <returns>True if vehicle recalled and found path the source.</returns>
        public bool Recall(ref Vehicle vehicle)
        {
            // Set internal target.
            this.Target = 0;
            this.GoingBack = true;

            if (Log.LogALot)
            {
                Log.DevDebug(this, "Recall", this.VehicleId);
            }

            // Recall the vehicle.
            return VehicleHelper.RecallVehicle(this.VehicleId, ref vehicle);
        }

        /// <summary>
        /// Sets the target and updates the games vehicle object.
        /// </summary>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>True if target set vehicle found path the target.</returns>
        public bool SetTarget(ushort targetBuildingId, ref Vehicle vehicle)
        {
            if (targetBuildingId == 0 && this.GoingBack)
            {
                return this.Recall(ref vehicle);
            }

            if (VehicleHelper.SetTarget(this.VehicleId, ref vehicle, targetBuildingId))
            {
                if (targetBuildingId != 0)
                {
                    this.LastAssigned = Global.CurrentFrame;
                    this.FreeToCollect = false;
                    this.GoingBack = false;
                }

                this.Target = targetBuildingId;
                return true;
            }

            this.LastAssigned = 0;
            this.FreeToCollect = (vehicle.m_flags & VehicleHelper.VehicleExists) != Vehicle.Flags.None;
            this.GoingBack = false;
            this.Target = 0;

            return false;
        }

        /// <summary>
        /// Updates the specified vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="freeToCollect">If set to <c>true</c> the vehicle is free.</param>
        /// <param name="checkAssignment">If set to <c>true</c> check vehicles assignment and possibly de-assign/recall vehicle].</param>
        public void Update(ref Vehicle vehicle, bool freeToCollect, bool checkAssignment = true)
        {
            this.LastSeen = Global.CurrentFrame;
            this.Position = vehicle.GetLastFramePosition();
            this.FreeToCollect = freeToCollect;
            this.GoingBack = vehicle.m_targetBuilding == 0 && (vehicle.m_flags & Vehicle.Flags.GoingBack) == Vehicle.Flags.GoingBack;
            this.Target = vehicle.m_targetBuilding;

            if (checkAssignment && (vehicle.m_flags & VehicleHelper.VehicleUnavailable) == Vehicle.Flags.None)
            {
                if (vehicle.m_targetBuilding == 0)
                {
                    if (!this.GoingBack && Global.CurrentFrame - this.LastAssigned > Global.RecallDelay && VehicleHelper.CanRecallVehicle(ref vehicle))
                    {
                        if (Log.LogALot)
                        {
                            Log.DevDebug(this, "Update", "CheckAssignment", "Recall", this.VehicleId, vehicle.m_flags);
                        }

                        this.Recall(ref vehicle);
                    }
                }
                else
                {
                    if (this.LastAssigned >= this.lastDeAssignStamp)
                    {
                        this.LastAssigned = Global.CurrentFrame;
                    }
                    else if (Global.CurrentFrame - this.LastAssigned > Global.DemandLingerDelay)
                    {
                        if (Log.LogALot)
                        {
                            Log.DevDebug(this, "Update", "CheckAssignment", "DeAssign", vehicle.m_targetBuilding, this.VehicleId);
                        }

                        this.DeAssign(ref vehicle);
                    }
                }
            }

            string localeKey;
            int bufCur, bufMax;
            vehicle.Info.m_vehicleAI.GetBufferStatus(this.VehicleId, ref vehicle, out localeKey, out bufCur, out bufMax);
            this.CapacityFree = bufMax - bufCur;
            this.CapacityUsed = (float)bufCur / (float)bufMax;
        }
    }
}