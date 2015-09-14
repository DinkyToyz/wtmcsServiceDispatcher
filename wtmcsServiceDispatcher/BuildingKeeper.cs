using ColossalFramework;
using System;
using System.Collections.Generic;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Keeps track of interesting buildings.
    /// </summary>
    internal class BuildingKeeper
    {
        /// <summary>
        /// The current/last building frame.
        /// </summary>
        private uint buildingFrame;

        /// <summary>
        /// The data is initialized.
        /// </summary>
        private bool isInitialized = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingKeeper"/> class.
        /// </summary>
        public BuildingKeeper()
        {
            this.HasDeadPeopleBuildingsToCheck = false;
            this.HasDirtyBuildingsToCheck = false;

            if (Global.Settings.DispatchHearses)
            {
                this.DeadPeopleBuildings = new Dictionary<ushort, TargetBuildingInfo>();
                this.HearseBuildings = new Dictionary<ushort, ServiceBuildingInfo>();
            }
            else
            {
                this.DeadPeopleBuildings = null;
                this.HearseBuildings = null;
            }

            if (Global.Settings.DispatchGarbageTrucks)
            {
                this.DirtyBuildings = new Dictionary<ushort, TargetBuildingInfo>();
                this.GarbageBuildings = new Dictionary<ushort, ServiceBuildingInfo>();
            }
            else
            {
                this.DirtyBuildings = null;
                this.GarbageBuildings = null;
            }

            Log.Debug(this, "Constructed");
        }

        /// <summary>
        /// Gets the dead people buildings.
        /// </summary>
        public Dictionary<ushort, TargetBuildingInfo> DeadPeopleBuildings { get; private set; }

        /// <summary>
        /// Gets the dirty buildings.
        /// </summary>
        public Dictionary<ushort, TargetBuildingInfo> DirtyBuildings { get; private set; }

        /// <summary>
        /// Gets the garbage buildings.
        /// </summary>
        public Dictionary<ushort, ServiceBuildingInfo> GarbageBuildings { get; private set; }

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
        public Dictionary<ushort, ServiceBuildingInfo> HearseBuildings { get; private set; }

        /// <summary>
        /// Logs a list of service building info for debug use.
        /// </summary>
        public void DebugListLogBuildings()
        {
            try
            {
                if (Global.Settings.DispatchHearses)
                {
                    if (this.DeadPeopleBuildings != null)
                    {
                        BuildingHelper.DebugListLog(this.DeadPeopleBuildings.Values);
                    }

                    if (this.HearseBuildings != null)
                    {
                        BuildingHelper.DebugListLog(this.HearseBuildings.Values);
                    }
                }

                if (Global.Settings.DispatchGarbageTrucks)
                {
                    if (this.DirtyBuildings != null)
                    {
                        BuildingHelper.DebugListLog(this.DirtyBuildings.Values);
                    }

                    if (this.GarbageBuildings != null)
                    {
                        BuildingHelper.DebugListLog(this.GarbageBuildings.Values);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(BuildingKeeper), "DebugListLogBuildings", ex);
            }
        }

        /// <summary>
        /// Logs a list of service building info for debug use.
        /// </summary>
        public void DebugListLogServiceBuildings()
        {
            try
            {
                if (Global.Settings.DispatchHearses && this.HearseBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.HearseBuildings.Values);
                }

                if (Global.Settings.DispatchGarbageTrucks && this.GarbageBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.GarbageBuildings.Values);
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(BuildingKeeper), "DebugListLogServiceBuildings", ex);
            }
        }

        /// <summary>
        /// Logs a list of service building info for debug use.
        /// </summary>
        public void DebugListLogTargetBuildings()
        {
            try
            {
                if (Global.Settings.DispatchHearses && this.DeadPeopleBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.DeadPeopleBuildings.Values);
                }

                if (Global.Settings.DispatchGarbageTrucks && this.DirtyBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.DirtyBuildings.Values);
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(BuildingKeeper), "DebugListLogTargetBuildings", ex);
            }
        }

        /// <summary>
        /// Updates data.
        /// </summary>
        /// <exception cref="System.Exception">Update frame loop counter too high.</exception>
        public void Update()
        {
            // Get and categorize buildings.
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            DistrictManager districtManager = null;
            if (Global.Settings.DispatchByDistrict || Log.LogNames || Log.LogALot)
            {
                districtManager = Singleton<DistrictManager>.instance;
            }

            this.HasDeadPeopleBuildingsToCheck = false;
            this.HasDirtyBuildingsToCheck = false;

            // First update?
            if (this.isInitialized)
            {
                // Data is initialized. Just check buildings for this frame.
                uint endFrame = this.GetFrameEnd();

                uint counter = 0;
                while (this.buildingFrame != endFrame)
                {
                    if (counter > 256)
                    {
                        throw new Exception("Update frame loop counter too high");
                    }
                    counter++;

                    this.buildingFrame = this.GetFrameNext(this.buildingFrame + 1);
                    FrameBoundaries bounds = this.GetFrameBoundaries(this.buildingFrame);

                    this.CategorizeBuildings(districtManager, ref buildings, bounds.FirstId, bounds.LastId);
                }
            }
            else
            {
                this.CategorizeBuildings(districtManager, ref buildings, 0, buildings.Length);

                this.buildingFrame = this.GetFrameEnd();
                this.isInitialized = Global.FramedUpdates;
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
                    if (!this.HearseBuildings.ContainsKey(buildingId))
                    {
                        ServiceBuildingInfo hearseBuilding = new ServiceBuildingInfo(districtManager, buildingId, ref building);
                        Log.Debug(this, "CategorizeBuilding", "Cemetary", buildingId, building.Info.name, hearseBuilding.BuildingName, hearseBuilding.Range, hearseBuilding.District);

                        this.HearseBuildings[buildingId] = hearseBuilding;
                    }
                    else
                    {
                        this.HearseBuildings[buildingId].Update(districtManager, ref building);
                    }
                }
                else if (this.HearseBuildings.ContainsKey(buildingId))
                {
                    Log.Debug(this, "CategorizeBuilding", "Not Cemetary", buildingId);

                    this.HearseBuildings.Remove(buildingId);
                }

                // Check dead people.
                if (building.m_deathProblemTimer > 0)
                {
                    if (!this.DeadPeopleBuildings.ContainsKey(buildingId))
                    {
                        TargetBuildingInfo deadPeopleBuilding = new TargetBuildingInfo(districtManager, buildingId, ref building, building.m_deathProblemTimer, null, true, Notification.Problem.Death);
                        if (Log.LogToFile) Log.Debug(this, "CategorizeBuilding", "Dead People", buildingId, building.Info.name, deadPeopleBuilding.BuildingName, building.m_deathProblemTimer, deadPeopleBuilding.ProblemValue, deadPeopleBuilding.HasProblem, deadPeopleBuilding.District);

                        this.DeadPeopleBuildings[buildingId] = deadPeopleBuilding;
                        this.HasDeadPeopleBuildingsToCheck = true;
                    }
                    else
                    {
                        this.DeadPeopleBuildings[buildingId].Update(districtManager, ref building, building.m_deathProblemTimer, null, true, Notification.Problem.Death);
                        this.HasDeadPeopleBuildingsToCheck = this.HasDeadPeopleBuildingsToCheck || this.DeadPeopleBuildings[buildingId].CheckThis;
                    }
                }
                else if (this.DeadPeopleBuildings.ContainsKey(buildingId))
                {
                    if (Log.LogToFile) Log.Debug(this, "CategorizeBuilding", "No Dead People", buildingId);

                    this.DeadPeopleBuildings.Remove(buildingId);
                }
            }

            if (Global.Settings.DispatchGarbageTrucks)
            {
                // Check landfills and incinerators.
                if (building.Info.m_buildingAI is LandfillSiteAI)
                {
                    if (!this.GarbageBuildings.ContainsKey(buildingId))
                    {
                        ServiceBuildingInfo garbageBuilding = new ServiceBuildingInfo(districtManager, buildingId, ref building);
                        Log.Debug(this, "CategorizeBuilding", "Landfill", buildingId, building.Info.name, garbageBuilding.BuildingName, garbageBuilding.Range, garbageBuilding.District);

                        this.GarbageBuildings[buildingId] = garbageBuilding;
                    }
                    else
                    {
                        this.GarbageBuildings[buildingId].Update(districtManager, ref building);
                    }
                }
                else if (this.GarbageBuildings.ContainsKey(buildingId))
                {
                    Log.Debug(this, "CategorizeBuilding", "Not Landfill", buildingId);

                    this.GarbageBuildings.Remove(buildingId);
                }

                // Check garbage.
                if (building.m_garbageBuffer >= Global.Settings.MinimumGarbageForDispatch && !(building.Info.m_buildingAI is LandfillSiteAI))
                {
                    if (!this.DirtyBuildings.ContainsKey(buildingId))
                    {
                        TargetBuildingInfo dirtyBuilding = new TargetBuildingInfo(districtManager, buildingId, ref building, null, building.m_garbageBuffer, true, Notification.Problem.Garbage);
                        if (Log.LogToFile) Log.Debug(this, "CategorizeBuilding", "Dirty", buildingId, building.Info.name, dirtyBuilding.BuildingName, building.m_garbageBuffer, dirtyBuilding.ProblemValue, dirtyBuilding.HasProblem, dirtyBuilding.District);

                        this.DirtyBuildings[buildingId] = dirtyBuilding;
                        this.HasDirtyBuildingsToCheck = true;
                    }
                    else
                    {
                        this.DirtyBuildings[buildingId].Update(districtManager, ref building, null, building.m_garbageBuffer, true, Notification.Problem.Garbage);
                        this.HasDirtyBuildingsToCheck = this.HasDirtyBuildingsToCheck || this.DirtyBuildings[buildingId].CheckThis;
                    }
                }
                else if (this.DirtyBuildings.ContainsKey(buildingId))
                {
                    if (building.m_garbageBuffer >= Global.Settings.MinimumGarbageForDispatch / 10)
                    {
                        this.DirtyBuildings[buildingId].Update(districtManager, ref building, null, building.m_garbageBuffer, false, Notification.Problem.Garbage);
                    }
                    else
                    {
                        if (Log.LogToFile) Log.Debug(this, "CategorizeBuilding", "Not Dirty", buildingId);

                        this.DirtyBuildings.Remove(buildingId);
                    }
                }
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
                if (buildings[id].Info == null || (buildings[id].m_flags & Building.Flags.Created) == Building.Flags.None || (buildings[id].m_flags & (Building.Flags.Abandoned | Building.Flags.BurnedDown | Building.Flags.Deleted)) != Building.Flags.None)
                {
                    if (Global.Settings.DispatchHearses)
                    {
                        if (this.HearseBuildings.ContainsKey(id))
                        {
                            this.HearseBuildings.Remove(id);
                            Log.Debug(this, "CategorizeBuildings", "Not Cemetary Building", id);
                        }

                        if (this.DeadPeopleBuildings.ContainsKey(id))
                        {
                            this.DeadPeopleBuildings.Remove(id);
                            Log.Debug(this, "CategorizeBuildings", "Not Dead People Building", id);
                        }
                    }

                    if (Global.Settings.DispatchGarbageTrucks)
                    {
                        if (this.GarbageBuildings.ContainsKey(id))
                        {
                            this.GarbageBuildings.Remove(id);
                            Log.Debug(this, "CategorizeBuildings", "Not Landfill Building", id);
                        }

                        if (this.DirtyBuildings.ContainsKey(id))
                        {
                            this.DirtyBuildings.Remove(id);
                            Log.Debug(this, "CategorizeBuildings", "Not Dirty Building", id);
                        }
                    }
                }
                else
                {
                    this.CategorizeBuilding(districtManager, id, ref buildings[id]);
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

            return new FrameBoundaries(frame * 128, ((frame + 1) * 128) - 1);
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
    }
}
