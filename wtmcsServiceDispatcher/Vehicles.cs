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
        /// Might log a list of vehicle info for debug use.
        /// </summary>
        public static void DebugListLog()
        {
            try
            {
                Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

                for (ushort id = 0; id < vehicles.Length; id++)
                {
                    if (vehicles[id].Info != null && (vehicles[id].m_flags & Vehicle.Flags.Spawned) == Vehicle.Flags.Spawned && (vehicles[id].Info.m_vehicleAI is HearseAI || vehicles[id].Info.m_vehicleAI is GarbageTruckAI))
                    {
                        List<string> info = new List<string>();

                        InstanceID instanceId;
                        float prgCur, prgMax;
                        int bufCur, bufMax;
                        string localeKey;

                        info.Add("VehicleId=" + id.ToString());
                        info.Add("AI=" + vehicles[id].Info.m_vehicleAI.GetType().ToString());
                        info.Add("InfoName='" + vehicles[id].Info.name + "'");

                        string name = GetVehicleName(id);
                        if (!String.IsNullOrEmpty(name) && name != vehicles[id].Info.name)
                        {
                            info.Add("VehicleName='" + name + "'");
                        }

                        string status = vehicles[id].Info.m_vehicleAI.GetLocalizedStatus(id, ref vehicles[id], out instanceId);
                        if (!String.IsNullOrEmpty(status))
                        {
                            info.Add("Status='" + status + "'");
                        }

                        info.Add("Source=" + vehicles[id].m_sourceBuilding.ToString());
                        info.Add("Target=" + vehicles[id].m_targetBuilding.ToString());

                        string flags = vehicles[id].m_flags.ToString();
                        if (flags.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }) >= 0)
                        {
                            foreach (Vehicle.Flags flag in Enum.GetValues(typeof(Vehicle.Flags)))
                            {
                                if (flag != Vehicle.Flags.None && (vehicles[id].m_flags & flag) == flag)
                                {
                                    flags += ", " + flag.ToString();
                                }
                            }
                        }
                        info.Add("Flags=" + flags);

                        info.Add("Enabled=" + vehicles[id].Info.enabled.ToString());
                        info.Add("Active=" + vehicles[id].Info.isActiveAndEnabled.ToString());
                        info.Add("AIEnabled=" + vehicles[id].Info.m_vehicleAI.enabled.ToString());
                        info.Add("AIActive=" + vehicles[id].Info.m_vehicleAI.isActiveAndEnabled.ToString());

                        if (vehicles[id].Info.m_vehicleAI.GetProgressStatus(id, ref vehicles[id], out prgCur, out prgMax))
                        {
                            info.Add("PrgCur=" + prgCur.ToString("F"));
                            info.Add("PrgMax=" + prgMax.ToString("F"));
                        }

                        vehicles[id].Info.m_vehicleAI.GetBufferStatus(id, ref vehicles[id], out localeKey, out bufCur, out bufMax);
                        if (!String.IsNullOrEmpty(localeKey))
                        {
                            info.Add("BufLocKey='" + localeKey + "'");
                        }
                        info.Add("BufCur=" + bufCur.ToString());
                        info.Add("BufMax=" + bufMax.ToString());

                        if (vehicles[id].Info.m_vehicleAI is HearseAI)
                        {
                            info.Add("Capacity=" + ((HearseAI)vehicles[id].Info.m_vehicleAI).m_corpseCapacity.ToString());
                        }

                        if (vehicles[id].Info.m_vehicleAI is GarbageTruckAI)
                        {
                            info.Add("Capacity=" + ((GarbageTruckAI)vehicles[id].Info.m_vehicleAI).m_cargoCapacity.ToString());
                        }

                        Log.DevDebug(typeof(Vehicles), "DebugListLog", String.Join("; ", info.ToArray()));
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

                    HandleVehicles(ref vehicles, bounds.FirstId, bounds.LastId);
                }
            }
            else
            {
                // Data is not initialized. Check all vehicles.
                HandleVehicles(ref vehicles, 0, vehicles.Length);

                vehicleFrame = GetFrameEnd();
                isInitialized = Global.FramedUpdates;
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
        /// <param name="firstVehicleId">The first vehicle identifier.</param>
        /// <param name="lastVehicleId">The last vehicle identifier.</param>
        private void HandleVehicles(ref Vehicle[] vehicles, ushort firstVehicleId, int lastVehicleId)
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
                    if (removedFromGrid != null)
                    {
                        if ((vehicles[id].m_flags & Vehicle.Flags.Stopped) == Vehicle.Flags.None)
                        {
                            if (removedFromGrid.Contains(id))
                            {
                                if (Log.LogToFile) Log.Debug(this, "HandleVehicles", "Moving", id, vehicles[id].Info.name, GetVehicleName(id));

                                removedFromGrid.Remove(id);
                            }
                        }
                        else if (((Global.Settings.RemoveHearsesFromGrid && vehicles[id].Info.m_vehicleAI is HearseAI) ||
                                  (Global.Settings.RemoveGarbageTrucksFromGrid && vehicles[id].Info.m_vehicleAI is GarbageTruckAI)) &&
                                 !removedFromGrid.Contains(id))
                        {
                            if (Log.LogToFile) Log.Debug(this, "HandleVehicles", "RemoveFromGrid", id, vehicles[id].Info.name, GetVehicleName(id));

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
            /// The vehcile is free.
            /// </summary>
            public bool CanCollect;

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
            /// <param name="canCollect">if set to <c>true</c> the vehile is free.</param>
            public ServiceVehicleInfo(ushort vehicleId, ref Vehicle vehicle, bool canCollect)
            {
                this.VehicleId = vehicleId;
                this.LastSeen = Global.CurrentFrame;
                this.Position = vehicle.GetLastFramePosition();
                this.Target = vehicle.m_targetBuilding;
                this.CanCollect = canCollect;
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
            /// <param name="canCollect">if set to <c>true</c> the vehile is free.</param>
            public void Update(ref Vehicle vehicle, bool canCollect)
            {
                this.LastSeen = Global.CurrentFrame;
                this.Position = vehicle.GetLastFramePosition();
                this.Target = vehicle.m_targetBuilding;
                this.CanCollect = canCollect;
            }
        }
    }
}