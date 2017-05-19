using ColossalFramework;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.ObjectHelpers
{
    /// <summary>
    /// Helper for tourists.
    /// </summary>
    internal static class TouristHelper
    {
        /// <summary>
        /// Check if tourist is confused.
        /// </summary>
        /// <param name="data">The citizen instance.</param>
        /// <returns>True if tourist is confused.</returns>
        public static bool IsConfused(ref CitizenInstance data)
        {
            // From TouristAI.GetLocalizedStatus from original game code at version 1.4.1-f2.
            ////CitizenManager instance1 = Singleton<CitizenManager>.instance;
            ////uint num1 = data.m_citizen;
            ////ushort vehicleID = (ushort)0;
            ////if ((int)num1 != 0)
            ////    vehicleID = instance1.m_citizens.m_buffer[num1].m_vehicle;
            ushort num2 = data.m_targetBuilding;
            if ((int)num2 != 0)
            {
                ////bool flag1 = (Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)num2].m_flags & Building.Flags.IncomingOutgoing) != Building.Flags.None;
                ////bool flag2 = (int)data.m_path == 0 && (data.m_flags & CitizenInstance.Flags.HangAround) != CitizenInstance.Flags.None;
                ////if ((int)vehicleID != 0)
                ////{
                ////    VehicleManager instance2 = Singleton<VehicleManager>.instance;
                ////    VehicleInfo info = instance2.m_vehicles.m_buffer[(int)vehicleID].Info;
                ////    if (info.m_class.m_service == ItemClass.Service.Residential && info.m_vehicleType != VehicleInfo.VehicleType.Bicycle)
                ////    {
                ////        if ((int)info.m_vehicleAI.GetOwnerID(vehicleID, ref instance2.m_vehicles.m_buffer[(int)vehicleID]).Citizen == (int)num1)
                ////        {
                ////            if (flag1)
                ////            {
                ////                target = InstanceID.Empty;
                ////                return Locale.Get("CITIZEN_STATUS_DRIVINGTO_OUTSIDE");
                ////            }
                ////            target = InstanceID.Empty;
                ////            target.Building = num2;
                ////            return Locale.Get("CITIZEN_STATUS_DRIVINGTO");
                ////        }
                ////    }
                ////    else if (info.m_class.m_service == ItemClass.Service.PublicTransport)
                ////    {
                ////        if (flag1)
                ////        {
                ////            target = InstanceID.Empty;
                ////            return Locale.Get("CITIZEN_STATUS_TRAVELLINGTO_OUTSIDE");
                ////        }
                ////        target = InstanceID.Empty;
                ////        target.Building = num2;
                ////        return Locale.Get("CITIZEN_STATUS_TRAVELLINGTO");
                ////    }
                ////}
                ////if (flag1)
                ////{
                ////    target = InstanceID.Empty;
                ////    return Locale.Get("CITIZEN_STATUS_GOINGTO_OUTSIDE");
                ////}
                ////if (flag2)
                ////{
                ////    target = InstanceID.Empty;
                ////    target.Building = num2;
                ////    return Locale.Get("CITIZEN_STATUS_VISITING");
                ////}
                ////target = InstanceID.Empty;
                ////target.Building = num2;
                ////return Locale.Get("CITIZEN_STATUS_GOINGTO");
                return false;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("CITIZEN_STATUS_CONFUSED");
            return true;
        }

        /// <summary>
        /// Check if tourist is confused.
        /// </summary>
        /// <param name="data">The citizen.</param>
        /// <returns>True if tourist is confused.</returns>
        public static bool IsConfused(ref Citizen data)
        {
            // From TouristAI.GetLocalizedStatus from original game code at version 1.4.1-f2.
            CitizenManager instance = Singleton<CitizenManager>.instance;
            ushort instanceID = data.m_instance;
            if ((int)instanceID != 0)
                return IsConfused(ref instance.m_instances.m_buffer[(int)instanceID]);
            Citizen.Location currentLocation = data.CurrentLocation;
            ushort num = data.m_visitBuilding;
            if (currentLocation == Citizen.Location.Visit && (int)num != 0)
            {
                ////target = InstanceID.Empty;
                ////target.Building = num;
                ////return Locale.Get("CITIZEN_STATUS_VISITING");
                return false;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("CITIZEN_STATUS_CONFUSED");
            return true;
        }
    }
}