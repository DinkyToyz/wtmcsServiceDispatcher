namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    internal interface IVehicleInfo
    {
        /// <summary>
        /// Gets the vehicle identifier.
        /// </summary>
        /// <value>
        /// The vehicle identifier.
        /// </value>
        ushort VehicleId { get; }

        /// <summary>
        /// Adds debug information data to information list.
        /// </summary>
        /// <param name="info">The information list.</param>
        void AddDebugInfoData(Log.InfoList info);
    }
}