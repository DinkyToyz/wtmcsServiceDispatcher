namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.ObjectHelpers
{
    /// <summary>
    /// Helper for ambulances.
    /// </summary>
    internal static class AmbulanceHelper
    {
        /// <summary>
        /// Check if ambulance is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if ambulance is confused.</returns>
        public static bool IsConfused(ref Vehicle data)
        {
            // From AmbulanceAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.GoingBack) != ~VehicleHelper.VehicleAll)
            {
                ////if ((int)data.m_transferSize == 0)
                ////{
                ////    target = InstanceID.Empty;
                ////    return Locale.Get("VEHICLE_STATUS_AMBULANCE_RETURN_EMPTY");
                ////}
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_AMBULANCE_RETURN_FULL");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.WaitingTarget) != ~VehicleHelper.VehicleAll)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_AMBULANCE_WAIT");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.Emergency2) != ~VehicleHelper.VehicleAll && (int)data.m_targetBuilding != 0)
            {
                ////target = InstanceID.Empty;
                ////target.Building = data.m_targetBuilding;
                ////return Locale.Get("VEHICLE_STATUS_AMBULANCE_EMERGENCY");
                return false;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }
    }
}