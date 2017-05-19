using ColossalFramework;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.ObjectHelpers
{
    /// <summary>
    /// Helper for residents.
    /// </summary>
    internal static class ResidentHelper
    {
        /// <summary>
        /// Check if resident is confused.
        /// </summary>
        /// <param name="data">The citizen instance.</param>
        /// <returns>True if resident is confused.</returns>
        public static bool IsConfused(ref CitizenInstance data)
        {
            // From ResidentAI.GetLocalizedStatus from original game code at version 1.4.1-f2.
            ////CitizenManager instance1 = Singleton<CitizenManager>.instance;
            ////uint num1 = data.m_citizen;
            ////bool flag1 = false;
            ////ushort num2 = (ushort)0;
            ////ushort num3 = (ushort)0;
            ////ushort vehicleID = (ushort)0;
            ////if ((int)num1 != 0)
            ////{
            ////    num2 = instance1.m_citizens.m_buffer[(IntPtr)num1].m_homeBuilding;
            ////    num3 = instance1.m_citizens.m_buffer[(IntPtr)num1].m_workBuilding;
            ////    vehicleID = instance1.m_citizens.m_buffer[(IntPtr)num1].m_vehicle;
            ////    flag1 = (instance1.m_citizens.m_buffer[(IntPtr)num1].m_flags & Citizen.Flags.Student) != Citizen.Flags.None;
            ////}
            ushort num4 = data.m_targetBuilding;
            if ((int)num4 != 0)
            {
                ////bool flag2 = (Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)num4].m_flags & Building.Flags.IncomingOutgoing) != Building.Flags.None;
                ////bool flag3 = (int)data.m_path == 0 && (data.m_flags & CitizenInstance.Flags.HangAround) != CitizenInstance.Flags.None;
                ////if ((int)vehicleID != 0)
                ////{
                ////    VehicleManager instance2 = Singleton<VehicleManager>.instance;
                ////    VehicleInfo info = instance2.m_vehicles.m_buffer[(int)vehicleID].Info;
                ////    if (info.m_class.m_service == ItemClass.Service.Residential && info.m_vehicleType != VehicleInfo.VehicleType.Bicycle)
                ////    {
                ////        if ((int)info.m_vehicleAI.GetOwnerID(vehicleID, ref instance2.m_vehicles.m_buffer[(int)vehicleID]).Citizen == (int)num1)
                ////        {
                ////            if (flag2)
                ////            {
                ////                target = InstanceID.Empty;
                ////                return Locale.Get("CITIZEN_STATUS_DRIVINGTO_OUTSIDE");
                ////            }
                ////            if ((int)num4 == (int)num2)
                ////            {
                ////                target = InstanceID.Empty;
                ////                return Locale.Get("CITIZEN_STATUS_DRIVINGTO_HOME");
                ////            }
                ////            if ((int)num4 == (int)num3)
                ////            {
                ////                target = InstanceID.Empty;
                ////                return Locale.Get(!flag1 ? "CITIZEN_STATUS_DRIVINGTO_WORK" : "CITIZEN_STATUS_DRIVINGTO_SCHOOL");
                ////            }
                ////            target = InstanceID.Empty;
                ////            target.Building = num4;
                ////            return Locale.Get("CITIZEN_STATUS_DRIVINGTO");
                ////        }
                ////    }
                ////    else if (info.m_class.m_service == ItemClass.Service.PublicTransport)
                ////    {
                ////        if ((data.m_flags & CitizenInstance.Flags.WaitingTaxi) != CitizenInstance.Flags.None)
                ////        {
                ////            target = InstanceID.Empty;
                ////            return Locale.Get("CITIZEN_STATUS_WAITING_TAXI");
                ////        }
                ////        if (flag2)
                ////        {
                ////            target = InstanceID.Empty;
                ////            return Locale.Get("CITIZEN_STATUS_TRAVELLINGTO_OUTSIDE");
                ////        }
                ////        if ((int)num4 == (int)num2)
                ////        {
                ////            target = InstanceID.Empty;
                ////            return Locale.Get("CITIZEN_STATUS_TRAVELLINGTO_HOME");
                ////        }
                ////        if ((int)num4 == (int)num3)
                ////        {
                ////            target = InstanceID.Empty;
                ////            return Locale.Get(!flag1 ? "CITIZEN_STATUS_TRAVELLINGTO_WORK" : "CITIZEN_STATUS_TRAVELLINGTO_SCHOOL");
                ////        }
                ////        target = InstanceID.Empty;
                ////        target.Building = num4;
                ////        return Locale.Get("CITIZEN_STATUS_TRAVELLINGTO");
                ////    }
                ////}
                ////if (flag2)
                ////{
                ////    target = InstanceID.Empty;
                ////    return Locale.Get("CITIZEN_STATUS_GOINGTO_OUTSIDE");
                ////}
                ////if ((int)num4 == (int)num2)
                ////{
                ////    if (flag3)
                ////    {
                ////        target = InstanceID.Empty;
                ////        return Locale.Get("CITIZEN_STATUS_AT_HOME");
                ////    }
                ////    target = InstanceID.Empty;
                ////    return Locale.Get("CITIZEN_STATUS_GOINGTO_HOME");
                ////}
                ////if ((int)num4 == (int)num3)
                ////{
                ////    if (flag3)
                ////    {
                ////        target = InstanceID.Empty;
                ////        return Locale.Get(!flag1 ? "CITIZEN_STATUS_AT_WORK" : "CITIZEN_STATUS_AT_SCHOOL");
                ////    }
                ////    target = InstanceID.Empty;
                ////    return Locale.Get(!flag1 ? "CITIZEN_STATUS_GOINGTO_WORK" : "CITIZEN_STATUS_GOINGTO_SCHOOL");
                ////}
                ////if (flag3)
                ////{
                ////    target = InstanceID.Empty;
                ////    target.Building = num4;
                ////    return Locale.Get("CITIZEN_STATUS_VISITING");
                ////}
                ////target = InstanceID.Empty;
                ////target.Building = num4;
                ////return Locale.Get("CITIZEN_STATUS_GOINGTO");
                return false;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("CITIZEN_STATUS_CONFUSED");
            return true;
        }

        /// <summary>
        /// Check if resident is confused.
        /// </summary>
        /// <param name="data">The citizen.</param>
        /// <returns>True if resident is confused.</returns>
        public static bool IsConfused(ref Citizen data)
        {
            // From ResidentAI.GetLocalizedStatus from original game code at version 1.7.0-f5.
            CitizenManager instance1 = Singleton<CitizenManager>.instance;
            ushort instance2 = data.m_instance;
            if ((int)instance2 != 0)
                return IsConfused(ref instance1.m_instances.m_buffer[(int)instance2]);
            Citizen.Location currentLocation = data.CurrentLocation;
            ushort homeBuilding = data.m_homeBuilding;
            ushort workBuilding = data.m_workBuilding;
            ushort visitBuilding = data.m_visitBuilding;
            //bool flag = (data.m_flags & Citizen.Flags.Student) != Citizen.Flags.None;
            if (currentLocation != Citizen.Location.Home)
            {
                if (currentLocation != Citizen.Location.Work)
                {
                    if (currentLocation == Citizen.Location.Visit && (int)visitBuilding != 0)
                    {
                        //target = InstanceID.Empty;
                        //target.Building = visitBuilding;
                        //return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_VISITING");
                        return false;
                    }
                }
                else if ((int)workBuilding != 0)
                {
                    //target = InstanceID.Empty;
                    //return ColossalFramework.Globalization.Locale.Get(!flag ? "CITIZEN_STATUS_AT_WORK" : "CITIZEN_STATUS_AT_SCHOOL");
                    return false;
                }
            }
            else if ((int)homeBuilding != 0)
            {
                //target = InstanceID.Empty;
                //return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_AT_HOME");
                return false;
            }
            //target = InstanceID.Empty;
            //return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_CONFUSED");
            return true;
        }
    }
}