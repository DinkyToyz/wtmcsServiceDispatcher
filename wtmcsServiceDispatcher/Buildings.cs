using ColossalFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    internal class Buildings
    {
        /// <summary>
        /// The current/last building frame.
        /// </summary>
        private uint buildingFrame;

        /// <summary>
        /// The data is initialized.
        /// </summary>
        private bool isInitialized = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Buildings"/> class.
        /// </summary>
        public Buildings()
        {
            HasDeadPeopleBuildingsToCheck = false;
            HasDirtyBuildingsToCheck = false;

            if (Global.Settings.DispatchHearses)
            {
                DeadPeopleBuildings = new Dictionary<ushort, TargetBuildingInfo>();
                HearseBuildings = new Dictionary<ushort, ServiceBuildingInfo>();
            }
            else
            {
                DeadPeopleBuildings = null;
                HearseBuildings = null;
            }

            if (Global.Settings.DispatchGarbageTrucks)
            {
                DirtyBuildings = new Dictionary<ushort, TargetBuildingInfo>();
                GarbageBuildings = new Dictionary<ushort, ServiceBuildingInfo>();
            }
            else
            {
                DirtyBuildings = null;
                GarbageBuildings = null;
            }

            Log.Debug(this, "Constructed");
        }

        /// <summary>
        /// The dead people buildings.
        /// </summary>
        public Dictionary<ushort, TargetBuildingInfo> DeadPeopleBuildings { get; private set; }

        /// <summary>
        /// The dirty buildings.
        /// </summary>
        public Dictionary<ushort, TargetBuildingInfo> DirtyBuildings { get; private set; }

        /// <summary>
        /// The garbage buildings.
        /// </summary>
        public Dictionary<ushort, ServiceBuildingInfo> GarbageBuildings { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance has dead people buildings that should be checked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has dead people buildings to check; otherwise, <c>false</c>.
        /// </value>
        public bool HasDeadPeopleBuildingsToCheck { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance has dirty buildings that should be checked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has dirty buildings to check; otherwise, <c>false</c>.
        /// </value>
        public bool HasDirtyBuildingsToCheck { get; private set; }

        /// <summary>
        /// The hearse buildings.
        /// </summary>
        public Dictionary<ushort, ServiceBuildingInfo> HearseBuildings { get; private set; }

        /// <summary>
        /// Logs a list of building info for debug use.
        /// </summary>
        public static void DebugListLog()
        {
            try
            {
                Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
                Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

                for (ushort id = 0; id < buildings.Length; id++)
                {
                    DebugListLog(buildings, vehicles, id);
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Buildings), "DebugListLog", ex);
            }
        }

        /// <summary>
        /// Gets the name of the building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <returns></returns>
        public static string GetBuildingName(ushort buildingId)
        {
            if (!Log.LogNames)
            {
                return null;
            }

            try
            {
                BuildingManager manager = Singleton<BuildingManager>.instance;
                string name = manager.GetBuildingName(buildingId, new InstanceID());

                return String.IsNullOrEmpty(name) ? (string)null : name;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Logs a list of service building info for debug use.
        /// </summary>
        public void DebugListLogBuildings()
        {
            try
            {
                if (Global.Settings.DispatchHearses)
                {
                    if (DeadPeopleBuildings != null)
                    {
                        DebugListLog(DeadPeopleBuildings.Values);
                    }

                    if (HearseBuildings != null)
                    {
                        DebugListLog(HearseBuildings.Values);
                    }
                }

                if (Global.Settings.DispatchGarbageTrucks)
                {
                    if (DirtyBuildings != null)
                    {
                        DebugListLog(DirtyBuildings.Values);
                    }

                    if (GarbageBuildings != null)
                    {
                        DebugListLog(GarbageBuildings.Values);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Buildings), "DebugListLogBuildings", ex);
            }
        }

        /// <summary>
        /// Logs a list of service building info for debug use.
        /// </summary>
        public void DebugListLogServiceBuildings()
        {
            try
            {
                if (Global.Settings.DispatchHearses && HearseBuildings != null)
                {
                    DebugListLog(HearseBuildings.Values);
                }

                if (Global.Settings.DispatchGarbageTrucks && GarbageBuildings != null)
                {
                    DebugListLog(GarbageBuildings.Values);
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Buildings), "DebugListLogServiceBuildings", ex);
            }
        }

        /// <summary>
        /// Logs a list of service building info for debug use.
        /// </summary>
        public void DebugListLogTargetBuildings()
        {
            try
            {
                if (Global.Settings.DispatchHearses && DeadPeopleBuildings != null)
                {
                    DebugListLog(DeadPeopleBuildings.Values);
                }

                if (Global.Settings.DispatchGarbageTrucks && DirtyBuildings != null)
                {
                    DebugListLog(DirtyBuildings.Values);
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Buildings), "DebugListLogTargetBuildings", ex);
            }
        }

        /// <summary>
        /// Updates data.
        /// </summary>
        public void Update()
        {
            // Get and categorize buildings.
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            DistrictManager districtManager = null;
            if (Global.Settings.DispatchByDistrict && districtManager != null)
            {
                districtManager = Singleton<DistrictManager>.instance;
            }

            HasDeadPeopleBuildingsToCheck = false;
            HasDirtyBuildingsToCheck = false;

            // First update?
            if (isInitialized)
            {
                // Data is initialized. Just check buildings for this frame.

                uint endFrame = GetFrameEnd();

                uint counter = 0;
                while (buildingFrame != endFrame)
                {
                    if (counter > 256)
                    {
                        throw new Exception("Update frame loop counter too high!");
                    }
                    counter++;

                    buildingFrame = GetFrameNext(buildingFrame + 1);
                    FrameBoundaries bounds = GetFrameBoundaries(buildingFrame);

                    CategorizeBuildings(districtManager, ref buildings, bounds.FirstId, bounds.LastId);
                }
            }
            else
            {
                CategorizeBuildings(districtManager, ref buildings, 0, buildings.Length);

                buildingFrame = GetFrameEnd();
                isInitialized = Global.FramedUpdates;
            }
        }

        /// <summary>
        /// Logs a list of building info for debug use.
        /// </summary>
        private static void DebugListLog(IEnumerable<ushort> buildingIds)
        {
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            foreach (ushort id in buildingIds)
            {
                DebugListLog(buildings, vehicles, id);
            }
        }

        /// <summary>
        /// Logs a list of building info for debug use.
        /// </summary>
        private static void DebugListLog(IEnumerable<TargetBuildingInfo> targetBuildings)
        {
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            foreach (TargetBuildingInfo building in targetBuildings)
            {
                DebugListLog(buildings, vehicles, building.BuildingId);
            }
        }

        /// <summary>
        /// Logs a list of building info for debug use.
        /// </summary>
        private static void DebugListLog(IEnumerable<ServiceBuildingInfo> serviceBuildings)
        {
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            foreach (ServiceBuildingInfo building in serviceBuildings)
            {
                DebugListLog(buildings, vehicles, building.BuildingId);
            }

            foreach (ServiceBuildingInfo building in serviceBuildings)
            {
                Vehicles.DebugListLog(building);
            }
        }

        /// <summary>
        /// Logs building info for debug use.
        /// </summary>
        private static void DebugListLog(Building[] buildings, Vehicle[] vehicles, ushort buildingId)
        {
            if (buildings[buildingId].Info != null && (buildings[buildingId].m_flags & Building.Flags.Created) == Building.Flags.Created)
            {
                Log.InfoList info = new Log.InfoList();

                info.Add("BuildingId", buildingId);
                info.Add("AI", buildings[buildingId].Info.m_buildingAI.GetType());
                info.Add("InfoName", buildings[buildingId].Info.name);

                string name = GetBuildingName(buildingId);
                if (!String.IsNullOrEmpty(name) && name != buildings[buildingId].Info.name)
                {
                    info.Add("BuildingName", name);
                }

                ushort vehicleCount = 0;
                ushort vehicleId = buildings[buildingId].m_ownVehicles;
                while (vehicleId != 0 && vehicleCount < ushort.MaxValue)
                {
                    vehicleCount++;
                    vehicleId = vehicles[vehicleId].m_nextOwnVehicle;
                }
                info.Add("OwnVehicles", vehicleCount);

                if (buildings[buildingId].Info.m_buildingAI is CemeteryAI)
                {
                    info.Add("CorpseCapacity", ((CemeteryAI)buildings[buildingId].Info.m_buildingAI).m_corpseCapacity);
                    info.Add("GraveCount", ((CemeteryAI)buildings[buildingId].Info.m_buildingAI).m_graveCount);
                    info.Add("HearseCount", ((CemeteryAI)buildings[buildingId].Info.m_buildingAI).m_hearseCount);
                    info.Add("CustomBuffer1", buildings[buildingId].m_customBuffer1); // GraveCapacity?
                    info.Add("CustomBuffer2", buildings[buildingId].m_customBuffer2);
                    info.Add("PR_HC_Calc", (buildings[buildingId].m_productionRate * ((CemeteryAI)buildings[buildingId].Info.m_buildingAI).m_hearseCount + 99) / 100); // Hearse capacity?
                }

                info.Add("ProductionRate", buildings[buildingId].m_productionRate);

                float range = buildings[buildingId].Info.m_buildingAI.GetCurrentRange(buildingId, ref buildings[buildingId]);
                range = range * range * Global.Settings.RangeModifier;
                if (range < Global.Settings.RangeMinimum)
                {
                    info.Add("Range", range, Global.Settings.RangeMinimum);
                }
                else if (range > Global.Settings.RangeMaximum)
                {
                    info.Add("Range", range, Global.Settings.RangeMaximum);
                }
                else
                {
                    info.Add("Range", range);
                }

                List<string> needs = new List<string>();
                if (buildings[buildingId].m_garbageBuffer >= Global.Settings.MinimumGarbageForDispatch)
                {
                    needs.Add("Dirty");
                }
                else if (buildings[buildingId].m_garbageBuffer > 0)
                {
                    needs.Add("Dusty");
                }
                if (buildings[buildingId].m_deathProblemTimer > 0)
                {
                    needs.Add("Dead");
                }
                if (buildings[buildingId].m_garbageBuffer * Dispatcher.ProblemBufferModifier >= Dispatcher.ProblemLimitForgotten ||
                    buildings[buildingId].m_deathProblemTimer * Dispatcher.ProblemTimerModifier >= Dispatcher.ProblemLimitForgotten)
                {
                    needs.Add("Forgotten");
                }
                info.Add("Needs", needs);

                info.Add("DeathProblemTimer", buildings[buildingId].m_deathProblemTimer);
                info.Add("HealthProblemTimer", buildings[buildingId].m_healthProblemTimer);
                info.Add("MajorProblemTimer", buildings[buildingId].m_majorProblemTimer);

                info.Add("GarbageAmount", buildings[buildingId].Info.m_buildingAI.GetGarbageAmount(buildingId, ref buildings[buildingId]));
                info.Add("GarbageBuffer", buildings[buildingId].m_garbageBuffer);

                string problems = buildings[buildingId].m_problems.ToString();
                if (problems.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }) >= 0)
                {
                    foreach (Notification.Problem problem in Enum.GetValues(typeof(Notification.Problem)))
                    {
                        if (problem != Notification.Problem.None && (buildings[buildingId].m_problems & problem) == problem)
                        {
                            problems += ", " + problem.ToString();
                        }
                    }
                }
                info.Add("Problems", problems);

                string flags = buildings[buildingId].m_flags.ToString();
                if (flags.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }) >= 0)
                {
                    foreach (Building.Flags flag in Enum.GetValues(typeof(Building.Flags)))
                    {
                        if (flag != Building.Flags.None && (buildings[buildingId].m_flags & flag) == flag)
                        {
                            flags += ", " + flag.ToString();
                        }
                    }
                }
                info.Add("Flags", flags);

                string status = buildings[buildingId].Info.m_buildingAI.GetLocalizedStatus(buildingId, ref buildings[buildingId]);
                if (!String.IsNullOrEmpty(status))
                {
                    info.Add("Status", status);
                }

                Log.DevDebug(typeof(Buildings), "DebugListLog", info.ToString());
            }
        }

        /// <summary>
        /// Categorizes the building.
        /// </summary>
        /// <param name="districtManager">The district manager.</param>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        private void CategorizeBuilding(DistrictManager districtManager, ushort buildingId, ref Building building)
        {
            // Checks for hearse dispatcher.
            if (Global.Settings.DispatchHearses)
            {
                // Check cemetaries and crematoriums.
                if (building.Info.m_buildingAI is CemeteryAI)
                {
                    if (!HearseBuildings.ContainsKey(buildingId))
                    {
                        ServiceBuildingInfo hearseBuilding = new ServiceBuildingInfo(districtManager, buildingId, ref building);
                        Log.Debug(this, "CategorizeBuilding", "Cemetary", buildingId, building.Info.name, hearseBuilding.BuildingName, hearseBuilding.Range, hearseBuilding.District);

                        HearseBuildings[buildingId] = hearseBuilding;
                    }
                    else
                    {
                        HearseBuildings[buildingId].Update(districtManager, ref building);
                    }
                }
                else if (HearseBuildings.ContainsKey(buildingId))
                {
                    Log.Debug(this, "CategorizeBuilding", "Not Cemetary", buildingId);

                    HearseBuildings.Remove(buildingId);
                }

                // Check dead people.
                if (building.m_deathProblemTimer > 0)
                {
                    if (!DeadPeopleBuildings.ContainsKey(buildingId))
                    {
                        TargetBuildingInfo deadPeopleBuilding = new TargetBuildingInfo(districtManager, buildingId, ref building, building.m_deathProblemTimer, null, true, Notification.Problem.Death);
                        if (Log.LogToFile) Log.Debug(this, "CategorizeBuilding", "Dead People", buildingId, building.Info.name, deadPeopleBuilding.BuildingName, building.m_deathProblemTimer, deadPeopleBuilding.ProblemValue, deadPeopleBuilding.HasProblem, deadPeopleBuilding.District);

                        DeadPeopleBuildings[buildingId] = deadPeopleBuilding;
                        HasDeadPeopleBuildingsToCheck = true;
                    }
                    else
                    {
                        DeadPeopleBuildings[buildingId].Update(districtManager, ref building, building.m_deathProblemTimer, null, true, Notification.Problem.Death);
                        HasDeadPeopleBuildingsToCheck = HasDeadPeopleBuildingsToCheck || DeadPeopleBuildings[buildingId].CheckThis;
                    }
                }
                else if (DeadPeopleBuildings.ContainsKey(buildingId))
                {
                    if (Log.LogToFile) Log.Debug(this, "CategorizeBuilding", "No Dead People", buildingId);

                    DeadPeopleBuildings.Remove(buildingId);
                }
            }

            if (Global.Settings.DispatchGarbageTrucks)
            {
                // Check landfills and incinerators.
                if (building.Info.m_buildingAI is LandfillSiteAI)
                {
                    if (!GarbageBuildings.ContainsKey(buildingId))
                    {
                        ServiceBuildingInfo garbageBuilding = new ServiceBuildingInfo(districtManager, buildingId, ref building);
                        Log.Debug(this, "CategorizeBuilding", "Landfill", buildingId, building.Info.name, garbageBuilding.BuildingName, garbageBuilding.Range, garbageBuilding.District);

                        GarbageBuildings[buildingId] = garbageBuilding;
                    }
                    else
                    {
                        GarbageBuildings[buildingId].Update(districtManager, ref building);
                    }
                }
                else if (GarbageBuildings.ContainsKey(buildingId))
                {
                    Log.Debug(this, "CategorizeBuilding", "Not Landfill", buildingId);

                    GarbageBuildings.Remove(buildingId);
                }

                // Check garbage.
                if (building.m_garbageBuffer >= Global.Settings.MinimumGarbageForDispatch && !(building.Info.m_buildingAI is LandfillSiteAI))
                {
                    if (!DirtyBuildings.ContainsKey(buildingId))
                    {
                        TargetBuildingInfo dirtyBuilding = new TargetBuildingInfo(districtManager, buildingId, ref building, null, building.m_garbageBuffer, true, Notification.Problem.Garbage);
                        if (Log.LogToFile) Log.Debug(this, "CategorizeBuilding", "Dirty", buildingId, building.Info.name, dirtyBuilding.BuildingName, building.m_garbageBuffer, dirtyBuilding.ProblemValue, dirtyBuilding.HasProblem, dirtyBuilding.District);

                        DirtyBuildings[buildingId] = dirtyBuilding;
                        HasDirtyBuildingsToCheck = true;
                    }
                    else
                    {
                        DirtyBuildings[buildingId].Update(districtManager, ref building, null, building.m_garbageBuffer, true, Notification.Problem.Garbage);
                        HasDirtyBuildingsToCheck = HasDirtyBuildingsToCheck || DirtyBuildings[buildingId].CheckThis;
                    }
                }
                else if (DirtyBuildings.ContainsKey(buildingId))
                {
                    if (building.m_garbageBuffer >= Global.Settings.MinimumGarbageForDispatch / 10)
                    {
                        DirtyBuildings[buildingId].Update(districtManager, ref building, null, building.m_garbageBuffer, false, Notification.Problem.Garbage);
                    }
                    else
                    {
                        if (Log.LogToFile) Log.Debug(this, "CategorizeBuilding", "Not Dirty", buildingId);

                        DirtyBuildings.Remove(buildingId);
                    }
                }
            }
        }

        /// <summary>
        /// Categorizes the buildings.
        /// </summary>
        /// <param name="districtManager">The district manager.</param>
        /// <param name="buildings">The buildings.</param>
        /// <param name="firstBuildingId">The first building identifier.</param>
        /// <param name="lastBuildingId">The last building identifier.</param>
        private void CategorizeBuildings(DistrictManager districtManager, ref Building[] buildings, ushort firstBuildingId, int lastBuildingId)
        {
            for (ushort id = firstBuildingId; id < lastBuildingId; id++)
            {
                if (buildings[id].Info == null || (buildings[id].m_flags & Building.Flags.Created) == Building.Flags.None || (buildings[id].m_flags & (Building.Flags.Abandoned | Building.Flags.BurnedDown | Building.Flags.Deleted)) != Building.Flags.None)
                {
                    if (Global.Settings.DispatchHearses)
                    {
                        if (HearseBuildings.ContainsKey(id))
                        {
                            HearseBuildings.Remove(id);
                            Log.Debug(this, "CategorizeBuildings", "Not Cemetary Building", id);
                        }

                        if (DeadPeopleBuildings.ContainsKey(id))
                        {
                            DeadPeopleBuildings.Remove(id);
                            Log.Debug(this, "CategorizeBuildings", "Not Dead People Building", id);
                        }
                    }

                    if (Global.Settings.DispatchGarbageTrucks)
                    {
                        if (GarbageBuildings.ContainsKey(id))
                        {
                            GarbageBuildings.Remove(id);
                            Log.Debug(this, "CategorizeBuildings", "Not Landfill Building", id);
                        }

                        if (DirtyBuildings.ContainsKey(id))
                        {
                            DirtyBuildings.Remove(id);
                            Log.Debug(this, "CategorizeBuildings", "Not Dirty Building", id);
                        }
                    }
                }
                else
                {
                    CategorizeBuilding(districtManager, id, ref buildings[id]);
                }
            }
        }

        /// <summary>
        /// Gets the frame boundaries.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>The frame boundaries.</returns>
        private FrameBoundaries GetFrameBoundaries(uint frame)
        {
            frame = frame & 255;

            return new FrameBoundaries(frame * 128, (frame + 1) * 128 - 1);
        }

        /// <summary>
        /// Gets the end frame.
        /// </summary>
        /// <returns>The end frame.</returns>
        private uint GetFrameEnd()
        {
            return Singleton<SimulationManager>.instance.m_currentFrameIndex & 255;
        }

        /// <summary>
        /// Gets the next frame.
        /// </summary>
        /// <param name="frame">The current/last frame.</param>
        /// <returns>The next frame.</returns>
        private uint GetFrameNext(uint frame)
        {
            return frame & 255;
        }

        /// <summary>
        /// Info about a service building.
        /// </summary>
        public class ServiceBuildingInfo
        {
            /// <summary>
            /// The building identifier.
            /// </summary>
            public ushort BuildingId = 0;

            /// <summary>
            /// The building can receive.
            /// </summary>
            public bool CanReceive;

            /// <summary>
            /// The distance to the target.
            /// </summary>
            public float Distance = float.PositiveInfinity;

            /// <summary>
            /// The district the building is in.
            /// </summary>
            public byte District = 0;

            /// <summary>
            /// The buildings first own vehicle.
            /// </summary>
            public ushort FirstOwnVehicleId = 0;

            /// <summary>
            /// The free vehicles count.
            /// </summary>
            public int VehiclesFree = 0;

            /// <summary>
            /// The building is in target district.
            /// </summary>
            public bool InDistrict = false;

            /// <summary>
            /// The target building is in service bilding's effective range.
            /// </summary>
            public bool InRange = true;

            /// <summary>
            /// The building's position.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// The building's effect range.
            /// </summary>
            public float Range = 0;

            /// <summary>
            /// The spare vehicles count;
            /// </summary>
            public int VehiclesSpare = 0;

            /// <summary>
            /// The vehicle in use count.
            /// </summary>
            public int VehiclesUsed = 0;

            /// <summary>
            /// The total vehicles count;
            /// </summary>
            public int VehiclesTotal = 0;

            /// <summary>
            /// The vehicles.
            /// </summary>
            public Dictionary<ushort, Vehicles.ServiceVehicleInfo> Vehicles = new Dictionary<ushort, Vehicles.ServiceVehicleInfo>();

            /// <summary>
            /// The last info update stamp.
            /// </summary>
            private uint lastInfoUpdate = 0;

            /// <summary>
            /// The last update stamp.
            /// </summary>
            private uint lastUpdate = 0;

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceBuildingInfo"/> class.
            /// </summary>
            /// <param name="districtManager">The district manager.</param>
            /// <param name="buildingId">The building identifier.</param>
            /// <param name="building">The building.</param>
            public ServiceBuildingInfo(DistrictManager districtManager, ushort buildingId, ref Building building)
            {
                this.BuildingId = buildingId;

                this.Update(districtManager, ref building);
            }

            /// <summary>
            /// Gets the name of the building.
            /// </summary>
            /// <value>
            /// The name of the building.
            /// </value>
            public string BuildingName
            {
                get
                {
                    return GetBuildingName(BuildingId);
                }
            }

            /// <summary>
            /// The building is updated.
            /// </summary>
            public bool Updated
            {
                get
                {
                    return lastUpdate == Global.CurrentFrame;
                }
            }

            /// <summary>
            /// Sets the target information.
            /// </summary>
            /// <param name="districtManager">The district manager.</param>
            /// <param name="building">The building.</param>
            /// <param name="ignoreRange">if set to <c>true</c> ignore the range.</param>
            public void SetTargetInfo(DistrictManager districtManager, TargetBuildingInfo building, bool ignoreRange)
            {
                this.Distance = (this.Position - building.Position).sqrMagnitude;

                if (Global.Settings.DispatchByDistrict && districtManager != null)
                {
                    byte district = districtManager.GetDistrict(building.Position);
                    this.InDistrict = (district == this.District);
                }
                else
                {
                    this.InDistrict = false;
                }

                this.InRange = ignoreRange || this.InDistrict || (Global.Settings.DispatchByRange && this.Distance < this.Range) || (!Global.Settings.DispatchByDistrict && !Global.Settings.DispatchByRange);
            }

            /// <summary>
            /// Updates the building info.
            /// </summary>
            /// <param name="districtManager">The district manager.</param>
            /// <param name="building">The building.</param>
            public void Update(DistrictManager districtManager, ref Building building)
            {
                this.lastUpdate = Global.CurrentFrame;

                this.Position = building.m_position;
                this.FirstOwnVehicleId = building.m_ownVehicles;

                if (lastInfoUpdate == 0 || Global.CurrentFrame - lastInfoUpdate > Global.ObjectUpdateInterval)
                {
                    if (districtManager != null)
                    {
                        this.District = districtManager.GetDistrict(Position);
                    }

                    if (Global.Settings.DispatchByRange)
                    {
                        this.Range = building.Info.m_buildingAI.GetCurrentRange(BuildingId, ref building);
                        this.Range = this.Range * this.Range * Global.Settings.RangeModifier;
                        if (Global.Settings.RangeLimit)
                        {
                            if (this.Range < Global.Settings.RangeMinimum)
                            {
                                this.Range = Global.Settings.RangeMinimum;
                            }
                            else if (this.Range > Global.Settings.RangeMaximum)
                            {
                                this.Range = Global.Settings.RangeMaximum;
                            }
                        }
                    }

                    lastInfoUpdate = Global.CurrentFrame;
                }

                if (building.Info.m_buildingAI is CemeteryAI)
                {
                    VehiclesTotal = ((CemeteryAI)building.Info.m_buildingAI).m_hearseCount;
                }
                else if (building.Info.m_buildingAI is LandfillSiteAI)
                {
                    VehiclesTotal = ((LandfillSiteAI)building.Info.m_buildingAI).m_garbageTruckCount;
                }

                this.CanReceive = (building.m_flags & (Building.Flags.CapacityFull | Building.Flags.Downgrading | Building.Flags.Demolishing | Building.Flags.Deleted | Building.Flags.BurnedDown)) == Building.Flags.None &&
                                  (building.m_flags & (Building.Flags.Created | Building.Flags.Created | Building.Flags.Active)) == (Building.Flags.Created | Building.Flags.Created | Building.Flags.Active) &&
                                  (building.m_problems & (Notification.Problem.Emptying | Notification.Problem.LandfillFull | Notification.Problem.RoadNotConnected | Notification.Problem.TurnedOff | Notification.Problem.FatalProblem)) == Notification.Problem.None &&
                                  !building.Info.m_buildingAI.IsFull(this.BuildingId, ref building);
            }

            /// <summary>
            /// Compares service buildings for priority sorting.
            /// </summary>
            public class PriorityComparer : IComparer<ServiceBuildingInfo>
            {
                /// <summary>
                /// Compares two buildings and returns a value indicating whether one is less than, equal to, or greater than the other.
                /// </summary>
                /// <param name="x">The first buildings to compare.</param>
                /// <param name="y">The second buildings to compare.</param>
                /// <returns>
                /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.
                /// </returns>
                public int Compare(ServiceBuildingInfo x, ServiceBuildingInfo y)
                {
                    if (x.InDistrict && !y.InDistrict)
                    {
                        return -1;
                    }
                    else if (y.InDistrict && !x.InDistrict)
                    {
                        return 1;
                    }
                    else
                    {
                        float s = x.Distance - y.Distance;
                        if (s < 0)
                        {
                            return -1;
                        }
                        else if (s > 0)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Info about a target building.
        /// </summary>
        public class TargetBuildingInfo
        {
            /// <summary>
            /// The building identifier.
            /// </summary>
            public ushort BuildingId;

            /// <summary>
            /// The district the building is in.
            /// </summary>
            public byte District;

            /// <summary>
            /// The building has a problem.
            /// </summary>
            public bool HasProblem;

            /// <summary>
            /// The buidling needs service.
            /// </summary>
            public bool NeedsService;

            /// <summary>
            /// The position.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// The problem timer.
            /// </summary>
            public int ProblemValue;

            /// <summary>
            /// Wether this building should be checked or not.
            /// </summary>
            private bool checkThis;

            /// <summary>
            /// The last check stamp.
            /// </summary>
            private uint lastCheck = 0;

            /// <summary>
            /// The last handled stamp.
            /// </summary>
            private uint lastHandled = 0;

            /// <summary>
            /// The last update stamp.
            /// </summary>
            private uint lastInfoUpdate;

            /// <summary>
            /// The last update stamp.
            /// </summary>
            private uint lastUpdate;

            /// <summary>
            /// Initializes a new instance of the <see cref="TargetBuildingInfo" /> class.
            /// </summary>
            /// <param name="districtManager">The district manager.</param>
            /// <param name="buildingId">The building identifier.</param>
            /// <param name="building">The building.</param>
            /// <param name="problemTimer">The problem timer.</param>
            /// <param name="problemBuffer">The problem buffer.</param>
            /// <param name="needsService">if set to <c>true</c> building needs service.</param>
            /// <param name="problemToCheck">The problem to check.</param>
            public TargetBuildingInfo(DistrictManager districtManager, ushort buildingId, ref Building building, byte? problemTimer, ushort? problemBuffer, bool needsService, Notification.Problem problemToCheck)
            {
                this.BuildingId = buildingId;

                this.Update(districtManager, ref building, problemTimer, problemBuffer, needsService, problemToCheck);
            }

            /// <summary>
            /// Gets the name of the building.
            /// </summary>
            /// <value>
            /// The name of the building.
            /// </value>
            public string BuildingName
            {
                get
                {
                    return GetBuildingName(BuildingId);
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="TargetBuildingInfo"/> is checked.
            /// </summary>
            /// <value>
            ///   <c>true</c> if checked; otherwise, <c>false</c>.
            /// </value>
            public bool Checked
            {
                get
                {
                    return (Global.RecheckInterval > 0 && Global.CurrentFrame - lastCheck < Global.RecheckInterval);
                }
                set
                {
                    if (value)
                    {
                        lastCheck = Global.CurrentFrame;
                    }
                    else
                    {
                        lastCheck = 0;
                    }
                }
            }

            /// <summary>
            /// Gets a value indicating whether to check this building.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this building should be checked; otherwise, <c>false</c>.
            /// </value>
            public bool CheckThis
            {
                get
                {
                    return this.checkThis && lastUpdate == Global.CurrentFrame;
                }
                set
                {
                    this.checkThis = value;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="TargetBuildingInfo"/> is handled.
            /// </summary>
            /// <value>
            ///   <c>true</c> if handled; otherwise, <c>false</c>.
            /// </value>
            public bool Handled
            {
                get
                {
                    return (Global.RecheckHandledInterval > 0 && Global.CurrentFrame - lastHandled < Global.RecheckHandledInterval);
                }
                set
                {
                    if (value)
                    {
                        this.checkThis = false;
                        lastHandled = Global.CurrentFrame;
                    }
                    else
                    {
                        lastHandled = 0;
                    }
                }
            }

            /// <summary>
            /// The building is updated.
            /// </summary>
            public bool Updated
            {
                get
                {
                    return lastUpdate == Global.CurrentFrame;
                }
            }

            /// <summary>
            /// Updates the building.
            /// </summary>
            /// <param name="districtManager">The district manager.</param>
            /// <param name="building">The building.</param>
            /// <param name="problemTimer">The problem timer.</param>
            /// <param name="problemBuffer">The problem buffer.</param>
            /// <param name="needsService">if set to <c>true</c> building needs service.</param>
            /// <param name="problemToCheck">The problem to check.</param>
            public void Update(DistrictManager districtManager, ref Building building, byte? problemTimer, ushort? problemBuffer, bool needsService, Notification.Problem problemToCheck)
            {
                this.lastUpdate = Global.CurrentFrame;

                if (problemTimer != null && problemTimer.HasValue)
                {
                    this.ProblemValue = (ushort)problemTimer.Value << 8;
                }
                else if (problemBuffer != null && problemBuffer.HasValue)
                {
                    this.ProblemValue = problemBuffer.Value;
                }
                else
                {
                    this.ProblemValue = 0;
                }

                this.HasProblem = (building.m_problems & problemToCheck) == problemToCheck || this.ProblemValue >= Dispatcher.ProblemLimit;
                this.Position = building.m_position;

                if (lastInfoUpdate == 0 || Global.CurrentFrame - lastInfoUpdate > Global.ObjectUpdateInterval)
                {
                    if (districtManager != null)
                    {
                        this.District = districtManager.GetDistrict(Position);
                    }
                    else if (lastInfoUpdate == 0)
                    {
                        this.District = 0;
                    }

                    lastInfoUpdate = Global.CurrentFrame;
                }

                this.NeedsService = needsService || this.HasProblem;

                this.checkThis = needsService &&
                                 ((Global.RecheckInterval == 0 || this.lastCheck == 0 || Global.CurrentFrame - this.lastCheck >= Global.RecheckInterval) &&
                                  (Global.RecheckHandledInterval == 0 || this.lastHandled == 0 || Global.CurrentFrame - this.lastHandled >= Global.RecheckHandledInterval));
            }

            /// <summary>
            /// Compares target buildings for priority sorting.
            /// </summary>
            public class PriorityComparer : IComparer<TargetBuildingInfo>
            {
                /// <summary>
                /// Compares two buildings and returns a value indicating whether one is less than, equal to, or greater than the other.
                /// </summary>
                /// <param name="x">The first buildings to compare.</param>
                /// <param name="y">The second buildings to compare.</param>
                /// <returns>
                /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.
                /// </returns>
                public int Compare(TargetBuildingInfo x, TargetBuildingInfo y)
                {
                    if (x.HasProblem && !y.HasProblem)
                    {
                        return -1;
                    }
                    else if (y.HasProblem && !x.HasProblem)
                    {
                        return 1;
                    }
                    else
                    {
                        return y.ProblemValue - x.ProblemValue;
                    }
                }
            }
        }
    }
}