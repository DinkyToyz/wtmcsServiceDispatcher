namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.ObjectHelpers
{
    /// <summary>
    /// Helper for fire trucks.
    /// </summary>
    internal static class FireTruckHelper
    {
        /// <summary>
        /// Check if passenger car is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if passenger car is confused.</returns>
        public static bool IsConfused(ref Vehicle data)
        {
            // From FireTruckAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.GoingBack) != ~VehicleHelper.VehicleAll)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_FIRETRUCK_RETURN");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.WaitingTarget) != ~VehicleHelper.VehicleAll)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_FIRETRUCK_WAIT");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.Emergency2) != ~VehicleHelper.VehicleAll)
            {
                if ((int)data.m_targetBuilding != 0)
                {
                    ////target = InstanceID.Empty;
                    ////target.Building = data.m_targetBuilding;
                    ////return Locale.Get("VEHICLE_STATUS_FIRETRUCK_EMERGENCY");
                }
                return false;
            }
            else if ((data.m_flags & Vehicle.Flags.Emergency1) != ~VehicleHelper.VehicleAll && (int)data.m_targetBuilding != 0)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_FIRETRUCK_EXTINGUISH");
                return false;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }
    }
}