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
                    if (Log.LogALot && Library.IsDebugBuild) Log.DevDebug(this, "Dispatch", bcParams.Setting, bcParams.OnlyProblematic, bcParams.MinProblemTimer, bcParams.IgnoreRange);

                    ushort stillWaiting = 0;

                    foreach (Buildings.TargetBuildingInfo targetBuilding in TargetBuildings.Where(tb => tb.CheckThis && tb.ProblemTimer >= bcParams.MinProblemTimer && (tb.HasProblem || !bcParams.OnlyProblematic)).OrderBy(tb => tb, Global.TargetBuildingInfoPriorityComparer))
                    {
                        // Initialize vehicle data.
                        if (!initialized)
                        {
                            initialized = true;

                            CollectVehicleData();
                            if (freeVehicles < 1)
                            {
                                if (Log.LogALot && Library.IsDebugBuild) Log.DevDebug(this, "Dispatch", "BreakCheck", stillWaiting, freeVehicles);
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
                                    if (Log.LogALot && Library.IsDebugBuild) Log.DevDebug(this, "Dispatch", "BreakCheck", stillWaiting, freeVehicles);
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
                        if (Log.LogALot && Library.IsDebugBuild) Log.DevDebug(this, "Dispatch", "BreakChecks", stillWaiting, freeVehicles);

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


                        if (v.Info.m_vehicleAI.GetLocalizedStatus(id, ref v, out instanceID) != _collecting)
                            continue;
private string _collecting = ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_HEARSE_COLLECT");                        
                        
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
                            Vehicles.ServiceVehicleInfo vehicle = new Vehicles.ServiceVehicleInfo(vehicleId, ref vehicles[vehicleId]);
                            if (Log.LogALot && Library.IsDebugBuild) Log.DevDebug(this, "CollectVehicles", "AddVehicle", serviceBuilding.BuildingId, vehicleId, vehicles[vehicleId].Info.name, vehicle.VehicleName);

                            if (Global.ForceTarget && vehicles[vehicleId].m_targetBuilding != 0)
                            {
                                AISetTarget(vehicleId, ref vehicles[vehicleId], 0);
                                vehicles[vehicleId].m_targetBuilding = 0;
                            }

                            serviceBuilding.Vehicles[vehicleId] = vehicle;
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
                        if (Log.LogALot && Library.IsDebugBuild) Log.DevDebug(this, "CollectVehicles", "RemoveVehicle", serviceBuilding.BuildingId, id, serviceBuilding.Vehicles[id].VehicleName);

                        serviceBuilding.Vehicles.Remove(id);
                    }
                    else
                    {
                        if (vehicles[vehicleId].m_sourceBuilding != serviceBuilding.BuildingId)
                        {
                            if (Log.LogALot && Library.IsDebugBuild) Log.DevDebug(this, "CollectVehicles", "RemoveVehicle", serviceBuilding.BuildingId, id, serviceBuilding.Vehicles[id].VehicleName);

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
                this.MinProblemTimer = minTimer;
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
                    case Settings.BuildingCheckParameters.InRange:
                        this.OnlyProblematic = false;
                        this.IgnoreRange = false;
                        this.MinProblemTimer = 0;
                        break;

                    case Settings.BuildingCheckParameters.Any:
                        this.OnlyProblematic = false;
                        this.IgnoreRange = true;
                        this.MinProblemTimer = 0;
                        break;

                    case Settings.BuildingCheckParameters.ProblematicInRange:
                        this.OnlyProblematic = true;
                        this.IgnoreRange = false;
                        this.MinProblemTimer = 0;
                        break;

                    case Settings.BuildingCheckParameters.ProblematicIgnoreRange:
                        this.OnlyProblematic = true;
                        this.IgnoreRange = true;
                        this.MinProblemTimer = 0;
                        break;

                    case Settings.BuildingCheckParameters.ForgottenInRange:
                        this.OnlyProblematic = false;
                        this.IgnoreRange = false;
                        this.MinProblemTimer = 0;
                        break;

                    case Settings.BuildingCheckParameters.ForgottenIgnoreRange:
                        this.OnlyProblematic = false;
                        this.IgnoreRange = true;
                        this.MinProblemTimer = 0;
                        break;

                    default:
                        this.OnlyProblematic = false;
                        this.IgnoreRange = true;
                        this.MinProblemTimer = 0;
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
            public byte MinProblemTimer { get; private set; }

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
            public static BuldingCheckParameters[] GetBuldingCheckParameters(Settings.BuildingCheckParameters[] buildingCheckParameters = null)
            {
                if (buildingCheckParameters == null)
                {
                    Global.InitSettings();
                    buildingCheckParameters = Global.Settings.BuildingChecksParameters;
                }

                return buildingCheckParameters.Select(bcp => new BuldingCheckParameters(bcp)).ToArray();
            }
        }
    }
}