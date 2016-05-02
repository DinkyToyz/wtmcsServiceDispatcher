using ColossalFramework;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Helpers for determining if objects are confused.
    /// </summary>
    /// <remarks>
    /// The "Confused" methods (and dependencies) are straight copies from game code even though that's
    /// slower than a simple condition checks to make it easier to see that the same logic is used.
    /// </remarks>
    internal static class ConfusionHelper
    {
        /// <summary>
        /// Determines whether the specified vehicle is confused.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>
        /// True if the vehicle is confused.
        /// </returns>
        public static bool VehicleIsConfused(ushort vehicleId, ref Vehicle vehicle)
        {
            if (vehicle.Info == null || vehicle.Info.m_vehicleAI == null)
            {
                return false;
            }
            else if (vehicle.Info.m_vehicleAI is HearseAI)
            {
                return HearseConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is GarbageTruckAI)
            {
                return GarbageTruckConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is AmbulanceAI)
            {
                return AmbulanceConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is PassengerCarAI)
            {
                return PassengerCarConfused(vehicleId, ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is BusAI)
            {
                return BusConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is CargoShipAI)
            {
                return CargoShipConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is CargoTrainAI)
            {
                return CargoTrainConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is CargoTruckAI)
            {
                return CargoTruckConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is FireTruckAI)
            {
                return FireTruckConfused(ref vehicle);
            }
            else
            {
                ////MetroTrainAI PassengerPlaneAI
                ////PassengerShipAI PassengerTrainAI PoliceCarAI CargoShipAI SnowTruckAI TaxiAI TramAI
                return false;
            }
        }

        /// <summary>
        /// Check if ambulance is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if ambulance is confused.</returns>
        private static bool AmbulanceConfused(ref Vehicle data)
        {
            // From AmbulanceAI.GetLocalizedStatus from original game code at version 1.4.0-f3.
            if ((data.m_flags & Vehicle.Flags.GoingBack) != Vehicle.Flags.None)
            {
                ////if ((int)data.m_transferSize == 0)
                ////{
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_AMBULANCE_RETURN_EMPTY");
                ////}
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_AMBULANCE_RETURN_FULL");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.WaitingTarget) != Vehicle.Flags.None)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_AMBULANCE_WAIT");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.Emergency2) != Vehicle.Flags.None && (int)data.m_targetBuilding != 0)
            {
                ////target = InstanceID.Empty;
                ////target.Building = data.m_targetBuilding;
                ////return Locale.Get("VEHICLE_STATUS_AMBULANCE_EMERGENCY");
                return false;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }

        /// <summary>
        /// Check if bus car is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if bus is confused.</returns>
        private static bool BusConfused(ref Vehicle data)
        {
            // From BusAI.GetLocalizedStatus from original game code at version 1.4.1-f2.
            if ((data.m_flags & Vehicle.Flags.Stopped) != Vehicle.Flags.None)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_BUS_STOPPED");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.GoingBack) != Vehicle.Flags.None)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_BUS_RETURN");
                return false;
            }
            if ((int)data.m_transportLine != 0)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_BUS_ROUTE");
                return false;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }

        /// <summary>
        /// Check if cargo ship is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if cargo ship is confused.</returns>
        private static bool CargoShipConfused(ref Vehicle data)
        {
            // From CargoShipAI.GetLocalizedStatus from original game code at version 1.4.1-f2.
            if ((data.m_flags & Vehicle.Flags.WaitingCargo) != Vehicle.Flags.None)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_CARGOSHIP_LOADING");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.GoingBack) != Vehicle.Flags.None)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_CARGOTRUCK_RETURN");
                return false;
            }
            if ((int)data.m_targetBuilding != 0)
            {
                ////target = InstanceID.Empty;
                ////target.Building = data.m_targetBuilding;
                ////return Locale.Get("VEHICLE_STATUS_CARGOSHIP_TRANSPORT");
                return false;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }

        /// <summary>
        /// Check if cargo train is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if cargo train is confused.</returns>
        private static bool CargoTrainConfused(ref Vehicle data)
        {
            // From CargoTrainAI.GetLocalizedStatus from original game code at version 1.4.1-f2.
            if ((data.m_flags & Vehicle.Flags.WaitingCargo) != Vehicle.Flags.None)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_CARGOTRAIN_LOADING");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.GoingBack) != Vehicle.Flags.None)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_CARGOTRUCK_RETURN");
                return false;
            }
            if ((int)data.m_targetBuilding != 0)
            {
                ////target = InstanceID.Empty;
                ////target.Building = data.m_targetBuilding;
                ////return Locale.Get("VEHICLE_STATUS_CARGOTRAIN_TRANSPORT");
                return false;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }

        /// <summary>
        /// Check if cargo truck is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if cargo truck is confused.</returns>
        private static bool CargoTruckConfused(ref Vehicle data)
        {
            // From CargoTruckAI.GetLocalizedStatus from original game code at version 1.4.1-f2.
            if ((data.m_flags & Vehicle.Flags.TransferToTarget) != Vehicle.Flags.None)
            {
                ushort num = data.m_targetBuilding;
                if ((data.m_flags & Vehicle.Flags.GoingBack) != Vehicle.Flags.None)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_CARGOTRUCK_RETURN");
                    return false;
                }
                if ((data.m_flags & Vehicle.Flags.WaitingTarget) != Vehicle.Flags.None)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_CARGOTRUCK_UNLOAD");
                    return false;
                }
                if ((int)num != 0)
                {
                    //Building.Flags flags = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)num].m_flags;
                    //TransferManager.TransferReason transferReason = (TransferManager.TransferReason)data.m_transferType;
                    //if ((data.m_flags & Vehicle.Flags.Exporting) != Vehicle.Flags.None || (flags & Building.Flags.IncomingOutgoing) != Building.Flags.None)
                    //{
                    //    target = InstanceID.Empty;
                    //    return Locale.Get("VEHICLE_STATUS_CARGOTRUCK_EXPORT", transferReason.ToString());
                    //}
                    //if ((data.m_flags & Vehicle.Flags.Importing) != Vehicle.Flags.None)
                    //{
                    //    target = InstanceID.Empty;
                    //    target.Building = num;
                    //    return Locale.Get("VEHICLE_STATUS_CARGOTRUCK_IMPORT", transferReason.ToString());
                    //}
                    //target = InstanceID.Empty;
                    //target.Building = num;
                    //return Locale.Get("VEHICLE_STATUS_CARGOTRUCK_DELIVER", transferReason.ToString());
                    return false;
                }
            }
            //target = InstanceID.Empty;
            //return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }

        /// <summary>
        /// Check if passenger car is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if passenger car is confused.</returns>
        private static bool FireTruckConfused(ref Vehicle data)
        {
            // From FireTruckAI.GetLocalizedStatus from original game code at version 1.4.1-f2.
            if ((data.m_flags & Vehicle.Flags.GoingBack) != Vehicle.Flags.None)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_FIRETRUCK_RETURN");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.WaitingTarget) != Vehicle.Flags.None)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_FIRETRUCK_WAIT");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.Emergency2) != Vehicle.Flags.None)
            {
                if ((int)data.m_targetBuilding != 0)
                {
                    ////target = InstanceID.Empty;
                    ////target.Building = data.m_targetBuilding;
                    ////return Locale.Get("VEHICLE_STATUS_FIRETRUCK_EMERGENCY");
                    return false;
                }
            }
            else if ((data.m_flags & Vehicle.Flags.Emergency1) != Vehicle.Flags.None && (int)data.m_targetBuilding != 0)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_FIRETRUCK_EXTINGUISH");
                return false;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }

        /// <summary>
        /// Check if garbage truck is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if garbage truck is confused.</returns>
        private static bool GarbageTruckConfused(ref Vehicle data)
        {
            // From GarbageTruckAI.GetLocalizedStatus from original game code at version 1.4.0-f3.
            if ((data.m_flags & Vehicle.Flags.TransferToSource) != Vehicle.Flags.None)
            {
                ////if ((data.m_flags & Vehicle.Flags.GoingBack) != Vehicle.Flags.None)
                ////{
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_GARBAGE_RETURN");
                ////}
                ////if ((data.m_flags & Vehicle.Flags.WaitingTarget) != Vehicle.Flags.None)
                ////{
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_GARBAGE_WAIT");
                ////}
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_GARBAGE_COLLECT");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.TransferToTarget) != Vehicle.Flags.None)
            {
                if ((data.m_flags & Vehicle.Flags.GoingBack) != Vehicle.Flags.None)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_GARBAGE_RETURN");
                    return false;
                }
                if ((data.m_flags & Vehicle.Flags.WaitingTarget) != Vehicle.Flags.None)
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

        /// <summary>
        /// Check if hearse is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if hearse is confused.</returns>
        private static bool HearseConfused(ref Vehicle data)
        {
            // From HearseAI.GetLocalizedStatus from original game code at version 1.4.0-f3.
            if ((data.m_flags & Vehicle.Flags.TransferToSource) != Vehicle.Flags.None)
            {
                if ((data.m_flags & (Vehicle.Flags.Stopped | Vehicle.Flags.WaitingTarget)) != Vehicle.Flags.None)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_HEARSE_WAIT");
                    return false;
                }
                if ((data.m_flags & Vehicle.Flags.GoingBack) != Vehicle.Flags.None)
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
            else if ((data.m_flags & Vehicle.Flags.TransferToTarget) != Vehicle.Flags.None)
            {
                if ((data.m_flags & Vehicle.Flags.GoingBack) != Vehicle.Flags.None)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_HEARSE_RETURN");
                    return false;
                }
                if ((data.m_flags & Vehicle.Flags.WaitingTarget) != Vehicle.Flags.None)
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

        /// <summary>
        /// Check if passenger car is confused.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if passenger car is confused.</returns>
        private static bool PassengerCarConfused(ushort vehicleId, ref Vehicle data)
        {
            // From PassengerCarAI.GetLocalizedStatus from original game code at version 1.4.1-f2.
            CitizenManager instance = Singleton<CitizenManager>.instance;
            ushort driverInstance = PassengerCarDriverInstance(vehicleId, ref data);
            ushort num1 = (ushort)0;
            if ((int)driverInstance != 0)
            {
                if ((data.m_flags & Vehicle.Flags.Parking) != Vehicle.Flags.None)
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
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_LEAVING");
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
        /// Copy of PassengerCarAI.GetDriverInstance to be used by ConfusedPassengerCar.
        /// </summary>
        /// <param name="vehicleID">The vehicle identifier.</param>
        /// <param name="data">The data.</param>
        /// <returns>The driver instance.</returns>
        private static ushort PassengerCarDriverInstance(ushort vehicleID, ref Vehicle data)
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