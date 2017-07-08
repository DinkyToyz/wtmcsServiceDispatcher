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