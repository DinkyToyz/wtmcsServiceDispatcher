﻿namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.ObjectHelpers
{
    /// <summary>
    /// Helper for cable cars.
    /// </summary>
    internal static class CableCarHelper
    {
        /// <summary>
        /// Check if cable car is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if cable car is confused.</returns>
        public static bool IsConfused(ref Vehicle data)
        {
            // From CableCarAI.GetLocalizedStatus from original game code at version 1.7.0-f5.
            if ((data.m_flags & Vehicle.Flags.Stopped) != ~(Vehicle.Flags.Created | Vehicle.Flags.Deleted | Vehicle.Flags.Spawned | Vehicle.Flags.Inverted | Vehicle.Flags.TransferToTarget | Vehicle.Flags.TransferToSource | Vehicle.Flags.Emergency1 | Vehicle.Flags.Emergency2 | Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped | Vehicle.Flags.Leaving | Vehicle.Flags.Arriving | Vehicle.Flags.Reversed | Vehicle.Flags.TakingOff | Vehicle.Flags.Flying | Vehicle.Flags.Landing | Vehicle.Flags.WaitingSpace | Vehicle.Flags.WaitingCargo | Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget | Vehicle.Flags.Importing | Vehicle.Flags.Exporting | Vehicle.Flags.Parking | Vehicle.Flags.CustomName | Vehicle.Flags.OnGravel | Vehicle.Flags.WaitingLoading | Vehicle.Flags.Congestion | Vehicle.Flags.DummyTraffic | Vehicle.Flags.Underground | Vehicle.Flags.Transition | Vehicle.Flags.InsideBuilding | Vehicle.Flags.LeftHandDrive))
            {
                //target = InstanceID.Empty;
                //return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_BUS_STOPPED");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.GoingBack) != ~(Vehicle.Flags.Created | Vehicle.Flags.Deleted | Vehicle.Flags.Spawned | Vehicle.Flags.Inverted | Vehicle.Flags.TransferToTarget | Vehicle.Flags.TransferToSource | Vehicle.Flags.Emergency1 | Vehicle.Flags.Emergency2 | Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped | Vehicle.Flags.Leaving | Vehicle.Flags.Arriving | Vehicle.Flags.Reversed | Vehicle.Flags.TakingOff | Vehicle.Flags.Flying | Vehicle.Flags.Landing | Vehicle.Flags.WaitingSpace | Vehicle.Flags.WaitingCargo | Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget | Vehicle.Flags.Importing | Vehicle.Flags.Exporting | Vehicle.Flags.Parking | Vehicle.Flags.CustomName | Vehicle.Flags.OnGravel | Vehicle.Flags.WaitingLoading | Vehicle.Flags.Congestion | Vehicle.Flags.DummyTraffic | Vehicle.Flags.Underground | Vehicle.Flags.Transition | Vehicle.Flags.InsideBuilding | Vehicle.Flags.LeftHandDrive))
            {
                //target = InstanceID.Empty;
                //return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_BUS_RETURN");
                return false;
            }
            if ((int)data.m_targetBuilding != 0)
            {
                //target = InstanceID.Empty;
                //return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_BUS_ROUTE");
                return false;
            }
            //target = InstanceID.Empty;
            //return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }
    }
}