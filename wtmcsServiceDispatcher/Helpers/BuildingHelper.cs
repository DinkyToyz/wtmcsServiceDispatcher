using ColossalFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Building helper functions.
    /// </summary>
    internal static class BuildingHelper
    {
        /// <summary>
        /// The health problem notification flags.
        /// </summary>
        public const Notification.Problem HealthProblems = Notification.Problem.DirtyWater | Notification.Problem.Pollution | Notification.Problem.Noise;

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
        /// Saves a list of building info for debug use.
        /// </summary>
        /// <exception cref="InvalidDataException">No building objects.</exception>
        public static void DumpBuildings()
        {
            if (!Global.LevelLoaded)
            {
                return;
            }

            bool logNames = Log.LogNames;
            Log.LogNames = true;

            try
            {
                List<KeyValuePair<string, string>> buildingList = new List<KeyValuePair<string, string>>();

                Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
                Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
                DistrictManager districtManager = Singleton<DistrictManager>.instance;
                CitizenManager citizenManager = Singleton<CitizenManager>.instance;

                for (ushort id = 0; id < buildings.Length; id++)
                {
                    if (buildings[id].m_flags != Building.Flags.None)
                    {
                        string sortValue;
                        try
                        {
                            sortValue = buildings[id].Info.m_buildingAI.GetType().ToString();
                        }
                        catch
                        {
                            sortValue = "?";
                        }

                        sortValue += id.ToString().PadLeft(16, '0');

                        buildingList.Add(new KeyValuePair<string, string>(sortValue, DebugInfoMsg(buildings, vehicles, districtManager, citizenManager, id, null, null, null, true).ToString()));
                    }
                }

                if (buildingList.Count == 0)
                {
                    throw new InvalidDataException("No building objects");
                }

                using (StreamWriter dumpFile = new StreamWriter(FileSystem.FilePathName(".Buildings.txt"), false))
                {
                    dumpFile.Write(String.Join(Environment.NewLine, buildingList.OrderBy(bi => bi.Key).Select(bi => bi.Value).ToArray()).ConformNewlines());
                    dumpFile.WriteLine();
                    dumpFile.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(VehicleKeeper), "DumpBuildings", ex);
            }
            finally
            {
                Log.LogNames = logNames;
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
        /// Gets the capacity amounts for a building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <param name="capacityAmount">The capacity amount.</param>
        /// <param name="capacityMax">The capacity maximum.</param>
        /// <param name="vehicleCount">The vehicle count.</param>
        /// <returns>True if amounts fetched.</returns>
        public static bool GetCapacityAmount(ushort buildingId, ref Building building, out int capacityAmount, out int capacityMax, out int vehicleCount)
        {
            if (building.Info.m_buildingAI is LandfillSiteAI)
            {
                vehicleCount = ((LandfillSiteAI)building.Info.m_buildingAI).m_garbageTruckCount;

                capacityAmount = ((LandfillSiteAI)building.Info.m_buildingAI).GetGarbageAmount(buildingId, ref building);
                capacityMax = ((LandfillSiteAI)building.Info.m_buildingAI).m_garbageCapacity;

                return true;
            }

            if (building.Info.m_buildingAI is CemeteryAI)
            {
                vehicleCount = ((CemeteryAI)building.Info.m_buildingAI).m_hearseCount;

                capacityAmount = building.m_customBuffer1;
                capacityMax = ((CemeteryAI)building.Info.m_buildingAI).m_graveCount;

                return true;
            }

            if (building.Info.m_buildingAI is HospitalAI)
            {
                vehicleCount = ((HospitalAI)building.Info.m_buildingAI).m_ambulanceCount;

                capacityAmount = 0;
                capacityMax = 0;

                return true;
            }

            vehicleCount = 0;
            capacityAmount = 0;
            capacityMax = 0;

            return false;
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

                return DistrictHelper.GetDistrictName(buildings[buildingId].m_position);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Starts the transfer.
        /// </summary>
        /// <param name="serviceBuildingId">The building identifier.</param>
        /// <param name="material">The material.</param>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <param name="targetCitizenId">The target citizen identifier.</param>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <returns>
        /// Vehicle info for the created vehicle.
        /// </returns>
        public static VehicleInfo StartTransfer(ushort serviceBuildingId, TransferManager.TransferReason material, ushort targetBuildingId, uint targetCitizenId, out ushort vehicleId)
        {
            return StartTransfer(serviceBuildingId, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[serviceBuildingId], material, targetBuildingId, targetCitizenId, out vehicleId);
        }

        /// <summary>
        /// Starts the transfer.
        /// </summary>
        /// <param name="serviceBuildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <param name="material">The material.</param>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <param name="targetCitizenId">The target citizen identifier.</param>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <returns>Vehicle info for the created vehicle.</returns>
        /// <exception cref="Exception">Loop counter too high.</exception>
        public static VehicleInfo StartTransfer(ushort serviceBuildingId, ref Building building, TransferManager.TransferReason material, ushort targetBuildingId, uint targetCitizenId, out ushort vehicleId)
        {
            if (building.Info.m_buildingAI is HospitalAI && targetCitizenId == 0)
            {
                return VehicleHelper.CreateServiceVehicle(serviceBuildingId, material, targetBuildingId, targetCitizenId, out vehicleId);
            }

            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            Citizen[] citizens = Singleton<CitizenManager>.instance.m_citizens.m_buffer;

            TransferManager.TransferOffer offer = TransferManagerHelper.MakeOffer(targetBuildingId, targetCitizenId);

            int count;
            HashSet<ushort> ownVehicles = new HashSet<ushort>();

            count = 0;
            vehicleId = building.m_ownVehicles;
            while (vehicleId != 0)
            {
                ownVehicles.Add(vehicleId);

                if (count >= ushort.MaxValue)
                {
                    throw new Exception("Loop counter too high");
                }

                count++;
                vehicleId = vehicles[vehicleId].m_nextOwnVehicle;
            }

            // Cast AI as games original AI so detoured methods are called, but not methods from not replaced classes.
            if (Global.Settings.CreationCompatibilityMode == ServiceDispatcherSettings.ModCompatibilityMode.UseInstanciatedClassMethods || !Global.Settings.AllowReflection())
            {
                building.Info.m_buildingAI.StartTransfer(serviceBuildingId, ref building, material, offer);
            }
            else if (building.Info.m_buildingAI is CemeteryAI)
            {
                ((CemeteryAI)building.Info.m_buildingAI.CastTo<CemeteryAI>()).StartTransfer(serviceBuildingId, ref building, material, offer);
            }
            else if (building.Info.m_buildingAI is LandfillSiteAI)
            {
                ((LandfillSiteAI)building.Info.m_buildingAI.CastTo<LandfillSiteAI>()).StartTransfer(serviceBuildingId, ref building, material, offer);
            }
            else if (building.Info.m_buildingAI is HospitalAI)
            {
                ((HospitalAI)building.Info.m_buildingAI.CastTo<HospitalAI>()).StartTransfer(serviceBuildingId, ref building, material, offer);
            }
            else
            {
                building.Info.m_buildingAI.StartTransfer(serviceBuildingId, ref building, material, offer);
            }

            ushort newVehicleId = 0;
            ushort waitingVehicleId = 0;

            Vehicle.Flags findFlags = Vehicle.Flags.Created;
            switch (material)
            {
                case TransferManager.TransferReason.Dead:
                case TransferManager.TransferReason.Garbage:
                case TransferManager.TransferReason.Sick:
                    findFlags |= Vehicle.Flags.TransferToSource;
                    break;

                case TransferManager.TransferReason.DeadMove:
                case TransferManager.TransferReason.GarbageMove:
                case TransferManager.TransferReason.SickMove:
                    findFlags |= Vehicle.Flags.TransferToSource;
                    break;
            }

            count = 0;
            vehicleId = building.m_ownVehicles;
            while (vehicleId != 0)
            {
                if (!ownVehicles.Contains(vehicleId) && (vehicles[vehicleId].m_flags & findFlags) == findFlags && vehicles[vehicleId].Info != null)
                {
                    if (vehicles[vehicleId].m_targetBuilding == targetBuildingId && (targetCitizenId == 0 || citizens[targetCitizenId].m_vehicle == vehicleId))
                    {
                        return vehicles[vehicleId].Info;
                    }

                    newVehicleId = vehicleId;
                    if ((vehicles[vehicleId].m_flags & Vehicle.Flags.WaitingTarget) == Vehicle.Flags.WaitingTarget)
                    {
                        waitingVehicleId = vehicleId;
                    }
                }

                if (count >= ushort.MaxValue)
                {
                    throw new Exception("Loop counter too high");
                }

                count++;
                vehicleId = vehicles[vehicleId].m_nextOwnVehicle;
            }

            if (waitingVehicleId != 0)
            {
                vehicleId = waitingVehicleId;
                //Log.DevDebug(typeof(BuildingHelper), "StartTransfer", "Waiting Vehicle", serviceBuildingId, targetBuildingId, targetCitizenId, material, vehicleId, vehicles[vehicleId].m_flags);
            }
            else if (newVehicleId != 0)
            {
                vehicleId = newVehicleId;
                //Log.DevDebug(typeof(BuildingHelper), "StartTransfer", "Guess Vehicle", serviceBuildingId, targetBuildingId, targetCitizenId, material, vehicleId, vehicles[vehicleId].m_flags);
            }
            else
            {
                vehicleId = 0;
                Log.Info(typeof(BuildingHelper), "StartTransfer", "Lost Vehicle", serviceBuildingId, targetBuildingId, targetCitizenId, material);

                return null;
            }

            if (!VehicleHelper.AssignTarget(vehicleId, ref vehicles[vehicleId], material, targetBuildingId, targetCitizenId))
            {
                return null;
            }

            return vehicles[vehicleId].Info;
        }

        /// <summary>
        /// Adds the service building information to debug information message.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="buildings">The buildings.</param>
        /// <param name="serviceBuilding">The service building.</param>
        /// <param name="tagSuffix">The tag suffix.</param>
        private static void AddServiceBuildingInfoToDebugInfoMsg(Log.InfoList info, Building[] buildings, ServiceBuildingInfo serviceBuilding, string tagSuffix)
        {
            if (serviceBuilding != null)
            {
                info.Add("CanReceive" + tagSuffix, serviceBuilding.CanReceive);
                info.Add("CapacityLevel" + tagSuffix, serviceBuilding.CapacityLevel);
                info.Add("CapactyFree" + tagSuffix, serviceBuilding.CapacityFree);
                info.Add("CapactyMax" + tagSuffix, serviceBuilding.CapacityMax);
                info.Add("CapactyOverflow" + tagSuffix, serviceBuilding.CurrentTargetCapacityOverflow);
                info.Add("CapacityPercent" + tagSuffix, serviceBuilding.CapacityPercent);
                info.Add("Range" + tagSuffix, serviceBuilding.Range);
                info.Add("VehiclesFree" + tagSuffix, serviceBuilding.VehiclesFree);
                info.Add("VehiclesSpare" + tagSuffix, serviceBuilding.VehiclesSpare);
                info.Add("VehiclesMade" + tagSuffix, serviceBuilding.VehiclesMade);
                info.Add("VehiclesTotal" + tagSuffix, serviceBuilding.VehiclesTotal);

                info.Add("CanReceive" + tagSuffix, serviceBuilding.CanReceive);
                info.Add("CanEmptyOther" + tagSuffix, serviceBuilding.CanEmptyOther);
                info.Add("CanBeEmptied" + tagSuffix, serviceBuilding.CanBeEmptied);

                info.Add("IsEmptying" + tagSuffix, serviceBuilding.IsEmptying);
                info.Add("IsAutoEmptying" + tagSuffix, serviceBuilding.IsAutoEmptying);
                info.Add("NeedsEmptying" + tagSuffix, serviceBuilding.NeedsEmptying);
                info.Add("EmptyingIsDone" + tagSuffix, serviceBuilding.EmptyingIsDone);

                info.Add("ServiceProblemCount" + tagSuffix, serviceBuilding.ServiceProblemCount);
                info.Add("ServiceProblemSize" + tagSuffix, serviceBuilding.ServiceProblemSize);

                ServiceBuildingInfo.AddToInfoMsg(info, serviceBuilding.BuildingId, ref buildings[serviceBuilding.BuildingId], tagSuffix);
            }
        }

        /// <summary>
        /// Adds the target building information to debug information message.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="targetBuilding">The target building.</param>
        /// <param name="tagSuffix">The tag suffix.</param>
        private static void AddTargetBuildingInfoToDebugInfoMsg(Log.InfoList info, TargetBuildingInfo targetBuilding, string tagSuffix)
        {
            if (targetBuilding != null)
            {
                info.Add("Demand" + tagSuffix, targetBuilding.Demand);
                info.Add("HasProblem" + tagSuffix, targetBuilding.HasProblem);
                info.Add("ProblemSize" + tagSuffix, targetBuilding.ProblemSize);
                info.Add("ProblemValue" + tagSuffix, targetBuilding.ProblemValue);

                info.Add("ProblemWeight" + tagSuffix, targetBuilding.ProblemWeight);
                info.Add("ServiceProblemCount" + tagSuffix, targetBuilding.ServiceProblemCount);
                info.Add("ServiceProblemSize" + tagSuffix, targetBuilding.ServiceProblemSize);
            }
        }

        /// <summary>
        /// Collects building info for debug use.
        /// </summary>
        /// <param name="buildings">The buildings.</param>
        /// <param name="vehicles">The vehicles.</param>
        /// <param name="districtManager">The district manager.</param>
        /// <param name="citizenManager">The citizen manager.</param>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="serviceBuilding">The service building.</param>
        /// <param name="targetBuilding">The target building.</param>
        /// <param name="buildingStamp">The building stamp.</param>
        /// <param name="verbose">If set to <c>true</c> include more information.</param>
        /// <returns>The debug information.</returns>
        private static Log.InfoList DebugInfoMsg(
            Building[] buildings,
            Vehicle[] vehicles,
            DistrictManager districtManager,
            CitizenManager citizenManager,
            ushort buildingId,
            ServiceBuildingInfo serviceBuilding,
            TargetBuildingInfo targetBuilding,
            BuildingStamp buildingStamp,
            bool verbose = false)
        {
            Log.InfoList info = new Log.InfoList();

            info.Add("BuildingId", buildingId);

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

            List<KeyValuePair<string, TargetBuildingInfo>> targetBuildings = null;
            List<KeyValuePair<string, ServiceBuildingInfo>> serviceBuildings = null;

            if (verbose && Global.Buildings != null)
            {
                //if (serviceBuilding == null)
                //{
                //    serviceBuilding = Global.Buildings.GetServiceBuilding(buildingId);
                //}

                //if (targetBuilding == null)
                //{
                //    targetBuilding = Global.Buildings.GetTargetBuilding(buildingId);
                //}

                targetBuildings = new List<KeyValuePair<string, TargetBuildingInfo>>();
                serviceBuildings = new List<KeyValuePair<string, ServiceBuildingInfo>>();

                if (serviceBuilding == null)
                {
                    if (Global.Buildings.GarbageBuildings != null && Global.Buildings.GarbageBuildings.TryGetValue(buildingId, out serviceBuilding))
                    {
                        serviceBuildings.Add(new KeyValuePair<string, ServiceBuildingInfo>("GB", serviceBuilding));
                    }

                    if (Global.Buildings.DeathCareBuildings != null && Global.Buildings.DeathCareBuildings.TryGetValue(buildingId, out serviceBuilding))
                    {
                        serviceBuildings.Add(new KeyValuePair<string, ServiceBuildingInfo>("DCB", serviceBuilding));
                    }

                    if (Global.Buildings.HealthCareBuildings != null && Global.Buildings.HealthCareBuildings.TryGetValue(buildingId, out serviceBuilding))
                    {
                        serviceBuildings.Add(new KeyValuePair<string, ServiceBuildingInfo>("HCB", serviceBuilding));
                    }

                    serviceBuilding = null;
                }

                if (targetBuilding == null)
                {
                    if (Global.Buildings.DeadPeopleBuildings != null && Global.Buildings.DeadPeopleBuildings.TryGetValue(buildingId, out targetBuilding))
                    {
                        targetBuildings.Add(new KeyValuePair<string, TargetBuildingInfo>("DPB", targetBuilding));
                    }

                    if (Global.Buildings.DirtyBuildings != null && Global.Buildings.DirtyBuildings.TryGetValue(buildingId, out targetBuilding))
                    {
                        targetBuildings.Add(new KeyValuePair<string, TargetBuildingInfo>("DB", targetBuilding));
                    }

                    if (Global.Buildings.SickPeopleBuildings != null && Global.Buildings.SickPeopleBuildings.TryGetValue(buildingId, out targetBuilding))
                    {
                        targetBuildings.Add(new KeyValuePair<string, TargetBuildingInfo>("SPB", targetBuilding));
                    }

                    targetBuilding = null;
                }
            }

            try
            {
                info.Add("AI", buildings[buildingId].Info.m_buildingAI.GetType());
                info.Add("InfoName", buildings[buildingId].Info.name);

                string name = GetBuildingName(buildingId);
                if (!String.IsNullOrEmpty(name) && name != buildings[buildingId].Info.name)
                {
                    info.Add("BuildingName", name);
                }
            }
            catch
            {
                info.Add("Error", "Info");
            }

            try
            {
                byte district = districtManager.GetDistrict(buildings[buildingId].m_position);
                info.Add("District", district);
                info.Add("DistrictName", districtManager.GetDistrictName(district));
            }
            catch (Exception ex)
            {
                info.Add("Exception", "District", ex.GetType().ToString(), ex.Message);
            }

            if (buildingStamp != null)
            {
                info.Add("Source", buildingStamp.Source);
                info.Add("SimulationTimeStamp", buildingStamp.SimulationTimeStamp);
                info.Add("SimulationTimeDelta", buildingStamp.SimulationTimeDelta);
            }

            AddServiceBuildingInfoToDebugInfoMsg(info, buildings, serviceBuilding, "B");
            if (serviceBuildings != null)
            {
                foreach (KeyValuePair<string, ServiceBuildingInfo> building in serviceBuildings)
                {
                    AddServiceBuildingInfoToDebugInfoMsg(info, buildings, building.Value, building.Key);
                }
            }

            AddTargetBuildingInfoToDebugInfoMsg(info, targetBuilding, "B");
            if (targetBuildings != null)
            {
                foreach (KeyValuePair<string, TargetBuildingInfo> building in targetBuildings)
                {
                    AddTargetBuildingInfoToDebugInfoMsg(info, building.Value, building.Key);
                }
            }

            if (verbose && Global.Buildings != null)
            {
                info.Add("Categories", Global.Buildings.GetCategories(buildingId));
            }

            float radius = float.NaN;
 
            int materialMax = 0;
            int materialAmount = 0;
            int serviceVehicleCount = 0;

            try
            {
                if (GetCapacityAmount(buildingId, ref buildings[buildingId], out materialAmount, out materialMax, out serviceVehicleCount))
                {
                    info.Add("CapacityAmount", materialAmount);
                    info.Add("CapacityMax", materialMax);
                    info.Add("ServiceVehicleCount", serviceVehicleCount);
                }

                serviceVehicleCount = 0;

                if (buildings[buildingId].Info.m_buildingAI is CemeteryAI)
                {
                    radius = ((CemeteryAI)buildings[buildingId].Info.m_buildingAI).m_deathCareRadius;

                    info.Add("DeathCareRadius", ((CemeteryAI)buildings[buildingId].Info.m_buildingAI).m_deathCareRadius);
                    info.Add("CorpseCapacity", ((CemeteryAI)buildings[buildingId].Info.m_buildingAI).m_corpseCapacity);
                    info.Add("GraveCount", ((CemeteryAI)buildings[buildingId].Info.m_buildingAI).m_graveCount);
                    info.Add("CustomBuffer1", buildings[buildingId].m_customBuffer1); // GraveUsed?
                    info.Add("CustomBuffer2", buildings[buildingId].m_customBuffer2);
                    info.Add("PR_HC_Calc", ((buildings[buildingId].m_productionRate * ((CemeteryAI)buildings[buildingId].Info.m_buildingAI).m_hearseCount) + 99) / 100); // Hearse capacity?
                    info.Add("IsFull", buildings[buildingId].Info.m_buildingAI.IsFull(buildingId, ref buildings[buildingId]));

                    buildings[buildingId].Info.m_buildingAI.GetMaterialAmount(buildingId, ref buildings[buildingId], TransferManager.TransferReason.Dead, out materialAmount, out materialMax);
                }
                else if (buildings[buildingId].Info.m_buildingAI is LandfillSiteAI)
                {
                    radius = ((LandfillSiteAI)buildings[buildingId].Info.m_buildingAI).m_collectRadius;

                    info.Add("CollectRadius", ((LandfillSiteAI)buildings[buildingId].Info.m_buildingAI).m_collectRadius);
                    info.Add("GarbageAmount", ((LandfillSiteAI)buildings[buildingId].Info.m_buildingAI).GetGarbageAmount(buildingId, ref buildings[buildingId]));
                    info.Add("GarbageCapacity", ((LandfillSiteAI)buildings[buildingId].Info.m_buildingAI).m_garbageCapacity);
                    info.Add("GarbageBuffer", buildings[buildingId].m_garbageBuffer);
                    info.Add("CustomBuffer1", buildings[buildingId].m_customBuffer1); // Garbage?
                    info.Add("IsFull", buildings[buildingId].Info.m_buildingAI.IsFull(buildingId, ref buildings[buildingId]));

                    buildings[buildingId].Info.m_buildingAI.GetMaterialAmount(buildingId, ref buildings[buildingId], TransferManager.TransferReason.Garbage, out materialAmount, out materialMax);
                }
                else if (buildings[buildingId].Info.m_buildingAI is HospitalAI)
                {
                    radius = ((HospitalAI)buildings[buildingId].Info.m_buildingAI).m_healthCareRadius;

                    info.Add("HealthCareRadius", ((HospitalAI)buildings[buildingId].Info.m_buildingAI).m_healthCareRadius);
                    info.Add("PatientCapacity", ((HospitalAI)buildings[buildingId].Info.m_buildingAI).m_patientCapacity);
                    info.Add("IsFull", buildings[buildingId].Info.m_buildingAI.IsFull(buildingId, ref buildings[buildingId]));

                    buildings[buildingId].Info.m_buildingAI.GetMaterialAmount(buildingId, ref buildings[buildingId], TransferManager.TransferReason.Sick, out materialAmount, out materialMax);
                }

                info.Add("materialMax", materialMax);
                info.Add("materialAmount", materialAmount);
                info.Add("materialFree", materialMax - materialAmount);
            }
            catch
            {
                info.Add("Error", "Material");
            }

            ushort ownVehicleCount = 0;
            ushort madeVehicleCount = 0;

            try
            {
                ushort vehicleId = buildings[buildingId].m_ownVehicles;
                while (vehicleId != 0 && ownVehicleCount < ushort.MaxValue)
                {
                    ownVehicleCount++;
                    try
                    {
                        if ((vehicles[vehicleId].m_transferType == (byte)TransferManager.TransferReason.Garbage || vehicles[vehicleId].m_transferType == (byte)TransferManager.TransferReason.Dead) &&
                            vehicles[vehicleId].Info != null &&
                            (vehicles[vehicleId].m_flags & Vehicle.Flags.Created) == Vehicle.Flags.Created &&
                            (vehicles[vehicleId].m_flags & VehicleHelper.VehicleExists) != ~VehicleHelper.VehicleAll)
                        {
                            madeVehicleCount++;
                        }
                    }
                    catch
                    {
                        info.Add("Error", "Vehicle");
                    }

                    vehicleId = vehicles[vehicleId].m_nextOwnVehicle;
                }
                info.Add("OwnVehicles", ownVehicleCount);
                info.Add("MadeVehicles", madeVehicleCount);
            }
            catch
            {
                info.Add("Error", "Vehicles");
            }

            int productionRate = buildings[buildingId].m_productionRate;

            info.Add("VehicleCount", serviceVehicleCount);
            info.Add("ProductionRate", productionRate);
            info.Add("VehicleCountNominal", ((productionRate * serviceVehicleCount) + 99) / 100);

            try
            {
                int budget = Singleton<EconomyManager>.instance.GetBudget(buildings[buildingId].Info.m_buildingAI.m_info.m_class);
                productionRate = PlayerBuildingAI.GetProductionRate(productionRate, budget);
                int productionRate100 = PlayerBuildingAI.GetProductionRate(100, budget);
                int actualVehicleCount = ((productionRate * serviceVehicleCount) + 99) / 100;
                int actualVehicleCount100 = ((productionRate100 * serviceVehicleCount) + 99) / 100;

                if (!float.IsNaN(radius))
                {
                    info.Add("Radius", radius);
                }

                info.Add("Budget", budget);
                info.Add("ProductionRateActual", productionRate, productionRate100);
                info.Add("VehicleCountActual", actualVehicleCount, actualVehicleCount100);
                info.Add("SpareVehicles", actualVehicleCount - ownVehicleCount, actualVehicleCount100 - ownVehicleCount);

                if (!float.IsNaN(radius))
                {
                    info.Add("ProductionRange", (double)productionRate * (double)radius * 0.00999999977648258);
                }
            }
            catch
            {
                info.Add("Error", "Budget");
            }

            try
            {
                float range = buildings[buildingId].Info.m_buildingAI.GetCurrentRange(buildingId, ref buildings[buildingId]);
                range = range * range * Global.Settings.RangeModifier;
                if (range < Global.Settings.RangeMinimum)
                {
                    info.Add("Range", range, '<', Global.Settings.RangeMinimum);
                }
                else if (range > Global.Settings.RangeMaximum)
                {
                    info.Add("Range", range, '>', Global.Settings.RangeMaximum);
                }
                else
                {
                    info.Add("Range", range, ">=<");
                }
            }
            catch
            {
                info.Add("Error", "Range");
            }

            try
            {
                List<string> needs = new List<string>();

                if (buildings[buildingId].m_garbageBuffer >= Global.Settings.Garbage.MinimumAmountForDispatch)
                {
                    needs.Add("Filthy");
                }
                else if (buildings[buildingId].m_garbageBuffer >= Global.Settings.Garbage.MinimumAmountForPatrol)
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
            }
            catch
            {
                info.Add("Error", "Needs");
            }

            info.Add("DeathProblemTimer", buildings[buildingId].m_deathProblemTimer);
            info.Add("HealthProblemTimer", buildings[buildingId].m_healthProblemTimer);
            info.Add("MajorProblemTimer", buildings[buildingId].m_majorProblemTimer);

            try
            {
                int citizens = 0;
                int count = 0;
                uint unitId = buildings[buildingId].m_citizenUnits;
                while (unitId != 0)
                {
                    CitizenUnit unit = citizenManager.m_units.m_buffer[unitId];

                    try
                    {
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
                    }
                    catch
                    {
                        info.Add("Error", "Citizen");
                    }

                    count++;
                    if (count > (int)ushort.MaxValue * 10)
                    {
                        break;
                    }

                    unitId = unit.m_nextUnit;
                }
                info.Add("DeadCitizens", citizens);
            }
            catch
            {
                info.Add("Error", "Citizens");
            }

            try
            {
                info.Add("GarbageAmount", buildings[buildingId].Info.m_buildingAI.GetGarbageAmount(buildingId, ref buildings[buildingId]));
                info.Add("GarbageBuffer", buildings[buildingId].m_garbageBuffer);
            }
            catch
            {
                info.Add("Error", "Garbage");
            }

            try
            {
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
            }
            catch
            {
                info.Add("Error", "Problems");
            }

            info.Add("FireIntensoty", buildings[buildingId].m_fireIntensity);

            try
            {
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
            }
            catch
            {
                info.Add("Error", "Flags");
            }

            try
            {
                string status = buildings[buildingId].Info.m_buildingAI.GetLocalizedStatus(buildingId, ref buildings[buildingId]);
                if (!String.IsNullOrEmpty(status))
                {
                    info.Add("Status", status);
                }
            }
            catch
            {
                info.Add("Error", "Status");
            }

            try
            {
                info.Add("AI", buildings[buildingId].Info.m_buildingAI.GetType().AssemblyQualifiedName);
            }
            catch
            {
                info.Add("Error", "AI");
            }

            return info;
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
                Log.InfoList info = DebugInfoMsg(buildings, vehicles, districtManager, citizenManager, buildingId, serviceBuilding, targetBuilding, buildingStamp);
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