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
        /// The service type.
        /// </summary>
        private SerializableSettings.ServiceType? serviceType = null;

        /// <summary>
        /// The plural vehicle name value.
        /// </summary>
        private string vehicleNamePluralValue = null;

        /// <summary>
        /// The singular vehicle name value.
        /// </summary>
        private string vehicleNameSingularValue = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceSettings"/> class.
        /// </summary>
        public ServiceSettings()
        {
            this.serviceType = SerializableSettings.ServiceType.None;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceSettings"/> class.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        public ServiceSettings(SerializableSettings.ServiceType serviceType)
        {
            this.serviceType = serviceType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceSettings"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public ServiceSettings(ServiceSettings settings)
        {
            if (settings != null)
            {
                this.CopyFrom(settings);

                this.serviceType = settings.serviceType;
                this.vehicleNamePluralValue = settings.vehicleNamePluralValue;
                this.vehicleNameSingularValue = settings.vehicleNameSingularValue;
            }
        }

        /// <summary>
        /// Gets or sets the type of the service.
        /// </summary>
        /// <value>
        /// The type of the service.
        /// </value>
        public SerializableSettings.ServiceType ServiceType
        {
            get => (this.serviceType == null || !this.serviceType.HasValue) ? SerializableSettings.ServiceType.None : this.serviceType.Value;
            set
            {
                if (this.serviceType == null || !this.serviceType.HasValue)
                {
                    this.serviceType = value;
                }
                else
                {
                    throw new InvalidOperationException("Write-once property modification");
                }
            }
        }

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
                    return "Service vehicles";
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
                    return "Service vehicle";
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
        /// Copies valus from the specified object to this instance.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public virtual void CopyFrom(ServiceSettings settings)
        {
            this.DispatchVehicles = settings.DispatchVehicles;
        }

        /// <summary>
        /// Copies valus from this instance to the specified object.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public virtual void CopyTo(ServiceSettings settings)
        {
            settings.CopyFrom(this);
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