namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.ObjectHelpers
{
    /// <summary>
    /// Helper for cargo trucks.
    /// </summary>
    internal static class CargoTruckHelper
    {
        /// <summary>
        /// Check if cargo truck is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if cargo truck is confused.</returns>
        private static bool IsConfused(ref Vehicle data)
        {
            // From CargoTruckAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.TransferToTarget) != ~VehicleHelper.VehicleAll)
            {
                ushort num = data.m_targetBuilding;
                if ((data.m_flags & Vehicle.Flags.GoingBack) != ~VehicleHelper.VehicleAll)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_CARGOTRUCK_RETURN");
                    return false;
                }
                if ((data.m_flags & Vehicle.Flags.WaitingTarget) != ~VehicleHelper.VehicleAll)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_CARGOTRUCK_UNLOAD");
                    return false;
                }
                if ((int)num != 0)
                {
                    ////Building.Flags flags = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)num].m_flags;
                    ////TransferManager.TransferReason transferReason = (TransferManager.TransferReason)data.m_transferType;
                    ////if ((data.m_flags & Vehicle.Flags.Exporting) != ~Vehicle.Flags.All || (flags & Building.Flags.IncomingOutgoing) != Building.Flags.None)
                    ////{
                    ////    target = InstanceID.Empty;
                    ////    return Locale.Get("VEHICLE_STATUS_CARGOTRUCK_EXPORT", transferReason.ToString());
                    ////}
                    ////if ((data.m_flags & Vehicle.Flags.Importing) != ~Vehicle.Flags.All)
                    ////{
                    ////    target = InstanceID.Empty;
                    ////    target.Building = num;
                    ////    return Locale.Get("VEHICLE_STATUS_CARGOTRUCK_IMPORT", transferReason.ToString());
                    ////}
                    ////target = InstanceID.Empty;
                    ////target.Building = num;
                    ////return Locale.Get("VEHICLE_STATUS_CARGOTRUCK_DELIVER", transferReason.ToString());
                    return false;
                }
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }
    }
}