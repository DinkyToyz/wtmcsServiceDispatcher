using ColossalFramework;
using System;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Detour class for GarbageTruckAI.ShouldReturnToSourceDetour.
    /// </summary>
    internal class GarbageTruckAIShouldReturnToSourceDetour : MethodDetoursBase
    {
        /// <summary>
        /// The number of calls to the detoured method.
        /// </summary>
        public static UInt64 Calls = 0;

        /// <summary>
        /// The number of returns.
        /// </summary>
        public static UInt64 Returns = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="GarbageTruckAIShouldReturnToSourceDetour"/> class.
        /// </summary>
        public GarbageTruckAIShouldReturnToSourceDetour()
            : base()
        {
            Calls = 0;
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
        protected override uint MaxGameVersion => Settings.MaxTestedGameVersion;

        /// <summary>
        /// The minimum game version for detouring.
        /// </summary>
        protected override uint MinGameVersion
        {
            get
            {
                return BuildConfig.MakeVersionNumber(1, 2, 2, BuildConfig.ReleaseType.Final, 0, BuildConfig.BuildType.Unknown);
            }
        }

        /// <summary>
        /// The original method name.
        /// </summary>
        protected override string OriginalMethodName
        {
            get
            {
                return "ShouldReturnToSource";
            }
        }

        /// <summary>
        /// The replacement method name.
        /// </summary>
        protected override string ReplacementMethodName
        {
            get
            {
                return "GarbageTruckAI_ShouldReturnToSource_Override";
            }
        }

        /// <summary>
        /// Logs the counts.
        /// </summary>
        public override void LogCounts()
        {
            Log.Debug(this, "Counts", Calls, Returns);
        }

        /// <summary>
        /// Copied from original game code at game version 1.5.0-f4.
        /// </summary>
        /// <param name="garbageTruckAI">The garbage truck AI.</param>
        /// <param name="vehicleID">The vehicle identifier.</param>
        /// <param name="data">The data.</param>
        /// <returns>True if vehicle should return to source.</returns>
        private static bool GarbageTruckAI_ShouldReturnToSource_Original(GarbageTruckAI garbageTruckAI, ushort vehicleID, ref Vehicle data)
        {
            if ((int)data.m_sourceBuilding != 0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                if (((int)instance.m_buildings.m_buffer[(int)data.m_sourceBuilding].m_productionRate == 0 || (instance.m_buildings.m_buffer[(int)data.m_sourceBuilding].m_flags & (Building.Flags.Downgrading | Building.Flags.BurnedDown)) != Building.Flags.None) && (int)instance.m_buildings.m_buffer[(int)data.m_sourceBuilding].m_fireIntensity == 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Vehicles with no target should always return to source.
        /// </summary>
        /// <param name="garbageTruckAI">The garbage truck AI.</param>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>True if vehicle should return to source.</returns>
        private static bool GarbageTruckAI_ShouldReturnToSource_Override(GarbageTruckAI garbageTruckAI, ushort vehicleId, ref Vehicle vehicle)
        {
            Calls++;

            if (vehicle.m_sourceBuilding == 0)
            {
                return false;
            }

            if (vehicle.m_targetBuilding == 0 && (vehicle.m_flags & Vehicle.Flags.TransferToTarget) == ~VehicleHelper.VehicleAll /* && (vehicle.m_flags & Vehicle.Flags.TransferToSource) == ~Vehicle.Flags.All */)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                if (instance.m_buildings.m_buffer[vehicle.m_sourceBuilding].m_fireIntensity == 0)
                {
                    Returns++;
                    return true;
                }
            }

            return GarbageTruckAI_ShouldReturnToSource_Original(garbageTruckAI, vehicleId, ref vehicle);
        }
    }
}