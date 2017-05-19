namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.ObjectHelpers
{
    /// <summary>
    /// Helper for police cars.
    /// </summary>
    internal static class PoliceCarHelper
    {
        /// <summary>
        /// Check if police car is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if police car is confused.</returns>
        public static bool IsConfused(ref Vehicle data)
        {
            // From PoliceCarAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if (data.Info.m_class.m_level >= ItemClass.Level.Level4)
            {
                if ((data.m_flags & Vehicle.Flags.GoingBack) != ~VehicleHelper.VehicleAll)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_PRISON_RETURN");
                    return false;
                }
                if ((data.m_flags & (Vehicle.Flags.Stopped | Vehicle.Flags.WaitingTarget)) != ~VehicleHelper.VehicleAll)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_PRISON_WAIT");
                    return false;
                }
                if ((int)data.m_targetBuilding != 0)
                {
                    ////target = InstanceID.Empty;
                    ////target.Building = data.m_targetBuilding;
                    ////return Locale.Get("VEHICLE_STATUS_PRISON_PICKINGUP");
                    return false;
                }
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
                return true;
            }
            if ((data.m_flags & Vehicle.Flags.GoingBack) != ~VehicleHelper.VehicleAll)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_POLICE_RETURN");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.Stopped) != ~VehicleHelper.VehicleAll)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_POLICE_STOPPED");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.WaitingTarget) != ~VehicleHelper.VehicleAll)
            {
                ////if ((data.m_flags & Vehicle.Flags.Leaving) != ~Vehicle.Flags.All)
                ////{
                ////    target = InstanceID.Empty;
                ////    return Locale.Get("VEHICLE_STATUS_POLICE_STOP_WAIT");
                ////}
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_POLICE_PATROL_WAIT");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.Emergency2) != ~VehicleHelper.VehicleAll)
            {
                if ((int)data.m_targetBuilding != 0)
                {
                    ////target = InstanceID.Empty;
                    ////target.Building = data.m_targetBuilding;
                    ////return Locale.Get("VEHICLE_STATUS_POLICE_EMERGENCY");
                    return false;
                }
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
                return true;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_POLICE_PATROL");
            return false;
        }
    }
}