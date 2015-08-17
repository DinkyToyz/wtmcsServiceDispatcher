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
                if (Log.LogALot && Library.IsDebugBuild) Log.Debug(this, "Dispatch", "Dispatch");

                // Collect buildings with dead people that has not been checked or handled recently.
                foreach (Buildings.TargetBuildingInfo targetBuilding in TargetBuildings.Where(tb => tb.Updated).OrderBy(tb => tb, new Buildings.TargetBuildingInfoComparer()))
                {
                    if (!Checked[targetBuilding.BuildingId] && !Handled[targetBuilding.BuildingId])
                    {
                        // Assign vehicles.
                        AssignVehicle(targetBuilding);
                        Checked[targetBuilding.BuildingId] = true;
                    }
                }
            }

            // Clean data.
            Clean();
        }

        /// <summary>
        /// Assigns a vehicle to a target building.
        /// </summary>
        /// <param name="targetBuilding">The target building.</param>
        protected void AssignVehicle(Buildings.TargetBuildingInfo targetBuilding)
        {
            // Get target district.
            DistrictManager districtManager = null;
            byte targetDistrict = 0;
            if (Global.Settings.DispatchByDistrict && districtManager != null)
            {
                districtManager = Singleton<DistrictManager>.instance;
                targetDistrict = districtManager.GetDistrict(targetBuilding.Position);
            }

            if (Log.LogALot && Library.IsDebugBuild) Log.Debug(this, "AssignVehicle", "TargetBuilding", targetBuilding.BuildingId, targetDistrict);

            
            // Set target info on service buildings.
            foreach (Buildings.ServiceBuildingInfo serviceBuilding in ServiceBuildings)
            {
                serviceBuilding.SetTargetInfo(districtManager, targetBuilding);

                if (Log.LogALot && Library.IsDebugBuild) Log.Debug(this, "AssignVehicle", "ServiceBuilding", "Set", serviceBuilding.BuildingId, serviceBuilding.District, serviceBuilding.InDistrict, serviceBuilding.Distance, serviceBuilding.InRange);
            }

            //Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            // Loop through service buildings in priority order and assign a vehicle to the target.
            foreach (Buildings.ServiceBuildingInfo serviceBuilding in ServiceBuildings.Where(sb => sb.InRange).OrderBy(sb => sb, new Buildings.ServiceBuildingInfoComparer()))
            {
                if (Log.LogALot && Library.IsDebugBuild) Log.Debug(this, "AssignVehicle", "ServiceBuilding", "Check", serviceBuilding.BuildingId, serviceBuilding.District, serviceBuilding.InDistrict, serviceBuilding.Distance);

                ushort vehicleFoundId = 0;
                float vehicleFoundDistance = float.PositiveInfinity;

                // Loop through vehicles and save the closest free vehicle.
                ushort vehicleId = serviceBuilding.FirstOwnVehicle;
                while (vehicleId != 0)
                {
                    Vehicle vehicle = vehicles[vehicleId];

                    if (vehicle.Info != null && vehicle.m_targetBuilding == 0 && (vehicle.m_flags & Vehicle.Flags.Spawned) == Vehicle.Flags.None && IsMyType(vehicle.Info))
                    {
                        float distance = (targetBuilding.Position - vehicle.GetLastFramePosition()).sqrMagnitude;
                        if (Log.LogALot && Library.IsDebugBuild) Log.Debug(this, "AssignVehicle", "Vehicle", "Check", vehicleId, distance);

                        if (vehicleFoundId == 0 || distance < vehicleFoundDistance)
                        {
                            vehicleFoundId = vehicleId;
                            vehicleFoundDistance = distance;

                            if (Log.LogALot && Library.IsDebugBuild) Log.Debug(this, "AssignVehicle", "Vehicle", "Save", vehicleId, distance);
                        }
                    }

                    vehicleId = vehicle.m_nextOwnVehicle;
                    if (vehicleId == serviceBuilding.FirstOwnVehicle)
                    {
                        break;
                    }
                }

                if (vehicleFoundId != 0)
                {
                    // A free vehicle was found, assign it to the target.
                    Handled[targetBuilding.BuildingId] = true;
                    if (Log.LogALot && Library.IsDebugBuild) Log.Debug(this, "AssignVehicle", "Assign", targetBuilding.BuildingId, vehicleId, vehicleFoundDistance);

                    if (!IsPretending)
                    {
                        vehicles[vehicleId].m_targetBuilding = targetBuilding.BuildingId;
                    }
                }
            }
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
                    return (interval > 0 && stamps.ContainsKey(targetId) && Global.CurrentFrame - stamps[targetId] < interval);
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