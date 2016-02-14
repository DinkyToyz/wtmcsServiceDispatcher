using System;
using System.Collections.Generic;
using System.Reflection;
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
        /// The start path find methods.
        /// </summary>
        private static Dictionary<Type, MethodInfo> startPathFindMethods = new Dictionary<Type, MethodInfo>();

        /// <summary>
        /// The unhandled AIs.
        /// </summary>
        private static HashSet<Type> unhandledAIs = new HashSet<Type>();

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
        /// De-assign target from vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="force">If set to <c>true</c> de-assign vehicle even if it is not assigned.</param>
        public static void DeAssignVehicle(ushort vehicleId, bool force = false)
        {
            DeAssignVehicle(vehicleId, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId], force);
        }

        /// <summary>
        /// De-assign target from vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle data.</param>
        /// <param name="force">If set to <c>true</c> de-assign vehicle even if it is not assigned.</param>
        public static void DeAssignVehicle(ushort vehicleId, ref Vehicle vehicle, bool force = false)
        {
            if (vehicle.m_targetBuilding != 0 || force)
            {
                vehicle.Info.m_vehicleAI.SetTarget(vehicleId, ref vehicle, 0);
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
        /// Initializes this class.
        /// </summary>
        public static void Initialize()
        {
            startPathFindMethods = new Dictionary<Type, MethodInfo>();
            unhandledAIs = new HashSet<Type>();
        }

        /// <summary>
        /// Recalls the vehicle to the service building.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <returns>True if vehicle recalled and found path the source.</returns>
        public static bool RecallVehicle(ushort vehicleId)
        {
            return RecallVehicle(vehicleId, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId]);
        }

        /// <summary>
        /// Recalls the vehicle to the service building.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle data.</param>
        /// <returns>True if vehicle recalled and found path the source.</returns>
        /// <exception cref="System.InvalidOperationException">Vehicle is not using hearse of garbage truck ai.</exception>
        public static bool RecallVehicle(ushort vehicleId, ref Vehicle vehicle)
        {
            try
            {
                // If already going back, no need to do anything.
                if (vehicle.m_targetBuilding == 0 && (vehicle.m_flags & Vehicle.Flags.GoingBack) == Vehicle.Flags.GoingBack)
                {
                    if (vehicle.m_path != 0 && (vehicle.m_flags & Vehicle.Flags.Spawned) == Vehicle.Flags.Spawned)
                    {
                        return true;
                    }

                    return SetTargetAgain(vehicleId, ref vehicle);
                }

                // Vehicle has spawned but not moved, just unspawn.
                Vector3 spawnPos = GetSpawnPosition(vehicleId, vehicle.Info, vehicle.m_sourceBuilding);
                if (vehicle.m_frame0.m_position == spawnPos && vehicle.m_frame1.m_position == spawnPos && vehicle.m_frame2.m_position == spawnPos && vehicle.m_frame3.m_position == spawnPos)
                {
                    Log.Debug(typeof(VehicleHelper), "RecallVehicle", "DeSpawn", vehicleId, vehicle, vehicle.Info.m_vehicleAI);
                    vehicle.Unspawn(vehicleId);

                    return true;
                }

                // Make sure <AI>.StartPathFind is available.
                if (!InitStartPathFind(vehicle.Info.m_vehicleAI))
                {
                    Log.Debug(typeof(VehicleHelper), "RecallVehicle", "SetTarget", vehicleId, vehicle, vehicle.Info.m_vehicleAI);

                    vehicle.Info.m_vehicleAI.SetTarget(vehicleId, ref vehicle, 0);
                    return (vehicle.m_flags & Vehicle.Flags.Spawned) != Vehicle.Flags.None;
                }

                Log.Debug(typeof(VehicleHelper), "RecallVehicle", vehicleId, vehicle, vehicle.Info.m_vehicleAI);

                try
                {
                    // From original GarbageTruckAI/HeasreAI.SetTarget and GarbageTruckAI/HearseAI.RemoveTarget code at game version 1.2.2 f3.
                    Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)vehicle.m_targetBuilding].RemoveGuestVehicle(vehicleId, ref vehicle);
                    vehicle.m_targetBuilding = 0;
                    vehicle.m_flags &= ~Vehicle.Flags.WaitingTarget;
                    vehicle.m_waitCounter = (byte)0;
                    vehicle.m_flags |= Vehicle.Flags.GoingBack;

                    if (CallStartPathFind(vehicleId, ref vehicle))
                    {
                        return true;
                    }

                    Log.Debug(typeof(VehicleHelper), "RecallVehicle", "UnSpawn", vehicleId, vehicle, vehicle.Info.m_vehicleAI);
                    vehicle.Unspawn(vehicleId);
                    return false;
                }
                catch (Exception exp)
                {
                    Log.Error(typeof(VehicleHelper), "RecallVehicle", exp);
                    FailStartPathFind(vehicle.Info.m_vehicleAI);

                    DeAssignVehicle(vehicleId, ref vehicle, true);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(VehicleHelper), "RecallVehicle", ex);
                return false;
            }
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
        /// <returns>True if target set and path to target found.</returns>
        public static bool SetTarget(ushort vehicleId, ref Vehicle vehicle, ushort targetBuildingId)
        {
            try
            {
                // If moving to target (emptying) call original.
                if ((vehicle.m_flags & Vehicle.Flags.TransferToTarget) == Vehicle.Flags.TransferToTarget)
                {
                    vehicle.Info.m_vehicleAI.SetTarget(vehicleId, ref vehicle, targetBuildingId);
                    return (vehicle.m_flags & Vehicle.Flags.Spawned) != Vehicle.Flags.None;
                }

                // If target is 0, recall vehicle.
                if (targetBuildingId == 0)
                {
                    return RecallVehicle(vehicleId, ref vehicle);
                }

                // Vehicle aldready has same target.
                if (targetBuildingId == vehicle.m_targetBuilding)
                {
                    return SetTargetAgain(vehicleId, ref vehicle);
                }

                // Make sure <AI>.StartPathFind is available.
                if (!InitStartPathFind(vehicle.Info.m_vehicleAI))
                {
                    Log.Debug(typeof(VehicleHelper), "SetTarget", "SetTarget", vehicleId, vehicle, vehicle.Info.m_vehicleAI);

                    vehicle.Info.m_vehicleAI.SetTarget(vehicleId, ref vehicle, targetBuildingId);
                    return (vehicle.m_flags & Vehicle.Flags.Spawned) != Vehicle.Flags.None;
                }

                Log.Debug(typeof(VehicleHelper), "SetTarget", vehicleId, vehicle, vehicle.Info.m_vehicleAI);

                Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

                // From original GarbageTruckAI/HeasreAI.SetTarget and GarbageTruckAI/HearseAI.RemoveTarget code at game version 1.2.2 f3.
                if (vehicle.m_targetBuilding != 0)
                {
                    buildings[vehicle.m_targetBuilding].RemoveGuestVehicle(vehicleId, ref vehicle);
                }

                vehicle.m_targetBuilding = targetBuildingId;
                vehicle.m_flags &= ~Vehicle.Flags.WaitingTarget;
                vehicle.m_waitCounter = (byte)0;

                buildings[targetBuildingId].AddGuestVehicle(vehicleId, ref vehicle);
                if ((buildings[targetBuildingId].m_flags & Building.Flags.IncomingOutgoing) != Building.Flags.None)
                {
                    if ((vehicle.m_flags & Vehicle.Flags.TransferToTarget) != Vehicle.Flags.None)
                    {
                        vehicle.m_flags |= Vehicle.Flags.Exporting;
                    }
                    else if ((vehicle.m_flags & Vehicle.Flags.TransferToSource) != Vehicle.Flags.None)
                    {
                        vehicle.m_flags |= Vehicle.Flags.Importing;
                    }
                }

                // Find path to target.
                if (CallStartPathFind(vehicleId, ref vehicle))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(VehicleHelper), "SetTarget", ex);
                return false;
            }

            // Error, or path not found. Recall vehicle.
            RecallVehicle(vehicleId, ref vehicle);
            return false;
        }

        /// <summary>
        /// Calls the the AIs StartPathFind.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>
        /// AIs StartPathFind methods return value.
        /// </returns>
        /// <exception cref="System.NullReferenceException">Vehicle AI StartPathFind not initialized.</exception>
        private static bool CallStartPathFind(ushort vehicleId, ref Vehicle vehicle)
        {
            MethodInfo methodInfo;
            if (startPathFindMethods.TryGetValue(vehicle.Info.m_vehicleAI.GetType(), out methodInfo))
            {
                Log.DevDebug(typeof(VehicleHelper), "StartPathFind", vehicleId, vehicle, vehicle.Info.m_vehicleAI);
                if ((bool)methodInfo.Invoke(vehicle.Info.m_vehicleAI, new object[] { vehicleId, vehicle }))
                {
                    return true;
                }

                ushort buildingId = (vehicle.m_targetBuilding == 0) ? vehicle.m_sourceBuilding : vehicle.m_targetBuilding;
                Log.Warning(typeof(VehicleHelper), "CallStartPathFind", "PathNotFound", vehicleId, buildingId, vehicle.Info.m_vehicleAI, GetVehicleName(vehicleId), BuildingHelper.GetBuildingName(buildingId));

                return false;
            }
            else
            {
                throw new NullReferenceException("Vehicle AI StartPathFind not initialized");
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

        /// <summary>
        /// Mark the AIs StartPathFind as failed.
        /// </summary>
        /// <param name="vehicleAI">The vehicle AI.</param>
        private static void FailStartPathFind(VehicleAI vehicleAI)
        {
            startPathFindMethods[vehicleAI.GetType()] = null;
        }

        /// <summary>
        /// Initializes method info for the vehicle AIs StartPathFind.
        /// </summary>
        /// <param name="vehicleAI">The vehicle AI.</param>
        /// <returns>True if method info initialized.</returns>
        /// <exception cref="System.NullReferenceException">
        /// Method info not returned.
        /// </exception>
        private static bool InitStartPathFind(VehicleAI vehicleAI)
        {
            if (!Global.Settings.UseReflection)
            {
                return false;
            }
            
            if (vehicleAI is HearseAI || vehicleAI is GarbageTruckAI)
            {
                MethodInfo methodInfo;
                if (startPathFindMethods.TryGetValue(vehicleAI.GetType(), out methodInfo))
                {
                    return methodInfo != null;
                }

                try
                {
                    Log.DevDebug(typeof(VehicleHelper), "InitStartPathFind", "FindMethod", vehicleAI);
                    methodInfo = MonoDetour.FindMethod(vehicleAI.GetType(), "StartPathFind", typeof(VehicleHelper), "StartPathFind_Signature");

                    if (methodInfo == null)
                    {
                        throw new NullReferenceException("Method info not returned");
                    }

                    startPathFindMethods[vehicleAI.GetType()] = methodInfo;
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Warning(typeof(VehicleHelper), "InitStartPathFind", "Failed", vehicleAI, ex.GetType().Name, ex.Message);
                    startPathFindMethods[vehicleAI.GetType()] = null;

                    return false;
                }
            }

            if (!unhandledAIs.Contains(vehicleAI.GetType()))
            {
                unhandledAIs.Add(vehicleAI.GetType());
                Log.Warning(typeof(VehicleHelper), "InitStartPathFind", "UnhandledAI", vehicleAI);
            }

            return false;
        }

        /// <summary>
        /// Sets the same vehicle target as already set.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>True if path found or vehicle spawned.</returns>
        private static bool SetTargetAgain(ushort vehicleId, ref Vehicle vehicle)
        {
            // From original GarbageTruckAI/HeasreAI.SetTarget code at game version 1.2.2 f3.
            if (vehicle.m_path == 0)
            {
                // Make sure <AI>.StartPathFind is available.
                if (!InitStartPathFind(vehicle.Info.m_vehicleAI))
                {
                    Log.Debug(typeof(VehicleHelper), "SetTargetAgain", "SetTarget", vehicleId, vehicle, vehicle.Info.m_vehicleAI);

                    vehicle.Info.m_vehicleAI.SetTarget(vehicleId, ref vehicle, vehicle.m_targetBuilding);
                    return (vehicle.m_flags & Vehicle.Flags.Spawned) != Vehicle.Flags.None;
                }

                Log.Debug(typeof(VehicleHelper), "SetTargetAgain", "StartPathFind", vehicleId, vehicle, vehicle.Info.m_vehicleAI);
                if (CallStartPathFind(vehicleId, ref vehicle))
                {
                    return true;
                }

                Log.Debug(typeof(VehicleHelper), "SetTargetAgain", "UnSpawn", vehicleId, vehicle, vehicle.Info.m_vehicleAI);
                vehicle.Unspawn(vehicleId);
                return false;
            }

            if ((vehicle.m_flags & Vehicle.Flags.Spawned) == Vehicle.Flags.Spawned)
            {
                return true;
            }

            return vehicle.Info.m_vehicleAI.TrySpawn(vehicleId, ref vehicle);
        }

        /// <summary>
        /// StartPathFind method signature.
        /// </summary>
        /// <param name="vehicleAI">The vehicle AI.</param>
        /// <param name="vehicleID">The vehicle identifier.</param>
        /// <param name="vehicleData">The vehicle data.</param>
        /// <returns>True if path find worked.</returns>
        /// <exception cref="System.NotImplementedException">Call to method signature.</exception>
        private static bool StartPathFind_Signature(VehicleAI vehicleAI, ushort vehicleID, ref Vehicle vehicleData)
        {
            throw new NotImplementedException("Call to method signature");
        }
    }
}