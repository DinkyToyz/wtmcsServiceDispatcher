using ColossalFramework;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.ObjectHelpers
{
    /// <summary>
    /// Helper for passenger trains.
    /// </summary>
    internal static class PassengerTrainHelper
    {
        /// <summary>
        /// Check if passenger train is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if passenger train is confused.</returns>
        public static bool IsConfused(ref Vehicle data)
        {
            // From PassengerTrainAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.Stopped) != ~VehicleHelper.VehicleAll)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_PASSENGERTRAIN_STOPPED");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.GoingBack) != ~VehicleHelper.VehicleAll)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_PASSENGERTRAIN_RETURN");
                return false;
            }
            if ((int)data.m_transportLine != 0)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_PASSENGERTRAIN_ROUTE");
                return false;
            }
            if ((int)data.m_targetBuilding != 0)
            {
                if ((data.m_flags & Vehicle.Flags.DummyTraffic) != ~VehicleHelper.VehicleAll)
                {
                    ////target = InstanceID.Empty;
                    ////target.Building = data.m_targetBuilding;
                    ////return Locale.Get("VEHICLE_STATUS_PASSENGERTRAIN_TRANSPORT");
                    return false;
                }
                ushort buildingID = Singleton<BuildingManager>.instance.FindBuilding(Singleton<NetManager>.instance.m_nodes.m_buffer[(int)data.m_targetBuilding].m_position, 128f, data.Info.m_class.m_service, data.Info.m_class.m_subService, Building.Flags.None, Building.Flags.None);
                if ((int)buildingID != 0)
                {
                    ////ushort parentBuilding = Building.FindParentBuilding(buildingID);
                    ////if ((int)parentBuilding != 0)
                    ////    buildingID = parentBuilding;
                    ////target = InstanceID.Empty;
                    ////target.Building = buildingID;
                    ////return Locale.Get("VEHICLE_STATUS_PASSENGERTRAIN_TRANSPORT");
                    return false;
                }
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }
    }
}