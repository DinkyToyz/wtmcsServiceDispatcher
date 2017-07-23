using System;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.SerializableSettings
{
    /// <summary>
    /// Binary settings serializer.
    /// </summary>
    internal static class BinarySettings
    {
        /// <summary>
        /// Whether this deserialized settings should actually be aplied.
        /// </summary>
        private static readonly bool applySettings = false;

        /// <summary>
        /// Deseralizes the specified serialized data and applies to global settings.
        /// </summary>
        /// <param name="serializedData">The serialized data.</param>
        /// <returns>True if any settings were applied.</returns>
        public static bool Deseralize(BinaryData serializedData)
        {
            bool applied = Deseralize(serializedData, Global.Settings);

            if (applied)
            {
                Global.ReInitializeHandlers();
            }

            return applied;
        }

        /// <summary>
        /// Deseralizes the specified serialized data.
        /// </summary>
        /// <param name="serializedData">The serialized data.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>True if any settings were applied.</returns>
        public static bool Deseralize(BinaryData serializedData, Settings settings)
        {
            if (serializedData == null || serializedData.Left == 0)
            {
                return false;
            }

            bool applied = false;

            try
            {
                Log.Debug(typeof(BinarySettings), "Deserialize", applySettings);

                byte version = serializedData.GetByte();
                if (version > 0)
                {
                    Log.Warning(typeof(BinarySettings), "Serialized data version too high", version, 0);
                    return applied;
                }

                while (DeserializeBlock(serializedData, settings) == DeserializationResult.Success)
                {
                    applied = applySettings;
                };
            }
            catch (Exception ex)
            {
                Log.Error(typeof(BinarySettings), "Deserialize", ex);
            }

            return applied;
        }

        /// <summary>
        /// Serializes the global settings.
        /// </summary>
        /// <returns>
        /// The serialized data.
        /// </returns>
        public static BinaryData Serialize()
        {
            return Serialize(Global.Settings);
        }

        /// <summary>
        /// Serializes the specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The serialized data.</returns>
        public static BinaryData Serialize(Settings settings)
        {
            BinaryData serializedData = new BinaryData();

            // Version.
            serializedData.Add((byte)0);

            // Global.
            SerializeCompatibilitySettings(serializedData, settings);
            SerializeRangeSettings(serializedData, settings);

            // Hidden services.
            SerializeHiddenServiceSettings(serializedData, settings.WreckingCrews);
            SerializeHiddenServiceSettings(serializedData, settings.RecoveryCrews);

            // Standard services.
            SerializeStandardServiceSettings(serializedData, settings.DeathCare);
            SerializeStandardServiceSettings(serializedData, settings.Garbage);
            SerializeStandardServiceSettings(serializedData, settings.HealthCare);

            return serializedData;
        }

        /// <summary>
        /// Deserializes the next block of data.
        /// </summary>
        /// <param name="serializedData">The serialized data.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>The deserialization result.</returns>
        private static DeserializationResult DeserializeBlock(BinaryData serializedData, Settings settings)
        {
            if (serializedData == null || serializedData.Left == 0)
            {
                return DeserializationResult.EndOfData;
            }

            SettingsType settingsType = serializedData.PeekSettingsType();
            Log.DevDebug(typeof(BinarySettings), "DeserializeBlock", applySettings, settingsType);

            switch (settingsType)
            {
                case SettingsType.StandardService:
                    return DeserializeStandardServiceSettings(serializedData, settings);

                case SettingsType.HiddenService:
                    return DeserializeHiddenServiceSettings(serializedData, settings);

                case SettingsType.ServiceRanges:
                    return DeserializeRangeSettings(serializedData, settings);

                case SettingsType.Compatibility:
                    return DeserializeCompatibilitySettings(serializedData, settings);

                default:
                    throw new InvalidOperationException("Unknown settings type: " + settingsType.ToString());
            }
        }

        /// <summary>
        /// Deserializes a compatibility settings block.
        /// </summary>
        /// <param name="serializedData">The serialized data.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>The deserialization result.</returns>
        private static DeserializationResult DeserializeCompatibilitySettings(BinaryData serializedData, Settings settings)
        {
            if (serializedData == null || serializedData.Left == 0)
            {
                return DeserializationResult.EndOfData;
            }

            Log.DevDebug(typeof(BinarySettings), "DeserializeCompatibilitySettings", applySettings);

            serializedData.ResetLocalCheckSum();

            SettingsType settingsType = serializedData.GetSettingsType();
            if (settingsType != SettingsType.Compatibility)
            {
                throw new InvalidOperationException("Not a compatibility settings block");
            }

            byte version = serializedData.GetByte();
            if (version > 0)
            {
                Log.Warning(typeof(BinarySettings), "Serialized data version too high", version, 0);
                return DeserializationResult.Error;
            }

            // Settings.
            ServiceDispatcherSettings.Allowance reflectionAllowance = serializedData.GetAllowance();
            bool blockTransferManagerOffers = serializedData.GetBool();
            ServiceDispatcherSettings.ModCompatibilityMode assignmentCompatibilityMode = serializedData.GetModCompatibilityMode();
            ServiceDispatcherSettings.ModCompatibilityMode creationCompatibilityMode = serializedData.GetModCompatibilityMode();

            serializedData.CheckLocalCheckSum();

            // Only use these settings if no settings file was loaded.
            if (!settings.Loaded)
            {
                Log.Debug(typeof(BinarySettings), "DeserializeCompatibilitySettings", applySettings, reflectionAllowance, blockTransferManagerOffers, assignmentCompatibilityMode, creationCompatibilityMode);

                if (applySettings)
                {
                    settings.ReflectionAllowance = reflectionAllowance;
                    settings.BlockTransferManagerOffers = blockTransferManagerOffers;
                    settings.AssignmentCompatibilityMode = assignmentCompatibilityMode;
                    settings.CreationCompatibilityMode = creationCompatibilityMode;
                }
            }

            return DeserializationResult.Success;
        }

        /// <summary>
        /// Deserializes a hidden service settings block.
        /// </summary>
        /// <param name="serializedData">The serialized data.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>The deserialization result.</returns>
        private static DeserializationResult DeserializeHiddenServiceSettings(BinaryData serializedData, Settings settings)
        {
            if (serializedData == null || serializedData.Left == 0)
            {
                return DeserializationResult.EndOfData;
            }

            Log.DevDebug(typeof(BinarySettings), "DeserializeHiddenServiceSettings", applySettings);

            serializedData.ResetLocalCheckSum();

            SettingsType settingsType = serializedData.GetSettingsType();
            if (settingsType != SettingsType.HiddenService)
            {
                throw new InvalidOperationException("Not a hidden service settings block");
            }

            byte version = serializedData.GetByte();
            if (version > 0)
            {
                Log.Warning(typeof(BinarySettings), "Serialized data version too high", version, 0);
                return DeserializationResult.Error;
            }

            HiddenServiceSettings serviceSettings;
            HiddenServiceSettings serializedSettings;
            ServiceType service = serializedData.GetServiceType();

            switch (service)
            {
                case ServiceType.WreckingCrews:
                    serviceSettings = settings.WreckingCrews;
                    break;

                case ServiceType.RecoveryCrews:
                    serviceSettings = settings.RecoveryCrews;
                    break;

                default:
                    Log.Warning(typeof(BinarySettings), "Not a hidden service", service);
                    serviceSettings = null;
                    break;
            }

            serializedSettings = new HiddenServiceSettings(serviceSettings);

            // Settings.
            serializedSettings.DispatchVehicles = serializedData.GetBool();
            serializedSettings.DelaySeconds = serializedData.GetDouble();

            serializedData.CheckLocalCheckSum();

            if (serviceSettings != null)
            {
                Log.Debug(typeof(BinarySettings), "DeserializeHiddenServiceSettings", applySettings, service, serializedSettings.DispatchVehicles, serializedSettings.DelaySeconds);

                if (applySettings)
                {
                    serviceSettings.CopyFrom(serializedSettings);
                }
            }

            return DeserializationResult.Success;
        }

        /// <summary>
        /// Deserializes a service range settings block.
        /// </summary>
        /// <param name="serializedData">The serialized data.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>The deserialization result.</returns>
        private static DeserializationResult DeserializeRangeSettings(BinaryData serializedData, Settings settings)
        {
            if (serializedData == null || serializedData.Left == 0)
            {
                return DeserializationResult.EndOfData;
            }

            Log.DevDebug(typeof(BinarySettings), "DeserializeRangeSettings", applySettings);

            serializedData.ResetLocalCheckSum();

            SettingsType settingsType = serializedData.GetSettingsType();
            if (settingsType != SettingsType.ServiceRanges)
            {
                throw new InvalidOperationException("Not a service range settings block");
            }

            byte version = serializedData.GetByte();
            if (version > 0)
            {
                Log.Warning(typeof(BinarySettings), "Serialized data version too high", version, 0);
                return DeserializationResult.Error;
            }

            // Settings.
            bool rangeLimit = serializedData.GetBool();
            float rangeMaximum = serializedData.GetFloat();
            float rangeMinimum = serializedData.GetFloat();
            float rangeModifier = serializedData.GetFloat();

            serializedData.CheckLocalCheckSum();

            Log.Debug(typeof(BinarySettings), "DeserializeRangeSettings", applySettings, rangeLimit, rangeMaximum, rangeMinimum, rangeModifier);

            if (applySettings)
            {
                settings.RangeLimit = rangeLimit;
                settings.RangeMaximum = rangeMaximum;
                settings.RangeMinimum = rangeMinimum;
                settings.RangeModifier = rangeModifier;
            }

            return DeserializationResult.Success;
        }

        /// <summary>
        /// Deserializes a standard service settings block.
        /// </summary>
        /// <param name="serializedData">The serialized data.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>The deserialization result.</returns>
        private static DeserializationResult DeserializeStandardServiceSettings(BinaryData serializedData, Settings settings)
        {
            if (serializedData == null || serializedData.Left == 0)
            {
                return DeserializationResult.EndOfData;
            }

            Log.DevDebug(typeof(BinarySettings), "DeserializeStandardServiceSettings", applySettings);

            serializedData.ResetLocalCheckSum();

            SettingsType settingsType = serializedData.GetSettingsType();
            if (settingsType != SettingsType.StandardService)
            {
                throw new InvalidOperationException("Not a standard service settings block");
            }

            byte version = serializedData.GetByte();
            if (version > 0)
            {
                Log.Warning(typeof(BinarySettings), "Serialized data version too high", version, 0);
                return DeserializationResult.Error;
            }

            StandardServiceSettings serviceSettings;
            StandardServiceSettings serializedSettings;
            ServiceType service = serializedData.GetServiceType();

            switch (service)
            {
                case ServiceType.DeathCare:
                    serviceSettings = settings.DeathCare;
                    break;

                case ServiceType.Garbage:
                    serviceSettings = settings.Garbage;
                    break;

                case ServiceType.HealthCare:
                    serviceSettings = settings.HealthCare;
                    break;

                default:
                    Log.Warning(typeof(BinarySettings), "Not a standard service", service);
                    serviceSettings = null;
                    break;
            }

            serializedSettings = new StandardServiceSettings(serviceSettings);

            // Settings.
            serializedSettings.DispatchVehicles = serializedData.GetBool();
            serializedSettings.DispatchByDistrict = serializedData.GetBool();
            serializedSettings.DispatchByRange = serializedData.GetBool();
            serializedSettings.AutoEmpty = serializedData.GetBool();
            serializedSettings.LimitOpportunisticCollection = serializedData.GetBool();
            serializedSettings.RemoveFromGrid = serializedData.GetBool();
            serializedSettings.IgnoreRangeUseClosestBuildings = serializedData.GetByte();
            serializedSettings.CreateSpares = serializedData.GetSpareVehiclesCreation();
            serializedSettings.ChecksPreset = serializedData.GetBuildingCheckOrder();
            serializedSettings.MinimumAmountForDispatch = serializedData.GetUshort();
            serializedSettings.MinimumAmountForPatrol = serializedData.GetUshort();
            serializedSettings.AutoEmptyStartLevelPercent = serializedData.GetByte();
            serializedSettings.AutoEmptyStopLevelPercent = serializedData.GetByte();

            // Custom check list.
            byte checksCustomLength = serializedData.GetByte();
            serializedSettings.ChecksCustom = serializedData.GetBuildingCheckParametersArray(checksCustomLength);

            serializedData.CheckLocalCheckSum();

            if (serviceSettings != null)
            {
                Log.Debug(typeof(BinarySettings), "DeserializeStandardServiceSettings", applySettings, service, serializedSettings.DispatchVehicles, serializedSettings.ChecksPreset, serializedSettings.ChecksParameters, serializedSettings.ChecksCustom);

                if (applySettings)
                {
                    serviceSettings.CopyFrom(serializedSettings);
                }
            }

            return DeserializationResult.Success;
        }

        /// <summary>
        /// Serializes the compatibility settings.
        /// </summary>
        /// <param name="serializedData">The serialized data.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>
        /// The serialized data.
        /// </returns>
        private static BinaryData SerializeCompatibilitySettings(BinaryData serializedData, Settings settings)
        {
            serializedData.ResetLocalCheckSum();

            // Settings types and version.
            serializedData.Add(SettingsType.Compatibility);
            serializedData.Add((byte)0);

            // Settings.
            serializedData.Add(settings.ReflectionAllowance);
            serializedData.Add(settings.BlockTransferManagerOffers);
            serializedData.Add(settings.AssignmentCompatibilityMode);
            serializedData.Add(settings.CreationCompatibilityMode);

            // Checksum
            serializedData.AddLocalCheckSum();

            return serializedData;
        }

        /// <summary>
        /// Serializes the hidden service settings.
        /// </summary>
        /// <param name="serializedData">The serialized data.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>
        /// The serialzed data.
        /// </returns>
        private static BinaryData SerializeHiddenServiceSettings(BinaryData serializedData, HiddenServiceSettings settings)
        {
            serializedData.ResetLocalCheckSum();

            // Settings types and version.
            serializedData.Add(SettingsType.HiddenService);
            serializedData.Add((byte)0);
            serializedData.Add(settings.ServiceType);

            // Settings.
            serializedData.Add(settings.DispatchVehicles);
            serializedData.Add(settings.DelaySeconds);

            // Checksum
            serializedData.AddLocalCheckSum();

            return serializedData;
        }

        /// <summary>
        /// Serializes the service range settings.
        /// </summary>
        /// <param name="serializedData">The serialized data.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>
        /// The serialized data.
        /// </returns>
        private static BinaryData SerializeRangeSettings(BinaryData serializedData, Settings settings)
        {
            serializedData.ResetLocalCheckSum();

            // Settings types and version.
            serializedData.Add(SettingsType.ServiceRanges);
            serializedData.Add((byte)0);

            // Settings.
            serializedData.Add(settings.RangeLimit);
            serializedData.Add(settings.RangeMaximum);
            serializedData.Add(settings.RangeMinimum);
            serializedData.Add(settings.RangeModifier);

            // Checksum
            serializedData.AddLocalCheckSum();

            return serializedData;
        }

        /// <summary>
        /// Serializes the standard service settings.
        /// </summary>
        /// <param name="serializedData">The serialized data.</param>
        /// <param name="settings">The settings.</param>
        private static void SerializeStandardServiceSettings(BinaryData serializedData, StandardServiceSettings settings)
        {
            serializedData.ResetLocalCheckSum();

            // Settings types and version.
            serializedData.Add(SettingsType.StandardService);
            serializedData.Add((byte)0);
            serializedData.Add(settings.ServiceType);

            // Simple settings.
            serializedData.Add(settings.DispatchVehicles);
            serializedData.Add(settings.DispatchByDistrict);
            serializedData.Add(settings.DispatchByRange);
            serializedData.Add(settings.AutoEmpty);
            serializedData.Add(settings.LimitOpportunisticCollection);
            serializedData.Add(settings.RemoveFromGrid);
            serializedData.Add(settings.IgnoreRangeUseClosestBuildings);
            serializedData.Add(settings.CreateSpares);
            serializedData.Add(settings.ChecksPreset);
            serializedData.Add(settings.MinimumAmountForDispatch);
            serializedData.Add(settings.MinimumAmountForPatrol);
            serializedData.Add((byte)settings.AutoEmptyStartLevelPercent);
            serializedData.Add((byte)settings.AutoEmptyStopLevelPercent);

            // Custom check list.
            serializedData.Add((byte)settings.ChecksCustom.Length);
            serializedData.Add(settings.ChecksCustom.TakeToArray(255));

            // Checksum
            serializedData.AddLocalCheckSum();
        }
    }
}