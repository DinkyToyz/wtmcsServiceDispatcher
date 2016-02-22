using System;
using System.Collections.Generic;
using ColossalFramework;

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
                DistrictManager districtManager = Singleton<DistrictManager>.instance;
                CitizenManager citizenManager = Singleton<CitizenManager>.instance;

                for (ushort id = 0; id < buildings.Length; id++)
                {
                    DebugListLog(buildings, vehicles, districtManager, citizenManager, id, null, null, null);
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(BuildingHelper), "DebugListLog", ex);
            }
        }

        /// <summary>
        /// Logs a list of building info for debug use.
        /// </summary>
        /// <param name="buildingStamps">The building stamps.</param>
        /// <param name="source">The source.</param>
        public static void DebugListLog(Dictionary<ushort, double> buildingStamps, string source = null)
        {
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            DistrictManager districtManager = Singleton<DistrictManager>.instance;
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;

            foreach (KeyValuePair<ushort, double> buildingStamp in buildingStamps)
            {
                DebugListLog(buildings, vehicles, districtManager, citizenManager, buildingStamp.Key, null, null, new BuildingStamp(buildingStamp, source));
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
            DistrictManager districtManager = Singleton<DistrictManager>.instance;
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;

            foreach (ushort id in buildingIds)
            {
                DebugListLog(buildings, vehicles, districtManager, citizenManager, id, null, null, null);
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
            DistrictManager districtManager = Singleton<DistrictManager>.instance;
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;

            foreach (TargetBuildingInfo building in targetBuildings)
            {
                DebugListLog(buildings, vehicles, districtManager, citizenManager, building.BuildingId, null, building, null);
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
            DistrictManager districtManager = Singleton<DistrictManager>.instance;
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;

            foreach (ServiceBuildingInfo building in serviceBuildings)
            {
                DebugListLog(buildings, vehicles, districtManager, citizenManager, building.BuildingId, building, null, null);
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
        /// Gets the name of the district for a building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <returns>
        /// The name of the district.
        /// </returns>
        public static string GetDistrictName(ushort buildingId)
        {
            if (!Log.LogNames)
            {
                return null;
            }

            try
            {
                Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
                if (buildings[buildingId].Info == null || (buildings[buildingId].m_flags & Building.Flags.Created) != Building.Flags.Created)
                {
                    return null;
                }

                return DistrictHelper.GetDistrictNameForPosition(buildings[buildingId].m_position);
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
        /// <param name="districtManager">The district manager.</param>
        /// <param name="citizenManager">The citizen manager.</param>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="serviceBuilding">The service building.</param>
        /// <param name="targetBuilding">The target building.</param>
        /// <param name="buildingStamp">The building stamp.</param>
        private static void DebugListLog(
            Building[] buildings,
            Vehicle[] vehicles,
            DistrictManager districtManager,
            CitizenManager citizenManager,
            ushort buildingId,
            ServiceBuildingInfo serviceBuilding,
            TargetBuildingInfo targetBuilding,
            BuildingStamp buildingStamp)
        {
            if (buildings[buildingId].Info != null && (buildings[buildingId].m_flags & Building.Flags.Created) == Building.Flags.Created)
            {
                Log.InfoList info = new Log.InfoList();

                if (buildingStamp != null)
                {
                    info.Add("O", "BuildingStamp");
                }

                if (serviceBuilding != null)
                {
                    info.Add("O", "ServiceBuilding");
                }

                if (targetBuilding != null)
                {
                    info.Add("O", "TargetBuilding");
                }

                info.Add("BuildingId", buildingId);
                info.Add("AI", buildings[buildingId].Info.m_buildingAI.GetType());
                info.Add("InfoName", buildings[buildingId].Info.name);

                string name = GetBuildingName(buildingId);
                if (!String.IsNullOrEmpty(name) && name != buildings[buildingId].Info.name)
                {
                    info.Add("BuildingName", name);
                }

                byte district = districtManager.GetDistrict(buildings[buildingId].m_position);
                info.Add("District", district);
                info.Add("DistrictName", districtManager.GetDistrictName(district));

                if (buildingStamp != null)
                {
                    info.Add("Source", buildingStamp.Source);
                    info.Add("SimulationTimeStamp", buildingStamp.SimulationTimeStamp);
                    info.Add("SimulationTimeDelta", buildingStamp.SimulationTimeDelta);
                }

                if (serviceBuilding != null)
                {
                    info.Add("CanReceive", serviceBuilding.CanReceive);
                    info.Add("CapacityLevel", serviceBuilding.CapacityLevel);
                    info.Add("CapactyFree", serviceBuilding.CapacityFree);
                    info.Add("CapactyMax", serviceBuilding.CapacityMax);
                    info.Add("CapactyOverflow", serviceBuilding.CapacityOverflow);
                    info.Add("Range", serviceBuilding.Range);
                    info.Add("VehiclesFree", serviceBuilding.VehiclesFree);
                    info.Add("VehiclesSpare", serviceBuilding.VehiclesSpare);
                    info.Add("VehiclesMade", serviceBuilding.VehiclesMade);
                    info.Add("VehiclesTotal", serviceBuilding.VehiclesTotal);
                }

                if (targetBuilding != null)
                {
                    info.Add("Demand", targetBuilding.Demand);
                    info.Add("HasProblem", targetBuilding.HasProblem);
                    info.Add("ProblemSize", targetBuilding.ProblemSize);
                    info.Add("ProblemValue", targetBuilding.ProblemValue);
                }

                int materialMax = 0;
                int materialAmount = 0;
                int serviceVehicleCount = 0;
                if (buildings[buildingId].Info.m_buildingAI is CemeteryAI)
                {
                    serviceVehicleCount = ((CemeteryAI)buildings[buildingId].Info.m_buildingAI).m_hearseCount;
                    info.Add("CorpseCapacity", ((CemeteryAI)buildings[buildingId].Info.m_buildingAI).m_corpseCapacity);
                    info.Add("GraveCount", ((CemeteryAI)buildings[buildingId].Info.m_buildingAI).m_graveCount);
                    info.Add("CustomBuffer1", buildings[buildingId].m_customBuffer1); // GraveCapacity?
                    info.Add("CustomBuffer2", buildings[buildingId].m_customBuffer2);
                    info.Add("PR_HC_Calc", ((buildings[buildingId].m_productionRate * ((CemeteryAI)buildings[buildingId].Info.m_buildingAI).m_hearseCount) + 99) / 100); // Hearse capacity?
                    buildings[buildingId].Info.m_buildingAI.GetMaterialAmount(buildingId, ref buildings[buildingId], TransferManager.TransferReason.Dead, out materialAmount, out materialMax);
                }
                else if (buildings[buildingId].Info.m_buildingAI is LandfillSiteAI)
                {
                    serviceVehicleCount = ((LandfillSiteAI)buildings[buildingId].Info.m_buildingAI).m_garbageTruckCount;
                    buildings[buildingId].Info.m_buildingAI.GetMaterialAmount(buildingId, ref buildings[buildingId], TransferManager.TransferReason.Garbage, out materialAmount, out materialMax);
                }
                info.Add("materialMax", materialMax);
                info.Add("materialAmount", materialAmount);
                info.Add("materialFree", materialMax - materialAmount);

                int productionRate = buildings[buildingId].m_productionRate;

                ushort ownVehicleCount = 0;
                ushort madeVehicleCount = 0;
                ushort vehicleId = buildings[buildingId].m_ownVehicles;
                while (vehicleId != 0 && ownVehicleCount < ushort.MaxValue)
                {
                    ownVehicleCount++;
                    if ((vehicles[vehicleId].m_transferType == (byte)TransferManager.TransferReason.Garbage || vehicles[vehicleId].m_transferType == (byte)TransferManager.TransferReason.Dead) &&
                        vehicles[vehicleId].Info != null && (vehicles[vehicleId].m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Spawned)) == (Vehicle.Flags.Created | Vehicle.Flags.Spawned))
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
                    needs.Add("Filthy");
                }
                if (buildings[buildingId].m_garbageBuffer >= Global.Settings.MinimumGarbageForPatrol)
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

                int citizens = 0;
                int count = 0;
                uint unitId = buildings[buildingId].m_citizenUnits;
                while (unitId != 0)
                {
                    CitizenUnit unit = citizenManager.m_units.m_buffer[unitId];
                    for (int i = 0; i < 5; i++)
                    {
                        uint citizenId = unit.GetCitizen(i);
                        if (citizenId != 0)
                        {
                            Citizen citizen = citizenManager.m_citizens.m_buffer[citizenId];
                            if (citizen.Dead && citizen.GetBuildingByLocation() == buildingId)
                            {
                                citizens++;
                            }
                        }
                    }

                    count++;
                    if (count > (int)ushort.MaxValue * 10)
                    {
                        break;
                    }

                    unitId = unit.m_nextUnit;
                }
                info.Add("DeadCitizens", citizens);

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

                info.Add("AI", buildings[buildingId].Info.m_buildingAI.GetType().AssemblyQualifiedName);

                Log.DevDebug(typeof(BuildingHelper), "DebugListLog", info.ToString());
            }
        }

        /// <summary>
        /// Building time stamp info class.
        /// </summary>
        private class BuildingStamp
        {
            /// <summary>
            /// The building identifier.
            /// </summary>
            public ushort BuildingId = 0;

            /// <summary>
            /// The simulation time stamp.
            /// </summary>
            public double SimulationTimeStamp = 0.0;

            /// <summary>
            /// The source.
            /// </summary>
            public string Source = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="BuildingStamp"/> class.
            /// </summary>
            /// <param name="buildingId">The building identifier.</param>
            /// <param name="simulationTimeStamp">The simulation time stamp.</param>
            /// <param name="source">The source.</param>
            public BuildingStamp(ushort buildingId, double simulationTimeStamp, string source = null)
            {
                this.BuildingId = buildingId;
                this.SimulationTimeStamp = simulationTimeStamp;
                this.Source = source;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="BuildingStamp"/> class.
            /// </summary>
            /// <param name="stamp">The stamp.</param>
            /// <param name="source">The source.</param>
            public BuildingStamp(KeyValuePair<ushort, double> stamp, string source = null)
            {
                this.BuildingId = stamp.Key;
                this.SimulationTimeStamp = stamp.Value;
                this.Source = source;
            }

            /// <summary>
            /// Gets the simulation time delta.
            /// </summary>
            /// <value>
            /// The simulation time delta.
            /// </value>
            public double SimulationTimeDelta
            {
                get
                {
                    return Global.SimulationTime - this.SimulationTimeStamp;
                }
            }
        }
    }
}