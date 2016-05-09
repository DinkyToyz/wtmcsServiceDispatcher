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
        /// The last confused check stamp.
        /// </summary>
        private uint confusedStamp = 0;

        /// <summary>
        /// The dispatcher type.
        /// </summary>
        private Dispatcher.DispatcherTypes dispatcherType = Dispatcher.DispatcherTypes.None;

        /// <summary>
        /// The vehicle is confused.
        /// </summary>
        private bool isConfused = false;

        /// <summary>
        /// The last target de-assign time stamp.
        /// </summary>
        private uint lastDeAssignStamp = 0;

        /// <summary>
        /// The last target stamp.
        /// </summary>
        private double lastTargetStamp = 0.0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceVehicleInfo" /> class.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="freeToCollect">If set to <c>true</c> the vehicle is free.</param>
        /// <param name="dispatcherType">Type of the dispatcher.</param>
        public ServiceVehicleInfo(ushort vehicleId, ref Vehicle vehicle, bool freeToCollect, Dispatcher.DispatcherTypes dispatcherType, ushort targetBuildingId = 0)
        {
            this.VehicleId = vehicleId;
            this.dispatcherType = dispatcherType;
            this.LastAssigned = 0;
            this.Target = targetBuildingId;

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
        /// Gets the name of the district.
        /// </summary>
        /// <value>
        /// The name of the district.
        /// </value>
        public string DistrictName
        {
            get
            {
                return DistrictHelper.GetDistrictName(this.Position);
            }
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
        /// Gets or sets a value indicating whether this vehicle confused.
        /// </summary>
        /// <value>
        /// <c>true</c> if this vehicle is confused; otherwise, <c>false</c>.
        /// </value>
        public bool IsConfused
        {
            get
            {
                if (this.confusedStamp != Global.CurrentFrame)
                {
                    Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

                    this.isConfused = ConfusionHelper.VehicleIsConfused(ref vehicles[this.VehicleId]);
                    this.confusedStamp = Global.CurrentFrame;
                }

                return this.isConfused;
            }

            set
            {
                this.isConfused = value;
                this.confusedStamp = Global.CurrentFrame;
            }
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
        /// <param name="dispatcherType">Type of the dispatcher.</param>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <returns>
        /// The service vehicle.
        /// </returns>
        public static ServiceVehicleInfo Create(ServiceBuildingInfo serviceBuilding, TransferManager.TransferReason material, Dispatcher.DispatcherTypes dispatcherType, ushort targetBuildingId = 0)
        {
            ushort vehicleId;
            VehicleInfo info = VehicleHelper.CreateServiceVehicle(serviceBuilding, material, targetBuildingId, out vehicleId);

            if (info == null)
            {
                return null;
            }

            VehicleManager manager = Singleton<VehicleManager>.instance;

            return new ServiceVehicleInfo(vehicleId, ref manager.m_vehicles.m_buffer[vehicleId], targetBuildingId == 0, dispatcherType, targetBuildingId);
        }

        /// <summary>
        /// De-assign target from vehicle.
        /// </summary>
        /// <param name="force">If set to <c>true</c> force de-assignment.</param>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="sourceBlock">The source block.</param>
        /// <param name="logMessage">The log message.</param>
        /// <returns>
        /// True if target is or was de-assigned.
        /// </returns>
        public bool DeAssign(bool force = false, object sourceObject = null, string sourceBlock = null, string logMessage = null)
        {
            return this.DeAssign(ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[this.VehicleId], force, sourceObject, sourceBlock, logMessage);
        }

        /// <summary>
        /// De-assign target from vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="force">If set to <c>true</c> force de-assignment.</param>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="sourceBlock">The source block.</param>
        /// <param name="logMessage">The log message.</param>
        /// <returns>
        /// True if target is or was de-assigned.
        /// </returns>
        public bool DeAssign(ref Vehicle vehicle, bool force = false, object sourceObject = null, string sourceBlock = null, string logMessage = null)
        {
            if (this.lastDeAssignStamp == Global.CurrentFrame)
            {
                return vehicle.m_targetBuilding == 0 && this.Target == 0;
            }

            if (force || Global.CurrentFrame - this.LastAssigned > Global.TargetLingerDelay || vehicle.m_targetBuilding != this.Target)
            {
                this.lastDeAssignStamp = Global.CurrentFrame;

                if (vehicle.m_targetBuilding == 0 && this.Target == 0)
                {
                    return true;
                }

                if (Log.LogALot && (sourceObject != null || sourceBlock != null || logMessage != null))
                {
                    Log.DevDebug(this, "DeAssign", sourceBlock, logMessage, this.VehicleId, vehicle.m_targetBuilding, this.Target);
                }

                // Set internal target.
                this.Target = 0;

                // Unassign the vehicle.
                return VehicleHelper.DeAssign(this.VehicleId, ref vehicle);
            }

            return vehicle.m_targetBuilding == 0 && this.Target == 0;
        }

        /// <summary>
        /// Sets the target and updates the games vehicle object.
        /// </summary>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>True if target set vehicle found path the target.</returns>
        public bool SetTarget(ushort targetBuildingId, ref Vehicle vehicle)
        {
            if (targetBuildingId == 0)
            {
                if (this.Target == 0 && vehicle.m_targetBuilding == 0)
                {
                    return true;
                }

                return this.DeAssign(ref vehicle, true, "SetTarget");
            }

            if (Log.LogALot)
            {
                Log.DevDebug(this, "SetTarget", this.VehicleId, targetBuildingId, this.Target, vehicle.m_targetBuilding, vehicle.m_flags);
            }

            if (VehicleHelper.SetTarget(this.VehicleId, ref vehicle, targetBuildingId))
            {
                this.LastAssigned = Global.CurrentFrame;
                this.lastTargetStamp = Global.SimulationTime;
                this.FreeToCollect = false;
                this.Target = targetBuildingId;

                return true;
            }

            Log.Debug(this, "SetTarget", "Failed", this.VehicleId, targetBuildingId, this.Target, vehicle.m_targetBuilding, vehicle.m_flags);

            this.LastAssigned = 0;
            this.lastTargetStamp = 0.0;
            this.FreeToCollect = false;
            this.Target = 0;

            return false;
        }

        /// <summary>
        /// Updates the specified vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="freeToCollect">If set to <c>true</c> the vehicle is free.</param>
        /// <param name="checkAssignment">If set to <c>true</c> check vehicles assignment and possibly de-assign vehicle].</param>
        public void Update(ref Vehicle vehicle, bool freeToCollect, bool checkAssignment)
        {
            if (this.LastSeen != Global.CurrentFrame)
            {
                if (!checkAssignment)
                {
                    string localeKey;
                    int bufCur, bufMax;
                    vehicle.Info.m_vehicleAI.GetBufferStatus(this.VehicleId, ref vehicle, out localeKey, out bufCur, out bufMax);
                    this.CapacityFree = bufMax - bufCur;
                    this.CapacityUsed = (float)bufCur / (float)bufMax;
                }
                else if ((vehicle.m_flags & (VehicleHelper.VehicleUnavailable | VehicleHelper.VehicleBusy)) == Vehicle.Flags.None)
                {
                    if (this.Target == 0 && Global.SimulationTime - this.lastTargetStamp > Global.RecallUnusedDelaySeconds &&
                        (vehicle.m_targetBuilding != 0 || (vehicle.m_flags & Vehicle.Flags.GoingBack) == Vehicle.Flags.None))
                    {
                        if (this.lastTargetStamp != 0.0)
                        {
                            if (Log.LogALot)
                            {
                                Log.DevDebug(this, "Update", this.dispatcherType, "CheckAssignment", "Recall", this.VehicleId, vehicle.m_targetBuilding, vehicle.m_flags);
                            }

                            VehicleHelper.DeAssign(this.VehicleId, ref vehicle, true);
                        }

                        this.lastTargetStamp = Global.SimulationTime;
                    }
                    else if (vehicle.m_targetBuilding != 0 && vehicle.m_targetBuilding != this.Target && Global.CurrentFrame - this.LastAssigned > Global.DemandLingerDelay)
                    {
                        if (Log.LogALot)
                        {
                            Log.DevDebug(this, "Update", this.dispatcherType, "CheckAssignment", "DeAssign", this.VehicleId, vehicle.m_targetBuilding, vehicle.m_flags);
                        }

                        this.DeAssign(ref vehicle, false, "Update");
                    }
                }
            }

            this.Position = vehicle.GetLastFramePosition();
            this.LastSeen = Global.CurrentFrame;
            this.FreeToCollect = freeToCollect && ((vehicle.m_flags & Vehicle.Flags.GoingBack) == Vehicle.Flags.None);

            if (vehicle.m_targetBuilding != this.Target)
            {
                this.Target = 0;
            }
            else if (this.Target != 0)
            {
                this.LastAssigned = Global.CurrentFrame;
                this.lastTargetStamp = Global.SimulationTime;
            }
        }
    }
}