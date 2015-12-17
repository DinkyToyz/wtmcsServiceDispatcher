using System;
using System.Collections.Generic;
using ColossalFramework;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// District helper functions.
    /// </summary>
    internal static class DistrictHelper
    {
        /// <summary>
        /// Gets the name of the district.
        /// </summary>
        /// <param name="district">The district.</param>
        /// <returns>The name of the district.</returns>
        public static string GetDistrictName(int district)
        {
            DistrictManager districtManager = Singleton<DistrictManager>.instance;

            return districtManager.GetDistrictName(district);
        }
    }
}
