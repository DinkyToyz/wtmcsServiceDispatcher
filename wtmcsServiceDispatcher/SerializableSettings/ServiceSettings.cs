using System;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Abstract base class for service settings.
    /// </summary>
    public abstract class ServiceSettings
    {
        /// <summary>
        /// The dispatch toggle.
        /// </summary>
        public bool DispatchVehicles = false;

        /// <summary>
        /// The plural vehicle name value.
        /// </summary>
        private string vehicleNamePluralValue = null;

        /// <summary>
        /// The singular vehicle name value.
        /// </summary>
        private string vehicleNameSingularValue = null;

        /// <summary>
        /// Gets or sets the plural vehicle name.
        /// </summary>
        /// <value>
        /// The plural vehicle name.
        /// </value>
        /// <exception cref="System.InvalidOperationException">Write-once property modification.</exception>
        public string VehicleNamePlural
        {
            get
            {
                if (this.vehicleNamePluralValue != null)
                {
                    return this.vehicleNamePluralValue;
                }
                else if (this.VehicleNameSingular != null)
                {
                    return this.VehicleNameSingular + "s";
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (this.vehicleNamePluralValue == null)
                {
                    this.vehicleNamePluralValue = value;
                }
                else
                {
                    throw new InvalidOperationException("Write-once property modification");
                }
            }
        }

        /// <summary>
        /// Gets or sets the singular vehicle name.
        /// </summary>
        /// <value>
        /// The singular vehicle name.
        /// </value>
        /// <exception cref="System.InvalidOperationException">Write-once property modification.</exception>
        public string VehicleNameSingular
        {
            get
            {
                if (this.vehicleNameSingularValue != null)
                {
                    return this.vehicleNameSingularValue;
                }
                else if (this.vehicleNamePluralValue != null && this.vehicleNamePluralValue.Length > 1)
                {
                    return this.vehicleNamePluralValue.Substring(0, this.vehicleNamePluralValue.Length - 1);
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (this.vehicleNameSingularValue == null)
                {
                    this.vehicleNameSingularValue = value;
                }
                else
                {
                    throw new InvalidOperationException("Write-once property modification");
                }
            }
        }

        /// <summary>
        /// Logs the settings.
        /// </summary>
        public virtual void LogSettings()
        {
            Log.Debug(this, "LogSettings", this.VehicleNamePlural, "DispatchVehicles", this.DispatchVehicles);
        }
    }
}