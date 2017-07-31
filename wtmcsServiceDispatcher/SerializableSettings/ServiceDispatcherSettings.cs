using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Serializable settings class.
    /// </summary>
    public class ServiceDispatcherSettings
    {
        /// <summary>
        /// The default assignment compatibility mode.
        /// </summary>
        public const ServiceDispatcherSettings.ModCompatibilityMode DefaultAssignmentCompatibilityMode = ServiceDispatcherSettings.ModCompatibilityMode.UseInstanciatedClassMethods;

        /// <summary>
        /// The default creation compatibility mode.
        /// </summary>
        public const ServiceDispatcherSettings.ModCompatibilityMode DefaultCreationCompatibilityMode = ServiceDispatcherSettings.ModCompatibilityMode.UseInstanciatedClassMethods;

        /// <summary>
        /// Whether something is allowed or not.
        /// </summary>
        public enum Allowance
        {
            /// <summary>
            /// The default rules applies.
            /// </summary>
            Default = 0,

            /// <summary>
            /// Never allowed.
            /// </summary>
            Never = 1,

            /// <summary>
            /// Always allowed.
            /// </summary>
            Always = 2
        }

        /// <summary>
        /// Order of building checks.
        /// </summary>
        public enum BuildingCheckOrder
        {
            /// <summary>
            /// Custom order.
            /// </summary>
            Custom = 0,

            /// <summary>
            /// 1, in range; 2, forgotten.
            /// </summary>
            InRange = 1,

            /// <summary>
            /// Straight order.
            /// </summary>
            FirstFirst = 2,

            /// <summary>
            /// 1, in range; 2, problematic out of range.
            /// </summary>
            InRangeFirst = 3,

            /// <summary>
            /// 1, problematic in range; 2, problematic; 3, in range.
            /// </summary>
            ProblematicFirst = 4,

            /// <summary>
            /// 1, very problematic in range; 2, very problematic; 3, in range; 4, problematic out of range.
            /// </summary>
            VeryProblematicFirst = 5,

            /// <summary>
            /// 1, forgotten in range; 2, forgotten out of range; 3, in range; 4, problematic out of range.
            /// </summary>
            ForgottenFirst = 6
        }

        /// <summary>
        /// Building check parameters.
        /// </summary>
        public enum BuildingCheckParameters
        {
            /// <summary>
            /// Custom parameters.
            /// </summary>
            Undefined = 0,

            /// <summary>
            /// Any buildings.
            /// </summary>
            Any = 1,

            /// <summary>
            /// Buildings in range.
            /// </summary>
            InRange = 2,

            /// <summary>
            /// Problematic buildings in range.
            /// </summary>
            ProblematicInRange = 3,

            /// <summary>
            /// Problematic buildings in or out of range.
            /// </summary>
            ProblematicIgnoreRange = 4,

            /// <summary>
            /// Problematic buildings in range.
            /// </summary>
            VeryProblematicInRange = 5,

            /// <summary>
            /// Problematic buildings in or out of range.
            /// </summary>
            VeryProblematicIgnoreRange = 6,

            /// <summary>
            /// Forgotten buildings in range.
            /// </summary>
            ForgottenInRange = 7,

            /// <summary>
            /// Forgotten buildings in or out of range.
            /// </summary>
            ForgottenIgnoreRange = 8
        }

        /// <summary>
        /// Setting for mod compatibility for object method calls.
        /// </summary>
        public enum ModCompatibilityMode
        {
            /// <summary>
            /// Use CSD custom code instead of object methods.
            /// </summary>
            UseCustomCode = 0,

            /// <summary>
            /// Cast object as original class before calling method.
            /// </summary>
            UseOriginalClassMethods = 1,

            /// <summary>
            /// Call method on instantiated object in the normal way.
            /// </summary>
            UseInstanciatedClassMethods = 2
        }

        /// <summary>
        /// Options for when to create spare vehicles.
        /// </summary>
        public enum SpareVehiclesCreation
        {
            /// <summary>
            /// Never create spare vehicles.
            /// </summary>
            Never = 0,

            /// <summary>
            /// Create spare vehicles when service building has no free vehicles.
            /// </summary>
            WhenNoFree = 1,

            /// <summary>
            /// Create spare vehicles when service building is closer to target than all free vehicles.
            /// </summary>
            WhenBuildingIsCloser = 2
        }

        /// <summary>
        /// Gets the current version.
        /// </summary>
        /// <value>
        /// The current version.
        /// </value>
        public static int CurrentVersion => SerializableSettings.SettingsVersion6.CurrentVersion;

        /// <summary>
        /// Gets the XML root.
        /// </summary>
        /// <value>
        /// The XML root.
        /// </value>
        public static XmlRootAttribute XmlRoot => new XmlRootAttribute("ServiceDispatcherSettings");

        /// <summary>
        /// Loads settings from the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        /// The settings.
        /// </returns>
        public static Settings Load(string fileName)
        {
            Settings settings = Load<SerializableSettings.SettingsVersion6>(fileName) ?? new Settings();

            //if (Global.EnableDevExperiments)
            //{
            //    TestSave<SerializableSettings.SettingsVersion6>(fileName, settings);
            //}

            return settings;
        }

        /// <summary>
        /// Saves settings to the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="settings">The settings.</param>
        public static void Save(string fileName, Settings settings)
        {
            Save<SerializableSettings.SettingsVersion6>(fileName, settings);

            //if (Global.EnableDevExperiments)
            //{
            //    TestSave<SerializableSettings.SettingsVersion6>(fileName, settings);
            //}
        }

        /// <summary>
        /// Loads settings from the specified file name.
        /// </summary>
        /// <typeparam name="T">The settings version type.</typeparam>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        /// The settings.
        /// </returns>
        internal static Settings Load<T>(string fileName) where T : class, SerializableSettings.ISerializableSettings, new()
        {
            Log.Debug(typeof(T), "Load", "Begin");

            try
            {
                if (File.Exists(fileName))
                {
                    Log.Info(typeof(T), "Load", fileName);

                    using (FileStream file = File.OpenRead(fileName))
                    {
                        bool canTryPrevious = true;
                        XmlSerializer ser = new XmlSerializer(typeof(T), ServiceDispatcherSettings.XmlRoot);

                        try
                        {
                            T cfg = ser.Deserialize(file) as T;

                            if (cfg == null)
                            {
                                throw new InvalidDataException("No data");
                            }

                            if (cfg.MinVersion > cfg.LoadedVersion)
                            {
                                throw new InvalidDataException("Data version too low: " + cfg.LoadedVersion.ToString() + " (" + cfg.MinVersion.ToString() + ")");
                            }
                            else if (cfg.MaxVersion < cfg.LoadedVersion)
                            {
                                canTryPrevious = false;
                                throw new InvalidDataException("Data version too high: " + cfg.LoadedVersion.ToString() + " (" + cfg.MinVersion.ToString() + ")");
                            }

                            Log.Debug(typeof(T), "Load", "Loaded");

                            cfg.Initialize();
                            Settings sets = cfg.GetSettings();

                            Log.Debug(typeof(T), "Load", "End");

                            return sets;
                        }
                        catch (Exception ex)
                        {
                            Log.Info(typeof(T), "Load", "Not loaded", fileName, ex.GetType(), ex.Message);

                            if (canTryPrevious)
                            {
                                if (typeof(T) == typeof(SerializableSettings.SettingsVersion6))
                                {
                                    return Load<SerializableSettings.SettingsVersion0>(fileName);
                                }
                            }

                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(T), "Load", ex);
            }

            Log.Debug(typeof(T), "Load", "End");

            return null;
        }

        /// <summary>
        /// Saves settings to the specified file name.
        /// </summary>
        /// <typeparam name="T">The settings version type.</typeparam>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="settings">The settings.</param>
        internal static bool Save<T>(string fileName, Settings settings) where T : class, SerializableSettings.ISerializableSettings, new()
        {
            Log.Debug(typeof(T), "Save", "Begin");

            try
            {
                string filePath = Path.GetDirectoryName(Path.GetFullPath(fileName));
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                if (File.Exists(fileName))
                {
                    try
                    {
                        File.Copy(fileName, fileName + ".bak", true);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(typeof(T), "Save", ex, "Copy to .bak failed");
                    }
                }

                Log.Info(typeof(T), "Save", fileName);

                using (FileStream file = File.Create(fileName))
                {
                    T cfg = new T();
                    cfg.SetSettings(settings);

                    XmlSerializer ser = new XmlSerializer(typeof(T), ServiceDispatcherSettings.XmlRoot);
                    ser.Serialize(file, cfg);
                    file.Flush();
                    file.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(typeof(T), "Save", ex);

                return false;
            }
            finally
            {
                Log.Debug(typeof(T), "Save", "End");
            }
        }

        /// <summary>
        /// Tests saving the settinsg as the specified version type.
        /// </summary>
        /// <typeparam name="T">The settings version type.</typeparam>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="settings">The settings.</param>
        private static void TestSave<T>(string fileName, Settings settings) where T : class, SerializableSettings.ISerializableSettings, new()
        {
            string fileNameBase = fileName + "." + typeof(T).Name + ".";

            Settings testSets = Load<T>(fileName);
            if (testSets != null)
            {
                Save<T>(fileNameBase + "0.xml", testSets);
            }

            Log.Debug(typeof(T), "TestSave", "Begin");

            if (Save<T>(fileNameBase + "1.xml", settings))
            {
                testSets = Load<T>(fileNameBase + "1.xml");

                if (testSets != null)
                {
                    Save<T>(fileNameBase + "2.xml", testSets);
                }
            }
        }

        /// <summary>
        /// Building checks preset information.
        /// </summary>
        public class BuildingChecksPresetInfo
        {
            /// <summary>
            /// The building checks.
            /// </summary>
            public BuildingCheckParameters[] BuildingChecks = null;

            /// <summary>
            /// The description.
            /// </summary>
            public string Description;

            /// <summary>
            /// The identifier.
            /// </summary>
            public BuildingCheckOrder Identifier;

            /// <summary>
            /// The name.
            /// </summary>
            public string Name;

            /// <summary>
            /// Initializes a new instance of the <see cref="BuildingChecksPresetInfo"/> class.
            /// </summary>
            public BuildingChecksPresetInfo()
            {
                this.Identifier = ServiceDispatcherSettings.BuildingCheckOrder.Custom;
                this.BuildingChecks = new BuildingCheckParameters[] { BuildingCheckParameters.Any };
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="BuildingChecksPresetInfo" /> class.
            /// </summary>
            /// <param name="buildingCheckOrder">The building check order preset.</param>
            public BuildingChecksPresetInfo(BuildingCheckOrder buildingCheckOrder)
            {
                this.Identifier = buildingCheckOrder;
                this.Name = Settings.GetBuildingCheckOrderName(buildingCheckOrder);
                this.Description = Settings.GetBuildingCheckOrderDescription(buildingCheckOrder);
                this.BuildingChecks = Settings.GetBuildingChecksParameters(this.Identifier);
            }

            ///// <summary>
            ///// Initializes a new instance of the <see cref="BuildingChecksPresetInfo"/> class.
            ///// </summary>
            ///// <param name="buildingChecks">The building checks.</param>
            //public BuildingChecksPresetInfo(string buildingChecks)
            //{
            //    this.Identifier = ServiceDispatcherSettings.BuildingCheckOrder.Custom;
            //    this.BuildingChecks = ToArray(buildingChecks);
            //}

            /// <summary>
            /// Converts string to building check array.
            /// </summary>
            /// <param name="buildingChecks">The building checks.</param>
            /// <param name="errors">The errors.</param>
            /// <returns>
            /// The building checks in an array.
            /// </returns>
            public static BuildingCheckParameters[] ToArray(string buildingChecks, out string errors)
            {
                if (String.IsNullOrEmpty(buildingChecks))
                {
                    errors = "No checks! 1";
                    return (buildingChecks == null) ? null : new BuildingCheckParameters[0];
                }

                StringBuilder errorString = new StringBuilder();
                List<BuildingCheckParameters> checks = new List<BuildingCheckParameters>();

                foreach (string check in buildingChecks.Split(',', ';', ':', '/', ' ', '\t', '\r', '\n'))
                {
                    if (!String.IsNullOrEmpty(check))
                    {
                        BuildingCheckParameters parameter;
                        if (Enums.TryConvertToBuildingCheckParameters(check, out parameter))
                        {
                            checks.Add(parameter);
                        }
                        else
                        {
                            if (errorString.Length > 0)
                            {
                                errorString.Append(", ");
                            }

                            errorString.Append(check);
                        }
                    }
                }

                if (errorString.Length > 0)
                {
                    errors = errorString.Insert(0, "Bad checks: ").ToString();
                }
                else if (checks.Count == 0)
                {
                    errors = "No checks! 2";
                }
                else
                {
                    errors = null;
                }

                return checks.ToArray();
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents the specified checks.
            /// </summary>
            /// <param name="buildingChecks">The building checks.</param>
            /// <returns>
            /// A <see cref="System.String" /> that represents the checks.
            /// </returns>
            public static string ToString(BuildingCheckParameters[] buildingChecks)
            {
                return buildingChecks == null
                    ? ""
                    : String.Join(", ", buildingChecks.WhereSelectToArray(c => c != BuildingCheckParameters.Undefined, c => c.ToString()));
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return ToString(this.BuildingChecks);
            }
        }
    }
}