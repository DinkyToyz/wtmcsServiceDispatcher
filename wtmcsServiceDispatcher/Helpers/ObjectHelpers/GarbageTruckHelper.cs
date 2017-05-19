namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.ObjectHelpers
{
    /// <summary>
    /// Helper for garbage trucks.
    /// </summary>
    internal static class GarbageTruckHelper
    {
        /// <summary>
        /// Check if garbage truck is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if garbage truck is confused.</returns>
        public static bool IsConfused(ref Vehicle data)
        {
            // From GarbageTruckAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.TransferToSource) != ~VehicleHelper.VehicleAll)
            {
                ////if ((data.m_flags & Vehicle.Flags.GoingBack) != ~Vehicle.Flags.All)
                ////{
                ////    target = InstanceID.Empty;
                ////    return Locale.Get("VEHICLE_STATUS_GARBAGE_RETURN");
                ////}
                ////if ((data.m_flags & Vehicle.Flags.WaitingTarget) != ~Vehicle.Flags.All)
                ////{
                ////    target = InstanceID.Empty;
                ////    return Locale.Get("VEHICLE_STATUS_GARBAGE_WAIT");
                ////}
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_GARBAGE_COLLECT");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.TransferToTarget) != ~VehicleHelper.VehicleAll)
            {
                if ((data.m_flags & Vehicle.Flags.GoingBack) != ~VehicleHelper.VehicleAll)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_GARBAGE_RETURN");
                    return false;
                }
                if ((data.m_flags & Vehicle.Flags.WaitingTarget) != ~VehicleHelper.VehicleAll)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_GARBAGE_UNLOAD");
                    return false;
                }
                if ((int)data.m_targetBuilding != 0)
                {
                    ////target = InstanceID.Empty;
                    ////target.Building = data.m_targetBuilding;
                    ////return Locale.Get("VEHICLE_STATUS_GARBAGE_TRANSFER");
                    return false;
                }
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }
    }
}