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
        /// The flags to check.
        /// </summary>
        public const Vehicle.Flags FlagsToCheck = Vehicle.Flags.WaitingPath | Vehicle.Flags.Parking;

        /// <summary>
        /// The vehicle's last position.
        /// </summary>
        private Vector3 checkFlagPosition = Vector3.zero;

        /// <summary>
        /// The flags that determined that this vehicle should be checked.
        /// </summary>
        private Vehicle.Flags checkFlags = Vehicle.Flags.None;

        /// <summary>
        /// The frame since when the vehicle has been had a flag that should be checked.
        /// Used for deciding when to consider a vehicle as stuck.
        /// </summary>
        private uint checkFlagSinceFrame = 0u;

        /// <summary>
        /// The simulation time stamp since when the vehicle has had a flag that should be checked.
        /// Used for deciding when to de-spawn vehicle.
        /// </summary>
        private double checkFlagSinceTime = 0.0;

        /// <summary>
        /// The vehicle has been confused since this frame.
        /// Used for deciding when to recall vehicle.
        /// </summary>
        private uint confusedSinceFrame = 0u;

        /// <summary>
        /// The vehicle has been confused since this time stamp.
        /// Used for deciding when to de-spawn vehicle.
        /// </summary>
        private double confusedSinceTime = 0.0;

        /// <summary>
        /// The dispatcher type.
        /// </summary>
        private Dispatcher.DispatcherTypes dispatcherType = Dispatcher.DispatcherTypes.None;

        /// <summary>
        /// The vehicle is broken.
        /// </summary>
        private bool isBroken = false;

        /// <summary>
        /// The vehicle is stuck.
        /// </summary>
        private bool isStuck = false;

        /// <summary>
        /// The target building identifier.
        /// </summary>
        private ushort targetBuildingId = 0;

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

            this.dispatcherType = Dispatcher.GetDispatcherType(ref vehicle);
        }

        /// <summary>
        /// Gets a value indicating whether the vehicle is the dispatcher's responsibility.
        /// </summary>
        /// <value>
        /// <c>true</c> if the vehicle is the dispatcher's responsibility; otherwise, <c>false</c>.
        /// </value>
        public bool DispatchersResponsibility
        {
            get
            {
                return Global.Settings.RemoveStuckVehicles ||
                       (Global.HearseDispatcher != null && this.dispatcherType == Dispatcher.DispatcherTypes.HearseDispatcher) ||
                       (Global.GarbageTruckDispatcher != null && this.dispatcherType == Dispatcher.DispatcherTypes.GarbageTruckDispatcher);
            }
        }

        /// <summary>
        /// Gets the amount of frames during which the vehicle has had a check flag.
        /// </summary>
        /// <value>
        /// The check flag frame count.
        /// </value>
        private uint CheckFlaggedForFrames
        {
            get
            {
                return (this.checkFlagSinceFrame > 0 && this.checkFlagSinceFrame < Global.CurrentFrame) ? Global.CurrentFrame - this.checkFlagSinceFrame : 0;
            }
        }

        /// <summary>
        /// Gets the duration in seconds during which the vehicle has had a check flag.
        /// </summary>
        /// <value>
        /// The check flag duration.
        /// </value>
        private double CheckFlaggedForSeconds
        {
            get
            {
                return (this.checkFlagSinceTime > 0 && this.checkFlagSinceTime < Global.SimulationTime) ? Global.SimulationTime - this.checkFlagSinceTime : 0;
            }
        }

        /// <summary>
        /// Gets the amount of frames during which the vehicle has been confused.
        /// </summary>
        /// <value>
        /// The check confusion frame count.
        /// </value>
        private uint ConfusedForFrames
        {
            get
            {
                return (this.confusedSinceFrame > 0 && this.confusedSinceTime < Global.CurrentFrame) ? Global.CurrentFrame - this.confusedSinceFrame : 0;
            }
        }

        /// <summary>
        /// Gets the duration in seconds during which the vehicle has been confused.
        /// </summary>
        /// <value>
        /// The confusion duration.
        /// </value>
        private double ConfusedForSeconds
        {
            get
            {
                return (this.confusedSinceTime > 0 && this.confusedSinceTime < Global.SimulationTime) ? Global.SimulationTime - this.confusedSinceTime : 0;
            }
        }

        /// <summary>
        /// Determines whether the specified vehicle has a problem.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>
        /// True if the vehicle has at least one problem.
        /// </returns>
        public static bool HasProblem(ushort vehicleId, ref Vehicle vehicle)
        {
            // Don't check unspawned vehicles.
            if (vehicle.Info == null || (vehicle.m_flags & Vehicle.Flags.Spawned) == Vehicle.Flags.None)
            {
                return false;
            }

            // Trailer?
            if (vehicle.m_leadingVehicle != 0)
            {
                // Todo: Check lost trailers?
                return false;
            }

            // Cargo parent?
            if (vehicle.m_cargoParent != 0)
            {
                // Todo: investigate how cargo parent works, so the can be checked as well.
                return false;
            }

            // Only check vehicles we dispatch unless told to check other vehicles as well.
            if (!(Global.Settings.RemoveStuckVehicles ||
                  (Global.HearseDispatcher != null && vehicle.Info.m_vehicleAI is HearseAI) ||
                  (Global.GarbageTruckDispatcher != null && vehicle.Info.m_vehicleAI is GarbageTruckAI)))
            {
                return false;
            }

            // Check vehicles flags.
            if ((vehicle.m_flags & FlagsToCheck) != Vehicle.Flags.None)
            {
                return true;
            }

            if (ConfusionHelper.VehicleIsConfused(vehicleId, ref vehicle))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds debug information data to information list.
        /// </summary>
        /// <param name="info">The information list.</param>
        public void AddDebugInfoData(Log.InfoList info)
        {
            this.AddDebugInfoData(info, false);
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
                    Log.Debug(this, "HandleProblem", "StuckOrBroken", "DeSpawn", this.vehicleId, VehicleHelper.GetVehicleName(this.vehicleId));
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
                    this.checkFlagSinceTime = 0.0;
                    this.checkFlagSinceFrame = 0u;
                }
            }

            if (this.ConfusedForFrames > Global.RecallConfusedDelay &&
                ((Global.HearseDispatcher != null && this.dispatcherType == Dispatcher.DispatcherTypes.HearseDispatcher) ||
                 (Global.GarbageTruckDispatcher != null && this.dispatcherType == Dispatcher.DispatcherTypes.GarbageTruckDispatcher)))
            {
                try
                {
                    // Recall vehicle.
                    Log.Debug(this, "HandleProblem", "Confused", "Recall", this.vehicleId, VehicleHelper.GetVehicleName(this.vehicleId));
                    Log.FlushBuffer();

                    VehicleHelper.RecallVehicle(this.vehicleId);

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(this, "HandleProblem", ex, this.vehicleId);
                    Log.FlushBuffer();
                }
                finally
                {
                    this.confusedSinceFrame = Global.CurrentFrame;
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
            // Check if vehicle has flag that should be checked.
            Vehicle.Flags flags = vehicle.m_flags & FlagsToCheck;
            if (flags == Vehicle.Flags.None)
            {
                if (Log.LogALot /*&& (this.checkFlags != Vehicle.Flags.None || this.checkFlagSinceFrame > 0 || this.checkFlagSinceTime > 0)*/)
                {
                    Log.DevDebug(this, "Update", "ResetCheckFlag", flags, this.vehicleId, this.CheckFlaggedForSeconds, this.CheckFlaggedForFrames, Global.Settings.RemoveStuckVehiclesDelaySeconds, Global.CheckFlagStuckDelay, this.checkFlags, flags, vehicle.m_targetBuilding, vehicle.m_flags, VehicleHelper.GetVehicleName(this.vehicleId), this.GetHashCode().ToString());
                }

                this.checkFlagSinceTime = 0;
                this.checkFlagSinceFrame = 0;
            }
            else
            {
                Vector3 position = vehicle.GetLastFramePosition();

                // Remember first time stamp the vehicle was seen with this flag at this position.
                if (this.checkFlagSinceFrame == 0 || this.checkFlagSinceTime == 0 || flags != this.checkFlags || (position - checkFlagPosition).sqrMagnitude > 0)
                {
                    if (Log.LogALot)
                    {
                        Log.DevDebug(this, "Update", "NewCheckFlag", flags, this.vehicleId, this.CheckFlaggedForSeconds, this.CheckFlaggedForFrames, Global.Settings.RemoveStuckVehiclesDelaySeconds, Global.CheckFlagStuckDelay, this.checkFlags, flags, this.checkFlagPosition, position, vehicle.m_targetBuilding, vehicle.m_flags, VehicleHelper.GetVehicleName(this.vehicleId), this.GetHashCode().ToString());
                    }

                    this.checkFlagPosition = position;
                    this.checkFlagSinceTime = Global.SimulationTime;
                    this.checkFlagSinceFrame = Global.CurrentFrame;
                }
                else if (Log.LogALot)
                {
                    Log.DevDebug(this, "Update", "CheckFlag", flags, this.vehicleId, this.CheckFlaggedForSeconds, this.CheckFlaggedForFrames, Global.Settings.RemoveStuckVehiclesDelaySeconds, Global.CheckFlagStuckDelay, this.checkFlags, flags, this.checkFlagPosition, position, vehicle.m_targetBuilding, vehicle.m_flags, VehicleHelper.GetVehicleName(this.vehicleId), this.GetHashCode().ToString());
                }
            }

            if (!ConfusionHelper.VehicleIsConfused(this.vehicleId, ref vehicle))
            {
                if (Log.LogALot /*&& (this.confusedSinceFrame > 0 || this.confusedSinceTime > 0)*/)
                {
                    Log.DevDebug(this, "Update", "ResetConfused", this.vehicleId, this.ConfusedForSeconds, this.ConfusedForFrames, Global.Settings.RemoveStuckVehiclesDelaySeconds, Global.RecallConfusedDelay, vehicle.m_targetBuilding, vehicle.m_flags, VehicleHelper.GetVehicleName(this.vehicleId));
                }

                this.confusedSinceTime = 0;
                this.confusedSinceFrame = 0;
            }
            else
            {
                if (this.confusedSinceFrame == 0 || this.confusedSinceTime == 0)
                {
                    if (Log.LogALot)
                    {
                        Log.DevDebug(this, "Update", "NewConfused", this.vehicleId, this.ConfusedForSeconds, this.ConfusedForFrames, Global.Settings.RemoveStuckVehiclesDelaySeconds, Global.RecallConfusedDelay, vehicle.m_targetBuilding, vehicle.m_flags, VehicleHelper.GetVehicleName(this.vehicleId));
                    }

                    this.confusedSinceTime = Global.SimulationTime;
                    this.confusedSinceFrame = Global.CurrentFrame;
                }
                else if (Log.LogALot)
                {
                    Log.DevDebug(this, "Update", "Confused", this.vehicleId, this.ConfusedForSeconds, this.ConfusedForFrames, Global.Settings.RemoveStuckVehiclesDelaySeconds, Global.RecallConfusedDelay, vehicle.m_targetBuilding, vehicle.m_flags, VehicleHelper.GetVehicleName(this.vehicleId));
                }
            }

            this.checkFlags = flags;
            this.targetBuildingId = vehicle.m_targetBuilding;

            if (!this.isStuck)
            {
                double delta;

                // Check if stuck with flag.
                if (this.checkFlags != Vehicle.Flags.None && this.CheckFlaggedForFrames > Global.CheckFlagStuckDelay)
                {
                    delta = this.CheckFlaggedForSeconds;

                    if (delta > Global.Settings.RemoveStuckVehiclesDelaySeconds)
                    {
                        Log.Info(this, "IsStuck", this.checkFlags, this.vehicleId, delta, VehicleHelper.GetVehicleName(this.vehicleId));

                        this.isStuck = true;
                    }
                }

                // Check if stuck confused.
                delta = this.ConfusedForSeconds;

                if (delta > Global.Settings.RemoveStuckVehiclesDelaySeconds)
                {
                    Log.Info(this, "IsStuck", "Confused", this.vehicleId, delta, VehicleHelper.GetVehicleName(this.vehicleId));

                    this.isStuck = true;
                }
            }
        }

        /// <summary>
        /// Adds debug information data to information list.
        /// </summary>
        /// <param name="info">The information list.</param>
        /// <param name="complete">If set to <c>true</c> add complete information.</param>
        private void AddDebugInfoData(Log.InfoList info, bool complete)
        {
            if (complete)
            {
                info.Add("VehicleId", this.vehicleId);

                Vehicle vehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[this.vehicleId];

                info.Add("LeadingVehicle", vehicle.m_leadingVehicle);
                info.Add("TrailingVehicle", vehicle.m_trailingVehicle);
                info.Add("Spawned", vehicle.m_flags & Vehicle.Flags.Spawned);
                info.Add("Flags", vehicle.m_flags);
                info.Add("Info", vehicle.Info);

                if (vehicle.Info != null)
                {
                    info.Add("IsLargeVehicle", vehicle.Info.m_isLargeVehicle);
                }
            }

            if (this.isStuck || this.isBroken)
            {
                info.Add("Problem", this.isStuck ? "Stuck" : null, this.isBroken ? "Broken" : null);
            }

            if (this.confusedSinceFrame > 0 || this.confusedSinceTime > 0)
            {
                info.Add("Confused", this.ConfusedForSeconds, this.ConfusedForFrames);
            }

            if (this.checkFlagSinceFrame > 0 || this.checkFlagSinceTime > 0)
            {
                info.Add("Flagged", this.checkFlags, this.CheckFlaggedForSeconds, this.CheckFlaggedForFrames);
            }
        }

        /// <summary>
        /// Log debug info.
        /// </summary>
        private void DebugLog()
        {
            Vehicle vehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[this.vehicleId];

            Log.InfoList info = new Log.InfoList();
            this.AddDebugInfoData(info, true);

            Log.DevDebug(this, "DebugLog", this.vehicleId, info);
        }
    }
}