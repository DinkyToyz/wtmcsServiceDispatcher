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
        /// The checked building frame stamps.
        /// </summary>
        protected FrameStamps Checked = null;

        /// <summary>
        /// The handled building frame stamps.
        /// </summary>
        protected FrameStamps Handled = null;

        /// <summary>
        /// The dispatcher is just pretending to dispatch.
        /// </summary>
        protected bool IsPretending = false;

        /// <summary>
        /// Last time data was cleaned.
        /// </summary>
        private uint lastClean = 0;

        /// <summary>
        /// The service buildings.
        /// </summary>
        private Dictionary<ushort, Buildings.serviceBuildingInfo> serviceBuildings = new Dictionary<ushort, Buildings.serviceBuildingInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Dispatcher"/> class.
        /// </summary>
        /// <param name="doPretend">if set to <c>true</c> [pretend].</param>
        public Dispatcher(bool doPretend = false)
        {
            Log.Debug(this, "Constructor", "Begin");

            IsPretending = doPretend;

            if (doPretend)
            {
                // Use intervals for pretending.
                Checked = new FrameStamps(Global.PretendRecheckInterval);
                Handled = new FrameStamps(Global.PretendRecheckHandledInterval);
            }
            else
            {
                // Use normal intervals.
                Checked = new FrameStamps(Global.RecheckInterval);
                Handled = new FrameStamps(Global.RecheckHandledInterval);
            }

            Log.Debug(this, "Constructor", "End");
        }

        /// <summary>
        /// Gets a value indicating whether this service has target buildings.
        /// </summary>
        /// <value>
        /// <c>true</c> if this service has target buildings; otherwise, <c>false</c>.
        /// </value>
        protected abstract bool HasTargetBuildings { get; }

        /// <summary>
        /// Gets the target buildings.
        /// </summary>
        /// <value>
        /// The target buildings.
        /// </value>
        protected abstract IEnumerable<ushort> TargetBuildings { get; }

        /// <summary>
        /// Dispatches vehicles to targets.
        /// </summary>
        public virtual void Dispatch()
        {
            // Dispatch, if needed.
            if (HasTargetBuildings)
            {
                if (Log.LogALot && Library.IsDebugBuild) Log.Debug(this, "Dispatch", "Dispatch");

                Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
                bool initialized = false;

                // Collect buildings with dead people that has not been checked or handled recently.
                foreach (ushort targetId in TargetBuildings)
                {
                    if (!Checked[targetId] && !Handled[targetId])
                    {
                        if (!initialized)
                        {
                            // Initialize buildings with vehicles.
                            Initialize();
                        }

                        // Assign vehicles.
                        AssignVehicle(targetId, buildings[targetId]);
                        Checked[targetId] = true;
                    }
                }
            }

            // Clean data.
            Clean();
        }

        /// <summary>
        /// Assigns a vehicle to a target building.
        /// </summary>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <param name="targetBuilding">The target building.</param>
        protected void AssignVehicle(ushort targetBuildingId, Building targetBuilding)
        {
            // Get target district.
            DistrictManager districtManager = null;
            byte targetDistrict = 0;
            if (Global.Settings.DispatchByDistrict && districtManager != null)
            {
                districtManager = Singleton<DistrictManager>.instance;
                targetDistrict = districtManager.GetDistrict(targetBuilding.m_position);
            }

            if (Log.LogALot && Library.IsDebugBuild) Log.Debug(this, "AssignVehicle", "TargetBuilding", targetBuildingId, targetDistrict);

            // Set target info on service buildings.
            foreach (Buildings.serviceBuildingInfo serviceBuilding in serviceBuildings.Values)
            {
                serviceBuilding.SetTargetInfo(districtManager, targetBuildingId, targetBuilding);

                if (Log.LogALot && Library.IsDebugBuild) Log.Debug(this, "AssignVehicle", "ServiceBuilding", "Set", serviceBuilding.BuildingId, serviceBuilding.District, serviceBuilding.InDistrict, serviceBuilding.Distance);
            }

            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            // Loop through service buildings in priority order and assign a vehicle to the target.
            foreach (Buildings.serviceBuildingInfo serviceBuilding in serviceBuildings.Values.OrderBy(i => i, new Buildings.serviceBuildingInfoComparer()))
            {
                if (Log.LogALot && Library.IsDebugBuild) Log.Debug(this, "AssignVehicle", "ServiceBuilding", "Check", serviceBuilding.BuildingId, serviceBuilding.District, serviceBuilding.InDistrict, serviceBuilding.Distance);

                Vehicles.ServiceVehicleInfo vehicleInfo = null;
                float vehicleDistance = float.PositiveInfinity;
                
                // Loop through vehicles and save the closest free vehicle.
                foreach (Vehicles.ServiceVehicleInfo vehicle in serviceBuilding.Vehicles)
                {
                    if (IsMyType(vehicles[vehicle.VehicleId].Info) && vehicles[vehicle.VehicleId].m_targetBuilding == 0)
                    {
                        float distance = (targetBuilding.m_position - vehicle.Position).sqrMagnitude;

                        if (Log.LogALot && Library.IsDebugBuild) Log.Debug(this, "AssignVehicle", "Vehicle", "Check", vehicle.VehicleId, distance);

                        if (vehicleInfo == null || distance < vehicleDistance)
                        {
                            vehicleInfo = vehicle;
                            vehicleDistance = distance;

                            if (Log.LogALot && Library.IsDebugBuild) Log.Debug(this, "AssignVehicle", "Vehicle", "Save", vehicle.VehicleId, distance);
                        }
                    }
                }

                if (vehicleInfo != null)
                {
                    // A free vehicle was found, assign it to the target.

                    Handled[targetBuildingId] = true;

                    if (Log.LogALot && Library.IsDebugBuild) Log.Debug(this, "AssignVehicle", "Assign", targetBuildingId, vehicleInfo.SourceBuilding, vehicleInfo.VehicleId, vehicleDistance);

                    if (!IsPretending)
                    {
                        vehicles[vehicleInfo.VehicleId].m_targetBuilding = targetBuildingId;
                    }
                }
            }
        }

        /// <summary>
        /// Assigns vehicles to targets.
        /// </summary>
        /// <param name="targets">The targets.</param>
        protected void AssignVehicles(ushort[] targets)
        {
            if (Library.IsDebugBuild) Log.Debug(this, "AssignVehicles", "Begin");

            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            // Loop through targets and assign vehicles.
            foreach (ushort target in targets)
            {
                AssignVehicle(target, buildings[target]);
            }

            if (Library.IsDebugBuild) Log.Debug(this, "AssignVehicles", "End");
        }

        /// <summary>
        /// Cleans the data.
        /// </summary>
        protected void Clean()
        {
            if (Global.CurrentFrame - lastClean < 300)
            {
                return;
            }

            Handled.Clean();
            Checked.Clean();

            lastClean = Global.CurrentFrame;
        }

        /// <summary>
        /// Initializes the service buidlings with vehicles.
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// Initializes the specified service buildings with vehicles.
        /// </summary>
        /// <param name="serviceBuildings">The service buildings.</param>
        /// <param name="serviceVehicles">The service vehicles.</param>
        protected void Initialize(IEnumerable<ushort> serviceBuildings, IEnumerable<Vehicles.ServiceVehicleInfo> serviceVehicles)
        {
            if (Library.IsDebugBuild) Log.Debug(this, "Initialize", "Begin");

            DistrictManager districtManager = Singleton<DistrictManager>.instance;
            Building[] buildingBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            this.serviceBuildings.Clear();

            // Collect building info.
            foreach (ushort serviceBuildingId in serviceBuildings)
            {
                if (Library.IsDebugBuild) Log.Debug(this, "Initialize", "serviceBuildings.Add", serviceBuildingId);

                this.serviceBuildings.Add(serviceBuildingId, new Buildings.serviceBuildingInfo(districtManager, serviceBuildingId, buildingBuffer[serviceBuildingId]));
            }

            // Collect vehicles for buildings.
            foreach (Vehicles.ServiceVehicleInfo vehicle in serviceVehicles)
            {
                if (serviceBuildings.Contains(vehicle.SourceBuilding))
                {
                    if (Library.IsDebugBuild) Log.Debug(this, "Initialize", "serviceBuildings.Vehicles.Add", vehicle.SourceBuilding, vehicle.VehicleId);

                    this.serviceBuildings[vehicle.SourceBuilding].Vehicles.Add(vehicle);
                }
            }

            if (Library.IsDebugBuild) Log.Debug(this, "Initialize", "End");
        }

        /// <summary>
        /// Determines whether vehicle is correct type of vehicle.
        /// </summary>
        /// <param name="vehicleInfo">The vehicle information.</param>
        /// <returns>True if vehicle is correct type.</returns>
        protected abstract bool IsMyType(VehicleInfo vehicleInfo);

        /// <summary>
        /// Building frame stamps.
        /// </summary>
        protected class FrameStamps
        {
            /// <summary>
            /// The interval.
            /// </summary>
            private uint interval;

            /// <summary>
            /// The stamps.
            /// </summary>
            private Dictionary<ushort, uint> stamps = new Dictionary<ushort, uint>();

            /// <summary>
            /// Initializes a new instance of the <see cref="FrameStamps"/> class.
            /// </summary>
            /// <param name="interval">The interval.</param>
            public FrameStamps(uint interval)
            {
                this.interval = interval;
            }

            /// <summary>
            /// Gets or sets the <see cref="System.Boolean"/> with the specified target identifier.
            /// </summary>
            /// <value>
            /// The <see cref="System.Boolean"/>.
            /// </value>
            /// <param name="targetId">The target identifier.</param>
            /// <returns></returns>
            public bool this[ushort targetId]
            {
                get
                {
                    if (!stamps.ContainsKey(targetId))
                    {
                        return false;
                    }

                    if (Global.CurrentFrame - stamps[targetId] < interval)
                    {
                        return true;
                    }

                    return false;
                }
                set
                {
                    if (value)
                    {
                        stamps[targetId] = Global.CurrentFrame;
                    }
                    else
                    {
                        stamps.Remove(targetId);
                    }
                }
            }

            /// <summary>
            /// Cleans the stamps.
            /// </summary>
            public void Clean()
            {
                KeyValuePair<ushort, uint>[] stampkvs = stamps.ToArray();
                foreach (KeyValuePair<ushort, uint> stamp in stampkvs)
                {
                    if (Global.CurrentFrame - stamp.Value > interval * 10)
                    {
                        stamps.Remove(stamp.Key);
                    }
                }
            }
        }
    }
}