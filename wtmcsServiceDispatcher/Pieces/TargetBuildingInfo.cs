using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Info about a target building.
    /// </summary>
    internal class TargetBuildingInfo : IBuildingInfo
    {
        /// <summary>
        /// Whether this building should be checked or not.
        /// </summary>
        private bool checkThis;

        /// <summary>
        /// The citizens.
        /// </summary>
        private Dictionary<uint, TargetCitizenInfo> citizens = null;

        /// <summary>
        /// The dispatcher type.
        /// </summary>
        private Dispatcher.DispatcherTypes dispatcherType = Dispatcher.DispatcherTypes.None;

        /// <summary>
        /// The last check stamp.
        /// </summary>
        private uint lastCheck = 0;

        /// <summary>
        /// The last handled stamp.
        /// </summary>
        private uint lastHandled = 0;

        /// <summary>
        /// The last update stamp.
        /// </summary>
        private uint lastInfoUpdate;

        /// <summary>
        /// The last update stamp.
        /// </summary>
        private uint lastUpdate;

        /// <summary>
        /// The last want frame stamp.
        /// </summary>
        private uint lastWantStamp = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetBuildingInfo" /> class.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <param name="dispatcherType">Type of the dispatcher.</param>
        /// <param name="demand">The demand.</param>
        public TargetBuildingInfo(ushort buildingId, ref Building building, Dispatcher.DispatcherTypes dispatcherType, ServiceDemand demand)
        {
            this.BuildingId = buildingId;
            this.dispatcherType = dispatcherType;

            if (this.dispatcherType == Dispatcher.DispatcherTypes.AmbulanceDispatcher)
            {
                this.citizens = new Dictionary<uint, TargetCitizenInfo>();
            }

            this.Update(ref building, demand);
        }

        /// <summary>
        /// Building demand states.
        /// </summary>
        public enum ServiceDemand
        {
            /// <summary>
            /// No demand.
            /// </summary>
            None = 0,

            /// <summary>
            /// Wants, but does not necessarily need, service.
            /// </summary>
            WantsService = 1,

            /// <summary>
            /// Needs service.
            /// </summary>
            NeedsService = 2
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
        /// Gets or sets a value indicating whether this <see cref="TargetBuildingInfo"/> is checked.
        /// </summary>
        /// <value>
        ///   <c>true</c> if checked; otherwise, <c>false</c>.
        /// </value>
        public bool Checked
        {
            get
            {
                return Global.RecheckInterval > 0 && Global.CurrentFrame - this.lastCheck < Global.RecheckInterval;
            }

            set
            {
                if (value)
                {
                    this.lastCheck = Global.CurrentFrame;
                }
                else
                {
                    this.lastCheck = 0;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to check this building.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this building should be checked; otherwise, <c>false</c>.
        /// </value>
        public bool CheckThis
        {
            get
            {
                return this.checkThis && this.lastUpdate == Global.CurrentFrame;
            }

            set
            {
                this.checkThis = value;
            }
        }

        /// <summary>
        /// Gets the citizen count.
        /// </summary>
        /// <value>
        /// The citizen count.
        /// </value>
        public int CitizenCount
        {
            get
            {
                return this.ProblemSize;
            }
        }

        /// <summary>
        /// Gets the citizens.
        /// </summary>
        /// <value>
        /// The citizens.
        /// </value>
        public IEnumerable<TargetCitizenInfo> Citizens
        {
            get
            {
                return (this.citizens == null) ? null : this.citizens.Values;
            }
        }

        /// <summary>
        /// Gets the assigned citizen count.
        /// </summary>
        /// <value>
        /// The assigned citizen count.
        /// </value>
        public int CitizensAssigned
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the un-assigned citizen count.
        /// </summary>
        /// <value>
        /// The un-assigned citizen count.
        /// </value>
        public int CitizensUnAssigned
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the demand.
        /// </summary>
        /// <value>
        /// The demand.
        /// </value>
        public ServiceDemand Demand
        {
            get
            {
                if (this.NeedsService)
                {
                    return ServiceDemand.NeedsService;
                }

                if (this.WantedService)
                {
                    return ServiceDemand.WantsService;
                }

                return ServiceDemand.None;
            }
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
        /// Gets or sets a value indicating whether this <see cref="TargetBuildingInfo"/> is handled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if handled; otherwise, <c>false</c>.
        /// </value>
        public bool Handled
        {
            get
            {
                return Global.RecheckHandledInterval >= 0 && Global.CurrentFrame - this.lastHandled < Global.RecheckHandledInterval;
            }

            set
            {
                if (value)
                {
                    this.checkThis = false;
                    this.lastHandled = Global.CurrentFrame;
                }
                else
                {
                    this.lastHandled = 0;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the building has been handled in this frame.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the building has been handled in this frame; otherwise, <c>false</c>.
        /// </value>
        public bool HandledNow
        {
            get
            {
                return this.lastHandled == Global.CurrentFrame;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the building has a problem.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has problem; otherwise, <c>false</c>.
        /// </value>
        public bool HasProblem
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the building needs service.
        /// </summary>
        /// <value>
        ///   <c>true</c> if building needs service; otherwise, <c>false</c>.
        /// </value>
        public bool NeedsService
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the next citizen.
        /// </summary>
        /// <value>
        /// The next citizen.
        /// </value>
        public TargetCitizenInfo NextCitizen
        {
            get
            {
                return (this.citizens == null) ? null : this.citizens.Values.Where(c => c.VehicleId == 0).OrderBy(c => c.ProblemSize).Last();
            }
        }

        /// <summary>
        /// Gets the position.
        /// </summary>
        public Vector3 Position
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the size of the problem.
        /// </summary>
        /// <value>
        /// The size of the problem.
        /// </value>
        public int ProblemSize
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the problem value.
        /// </summary>
        /// <value>
        /// The problem value.
        /// </value>
        public int ProblemValue
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the problem weight for sorting.
        /// </summary>
        /// <value>
        /// The problem sorting weight.
        /// </value>
        public int ProblemWeight
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
        public ushort ServiceProblemCount => this.serviceProblemCountValue;

        /// <summary>
        /// The service problem count value.
        /// </summary>
        private ushort serviceProblemCountValue;

        /// <summary>
        /// Gets the service problem magnitude.
        /// </summary>
        /// <value>
        /// The service problem magnitude.
        /// </value>
        public uint ServiceProblemMagnitude { get; private set; }

        /// <summary>
        /// Gets the total size of the service problem.
        /// </summary>
        /// <value>
        /// The size of the service problem.
        /// </value>
        public uint ServiceProblemSize => this.serviceProblemSizeValue;

        /// <summary>
        /// The service problem size value
        /// </summary>
        private uint serviceProblemSizeValue;

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
        /// Gets a value indicating whether the building have no want for service.
        /// </summary>
        /// <value>
        ///   <c>true</c> if building have no want for service; otherwise, <c>false</c>.
        /// </value>
        public bool WantedService
        {
            get
            {
                return this.WantsService || Global.CurrentFrame - this.lastWantStamp <= Global.DemandLingerDelay;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the building wants service.
        /// </summary>
        /// <value>
        ///   <c>true</c> if building wants service; otherwise, <c>false</c>.
        /// </value>
        public bool WantsService
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void ReInitialize()
        {
        }

        /// <summary>
        /// Updates the building.
        /// </summary>
        /// <param name="building">The building.</param>
        /// <param name="demand">The demand.</param>
        /// <exception cref="System.Exception">Loop counter too high.</exception>
        public void Update(ref Building building, ServiceDemand demand)
        {
            this.lastUpdate = Global.CurrentFrame;

            Notification.Problem problemToCheck;

            switch (this.dispatcherType)
            {
                case Dispatcher.DispatcherTypes.HearseDispatcher:
                    this.UpdateCitizens(ref building);
                    this.ProblemValue = ((ushort)building.m_deathProblemTimer << 8);
                    problemToCheck = Notification.Problem.Death;
                    break;

                case Dispatcher.DispatcherTypes.GarbageTruckDispatcher:
                    this.ProblemValue = building.m_garbageBuffer;
                    this.ProblemSize = building.m_garbageBuffer;
                    problemToCheck = Notification.Problem.Garbage;
                    break;

                case Dispatcher.DispatcherTypes.AmbulanceDispatcher:
                    this.UpdateCitizens(ref building);
                    this.ProblemValue = (ushort)building.m_healthProblemTimer << 8;
                    problemToCheck = BuildingHelper.HealthProblems;
                    break;

                default:
                    this.ProblemValue = 0;
                    this.ProblemSize = 0;
                    problemToCheck = Notification.Problem.None;
                    break;
            }

            if (Global.ServiceProblems == null)
            {
                this.serviceProblemCountValue = 0;
                this.serviceProblemSizeValue = 0;
                this.ServiceProblemMagnitude = 0;
                this.ProblemWeight = this.ProblemValue;
            }
            else
            {
                Global.ServiceProblems.GetTargetBuildingProblemInfo(this.BuildingId, out this.serviceProblemCountValue, out this.serviceProblemSizeValue);

                if (this.serviceProblemSizeValue > 0 && this.serviceProblemCountValue > 0)
                {
                    uint multiplier = (uint)(Math.Min(10.0, Math.Round(0.5 + (float)this.serviceProblemCountValue / 4.0, 0.0)));
                    this.ServiceProblemMagnitude = (uint)this.serviceProblemSizeValue * multiplier;
                    long weight = this.ProblemValue - this.ServiceProblemMagnitude;
                    this.ProblemWeight = (int)Math.Max(Math.Min(weight, int.MaxValue), 0L);

                    if (Log.LogALot && Log.LogToFile)
                    {
                        ServiceProblemKeeper.DevLog("TargetBuildingProblemWeighting",
                            Log.Data("TargetBuilding", this.BuildingId, BuildingHelper.GetBuildingName(this.BuildingId)),
                            Log.Data("BuildingProblem", this.serviceProblemCountValue, this.serviceProblemSizeValue),
                            Log.Data("Modifier", multiplier, this.ServiceProblemMagnitude, weight),
                            Log.Data("Weight", this.ProblemValue, this.ProblemWeight, this.ProblemWeight - this.ProblemValue));
                    }
                }
                else
                {
                    this.ServiceProblemMagnitude = 0;
                    this.ProblemWeight = this.ProblemValue;

                    ////if (Log.LogALot && Log.LogToFile && Global.EnableDevExperiments)
                    ////{
                    ////    ServiceProblemKeeper.DevLog("TargetBuildingProblemWeighting",
                    ////        Log.Data("TargetBuilding", this.BuildingId, BuildingHelper.GetBuildingName(this.BuildingId)),
                    ////        Log.Data("BuildingProblem", this.ServiceProblemCount, this.ServiceProblemSize),
                    ////        Log.Data("Weight", this.ProblemValue, this.ProblemWeight, 0));
                    ////}
                }
            }

            this.HasProblem = (this.ProblemSize > 0) && ((building.m_problems & problemToCheck) != Notification.Problem.None || this.ProblemValue >= Dispatcher.ProblemLimit);
            this.Position = building.m_position;

            this.UpdateValues(ref building, false);

            this.NeedsService = (this.ProblemSize > 0) && (this.HasProblem || demand == ServiceDemand.NeedsService);
            this.WantsService = (this.ProblemSize > 0) && (this.NeedsService || demand == ServiceDemand.WantsService);
            if (this.WantsService || this.lastWantStamp == 0)
            {
                this.lastWantStamp = Global.CurrentFrame;
            }

            this.checkThis = this.WantsService &&
                             ((Global.RecheckInterval == 0 || this.lastCheck == 0 || Global.CurrentFrame - this.lastCheck >= Global.RecheckInterval) &&
                              (Global.RecheckHandledInterval == 0 || this.lastHandled == 0 || Global.CurrentFrame - this.lastHandled >= Global.RecheckHandledInterval));
        }

        /// <summary>
        /// Updates the building values.
        /// </summary>
        /// <param name="building">The building.</param>
        /// <param name="ignoreInterval">If set to <c>true</c> ignore object update interval.</param>
        /// <returns>
        /// True if updated.
        /// </returns>
        public bool UpdateValues(ref Building building, bool ignoreInterval = true)
        {
            if (this.lastInfoUpdate == 0 || (ignoreInterval && this.lastInfoUpdate != Global.CurrentFrame) || Global.CurrentFrame - this.lastInfoUpdate > Global.ObjectUpdateInterval)
            {
                if (Global.Settings.DispatchAnyByDistrict || Log.LogNames)
                {
                    this.District = Singleton<DistrictManager>.instance.GetDistrict(this.Position);
                }
                else if (this.lastInfoUpdate == 0)
                {
                    this.District = 0;
                }

                this.lastInfoUpdate = Global.CurrentFrame;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates the citizens.
        /// </summary>
        /// <param name="building">The building.</param>
        /// <exception cref="Exception">Loop counter too high.</exception>
        private void UpdateCitizens(ref Building building)
        {
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;

            int count = 0;
            int size = 0;
            int assigned = 0;
            int unassigned = 0;
            uint unitId = building.m_citizenUnits;

            while (unitId != 0)
            {
                CitizenUnit unit = citizenManager.m_units.m_buffer[unitId];
                for (int i = 0; i < 5; i++)
                {
                    uint citizenId = unit.GetCitizen(i);
                    if (citizenId != 0)
                    {
                        Citizen citizen = citizenManager.m_citizens.m_buffer[citizenId];
                        if (((this.dispatcherType == Dispatcher.DispatcherTypes.HearseDispatcher && citizen.Dead) ||
                             (this.dispatcherType == Dispatcher.DispatcherTypes.AmbulanceDispatcher && citizen.Sick)) &&
                            citizen.GetBuildingByLocation() == this.BuildingId)
                        {
                            size++;

                            if (this.citizens != null)
                            {
                                TargetCitizenInfo citizenInfo;
                                if (this.citizens.TryGetValue(citizenId, out citizenInfo))
                                {
                                    citizenInfo.Update(ref citizen, this.dispatcherType);
                                    if (citizenInfo.VehicleId == 0)
                                    {
                                        unassigned++;
                                    }
                                    else
                                    {
                                        assigned++;
                                    }
                                }
                                else
                                {
                                    this.citizens[citizenId] = new TargetCitizenInfo(citizenId, ref citizen, this.dispatcherType);
                                    unassigned++;
                                }
                            }
                        }
                    }
                }

                count++;
                if (count > (int)ushort.MaxValue * 10)
                {
                    throw new Exception("Loop counter too high");
                }

                unitId = unit.m_nextUnit;
            }

            this.ProblemSize = size;
            this.CitizensAssigned = assigned;
            this.CitizensUnAssigned = unassigned;
        }

        /// <summary>
        /// Compares target buildings for priority sorting.
        /// </summary>
        public class PriorityComparer : IComparer<TargetBuildingInfo>
        {
            /// <summary>
            /// Compares two buildings and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// </summary>
            /// <param name="x">The first buildings to compare.</param>
            /// <param name="y">The second buildings to compare.</param>
            /// <returns>
            /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.
            /// </returns>
            public int Compare(TargetBuildingInfo x, TargetBuildingInfo y)
            {
                if (y.ServiceProblemMagnitude * 100 < x.ServiceProblemMagnitude)
                {
                    return -1;
                }
                else if (y.ServiceProblemMagnitude > x.ServiceProblemMagnitude * 100)
                {
                    return 1;
                }

                if (x.HasProblem && !y.HasProblem)
                {
                    return -1;
                }
                else if (y.HasProblem && !x.HasProblem)
                {
                    return 1;
                }

                if (y.ProblemWeight < x.ProblemWeight)
                {
                    return -1;
                }
                else if (y.ProblemWeight > x.ProblemWeight)
                {
                    return 1;
                }

                return 0;
            }
        }

        /// <summary>
        /// Info about service target citizen.
        /// </summary>
        public class TargetCitizenInfo
        {
            /// <summary>
            /// The citizen identifier.
            /// </summary>
            public readonly uint CitizenId;

            /// <summary>
            /// The vehicle identifier.
            /// </summary>
            public ushort VehicleId;

            /// <summary>
            /// The last update stamp.
            /// </summary>
            private uint lastUpdateStamp = 0u;

            /// <summary>
            /// Initializes a new instance of the <see cref="TargetCitizenInfo"/> class.
            /// </summary>
            /// <param name="citizenId">The citizen identifier.</param>
            /// <param name="citizen">The citizen.</param>
            /// <param name="dispatcherType">Type of the dispatcher.</param>
            public TargetCitizenInfo(uint citizenId, ref Citizen citizen, Dispatcher.DispatcherTypes dispatcherType)
            {
                this.CitizenId = citizenId;
                this.VehicleId = 0;

                this.Update(ref citizen, dispatcherType);
            }

            /// <summary>
            /// Gets the size of the problem.
            /// </summary>
            /// <value>
            /// The size of the problem.
            /// </value>
            public int ProblemSize
            {
                get; private set;
            }

            /// <summary>
            /// Gets a value indicating whether this <see cref="TargetCitizenInfo"/> is updated.
            /// </summary>
            /// <value>
            ///   <c>true</c> if updated; otherwise, <c>false</c>.
            /// </value>
            public bool Updated
            {
                get
                {
                    return this.lastUpdateStamp == Global.CurrentFrame;
                }
            }

            /// <summary>
            /// Updates the specified citizen.
            /// </summary>
            /// <param name="citizen">The citizen.</param>
            /// <param name="dispatcherType">Type of the dispatcher.</param>
            public void Update(ref Citizen citizen, Dispatcher.DispatcherTypes dispatcherType)
            {
                if (dispatcherType == Dispatcher.DispatcherTypes.AmbulanceDispatcher)
                {
                    this.ProblemSize = ((int)citizen.m_health) << 8;
                }

                if (citizen.m_vehicle != this.VehicleId)
                {
                    this.VehicleId = 0;
                }

                this.lastUpdateStamp = Global.CurrentFrame;
            }
        }
    }
}