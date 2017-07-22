using System;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Settings for hidden services.
    /// </summary>
    /// <seealso cref="WhatThe.Mods.CitiesSkylines.ServiceDispatcher.ServiceSettings" />
    public class HiddenServiceSettings : ServiceSettings
    {
        /// <summary>
        /// The automatic delay in seconds.
        /// </summary>
        public double DelaySeconds = 5.0 * 60.0;

        /// <summary>
        /// Initializes a new instance of the <see cref="HiddenServiceSettings"/> class.
        /// </summary>
        public HiddenServiceSettings() : base()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HiddenServiceSettings"/> class.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        public HiddenServiceSettings(SerializableSettings.ServiceType serviceType) : base(serviceType)
        {
            switch (serviceType)
            {
                case SerializableSettings.ServiceType.WreckingCrews:
                    this.VehicleNamePlural = "Bulldozers";
                    break;

                case SerializableSettings.ServiceType.RecoveryCrews:
                    this.VehicleNamePlural = "Recovery Services";
                    this.VehicleNameSingular = "Recovery";
                    break;

                default:
                    throw new InvalidOperationException("Not a hidden service: " + serviceType.ToString());
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HiddenServiceSettings"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public HiddenServiceSettings(HiddenServiceSettings settings) : base(settings)
        { }

        /// <summary>
        /// Gets or sets the automatic delay in minutes.
        /// </summary>
        /// <value>
        /// The automatic delay in minutes.
        /// </value>
        public double DelayMinutes
        {
            get
            {
                return this.DelaySeconds / 60.0;
            }

            set
            {
                this.DelaySeconds = (value < 0.0) ? 0.0 : value * 60.0;
            }
        }

        /// <summary>
        /// Copies valus from the specified object to this instance.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public override void CopyFrom(ServiceSettings settings)
        {
            base.CopyFrom(settings);

            if (settings is HiddenServiceSettings)
            {
                this.DelaySeconds = ((HiddenServiceSettings)settings).DelaySeconds;
            }
        }

        /// <summary>
        /// Logs the settings.
        /// </summary>
        public override void LogSettings()
        {
            base.LogSettings();

            Log.Debug(this, "LogSettings", this.VehicleNamePlural, "DelayMinutes", this.DelayMinutes);
            Log.Debug(this, "LogSettings", this.VehicleNamePlural, "DelaySeconds", this.DelaySeconds);
        }
    }
}