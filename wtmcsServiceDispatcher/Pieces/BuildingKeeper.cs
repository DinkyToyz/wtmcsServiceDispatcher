﻿using System;
using System.Collections.Generic;
using ColossalFramework;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Keeps track of interesting buildings.
    /// </summary>
    internal class BuildingKeeper : IHandlerPart
    {
        /// <summary>
        /// The current/last update bucket.
        /// </summary>
        private uint bucket;

        /// <summary>
        /// The building object bucket manager.
        /// </summary>
        private Bucketeer bucketeer;

        /// <summary>
        /// The bucket factor.
        /// </summary>
        private uint bucketFactor = 192;

        /// <summary>
        /// The bucket mask.
        /// </summary>
        private uint bucketMask = 255;

        /// <summary>
        /// The desolate buildings.
        /// </summary>
        private Dictionary<ushort, Double> desolateBuildings;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingKeeper"/> class.
        /// </summary>
        public BuildingKeeper()
        {
            this.Initialize(true);
            Log.Debug(this, "Constructed");
        }

        /// <summary>
        /// Gets the dead people buildings.
        /// </summary>
        public Dictionary<ushort, TargetBuildingInfo> DeadPeopleBuildings
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the dirty buildings.
        /// </summary>
        public Dictionary<ushort, TargetBuildingInfo> DirtyBuildings
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the garbage buildings.
        /// </summary>
        public Dictionary<ushort, ServiceBuildingInfo> GarbageBuildings
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance has dead people buildings that should be checked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has dead people buildings to check; otherwise, <c>false</c>.
        /// </value>
        public bool HasDeadPeopleBuildingsToCheck
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance has dirty buildings that should be checked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has dirty buildings to check; otherwise, <c>false</c>.
        /// </value>
        public bool HasDirtyBuildingsToCheck
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the hearse buildings.
        /// </summary>
        public Dictionary<ushort, ServiceBuildingInfo> HearseBuildings
        {
            get;
            private set;
        }

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

                if (Global.Settings.AutoBulldozeBuildings)
                {
                    if (this.desolateBuildings != null)
                    {
                        BuildingHelper.DebugListLog(this.desolateBuildings, "DesolateBuildings");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(BuildingKeeper), "DebugListLogBuildings", ex);
            }
        }

        /// <summary>
        /// Logs a list of desolate building info for debug use.
        /// </summary>
        public void DebugListLogDesolateBuildings()
        {
            try
            {
                if (Global.Settings.AutoBulldozeBuildings && this.desolateBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.desolateBuildings, "DesolateBuildings");
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(BuildingKeeper), "DebugListLogDesolateBuildings", ex);
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
        /// Re-initialize the part.
        /// </summary>
        public void ReInitialize()
        {
            this.Initialize(false);
        }

        /// <summary>
        /// Updates data.
        /// </summary>
        /// <exception cref="System.Exception">Update bucket loop counter too high.</exception>
        public void Update()
        {
            // Get and categorize buildings.
            this.HasDeadPeopleBuildingsToCheck = false;
            this.HasDirtyBuildingsToCheck = false;

            // First update?
            if (this.bucketeer != null)
            {
                // Data is initialized. Just check buildings for this frame.
                uint endBucket = this.bucketeer.GetEnd();

                uint counter = 0;
                while (this.bucket != endBucket)
                {
                    if (counter > 256)
                    {
                        throw new Exception("Update bucket loop counter too high");
                    }
                    counter++;

                    this.bucket = this.bucketeer.GetNext(this.bucket + 1);
                    Bucketeer.Boundaries bounds = this.bucketeer.GetBoundaries(this.bucket);

                    this.CategorizeBuildings(bounds.FirstId, bounds.LastId);
                }
            }
            else
            {
                if (Global.BucketedUpdates)
                {
                    uint length = (uint)Singleton<BuildingManager>.instance.m_buildings.m_buffer.Length;
                    this.bucketFactor = length / (this.bucketMask + 1);
                    Log.Debug(this, "Update", "bucketFactor", length, this.bucketMask, this.bucketFactor);

                    this.bucketeer = new Bucketeer(this.bucketMask, this.bucketFactor);
                    this.bucket = this.bucketeer.GetEnd();
                }

                this.CategorizeBuildings();
            }

            if (Global.BuildingUpdateNeeded)
            {
                if (Global.Settings.DispatchHearses)
                {
                    if (this.HearseBuildings != null)
                    {
                        this.UpdateValues(this.HearseBuildings.Values);
                    }

                    if (this.DeadPeopleBuildings != null)
                    {
                        this.UpdateValues(this.DeadPeopleBuildings.Values);
                    }
                }

                if (Global.Settings.DispatchGarbageTrucks)
                {
                    if (this.GarbageBuildings != null)
                    {
                        this.UpdateValues(this.GarbageBuildings.Values);
                    }

                    if (this.DirtyBuildings != null)
                    {
                        this.UpdateValues(this.DirtyBuildings.Values);
                    }
                }

                Global.BuildingUpdateNeeded = false;
            }
        }

        /// <summary>
        /// Categorizes the building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        private void CategorizeBuilding(ushort buildingId, ref Building building)
        {
            // Checks for hearse dispatcher.
            if (Global.Settings.DispatchHearses)
            {
                // Check cemetaries and crematoriums.
                if (building.Info.m_buildingAI is CemeteryAI)
                {
                    if (!this.HearseBuildings.ContainsKey(buildingId))
                    {
                        ServiceBuildingInfo hearseBuilding = new ServiceBuildingInfo(buildingId, ref building, Dispatcher.DispatcherTypes.HearseDispatcher);
                        Log.Debug(this, "CategorizeBuilding", "Cemetary", buildingId, building.Info.name, hearseBuilding.BuildingName, hearseBuilding.Range, hearseBuilding.District);

                        this.HearseBuildings[buildingId] = hearseBuilding;
                    }
                    else
                    {
                        this.HearseBuildings[buildingId].Update(ref building);
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
                        TargetBuildingInfo deadPeopleBuilding = new TargetBuildingInfo(buildingId, ref building, Notification.Problem.Death, TargetBuildingInfo.ServiceDemand.NeedsService);
                        if (Log.LogToFile)
                        {
                            Log.Debug(this, "CategorizeBuilding", "Dead People", buildingId, building.Info.name, deadPeopleBuilding.BuildingName, building.m_deathProblemTimer, deadPeopleBuilding.ProblemValue, deadPeopleBuilding.HasProblem, deadPeopleBuilding.District);
                        }

                        this.DeadPeopleBuildings[buildingId] = deadPeopleBuilding;
                        this.HasDeadPeopleBuildingsToCheck = true;
                    }
                    else
                    {
                        this.DeadPeopleBuildings[buildingId].Update(ref building, Notification.Problem.Death, TargetBuildingInfo.ServiceDemand.NeedsService);
                        this.HasDeadPeopleBuildingsToCheck = this.HasDeadPeopleBuildingsToCheck || this.DeadPeopleBuildings[buildingId].CheckThis;
                    }
                }
                else if (this.DeadPeopleBuildings.ContainsKey(buildingId))
                {
                    if (this.DeadPeopleBuildings[buildingId].WantedService)
                    {
                        this.DeadPeopleBuildings[buildingId].Update(ref building, Notification.Problem.Death, TargetBuildingInfo.ServiceDemand.None);
                    }
                    else
                    {
                        if (Log.LogToFile)
                        {
                            Log.Debug(this, "CategorizeBuilding", "No Dead People", buildingId);
                        }

                        this.DeadPeopleBuildings.Remove(buildingId);
                    }
                }
            }

            if (Global.Settings.DispatchGarbageTrucks)
            {
                // Check landfills and incinerators.
                if (building.Info.m_buildingAI is LandfillSiteAI)
                {
                    if (!this.GarbageBuildings.ContainsKey(buildingId))
                    {
                        ServiceBuildingInfo garbageBuilding = new ServiceBuildingInfo(buildingId, ref building, Dispatcher.DispatcherTypes.GarbageTruckDispatcher);
                        Log.Debug(this, "CategorizeBuilding", "Landfill", buildingId, building.Info.name, garbageBuilding.BuildingName, garbageBuilding.Range, garbageBuilding.District);

                        this.GarbageBuildings[buildingId] = garbageBuilding;
                    }
                    else
                    {
                        this.GarbageBuildings[buildingId].Update(ref building);
                    }
                }
                else if (this.GarbageBuildings.ContainsKey(buildingId))
                {
                    Log.Debug(this, "CategorizeBuilding", "Not Landfill", buildingId);

                    this.GarbageBuildings.Remove(buildingId);
                }

                // Check garbage.
                TargetBuildingInfo.ServiceDemand demand;
                if (building.m_garbageBuffer >= Global.Settings.MinimumGarbageForDispatch)
                {
                    demand = TargetBuildingInfo.ServiceDemand.NeedsService;
                }
                else if (building.m_garbageBuffer >= Global.Settings.MinimumGarbageForPatrol)
                {
                    demand = TargetBuildingInfo.ServiceDemand.WantsService;
                }
                else
                {
                    demand = TargetBuildingInfo.ServiceDemand.None;
                }

                if (demand != TargetBuildingInfo.ServiceDemand.None && !(building.Info.m_buildingAI is LandfillSiteAI))
                {
                    if (!this.DirtyBuildings.ContainsKey(buildingId))
                    {
                        TargetBuildingInfo dirtyBuilding = new TargetBuildingInfo(buildingId, ref building, Notification.Problem.Garbage, demand);
                        if (Log.LogToFile)
                        {
                            Log.Debug(this, "CategorizeBuilding", "Dirty", demand, buildingId, building.Info.name, dirtyBuilding.BuildingName, building.m_garbageBuffer, dirtyBuilding.ProblemValue, dirtyBuilding.HasProblem, dirtyBuilding.District);
                        }

                        this.DirtyBuildings[buildingId] = dirtyBuilding;
                        this.HasDirtyBuildingsToCheck = true;
                    }
                    else
                    {
                        this.DirtyBuildings[buildingId].Update(ref building, Notification.Problem.Garbage, demand);
                        this.HasDirtyBuildingsToCheck = this.HasDirtyBuildingsToCheck || this.DirtyBuildings[buildingId].CheckThis;
                    }
                }
                else if (this.DirtyBuildings.ContainsKey(buildingId))
                {
                    if ((building.m_garbageBuffer > 10 && (building.m_garbageBuffer >= Global.Settings.MinimumGarbageForDispatch / 10 || building.m_garbageBuffer >= Global.Settings.MinimumGarbageForPatrol / 2)) || this.DirtyBuildings[buildingId].WantedService)
                    {
                        this.DirtyBuildings[buildingId].Update(ref building, Notification.Problem.Garbage, TargetBuildingInfo.ServiceDemand.None);
                    }
                    else
                    {
                        if (Log.LogToFile)
                        {
                            Log.Debug(this, "CategorizeBuilding", "Not Dirty", buildingId);
                        }

                        this.DirtyBuildings.Remove(buildingId);
                    }
                }
            }
        }

        /// <summary>
        /// Categorizes the buildings.
        /// </summary>
        private void CategorizeBuildings()
        {
            this.CategorizeBuildings(0, Singleton<BuildingManager>.instance.m_buildings.m_buffer.Length);
        }

        /// <summary>
        /// Categorizes the buildings.
        /// </summary>
        /// <param name="firstBuildingId">The first building identifier.</param>
        /// <param name="lastBuildingId">The last building identifier.</param>
        private void CategorizeBuildings(ushort firstBuildingId, int lastBuildingId)
        {
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            bool desolateBuilding;
            bool usableBuilding;

            for (ushort id = firstBuildingId; id < lastBuildingId; id++)
            {
                if (buildings[id].Info == null)
                {
                    usableBuilding = false;
                    desolateBuilding = false;
                }
                else
                {
                    desolateBuilding = (buildings[id].m_flags & (Building.Flags.Abandoned | Building.Flags.BurnedDown)) != Building.Flags.None;
                    usableBuilding = (buildings[id].m_flags & Building.Flags.Created) == Building.Flags.Created && (buildings[id].m_flags & (Building.Flags.Abandoned | Building.Flags.BurnedDown | Building.Flags.Deleted | Building.Flags.Hidden)) == Building.Flags.None;
                }

                // Handle buildings needing services.
                if (usableBuilding)
                {
                    this.CategorizeBuilding(id, ref buildings[id]);
                }
                else
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

                // Handle abandonded and burnet down buildings.
                if (this.desolateBuildings != null)
                {
                    double stamp;
                    bool known = this.desolateBuildings.TryGetValue(id, out stamp);

                    if (desolateBuilding)
                    {
                        double delta;

                        if (known)
                        {
                            delta = Global.SimulationTime - stamp;

                            if (Log.LogALot && Log.LogToFile)
                            {
                                Log.DevDebug(this, "CategorizeBuildings", "Desolate Building", id, delta);
                            }
                        }
                        else
                        {
                            delta = 0.0;
                            this.desolateBuildings[id] = Global.SimulationTime;
                            Log.Debug(this, "CategorizeBuildings", "Desolate Building", id);
                        }

                        if (delta >= Global.Settings.AutoBulldozeBuildingsDelaySeconds)
                        {
                            Log.Debug(this, "CategorizeBuildings", "Buldoze Building", id);
                            BulldozeHelper.BulldozeBuilding(id);
                        }
                    }
                    else if (known)
                    {
                        this.desolateBuildings.Remove(id);
                        Log.Debug(this, "CategorizeBuildings", "Not Desolate Building", id);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the data lists.
        /// </summary>
        /// <param name="constructing">If set to <c>true</c> object is being constructed.</param>
        private void Initialize(bool constructing)
        {
            Log.InfoList info = new Log.InfoList();
            info.Add("constructing", constructing);

            info.Add("DispatchHearses", Global.Settings.DispatchHearses);
            if (Global.Settings.DispatchHearses)
            {
                if (constructing || this.DeadPeopleBuildings == null)
                {
                    info.Add("DeadPeopleBuildings", "new");
                    this.HasDeadPeopleBuildingsToCheck = false;
                    this.DeadPeopleBuildings = new Dictionary<ushort, TargetBuildingInfo>();
                }

                if (constructing || this.HearseBuildings == null)
                {
                    info.Add("HearseBuildings", "new");
                    this.HasDeadPeopleBuildingsToCheck = false;
                    this.HearseBuildings = new Dictionary<ushort, ServiceBuildingInfo>();
                }
            }
            else
            {
                this.DeadPeopleBuildings = null;
                this.HearseBuildings = null;
            }

            info.Add("DispatchGarbageTrucks", Global.Settings.DispatchGarbageTrucks);
            if (Global.Settings.DispatchGarbageTrucks)
            {
                if (constructing || this.DirtyBuildings == null)
                {
                    info.Add("DirtyBuildings", "new");
                    this.HasDirtyBuildingsToCheck = false;
                    this.DirtyBuildings = new Dictionary<ushort, TargetBuildingInfo>();
                }

                if (constructing || this.GarbageBuildings == null)
                {
                    info.Add("GarbageBuildings", "new");
                    this.HasDirtyBuildingsToCheck = false;
                    this.GarbageBuildings = new Dictionary<ushort, ServiceBuildingInfo>();
                }
            }
            else
            {
                this.DirtyBuildings = null;
                this.GarbageBuildings = null;
            }

            info.Add("AutoBulldozeBuildings", Global.Settings.AutoBulldozeBuildings);
            info.Add("CanBulldoze", BulldozeHelper.CanBulldoze);
            if (Global.Settings.AutoBulldozeBuildings && BulldozeHelper.CanBulldoze)
            {
                if (this.desolateBuildings == null)
                {
                    info.Add("Desolate", "new");
                    this.desolateBuildings = new Dictionary<ushort, double>();
                }
            }
            else
            {
                this.desolateBuildings = null;
            }

            Log.Debug(this, "Initialize", info);
        }

        /// <summary>
        /// Updates the building values.
        /// </summary>
        /// <typeparam name="T">The building class type.</typeparam>
        /// <param name="infoBuildings">The buildings.</param>
        private void UpdateValues<T>(IEnumerable<T> infoBuildings) where T : IBuildingInfo
        {
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            DistrictManager districtManager = null;
            if (Global.Settings.DispatchAnyByDistrict)
            {
                districtManager = Singleton<DistrictManager>.instance;
            }

            foreach (T building in infoBuildings)
            {
                building.ReInitialize();
                building.UpdateValues(ref buildings[building.BuildingId], true);
            }
        }
    }
}