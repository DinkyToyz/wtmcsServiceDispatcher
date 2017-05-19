namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.ObjectHelpers
{
    /// <summary>
    /// Helper for tourists.
    /// </summary>
    internal static class TramHelper
    {
        /// <summary>
        /// Check if tram is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if tram is confused.</returns>
        public static bool IsConfused(ref Vehicle data)
        {
            // From TramAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.Stopped) != ~VehicleHelper.VehicleAll)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_TRAM_STOPPED");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.GoingBack) != ~VehicleHelper.VehicleAll)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_TRAM_RETURN");
                return false;
            }
            if ((int)data.m_transportLine != 0)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_TRAM_ROUTE");
                return false;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }
    }
}