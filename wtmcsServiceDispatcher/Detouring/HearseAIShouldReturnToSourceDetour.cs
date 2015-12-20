﻿using System;
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
                return BuildConfig.MakeVersionNumber(1, 3, 0, BuildConfig.ReleaseType.Final, 0, BuildConfig.BuildType.Unknown);
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
        /// Vehicles with no target should always return to source.
        /// </summary>
        /// <param name="garbageTruckAI">The hearse AI.</param>
        /// <param name="vehicleID">The vehicle identifier.</param>
        /// <param name="data">The data.</param>
        /// <returns>Always true.</returns>
        private static bool HearseAI_ShouldReturnToSource_Override(HearseAI garbageTruckAI, ushort vehicleID, ref Vehicle data)
        {
            Calls++;
            return true;
        }

        /// <summary>
        /// Copied from original game code at game version 1.2.2 f3.
        /// </summary>
        /// <param name="garbageTruckAI">The hearse AI.</param>
        /// <param name="vehicleID">The vehicle identifier.</param>
        /// <param name="data">The data.</param>
        /// <returns>True if vehicle should return to source.</returns>
        private static bool HearseAI_ShouldReturnToSource_Original(GarbageTruckAI garbageTruckAI, ushort vehicleID, ref Vehicle data)
        {
            if ((int)data.m_sourceBuilding != 0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                if ((instance.m_buildings.m_buffer[(int)data.m_sourceBuilding].m_flags & (Building.Flags.Active | Building.Flags.Downgrading)) != Building.Flags.Active && (int)instance.m_buildings.m_buffer[(int)data.m_sourceBuilding].m_fireIntensity == 0)
                    return true;
            }
            return false;
        }
     }
}
