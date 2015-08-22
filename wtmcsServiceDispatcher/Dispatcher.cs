using ColossalFramework;
using System.Collections.Generic;
using System.Linq;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Service vehicle dispatch base class.
    /// </summary>
    internal abstract class Dispatcher
    {
        /// <summary>
        /// The assigned targets.
        /// </summary>
        protected Dictionary<ushort, uint> assignedTargets = new Dictionary<ushort, uint>();

        /// <summary>
        /// The dispatcher is just pretending to dispatch.
        /// </summary>
        protected bool IsPretending = Global.PretendToHandleStuff;

        /// <summary>
        /// The free vehicle count.
        /// </summary>
        private int freeVehicles = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dispatcher"/> class.
        /// </summary>
        public Dispatcher()
        {
            Log.Debug(this, "Constructed");
        }

        /// <summary>
        /// Gets a value indicating whether this service has target buildings.
        /// </summary>
        /// <value>
        /// <c>true</c> if this service has target buildings; otherwise, <c>false</c>.
        /// </value>
        protected abstract bool HasTargetBuildings { get; }

        /// <summary>
        /// Gets the service buildings.
        /// </summary>
        /// <value>
        /// The service buildings.
        /// </value>
        protected abstract IEnumerable<Buildings.ServiceBuildingInfo> ServiceBuildings { get; }

        /// <summary>
        /// Gets the target buildings.
        /// </summary>
        /// <value>
        /// The target buildings.
        /// </value>
        protected abstract IEnumerable<Buildings.TargetBuildingInfo> TargetBuildings { get; }

        /// <summary>
        /// Dispatches vehicles to targets.
        /// </summary>
        public virtual void Dispatch()
        {
            // Dispatch, if needed.
            if (HasTargetBuildings)
            {
                //if (Log.LogALot && Library.IsDebugBuild) Log.DevDebug(this, "Dispatch", "Dispatch");

                bool initialized = false;
                foreach (BuldingCheckParameters bcParams in Global.BuldingCheckParameters)
                {
                    ushort stillWaiting = 0;

                    foreach (Buildings.TargetBuildingInfo targetBuilding in TargetBuildings.Where(tb => tb.CheckThis && tb.ProblemTimer >= bcParams.MinTimer && (tb.HasProblem || !bcParams.OnlyProblematic)).OrderBy(tb => tb, Global.TargetBuildingInfoPriorityComparer))
                    {
                        // Initialize vehicle data.
                        if (!initialized)
                        {
                            initialized = true;

                            CollectVehicleData();
                            if (freeVehicles < 1)
                            {
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
                                    break;
                                }
                            }
                            else
                            {
                                stillWaiting++;
                            }
                        }

                        targetBuilding.Checked = true;
                    }

                    if (stillWaiting == 0 || freeVehicles < 1)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Set target with the vehicles AI.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="buildingId">The building identifier.</param>
        protected abstract void AISetTarget(ushort vehicleId, ref Vehicle vehicle, ushort buildingId);

        /// <summary>
        /// Determines whether vehicle is correct type of vehicle.
        /// </summary>
        /// <param name="vehicleInfo">The vehicle information.</param>
        /// <returns>True if vehicle is correct type.</returns>
        protected abstract bool IsMyType(VehicleInfo vehicleInfo);

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

            //if (Log.LogALot && Library.IsDebugBuild) Log.DevDebug(this, "AssignVehicle", "TargetBuilding", targetBuilding.BuildingId, targetDistrict);

            // Set target info on service buildings.
            foreach (Buildings.ServiceBuildingInfo serviceBuilding in ServiceBuildings)
            {
                serviceBuilding.SetTargetInfo(districtManager, targetBuilding, ignoreRange);

                //if (Log.LogALot && Library.IsDebugBuild) Log.DevDebug(this, "AssignVehicle", "ServiceBuilding", "Set", serviceBuilding.BuildingId, serviceBuilding.District, serviceBuilding.InDistrict, serviceBuilding.Distance, serviceBuilding.InRange);
            }

            // Loop through service buildings in priority order and assign a vehicle to the target.
            foreach (Buildings.ServiceBuildingInfo serviceBuilding in ServiceBuildings.Where(sb => sb.InRange).OrderBy(sb => sb, Global.ServiceBuildingInfoPriorityComparer))
            {
                //if (Log.LogALot && Library.IsDebugBuild) Log.DevDebug(this, "AssignVehicle", "ServiceBuilding", "Check", serviceBuilding.BuildingId, serviceBuilding.District, serviceBuilding.InDistrict, serviceBuilding.Distance);

                ushort vehicleFoundId = 0;
                float vehicleFoundDistance = float.PositiveInfinity;

                // Loop through vehicles and save the closest free vehicle.
                foreach (Vehicles.ServiceVehicleInfo vehicleInfo in serviceBuilding.Vehicles.Values)
                {
                    if (vehicleInfo.Target == 0)
                    {
                        float distance = (targetBuilding.Position - vehicleInfo.Position).sqrMagnitude;
                        //if (Log.LogALot && Library.IsDebugBuild) Log.DevDebug(this, "AssignVehicle", "Vehicle", "Check", vehicleInfo.VehicleId, distance);

                        if (vehicleFoundId == 0 || distance < vehicleFoundDistance)
                        {
                            vehicleFoundId = vehicleInfo.VehicleId;
                            vehicleFoundDistance = distance;

                            //if (Log.LogALot && Library.IsDebugBuild) Log.DevDebug(this, "AssignVehicle", "Vehicle", "Save", vehicleInfo.VehicleId, distance);
                        }
                    }
                }

                if (vehicleFoundId != 0)
                {
                    // A free vehicle was found, assign it to the target.
                    targetBuilding.Handled = true;
                    if (Log.LogALot || Library.IsDebugBuild) Log.Debug(this, "AssignVehicle", "Assign", targetBuilding.BuildingId, vehicleFoundId, vehicleFoundDistance);

                    if (!IsPretending)
                    {
                        Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

                        AISetTarget(vehicleFoundId, ref vehicles[vehicleFoundId], targetBuilding.BuildingId);
                        vehicles[vehicleFoundId].m_targetBuilding = targetBuilding.BuildingId;

                        assignedTargets[targetBuilding.BuildingId] = Global.CurrentFrame;

                        serviceBuilding.Vehicles[vehicleFoundId].Target = targetBuilding.BuildingId;

                        freeVehicles--;
                    }

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

            // Loop through the service buildings.
            foreach (Buildings.ServiceBuildingInfo serviceBuilding in ServiceBuildings)
            {
                // Loop therough the vehicles.
                ushort vehicleId = serviceBuilding.FirstOwnVehicle;
                while (vehicleId != 0)
                {
                    // Add or update status for relevant vegicles.
                    if (vehicles[vehicleId].Info != null && (vehicles[vehicleId].m_flags & Vehicle.Flags.Spawned) == Vehicle.Flags.Spawned && IsMyType(vehicles[vehicleId].Info))
                    {
                        // Update vehicle status.
                        if (serviceBuilding.Vehicles.ContainsKey(vehicleId))
                        {
                            if (Global.ForceTarget && vehicles[vehicleId].m_targetBuilding != 0 && vehicles[vehicleId].m_targetBuilding != serviceBuilding.Vehicles[vehicleId].Target)
                            {
                                AISetTarget(vehicleId, ref vehicles[vehicleId], 0);
                                vehicles[vehicleId].m_targetBuilding = 0;
                            }

                            serviceBuilding.Vehicles[vehicleId].Update(ref vehicles[vehicleId]);
                        }
                        else
                        {
                            if (Log.LogALot && Library.IsDebugBuild) Log.DevDebug(this, "CollectVehicles", "AddVehicle", serviceBuilding.BuildingId, vehicleId, vehicles[vehicleId].Info.name);

                            if (Global.ForceTarget && vehicles[vehicleId].m_targetBuilding != 0)
                            {
                                AISetTarget(vehicleId, ref vehicles[vehicleId], 0);
                                vehicles[vehicleId].m_targetBuilding = 0;
                            }

                            serviceBuilding.Vehicles[vehicleId] = new Vehicles.ServiceVehicleInfo(vehicleId, ref vehicles[vehicleId]);
                        }

                        // Update assigned target status.
                        if (vehicles[vehicleId].m_targetBuilding != 0)
                        {
                            if (Log.LogALot && Library.IsDebugBuild && !assignedTargets.ContainsKey(vehicles[vehicleId].m_targetBuilding)) Log.DevDebug(this, "CollectVehicles", "AddAssigned", serviceBuilding.BuildingId, vehicleId, vehicles[vehicleId].m_targetBuilding);

                            assignedTargets[vehicles[vehicleId].m_targetBuilding] = Global.CurrentFrame;
                        }
                        else
                        {
                            freeVehicles++;
                        }
                    }

                    vehicleId = vehicles[vehicleId].m_nextOwnVehicle;
                    if (vehicleId == serviceBuilding.FirstOwnVehicle)
                    {
                        break;
                    }
                }

                // Remove old vehicles.
                ushort[] removeVehicles = serviceBuilding.Vehicles.Values.Where(v => v.LastSeen != Global.CurrentFrame).Select(v => v.VehicleId).ToArray();
                foreach (ushort id in removeVehicles)
                {
                    if (vehicles[vehicleId].Info == null || (vehicles[vehicleId].m_flags & Vehicle.Flags.Spawned) != Vehicle.Flags.Spawned || !IsMyType(vehicles[vehicleId].Info))
                    {
                        if (Log.LogALot && Library.IsDebugBuild) Log.DevDebug(this, "CollectVehicles", "RemoveVehicle", serviceBuilding.BuildingId, id);

                        serviceBuilding.Vehicles.Remove(id);
                    }
                    else
                    {
                        if (vehicles[vehicleId].m_sourceBuilding != serviceBuilding.BuildingId)
                        {
                            if (Log.LogALot && Library.IsDebugBuild) Log.DevDebug(this, "CollectVehicles", "RemoveVehicle", serviceBuilding.BuildingId, id);

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
                if (Log.LogALot && Library.IsDebugBuild) Log.DevDebug(this, "CollectVehicles", "RemoveAssigned", id);

                assignedTargets.Remove(id);
            }
        }

        /// <summary>
        /// Building check parameters.
        /// </summary>
        public struct BuldingCheckParameters
        {
            /// <summary>
            /// Ignore the range
            /// </summary>
            public bool IgnoreRange;

            /// <summary>
            /// The minimum problem timer value.
            /// </summary>
            public byte MinTimer;

            /// <summary>
            /// Only problematic buildings.
            /// </summary>
            public bool OnlyProblematic;

            /// <summary>
            /// Initializes a new instance of the <see cref="BuldingCheckParameters"/> struct.
            /// </summary>
            /// <param name="onlyProblematic">if set to <c>true</c> check only problematic buildings.</param>
            /// <param name="ignoreRange">if set to <c>true</c> ignore range.</param>
            /// <param name="minTimer">The minimum problem timer value.</param>
            public BuldingCheckParameters(bool onlyProblematic, bool ignoreRange, byte minTimer)
            {
                this.OnlyProblematic = onlyProblematic;
                this.IgnoreRange = ignoreRange;
                this.MinTimer = minTimer;
            }

            /// <summary>
            /// 1, forgotten in range; 2, forgotten out of range; 3, in range; 4, problematic out of range.
            /// </summary>
            public static BuldingCheckParameters[] ForgottenFirst
            {
                get
                {
                    return new BuldingCheckParameters[]
                    {
                        ForgottenInRange,
                        ForgottenIgnoreRange,
                        InRange,
                        ProblematicIgnoreRange
                    };
                }
            }

            /// <summary>
            /// Forgotten buildings in or out of range.
            /// </summary>
            public static BuldingCheckParameters ForgottenIgnoreRange
            {
                get
                {
                    return new BuldingCheckParameters(false, true, 255);
                }
            }

            /// <summary>
            /// Forgotten buildings in range.
            /// </summary>
            public static BuldingCheckParameters ForgottenInRange
            {
                get
                {
                    return new BuldingCheckParameters(false, false, 255);
                }
            }

            /// <summary>
            /// Buildings in range.
            /// </summary>
            public static BuldingCheckParameters InRange
            {
                get
                {
                    return new BuldingCheckParameters(false, false, 0);
                }
            }

            /// <summary>
            /// 1, in range; 2, problematic out of range.
            /// </summary>
            public static BuldingCheckParameters[] InRangeFirst
            {
                get
                {
                    return new BuldingCheckParameters[]
                    {
                        InRange,
                        ProblematicIgnoreRange
                    };
                }
            }

            /// <summary>
            /// 1, problematic in range; 2, problematic; 3, in range.
            /// </summary>
            public static BuldingCheckParameters[] ProblematicFirst
            {
                get
                {
                    return new BuldingCheckParameters[]
                    {
                        ProblematicInRange,
                        ProblematicIgnoreRange,
                        InRange
                    };
                }
            }

            /// <summary>
            /// Problematic buildings in or out of range.
            /// </summary>
            public static BuldingCheckParameters ProblematicIgnoreRange
            {
                get
                {
                    return new BuldingCheckParameters(true, true, 0);
                }
            }

            /// <summary>
            /// Problematic buildings in range.
            /// </summary>
            public static BuldingCheckParameters ProblematicInRange
            {
                get
                {
                    return new BuldingCheckParameters(true, false, 0);
                }
            }
        }
    }
}