namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.ObjectHelpers
{
    /// <summary>
    /// Helper for hearses.
    /// </summary>
    internal static class HearseHelper
    {
        /// <summary>
        /// Check if hearse is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if hearse is confused.</returns>
        public static bool IsConfused(ref Vehicle data)
        {
            // From HearseAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.TransferToSource) != ~VehicleHelper.VehicleAll)
            {
                if ((data.m_flags & (Vehicle.Flags.Stopped | Vehicle.Flags.WaitingTarget)) != ~VehicleHelper.VehicleAll)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_HEARSE_WAIT");
                    return false;
                }
                if ((data.m_flags & Vehicle.Flags.GoingBack) != ~VehicleHelper.VehicleAll)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_HEARSE_RETURN");
                    return false;
                }
                if ((int)data.m_targetBuilding != 0)
                {
                    ////target = InstanceID.Empty;
                    ////target.Building = data.m_targetBuilding;
                    ////return Locale.Get("VEHICLE_STATUS_HEARSE_COLLECT");
                    return false;
                }
            }
            else if ((data.m_flags & Vehicle.Flags.TransferToTarget) != ~VehicleHelper.VehicleAll)
            {
                if ((data.m_flags & Vehicle.Flags.GoingBack) != ~VehicleHelper.VehicleAll)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_HEARSE_RETURN");
                    return false;
                }
                if ((data.m_flags & Vehicle.Flags.WaitingTarget) != ~VehicleHelper.VehicleAll)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_HEARSE_UNLOAD");
                    return false;
                }
                if ((int)data.m_targetBuilding != 0)
                {
                    ////target = InstanceID.Empty;
                    ////target.Building = data.m_targetBuilding;
                    ////return Locale.Get("VEHICLE_STATUS_HEARSE_TRANSFER");
                    return false;
                }
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }
    }
}