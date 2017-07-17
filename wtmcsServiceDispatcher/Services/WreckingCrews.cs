namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.Services
{
    /// <summary>
    /// Wrecking crew service handler.
    /// </summary>
    /// <seealso cref="WhatThe.Mods.CitiesSkylines.ServiceDispatcher.Services.HiddenBuildingService" />
    internal class WreckingCrews : HiddenBuildingService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WreckingCrews"/> class.
        /// </summary>
        public WreckingCrews() : base(ServiceHelper.ServiceType.BulldozerDispatcher, Global.Settings.WreckingCrews)
        { }

        /// <summary>
        /// Gets the service category.
        /// </summary>
        /// <value>
        /// The service category.
        /// </value>
        public override string ServiceCategory => "WreckingCrews";

        /// <summary>
        /// Gets the service log pre/suffix.
        /// </summary>
        /// <value>
        /// The service log pre/suffix.
        /// </value>
        public override string ServiceLogFix => "HWC";

        /// <summary>
        /// Gets the target category.
        /// </summary>
        /// <value>
        /// The target category.
        /// </value>
        public override string TargetCategory => "DesolateBuildings";

        /// <summary>
        /// Gets the target log pre/suffix.
        /// </summary>
        /// <value>
        /// The target log pre/suffix.
        /// </value>
        public override string TargetLogFix => "TBHD";

        /// <summary>
        /// Gets a value indicating whether this instance can dispatch.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can dispatch; otherwise, <c>false</c>.
        /// </value>
        protected override bool CanDispatch => BulldozeHelper.CanBulldoze;

        /// <summary>
        /// Check if dipatcher Dispatches for building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <returns>
        /// True if building needs to be handled.
        /// </returns>
        protected override bool GetBuildingNeed(ushort buildingId, ref Building building)
        {
            return (building.m_flags & (Building.Flags.Abandoned | Building.Flags.BurnedDown)) != Building.Flags.None;
        }

        /// <summary>
        /// Handles the building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        protected override void HandleBuilding(ushort buildingId, ref Building building)
        {
            BulldozeHelper.BulldozeBuilding(buildingId);
        }
    }
}