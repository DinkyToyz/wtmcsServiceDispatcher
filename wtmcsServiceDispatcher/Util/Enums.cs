using System;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Enum conversion utils.
    /// </summary>
    internal static class Enums
    {
        /// <summary>
        /// Converts to allowance.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The allowance.</returns>
        public static ServiceDispatcherSettings.Allowance ConvertToAllowance(int value)
        {
            foreach (ServiceDispatcherSettings.Allowance allowance in Enum.GetValues(typeof(ServiceDispatcherSettings.Allowance)))
            {
                if (value == (int)allowance)
                {
                    return allowance;
                }
            }

            throw new InvalidCastException("Cannot convert value (" + value.ToString() + ") to Allowance");
        }

        /// <summary>
        /// Converts to building check order.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The building check order.</returns>
        public static ServiceDispatcherSettings.BuildingCheckOrder ConvertToBuildingCheckOrder(int value)
        {
            foreach (ServiceDispatcherSettings.BuildingCheckOrder order in Enum.GetValues(typeof(ServiceDispatcherSettings.BuildingCheckOrder)))
            {
                if (value == (int)order)
                {
                    return order;
                }
            }

            throw new InvalidCastException("Cannot convert value (" + value.ToString() + ") to BuildingCheckOrder");
        }

        /// <summary>
        /// Converts to mod compatibility mode.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The mod compatibility mode</returns>
        public static ServiceDispatcherSettings.ModCompatibilityMode ConvertToModCompatibilityMode(int value)
        {
            foreach (ServiceDispatcherSettings.ModCompatibilityMode mode in Enum.GetValues(typeof(ServiceDispatcherSettings.ModCompatibilityMode)))
            {
                if (value == (int)mode)
                {
                    return mode;
                }
            }

            throw new InvalidCastException("Cannot convert value (" + value.ToString() + ") to ModCompatibilityMode");
        }

        /// <summary>
        /// Converts to spare vehicles creation.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The spare vehicles creation.</returns>
        public static ServiceDispatcherSettings.SpareVehiclesCreation ConvertToSpareVehiclesCreation(int value)
        {
            foreach (ServiceDispatcherSettings.SpareVehiclesCreation creation in Enum.GetValues(typeof(ServiceDispatcherSettings.SpareVehiclesCreation)))
            {
                if (value == (int)creation)
                {
                    return creation;
                }
            }

            throw new InvalidCastException("Cannot convert value (" + value.ToString() + ") to SpareVehiclesCreation");
        }

        /// <summary>
        /// Tries to convert to building check parameters.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="buildingCheckParameters">The building check parameters.</param>
        /// <returns>True on success.</returns>
        public static bool TryConvertToBuildingCheckParameters(string value, out ServiceDispatcherSettings.BuildingCheckParameters buildingCheckParameters)
        {
            value = value.ToLowerInvariant();
            foreach (ServiceDispatcherSettings.BuildingCheckParameters parameters in Enum.GetValues(typeof(ServiceDispatcherSettings.BuildingCheckParameters)))
            {
                if (value == parameters.ToString().ToLowerInvariant())
                {
                    buildingCheckParameters = parameters;
                    return true;
                }
            }

            buildingCheckParameters = ServiceDispatcherSettings.BuildingCheckParameters.Any;
            return false;
        }

        /// <summary>
        /// Tries to convert to transfer reason.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="transferReason">The transfer reason.</param>
        /// <returns>
        /// True on success.
        /// </returns>
        public static bool TryConvertToTransferReason(byte value, out TransferManager.TransferReason transferReason)
        {
            foreach (TransferManager.TransferReason reason in Enum.GetValues(typeof(TransferManager.TransferReason)))
            {
                if (value == (int)reason)
                {
                    transferReason = reason;
                    return true;
                }
            }

            transferReason = TransferManager.TransferReason.None;
            return false;
        }
    }
}