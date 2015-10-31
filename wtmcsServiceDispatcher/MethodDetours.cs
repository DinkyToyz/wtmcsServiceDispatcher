using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Method detours.
    /// </summary>
    internal class MethodDetours : IDisposable
    {
        /// <summary>
        /// The GarbageTruckAI.TryCollectGarbage key.
        /// </summary>
        private static readonly string GarbageTruckAI_TryCollectGarbage_Key = "GarbageTruckAI_TryCollectGarbage";

        /// <summary>
        /// The maximum game version for detouring GarbageTruckAI.TryCollectGarbage.
        /// </summary>
        private static readonly uint GarbageTruckAI_TryCollectGarbage_MaxVer = BuildConfig.MakeVersionNumber(1, 3, 0, BuildConfig.ReleaseType.Final, 0, BuildConfig.BuildType.Unknown);

        /// <summary>
        /// The minimum game version for detouring GarbageTruckAI.TryCollectGarbage.
        /// </summary>
        private static readonly uint GarbageTruckAI_TryCollectGarbage_MinVer = BuildConfig.MakeVersionNumber(1, 2, 0, BuildConfig.ReleaseType.Final, 0, BuildConfig.BuildType.Unknown);

        /// <summary>
        /// Error when detouring GarbageTruckAI.TryCollectGarbage.
        /// </summary>
        private static bool garbageTruckAI_TryCollectGarbage_Error = false;

        /// <summary>
        /// The detours.
        /// </summary>
        private Dictionary<string, MonoDetour> detours = new Dictionary<string, MonoDetour>();

        /// <summary>
        /// Finalizes an instance of the <see cref="MethodDetours"/> class.
        /// </summary>
        ~MethodDetours()
        {
            this.Dispose();
        }

        /// <summary>
        /// Gets a value indicating whether GarbageTruckAI.TryCollectGarbage can be detoured.
        /// </summary>
        /// <value>
        /// <c>true</c> if [can_ detour_ garbage truck a i_ try collect garbage]; otherwise, <c>false</c>.
        /// </value>
        public bool Can_Detour_GarbageTruckAI_TryCollectGarbage
        {
            get
            {
                return !garbageTruckAI_TryCollectGarbage_Error &&
                       BuildConfig.APPLICATION_VERSION >= GarbageTruckAI_TryCollectGarbage_MinVer &&
                       BuildConfig.APPLICATION_VERSION < GarbageTruckAI_TryCollectGarbage_MaxVer;
            }
        }

        /// <summary>
        /// Detours GarbageTruckAI.TryCollectGarbage.
        /// </summary>
        public void Detour_GarbageTruckAI_TryCollectGarbage()
        {
            if (this.Can_Detour_GarbageTruckAI_TryCollectGarbage)
            {
                try
                {
                    this.Detour(GarbageTruckAI_TryCollectGarbage_Key, typeof(GarbageTruckAI), "TryCollectGarbage", "GarbageTruckAI_TryCollectGarbage_Override");
                }
                catch (Exception ex)
                {
                    garbageTruckAI_TryCollectGarbage_Error = true;
                    Log.Error(this, "Detour_GarbageTruckAI_TryCollectGarbage", ex);
                }
            }
        }

        /// <summary>
        /// Reverts all detours and releases the detour objects.
        /// </summary>
        public void Dispose()
        {
            this.Revert(true);
        }

        /// <summary>
        /// Reverts all detours.
        /// </summary>
        public void Revert()
        {
            this.Revert(false);
        }

        /// <summary>
        /// Reverts detours.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Revert(string key)
        {
            this.Revert(key, false);
        }

        /// <summary>
        /// Reverts the GarbageTruckAI.TryCollectGarbage.
        /// </summary>
        public void Revert_GarbageTruckAI_TryCollectGarbage()
        {
            this.Revert(GarbageTruckAI_TryCollectGarbage_Key);
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
            Log.InfoList logInfo = null;
            if (Log.LogToFile && Log.LogALot)
            {
                logInfo = NewGarbageVehicleInfoList(vehicleID, ref vehicleData, buildingID, ref building);
            }

            if (vehicleData.m_targetBuilding == 0)
            {
                if (Log.LogToFile && Log.LogALot)
                {
                    Log.Debug(typeof(MethodDetours), "GarbageTruckAI_TryCollectGarbage_Override", "NoTarget", logInfo);
                }

                GarbageTruckAI_TryCollectGarbage_Original(garbageTruckAI, vehicleID, ref vehicleData, ref frameData, buildingID, ref building);
                return;
            }

            if (vehicleData.m_targetBuilding == buildingID)
            {
                if (Log.LogToFile && Log.LogALot)
                {
                    Log.Debug(typeof(MethodDetours), "GarbageTruckAI_TryCollectGarbage_Override", "IsTarget", logInfo);
                }

                GarbageTruckAI_TryCollectGarbage_Original(garbageTruckAI, vehicleID, ref vehicleData, ref frameData, buildingID, ref building);
                return;
            }

            int freeCapacity = garbageTruckAI.m_cargoCapacity - vehicleData.m_transferSize;
            if (freeCapacity < 0)
            {
                freeCapacity = 0;
            }
            logInfo.Add("FreeCapacity", freeCapacity);

            int buildingMax;
            int buildingAmount;

            building.Info.m_buildingAI.GetMaterialAmount(buildingID, ref building, (TransferManager.TransferReason)vehicleData.m_transferType, out buildingAmount, out buildingMax);
            logInfo.Add("DirtyBuildingAmount", buildingAmount);

            if (buildingAmount > freeCapacity)
            {
                if (Log.LogToFile && Log.LogALot)
                {
                    Log.Debug(typeof(MethodDetours), "GarbageTruckAI_TryCollectGarbage_Override", "NoCapacity", logInfo);
                }

                return;
            }

            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            Building targetBuilding = buildings[vehicleData.m_targetBuilding];

            int targetBuildingAmount;
            targetBuilding.Info.m_buildingAI.GetMaterialAmount(buildingID, ref building, (TransferManager.TransferReason)vehicleData.m_transferType, out targetBuildingAmount, out buildingMax);
            logInfo.Add("TargetBuildingAmount", targetBuildingAmount);

            if (buildingAmount + targetBuildingAmount > freeCapacity)
            {
                if (Log.LogToFile && Log.LogALot)
                {
                    Log.Debug(typeof(MethodDetours), "GarbageTruckAI_TryCollectGarbage_Override", "NotEnoughCapacity", logInfo);
                }

                return;
            }

            if (Log.LogToFile && Log.LogALot)
            {
                Log.Debug(typeof(MethodDetours), "GarbageTruckAI_TryCollectGarbage_Override", "Default", logInfo);
            }

            GarbageTruckAI_TryCollectGarbage_Original(garbageTruckAI, vehicleID, ref vehicleData, ref frameData, buildingID, ref building);
        }

        /// <summary>
        /// Detours the specified method.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="originalClass">The original class.</param>
        /// <param name="originalMethodName">Name of the original method.</param>
        /// <param name="replacementMethodName">Name of the replacement method.</param>
        private void Detour(string key, Type originalClass, string originalMethodName, string replacementMethodName)
        {
            MonoDetour detour = this.detours.ContainsKey(key) ? this.detours[key] : null;
            Log.Info(this, "Detour", key, detour);

            if (detour == null)
            {
                detour = new MonoDetour(originalClass, typeof(MethodDetours), originalMethodName, replacementMethodName);
                this.detours[key] = detour;
                Log.Debug(this, "Detour", "Created", key, detour);
            }

            if (!detour.IsDetoured)
            {
                detour.Detour();
                Log.Debug(this, "Detour", "Detoured", key, detour);
            }
        }

        /// <summary>
        /// Reverts all detours.
        /// </summary>
        /// <param name="dispose">If set to <c>true</c> release the detour objects.</param>
        private void Revert(bool dispose = false)
        {
            string[] keys = this.detours.Keys.ToArray();
            foreach (string key in keys)
            {
                this.Revert(key);
            }

            if (dispose)
            {
                Log.Debug(this, "Clear");
                this.detours.Clear();
            }
        }

        /// <summary>
        /// Reverts all detours.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="dispose">If set to <c>true</c> release the detour object.</param>
        private void Revert(string key, bool dispose = false)
        {
            if (this.detours.ContainsKey(key))
            {
                if (this.detours[key].IsDetoured)
                {
                    Log.Info(this, "Revert", key, this.detours[key]);
                    try
                    {
                        this.detours[key].Revert();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(this, "Revert", ex, key, this.detours[key]);
                        this.detours.Remove(key);
                    }
                    Log.Info(this, "Reverted", key, this.detours[key]);
                }

                if (dispose)
                {
                    Log.Debug(this, "Dispose", key);
                    this.detours.Remove(key);
                }
            }
        }
    }
}
