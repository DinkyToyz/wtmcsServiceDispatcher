namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.SerializableSettings
{
    /// <summary>
    /// The different service types.
    /// </summary>
    public enum ServiceType
    {
        /// <summary>
        /// Not a service.
        /// </summary>
        None = 0,

        /// <summary>
        /// The death care standard service.
        /// </summary>
        DeathCare = 1,

        /// <summary>
        /// The garbage standard service.
        /// </summary>
        Garbage = 2,

        /// <summary>
        /// The health care standard service.
        /// </summary>
        HealthCare = 3,

        /// <summary>
        /// The wrecking crew hidden service.
        /// </summary>
        WreckingCrews = 4,

        /// <summary>
        /// The recovery crew hidden service.
        /// </summary>
        RecoveryCrews = 5
    }

    /// <summary>
    /// The different settings types.
    /// </summary>
    public enum SettingsType
    {
        /// <summary>
        /// Not a settngs block.
        /// </summary>
        None = 0,

        /// <summary>
        /// A standard service block.
        /// </summary>
        StandardService = 1,

        /// <summary>
        /// A hidden service block.
        /// </summary>
        HiddenService = 2,

        /// <summary>
        /// The service range settings block.
        /// </summary>
        ServiceRanges = 3,

        /// <summary>
        /// The compatibility settings block.
        /// </summary>
        Compatibility = 4
    }

    /// <summary>
    /// Result of deserialization.
    /// </summary>
    internal enum DeserializationResult
    {
        /// <summary>
        /// The end of data was reached.
        /// </summary>
        EndOfData = 0,

        /// <summary>
        /// Deserialization was successfull.
        /// </summary>
        Success = 1,

        /// <summary>
        /// An error occured.
        /// </summary>
        Error = 2
    }
}