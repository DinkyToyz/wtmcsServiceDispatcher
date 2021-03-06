﻿using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Service vehicle dispatch base class.
    /// </summary>
    internal class Dispatcher : IHandlerPart
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
        /// The service settings.
        /// </summary>
        private StandardServiceSettings serviceSettings = null;

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
            GarbageTruckDispatcher = 2,

            /// <summary>
            /// Dispatches ambulances.
            /// </summary>
            AmbulanceDispatcher = 3
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
                        return Global.Buildings.DeathCare.HasBuildingsToCheck;

                    case DispatcherTypes.GarbageTruckDispatcher:
                        return Global.Buildings.Garbage.HasBuildingsToCheck;

                    case DispatcherTypes.AmbulanceDispatcher:
                        return Global.Buildings.HealthCare.HasBuildingsToCheck;

                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Gets the type of the dispatcher.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>The type of the dispatcher.</returns>
        public static DispatcherTypes GetDispatcherType(ref Vehicle vehicle)
        {
            if (vehicle.Info == null || vehicle.Info.m_vehicleAI == null)
            {
                return Dispatcher.DispatcherTypes.None;
            }
            else if (vehicle.Info.m_vehicleAI is HearseAI)
            {
                return Dispatcher.DispatcherTypes.HearseDispatcher;
            }
            else if (vehicle.Info.m_vehicleAI is GarbageTruckAI)
            {
                return Dispatcher.DispatcherTypes.GarbageTruckDispatcher;
            }
            else if (vehicle.Info.m_vehicleAI is AmbulanceAI)
            {
                return Dispatcher.DispatcherTypes.AmbulanceDispatcher;
            }
            else
            {
                return Dispatcher.DispatcherTypes.None;
            }
        }

        /// <summary>
        /// Checks the vehicle target.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        public void CheckVehicleTarget(ushort vehicleId, ref Vehicle vehicle)
        {
            ServiceBuildingInfo serviceBuilding;
            ServiceVehicleInfo serviceVehicle;

            if (this.serviceBuildings.TryGetValue(vehicle.m_sourceBuilding, out serviceBuilding))
            {
                if (!serviceBuilding.Vehicles.TryGetValue(vehicleId, out serviceVehicle))
                {
                    serviceVehicle = null;
                }
            }
            else
            {
                serviceBuilding = null;
                serviceVehicle = null;
            }

            if (serviceBuilding == null)
            {
                if (vehicle.m_targetBuilding != 0 && (vehicle.m_flags & (VehicleHelper.VehicleUnavailable | VehicleHelper.VehicleBusy)) == ~VehicleHelper.VehicleAll)
                {
                    if (Log.LogALot)
                    {
                        Log.DevDebug(this, "CheckVehicleTarget", "NewBuilding", vehicleId, vehicle.m_targetBuilding);
                    }

                    Global.TransferOffersCleaningNeeded = true;
                    VehicleHelper.DeAssign(vehicleId, ref vehicle);
                }
            }
            else if (serviceVehicle == null)
            {
                if (vehicle.m_targetBuilding != 0 && (vehicle.m_flags & (VehicleHelper.VehicleUnavailable | VehicleHelper.VehicleBusy)) == ~VehicleHelper.VehicleAll)
                {
                    if (Log.LogALot)
                    {
                        Log.DevDebug(this, "CheckVehicleTarget", "NewVehicle", vehicleId, vehicle.m_targetBuilding);
                    }

                    Global.TransferOffersCleaningNeeded = true;
                    VehicleHelper.DeAssign(vehicleId, ref vehicle);
                }
            }
            else
            {
                VehicleResult vehicleResult = true;

                if (vehicle.m_targetBuilding != 0)
                {
                    if ((vehicle.m_flags & (VehicleHelper.VehicleUnavailable | VehicleHelper.VehicleBusy)) == ~VehicleHelper.VehicleAll)
                    {
                        if (vehicle.m_targetBuilding != serviceVehicle.Target)
                        {
                            Global.TransferOffersCleaningNeeded = true;
                            vehicleResult = serviceVehicle.DeAssign(ref vehicle, false, this, "CheckVehicleTarget", "WrongTarget");
                        }
                        else
                        {
                            TargetBuildingInfo targetBuilding;
                            if (!this.targetBuildings.TryGetValue(vehicle.m_targetBuilding, out targetBuilding) || !targetBuilding.WantedService)
                            {
                                Global.TransferOffersCleaningNeeded = true;
                                vehicleResult = serviceVehicle.DeAssign(ref vehicle, false, this, "CheckVehicleTarget", "NoNeed");
                            }
                        }
                    }
                }

                if (!vehicleResult.DeSpawned)
                {
                    vehicleResult = serviceVehicle.Update(ref vehicle, vehicle.m_targetBuilding == 0, true, false);

                    if (vehicleResult.DeAssigned)
                    {
                        Global.TransferOffersCleaningNeeded = true;
                    }
                }
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
                Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

                bool[] includeUneedies;
                if (this.serviceSettings.Patrol)
                {
                    includeUneedies = new bool[] { false, true };
                }
                else
                {
                    includeUneedies = new bool[] { false };
                }

                // Get buildings to check.
                TargetBuildingInfo[] checkBuildings = this.targetBuildings.Values.WhereToArray(tb => tb.CheckThis && !tb.HandledNow);

                if (checkBuildings.Length > 1)
                {
                    Array.Sort(checkBuildings, Global.TargetBuildingInfoPriorityComparer);
                }

                foreach (BuldingCheckParameters checkParams in this.buildingChecks)
                {
                    foreach (TargetBuildingInfo targetBuilding in checkBuildings.Where(tb => checkParams.CheckThis(tb)))
                    {
                        // Skip missing buildings.
                        if (buildings[targetBuilding.BuildingId].Info == null || (buildings[targetBuilding.BuildingId].m_flags & Building.Flags.Created) == Building.Flags.None || (buildings[targetBuilding.BuildingId].m_flags & (Building.Flags.Abandoned | Building.Flags.BurnedDown | Building.Flags.Deleted | Building.Flags.Hidden)) != Building.Flags.None)
                        {
                            continue;
                        }

                        // Initialize vehicle data.
                        if (!initialized)
                        {
                            initialized = true;

                            this.CollectVehicleData(buildings, checkParams.AllowCreateSpares);
                            if (this.freeVehicles < 1)
                            {
                                break;
                            }
                        }

                        // Assign vehicles, unless allredy done.
                        if (this.assignedTargets.ContainsKey(targetBuilding.BuildingId))
                        {
                            targetBuilding.Handled = true;
                        }
                        else
                        {
                            if (Log.LogALot)
                            {
                                Log.DevDebug(this, "Dispatch", "AssignVehicle", targetBuilding.BuildingId, checkParams.IgnoreRange, checkParams.AllowCreateSpares, targetBuilding.BuildingName, targetBuilding.DistrictName);
                            }

                            if (this.AssignVehicle(targetBuilding, checkParams.IgnoreRange, checkParams.AllowCreateSpares))
                            {
                                if (this.freeVehicles < 1)
                                {
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

                    if (initialized && this.freeVehicles < 1)
                    {
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
            bool canCreateSpares = allowCreateSpares && this.serviceSettings.CreateSpares != ServiceDispatcherSettings.SpareVehiclesCreation.Never;

            // Vehicles that failed to find a path.
            HashSet<ushort> lostVehicles = new HashSet<ushort>();

            // Found vehicle that has enough free capacity.
            ushort foundVehicleId;
            float foundVehicleDistance;
            ServiceBuildingInfo foundVehicleBuilding;

            // Found vehicle with decent free capacity.
            ushort foundVehicleDecentId;
            float foundVehicleDecentDistance;

            // Found vehicle with ok free capacity.
            ushort foundVehicleOkId;
            float foundVehicleOkDistance;
            ServiceBuildingInfo foundVehicleOkBuilding;
            bool foundVehicleOkCheck;

            // Found vehicle with any free capacity.
            ushort foundVehicleLastResortId;
            float foundVehicleLastResortDistance;
            ServiceBuildingInfo foundVehicleLastResortBuilding;
            bool foundVehicleLastResortCheck;

            // Created new vehicle.
            bool createdVehicle;

            ServiceBuildingInfo[] checkBuildings;
            //int buildingCountTotal = this.serviceBuildings.Count;
            //int buildingCountUsableInRange;
            //string buildingFilter;

            // Set target info on service buildings.
            foreach (ServiceBuildingInfo serviceBuilding in this.serviceBuildings.Values)
            {
                serviceBuilding.SetCurrentTargetInfo(targetBuilding, canCreateSpares);
            }

            // Get usable buildings.
            if (!this.serviceSettings.DispatchByRange && !this.serviceSettings.DispatchByDistrict)
            {
                // All buildings than can dispatch.
                checkBuildings = this.serviceBuildings.Values.WhereToArray(sb => sb.CurrentTargetCanDispatch);

                //buildingFilter = "CurrentTargetCanDispatch";
                //buildingCountUsableInRange = checkBuildings.Length;
            }
            else if (!ignoreRange)
            {
                // All buildings in range than can dispatch.
                checkBuildings = this.serviceBuildings.Values.WhereToArray(sb => sb.CurrentTargetCanDispatch && sb.CurrentTargetInRange);

                //buildingFilter = "CurrentTargetCanDispatch & CurrentTargetInRange";
                //buildingCountUsableInRange = checkBuildings.Length;
            }
            else if (this.serviceSettings.IgnoreRangeUseClosestBuildings > 0)
            {
                // All buildings in range than can dispatch.
                List<ServiceBuildingInfo> checkServiceBuildings = this.serviceBuildings.Values.WhereToList(sb => sb.CurrentTargetCanDispatch && sb.CurrentTargetInRange);

                //buildingFilter = "(CurrentTargetCanDispatch & CurrentTargetInRange), (!CurrentTargetInRange[" + this.serviceSettings.IgnoreRangeUseClosestBuildings.ToString() + "] & sb.CurrentTargetCanDispatch)";
                //buildingCountUsableInRange = checkServiceBuildings.Count;

                // Closest buildings out of range, if they can dispatch.
                checkServiceBuildings.AddRange(this.serviceBuildings.Values
                                            .Where(sb => !sb.CurrentTargetInRange)
                                            .OrderByTake(sb => sb.CurrentTargetDistance, this.serviceSettings.IgnoreRangeUseClosestBuildings)
                                            .Where(sb => sb.CurrentTargetCanDispatch));

                checkBuildings = checkServiceBuildings.ToArray();
            }
            else
            {
                // All buildings than can dispatch.
                checkBuildings = this.serviceBuildings.Values.WhereToArray(sb => sb.CurrentTargetCanDispatch);

                //buildingFilter = "CurrentTargetCanDispatch";
                //buildingCountUsableInRange = checkBuildings.Length;
            }

            //if (Log.LogALot)
            //{
            //    Log.DevDebug(this, "AssignVehicle", "UsableBuildings", buildingFilter,
            //        this.serviceSettings.DispatchByRange, this.serviceSettings.DispatchByDistrict, ignoreRange,
            //        this.serviceSettings.IgnoreRangeUseClosestBuildings,
            //        buildingCountTotal, buildingCountUsableInRange, checkBuildings.Length);
            //}

            if (checkBuildings.Length == 0)
            {
                if (Log.LogALot)
                {
                    Log.DevDebug(this, "AssignVehicle", "NoBuildings");
                }

                return false;
            }

            // Sort buildings in usability order.
            if (checkBuildings.Length > 1)
            {
                Array.Sort(checkBuildings, Global.ServiceBuildingInfoPriorityComparer);
            }

            // try n times (in case vehicles have problem finding a path to the buidling)
            for (int tries = 0; tries < 16; tries++)
            {
                // Found vehicle that has enough free capacity.
                foundVehicleId = 0;
                foundVehicleDistance = float.PositiveInfinity;
                foundVehicleBuilding = null;

                // Found vehicle with decent free capacity.
                foundVehicleDecentId = 0;
                foundVehicleDecentDistance = float.PositiveInfinity;

                // Found vehicle with ok free capacity.
                foundVehicleOkId = 0;
                foundVehicleOkDistance = float.PositiveInfinity;
                foundVehicleOkBuilding = null;

                // Found vehicle with any free capacity.
                foundVehicleLastResortId = 0;
                foundVehicleLastResortDistance = float.PositiveInfinity;
                foundVehicleLastResortBuilding = null;

                // Created new vehicle.
                createdVehicle = false;

                // Loop through service buildings in priority order and assign a vehicle to the target.
                foreach (ServiceBuildingInfo serviceBuilding in checkBuildings)
                {
                    if (tries > 0 && !serviceBuilding.CurrentTargetCanDispatch)
                    {
                        continue;
                    }

                    // Found vehicle that has enough free capacity.
                    foundVehicleId = 0;
                    foundVehicleDistance = float.PositiveInfinity;

                    // Found vehicle with decent free capacity.
                    foundVehicleDecentId = 0;
                    foundVehicleDecentDistance = float.PositiveInfinity;

                    // Whether to check for vehicles with not so decent free capacity.
                    foundVehicleOkCheck = foundVehicleOkId == 0;
                    foundVehicleLastResortCheck = foundVehicleLastResortId == 0;

                    // If prefer to send new vehicle when building is closer.
                    if (allowCreateSpares && this.serviceSettings.CreateSpares == ServiceDispatcherSettings.SpareVehiclesCreation.WhenBuildingIsCloser && serviceBuilding.VehiclesSpare > 0)
                    {
                        foundVehicleDistance = serviceBuilding.CurrentTargetDistance;
                    }

                    if (Log.LogALot)
                    {
                        Log.DevDebug(this, "AssignVehicle", "ServiceBuilding", serviceBuilding.BuildingId, serviceBuilding.BuildingName, serviceBuilding.DistrictName, serviceBuilding.CurrentTargetDistance, foundVehicleDistance, serviceBuilding.Position, targetBuilding.Position, serviceBuilding.Position - targetBuilding.Position, Vector3.SqrMagnitude(serviceBuilding.Position - targetBuilding.Position), Vector3.SqrMagnitude(targetBuilding.Position - serviceBuilding.Position));
                    }

                    // Loop through vehicles and save the closest free vehicle.
                    foreach (ServiceVehicleInfo vehicleInfo in serviceBuilding.Vehicles.Values.Where(vi => vi.FreeToCollect))
                    {
                        // Don't re-check vehicles that just failed to find path.
                        if (lostVehicles.Contains(vehicleInfo.VehicleId))
                        {
                            continue;
                        }

                        if (vehicleInfo.IsConfused)
                        {
                            lostVehicles.Add(vehicleInfo.VehicleId);
                            continue;
                        }

                        float distance = (targetBuilding.Position - vehicleInfo.Position).sqrMagnitude;

                        // Check for vehicle with enough free capacity.
                        if ((distance < foundVehicleDistance || (foundVehicleId == 0 && distance == foundVehicleDistance)) && vehicleInfo.CapacityFree >= targetBuilding.ProblemSize)
                        {
                            if (Log.LogALot)
                            {
                                Log.DevDebug(this, "AssignVehicle", "Found", vehicleInfo.VehicleId, distance);
                            }

                            foundVehicleId = vehicleInfo.VehicleId;
                            foundVehicleDistance = distance;
                        }

                        // Check for vehicle with decent free capacity.
                        if (distance < foundVehicleDecentDistance && (vehicleInfo.CapacityUsed < CapacityUsedDecent || (float)vehicleInfo.CapacityFree >= (float)targetBuilding.ProblemSize * CapacityProportionDecent))
                        {
                            if (Log.LogALot)
                            {
                                Log.DevDebug(this, "AssignVehicle", "FoundDecent", vehicleInfo.VehicleId, distance);
                            }

                            foundVehicleDecentId = vehicleInfo.VehicleId;
                            foundVehicleDecentDistance = distance;
                        }

                        // Check for vehicle with ok free capacity.
                        if (foundVehicleOkCheck && distance < foundVehicleOkDistance && (vehicleInfo.CapacityUsed < CapacityUsedOk || (float)vehicleInfo.CapacityFree >= (float)targetBuilding.ProblemSize * CapacityProportionOk))
                        {
                            if (Log.LogALot)
                            {
                                Log.DevDebug(this, "AssignVehicle", "FoundOk", vehicleInfo.VehicleId, distance);
                            }

                            foundVehicleOkId = vehicleInfo.VehicleId;
                            foundVehicleOkDistance = distance;
                            foundVehicleOkBuilding = serviceBuilding;
                        }

                        // Check for vehicle with any free capacity.
                        if (foundVehicleLastResortCheck && distance < foundVehicleLastResortDistance && vehicleInfo.CapacityFree > 0)
                        {
                            if (Log.LogALot)
                            {
                                Log.DevDebug(this, "AssignVehicle", "FoundLastResort", vehicleInfo.VehicleId, distance);
                            }

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

                        if (Log.LogToFile)
                        {
                            Log.Debug(this, "AssignVehicle", "UsingDecent", targetBuilding.BuildingId, serviceBuilding.BuildingId, serviceBuilding.VehiclesSpare, foundVehicleId, foundVehicleDistance);
                        }
                    }

                    // No free vehicle found, but building has spare vehicles so we send one of those.
                    if (foundVehicleId == 0 && allowCreateSpares && !lostVehicles.Contains(0) && this.serviceSettings.CreateSpares != ServiceDispatcherSettings.SpareVehiclesCreation.Never && serviceBuilding.VehiclesSpare > 0)
                    {
                        if (Log.LogToFile)
                        {
                            Log.Debug(this, "AssignVehicle", "CreateSpare", targetBuilding.BuildingId, serviceBuilding.BuildingId, serviceBuilding.VehiclesSpare);
                        }

                        lostVehicles.Add(0);

                        foundVehicleId = serviceBuilding.CreateVehicle(this.TransferType, targetBuilding.BuildingId);
                        if (foundVehicleId == 0)
                        {
                            if (Log.LogToFile)
                            {
                                Log.Debug(this, "AssignVehicle", "SpareNotCreated", targetBuilding.BuildingId, serviceBuilding.BuildingId);
                            }

                            if (Global.ServiceProblems != null)
                            {
                                Global.ServiceProblems.Add(ServiceProblemKeeper.ServiceProblem.VehicleNotCreated, serviceBuilding.BuildingId, targetBuilding.BuildingId);
                            }
                        }
                        else
                        {
                            createdVehicle = true;
                            foundVehicleDistance = serviceBuilding.CurrentTargetDistance;

                            if (Log.LogALot)
                            {
                                Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

                                Log.DevDebug(
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
                                    targetBuilding.DistrictName,
                                    serviceBuilding.BuildingName,
                                    serviceBuilding.DistrictName,
                                    VehicleHelper.GetVehicleName(foundVehicleId),
                                    vehicles[foundVehicleId].m_targetBuilding,
                                    vehicles[foundVehicleId].m_flags);
                            }
                        }
                    }

                    if (foundVehicleId != 0)
                    {
                        foundVehicleBuilding = serviceBuilding;
                        break;
                    }
                }

                if (!createdVehicle)
                {
                    // If no vehicle with decent free capacity (including the spare vehicles in the buildings), use the closest with ok free capacity instead.
                    if (foundVehicleDistance == float.PositiveInfinity && foundVehicleOkDistance < float.PositiveInfinity)
                    {
                        foundVehicleId = foundVehicleDecentId;
                        foundVehicleDistance = foundVehicleDecentDistance;
                        foundVehicleBuilding = foundVehicleOkBuilding;

                        if (Log.LogToFile)
                        {
                            Log.Debug(this, "AssignVehicle", "UsingOk", targetBuilding.BuildingId, foundVehicleBuilding.BuildingId, foundVehicleBuilding.VehiclesSpare, foundVehicleId, foundVehicleDistance);
                        }
                    }

                    // If no vehicle with ok free capacity (including the spare vehicles in the buildings), use the closest with any free capacity instead.
                    if (foundVehicleDistance == float.PositiveInfinity && foundVehicleLastResortDistance < float.PositiveInfinity)
                    {
                        foundVehicleId = foundVehicleLastResortId;
                        foundVehicleDistance = foundVehicleLastResortDistance;
                        foundVehicleBuilding = foundVehicleLastResortBuilding;

                        if (Log.LogToFile)
                        {
                            Log.Debug(this, "AssignVehicle", "UsingLastResort", targetBuilding.BuildingId, foundVehicleBuilding.BuildingId, foundVehicleBuilding.VehiclesSpare, foundVehicleId, foundVehicleDistance);
                        }
                    }

                    // No free vehicle was found, return.
                    if (foundVehicleId == 0)
                    {
                        if (Log.LogALot)
                        {
                            Log.DevDebug(this, "AssignVehicle", "NotFound");
                        }

                        return false;
                    }

                    // A free vehicle was found, assign it to the target.
                    if (!foundVehicleBuilding.SetVehicleTarget(foundVehicleId, targetBuilding.BuildingId, this.TransferType))
                    {
                        // The vehicle failed to find a path to the target.
                        if (Log.LogToFile)
                        {
                            Log.Debug(this, "AssignVehicle", "SetTarget", "Failed", targetBuilding.BuildingId, foundVehicleBuilding.BuildingId, foundVehicleBuilding.VehiclesSpare, foundVehicleId, foundVehicleDistance);
                        }

                        if (Global.ServiceProblems != null)
                        {
                            Global.ServiceProblems.Add(ServiceProblemKeeper.ServiceProblem.PathNotFound, targetBuilding.BuildingId, foundVehicleBuilding.BuildingId);
                        }

                        lostVehicles.Add(foundVehicleId);

                        continue;
                    }

                    this.freeVehicles--;
                    foundVehicleBuilding.VehiclesFree--;
                }

                targetBuilding.Handled = true;

                this.assignedTargets[targetBuilding.BuildingId] = Global.CurrentFrame;

                if (Log.LogToFile)
                {
                    if (Log.LogALot)
                    {
                        Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

                        Log.DevDebug(
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
                        Log.DevDebug(
                            this,
                            "AssignVehicle",
                            "Assign",
                            "S",
                            targetBuilding.BuildingId,
                            foundVehicleBuilding.BuildingId,
                            foundVehicleId,
                            foundVehicleBuilding.District,
                            foundVehicleBuilding.CurrentTargetInDistrict,
                            foundVehicleBuilding.CurrentTargetInRange,
                            foundVehicleBuilding.Range,
                            foundVehicleBuilding.CurrentTargetDistance,
                            foundVehicleBuilding.VehiclesTotal,
                            foundVehicleBuilding.VehiclesMade,
                            foundVehicleBuilding.VehiclesFree,
                            foundVehicleBuilding.VehiclesSpare,
                            foundVehicleBuilding.BuildingName,
                            foundVehicleBuilding.DistrictName,
                            foundVehicleBuilding.CurrentTargetInDistrict ? "InDistrict" : (string)null,
                            foundVehicleBuilding.CurrentTargetInRange ? "InRange" : "OutOfRange");
                        Log.DevDebug(
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
                            VehicleHelper.GetVehicleName(foundVehicleId),
                            VehicleHelper.GetDistrictName(foundVehicleId),
                            vehicles[foundVehicleId].m_targetBuilding,
                            vehicles[foundVehicleId].m_flags);
                    }
                    else if (Log.LogNames)
                    {
                        Log.DevDebug(
                            this,
                            "AssignVehicle",
                            "Assign",
                            targetBuilding.BuildingId,
                            foundVehicleBuilding.BuildingId,
                            foundVehicleId,
                            targetBuilding.HasProblem,
                            targetBuilding.ProblemValue,
                            foundVehicleBuilding.CurrentTargetInDistrict,
                            foundVehicleBuilding.CurrentTargetInRange,
                            foundVehicleBuilding.Range,
                            foundVehicleBuilding.CurrentTargetDistance,
                            foundVehicleBuilding.CurrentTargetDistance,
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
                            foundVehicleBuilding.CurrentTargetInDistrict,
                            foundVehicleBuilding.CurrentTargetInRange,
                            foundVehicleBuilding.Range,
                            foundVehicleBuilding.CurrentTargetDistance,
                            foundVehicleDistance);
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Collects the vehicle data.
        /// </summary>
        /// <param name="buildings">The CS buildings.</param>
        /// <param name="allowCreateSpares">If set to <c>true</c> allow creation of spare vehicles.</param>
        /// <exception cref="System.Exception">Loop counter too high.</exception>
        private void CollectVehicleData(Building[] buildings, bool allowCreateSpares)
        {
            this.freeVehicles = 0;
            int freeBuildings = 0;

            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            HashSet<Type> vehicleAIs = null;
            if (this.lastVehicleDetourClassCheck == 0 || Global.CurrentFrame - this.lastVehicleDetourClassCheck > Global.ClassCheckInterval)
            {
                vehicleAIs = new HashSet<Type>();
            }

            // Loop through the service buildings.
            foreach (ServiceBuildingInfo serviceBuilding in this.serviceBuildings.Values)
            {
                // Skip missing buildings.
                if (buildings[serviceBuilding.BuildingId].Info == null || (buildings[serviceBuilding.BuildingId].m_flags & Building.Flags.Created) == Building.Flags.None || (buildings[serviceBuilding.BuildingId].m_flags & (Building.Flags.Abandoned | Building.Flags.BurnedDown | Building.Flags.Deleted | Building.Flags.Hidden)) != Building.Flags.None)
                {
                    if (Log.LogALot)
                    {
                        Log.DevDebug(this, "CollectVehicleData", "NotBuilding", serviceBuilding.BuildingId);
                    }

                    serviceBuilding.CanReceive = false;
                    continue;
                }

                if (allowCreateSpares && this.serviceSettings.CreateSpares != ServiceDispatcherSettings.SpareVehiclesCreation.Never && serviceBuilding.CanReceive)
                {
                    this.freeVehicles += serviceBuilding.VehiclesSpare;
                }

                // Loop through the vehicles.
                int vehiclesMade = 0;
                int vehiclesFree = 0;
                int count = 0;

                serviceBuilding.FirstOwnVehicleId = buildings[serviceBuilding.BuildingId].m_ownVehicles;
                ushort vehicleId = serviceBuilding.FirstOwnVehicleId;
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
                        if (vehicles[vehicleId].Info != null &&
                            (vehicles[vehicleId].m_flags & Vehicle.Flags.Created) == Vehicle.Flags.Created &&
                            (vehicles[vehicleId].m_flags & VehicleHelper.VehicleExists) != ~VehicleHelper.VehicleAll)
                        {
                            if (vehicleAIs != null)
                            {
                                vehicleAIs.Add(vehicles[vehicleId].Info.m_vehicleAI.GetType());
                            }

                            // Check if vehicle is free to dispatch and has free space.
                            int loadSize, loadMax;
                            vehicles[vehicleId].Info.m_vehicleAI.GetSize(vehicleId, ref vehicles[vehicleId], out loadSize, out loadMax);

                            bool collecting = (vehicles[vehicleId].m_flags & Vehicle.Flags.TransferToSource) != ~VehicleHelper.VehicleAll && (vehicles[vehicleId].m_flags & Vehicle.Flags.TransferToTarget) == ~VehicleHelper.VehicleAll;
                            bool loading = (vehicles[vehicleId].m_flags & (Vehicle.Flags.Arriving | Vehicle.Flags.Stopped)) != ~VehicleHelper.VehicleAll;
                            bool canCollect = collecting && !loading && loadSize < loadMax;
                            bool hasTarget = vehicles[vehicleId].m_targetBuilding != 0 && !(collecting && vehicles[vehicleId].m_targetBuilding == serviceBuilding.BuildingId && !this.targetBuildings.ContainsKey(serviceBuilding.BuildingId));
                            bool unavailable = (vehicles[vehicleId].m_flags & VehicleHelper.VehicleUnavailable) != ~VehicleHelper.VehicleAll;
                            bool busy = (vehicles[vehicleId].m_flags & VehicleHelper.VehicleBusy) != ~VehicleHelper.VehicleAll;

                            VehicleResult vehicleResult = true;

                            // Update vehicle status.
                            ServiceVehicleInfo serviceVehicle;
                            if (serviceBuilding.Vehicles.TryGetValue(vehicleId, out serviceVehicle))
                            {
                                if (collecting && !loading && !unavailable && !busy && vehicles[vehicleId].m_targetBuilding != 0)
                                {
                                    TargetBuildingInfo targetBuilding;
                                    if (!this.targetBuildings.TryGetValue(vehicles[vehicleId].m_targetBuilding, out targetBuilding))
                                    {
                                        targetBuilding = null;
                                    }

                                    if (targetBuilding == null || !targetBuilding.WantedService || (vehicles[vehicleId].m_targetBuilding != serviceBuilding.BuildingId && !hasTarget))
                                    {
                                        Global.TransferOffersCleaningNeeded = true;
                                        vehicleResult = serviceVehicle.DeAssign(ref vehicles[vehicleId], false, this, "CollectVehicleData", "NoNeed");
                                        hasTarget = false;
                                    }
                                    else if (vehicles[vehicleId].m_targetBuilding != serviceVehicle.Target)
                                    {
                                        Global.TransferOffersCleaningNeeded = true;
                                        vehicleResult = serviceVehicle.DeAssign(ref vehicles[vehicleId], false, this, "CollectVehicleData", "WrongTarget");
                                        hasTarget = false;
                                    }
                                    else if (targetBuilding != null && targetBuilding.NeedsService)
                                    {
                                        busy = true;
                                    }
                                }

                                if (!vehicleResult.DeSpawned)
                                {
                                    serviceVehicle.Update(ref vehicles[vehicleId], canCollect && !hasTarget && !busy && !unavailable, false, true);
                                }
                                else if (Log.LogALot)
                                {
                                    Log.DevDebug(this, "CollectVehicleData", "DeSpawned", serviceBuilding.BuildingId, vehicleId, collecting, loadSize, loadMax, loading, unavailable, busy, hasTarget, vehicleResult);
                                }
                            }
                            else
                            {
                                if (collecting && !loading && !unavailable && !busy && vehicles[vehicleId].m_targetBuilding != 0)
                                {
                                    Global.TransferOffersCleaningNeeded = true;
                                    vehicleResult = VehicleHelper.DeAssign(vehicleId, ref vehicles[vehicleId]);
                                    hasTarget = false;
                                }

                                if (!vehicleResult.DeSpawned)
                                {
                                    serviceVehicle = new ServiceVehicleInfo(vehicleId, ref vehicles[vehicleId], canCollect && !hasTarget && !busy && !unavailable, this.DispatcherType, 0);
                                    if (Log.LogALot)
                                    {
                                        Log.DevDebug(this, "CollectVehicleData", "AddVehicle", serviceBuilding.BuildingId, vehicleId, vehicles[vehicleId].Info.name, serviceVehicle.VehicleName, serviceVehicle.FreeToCollect, collecting, vehicles[vehicleId].m_flags, loadSize, loadMax, loading, unavailable, busy, hasTarget, vehicles[vehicleId].m_targetBuilding, vehicleResult);
                                    }

                                    serviceBuilding.Vehicles[vehicleId] = serviceVehicle;
                                }
                                else if (Log.LogALot)
                                {
                                    Log.DevDebug(this, "CollectVehicleData", "DeSpawned", serviceBuilding.BuildingId, vehicleId, collecting, loadSize, loadMax, loading, unavailable, busy, hasTarget, vehicleResult);
                                }
                            }

                            // Update counts and assigned target status.
                            if (!vehicleResult.DeSpawned)
                            {
                                vehiclesMade++;

                                if (serviceVehicle.FreeToCollect)
                                {
                                    this.freeVehicles++;
                                    vehiclesFree++;
                                }

                                if (collecting && hasTarget && !vehicleResult.DeAssigned)
                                {
                                    //if (Log.LogALot && !this.assignedTargets.ContainsKey(vehicles[vehicleId].m_targetBuilding))
                                    //{
                                    //    Log.DevDebug(this, "CollectVehicleData", "AddAssigned", serviceBuilding.BuildingId, vehicleId, vehicles[vehicleId].m_targetBuilding);
                                    //}

                                    this.assignedTargets[vehicles[vehicleId].m_targetBuilding] = Global.CurrentFrame;
                                }
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

                if (vehiclesFree > 0)
                {
                    freeBuildings++;
                }

                // Remove old vehicles.
                KeyValuePair<ushort, ushort>[] removeVehicles = serviceBuilding.Vehicles.Values.WhereSelectToArray(v => v.LastSeen != Global.CurrentFrame, v => new KeyValuePair<ushort, ushort>(v.VehicleId, v.Target));
                foreach (KeyValuePair<ushort, ushort> vehicle in removeVehicles)
                {
                    if (vehicles[vehicle.Key].Info == null ||
                        (vehicles[vehicle.Key].m_flags & Vehicle.Flags.Created) == ~VehicleHelper.VehicleAll ||
                        (vehicles[vehicle.Key].m_flags & VehicleHelper.VehicleExists) == ~VehicleHelper.VehicleAll ||
                        vehicles[vehicle.Key].m_transferType != this.TransferType)
                    {
                        if (Log.LogALot)
                        {
                            Log.DevDebug(this, "CollectVehicleData", "RemoveNonVehicle", serviceBuilding.BuildingId, vehicle.Key, vehicles[vehicle.Key].m_flags, vehicles[vehicle.Key].Info, vehicles[vehicle.Key].m_transferType);
                        }

                        if (vehicle.Value > 0 && Global.ServiceProblems != null)
                        {
                            Global.ServiceProblems.Add(ServiceProblemKeeper.ServiceProblem.VehicleGone, serviceBuilding.BuildingId, vehicle.Value);
                        }

                        serviceBuilding.Vehicles.Remove(vehicle.Key);
                    }
                    else
                    {
                        if (vehicles[vehicle.Key].m_sourceBuilding != serviceBuilding.BuildingId)
                        {
                            if (Log.LogALot)
                            {
                                Log.DevDebug(this, "CollectVehicleData", "RemoveMovedVehicle", serviceBuilding.BuildingId, vehicle.Key, vehicles[vehicle.Key].m_flags);
                            }

                            if (vehicle.Value > 0 && Global.ServiceProblems != null)
                            {
                                Global.ServiceProblems.Add(ServiceProblemKeeper.ServiceProblem.VehicleGone, serviceBuilding.BuildingId, vehicle.Value);
                            }

                            serviceBuilding.Vehicles.Remove(vehicle.Key);
                        }

                        this.assignedTargets[vehicles[vehicle.Key].m_targetBuilding] = Global.CurrentFrame;
                    }
                }
            }

            // Remove old target assigments.
            ushort[] removeTargets = this.assignedTargets.WhereSelectToArray(at => at.Value != Global.CurrentFrame, at => at.Key);
            foreach (ushort id in removeTargets)
            {
                if (Log.LogALot)
                {
                    Log.DevDebug(this, "CollectVehicleData", "RemoveAssigned", id);
                }

                this.assignedTargets.Remove(id);
            }

            if (vehicleAIs != null)
            {
                foreach (Type classType in vehicleAIs)
                {
                    Detours.AddClass(classType);
                }
            }

            if (Log.LogALot)
            {
                Log.DevDebug(this, "CollectVehicleData", "Free", freeBuildings, this.freeVehicles);
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
                        this.serviceBuildings = Global.Buildings.DeathCare.ServiceBuildings;
                        this.targetBuildings = Global.Buildings.DeathCare.TargetBuildings;
                    }

                    this.serviceSettings = Global.Settings.DeathCare;
                    break;

                case DispatcherTypes.GarbageTruckDispatcher:
                    if (constructing)
                    {
                        this.TransferType = (byte)TransferManager.TransferReason.Garbage;
                        this.serviceBuildings = Global.Buildings.Garbage.ServiceBuildings;
                        this.targetBuildings = Global.Buildings.Garbage.TargetBuildings;
                    }

                    this.serviceSettings = Global.Settings.DeathCare;
                    break;

                case DispatcherTypes.AmbulanceDispatcher:
                    if (constructing)
                    {
                        this.TransferType = (byte)TransferManager.TransferReason.Sick;
                        this.serviceBuildings = Global.Buildings.HealthCare.ServiceBuildings;
                        this.targetBuildings = Global.Buildings.HealthCare.TargetBuildings;
                    }

                    this.serviceSettings = Global.Settings.HealthCare;
                    break;

                default:
                    throw new Exception("Bad dispatcher type");
            }

            if (this.serviceSettings.Patrol)
            {
                this.buildingChecks = BuldingCheckParameters.GetBuldingCheckParametersWithPatrol(this.serviceSettings.ChecksParameters);
            }
            else
            {
                this.buildingChecks = BuldingCheckParameters.GetBuldingCheckParameters(this.serviceSettings.ChecksParameters);
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
                this.Setting = ServiceDispatcherSettings.BuildingCheckParameters.Undefined;
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
            public BuldingCheckParameters(ServiceDispatcherSettings.BuildingCheckParameters buildingCheckParameters)
            {
                this.Setting = buildingCheckParameters;

                this.IncludeUneedy = false;
                this.AllowCreateSpares = true;

                switch (buildingCheckParameters)
                {
                    case ServiceDispatcherSettings.BuildingCheckParameters.Any:
                        this.OnlyProblematic = false;
                        this.IgnoreRange = true;
                        this.MinProblemValue = 0;
                        break;

                    case ServiceDispatcherSettings.BuildingCheckParameters.InRange:
                        this.OnlyProblematic = false;
                        this.IgnoreRange = false;
                        this.MinProblemValue = 0;
                        break;

                    case ServiceDispatcherSettings.BuildingCheckParameters.ProblematicInRange:
                        this.OnlyProblematic = true;
                        this.IgnoreRange = false;
                        this.MinProblemValue = 0;
                        break;

                    case ServiceDispatcherSettings.BuildingCheckParameters.ProblematicIgnoreRange:
                        this.OnlyProblematic = true;
                        this.IgnoreRange = true;
                        this.MinProblemValue = 0;
                        break;

                    case ServiceDispatcherSettings.BuildingCheckParameters.VeryProblematicInRange:
                        this.OnlyProblematic = true;
                        this.IgnoreRange = false;
                        this.MinProblemValue = ProblemLimitMajor;
                        break;

                    case ServiceDispatcherSettings.BuildingCheckParameters.VeryProblematicIgnoreRange:
                        this.OnlyProblematic = true;
                        this.IgnoreRange = true;
                        this.MinProblemValue = ProblemLimitMajor;
                        break;

                    case ServiceDispatcherSettings.BuildingCheckParameters.ForgottenInRange:
                        this.OnlyProblematic = true;
                        this.IgnoreRange = false;
                        this.MinProblemValue = ProblemLimitForgotten;
                        break;

                    case ServiceDispatcherSettings.BuildingCheckParameters.ForgottenIgnoreRange:
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
            public ServiceDispatcherSettings.BuildingCheckParameters Setting
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
            public static BuldingCheckParameters[] GetBuldingCheckParameters(ServiceDispatcherSettings.BuildingCheckParameters[] buildingCheckParameters)
            {
                return buildingCheckParameters.SelectToArray(bcp => new BuldingCheckParameters(bcp));
            }

            /// <summary>
            /// Gets the dispatcher building check parameters with an extra entry for patrolling vehicles.
            /// </summary>
            /// <param name="buildingCheckParameters">The building check parameters.</param>
            /// <returns>
            /// The dispatcher building check parameters.
            /// </returns>
            public static BuldingCheckParameters[] GetBuldingCheckParametersWithPatrol(ServiceDispatcherSettings.BuildingCheckParameters[] buildingCheckParameters)
            {
                List<BuldingCheckParameters> parameters = buildingCheckParameters.SelectToList(bcp => new BuldingCheckParameters(bcp));
                parameters.Add(new BuldingCheckParameters(false, true, false, 0, false));

                return parameters.ToArray();
            }

            /// <summary>
            /// Check whether this building should be checked.
            /// </summary>
            /// <param name="building">The building.</param>
            /// <returns>
            /// True if the building should be checked.
            /// </returns>
            public bool CheckThis(TargetBuildingInfo building)
            {
                return building.CheckThis && !building.HandledNow &&
                       (building.NeedsService || (this.IncludeUneedy && building.WantsService)) &&
                       building.ProblemValue >= this.MinProblemValue && (building.HasProblem || !this.OnlyProblematic);
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                Log.InfoList info = new Log.InfoList("BuldingCheckParams: ");

                info.Add("OnlyProblematic", this.OnlyProblematic);
                info.Add("IncludeUneedy", this.IncludeUneedy);
                info.Add("IgnoreRange", this.IgnoreRange);
                info.Add("MinProblemValue", this.MinProblemValue);
                info.Add("AllowCreateSpares", this.AllowCreateSpares);

                return info.ToString();
            }
        }
    }
}