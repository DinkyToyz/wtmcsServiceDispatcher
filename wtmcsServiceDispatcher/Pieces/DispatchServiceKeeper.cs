using ColossalFramework;
using System;
using System.Collections.Generic;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Keeps track of dispatch services.
    /// </summary>
    internal class DispatchServiceKeeper : IHandlerPart
    {
        /// <summary>
        /// The services.
        /// </summary>
        private DispatchService[] services = null;

        /// <summary>
        /// Gets the dispatching services.
        /// </summary>
        /// <value>
        /// The dispatching services.
        /// </value>
        public IEnumerable<DispatchService> DispatchingServices => this.services.Where(s => s != null && s.IsDispatching);

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <value>
        /// The services.
        /// </value>
        public IEnumerable<DispatchService> EnabledServices => this.services.Where(s => s != null && s.Enabled);

        /// <summary>
        /// Gets the <see cref="DispatchService"/> with the specified dispatcher type.
        /// </summary>
        /// <value>
        /// The <see cref="DispatchService"/>.
        /// </value>
        /// <param name="DispatcherType">Type of the dispatcher.</param>
        /// <returns>The <see cref="DispatchService"/>.</returns>
        public DispatchService this[Dispatcher.DispatcherTypes DispatcherType] => this.services[(int)DispatcherType];

        /// <summary>
        /// Categorizes the building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        public void CategorizeBuilding(ushort buildingId, ref Building building)
        {
            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null && (this.services[i].DispatchVehicles || this.services[i].AutoEmpty))
                {
                    // Check if service building.
                    if (this.services[i].DispatchFromBuilding(buildingId, ref building))
                    {
                        ServiceBuildingInfo serviceBuilding;

                        if (!this.services[i].ServiceBuildings.TryGetValue(buildingId, out serviceBuilding))
                        {
                            serviceBuilding = new ServiceBuildingInfo(buildingId, ref building, this.services[i].DispatcherType);
                            Log.Debug(this, "CategorizeBuilding", "Add", this.services[i].ServiceCategory, buildingId, building.Info.name, serviceBuilding.BuildingName, serviceBuilding.Range, serviceBuilding.District);

                            this.services[i].ServiceBuildings[buildingId] = serviceBuilding;
                        }
                        else
                        {
                            serviceBuilding.Update(ref building);
                        }

                        if (this.services[i].AutoEmpty && (serviceBuilding.NeedsEmptying || serviceBuilding.EmptyingIsDone))
                        {
                            this.services[i].BuildingsInNeedOfEmptyingChange.Add(serviceBuilding);
                        }
                    }
                    else if (this.services[i].ServiceBuildings.ContainsKey(buildingId))
                    {
                        Log.Debug(this, "CategorizeBuilding", "Del", this.services[i].ServiceCategory, buildingId);

                        this.services[i].ServiceBuildings.Remove(buildingId);
                    }

                    // Check if target building.
                    if (this.services[i].DispatchVehicles)
                    {
                        TargetBuildingInfo.ServiceDemand demand = this.services[i].GetTargetBuildingDemand(buildingId, ref building);

                        if (demand != TargetBuildingInfo.ServiceDemand.None && this.services[i].DispatchToBuilding(buildingId, ref building))
                        {
                            if (!this.services[i].TargetBuildings.ContainsKey(buildingId))
                            {
                                TargetBuildingInfo targetBuilding = new TargetBuildingInfo(buildingId, ref building, this.services[i].DispatcherType, demand);

                                if (Log.LogToFile)
                                {
                                    Log.Debug(this, "CategorizeBuilding", "Add", this.services[i].TargetCategory, buildingId, building.Info.name, targetBuilding.BuildingName, targetBuilding.ProblemValue, targetBuilding.HasProblem, targetBuilding.District);
                                }

                                this.services[i].TargetBuildings[buildingId] = targetBuilding;
                                this.services[i].HasTargetBuildingsToCheck = true;
                            }
                            else
                            {
                                this.services[i].TargetBuildings[buildingId].Update(ref building, demand);
                                this.services[i].HasTargetBuildingsToCheck = this.services[i].HasTargetBuildingsToCheck || this.services[i].TargetBuildings[buildingId].CheckThis;
                            }
                        }
                        else if (this.services[i].TargetBuildings.ContainsKey(buildingId))
                        {
                            if (this.services[i].DelayTargetBuildingRemoval(buildingId, ref building))
                            {
                                this.services[i].TargetBuildings[buildingId].Update(ref building, TargetBuildingInfo.ServiceDemand.None);
                            }
                            else
                            {
                                if (Log.LogToFile)
                                {
                                    Log.Debug(this, "CategorizeBuilding", "Del", this.services[i].TargetCategory, buildingId);
                                }

                                this.services[i].TargetBuildings.Remove(buildingId);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finish the categorization.
        /// </summary>
        public void CategorizeFinish()
        {
            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null && this.services[i].AutoEmpty && this.services[i].BuildingsInNeedOfEmptyingChange.Count > 0)
                {
                    this.services[i].BuildingsCanEmptyOther = 0;
                    this.services[i].BuildingsEmptying = 0;

                    foreach (ServiceBuildingInfo building in this.services[i].ServiceBuildings.Values)
                    {
                        if (building.IsAutoEmptying)
                        {
                            this.services[i].BuildingsEmptying++;
                        }

                        if (building.CanEmptyOther)
                        {
                            this.services[i].BuildingsCanEmptyOther++;
                        }
                    }

                    foreach (ServiceBuildingInfo building in this.services[i].ServiceBuildings.Values)
                    {
                        if (building.EmptyingIsDone)
                        {
                            building.AutoEmptyStop();
                            this.services[i].BuildingsEmptying--;
                        }
                        else if (building.NeedsEmptying && this.services[i].BuildingsCanEmptyOther > this.services[i].BuildingsEmptying)
                        {
                            building.AutoEmptyStart();
                            this.services[i].BuildingsEmptying++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Prepare for categorization.
        /// </summary>
        public void CategorizePrepare()
        {
            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null && this.services[i].AutoEmpty)
                {
                    this.services[i].BuildingsInNeedOfEmptyingChange.Clear();
                }
            }
        }

        /// <summary>
        /// Checks the vehicle target.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        public void CheckVehicleTarget(ushort vehicleId, ref Vehicle vehicle)
        {
            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null && this.services[i].IsDispatching && vehicle.m_transferType == this.services[i].Dispatcher.TransferType)
                {
                    this.services[i].Dispatcher.CheckVehicleTarget(vehicleId, ref vehicle);
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
                for (int i = 0; i < this.services.Length; i++)
                {
                    if (this.services[i] != null)
                    {
                        if (this.services[i].TargetBuildings != null)
                        {
                            BuildingHelper.DebugListLog(this.services[i].TargetBuildings.Values);
                        }

                        if (this.services[i].ServiceBuildings != null)
                        {
                            BuildingHelper.DebugListLog(this.services[i].ServiceBuildings.Values);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "DebugListLogBuildings", ex);
            }
        }

        /// <summary>
        /// Dispatches vehicles to targets.
        /// </summary>
        public void Dispatch()
        {
            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null && this.services[i].IsDispatching)
                {
                    this.services[i].Dispatcher.Dispatch();
                }
            }
        }

        /// <summary>
        /// Gets the building categories for a building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <returns>The categories in which the building has been categorized.</returns>
        public IEnumerable<string> GetCategories(ushort buildingId)
        {
            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null)
                {
                    if (this.services[i].ServiceBuildings != null && this.services[i].ServiceBuildings.ContainsKey(buildingId))
                    {
                        yield return this.services[i].ServiceCategory;
                    }

                    if (this.services[i].TargetBuildings != null && this.services[i].TargetBuildings.ContainsKey(buildingId))
                    {
                        yield return this.services[i].TargetCategory;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the service building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <returns>A service building with the id, or null.</returns>
        public ServiceBuildingInfo GetServiceBuilding(ushort buildingId)
        {
            ServiceBuildingInfo building;

            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null & this.services[i].ServiceBuildings != null && this.services[i].ServiceBuildings.TryGetValue(buildingId, out building))
                {
                    return building;
                }
            }

            return null;
        }

        /// <summary>
        /// Initializes the data lists.
        /// </summary>
        /// <param name="constructing">If set to <c>true</c> object is being constructed.</param>
        public void Initialize(bool constructing)
        {
            Log.Debug(this, "Initialize", constructing);

            if (constructing || this.services == null)
            {
                this.services = new DispatchService[]
                   {
                        new DispatchService.DeathCare(),
                        new DispatchService.Garbage(),
                        (Global.EnableDevExperiments ? new DispatchService.HealthCare() : null)
                   };
            }
            else
            {
                foreach (DispatchService service in this.services)
                {
                    if (service != null)
                    {
                        service.ReInitialize();
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the specified disapatcher service is dispatching.
        /// </summary>
        /// <param name="dispatcherType">Type of the dispatcher.</param>
        /// <returns>
        ///   <c>true</c> if the specified dispatcher service is dispatching; otherwise, <c>false</c>.
        /// </returns>
        public bool IsDispatching(Dispatcher.DispatcherTypes dispatcherType)
        {
            return this.services[(int)dispatcherType] != null && this.services[(int)dispatcherType].IsDispatching;
        }

        /// <summary>
        /// Re-initialize the part.
        /// </summary>
        public void ReInitialize()
        {
            this.Initialize(false);
        }

        /// <summary>
        /// Remove categorized buidling.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        public void UnCategorizeBuilding(ushort buildingId)
        {
            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null)
                {
                    if (this.services[i].ServiceBuildings != null && this.services[i].ServiceBuildings.ContainsKey(buildingId))
                    {
                        this.services[i].ServiceBuildings.Remove(buildingId);
                        Log.Debug(this, "CategorizeBuildings", "Rem", this.services[i].ServiceCategory, buildingId);
                    }

                    if (this.services[i].TargetBuildings != null && this.services[i].TargetBuildings.ContainsKey(buildingId))
                    {
                        this.services[i].TargetBuildings.Remove(buildingId);
                        Log.Debug(this, "CategorizeBuildings", "Rem", this.services[i].TargetCategory, buildingId);
                    }
                }
            }
        }

        /// <summary>
        /// Update all buildings.
        /// </summary>
        public void UpdateAllBuildings()
        {
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            DistrictManager districtManager = null;
            if (Global.Settings.DispatchAnyByDistrict)
            {
                districtManager = Singleton<DistrictManager>.instance;
            }

            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null)
                {
                    if (this.services[i].ServiceBuildings != null)
                    {
                        foreach (ServiceBuildingInfo building in this.services[i].ServiceBuildings.Values)
                        {
                            building.ReInitialize();
                            building.UpdateValues(ref buildings[building.BuildingId], true);
                        }
                    }

                    if (this.services[i].TargetBuildings != null)
                    {
                        foreach (TargetBuildingInfo building in this.services[i].TargetBuildings.Values)
                        {
                            building.ReInitialize();
                            building.UpdateValues(ref buildings[building.BuildingId], true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Prepare for updating.
        /// </summary>
        public void UpdatePrepare()
        {
            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null)
                {
                    this.services[i].HasTargetBuildingsToCheck = false;
                }
            }
        }
    }
}