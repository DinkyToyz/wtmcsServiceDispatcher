using System;
using ColossalFramework;
using UnityEngine;

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
            if (!Log.LogNames)
            {
                return null;
            }

            try
            {
                DistrictManager districtManager = Singleton<DistrictManager>.instance;
                string name = districtManager.GetDistrictName(district);

                return String.IsNullOrEmpty(name) ? (string)null : name;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the name of the district for a position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>The name of the district.</returns>
        public static string GetDistrictNameForPosition(Vector3 position)
        {
            if (!Log.LogNames)
            {
                return null;
            }

            try
            {
                DistrictManager districtManager = Singleton<DistrictManager>.instance;
                string name = districtManager.GetDistrictName(districtManager.GetDistrict(position));

                return String.IsNullOrEmpty(name) ? (string)null : name;
            }
            catch
            {
                return null;
            }
        }
    }
}