using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.Services
{
    /// <summary>
    /// Base class for dispatch services.
    /// </summary>
    internal abstract class DispatchService : IService
    {
        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        public readonly StandardServiceSettings Settings;

        /// <summary>
        /// The buiildings in need of emptying change.
        /// </summary>
        protected List<ServiceBuildingInfo> BuildingsInNeedOfEmptyingChange = null;

        /// <summary>
        /// The dispatcher.
        /// </summary>
        protected Dispatcher dispatcher = null;

        /// <summary>
        /// The service buildings.
        /// </summary>
        protected Dictionary<ushort, ServiceBuildingInfo> serviceBuildings = null;

        /// <summary>
        /// The target buildings.
        /// </summary>
        protected Dictionary<ushort, TargetBuildingInfo> targetBuildings = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatchService" /> class.
        /// </summary>
        /// <param name="serviceType">Type of the dispatcher.</param>
        /// <param name="transferType">Type of the transfer.</param>
        /// <param name="settings">The settings.</param>
        public DispatchService(ServiceHelper.ServiceType serviceType, byte transferType, StandardServiceSettings settings)
        {
            this.Settings = settings;
            this.ServiceType = serviceType;
            this.TransferType = transferType;

            this.HasTargetBuildingsToCheck = false;

            this.Initialize(true);
        }

        /// <summary>
        /// Gets a value indicating whether to empty automatically.
        /// </summary>
        /// <value>
        ///   <c>true</c> if automatic emptying; otherwise, <c>false</c>.
        /// </value>
        public bool AutoEmpty => Settings.AutoEmpty;

        /// <summary>
        /// Gets a value indicating whether this dispatcher creates vehicles.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the dispatcher creates vehicles; otherwise, <c>false</c>.
        /// </value>
        public bool CreatesVehicles => this.IsDispatching && this.Settings.CreateSpares != ServiceDispatcherSettings.SpareVehiclesCreation.Never;

        /// <summary>
        /// Gets a value indicating whether to dispatch vehicles.
        /// </summary>
        /// <value>
        ///   <c>true</c> if dispatching vehicles; otherwise, <c>false</c>.
        /// </value>
        public bool DispatchVehicles => this.Settings.DispatchVehicles;

        /// <summary>
        /// Gets a value indicating whether this <see cref="DispatchService"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public bool Enabled => this.IsDispatching || this.IsAutoEmptying;

        /// <summary>
        /// Gets a value indicating whether this instance has buildings.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has buildings; otherwise, <c>false</c>.
        /// </value>
        public bool HasBuildings => this.serviceBuildings != null || this.targetBuildings != null;

        /// <summary>
        /// Gets a value indicating whether this instance has target buildings that should be checked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has target buildings to check; otherwise, <c>false</c>.
        /// </value>
        public bool HasTargetBuildingsToCheck { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this instance has vehicles.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has vehicles; otherwise, <c>false</c>.
        /// </value>
        public bool HasVehicles => this.serviceBuildings != null;

        /// <summary>
        /// Gets a value indicating whether this instance is auto-emptying.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is auto-emptying; otherwise, <c>false</c>.
        /// </value>
        public bool IsAutoEmptying => this.Settings.AutoEmpty && this.serviceBuildings != null && this.BuildingsInNeedOfEmptyingChange != null;

        /// <summary>
        /// Gets a value indicating whether this instance is dispatching.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is dispatching; otherwise, <c>false</c>.
        /// </value>
        public bool IsDispatching => this.Settings.DispatchVehicles && this.dispatcher != null && this.serviceBuildings != null && this.targetBuildings != null;

        /// <summary>
        /// Gets a value indicating whether to remove unusable buildings.
        /// </summary>
        /// <value>
        ///   <c>true</c> if unusable buildings should be removed; otherwise, <c>false</c>.
        /// </value>
        public bool RemoveUnusableBuildings => true;

        /// <summary>
        /// Gets the service buildings.
        /// </summary>
        /// <value>
        /// The service buildings.
        /// </value>
        public IEnumerable<ServiceBuildingInfo> ServiceBuildings => this.serviceBuildings.Values;

        /// <summary>
        /// Gets the service category.
        /// </summary>
        /// <value>
        /// The service category.
        /// </value>
        public abstract string ServiceCategory { get; }

        /// <summary>
        /// Gets the service log pre/suffix.
        /// </summary>
        /// <value>
        /// The service log pre/suffix.
        /// </value>
        public abstract string ServiceLogFix { get; }

        /// <summary>
        /// The dispatcher type.
        /// </summary>
        public ServiceHelper.ServiceType ServiceType { get; private set; }

        /// <summary>
        /// Gets the target buildings.
        /// </summary>
        /// <value>
        /// The target buildings.
        /// </value>
        public IEnumerable<TargetBuildingInfo> TargetBuildings => this.targetBuildings.Values;

        /// <summary>
        /// Gets the target buildings to check.
        /// </summary>
        /// <value>
        /// The target buildings to check.
        /// </value>
        public TargetBuildingInfo[] TargetBuildingsToCheck => this.targetBuildings.Values.WhereToArray(tb => tb.CheckThis && !tb.HandledNow);

        /// <summary>
        /// Gets the target category.
        /// </summary>
        /// <value>
        /// The target category.
        /// </value>
        public abstract string TargetCategory { get; }

        /// <summary>
        /// Gets the target log pre/suffix.
        /// </summary>
        /// <value>
        /// The target log pre/suffix.
        /// </value>
        public abstract string TargetLogFix { get; }

        /// <summary>
        /// Gets the transfer reason.
        /// </summary>
        /// <value>
        /// The transfer reason.
        /// </value>
        public TransferManager.TransferReason TransferReason => (TransferManager.TransferReason)this.TransferType;

        /// <summary>
        /// The transfer type.
        /// </summary>
        public byte TransferType { get; private set; }

        /// <summary>
        /// Categorizes the building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        public void CheckBuilding(ushort buildingId, ref Building building)
        {
            // Check if service building.
            if (this.DispatchFromBuilding(buildingId, ref building))
            {
                ServiceBuildingInfo serviceBuilding;

                if (!this.serviceBuildings.TryGetValue(buildingId, out serviceBuilding))
                {
                    serviceBuilding = new ServiceBuildingInfo(buildingId, ref building, this.ServiceType);
                    Log.Debug(this, "CheckBuilding", "Add", buildingId, building.Info.name, serviceBuilding.BuildingName, serviceBuilding.Range, serviceBuilding.District);

                    this.serviceBuildings[buildingId] = serviceBuilding;
                }
                else
                {
                    serviceBuilding.Update(ref building);
                }

                if (this.AutoEmpty && (serviceBuilding.NeedsEmptying || serviceBuilding.EmptyingIsDone))
                {
                    this.BuildingsInNeedOfEmptyingChange.Add(serviceBuilding);
                }
            }
            else if (this.serviceBuildings.ContainsKey(buildingId))
            {
                Log.Debug(this, "CheckBuilding", "Del", buildingId);

                this.serviceBuildings.Remove(buildingId);
            }

            // Check if target building.
            if (this.DispatchVehicles)
            {
                TargetBuildingInfo.ServiceDemand demand = this.GetTargetBuildingDemand(buildingId, ref building);

                if (demand != TargetBuildingInfo.ServiceDemand.None && this.DispatchToBuilding(buildingId, ref building))
                {
                    if (!this.targetBuildings.ContainsKey(buildingId))
                    {
                        TargetBuildingInfo targetBuilding = new TargetBuildingInfo(buildingId, ref building, this.ServiceType, demand);

                        if (Log.LogToFile)
                        {
                            Log.Debug(this, "CheckBuilding", "Add", buildingId, building.Info.name, targetBuilding.BuildingName, targetBuilding.ProblemValue, targetBuilding.HasProblem, targetBuilding.District);
                        }

                        this.targetBuildings[buildingId] = targetBuilding;
                        this.HasTargetBuildingsToCheck = true;
                    }
                    else
                    {
                        this.targetBuildings[buildingId].Update(ref building, demand);
                        this.HasTargetBuildingsToCheck = this.HasTargetBuildingsToCheck || this.targetBuildings[buildingId].CheckThis;
                    }
                }
                else if (this.targetBuildings.ContainsKey(buildingId))
                {
                    if (this.DelayTargetBuildingRemoval(buildingId, ref building))
                    {
                        this.targetBuildings[buildingId].Update(ref building, TargetBuildingInfo.ServiceDemand.None);
                    }
                    else
                    {
                        if (Log.LogToFile)
                        {
                            Log.Debug(this, "CheckBuilding", "Del", buildingId);
                        }

                        this.targetBuildings.Remove(buildingId);
                    }
                }
            }
        }

        /// <summary>
        /// Finish the categorization.
        /// </summary>
        public void CheckBuildingsFinish()
        {
            uint canEmptyOther = 0;
            uint emptying = 0;

            if (this.BuildingsInNeedOfEmptyingChange != null && this.BuildingsInNeedOfEmptyingChange.Count > 0)
            {
                if (this.serviceBuildings != null)
                {
                    foreach (ServiceBuildingInfo building in this.serviceBuildings.Values)
                    {
                        if (building.IsAutoEmptying)
                        {
                            emptying++;
                        }

                        if (building.CanEmptyOther)
                        {
                            canEmptyOther++;
                        }
                    }

                    foreach (ServiceBuildingInfo building in this.BuildingsInNeedOfEmptyingChange)
                    {
                        if (building.EmptyingIsDone && building.IsAutoEmptying)
                        {
                            building.AutoEmptyStop();
                            emptying--;
                        }
                        else if (building.NeedsEmptying && !building.IsAutoEmptying && canEmptyOther > emptying)
                        {
                            building.AutoEmptyStart();
                            emptying++;
                        }
                    }
                }

                this.BuildingsInNeedOfEmptyingChange.Clear();
            }
        }

        /// <summary>
        /// Prepare for categorization.
        /// </summary>
        public void CheckBuildingsPrepare()
        { }

        /// <summary>
        /// Checks the vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        public void CheckVehicle(ushort vehicleId, ref Vehicle vehicle)
        { }

        /// <summary>
        /// Checks the vehicle target.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        public void CheckVehicleTarget(ushort vehicleId, ref Vehicle vehicle)
        {
            if (this.dispatcher != null)
            {
                this.dispatcher.CheckVehicleTarget(vehicleId, ref vehicle);
            }
        }

        /// <summary>
        /// Determines whether the specified building identifier is contained in this instance's target list.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <returns>
        ///   <c>true</c> if the specified building identifier is in the target list; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsTargetBuilding(ushort buildingId)
        {
            return this.targetBuildings != null && this.targetBuildings.ContainsKey(buildingId);
        }

        /// <summary>
        /// Logs a list of service building info for debug use.
        /// </summary>
        public void DebugListLogBuildings()
        {
            try
            {
                if (this.targetBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.targetBuildings.Values, this.TargetCategory);
                }

                if (this.serviceBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.serviceBuildings.Values, this.ServiceCategory);
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "DebugListLogBuildings", ex);
            }
        }

        /// <summary>
        /// Logs a list of vehicle info for debug use.
        /// </summary>
        public void DebugListLogVehicles()
        { }

        /// <summary>
        /// Dispatches vehicles to targets.
        /// </summary>
        public void Dispatch()
        {
            if (this.dispatcher != null)
            {
                this.dispatcher.Dispatch();
            }
        }

        /// <summary>
        /// Gets the building categories for a building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <returns>The categories in which the building has been categorized.</returns>
        public IEnumerable<string> GetCategories(ushort buildingId)
        {
            if (this.serviceBuildings != null && this.serviceBuildings.ContainsKey(buildingId))
            {
                yield return this.ServiceCategory;
            }

            if (this.targetBuildings != null && this.targetBuildings.ContainsKey(buildingId))
            {
                yield return this.TargetCategory;
            }
        }

        /// <summary>
        /// Gets the usable service buildings.
        /// </summary>
        /// <param name="ignoreRange">if set to <c>true</c> ignore range.</param>
        /// <returns>The usable service buildings.</returns>
        public ServiceBuildingInfo[] GetUsableServiceBuildings(bool ignoreRange)
        {
            if (!this.Settings.DispatchByRange && !this.Settings.DispatchByDistrict)
            {
                // All buildings than can dispatch.
                return this.serviceBuildings.Values.WhereToArray(sb => sb.CurrentTargetCanDispatch);
            }

            if (!ignoreRange)
            {
                // All buildings in range than can dispatch.
                return this.serviceBuildings.Values.WhereToArray(sb => sb.CurrentTargetCanDispatch && sb.CurrentTargetInRange);
            }

            if (this.Settings.IgnoreRangeUseClosestBuildings > 0)
            {
                // All buildings in range than can dispatch.
                List<ServiceBuildingInfo> checkServiceBuildings = this.serviceBuildings.Values.WhereToList(sb => sb.CurrentTargetCanDispatch && sb.CurrentTargetInRange);

                // Closest buildings out of range, if they can dispatch.
                checkServiceBuildings.AddRange(this.serviceBuildings.Values
                                            .Where(sb => !sb.CurrentTargetInRange)
                                            .OrderByTake(sb => sb.CurrentTargetDistance, this.Settings.IgnoreRangeUseClosestBuildings)
                                            .Where(sb => sb.CurrentTargetCanDispatch));

                return checkServiceBuildings.ToArray();
            }

            // All buildings than can dispatch.
            return this.serviceBuildings.Values.WhereToArray(sb => sb.CurrentTargetCanDispatch);
        }

        /// <summary>
        /// Reinitializes this instance.
        /// </summary>
        public void ReInitialize()
        {
            this.Initialize(false);
        }

        /// <summary>
        /// Reinitializes the dispatcher.
        /// </summary>
        public void ReinitializeDispatcher()
        {
            if (this.dispatcher != null)
            {
                this.dispatcher.ReInitialize();
            }
        }

        /// <summary>
        /// Remove categorized building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        public void RemoveBuilding(ushort buildingId)
        {
            if (this.serviceBuildings != null && this.serviceBuildings.ContainsKey(buildingId))
            {
                this.serviceBuildings.Remove(buildingId);
                Log.Debug(this, "RemoveBuilding", "Rem", this.ServiceCategory, buildingId);
            }

            if (this.targetBuildings != null && this.targetBuildings.ContainsKey(buildingId))
            {
                this.targetBuildings.Remove(buildingId);
                Log.Debug(this, "RemoveBuilding", "Rem", this.TargetCategory, buildingId);
            }
        }

        /// <summary>
        /// Remove categorized vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        public void RemoveVehicle(ushort vehicleId)
        { }

        /// <summary>
        /// Gets the service building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <returns>True if building found.</returns>
        public bool TryGetServiceBuilding(ushort buildingId, out ServiceBuildingInfo building)
        {
            if (this.serviceBuildings == null)
            {
                building = null;
                return false;
            }

            return this.serviceBuildings.TryGetValue(buildingId, out building);
        }

        /// <summary>
        /// Gets the target building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <returns>True if building found.</returns>
        public bool TryGetTargetBuilding(ushort buildingId, out TargetBuildingInfo building)
        {
            if (this.targetBuildings == null)
            {
                building = null;
                return false;
            }

            return this.targetBuildings.TryGetValue(buildingId, out building);
        }

        /// <summary>
        /// Gets the service building.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>
        /// True if building found.
        /// </returns>
        public bool TryGetVehicle(ushort vehicleId, out IVehicleInfo vehicle)
        {
            vehicle = null;
            return false;
        }

        /// <summary>
        /// Update all buildings.
        /// </summary>
        public void UpdateAllBuildings()
        {
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            if (this.serviceBuildings != null)
            {
                foreach (ServiceBuildingInfo building in this.serviceBuildings.Values)
                {
                    building.ReInitialize();
                    building.UpdateValues(ref buildings[building.BuildingId], true);
                }
            }

            if (this.targetBuildings != null)
            {
                foreach (TargetBuildingInfo building in this.targetBuildings.Values)
                {
                    building.ReInitialize();
                    building.UpdateValues(ref buildings[building.BuildingId], true);
                }
            }
        }

        /// <summary>
        /// Update all vehicles.
        /// </summary>
        public void UpdateAllVehicles()
        {
            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            if (this.serviceBuildings != null)
            {
                foreach (ServiceBuildingInfo building in this.serviceBuildings.Values)
                {
                    foreach (ServiceVehicleInfo vehicle in building.Vehicles.Values)
                    {
                        vehicle.ReInitialize();
                        vehicle.UpdateValues(ref vehicles[vehicle.VehicleId], true);
                    }
                }
            }
        }

        /// <summary>
        /// Finish the update.
        /// </summary>
        public void UpdateBuildingsFinish()
        { }

        /// <summary>
        /// Prepare for update.
        /// </summary>
        public void UpdateBuildingsPrepare()
        {
            this.HasTargetBuildingsToCheck = false;
        }

        /// <summary>
        /// Checks if removal of target building should be delayed.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <returns>
        /// True if target buidling should not yet be removed.
        /// </returns>
        protected virtual bool DelayTargetBuildingRemoval(ushort buildingId, ref Building building)
        {
            return this.targetBuildings[buildingId].WantedService;
        }

        /// <summary>
        /// Determines whether the specified building is a service building for this service.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <returns>
        ///   <c>true</c> if the specified building is a service building; otherwise, <c>false</c>.
        /// </returns>
        protected abstract bool DispatchFromBuilding(ushort buildingId, ref Building building);

        /// <summary>
        /// Determines whether the specified building is a a building this service is willing to dispatch to.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <returns>
        ///   <c>true</c> if the specified building can be dispatched to; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool DispatchToBuilding(ushort buildingId, ref Building building)
        {
            return true;
        }

        /// <summary>
        /// Gets the target building demand.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <returns>
        /// The demand.
        /// </returns>
        protected abstract TargetBuildingInfo.ServiceDemand GetTargetBuildingDemand(ushort buildingId, ref Building building);

        /// <summary>
        /// Initializes the data lists.
        /// </summary>
        /// <param name="constructing">If set to <c>true</c> object is being constructed.</param>
        protected virtual void Initialize(bool constructing)
        {
            Log.InfoList info = new Log.InfoList();

            info.Add("Constructing", constructing);
            info.Add("DispatchVehicles", this.Settings.DispatchVehicles);
            info.Add("AutoEmpty", this.Settings.AutoEmpty);

            if (constructing || !this.Settings.DispatchVehicles || this.targetBuildings == null)
            {
                this.HasTargetBuildingsToCheck = false;

                if (!this.Settings.DispatchVehicles)
                {
                    this.targetBuildings = null;
                    this.dispatcher = null;
                }
                else
                {
                    if (constructing || this.targetBuildings == null)
                    {
                        info.Add("TargetBuildings", "new");
                        this.targetBuildings = new Dictionary<ushort, TargetBuildingInfo>();
                    }

                    if (constructing || this.dispatcher == null)
                    {
                        info.Add("Dispatcher", "new");
                        this.dispatcher = new Dispatcher(this);
                    }
                    else
                    {
                        this.dispatcher.ReInitialize();
                    }
                }
            }

            if (!this.Settings.AutoEmpty)
            {
                this.BuildingsInNeedOfEmptyingChange = null;
            }
            else if (constructing || this.BuildingsInNeedOfEmptyingChange == null)
            {
                info.Add("BuildingsInNeedOfEmptying", "new");
                this.BuildingsInNeedOfEmptyingChange = new List<ServiceBuildingInfo>();
            }

            if (constructing || !this.Settings.DispatchVehicles || !this.Settings.AutoEmpty || this.serviceBuildings == null)
            {
                if (!this.Settings.DispatchVehicles && !this.Settings.AutoEmpty)
                {
                    this.serviceBuildings = null;
                }
                else if (constructing || this.serviceBuildings == null)
                {
                    info.Add("ServiceBuildings", "new");
                    this.serviceBuildings = new Dictionary<ushort, ServiceBuildingInfo>();
                }
            }

            Log.Debug(this, "Initialize", info);
        }
    }
}