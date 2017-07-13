using ColossalFramework;
using System;
using System.Collections.Generic;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Interface for service data keeper.
    /// </summary>
    internal interface IDispatchService
    {
        /// <summary>
        /// Gets a value indicating whether to empty automatically.
        /// </summary>
        /// <value>
        ///   <c>true</c> if automatic emptying; otherwise, <c>false</c>.
        /// </value>
        bool AutoEmpty { get; }

        /// <summary>
        /// The buildings-can-empty-other counter.
        /// </summary>
        uint BuildingsCanEmptyOther { get; set; }

        /// <summary>
        /// The buildings-emptying counter.
        /// </summary>
        uint BuildingsEmptying { get; set; }

        /// <summary>
        /// The buiildings in need of emptying change.
        /// </summary>
        List<ServiceBuildingInfo> BuildingsInNeedOfEmptyingChange { get; }

        /// <summary>
        /// Gets the type of the dispatcher.
        /// </summary>
        /// <value>
        /// The type of the dispatcher.
        /// </value>
        Dispatcher.DispatcherTypes DispatcherType { get; }

        /// <summary>
        /// Gets a value indicating whether to dispatch vehicles.
        /// </summary>
        /// <value>
        ///   <c>true</c> if dispatching vehicles; otherwise, <c>false</c>.
        /// </value>
        bool DispatchVehicles { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has target buildings that should be checked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has target buildings to check; otherwise, <c>false</c>.
        /// </value>
        bool HasTargetBuildingsToCheck { get; set; }

        /// <summary>
        /// Gets the service buildings.
        /// </summary>
        Dictionary<ushort, ServiceBuildingInfo> ServiceBuildings { get; }

        /// <summary>
        /// Gets the service category.
        /// </summary>
        /// <value>
        /// The service category.
        /// </value>
        string ServiceCategory { get; }

        /// <summary>
        /// Gets the service log pre/suffix.
        /// </summary>
        /// <value>
        /// The service log pre/suffix.
        /// </value>
        string ServiceLogFix { get; }

        /// <summary>
        /// Gets the target buildings.
        /// </summary>
        Dictionary<ushort, TargetBuildingInfo> TargetBuildings { get; }

        /// <summary>
        /// Gets the target category.
        /// </summary>
        /// <value>
        /// The target category.
        /// </value>
        string TargetCategory { get; }

        /// <summary>
        /// Gets the target log pre/suffix.
        /// </summary>
        /// <value>
        /// The target log pre/suffix.
        /// </value>
        string TargetLogFix { get; }

        /// <summary>
        /// Checks if removal of target building should be delayed.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <returns>
        /// True if target buidling should not yet be removed.
        /// </returns>
        bool DelayTargetBuildingRemoval(ushort buildingId, ref Building building);

        /// <summary>
        /// Determines whether the specified building is a service building for this service.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <returns>
        ///   <c>true</c> if the specified building is a service building; otherwise, <c>false</c>.
        /// </returns>
        bool DispatchFromBuilding(ushort buildingId, ref Building building);

        /// <summary>
        /// Determines whether the specified building is a a building this service is willing to dispatch to.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <returns>
        ///   <c>true</c> if the specified building can be dispatched to; otherwise, <c>false</c>.
        /// </returns>
        bool DispatchToBuilding(ushort buildingId, ref Building building);

        /// <summary>
        /// Gets the target building demand.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <returns>
        /// The demand.
        /// </returns>
        TargetBuildingInfo.ServiceDemand GetTargetBuildingDemand(ushort buildingId, ref Building building);

        /// <summary>
        /// Re-initialize the part.
        /// </summary>
        void ReInitialize();
    }

    /// <summary>
    /// Keeps track of dispatch services.
    /// </summary>
    internal class DispatchServiceKeeper : IHandlerPart
    {
        /// <summary>
        /// The services.
        /// </summary>
        private IDispatchService[] services = new IDispatchService[(int)Dispatcher.DispatcherTypes.None];

        /// <summary>
        /// Gets the dispatching services.
        /// </summary>
        /// <value>
        /// The dispatching services.
        /// </value>
        public IEnumerable<IDispatchService> DispatchingServices => this.services.Where(s => s != null && s.DispatchVehicles && s.ServiceBuildings != null && s.TargetBuildings != null);

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <value>
        /// The services.
        /// </value>
        public IEnumerable<IDispatchService> Services => this.services.Where(s => s != null && (s.ServiceBuildings != null || s.TargetBuildings != null));

        /// <summary>
        /// Gets the <see cref="IDispatchService"/> with the specified dispatcher type.
        /// </summary>
        /// <value>
        /// The <see cref="IDispatchService"/>.
        /// </value>
        /// <param name="DispatcherType">Type of the dispatcher.</param>
        /// <returns>The <see cref="IDispatchService"/>.</returns>
        public IDispatchService this[Dispatcher.DispatcherTypes DispatcherType] => this.services[(int)DispatcherType];

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

            this.Initialize<DispatchService.DeathCare>(
                constructing,
                Dispatcher.DispatcherTypes.HearseDispatcher,
                Global.Settings.DeathCare.DispatchVehicles,
                Global.Settings.DeathCare.AutoEmpty);

            this.Initialize<DispatchService.Garbage>(
                constructing,
                Dispatcher.DispatcherTypes.GarbageTruckDispatcher,
                Global.Settings.Garbage.DispatchVehicles,
                Global.Settings.Garbage.AutoEmpty);

            this.Initialize<DispatchService.HealthCare>(
                constructing,
                Dispatcher.DispatcherTypes.AmbulanceDispatcher,
                Global.Settings.HealthCare.DispatchVehicles,
                Global.Settings.HealthCare.AutoEmpty);
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

        /// <summary>
        /// Initializes a dispatch service.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <param name="constructing">If set to <c>true</c> object is being constructed.</param>
        /// <param name="dispatcherType">Type of the dispatcher.</param>
        /// <param name="dispatchVehicles">if set to <c>true</c> the service should dispatch vehicles.</param>
        /// <param name="autoEmpty">if set to <c>true</c> the service should empty automatically.</param>
        private void Initialize<T>(bool constructing, Dispatcher.DispatcherTypes dispatcherType, bool dispatchVehicles, bool autoEmpty) where T : IDispatchService, new()
        {
            if (dispatchVehicles || autoEmpty)
            {
                if (constructing || this.services[(int)dispatcherType] == null)
                {
                    this.services[(int)dispatcherType] = new T();
                }
                else
                {
                    this.services[(int)dispatcherType].ReInitialize();
                }
            }
            else
            {
                this.services[(int)dispatcherType] = null;
            }
        }

        /// <summary>
        /// Base class for global dispatch service data.
        /// </summary>
        public abstract class DispatchService
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DispatchService"/> class.
            /// </summary>
            public DispatchService()
            {
                this.BuildingsCanEmptyOther = 0;
                this.BuildingsEmptying = 0;
                this.HasTargetBuildingsToCheck = false;
                this.BuildingsInNeedOfEmptyingChange = null;
                this.ServiceBuildings = null;
                this.TargetBuildings = null;
            }

            /// <summary>
            /// The death-care-buildings-can-empty-other counter.
            /// </summary>
            public uint BuildingsCanEmptyOther { get; set; }

            /// <summary>
            /// The death-care-buildings-emptying counter.
            /// </summary>
            public uint BuildingsEmptying { get; set; }

            /// <summary>
            /// The buildings in need of emptying change.
            /// </summary>
            public List<ServiceBuildingInfo> BuildingsInNeedOfEmptyingChange { get; private set; }

            /// <summary>
            /// Gets a value indicating whether this instance has target buildings that should be checked.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance has target buildings to check; otherwise, <c>false</c>.
            /// </value>
            public bool HasTargetBuildingsToCheck { get; set; }

            /// <summary>
            /// Gets the service buildings.
            /// </summary>
            public Dictionary<ushort, ServiceBuildingInfo> ServiceBuildings { get; private set; }

            /// <summary>
            /// Gets the target buildings.
            /// </summary>
            public Dictionary<ushort, TargetBuildingInfo> TargetBuildings { get; private set; }

            /// <summary>
            /// Initializes the data lists.
            /// </summary>
            /// <param name="constructing">If set to <c>true</c> object is being constructed.</param>
            /// <param name="dispatchVehicles">if set to <c>true</c> dispatch vehicles.</param>
            /// <param name="autoEmpty">if set to <c>true</c> empty automatically.</param>
            private void Initialize(bool constructing, bool dispatchVehicles, bool autoEmpty)
            {
                Log.InfoList info = new Log.InfoList();

                info.Add("Constructing", constructing);
                info.Add("DispatchVehicles", dispatchVehicles);
                info.Add("AutoEmpty", autoEmpty);

                if (constructing || !dispatchVehicles || this.TargetBuildings == null)
                {
                    this.HasTargetBuildingsToCheck = false;

                    if (!dispatchVehicles)
                    {
                        this.TargetBuildings = null;
                    }
                    else if (constructing || this.TargetBuildings == null)
                    {
                        info.Add("TargetBuildings", "new");
                        this.TargetBuildings = new Dictionary<ushort, TargetBuildingInfo>();
                    }
                }

                if (!autoEmpty)
                {
                    this.BuildingsInNeedOfEmptyingChange = null;
                }
                else if (constructing || this.BuildingsInNeedOfEmptyingChange == null)
                {
                    info.Add("BuildingsInNeedOfEmptying", "new");
                    this.BuildingsInNeedOfEmptyingChange = new List<ServiceBuildingInfo>();
                }

                if (constructing || !dispatchVehicles || !autoEmpty || this.ServiceBuildings == null)
                {
                    this.BuildingsCanEmptyOther = 0;
                    this.BuildingsEmptying = 0;

                    if (!dispatchVehicles && !autoEmpty)
                    {
                        this.ServiceBuildings = null;
                    }
                    else if (constructing || this.ServiceBuildings == null)
                    {
                        info.Add("ServiceBuildings", "new");
                        this.ServiceBuildings = new Dictionary<ushort, ServiceBuildingInfo>();
                    }
                }

                Log.Debug(this, "Initialize", info);
            }

            /// <summary>
            /// Death care service keeper.
            /// </summary>
            /// <seealso cref="WhatThe.Mods.CitiesSkylines.ServiceDispatcher.DispatchServiceKeeper.DispatchService" />
            /// <seealso cref="WhatThe.Mods.CitiesSkylines.ServiceDispatcher.IDispatchService" />
            public class DeathCare : DispatchService, IDispatchService
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="DeathCare"/> class.
                /// </summary>
                public DeathCare() : base()
                {
                    this.Initialize(true, this.DispatchVehicles, this.AutoEmpty);
                }

                /// <summary>
                /// Gets a value indicating whether to empty automatically.
                /// </summary>
                /// <value>
                ///   <c>true</c> if automatic emptying; otherwise, <c>false</c>.
                /// </value>
                public bool AutoEmpty => Global.Settings.DeathCare.AutoEmpty;

                /// <summary>
                /// Gets the type of the dispatcher.
                /// </summary>
                /// <value>
                /// The type of the dispatcher.
                /// </value>
                public Dispatcher.DispatcherTypes DispatcherType => Dispatcher.DispatcherTypes.HearseDispatcher;

                /// <summary>
                /// Gets a value indicating whether to dispatch vehicles.
                /// </summary>
                /// <value>
                ///   <c>true</c> if dispatching vehicles; otherwise, <c>false</c>.
                /// </value>
                public bool DispatchVehicles => Global.Settings.DeathCare.DispatchVehicles;

                /// <summary>
                /// Gets the service category.
                /// </summary>
                /// <value>
                /// The service category.
                /// </value>
                public string ServiceCategory => "DeathCare";

                /// <summary>
                /// Gets the service log pre/suffix.
                /// </summary>
                /// <value>
                /// The service log pre/suffix.
                /// </value>
                public string ServiceLogFix => "SBDC";

                /// <summary>
                /// Gets the target category.
                /// </summary>
                /// <value>
                /// The target category.
                /// </value>
                public string TargetCategory => "DeadPeople";

                /// <summary>
                /// Gets the target log pre/suffix.
                /// </summary>
                /// <value>
                /// The target log pre/suffix.
                /// </value>
                public string TargetLogFix => "TBDP";

                /// <summary>
                /// Checks if removal of target building should be delayed.
                /// </summary>
                /// <param name="buildingId">The building identifier.</param>
                /// <param name="building">The building.</param>
                /// <returns>
                /// True if target buidling should not yet be removed.
                /// </returns>
                public bool DelayTargetBuildingRemoval(ushort buildingId, ref Building building)
                {
                    return this.TargetBuildings[buildingId].WantedService;
                }

                /// <summary>
                /// Determines whether the specified building is a service building for this service.
                /// </summary>
                /// <param name="buildingId">The building identifier.</param>
                /// <param name="building">The building.</param>
                /// <returns>
                ///   <c>true</c> if the specified building is a service building; otherwise, <c>false</c>.
                /// </returns>
                public bool DispatchFromBuilding(ushort buildingId, ref Building building)
                {
                    return building.Info.m_buildingAI is CemeteryAI;
                }

                /// <summary>
                /// Determines whether the specified building is a a building this service is willing to dispatch to.
                /// </summary>
                /// <param name="buildingId">The building identifier.</param>
                /// <param name="building">The building.</param>
                /// <returns>
                ///   <c>true</c> if the specified building can be dispatched to; otherwise, <c>false</c>.
                /// </returns>
                public bool DispatchToBuilding(ushort buildingId, ref Building building)
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
                public TargetBuildingInfo.ServiceDemand GetTargetBuildingDemand(ushort buildingId, ref Building building)
                {
                    return (building.m_deathProblemTimer > 0) ? TargetBuildingInfo.ServiceDemand.NeedsService : TargetBuildingInfo.ServiceDemand.None;
                }

                /// <summary>
                /// Re-initialize the part.
                /// </summary>
                public void ReInitialize()
                {
                    this.Initialize(false, this.DispatchVehicles, this.AutoEmpty);
                }
            }

            /// <summary>
            /// Garbage service keeper.
            /// </summary>
            /// <seealso cref="WhatThe.Mods.CitiesSkylines.ServiceDispatcher.DispatchServiceKeeper.DispatchService" />
            /// <seealso cref="WhatThe.Mods.CitiesSkylines.ServiceDispatcher.IDispatchService" />
            public class Garbage : DispatchService, IDispatchService
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="DeathCare"/> class.
                /// </summary>
                public Garbage() : base()
                {
                    this.Initialize(true, this.DispatchVehicles, this.AutoEmpty);
                }

                /// <summary>
                /// Gets a value indicating whether to empty automatically.
                /// </summary>
                /// <value>
                ///   <c>true</c> if automatic emptying; otherwise, <c>false</c>.
                /// </value>
                public bool AutoEmpty => Global.Settings.Garbage.AutoEmpty;

                /// <summary>
                /// Gets the type of the dispatcher.
                /// </summary>
                /// <value>
                /// The type of the dispatcher.
                /// </value>
                public Dispatcher.DispatcherTypes DispatcherType => Dispatcher.DispatcherTypes.GarbageTruckDispatcher;

                /// <summary>
                /// Gets a value indicating whether to dispatch vehicles.
                /// </summary>
                /// <value>
                ///   <c>true</c> if dispatching vehicles; otherwise, <c>false</c>.
                /// </value>
                public bool DispatchVehicles => Global.Settings.Garbage.DispatchVehicles;

                /// <summary>
                /// Gets the service category.
                /// </summary>
                /// <value>
                /// The service category.
                /// </value>
                public string ServiceCategory => "Garbage";

                /// <summary>
                /// Gets the service log pre/suffix.
                /// </summary>
                /// <value>
                /// The service log pre/suffix.
                /// </value>
                public string ServiceLogFix => "SBG";

                /// <summary>
                /// Gets the target category.
                /// </summary>
                /// <value>
                /// The target category.
                /// </value>
                public string TargetCategory => "Dirty";

                /// <summary>
                /// Gets the target log pre/suffix.
                /// </summary>
                /// <value>
                /// The target log pre/suffix.
                /// </value>
                public string TargetLogFix => "TBD";

                /// <summary>
                /// Checks if removal of target building should be delayed.
                /// </summary>
                /// <param name="buildingId">The building identifier.</param>
                /// <param name="building">The building.</param>
                /// <returns>
                /// True if target buidling should not yet be removed.
                /// </returns>
                public bool DelayTargetBuildingRemoval(ushort buildingId, ref Building building)
                {
                    return this.TargetBuildings[buildingId].WantedService;
                }

                /// <summary>
                /// Determines whether the specified building is a service building for this service.
                /// </summary>
                /// <param name="buildingId">The building identifier.</param>
                /// <param name="building">The building.</param>
                /// <returns>
                ///   <c>true</c> if the specified building is a service building; otherwise, <c>false</c>.
                /// </returns>
                public bool DispatchFromBuilding(ushort buildingId, ref Building building)
                {
                    return building.Info.m_buildingAI is LandfillSiteAI;
                }

                /// <summary>
                /// Determines whether the specified building is a a building this service is willing to dispatch to.
                /// </summary>
                /// <param name="buildingId">The building identifier.</param>
                /// <param name="building">The building.</param>
                /// <returns>
                ///   <c>true</c> if the specified building can be dispatched to; otherwise, <c>false</c>.
                /// </returns>
                public bool DispatchToBuilding(ushort buildingId, ref Building building)
                {
                    return !(building.Info.m_buildingAI is LandfillSiteAI);
                }

                /// <summary>
                /// Gets the target building demand.
                /// </summary>
                /// <param name="buildingId">The building identifier.</param>
                /// <param name="building">The building.</param>
                /// <returns>
                /// The demand.
                /// </returns>
                public TargetBuildingInfo.ServiceDemand GetTargetBuildingDemand(ushort buildingId, ref Building building)
                {
                    if (building.m_garbageBuffer >= Global.Settings.Garbage.MinimumAmountForDispatch)
                    {
                        return TargetBuildingInfo.ServiceDemand.NeedsService;
                    }
                    else if (building.m_garbageBuffer >= Global.Settings.Garbage.MinimumAmountForPatrol)
                    {
                        return TargetBuildingInfo.ServiceDemand.WantsService;
                    }
                    else
                    {
                        return TargetBuildingInfo.ServiceDemand.None;
                    }
                }

                /// <summary>
                /// Re-initialize the part.
                /// </summary>
                public void ReInitialize()
                {
                    this.Initialize(false, this.DispatchVehicles, this.AutoEmpty);
                }
            }

            /// <summary>
            /// Health care service keeper.
            /// </summary>
            /// <seealso cref="WhatThe.Mods.CitiesSkylines.ServiceDispatcher.DispatchServiceKeeper.DispatchService" />
            /// <seealso cref="WhatThe.Mods.CitiesSkylines.ServiceDispatcher.IDispatchService" />
            public class HealthCare : DispatchService, IDispatchService
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="DeathCare"/> class.
                /// </summary>
                public HealthCare() : base()
                {
                    this.Initialize(true, this.DispatchVehicles, this.AutoEmpty);
                }

                /// <summary>
                /// Gets a value indicating whether to empty automatically.
                /// </summary>
                /// <value>
                ///   <c>true</c> if automatic emptying; otherwise, <c>false</c>.
                /// </value>
                public bool AutoEmpty => Global.Settings.HealthCare.AutoEmpty;

                /// <summary>
                /// Gets the type of the dispatcher.
                /// </summary>
                /// <value>
                /// The type of the dispatcher.
                /// </value>
                public Dispatcher.DispatcherTypes DispatcherType => Dispatcher.DispatcherTypes.AmbulanceDispatcher;

                /// <summary>
                /// Gets a value indicating whether to dispatch vehicles.
                /// </summary>
                /// <value>
                ///   <c>true</c> if dispatching vehicles; otherwise, <c>false</c>.
                /// </value>
                public bool DispatchVehicles => Global.Settings.HealthCare.DispatchVehicles;

                /// <summary>
                /// Gets the service category.
                /// </summary>
                /// <value>
                /// The service category.
                /// </value>
                public string ServiceCategory => "HealthCare";

                /// <summary>
                /// Gets the service log pre/suffix.
                /// </summary>
                /// <value>
                /// The service log pre/suffix.
                /// </value>
                public string ServiceLogFix => "SBHC";

                /// <summary>
                /// Gets the target category.
                /// </summary>
                /// <value>
                /// The target category.
                /// </value>
                public string TargetCategory => "SickPeople";

                /// <summary>
                /// Gets the target log pre/suffix.
                /// </summary>
                /// <value>
                /// The target log pre/suffix.
                /// </value>
                public string TargetLogFix => "TBSP";

                /// <summary>
                /// Checks if removal of target building should be delayed.
                /// </summary>
                /// <param name="buildingId">The building identifier.</param>
                /// <param name="building">The building.</param>
                /// <returns>
                /// True if target buidling should not yet be removed.
                /// </returns>
                public bool DelayTargetBuildingRemoval(ushort buildingId, ref Building building)
                {
                    return ((building.m_garbageBuffer > 10 &&
                            (building.m_garbageBuffer >= Global.Settings.Garbage.MinimumAmountForDispatch / 10 ||
                             building.m_garbageBuffer >= Global.Settings.Garbage.MinimumAmountForPatrol / 2)) ||
                            this.TargetBuildings[buildingId].WantedService);
                }

                /// <summary>
                /// Determines whether the specified building is a service building for this service.
                /// </summary>
                /// <param name="buildingId">The building identifier.</param>
                /// <param name="building">The building.</param>
                /// <returns>
                ///   <c>true</c> if the specified building is a service building; otherwise, <c>false</c>.
                /// </returns>
                public bool DispatchFromBuilding(ushort buildingId, ref Building building)
                {
                    return building.Info.m_buildingAI is HospitalAI;
                }

                /// <summary>
                /// Determines whether the specified building is a a building this service is willing to dispatch to.
                /// </summary>
                /// <param name="buildingId">The building identifier.</param>
                /// <param name="building">The building.</param>
                /// <returns>
                ///   <c>true</c> if the specified building can be dispatched to; otherwise, <c>false</c>.
                /// </returns>
                public bool DispatchToBuilding(ushort buildingId, ref Building building)
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
                public TargetBuildingInfo.ServiceDemand GetTargetBuildingDemand(ushort buildingId, ref Building building)
                {
                    if (building.m_garbageBuffer >= Global.Settings.Garbage.MinimumAmountForDispatch)
                    {
                        return TargetBuildingInfo.ServiceDemand.NeedsService;
                    }
                    else if (building.m_garbageBuffer >= Global.Settings.Garbage.MinimumAmountForPatrol)
                    {
                        return TargetBuildingInfo.ServiceDemand.WantsService;
                    }
                    else
                    {
                        return TargetBuildingInfo.ServiceDemand.None;
                    }
                }

                /// <summary>
                /// Re-initialize the part.
                /// </summary>
                public void ReInitialize()
                {
                    this.Initialize(false, this.DispatchVehicles, this.AutoEmpty);
                }
            }
        }
    }
}