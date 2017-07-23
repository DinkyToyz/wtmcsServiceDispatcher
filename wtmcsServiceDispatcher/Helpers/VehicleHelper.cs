using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Keeps track of interesting vehicles.
    /// </summary>
    internal static class VehicleHelper
    {
        /// <summary>
        /// All vehicle flags.
        /// </summary>
        public const Vehicle.Flags VehicleAll = Vehicle.AllFlags;

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
        public const Vehicle.Flags VehicleUnavailable = /*Vehicle.Flags.WaitingPath |*/ Vehicle.Flags.WaitingSpace | Vehicle.Flags.WaitingLoading | Vehicle.Flags.Deleted;

        /// <summary>
        /// Assigns target to vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="material">The material.</param>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <param name="targetCitizenId">The target citizen identifier.</param>
        /// <returns>The result of the action.</returns>
        /// <exception cref="System.InvalidOperationException">Ambulances must use citizen assignment
        /// or
        /// Material must be specified.</exception>
        /// <exception cref="System.NotImplementedException">Ambulance dispatching not fully implemented yet.</exception>
        public static VehicleResult AssignTarget(ushort vehicleId, ref Vehicle vehicle, TransferManager.TransferReason? material, ushort targetBuildingId, uint targetCitizenId)
        {
            if (targetBuildingId == 0 && targetCitizenId == 0)
            {
                return VehicleHelper.SetTarget(vehicleId, ref vehicle, targetBuildingId, targetCitizenId);
            }

            if (vehicle.Info.m_vehicleAI is AmbulanceAI && targetCitizenId == 0)
            {
                throw new InvalidOperationException("Ambulances must use citizen assignment");
            }

            if (Global.Settings.AssignmentCompatibilityMode == ServiceDispatcherSettings.ModCompatibilityMode.UseCustomCode)
            {
                if (vehicle.Info.m_vehicleAI is AmbulanceAI)
                {
                    throw new NotImplementedException("Ambulance dispatching not fully implemented yet");
                }

                return VehicleHelper.SetTarget(vehicleId, ref vehicle, targetBuildingId, targetCitizenId);
            }

            if (material == null || !material.HasValue)
            {
                if (vehicle.Info.m_vehicleAI is HearseAI)
                {
                    material = TransferManager.TransferReason.Dead;
                }
                else if (vehicle.Info.m_vehicleAI is GarbageTruckAI)
                {
                    material = TransferManager.TransferReason.Garbage;
                }
                else if (vehicle.Info.m_vehicleAI is AmbulanceAI)
                {
                    material = TransferManager.TransferReason.Sick;
                }
                else
                {
                    throw new InvalidOperationException("Material must be specified");
                }
            }

            return new VehicleResult(StartTransfer(vehicleId, ref vehicle, material.Value, targetBuildingId, targetCitizenId), VehicleResult.Result.Assigned);
        }

        /// <summary>
        /// Creates the service vehicle.
        /// </summary>
        /// <param name="serviceBuildingId">The service building identifier.</param>
        /// <param name="material">The material.</param>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <param name="targetCitizenId">The target citizen identifier.</param>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <returns>
        /// The vehicle information.
        /// </returns>
        /// <exception cref="System.NotImplementedException">Target citizen not implemented yet.</exception>
        /// <exception cref="System.InvalidOperationException">Hospital assigments reuires target citizen.</exception>
        /// <exception cref="System.ArgumentException">Unhandled material.</exception>
        /// <exception cref="ArgumentException">Unhandled material.</exception>
        public static VehicleInfo CreateServiceVehicle(ushort serviceBuildingId, TransferManager.TransferReason material, ushort targetBuildingId, uint targetCitizenId, out ushort vehicleId)
        {
            if (targetCitizenId != 0)
            {
                throw new NotImplementedException("Target citizen not implemented yet");
            }

            vehicleId = 0;

            VehicleManager manager = Singleton<VehicleManager>.instance;
            ColossalFramework.Math.Randomizer randomizer = Singleton<SimulationManager>.instance.m_randomizer;

            Building building = BuildingHelper.GetBuilding(serviceBuildingId);

            if (building.Info.m_buildingAI is HospitalAI && targetCitizenId == 0)
            {
                throw new InvalidOperationException("Hospital assigments reuires target citizen");
            }

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

            info.m_vehicleAI.SetSource(vehicleId, ref manager.m_vehicles.m_buffer[vehicleId], serviceBuildingId);

            if (targetBuildingId != 0 && !AssignTarget(vehicleId, ref manager.m_vehicles.m_buffer[vehicleId], material, targetBuildingId, 0))
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
        /// <param name="recall">If set to <c>true</c> recall vehicle to service building.</param>
        /// <returns>
        /// The result of the action.
        /// </returns>
        public static VehicleResult DeAssign(ushort vehicleId, bool recall = false)
        {
            return DeAssign(vehicleId, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId], recall);
        }

        /// <summary>
        /// Removes target assignment from vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle data.</param>
        /// <param name="recall">If set to <c>true</c> recall vehicle to service building.</param>
        /// <returns>
        /// The result of the action.
        /// </returns>
        public static VehicleResult DeAssign(ushort vehicleId, ref Vehicle vehicle, bool recall = false)
        {
            try
            {
                // Vehicle has spawned but not moved, just unspawn.
                Vector3 spawnPos = GetSpawnPosition(vehicleId, vehicle.Info, vehicle.m_sourceBuilding);

                if ((vehicle.m_frame0.m_position - spawnPos).sqrMagnitude < 1 &&
                    (vehicle.m_frame1.m_position - spawnPos).sqrMagnitude < 1 &&
                    (vehicle.m_frame2.m_position - spawnPos).sqrMagnitude < 1 &&
                    (vehicle.m_frame3.m_position - spawnPos).sqrMagnitude < 1)
                {
                    Log.Debug(typeof(VehicleHelper), "DeAssign", "DeSpawn", vehicleId, vehicle, vehicle.Info.m_vehicleAI);

                    if ((vehicle.m_flags & Vehicle.Flags.WaitingPath) == Vehicle.Flags.WaitingPath && vehicle.m_path != 0)
                    {
                        Singleton<PathManager>.instance.ReleasePath(vehicle.m_path);
                        vehicle.m_path = 0;
                        vehicle.m_flags &= ~Vehicle.Flags.WaitingPath;
                    }

                    vehicle.m_flags &= ~Vehicle.Flags.WaitingSpace;
                    DeSpawn(vehicleId);

                    return new VehicleResult(VehicleResult.Result.DeSpawned);
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(VehicleHelper), "DeAssign", ex);
            }

            VehicleResult result = SetTarget(vehicleId, ref vehicle, 0, 0);

            if ((vehicle.m_flags & Vehicle.Flags.WaitingTarget) != ~VehicleHelper.VehicleAll)
            {
                Global.TransferOffersCleaningNeeded = true;

                if (recall)
                {
                    //if (Log.LogALot)
                    //{
                    //    Log.DevDebug(typeof(VehicleHelper), "DeAssign", "Recall", vehicleId, vehicle.m_targetBuilding, vehicle.m_flags);
                    //}

                    vehicle.m_waitCounter = byte.MaxValue - 1;

                    if (result)
                    {
                        result = new VehicleResult(VehicleResult.Result.Recalled);
                    }
                }
            }

            return result;
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

        public static void DeSpawn(ushort vehicleId)
        {
            if (Log.LogALot)
            {
                Log.DevDebug(typeof(VehicleHelper), "DeSpawn", vehicleId);
            }

            VehicleManager manager = Singleton<VehicleManager>.instance;
            Vehicle[] vehicles = manager.m_vehicles.m_buffer;

            if (vehicles[vehicleId].m_flags == ~(Vehicle.Flags.Created | Vehicle.Flags.Deleted | Vehicle.Flags.Spawned | Vehicle.Flags.Inverted | Vehicle.Flags.TransferToTarget | Vehicle.Flags.TransferToSource | Vehicle.Flags.Emergency1 | Vehicle.Flags.Emergency2 | Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped | Vehicle.Flags.Leaving | Vehicle.Flags.Arriving | Vehicle.Flags.Reversed | Vehicle.Flags.TakingOff | Vehicle.Flags.Flying | Vehicle.Flags.Landing | Vehicle.Flags.WaitingSpace | Vehicle.Flags.WaitingCargo | Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget | Vehicle.Flags.Importing | Vehicle.Flags.Exporting | Vehicle.Flags.Parking | Vehicle.Flags.CustomName | Vehicle.Flags.OnGravel | Vehicle.Flags.WaitingLoading | Vehicle.Flags.Congestion | Vehicle.Flags.DummyTraffic | Vehicle.Flags.Underground | Vehicle.Flags.Transition | Vehicle.Flags.InsideBuilding | Vehicle.Flags.LeftHandDrive))
            {
                if (Log.LogALot)
                {
                    Log.DevDebug(typeof(VehicleHelper), "DeSpawn", vehicleId, "Unspawn");
                }

                vehicles[vehicleId].Unspawn(vehicleId);
                return;
            }

            if (Log.LogALot)
            {
                Log.DevDebug(typeof(VehicleHelper), "DeSpawn", vehicleId, "Release");
            }

            manager.ReleaseVehicle(vehicleId);

            if (!(vehicles[vehicleId].m_flags == ~(Vehicle.Flags.Created | Vehicle.Flags.Deleted | Vehicle.Flags.Spawned | Vehicle.Flags.Inverted | Vehicle.Flags.TransferToTarget | Vehicle.Flags.TransferToSource | Vehicle.Flags.Emergency1 | Vehicle.Flags.Emergency2 | Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped | Vehicle.Flags.Leaving | Vehicle.Flags.Arriving | Vehicle.Flags.Reversed | Vehicle.Flags.TakingOff | Vehicle.Flags.Flying | Vehicle.Flags.Landing | Vehicle.Flags.WaitingSpace | Vehicle.Flags.WaitingCargo | Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget | Vehicle.Flags.Importing | Vehicle.Flags.Exporting | Vehicle.Flags.Parking | Vehicle.Flags.CustomName | Vehicle.Flags.OnGravel | Vehicle.Flags.WaitingLoading | Vehicle.Flags.Congestion | Vehicle.Flags.DummyTraffic | Vehicle.Flags.Underground | Vehicle.Flags.Transition | Vehicle.Flags.InsideBuilding | Vehicle.Flags.LeftHandDrive)))
            {
                if (Log.LogALot)
                {
                    Log.DevDebug(typeof(VehicleHelper), "DeSpawn", vehicleId, "Unspawn");
                }

                vehicles[vehicleId].Unspawn(vehicleId);
            }
        }

        /// <summary>
        /// Saves a list of vehicle info for debug use.
        /// </summary>
        /// <exception cref="InvalidDataException">No vehicle objects.</exception>
        public static void DumpVehicles(bool openDumpedFile = false)
        {
            try
            {
                Global.DumpData("StuckVehicles", openDumpedFile, true, () =>
                {
                    List<KeyValuePair<string, string>> vehicleList = new List<KeyValuePair<string, string>>();

                    HashSet<ushort> listed = new HashSet<ushort>();

                    Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
                    Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

                    for (ushort id = 0; id < vehicles.Length; id++)
                    {
                        if ((vehicles[id].m_flags & VehicleHelper.VehicleAll) != ~VehicleHelper.VehicleAll && (vehicles[id].m_leadingVehicle == 0 || vehicles[id].m_leadingVehicle == id))
                        {
                            string sortPrefix;
                            try
                            {
                                sortPrefix = "0" + vehicles[id].Info.m_vehicleAI.GetType().ToString();
                            }
                            catch
                            {
                                sortPrefix = "0?";
                            }

                            sortPrefix += id.ToString().PadLeft(16, '0');

                            string sortValue = sortPrefix + "".PadLeft(16, '0');

                            listed.Add(id);
                            vehicleList.Add(new KeyValuePair<string, string>(sortValue, DebugInfoMsg(vehicles, buildings, id, true).ToString()));

                            if (vehicles[id].m_trailingVehicle != 0)
                            {
                                ushort trailerCount = 0;
                                ushort trailerId = vehicles[id].m_trailingVehicle;

                                while (trailerId != 0 && trailerCount < ushort.MaxValue)
                                {
                                    trailerCount++;

                                    if (!listed.Contains(trailerId))
                                    {
                                        sortValue = sortPrefix + trailerCount.ToString().PadLeft(16, '0');

                                        listed.Add(trailerId);
                                        vehicleList.Add(new KeyValuePair<string, string>(sortValue, DebugInfoMsg(vehicles, buildings, trailerId, true).ToString()));
                                    }

                                    trailerId = vehicles[trailerId].m_trailingVehicle;
                                }
                            }
                        }
                    }

                    for (ushort id = 0; id < vehicles.Length; id++)
                    {
                        string sortValue;
                        try
                        {
                            sortValue = "1" + vehicles[id].Info.m_vehicleAI.GetType().ToString();
                        }
                        catch
                        {
                            sortValue = "1?";
                        }

                        sortValue += id.ToString().PadLeft(16, '0');

                        if (!listed.Contains(id) && (vehicles[id].m_flags & VehicleHelper.VehicleAll) != ~VehicleHelper.VehicleAll)
                        {
                            listed.Add(id);
                            vehicleList.Add(new KeyValuePair<string, string>(sortValue, DebugInfoMsg(vehicles, buildings, id, true).ToString()));
                        }
                    }

                    return vehicleList.OrderBy(v => v.Key).SelectToArray(v => v.Value);
                });
            }
            catch (Exception ex)
            {
                Log.Error(typeof(VehicleKeeper), "DumpVehicles", ex, openDumpedFile);
            }
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

            if ((vehicles[vehicleId].m_flags & VehicleExists) == ~VehicleHelper.VehicleAll)
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
            string name;

            info.Add("VehicleId", vehicleId);

            if (verbose)
            {
                info.Add("LeadingVehicle", vehicles[vehicleId].m_leadingVehicle);
                info.Add("TrailingVehicle", vehicles[vehicleId].m_trailingVehicle);
                info.Add("CargoParent", vehicles[vehicleId].m_cargoParent);
            }

            try
            {
                info.Add("AI", vehicles[vehicleId].Info.m_vehicleAI.GetType());
                info.Add("InfoName", vehicles[vehicleId].Info.name);

                name = GetVehicleName(vehicleId);
                if (!String.IsNullOrEmpty(name) && name != vehicles[vehicleId].Info.name)
                {
                    info.Add("VehicleName", name);
                }
            }
            catch
            {
                info.Add("Error", "Info");
            }

            try
            {
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
            }
            catch
            {
                info.Add("Error", "Transfer");
            }

            try
            {
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
            }
            catch
            {
                info.Add("Error", "SourceBuilding");
            }

            try
            {
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
            }
            catch
            {
                info.Add("Error", "TargetBuilding");
            }

            try
            {
                string flags = vehicles[vehicleId].m_flags.ToString();
                if (flags.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }) >= 0)
                {
                    foreach (Vehicle.Flags flag in Enum.GetValues(typeof(Vehicle.Flags)))
                    {
                        if ((vehicles[vehicleId].m_flags & flag) == flag)
                        {
                            flags += ", " + flag.ToString();
                        }
                    }
                }
                info.Add("Flags", flags);
            }
            catch
            {
                info.Add("Error", "Flags");
            }

            try
            {
                info.Add("Enabled", vehicles[vehicleId].Info.enabled);
                info.Add("Active", vehicles[vehicleId].Info.isActiveAndEnabled);
                info.Add("AIEnabled", vehicles[vehicleId].Info.m_vehicleAI.enabled);
                info.Add("AIActive", vehicles[vehicleId].Info.m_vehicleAI.isActiveAndEnabled);
            }
            catch
            {
                info.Add("Error", "Info");
            }

            try
            {
                if (vehicles[vehicleId].Info.m_vehicleAI.GetProgressStatus(vehicleId, ref vehicles[vehicleId], out prgCur, out prgMax))
                {
                    info.Add("PrgCur", prgCur);
                    info.Add("PrgMax", prgMax);
                }
            }
            catch
            {
                info.Add("Error", "Progress");
            }

            try
            {
                vehicles[vehicleId].Info.m_vehicleAI.GetBufferStatus(vehicleId, ref vehicles[vehicleId], out localeKey, out bufCur, out bufMax);
                if (!String.IsNullOrEmpty(localeKey))
                {
                    info.Add("BufLocKey", localeKey);
                }
                info.Add("BufCur", bufCur);
                info.Add("BufMax", bufMax);
            }
            catch
            {
                info.Add("Error", "Buffer");
            }

            info.Add("TransferSize", vehicles[vehicleId].m_transferSize);

            try
            {
                if (vehicles[vehicleId].Info.m_vehicleAI is HearseAI)
                {
                    info.Add("Capacity", ((HearseAI)vehicles[vehicleId].Info.m_vehicleAI).m_corpseCapacity);
                }
                else if (vehicles[vehicleId].Info.m_vehicleAI is GarbageTruckAI)
                {
                    info.Add("Capacity", ((GarbageTruckAI)vehicles[vehicleId].Info.m_vehicleAI).m_cargoCapacity);
                }
                else if (vehicles[vehicleId].Info.m_vehicleAI is AmbulanceAI)
                {
                    info.Add("Capacity", ((AmbulanceAI)vehicles[vehicleId].Info.m_vehicleAI).m_patientCapacity);
                }
            }
            catch
            {
                info.Add("Error", "Capacity");
            }

            try
            {
                string status = vehicles[vehicleId].Info.m_vehicleAI.GetLocalizedStatus(vehicleId, ref vehicles[vehicleId], out instanceId);
                if (!String.IsNullOrEmpty(status))
                {
                    info.Add("Status", status);
                }
            }
            catch
            {
                info.Add("Error", "Status");
            }

            if (verbose)
            {
                try
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
                }
                catch
                {
                    info.Add("Error", "Trailing");
                }

                info.Add("WaitCounter", vehicles[vehicleId].m_waitCounter);

                try
                {
                    info.Add("Position", vehicles[vehicleId].GetLastFramePosition());
                }
                catch
                {
                    info.Add("Error", "Position");
                }

                if (Global.Vehicles != null)
                {
                    try
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
                    catch
                    {
                        info.Add("Error", "Stuck");
                    }
                }
            }

            try
            {
                info.Add("AI", vehicles[vehicleId].Info.m_vehicleAI.GetType().AssemblyQualifiedName);
            }
            catch
            {
                info.Add("Error", "AI");
            }

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
            if (vehicles[vehicleId].Info != null && (vehicles[vehicleId].m_flags & VehicleHelper.VehicleExists) != ~VehicleHelper.VehicleAll &&
                (vehicles[vehicleId].Info.m_vehicleAI is HearseAI || vehicles[vehicleId].Info.m_vehicleAI is GarbageTruckAI || vehicles[vehicleId].Info.m_vehicleAI is AmbulanceAI))
            {
                Log.DevDebug(typeof(VehicleKeeper), "DebugListLog", DebugInfoMsg(vehicles, buildings, vehicleId).ToString());
            }
        }

        /// <summary>
        /// Sets the vehicles' target.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <param name="targetCitizenId">The target citizen identifier.</param>
        /// <returns>The result of the action.</returns>
        private static VehicleResult SetTarget(ushort vehicleId, ref Vehicle vehicle, ushort targetBuildingId, uint targetCitizenId)
        {
            try
            {
                //if (Log.LogALot)
                //{
                //    Log.DevDebug(typeof(VehicleHelper), "SetTarget", vehicleId, targetBuildingId, targetCitizenId, vehicle.m_targetBuilding, vehicle.m_flags);
                //}

                if (targetCitizenId != 0)
                {
                    ushort targetCitizenBuildingId = Singleton<CitizenManager>.instance.m_citizens.m_buffer[targetCitizenId].GetBuildingByLocation();

                    if (targetBuildingId == 0)
                    {
                        targetBuildingId = targetCitizenBuildingId;
                    }
                    else if (targetCitizenBuildingId != targetBuildingId)
                    {
                        Log.Warning(typeof(VehicleHelper), "SetTarget", "Target citizen location not same as target building", targetBuildingId, targetCitizenBuildingId);
                        return new VehicleResult(VehicleResult.Result.Failure);
                    }
                }

                if (targetBuildingId != 0)
                {
                    vehicle.m_flags &= ~Vehicle.Flags.GoingBack;
                }

                vehicle.Info.m_vehicleAI.SetTarget(vehicleId, ref vehicle, targetBuildingId);
                //if (Log.LogALot)
                //{
                //    Log.DevDebug(typeof(VehicleHelper), "SetTarget", "Target Set", vehicleId, targetBuildingId, targetCitizenId, vehicle.m_targetBuilding, vehicle.m_flags, VehicleHelper.VehicleExists, VehicleHelper.VehicleAll);
                //}

                VehicleResult.Result resultValue = (targetBuildingId == 0) ? VehicleResult.Result.DeAssigned : VehicleResult.Result.Assigned;

                if (targetCitizenId == 0)
                {
                    return new VehicleResult((vehicle.m_flags & VehicleHelper.VehicleExists) != ~VehicleHelper.VehicleAll, resultValue);
                }
                else
                {
                    Citizen[] citizens = Singleton<CitizenManager>.instance.m_citizens.m_buffer;
                    citizens[targetCitizenId].SetVehicle(targetCitizenId, vehicleId, 0);

                    return new VehicleResult((vehicle.m_flags & VehicleHelper.VehicleExists) == ~VehicleHelper.VehicleAll && citizens[targetCitizenId].m_vehicle == vehicleId, resultValue);
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(VehicleHelper), "SetTarget", ex);
                return new VehicleResult(VehicleResult.Result.Failure);
            }
        }

        /// <summary>
        /// Starts the transfer.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="material">The material.</param>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <param name="targetCitizenId">The target citizen identifier.</param>
        /// <returns>True on success.</returns>
        private static bool StartTransfer(ushort vehicleId, ref Vehicle vehicle, TransferManager.TransferReason material, ushort targetBuildingId, uint targetCitizenId)
        {
            TransferManager.TransferOffer offer = TransferManagerHelper.MakeOffer(targetBuildingId, targetCitizenId);

            vehicle.m_flags &= ~Vehicle.Flags.GoingBack;
            vehicle.m_flags |= Vehicle.Flags.WaitingTarget;

            // Cast AI as games original AI so detoured methods are called, but not methods from not replaced classes.
            if (Global.Settings.AssignmentCompatibilityMode == ServiceDispatcherSettings.ModCompatibilityMode.UseInstanciatedClassMethods || !Global.Settings.AllowReflection())
            {
                vehicle.Info.m_vehicleAI.StartTransfer(vehicleId, ref vehicle, material, offer);
            }
            else if (vehicle.Info.m_vehicleAI is HearseAI)
            {
                ((HearseAI)vehicle.Info.m_vehicleAI.CastTo<HearseAI>()).StartTransfer(vehicleId, ref vehicle, material, offer);
            }
            else if (vehicle.Info.m_vehicleAI is GarbageTruckAI)
            {
                ((GarbageTruckAI)vehicle.Info.m_vehicleAI.CastTo<GarbageTruckAI>()).StartTransfer(vehicleId, ref vehicle, material, offer);
            }
            else if (vehicle.Info.m_vehicleAI is AmbulanceAI)
            {
                ((AmbulanceAI)vehicle.Info.m_vehicleAI.CastTo<AmbulanceAI>()).StartTransfer(vehicleId, ref vehicle, material, offer);
            }
            else
            {
                vehicle.Info.m_vehicleAI.StartTransfer(vehicleId, ref vehicle, material, offer);
            }

            if (vehicle.m_targetBuilding == targetBuildingId && (targetCitizenId == 0 || Singleton<CitizenManager>.instance.m_citizens.m_buffer[targetCitizenId].m_vehicle == vehicleId))
            {
                return true;
            }

            Log.Warning(typeof(VehicleHelper), "StartTransfer", "Target Not Assigned", vehicleId, targetBuildingId, targetCitizenId, material, vehicle.m_sourceBuilding, vehicle.m_targetBuilding, (TransferManager.TransferReason)vehicle.m_transferType, vehicle.m_flags);
            return false;
        }
    }
}