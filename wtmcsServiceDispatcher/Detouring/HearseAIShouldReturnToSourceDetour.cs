using System;
using ColossalFramework;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Detour class for HearseAI.ShouldReturnToSourceDetour.
    /// </summary>
    internal class HearseAIShouldReturnToSourceDetour : MethodDetoursBase
    {
        /// <summary>
        /// The number of calls to the detoured method.
        /// </summary>
        public static UInt64 Calls = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="HearseAIShouldReturnToSourceDetour"/> class.
        /// </summary>
        public HearseAIShouldReturnToSourceDetour()
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
                return typeof(HearseAI);
            }
        }

        /// <summary>
        /// The maximum game version for detouring.
        /// </summary>
        protected override uint MaxGameVersion
        {
            get
            {
                return BuildConfig.MakeVersionNumber(1, 4, 0, BuildConfig.ReleaseType.Final, 0, BuildConfig.BuildType.Unknown);
            }
        }

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
                return "HearseAI_ShouldReturnToSource_Override";
            }
        }

        /// <summary>
        /// Logs the counts.
        /// </summary>
        public override void LogCounts()
        {
            Log.Debug(this, "Counts", Calls);
        }

        /// <summary>
        /// Copied from original game code at game version 1.2.2 f3.
        /// </summary>
        /// <param name="hearseAI">The hearse AI.</param>
        /// <param name="vehicleID">The vehicle identifier.</param>
        /// <param name="data">The data.</param>
        /// <returns>True if vehicle should return to source.</returns>
        private static bool HearseAI_ShouldReturnToSource_Original(HearseAI hearseAI, ushort vehicleID, ref Vehicle data)
        {
            if ((int)data.m_sourceBuilding != 0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                if ((instance.m_buildings.m_buffer[(int)data.m_sourceBuilding].m_flags & (Building.Flags.Active | Building.Flags.Downgrading)) != Building.Flags.Active && (int)instance.m_buildings.m_buffer[(int)data.m_sourceBuilding].m_fireIntensity == 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Vehicles with no target should always return to source.
        /// </summary>
        /// <param name="hearseAI">The hearse AI.</param>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>True if vehicle should return to source.</returns>
        private static bool HearseAI_ShouldReturnToSource_Override(HearseAI hearseAI, ushort vehicleId, ref Vehicle vehicle)
        {
            Calls++;
            if (vehicle.m_sourceBuilding == 0)
            {
                return false;
            }

            if (vehicle.m_targetBuilding == 0 && (vehicle.m_flags & Vehicle.Flags.TransferToTarget) == Vehicle.Flags.None)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                return instance.m_buildings.m_buffer[vehicle.m_sourceBuilding].m_fireIntensity == 0;
            }

            return HearseAI_ShouldReturnToSource_Original(hearseAI, vehicleId, ref vehicle);
        }
    }
}