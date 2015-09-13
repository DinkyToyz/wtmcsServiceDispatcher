using System.Collections.Generic;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Info about a target building.
    /// </summary>
    internal class TargetBuildingInfo
    {
        /// <summary>
        /// The building identifier.
        /// </summary>
        public ushort BuildingId;

        /// <summary>
        /// The district the building is in.
        /// </summary>
        public byte District;

        /// <summary>
        /// The building has a problem.
        /// </summary>
        public bool HasProblem;

        /// <summary>
        /// The building needs service.
        /// </summary>
        public bool NeedsService;

        /// <summary>
        /// The position.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The problem timer.
        /// </summary>
        public int ProblemValue;

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
        /// Initializes a new instance of the <see cref="TargetBuildingInfo" /> class.
        /// </summary>
        /// <param name="districtManager">The district manager.</param>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <param name="problemTimer">The problem timer.</param>
        /// <param name="problemBuffer">The problem buffer.</param>
        /// <param name="needsService">If set to <c>true</c> building needs service.</param>
        /// <param name="problemToCheck">The problem to check.</param>
        public TargetBuildingInfo(DistrictManager districtManager, ushort buildingId, ref Building building, byte? problemTimer, ushort? problemBuffer, bool needsService, Notification.Problem problemToCheck)
        {
            this.BuildingId = buildingId;

            this.Update(districtManager, ref building, problemTimer, problemBuffer, needsService, problemToCheck);
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
        /// Gets or sets a value indicating whether this <see cref="TargetBuildingInfo"/> is handled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if handled; otherwise, <c>false</c>.
        /// </value>
        public bool Handled
        {
            get
            {
                return Global.RecheckHandledInterval > 0 && Global.CurrentFrame - this.lastHandled < Global.RecheckHandledInterval;
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
        /// Updates the building.
        /// </summary>
        /// <param name="districtManager">The district manager.</param>
        /// <param name="building">The building.</param>
        /// <param name="problemTimer">The problem timer.</param>
        /// <param name="problemBuffer">The problem buffer.</param>
        /// <param name="needsService">If set to <c>true</c> building needs service.</param>
        /// <param name="problemToCheck">The problem to check.</param>
        public void Update(DistrictManager districtManager, ref Building building, byte? problemTimer, ushort? problemBuffer, bool needsService, Notification.Problem problemToCheck)
        {
            this.lastUpdate = Global.CurrentFrame;

            if (problemTimer != null && problemTimer.HasValue)
            {
                this.ProblemValue = (ushort)problemTimer.Value << 8;
            }
            else if (problemBuffer != null && problemBuffer.HasValue)
            {
                this.ProblemValue = problemBuffer.Value;
            }
            else
            {
                this.ProblemValue = 0;
            }

            this.HasProblem = (building.m_problems & problemToCheck) == problemToCheck || this.ProblemValue >= Dispatcher.ProblemLimit;
            this.Position = building.m_position;

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

                this.lastInfoUpdate = Global.CurrentFrame;
            }

            this.NeedsService = needsService || this.HasProblem;

            this.checkThis = needsService &&
                             ((Global.RecheckInterval == 0 || this.lastCheck == 0 || Global.CurrentFrame - this.lastCheck >= Global.RecheckInterval) &&
                              (Global.RecheckHandledInterval == 0 || this.lastHandled == 0 || Global.CurrentFrame - this.lastHandled >= Global.RecheckHandledInterval));
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
        }
    }
}
