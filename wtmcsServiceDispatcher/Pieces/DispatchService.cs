using System.Collections.Generic;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Base class for global dispatch service data.
    /// </summary>
    internal abstract class DispatchService : IHandlerPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DispatchService"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public DispatchService(StandardServiceSettings settings)
        {
            this.Settings = settings;
            this.BuildingsCanEmptyOther = 0;
            this.BuildingsEmptying = 0;
            this.HasTargetBuildingsToCheck = false;
            this.BuildingsInNeedOfEmptyingChange = null;
            this.ServiceBuildings = null;
            this.TargetBuildings = null;
            this.Dispatcher = null;
        }

        /// <summary>
        /// Gets a value indicating whether to empty automatically.
        /// </summary>
        /// <value>
        ///   <c>true</c> if automatic emptying; otherwise, <c>false</c>.
        /// </value>
        public bool AutoEmpty => Settings.AutoEmpty;

        /// <summary>
        /// The buildings-can-empty-other counter.
        /// </summary>
        public uint BuildingsCanEmptyOther { get; set; }

        /// <summary>
        /// The buildings-emptying counter.
        /// </summary>
        public uint BuildingsEmptying { get; set; }

        /// <summary>
        /// The buiildings in need of emptying change.
        /// </summary>
        public List<ServiceBuildingInfo> BuildingsInNeedOfEmptyingChange { get; private set; }

        /// <summary>
        /// Gets the dispatcher.
        /// </summary>
        /// <value>
        /// The dispatcher.
        /// </value>
        public Dispatcher Dispatcher { get; private set; }

        /// <summary>
        /// Gets the type of the dispatcher.
        /// </summary>
        /// <value>
        /// The type of the dispatcher.
        /// </value>
        public abstract Dispatcher.DispatcherTypes DispatcherType { get; }

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
        /// Gets a value indicating whether this instance has target buildings that should be checked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has target buildings to check; otherwise, <c>false</c>.
        /// </value>
        public bool HasTargetBuildingsToCheck { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is auto-emptying.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is auto-emptying; otherwise, <c>false</c>.
        /// </value>
        public bool IsAutoEmptying => this.Settings.AutoEmpty && this.ServiceBuildings != null && this.BuildingsInNeedOfEmptyingChange != null;

        /// <summary>
        /// Gets a value indicating whether this instance is dispatching.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is dispatching; otherwise, <c>false</c>.
        /// </value>
        public bool IsDispatching => this.Settings.DispatchVehicles && this.Dispatcher != null && this.ServiceBuildings != null && this.TargetBuildings != null;

        /// <summary>
        /// Gets the service buildings.
        /// </summary>
        public Dictionary<ushort, ServiceBuildingInfo> ServiceBuildings { get; private set; }

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
        /// Gets the settings.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        public StandardServiceSettings Settings { get; private set; }

        /// <summary>
        /// Gets the target buildings.
        /// </summary>
        public Dictionary<ushort, TargetBuildingInfo> TargetBuildings { get; private set; }

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
        /// Checks if removal of target building should be delayed.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <returns>
        /// True if target buidling should not yet be removed.
        /// </returns>
        public abstract bool DelayTargetBuildingRemoval(ushort buildingId, ref Building building);

        /// <summary>
        /// Determines whether the specified building is a service building for this service.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <returns>
        ///   <c>true</c> if the specified building is a service building; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool DispatchFromBuilding(ushort buildingId, ref Building building);

        /// <summary>
        /// Determines whether the specified building is a a building this service is willing to dispatch to.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <returns>
        ///   <c>true</c> if the specified building can be dispatched to; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool DispatchToBuilding(ushort buildingId, ref Building building);

        /// <summary>
        /// Gets the target building demand.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <returns>
        /// The demand.
        /// </returns>
        public abstract TargetBuildingInfo.ServiceDemand GetTargetBuildingDemand(ushort buildingId, ref Building building);

        /// <summary>
        /// Res the initialize.
        /// </summary>
        public void ReInitialize()
        {
            this.Initialize(false);
        }

        /// <summary>
        /// Initializes the data lists.
        /// </summary>
        /// <param name="constructing">If set to <c>true</c> object is being constructed.</param>
        private void Initialize(bool constructing)
        {
            Log.InfoList info = new Log.InfoList();

            info.Add("Constructing", constructing);
            info.Add("DispatchVehicles", this.Settings.DispatchVehicles);
            info.Add("AutoEmpty", this.Settings.AutoEmpty);

            if (constructing || !this.Settings.DispatchVehicles || this.TargetBuildings == null)
            {
                this.HasTargetBuildingsToCheck = false;

                if (!this.Settings.DispatchVehicles)
                {
                    this.TargetBuildings = null;
                    this.Dispatcher = null;
                }
                else
                {
                    if (constructing || this.TargetBuildings == null)
                    {
                        info.Add("TargetBuildings", "new");
                        this.TargetBuildings = new Dictionary<ushort, TargetBuildingInfo>();
                    }

                    if (constructing || this.Dispatcher == null)
                    {
                        info.Add("Dispatcher", "new");
                        this.Dispatcher = new Dispatcher(this);
                    }
                    else
                    {
                        this.Dispatcher.ReInitialize();
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

            if (constructing || !this.Settings.DispatchVehicles || !this.Settings.AutoEmpty || this.ServiceBuildings == null)
            {
                this.BuildingsCanEmptyOther = 0;
                this.BuildingsEmptying = 0;

                if (!this.Settings.DispatchVehicles && !this.Settings.AutoEmpty)
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
        /// <seealso cref="WhatThe.Mods.CitiesSkylines.ServiceDispatcher.DispatchService" />
        public class DeathCare : DispatchService
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DeathCare"/> class.
            /// </summary>
            public DeathCare() : base(Global.Settings.DeathCare)
            {
                this.Initialize(true);
            }

            /// <summary>
            /// Gets the type of the dispatcher.
            /// </summary>
            /// <value>
            /// The type of the dispatcher.
            /// </value>
            public override Dispatcher.DispatcherTypes DispatcherType => Dispatcher.DispatcherTypes.HearseDispatcher;

            /// <summary>
            /// Gets the service category.
            /// </summary>
            /// <value>
            /// The service category.
            /// </value>
            public override string ServiceCategory => "DeathCare";

            /// <summary>
            /// Gets the service log pre/suffix.
            /// </summary>
            /// <value>
            /// The service log pre/suffix.
            /// </value>
            public override string ServiceLogFix => "SBDC";

            /// <summary>
            /// Gets the target category.
            /// </summary>
            /// <value>
            /// The target category.
            /// </value>
            public override string TargetCategory => "DeadPeople";

            /// <summary>
            /// Gets the target log pre/suffix.
            /// </summary>
            /// <value>
            /// The target log pre/suffix.
            /// </value>
            public override string TargetLogFix => "TBDP";

            /// <summary>
            /// Checks if removal of target building should be delayed.
            /// </summary>
            /// <param name="buildingId">The building identifier.</param>
            /// <param name="building">The building.</param>
            /// <returns>
            /// True if target buidling should not yet be removed.
            /// </returns>
            public override bool DelayTargetBuildingRemoval(ushort buildingId, ref Building building)
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
            public override bool DispatchFromBuilding(ushort buildingId, ref Building building)
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
            public override bool DispatchToBuilding(ushort buildingId, ref Building building)
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
            public override TargetBuildingInfo.ServiceDemand GetTargetBuildingDemand(ushort buildingId, ref Building building)
            {
                return (building.m_deathProblemTimer > 0) ? TargetBuildingInfo.ServiceDemand.NeedsService : TargetBuildingInfo.ServiceDemand.None;
            }
        }

        /// <summary>
        /// Garbage service keeper.
        /// </summary>
        /// <seealso cref="WhatThe.Mods.CitiesSkylines.ServiceDispatcher.DispatchService" />
        public class Garbage : DispatchService
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DeathCare"/> class.
            /// </summary>
            public Garbage() : base(Global.Settings.Garbage)
            {
                this.Initialize(true);
            }

            /// <summary>
            /// Gets the type of the dispatcher.
            /// </summary>
            /// <value>
            /// The type of the dispatcher.
            /// </value>
            public override Dispatcher.DispatcherTypes DispatcherType => Dispatcher.DispatcherTypes.GarbageTruckDispatcher;

            /// <summary>
            /// Gets the service category.
            /// </summary>
            /// <value>
            /// The service category.
            /// </value>
            public override string ServiceCategory => "Garbage";

            /// <summary>
            /// Gets the service log pre/suffix.
            /// </summary>
            /// <value>
            /// The service log pre/suffix.
            /// </value>
            public override string ServiceLogFix => "SBG";

            /// <summary>
            /// Gets the target category.
            /// </summary>
            /// <value>
            /// The target category.
            /// </value>
            public override string TargetCategory => "Dirty";

            /// <summary>
            /// Gets the target log pre/suffix.
            /// </summary>
            /// <value>
            /// The target log pre/suffix.
            /// </value>
            public override string TargetLogFix => "TBD";

            /// <summary>
            /// Checks if removal of target building should be delayed.
            /// </summary>
            /// <param name="buildingId">The building identifier.</param>
            /// <param name="building">The building.</param>
            /// <returns>
            /// True if target buidling should not yet be removed.
            /// </returns>
            public override bool DelayTargetBuildingRemoval(ushort buildingId, ref Building building)
            {
                return ((building.m_garbageBuffer > 10 &&
                        (building.m_garbageBuffer >= this.Settings.MinimumAmountForDispatch / 10 ||
                         building.m_garbageBuffer >= this.Settings.MinimumAmountForPatrol / 2)) ||
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
            public override bool DispatchFromBuilding(ushort buildingId, ref Building building)
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
            public override bool DispatchToBuilding(ushort buildingId, ref Building building)
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
            public override TargetBuildingInfo.ServiceDemand GetTargetBuildingDemand(ushort buildingId, ref Building building)
            {
                if (building.m_garbageBuffer >= this.Settings.MinimumAmountForDispatch)
                {
                    return TargetBuildingInfo.ServiceDemand.NeedsService;
                }
                else if (building.m_garbageBuffer >= this.Settings.MinimumAmountForPatrol)
                {
                    return TargetBuildingInfo.ServiceDemand.WantsService;
                }
                else
                {
                    return TargetBuildingInfo.ServiceDemand.None;
                }
            }
        }

        /// <summary>
        /// Health care service keeper.
        /// </summary>
        /// <seealso cref="WhatThe.Mods.CitiesSkylines.ServiceDispatcher.DispatchService" />
        public class HealthCare : DispatchService
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DeathCare"/> class.
            /// </summary>
            public HealthCare() : base(Global.Settings.HealthCare)
            {
                this.Initialize(true);
            }

            /// <summary>
            /// Gets the type of the dispatcher.
            /// </summary>
            /// <value>
            /// The type of the dispatcher.
            /// </value>
            public override Dispatcher.DispatcherTypes DispatcherType => Dispatcher.DispatcherTypes.AmbulanceDispatcher;

            /// <summary>
            /// Gets the service category.
            /// </summary>
            /// <value>
            /// The service category.
            /// </value>
            public override string ServiceCategory => "HealthCare";

            /// <summary>
            /// Gets the service log pre/suffix.
            /// </summary>
            /// <value>
            /// The service log pre/suffix.
            /// </value>
            public override string ServiceLogFix => "SBHC";

            /// <summary>
            /// Gets the target category.
            /// </summary>
            /// <value>
            /// The target category.
            /// </value>
            public override string TargetCategory => "SickPeople";

            /// <summary>
            /// Gets the target log pre/suffix.
            /// </summary>
            /// <value>
            /// The target log pre/suffix.
            /// </value>
            public override string TargetLogFix => "TBSP";

            /// <summary>
            /// Checks if removal of target building should be delayed.
            /// </summary>
            /// <param name="buildingId">The building identifier.</param>
            /// <param name="building">The building.</param>
            /// <returns>
            /// True if target buidling should not yet be removed.
            /// </returns>
            public override bool DelayTargetBuildingRemoval(ushort buildingId, ref Building building)
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
            public override bool DispatchFromBuilding(ushort buildingId, ref Building building)
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
            public override bool DispatchToBuilding(ushort buildingId, ref Building building)
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
            public override TargetBuildingInfo.ServiceDemand GetTargetBuildingDemand(ushort buildingId, ref Building building)
            {
                return (building.m_healthProblemTimer > 0) ? TargetBuildingInfo.ServiceDemand.NeedsService : TargetBuildingInfo.ServiceDemand.None;
            }
        }
    }
}