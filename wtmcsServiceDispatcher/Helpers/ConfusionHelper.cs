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
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>
        /// True if the vehicle is confused.
        /// </returns>
        public static bool VehicleIsConfused(ref Vehicle vehicle)
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
                return PassengerCarConfused(ref vehicle);
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
            else if (vehicle.Info.m_vehicleAI is MetroTrainAI)
            {
                return MetroTrainConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is PassengerPlaneAI)
            {
                return PassengerPlaneConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is PassengerShipAI)
            {
                return PassengerShipConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is PassengerTrainAI)
            {
                return PassengerTrainConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is PoliceCarAI)
            {
                return PoliceCarConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is SnowTruckAI)
            {
                return SnowTruckConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is TaxiAI)
            {
                return TaxiConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is TramAI)
            {
                return TramConfused(ref vehicle);
            }
            else
            {
                //// PrisonerAI ResidentAI TouristAI
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
            // From AmbulanceAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.GoingBack) != ~Vehicle.Flags.All)
            {
                ////if ((int)data.m_transferSize == 0)
                ////{
                ////    target = InstanceID.Empty;
                ////    return Locale.Get("VEHICLE_STATUS_AMBULANCE_RETURN_EMPTY");
                ////}
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_AMBULANCE_RETURN_FULL");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.WaitingTarget) != ~Vehicle.Flags.All)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_AMBULANCE_WAIT");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.Emergency2) != ~Vehicle.Flags.All && (int)data.m_targetBuilding != 0)
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
            // From BusAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.Stopped) != ~Vehicle.Flags.All)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_BUS_STOPPED");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.GoingBack) != ~Vehicle.Flags.All)
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
            // From CargoShipAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.WaitingCargo) != ~Vehicle.Flags.All)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_CARGOSHIP_LOADING");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.GoingBack) != ~Vehicle.Flags.All)
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
            // From CargoTrainAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.WaitingCargo) != ~Vehicle.Flags.All)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_CARGOTRAIN_LOADING");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.GoingBack) != ~Vehicle.Flags.All)
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
            // From CargoTruckAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.TransferToTarget) != ~Vehicle.Flags.All)
            {
                ushort num = data.m_targetBuilding;
                if ((data.m_flags & Vehicle.Flags.GoingBack) != ~Vehicle.Flags.All)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_CARGOTRUCK_RETURN");
                    return false;
                }
                if ((data.m_flags & Vehicle.Flags.WaitingTarget) != ~Vehicle.Flags.All)
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

        /// <summary>
        /// Check if passenger car is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if passenger car is confused.</returns>
        private static bool FireTruckConfused(ref Vehicle data)
        {
            // From FireTruckAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.GoingBack) != ~Vehicle.Flags.All)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_FIRETRUCK_RETURN");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.WaitingTarget) != ~Vehicle.Flags.All)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_FIRETRUCK_WAIT");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.Emergency2) != ~Vehicle.Flags.All)
            {
                if ((int)data.m_targetBuilding != 0)
                {
                    ////target = InstanceID.Empty;
                    ////target.Building = data.m_targetBuilding;
                    ////return Locale.Get("VEHICLE_STATUS_FIRETRUCK_EMERGENCY");
                }
                return false;
            }
            else if ((data.m_flags & Vehicle.Flags.Emergency1) != ~Vehicle.Flags.All && (int)data.m_targetBuilding != 0)
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
            // From GarbageTruckAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.TransferToSource) != ~Vehicle.Flags.All)
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
            if ((data.m_flags & Vehicle.Flags.TransferToTarget) != ~Vehicle.Flags.All)
            {
                if ((data.m_flags & Vehicle.Flags.GoingBack) != ~Vehicle.Flags.All)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_GARBAGE_RETURN");
                    return false;
                }
                if ((data.m_flags & Vehicle.Flags.WaitingTarget) != ~Vehicle.Flags.All)
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
            // From HearseAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.TransferToSource) != ~Vehicle.Flags.All)
            {
                if ((data.m_flags & (Vehicle.Flags.Stopped | Vehicle.Flags.WaitingTarget)) != ~Vehicle.Flags.All)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_HEARSE_WAIT");
                    return false;
                }
                if ((data.m_flags & Vehicle.Flags.GoingBack) != ~Vehicle.Flags.All)
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
            else if ((data.m_flags & Vehicle.Flags.TransferToTarget) != ~Vehicle.Flags.All)
            {
                if ((data.m_flags & Vehicle.Flags.GoingBack) != ~Vehicle.Flags.All)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_HEARSE_RETURN");
                    return false;
                }
                if ((data.m_flags & Vehicle.Flags.WaitingTarget) != ~Vehicle.Flags.All)
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
        /// Check if metro is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if metro is confused.</returns>
        private static bool MetroTrainConfused(ref Vehicle data)
        {
            // From MetroTrainAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.Stopped) != ~Vehicle.Flags.All)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_METRO_STOPPED");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.GoingBack) != ~Vehicle.Flags.All)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_METRO_RETURN");
                return false;
            }
            if ((int)data.m_transportLine != 0)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_METRO_ROUTE");
                return false;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }

        /// <summary>
        /// Check if passenger car is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>
        /// True if passenger car is confused.
        /// </returns>
        private static bool PassengerCarConfused(ref Vehicle data)
        {
            // From PassengerCarAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            CitizenManager instance = Singleton<CitizenManager>.instance;
            ushort driverInstance = ConfusionHelper.PassengerCarDriverInstance(ref data);
            ushort num1 = (ushort)0;
            if ((int)driverInstance != 0)
            {
                if ((data.m_flags & Vehicle.Flags.Parking) != ~Vehicle.Flags.All)
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
        private static ushort PassengerCarDriverInstance(ref Vehicle data)
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

        /// <summary>
        /// Check if plane is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if plane is confused.</returns>
        private static bool PassengerPlaneConfused(ref Vehicle data)
        {
            // From PassengerPlaneAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.Stopped) != ~Vehicle.Flags.All)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_AIRPLANE_BOARDING");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.Landing) != ~Vehicle.Flags.All)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_AIRPLANE_LANDING");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.TakingOff) != ~Vehicle.Flags.All)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_AIRPLANE_TAKING_OFF");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.Flying) == ~Vehicle.Flags.All)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_AIRPLANE_TAXIING");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.GoingBack) != ~Vehicle.Flags.All)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_CARGOTRUCK_RETURN");
                return false;
            }
            if ((int)data.m_targetBuilding != 0)
            {
                if ((data.m_flags & Vehicle.Flags.DummyTraffic) != ~Vehicle.Flags.All)
                {
                    ////target = InstanceID.Empty;
                    ////target.Building = data.m_targetBuilding;
                    ////return Locale.Get("VEHICLE_STATUS_AIRPLANE_FLYING");
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
                    ////return Locale.Get("VEHICLE_STATUS_AIRPLANE_FLYING");
                    return false;
                }
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }

        /// <summary>
        /// Check if passenger ship is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if passenger ship is confused.</returns>
        private static bool PassengerShipConfused(ref Vehicle data)
        {
            // From PassengerShipAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.Stopped) != ~Vehicle.Flags.All)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_PASSENGERSHIP_STOPPED");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.GoingBack) != ~Vehicle.Flags.All)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_CARGOTRUCK_RETURN");
                return false;
            }
            if ((int)data.m_targetBuilding != 0)
            {
                if ((data.m_flags & Vehicle.Flags.DummyTraffic) != ~Vehicle.Flags.All)
                {
                    ////target = InstanceID.Empty;
                    ////target.Building = data.m_targetBuilding;
                    ////return Locale.Get("VEHICLE_STATUS_PASSENGERSHIP_TRANSPORT");
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
                    ////return Locale.Get("VEHICLE_STATUS_PASSENGERSHIP_TRANSPORT");
                    return false;
                }
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }

        /// <summary>
        /// Check if passenger train is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if passenger train is confused.</returns>
        private static bool PassengerTrainConfused(ref Vehicle data)
        {
            // From PassengerTrainAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.Stopped) != ~Vehicle.Flags.All)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_PASSENGERTRAIN_STOPPED");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.GoingBack) != ~Vehicle.Flags.All)
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
                if ((data.m_flags & Vehicle.Flags.DummyTraffic) != ~Vehicle.Flags.All)
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

        /// <summary>
        /// Check if police car is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if police car is confused.</returns>
        private static bool PoliceCarConfused(ref Vehicle data)
        {
            // From PoliceCarAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if (data.Info.m_class.m_level >= ItemClass.Level.Level4)
            {
                if ((data.m_flags & Vehicle.Flags.GoingBack) != ~Vehicle.Flags.All)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_PRISON_RETURN");
                    return false;
                }
                if ((data.m_flags & (Vehicle.Flags.Stopped | Vehicle.Flags.WaitingTarget)) != ~Vehicle.Flags.All)
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
            if ((data.m_flags & Vehicle.Flags.GoingBack) != ~Vehicle.Flags.All)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_POLICE_RETURN");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.Stopped) != ~Vehicle.Flags.All)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_POLICE_STOPPED");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.WaitingTarget) != ~Vehicle.Flags.All)
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
            if ((data.m_flags & Vehicle.Flags.Emergency2) != ~Vehicle.Flags.All)
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

        /// <summary>
        /// Check if prisoner is confused.
        /// </summary>
        /// <param name="data">The citizen.</param>
        /// <returns>True if prisoner is confused.</returns>
        private static bool PrisonerConfused(ref Citizen data)
        {
            // From PrisonerAI.GetLocalizedStatus from original game code at version 1.4.1-f2.
            CitizenManager instance = Singleton<CitizenManager>.instance;
            ushort instanceID = data.m_instance;
            if ((int)instanceID != 0)
                return PrisonerConfused(ref instance.m_instances.m_buffer[(int)instanceID]);
            if ((int)data.m_visitBuilding != 0)
            {
                ////target = InstanceID.Empty;
                ////target.Building = data.m_visitBuilding;
                ////return Locale.Get("CITIZEN_STATUS_VISITING");
                return false;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("CITIZEN_STATUS_CONFUSED");
            return true;
        }

        /// <summary>
        /// Check if prisoner is confused.
        /// </summary>
        /// <param name="data">The citizen instance.</param>
        /// <returns>True if prisoner is confused.</returns>
        private static bool PrisonerConfused(ref CitizenInstance data)
        {
            // From PrisonerAI.GetLocalizedStatus from original game code at version 1.4.1-f2.
            ushort num = data.m_targetBuilding;
            if ((int)num != 0)
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

        /// <summary>
        /// Check if resident is confused.
        /// </summary>
        /// <param name="data">The citizen instance.</param>
        /// <returns>True if resident is confused.</returns>
        private static bool ResidentConfused(ref CitizenInstance data)
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
        private static bool ResidentConfused(ref Citizen data)
        {
            // From ResidentAI.GetLocalizedStatus from original game code at version 1.4.1-f2.
            CitizenManager instance = Singleton<CitizenManager>.instance;
            ushort instanceID = data.m_instance;
            if ((int)instanceID != 0)
                return ResidentConfused(ref instance.m_instances.m_buffer[(int)instanceID]);
            Citizen.Location currentLocation = data.CurrentLocation;
            ushort num1 = data.m_homeBuilding;
            ushort num2 = data.m_workBuilding;
            ushort num3 = data.m_visitBuilding;
            ////bool flag = (data.m_flags & Citizen.Flags.Student) != Citizen.Flags.None;
            switch (currentLocation)
            {
                case Citizen.Location.Home:
                    if ((int)num1 != 0)
                    {
                        ////target = InstanceID.Empty;
                        ////return Locale.Get("CITIZEN_STATUS_AT_HOME");
                        return false;
                    }
                    break;

                case Citizen.Location.Work:
                    if ((int)num2 != 0)
                    {
                        ////target = InstanceID.Empty;
                        ////return Locale.Get(!flag ? "CITIZEN_STATUS_AT_WORK" : "CITIZEN_STATUS_AT_SCHOOL");
                        return false;
                    }
                    break;

                case Citizen.Location.Visit:
                    if ((int)num3 != 0)
                    {
                        ////target = InstanceID.Empty;
                        ////target.Building = num3;
                        ////return Locale.Get("CITIZEN_STATUS_VISITING");
                        return false;
                    }
                    break;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("CITIZEN_STATUS_CONFUSED");
            return true;
        }

        /// <summary>
        /// Check if snow truck is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if snow truck is confused.</returns>
        private static bool SnowTruckConfused(ref Vehicle data)
        {
            // From SnowTruckAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.TransferToSource) != ~Vehicle.Flags.All)
            {
                ////if ((data.m_flags & Vehicle.Flags.GoingBack) != ~Vehicle.Flags.All)
                ////{
                ////    target = InstanceID.Empty;
                ////    return Locale.Get("VEHICLE_STATUS_SNOW_RETURN");
                ////}
                ////if ((data.m_flags & Vehicle.Flags.WaitingTarget) != ~Vehicle.Flags.All)
                ////{
                ////    target = InstanceID.Empty;
                ////    return Locale.Get("VEHICLE_STATUS_SNOW_WAIT");
                ////}
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_SNOW_COLLECT");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.TransferToTarget) != ~Vehicle.Flags.All)
            {
                if ((data.m_flags & Vehicle.Flags.GoingBack) != ~Vehicle.Flags.All)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_SNOW_RETURN");
                    return false;
                }
                if ((data.m_flags & Vehicle.Flags.WaitingTarget) != ~Vehicle.Flags.All)
                {
                    ////target = InstanceID.Empty;
                    ////return Locale.Get("VEHICLE_STATUS_SNOW_UNLOAD");
                    return false;
                }
                if ((int)data.m_targetBuilding != 0)
                {
                    ////target = InstanceID.Empty;
                    ////target.Building = data.m_targetBuilding;
                    ////return Locale.Get("VEHICLE_STATUS_SNOW_TRANSFER");
                    return false;
                }
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }

        /// <summary>
        /// Check if taxi is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if taxi is confused.</returns>
        private static bool TaxiConfused(ref Vehicle data)
        {
            // From TaxiAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            ushort passengerInstance = ConfusionHelper.TaxiGetPassengerInstance(ref data);
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
            if ((data.m_flags & Vehicle.Flags.WaitingTarget) != ~Vehicle.Flags.All)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_TAXI_PLANNING");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.GoingBack) != ~Vehicle.Flags.All)
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
        private static ushort TaxiGetPassengerInstance(ref Vehicle data)
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

        /// <summary>
        /// Check if tourist is confused.
        /// </summary>
        /// <param name="data">The citizen instance.</param>
        /// <returns>True if tourist is confused.</returns>
        private static bool TouristConfused(ref CitizenInstance data)
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
        private static bool TouristConfused(ref Citizen data)
        {
            // From TouristAI.GetLocalizedStatus from original game code at version 1.4.1-f2.
            CitizenManager instance = Singleton<CitizenManager>.instance;
            ushort instanceID = data.m_instance;
            if ((int)instanceID != 0)
                return TouristConfused(ref instance.m_instances.m_buffer[(int)instanceID]);
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

        /// <summary>
        /// Check if tram is confused.
        /// </summary>
        /// <param name="data">The vehicle.</param>
        /// <returns>True if tram is confused.</returns>
        private static bool TramConfused(ref Vehicle data)
        {
            // From TramAI.GetLocalizedStatus from original game code at version 1.5.0-f4.
            if ((data.m_flags & Vehicle.Flags.Stopped) != ~Vehicle.Flags.All)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_TRAM_STOPPED");
                return false;
            }
            if ((data.m_flags & Vehicle.Flags.GoingBack) != ~Vehicle.Flags.All)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_TRAM_RETURN");
                return false;
            }
            if ((int)data.m_transportLine != 0)
            {
                ////target = InstanceID.Empty;
                ////return Locale.Get("VEHICLE_STATUS_TRAM_ROUTE");
                return false;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("VEHICLE_STATUS_CONFUSED");
            return true;
        }
    }
}