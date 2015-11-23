using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Service vehicle dispatch base class.
    /// </summary>
    internal class Dispatcher
    {
        /// <summary>
        /// The problem buffer modifier.
        /// </summary>
        public static readonly ushort ProblemBufferModifier = 1;

        /// <summary>
        /// The problem limit.
        /// </summary>
        public static readonly ushort ProblemLimit = 3000;

        /// <summary>
        /// The forgotten problem value limit.
        /// </summary>
        public static readonly ushort ProblemLimitForgotten = 12000;

        /// <summary>
        /// The problem major limit.
        /// </summary>
        public static readonly ushort ProblemLimitMajor = 6000;

        /// <summary>
        /// The problem timer modifier.
        /// </summary>
        public static readonly ushort ProblemTimerModifier = 48;

        /// <summary>
        /// The dispatcher type.
        /// </summary>
        public readonly DispatcherTypes DispatcherType;

        /// <summary>
        /// The decent capacity proportion.
        /// </summary>
        private static readonly float CapacityProportionDecent = 0.5f;

        /// <summary>
        /// The ok capacity proportion.
        /// </summary>
        private static readonly float CapacityProportionOk = 1f / 3f;

        /// <summary>
        /// The decent used capacity percentage.
        /// </summary>
        private static readonly float CapacityUsedDecent = 0.75f;

        /// <summary>
        /// The ok used capacity percentage.
        /// </summary>
        private static readonly float CapacityUsedOk = 0.9f;

        /// <summary>
        /// The assigned targets.
        /// </summary>
        private Dictionary<ushort, uint> assignedTargets = new Dictionary<ushort, uint>();

        /// <summary>
        /// The building check parameters.
        /// </summary>
        private BuldingCheckParameters[] buildingChecks;

        /// <summary>
        /// When to create spare vehicles.
        /// </summary>
        private Settings.SpareVehiclesCreation createSpareVehicles = Settings.SpareVehiclesCreation.Never;

        /// <summary>
        /// Dispatch services by district.
        /// </summary>
        private bool dispatchByDistrict = false;

        /// <summary>
        /// The free vehicle count.
        /// </summary>
        private int freeVehicles = 0;

        /// <summary>
        /// The last vehicle detour class check stamp.
        /// </summary>
        private int lastVehicleDetourClassCheck = 0;

        /// <summary>
        /// The service buildings.
        /// </summary>
        private Dictionary<ushort, ServiceBuildingInfo> serviceBuildings;

        /// <summary>
        /// The target buildings.
        /// </summary>
        private Dictionary<ushort, TargetBuildingInfo> targetBuildings;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dispatcher" /> class.
        /// </summary>
        /// <param name="dispatcherType">Type of the dispatcher.</param>
        /// <exception cref="System.Exception">Bad dispatcher type.</exception>
        public Dispatcher(DispatcherTypes dispatcherType)
        {
            this.DispatcherType = dispatcherType;

            this.Initialize(true);

            Log.Debug(this, "Constructed");
        }

        /// <summary>
        /// The dispatcher types.
        /// </summary>
        public enum DispatcherTypes
        {
            /// <summary>
            /// Not a dispatcher.
            /// </summary>
            None = 0,

            /// <summary>
            /// Dispatches hearses.
            /// </summary>
            HearseDispatcher = 1,

            /// <summary>
            /// Dispatches garbage trucks.
            /// </summary>
            GarbageTruckDispatcher = 2
        }

        /// <summary>
        /// Gets or sets the type of the transfer.
        /// </summary>
        /// <value>
        /// The type of the transfer.
        /// </value>
        public byte TransferType
        {
            get;
            protected set;
        }

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
            ServiceBuildingInfo serviceBuilding = this.serviceBuildings.ContainsKey(vehicle.m_sourceBuilding) ? this.serviceBuildings[vehicle.m_sourceBuilding] : null;

            if (serviceBuilding == null)
            {
                if (Log.LogALot)
                {
                    Log.DevDebug(this, "CheckVehicleTarget", "NewBuilding", vehicleId, vehicle.m_targetBuilding);
                }

                vehicle.Info.m_vehicleAI.SetTarget(vehicleId, ref vehicle, 0);
            }
            else if (!serviceBuilding.Vehicles.ContainsKey(vehicleId))
            {
                if (Log.LogALot)
                {
                    Log.DevDebug(this, "CheckVehicleTarget", "NewVehicle", vehicleId, vehicle.m_targetBuilding);
                }

                vehicle.Info.m_vehicleAI.SetTarget(vehicleId, ref vehicle, (ushort)0); // DeAssignToSource ? vehicle.m_sourceBuilding : (ushort)0
            }
            else if (!this.targetBuildings.ContainsKey(vehicle.m_targetBuilding) || this.targetBuildings[vehicle.m_targetBuilding].DontWantService)
            {
                if (Log.LogALot)
                {
                    Log.DevDebug(this, "CheckVehicleTarget", "NoNeed", vehicleId, vehicle.m_targetBuilding);
                }

                vehicle.Info.m_vehicleAI.SetTarget(vehicleId, ref vehicle, (ushort)0); // DeAssignToSource ? vehicle.m_sourceBuilding : (ushort)0
                serviceBuilding.Vehicles[vehicleId].Target = (ushort)0; // DeAssignToSource ? vehicle.m_sourceBuilding : (ushort)0
            }
            else if (vehicle.m_targetBuilding != serviceBuilding.Vehicles[vehicleId].Target)
            {
                if (Log.LogALot)
                {
                    Log.DevDebug(this, "CheckVehicleTarget", "WrongTarget", vehicleId, vehicle.m_targetBuilding, serviceBuilding.Vehicles[vehicleId].Target);
                }

                vehicle.Info.m_vehicleAI.SetTarget(vehicleId, ref vehicle, (ushort)0); // DeAssignToSource ? vehicle.m_sourceBuilding : (ushort)0
                serviceBuilding.Vehicles[vehicleId].Target = (ushort)0; // DeAssignToSource ? vehicle.m_sourceBuilding : (ushort)0
            }
        }

        /// <summary>
        /// Dispatches vehicles to targets.
        /// </summary>
        public virtual void Dispatch()
        {
            // Dispatch, if needed.
            if (this.HasTargetBuildings)
            {
                bool initialized = false;

                Building[] buildings = null;

                foreach (BuldingCheckParameters checkParams in this.buildingChecks)
                {
                    if (Log.LogALot)
                        Log.DevDebug(this, "Dispatch", checkParams.Setting, checkParams.OnlyProblematic, checkParams.MinProblemValue, checkParams.IgnoreRange);

                    foreach (TargetBuildingInfo targetBuilding in this.targetBuildings.Values.Where(tb => checkParams.CheckThis(tb)).OrderBy(tb => tb, Global.TargetBuildingInfoPriorityComparer))
                    {
                        // Initialize vehicle data.
                        if (!initialized)
                        {
                            initialized = true;
                            buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

                            this.CollectVehicleData(buildings);
                            if (this.freeVehicles < 1)
                            {
                                if (Log.LogALot)
                                    Log.DevDebug(this, "Dispatch", "BreakCheck");
                                break;
                            }
                        }

                        // Skip missing buildings.
                        if (buildings[targetBuilding.BuildingId].Info == null || (buildings[targetBuilding.BuildingId].m_flags & Building.Flags.Created) == Building.Flags.None || (buildings[targetBuilding.BuildingId].m_flags & (Building.Flags.Abandoned | Building.Flags.BurnedDown | Building.Flags.Deleted)) != Building.Flags.None)
                        {
                            continue;
                        }

                        // Assign vehicles, unless allredy done.
                        if (this.assignedTargets.ContainsKey(targetBuilding.BuildingId))
                        {
                            targetBuilding.Handled = true;
                        }
                        else
                        {
                            if (this.AssignVehicle(targetBuilding, checkParams.IgnoreRange, checkParams.AllowCreateSpares))
                            {
                                if (this.freeVehicles < 1)
                                {
                                    if (Log.LogALot)
                                        Log.DevDebug(this, "Dispatch", "BreakCheck");
                                    break;
                                }
                            }
                            else if (checkParams.IgnoreRange)
                            {
                                targetBuilding.CheckThis = false;
                            }
                        }

                        targetBuilding.Checked = true;
                    }

                    if (this.freeVehicles < 1)
                    {
                        if (Log.LogALot)
                            Log.DevDebug(this, "Dispatch", "BreakChecks");

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Reinitializes the building check parameters.
        /// </summary>
        public void ReInitialize()
        {
            this.Initialize(false);
        }

        /// <summary>
        /// Assigns a vehicle to a target building.
        /// </summary>
        /// <param name="targetBuilding">The target building.</param>
        /// <param name="ignoreRange">If set to <c>true</c> ignore the building range.</param>
        /// <param name="allowCreateSpares">If set to <c>true</c> allow creation of spare vehicles.</param>
        /// <returns>
        /// True if vehicle was assigned.
        /// </returns>
        private bool AssignVehicle(TargetBuildingInfo targetBuilding, bool ignoreRange, bool allowCreateSpares)
        {
            // Get target district.
            byte targetDistrict = 0;
            if (this.dispatchByDistrict)
            {
                targetDistrict = Singleton<DistrictManager>.instance.GetDistrict(targetBuilding.Position);
            }

            // Set target info on service buildings.
            foreach (ServiceBuildingInfo serviceBuilding in this.serviceBuildings.Values.Where(sb => sb.CanReceive && sb.VehiclesFree > 0))
            {
                serviceBuilding.SetTargetInfo(targetBuilding, ignoreRange);
            }

            // Found vehicle that has enough free capacity.
            ushort foundVehicleId = 0;
            float foundVehicleDistance = float.PositiveInfinity;
            ServiceBuildingInfo foundVehicleBuilding = null;

            // Found vehicle with decent free capacity.
            ushort foundVehicleDecentId = 0;
            float foundVehicleDecentDistance = float.PositiveInfinity;

            // Found vehicle with ok free capacity.
            ushort foundVehicleOkId = 0;
            float foundVehicleOkDistance = float.PositiveInfinity;
            ServiceBuildingInfo foundVehicleOkBuilding = null;
            bool foundVehicleOkCheck;

            // Found vehicle with any free capacity.
            ushort foundVehicleLastResortId = 0;
            float foundVehicleLastResortDistance = float.PositiveInfinity;
            ServiceBuildingInfo foundVehicleLastResortBuilding = null;
            bool foundVehicleLastResortCheck;

            // Loop through service buildings in priority order and assign a vehicle to the target.
            foreach (ServiceBuildingInfo serviceBuilding in this.serviceBuildings.Values.Where(sb => sb.CanReceive && sb.VehiclesFree > 0 && sb.InRange).OrderBy(sb => sb, Global.ServiceBuildingInfoPriorityComparer))
            {
                // Found vehicle that has enough free capacity.
                foundVehicleId = 0;
                foundVehicleDistance = float.PositiveInfinity;

                // Found vehicle with decent free capacity.
                foundVehicleDecentId = 0;
                foundVehicleDecentDistance = float.PositiveInfinity;

                // Whether to check for vehicles with not so decent free capacity.
                foundVehicleOkCheck = foundVehicleOkId == 0;
                foundVehicleLastResortCheck = foundVehicleLastResortId == 0;

                // If prefer to send new vehcile when building is closer.
                if (allowCreateSpares && this.createSpareVehicles == Settings.SpareVehiclesCreation.WhenBuildingIsCloser && serviceBuilding.VehiclesSpare > 0)
                {
                    foundVehicleDistance = serviceBuilding.Distance;
                }

                // Loop through vehicles and save the closest free vehicle.
                foreach (ServiceVehicleInfo vehicleInfo in serviceBuilding.Vehicles.Values.Where(vi => vi.FreeToCollect))
                {
                    float distance = (targetBuilding.Position - vehicleInfo.Position).sqrMagnitude;

                    // Check for vehicle with enough free capacity.
                    if (distance < foundVehicleDistance && vehicleInfo.CapacityFree >= targetBuilding.ProblemSize)
                    {
                        foundVehicleId = vehicleInfo.VehicleId;
                        foundVehicleDistance = distance;
                    }

                    // Check for vehicle with decent free capacity.
                    if (distance < foundVehicleDecentDistance && (vehicleInfo.CapacityUsed < CapacityUsedDecent || (float)vehicleInfo.CapacityFree >= (float)targetBuilding.ProblemSize * CapacityProportionDecent))
                    {
                        foundVehicleDecentId = vehicleInfo.VehicleId;
                        foundVehicleDecentDistance = distance;
                    }

                    // Check for vehicle with ok free capacity.
                    if (foundVehicleOkCheck && distance < foundVehicleOkDistance && (vehicleInfo.CapacityUsed < CapacityUsedOk || (float)vehicleInfo.CapacityFree >= (float)targetBuilding.ProblemSize * CapacityProportionOk))
                    {
                        foundVehicleOkId = vehicleInfo.VehicleId;
                        foundVehicleOkDistance = distance;
                        foundVehicleOkBuilding = serviceBuilding;
                    }

                    // Check for vehicle with any free capacity.
                    if (foundVehicleLastResortCheck && distance < foundVehicleLastResortDistance && vehicleInfo.CapacityFree > 0)
                    {
                        foundVehicleLastResortId = vehicleInfo.VehicleId;
                        foundVehicleLastResortDistance = distance;
                        foundVehicleLastResortBuilding = serviceBuilding;
                    }
                }

                // If no vehicle with enough free capacity (including the spare vehicles in the building), use the closest with decent free capacity instead.
                if (foundVehicleDistance == float.PositiveInfinity && foundVehicleDecentDistance < float.PositiveInfinity)
                {
                    foundVehicleId = foundVehicleDecentId;
                    foundVehicleDistance = foundVehicleDecentDistance;

                    Log.Debug(this, "AssignVehicle", "UsingDecent", targetBuilding.BuildingId, serviceBuilding.BuildingId, serviceBuilding.VehiclesSpare, foundVehicleId, foundVehicleDistance);
                }

                // No free vehicle found, but building has spare vehicles so we send one of those.
                if (foundVehicleId == 0 && allowCreateSpares && this.createSpareVehicles != Settings.SpareVehiclesCreation.Never && serviceBuilding.VehiclesSpare > 0)
                {
                    Log.Debug(this, "AssignVehicle", "CreateSpare", targetBuilding.BuildingId, serviceBuilding.BuildingId, serviceBuilding.VehiclesSpare);

                    foundVehicleId = serviceBuilding.CreateVehicle(this.TransferType);
                    if (foundVehicleId == 0)
                    {
                        Log.Debug(this, "AssignVehicle", "SpareNotCreated", targetBuilding.BuildingId, serviceBuilding.BuildingId);
                    }
                    else
                    {
                        this.freeVehicles++;
                        foundVehicleDistance = serviceBuilding.Distance;
                        if (Log.LogALot)
                        {
                            Log.Debug(
                                this,
                                "AssignVehicle",
                                "Assign",
                                "C",
                                targetBuilding.BuildingId,
                                serviceBuilding.BuildingId,
                                foundVehicleId,
                                serviceBuilding.VehiclesSpare,
                                serviceBuilding.VehiclesFree,
                                serviceBuilding.VehiclesTotal,
                                targetBuilding.BuildingName,
                                serviceBuilding.BuildingName,
                                VehicleHelper.GetVehicleName(foundVehicleId));
                        }
                    }
                }

                if (foundVehicleId != 0)
                {
                    foundVehicleBuilding = serviceBuilding;
                    break;
                }
            }

            // If no vehicle with decent free capacity (including the spare vehicles in the buildings), use the closest with ok free capacity instead.
            if (foundVehicleDistance == float.PositiveInfinity && foundVehicleOkDistance < float.PositiveInfinity)
            {
                foundVehicleId = foundVehicleDecentId;
                foundVehicleDistance = foundVehicleDecentDistance;
                foundVehicleBuilding = foundVehicleOkBuilding;

                Log.Debug(this, "AssignVehicle", "UsingOk", targetBuilding.BuildingId, foundVehicleBuilding.BuildingId, foundVehicleBuilding.VehiclesSpare, foundVehicleId, foundVehicleDistance);
            }

            // If no vehicle with ok free capacity (including the spare vehicles in the buildings), use the closest with any free capacity instead.
            if (foundVehicleDistance == float.PositiveInfinity && foundVehicleLastResortDistance < float.PositiveInfinity)
            {
                foundVehicleId = foundVehicleLastResortId;
                foundVehicleDistance = foundVehicleLastResortDistance;
                foundVehicleBuilding = foundVehicleLastResortBuilding;

                Log.Debug(this, "AssignVehicle", "UsingLastResort", targetBuilding.BuildingId, foundVehicleBuilding.BuildingId, foundVehicleBuilding.VehiclesSpare, foundVehicleId, foundVehicleDistance);
            }

            // No free vehicle was found, return.
            if (foundVehicleId == 0)
            {
                return false;
            }

            // A free vehicle was found, assign it to the target.
            targetBuilding.Handled = true;
            if (Log.LogToFile)
            {
                if (Log.LogALot)
                {
                    Log.Debug(
                        this,
                        "AssignVehicle",
                        "Assign",
                        "T",
                        targetBuilding.BuildingId,
                        foundVehicleBuilding.BuildingId,
                        foundVehicleId,
                        targetBuilding.District,
                        targetBuilding.HasProblem,
                        targetBuilding.ProblemValue,
                        targetBuilding.ProblemSize,
                        targetBuilding.BuildingName,
                        targetBuilding.DistrictName,
                        targetBuilding.HasProblem ? "HasProblem" : (string)null,
                        targetBuilding.ProblemValue >= ProblemLimit ? "ProblemLimit" : (string)null,
                        targetBuilding.ProblemValue >= ProblemLimitMajor ? "ProblemLimitMajor" : (string)null,
                        targetBuilding.ProblemValue >= ProblemLimitForgotten ? "ProblemLimitForgotten" : (string)null);
                    Log.Debug(
                        this,
                        "AssignVehicle",
                        "Assign",
                        "S",
                        targetBuilding.BuildingId,
                        foundVehicleBuilding.BuildingId,
                        foundVehicleId,
                        foundVehicleBuilding.District,
                        foundVehicleBuilding.InDistrict,
                        foundVehicleBuilding.InRange,
                        foundVehicleBuilding.Range,
                        foundVehicleBuilding.Distance,
                        foundVehicleBuilding.VehiclesTotal,
                        foundVehicleBuilding.VehiclesMade,
                        foundVehicleBuilding.VehiclesFree,
                        foundVehicleBuilding.VehiclesSpare,
                        foundVehicleBuilding.BuildingName,
                        foundVehicleBuilding.DistrictName,
                        foundVehicleBuilding.InDistrict ? "InDistrict" : (string)null,
                        foundVehicleBuilding.InRange ? "InRange" : "OutOfRange");
                    Log.Debug(
                        this,
                        "AssignVehicle",
                        "Assign",
                        "V",
                        targetBuilding.BuildingId,
                        foundVehicleBuilding.BuildingId,
                        foundVehicleId,
                        foundVehicleDistance,
                        foundVehicleBuilding.Vehicles[foundVehicleId].CapacityUsed,
                        foundVehicleBuilding.Vehicles[foundVehicleId].CapacityFree,
                        VehicleHelper.GetVehicleName(foundVehicleId));
                }
                else if (Log.LogNames)
                {
                    Log.Debug(
                        this,
                        "AssignVehicle",
                        "Assign",
                        targetBuilding.BuildingId,
                        foundVehicleBuilding.BuildingId,
                        foundVehicleId,
                        targetBuilding.HasProblem,
                        targetBuilding.ProblemValue,
                        foundVehicleBuilding.InDistrict,
                        foundVehicleBuilding.InRange,
                        foundVehicleBuilding.Range,
                        foundVehicleBuilding.Distance,
                        foundVehicleBuilding.Distance,
                        targetBuilding.BuildingName,
                        targetBuilding.DistrictName,
                        foundVehicleBuilding.BuildingName,
                        foundVehicleBuilding.DistrictName,
                        VehicleHelper.GetVehicleName(foundVehicleId));
                }
                else
                {
                    Log.Debug(
                        this,
                        "AssignVehicle",
                        "Assign",
                        targetBuilding.BuildingId,
                        foundVehicleBuilding.BuildingId,
                        foundVehicleId,
                        targetBuilding.HasProblem,
                        targetBuilding.ProblemValue,
                        foundVehicleBuilding.InDistrict,
                        foundVehicleBuilding.InRange,
                        foundVehicleBuilding.Range,
                        foundVehicleBuilding.Distance,
                        foundVehicleDistance);
                }
            }

            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            vehicles[foundVehicleId].Info.m_vehicleAI.SetTarget(foundVehicleId, ref vehicles[foundVehicleId], targetBuilding.BuildingId);

            this.assignedTargets[targetBuilding.BuildingId] = Global.CurrentFrame;
            foundVehicleBuilding.Vehicles[foundVehicleId].Target = targetBuilding.BuildingId;
            foundVehicleBuilding.Vehicles[foundVehicleId].FreeToCollect = false;

            this.freeVehicles--;
            foundVehicleBuilding.VehiclesFree--;

            return true;
        }

        /// <summary>
        /// Collects the vehicle data.
        /// </summary>
        /// <param name="buildings">The CS buildings.</param>
        /// <exception cref="System.Exception">Loop counter too high.</exception>
        private void CollectVehicleData(Building[] buildings)
        {
            this.freeVehicles = 0;

            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            ushort vehicleId;
            bool canCollect;
            bool collecting;
            bool loading;
            bool hasTarget;
            int loadSize, loadMax;
            int vehiclesMade;
            int vehiclesFree;
            int count;

            HashSet<Type> vehicleAIs = null;
            if (this.DispatcherType == DispatcherTypes.GarbageTruckDispatcher && (this.lastVehicleDetourClassCheck == 0 || Global.CurrentFrame - this.lastVehicleDetourClassCheck > Global.ClassCheckInterval))
            {
                vehicleAIs = new HashSet<Type>();
            }

            // Loop through the service buildings.
            foreach (ServiceBuildingInfo serviceBuilding in this.serviceBuildings.Values)
            {
                // Skip missing buildings.
                if (buildings[serviceBuilding.BuildingId].Info == null || (buildings[serviceBuilding.BuildingId].m_flags & Building.Flags.Created) == Building.Flags.None || (buildings[serviceBuilding.BuildingId].m_flags & (Building.Flags.Abandoned | Building.Flags.BurnedDown | Building.Flags.Deleted)) != Building.Flags.None)
                {
                    serviceBuilding.CanReceive = false;
                    continue;
                }

                // Loop through the vehicles.
                vehiclesMade = 0;
                vehiclesFree = 0;

                count = 0;
                serviceBuilding.FirstOwnVehicleId = buildings[serviceBuilding.BuildingId].m_ownVehicles;
                vehicleId = serviceBuilding.FirstOwnVehicleId;
                while (vehicleId != 0)
                {
                    if (count >= ushort.MaxValue)
                    {
                        throw new Exception("Loop counter too high");
                    }
                    count++;

                    if (vehicles[vehicleId].m_transferType == this.TransferType)
                    {
                        // Add or update status for relevant vehicles.
                        if (vehicles[vehicleId].Info != null && (vehicles[vehicleId].m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Spawned)) != Vehicle.Flags.None)
                        {
                            vehiclesMade++;

                            if (vehicleAIs != null)
                            {
                                vehicleAIs.Add(vehicles[vehicleId].Info.m_vehicleAI.GetType());
                            }

                            // Check if vehicle is free to dispatch and has free space.
                            vehicles[vehicleId].Info.m_vehicleAI.GetSize(vehicleId, ref vehicles[vehicleId], out loadSize, out loadMax);

                            collecting = (vehicles[vehicleId].m_flags & Vehicle.Flags.TransferToSource) != Vehicle.Flags.None && (vehicles[vehicleId].m_flags & Vehicle.Flags.TransferToTarget) == Vehicle.Flags.None;
                            loading = (vehicles[vehicleId].m_flags & (Vehicle.Flags.Arriving | Vehicle.Flags.Stopped)) != Vehicle.Flags.None && (vehicles[vehicleId].m_flags & Vehicle.Flags.WaitingTarget) == Vehicle.Flags.None;
                            canCollect = collecting && !loading && loadSize < loadMax;
                            hasTarget = vehicles[vehicleId].m_targetBuilding != 0 && !(collecting && vehicles[vehicleId].m_targetBuilding == serviceBuilding.BuildingId && !this.targetBuildings.ContainsKey(serviceBuilding.BuildingId));

                            // Update vehicle status.
                            if (serviceBuilding.Vehicles.ContainsKey(vehicleId))
                            {
                                bool busy = false;

                                if (collecting && !loading && vehicles[vehicleId].m_targetBuilding != 0)
                                {
                                    if (!this.targetBuildings.ContainsKey(vehicles[vehicleId].m_targetBuilding) || this.targetBuildings[vehicles[vehicleId].m_targetBuilding].DontWantService)
                                    {
                                        if (Log.LogALot)
                                        {
                                            Log.DevDebug(this, "CollectVehicles", "NoNeed", vehicleId, vehicles[vehicleId].m_targetBuilding, serviceBuilding.Vehicles[vehicleId].Target);
                                        }

                                        vehicles[vehicleId].Info.m_vehicleAI.SetTarget(vehicleId, ref vehicles[vehicleId], (ushort)0); // DeAssignToSource ? serviceBuilding.BuildingId : (ushort)0)
                                        hasTarget = false;
                                    }
                                    else if (vehicles[vehicleId].m_targetBuilding != serviceBuilding.Vehicles[vehicleId].Target)
                                    {
                                        if (Log.LogALot)
                                        {
                                            Log.DevDebug(this, "CollectVehicles", "WrongTarget", vehicleId, vehicles[vehicleId].m_targetBuilding, serviceBuilding.Vehicles[vehicleId].Target);
                                        }

                                        vehicles[vehicleId].Info.m_vehicleAI.SetTarget(vehicleId, ref vehicles[vehicleId], (ushort)0); // DeAssignToSource ? serviceBuilding.BuildingId : (ushort)0)
                                        hasTarget = false;
                                    }
                                    else if (this.targetBuildings[vehicles[vehicleId].m_targetBuilding].NeedsService)
                                    {
                                        busy = true;
                                    }
                                }

                                serviceBuilding.Vehicles[vehicleId].Update(ref vehicles[vehicleId], canCollect && !hasTarget && !busy);
                            }
                            else
                            {
                                if (collecting && !loading && vehicles[vehicleId].m_targetBuilding != 0)
                                {
                                    // DeAssignToSource ? serviceBuilding.BuildingId : (ushort)0
                                    vehicles[vehicleId].Info.m_vehicleAI.SetTarget(vehicleId, ref vehicles[vehicleId], (ushort)0); // DeAssignToSource ? serviceBuilding.BuildingId : (ushort)0)
                                    hasTarget = false;
                                }

                                ServiceVehicleInfo serviceVehicle = new ServiceVehicleInfo(vehicleId, ref vehicles[vehicleId], canCollect && !hasTarget);
                                if (Log.LogALot)
                                {
                                    Log.DevDebug(this, "CollectVehicles", "AddVehicle", serviceBuilding.BuildingId, vehicleId, vehicles[vehicleId].Info.name, serviceVehicle.VehicleName, serviceVehicle.FreeToCollect, collecting);
                                }

                                serviceBuilding.Vehicles[vehicleId] = serviceVehicle;
                            }

                            // If target doesn't need service, deassign...
                            if (collecting && !loading && vehicles[vehicleId].m_targetBuilding != 0 && vehicles[vehicleId].m_targetBuilding != serviceBuilding.BuildingId && !hasTarget)
                            {
                                if (Log.LogALot)
                                {
                                    Log.DevDebug(this, "CollectVehicles", "NoNeed", vehicleId, vehicles[vehicleId].m_targetBuilding);
                                }

                                vehicles[vehicleId].Info.m_vehicleAI.SetTarget(vehicleId, ref vehicles[vehicleId], (ushort)0); // DeAssignToSource ? serviceBuilding.BuildingId : (ushort)0)
                                vehicles[vehicleId].m_targetBuilding = (ushort)0; // DeAssignToSource ? serviceBuilding.BuildingId : (ushort)0)
                            }

                            // Update assigned target status.
                            if (collecting && hasTarget)
                            {
                                if (Log.LogALot && !this.assignedTargets.ContainsKey(vehicles[vehicleId].m_targetBuilding))
                                {
                                    Log.DevDebug(this, "CollectVehicles", "AddAssigned", serviceBuilding.BuildingId, vehicleId, vehicles[vehicleId].m_targetBuilding);
                                }

                                this.assignedTargets[vehicles[vehicleId].m_targetBuilding] = Global.CurrentFrame;
                            }
                            else if (canCollect)
                            {
                                this.freeVehicles++;
                                vehiclesFree++;
                            }
                        }
                    }

                    vehicleId = vehicles[vehicleId].m_nextOwnVehicle;
                    if (vehicleId == serviceBuilding.FirstOwnVehicleId)
                    {
                        break;
                    }
                }

                // Set counts in service building.
                serviceBuilding.VehiclesMade = vehiclesMade;
                serviceBuilding.VehiclesFree = vehiclesFree;

                // Remove old vehicles.
                ushort[] removeVehicles = serviceBuilding.Vehicles.Values.Where(v => v.LastSeen != Global.CurrentFrame).Select(v => v.VehicleId).ToArray();
                foreach (ushort id in removeVehicles)
                {
                    if (vehicles[vehicleId].Info == null || (vehicles[vehicleId].m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Spawned)) != (Vehicle.Flags.Created | Vehicle.Flags.Spawned) || vehicles[vehicleId].m_transferType != this.TransferType)
                    {
                        if (Log.LogALot)
                        {
                            Log.DevDebug(this, "CollectVehicles", "RemoveNonVehicle", serviceBuilding.BuildingId, id);
                        }

                        serviceBuilding.Vehicles.Remove(id);
                    }
                    else
                    {
                        if (vehicles[vehicleId].m_sourceBuilding != serviceBuilding.BuildingId)
                        {
                            if (Log.LogALot)
                            {
                                Log.DevDebug(this, "CollectVehicles", "RemoveMovedVehicle", serviceBuilding.BuildingId, id);
                            }

                            serviceBuilding.Vehicles.Remove(id);
                        }

                        this.assignedTargets[vehicles[vehicleId].m_targetBuilding] = Global.CurrentFrame;
                    }
                }
            }

            // Remove old target assigments.
            ushort[] removeTargets = this.assignedTargets.Where(at => at.Value != Global.CurrentFrame).Select(at => at.Key).ToArray();
            foreach (ushort id in removeTargets)
            {
                if (Log.LogALot)
                {
                    Log.DevDebug(this, "CollectVehicles", "RemoveAssigned", id);
                }

                this.assignedTargets.Remove(id);
            }

            if (vehicleAIs != null && this.DispatcherType == DispatcherTypes.GarbageTruckDispatcher)
            {
                foreach (Type classType in vehicleAIs)
                {
                    Global.AddGarbageTruckClass(classType);
                }
            }
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="constructing">If set to <c>true</c> create all objects.</param>
        /// <exception cref="System.Exception">Bad dispatcher type.</exception>
        private void Initialize(bool constructing)
        {
            switch (this.DispatcherType)
            {
                case DispatcherTypes.HearseDispatcher:
                    if (constructing)
                    {
                        this.TransferType = (byte)TransferManager.TransferReason.Dead;
                        this.serviceBuildings = Global.Buildings.HearseBuildings;
                        this.targetBuildings = Global.Buildings.DeadPeopleBuildings;
                    }

                    this.buildingChecks = BuldingCheckParameters.GetBuldingCheckParameters(Global.Settings.DeathChecksParameters);
                    this.createSpareVehicles = Global.Settings.CreateSpareHearses;
                    this.dispatchByDistrict = Global.Settings.DispatchHearsesByDistrict;

                    break;

                case DispatcherTypes.GarbageTruckDispatcher:
                    if (constructing)
                    {
                        this.TransferType = (byte)TransferManager.TransferReason.Garbage;
                        this.serviceBuildings = Global.Buildings.GarbageBuildings;
                        this.targetBuildings = Global.Buildings.DirtyBuildings;
                    }

                    if (Global.Settings.MinimumGarbageForPatrol > 0 && Global.Settings.MinimumGarbageForPatrol < Global.Settings.MinimumGarbageForDispatch)
                    {
                        this.buildingChecks = BuldingCheckParameters.GetBuldingCheckParametersWithPatrol(Global.Settings.GarbageChecksParameters);
                    }
                    else
                    {
                        this.buildingChecks = BuldingCheckParameters.GetBuldingCheckParameters(Global.Settings.GarbageChecksParameters);
                    }

                    this.createSpareVehicles = Global.Settings.CreateSpareGarbageTrucks;
                    this.dispatchByDistrict = Global.Settings.DispatchGarbageTrucksByDistrict;

                    break;

                default:
                    throw new Exception("Bad dispatcher type");
            }
        }

        /// <summary>
        /// Building check parameters.
        /// </summary>
        public class BuldingCheckParameters
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="BuldingCheckParameters" /> class.
            /// </summary>
            /// <param name="onlyProblematic">If set to <c>true</c> check only problematic buildings.</param>
            /// <param name="includeUneedy">If set to <c>true</c> include buildings with wants but no needs.</param>
            /// <param name="ignoreRange">If set to <c>true</c> ignore range.</param>
            /// <param name="minProblemValue">The minimum problem timer value.</param>
            /// <param name="allowCreateSpares">If set to <c>true</c> allow creation of spare vehicles.</param>
            public BuldingCheckParameters(bool onlyProblematic, bool includeUneedy, bool ignoreRange, byte minProblemValue, bool allowCreateSpares)
            {
                this.Setting = Settings.BuildingCheckParameters.Custom;
                this.OnlyProblematic = onlyProblematic;
                this.IncludeUneedy = includeUneedy;
                this.IgnoreRange = ignoreRange;
                this.MinProblemValue = minProblemValue;
                this.AllowCreateSpares = allowCreateSpares;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="BuldingCheckParameters"/> class.
            /// </summary>
            /// <param name="buildingCheckParameters">The building check parameters.</param>
            public BuldingCheckParameters(Settings.BuildingCheckParameters buildingCheckParameters)
            {
                this.Setting = buildingCheckParameters;

                this.IncludeUneedy = false;
                this.AllowCreateSpares = true;

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
            /// Gets a value indicating whether to allow creation of spare vehicles.
            /// </summary>
            /// <value>
            ///   <c>true</c> if creation of spare vehicles is allowed; otherwise, <c>false</c>.
            /// </value>
            public bool AllowCreateSpares
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets a value indicating whether to ignore range.
            /// </summary>
            /// <value>
            /// <c>true</c> if the range should be ignored; otherwise, <c>false</c>.
            /// </value>
            public bool IgnoreRange
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets a value indicating whether buildings that wants, but does not need, service should be checked.
            /// </summary>
            /// <value>
            ///   <c>true</c> if buildings with wants, but not needs, should be checked; otherwise, <c>false</c>.
            /// </value>
            public bool IncludeUneedy
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the minimum problem timer.
            /// </summary>
            /// <value>
            /// The minimum problem timer.
            /// </value>
            public ushort MinProblemValue
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets a value indicating whether only problematic buildings should be checked.
            /// </summary>
            /// <value>
            ///   <c>true</c> if only problematic buildings should be checked; otherwise, <c>false</c>.
            /// </value>
            public bool OnlyProblematic
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the setting.
            /// </summary>
            /// <value>
            /// The setting.
            /// </value>
            public Settings.BuildingCheckParameters Setting
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the dispatcher building check parameters.
            /// </summary>
            /// <param name="buildingCheckParameters">The configured building check parameters.</param>
            /// <returns>
            /// The dispatcher building check parameters.
            /// </returns>
            public static BuldingCheckParameters[] GetBuldingCheckParameters(Settings.BuildingCheckParameters[] buildingCheckParameters)
            {
                return buildingCheckParameters.Select(bcp => new BuldingCheckParameters(bcp)).ToArray();
            }

            /// <summary>
            /// Gets the dispatcher building check parameters with an extra entry for patrolling vehicles.
            /// </summary>
            /// <param name="buildingCheckParameters">The building check parameters.</param>
            /// <returns>
            /// The dispatcher building check parameters.
            /// </returns>
            public static BuldingCheckParameters[] GetBuldingCheckParametersWithPatrol(Settings.BuildingCheckParameters[] buildingCheckParameters)
            {
                List<BuldingCheckParameters> parameters = buildingCheckParameters.Select(bcp => new BuldingCheckParameters(bcp)).ToList();
                parameters.Add(new BuldingCheckParameters(false, true, false, 0, false));

                return parameters.ToArray();
            }

            /// <summary>
            /// Check whether this building should be checked.
            /// </summary>
            /// <param name="building">The building.</param>
            /// <returns>True if the building should be checked.</returns>
            public bool CheckThis(TargetBuildingInfo building)
            {
                return building.CheckThis && (building.NeedsService || (this.IncludeUneedy && building.WantsService)) && building.ProblemValue >= this.MinProblemValue && (building.HasProblem || !this.OnlyProblematic);
            }
        }
    }
}