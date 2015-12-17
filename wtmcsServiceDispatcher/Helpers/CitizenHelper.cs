using System;
using ColossalFramework;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Citizen helper functions.
    /// </summary>
    internal static class CitizenHelper
    {
        /// <summary>
        /// Gets the name of the citizen.
        /// </summary>
        /// <param name="citizenId">The citizen identifier.</param>
        /// <returns>
        /// The name of the citizen.
        /// </returns>
        public static string GetName(uint citizenId)
        {
            if (!Log.LogNames)
            {
                return null;
            }

            try
            {
                string name = Singleton<CitizenManager>.instance.GetCitizenName(citizenId);

                return String.IsNullOrEmpty(name) ? (string)null : name;
            }
            catch
            {
                return null;
            }
        }
    }
}