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

                return GetDistrictName(districtManager, district);
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
        public static string GetDistrictName(Vector3 position)
        {
            if (!Log.LogNames)
            {
                return null;
            }

            try
            {
                DistrictManager districtManager = Singleton<DistrictManager>.instance;
                byte district = districtManager.GetDistrict(position);

                return GetDistrictName(districtManager, district);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the name of the district.
        /// </summary>
        /// <param name="districtManager">The district manager.</param>
        /// <param name="district">The district.</param>
        /// <returns>
        /// The name of the district.
        /// </returns>
        private static string GetDistrictName(DistrictManager districtManager, int district)
        {
            if (district == 0)
            {
                return "(no district)";
            }

            try
            {
                string name = districtManager.GetDistrictName(district);

                return String.IsNullOrEmpty(name) ? "(unknown district)" : name;
            }
            catch
            {
                return null;
            }
        }
    }
}