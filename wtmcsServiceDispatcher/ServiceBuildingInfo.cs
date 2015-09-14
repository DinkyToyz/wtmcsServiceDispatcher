using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Info about a service building.
    /// </summary>
    internal class ServiceBuildingInfo
    {
        /// <summary>
        /// The building identifier.
        /// </summary>
        public ushort BuildingId;

        /// <summary>
        /// The building can receive.
        /// </summary>
        public bool CanReceive;

        /// <summary>
        /// The distance to the target.
        /// </summary>
        public float Distance = float.PositiveInfinity;

        /// <summary>
        /// The district the building is in.
        /// </summary>
        public byte District;

        /// <summary>
        /// The buildings first own vehicle.
        /// </summary>
        public ushort FirstOwnVehicleId;

        /// <summary>
        /// The building is in target district.
        /// </summary>
        public bool InDistrict = false;

        /// <summary>
        /// The target building is in service building's effective range.
        /// </summary>
        public bool InRange = true;

        /// <summary>
        /// The building's position.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The building's effect range.
        /// </summary>
        public float Range = 0;

        /// <summary>
        /// The vehicles.
        /// </summary>
        public Dictionary<ushort, ServiceVehicleInfo> Vehicles = new Dictionary<ushort, ServiceVehicleInfo>();

        /// <summary>
        /// The free vehicles count.
        /// </summary>
        public int VehiclesFree = 0;

        /// <summary>
        /// The last info update stamp.
        /// </summary>
        private uint lastInfoUpdate = 0;

        /// <summary>
        /// The last update stamp.
        /// </summary>
        private uint lastUpdate = 0;

        /// <summary>
        /// The vehicle in use count.
        /// </summary>
        private int vehiclesMade = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBuildingInfo"/> class.
        /// </summary>
        /// <param name="districtManager">The district manager.</param>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        public ServiceBuildingInfo(DistrictManager districtManager, ushort buildingId, ref Building building)
        {
            this.BuildingId = buildingId;

            this.Update(districtManager, ref building);
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
        /// Gets the name of the district.
        /// </summary>
        /// <value>
        /// The name of the district.
        /// </value>
        public string DistrictName
        {
            get
            {
                return BuildingHelper.GetDistrictName(this.District);
            }
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
        public int VehiclesSpare { get; private set; }

        /// <summary>
        /// Gets the total vehicles count.
        /// </summary>
        public int VehiclesTotal { get; private set; }

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
            ServiceVehicleInfo serviceVehicle = VehicleHelper.CreateServiceVehicle(this, (TransferManager.TransferReason)transferType);
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
        /// Sets the target information.
        /// </summary>
        /// <param name="districtManager">The district manager.</param>
        /// <param name="building">The building.</param>
        /// <param name="ignoreRange">If set to <c>true</c> ignore the range.</param>
        public void SetTargetInfo(DistrictManager districtManager, TargetBuildingInfo building, bool ignoreRange)
        {
            this.Distance = (this.Position - building.Position).sqrMagnitude;

            if (Global.Settings.DispatchByDistrict && districtManager != null)
            {
                byte district = districtManager.GetDistrict(building.Position);
                this.InDistrict = district == this.District;
            }
            else
            {
                this.InDistrict = false;
            }

            this.InRange = ignoreRange || this.InDistrict || (Global.Settings.DispatchByRange && this.Distance < this.Range) || (!Global.Settings.DispatchByDistrict && !Global.Settings.DispatchByRange);
        }

        /// <summary>
        /// Updates the building info.
        /// </summary>
        /// <param name="districtManager">The district manager.</param>
        /// <param name="building">The building.</param>
        public void Update(DistrictManager districtManager, ref Building building)
        {
            this.lastUpdate = Global.CurrentFrame;

            this.Position = building.m_position;
            this.FirstOwnVehicleId = building.m_ownVehicles;

            if (this.lastInfoUpdate == 0 || Global.CurrentFrame - this.lastInfoUpdate > Global.ObjectUpdateInterval)
            {
                if (districtManager != null)
                {
                    this.District = districtManager.GetDistrict(this.Position);
                }
                else if (this.lastInfoUpdate == 0)
                {
                    this.District = 0;
                }

                if (Global.Settings.DispatchByRange)
                {
                    this.Range = building.Info.m_buildingAI.GetCurrentRange(this.BuildingId, ref building);
                    this.Range = this.Range * this.Range * Global.Settings.RangeModifier;
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

                int productionRate = PlayerBuildingAI.GetProductionRate(100, Singleton<EconomyManager>.instance.GetBudget(building.Info.m_buildingAI.m_info.m_class));
                if (building.Info.m_buildingAI is CemeteryAI)
                {
                    this.VehiclesTotal = ((CemeteryAI)building.Info.m_buildingAI).m_hearseCount;
                }
                else if (building.Info.m_buildingAI is LandfillSiteAI)
                {
                    this.VehiclesTotal = ((LandfillSiteAI)building.Info.m_buildingAI).m_garbageTruckCount;
                }
                this.VehiclesTotal = ((productionRate * this.VehiclesTotal) + 99) / 100;

                this.lastInfoUpdate = Global.CurrentFrame;
            }

            this.CanReceive = (building.m_flags & (Building.Flags.CapacityFull | Building.Flags.Downgrading | Building.Flags.Demolishing | Building.Flags.Deleted | Building.Flags.BurnedDown)) == Building.Flags.None &&
                              (building.m_flags & (Building.Flags.Created | Building.Flags.Created | Building.Flags.Active)) == (Building.Flags.Created | Building.Flags.Created | Building.Flags.Active) &&
                              (building.m_problems & (Notification.Problem.Emptying | Notification.Problem.LandfillFull | Notification.Problem.RoadNotConnected | Notification.Problem.TurnedOff | Notification.Problem.FatalProblem)) == Notification.Problem.None &&
                              !building.Info.m_buildingAI.IsFull(this.BuildingId, ref building);
        }

        /// <summary>
        /// Compares service buildings for priority sorting.
        /// </summary>
        public class PriorityComparer : IComparer<ServiceBuildingInfo>
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
                else
                {
                    float s = x.Distance - y.Distance;
                    if (s < 0)
                    {
                        return -1;
                    }
                    else if (s > 0)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }
    }
}
