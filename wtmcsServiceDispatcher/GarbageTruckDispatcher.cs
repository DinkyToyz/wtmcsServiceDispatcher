using System.Collections.Generic;

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
            IsPretending = true;
            Log.Debug(this, "Constructed");
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

        /// <summary>
        /// Gets the service buildings.
        /// </summary>
        /// <value>
        /// The service buildings.
        /// </value>
        protected override IEnumerable<Buildings.ServiceBuildingInfo> ServiceBuildings
        {
            get
            {
                return Global.Buildings.GarbageBuildings;
            }
        }

        /// <summary>
        /// Gets the target buildings.
        /// </summary>
        /// <value>
        /// The target buildings.
        /// </value>
        protected override IEnumerable<Buildings.TargetBuildingInfo> TargetBuildings
        {
            get
            {
                return Global.Buildings.DirtyBuildings;
            }
        }

        /// <summary>
        /// Set target with the vehicles AI.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="buildingId">The building identifier.</param>
        protected override void AISetTarget(ushort vehicleId, ref Vehicle vehicle, ushort buildingId)
        {
            ((GarbageTruckAI)(vehicle.Info.m_vehicleAI)).SetTarget(vehicleId, ref vehicle, buildingId);
        }

        /// <summary>
        /// Determines whether vehicle is correct type of vehicle.
        /// </summary>
        /// <param name="vehicleInfo">The vehicle information.</param>
        /// <returns>True if vehicle is correct type.</returns>
        protected override bool IsMyType(VehicleInfo vehicleInfo)
        {
            return (vehicleInfo != null && vehicleInfo.m_vehicleAI is GarbageTruckAI);
        }
    }
}