namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.Services
{
    /// <summary>
    /// Health care service.
    /// </summary>
    /// <seealso cref="WhatThe.Mods.CitiesSkylines.ServiceDispatcher.Services.DispatchService" />
    internal class HealthCare : DispatchService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HealthCare"/> class.
        /// </summary>
        public HealthCare() : base(ServiceHelper.ServiceType.AmbulanceDispatcher, (byte)TransferManager.TransferReason.Sick, Global.Settings.HealthCare)
        { }

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
        /// Determines whether the specified building is a service building for this service.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <returns>
        ///   <c>true</c> if the specified building is a service building; otherwise, <c>false</c>.
        /// </returns>
        protected override bool DispatchFromBuilding(ushort buildingId, ref Building building)
        {
            return building.Info.m_buildingAI is HospitalAI;
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
            return (building.m_healthProblemTimer > 0) ? TargetBuildingInfo.ServiceDemand.NeedsService : TargetBuildingInfo.ServiceDemand.None;
        }
    }
}