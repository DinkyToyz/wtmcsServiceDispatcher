using ColossalFramework;
using System;
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
        /// The problem buffer modifier
        /// </summary>
        public static readonly ushort ProblemBufferModifier = 1;

        /// <summary>
        /// The forgotten problem value limit.
        /// </summary>
        public static readonly ushort ProblemForgottenLimit = 255 * 256;

        /// <summary>
        /// The problem timer modifier.
        /// </summary>
        public static readonly ushort ProblemTimerModifier = 256;

        /// <summary>
        /// The assigned targets.
        /// </summary>
        protected Dictionary<ushort, uint> assignedTargets = new Dictionary<ushort, uint>();

        /// <summary>
        /// The bulding check parameters.
        /// </summary>
        protected Dispatcher.BuldingCheckParameters[] BuldingChecks = null;

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
        protected abstract Dictionary<ushort, Buildings.ServiceBuildingInfo> ServiceBuildings { get; }

        /// <summary>
        /// Gets the target buildings.
        /// </summary>
        /// <value>
        /// The target buildings.
        /// </value>
        protected abstract Dictionary<ushort, Buildings.TargetBuildingInfo> TargetBuildings { get; }

        /// <summary>
        /// Checks the vehicle target.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        public void CheckVehicleTarget(ushort vehicleId, ref Vehicle vehicle)
        {
            if (!TargetBuildings.ContainsKey(vehicle.m_targetBuilding))
            {
                if (Log.LogALot) Log.DevDebug(this, "CheckVehicleTarget", "NoNeed", vehicleId, vehicle.m_targetBuilding);

                AISetTarget(vehicleId, ref vehicle, 0);
                vehicle.m_targetBuilding = 0;
            }
            else
            {
                bool known = false;
                foreach (Buildings.ServiceBuildingInfo serviceBuilding in ServiceBuildings.Values)
                {
                    if (serviceBuilding.Vehicles.ContainsKey(vehicleId))
                    {
                        known = true;
                        if (vehicle.m_targetBuilding != serviceBuilding.Vehicles[vehicleId].Target)
                        {
                            if (Log.LogALot) Log.DevDebug(this, "CheckVehicleTarget", "WrongTarget", vehicleId, vehicle.m_targetBuilding, serviceBuilding.Vehicles[vehicleId].Target);

                            serviceBuilding.Vehicles[vehicleId].Target = 0;
                            AISetTarget(vehicleId, ref vehicle, 0);
                            vehicle.m_targetBuilding = 0;
                        }
                    }
                }

                if (!known)
                {
                    if (Log.LogALot) Log.DevDebug(this, "CheckVehicleTarget", "New", vehicleId, vehicle.m_targetBuilding);

                    AISetTarget(vehicleId, ref vehicle, 0);
                    vehicle.m_targetBuilding = 0;
                }
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
                foreach (BuldingCheckParameters bcParams in BuldingChecks)
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
        /// Initializes the building check parameters.
        /// </summary>
        public abstract void InitBuildingChecks();

        /// <summary>
        /// Determines whether vehicle is correct type of vehicle.
        /// </summary>
        /// <param name="vehicleInfo">The vehicle information.</param>
        /// <returns>True if vehicle is correct type.</returns>
        public abstract bool IsCorrectType(VehicleInfo vehicleInfo);

        /// <summary>
        /// Get capacity using the correct AI cast.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns></returns>
        protected abstract int AIGetCapacity(ref Vehicle vehicle);

        /// <summary>
        /// Set target with the vehicles AI.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="buildingId">The building identifier.</param>
        protected abstract void AISetTarget(ushort vehicleId, ref Vehicle vehicle, ushort buildingId);

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
            foreach (Buildings.ServiceBuildingInfo serviceBuilding in ServiceBuildings.Values)
            {
                serviceBuilding.SetTargetInfo(districtManager, targetBuilding, ignoreRange);
            }

            // Loop through service buildings in priority order and assign a vehicle to the target.
            foreach (Buildings.ServiceBuildingInfo serviceBuilding in ServiceBuildings.Values.Where(sb => sb.InRange).OrderBy(sb => sb, Global.ServiceBuildingInfoPriorityComparer))
            {
                ushort vehicleFoundId = 0;
                float vehicleFoundDistance = float.PositiveInfinity;

                // Loop through vehicles and save the closest free vehicle.
                foreach (Vehicles.ServiceVehicleInfo vehicleInfo in serviceBuilding.Vehicles.Values)
                {
                    if (vehicleInfo.CanCollect && vehicleInfo.Target == 0)
                    {
                        float distance = (targetBuilding.Position - vehicleInfo.Position).sqrMagnitude;

                        if (vehicleFoundId == 0 || distance < vehicleFoundDistance)
                        {
                            vehicleFoundId = vehicleInfo.VehicleId;
                            vehicleFoundDistance = distance;
                        }
                    }
                }

                if (vehicleFoundId != 0)
                {
                    // A free vehicle was found, assign it to the target.
                    targetBuilding.Handled = true;
                    if (Log.LogToFile) Log.Debug(this, "AssignVehicle", "Assign", targetBuilding.BuildingId, targetBuilding.HasProblem, targetBuilding.ProblemValue, vehicleFoundId, vehicleFoundDistance);

                    Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

                    AISetTarget(vehicleFoundId, ref vehicles[vehicleFoundId], targetBuilding.BuildingId);
                    vehicles[vehicleFoundId].m_targetBuilding = targetBuilding.BuildingId;

                    assignedTargets[targetBuilding.BuildingId] = Global.CurrentFrame;
                    serviceBuilding.Vehicles[vehicleFoundId].Target = targetBuilding.BuildingId;

                    freeVehicles--;

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

            // Loop through the service buildings.
            foreach (Buildings.ServiceBuildingInfo serviceBuilding in ServiceBuildings.Values)
            {
                // Loop through the vehicles.
                int count = 0;
                vehicleId = serviceBuilding.FirstOwnVehicleId;
                while (vehicleId != 0)
                {
                    // Add or update status for relevant vegicles.
                    if (vehicles[vehicleId].Info != null && (vehicles[vehicleId].m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Spawned)) != Vehicle.Flags.None && IsCorrectType(vehicles[vehicleId].Info))
                    {
                        // Check if vehicle is free to dispatch and has free space.
                        collecting = (vehicles[vehicleId].m_flags & Vehicle.Flags.TransferToSource) != Vehicle.Flags.None && (vehicles[vehicleId].m_flags & Vehicle.Flags.TransferToTarget) == Vehicle.Flags.None;
                        loading = (vehicles[vehicleId].m_flags & (Vehicle.Flags.Arriving | Vehicle.Flags.Stopped)) != Vehicle.Flags.None;
                        canCollect = collecting && !loading && vehicles[vehicleId].m_transferSize < AIGetCapacity(ref vehicles[vehicleId]);

                        // Update vehicle status.
                        if (serviceBuilding.Vehicles.ContainsKey(vehicleId))
                        {
                            if (collecting && !loading && vehicles[vehicleId].m_targetBuilding != 0 && vehicles[vehicleId].m_targetBuilding != serviceBuilding.Vehicles[vehicleId].Target)
                            {
                                if (Log.LogALot) Log.DevDebug(this, "CollectVehicles", "WrongTarget", vehicleId, vehicles[vehicleId].m_targetBuilding, serviceBuilding.Vehicles[vehicleId].Target);

                                AISetTarget(vehicleId, ref vehicles[vehicleId], 0);
                                vehicles[vehicleId].m_targetBuilding = 0;
                            }

                            serviceBuilding.Vehicles[vehicleId].Update(ref vehicles[vehicleId], canCollect);
                        }
                        else
                        {
                            if (collecting && !loading && vehicles[vehicleId].m_targetBuilding != 0)
                            {
                                AISetTarget(vehicleId, ref vehicles[vehicleId], 0);
                                vehicles[vehicleId].m_targetBuilding = 0;
                            }

                            Vehicles.ServiceVehicleInfo vehicle = new Vehicles.ServiceVehicleInfo(vehicleId, ref vehicles[vehicleId], canCollect);
                            if (Log.LogALot) Log.DevDebug(this, "CollectVehicles", "AddVehicle", serviceBuilding.BuildingId, vehicleId, vehicles[vehicleId].Info.name, vehicle.VehicleName, vehicle.CanCollect, collecting);

                            serviceBuilding.Vehicles[vehicleId] = vehicle;
                        }

                        // If target doesn't need service, deassign...
                        if (collecting && !loading && vehicles[vehicleId].m_targetBuilding != 0 && !TargetBuildings.ContainsKey(vehicles[vehicleId].m_targetBuilding))
                        {
                            if (Log.LogALot) Log.DevDebug(this, "CollectVehicles", "NoNeed", vehicleId, vehicles[vehicleId].m_targetBuilding);

                            AISetTarget(vehicleId, ref vehicles[vehicleId], 0);
                            vehicles[vehicleId].m_targetBuilding = 0;
                        }

                        // Update assigned target status.
                        if (collecting && vehicles[vehicleId].m_targetBuilding != 0)
                        {
                            if (Log.LogALot && !assignedTargets.ContainsKey(vehicles[vehicleId].m_targetBuilding)) Log.DevDebug(this, "CollectVehicles", "AddAssigned", serviceBuilding.BuildingId, vehicleId, vehicles[vehicleId].m_targetBuilding);

                            assignedTargets[vehicles[vehicleId].m_targetBuilding] = Global.CurrentFrame;
                        }
                        else if (canCollect)
                        {
                            freeVehicles++;
                        }
                    }

                    vehicleId = vehicles[vehicleId].m_nextOwnVehicle;
                    if (vehicleId == serviceBuilding.FirstOwnVehicleId)
                    {
                        break;
                    }

                    count++;
                    if (count > ushort.MaxValue)
                    {
                        throw new Exception("Loop counter too high!");
                    }
                }

                // Remove old vehicles.
                ushort[] removeVehicles = serviceBuilding.Vehicles.Values.Where(v => v.LastSeen != Global.CurrentFrame).Select(v => v.VehicleId).ToArray();
                foreach (ushort id in removeVehicles)
                {
                    if (vehicles[vehicleId].Info == null || (vehicles[vehicleId].m_flags & Vehicle.Flags.Spawned) != Vehicle.Flags.Spawned || !IsCorrectType(vehicles[vehicleId].Info))
                    {
                        if (Log.LogALot) Log.DevDebug(this, "CollectVehicles", "RemoveVehicle", serviceBuilding.BuildingId, id);

                        serviceBuilding.Vehicles.Remove(id);
                    }
                    else
                    {
                        if (vehicles[vehicleId].m_sourceBuilding != serviceBuilding.BuildingId)
                        {
                            if (Log.LogALot) Log.DevDebug(this, "CollectVehicles", "RemoveVehicle", serviceBuilding.BuildingId, id);

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
                    case Settings.BuildingCheckParameters.InRange:
                        this.OnlyProblematic = false;
                        this.IgnoreRange = false;
                        this.MinProblemValue = 0;
                        break;

                    case Settings.BuildingCheckParameters.Any:
                        this.OnlyProblematic = false;
                        this.IgnoreRange = true;
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

                    case Settings.BuildingCheckParameters.ForgottenInRange:
                        this.OnlyProblematic = false;
                        this.IgnoreRange = false;
                        this.MinProblemValue = ProblemForgottenLimit;
                        break;

                    case Settings.BuildingCheckParameters.ForgottenIgnoreRange:
                        this.OnlyProblematic = false;
                        this.IgnoreRange = true;
                        this.MinProblemValue = ProblemForgottenLimit;
                        break;

                    default:
                        this.OnlyProblematic = false;
                        this.IgnoreRange = true;
                        this.MinProblemValue = 0;
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