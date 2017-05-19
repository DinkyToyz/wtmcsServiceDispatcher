namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.ObjectHelpers
{
    /// <summary>
    /// Helper for metro trains.
    /// </summary>
    internal static class MetroTrainHelper
    {
        /// <summary>
        /// Check if metro is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if metro is confused.</returns>
        public static bool IsConfused(ref Vehicle data)
        {
            // From MetroTrainAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.Stopped) != ~VehicleHelper.VehicleAll)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_METRO_STOPPED");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.GoingBack) != ~VehicleHelper.VehicleAll)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_METRO_RETURN");
                return false;
            }
            if ((int)data.m_transportLine != 0)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_METRO_ROUTE");
                return false;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }
    }
}