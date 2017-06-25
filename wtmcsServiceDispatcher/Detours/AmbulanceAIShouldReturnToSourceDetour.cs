using ColossalFramework;
using System;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Detour class for AmbulanceAI.ShouldReturnToSourceDetour.
    /// </summary>
    internal class AmbulanceAIShouldReturnToSourceDetour : MethodDetoursBase
    {
        /// <summary>
        /// The number of calls to the detoured method.
        /// </summary>
        public static UInt64 Calls = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="AmbulanceAIShouldReturnToSourceDetour"/> class.
        /// </summary>
        public AmbulanceAIShouldReturnToSourceDetour()
            : base()
        {
            Calls = 0;
        }

        /// <summary>
        /// Gets the counts.
        /// </summary>
        /// <value>
        /// The counts.
        /// </value>
        public override ulong[] Counts
        {
            get
            {
                return new UInt64[] { Calls };
            }
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
        protected override uint MaxGameVersion => Settings.AboveMaxTestedGameVersion;

        /// <summary>
        /// The minimum game version for detouring.
        /// </summary>
        protected override uint MinGameVersion
        {
            get
            {
                return BuildConfig.MakeVersionNumber(1, 4, 0, BuildConfig.ReleaseType.Final, 0, BuildConfig.BuildType.Unknown);
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
                return "AmbulanceAI_ShouldReturnToSource_Override";
            }
        }

        /// <summary>
        /// Copied from original game code at game version 1.5.0-f4.
        /// </summary>
        /// <param name="ambulanceAI">The ambulance AI.</param>
        /// <param name="vehicleID">The vehicle identifier.</param>
        /// <param name="data">The data.</param>
        /// <returns>True if vehicle should return to source.</returns>
        private static bool AmbulanceAI_ShouldReturnToSource_Original(AmbulanceAI ambulanceAI, ushort vehicleID, ref Vehicle data)
        {
            if ((int)data.m_sourceBuilding != 0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                if ((instance.m_buildings.m_buffer[(int)data.m_sourceBuilding].m_flags & Building.Flags.Active) == Building.Flags.None && (int)instance.m_buildings.m_buffer[(int)data.m_sourceBuilding].m_fireIntensity == 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Vehicles with no target should always return to source.
        /// </summary>
        /// <param name="ambulanceAI">The ambulance AI.</param>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>True if vehicle should return to source.</returns>
        private static bool AmbulanceAI_ShouldReturnToSource_Override(AmbulanceAI ambulanceAI, ushort vehicleId, ref Vehicle vehicle)
        {
            Calls++;
            if (vehicle.m_sourceBuilding == 0)
            {
                return false;
            }

            if (vehicle.m_targetBuilding == 0 && (vehicle.m_flags & Vehicle.Flags.TransferToTarget) == ~VehicleHelper.VehicleAll)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                if (instance.m_buildings.m_buffer[vehicle.m_sourceBuilding].m_fireIntensity == 0)
                {
                    return true;
                }
            }

            return AmbulanceAI_ShouldReturnToSource_Original(ambulanceAI, vehicleId, ref vehicle);
        }
    }
}