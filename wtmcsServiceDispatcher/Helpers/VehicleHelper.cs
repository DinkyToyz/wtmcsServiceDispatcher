using System;
using System.Collections.Generic;
using System.IO;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Keeps track of interesting vehicles.
    /// </summary>
    internal static class VehicleHelper
    {
        /// <summary>
        /// The vehicle is busy if any of these flags are set.
        /// </summary>
        public const Vehicle.Flags VehicleBusy = Vehicle.Flags.Arriving | Vehicle.Flags.Landing | Vehicle.Flags.Leaving | Vehicle.Flags.Parking | Vehicle.Flags.TakingOff;

        /// <summary>
        /// The vehicle exists if any of these flags are set.
        /// </summary>
        public const Vehicle.Flags VehicleExists = Vehicle.Flags.Spawned | Vehicle.Flags.WaitingPath | Vehicle.Flags.WaitingSpace;

        /// <summary>
        /// The vehicle is unavailable if any of these flags are set.
        /// </summary>
        public const Vehicle.Flags VehicleUnavailable = Vehicle.Flags.WaitingPath | Vehicle.Flags.WaitingSpace | Vehicle.Flags.WaitingLoading | Vehicle.Flags.Deleted;

        /// <summary>
        /// Creates the service vehicle.
        /// </summary>
        /// <param name="serviceBuilding">The service building.</param>
        /// <param name="material">The material.</param>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <returns>
        /// The vehicle information.
        /// </returns>
        /// <exception cref="System.ArgumentException">Unhandled material.</exception>
        public static VehicleInfo CreateServiceVehicle(ServiceBuildingInfo serviceBuilding, TransferManager.TransferReason material, out ushort vehicleId)
        {
            return CreateServiceVehicle(serviceBuilding, material, 0, out vehicleId);
        }

        /// <summary>
        /// Creates the service vehicle.
        /// </summary>
        /// <param name="serviceBuilding">The service building.</param>
        /// <param name="material">The material.</param>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <returns>
        /// The vehicle information.
        /// </returns>
        /// <exception cref="System.ArgumentException">Unhandled material.</exception>
        public static VehicleInfo CreateServiceVehicle(ServiceBuildingInfo serviceBuilding, TransferManager.TransferReason material, ushort targetBuildingId, out ushort vehicleId)
        {
            vehicleId = 0;

            VehicleManager manager = Singleton<VehicleManager>.instance;
            ColossalFramework.Math.Randomizer randomizer = Singleton<SimulationManager>.instance.m_randomizer;

            Building building = BuildingHelper.GetBuilding(serviceBuilding.BuildingId);

            VehicleInfo info = manager.GetRandomVehicleInfo(ref randomizer, building.Info.m_class.m_service, building.Info.m_class.m_subService, building.Info.m_class.m_level);
            if (info == null)
            {
                Log.Debug(typeof(VehicleKeeper), "CreateVehicle", "GetRandomVehicleInfo", "no vehicle");
                return null;
            }

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

            if (!manager.CreateVehicle(out vehicleId, ref randomizer, info, building.m_position, material, transferToSource, transferToTarget))
            {
                Log.Debug(typeof(VehicleKeeper), "CreateVehicle", "CreateVehicle", "not created");
                return null;
            }

            info.m_vehicleAI.SetSource(vehicleId, ref manager.m_vehicles.m_buffer[vehicleId], serviceBuilding.BuildingId);

            if (targetBuildingId != 0 && !SetTarget(vehicleId, targetBuildingId))
            {
                Log.Debug(typeof(VehicleKeeper), "CreateVehicle", "SetTarget", "target not set");
                return null;
            }

            return info;
        }

        /// <summary>
        /// Removes target assignment from vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <returns>True if vehicle de-assigned and ok.</returns>
        /// <param name="recall">If set to <c>true</c> recall vehicle to service building.</param>
        public static bool DeAssign(ushort vehicleId, bool recall = false)
        {
            return DeAssign(vehicleId, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId], recall);
        }

        /// <summary>
        /// Removes target assignment from vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle data.</param>
        /// <returns>True if vehicle de-assigned and ok.</returns>
        /// <param name="recall">If set to <c>true</c> recall vehicle to service building.</param>
        public static bool DeAssign(ushort vehicleId, ref Vehicle vehicle, bool recall = false)
        {
            try
            {
                // Vehicle has spawned but not moved, just unspawn.
                Vector3 spawnPos = GetSpawnPosition(vehicleId, vehicle.Info, vehicle.m_sourceBuilding);
                if (vehicle.m_frame0.m_position == spawnPos && vehicle.m_frame1.m_position == spawnPos && vehicle.m_frame2.m_position == spawnPos && vehicle.m_frame3.m_position == spawnPos)
                {
                    Log.Debug(typeof(VehicleHelper), "DeAssign", "DeSpawn", vehicleId, vehicle, vehicle.Info.m_vehicleAI);

                    vehicle.m_flags &= ~Vehicle.Flags.WaitingSpace;
                    vehicle.m_flags &= ~Vehicle.Flags.WaitingPath;
                    vehicle.Unspawn(vehicleId);

                    ////if (Log.LogToFile && Log.LogALot)
                    ////{
                    ////    DebugListLog(vehicleId);
                    ////}

                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(VehicleHelper), "DeAssign", ex);
            }

            bool targetSet = SetTarget(vehicleId, ref vehicle, 0);

            if ((vehicle.m_flags & Vehicle.Flags.WaitingTarget) != Vehicle.Flags.None)
            {
                Global.TransferOffersCleaningNeeded = true;

                if (recall)
                {
                    if (Log.LogALot && Log.LogToFile)
                    {
                        Log.DevDebug(typeof(VehicleHelper), "DeAssign", "Recall", vehicleId, vehicle.m_targetBuilding, vehicle.m_flags);
                    }

                    vehicle.m_waitCounter = byte.MaxValue - 1;
                }
            }

            return targetSet;
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
        /// Logs vehicle info for debug use.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        public static void DebugListLog(ushort vehicleId)
        {
            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            DebugListLog(vehicles, buildings, vehicleId);
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
        /// Saves a list of vehicle info for debug use.
        /// </summary>
        /// <exception cref="InvalidDataException">No vehicle objects.</exception>
        public static void DumpVehicles()
        {
            bool logNames = Log.LogNames;
            Log.LogNames = true;

            try
            {
                List<string> vehicleList = new List<string>();
                HashSet<ushort> listed = new HashSet<ushort>();

                Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
                Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

                for (ushort id = 0; id < vehicles.Length; id++)
                {
                    if (vehicles[id].m_flags != Vehicle.Flags.None && vehicles[id].m_leadingVehicle == 0)
                    {
                        listed.Add(id);
                        vehicleList.Add(DebugInfoMsg(vehicles, buildings, id, true).ToString());

                        if (vehicles[id].m_trailingVehicle != 0)
                        {
                            ushort trailerCount = 0;
                            ushort trailerId = vehicles[id].m_trailingVehicle;

                            while (trailerId != 0 && trailerCount < ushort.MaxValue)
                            {
                                if (!listed.Contains(trailerId))
                                {
                                    listed.Add(trailerId);
                                    vehicleList.Add(DebugInfoMsg(vehicles, buildings, trailerId, true).ToString());
                                }

                                trailerId = vehicles[trailerId].m_trailingVehicle;
                                trailerCount++;
                            }
                        }
                    }
                }

                for (ushort id = 0; id < vehicles.Length; id++)
                {
                    if (!listed.Contains(id) && vehicles[id].m_flags != Vehicle.Flags.None)
                    {
                        listed.Add(id);
                        vehicleList.Add(DebugInfoMsg(vehicles, buildings, id, true).ToString());
                    }
                }

                if (vehicleList.Count == 0)
                {
                    throw new InvalidDataException("No vehicle objects");
                }

                vehicleList.Add("");
                using (StreamWriter dumpFile = new StreamWriter(FileSystem.FilePathName(".Vehicles.txt"), false))
                {
                    dumpFile.Write(String.Join("\n", vehicleList.ToArray()).ConformNewlines());
                    dumpFile.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(VehicleKeeper), "DumpVehicles", ex);
            }

            Log.LogNames = logNames;
        }

        /// <summary>
        /// Gets the name of the district.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <returns>The name of the district.</returns>
        public static string GetDistrictName(ushort vehicleId)
        {
            if (!Log.LogNames)
            {
                return null;
            }

            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            if ((vehicles[vehicleId].m_flags & VehicleExists) == Vehicle.Flags.None)
            {
                return null;
            }

            return DistrictHelper.GetDistrictName(vehicles[vehicleId].GetLastFramePosition());
        }

        /// <summary>
        /// Gets the spawn position.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicleInfo">The vehicle information.</param>
        /// <param name="buildingId">The building identifier.</param>
        /// <returns>The spawn position.</returns>
        public static Vector3 GetSpawnPosition(ushort vehicleId, VehicleInfo vehicleInfo, ushort buildingId)
        {
            try
            {
                Building building = BuildingHelper.GetBuilding(buildingId);
                Randomizer randomizer = new Randomizer((int)buildingId);
                Vector3 position;
                Vector3 target;
                building.Info.m_buildingAI.CalculateSpawnPosition(buildingId, ref building, ref randomizer, vehicleInfo, out position, out target);

                return position;
            }
            catch (Exception)
            {
                return Vector3.zero;
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
        /// Initializes this class.
        /// </summary>
        public static void Initialize()
        {
        }

        /// <summary>
        /// Sets the vehicles' target.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <returns>True if target set and path to target found.</returns>
        public static bool SetTarget(ushort vehicleId, ushort targetBuildingId)
        {
            return SetTarget(vehicleId, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId], targetBuildingId);
        }

        /// <summary>
        /// Sets the vehicles' target.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <returns>
        /// True if target set and path to target found.
        /// </returns>
        public static bool SetTarget(ushort vehicleId, ref Vehicle vehicle, ushort targetBuildingId)
        {
            try
            {
                if (Log.LogALot && Log.LogToFile)
                {
                    Log.DevDebug(typeof(VehicleHelper), "SetTarget", vehicleId, targetBuildingId, vehicle.m_targetBuilding, vehicle.m_flags);
                }

                if (targetBuildingId != 0)
                {
                    vehicle.m_flags &= ~Vehicle.Flags.GoingBack;
                }

                vehicle.Info.m_vehicleAI.SetTarget(vehicleId, ref vehicle, targetBuildingId);

                return (vehicle.m_flags & VehicleHelper.VehicleExists) != Vehicle.Flags.None;
            }
            catch (Exception ex)
            {
                Log.Error(typeof(VehicleHelper), "SetTarget", ex);
                return false;
            }
        }

        /// <summary>
        /// Collects vehicle debug information.
        /// </summary>
        /// <param name="vehicles">The vehicles.</param>
        /// <param name="buildings">The buildings.</param>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="verbose">If set to <c>true</c> include more information.</param>
        /// <returns>
        /// Vehicle information.
        /// </returns>
        private static Log.InfoList DebugInfoMsg(Vehicle[] vehicles, Building[] buildings, ushort vehicleId, bool verbose = false)
        {
            Log.InfoList info = new Log.InfoList();

            InstanceID instanceId;
            float prgCur, prgMax;
            int bufCur, bufMax;
            string localeKey;
            float distance;

            info.Add("VehicleId", vehicleId);

            if (verbose)
            {
                info.Add("LeadingVehicle", vehicles[vehicleId].m_leadingVehicle);
                info.Add("TrailingVehicle", vehicles[vehicleId].m_trailingVehicle);
                info.Add("CargoParent", vehicles[vehicleId].m_cargoParent);
            }

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

            if (verbose)
            {
                if (vehicles[vehicleId].m_leadingVehicle == 0 && vehicles[vehicleId].m_trailingVehicle != 0)
                {
                    ushort trailerCount = 0;
                    ushort trailerId = vehicles[vehicleId].m_trailingVehicle;

                    while (trailerId != 0 && trailerCount < ushort.MaxValue)
                    {
                        trailerId = vehicles[trailerId].m_trailingVehicle;
                        trailerCount++;
                    }

                    info.Add("TrailerCount", trailerCount);
                }

                info.Add("WaitCounter", vehicles[vehicleId].m_waitCounter);
                info.Add("Position", vehicles[vehicleId].GetLastFramePosition());

                if (Global.Vehicles != null)
                {
                    if (Global.Vehicles.StuckVehicles != null)
                    {
                        StuckVehicleInfo stuckVehicle;
                        if (Global.Vehicles.StuckVehicles.TryGetValue(vehicleId, out stuckVehicle))
                        {
                            stuckVehicle.AddDebugInfoData(info);
                        }
                    }
                }
            }

            info.Add("AI", vehicles[vehicleId].Info.m_vehicleAI.GetType().AssemblyQualifiedName);

            return info;
        }

        /// <summary>
        /// Log vehicle info for debug use.
        /// </summary>
        /// <param name="vehicles">The vehicles.</param>
        /// <param name="buildings">The buildings.</param>
        /// <param name="vehicleId">The vehicle identifier.</param>
        private static void DebugListLog(Vehicle[] vehicles, Building[] buildings, ushort vehicleId)
        {
            if (vehicles[vehicleId].Info != null && (vehicles[vehicleId].m_flags & VehicleHelper.VehicleExists) != Vehicle.Flags.None &&
                (vehicles[vehicleId].Info.m_vehicleAI is HearseAI || vehicles[vehicleId].Info.m_vehicleAI is GarbageTruckAI))
            {
                Log.DevDebug(typeof(VehicleKeeper), "DebugListLog", DebugInfoMsg(vehicles, buildings, vehicleId).ToString());
            }
        }
    }
}