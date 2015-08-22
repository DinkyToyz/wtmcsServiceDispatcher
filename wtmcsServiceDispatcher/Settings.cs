using System;
using System.IO;
using System.Xml.Serialization;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Mod settings.
    /// </summary>
    internal class Settings
    {
        /// <summary>
        /// The settings version.
        /// </summary>
        public readonly int Version = 1;

        /// <summary>
        /// The building checks.
        /// </summary>
        public BuildingCheckOrder BuildingChecks = BuildingCheckOrder.InRangeFirst;

        /// <summary>
        /// The custom building checks.
        /// </summary>
        public BuildingCheckParameters[] BuildingChecksCustom = null;

        /// <summary>
        /// Wether dispatchers should care about districts or not.
        /// </summary>
        public bool DispatchByDistrict = false;

        /// <summary>
        /// Wether hearses should be handled or not.
        /// </summary>
        public bool DispatchHearses = true;

        /// <summary>
        /// Limit service building range for target buildings without problems.
        /// </summary>
        public bool LimitRange = false;

        /// <summary>
        /// The range modifier
        /// </summary>
        public float RangeModifier = 1.5f;

        /// <summary>
        /// Wether stopped hearses should be removed from grid or not.
        /// </summary>
        public bool RemoveHearsesFromGrid = false;

        /// <summary>
        /// The save count.
        /// </summary>
        public uint SaveCount = 0;

        /// <summary>
        /// The settings version in the loaded file.
        /// </summary>
        private int? loadedVersion = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        /// <param name="settings">The file settings.</param>
        public Settings(ServiceDispatcherSettings settings = null)
        {
            if (settings != null)
            {
                this.loadedVersion = settings.Version;
                this.DispatchHearses = settings.DispatchHearses;
                this.DispatchByDistrict = settings.DispatchByDistrict;
                this.RemoveHearsesFromGrid = settings.RemoveHearsesFromGrid;
                this.LimitRange = settings.LimitRange;
                this.RangeModifier = settings.RangeModifier;

                this.BuildingChecks = settings.BuildingChecks;
                this.BuildingChecksCustom = settings.BuildingChecksCustom;
                if (this.BuildingChecks == BuildingCheckOrder.Custom && (BuildingChecksCustom == null || BuildingChecksCustom.Length == 0))
                {
                    this.BuildingChecks = BuildingCheckOrder.InRangeFirst;
                }
            }
        }

        /// <summary>
        /// Order of builting checks.
        /// </summary>
        public enum BuildingCheckOrder
        {
            /// <summary>
            /// Custom order.
            /// </summary>
            Custom = 0,

            /// <summary>
            /// 1, in range; 2, problematic out of range.
            /// </summary>
            InRangeFirst = 1,

            /// <summary>
            /// 1, problematic in range; 2, problematic; 3, in range.
            /// </summary>
            ProblematicFirst = 2,

            /// <summary>
            /// 1, forgotten in range; 2, forgotten out of range; 3, in range; 4, problematic out of range.
            /// </summary>
            ForgottenFirst = 3
        }

        /// <summary>
        /// Building check parameters.
        /// </summary>
        public enum BuildingCheckParameters
        {
            /// <summary>
            /// Buildings in range.
            /// </summary>
            InRange = 0,

            /// <summary>
            /// Problematic buildings in range.
            /// </summary>
            ProblematicInRange = 1,

            /// <summary>
            /// Problematic buildings in or out of range.
            /// </summary>
            ProblematicIgnoreRange = 2,

            /// <summary>
            /// Forgotten buildings in range.
            /// </summary>
            ForgottenInRange = 3,

            /// <summary>
            /// Forgotten buildings in or out of range.
            /// </summary>
            ForgottenIgnoreRange = 4
        }

        /// <summary>
        /// Gets the complete path.
        /// </summary>
        /// <value>
        /// The complete path.
        /// </value>
        public static string FilePathName
        {
            get
            {
                return FileSystem.FilePathName(".xml");
            }
        }

        /// <summary>
        /// Gets the settings version in the loaded file.
        /// </summary>
        /// <value>
        /// The settings version in the loaded file.
        /// </value>
        public int LoadedVersion
        {
            get
            {
                return (loadedVersion == null || !loadedVersion.HasValue) ? 0 : loadedVersion.Value;
            }
        }

        /// <summary>
        /// Loads settings from the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The settings.</returns>
        public static Settings Load(string fileName = null)
        {
            Log.Debug(typeof(Settings), "Load", "Begin");

            try
            {
                if (fileName == null)
                {
                    fileName = FilePathName;
                }

                if (File.Exists(fileName))
                {
                    Log.Info(typeof(Settings), "Load", fileName);

                    using (FileStream file = File.OpenRead(fileName))
                    {
                        XmlSerializer ser = new XmlSerializer(typeof(ServiceDispatcherSettings));
                        ServiceDispatcherSettings cfg = ser.Deserialize(file) as ServiceDispatcherSettings;
                        if (cfg != null)
                        {
                            Log.Debug(typeof(Settings), "Load", "Loaded");

                            Settings sets = new Settings(cfg);

                            Log.Debug(typeof(Settings), "Load", "End");
                            return sets;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Settings), "Load", ex);
            }

            Log.Debug(typeof(Settings), "Load", "End");
            return new Settings();
        }

        /// <summary>
        /// Gets the building checks parameters for the specified check order.
        /// </summary>
        /// <param name="buildingChecks">The building check order.</param>
        /// <returns></returns>
        public BuildingCheckParameters[] GetBuildingChecksParameters(BuildingCheckOrder buildingChecks)
        {
            switch (buildingChecks)
            {
                case Settings.BuildingCheckOrder.InRangeFirst:
                    return new BuildingCheckParameters[] { BuildingCheckParameters.InRange, BuildingCheckParameters.ProblematicIgnoreRange };

                case Settings.BuildingCheckOrder.ProblematicFirst:
                    return new BuildingCheckParameters[] { BuildingCheckParameters.ProblematicInRange, BuildingCheckParameters.ProblematicIgnoreRange, BuildingCheckParameters.InRange };

                case Settings.BuildingCheckOrder.ForgottenFirst:
                    return new BuildingCheckParameters[] { BuildingCheckParameters.ForgottenInRange, BuildingCheckParameters.ForgottenIgnoreRange, BuildingCheckParameters.InRange, BuildingCheckParameters.ProblematicIgnoreRange };

                case Settings.BuildingCheckOrder.Custom:
                    return (BuildingChecksCustom != null) ? BuildingChecksCustom : GetBuildingChecksParameters(BuildingChecks != BuildingCheckOrder.Custom ? BuildingChecks : BuildingCheckOrder.InRangeFirst);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Saves settings to the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public void Save(string fileName = null)
        {
            Log.Debug(this, "Save", "Begin");

            try
            {
                if (fileName == null)
                {
                    fileName = FilePathName;
                }

                string filePath = Path.GetDirectoryName(Path.GetFullPath(fileName));
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                Log.Info(this, "Save", fileName);

                SaveCount++;
                using (FileStream file = File.Create(fileName))
                {
                    ServiceDispatcherSettings cfg = new ServiceDispatcherSettings();
                    cfg.DispatchHearses = this.DispatchHearses;
                    cfg.DispatchByDistrict = this.DispatchByDistrict;
                    cfg.RemoveHearsesFromGrid = this.RemoveHearsesFromGrid;
                    cfg.LimitRange = this.LimitRange;
                    cfg.RangeModifier = this.RangeModifier;
                    cfg.BuildingChecks = this.BuildingChecks;
                    cfg.BuildingChecksCustom = this.BuildingChecksCustom;
                    cfg.Version = this.Version;
                    cfg.SaveCount = SaveCount;

                    XmlSerializer ser = new XmlSerializer(typeof(ServiceDispatcherSettings));
                    ser.Serialize(file, cfg);
                    file.Flush();
                    file.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "Save", ex);
            }

            Log.Debug(this, "Save", "End");
        }

        /// <summary>
        /// Serializable settings class.
        /// </summary>
        [Serializable]
        public class ServiceDispatcherSettings
        {
            /// <summary>
            /// The building checks.
            /// </summary>
            public BuildingCheckOrder BuildingChecks = BuildingCheckOrder.InRangeFirst;

            /// <summary>
            /// The custom building checks.
            /// </summary>
            public BuildingCheckParameters[] BuildingChecksCustom = null;

            public Boolean DispatchByDistrict = false;

            /// <summary>
            /// Wether hearses should be handled or not.
            /// </summary>
            public Boolean DispatchHearses = true;

            /// <summary>
            /// Limit service building range for target buildings without problems.
            /// </summary>
            public bool LimitRange = false;

            /// <summary>
            /// The range modifier
            /// </summary>
            public float RangeModifier = 1.0f;

            /// <summary>
            /// Wether stopped hearses should be removed from grid or not.
            /// </summary>
            public bool RemoveHearsesFromGrid = false;

            /// <summary>
            /// The save count.
            /// </summary>
            public uint SaveCount = 0;

            /// <summary>
            /// The settings version.
            /// </summary>
            public int Version = 0;
        }
    }
}