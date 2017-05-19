using ColossalFramework;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.ObjectHelpers
{
    /// <summary>
    /// Helper for passenger cars.
    /// </summary>
    internal static class PassengerCarHelper
    {
        /// <summary>
        /// Check if passenger car is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>
        /// True if passenger car is confused.
        /// </returns>
        public static bool IsConfused(ref Vehicle data)
        {
            // From PassengerCarAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            CitizenManager instance = Singleton<CitizenManager>.instance;
            ushort driverInstance = GetDriverInstance(ref data);
            ushort num1 = (ushort)0;
            if ((int)driverInstance != 0)
            {
                if ((data.m_flags & Vehicle.Flags.Parking) != ~VehicleHelper.VehicleAll)
                {
                    uint num2 = instance.m_instances.m_buffer[(int)driverInstance].m_citizen;
                    if ((int)num2 != 0 && (int)instance.m_citizens.m_buffer[num2].m_parkedVehicle != 0)
                    {
                        ////target = InstanceID.Empty;
                        ////return Locale.Get("VEHICLE_STATUS_PARKING");
                        return false;
                    }
                }
                num1 = instance.m_instances.m_buffer[(int)driverInstance].m_targetBuilding;
            }
            if ((int)num1 != 0)
            {
                ////if ((Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)num1].m_flags & Building.Flags.IncomingOutgoing) != Building.Flags.None)
                ////{
                ////    target = InstanceID.Empty;
                ////    return Locale.Get("VEHICLE_STATUS_LEAVING");
                ////}
                ////target = InstanceID.Empty;
                ////target.Building = num1;
                ////return Locale.Get("VEHICLE_STATUS_GOINGTO");
                return false;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }

        /// <summary>
        /// Copy of PassengerCarAI.GetDriverInstance to be used by PassengerCarConfused.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The driver instance.</returns>
        private static ushort GetDriverInstance(ref Vehicle data)
        {
            // From PassengerCarAI.GetDriverInstance from original game code at version 1.4.1-f2.
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num1 = data.m_citizenUnits;
            int num2 = 0;
            while ((int)num1 != 0)
            {
                uint num3 = instance.m_units.m_buffer[num1].m_nextUnit;
                for (int index = 0; index < 5; ++index)
                {
                    uint citizen = instance.m_units.m_buffer[num1].GetCitizen(index);
                    if ((int)citizen != 0)
                    {
                        ushort num4 = instance.m_citizens.m_buffer[citizen].m_instance;
                        if ((int)num4 != 0)
                            return num4;
                    }
                }
                num1 = num3;
                if (++num2 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }
            return (ushort)0;
        }
    }
}