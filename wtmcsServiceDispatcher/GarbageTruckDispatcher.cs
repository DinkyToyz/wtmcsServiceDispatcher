namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Dispatches garbage trucks to buildings with garbage.
    /// </summary>
    internal class GarbageTruckDispatcher : Dispatcher
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GarbageTruckDispatcher"/> class.
        /// </summary>
        public GarbageTruckDispatcher()
            : base()
        {
            transferType = (byte)TransferManager.TransferReason.Garbage;
            serviceBuildings = Global.Buildings.GarbageBuildings;
            TargetBuildings = Global.Buildings.DirtyBuildings;
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
                return BuldingCheckParameters.GetBuldingCheckParameters(Global.Settings.GarbageChecksParameters);
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
                return Global.Buildings.HasDirtyBuildingsToCheck;
            }
        }
    }
}