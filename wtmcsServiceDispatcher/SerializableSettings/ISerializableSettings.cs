namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.SerializableSettings
{
    /// <summary>
    /// Serializable settings interface.
    /// </summary>
    public interface ISerializableSettings
    {
        /// <summary>
        /// Gets the loaded version.
        /// </summary>
        /// <value>
        /// The loaded version.
        /// </value>
        int LoadedVersion { get; }

        /// <summary>
        /// Gets the maximum version.
        /// </summary>
        /// <value>
        /// The maximum version.
        /// </value>
        int MaxVersion { get; }

        /// <summary>
        /// Gets the minimum version.
        /// </summary>
        /// <value>
        /// The minimum version.
        /// </value>
        int MinVersion { get; }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <returns>The settings.</returns>
        Settings GetSettings();

        /// <summary>
        /// Initializes the settings.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Sets the settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        void SetSettings(Settings settings);
    }
}