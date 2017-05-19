using ColossalFramework;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.ObjectHelpers
{
    /// <summary>
    /// Helper for prisoners.
    /// </summary>
    internal static class PrisonerHelper
    {
        /// <summary>
        /// Check if prisoner is confused.
        /// </summary>
        /// <param name="data">The citizen.</param>
        /// <returns>True if prisoner is confused.</returns>
        public static bool IsConfused(ref Citizen data)
        {
            // From PrisonerAI.GetLocalizedStatus from original game code at version 1.4.1-f2.
            CitizenManager instance = Singleton<CitizenManager>.instance;
            ushort instanceID = data.m_instance;
            if ((int)instanceID != 0)
                return IsConfused(ref instance.m_instances.m_buffer[(int)instanceID]);
            if ((int)data.m_visitBuilding != 0)
            {
                ////target = InstanceID.Empty;
                ////target.Building = data.m_visitBuilding;
                ////return Locale.Get("CITIZEN_STATUS_VISITING");
                return false;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("CITIZEN_STATUS_CONFUSED");
            return true;
        }

        /// <summary>
        /// Check if prisoner is confused.
        /// </summary>
        /// <param name="data">The citizen instance.</param>
        /// <returns>True if prisoner is confused.</returns>
        public static bool IsConfused(ref CitizenInstance data)
        {
            // From PrisonerAI.GetLocalizedStatus from original game code at version 1.4.1-f2.
            ushort num = data.m_targetBuilding;
            if ((int)num != 0)
            {
                ////target = InstanceID.Empty;
                ////target.Building = num;
                ////return Locale.Get("CITIZEN_STATUS_VISITING");
                return false;
            }
            ////target = InstanceID.Empty;
            ////return Locale.Get("CITIZEN_STATUS_CONFUSED");
            return true;
        }
    }
}