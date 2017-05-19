using WhatThe.Mods.CitiesSkylines.ServiceDispatcher.ObjectHelpers;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Helpers for determining if objects are confused.
    /// </summary>
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
                return HearseHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is GarbageTruckAI)
            {
                return GarbageTruckHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is AmbulanceAI)
            {
                return AmbulanceHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is PassengerCarAI)
            {
                return PassengerCarHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is BusAI)
            {
                return BusHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is CargoShipAI)
            {
                return CargoShipHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is CargoTrainAI)
            {
                return CargoTrainHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is CargoTruckAI)
            {
                return CargoTrainHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is FireTruckAI)
            {
                return FireTruckHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is MetroTrainAI)
            {
                return MetroTrainHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is PassengerPlaneAI)
            {
                return PassengerPlaneHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is PassengerShipAI)
            {
                return PassengerShipHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is PassengerTrainAI)
            {
                return PassengerTrainHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is PoliceCarAI)
            {
                return PoliceCarHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is SnowTruckAI)
            {
                return SnowTruckHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is TaxiAI)
            {
                return TaxiHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is TramAI)
            {
                return TramHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is PassengerBlimpAI)
            {
                return PassengerBlimpHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is PassengerFerryAI)
            {
                return PassengerFerryHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is FireCopterAI)
            {
                return FireCopterHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is DisasterResponseVehicleAI)
            {
                return DisasterResponseVehicleHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is DisasterResponseCopterAI)
            {
                return DisasterResponseCopterHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is AmbulanceCopterAI)
            {
                return AmbulanceCopterHelper.IsConfused(ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is CableCarAI)
            {
                return CableCarHelper.IsConfused(ref vehicle);
            }
            else
            {
                return false;
            }
        }

        // TODO: CitizenIsConfused ThingyIsConfused?
        // PrisonerAI
        // ResidentAI
        // TouristAI
        // FiremanAI
        // HearseDriverAI
        // LivestockAI
        // ParamedicAI
        // PetAI
        // PoliceOfficerAI
        // RescueAnimalAI
        // RescueWorkerAI
        // WildlifeAI
    }
}