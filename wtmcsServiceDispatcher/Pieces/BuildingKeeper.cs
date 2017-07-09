using ColossalFramework;
using System;
using System.Collections.Generic;

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
        /// The cemeteries in need of emptying change.
        /// </summary>
        private List<ServiceBuildingInfo> cemeteriesInNeedOfEmptyingChange = null;

        /// <summary>
        /// The death-care-buildings-can-empty-other counter.
        /// </summary>
        private uint deathCareBuildingsCanEmptyOther = 0u;

        /// <summary>
        /// The death-care-buildings-emptying counter.
        /// </summary>
        private uint deathCareBuildingsEmptying = 0u;

        /// <summary>
        /// The garbage-buildings-can-empty-other counter.
        /// </summary>
        private uint garbageBuildingsCanEmptyOther = 0u;

        /// <summary>
        /// The garbage-buildings-emptying counter.
        /// </summary>
        private uint garbageBuildingsEmptying = 0u;

        /// <summary>
        /// The landfills in need of emptying change.
        /// </summary>
        private List<ServiceBuildingInfo> landfillsInNeedOfEmptyingChange = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingKeeper"/> class.
        /// </summary>
        public BuildingKeeper()
        {
            this.Initialize(true);
            Log.Debug(this, "Constructed");
        }

        /// <summary>
        /// Gets the building list pairs.
        /// </summary>
        /// <value>
        /// The building list pairs.
        /// </value>
        public IEnumerable<BuildingListPair> BuildingListPairs
        {
            get
            {
                if (this.GarbageBuildings != null && this.DirtyBuildings != null)
                {
                    yield return new BuildingListPair(this.GarbageBuildings, this.DirtyBuildings);
                }

                if (this.DeathCareBuildings != null && this.DeadPeopleBuildings != null)
                {
                    yield return new BuildingListPair(this.DeathCareBuildings, this.DeadPeopleBuildings);
                }

                if (this.HealthCareBuildings != null && this.SickPeopleBuildings != null)
                {
                    yield return new BuildingListPair(this.HealthCareBuildings, this.SickPeopleBuildings);
                }
            }
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
        /// Gets the hearse buildings.
        /// </summary>
        public Dictionary<ushort, ServiceBuildingInfo> DeathCareBuildings
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the desolate buildings.
        /// </summary>
        public Dictionary<ushort, Double> DesolateBuildings
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
        /// Gets a value indicating whether this instance has sick people buildings that should be checked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has sick people buildings to check; otherwise, <c>false</c>.
        /// </value>
        public bool HasSickPeopleBuildingsToCheck
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the ambulance buildings.
        /// </summary>
        public Dictionary<ushort, ServiceBuildingInfo> HealthCareBuildings
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service building lists.
        /// </summary>
        /// <value>
        /// The service building lists.
        /// </value>
        public IEnumerable<Dictionary<ushort, ServiceBuildingInfo>> ServiceBuildingLists
        {
            get
            {
                if (this.GarbageBuildings != null)
                {
                    yield return this.GarbageBuildings;
                }

                if (this.DeathCareBuildings != null)
                {
                    yield return this.DeathCareBuildings;
                }

                if (this.HealthCareBuildings != null)
                {
                    yield return this.HealthCareBuildings;
                }
            }
        }

        /// <summary>
        /// Gets the sick people buildings.
        /// </summary>
        public Dictionary<ushort, TargetBuildingInfo> SickPeopleBuildings
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the target building lists.
        /// </summary>
        /// <value>
        /// The target building lists.
        /// </value>
        public IEnumerable<Dictionary<ushort, TargetBuildingInfo>> TargetBuildingLists
        {
            get
            {
                if (this.DirtyBuildings != null)
                {
                    yield return this.DirtyBuildings;
                }

                if (this.DeadPeopleBuildings != null)
                {
                    yield return this.DeadPeopleBuildings;
                }

                if (this.SickPeopleBuildings != null)
                {
                    yield return this.SickPeopleBuildings;
                }
            }
        }

        /// <summary>
        /// Logs a list of service building info for debug use.
        /// </summary>
        public void DebugListLogBuildings()
        {
            try
            {
                if (this.DeadPeopleBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.DeadPeopleBuildings.Values);
                }

                if (this.DeathCareBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.DeathCareBuildings.Values);
                }

                if (this.DirtyBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.DirtyBuildings.Values);
                }

                if (this.GarbageBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.GarbageBuildings.Values);
                }

                if (this.SickPeopleBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.SickPeopleBuildings.Values);
                }

                if (this.HealthCareBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.HealthCareBuildings.Values);
                }

                if (this.DesolateBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.DesolateBuildings, "DesolateBuildings");
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "DebugListLogBuildings", ex);
            }
        }

        /// <summary>
        /// Logs a list of desolate building info for debug use.
        /// </summary>
        public void DebugListLogDesolateBuildings()
        {
            try
            {
                if (Global.Settings.WreckingCrews.DispatchVehicles && this.DesolateBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.DesolateBuildings, "DesolateBuildings");
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "DebugListLogDesolateBuildings", ex);
            }
        }

        /// <summary>
        /// Logs a list of service building info for debug use.
        /// </summary>
        public void DebugListLogServiceBuildings()
        {
            try
            {
                if (this.DeathCareBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.DeathCareBuildings.Values);
                }

                if (this.GarbageBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.GarbageBuildings.Values);
                }

                if (this.HealthCareBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.HealthCareBuildings.Values);
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "DebugListLogServiceBuildings", ex);
            }
        }

        /// <summary>
        /// Logs a list of service building info for debug use.
        /// </summary>
        public void DebugListLogTargetBuildings()
        {
            try
            {
                if (this.DeadPeopleBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.DeadPeopleBuildings.Values);
                }

                if (this.DirtyBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.DirtyBuildings.Values);
                }

                if (this.SickPeopleBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.SickPeopleBuildings.Values);
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "DebugListLogTargetBuildings", ex);
            }
        }

        /// <summary>
        /// Gets the buidling categories for a building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <returns>The categories in which the buidling has been categorized.</returns>
        public IEnumerable<string> GetCategories(ushort buildingId)
        {
            if (this.GarbageBuildings != null && this.GarbageBuildings.ContainsKey(buildingId))
            {
                yield return "Garbage";
            }

            if (this.DeathCareBuildings != null && this.DeathCareBuildings.ContainsKey(buildingId))
            {
                yield return "DeathCare";
            }

            if (this.HealthCareBuildings != null && this.HealthCareBuildings.ContainsKey(buildingId))
            {
                yield return "HealthCare";
            }

            if (this.DirtyBuildings != null && this.DirtyBuildings.ContainsKey(buildingId))
            {
                yield return "Dirty";
            }

            if (this.DeadPeopleBuildings != null && this.DeadPeopleBuildings.ContainsKey(buildingId))
            {
                yield return "DeadPeople";
            }

            if (this.SickPeopleBuildings != null && this.SickPeopleBuildings.ContainsKey(buildingId))
            {
                yield return "SickPeople";
            }

            if (this.DesolateBuildings != null && this.DesolateBuildings.ContainsKey(buildingId))
            {
                yield return "Desolate";
            }
        }

        /// <summary>
        /// Gets the service building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <returns>A list of service buildings with the id.</returns>
        public ServiceBuildingInfo GetServiceBuilding(ushort buildingId)
        {
            ServiceBuildingInfo building;

            if (this.GarbageBuildings != null && this.GarbageBuildings.TryGetValue(buildingId, out building))
            {
                return building;
            }

            if (this.DeathCareBuildings != null && this.DeathCareBuildings.TryGetValue(buildingId, out building))
            {
                return building;
            }

            if (this.HealthCareBuildings != null && this.HealthCareBuildings.TryGetValue(buildingId, out building))
            {
                return building;
            }

            return null;
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
            this.HasSickPeopleBuildingsToCheck = false;

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
                    //Log.DevDebug(this, "Update", "bucketFactor", length, this.bucketMask, this.bucketFactor);

                    this.bucketeer = new Bucketeer(this.bucketMask, this.bucketFactor);
                    this.bucket = this.bucketeer.GetEnd();
                }

                this.CategorizeBuildings();
            }

            if (Global.BuildingUpdateNeeded)
            {
                if (this.DeathCareBuildings != null)
                {
                    this.UpdateValues(this.DeathCareBuildings.Values);
                }

                if (this.DeadPeopleBuildings != null)
                {
                    this.UpdateValues(this.DeadPeopleBuildings.Values);
                }

                if (this.GarbageBuildings != null)
                {
                    this.UpdateValues(this.GarbageBuildings.Values);
                }

                if (this.DirtyBuildings != null)
                {
                    this.UpdateValues(this.DirtyBuildings.Values);
                }

                if (this.HealthCareBuildings != null)
                {
                    this.UpdateValues(this.HealthCareBuildings.Values);
                }

                if (this.SickPeopleBuildings != null)
                {
                    this.UpdateValues(this.SickPeopleBuildings.Values);
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
            if (Global.Settings.DeathCare.DispatchVehicles || Global.Settings.DeathCare.AutoEmpty)
            {
                // Check cemetaries and crematoriums.
                if (building.Info.m_buildingAI is CemeteryAI)
                {
                    ServiceBuildingInfo hearseBuilding;

                    if (!this.DeathCareBuildings.TryGetValue(buildingId, out hearseBuilding))
                    {
                        hearseBuilding = new ServiceBuildingInfo(buildingId, ref building, Dispatcher.DispatcherTypes.HearseDispatcher);
                        Log.Debug(this, "CategorizeBuilding", "Cemetary", buildingId, building.Info.name, hearseBuilding.BuildingName, hearseBuilding.Range, hearseBuilding.District);

                        this.DeathCareBuildings[buildingId] = hearseBuilding;
                    }
                    else
                    {
                        hearseBuilding.Update(ref building);
                    }

                    if (Global.Settings.DeathCare.AutoEmpty && (hearseBuilding.NeedsEmptying || hearseBuilding.EmptyingIsDone))
                    {
                        this.cemeteriesInNeedOfEmptyingChange.Add(hearseBuilding);
                    }
                }
                else if (this.DeathCareBuildings.ContainsKey(buildingId))
                {
                    Log.Debug(this, "CategorizeBuilding", "Not Cemetary", buildingId);

                    this.DeathCareBuildings.Remove(buildingId);
                }

                // Check dead people.
                if (Global.Settings.DeathCare.DispatchVehicles)
                {
                    if (building.m_deathProblemTimer > 0)
                    {
                        if (!this.DeadPeopleBuildings.ContainsKey(buildingId))
                        {
                            TargetBuildingInfo deadPeopleBuilding = new TargetBuildingInfo(buildingId, ref building, Dispatcher.DispatcherTypes.HearseDispatcher, TargetBuildingInfo.ServiceDemand.NeedsService);
                            if (Log.LogToFile)
                            {
                                Log.Debug(this, "CategorizeBuilding", "Dead People", buildingId, building.Info.name, deadPeopleBuilding.BuildingName, building.m_deathProblemTimer, deadPeopleBuilding.ProblemValue, deadPeopleBuilding.HasProblem, deadPeopleBuilding.District);
                            }

                            this.DeadPeopleBuildings[buildingId] = deadPeopleBuilding;
                            this.HasDeadPeopleBuildingsToCheck = true;
                        }
                        else
                        {
                            this.DeadPeopleBuildings[buildingId].Update(ref building, TargetBuildingInfo.ServiceDemand.NeedsService);
                            this.HasDeadPeopleBuildingsToCheck = this.HasDeadPeopleBuildingsToCheck || this.DeadPeopleBuildings[buildingId].CheckThis;
                        }
                    }
                    else if (this.DeadPeopleBuildings.ContainsKey(buildingId))
                    {
                        if (this.DeadPeopleBuildings[buildingId].WantedService)
                        {
                            this.DeadPeopleBuildings[buildingId].Update(ref building, TargetBuildingInfo.ServiceDemand.None);
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
            }

            // Checks for garbage truck dispatcher.
            if (Global.Settings.Garbage.DispatchVehicles || Global.Settings.Garbage.AutoEmpty)
            {
                // Check landfills and incinerators.
                if (building.Info.m_buildingAI is LandfillSiteAI)
                {
                    ServiceBuildingInfo garbageBuilding;

                    if (!this.GarbageBuildings.TryGetValue(buildingId, out garbageBuilding))
                    {
                        garbageBuilding = new ServiceBuildingInfo(buildingId, ref building, Dispatcher.DispatcherTypes.GarbageTruckDispatcher);
                        Log.Debug(this, "CategorizeBuilding", "Landfill", buildingId, building.Info.name, garbageBuilding.BuildingName, garbageBuilding.Range, garbageBuilding.District);

                        this.GarbageBuildings[buildingId] = garbageBuilding;
                    }
                    else
                    {
                        garbageBuilding.Update(ref building);
                    }

                    if (Global.Settings.Garbage.AutoEmpty && (garbageBuilding.NeedsEmptying || garbageBuilding.EmptyingIsDone))
                    {
                        this.landfillsInNeedOfEmptyingChange.Add(garbageBuilding);
                    }
                }
                else if (this.GarbageBuildings.ContainsKey(buildingId))
                {
                    Log.Debug(this, "CategorizeBuilding", "Not Landfill", buildingId);

                    this.GarbageBuildings.Remove(buildingId);
                }

                // Check garbage.
                if (Global.Settings.Garbage.DispatchVehicles)
                {
                    TargetBuildingInfo.ServiceDemand demand;
                    if (building.m_garbageBuffer >= Global.Settings.Garbage.MinimumAmountForDispatch)
                    {
                        demand = TargetBuildingInfo.ServiceDemand.NeedsService;
                    }
                    else if (building.m_garbageBuffer >= Global.Settings.Garbage.MinimumAmountForPatrol)
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
                            TargetBuildingInfo dirtyBuilding = new TargetBuildingInfo(buildingId, ref building, Dispatcher.DispatcherTypes.GarbageTruckDispatcher, demand);
                            if (Log.LogToFile)
                            {
                                Log.Debug(this, "CategorizeBuilding", "Dirty", demand, buildingId, building.Info.name, dirtyBuilding.BuildingName, building.m_garbageBuffer, dirtyBuilding.ProblemValue, dirtyBuilding.HasProblem, dirtyBuilding.District);
                            }

                            this.DirtyBuildings[buildingId] = dirtyBuilding;
                            this.HasDirtyBuildingsToCheck = true;
                        }
                        else
                        {
                            this.DirtyBuildings[buildingId].Update(ref building, demand);
                            this.HasDirtyBuildingsToCheck = this.HasDirtyBuildingsToCheck || this.DirtyBuildings[buildingId].CheckThis;
                        }
                    }
                    else if (this.DirtyBuildings.ContainsKey(buildingId))
                    {
                        if ((building.m_garbageBuffer > 10 && (building.m_garbageBuffer >= Global.Settings.Garbage.MinimumAmountForDispatch / 10 || building.m_garbageBuffer >= Global.Settings.Garbage.MinimumAmountForPatrol / 2)) || this.DirtyBuildings[buildingId].WantedService)
                        {
                            this.DirtyBuildings[buildingId].Update(ref building, TargetBuildingInfo.ServiceDemand.None);
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

            // Checks for ambulance dispatcher.
            if (Global.Settings.HealthCare.DispatchVehicles)
            {
                // Check hospitals and clinics.
                if (building.Info.m_buildingAI is HospitalAI)
                {
                    if (!this.HealthCareBuildings.ContainsKey(buildingId))
                    {
                        ServiceBuildingInfo ambulanceBuilding = new ServiceBuildingInfo(buildingId, ref building, Dispatcher.DispatcherTypes.AmbulanceDispatcher);
                        Log.Debug(this, "CategorizeBuilding", "Medical", buildingId, building.Info.name, ambulanceBuilding.BuildingName, ambulanceBuilding.Range, ambulanceBuilding.District);

                        this.HealthCareBuildings[buildingId] = ambulanceBuilding;
                    }
                    else
                    {
                        this.HealthCareBuildings[buildingId].Update(ref building);
                    }
                }
                else if (this.HealthCareBuildings.ContainsKey(buildingId))
                {
                    Log.Debug(this, "CategorizeBuilding", "Not Medical", buildingId);

                    this.HealthCareBuildings.Remove(buildingId);
                }

                // Check sick people.
                if (building.m_healthProblemTimer > 0)
                {
                    if (!this.SickPeopleBuildings.ContainsKey(buildingId))
                    {
                        TargetBuildingInfo sickPeopleBuilding = new TargetBuildingInfo(buildingId, ref building, Dispatcher.DispatcherTypes.AmbulanceDispatcher, TargetBuildingInfo.ServiceDemand.NeedsService);
                        if (Log.LogToFile)
                        {
                            Log.Debug(this, "CategorizeBuilding", "Sick People", buildingId, building.Info.name, sickPeopleBuilding.BuildingName, building.m_healthProblemTimer, sickPeopleBuilding.ProblemValue, sickPeopleBuilding.HasProblem, sickPeopleBuilding.District);
                        }

                        this.SickPeopleBuildings[buildingId] = sickPeopleBuilding;
                        this.HasSickPeopleBuildingsToCheck = true;
                    }
                    else
                    {
                        this.SickPeopleBuildings[buildingId].Update(ref building, TargetBuildingInfo.ServiceDemand.NeedsService);
                        this.HasSickPeopleBuildingsToCheck = this.HasSickPeopleBuildingsToCheck || this.SickPeopleBuildings[buildingId].CheckThis;
                    }
                }
                else if (this.SickPeopleBuildings.ContainsKey(buildingId))
                {
                    if (this.SickPeopleBuildings[buildingId].WantedService)
                    {
                        this.SickPeopleBuildings[buildingId].Update(ref building, TargetBuildingInfo.ServiceDemand.None);
                    }
                    else
                    {
                        if (Log.LogToFile)
                        {
                            Log.Debug(this, "CategorizeBuilding", "No Sick People", buildingId);
                        }

                        this.SickPeopleBuildings.Remove(buildingId);
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

            if (this.cemeteriesInNeedOfEmptyingChange != null)
            {
                this.cemeteriesInNeedOfEmptyingChange.Clear();
            }

            if (this.landfillsInNeedOfEmptyingChange != null)
            {
                this.landfillsInNeedOfEmptyingChange.Clear();
            }

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
                    if (this.DeathCareBuildings != null && this.DeathCareBuildings.ContainsKey(id))
                    {
                        this.DeathCareBuildings.Remove(id);
                        Log.Debug(this, "CategorizeBuildings", "Not Cemetary Building", id);
                    }

                    if (this.DeadPeopleBuildings != null && this.DeadPeopleBuildings.ContainsKey(id))
                    {
                        this.DeadPeopleBuildings.Remove(id);
                        Log.Debug(this, "CategorizeBuildings", "Not Dead People Building", id);
                    }

                    if (this.GarbageBuildings != null && this.GarbageBuildings.ContainsKey(id))
                    {
                        this.GarbageBuildings.Remove(id);
                        Log.Debug(this, "CategorizeBuildings", "Not Landfill Building", id);
                    }

                    if (this.DirtyBuildings != null && this.DirtyBuildings.ContainsKey(id))
                    {
                        this.DirtyBuildings.Remove(id);
                        Log.Debug(this, "CategorizeBuildings", "Not Dirty Building", id);
                    }

                    if (this.HealthCareBuildings != null && this.HealthCareBuildings.ContainsKey(id))
                    {
                        this.HealthCareBuildings.Remove(id);
                        Log.Debug(this, "CategorizeBuildings", "Not Medical Building", id);
                    }

                    if (this.SickPeopleBuildings != null && this.SickPeopleBuildings.ContainsKey(id))
                    {
                        this.SickPeopleBuildings.Remove(id);
                        Log.Debug(this, "CategorizeBuildings", "Not Sick People Building", id);
                    }
                }

                // Handle abandonded and burnet down buildings.
                if (this.DesolateBuildings != null)
                {
                    double stamp;
                    bool known = this.DesolateBuildings.TryGetValue(id, out stamp);

                    if (desolateBuilding)
                    {
                        double delta;

                        if (known)
                        {
                            delta = Global.SimulationTime - stamp;
                        }
                        else
                        {
                            delta = 0.0;
                            this.DesolateBuildings[id] = Global.SimulationTime;
                            Log.Debug(this, "CategorizeBuildings", "Desolate Building", id);
                        }

                        if (delta >= Global.Settings.WreckingCrews.DelaySeconds)
                        {
                            Log.Debug(this, "CategorizeBuildings", "Bulldoze Building", id);
                            BulldozeHelper.BulldozeBuilding(id);
                        }
                    }
                    else if (known)
                    {
                        this.DesolateBuildings.Remove(id);
                        Log.Debug(this, "CategorizeBuildings", "Not Desolate Building", id);
                    }
                }
            }

            if (Global.Settings.DeathCare.AutoEmpty && this.cemeteriesInNeedOfEmptyingChange.Count > 0)
            {
                this.CountForEmptying(this.DeathCareBuildings, out this.deathCareBuildingsCanEmptyOther, out this.deathCareBuildingsEmptying);
                this.ChangeAutoEmptying(this.cemeteriesInNeedOfEmptyingChange, ref this.deathCareBuildingsCanEmptyOther, ref this.deathCareBuildingsEmptying);
            }

            if (Global.Settings.Garbage.AutoEmpty && this.landfillsInNeedOfEmptyingChange.Count > 0)
            {
                this.CountForEmptying(this.GarbageBuildings, out this.garbageBuildingsCanEmptyOther, out this.garbageBuildingsEmptying);
                this.ChangeAutoEmptying(this.landfillsInNeedOfEmptyingChange, ref this.garbageBuildingsCanEmptyOther, ref this.garbageBuildingsEmptying);
            }
        }

        /// <summary>
        /// Changes the automatic emptying.
        /// </summary>
        /// <param name="serviceBuildings">The service buildings.</param>
        /// <param name="canEmptyOther">The can-empty-other counter.</param>
        /// <param name="isEmptying">The is-emptying counter.</param>
        private void ChangeAutoEmptying(List<ServiceBuildingInfo> serviceBuildings, ref uint canEmptyOther, ref uint isEmptying)
        {
            foreach (ServiceBuildingInfo building in serviceBuildings)
            {
                if (building.EmptyingIsDone)
                {
                    building.AutoEmptyStop();
                    isEmptying--;
                }
                else if (building.NeedsEmptying && canEmptyOther > isEmptying)
                {
                    building.AutoEmptyStart();
                    isEmptying++;
                }
            }
        }

        /// <summary>
        /// Counts for emptying.
        /// </summary>
        /// <param name="serviceBuildings">The service buildings.</param>
        /// <param name="canEmptyOther">The can-empty-other counter.</param>
        /// <param name="isEmptying">The is-emptying counter.</param>
        private void CountForEmptying(Dictionary<ushort, ServiceBuildingInfo> serviceBuildings, out uint canEmptyOther, out uint isEmptying)
        {
            isEmptying = 0;
            canEmptyOther = 0;

            foreach (ServiceBuildingInfo building in serviceBuildings.Values)
            {
                if (building.IsAutoEmptying)
                {
                    isEmptying++;
                }

                if (building.CanEmptyOther)
                {
                    canEmptyOther++;
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

            info.Add("DispatchHearses", Global.Settings.DeathCare.DispatchVehicles);
            info.Add("AutoEmptyCemeteries", Global.Settings.DeathCare.AutoEmpty);

            if (!Global.Settings.DeathCare.DispatchVehicles)
            {
                this.HasDeadPeopleBuildingsToCheck = false;
                this.DeadPeopleBuildings = null;
            }
            else if (constructing || this.DeadPeopleBuildings == null)
            {
                info.Add("DeadPeopleBuildings", "new");
                this.HasDeadPeopleBuildingsToCheck = false;
                this.DeadPeopleBuildings = new Dictionary<ushort, TargetBuildingInfo>();
            }

            if (!Global.Settings.DeathCare.AutoEmpty)
            {
                this.cemeteriesInNeedOfEmptyingChange = null;
            }
            else if (constructing || this.cemeteriesInNeedOfEmptyingChange == null)
            {
                info.Add("CemeteriesInNeedOfEmptying", "new");
                this.cemeteriesInNeedOfEmptyingChange = new List<ServiceBuildingInfo>();
            }

            if (!Global.Settings.DeathCare.DispatchVehicles && !Global.Settings.DeathCare.AutoEmpty)
            {
                this.DeathCareBuildings = null;
            }
            else if (constructing || this.DeathCareBuildings == null)
            {
                info.Add("DeathCareBuildings", "new");
                this.DeathCareBuildings = new Dictionary<ushort, ServiceBuildingInfo>();
            }

            info.Add("DispatchGarbageTrucks", Global.Settings.Garbage.DispatchVehicles);
            info.Add("AutoEmptyLandfills", Global.Settings.Garbage.AutoEmpty);

            if (!Global.Settings.Garbage.DispatchVehicles)
            {
                this.HasDirtyBuildingsToCheck = false;
                this.DirtyBuildings = null;
            }
            else if (constructing || this.DirtyBuildings == null)
            {
                info.Add("DirtyBuildings", "new");
                this.HasDirtyBuildingsToCheck = false;
                this.DirtyBuildings = new Dictionary<ushort, TargetBuildingInfo>();
            }

            if (!Global.Settings.Garbage.AutoEmpty)
            {
                this.landfillsInNeedOfEmptyingChange = null;
            }
            else
            {
                info.Add("LandfillsInNeedOfEmptying", "new");
                this.landfillsInNeedOfEmptyingChange = new List<ServiceBuildingInfo>();
            }

            if (!Global.Settings.Garbage.DispatchVehicles && !Global.Settings.Garbage.AutoEmpty)
            {
                this.GarbageBuildings = null;
            }
            else if (constructing || this.GarbageBuildings == null)
            {
                info.Add("GarbageBuildings", "new");
                this.HasDirtyBuildingsToCheck = false;
                this.GarbageBuildings = new Dictionary<ushort, ServiceBuildingInfo>();
            }

            info.Add("DispatchAmbulances", Global.Settings.HealthCare.DispatchVehicles);

            if (Global.Settings.HealthCare.DispatchVehicles)
            {
                if (constructing || this.SickPeopleBuildings == null)
                {
                    info.Add("SickPeopleBuildings", "new");
                    this.HasSickPeopleBuildingsToCheck = false;
                    this.SickPeopleBuildings = new Dictionary<ushort, TargetBuildingInfo>();
                }

                if (constructing || this.HealthCareBuildings == null)
                {
                    info.Add("AmbulanceBuildings", "new");
                    this.HasSickPeopleBuildingsToCheck = false;
                    this.HealthCareBuildings = new Dictionary<ushort, ServiceBuildingInfo>();
                }
            }
            else
            {
                this.SickPeopleBuildings = null;
                this.HealthCareBuildings = null;
            }

            info.Add("AutoBulldozeBuildings", Global.Settings.WreckingCrews.DispatchVehicles);
            info.Add("CanBulldoze", BulldozeHelper.CanBulldoze);

            if (Global.Settings.WreckingCrews.DispatchVehicles && BulldozeHelper.CanBulldoze)
            {
                if (this.DesolateBuildings == null)
                {
                    info.Add("Desolate", "new");
                    this.DesolateBuildings = new Dictionary<ushort, double>();
                }
            }
            else
            {
                this.DesolateBuildings = null;
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

        /// <summary>
        /// A pair of service and target building lists.
        /// </summary>
        public struct BuildingListPair
        {
            /// <summary>
            /// The service buildings.
            /// </summary>
            public readonly Dictionary<ushort, ServiceBuildingInfo> ServiceBuildings;

            /// <summary>
            /// The target buildings.
            /// </summary>
            public readonly Dictionary<ushort, TargetBuildingInfo> TargetBuildings;

            /// <summary>
            /// Initializes a new instance of the <see cref="BuildingListPair"/> struct.
            /// </summary>
            /// <param name="serviceBuildings">The service buildings.</param>
            /// <param name="targetBuildings">The target buildings.</param>
            public BuildingListPair(Dictionary<ushort, ServiceBuildingInfo> serviceBuildings, Dictionary<ushort, TargetBuildingInfo> targetBuildings)
            {
                this.ServiceBuildings = serviceBuildings;
                this.TargetBuildings = targetBuildings;
            }
        }
    }
}