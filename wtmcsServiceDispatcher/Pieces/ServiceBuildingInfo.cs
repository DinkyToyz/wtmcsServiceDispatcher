using System.Collections.Generic;
using ColossalFramework;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Info about a service building.
    /// </summary>
    internal class ServiceBuildingInfo : IBuildingInfo
    {
        /// <summary>
        /// Dispatch services by district.
        /// </summary>
        private bool dispatchByDistrict = false;

        /// <summary>
        /// Dispatch services by range.
        /// </summary>
        private bool dispatchByRange = true;

        /// <summary>
        /// The dispatcher type.
        /// </summary>
        private Dispatcher.DispatcherTypes dispatcherType = Dispatcher.DispatcherTypes.None;

        /// <summary>
        /// The last capacity update stamp.
        /// </summary>
        private uint lastCapacityUpdate = 0;

        /// <summary>
        /// The last info update stamp.
        /// </summary>
        private uint lastInfoUpdate = 0;

        /// <summary>
        /// The last update stamp.
        /// </summary>
        private uint lastUpdate = 0;

        /// <summary>
        /// The original range.
        /// </summary>
        private float orgRange = 0;

        /// <summary>
        /// The vehicle in use count.
        /// </summary>
        private int vehiclesMade = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBuildingInfo" /> class.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <param name="dispatcherType">Type of the dispatcher.</param>
        public ServiceBuildingInfo(ushort buildingId, ref Building building, Dispatcher.DispatcherTypes dispatcherType)
        {
            this.BuildingId = buildingId;
            this.Distance = float.PositiveInfinity;
            this.InDistrict = false;
            this.InRange = true;
            this.Range = 0;
            this.Vehicles = new Dictionary<ushort, ServiceVehicleInfo>();
            this.VehiclesFree = 0;
            this.dispatcherType = dispatcherType;

            this.Initialize();

            this.Update(ref building);
        }

        /// <summary>
        /// Capacity level value.
        /// </summary>
        public enum CapacityLevels
        {
            /// <summary>
            /// The building is empty.
            /// </summary>
            Empty = 1,

            /// <summary>
            /// The building has a high free capacity.
            /// </summary>
            High = 2,

            /// <summary>
            /// The building has a medium free capacity.
            /// </summary>
            Medium = 3,

            /// <summary>
            /// The building has a low free capacity.
            /// </summary>
            Low = 4,

            /// <summary>
            /// The building is full.
            /// </summary>
            Full = 5
        }

        /// <summary>
        /// Gets the CS building.
        /// </summary>
        /// <value>
        /// The CS building.
        /// </value>
        public Building Building
        {
            get
            {
                return BuildingHelper.GetBuilding(this.BuildingId);
            }
        }

        /// <summary>
        /// Gets the building identifier.
        /// </summary>
        public ushort BuildingId
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the CS building information.
        /// </summary>
        /// <value>
        /// The building CS information.
        /// </value>
        public BuildingInfo BuildingInfo
        {
            get
            {
                return BuildingHelper.GetBuildingInfo(this.BuildingId);
            }
        }

        /// <summary>
        /// Gets the name of the building.
        /// </summary>
        /// <value>
        /// The name of the building.
        /// </value>
        public string BuildingName
        {
            get
            {
                return BuildingHelper.GetBuildingName(this.BuildingId);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the building can receive anything.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can receive; otherwise, <c>false</c>.
        /// </value>
        public bool CanReceive
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the free capacity.
        /// </summary>
        /// <value>
        /// The free capacity.
        /// </value>
        public int CapacityFree
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the capacity level.
        /// </summary>
        /// <value>
        /// The capacity level.
        /// </value>
        public CapacityLevels CapacityLevel
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the capacity maximum.
        /// </summary>
        /// <value>
        /// The capacity maximum.
        /// </value>
        public int CapacityMax
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or the capacity overflow.
        /// </summary>
        /// <value>
        /// The capacity overflow.
        /// </value>
        public int CapacityOverflow
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the distance to the target.
        /// </summary>
        public float Distance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the district the building is in.
        /// </summary>
        public byte District
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the district.
        /// </summary>
        /// <value>
        /// The name of the district.
        /// </value>
        public string DistrictName
        {
            get
            {
                return DistrictHelper.GetDistrictName(this.District);
            }
        }

        /// <summary>
        /// Gets or sets the first link in the buildings list of own vehicles.
        /// </summary>
        public ushort FirstOwnVehicleId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the building is in target district.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [in district]; otherwise, <c>false</c>.
        /// </value>
        public bool InDistrict
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the target building is in service building's effective range.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [in range]; otherwise, <c>false</c>.
        /// </value>
        public bool InRange
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the building's position.
        /// </summary>
        public Vector3 Position
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the building's effect range.
        /// </summary>
        public float Range
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the building is updated.
        /// </summary>
        public bool Updated
        {
            get
            {
                return this.lastUpdate == Global.CurrentFrame;
            }
        }

        /// <summary>
        /// Gets the building's vehicles.
        /// </summary>
        public Dictionary<ushort, ServiceVehicleInfo> Vehicles
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the free vehicles count.
        /// </summary>
        public int VehiclesFree
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the vehicle in use count.
        /// </summary>
        /// <value>
        /// The vehicle in use count.
        /// </value>
        public int VehiclesMade
        {
            get
            {
                return this.vehiclesMade;
            }

            set
            {
                this.vehiclesMade = value;
                this.VehiclesSpare = this.VehiclesTotal - this.vehiclesMade;
            }
        }

        /// <summary>
        /// Gets the spare vehicles count.
        /// </summary>
        public int VehiclesSpare
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the total vehicles count.
        /// </summary>
        public int VehiclesTotal
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates the vehicle.
        /// </summary>
        /// <param name="transferType">Type of the transfer.</param>
        /// <returns>The vehicle identifier.</returns>
        public ushort CreateVehicle(byte transferType)
        {
            if (this.VehiclesSpare < 1)
            {
                return 0;
            }

            ServiceVehicleInfo serviceVehicle = ServiceVehicleInfo.Create(this, (TransferManager.TransferReason)transferType);
            if (serviceVehicle == null)
            {
                return 0;
            }

            this.VehiclesSpare--;
            this.VehiclesFree++;
            this.VehiclesMade++;
            this.Vehicles[serviceVehicle.VehicleId] = serviceVehicle;

            return serviceVehicle.VehicleId;
        }

        /// <summary>
        /// Reinitializes this instance.
        /// </summary>
        public void ReInitialize()
        {
            this.Initialize();
        }

        /// <summary>
        /// Sets the target information.
        /// </summary>
        /// <param name="building">The building.</param>
        /// <param name="ignoreRange">If set to <c>true</c> ignore the range.</param>
        public void SetTargetInfo(TargetBuildingInfo building, bool ignoreRange)
        {
            if (this.lastUpdate != Global.CurrentFrame)
            {
                this.Update(ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[this.BuildingId], true);
            }

            this.Distance = (this.Position - building.Position).sqrMagnitude;

            if (this.dispatchByDistrict)
            {
                byte district = Singleton<DistrictManager>.instance.GetDistrict(building.Position);
                this.InDistrict = district == this.District;
            }
            else
            {
                this.InDistrict = false;
            }

            this.InRange = ignoreRange || this.InDistrict || (this.dispatchByRange && this.Distance < this.Range) || (!this.dispatchByDistrict && !this.dispatchByRange);

            this.CapacityOverflow = (this.CapacityFree >= building.ProblemSize) ? 0 : building.ProblemSize - this.CapacityFree;
        }

        /// <summary>
        /// Updates the building info.
        /// </summary>
        /// <param name="building">The building.</param>
        /// <param name="checkCapacityInterval">If set to <c>true</c> update capacity if outside capacity interval.</param>
        public void Update(ref Building building, bool checkCapacityInterval = false)
        {
            this.lastUpdate = Global.CurrentFrame;

            this.Position = building.m_position;
            this.FirstOwnVehicleId = building.m_ownVehicles;

            bool didUpdate = this.UpdateValues(ref building);

            if (didUpdate || this.lastCapacityUpdate == 0 || (checkCapacityInterval && Global.CurrentFrame - this.lastCapacityUpdate > Global.CapacityUpdateInterval))
            {
                int max;
                int amount;
                int productionRate = PlayerBuildingAI.GetProductionRate(100, Singleton<EconomyManager>.instance.GetBudget(building.Info.m_buildingAI.m_info.m_class));
                if (building.Info.m_buildingAI is CemeteryAI)
                {
                    this.VehiclesTotal = ((CemeteryAI)building.Info.m_buildingAI).m_hearseCount;
                    building.Info.m_buildingAI.GetMaterialAmount(this.BuildingId, ref building, TransferManager.TransferReason.Dead, out amount, out max);
                }
                else if (building.Info.m_buildingAI is LandfillSiteAI)
                {
                    this.VehiclesTotal = ((LandfillSiteAI)building.Info.m_buildingAI).m_garbageTruckCount;
                    building.Info.m_buildingAI.GetMaterialAmount(this.BuildingId, ref building, TransferManager.TransferReason.Garbage, out amount, out max);
                }
                else
                {
                    this.VehiclesTotal = 0;
                    amount = 0;
                    max = 0;
                }
                this.VehiclesTotal = ((productionRate * this.VehiclesTotal) + 99) / 100;
                this.CapacityMax = max;
                this.CapacityFree = max - amount;

                didUpdate = true;
                this.lastCapacityUpdate = Global.CurrentFrame;
            }

            this.CanReceive = (building.m_flags & (Building.Flags.CapacityFull | Building.Flags.Downgrading | Building.Flags.Demolishing | Building.Flags.Deleted | Building.Flags.BurnedDown)) == Building.Flags.None &&
                              (building.m_flags & (Building.Flags.Created | Building.Flags.Completed)) == (Building.Flags.Created | Building.Flags.Completed) &&
                              (building.m_problems & (Notification.Problem.Emptying | Notification.Problem.LandfillFull | Notification.Problem.RoadNotConnected | Notification.Problem.TurnedOff | Notification.Problem.FatalProblem)) == Notification.Problem.None &&
                              this.CapacityFree > 0 && !building.Info.m_buildingAI.IsFull(this.BuildingId, ref building);

            if ((building.m_flags & Building.Flags.CapacityFull) == Building.Flags.CapacityFull)
            {
                this.CapacityLevel = CapacityLevels.Full;
            }
            else if ((building.m_flags & Building.Flags.CapacityStep1) == Building.Flags.CapacityStep1)
            {
                this.CapacityLevel = CapacityLevels.Low;
            }
            else if ((building.m_flags & Building.Flags.CapacityStep2) == Building.Flags.CapacityStep2)
            {
                this.CapacityLevel = CapacityLevels.Medium;
            }
            else if (this.CapacityFree == this.CapacityMax)
            {
                this.CapacityLevel = CapacityLevels.Empty;
            }
            else
            {
                this.CapacityLevel = CapacityLevels.High;
            }

            if (Log.LogALot && !this.CanReceive)
            {
                Log.DevDebug(this, "Update", this.BuildingId, this.BuildingName, building.m_flags, building.m_problems, this.CapacityFree, building.Info.m_buildingAI.IsFull(this.BuildingId, ref building));
            }
        }

        /// <summary>
        /// Updates the building values.
        /// </summary>
        /// <param name="building">The building.</param>
        /// <param name="ignoreInterval">If set to <c>true</c> ignore object update interval.</param>
        /// <returns>
        /// True if updated.
        /// </returns>
        public bool UpdateValues(ref Building building, bool ignoreInterval = false)
        {
            if (this.lastInfoUpdate == 0 || (ignoreInterval && this.lastInfoUpdate != Global.CurrentFrame) || Global.CurrentFrame - this.lastInfoUpdate > Global.ObjectUpdateInterval)
            {
                if (this.dispatchByDistrict)
                {
                    this.District = Singleton<DistrictManager>.instance.GetDistrict(this.Position);
                }
                else if (this.lastInfoUpdate == 0)
                {
                    this.District = 0;
                }

                if (this.dispatchByRange)
                {
                    this.orgRange = building.Info.m_buildingAI.GetCurrentRange(this.BuildingId, ref building);
                    this.Range = this.orgRange * this.orgRange * Global.Settings.RangeModifier;
                    if (Global.Settings.RangeLimit)
                    {
                        if (this.Range < Global.Settings.RangeMinimum)
                        {
                            this.Range = Global.Settings.RangeMinimum;
                        }
                        else if (this.Range > Global.Settings.RangeMaximum)
                        {
                            this.Range = Global.Settings.RangeMaximum;
                        }
                    }
                }

                this.lastInfoUpdate = Global.CurrentFrame;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Initialize()
        {
            switch (this.dispatcherType)
            {
                case Dispatcher.DispatcherTypes.HearseDispatcher:
                    this.dispatchByDistrict = Global.Settings.DispatchHearsesByDistrict;
                    this.dispatchByRange = Global.Settings.DispatchHearsesByRange;
                    break;

                case Dispatcher.DispatcherTypes.GarbageTruckDispatcher:
                    this.dispatchByDistrict = Global.Settings.DispatchGarbageTrucksByDistrict;
                    this.dispatchByRange = Global.Settings.DispatchGarbageTrucksByRange;
                    break;
            }
        }

        /// <summary>
        /// Compares service buildings for priority sorting.
        /// </summary>
        public class PriorityComparer : IComparer<ServiceBuildingInfo>, IHandlerPart
        {
            /// <summary>
            /// Compares two buildings and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// </summary>
            /// <param name="x">The first buildings to compare.</param>
            /// <param name="y">The second buildings to compare.</param>
            /// <returns>
            /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.
            /// </returns>
            public int Compare(ServiceBuildingInfo x, ServiceBuildingInfo y)
            {
                if (x.InDistrict && !y.InDistrict)
                {
                    return -1;
                }
                else if (y.InDistrict && !x.InDistrict)
                {
                    return 1;
                }

                int c;
                float s, d;

                c = x.CapacityOverflow - y.CapacityOverflow;
                if (c < 0)
                {
                    return -1;
                }
                else if (c > 0)
                {
                    return 1;
                }

                d = Mathf.Min(x.Distance / 100.0f, y.Distance / 100.0f);
                s = x.Distance - y.Distance;
                if (s < -d)
                {
                    return -1;
                }
                else if (s > d)
                {
                    return 1;
                }

                c = x.CapacityLevel - y.CapacityLevel;
                if (c < 0)
                {
                    return -1;
                }
                else if (c > 0)
                {
                    return 1;
                }

                s = x.Distance - y.Distance;
                if (s < 0)
                {
                    return -1;
                }
                else if (s > 0)
                {
                    return 1;
                }

                return 0;
            }

            /// <summary>
            /// Re-initialize the part.
            /// </summary>
            public void ReInitialize()
            {
            }
        }
    }
}