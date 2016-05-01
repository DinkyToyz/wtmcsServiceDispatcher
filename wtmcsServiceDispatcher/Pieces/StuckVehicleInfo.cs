﻿using System;
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
        /// The vehicle's last position.
        /// </summary>
        private Vector3 lastPosition = Vector3.zero;

        /// <summary>
        /// The target building identifier.
        /// </summary>
        private ushort targetBuildingId = 0;

        /// <summary>
        /// The frame since when the vehicle has been waiting for a path.
        /// Used for deciding when to consider a vehicle as stuck.
        /// </summary>
        private uint waitingForPathSinceFrame = 0u;

        /// <summary>
        /// The simulation time stamp since when the vehicle has been waiting for a path.
        /// Used for deciding when to de-spawn vehicle.
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

            // Only check vehicles we dispatch unless told to check other vehicles as well.
            if (!(Global.Settings.RemoveStuckVehicles ||
                  (Global.HearseDispatcher != null && vehicle.Info.m_vehicleAI is HearseAI) ||
                  (Global.GarbageTruckDispatcher != null && vehicle.Info.m_vehicleAI is GarbageTruckAI)))
            {
                return false;
            }

            // Trailer?
            if (vehicle.m_leadingVehicle != 0)
            {
                // Todo: Check lost trailers?
                return false;
            }

            // Check vehicles waiting for path.
            if ((vehicle.m_flags & Vehicle.Flags.WaitingPath) != Vehicle.Flags.None)
            {
                return true;
            }

            if (IsConfused(vehicleId, ref vehicle))
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
                    this.waitingForPathSinceTime = 0.0;
                    this.waitingForPathSinceFrame = 0u;
                }
            }

            if (this.confusedSinceFrame > 0 && Global.CurrentFrame - this.confusedSinceFrame > Global.RecallConfusedDelay &&
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
                    Log.DevDebug(this, "Update", "WaitingPath", this.vehicleId, Global.SimulationTime - this.waitingForPathSinceTime, Global.CurrentFrame - this.waitingForPathSinceFrame, Global.Settings.RemoveStuckVehiclesDelaySeconds, Global.WaitPathStuckDelay, this.lastPosition, position, vehicle.m_targetBuilding, vehicle.m_flags, VehicleHelper.GetVehicleName(this.vehicleId));
                }

                // Remember first time stamp the vehicle was seen waiting at this position.
                if (this.waitingForPathSinceFrame == 0 || this.waitingForPathSinceTime == 0 || position != this.lastPosition)
                {
                    this.waitingForPathSinceTime = Global.SimulationTime;
                    this.waitingForPathSinceFrame = Global.CurrentFrame;
                }
            }

            if (!IsConfused(this.vehicleId, ref vehicle))
            {
                this.confusedSinceTime = 0;
                this.confusedSinceFrame = 0;
            }
            else
            {
                if (Log.LogALot)
                {
                    Log.DevDebug(this, "Update", "Confused", this.vehicleId, Global.SimulationTime - this.confusedSinceTime, Global.CurrentFrame - this.confusedSinceFrame, Global.Settings.RemoveStuckVehiclesDelaySeconds, Global.RecallConfusedDelay, this.lastPosition, position, vehicle.m_targetBuilding, vehicle.m_flags, VehicleHelper.GetVehicleName(this.vehicleId));
                }

                if (this.confusedSinceFrame == 0 || this.confusedSinceTime == 0)
                {
                    this.confusedSinceTime = Global.SimulationTime;
                    this.confusedSinceFrame = Global.CurrentFrame;
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

                if (this.confusedSinceTime > 0.0)
                {
                    double delta = Global.SimulationTime - this.confusedSinceTime;

                    if (delta > Global.Settings.RemoveStuckVehiclesDelaySeconds)
                    {
                        Log.Info(this, "IsStuck", "Confused", this.vehicleId, delta, VehicleHelper.GetVehicleName(this.vehicleId));

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
        private static bool ConfusedAmbulance(ref Vehicle data)
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
        /// Check if garbage truck is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if garbage truck is confused.</returns>
        private static bool ConfusedGarbageTruck(ref Vehicle data)
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
        /// Check if hearse is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if hearse is confused.</returns>
        private static bool ConfusedHearse(ref Vehicle data)
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
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_HEARSE_RETURN");
                    return false;
                }
                if ((int)data.m_targetBuilding != 0)
                {
                    ////target = InstanceID.Empty;
                    ////target.Building = data.m_targetBuilding;
                    ////return Locale.Get("VEHICLE_STATUS_HEARSE_COLLECT");
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
        /// Check if passenger car is confused.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if passenger car is confused.</returns>
        private static bool ConfusedPassengerCar(ushort vehicleId, ref Vehicle data)
        {
            // From PassengerCarAI.GetLocalizedStatus from original game code at version 1.4.1-f2.
            // Straight copy from game code even though that's slower than a simple condition check
            // to make it easier to see that the same logic is used.
            CitizenManager instance = Singleton<CitizenManager>.instance;
            ushort driverInstance = ConfusedPassengerCar_GetDriverInstance(vehicleId, ref data);
            ushort num1 = (ushort)0;
            if ((int)driverInstance != 0)
            {
                if ((data.m_flags & Vehicle.Flags.Parking) != Vehicle.Flags.None)
                {
                    uint num2 = instance.m_instances.m_buffer[(int)driverInstance].m_citizen;
                    if ((int)num2 != 0 && (int)instance.m_citizens.m_buffer[num2].m_parkedVehicle != 0)
                    {
                        ////target = InstanceID.Empty;
                        ////return Locale.Get("VEHICLE_STATUS_PARKING");
                        return false;
                    }
                }
                num1 = instance.m_instances.m_buffer[(int)driverInstance].m_targetBuilding;
            }
            if ((int)num1 != 0)
            {
                if ((Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)num1].m_flags & Building.Flags.IncomingOutgoing) != Building.Flags.None)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_LEAVING");
                    return false;
                }
                ////target = InstanceID.Empty;
                ////target.Building = num1;
                ////return Locale.Get("VEHICLE_STATUS_GOINGTO");
                return false;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }

        /// <summary>
        /// Copy of PassengerCarAI.GetDriverInstance to be used by ConfusedPassengerCar.
        /// </summary>
        /// <param name="vehicleID">The vehicle identifier.</param>
        /// <param name="data">The data.</param>
        /// <returns>The driver instance.</returns>
        private static ushort ConfusedPassengerCar_GetDriverInstance(ushort vehicleID, ref Vehicle data)
        {
            // Straight copy from PassengerCarAI.GetDriverInstance from original game code at version 1.4.1-f2.
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num1 = data.m_citizenUnits;
            int num2 = 0;
            while ((int)num1 != 0)
            {
                uint num3 = instance.m_units.m_buffer[num1].m_nextUnit;
                for (int index = 0; index < 5; ++index)
                {
                    uint citizen = instance.m_units.m_buffer[num1].GetCitizen(index);
                    if ((int)citizen != 0)
                    {
                        ushort num4 = instance.m_citizens.m_buffer[citizen].m_instance;
                        if ((int)num4 != 0)
                            return num4;
                    }
                }
                num1 = num3;
                if (++num2 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }
            return (ushort)0;
        }

        /// <summary>
        /// Determines whether the specified vehicle is confused.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>
        /// True if the vehicle is confused.
        /// </returns>
        private static bool IsConfused(ushort vehicleId, ref Vehicle vehicle)
        {
            if (vehicle.Info.m_vehicleAI is HearseAI)
            {
                return ConfusedHearse(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is GarbageTruckAI)
            {
                return ConfusedGarbageTruck(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is AmbulanceAI)
            {
                return ConfusedAmbulance(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is PassengerCarAI)
            {
                return ConfusedPassengerCar(vehicleId, ref vehicle);
            }
            else
            {
                ////BusAI CargoShipAI CargoTrainAI CargoTruckAI FireTruckAI MetroTrainAI PassengerPlaneAI
                ////PassengerShipAI PassengerTrainAI PoliceCarAI CargoShipAI SnowTruckAI TaxiAI TramAI
                return false;
            }
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