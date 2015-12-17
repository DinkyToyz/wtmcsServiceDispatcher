using System;
using ColossalFramework;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Transport line helper functions.
    /// </summary>
    internal static class TransportLineHelper
    {
        /// <summary>
        /// Gets the name of the line.
        /// </summary>
        /// <param name="lineId">The line identifier.</param>
        /// <returns>
        /// The name of the line.
        /// </returns>
        public static string GetLineName(ushort lineId)
        {
            if (!Log.LogNames)
            {
                return null;
            }

            try
            {
                string name = Singleton<TransportManager>.instance.GetLineName(lineId);

                return String.IsNullOrEmpty(name) ? (string)null : name;
            }
            catch
            {
                return null;
            }
        }
    }
}