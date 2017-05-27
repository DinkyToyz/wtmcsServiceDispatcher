using System;
using System.Collections.Generic;
using System.Linq;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Keeps track of problems.
    /// </summary>
    internal class ServiceProblemKeeper
    {
        /// <summary>
        /// Whether this functionality is broken.
        /// </summary>
        private bool broken = false;

        /// <summary>
        /// The service problem size where difference is counted as important.
        /// </summary>
        public const ushort ProblemSizeImportant = 100;

        /// <summary>
        /// The default service problem weight.
        /// </summary>
        private readonly byte defaultWeight = 10;

        /// <summary>
        /// The service problem notes.
        /// </summary>
        private List<ServiceProblemNote> ProblemNotes = null;

        /// <summary>
        /// The service problem sizes.
        /// </summary>
        private Dictionary<uint, uint> ProblemSizes = null;

        /// <summary>
        /// The service problem weights.
        /// </summary>
        private Dictionary<ServiceProblem, byte> ServiceProblemWeights = new Dictionary<ServiceProblem, byte>
        {
            {ServiceProblem.VehicleGone, 20},
            {ServiceProblem.VehicleNotCreated, 30 },
            {ServiceProblem.PathNotFound, 30 }
        };

        /// <summary>
        /// The building problems.
        /// </summary>
        private Dictionary<ushort, BuildingProblem> TargetBuildingProblems = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProblemKeeper"/> class.
        /// </summary>
        public ServiceProblemKeeper()
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
        /// Notes a problem.
        /// </summary>
        /// <param name="problem">The problem.</param>
        /// <param name="serviceBuildingId">The service building identifier.</param>
        /// <param name="targetBuildingId">The target building identifier.</param>
        public void Add(ServiceProblem problem, ushort serviceBuildingId, ushort targetBuildingId)
        {
            if (this.broken)
            {
                return;
            }

            try
            {
                Log.Debug(
                    this, "Add", problem, serviceBuildingId, targetBuildingId,
                    BuildingHelper.GetBuildingName(serviceBuildingId),
                    BuildingHelper.GetDistrictName(serviceBuildingId),
                    BuildingHelper.GetBuildingName(targetBuildingId),
                    BuildingHelper.GetDistrictName(targetBuildingId));

                uint key = MakeBuildingKey(serviceBuildingId, targetBuildingId);
                byte weight = this.GetServiceProblemWeight(problem);

                uint oldValue;
                if (!this.ProblemSizes.TryGetValue(key, out oldValue))
                {
                    oldValue = 0;
                }

                uint newValue = oldValue + weight;
                this.ProblemSizes[key] = newValue;

                ushort oldCount = 0;
                uint oldSize = 0;
                BuildingProblem buildingProblem;
                if (this.TargetBuildingProblems.TryGetValue(targetBuildingId, out buildingProblem))
                {
                    oldCount = buildingProblem.Count;
                    oldSize = buildingProblem.Size;

                    buildingProblem.Count++;
                    buildingProblem.Size += weight;
                }
                else
                {
                    buildingProblem = new BuildingProblem(weight);
                }

                this.TargetBuildingProblems[targetBuildingId] = buildingProblem;

                Log.Debug(this, "Add", problem, serviceBuildingId, targetBuildingId, key, oldValue, oldCount, oldSize, weight, newValue, buildingProblem.Count, buildingProblem.Size);

                if (this.ProblemNotes != null)
                {
                    this.ProblemNotes.Add(new ServiceProblemNote(problem, serviceBuildingId, targetBuildingId, weight));
                }
            }
            catch (Exception ex)
            {
                this.broken = true;
                Log.Error(this, "Add", ex);
            }
        }

        /// <summary>
        /// Logs a list of service problem info for debug use.
        /// </summary>
        public void DebugListLogServiceProblems()
        {
            try
            {
                foreach (KeyValuePair<uint, uint> size in this.ProblemSizes.ToArray())
                {
                    ushort serviceBuildingId = GetServiceBuildingFromBuildingKey(size.Key);
                    ushort targetBuildingId = GetTargetBuildingFromBuildingKey(size.Key);

                    Log.InfoList info = new Log.InfoList();

                    info.Add("ServiceBuildingId", serviceBuildingId);
                    info.Add("TargetBuildingId", targetBuildingId);
                    info.Add("ProblemSize", size.Value);

                    info.Add("ServiceBuildingName", BuildingHelper.GetBuildingName(serviceBuildingId));
                    info.Add("TargetBuildingName", BuildingHelper.GetBuildingName(targetBuildingId));

                    Log.DevDebug(this, "DebugListLog", info.ToString());
                }

                if (this.ProblemNotes != null)
                {
                    foreach (ServiceProblemNote note in this.ProblemNotes.OrderBy(n => n.ProblemFrame))
                    {
                        note.DebugListLog();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "AddServiceProblemNote", ex);
            }
        }

        /// <summary>
        /// Gets the size of the service problem.
        /// </summary>
        /// <param name="serviceBuildingId">The service building identifier.</param>
        /// <param name="targetBuidlingId">The target buidling identifier.</param>
        /// <returns>The service problem size.</returns>
        public uint GetProblemSize(ushort serviceBuildingId, ushort targetBuidlingId)
        {
            uint size;
            if (this.ProblemSizes.TryGetValue(MakeBuildingKey(serviceBuildingId, targetBuidlingId), out size))
            {
                return size;
            }

            return 0;
        }

        /// <summary>
        /// Gets the number of problems for the service building.
        /// </summary>
        /// <param name="serviceBuildingId">The service building identifier.</param>
        /// <returns>The number of problems for the building.</returns>
        public ushort GetServiceBuildingProblemCount(ushort serviceBuildingId)
        {
            return (ushort)(this.ProblemSizes.Where(sz => GetServiceBuildingFromBuildingKey(sz.Key) == serviceBuildingId).Count());
        }

        /// <summary>
        /// Gets the size of the service building problem.
        /// </summary>
        /// <param name="serviceBuildingId">The service building identifier.</param>
        /// <returns>The total problem size for the building.</returns>
        public uint GetServiceBuildingProblemSize(ushort serviceBuildingId)
        {
            return (uint)(this.ProblemSizes.Where(sz => GetServiceBuildingFromBuildingKey(sz.Key) == serviceBuildingId).Sum(sz => sz.Value));
        }

        /// <summary>
        /// Gets the number of problems for the target building.
        /// </summary>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <returns>The number of problems for the building.</returns>
        public ushort GetTargetBuildingProblemCount(ushort targetBuildingId)
        {
            BuildingProblem problem;
            if (this.TargetBuildingProblems.TryGetValue(targetBuildingId, out problem))
            {
                return problem.Count;
            }

            return 0;
        }

        /// <summary>
        /// Gets the target building problem information.
        /// </summary>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <param name="problemCount">The problem count.</param>
        /// <param name="problemSize">Size of the problem.</param>
        public void GetTargetBuildingProblemInfo(ushort targetBuildingId, out ushort problemCount, out uint problemSize)
        {
            BuildingProblem problem;
            if (this.TargetBuildingProblems.TryGetValue(targetBuildingId, out problem))
            {
                problemCount = problem.Count;
                problemSize = problem.Size;
            }
            else
            {
                problemCount = 0;
                problemSize = 0;
            }
        }

        /// <summary>
        /// Gets the size of the target building problem.
        /// </summary>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <returns>The total problem size for the building.</returns>
        public uint GetTargetBuildingProblemSize(ushort targetBuildingId)
        {
            BuildingProblem problem;
            if (this.TargetBuildingProblems.TryGetValue(targetBuildingId, out problem))
            {
                return problem.Size;
            }

            return 0;
        }

        /// <summary>
        /// Re-initialize the part.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void ReInitialize()
        {
            if (this.ProblemSizes == null)
            {
                this.ProblemSizes = new Dictionary<uint, uint>();
            }
            else
            {
                this.ProblemSizes.Clear();
            }

            if (this.TargetBuildingProblems == null)
            {
                this.TargetBuildingProblems = new Dictionary<ushort, BuildingProblem>();
            }
            else
            {
                this.TargetBuildingProblems.Clear();
            }

            if (Log.LogALot && Log.LogToFile)
            {
                if (this.ProblemNotes == null)
                {
                    this.ProblemNotes = new List<ServiceProblemNote>();
                }
                else
                {
                    this.ProblemNotes.Clear();
                }
            }
            else if (this.ProblemNotes != null)
            {
                this.ProblemNotes = null;
            }

            this.LastUpdate = Global.CurrentFrame;
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public void Update()
        {
            try
            {
                ushort buildingId;
                BuildingProblem problem;

                uint decrement = (Global.CurrentFrame - this.LastUpdate) / Global.ProblemLingerDelay;

                if (decrement == 0)
                {
                    return;
                }

                if (Log.LogALot && Log.LogToFile)
                {
                    Log.DevDebug(this, "Update", decrement);
                }

                foreach (KeyValuePair<uint, uint> size in this.ProblemSizes.ToArray())
                {
                    buildingId = GetTargetBuildingFromBuildingKey(size.Key);
                    Log.DevDebug(this, "Update", buildingId, decrement);

                    problem = this.TargetBuildingProblems[buildingId];

                    if (size.Value <= decrement)
                    {
                        if (Log.LogALot && Log.LogToFile)
                        {
                            Log.DevDebug(this, "Update", "remove", GetServiceBuildingFromBuildingKey(size.Key), buildingId, size.Value, problem.Size, problem.Count);
                        }

                        if (problem.Count <= 1)
                        {
                            this.TargetBuildingProblems.Remove(buildingId);
                        }
                        else
                        {
                            problem.Count--;
                            problem.Size = (problem.Size <= size.Value) ? 1 : problem.Size - size.Value;
                            this.TargetBuildingProblems[buildingId] = problem;
                        }

                        this.ProblemSizes.Remove(size.Key);
                    }
                    else
                    {
                        if (Log.LogALot && Log.LogToFile)
                        {
                            Log.DevDebug(this, "Update", "decrement", GetServiceBuildingFromBuildingKey(size.Key), buildingId, size.Value, problem.Size, size.Value - decrement, problem.Size -= decrement);
                        }

                        this.ProblemSizes[size.Key] = size.Value - decrement;

                        problem.Size = (problem.Size <= decrement) ? size.Value - decrement : problem.Size - decrement;
                        this.TargetBuildingProblems[buildingId] = problem;
                    }
                }

                if (this.ProblemNotes != null && decrement >= 10 && this.ProblemNotes.Count > 0)
                {
                    this.ProblemNotes.RemoveAll(n => Global.CurrentFrame - n.ProblemFrame > Global.ProblemLingerDelay * n.ProblemWeight);
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
        /// Gets the service building from the building key.
        /// </summary>
        /// <param name="buildingKey">The building key.</param>
        /// <returns>The service building id.</returns>
        private static ushort GetServiceBuildingFromBuildingKey(uint buildingKey)
        {
            return (ushort)((buildingKey >> 16) & 0xFFFF);
        }

        /// <summary>
        /// Gets the target building from the building key.
        /// </summary>
        /// <param name="buildingKey">The building key.</param>
        /// <returns>The target building id.</returns>
        private static ushort GetTargetBuildingFromBuildingKey(uint buildingKey)
        {
            return (ushort)(buildingKey & 0xFFFF);
        }

        /// <summary>
        /// Makes a building key.
        /// </summary>
        /// <param name="serviceBuildingId">The service building identifier.</param>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <returns>The building key.</returns>
        private static uint MakeBuildingKey(ushort serviceBuildingId, ushort targetBuildingId)
        {
            return ((uint)serviceBuildingId << 16) | (uint)targetBuildingId;
        }

        /// <summary>
        /// Gets the service problem weight.
        /// </summary>
        /// <param name="serviceProblem">The service problem.</param>
        /// <returns>The service problem weight.</returns>
        private byte GetServiceProblemWeight(ServiceProblem serviceProblem)
        {
            byte weight;
            if (this.ServiceProblemWeights.TryGetValue(serviceProblem, out weight))
            {
                return weight;
            }

            return this.defaultWeight;
        }

        /// <summary>
        /// The building problem info.
        /// </summary>
        private struct BuildingProblem
        {
            /// <summary>
            /// The problem count.
            /// </summary>
            public ushort Count;

            /// <summary>
            /// The problem size.
            /// </summary>
            public uint Size;

            /// <summary>
            /// Initializes a new instance of the <see cref="BuildingProblem"/> struct.
            /// </summary>
            /// <param name="problemSize">Size of the problem.</param>
            public BuildingProblem(uint problemSize)
            {
                this.Count = 1;
                this.Size = problemSize;
            }
        }

        /// <summary>
        /// Note that there was a service problem.
        /// </summary>
        private struct ServiceProblemNote
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceProblemNote" /> struct.
            /// </summary>
            /// <param name="serviceProblem">The problem.</param>
            /// <param name="serviceBuildingId">The service building.</param>
            /// <param name="targetBuildingId">The target building.</param>
            /// <param name="problemWeight">The problem weight.</param>
            public ServiceProblemNote(ServiceProblem serviceProblem, ushort serviceBuildingId, ushort targetBuildingId, byte problemWeight)
            {
                this.ServiceProblem = serviceProblem;
                this.ProblemFrame = Global.CurrentFrame;
                this.BuildingKey = MakeBuildingKey(serviceBuildingId, targetBuildingId);
                this.ProblemWeight = problemWeight;
            }

            /// <summary>
            /// Gets or sets the building key.
            /// </summary>
            /// <value>
            /// The building key.
            /// </value>
            public uint BuildingKey { get; private set; }

            /// <summary>
            /// Gets the problem frame.
            /// </summary>
            /// <value>
            /// The problem frame.
            /// </value>
            public uint ProblemFrame { get; private set; }

            /// <summary>
            /// Gets the problem weight.
            /// </summary>
            /// <value>
            /// The problem value.
            /// </value>
            public byte ProblemWeight { get; private set; }

            /// <summary>
            /// Gets the service building.
            /// </summary>
            /// <value>
            /// The service building.
            /// </value>
            public ushort ServiceBuilding => GetServiceBuildingFromBuildingKey(this.BuildingKey);

            /// <summary>
            /// Gets or sets the service problem.
            /// </summary>
            /// <value>
            /// The service problem.
            /// </value>
            public ServiceProblem ServiceProblem { get; private set; }

            /// <summary>
            /// Gets the target building.
            /// </summary>
            /// <value>
            /// The target building.
            /// </value>
            public ushort TargetBuilding => GetTargetBuildingFromBuildingKey(this.BuildingKey);

            public void DebugListLog()
            {
                Log.InfoList info = new Log.InfoList();

                info.Add("ServiceBuildingId", this.ServiceBuilding);
                info.Add("TargetBuildingId", this.TargetBuilding);
                info.Add("Problem", this.ServiceProblem);
                info.Add("Weight", this.ProblemWeight);

                info.Add("ServiceBuildingName", BuildingHelper.GetBuildingName(this.ServiceBuilding));
                info.Add("TargetBuildingName", BuildingHelper.GetBuildingName(this.TargetBuilding));

                Log.DevDebug(this, "DebugListLog", info.ToString());
            }
        }
    }
}