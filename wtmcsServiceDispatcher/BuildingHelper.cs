﻿using ColossalFramework;
using System;
using System.Collections.Generic;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Building helper functions.
    /// </summary>
    internal static class BuildingHelper
    {
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
                Log.Error(typeof(BuildingKeeper), "DebugListLog", ex);
            }
        }

        /// <summary>
        /// Logs a list of building info for debug use.
        /// </summary>
        /// <param name="buildingIds">The building ids.</param>
        public static void DebugListLog(IEnumerable<ushort> buildingIds)
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
        /// <param name="targetBuildings">The target buildings.</param>
        public static void DebugListLog(IEnumerable<TargetBuildingInfo> targetBuildings)
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
        /// <param name="serviceBuildings">The service buildings.</param>
        public static void DebugListLog(IEnumerable<ServiceBuildingInfo> serviceBuildings)
        {
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            foreach (ServiceBuildingInfo building in serviceBuildings)
            {
                DebugListLog(buildings, vehicles, building.BuildingId);
            }

            foreach (ServiceBuildingInfo building in serviceBuildings)
            {
                VehicleHelper.DebugListLog(building);
            }
        }

        /// <summary>
        /// Gets the CS building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <returns>The CS building.</returns>
        public static Building GetBuilding(ushort buildingId)
        {
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            return buildings[buildingId];
        }

        /// <summary>
        /// Gets the CS building information.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <returns>The CS building information.</returns>
        public static BuildingInfo GetBuildingInfo(ushort buildingId)
        {
            return GetBuilding(buildingId).Info;
        }

        /// <summary>
        /// Gets the name of the building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <returns>The buildings name.</returns>
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
        /// Logs building info for debug use.
        /// </summary>
        /// <param name="buildings">The buildings.</param>
        /// <param name="vehicles">The vehicles.</param>
        /// <param name="buildingId">The building identifier.</param>
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

                int serviceVehicleCount = 0;
                if (buildings[buildingId].Info.m_buildingAI is CemeteryAI)
                {
                    serviceVehicleCount = ((CemeteryAI)buildings[buildingId].Info.m_buildingAI).m_hearseCount;
                    info.Add("CorpseCapacity", ((CemeteryAI)buildings[buildingId].Info.m_buildingAI).m_corpseCapacity);
                    info.Add("GraveCount", ((CemeteryAI)buildings[buildingId].Info.m_buildingAI).m_graveCount);
                    info.Add("CustomBuffer1", buildings[buildingId].m_customBuffer1); // GraveCapacity?
                    info.Add("CustomBuffer2", buildings[buildingId].m_customBuffer2);
                    info.Add("PR_HC_Calc", ((buildings[buildingId].m_productionRate * ((CemeteryAI)buildings[buildingId].Info.m_buildingAI).m_hearseCount) + 99) / 100); // Hearse capacity?
                }
                else if (buildings[buildingId].Info.m_buildingAI is LandfillSiteAI)
                {
                    serviceVehicleCount = ((LandfillSiteAI)buildings[buildingId].Info.m_buildingAI).m_garbageTruckCount;
                }

                int productionRate = buildings[buildingId].m_productionRate;

                ushort ownVehicleCount = 0;
                ushort madeVehicleCount = 0;
                ushort vehicleId = buildings[buildingId].m_ownVehicles;
                while (vehicleId != 0 && ownVehicleCount < ushort.MaxValue)
                {
                    ownVehicleCount++;
                    if ((vehicles[vehicleId].m_transferType == (byte)TransferManager.TransferReason.Garbage || vehicles[vehicleId].m_transferType == (byte)TransferManager.TransferReason.Dead) &&
                        vehicles[vehicleId].Info != null && (vehicles[vehicleId].m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Spawned)) != Vehicle.Flags.None)
                    {
                        madeVehicleCount++;
                    }

                    vehicleId = vehicles[vehicleId].m_nextOwnVehicle;
                }
                info.Add("OwnVehicles", ownVehicleCount);
                info.Add("MadeVehicles", madeVehicleCount);

                info.Add("VehicleCount", serviceVehicleCount);
                info.Add("ProductionRate", productionRate);
                info.Add("VehicleCountNominal", ((productionRate * serviceVehicleCount) + 99) / 100);

                int budget = Singleton<EconomyManager>.instance.GetBudget(buildings[buildingId].Info.m_buildingAI.m_info.m_class);
                productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                int actualVehicleCount = ((productionRate * serviceVehicleCount) + 99) / 100;
                info.Add("Budget", budget);
                info.Add("ProductionRateActual", productionRate);
                info.Add("VehicleCountActual", actualVehicleCount);
                info.Add("SpareVehicles", actualVehicleCount - ownVehicleCount);

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

                Log.DevDebug(typeof(BuildingKeeper), "DebugListLog", info.ToString());
            }
        }
    }
}