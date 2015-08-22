using ColossalFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    internal class Buildings
    {
        /// <summary>
        /// The current/last building frame.
        /// </summary>
        private uint buildingFrame;

        /// <summary>
        /// The dead people buildings.
        /// </summary>
        private Dictionary<ushort, TargetBuildingInfo> deadPeopleBuildings = new Dictionary<ushort, TargetBuildingInfo>();

        /// <summary>
        /// The dirty buildings.
        /// </summary>
        private Dictionary<ushort, TargetBuildingInfo> dirtyBuildings = new Dictionary<ushort, TargetBuildingInfo>();

        /// <summary>
        /// The garbage buildings.
        /// </summary>
        private Dictionary<ushort, ServiceBuildingInfo> garbageBuildings = new Dictionary<ushort, ServiceBuildingInfo>();

        /// <summary>
        /// The hearse buildings.
        /// </summary>
        private Dictionary<ushort, ServiceBuildingInfo> hearseBuildings = new Dictionary<ushort, ServiceBuildingInfo>();

        /// <summary>
        /// The data is initialized.
        /// </summary>
        private bool isInitialized = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Buildings"/> class.
        /// </summary>
        public Buildings()
        {
            HasDeadPeopleBuildingsToCheck = false;
            HasDirtyBuildingsToCheck = false;
            Log.Debug(this, "Constructed");
        }

        /// <summary>
        /// Gets the dead people buildings.
        /// </summary>
        /// <value>
        /// The dead people buildings.
        /// </value>
        public IEnumerable<TargetBuildingInfo> DeadPeopleBuildings
        {
            get
            {
                return deadPeopleBuildings.Values;
            }
        }

        /// <summary>
        /// Gets the dirty buildings.
        /// </summary>
        /// <value>
        /// The dirty buildings.
        /// </value>
        public IEnumerable<TargetBuildingInfo> DirtyBuildings
        {
            get
            {
                return dirtyBuildings.Values;
            }
        }

        /// <summary>
        /// Gets the garbage buildings.
        /// </summary>
        /// <value>
        /// The garbage buildings.
        /// </value>
        public IEnumerable<ServiceBuildingInfo> GarbageBuildings
        {
            get
            {
                return garbageBuildings.Values;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has dead people buildings that should be checked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has dead people buildings to check; otherwise, <c>false</c>.
        /// </value>
        public bool HasDeadPeopleBuildingsToCheck { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance has dirty buildings that should be checked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has dirty buildings to check; otherwise, <c>false</c>.
        /// </value>
        public bool HasDirtyBuildingsToCheck { get; private set; }

        /// <summary>
        /// Gets the hearse buildings.
        /// </summary>
        /// <value>
        /// The hearse buildings.
        /// </value>
        public IEnumerable<ServiceBuildingInfo> HearseBuildings
        {
            get
            {
                return hearseBuildings.Values;
            }
        }

        /// <summary>
        /// Updates data.
        /// </summary>
        public void Update()
        {
            // Get and categorize buildings.
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            DistrictManager districtManager = null;
            if (Global.Settings.DispatchByDistrict && districtManager != null)
            {
                districtManager = Singleton<DistrictManager>.instance;
            }

            HasDeadPeopleBuildingsToCheck = false;
            HasDirtyBuildingsToCheck = false;

            // First update?
            if (isInitialized)
            {
                // Data is initialized. Just check buildings for this frame.

                uint endFrame = GetFrameEnd();

                uint counter = 0;
                while (buildingFrame != endFrame)
                {
                    if (counter > 256)
                    {
                        throw new Exception("Update frame loop counter too high!");
                    }
                    counter++;

                    buildingFrame = GetFrameNext(buildingFrame + 1);
                    FrameBoundaries bounds = GetFrameBoundaries(buildingFrame);

                    CategorizeBuildings(districtManager, ref buildings, bounds.FirstId, bounds.LastId);
                }
            }
            else
            {
                // Data is not initialized. Check all buildings.
                //if (Log.LogALot && Library.IsDebugBuild) Log.DevDebug(this, "OnUpdate", "Intialize");

                CategorizeBuildings(districtManager, ref buildings, 0, buildings.Length);

                buildingFrame = GetFrameEnd();
                isInitialized = Global.FramedUpdates;
            }
        }

        /// <summary>
        /// Categorizes the building.
        /// </summary>
        /// <param name="districtManager">The district manager.</param>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        private void CategorizeBuilding(DistrictManager districtManager, ushort buildingId, ref Building building)
        {
            // Checks for hearse dispatcher.
            if (Global.Settings.DispatchHearses)
            {
                // Check cemetaries and crematoriums.
                if (building.Info.m_buildingAI is CemeteryAI)
                {
                    if (!hearseBuildings.ContainsKey(buildingId))
                    {
                        ServiceBuildingInfo hearseBuilding = new ServiceBuildingInfo(districtManager, buildingId, ref building);
                        Log.Debug(this, "CategorizeBuilding", "Cemetary", buildingId, building.Info.name, hearseBuilding.BuildingName, hearseBuilding.Range, hearseBuilding.District);

                        hearseBuildings[buildingId] = hearseBuilding;
                    }
                    else
                    {
                        hearseBuildings[buildingId].Update(districtManager, ref building);
                    }
                }
                else if (hearseBuildings.ContainsKey(buildingId))
                {
                    Log.Debug(this, "CategorizeBuilding", "Not Cemetary", buildingId, building.Info.name, hearseBuildings[buildingId].BuildingName);

                    hearseBuildings.Remove(buildingId);
                }

                // Check dead people.
                if (building.m_deathProblemTimer > 0)
                {
                    bool problematic = (building.m_problems & Notification.Problem.Death) != Notification.Problem.None;

                    if (!deadPeopleBuildings.ContainsKey(buildingId))
                    {
                        TargetBuildingInfo deadPeopleBuilding = new TargetBuildingInfo(districtManager, buildingId, ref building, building.m_deathProblemTimer, 0, Notification.Problem.Death);
                        if (Log.LogALot || Library.IsDebugBuild) Log.Debug(this, "CategorizeBuilding", "Dead People", buildingId, building.Info.name, deadPeopleBuilding.BuildingName, deadPeopleBuilding.ProblemTimer, deadPeopleBuilding.HasProblem, deadPeopleBuilding.District);

                        deadPeopleBuildings[buildingId] = deadPeopleBuilding;
                        HasDeadPeopleBuildingsToCheck = true;
                    }
                    else
                    {
                        deadPeopleBuildings[buildingId].Update(districtManager, ref building, building.m_deathProblemTimer, 0, Notification.Problem.Death);
                        HasDeadPeopleBuildingsToCheck = HasDeadPeopleBuildingsToCheck || deadPeopleBuildings[buildingId].CheckThis;
                    }
                }
                else if (deadPeopleBuildings.ContainsKey(buildingId))
                {
                    if (Log.LogALot || Library.IsDebugBuild) Log.Debug(this, "CategorizeBuilding", "No Dead People", buildingId, building.Info.name, deadPeopleBuildings[buildingId].BuildingName);

                    deadPeopleBuildings.Remove(buildingId);
                }
            }

            if (Global.Settings.DispatchGarbageTrucks)
            {
                // Check landfills and incinerators.
                if (building.Info.m_buildingAI is LandfillSiteAI)
                {
                    if (!garbageBuildings.ContainsKey(buildingId))
                    {
                        ServiceBuildingInfo garbageBuilding = new ServiceBuildingInfo(districtManager, buildingId, ref building);
                        Log.Debug(this, "CategorizeBuilding", "Landfill", buildingId, building.Info.name, garbageBuilding.BuildingName, garbageBuilding.Range, garbageBuilding.District);

                        garbageBuildings[buildingId] = garbageBuilding;
                    }
                    else
                    {
                        garbageBuildings[buildingId].Update(districtManager, ref building);
                    }
                }
                else if (garbageBuildings.ContainsKey(buildingId))
                {
                    Log.Debug(this, "CategorizeBuilding", "Not Landfill", buildingId, building.Info.name, garbageBuildings[buildingId].BuildingName);

                    garbageBuildings.Remove(buildingId);
                }

                // Check dead people.
                int garbageAmount = building.Info.m_buildingAI.GetGarbageAmount(buildingId, ref building);
                if (garbageAmount > 0)
                {
                    bool problematic = (building.m_problems & Notification.Problem.Garbage) != Notification.Problem.None;

                    if (!dirtyBuildings.ContainsKey(buildingId))
                    {
                        TargetBuildingInfo dirtyBuilding = new TargetBuildingInfo(districtManager, buildingId, ref building, 0, garbageAmount, Notification.Problem.Garbage);
                        if (Log.LogALot || Library.IsDebugBuild) Log.Debug(this, "CategorizeBuilding", "Dirty", buildingId, building.Info.name, dirtyBuilding.BuildingName, garbageAmount, dirtyBuilding.HasProblem, dirtyBuilding.District);

                        dirtyBuildings[buildingId] = dirtyBuilding;
                        HasDirtyBuildingsToCheck = true;
                    }
                    else
                    {
                        dirtyBuildings[buildingId].Update(districtManager, ref building, building.m_serviceProblemTimer, garbageAmount, Notification.Problem.Garbage);
                        HasDirtyBuildingsToCheck = HasDirtyBuildingsToCheck || dirtyBuildings[buildingId].CheckThis;
                    }
                }
                else if (dirtyBuildings.ContainsKey(buildingId))
                {
                    if (Log.LogALot || Library.IsDebugBuild) Log.Debug(this, "CategorizeBuilding", "Not Dirty", buildingId, building.Info.name, dirtyBuildings[buildingId].BuildingName);

                    dirtyBuildings.Remove(buildingId);
                }
            }
        }

        /// <summary>
        /// Gets the name of the building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <returns></returns>
        public static string GetBuildingName(ushort buildingId)
        {
            try
            {
                string name = null;
                BuildingManager manager = Singleton<BuildingManager>.instance;

                try
                {
                    name = manager.GetBuildingName(buildingId, new InstanceID());
                }
                catch { }

                if (String.IsNullOrEmpty(name))
                {
                    Building[] buildings = manager.m_buildings.m_buffer;

                    name = buildings[buildingId].Info.name;
                }

                return String.IsNullOrEmpty(name) ? (string)null : name;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Categorizes the buildings.
        /// </summary>
        /// <param name="districtManager">The district manager.</param>
        /// <param name="buildings">The buildings.</param>
        /// <param name="firstBuildingId">The first building identifier.</param>
        /// <param name="lastBuildingId">The last building identifier.</param>
        private void CategorizeBuildings(DistrictManager districtManager, ref Building[] buildings, ushort firstBuildingId, int lastBuildingId)
        {
            for (ushort id = firstBuildingId; id < lastBuildingId; id++)
            {
                if (buildings[id].Info == null)
                {
                    if (Global.Settings.DispatchHearses)
                    {
                        if (hearseBuildings.ContainsKey(id))
                        {
                            hearseBuildings.Remove(id);
                            Log.Debug(this, "CategorizeBuildings", "Not Cemetary Building", id);
                        }

                        if (deadPeopleBuildings.ContainsKey(id))
                        {
                            hearseBuildings.Remove(id);
                            Log.Debug(this, "CategorizeBuildings", "Not Dead People Building", id);
                        }
                    }

                    if (Global.Settings.DispatchGarbageTrucks)
                    {
                        if (garbageBuildings.ContainsKey(id))
                        {
                            garbageBuildings.Remove(id);
                            Log.Debug(this, "CategorizeBuildings", "Not Landfill Building", id);
                        }

                        if (dirtyBuildings.ContainsKey(id))
                        {
                            dirtyBuildings.Remove(id);
                            Log.Debug(this, "CategorizeBuildings", "Not Dirty Building", id);
                        }
                    }
                }
                else
                {
                    CategorizeBuilding(districtManager, id, ref buildings[id]);
                }
            }
        }

        /// <summary>
        /// Gets the frame boundaries.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>The frame boundaries.</returns>
        private FrameBoundaries GetFrameBoundaries(uint frame)
        {
            frame = frame & 255;

            return new FrameBoundaries(frame * 128, (frame + 1) * 128 - 1);
        }

        /// <summary>
        /// Gets the end frame.
        /// </summary>
        /// <returns>The end frame.</returns>
        private uint GetFrameEnd()
        {
            return Singleton<SimulationManager>.instance.m_currentFrameIndex & 255;
        }

        /// <summary>
        /// Gets the next frame.
        /// </summary>
        /// <param name="frame">The current/last frame.</param>
        /// <returns>The next frame.</returns>
        private uint GetFrameNext(uint frame)
        {
            return frame & 255;
        }

        /// <summary>
        /// Info about a service building.
        /// </summary>
        public class ServiceBuildingInfo
        {
            /// <summary>
            /// The building identifier.
            /// </summary>
            public ushort BuildingId = 0;

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
                    return GetBuildingName(BuildingId);
                }
            }

            /// <summary>
            /// The distance to the target.
            /// </summary>
            public float Distance = float.PositiveInfinity;

            /// <summary>
            /// The district the building is in.
            /// </summary>
            public byte District = 0;

            /// <summary>
            /// The buildings first own vehicle.
            /// </summary>
            public ushort FirstOwnVehicle = 0;

            /// <summary>
            /// The building is in target district.
            /// </summary>
            public bool InDistrict = false;

            /// <summary>
            /// The target building is in service bilding's effective range.
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
            public Dictionary<ushort, Vehicles.ServiceVehicleInfo> Vehicles = new Dictionary<ushort, Vehicles.ServiceVehicleInfo>();

            /// <summary>
            /// The last info update stamp.
            /// </summary>
            private uint lastInfoUpdate = 0;

            /// <summary>
            /// The last update stamp.
            /// </summary>
            private uint lastUpdate = 0;

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceBuildingInfo"/> class.
            /// </summary>
            /// <param name="districtManager">The district manager.</param>
            /// <param name="buildingId">The building identifier.</param>
            /// <param name="building">The building.</param>
            public ServiceBuildingInfo(DistrictManager districtManager, ushort buildingId, ref Building building)
            {
                this.lastUpdate = Global.CurrentFrame;

                this.BuildingId = buildingId;
                this.Position = building.m_position;
                this.FirstOwnVehicle = building.m_ownVehicles;

                if (districtManager != null)
                {
                    this.District = districtManager.GetDistrict(Position);
                }

                if (Global.Settings.LimitRange)
                {
                    this.Range = building.Info.m_buildingAI.GetCurrentRange(buildingId, ref building);
                    this.Range = this.Range * this.Range * Global.Settings.RangeModifier;
                }
            }

            /// <summary>
            /// The building is updated.
            /// </summary>
            public bool Updated
            {
                get
                {
                    return lastUpdate == Global.CurrentFrame;
                }
            }

            /// <summary>
            /// Sets the target information.
            /// </summary>
            /// <param name="districtManager">The district manager.</param>
            /// <param name="building">The building.</param>
            /// <param name="ignoreRange">if set to <c>true</c> ignore the range.</param>
            public void SetTargetInfo(DistrictManager districtManager, TargetBuildingInfo building, bool ignoreRange)
            {
                this.Distance = (this.Position - building.Position).sqrMagnitude;

                if (Global.Settings.DispatchByDistrict && districtManager != null)
                {
                    byte district = districtManager.GetDistrict(building.Position);
                    this.InDistrict = (district == this.District);
                }
                else
                {
                    this.InDistrict = false;
                }

                this.InRange = !Global.Settings.LimitRange || ignoreRange || this.InDistrict || (this.Distance < this.Range);
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
                this.FirstOwnVehicle = building.m_ownVehicles;

                if (Global.CurrentFrame - lastInfoUpdate > Global.ObjectUpdateInterval)
                {
                    lastInfoUpdate = Global.ObjectUpdateInterval;

                    if (districtManager != null)
                    {
                        this.District = districtManager.GetDistrict(Position);
                    }

                    if (Global.Settings.LimitRange)
                    {
                        this.Range = building.Info.m_buildingAI.GetCurrentRange(BuildingId, ref building);
                        this.Range = this.Range * this.Range * 2;
                    }
                }
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

        /// <summary>
        /// Info about a target building.
        /// </summary>
        public class TargetBuildingInfo
        {
            /// <summary>
            /// The amount of something.
            /// </summary>
            public int Amount;

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
            /// The position.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// The problem timer.
            /// </summary>
            public byte ProblemTimer;

            /// <summary>
            /// Wether this building should be checked or not.
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
            /// <param name="amount">The amount.</param>
            /// <param name="problemToCheck">The problem to check.</param>
            public TargetBuildingInfo(DistrictManager districtManager, ushort buildingId, ref Building building, byte problemTimer, int amount, Notification.Problem problemToCheck)
            {
                this.lastUpdate = Global.CurrentFrame;
                this.lastInfoUpdate = Global.CurrentFrame;

                this.BuildingId = buildingId;
                this.ProblemTimer = problemTimer;
                this.Amount = amount;
                this.HasProblem = (building.m_problems & problemToCheck) == problemToCheck;
                this.Position = building.m_position;

                if (districtManager != null)
                {
                    this.District = districtManager.GetDistrict(Position);
                }
                else
                {
                    this.District = 0;
                }

                checkThis = true;
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
                    return (Global.RecheckInterval > 0 && Global.CurrentFrame - lastCheck < Global.RecheckInterval);
                }
                set
                {
                    if (value)
                    {
                        lastCheck = Global.CurrentFrame;
                    }
                    else
                    {
                        lastCheck = 0;
                    }
                }
            }

            /// <summary>
            /// Gets a value indicating whether to check this building.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this building should be checked; otherwise, <c>false</c>.
            /// </value>
            public bool CheckThis
            {
                get
                {
                    return this.checkThis && lastUpdate == Global.CurrentFrame;
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
                    return (Global.RecheckHandledInterval > 0 && Global.CurrentFrame - lastHandled < Global.RecheckHandledInterval);
                }
                set
                {
                    if (value)
                    {
                        this.checkThis = false;
                        lastHandled = Global.CurrentFrame;
                    }
                    else
                    {
                        lastHandled = 0;
                    }
                }
            }

            /// <summary>
            /// The building is updated.
            /// </summary>
            public bool Updated
            {
                get
                {
                    return lastUpdate == Global.CurrentFrame;
                }
            }

            /// <summary>
            /// Updates the building.
            /// </summary>
            /// <param name="districtManager">The district manager.</param>
            /// <param name="building">The building.</param>
            /// <param name="problemTimer">The problem timer.</param>
            /// <param name="amount">The amount.</param>
            /// <param name="problemToCheck">The problem to check.</param>
            public void Update(DistrictManager districtManager, ref Building building, byte problemTimer, int amount, Notification.Problem problemToCheck)
            {
                this.lastUpdate = Global.CurrentFrame;

                this.ProblemTimer = problemTimer;
                this.Amount = amount;
                this.HasProblem = (building.m_problems & problemToCheck) == problemToCheck;
                this.Position = building.m_position;

                if (Global.CurrentFrame - lastInfoUpdate > Global.ObjectUpdateInterval)
                {
                    lastInfoUpdate = Global.ObjectUpdateInterval;

                    if (districtManager != null)
                    {
                        this.District = districtManager.GetDistrict(Position);
                    }
                }

                this.checkThis = ((Global.RecheckInterval == 0 || Global.CurrentFrame - lastCheck >= Global.RecheckInterval) &&
                                  (Global.RecheckHandledInterval == 0 || Global.CurrentFrame - lastHandled >= Global.RecheckHandledInterval));
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
                    return GetBuildingName(BuildingId);
                }
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
                        int r = (int)y.ProblemTimer - (int)x.ProblemTimer;
                        if (r == 0)
                        {
                            r = y.Amount - x.Amount;
                        }

                        return r;
                    }
                }
            }
        }
    }
}