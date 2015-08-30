using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Service vehicle dispatch base class.
    /// </summary>
    internal class Dispatcher
    {
        /// <summary>
        /// The problem buffer modifier
        /// </summary>
        public static readonly ushort ProblemBufferModifier = 1;

        /// <summary>
        /// The forgotten problem value limit.
        /// </summary>
        public static readonly ushort ProblemLimitForgotten = 12000;

        /// <summary>
        /// The problem timer modifier.
        /// </summary>
        public static readonly ushort ProblemTimerModifier = 48;

        /// <summary>
        /// The problem limit.
        /// </summary>
        public static ushort ProblemLimit = 3000;

        /// <summary>
        /// The problem major limit.
        /// </summary>
        public static ushort ProblemLimitMajor = 6000;

        /// <summary>
        /// The dispatcher type.
        /// </summary>
        public readonly DispatcherTypes DispatcherType;

        /// <summary>
        /// The assigned targets.
        /// </summary>
        protected Dictionary<ushort, uint> assignedTargets = new Dictionary<ushort, uint>();

        /// <summary>
        /// The service buildings.
        /// </summary>
        protected Dictionary<ushort, Buildings.ServiceBuildingInfo> serviceBuildings;

        /// <summary>
        /// The target buildings
        /// </summary>
        protected Dictionary<ushort, Buildings.TargetBuildingInfo> TargetBuildings;

        /// <summary>
        /// Set target to source building when de-assigning vehicle.
        /// </summary>
        private const bool DeAssignToSource = false;

        /// <summary>
        /// The bulding check parameters.
        /// </summary>
        private BuldingCheckParameters[] buildingChecks;

        /// <summary>
        /// The free vehicle count.
        /// </summary>
        private int freeVehicles = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dispatcher"/> class.
        /// </summary>
        public Dispatcher(DispatcherTypes dispatcherType)
        {
            this.DispatcherType = dispatcherType;

            switch (this.DispatcherType)
            {
                case DispatcherTypes.HearseDispatcher:
                    transferType = (byte)TransferManager.TransferReason.Dead;
                    serviceBuildings = Global.Buildings.HearseBuildings;
                    TargetBuildings = Global.Buildings.DeadPeopleBuildings;
                    buildingChecks = BuldingCheckParameters.GetBuldingCheckParameters(Global.Settings.DeathChecksParameters);
                    break;

                case DispatcherTypes.GarbageTruckDispatcher:
                    transferType = (byte)TransferManager.TransferReason.Garbage;
                    serviceBuildings = Global.Buildings.GarbageBuildings;
                    TargetBuildings = Global.Buildings.DirtyBuildings;
                    buildingChecks = BuldingCheckParameters.GetBuldingCheckParameters(Global.Settings.GarbageChecksParameters);
                    break;

                default:
                    throw new Exception("Bad dispatcher type");
            }

            Log.Debug(this, "Constructed");
        }

        /// <summary>
        /// The dispatcher types.
        /// </summary>
        public enum DispatcherTypes
        {
            None = 0,
            HearseDispatcher = 1,
            GarbageTruckDispatcher = 2
        }

        /// <summary>
        /// Gets or sets the type of the transfer.
        /// </summary>
        /// <value>
        /// The type of the transfer.
        /// </value>
        public byte transferType { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this service has target buildings.
        /// </summary>
        /// <value>
        /// <c>true</c> if this service has target buildings; otherwise, <c>false</c>.
        /// </value>
        protected bool HasTargetBuildings
        {
            get
            {
                switch (this.DispatcherType)
                {
                    case DispatcherTypes.HearseDispatcher:
                        return Global.Buildings.HasDeadPeopleBuildingsToCheck;

                    case DispatcherTypes.GarbageTruckDispatcher:
                        return Global.Buildings.HasDirtyBuildingsToCheck;

                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Checks the vehicle target.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        public void CheckVehicleTarget(ushort vehicleId, ref Vehicle vehicle)
        {
            Buildings.ServiceBuildingInfo serviceBuilding = serviceBuildings.ContainsKey(vehicle.m_sourceBuilding) ? serviceBuildings[vehicle.m_sourceBuilding] : null;

            if (serviceBuilding == null)
            {
                if (Log.LogALot) Log.DevDebug(this, "CheckVehicleTarget", "NewBuilding", vehicleId, vehicle.m_targetBuilding);

                vehicle.Info.m_vehicleAI.SetTarget(vehicleId, ref vehicle, 0);
            }
            else if (!serviceBuilding.Vehicles.ContainsKey(vehicleId))
            {
                if (Log.LogALot) Log.DevDebug(this, "CheckVehicleTarget", "NewVehicle", vehicleId, vehicle.m_targetBuilding);

                vehicle.Info.m_vehicleAI.SetTarget(vehicleId, ref vehicle, DeAssignToSource ? vehicle.m_sourceBuilding : (ushort)0);
            }
            else if (!TargetBuildings.ContainsKey(vehicle.m_targetBuilding))
            {
                if (Log.LogALot) Log.DevDebug(this, "CheckVehicleTarget", "NoNeed", vehicleId, vehicle.m_targetBuilding);

                vehicle.Info.m_vehicleAI.SetTarget(vehicleId, ref vehicle, DeAssignToSource ? vehicle.m_sourceBuilding : (ushort)0);
                serviceBuilding.Vehicles[vehicleId].Target = DeAssignToSource ? vehicle.m_sourceBuilding : (ushort)0;
            }
            else if (vehicle.m_targetBuilding != serviceBuilding.Vehicles[vehicleId].Target)
            {
                if (Log.LogALot) Log.DevDebug(this, "CheckVehicleTarget", "WrongTarget", vehicleId, vehicle.m_targetBuilding, serviceBuilding.Vehicles[vehicleId].Target);

                vehicle.Info.m_vehicleAI.SetTarget(vehicleId, ref vehicle, DeAssignToSource ? vehicle.m_sourceBuilding : (ushort)0);
                serviceBuilding.Vehicles[vehicleId].Target = DeAssignToSource ? vehicle.m_sourceBuilding : (ushort)0;
            }
        }

        /// <summary>
        /// Dispatches vehicles to targets.
        /// </summary>
        public virtual void Dispatch()
        {
            // Dispatch, if needed.
            if (HasTargetBuildings)
            {
                bool initialized = false;
                foreach (BuldingCheckParameters bcParams in buildingChecks)
                {
                    if (Log.LogALot) Log.DevDebug(this, "Dispatch", bcParams.Setting, bcParams.OnlyProblematic, bcParams.MinProblemValue, bcParams.IgnoreRange);

                    foreach (Buildings.TargetBuildingInfo targetBuilding in TargetBuildings.Values.Where(tb => tb.CheckThis && tb.ProblemValue >= bcParams.MinProblemValue && (tb.HasProblem || !bcParams.OnlyProblematic)).OrderBy(tb => tb, Global.TargetBuildingInfoPriorityComparer))
                    {
                        // Initialize vehicle data.
                        if (!initialized)
                        {
                            initialized = true;

                            CollectVehicleData();
                            if (freeVehicles < 1)
                            {
                                if (Log.LogALot) Log.DevDebug(this, "Dispatch", "BreakCheck");
                                break;
                            }
                        }

                        // Assign vehicles, unless allredy done.
                        if (assignedTargets.ContainsKey(targetBuilding.BuildingId))
                        {
                            targetBuilding.Handled = true;
                        }
                        else
                        {
                            if (AssignVehicle(targetBuilding, bcParams.IgnoreRange))
                            {
                                if (freeVehicles < 1)
                                {
                                    if (Log.LogALot) Log.DevDebug(this, "Dispatch", "BreakCheck");
                                    break;
                                }
                            }
                            else if (bcParams.IgnoreRange)
                            {
                                targetBuilding.CheckThis = false;
                            }
                        }

                        targetBuilding.Checked = true;
                    }

                    if (freeVehicles < 1)
                    {
                        if (Log.LogALot) Log.DevDebug(this, "Dispatch", "BreakChecks");

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Reinitializes the building check parameters.
        /// </summary>
        public void ReInitBuildingChecks()
        {
            switch (this.DispatcherType)
            {
                case DispatcherTypes.HearseDispatcher:
                    buildingChecks = BuldingCheckParameters.GetBuldingCheckParameters(Global.Settings.DeathChecksParameters);
                    break;

                case DispatcherTypes.GarbageTruckDispatcher:
                    buildingChecks = BuldingCheckParameters.GetBuldingCheckParameters(Global.Settings.GarbageChecksParameters);
                    break;

                default:
                    throw new Exception("Bad dispatcher type");
            }
        }

        /// <summary>
        /// Assigns a vehicle to a target building.
        /// </summary>
        /// <param name="targetBuilding">The target building.</param>
        /// <param name="ignoreRange">if set to <c>true</c> ignore the building range.</param>
        /// <returns></returns>
        private bool AssignVehicle(Buildings.TargetBuildingInfo targetBuilding, bool ignoreRange)
        {
            // Get target district.
            DistrictManager districtManager = null;
            byte targetDistrict = 0;
            if (Global.Settings.DispatchByDistrict && districtManager != null)
            {
                districtManager = Singleton<DistrictManager>.instance;
                targetDistrict = districtManager.GetDistrict(targetBuilding.Position);
            }

            // Set target info on service buildings.
            foreach (Buildings.ServiceBuildingInfo serviceBuilding in serviceBuildings.Values.Where(sb => sb.CanReceive && sb.VehiclesFree > 0))
            {
                serviceBuilding.SetTargetInfo(districtManager, targetBuilding, ignoreRange);
            }

            // Loop through service buildings in priority order and assign a vehicle to the target.
            foreach (Buildings.ServiceBuildingInfo serviceBuilding in serviceBuildings.Values.Where(sb => sb.CanReceive && sb.VehiclesFree > 0 && sb.InRange).OrderBy(sb => sb, Global.ServiceBuildingInfoPriorityComparer))
            {
                ushort vehicleFoundId = 0;
                float vehicleFoundDistance = float.PositiveInfinity;

                // Loop through vehicles and save the closest free vehicle.
                foreach (Vehicles.ServiceVehicleInfo vehicleInfo in serviceBuilding.Vehicles.Values.Where(vi => vi.FreeToCollect))
                {
                    float distance = (targetBuilding.Position - vehicleInfo.Position).sqrMagnitude;

                    if (vehicleFoundId == 0 || distance < vehicleFoundDistance)
                    {
                        vehicleFoundId = vehicleInfo.VehicleId;
                        vehicleFoundDistance = distance;
                    }
                }

                if (vehicleFoundId != 0)
                {
                    // A free vehicle was found, assign it to the target.
                    targetBuilding.Handled = true;
                    if (Log.LogToFile)
                    {
                        if (Log.LogALot)
                        {
                            Log.Debug(this, "AssignVehicle", "Assign", "T", targetBuilding.BuildingId, serviceBuilding.BuildingId, vehicleFoundId,
                                        targetBuilding.District,
                                        targetBuilding.HasProblem, targetBuilding.ProblemValue,
                                        targetBuilding.BuildingName,
                                        targetBuilding.BuildingId, targetBuilding.HasProblem ? "HasProblem" : (string)null,
                                        targetBuilding.ProblemValue >= ProblemLimit ? "ProblemLimit" : (string)null,
                                        targetBuilding.ProblemValue >= ProblemLimitMajor ? "ProblemLimitMajor" : (string)null,
                                        targetBuilding.ProblemValue >= ProblemLimitForgotten ? "ProblemLimitForgotten" : (string)null);
                            Log.Debug(this, "AssignVehicle", "Assign", "S", targetBuilding.BuildingId, serviceBuilding.BuildingId, vehicleFoundId,
                                        serviceBuilding.InDistrict, serviceBuilding.InRange,
                                        serviceBuilding.District, serviceBuilding.Range, serviceBuilding.Distance,
                                        serviceBuilding.VehiclesTotal, serviceBuilding.VehiclesUsed, serviceBuilding.VehiclesFree, serviceBuilding.VehiclesSpare,
                                        serviceBuilding.BuildingName,
                                        serviceBuilding.InDistrict ? "InDistrict" : (string)null,
                                        serviceBuilding.InRange ? "InRange" : "OutOfRange");
                            Log.Debug(this, "AssignVehicle", "Assign", "V", targetBuilding.BuildingId, serviceBuilding.BuildingId, vehicleFoundId,
                                        vehicleFoundDistance,
                                        Vehicles.GetVehicleName(vehicleFoundId));
                        }
                        else
                        {
                            Log.Debug(this, "AssignVehicle", "Assign", targetBuilding.BuildingId, serviceBuilding.BuildingId, vehicleFoundId,
                                            targetBuilding.HasProblem, targetBuilding.ProblemValue,
                                            serviceBuilding.InDistrict, serviceBuilding.InRange, serviceBuilding.Range, serviceBuilding.Distance,
                                            vehicleFoundDistance);
                        }
                    }

                    Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

                    vehicles[vehicleFoundId].Info.m_vehicleAI.SetTarget(vehicleFoundId, ref vehicles[vehicleFoundId], targetBuilding.BuildingId);

                    assignedTargets[targetBuilding.BuildingId] = Global.CurrentFrame;
                    serviceBuilding.Vehicles[vehicleFoundId].Target = targetBuilding.BuildingId;
                    serviceBuilding.Vehicles[vehicleFoundId].FreeToCollect = false;

                    freeVehicles--;
                    serviceBuilding.VehiclesFree--;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Collects the vehicle data.
        /// </summary>
        private void CollectVehicleData()
        {
            freeVehicles = 0;

            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            ushort vehicleId;
            bool canCollect;
            bool collecting;
            bool loading;
            bool hasTarget;
            int loadSize, loadMax;

            // Loop through the service buildings.
            foreach (Buildings.ServiceBuildingInfo serviceBuilding in serviceBuildings.Values)
            {
                // Loop through the vehicles.
                serviceBuilding.VehiclesUsed = 0;
                serviceBuilding.VehiclesFree = 0;

                int count = 0;
                vehicleId = serviceBuilding.FirstOwnVehicleId;
                while (vehicleId != 0)
                {
                    if (vehicles[vehicleId].m_transferType == transferType)
                    {
                        serviceBuilding.VehiclesUsed++;

                        // Add or update status for relevant vegicles.
                        if (vehicles[vehicleId].Info != null && (vehicles[vehicleId].m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Spawned)) != Vehicle.Flags.None)
                        {
                            // Check if vehicle is free to dispatch and has free space.
                            vehicles[vehicleId].Info.m_vehicleAI.GetSize(vehicleId, ref vehicles[vehicleId], out loadSize, out loadMax);

                            collecting = (vehicles[vehicleId].m_flags & Vehicle.Flags.TransferToSource) != Vehicle.Flags.None && (vehicles[vehicleId].m_flags & Vehicle.Flags.TransferToTarget) == Vehicle.Flags.None;
                            loading = (vehicles[vehicleId].m_flags & (Vehicle.Flags.Arriving | Vehicle.Flags.Stopped)) != Vehicle.Flags.None && (vehicles[vehicleId].m_flags & Vehicle.Flags.WaitingTarget) == Vehicle.Flags.None;
                            canCollect = collecting && !loading && loadSize < loadMax;
                            hasTarget = vehicles[vehicleId].m_targetBuilding != 0 && !(collecting && vehicles[vehicleId].m_targetBuilding == serviceBuilding.BuildingId && !TargetBuildings.ContainsKey(serviceBuilding.BuildingId));

                            // Update vehicle status.
                            if (serviceBuilding.Vehicles.ContainsKey(vehicleId))
                            {
                                if (collecting && !loading && vehicles[vehicleId].m_targetBuilding != 0 && vehicles[vehicleId].m_targetBuilding != serviceBuilding.Vehicles[vehicleId].Target)
                                {
                                    if (Log.LogALot) Log.DevDebug(this, "CollectVehicles", "WrongTarget", vehicleId, vehicles[vehicleId].m_targetBuilding, serviceBuilding.Vehicles[vehicleId].Target);

                                    vehicles[vehicleId].Info.m_vehicleAI.SetTarget(vehicleId, ref vehicles[vehicleId], DeAssignToSource ? serviceBuilding.BuildingId : (ushort)0);
                                    hasTarget = false;
                                }

                                serviceBuilding.Vehicles[vehicleId].Update(ref vehicles[vehicleId], canCollect && !hasTarget);
                            }
                            else
                            {
                                if (collecting && !loading && vehicles[vehicleId].m_targetBuilding != 0)
                                {
                                    vehicles[vehicleId].Info.m_vehicleAI.SetTarget(vehicleId, ref vehicles[vehicleId], DeAssignToSource ? serviceBuilding.BuildingId : (ushort)0);
                                    hasTarget = false;
                                }

                                Vehicles.ServiceVehicleInfo serviceVehicle = new Vehicles.ServiceVehicleInfo(vehicleId, ref vehicles[vehicleId], canCollect && !hasTarget);
                                if (Log.LogALot) Log.DevDebug(this, "CollectVehicles", "AddVehicle", serviceBuilding.BuildingId, vehicleId, vehicles[vehicleId].Info.name, serviceVehicle.VehicleName, serviceVehicle.FreeToCollect, collecting);

                                serviceBuilding.Vehicles[vehicleId] = serviceVehicle;
                            }

                            // If target doesn't need service, deassign...
                            if (collecting && !loading && vehicles[vehicleId].m_targetBuilding != 0 && vehicles[vehicleId].m_targetBuilding != serviceBuilding.BuildingId && !hasTarget)
                            {
                                if (Log.LogALot) Log.DevDebug(this, "CollectVehicles", "NoNeed", vehicleId, vehicles[vehicleId].m_targetBuilding);

                                vehicles[vehicleId].Info.m_vehicleAI.SetTarget(vehicleId, ref vehicles[vehicleId], DeAssignToSource ? serviceBuilding.BuildingId : (ushort)0);
                                vehicles[vehicleId].m_targetBuilding = DeAssignToSource ? serviceBuilding.BuildingId : (ushort)0;
                            }

                            // Update assigned target status.
                            if (collecting && hasTarget)
                            {
                                if (Log.LogALot && !assignedTargets.ContainsKey(vehicles[vehicleId].m_targetBuilding)) Log.DevDebug(this, "CollectVehicles", "AddAssigned", serviceBuilding.BuildingId, vehicleId, vehicles[vehicleId].m_targetBuilding);

                                assignedTargets[vehicles[vehicleId].m_targetBuilding] = Global.CurrentFrame;
                            }
                            else if (canCollect)
                            {
                                freeVehicles++;
                                serviceBuilding.VehiclesFree++;
                            }
                        }
                    }

                    vehicleId = vehicles[vehicleId].m_nextOwnVehicle;
                    if (vehicleId == serviceBuilding.FirstOwnVehicleId)
                    {
                        break;
                    }

                    count++;
                    if (count >= ushort.MaxValue)
                    {
                        throw new Exception("Loop counter too high!");
                    }
                }

                serviceBuilding.VehiclesSpare = serviceBuilding.VehiclesTotal - serviceBuilding.VehiclesUsed;

                // Remove old vehicles.
                ushort[] removeVehicles = serviceBuilding.Vehicles.Values.Where(v => v.LastSeen != Global.CurrentFrame).Select(v => v.VehicleId).ToArray();
                foreach (ushort id in removeVehicles)
                {
                    if (vehicles[vehicleId].Info == null || (vehicles[vehicleId].m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Spawned)) != (Vehicle.Flags.Created | Vehicle.Flags.Spawned) || vehicles[vehicleId].m_transferType != transferType)
                    {
                        if (Log.LogALot) Log.DevDebug(this, "CollectVehicles", "RemoveNonVehicle", serviceBuilding.BuildingId, id);

                        serviceBuilding.Vehicles.Remove(id);
                    }
                    else
                    {
                        if (vehicles[vehicleId].m_sourceBuilding != serviceBuilding.BuildingId)
                        {
                            if (Log.LogALot) Log.DevDebug(this, "CollectVehicles", "RemoveMovedVehicle", serviceBuilding.BuildingId, id);

                            serviceBuilding.Vehicles.Remove(id);
                        }

                        assignedTargets[vehicles[vehicleId].m_targetBuilding] = Global.CurrentFrame;
                    }
                }
            }

            // Remove old target assigments.
            ushort[] removeTargets = assignedTargets.Where(at => at.Value != Global.CurrentFrame).Select(at => at.Key).ToArray();
            foreach (ushort id in removeTargets)
            {
                if (Log.LogALot) Log.DevDebug(this, "CollectVehicles", "RemoveAssigned", id);

                assignedTargets.Remove(id);
            }
        }

        /// <summary>
        /// Building check parameters.
        /// </summary>
        public class BuldingCheckParameters
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="BuldingCheckParameters"/> struct.
            /// </summary>
            /// <param name="onlyProblematic">if set to <c>true</c> check only problematic buildings.</param>
            /// <param name="ignoreRange">if set to <c>true</c> ignore range.</param>
            /// <param name="minTimer">The minimum problem timer value.</param>
            public BuldingCheckParameters(bool onlyProblematic, bool ignoreRange, byte minTimer)
            {
                this.Setting = Settings.BuildingCheckParameters.Custom;
                this.OnlyProblematic = onlyProblematic;
                this.IgnoreRange = ignoreRange;
                this.MinProblemValue = minTimer;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="BuldingCheckParameters"/> class.
            /// </summary>
            /// <param name="buildingCheckParameters">The building check parameters.</param>
            public BuldingCheckParameters(Settings.BuildingCheckParameters buildingCheckParameters)
            {
                this.Setting = buildingCheckParameters;

                switch (buildingCheckParameters)
                {
                    case Settings.BuildingCheckParameters.Any:
                        this.OnlyProblematic = false;
                        this.IgnoreRange = true;
                        this.MinProblemValue = 0;
                        break;

                    case Settings.BuildingCheckParameters.InRange:
                        this.OnlyProblematic = false;
                        this.IgnoreRange = false;
                        this.MinProblemValue = 0;
                        break;

                    case Settings.BuildingCheckParameters.ProblematicInRange:
                        this.OnlyProblematic = true;
                        this.IgnoreRange = false;
                        this.MinProblemValue = 0;
                        break;

                    case Settings.BuildingCheckParameters.ProblematicIgnoreRange:
                        this.OnlyProblematic = true;
                        this.IgnoreRange = true;
                        this.MinProblemValue = 0;
                        break;

                    case Settings.BuildingCheckParameters.VeryProblematicInRange:
                        this.OnlyProblematic = true;
                        this.IgnoreRange = false;
                        this.MinProblemValue = ProblemLimitMajor;
                        break;

                    case Settings.BuildingCheckParameters.VeryProblematicIgnoreRange:
                        this.OnlyProblematic = true;
                        this.IgnoreRange = true;
                        this.MinProblemValue = ProblemLimitMajor;
                        break;

                    case Settings.BuildingCheckParameters.ForgottenInRange:
                        this.OnlyProblematic = true;
                        this.IgnoreRange = false;
                        this.MinProblemValue = ProblemLimitForgotten;
                        break;

                    case Settings.BuildingCheckParameters.ForgottenIgnoreRange:
                        this.OnlyProblematic = true;
                        this.IgnoreRange = true;
                        this.MinProblemValue = ProblemLimitForgotten;
                        break;

                    default:
                        this.OnlyProblematic = false;
                        this.IgnoreRange = true;
                        this.MinProblemValue = ProblemLimit;
                        break;
                }
            }

            /// <summary>
            /// Gets a value indicating whether to ignore range.
            /// </summary>
            /// <value>
            /// <c>true</c> if the range should be ignored; otherwise, <c>false</c>.
            /// </value>
            public bool IgnoreRange { get; private set; }

            /// <summary>
            /// Gets the minimum problem timer.
            /// </summary>
            /// <value>
            /// The minimum problem timer.
            /// </value>
            public ushort MinProblemValue { get; private set; }

            /// <summary>
            /// Gets a value indicating whether only problematic buildings should be checked.
            /// </summary>
            /// <value>
            ///   <c>true</c> if only problematic buildings should be checked; otherwise, <c>false</c>.
            /// </value>
            public bool OnlyProblematic { get; private set; }

            /// <summary>
            /// Gets the setting.
            /// </summary>
            /// <value>
            /// The setting.
            /// </value>
            public Settings.BuildingCheckParameters Setting { get; private set; }

            /// <summary>
            /// Gets the bulding check parameters.
            /// </summary>
            /// <param name="buildingCheckParameters">The building check parameters.</param>
            /// <returns>
            /// The bulding check parameters.
            /// </returns>
            public static BuldingCheckParameters[] GetBuldingCheckParameters(Settings.BuildingCheckParameters[] buildingCheckParameters)
            {
                return buildingCheckParameters.Select(bcp => new BuldingCheckParameters(bcp)).ToArray();
            }
        }
    }
}