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
        /// Gets the type of the dispatcher.
        /// </summary>
        /// <value>
        /// The type of the dispatcher.
        /// </value>
        Dispatcher.DispatcherTypes DispatcherType { get; }

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
        /// Gets the service building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <returns>A service building with the id, or null.</returns>
        ServiceBuildingInfo GetServiceBuilding(ushort buildingId);

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
        bool IsServiceBuilding(ushort buildingId, ref Building building);

        /// <summary>
        /// Re-initialize the part.
        /// </summary>
        void ReInitialize();
    }

    /// <summary>
    /// Keeps track of dispatch services.
    /// </summary>
    internal class DispatchServiceKeeper
    {
        /// <summary>
        /// The services.
        /// </summary>
        private IDispatchService[] Services = new IDispatchService[(int)Dispatcher.DispatcherTypes.None];

        /// <summary>
        /// Gets the <see cref="IDispatchService"/> with the specified dispatcher type.
        /// </summary>
        /// <value>
        /// The <see cref="IDispatchService"/>.
        /// </value>
        /// <param name="DispatcherType">Type of the dispatcher.</param>
        /// <returns>The <see cref="IDispatchService"/>.</returns>
        public IDispatchService this[Dispatcher.DispatcherTypes DispatcherType] => this.Services[(int)DispatcherType];

        private void Initialize<T>(bool constructing, Dispatcher.DispatcherTypes dispatcherType, bool dispatchVehicles, bool autoEmpty) where T : IDispatchService, new()
        {
            if (dispatchVehicles || autoEmpty)
            {
                if (constructing || this.Services[(int)Dispatcher.DispatcherTypes.HearseDispatcher] == null)
                {
                    this.Services[(int)Dispatcher.DispatcherTypes.HearseDispatcher] = new T();
                }
                else
                {
                    this.Services[(int)Dispatcher.DispatcherTypes.HearseDispatcher].ReInitialize();
                }
            }
            else
            {
                this.Services[(int)Dispatcher.DispatcherTypes.HearseDispatcher] = null;
            }
        }

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
            /// Gets the service building.
            /// </summary>
            /// <param name="buildingId">The building identifier.</param>
            /// <returns>A service building with the id, or null.</returns>
            public ServiceBuildingInfo GetServiceBuilding(ushort buildingId)
            {
                ServiceBuildingInfo building;

                if (this.ServiceBuildings != null && this.ServiceBuildings.TryGetValue(buildingId, out building))
                {
                    return building;
                }

                return null;
            }

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
                /// Gets the target category.
                /// </summary>
                /// <value>
                /// The target category.
                /// </value>
                public string TargetCategory => "DeadPeople";

                /// <summary>
                /// Gets the type of the dispatcher.
                /// </summary>
                /// <value>
                /// The type of the dispatcher.
                /// </value>
                public Dispatcher.DispatcherTypes DispatcherType => Dispatcher.DispatcherTypes.HearseDispatcher;

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
                /// Determines whether the specified building is a service building for this service.
                /// </summary>
                /// <param name="buildingId">The building identifier.</param>
                /// <param name="building">The building.</param>
                /// <returns>
                ///   <c>true</c> if the specified building is a service building; otherwise, <c>false</c>.
                /// </returns>
                public bool IsServiceBuilding(ushort buildingId, ref Building building)
                {
                    return building.Info.m_buildingAI is CemeteryAI;
                }

                /// <summary>
                /// Re-initialize the part.
                /// </summary>
                public void ReInitialize()
                {
                    this.Initialize(false, this.DispatchVehicles, this.AutoEmpty);
                }

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
                    return false;
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
                /// Gets the target category.
                /// </summary>
                /// <value>
                /// The target category.
                /// </value>
                public string TargetCategory => "Dirty";

                /// <summary>
                /// Gets the type of the dispatcher.
                /// </summary>
                /// <value>
                /// The type of the dispatcher.
                /// </value>
                public Dispatcher.DispatcherTypes DispatcherType => Dispatcher.DispatcherTypes.GarbageTruckDispatcher;

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
                /// Determines whether the specified building is a service building for this service.
                /// </summary>
                /// <param name="buildingId">The building identifier.</param>
                /// <param name="building">The building.</param>
                /// <returns>
                ///   <c>true</c> if the specified building is a service building; otherwise, <c>false</c>.
                /// </returns>
                public bool IsServiceBuilding(ushort buildingId, ref Building building)
                {
                    return building.Info.m_buildingAI is LandfillSiteAI;
                }

                /// <summary>
                /// Re-initialize the part.
                /// </summary>
                public void ReInitialize()
                {
                    this.Initialize(false, this.DispatchVehicles, this.AutoEmpty);
                }

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
                    return false;
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
                /// Gets the target category.
                /// </summary>
                /// <value>
                /// The target category.
                /// </value>
                public string TargetCategory => "SickPeople";

                /// <summary>
                /// Gets the type of the dispatcher.
                /// </summary>
                /// <value>
                /// The type of the dispatcher.
                /// </value>
                public Dispatcher.DispatcherTypes DispatcherType => Dispatcher.DispatcherTypes.AmbulanceDispatcher;

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
                /// Determines whether the specified building is a service building for this service.
                /// </summary>
                /// <param name="buildingId">The building identifier.</param>
                /// <param name="building">The building.</param>
                /// <returns>
                ///   <c>true</c> if the specified building is a service building; otherwise, <c>false</c>.
                /// </returns>
                public bool IsServiceBuilding(ushort buildingId, ref Building building)
                {
                    return building.Info.m_buildingAI is HospitalAI;
                }

                /// <summary>
                /// Re-initialize the part.
                /// </summary>
                public void ReInitialize()
                {
                    this.Initialize(false, this.DispatchVehicles, this.AutoEmpty);
                }

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
            }
        }
    }
}