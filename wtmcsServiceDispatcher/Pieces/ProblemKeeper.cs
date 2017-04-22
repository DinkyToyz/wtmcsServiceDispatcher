using System.Collections.Generic;
using System.Linq;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Keeps track of problems.
    /// </summary>
    internal class ProblemKeeper : IHandlerPart
    {
        /// <summary>
        /// The service problem notes.
        /// </summary>
        private List<ServiceProblemNote> ServiceProblemNotes = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProblemKeeper"/> class.
        /// </summary>
        public ProblemKeeper()
        {
            this.ReInitialize();
        }

        /// <summary>
        /// Service problems.
        /// </summary>
        public enum ServiceProblem
        {
            /// <summary>
            /// The vehicle was not created.
            /// </summary>
            VehicleNotCreated,

            /// <summary>
            /// The vehicle could not find a path to the target building.
            /// </summary>
            PathNotFound,

            /// <summary>
            /// The vehicle disppeared after beein assigned a target.
            /// </summary>
            VehicleGone
        }

        /// <summary>
        /// Gets the size of the building problem.
        /// </summary>
        /// <param name="serviceBuildingId">The service building identifier.</param>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <returns>The problem size.</returns>
        public int GetBuildingProblemSize(ushort serviceBuildingId, ushort targetBuildingId)
        {
            return this.ServiceProblemNotes.Where(bp => bp.ServiceBuilding == serviceBuildingId && bp.TargetBuilding == targetBuildingId).Sum(bp => bp.ProblemValue);
        }

        /// <summary>
        /// Gets the size of the service building problem.
        /// </summary>
        /// <param name="serviceBuildingId">The service building identifier.</param>
        /// <returns>The problem size.</returns>
        public int GetServiceBuildingProblemSize(ushort serviceBuildingId)
        {
            return this.ServiceProblemNotes.Where(bp => bp.ServiceBuilding == serviceBuildingId).Sum(bp => bp.ProblemValue);
        }

        /// <summary>
        /// Gets the size of the target building problem.
        /// </summary>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <returns>The problem size.</returns>
        public int GetTargetBuildingProblemSize(ushort targetBuildingId)
        {
            return this.ServiceProblemNotes.Where(bp => bp.TargetBuilding == targetBuildingId).Sum(bp => bp.ProblemValue);
        }

        /// <summary>
        /// Notes a problem.
        /// </summary>
        /// <param name="problem">The problem.</param>
        /// <param name="serviceBuildingId">The service building identifier.</param>
        /// <param name="targetBuildingId">The target building identifier.</param>
        public void NoteServiceProblem(ServiceProblem problem, ushort serviceBuildingId, ushort targetBuildingId)
        {
            if (Global.EnableExperiments || Global.EnableDevExperiments || Log.LogALot || Log.LogLevel >= Log.Level.Debug)
            {
                Log.Debug(
                    this, "NoteServiceProblem", problem, serviceBuildingId, targetBuildingId,
                    BuildingHelper.GetBuildingName(serviceBuildingId),
                    BuildingHelper.GetDistrictName(serviceBuildingId),
                    BuildingHelper.GetBuildingName(targetBuildingId),
                    BuildingHelper.GetDistrictName(targetBuildingId));
            }
        }

        /// <summary>
        /// Re-initialize the part.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void ReInitialize()
        {
            if (this.ServiceProblemNotes == null)
            {
                this.ServiceProblemNotes = new List<ServiceProblemNote>();
            }
            else
            {
                this.ServiceProblemNotes.Clear();
            }
        }

        /// <summary>
        /// Cleans this instance.
        /// </summary>
        private void Clean()
        {
            this.ServiceProblemNotes.RemoveAll(bp => Global.CurrentFrame - bp.ProblemFrame > Global.ProblemLingerDelay);
        }

        /// <summary>
        /// Note that there was a service problem.
        /// </summary>
        private struct ServiceProblemNote
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceProblemNote"/> struct.
            /// </summary>
            /// <param name="problem">The problem.</param>
            /// <param name="serviceBuilding">The service building.</param>
            /// <param name="targetBuilding">The target building.</param>
            public ServiceProblemNote(ServiceProblem problem, ushort serviceBuilding, ushort targetBuilding)
            {
                this.ProblemFrame = Global.CurrentFrame;

                this.ProblemValue = 1;
                this.ServiceBuilding = serviceBuilding;
                this.TargetBuilding = targetBuilding;
            }

            /// <summary>
            /// Gets or sets the problem frame.
            /// </summary>
            /// <value>
            /// The problem frame.
            /// </value>
            public uint ProblemFrame { get; private set; }

            /// <summary>
            /// Gets or sets the problem value.
            /// </summary>
            /// <value>
            /// The problem value.
            /// </value>
            public ushort ProblemValue { get; private set; }

            /// <summary>
            /// Gets or sets the service building.
            /// </summary>
            /// <value>
            /// The service building.
            /// </value>
            public ushort ServiceBuilding { get; private set; }

            /// <summary>
            /// Gets or sets the target building.
            /// </summary>
            /// <value>
            /// The target building.
            /// </value>
            public ushort TargetBuilding { get; private set; }
        }
    }
}