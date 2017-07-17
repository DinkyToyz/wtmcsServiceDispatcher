namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.Services
{
    /// <summary>
    /// Garbage service.
    /// </summary>
    /// <seealso cref="WhatThe.Mods.CitiesSkylines.ServiceDispatcher.Services.DispatchService" />
    internal class Garbage : DispatchService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Garbage"/> class.
        /// </summary>
        public Garbage() : base(ServiceHelper.ServiceType.GarbageTruckDispatcher, (byte)TransferManager.TransferReason.Garbage, Global.Settings.Garbage)
        { }

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
        protected override bool DelayTargetBuildingRemoval(ushort buildingId, ref Building building)
        {
            return ((building.m_garbageBuffer > 10 &&
                    (building.m_garbageBuffer >= this.Settings.MinimumAmountForDispatch / 10 ||
                     building.m_garbageBuffer >= this.Settings.MinimumAmountForPatrol / 2)) ||
                     base.DelayTargetBuildingRemoval(buildingId, ref building));
        }

        /// <summary>
        /// Determines whether the specified building is a service building for this service.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <returns>
        ///   <c>true</c> if the specified building is a service building; otherwise, <c>false</c>.
        /// </returns>
        protected override bool DispatchFromBuilding(ushort buildingId, ref Building building)
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
        protected override bool DispatchToBuilding(ushort buildingId, ref Building building)
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
        protected override TargetBuildingInfo.ServiceDemand GetTargetBuildingDemand(ushort buildingId, ref Building building)
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
}