namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Dispatches hearses to buildings with dead people.
    /// </summary>
    internal class HearseDispatcher : Dispatcher
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HearseDispatcher"/> class.
        /// </summary>
        public HearseDispatcher()
            : base()
        {
            transferType = (byte)TransferManager.TransferReason.Dead;
            serviceBuildings = Global.Buildings.HearseBuildings;
            TargetBuildings = Global.Buildings.DeadPeopleBuildings;
            Log.Debug(this, "Constructed");
        }

        /// <summary>
        /// Gets the building check paramaters.
        /// </summary>
        /// <value>
        /// The building check paramaters.
        /// </value>
        protected override BuldingCheckParameters[] BuildingCheckParamaters
        {
            get
            {
                return BuldingCheckParameters.GetBuldingCheckParameters(Global.Settings.DeathChecksParameters);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this service has target buildings.
        /// </summary>
        /// <value>
        /// <c>true</c> if this service has target buildings; otherwise, <c>false</c>.
        /// </value>
        protected override bool HasTargetBuildings
        {
            get
            {
                return Global.Buildings.HasDeadPeopleBuildingsToCheck;
            }
        }
    }
}