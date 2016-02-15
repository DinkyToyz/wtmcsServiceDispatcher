using System;
using System.Collections.Generic;
using ColossalFramework;
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
        /// <param name="problemToCheck">The problem to check.</param>
        /// <param name="demand">The demand.</param>
        public TargetBuildingInfo(ushort buildingId, ref Building building, Notification.Problem problemToCheck, ServiceDemand demand)
        {
            this.BuildingId = buildingId;

            this.Update(ref building, problemToCheck, demand);
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
        /// <param name="problemToCheck">The problem to check.</param>
        /// <param name="demand">The demand.</param>
        /// <exception cref="System.Exception">Loop counter too high.</exception>
        public void Update(ref Building building, Notification.Problem problemToCheck, ServiceDemand demand)
        {
            this.lastUpdate = Global.CurrentFrame;

            switch (problemToCheck)
            {
                case Notification.Problem.Death:
                    CitizenManager citizenManager = Singleton<CitizenManager>.instance;

                    int size = 0;
                    int count = 0;
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
                                if (citizen.Dead && citizen.GetBuildingByLocation() == this.BuildingId)
                                {
                                    size++;
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

                    this.ProblemValue = (ushort)building.m_deathProblemTimer << 8;
                    this.ProblemSize = size;
                    break;

                case Notification.Problem.Garbage:
                    this.ProblemValue = building.m_garbageBuffer;
                    this.ProblemSize = building.m_garbageBuffer;
                    break;

                default:
                    this.ProblemValue = 0;
                    this.ProblemSize = 0;
                    break;
            }

            this.HasProblem = (this.ProblemSize > 0) && ((building.m_problems & problemToCheck) == problemToCheck || this.ProblemValue >= Dispatcher.ProblemLimit);
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
                if (Global.Settings.DispatchAnyByDistrict)
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
        /// Compares target buildings for priority sorting.
        /// </summary>
        public class PriorityComparer : IComparer<TargetBuildingInfo>, IHandlerPart
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
                if (x.HasProblem && !y.HasProblem)
                {
                    return -1;
                }
                else if (y.HasProblem && !x.HasProblem)
                {
                    return 1;
                }
                else
                {
                    return y.ProblemValue - x.ProblemValue;
                }
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