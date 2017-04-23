using ColossalFramework;
using System;
using System.Collections.Generic;
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
        private Vehicle.Flags checkFlags = ~VehicleHelper.VehicleAll;

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
        /// The vehicle has been confused/de-assigned since this frame.
        /// Used for deciding when to de-assign vehicle.
        /// </summary>
        private uint confusedDeAssignedSinceFrame = 0u;

        /// <summary>
        /// The vehicle has been confused since this frame.
        /// Used for deciding when to de-assign vehicle.
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
        /// The last de-assign time stamp.
        /// </summary>
        private uint lastDeAssignStamp = 0u;

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
            this.dispatcherType = Dispatcher.GetDispatcherType(ref vehicle);

            this.Update(ref vehicle);
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
                return Global.Settings.RecoveryCrews.DispatchVehicles ||
                       (Global.Settings.DeathCare.DispatchVehicles && Global.HearseDispatcher != null && this.dispatcherType == Dispatcher.DispatcherTypes.HearseDispatcher) ||
                       (Global.Settings.Garbage.DispatchVehicles && Global.GarbageTruckDispatcher != null && this.dispatcherType == Dispatcher.DispatcherTypes.GarbageTruckDispatcher) ||
                       (Global.Settings.HealthCare.DispatchVehicles && Global.AmbulanceDispatcher != null && this.dispatcherType == Dispatcher.DispatcherTypes.AmbulanceDispatcher);
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
        /// Gets the amount of frames during which the vehicle has been confused/de-assigned.
        /// </summary>
        /// <value>
        /// The de-assign check confusion frame count.
        /// </value>
        private uint ConfusedDeAssignedForFrames
        {
            get
            {
                return (this.confusedDeAssignedSinceFrame > 0 && this.confusedDeAssignedSinceFrame < Global.CurrentFrame) ? Global.CurrentFrame - this.confusedDeAssignedSinceFrame : 0;
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
                return (this.confusedSinceFrame > 0 && this.confusedSinceFrame < Global.CurrentFrame) ? Global.CurrentFrame - this.confusedSinceFrame : 0;
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
            if (vehicle.Info == null || (vehicle.m_flags & Vehicle.Flags.Spawned) == ~VehicleHelper.VehicleAll)
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
            if (!(Global.Settings.RecoveryCrews.DispatchVehicles ||
                  (Global.Settings.DeathCare.DispatchVehicles && Global.HearseDispatcher != null && vehicle.Info.m_vehicleAI is HearseAI) ||
                  (Global.Settings.Garbage.DispatchVehicles && Global.GarbageTruckDispatcher != null && vehicle.Info.m_vehicleAI is GarbageTruckAI) ||
                  (Global.Settings.HealthCare.DispatchVehicles && Global.AmbulanceDispatcher != null && vehicle.Info.m_vehicleAI is AmbulanceAI)))
            {
                return false;
            }

            // Check vehicles flags.
            if ((vehicle.m_flags & FlagsToCheck) != ~VehicleHelper.VehicleAll)
            {
                return true;
            }

            if (ConfusionHelper.VehicleIsConfused(ref vehicle))
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
        /// Log debug info.
        /// </summary>
        public void DebugLog()
        {
            Log.InfoList info = new Log.InfoList();
            this.AddDebugInfoData(info, true);

            Log.DevDebug(this, "DebugLog", this.vehicleId, info);
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

            if (this.ConfusedDeAssignedForFrames > Global.DeAssignConfusedDelay &&
                ((Global.Settings.DeathCare.DispatchVehicles && Global.HearseDispatcher != null && this.dispatcherType == Dispatcher.DispatcherTypes.HearseDispatcher) ||
                 (Global.Settings.Garbage.DispatchVehicles && Global.GarbageTruckDispatcher != null && this.dispatcherType == Dispatcher.DispatcherTypes.GarbageTruckDispatcher) ||
                 (Global.Settings.HealthCare.DispatchVehicles && Global.AmbulanceDispatcher != null && this.dispatcherType == Dispatcher.DispatcherTypes.HearseDispatcher)))
            {
                try
                {
                    // De-assign vehicle.
                    Log.Debug(this, "HandleProblem", "Confused", "DeAssign", this.vehicleId, VehicleHelper.GetVehicleName(this.vehicleId));
                    Log.FlushBuffer();

                    this.DeAssign();
                }
                catch (Exception ex)
                {
                    Log.Error(this, "HandleProblem", ex, this.vehicleId);
                    Log.FlushBuffer();
                }
                finally
                {
                    this.confusedDeAssignedSinceFrame = Global.CurrentFrame;
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
            this.targetBuildingId = vehicle.m_targetBuilding;

            // Check if vehicle has flag that should be checked.
            Vehicle.Flags flags = vehicle.m_flags & FlagsToCheck;
            if ((flags & VehicleHelper.VehicleAll) != ~VehicleHelper.VehicleAll)
            {
                Vector3 position = vehicle.GetLastFramePosition();

                // Remember first time stamp the vehicle was seen with this flag at this position.
                if (this.checkFlagSinceFrame == 0 || this.checkFlagSinceTime == 0 || flags != this.checkFlags || Math.Truncate((position - this.checkFlagPosition).sqrMagnitude) > 0)
                {
                    ////if (Log.LogALot)
                    ////{
                    ////    if (this.checkFlags == Vehicle.Flags.None)
                    ////    {
                    ////        Log.DevDebug(this, "Update", "NewCheckFlag", flags, this.vehicleId, this.CheckFlaggedForSeconds, this.CheckFlaggedForFrames, Global.Settings.RemoveStuckVehiclesDelaySeconds, Global.CheckFlagStuckDelay, this.checkFlags, flags, this.checkFlagPosition, position, '-', vehicle.m_targetBuilding, vehicle.m_flags, VehicleHelper.GetVehicleName(this.vehicleId), this.GetHashCode().ToString());
                    ////    }
                    ////    else
                    ////    {
                    ////        Log.DevDebug(this, "Update", "NewCheckFlag", flags, this.vehicleId, this.CheckFlaggedForSeconds, this.CheckFlaggedForFrames, Global.Settings.RemoveStuckVehiclesDelaySeconds, Global.CheckFlagStuckDelay, this.checkFlags, flags, this.checkFlagPosition, position, (position - this.checkFlagPosition).sqrMagnitude, vehicle.m_targetBuilding, vehicle.m_flags, VehicleHelper.GetVehicleName(this.vehicleId), this.GetHashCode().ToString());
                    ////    }
                    ////}

                    this.checkFlags = flags;
                    this.checkFlagPosition = position;
                    this.checkFlagSinceTime = Global.SimulationTime;
                    this.checkFlagSinceFrame = Global.CurrentFrame;
                }
                ////else if (Log.LogALot)
                ////{
                ////    Log.DevDebug(this, "Update", "CheckFlag", flags, this.vehicleId, this.CheckFlaggedForSeconds, this.CheckFlaggedForFrames, Global.Settings.RemoveStuckVehiclesDelaySeconds, Global.CheckFlagStuckDelay, this.checkFlags, flags, this.checkFlagPosition, position, (position - this.checkFlagPosition).sqrMagnitude, vehicle.m_targetBuilding, vehicle.m_flags, VehicleHelper.GetVehicleName(this.vehicleId), this.GetHashCode().ToString());
                ////}
            }
            else if ((this.checkFlags & VehicleHelper.VehicleAll) != ~VehicleHelper.VehicleAll || this.checkFlagSinceTime != 0 || this.checkFlagSinceFrame != 0)
            {
                ////if (Log.LogALot)
                ////{
                ////    Log.DevDebug(this, "Update", "ResetCheckFlag", flags, this.vehicleId, this.CheckFlaggedForSeconds, this.CheckFlaggedForFrames, Global.Settings.RemoveStuckVehiclesDelaySeconds, Global.CheckFlagStuckDelay, this.checkFlags, flags, vehicle.m_targetBuilding, vehicle.m_flags, VehicleHelper.GetVehicleName(this.vehicleId), this.GetHashCode().ToString());
                ////}

                this.checkFlags = ~VehicleHelper.VehicleAll;
                this.checkFlagSinceTime = 0;
                this.checkFlagSinceFrame = 0;
            }

            // Check if vehicle is confused.
            if (ConfusionHelper.VehicleIsConfused(ref vehicle))
            {
                if (this.confusedDeAssignedSinceFrame == 0)
                {
                    this.confusedDeAssignedSinceFrame = Global.CurrentFrame;
                }

                if (this.confusedSinceFrame == 0 || this.confusedSinceTime == 0)
                {
                    if (Log.LogALot)
                    {
                        Log.DevDebug(this, "Update", "NewConfused", this.vehicleId, this.ConfusedForSeconds, this.ConfusedForFrames, Global.Settings.RecoveryCrews.DelaySeconds, Global.DeAssignConfusedDelay, vehicle.m_targetBuilding, vehicle.m_flags, VehicleHelper.GetVehicleName(this.vehicleId));
                    }

                    this.confusedSinceTime = Global.SimulationTime;
                    this.confusedSinceFrame = Global.CurrentFrame;
                    this.confusedDeAssignedSinceFrame = Global.CurrentFrame;
                }
                ////else if (Log.LogALot)
                ////{
                ////    Log.DevDebug(this, "Update", "Confused", this.vehicleId, this.ConfusedForSeconds, this.ConfusedForFrames, Global.Settings.RemoveStuckVehiclesDelaySeconds, Global.DeAssignConfusedDelay, vehicle.m_targetBuilding, vehicle.m_flags, VehicleHelper.GetVehicleName(this.vehicleId));
                ////}
            }
            else if (this.confusedSinceTime != 0 || this.confusedSinceFrame != 0 || this.confusedDeAssignedSinceFrame != 0)
            {
                ////if (Log.LogALot)
                ////{
                ////    Log.DevDebug(this, "Update", "ResetConfused", this.vehicleId, this.ConfusedForSeconds, this.ConfusedForFrames, Global.Settings.RemoveStuckVehiclesDelaySeconds, Global.DeAssignConfusedDelay, vehicle.m_targetBuilding, vehicle.m_flags, VehicleHelper.GetVehicleName(this.vehicleId));
                ////}

                this.confusedSinceTime = 0;
                this.confusedSinceFrame = 0;
                this.confusedDeAssignedSinceFrame = 0;
            }

            // Check if vehicle is stuck.
            if (!this.isStuck)
            {
                double delta;

                // Check if stuck with flag.
                if ((this.checkFlags & VehicleHelper.VehicleAll) != ~VehicleHelper.VehicleAll && this.CheckFlaggedForFrames > Global.CheckFlagStuckDelay)
                {
                    delta = this.CheckFlaggedForSeconds;

                    if (delta > Global.Settings.RecoveryCrews.DelaySeconds)
                    {
                        Log.Info(this, "IsStuck", this.checkFlags, this.vehicleId, delta, VehicleHelper.GetVehicleName(this.vehicleId));

                        this.isStuck = true;
                    }
                }

                // Check if stuck confused.
                if (this.ConfusedForFrames > Global.CheckFlagStuckDelay)
                {
                    delta = this.ConfusedForSeconds;

                    if (delta > Global.Settings.RecoveryCrews.DelaySeconds)
                    {
                        Log.Info(this, "IsStuck", "Confused", this.vehicleId, delta, VehicleHelper.GetVehicleName(this.vehicleId));

                        this.isStuck = true;
                    }
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

            info.Add("Dispatcher", this.dispatcherType);
            info.Add("Responsible", this.DispatchersResponsibility);
        }

        /// <summary>
        /// De-assign the vehicle.
        /// </summary>
        /// <param name="serviceBuildings">The service buildings.</param>
        /// <param name="targetBuildings">The target buildings.</param>
        private void DeAssign(Dictionary<ushort, ServiceBuildingInfo> serviceBuildings, Dictionary<ushort, TargetBuildingInfo> targetBuildings)
        {
            if (serviceBuildings != null)
            {
                foreach (ServiceBuildingInfo serviceBuilding in serviceBuildings.Values)
                {
                    ServiceVehicleInfo serviceVehicle;

                    if (serviceBuilding.Vehicles.TryGetValue(this.vehicleId, out serviceVehicle) && serviceVehicle.Target != 0)
                    {
                        if (targetBuildings != null)
                        {
                            TargetBuildingInfo targetBuilding;

                            if (targetBuildings.TryGetValue(serviceVehicle.Target, out targetBuilding))
                            {
                                targetBuilding.Handled = false;
                            }
                        }

                        if (this.lastDeAssignStamp != Global.CurrentFrame)
                        {
                            serviceVehicle.DeAssign();

                            this.lastDeAssignStamp = Global.CurrentFrame;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// De-assigns the vehicle.
        /// </summary>
        private void DeAssign()
        {
            if (Global.Buildings != null)
            {
                this.DeAssign(Global.Buildings.DeathCareBuildings, Global.Buildings.DeadPeopleBuildings);
                this.DeAssign(Global.Buildings.GarbageBuildings, Global.Buildings.DirtyBuildings);
                this.DeAssign(Global.Buildings.HealthCareBuildings, Global.Buildings.SickPeopleBuildings);
            }

            if (this.lastDeAssignStamp != Global.CurrentFrame)
            {
                if (Log.LogALot)
                {
                    Log.DevDebug(this, "DeAssign", this.dispatcherType, this.vehicleId);
                }

                VehicleHelper.DeAssign(this.vehicleId);
            }
        }
    }
}