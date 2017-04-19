using ColossalFramework;
using System;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Detour class for GarbageTruckAI.TryCollectGarbage.
    /// </summary>
    internal class GarbageTruckAITryCollectGarbageDetour : MethodDetoursBase
    {
        /// <summary>
        /// The number of limitations.
        /// </summary>
        public static UInt64 Limitations = 0;

        /// <summary>
        /// The number of tries.
        /// </summary>
        public static UInt64 Tries = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="GarbageTruckAITryCollectGarbageDetour"/> class.
        /// </summary>
        public GarbageTruckAITryCollectGarbageDetour()
            : base()
        {
            Tries = 0;
            Limitations = 0;
        }

        /// <summary>
        /// The original class type.
        /// </summary>
        public override Type OriginalClassType
        {
            get
            {
                return typeof(GarbageTruckAI);
            }
        }

        /// <summary>
        /// The maximum game version for detouring.
        /// </summary>
        protected override uint MaxGameVersion => Settings.AboveMaxTestedGameVersion;

        /// <summary>
        /// The minimum game version for detouring.
        /// </summary>
        protected override uint MinGameVersion
        {
            get
            {
                return BuildConfig.MakeVersionNumber(1, 2, 0, BuildConfig.ReleaseType.Final, 0, BuildConfig.BuildType.Unknown);
            }
        }

        /// <summary>
        /// The original method name.
        /// </summary>
        protected override string OriginalMethodName
        {
            get
            {
                return "TryCollectGarbage";
            }
        }

        /// <summary>
        /// The replacement method name.
        /// </summary>
        protected override string ReplacementMethodName
        {
            get
            {
                return "GarbageTruckAI_TryCollectGarbage_Override";
            }
        }

        /// <summary>
        /// Logs the counts.
        /// </summary>
        public override void LogCounts()
        {
            Log.Debug(this, "Counts", Tries, Limitations);
        }

        /// <summary>
        /// Copied from original game code at game version 1.2.0.
        /// </summary>
        /// <param name="garbageTruckAI">The garbage truck AI instance.</param>
        /// <param name="vehicleID">The vehicle identifier.</param>
        /// <param name="vehicleData">The vehicle data.</param>
        /// <param name="frameData">The frame data.</param>
        /// <param name="buildingID">The building identifier.</param>
        /// <param name="building">The building.</param>
        private static void GarbageTruckAI_TryCollectGarbage_Original(GarbageTruckAI garbageTruckAI, ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort buildingID, ref Building building)
        {
            if ((double)Vector3.SqrMagnitude(building.CalculateSidewalkPosition() - frameData.m_position) >= 1024.0)
                return;
            int amountDelta = Mathf.Min(0, (int)vehicleData.m_transferSize - garbageTruckAI.m_cargoCapacity);
            if (amountDelta == 0)
                return;
            building.Info.m_buildingAI.ModifyMaterialBuffer(buildingID, ref building, (TransferManager.TransferReason)vehicleData.m_transferType, ref amountDelta);
            if (amountDelta == 0)
                return;
            vehicleData.m_transferSize += (ushort)Mathf.Max(0, -amountDelta);
        }

        /// <summary>
        /// Method overriding GarbageTruckAI.TryCollectGarbage.
        /// </summary>
        /// <param name="garbageTruckAI">The garbage truck AI instance.</param>
        /// <param name="vehicleID">The vehicle identifier.</param>
        /// <param name="vehicleData">The vehicle data.</param>
        /// <param name="frameData">The frame data.</param>
        /// <param name="buildingID">The building identifier.</param>
        /// <param name="building">The building.</param>
        private static void GarbageTruckAI_TryCollectGarbage_Override(GarbageTruckAI garbageTruckAI, ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort buildingID, ref Building building)
        {
            try
            {
                Tries++;

                ////if (Log.LogALot)
                ////{
                ////    Log.DevDebug(typeof(GarbageTruckAITryCollectGarbageDetour), "GarbageTruckAI_TryCollectGarbage_Override", vehicleID, buildingID);
                ////}

                if (garbageTruckAI == null || vehicleID == 0 || buildingID == 0)
                {
                    return;
                }

                ////Log.InfoList logInfo = null;
                ////if (Log.LogToFile && Log.LogALot)
                ////{
                ////    logInfo = NewGarbageVehicleInfoList(vehicleID, ref vehicleData, buildingID, ref building);
                ////}

                if (vehicleData.m_targetBuilding == 0)
                {
                    ////if (Log.LogToFile && Log.LogALot)
                    ////{
                    ////    Log.DevDebug(typeof(GarbageTruckAITryCollectGarbageDetour), "GarbageTruckAI_TryCollectGarbage_Override", "NoTarget", logInfo);
                    ////}

                    GarbageTruckAI_TryCollectGarbage_Original(garbageTruckAI, vehicleID, ref vehicleData, ref frameData, buildingID, ref building);
                    return;
                }

                if (vehicleData.m_targetBuilding == buildingID)
                {
                    ////if (Log.LogToFile && Log.LogALot)
                    ////{
                    ////    Log.DevDebug(typeof(GarbageTruckAITryCollectGarbageDetour), "GarbageTruckAI_TryCollectGarbage_Override", "IsTarget", logInfo);
                    ////}

                    GarbageTruckAI_TryCollectGarbage_Original(garbageTruckAI, vehicleID, ref vehicleData, ref frameData, buildingID, ref building);
                    return;
                }

                int freeCapacity = garbageTruckAI.m_cargoCapacity - vehicleData.m_transferSize;
                if (freeCapacity < 0)
                {
                    freeCapacity = 0;
                }
                ////if (logInfo != null)
                ////{
                ////    logInfo.Add("FreeCapacity", freeCapacity);
                ////}

                int buildingMax;
                int buildingAmount;
                building.Info.m_buildingAI.GetMaterialAmount(buildingID, ref building, (TransferManager.TransferReason)vehicleData.m_transferType, out buildingAmount, out buildingMax);
                ////if (logInfo != null)
                ////{
                ////    logInfo.Add("DirtyBuildingAmount", buildingAmount);
                ////}

                if (buildingAmount > freeCapacity)
                {
                    ////if (Log.LogToFile && Log.LogALot)
                    ////{
                    ////    Log.DevDebug(typeof(GarbageTruckAITryCollectGarbageDetour), "GarbageTruckAI_TryCollectGarbage_Override", "NoCapacity", logInfo);
                    ////}

                    Limitations++;

                    return;
                }

                Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
                Building targetBuilding = buildings[vehicleData.m_targetBuilding];

                int targetBuildingAmount;
                targetBuilding.Info.m_buildingAI.GetMaterialAmount(vehicleData.m_targetBuilding, ref building, (TransferManager.TransferReason)vehicleData.m_transferType, out targetBuildingAmount, out buildingMax);
                ////if (logInfo != null)
                ////{
                ////    logInfo.Add("TargetBuildingAmount", targetBuildingAmount);
                ////}

                if (buildingAmount + targetBuildingAmount > freeCapacity)
                {
                    ////if (Log.LogToFile && Log.LogALot)
                    ////{
                    ////    Log.DevDebug(typeof(GarbageTruckAITryCollectGarbageDetour), "GarbageTruckAI_TryCollectGarbage_Override", "NotEnoughCapacity", logInfo);
                    ////}

                    Limitations++;

                    return;
                }

                ////if (Log.LogToFile && Log.LogALot)
                ////{
                ////    Log.DevDebug(typeof(GarbageTruckAITryCollectGarbageDetour), "GarbageTruckAI_TryCollectGarbage_Override", "Default", logInfo);
                ////}

                GarbageTruckAI_TryCollectGarbage_Original(garbageTruckAI, vehicleID, ref vehicleData, ref frameData, buildingID, ref building);
            }
            catch (Exception ex)
            {
                Log.Error(typeof(GarbageTruckAITryCollectGarbageDetour), "GarbageTruckAI_TryCollectGarbage_Override", ex);

                Detours.Abort(Detours.Methods.GarbageTruckAI_TryCollectGarbage);
            }
        }

        /// <summary>
        /// Creates a new instance of an information list with garbage vehicle info.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="dirtyBuildingId">The dirty building identifier.</param>
        /// <param name="dirtyBuilding">The dirty building.</param>
        /// <returns>The information list.</returns>
        private static Log.InfoList NewGarbageVehicleInfoList(ushort vehicleId, ref Vehicle vehicle, ushort dirtyBuildingId, ref Building dirtyBuilding)
        {
            Log.InfoList infoList = new Log.InfoList();
            infoList.Add("SourceBuilding", vehicle.m_sourceBuilding, BuildingHelper.GetBuildingName(vehicle.m_sourceBuilding));
            infoList.Add("Vehicle", vehicleId, VehicleHelper.GetVehicleName(vehicleId));
            infoList.Add("DirtyBuilding", dirtyBuildingId, BuildingHelper.GetBuildingName(dirtyBuildingId));
            infoList.Add("TargetBuilding", vehicle.m_targetBuilding, BuildingHelper.GetBuildingName(vehicle.m_targetBuilding));

            return infoList;
        }
    }
}