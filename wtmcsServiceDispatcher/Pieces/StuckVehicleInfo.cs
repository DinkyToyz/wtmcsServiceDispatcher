using System;
using ColossalFramework;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Stuck vehicle information holder.
    /// </summary>
    internal class StuckVehicleInfo
    {
        /// <summary>
        /// The vehicle is broken.
        /// </summary>
        private bool isBroken = false;

        /// <summary>
        /// The vehicle is stuck.
        /// </summary>
        private bool isStuck = false;

        /// <summary>
        /// The vehicle's last position.
        /// </summary>
        private Vector3 lastPosition = Vector3.zero;

        /// <summary>
        /// The target building identifier.
        /// </summary>
        private ushort targetBuildingId = 0;

        /// <summary>
        /// The frame since when the vehicle has been waiting for a path.
        /// </summary>
        private uint waitingForPathSinceFrame = 0u;

        /// <summary>
        /// The simulation time stamp since when the vehicle has been waiting for a path.
        /// </summary>
        private double waitingForPathSinceTime = 0.0;

        /// <summary>
        /// The vehicle identifier.
        /// </summary>
        private ushort vehicleId = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="StuckVehicleInfo"/> class.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        public StuckVehicleInfo(ushort vehicleId, ref Vehicle vehicle)
        {
            this.vehicleId = vehicleId;
            this.Update(ref vehicle);
        }

        /// <summary>
        /// Determines whether the specified vehicle has a problem.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>True if the vehicle has at least one problem.</returns>
        public static bool HasProblem(ref Vehicle vehicle)
        {
            // Don't check unspawned vehicles.
            if (vehicle.Info == null || (vehicle.m_flags & Vehicle.Flags.Spawned) == Vehicle.Flags.None)
            {
                return false;
            }

            // Only check vehicles we dispatch unless told to check other vehicles as well.
            if (!(Global.Settings.RemoveStuckVehicles ||
                  (Global.HearseDispatcher != null && vehicle.Info.m_vehicleAI is HearseAI) ||
                  (Global.GarbageTruckDispatcher != null && vehicle.Info.m_vehicleAI is GarbageTruckAI)))
            {
                return false;
            }

            // Check vehicles waiting for path.
            if ((vehicle.m_flags & Vehicle.Flags.WaitingPath) != Vehicle.Flags.None)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handles the vehicles problems.
        /// </summary>
        /// <returns>True if a problem was handled.</returns>
        public bool HandleProblem()
        {
            if (this.isStuck || this.isBroken)
            {
                try
                {
                    // Remove vehicle.
                    Log.Debug(this, "HandleProblem", "DeSpawn", this.vehicleId, VehicleHelper.GetVehicleName(this.vehicleId));
                    Log.FlushBuffer();

                    Singleton<VehicleManager>.instance.m_vehicles.m_buffer[this.vehicleId].Unspawn(this.vehicleId);

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(this, "HandleProblem", ex, this.vehicleId);
                    Log.FlushBuffer();
                }
                finally
                {
                    this.isStuck = false;
                    this.isBroken = false;
                    this.waitingForPathSinceTime = 0.0;
                    this.waitingForPathSinceFrame = 0u;
                }
            }

            return false;
        }

        /// <summary>
        /// Updates the specified vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        public void Update(ref Vehicle vehicle)
        {
            Vector3 position = Vector3.zero;

            // Check if vehicle is waiting for path.
            if ((vehicle.m_flags & Vehicle.Flags.WaitingPath) == Vehicle.Flags.None)
            {
                this.waitingForPathSinceTime = 0;
                this.waitingForPathSinceFrame = 0;
            }
            else
            {
                position = vehicle.GetLastFramePosition();

                if (Log.LogALot)
                {
                    Log.DevDebug(this, "Update", "WaitingPath", this.vehicleId, this.waitingForPathSinceTime, Global.SimulationTime, Global.SimulationTime - this.waitingForPathSinceTime, this.lastPosition, position, vehicle.m_targetBuilding, vehicle.m_flags, VehicleHelper.GetVehicleName(this.vehicleId));
                }

                // Remember first time stamp the vehicle was seen waiting at this position.
                if (this.waitingForPathSinceFrame == 0 || this.waitingForPathSinceTime == 0 || position != this.lastPosition)
                {
                    this.waitingForPathSinceTime = Global.SimulationTime;
                    this.waitingForPathSinceFrame = Global.CurrentFrame;
                }
            }

            this.lastPosition = position;
            this.targetBuildingId = vehicle.m_targetBuilding;

            if (!this.isStuck)
            {
                // Check if stuck waiting for path.
                if (this.waitingForPathSinceTime > 0.0)
                {
                    double delta = Global.SimulationTime - this.waitingForPathSinceTime;

                    if (delta > Global.Settings.RemoveStuckVehiclesDelaySeconds && Global.CurrentFrame - this.waitingForPathSinceFrame > Global.WaitPathStuckDelay)
                    {
                        Log.Info(this, "IsStuck", "WaitingForPath", this.vehicleId, delta, VehicleHelper.GetVehicleName(this.vehicleId));

                        this.isStuck = true;
                    }
                }
            }
        }

        /// <summary>
        /// Check if ambulance is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if ambulance is confused.</returns>
        private bool ConfusedAmbulance(ref Vehicle data)
        {
            // From AmbulanceAI.GetLocalizedStatus from original game code at version 1.4.0-f3.
            // Straight copy from game code even though that's slower than a simple condition check
            // to make it easier to see that the same logic is used.
            if ((data.m_flags & Vehicle.Flags.GoingBack) != Vehicle.Flags.None)
            {
                if ((int)data.m_transferSize == 0)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_AMBULANCE_RETURN_EMPTY");
                    return false;
                }
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_AMBULANCE_RETURN_FULL");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.WaitingTarget) != Vehicle.Flags.None)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_AMBULANCE_WAIT");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.Emergency2) != Vehicle.Flags.None && (int)data.m_targetBuilding != 0)
            {
                ////target = InstanceID.Empty;
                ////target.Building = data.m_targetBuilding;
                ////return Locale.Get("VEHICLE_STATUS_AMBULANCE_EMERGENCY");
                return false;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }

        /// <summary>
        /// Check if hearse is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if hearse is confused.</returns>
        private bool ConfusedHearse(ref Vehicle data)
        {
            // From HearseAI.GetLocalizedStatus from original game code at version 1.4.0-f3.
            // Straight copy from game code even though that's slower than a simple condition check
            // to make it easier to see that the same logic is used.
            if ((data.m_flags & Vehicle.Flags.TransferToSource) != Vehicle.Flags.None)
            {
                if ((data.m_flags & (Vehicle.Flags.Stopped | Vehicle.Flags.WaitingTarget)) != Vehicle.Flags.None)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_HEARSE_WAIT");
                    return false;
                }
                if ((data.m_flags & Vehicle.Flags.GoingBack) != Vehicle.Flags.None)
                {
                    //target = InstanceID.Empty;
                    //return Locale.Get("VEHICLE_STATUS_HEARSE_RETURN");
                    return false;
                }
                if ((int)data.m_targetBuilding != 0)
                {
                    //target = InstanceID.Empty;
                    //target.Building = data.m_targetBuilding;
                    //return Locale.Get("VEHICLE_STATUS_HEARSE_COLLECT");
                    return false;
                }
            }
            else if ((data.m_flags & Vehicle.Flags.TransferToTarget) != Vehicle.Flags.None)
            {
                if ((data.m_flags & Vehicle.Flags.GoingBack) != Vehicle.Flags.None)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_HEARSE_RETURN");
                    return false;
                }
                if ((data.m_flags & Vehicle.Flags.WaitingTarget) != Vehicle.Flags.None)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_HEARSE_UNLOAD");
                    return false;
                }
                if ((int)data.m_targetBuilding != 0)
                {
                    ////target = InstanceID.Empty;
                    ////target.Building = data.m_targetBuilding;
                    ////return Locale.Get("VEHICLE_STATUS_HEARSE_TRANSFER");
                    return false;
                }
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }

        /// <summary>
        /// Check if garbage truck is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if garbage truck is confused.</returns>
        private bool ConfusedGarbageTruck(ref Vehicle data)
        {
            // From GarbageTruckAI.GetLocalizedStatus from original game code at version 1.4.0-f3.
            // Straight copy from game code even though that's slower than a simple condition check
            // to make it easier to see that the same logic is used.
            if ((data.m_flags & Vehicle.Flags.TransferToSource) != Vehicle.Flags.None)
            {
                if ((data.m_flags & Vehicle.Flags.GoingBack) != Vehicle.Flags.None)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_GARBAGE_RETURN");
                    return false;
                }
                if ((data.m_flags & Vehicle.Flags.WaitingTarget) != Vehicle.Flags.None)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_GARBAGE_WAIT");
                    return false;
                }
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_GARBAGE_COLLECT");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.TransferToTarget) != Vehicle.Flags.None)
            {
                if ((data.m_flags & Vehicle.Flags.GoingBack) != Vehicle.Flags.None)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_GARBAGE_RETURN");
                    return false;
                }
                if ((data.m_flags & Vehicle.Flags.WaitingTarget) != Vehicle.Flags.None)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_GARBAGE_UNLOAD");
                    return false;
                }
                if ((int)data.m_targetBuilding != 0)
                {
                    ////target = InstanceID.Empty;
                    ////target.Building = data.m_targetBuilding;
                    ////return Locale.Get("VEHICLE_STATUS_GARBAGE_TRANSFER");
                    return false;
                }
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }

        /// <summary>
        /// Log debug info.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        private void DebugLog(ushort vehicleId)
        {
            Vehicle vehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[this.vehicleId];

            Log.InfoList info = new Log.InfoList();

            info.Add("m_leadingVehicle", vehicle.m_leadingVehicle);
            info.Add("m_trailingVehicle", vehicle.m_trailingVehicle);
            info.Add("Spawned", vehicle.m_flags & Vehicle.Flags.Spawned);
            info.Add("Flags", vehicle.m_flags);
            info.Add("Info", vehicle.Info);

            if (vehicle.Info != null)
            {
                info.Add("Info.m_isLargeVehicle", vehicle.Info.m_isLargeVehicle);
            }

            Log.DevDebug(this, "DebugLog", vehicleId, info);
        }
    }
}