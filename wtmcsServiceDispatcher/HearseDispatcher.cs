using System.Collections.Generic;

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
                return Global.Buildings.HasDeadPeopleBuildingsToCheck;
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
                return Global.Buildings.HearseBuildings;
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
                return Global.Buildings.DeadPeopleBuildings;
            }
        }

        /// <summary>
        /// Determines whether vehicle is correct type of vehicle.
        /// </summary>
        /// <param name="vehicleInfo">The vehicle information.</param>
        /// <returns>True if vehicle is correct type.</returns>
        protected override bool IsMyType(VehicleInfo vehicleInfo)
        {
            return (vehicleInfo != null && vehicleInfo.m_vehicleAI is HearseAI);
        }
    }
}