namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.ObjectHelpers
{
    /// <summary>
    /// Helper for cargo ships.
    /// </summary>
    internal static class CargoShipHelper
    {
        /// <summary>
        /// Check if cargo ship is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if cargo ship is confused.</returns>
        public static bool IsConfused(ref Vehicle data)
        {
            // From CargoShipAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.WaitingCargo) != ~VehicleHelper.VehicleAll)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_CARGOSHIP_LOADING");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.GoingBack) != ~VehicleHelper.VehicleAll)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_CARGOTRUCK_RETURN");
                return false;
            }
            if ((int)data.m_targetBuilding != 0)
            {
                ////target = InstanceID.Empty;
                ////target.Building = data.m_targetBuilding;
                ////return Locale.Get("VEHICLE_STATUS_CARGOSHIP_TRANSPORT");
                return false;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }
    }
}