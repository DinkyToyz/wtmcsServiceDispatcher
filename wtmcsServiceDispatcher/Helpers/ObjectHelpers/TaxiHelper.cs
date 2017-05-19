using ColossalFramework;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.ObjectHelpers
{
    /// <summary>
    /// Helper for taxis.
    /// </summary>
    internal static class TaxiHelper
    {
        /// <summary>
        /// Check if taxi is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if taxi is confused.</returns>
        public static bool IsConfused(ref Vehicle data)
        {
            // From TaxiAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            ushort passengerInstance = GetPassengerInstance(ref data);
            if ((int)passengerInstance != 0)
            {
                ////if ((Singleton<CitizenManager>.instance.m_instances.m_buffer[(int)passengerInstance].m_flags & CitizenInstance.Flags.Character) != CitizenInstance.Flags.None)
                ////{
                ////    target = InstanceID.Empty;
                ////    target.Citizen = Singleton<CitizenManager>.instance.m_instances.m_buffer[(int)passengerInstance].m_citizen;
                ////    return Locale.Get("VEHICLE_STATUS_TAXI_PICKINGUP");
                ////}
                ////target = InstanceID.Empty;
                ////target.Building = Singleton<CitizenManager>.instance.m_instances.m_buffer[(int)passengerInstance].m_targetBuilding;
                ////return Locale.Get("VEHICLE_STATUS_TAXI_TRANSPORTING");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.WaitingTarget) != ~VehicleHelper.VehicleAll)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_TAXI_PLANNING");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.GoingBack) != ~VehicleHelper.VehicleAll)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_TAXI_RETURN");
                return false;
            }
            if ((int)data.m_targetBuilding != 0)
            {
                ////if ((data.m_flags & Vehicle.Flags.WaitingCargo) != ~Vehicle.Flags.All)
                ////{
                ////    target = InstanceID.Empty;
                ////    return Locale.Get("VEHICLE_STATUS_TAXI_WAIT");
                ////}
                ////target = InstanceID.Empty;
                ////target.Building = data.m_targetBuilding;
                ////return Locale.Get("VEHICLE_STATUS_TAXI_HEADING");
                return false;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }

        /// <summary>
        /// Copy of TaxiAI.GetPassengerInstance to be used by TaxiConfused.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The passenger instance.</returns>
        private static ushort GetPassengerInstance(ref Vehicle data)
        {
            // From TaxiAI.GetPassengerInstance from original game code at version 1.4.1-f2.
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