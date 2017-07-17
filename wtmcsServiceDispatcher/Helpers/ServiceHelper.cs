namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Service stuff.
    /// </summary>
    internal class ServiceHelper
    {
        /// <summary>
        /// The service types.
        /// </summary>
        public enum ServiceType
        {
            /// <summary>
            /// Dispatches hearses.
            /// </summary>
            HearseDispatcher = 0,

            /// <summary>
            /// Dispatches garbage trucks.
            /// </summary>
            GarbageTruckDispatcher = 1,

            /// <summary>
            /// Dispatches ambulances.
            /// </summary>
            AmbulanceDispatcher = 2,

            /// <summary>
            /// Dispatches wrecking crews.
            /// </summary>
            BulldozerDispatcher = 3,

            /// <summary>
            /// Dispatches recovery crews.
            /// </summary>
            RecoveryCrewDispatcher = 4,

            /// <summary>
            /// Removes vehicles from grid.
            /// </summary>
            Unblocker = 5,

            /// <summary>
            /// Not a dispatcher.
            /// </summary>
            None = 6
        }

        /// <summary>
        /// Gets the type of the service.
        /// </summary>
        /// <param name="transferReason">The transfer reason.</param>
        /// <returns>The service type.</returns>
        public static ServiceType GetServiceType(TransferManager.TransferReason transferReason)
        {
            switch (transferReason)
            {
                case TransferManager.TransferReason.Dead:
                    return ServiceType.HearseDispatcher;

                case TransferManager.TransferReason.Garbage:
                    return ServiceType.GarbageTruckDispatcher;

                case TransferManager.TransferReason.Sick:
                    return ServiceType.AmbulanceDispatcher;

                default:
                    return ServiceType.None;
            }
        }

        /// <summary>
        /// Gets the type of the service.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>The service type.</returns>
        public static ServiceType GetServiceType(ref Vehicle vehicle)
        {
            if (vehicle.Info.m_vehicleAI is HearseAI)
            {
                return ServiceType.HearseDispatcher;
            }
            else if (vehicle.Info.m_vehicleAI is GarbageTruckAI)
            {
                return ServiceType.GarbageTruckDispatcher;
            }
            else if (vehicle.Info.m_vehicleAI is AmbulanceAI)
            {
                return ServiceType.AmbulanceDispatcher;
            }
            else
            {
                return ServiceType.None;
            }
        }
    }
}