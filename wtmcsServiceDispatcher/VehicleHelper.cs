using System;
using System.Collections.Generic;
using ColossalFramework;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Keeps track of interesting vehicles.
    /// </summary>
    internal static class VehicleHelper
    {
        /// <summary>
        /// Determines whether the vehicle can be recalled.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>True if the vehicle can be recalled.</returns>
        public static bool CanRecallVehicle(ref Vehicle vehicle)
        {
            return (vehicle.Info.m_vehicleAI is HearseAI && Global.Settings.RecallHearses) ||
                   (vehicle.Info.m_vehicleAI is GarbageTruckAI && Global.Settings.RecallGarbageTrucks);
        }

        /// <summary>
        /// Creates the service vehicle.
        /// </summary>
        /// <param name="serviceBuilding">The service building.</param>
        /// <param name="material">The material.</param>
        /// <returns>
        /// The vehicle information.
        /// </returns>
        /// <exception cref="System.ArgumentException">Unhandled material.</exception>
        public static ServiceVehicleInfo CreateServiceVehicle(ServiceBuildingInfo serviceBuilding, TransferManager.TransferReason material)
        {
            VehicleManager manager = Singleton<VehicleManager>.instance;
            ColossalFramework.Math.Randomizer randomizer = Singleton<SimulationManager>.instance.m_randomizer;

            Building building = BuildingHelper.GetBuilding(serviceBuilding.BuildingId);

            VehicleInfo info = manager.GetRandomVehicleInfo(ref randomizer, building.Info.m_class.m_service, building.Info.m_class.m_subService, building.Info.m_class.m_level);
            if (info == null)
            {
                Log.Debug(typeof(VehicleKeeper), "Create", "GetRandomVehicleInfo", "no vehicle");
                return null;
            }

            Vehicle[] vehicles = manager.m_vehicles.m_buffer;

            bool transferToSource;
            bool transferToTarget;

            switch (material)
            {
                case TransferManager.TransferReason.Dead:
                    transferToSource = true;
                    transferToTarget = false;
                    break;

                case TransferManager.TransferReason.DeadMove:
                    transferToSource = false;
                    transferToTarget = true;
                    break;

                case TransferManager.TransferReason.Garbage:
                    transferToSource = true;
                    transferToTarget = false;
                    break;

                case TransferManager.TransferReason.GarbageMove:
                    transferToSource = false;
                    transferToTarget = true;
                    break;

                default:
                    throw new ArgumentException("Unhandled material: " + material.ToString());
            }

            ushort vehicleId;
            if (!manager.CreateVehicle(out vehicleId, ref randomizer, info, building.m_position, material, transferToSource, transferToTarget))
            {
                Log.Debug(typeof(VehicleKeeper), "Create", "CreateVehicle", "not created");
                return null;
            }

            info.m_vehicleAI.SetSource(vehicleId, ref vehicles[vehicleId], serviceBuilding.BuildingId);

            return new ServiceVehicleInfo(vehicleId, ref vehicles[vehicleId], true);
        }

        /// <summary>
        /// De-assign target from vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        public static void DeAssignVehicle(ushort vehicleId)
        {
            DeAssignVehicle(vehicleId, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId]);
        }

        /// <summary>
        /// De-assign target from vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle data.</param>
        public static void DeAssignVehicle(ushort vehicleId, ref Vehicle vehicle)
        {
            if (vehicle.m_targetBuilding != 0)
            {
                vehicle.Info.m_vehicleAI.SetTarget(vehicleId, ref vehicle, 0);
                vehicle.m_targetBuilding = 0;
            }
        }

        /// <summary>
        /// Logs a list of vehicle info for debug use.
        /// </summary>
        public static void DebugListLog()
        {
            try
            {
                Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
                Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

                for (ushort id = 0; id < vehicles.Length; id++)
                {
                    DebugListLog(vehicles, buildings, id);
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(VehicleKeeper), "DebugListLog()", ex);
            }
        }

        /// <summary>
        /// Logs a list of vehicle info for debug use.
        /// </summary>
        /// <param name="building">The building.</param>
        /// <exception cref="System.Exception">Loop counter too high.</exception>
        public static void DebugListLog(ServiceBuildingInfo building)
        {
            try
            {
                Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
                Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

                int count = 0;
                ushort id = building.FirstOwnVehicleId;
                while (id != 0)
                {
                    DebugListLog(vehicles, buildings, id);

                    id = vehicles[id].m_nextOwnVehicle;
                    if (id == building.FirstOwnVehicleId)
                    {
                        break;
                    }

                    count++;
                    if (count > ushort.MaxValue)
                    {
                        throw new Exception("Loop counter too high");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(VehicleKeeper), "DebugListLog", ex);
            }
        }

        /// <summary>
        /// Logs a list of vehicle info for debug use.
        /// </summary>
        /// <param name="vehicleIds">The vehicle ids.</param>
        public static void DebugListLog(IEnumerable<ushort> vehicleIds)
        {
            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            foreach (ushort id in vehicleIds)
            {
                DebugListLog(vehicles, buildings, id);
            }
        }

        /// <summary>
        /// Logs a list of vehicle info for debug use.
        /// </summary>
        /// <param name="serviceVehicles">The service vehicles.</param>
        public static void DebugListLog(IEnumerable<ServiceVehicleInfo> serviceVehicles)
        {
            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            foreach (ServiceVehicleInfo vehicle in serviceVehicles)
            {
                DebugListLog(vehicles, buildings, vehicle.VehicleId);
            }
        }

        /// <summary>
        /// Gets the name of the vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <returns>The name of the vehicle.</returns>
        public static string GetVehicleName(ushort vehicleId)
        {
            if (!Log.LogNames)
            {
                return null;
            }

            try
            {
                VehicleManager manager = Singleton<VehicleManager>.instance;
                string name = manager.GetVehicleName(vehicleId);

                return String.IsNullOrEmpty(name) ? (string)null : name;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Recalls the vehicle to the service building.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        public static void RecallVehicle(ushort vehicleId)
        {
            RecallVehicle(vehicleId, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId]);
        }

        /// <summary>
        /// Recalls the vehicle to the service building.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle data.</param>
        /// <exception cref="System.InvalidOperationException">Vehicle is not using hearse of garbage truck ai.</exception>
        public static void RecallVehicle(ushort vehicleId, ref Vehicle vehicle)
        {
            // If already going back, no need to do anything.
            if (vehicle.m_targetBuilding == 0 && (vehicle.m_flags & Vehicle.Flags.GoingBack) == Vehicle.Flags.GoingBack)
            {
                return;
            }

            // Call recall for used AI.
            if (vehicle.Info.m_vehicleAI is HearseAI)
            {
                ((HearseAIAssistant)vehicle.Info.m_vehicleAI).Recall(vehicleId, ref vehicle);
            }
            else if (vehicle.Info.m_vehicleAI is GarbageTruckAI)
            {
                ((GarbageTruckAIAssistant)vehicle.Info.m_vehicleAI).Recall(vehicleId, ref vehicle);
            }
            else
            {
                throw new InvalidOperationException("Vehicle is not using hearse of garbage truck ai.");
            }
        }

        /// <summary>
        /// Log vehicle info for debug use.
        /// </summary>
        /// <param name="vehicles">The vehicles.</param>
        /// <param name="buildings">The buildings.</param>
        /// <param name="vehicleId">The vehicle identifier.</param>
        private static void DebugListLog(Vehicle[] vehicles, Building[] buildings, ushort vehicleId)
        {
            if (vehicles[vehicleId].Info != null && (vehicles[vehicleId].m_flags & Vehicle.Flags.Spawned) == Vehicle.Flags.Spawned && (vehicles[vehicleId].Info.m_vehicleAI is HearseAI || vehicles[vehicleId].Info.m_vehicleAI is GarbageTruckAI))
            {
                Log.InfoList info = new Log.InfoList();

                InstanceID instanceId;
                float prgCur, prgMax;
                int bufCur, bufMax;
                string localeKey;
                float distance;

                info.Add("VehicleId", vehicleId);
                info.Add("AI", vehicles[vehicleId].Info.m_vehicleAI.GetType());
                info.Add("InfoName", vehicles[vehicleId].Info.name);

                string name = GetVehicleName(vehicleId);
                if (!String.IsNullOrEmpty(name) && name != vehicles[vehicleId].Info.name)
                {
                    info.Add("VehicleName", name);
                }

                string type = vehicles[vehicleId].m_transferType.ToString();
                foreach (TransferManager.TransferReason reason in Enum.GetValues(typeof(TransferManager.TransferReason)))
                {
                    if ((byte)reason == vehicles[vehicleId].m_transferType)
                    {
                        type = reason.ToString();
                        break;
                    }
                }
                info.Add("Type", type);

                if (vehicles[vehicleId].m_sourceBuilding != 0 && buildings[vehicles[vehicleId].m_sourceBuilding].Info != null)
                {
                    distance = (vehicles[vehicleId].GetLastFramePosition() - buildings[vehicles[vehicleId].m_sourceBuilding].m_position).sqrMagnitude;
                    name = BuildingHelper.GetBuildingName(vehicles[vehicleId].m_sourceBuilding);
                    if (String.IsNullOrEmpty(name))
                    {
                        name = buildings[vehicles[vehicleId].m_sourceBuilding].Info.name;
                    }

                    info.Add("Source", vehicles[vehicleId].m_sourceBuilding, name, distance);
                }

                if (vehicles[vehicleId].m_targetBuilding != 0 && buildings[vehicles[vehicleId].m_targetBuilding].Info != null)
                {
                    distance = (vehicles[vehicleId].GetLastFramePosition() - buildings[vehicles[vehicleId].m_targetBuilding].m_position).sqrMagnitude;
                    name = BuildingHelper.GetBuildingName(vehicles[vehicleId].m_targetBuilding);
                    if (String.IsNullOrEmpty(name))
                    {
                        name = buildings[vehicles[vehicleId].m_targetBuilding].Info.name;
                    }

                    info.Add("Target", vehicles[vehicleId].m_targetBuilding, name, distance);
                }

                string flags = vehicles[vehicleId].m_flags.ToString();
                if (flags.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }) >= 0)
                {
                    foreach (Vehicle.Flags flag in Enum.GetValues(typeof(Vehicle.Flags)))
                    {
                        if (flag != Vehicle.Flags.None && (vehicles[vehicleId].m_flags & flag) == flag)
                        {
                            flags += ", " + flag.ToString();
                        }
                    }
                }
                info.Add("Flags", flags);

                info.Add("Enabled", vehicles[vehicleId].Info.enabled);
                info.Add("Active", vehicles[vehicleId].Info.isActiveAndEnabled);
                info.Add("AIEnabled", vehicles[vehicleId].Info.m_vehicleAI.enabled);
                info.Add("AIActive", vehicles[vehicleId].Info.m_vehicleAI.isActiveAndEnabled);

                if (vehicles[vehicleId].Info.m_vehicleAI.GetProgressStatus(vehicleId, ref vehicles[vehicleId], out prgCur, out prgMax))
                {
                    info.Add("PrgCur", prgCur);
                    info.Add("PrgMax", prgMax);
                }

                vehicles[vehicleId].Info.m_vehicleAI.GetBufferStatus(vehicleId, ref vehicles[vehicleId], out localeKey, out bufCur, out bufMax);
                if (!String.IsNullOrEmpty(localeKey))
                {
                    info.Add("BufLocKey", localeKey);
                }
                info.Add("BufCur", bufCur);
                info.Add("BufMax", bufMax);

                info.Add("TransferSize", vehicles[vehicleId].m_transferSize);

                if (vehicles[vehicleId].Info.m_vehicleAI is HearseAI)
                {
                    info.Add("Capacity", ((HearseAI)vehicles[vehicleId].Info.m_vehicleAI).m_corpseCapacity);
                }
                else if (vehicles[vehicleId].Info.m_vehicleAI is GarbageTruckAI)
                {
                    info.Add("Capacity", ((GarbageTruckAI)vehicles[vehicleId].Info.m_vehicleAI).m_cargoCapacity);
                }

                string status = vehicles[vehicleId].Info.m_vehicleAI.GetLocalizedStatus(vehicleId, ref vehicles[vehicleId], out instanceId);
                if (!String.IsNullOrEmpty(status))
                {
                    info.Add("Status", status);
                }

                info.Add("AI", vehicles[vehicleId].Info.m_vehicleAI.GetType().AssemblyQualifiedName);

                Log.DevDebug(typeof(VehicleKeeper), "DebugListLog", info.ToString());
            }
        }
    }
}