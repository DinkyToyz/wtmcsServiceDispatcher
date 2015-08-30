using ColossalFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Vehicle data.
    /// </summary>
    internal class Vehicles
    {
        /// <summary>
        /// The data is initialized.
        /// </summary>
        private bool isInitialized = false;

        /// <summary>
        /// The vehicles that have been removed from grid.
        /// </summary>
        private HashSet<ushort> removedFromGrid = new HashSet<ushort>();

        /// <summary>
        /// The current/last building frame.
        /// </summary>
        private uint vehicleFrame;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vehicles"/> class.
        /// </summary>
        public Vehicles()
        {
            Log.Debug(this, "Constructed");
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
                Log.Error(typeof(Vehicles), "DebugListLog()", ex);
            }
        }

        /// <summary>
        /// Logs a list of vehicle info for debug use.
        /// </summary>
        public static void DebugListLog(Buildings.ServiceBuildingInfo building)
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
                        throw new Exception("Loop counter too high!");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Vehicles), "DebugListLog", ex);
            }
        }

        /// <summary>
        /// Gets the name of the vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <returns></returns>
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
        /// Updates data.
        /// </summary>
        public void Update()
        {
            // Get and categorize vehicles.
            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            // First update?
            if (isInitialized)
            {
                // Data is initialized. Just check buildings for this frame.

                uint endFrame = GetFrameEnd();

                uint counter = 0;
                while (vehicleFrame != endFrame)
                {
                    if (counter > 256)
                    {
                        throw new Exception("Update frame loop counter to high!");
                    }
                    counter++;

                    vehicleFrame = GetFrameNext(vehicleFrame + 1);
                    FrameBoundaries bounds = GetFrameBoundaries(vehicleFrame);

                    HandleVehicles(ref vehicles, buildings, bounds.FirstId, bounds.LastId);
                }
            }
            else
            {
                // Data is not initialized. Check all vehicles.
                HandleVehicles(ref vehicles, buildings, 0, vehicles.Length);

                vehicleFrame = GetFrameEnd();
                isInitialized = Global.FramedUpdates;
            }
        }

        /// <summary>
        /// Logs a list of vehicle info for debug use.
        /// </summary>
        private static void DebugListLog(IEnumerable<ushort> vehicleIds)
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
        private static void DebugListLog(IEnumerable<ServiceVehicleInfo> serviceVehicles)
        {
            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            foreach (ServiceVehicleInfo vehicle in serviceVehicles)
            {
                DebugListLog(vehicles, buildings, vehicle.VehicleId);
            }
        }

        /// <summary>
        /// Log vehicle info for debug use.
        /// </summary>
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
                    name = Buildings.GetBuildingName(vehicles[vehicleId].m_sourceBuilding);
                    if (String.IsNullOrEmpty(name))
                    {
                        name = buildings[vehicles[vehicleId].m_sourceBuilding].Info.name;
                    }

                    info.Add("Source", vehicles[vehicleId].m_sourceBuilding, name, distance);
                }

                if (vehicles[vehicleId].m_targetBuilding != 0 && buildings[vehicles[vehicleId].m_targetBuilding].Info != null)
                {
                    distance = (vehicles[vehicleId].GetLastFramePosition() - buildings[vehicles[vehicleId].m_targetBuilding].m_position).sqrMagnitude;
                    name = Buildings.GetBuildingName(vehicles[vehicleId].m_targetBuilding);
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

                Log.DevDebug(typeof(Vehicles), "DebugListLog", info.ToString());
            }
        }

        /// <summary>
        /// Gets the frame boundaries.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>The frame boundaries.</returns>
        private FrameBoundaries GetFrameBoundaries(uint frame)
        {
            frame = frame & 15;

            return new FrameBoundaries(frame * 1024, (frame + 1) * 1024 - 1);
        }

        /// <summary>
        /// Gets the end frame.
        /// </summary>
        /// <returns>The end frame.</returns>
        private uint GetFrameEnd()
        {
            return Singleton<SimulationManager>.instance.m_currentFrameIndex & 15;
        }

        /// <summary>
        /// Gets the next frame.
        /// </summary>
        /// <param name="frame">The current/last frame.</param>
        /// <returns>The next frame.</returns>
        private uint GetFrameNext(uint frame)
        {
            return frame & 15;
        }

        /// <summary>
        /// Categorizes the vehicles.
        /// </summary>
        /// <param name="vehicles">The vehicles.</param>
        /// <param name="buildings">The buildings.</param>
        /// <param name="firstVehicleId">The first vehicle identifier.</param>
        /// <param name="lastVehicleId">The last vehicle identifier.</param>
        private void HandleVehicles(ref Vehicle[] vehicles, Building[] buildings, ushort firstVehicleId, int lastVehicleId)
        {
            for (ushort id = firstVehicleId; id < lastVehicleId; id++)
            {
                if (vehicles[id].m_leadingVehicle != 0 || vehicles[id].m_cargoParent != 0 || vehicles[id].Info == null || (vehicles[id].m_flags & Vehicle.Flags.Spawned) == Vehicle.Flags.None)
                {
                    if (removedFromGrid != null && removedFromGrid.Contains(id))
                    {
                        removedFromGrid.Remove(id);
                    }
                }
                else
                {
                    if ((vehicles[id].m_flags & Vehicle.Flags.TransferToSource) != Vehicle.Flags.None && (vehicles[id].m_flags & (Vehicle.Flags.TransferToTarget | Vehicle.Flags.Arriving | Vehicle.Flags.Stopped)) == Vehicle.Flags.None &&
                        vehicles[id].m_targetBuilding != 0 && vehicles[id].m_targetBuilding != vehicles[id].m_sourceBuilding && (buildings[vehicles[id].m_sourceBuilding].m_flags & Building.Flags.Downgrading) == Building.Flags.None)
                    {
                        if (Global.HearseDispatcher != null && vehicles[id].m_transferType == Global.HearseDispatcher.transferType)
                        {
                            Global.HearseDispatcher.CheckVehicleTarget(id, ref vehicles[id]);
                        }

                        if (Global.GarbageTruckDispatcher != null && vehicles[id].m_transferType == Global.GarbageTruckDispatcher.transferType)
                        {
                            Global.GarbageTruckDispatcher.CheckVehicleTarget(id, ref vehicles[id]);
                        }
                    }

                    if (removedFromGrid != null)
                    {
                        if ((vehicles[id].m_flags & Vehicle.Flags.Stopped) == Vehicle.Flags.None)
                        {
                            if (removedFromGrid.Contains(id))
                            {
                                if (Log.LogToFile) Log.Debug(this, "HandleVehicles", "Moving", id, vehicles[id].m_targetBuilding, vehicles[id].Info.name, GetVehicleName(id), vehicles[id].m_flags);

                                removedFromGrid.Remove(id);
                            }
                        }
                        else if (((Global.Settings.RemoveHearsesFromGrid && vehicles[id].Info.m_vehicleAI is HearseAI) ||
                                  (Global.Settings.RemoveGarbageTrucksFromGrid && vehicles[id].Info.m_vehicleAI is GarbageTruckAI)) &&
                                 !removedFromGrid.Contains(id))
                        {
                            if (Log.LogToFile) Log.Debug(this, "HandleVehicles", "RemoveFromGrid", id, vehicles[id].m_targetBuilding, vehicles[id].Info.name, GetVehicleName(id), vehicles[id].m_flags);

                            Singleton<VehicleManager>.instance.RemoveFromGrid(id, ref vehicles[id], false);
                            removedFromGrid.Add(id);
                        }
                    }
                }
            }
        }

        public class ServiceVehicleInfo
        {
            /// <summary>
            /// The vehilce is free.
            /// </summary>
            public bool FreeToCollect;

            /// <summary>
            /// The last seen stamp.
            /// </summary>
            public uint LastSeen;

            /// <summary>
            /// The position.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// The target
            /// </summary>
            public ushort Target;

            /// <summary>
            /// The vehicle identifier.
            /// </summary>
            public ushort VehicleId;

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceVehicleInfo" /> class.
            /// </summary>
            /// <param name="vehicleId">The vehicle identifier.</param>
            /// <param name="vehicle">The vehicle.</param>
            /// <param name="freeToCollect">if set to <c>true</c> the vehile is free.</param>
            public ServiceVehicleInfo(ushort vehicleId, ref Vehicle vehicle, bool freeToCollect)
            {
                this.VehicleId = vehicleId;

                this.Update(ref vehicle, freeToCollect);
            }

            /// <summary>
            /// Gets the name of the vehicle.
            /// </summary>
            /// <value>
            /// The name of the vehicle.
            /// </value>
            public string VehicleName
            {
                get
                {
                    return GetVehicleName(VehicleId);
                }
            }

            /// <summary>
            /// Updates the specified vehicle.
            /// </summary>
            /// <param name="vehicle">The vehicle.</param>
            /// <param name="freeToCollect">if set to <c>true</c> the vehile is free.</param>
            public void Update(ref Vehicle vehicle, bool freeToCollect)
            {
                this.LastSeen = Global.CurrentFrame;
                this.Position = vehicle.GetLastFramePosition();
                this.Target = vehicle.m_targetBuilding;
                this.FreeToCollect = freeToCollect;
            }
        }
    }
}