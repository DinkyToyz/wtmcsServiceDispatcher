using ColossalFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Info about a service building.
    /// </summary>
    internal class ServiceBuildingInfo : IBuildingInfo
    {
        /// <summary>
        /// The dispatcher type.
        /// </summary>
        private ServiceHelper.ServiceType serviceType = ServiceHelper.ServiceType.None;

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
        /// The service settings.
        /// </summary>
        private StandardServiceSettings serviceSettings = null;

        /// <summary>
        /// The free vehicles count.
        /// </summary>
        private int vehiclesFree = 0;

        /// <summary>
        /// The vehicle in use count.
        /// </summary>
        private int vehiclesMade = 0;

        /// <summary>
        /// The spare vehicles count.
        /// </summary>
        private int vehiclesSpare = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBuildingInfo" /> class.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <param name="serviceType">Type of the dispatcher.</param>
        public ServiceBuildingInfo(ushort buildingId, ref Building building, ServiceHelper.ServiceType serviceType)
        {
            this.BuildingId = buildingId;
            this.CurrentTargetDistance = float.PositiveInfinity;
            this.CurrentTargetInDistrict = false;
            this.CurrentTargetInRange = true;
            this.CurrentTargetCapacityOverflow = 0;
            this.CurrentTargetServiceProblemSize = 0;
            this.Range = 0;
            this.Vehicles = new Dictionary<ushort, ServiceVehicleInfo>();
            this.serviceType = serviceType;
            this.IsAutoEmptying = false;

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
        /// Gets a value indicating whether this instance can be emptied.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can be emptied; otherwise, <c>false</c>.
        /// </value>
        public bool CanBeEmptied
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance can empty other.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can empty other; otherwise, <c>false</c>.
        /// </value>
        public bool CanEmptyOther
        {
            get;
            private set;
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
        /// Gets the capacity level.
        /// </summary>
        /// <value>
        /// The capacity level.
        /// </value>
        public float CapacityPercent
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance can dispatch.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can dispatch; otherwise, <c>false</c>.
        /// </value>
        public bool CurrentTargetCanCreateSpares
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance can dispatch.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can dispatch; otherwise, <c>false</c>.
        /// </value>
        public bool CurrentTargetCanDispatch
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
        public int CurrentTargetCapacityOverflow
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the distance to the target.
        /// </summary>
        public float CurrentTargetDistance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the building is in target district.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [in district]; otherwise, <c>false</c>.
        /// </value>
        public bool CurrentTargetInDistrict
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
        public bool CurrentTargetInRange
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the size of the current target service problem.
        /// </summary>
        /// <value>
        /// The size of the current target service problem.
        /// </value>
        public uint CurrentTargetServiceProblemSize { get; private set; }

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
        /// Gets a value indicating whether [emptying is done].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [emptying is done]; otherwise, <c>false</c>.
        /// </value>
        public bool EmptyingIsDone
        {
            get;
            private set;
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
        /// Gets a value indicating whether this instance is automatic emptying.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is automatic emptying; otherwise, <c>false</c>.
        /// </value>
        public bool IsAutoEmptying
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is emptying.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is emptying; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmptying
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether building needs emptying.
        /// </summary>
        /// <value>
        ///   <c>true</c> if building needs emptying; otherwise, <c>false</c>.
        /// </value>
        public bool NeedsEmptying
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
        /// Gets the number service problems.
        /// </summary>
        /// <value>
        /// The number of service problems.
        /// </value>
        public ushort ServiceProblemCount
        {
            get
            {
                return (Global.ServiceProblems == null) ? (ushort)0 : Global.ServiceProblems.GetServiceBuildingProblemCount(this.BuildingId);
            }
        }

        /// <summary>
        /// Gets the total size of the service problem.
        /// </summary>
        /// <value>
        /// The size of the service problem.
        /// </value>
        public uint ServiceProblemSize
        {
            get
            {
                return (Global.ServiceProblems == null) ? 0 : Global.ServiceProblems.GetServiceBuildingProblemSize(this.BuildingId);
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
            get => this.vehiclesFree;
            set
            {
                this.vehiclesFree = value;
                this.UpdateTargetInfo();
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
                this.vehiclesSpare = this.VehiclesTotal - this.vehiclesMade;
            }
        }

        /// <summary>
        /// Gets the spare vehicles count.
        /// </summary>
        public int VehiclesSpare => this.vehiclesSpare;

        /// <summary>
        /// Gets the total vehicles count.
        /// </summary>
        public int VehiclesTotal
        {
            get;
            private set;
        }

        /// <summary>
        /// Adds debug info to info list.
        /// </summary>
        /// <param name="info">The information list.</param>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <param name="tagSuffix">The tag suffix.</param>
        public static void AddToInfoMsg(Log.InfoList info, ushort buildingId, ref Building building, string tagSuffix = null)
        {
            info.Add("sbiCanReceiveFlags1" + tagSuffix, (building.m_flags & (Building.Flags.Downgrading | Building.Flags.Demolishing | Building.Flags.Deleted | Building.Flags.Hidden | Building.Flags.BurnedDown | Building.Flags.Collapsed | Building.Flags.Evacuating)) == Building.Flags.None);
            info.Add("sbiCanReceiveFlags2" + tagSuffix, (building.m_flags & (Building.Flags.Created | Building.Flags.Completed)) == (Building.Flags.Created | Building.Flags.Completed));
            info.Add("sbiCanReceiveFlagsCapacityFull" + tagSuffix, (building.m_flags & Building.Flags.CapacityFull) != Building.Flags.CapacityFull);
            info.Add("sbiCanReceiveProblems" + tagSuffix, (building.m_problems & (Notification.Problem.Emptying | Notification.Problem.LandfillFull | Notification.Problem.RoadNotConnected | Notification.Problem.TurnedOff | Notification.Problem.FatalProblem)) == Notification.Problem.None);

            int max;
            int amount;
            int vehicles;
            int free;

            if (BuildingHelper.GetCapacityAmount(buildingId, ref building, out amount, out max, out vehicles))
            {
                if ((building.m_flags & Building.Flags.CapacityFull) == Building.Flags.CapacityFull)
                {
                    free = 0;
                }
                else
                {
                    free = max - amount;
                }

                info.Add("sbiCanReceiveCapacity" + tagSuffix, (max == 0 || free > 0));
            }

            info.Add("sbiCanReceiveFire" + tagSuffix, building.m_fireIntensity <= 0);
            info.Add("sbiCanReceiveIsFull" + tagSuffix, !building.Info.m_buildingAI.IsFull(buildingId, ref building));
        }

        /// <summary>
        /// Starts the automatic emptying.
        /// </summary>
        /// <returns>True if started.</returns>
        public bool AutoEmptyStart()
        {
            if (this.IsEmptying)
            {
                Log.Debug(this, "AutoEmptyStart", "AlreadyEmptying", this.BuildingId, this.IsAutoEmptying, this.BuildingName, this.DistrictName);
                return this.IsAutoEmptying;
            }

            if (!this.NeedsEmptying)
            {
                Log.Debug(this, "AutoEmptyStart", "NoNeed", this.BuildingId, this.BuildingName, this.DistrictName);
                return false;
            }

            try
            {
                Log.Debug(this, "AutoEmptyStart", this.BuildingId, this.BuildingName, this.DistrictName);

                this.SetEmptying(ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[this.BuildingId], true);
                this.IsAutoEmptying = true;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(this, "AutoEmptyStart", ex, this.BuildingId);
                return false;
            }
        }

        /// <summary>
        /// Stops the automatic emptying.
        /// </summary>
        /// <returns>True if stop.</returns>
        public bool AutoEmptyStop()
        {
            if (!this.IsEmptying || !this.IsAutoEmptying)
            {
                Log.Debug(this, "AutoEmptyStop", "NotEmptying", this.BuildingId, this.IsAutoEmptying, this.BuildingName, this.DistrictName);
                return false;
            }

            try
            {
                Log.Debug(this, "AutoEmptyStop", this.BuildingId, this.BuildingName, this.DistrictName);

                this.SetEmptying(ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[this.BuildingId], false);
                this.IsAutoEmptying = false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(this, "AutoEmptyStop", ex, this.BuildingId);
                return false;
            }
        }

        /// <summary>
        /// Creates the vehicle.
        /// </summary>
        /// <param name="transferType">Type of the transfer.</param>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <param name="targetCitizenId">The target citizen identifier.</param>
        /// <returns>
        /// The vehicle identifier.
        /// </returns>
        public ushort CreateVehicle(byte transferType, ushort targetBuildingId = 0, uint targetCitizenId = 0)
        {
            if (this.vehiclesSpare < 1)
            {
                return 0;
            }

            ServiceVehicleInfo serviceVehicle = ServiceVehicleInfo.Create(this, (TransferManager.TransferReason)transferType, this.serviceType, targetBuildingId, targetCitizenId);
            if (serviceVehicle == null)
            {
                return 0;
            }

            this.vehiclesSpare--;
            this.vehiclesMade++;

            if (targetBuildingId == 0)
            {
                this.vehiclesFree++;
            }

            this.UpdateTargetInfo();

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
        /// <param name="canCreateSpares">if set to <c>true</c> service can create spare vehicles.</param>
        public void SetCurrentTargetInfo(TargetBuildingInfo building, bool canCreateSpares)
        {
            if (this.lastUpdate != Global.CurrentFrame)
            {
                this.Update(ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[this.BuildingId], true);
            }

            this.CurrentTargetCanCreateSpares = canCreateSpares;
            this.UpdateTargetInfo();

            if (this.CurrentTargetCanDispatch)
            {
                this.CurrentTargetDistance = (this.Position - building.Position).sqrMagnitude;

                if (this.serviceSettings.DispatchByDistrict)
                {
                    this.CurrentTargetInDistrict = building.District == this.District;
                }
                else
                {
                    this.CurrentTargetInDistrict = false;
                }

                this.CurrentTargetInRange = this.CurrentTargetInDistrict || (this.serviceSettings.DispatchByRange && this.CurrentTargetDistance < this.Range) || (!this.serviceSettings.DispatchByDistrict && !this.serviceSettings.DispatchByRange);

                this.CurrentTargetCapacityOverflow = (this.CapacityFree >= building.ProblemSize) ? 0 : building.ProblemSize - this.CapacityFree;

                if (Global.ServiceProblems == null)
                {
                    this.CurrentTargetServiceProblemSize = 0;
                }
                else
                {
                    this.CurrentTargetServiceProblemSize = Global.ServiceProblems.GetProblemSize(this.BuildingId, building.BuildingId);

                    if (Log.LogALot && this.CurrentTargetServiceProblemSize > 0)
                    {
                        ServiceProblemKeeper.DevLog("SetServiceBuildingCurrentTargetInfo",
                                Log.Data("ServiceBuilding", this.BuildingId, BuildingHelper.GetBuildingName(this.BuildingId)),
                                Log.Data("TargetBuilding", building.BuildingId, BuildingHelper.GetBuildingName(building.BuildingId)),
                                "ProblemSize", this.CurrentTargetServiceProblemSize);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the vehicle target.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <param name="transferType">Type of the transfer.</param>
        /// <returns>The result.</returns>
        public VehicleResult SetVehicleTarget(ushort vehicleId, ushort targetBuildingId, byte transferType)
        {
            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            VehicleResult result = this.Vehicles[vehicleId].SetTarget(targetBuildingId, ref vehicles[vehicleId], (TransferManager.TransferReason)transferType);

            if (!result)
            {
                if (Log.LogALot)
                {
                    Log.Debug(this, "SetVehicleTarget", result, this.BuildingId, vehicleId, targetBuildingId, vehicles[vehicleId].m_flags);
                }

                if (result.DeSpawned)
                {
                    this.vehiclesFree--;
                    this.vehiclesMade--;
                    this.vehiclesSpare++;
                    this.UpdateTargetInfo();
                }
            }

            return result;
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
                int vehicles;

                int productionRate = PlayerBuildingAI.GetProductionRate(100, Singleton<EconomyManager>.instance.GetBudget(building.Info.m_buildingAI.m_info.m_class));

                if (BuildingHelper.GetCapacityAmount(this.BuildingId, ref building, out amount, out max, out vehicles))
                {
                    this.VehiclesTotal = vehicles;
                }
                else
                {
                    this.VehiclesTotal = 0;
                    amount = 0;
                    max = 0;
                }

                this.VehiclesTotal = ((productionRate * this.VehiclesTotal) + 99) / 100;
                this.CapacityMax = max;

                if ((building.m_flags & Building.Flags.CapacityFull) == Building.Flags.CapacityFull)
                {
                    this.CapacityFree = 0;
                    this.CapacityPercent = 100.0f;
                }
                else
                {
                    this.CapacityFree = max - amount;

                    if (max == 0)
                    {
                        this.CapacityPercent = 0.0f;
                    }
                    else
                    {
                        this.CapacityPercent = ((float)amount / (float)max) * 100.0f;
                    }
                }

                this.CanBeEmptied = building.Info.m_buildingAI.CanBeEmptied();
                this.IsEmptying = (building.m_flags & Building.Flags.Downgrading) != Building.Flags.None;
                this.IsAutoEmptying = this.IsAutoEmptying & this.IsEmptying;

                this.NeedsEmptying = this.serviceSettings.AutoEmpty && !this.IsEmptying &&
                                     ((building.m_flags & Building.Flags.CapacityFull) == Building.Flags.CapacityFull ||
                                      this.CapacityPercent >= (float)this.serviceSettings.AutoEmptyStartLevelPercent);

                this.EmptyingIsDone = this.IsAutoEmptying &&
                                      ((building.m_flags & Building.Flags.CapacityFull) != Building.Flags.CapacityFull &&
                                      this.CapacityPercent <= (float)this.serviceSettings.AutoEmptyStopLevelPercent);

                this.CanEmptyOther = this.serviceSettings.AutoEmpty && !this.CanBeEmptied &&
                                     (building.m_flags & Building.Flags.Active) != Building.Flags.None && building.m_productionRate > 0;

                didUpdate = true;
                this.lastCapacityUpdate = Global.CurrentFrame;
            }

            this.CanReceive = (building.m_flags & (Building.Flags.Downgrading | Building.Flags.Demolishing | Building.Flags.Deleted | Building.Flags.Hidden | Building.Flags.BurnedDown | Building.Flags.Collapsed | Building.Flags.Evacuating)) == Building.Flags.None &&
                              (building.m_flags & (Building.Flags.Created | Building.Flags.Completed)) == (Building.Flags.Created | Building.Flags.Completed) &&
                              (building.m_flags & Building.Flags.CapacityFull) != Building.Flags.CapacityFull &&
                              (building.m_problems & (Notification.Problem.Emptying | Notification.Problem.LandfillFull | Notification.Problem.RoadNotConnected | Notification.Problem.TurnedOff | Notification.Problem.FatalProblem)) == Notification.Problem.None &&
                              (this.CapacityMax == 0 || this.CapacityFree > 0) &&
                              building.m_fireIntensity <= 0 && !building.Info.m_buildingAI.IsFull(this.BuildingId, ref building);

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
                if (this.serviceSettings.DispatchByDistrict || Log.LogNames)
                {
                    this.District = Singleton<DistrictManager>.instance.GetDistrict(this.Position);
                }
                else if (this.lastInfoUpdate == 0)
                {
                    this.District = 0;
                }

                if (this.serviceSettings.DispatchByRange)
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
            this.serviceSettings = Global.GetServiceSettings(this.serviceType);
        }

        /// <summary>
        /// Sets the emptying.
        /// </summary>
        /// <param name="building">The building.</param>
        /// <param name="emptying">If set to <c>true</c> empty building.</param>
        private void SetEmptying(ref Building building, bool emptying)
        {
            building.Info.m_buildingAI.SetEmptying(this.BuildingId, ref building, emptying);
        }

        private void UpdateTargetInfo()
        {
            this.CurrentTargetCanDispatch = this.CanReceive && (this.vehiclesFree > 0 || (this.CurrentTargetCanCreateSpares && this.vehiclesSpare > 0));
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
                long c;

                // Check service problem difference. Use if big enough.
                c = x.CurrentTargetServiceProblemSize - y.CurrentTargetServiceProblemSize;
                if (Math.Abs(c) > ServiceProblemKeeper.ProblemSizeImportant)
                {
                    return (c < 0) ? -1 : 1;
                }

                // Check range.
                if (x.CurrentTargetInRange && !y.CurrentTargetInRange)
                {
                    return -1;
                }
                else if (y.CurrentTargetInRange && !x.CurrentTargetInRange)
                {
                    return 1;
                }

                // Check district.
                if (x.CurrentTargetInDistrict && !y.CurrentTargetInDistrict)
                {
                    return -1;
                }
                else if (y.CurrentTargetInDistrict && !x.CurrentTargetInDistrict)
                {
                    return 1;
                }

                // Check service problem difference again.
                if (c < 0)
                {
                    return -1;
                }
                else if (c > 0)
                {
                    return 1;
                }

                // Check overflow.
                c = x.CurrentTargetCapacityOverflow - y.CurrentTargetCapacityOverflow;
                if (c < 0)
                {
                    return -1;
                }
                else if (c > 0)
                {
                    return 1;
                }

                float s, d;

                // Check low accuracy distance.
                d = Math.Min(x.CurrentTargetDistance / 100.0f, y.CurrentTargetDistance / 100.0f);
                s = (x.CurrentTargetDistance - y.CurrentTargetDistance) / 100.0f;
                if (s < -d)
                {
                    return -1;
                }
                else if (s > d)
                {
                    return 1;
                }

                // Check capacity.
                c = x.CapacityLevel - y.CapacityLevel;
                if (c < 0)
                {
                    return -1;
                }
                else if (c > 0)
                {
                    return 1;
                }

                // Check high accuracy distance.
                s = x.CurrentTargetDistance - y.CurrentTargetDistance;
                if (s < 0)
                {
                    return -1;
                }
                else if (s > 0)
                {
                    return 1;
                }

                // No difference...
                return 0;
            }
        }
    }
}