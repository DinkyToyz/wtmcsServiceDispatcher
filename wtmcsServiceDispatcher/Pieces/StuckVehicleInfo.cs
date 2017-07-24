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
        /// The handling errors.
        /// </summary>
        private uint handlingErrors = 0;

        /// <summary>
        /// The cargo-vhild flag.
        /// </summary>
        private bool hasCargoParent = false;

        /// <summary>
        /// The vehicle is broken.
        /// </summary>
        private bool isBroken = false;

        private bool isConfused = false;

        private bool isFlagged = false;

        /// <summary>
        /// The vehicle is lost.
        /// </summary>
        private bool isLost = false;

        /// <summary>
        /// The vehicle is stuck.
        /// </summary>
        private bool isStuck = false;

        /// <summary>
        /// The trailer flag.
        /// </summary>
        private bool isTrailer = false;

        /// <summary>
        /// The last de-assign time stamp.
        /// </summary>
        private uint lastDeAssignStamp = 0u;

        /// <summary>
        /// The last handled stamp.
        /// </summary>
        private uint lastHandledStamp = 0;

        /// <summary>
        /// The lost reason.
        /// </summary>
        private LostReasons lostReason = LostReasons.None;

        /// <summary>
        /// The vehicle has been lost since this frame.
        /// Used for deciding when to de-assign vehicle.
        /// </summary>
        private uint lostSinceFrame = 0u;

        /// <summary>
        /// The vehicle has been lost since this time stamp.
        /// Used for deciding when to de-spawn vehicle.
        /// </summary>
        private double lostSinceTime = 0.0;

        /// <summary>
        /// The target building identifier.
        /// </summary>
        private ushort targetBuildingId = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="StuckVehicleInfo"/> class.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        public StuckVehicleInfo(ushort vehicleId, ref Vehicle vehicle)
        {
            this.VehicleId = vehicleId;

            if (vehicle.m_leadingVehicle == 0)
            {
                this.dispatcherType = Dispatcher.GetDispatcherType(ref vehicle);
            }
            else
            {
                try
                {
                    this.dispatcherType = Dispatcher.GetDispatcherType(ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId]);
                }
                catch
                {
                    this.dispatcherType = Dispatcher.DispatcherTypes.None;
                }
            }

            this.Update(ref vehicle);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StuckVehicleInfo" /> class.
        /// </summary>
        private StuckVehicleInfo()
        {
            this.VehicleId = 0;
        }

        /// <summary>
        /// The different resons for considering a trailer lost.
        /// </summary>
        public enum LostReasons
        {
            /// <summary>
            /// Not lost.
            /// </summary>
            None = 0,

            /// <summary>
            /// Lead vehicle isn't.
            /// </summary>
            NoLead = 1,

            /// <summary>
            /// Broken chain of vehicles.
            /// </summary>
            IgnorantLead = 2
        }

        /// <summary>
        /// Gets the information header.
        /// </summary>
        /// <returns>The information header.</returns>
        public static string InfoHeader => InfoString(null, null, null, true);

        /// <summary>
        /// Gets a value indicating whether this <see cref="StuckVehicleInfo"/> is confused.
        /// </summary>
        /// <value>
        ///   <c>true</c> if confused; otherwise, <c>false</c>.
        /// </value>
        public bool Confused => this.confusedSinceFrame > 0 && this.confusedSinceTime > 0;

        /// <summary>
        /// Gets a value indicating whether the vehicle is the dispatcher's responsibility.
        /// </summary>
        /// <value>
        /// <c>true</c> if the vehicle is the dispatcher's responsibility; otherwise, <c>false</c>.
        /// </value>
        public bool DispatchersResponsibility => Global.Settings.RecoveryCrews.DispatchVehicles || IsDispatchersResponsibility(this.dispatcherType);

        /// <summary>
        /// Gets the type of the dispatcher.
        /// </summary>
        /// <value>
        /// The type of the dispatcher.
        /// </value>
        public Dispatcher.DispatcherTypes DispatcherType => this.dispatcherType;

        /// <summary>
        /// Gets the extra information.
        /// </summary>
        /// <value>
        /// The extra information.
        /// </value>
        public IEnumerable<string> ExtraInfo
        {
            get
            {
                List<string> info = new List<string>();

                if (this.isTrailer)
                {
                    info.Add("Trailer");
                }

                if (this.hasCargoParent)
                {
                    info.Add("CargoChild");
                }

                return (info.Count == 0) ? null : info;
            }
        }

        /// <summary>
        /// Gets the flagged flags.
        /// </summary>
        /// <value>
        /// The flagged flags.
        /// </value>
        public Vehicle.Flags Flagged => this.checkFlags;

        /// <summary>
        /// Gets the lost-reason.
        /// </summary>
        /// <value>
        /// The lost-reason.
        /// </value>
        public LostReasons Lost => this.lostReason;

        /// <summary>
        /// Gets the problem level.
        /// </summary>
        /// <value>
        /// The problem level.
        /// </value>
        public int ProblemLevel
        {
            get
            {
                byte level = 0;

                if (this.isFlagged)
                {
                    level += 1;
                }

                if (this.isLost)
                {
                    level += 2;
                }

                if (this.isConfused)
                {
                    level += 4;
                }

                if (this.isBroken)
                {
                    level += 8;
                }

                if (this.isStuck)
                {
                    level = 16;
                }

                return level;
            }
        }

        /// <summary>
        /// The vehicle identifier.
        /// </summary>
        public ushort VehicleId { get; private set; }

        /// <summary>
        /// Gets the name of the vehicle.
        /// </summary>
        /// <value>
        /// The name of the vehicle.
        /// </value>
        public string VehicleName => VehicleHelper.GetVehicleName(this.VehicleId);

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
        /// Gets the amount of frames during which the vehicle has been lost.
        /// </summary>
        /// <value>
        /// The lost frame count.
        /// </value>
        private uint LostForFrames
        {
            get
            {
                return (this.lostSinceFrame > 0 && this.lostSinceFrame < Global.CurrentFrame) ? Global.CurrentFrame - this.lostSinceFrame : 0;
            }
        }

        /// <summary>
        /// Gets the duration in seconds during which the vehicle has been lost.
        /// </summary>
        /// <value>
        /// The lost duration.
        /// </value>
        private double LostForSeconds
        {
            get
            {
                return (this.lostSinceTime > 0 && this.lostSinceTime < Global.SimulationTime) ? Global.SimulationTime - this.lostSinceTime : 0;
            }
        }

        /// <summary>
        /// Tries to deserialize the specified serialized data to this instance.
        /// </summary>
        /// <param name="serializedData">The serialized data.</param>
        /// <param name="stuckVehicleInfo">The stuck vehicle information.</param>
        /// <returns>
        /// The deserialization result.
        /// </returns>
        public static SerializableSettings.DeserializationResult Deserialize(SerializableSettings.BinaryData serializedData, out StuckVehicleInfo stuckVehicleInfo)
        {
            stuckVehicleInfo = new StuckVehicleInfo();
            SerializableSettings.DeserializationResult result = stuckVehicleInfo.Deserialize(serializedData);

            if (result != SerializableSettings.DeserializationResult.Success)
            {
                stuckVehicleInfo = null;
            }

            return result;
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

            // Cargo parent?
            if (vehicle.m_cargoParent != 0 && !Global.Settings.Experimental.AllowTrackingOfCargoChildren)
            {
                return false;
            }

            // Trailer?
            if (vehicle.m_leadingVehicle != 0)
            {
                if (!Global.Settings.Experimental.TrackLostTrailers)
                {
                    return false;
                }

                return TrailerHasProblem(vehicleId, ref vehicle);
            }

            // Only check vehicles we dispatch unless told to check other vehicles as well.
            if (!(Global.Settings.RecoveryCrews.DispatchVehicles || IsDispatchersResponsibility(ref vehicle)))
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
        /// Determines whether this vehicle is dispatchers responsibility.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>
        ///   <c>true</c> if dispatchers responsibility; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDispatchersResponsibility(ref Vehicle vehicle)
        {
            return vehicle.Info != null && vehicle.Info.m_vehicleAI != null &&
                   ((Global.Settings.DeathCare.DispatchVehicles && Global.HearseDispatcher != null && vehicle.Info.m_vehicleAI is HearseAI) ||
                    (Global.Settings.Garbage.DispatchVehicles && Global.GarbageTruckDispatcher != null && vehicle.Info.m_vehicleAI is GarbageTruckAI) ||
                    (Global.Settings.HealthCare.DispatchVehicles && Global.AmbulanceDispatcher != null && vehicle.Info.m_vehicleAI is AmbulanceAI));
        }

        /// <summary>
        /// Determines whether this dispatcher type is dispatchers responsibility.
        /// </summary>
        /// <param name="dispatcherType">Type of the dispatcher.</param>
        /// <returns>
        ///   <c>true</c> if dispatchers responsibility; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDispatchersResponsibility(Dispatcher.DispatcherTypes dispatcherType)
        {
            return (Global.Settings.DeathCare.DispatchVehicles && Global.HearseDispatcher != null && dispatcherType == Dispatcher.DispatcherTypes.HearseDispatcher) ||
                   (Global.Settings.Garbage.DispatchVehicles && Global.GarbageTruckDispatcher != null && dispatcherType == Dispatcher.DispatcherTypes.GarbageTruckDispatcher) ||
                   (Global.Settings.HealthCare.DispatchVehicles && Global.AmbulanceDispatcher != null && dispatcherType == Dispatcher.DispatcherTypes.AmbulanceDispatcher);
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

            Log.DevDebug(this, "DebugLog", this.VehicleId, info);
        }

        /// <summary>
        /// Gets the information line.
        /// </summary>
        /// <param name="vehicles">The vehicles.</param>
        /// <param name="buildings">The buildings.</param>
        /// <returns>The information line.</returns>
        public string GetInfoLine(Vehicle[] vehicles = null, Building[] buildings = null)
        {
            return InfoString(this, vehicles, buildings, false);
        }

        /// <summary>
        /// Handles the vehicles problems.
        /// </summary>
        /// <returns>True if a problem was handled.</returns>
        public bool HandleProblem()
        {
            if (this.hasCargoParent &&
                !((this.isFlagged || this.isLost) && Global.Settings.Experimental.AllowRemovalOfStuckCargoChildren) &&
                !(this.isConfused && Global.Settings.Experimental.AllowRemovalOfConfusedCargoChildren))
            {
                if (Log.LogALot)
                {
                    Log.DevDebug(this, "HandleProblem", "StuckLostBroken", "IgnoreCargoChild", this.VehicleId, VehicleHelper.GetVehicleName(this.VehicleId));
                }

                this.lastHandledStamp = Global.CurrentFrame;
                return false;
            }

            if (this.isTrailer && !Global.Settings.Experimental.RemoveLostTrailers)
            {
                if (Log.LogALot)
                {
                    Log.DevDebug(this, "HandleProblem", "StuckLostBroken", "IgnoreTrailer", this.VehicleId, VehicleHelper.GetVehicleName(this.VehicleId));
                }

                this.lastHandledStamp = Global.CurrentFrame;
                return false;
            }

            if (this.isStuck && this.lastHandledStamp + Global.CheckFlagStuckDelay < Global.CurrentFrame)
            {
                try
                {
                    if (this.isTrailer)
                    {
                        // Remove trailer vehicle.
                        Log.Debug(this, "HandleProblem", "StuckLostBroken", "DeSpawnTrailer", this.VehicleId, this.ExtraInfo, VehicleHelper.GetVehicleName(this.VehicleId));
                        this.DeSpawnTrailer();
                    }
                    else
                    {
                        // Remove vehicle.
                        Log.Debug(this, "HandleProblem", "StuckLostBroken", "DeSpawn", this.VehicleId, this.ExtraInfo, VehicleHelper.GetVehicleName(this.VehicleId));
                        this.DeSpawn();
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(this, "HandleProblem", ex, this.VehicleId);
                    this.handlingErrors++;

                    return this.handlingErrors > 10;
                }
                finally
                {
                    this.isStuck = false;
                    this.lastHandledStamp = Global.CurrentFrame;
                }
            }

            if (this.ConfusedDeAssignedForFrames > Global.DeAssignConfusedDelay && !this.isTrailer && IsDispatchersResponsibility(this.dispatcherType))
            {
                try
                {
                    // De-assign vehicle.
                    Log.Debug(this, "HandleProblem", "Confused", "DeAssign", this.VehicleId, VehicleHelper.GetVehicleName(this.VehicleId));
                    this.DeAssign();
                }
                catch (Exception ex)
                {
                    Log.Error(this, "HandleProblem", ex, this.VehicleId);
                    this.handlingErrors++;

                    return this.handlingErrors > 10;
                }
                finally
                {
                    this.confusedDeAssignedSinceFrame = Global.CurrentFrame;
                }
            }

            return false;
        }

        /// <summary>
        /// Serializes this instance.
        /// </summary>
        /// <returns>The serialized data.</returns>
        public void Serialize(SerializableSettings.BinaryData serializedData)
        {
            serializedData.ResetLocalCheckSum();

            // Version.
            serializedData.AddVersion(0);

            // Data.
            serializedData.Add(this.VehicleId);
            serializedData.Add(this.targetBuildingId);
            serializedData.Add(this.dispatcherType);
            serializedData.Add(this.checkFlags);
            serializedData.Add(this.checkFlagPosition);
            serializedData.Add(this.checkFlagSinceFrame);
            serializedData.Add(this.checkFlagSinceTime);
            serializedData.Add(this.confusedDeAssignedSinceFrame);
            serializedData.Add(this.confusedSinceFrame);
            serializedData.Add(this.confusedSinceTime);
            serializedData.Add(this.lostSinceFrame);
            serializedData.Add(this.lostSinceTime);
            serializedData.Add(this.lostReason);

            serializedData.AddLocalCheckSum();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            Log.InfoList info = new Log.InfoList("StuckVehicleInfo: ");

            info.Add("VehicleId", this.VehicleId);
            info.Add("CheckFlags", this.checkFlags);
            info.Add("DispatcherType", this.dispatcherType);
            info.Add("DispatchersResponsibility", this.DispatchersResponsibility);

            return info.ToString();
        }

        /// <summary>
        /// Updates the specified vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        public void Update(ref Vehicle vehicle)
        {
            this.targetBuildingId = vehicle.m_targetBuilding;
            this.isTrailer = vehicle.m_leadingVehicle != 0;
            this.hasCargoParent = vehicle.m_cargoParent != 0;

            if (this.hasCargoParent && !Global.Settings.Experimental.AllowTrackingOfCargoChildren)
            {
                return;
            }

            if (this.isTrailer)
            {
                if (Global.Settings.Experimental.TrackLostTrailers)
                {
                    this.UpdateTrailer(ref vehicle);
                }

                return;
            }

            // Check if vehicle has flag that should be checked.
            Vehicle.Flags flags = vehicle.m_flags & FlagsToCheck;
            if ((flags & VehicleHelper.VehicleAll) != ~VehicleHelper.VehicleAll)
            {
                Vector3 position = vehicle.GetLastFramePosition();

                // Remember first time stamp the vehicle was seen with this flag at this position.
                if (this.checkFlagSinceFrame == 0 || this.checkFlagSinceTime == 0 || flags != this.checkFlags || Math.Truncate((position - this.checkFlagPosition).sqrMagnitude) > 0)
                {
                    this.isFlagged = false;
                    this.checkFlags = flags;
                    this.checkFlagPosition = position;
                    this.checkFlagSinceTime = Global.SimulationTime;
                    this.checkFlagSinceFrame = Global.CurrentFrame;
                }
                else if (!this.isFlagged && this.CheckFlaggedForFrames > Global.CheckFlagStuckDelay)
                {
                    // Check if stuck with flag.
                    double delta = this.CheckFlaggedForSeconds;

                    if (delta > Global.Settings.RecoveryCrews.DelaySeconds)
                    {
                        Log.Info(this, "IsFlagged", this.checkFlags, this.VehicleId, delta, VehicleHelper.GetVehicleName(this.VehicleId));
                        this.isFlagged = true;
                    }
                }
            }
            else if ((this.checkFlags & VehicleHelper.VehicleAll) != ~VehicleHelper.VehicleAll || this.checkFlagSinceTime != 0 || this.checkFlagSinceFrame != 0)
            {
                this.isFlagged = false;
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
                        Log.DevDebug(this, "Update", "NewConfused", this.VehicleId, this.ConfusedForSeconds, this.ConfusedForFrames, Global.Settings.RecoveryCrews.DelaySeconds, Global.DeAssignConfusedDelay, vehicle.m_targetBuilding, vehicle.m_flags, VehicleHelper.GetVehicleName(this.VehicleId));
                    }

                    this.isConfused = false;
                    this.confusedSinceTime = Global.SimulationTime;
                    this.confusedSinceFrame = Global.CurrentFrame;
                    this.confusedDeAssignedSinceFrame = Global.CurrentFrame;
                }
                else if (!this.isConfused && this.ConfusedForFrames > Global.CheckFlagStuckDelay)
                {
                    // Check if stuck confused.
                    double delta = this.ConfusedForSeconds;

                    if (delta > Global.Settings.RecoveryCrews.DelaySeconds)
                    {
                        Log.Info(this, "IsConfused", this.VehicleId, delta, VehicleHelper.GetVehicleName(this.VehicleId));
                        this.isConfused = true;
                    }
                }
            }
            else if (this.confusedSinceTime != 0 || this.confusedSinceFrame != 0 || this.confusedDeAssignedSinceFrame != 0)
            {
                this.isConfused = false;
                this.confusedSinceTime = 0;
                this.confusedSinceFrame = 0;
                this.confusedDeAssignedSinceFrame = 0;
            }

            this.isStuck = this.isLost || this.isConfused || this.isFlagged || this.isBroken;
        }

        /// <summary>
        /// De-spawns the vehicle.
        /// </summary>
        private static void DeSpawnVehicle(ushort vehicleId)
        {
            if (Global.Buildings != null)
            {
                foreach (BuildingKeeper.StandardServiceBuildings buildings in Global.Buildings.StandardServices)
                {
                    foreach (ServiceBuildingInfo serviceBuilding in buildings.ServiceBuildings.Values)
                    {
                        ServiceVehicleInfo serviceVehicle;

                        if (serviceBuilding.Vehicles.TryGetValue(vehicleId, out serviceVehicle) && serviceVehicle.Target != 0)
                        {
                            TargetBuildingInfo targetBuilding;

                            if (buildings.TargetBuildings.TryGetValue(serviceVehicle.Target, out targetBuilding))
                            {
                                targetBuilding.Handled = false;
                            }
                        }

                        serviceBuilding.Vehicles.Remove(vehicleId);
                    }
                }
            }

            VehicleHelper.DeSpawn(vehicleId);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="vehicles">The vehicles.</param>
        /// <param name="buildings">The buildings.</param>
        /// <param name="getHead">if set to <c>true</c> get header instead of data.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        private static string InfoString(StuckVehicleInfo vehicle, Vehicle[] vehicles, Building[] buildings, bool getHead)
        {
            Log.InfoList info = new Log.InfoList();

            List<string> status = null;
            List<string> dispatch = null;
            string ai = null;

            if (!getHead)
            {
                if (vehicles == null)
                {
                    vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
                }

                if (buildings == null)
                {
                    buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
                }

                status = new List<string>(1);
                if (vehicle.isStuck)
                {
                    status.Add("Stuck");
                }
                if (vehicle.isBroken)
                {
                    status.Add("Broken");
                }
                if (vehicle.isLost)
                {
                    status.Add("Lost");
                }
                if (vehicle.isConfused)
                {
                    status.Add("Confused");
                }
                if (vehicle.isFlagged)
                {
                    status.Add("Flagged");
                }
                if (status.Count == 0)
                {
                    status.Add("Checking");
                }

                dispatch = new List<string>(2);
                if (IsDispatchersResponsibility(vehicle.dispatcherType))
                {
                    dispatch.Add("IsResponsible");
                }
                if (vehicle.dispatcherType != Dispatcher.DispatcherTypes.None)
                {
                    dispatch.Add(vehicle.dispatcherType.ToString());
                }

                try
                {
                    if (vehicles[vehicle.VehicleId].Info != null)
                    {
                        if (vehicles[vehicle.VehicleId].Info.m_vehicleAI != null)
                        {
                            ai = vehicles[vehicle.VehicleId].Info.m_vehicleAI.GetType().ToString();
                        }
                    }
                }
                catch { }
            }

            if (getHead)
            {
                info.Add("<Status>", "<Status>");
                info.Add("<Vehicle>", "<VehicleId>", "[VehicleName]", "[VehicleAI]", "[ExtraInfo]");
            }
            else
            {
                info.Add("Status", status);
                info.Add("Vehicle", vehicle.VehicleId, vehicle.VehicleName, ai, vehicle.ExtraInfo);
            }

            if (getHead)
            {
                info.Add("[Dispatch]", "[Responsibility]", "[DispatcherType]");
            }
            else if (dispatch.Count > 0)
            {
                info.Add("Dispatch", dispatch);
            }

            if (getHead)
            {
                info.Add("[Confused]", "<ConfusedForSeconds>", "<ConfusedForFrames>");
            }
            else if (vehicle.confusedSinceFrame > 0 || vehicle.confusedSinceTime > 0.0)
            {
                info.Add("Confused", vehicle.ConfusedForSeconds, vehicle.ConfusedForFrames);
            }

            if (getHead)
            {
                info.Add("[Flagged]", "<FlaggedForSeconds>", "<FlaggedForFrames>", "[Flags]", "[Position]");
            }
            else if (vehicle.checkFlagSinceFrame > 0 || vehicle.checkFlagSinceTime > 0.0)
            {
                info.Add("Flagged", vehicle.CheckFlaggedForSeconds, vehicle.CheckFlaggedForFrames, vehicle.checkFlags, vehicle.checkFlagPosition);
            }

            if (getHead)
            {
                info.Add("[Lost]", "<LostForSeconds>", "<LostForFrames>", "[LostReason]");
            }
            else if (vehicle.lostSinceFrame > 0 || vehicle.lostSinceTime > 0.0)
            {
                info.Add("Lost", vehicle.LostForSeconds, vehicle.LostForFrames, vehicle.lostReason);
            }

            if (getHead)
            {
                info.Add("[District]", "<districtId>", "[DistrictName]");
            }
            else if (vehicle.checkFlagSinceFrame > 0 || vehicle.checkFlagSinceTime > 0.0)
            {
                try
                {
                    byte districtId = DistrictHelper.GetDistrict(vehicle.checkFlagPosition);
                    if (districtId != 0)
                    {
                        info.Add("District", districtId, DistrictHelper.GetDistrictName(districtId));
                    }
                }
                catch { }
            }

            if (getHead)
            {
                InfoStringInfoForBuilding(null, null, null, true, 0, "SourceBuilding", info);
                InfoStringInfoForBuilding(null, null, null, true, 0, "TargetBuilding", info);
            }
            else
            {
                try
                {
                    InfoStringInfoForBuilding(vehicle, vehicles, buildings, false, vehicles[vehicle.VehicleId].m_sourceBuilding, "SourceBuilding", info);
                    InfoStringInfoForBuilding(vehicle, vehicles, buildings, false, vehicles[vehicle.VehicleId].m_targetBuilding, "TargetBuilding", info);
                }
                catch { }
            }

            if (getHead)
            {
                info.Add("[Handling]", "<HandledForFrames>", "<HandlingErrors>");
            }
            else if (vehicle.handlingErrors > 0 || vehicle.lastHandledStamp > 0)
            {
                info.Add("Handling", (vehicle.lastHandledStamp > 0 && vehicle.lastHandledStamp < Global.CurrentFrame) ? Global.CurrentFrame - vehicle.lastHandledStamp : 0, vehicle.handlingErrors);
            }

            return info.ToString();
        }

        /// <summary>
        /// Adds the building information to the string information.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="vehicles">The vehicles.</param>
        /// <param name="buildings">The buildings.</param>
        /// <param name="getHead">if set to <c>true</c> get for header instead of data.</param>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="buildingType">Type of the building.</param>
        /// <param name="info">The information.</param>
        private static void InfoStringInfoForBuilding(StuckVehicleInfo vehicle, Vehicle[] vehicles, Building[] buildings, bool getHead, ushort buildingId, string buildingType, Log.InfoList info)
        {
            if (getHead)
            {
                info.Add("[" + buildingType + "]", "<buildingId>", "[BuildingName]", "[districtId]", "[DistrictName]");
                return;
            }

            if (buildingId == 0)
            {
                return;
            }

            byte districtId = BuildingHelper.GetDistrict(buildingId);

            if (districtId == 0)
            {
                info.Add(
                    buildingType,
                    buildingId,
                    BuildingHelper.GetBuildingName(vehicles[vehicle.VehicleId].m_sourceBuilding));
            }
            else
            {
                info.Add(
                    buildingType,
                    buildingId,
                    BuildingHelper.GetBuildingName(vehicles[vehicle.VehicleId].m_sourceBuilding),
                    districtId,
                    DistrictHelper.GetDistrictName(districtId));
            }
        }

        /// <summary>
        /// Determines whether the specified trailer vehicle has a problem.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>
        /// True if the vehicle has at least one problem.
        /// </returns>
        private static bool TrailerHasProblem(ushort vehicleId, ref Vehicle vehicle)
        {
            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            ushort count = 0;
            ushort leadId = vehicleId;
            ushort nextId = vehicle.m_leadingVehicle;
            while (nextId != 0)
            {
                if (vehicles[nextId].m_trailingVehicle != leadId)
                {
                    return true;
                }

                if (count >= ushort.MaxValue)
                {
                    throw new Exception("Loop counter too high");
                }
                count++;

                leadId = nextId;
                nextId = vehicles[leadId].m_leadingVehicle;
            }

            if (!(Global.Settings.RecoveryCrews.DispatchVehicles || IsDispatchersResponsibility(ref vehicles[leadId])))
            {
                return false;
            }

            if (vehicles[leadId].Info == null || (vehicles[leadId].m_flags & Vehicle.Flags.Spawned) == ~VehicleHelper.VehicleAll)
            {
                return true;
            }

            return false;
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
                info.Add("VehicleId", this.VehicleId);

                Vehicle vehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[this.VehicleId];

                info.Add("LeadingVehicle", vehicle.m_leadingVehicle);
                info.Add("TrailingVehicle", vehicle.m_trailingVehicle);
                info.Add("CargoParent", vehicle.m_cargoParent);
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
        /// De-assigns the vehicle.
        /// </summary>
        private void DeAssign()
        {
            if (Global.Buildings != null)
            {
                foreach (BuildingKeeper.StandardServiceBuildings buildings in Global.Buildings.StandardServices)
                {
                    foreach (ServiceBuildingInfo serviceBuilding in buildings.ServiceBuildings.Values)
                    {
                        ServiceVehicleInfo serviceVehicle;

                        if (serviceBuilding.Vehicles.TryGetValue(this.VehicleId, out serviceVehicle) && serviceVehicle.Target != 0)
                        {
                            TargetBuildingInfo targetBuilding;

                            if (buildings.TargetBuildings.TryGetValue(serviceVehicle.Target, out targetBuilding))
                            {
                                targetBuilding.Handled = false;
                            }

                            if (this.lastDeAssignStamp != Global.CurrentFrame)
                            {
                                if (serviceVehicle.DeAssign().DeSpawned)
                                {
                                    serviceBuilding.Vehicles.Remove(this.VehicleId);
                                }

                                this.lastDeAssignStamp = Global.CurrentFrame;
                            }
                        }
                    }
                }
            }

            if (this.lastDeAssignStamp != Global.CurrentFrame)
            {
                if (Log.LogALot)
                {
                    Log.DevDebug(this, "DeAssign", this.dispatcherType, this.VehicleId);
                }

                VehicleHelper.DeAssign(this.VehicleId);
            }
        }

        /// <summary>
        /// Deserializes the specified serialized data to this instance.
        /// </summary>
        /// <param name="serializedData">The serialized data.</param>
        /// <returns>
        /// The deserialization result.
        /// </returns>
        private SerializableSettings.DeserializationResult Deserialize(SerializableSettings.BinaryData serializedData)
        {
            if (serializedData == null || serializedData.Left == 0)
            {
                return SerializableSettings.DeserializationResult.EndOfData;
            }

            serializedData.ResetLocalCheckSum();

            ushort version = serializedData.GetVersion();
            if (version > 0)
            {
                Log.Warning(this, "Deserialize", "Serialized data version too high!", version, 0);
                return SerializableSettings.DeserializationResult.Error;
            }

            this.VehicleId = serializedData.GetUshort();
            this.targetBuildingId = serializedData.GetUshort();
            this.dispatcherType = serializedData.GetDispatcherType();
            this.checkFlags = serializedData.GetVehicleFlags();
            this.checkFlagPosition = serializedData.GetVector3();
            this.checkFlagSinceFrame = serializedData.GetUint();
            this.checkFlagSinceTime = serializedData.GetDouble();
            this.confusedDeAssignedSinceFrame = serializedData.GetUint();
            this.confusedSinceFrame = serializedData.GetUint();
            this.confusedSinceTime = serializedData.GetDouble();
            this.lastHandledStamp = serializedData.GetUint();
            this.lostReason = serializedData.GetLostReason();

            serializedData.CheckLocalCheckSum();

            return SerializableSettings.DeserializationResult.Success;
        }

        /// <summary>
        /// De-spawns the vehicle.
        /// </summary>
        private void DeSpawn()
        {
            DeSpawnVehicle(this.VehicleId);
        }

        private void DeSpawnTrailer()
        {
            ushort count;
            ushort prevId;
            ushort nextId;

            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            LinkedList<ushort> ids = new LinkedList<ushort>(new ushort[] { this.VehicleId });

            count = 0;
            prevId = this.VehicleId;
            nextId = vehicles[prevId].m_leadingVehicle;
            while (nextId != 0)
            {
                if (vehicles[nextId].m_trailingVehicle != prevId)
                {
                    break;
                }

                ids.AddFirst(nextId);

                if (count >= ushort.MaxValue)
                {
                    throw new Exception("Loop counter too high");
                }
                count++;

                prevId = nextId;
                nextId = vehicles[prevId].m_leadingVehicle;
            }

            count = 0;
            prevId = this.VehicleId;
            nextId = vehicles[prevId].m_trailingVehicle;
            while (nextId != 0)
            {
                if (vehicles[nextId].m_leadingVehicle != prevId)
                {
                    break;
                }
                ids.AddLast(nextId);

                if (count >= ushort.MaxValue)
                {
                    throw new Exception("Loop counter too high");
                }
                count++;

                prevId = nextId;
                nextId = vehicles[prevId].m_trailingVehicle;
            }

            count = 0;
            foreach (ushort id in ids)
            {
                if (count == 0 && vehicles[id].m_leadingVehicle == 0)
                {
                    DeSpawnVehicle(id);
                }
                else
                {
                    VehicleHelper.DeSpawn(id);
                }

                count++;
            }
        }

        /// <summary>
        /// Updates the specified trailer vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        private void UpdateTrailer(ref Vehicle vehicle)
        {
            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            LostReasons lost = LostReasons.None;

            ushort count = 0;
            ushort leadId = this.VehicleId;
            ushort nextId = vehicle.m_leadingVehicle;
            while (nextId != 0)
            {
                if (vehicles[nextId].m_trailingVehicle != leadId)
                {
                    lost = LostReasons.IgnorantLead;
                    break;
                }

                if (count >= ushort.MaxValue)
                {
                    throw new Exception("Loop counter too high");
                }
                count++;

                leadId = nextId;
                nextId = vehicles[leadId].m_leadingVehicle;
            }

            if (lost == LostReasons.None && (vehicles[leadId].Info == null || (vehicles[leadId].m_flags & Vehicle.Flags.Spawned) == ~VehicleHelper.VehicleAll))
            {
                lost = LostReasons.NoLead;
            }

            if (lost != LostReasons.None)
            {
                if (this.lostSinceFrame == 0 || this.lostSinceTime == 0 || this.lostReason == LostReasons.None || lost != this.lostReason)
                {
                    if (Log.LogALot)
                    {
                        Log.DevDebug(this, "UpdateTrailer", "NewLost", lost, this.VehicleId, this.LostForSeconds, this.LostForFrames, Global.Settings.RecoveryCrews.DelaySeconds, Global.CheckFlagStuckDelay, vehicle.m_leadingVehicle, vehicle.m_flags, VehicleHelper.GetVehicleName(this.VehicleId));
                    }

                    this.isLost = false;
                    this.lostReason = lost;
                    this.lostSinceTime = Global.SimulationTime;
                    this.lostSinceFrame = Global.CurrentFrame;
                }
                else if (!this.isLost)
                {
                    double delta;

                    if (this.lostReason != LostReasons.None && this.LostForFrames > Global.CheckFlagStuckDelay)
                    {
                        delta = this.LostForSeconds;

                        if (delta > Global.Settings.RecoveryCrews.DelaySeconds)
                        {
                            Log.Info(this, "IsLost", lost, this.VehicleId, delta, VehicleHelper.GetVehicleName(this.VehicleId));
                            this.isLost = true;
                        }
                    }
                }
            }
            else
            {
                this.isLost = false;
                this.lostReason = LostReasons.None;
                this.lostSinceTime = 0;
                this.lostSinceFrame = 0;
            }

            this.isStuck = this.isLost || this.isConfused || this.isFlagged || this.isBroken;
        }
    }
}