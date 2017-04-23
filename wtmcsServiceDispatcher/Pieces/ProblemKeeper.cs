using System;
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
        /// The service problem counts.
        /// </summary>
        private Dictionary<uint, uint> ServiceProblemCounts = null;

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
        /// The last update stamp.
        /// </summary>
        public uint LastUpdate { get; private set; }

        /// <summary>
        /// Gets the service building problem sizes.
        /// </summary>
        /// <value>
        /// The service building problem sizes.
        /// </value>
        private IEnumerable<ServiceProblemNote> ServiceBuildingProblemSizes
        {
            get
            {
                return this.ServiceProblemNotes
                        .GroupBy(
                            bp => bp.ServiceBuilding,
                            (sb, bpl) => new ServiceProblemNote(
                                                bpl.Max(bp => bp.ProblemFrame),
                                                (uint)bpl.Sum(bp => bp.ProblemSize),
                                                sb, 0
                            ));
            }
        }

        /// <summary>
        /// Gets the service problem sizes.
        /// </summary>
        /// <value>
        /// The service problem sizes.
        /// </value>
        private IEnumerable<ServiceProblemNote> ServiceProblemSizes
        {
            get
            {
                return this.ServiceProblemNotes
                        .GroupBy(
                            bp => bp.BuildingKey,
                            (k, bpl) => new ServiceProblemNote(
                                                bpl.Max(bp => bp.ProblemFrame),
                                                (uint)bpl.Sum(bp => bp.ProblemSize),
                                                k
                            ));
            }
        }

        /// <summary>
        /// Gets the target building problem sizes.
        /// </summary>
        /// <value>
        /// The target building problem sizes.
        /// </value>
        private IEnumerable<ServiceProblemNote> TargetBuildingProblemSizes
        {
            get
            {
                return this.ServiceProblemNotes
                        .GroupBy(
                            bp => bp.TargetBuilding,
                            (tb, bpl) => new ServiceProblemNote(
                                            bpl.Max(bp => bp.ProblemFrame),
                                            (uint)bpl.Sum(bp => bp.ProblemSize),
                                            0, tb
                            ));
            }
        }

        /// <summary>
        /// Notes a problem.
        /// </summary>
        /// <param name="problem">The problem.</param>
        /// <param name="serviceBuildingId">The service building identifier.</param>
        /// <param name="targetBuildingId">The target building identifier.</param>
        public void AddServiceProblemNote(ServiceProblem problem, ushort serviceBuildingId, ushort targetBuildingId)
        {
            try
            {
                if (Global.EnableExperiments || Global.EnableDevExperiments || Log.LogALot || Log.LogLevel >= Log.Level.Debug)
                {
                    Log.Debug(
                        this, "NoteServiceProblem", problem, serviceBuildingId, targetBuildingId,
                        BuildingHelper.GetBuildingName(serviceBuildingId),
                        BuildingHelper.GetDistrictName(serviceBuildingId),
                        BuildingHelper.GetBuildingName(targetBuildingId),
                        BuildingHelper.GetDistrictName(targetBuildingId));

                    if (Global.EnableDevExperiments)
                    {
                        ServiceProblemNote note = new ServiceProblemNote(problem, serviceBuildingId, targetBuildingId);

                        uint oldValue;
                        if (!this.ServiceProblemCounts.TryGetValue(note.BuildingKey, out oldValue))
                        {
                            oldValue = 0;
                        }

                        this.ServiceProblemCounts[note.BuildingKey] = note.ProblemSize + oldValue;
                        this.ServiceProblemNotes.Add(note);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "AddServiceProblemNote", ex);
            }
        }

        /// <summary>
        /// Log debug info.
        /// </summary>
        public void DebugListLogServiceProblems()
        {
            try
            {
                DebugListLog(this.ServiceProblemNotes, "Note");
                DebugListLog(this.ServiceProblemSizes, "Size");
                DebugListLog(this.ServiceBuildingProblemSizes, "Size");
                DebugListLog(this.TargetBuildingProblemSizes, "Size");
            }
            catch (Exception ex)
            {
                Log.Error(this, "DebugListLogServiceProblems", ex);
            }
        }

        /// <summary>
        /// Gets the size of the building problem.
        /// </summary>
        /// <param name="serviceBuildingId">The service building identifier.</param>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <returns>The problem size.</returns>
        public uint GetBuildingProblemSize(ushort serviceBuildingId, ushort targetBuildingId)
        {
            uint key = ((uint)serviceBuildingId << 8) & (uint)targetBuildingId;
            uint size;

            if (this.ServiceProblemCounts.TryGetValue(key, out size))
            {
                return size;
            }

            return 0;
        }

        /// <summary>
        /// Gets the size of the service building problem.
        /// </summary>
        /// <param name="serviceBuildingId">The service building identifier.</param>
        /// <returns>The problem size.</returns>
        public uint GetServiceBuildingProblemSize(ushort serviceBuildingId)
        {
            return (uint)this.ServiceProblemNotes.Where(bp => bp.ServiceBuilding == serviceBuildingId).Sum(bp => bp.ProblemSize);
        }

        /// <summary>
        /// Gets the size of the target building problem.
        /// </summary>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <returns>The problem size.</returns>
        public uint GetTargetBuildingProblemSize(ushort targetBuildingId)
        {
            return (uint)this.ServiceProblemNotes.Where(bp => bp.TargetBuilding == targetBuildingId).Sum(bp => bp.ProblemSize);
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

            if (this.ServiceProblemCounts == null)
            {
                this.ServiceProblemCounts = new Dictionary<uint, uint>();
            }
            else
            {
                this.ServiceProblemCounts.Clear();
            }

            this.LastUpdate = Global.CurrentFrame;
        }

        /// <summary>
        /// Cleans this instance.
        /// </summary>
        public void Update()
        {
            try
            {
                IEnumerable<KeyValuePair<uint, uint>> oldCounts =
                    this.ServiceProblemNotes
                        .Where(bp => Global.CurrentFrame - bp.ProblemFrame > Global.ProblemLingerDelay)
                        .GroupBy(
                            bp => bp.BuildingKey,
                            (k, bpl) => new KeyValuePair<uint, uint>(
                                                k,
                                                (uint)bpl.Sum(bp => bp.ProblemSize)
                            ));

                foreach (KeyValuePair<uint, uint> oldCount in oldCounts)
                {
                    uint oldValue;
                    if (this.ServiceProblemCounts.TryGetValue(oldCount.Key, out oldValue))
                    {
                        if (oldValue <= oldCount.Value)
                        {
                            this.ServiceProblemCounts.Remove(oldCount.Key);
                        }
                        else
                        {
                            this.ServiceProblemCounts[oldCount.Key] = oldValue - oldCount.Value;
                        }
                    }
                }

                if (this.ServiceProblemNotes.Count > 0)
                {
                    this.ServiceProblemNotes.RemoveAll(bp => Global.CurrentFrame - bp.ProblemFrame > Global.ProblemLingerDelay);
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "Update", ex);
            }
            finally
            {
                this.LastUpdate = Global.CurrentFrame;
            }
        }

        /// <summary>
        /// Collects problem info for debug use.
        /// </summary>
        /// <param name="problemNote">The problem note.</param>
        /// <returns>The debug information.</returns>
        private static Log.InfoList DebugInfoMsg(ServiceProblemNote problemNote)
        {
            Log.InfoList info = new Log.InfoList();

            info.Add("BuildingKey", problemNote.BuildingKey);

            if (problemNote.ServiceBuilding == 0)
            {
                info.Add("O", "TargetBuilding");
                info.Add("BuildingId", problemNote.TargetBuilding);
            }
            else if (problemNote.TargetBuilding == 0)
            {
                info.Add("O", "ServiceBuilding");
                info.Add("BuildingId", problemNote.ServiceBuilding);
            }
            else
            {
                info.Add("O", "BuildingPair");
                info.Add("BuildingId", problemNote.ServiceBuilding, problemNote.TargetBuilding);
            }

            info.Add("ProblemSize", problemNote.ProblemSize);
            info.Add("ProblemFrame", problemNote.ProblemFrame);

            if (problemNote.ServiceBuilding > 0)
            {
                info.Add("ServiceBuildingId", problemNote.ServiceBuilding);
                info.Add("ServiceBuildingName", BuildingHelper.GetBuildingName(problemNote.ServiceBuilding));
                info.Add("ServiceBuildingDistrict", BuildingHelper.GetDistrictName(problemNote.ServiceBuilding));
            }

            if (problemNote.TargetBuilding > 0)
            {
                info.Add("TargetBuildingId", problemNote.TargetBuilding);
                info.Add("TargetBuildingName", BuildingHelper.GetBuildingName(problemNote.TargetBuilding));
                info.Add("TargetBuildingDistrict", BuildingHelper.GetDistrictName(problemNote.TargetBuilding));
            }

            return info;
        }

        /// <summary>
        /// Logs a list of problem info for debug use.
        /// </summary>
        /// <param name="problemNotes">The problem notes.</param>
        /// <param name="type">The type.</param>
        private static void DebugListLog(IEnumerable<ServiceProblemNote> problemNotes, string type)
        {
            foreach (ServiceProblemNote problemNote in problemNotes)
            {
                Log.InfoList info = DebugInfoMsg(problemNote);
                Log.DevDebug(typeof(ProblemKeeper), "DebugListLog", type, info.ToString());
            }
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
            /// <param name="serviceBuildingId">The service building.</param>
            /// <param name="targetBuildingId">The target building.</param>
            public ServiceProblemNote(ServiceProblem problem, ushort serviceBuildingId, ushort targetBuildingId)
            {
                this.ProblemFrame = Global.CurrentFrame;
                this.BuildingKey = ((uint)serviceBuildingId << 8) & (uint)targetBuildingId;

                switch (problem)
                {
                    case ServiceProblem.VehicleGone:
                        this.ProblemSize = 2;
                        break;

                    case ServiceProblem.VehicleNotCreated:
                    case ServiceProblem.PathNotFound:
                        this.ProblemSize = 3;
                        break;

                    default:
                        this.ProblemSize = 1;
                        break;
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceProblemNote" /> struct.
            /// </summary>
            /// <param name="highestFrame">The highest frame.</param>
            /// <param name="problemSize">Size of the problem.</param>
            /// <param name="serviceBuildingId">The service building identifier.</param>
            /// <param name="targetBuildingId">The target building identifier.</param>
            public ServiceProblemNote(uint highestFrame, uint problemSize, ushort serviceBuildingId, ushort targetBuildingId)
            {
                this.ProblemFrame = highestFrame;
                this.ProblemSize = problemSize;
                this.BuildingKey = ((uint)serviceBuildingId << 8) & (uint)targetBuildingId;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceProblemNote" /> struct.
            /// </summary>
            /// <param name="highestFrame">The highest frame.</param>
            /// <param name="problemSize">Size of the problem.</param>
            /// <param name="buildingKey">The building key.</param>
            public ServiceProblemNote(uint highestFrame, uint problemSize, uint buildingKey)
            {
                this.ProblemFrame = highestFrame;
                this.ProblemSize = problemSize;
                this.BuildingKey = buildingKey;
            }

            /// <summary>
            /// Gets or sets the building key.
            /// </summary>
            /// <value>
            /// The building key.
            /// </value>
            public uint BuildingKey { get; private set; }

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
            public uint ProblemSize { get; private set; }

            /// <summary>
            /// Gets or sets the service building.
            /// </summary>
            /// <value>
            /// The service building.
            /// </value>
            public ushort ServiceBuilding
            {
                get
                {
                    return (ushort)((this.BuildingKey >> 8) & 0xFFFF);
                }
            }

            /// <summary>
            /// Gets or sets the target building.
            /// </summary>
            /// <value>
            /// The target building.
            /// </value>
            public ushort TargetBuilding
            {
                get
                {
                    return (ushort)(this.BuildingKey & 0xFFFF);
                }
            }
        }
    }
}