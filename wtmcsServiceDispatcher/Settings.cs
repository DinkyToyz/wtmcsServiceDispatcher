using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Mod settings.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// The settings version.
        /// </summary>
        public readonly int Version = 1;

        /// <summary>
        /// Wether dispatchers should care about districts or not.
        /// </summary>
        public bool DispatchByDistrict = false;

        /// <summary>
        /// Wether garbage trucks should be handled or not.
        /// </summary>
        public bool DispatchGarbageTrucks = true;

        /// <summary>
        /// Wether hearses should be handled or not.
        /// </summary>
        public bool DispatchHearses = true;

        /// <summary>
        /// Limit service building range.
        /// </summary>
        public bool LimitRange = true;

        /// <summary>
        /// The minimum amount of garbage to dispatch a truck for.
        /// </summary>
        public ushort MinimumGarbageForDispatch = 2000;

        /// <summary>
        /// The range modifier
        /// </summary>
        public float RangeModifier = 1.0f;

        /// <summary>
        /// Wether stopped garbage trucks should be removed from grid or not.
        /// </summary>
        public bool RemoveGarbageTrucksFromGrid = false;

        /// <summary>
        /// Wether stopped hearses should be removed from grid or not.
        /// </summary>
        public bool RemoveHearsesFromGrid = true;

        /// <summary>
        /// The save count.
        /// </summary>
        public uint SaveCount = 0;

        /// <summary>
        /// The display names for the building checks orders.
        /// </summary>
        private static Dictionary<BuildingCheckOrder, string> buildingCheckOrderNames = new Dictionary<BuildingCheckOrder, string>()
        {
            { BuildingCheckOrder.Custom, "Custom" },
            { BuildingCheckOrder.FirstFirst, "First first" },
            { BuildingCheckOrder.ForgottenFirst, "Forgotten first" },
            { BuildingCheckOrder.InRange, "In range" },
            { BuildingCheckOrder.InRangeFirst, "In range first" },
            { BuildingCheckOrder.ProblematicFirst, "Problematic first" }
        };

        /// <summary>
        /// The custom building checks.
        /// </summary>
        private BuildingCheckParameters[] deathChecksCustom = null;

        /// <summary>
        /// The dead people building checks presets.
        /// </summary>
        private BuildingCheckOrder deathChecksPreset = BuildingCheckOrder.InRange;

        /// <summary>
        /// The custom building checks.
        /// </summary>
        private BuildingCheckParameters[] garbageChecksCustom = null;

        /// <summary>
        /// The dirty building checks presets.
        /// </summary>
        private BuildingCheckOrder garbageChecksPreset = BuildingCheckOrder.InRange;

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

                this.DispatchByDistrict = settings.DispatchByDistrict;
                this.LimitRange = settings.LimitRange;
                this.RangeModifier = settings.RangeModifier;

                this.DispatchHearses = settings.DispatchHearses;
                this.RemoveHearsesFromGrid = settings.RemoveHearsesFromGrid;
                this.deathChecksPreset = settings.DeathChecksPreset;
                this.deathChecksCustom = settings.DeathChecksCustom;
                if (this.deathChecksPreset == BuildingCheckOrder.Custom && (this.deathChecksCustom == null || this.deathChecksCustom.Length == 0))
                {
                    this.deathChecksCustom = GetBuildingChecksParameters(BuildingCheckOrder.InRange);
                }

                this.DispatchGarbageTrucks = settings.DispatchGarbageTrucks;
                this.RemoveGarbageTrucksFromGrid = settings.RemoveGarbageTrucksFromGrid;
                this.MinimumGarbageForDispatch = settings.MinimumGarbageForDispatch;
                this.garbageChecksPreset = settings.GarbageChecksPreset;
                this.garbageChecksCustom = settings.GarbageChecksCustom;
                if (this.garbageChecksPreset == BuildingCheckOrder.Custom && (this.garbageChecksCustom == null || this.garbageChecksCustom.Length == 0))
                {
                    this.garbageChecksCustom = GetBuildingChecksParameters(BuildingCheckOrder.InRange);
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
            /// 1, in range; 2, forgotten.
            /// </summary>
            InRange = 1,

            /// <summary>
            /// Custom order.
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
            /// 1, forgotten in range; 2, forgotten out of range; 3, in range; 4, problematic out of range.
            /// </summary>
            ForgottenFirst = 5
        }

        /// <summary>
        /// Building check parameters.
        /// </summary>
        public enum BuildingCheckParameters
        {
            /// <summary>
            /// Custom parameters.
            /// </summary>
            Custom = 0,

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
            /// Forgotten buildings in range.
            /// </summary>
            ForgottenInRange = 5,

            /// <summary>
            /// Forgotten buildings in or out of range.
            /// </summary>
            ForgottenIgnoreRange = 6
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
        /// Gets the dead people building checks parameters.
        /// </summary>
        /// <value>
        /// The building checks parameters.
        /// </value>
        public BuildingCheckParameters[] DeathChecksParameters
        {
            get
            {
                return GetBuildingChecksParameters(this.deathChecksPreset, this.deathChecksCustom);
            }
        }

        /// <summary>
        /// Gets or sets the dead people building checks preset.
        /// </summary>
        /// <value>
        /// The building checks preset.
        /// </value>
        public BuildingCheckOrder DeathChecksPreset
        {
            get
            {
                return deathChecksPreset;
            }
            set
            {
                deathChecksPreset = value;

                if (value == BuildingCheckOrder.Custom && (deathChecksCustom == null || deathChecksCustom.Length == 0))
                {
                    deathChecksCustom = GetBuildingChecksParameters(BuildingCheckOrder.InRange);
                }
            }
        }

        /// <summary>
        /// Gets the garbage building checks parameters.
        /// </summary>
        /// <value>
        /// The building checks parameters.
        /// </value>
        public BuildingCheckParameters[] GarbageChecksParameters
        {
            get
            {
                return GetBuildingChecksParameters(this.garbageChecksPreset, this.garbageChecksCustom);
            }
        }

        /// <summary>
        /// Gets or sets the building checks preset.
        /// </summary>
        /// <value>
        /// The building checks preset.
        /// </value>
        public BuildingCheckOrder GarbageChecksPreset
        {
            get
            {
                return garbageChecksPreset;
            }
            set
            {
                garbageChecksPreset = value;

                if (value == BuildingCheckOrder.Custom && (garbageChecksCustom == null || garbageChecksCustom.Length == 0))
                {
                    garbageChecksCustom = GetBuildingChecksParameters(BuildingCheckOrder.InRange);
                }
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
        /// Gets the display name of the building check order.
        /// </summary>
        /// <param name="checkOrder">The check order.</param>
        /// <returns>The display name.</returns>
        public static string GetBuildingCheckOrderName(BuildingCheckOrder checkOrder)
        {
            return buildingCheckOrderNames.ContainsKey(checkOrder) ? buildingCheckOrderNames[checkOrder] : checkOrder.ToString();
        }

        /// <summary>
        /// Gets the building checks parameters for the specified check order.
        /// </summary>
        /// <param name="buildingChecks">The building check order.</param>
        /// <param name="customBuildingCheckParameters">The custom building check parameters.</param>
        /// <returns>
        /// The building checks parameters.
        /// </returns>
        public static BuildingCheckParameters[] GetBuildingChecksParameters(BuildingCheckOrder buildingChecks, BuildingCheckParameters[] customBuildingCheckParameters = null)
        {
            switch (buildingChecks)
            {
                case BuildingCheckOrder.FirstFirst:
                    return new BuildingCheckParameters[] { BuildingCheckParameters.Any };

                case BuildingCheckOrder.InRangeFirst:
                    return new BuildingCheckParameters[] { BuildingCheckParameters.InRange, BuildingCheckParameters.ProblematicIgnoreRange };

                case BuildingCheckOrder.ProblematicFirst:
                    return new BuildingCheckParameters[] { BuildingCheckParameters.ProblematicInRange, BuildingCheckParameters.ProblematicIgnoreRange, BuildingCheckParameters.InRange };

                case BuildingCheckOrder.ForgottenFirst:
                    return new BuildingCheckParameters[] { BuildingCheckParameters.ForgottenInRange, BuildingCheckParameters.ForgottenIgnoreRange, BuildingCheckParameters.InRange, BuildingCheckParameters.ProblematicIgnoreRange };

                case BuildingCheckOrder.InRange:
                    return new BuildingCheckParameters[] { BuildingCheckParameters.InRange, BuildingCheckParameters.ForgottenIgnoreRange };

                case Settings.BuildingCheckOrder.Custom:
                    return (customBuildingCheckParameters != null) ? customBuildingCheckParameters : GetBuildingChecksParameters(BuildingCheckOrder.InRange);

                default:
                    return GetBuildingChecksParameters(BuildingCheckOrder.FirstFirst);
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
        /// Logs the settings.
        /// </summary>
        public void LogSettings()
        {
            Log.Debug(this, "LogSettings", "DispatchByDistrict", DispatchByDistrict);
            Log.Debug(this, "LogSettings", "LimitRange", LimitRange);
            Log.Debug(this, "LogSettings", "RangeModifier", RangeModifier);

            Log.Debug(this, "LogSettings", "DispatchHearses", DispatchHearses);
            Log.Debug(this, "LogSettings", "RemoveHearsesFromGrid", RemoveHearsesFromGrid);
            Log.Debug(this, "LogSettings", "DeathChecks", (byte)deathChecksPreset, deathChecksPreset, GetBuildingCheckOrderName(deathChecksPreset));
            Log.Debug(this, "LogSettings", "DeathChecksParameters", String.Join(", ", DeathChecksParameters.Select(bc => bc.ToString()).ToArray()));
            if (deathChecksCustom != null)
            {
                Log.Debug(this, "LogSettings", "DeathChecksCustom", String.Join(", ", deathChecksCustom.Select(bc => bc.ToString()).ToArray()));
            }

            Log.Debug(this, "LogSettings", "DispatchGarbageTrucks", DispatchGarbageTrucks);
            Log.Debug(this, "LogSettings", "RemoveGarbageTrucksFromGrid", RemoveGarbageTrucksFromGrid);
            Log.Debug(this, "LogSettings", "MinimumGarbageForDispatch", MinimumGarbageForDispatch);
            Log.Debug(this, "LogSettings", "GarbageChecks", (byte)garbageChecksPreset, garbageChecksPreset, GetBuildingCheckOrderName(garbageChecksPreset));
            Log.Debug(this, "LogSettings", "GarbageChecksParameters", String.Join(", ", GarbageChecksParameters.Select(bc => bc.ToString()).ToArray()));
            if (garbageChecksCustom != null)
            {
                Log.Debug(this, "LogSettings", "GarbageChecksCustom", String.Join(", ", garbageChecksCustom.Select(bc => bc.ToString()).ToArray()));
            }

            Log.Debug(this, "LogSettings", Version, LoadedVersion, SaveCount);
        }

        /// <summary>
        /// Saves settings to the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public void Save(string fileName = null)
        {
            Log.Debug(this, "Save", "Begin");

            if (Log.LogALot || Library.IsDebugBuild) LogSettings();

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

                    cfg.DispatchByDistrict = this.DispatchByDistrict;
                    cfg.LimitRange = this.LimitRange;
                    cfg.RangeModifier = this.RangeModifier;

                    cfg.RemoveHearsesFromGrid = this.RemoveHearsesFromGrid;
                    cfg.DispatchHearses = this.DispatchHearses;
                    cfg.DeathChecksPreset = this.deathChecksPreset;
                    cfg.DeathChecksCustom = this.deathChecksCustom;
                    cfg.DeathChecksCurrent = this.DeathChecksParameters;

                    cfg.RemoveGarbageTrucksFromGrid = this.RemoveGarbageTrucksFromGrid;
                    cfg.DispatchGarbageTrucks = this.DispatchGarbageTrucks;
                    cfg.MinimumGarbageForDispatch = this.MinimumGarbageForDispatch;
                    cfg.GarbageChecksPreset = this.garbageChecksPreset;
                    cfg.GarbageChecksCustom = this.garbageChecksCustom;
                    cfg.GarbageChecksCurrent = this.GarbageChecksParameters;

                    cfg.BuildingChecksPresets = (Enum.GetValues(typeof(BuildingCheckOrder)) as BuildingCheckOrder[]).Where(bco => bco != BuildingCheckOrder.Custom).Select(bco => new ServiceDispatcherSettings.BuildingChecksPresetInfo(bco)).ToArray();
                    cfg.BuildingChecksPossible = (Enum.GetValues(typeof(BuildingCheckParameters)) as BuildingCheckParameters[]).Where(bcp => bcp != BuildingCheckParameters.Custom).ToArray();

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
            /// The possible building checks.
            /// </summary>
            public BuildingCheckParameters[] BuildingChecksPossible = null;

            /// <summary>
            /// The possible building checks presets.
            /// </summary>
            public BuildingChecksPresetInfo[] BuildingChecksPresets = null;

            /// <summary>
            /// The current dead people building checks.
            /// </summary>
            public BuildingCheckParameters[] DeathChecksCurrent = null;

            /// <summary>
            /// The custom building checks.
            /// </summary>
            public BuildingCheckParameters[] DeathChecksCustom = null;

            /// <summary>
            /// The dead people building checks presets.
            /// </summary>
            public BuildingCheckOrder DeathChecksPreset = BuildingCheckOrder.InRange;

            /// <summary>
            /// Wether the dispatch should be limited by district or not.
            /// </summary>
            public Boolean DispatchByDistrict = false;

            /// <summary>
            /// Wether garbage trucks should be handled or not.
            /// </summary>
            public Boolean DispatchGarbageTrucks = true;

            /// <summary>
            /// Wether hearses should be handled or not.
            /// </summary>
            public Boolean DispatchHearses = true;

            /// <summary>
            /// The current garbage building checks.
            /// </summary>
            public BuildingCheckParameters[] GarbageChecksCurrent = null;

            /// <summary>
            /// The custom building checks.
            /// </summary>
            public BuildingCheckParameters[] GarbageChecksCustom = null;

            /// <summary>
            /// The dirty building checks presets.
            /// </summary>
            public BuildingCheckOrder GarbageChecksPreset = BuildingCheckOrder.InRange;

            /// <summary>
            /// Limit service building range for target buildings without problems.
            /// </summary>
            public bool LimitRange = false;

            /// <summary>
            /// The minimum amount of garbage to dispatch a truck for.
            /// </summary>
            public ushort MinimumGarbageForDispatch = 2000;

            /// <summary>
            /// The range modifier
            /// </summary>
            public float RangeModifier = 1.0f;

            /// <summary>
            /// Wether stopped garbage trucks should be removed from grid or not.
            /// </summary>
            public bool RemoveGarbageTrucksFromGrid = false;

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
                /// The identifier
                /// </summary>
                public BuildingCheckOrder Identifier;

                /// <summary>
                /// Initializes a new instance of the <see cref="BuildingChecksPresetInfo"/> class.
                /// </summary>
                public BuildingChecksPresetInfo()
                {
                    Identifier = BuildingCheckOrder.Custom;
                    BuildingChecks = new BuildingCheckParameters[] { BuildingCheckParameters.Any };
                }

                /// <summary>
                /// Initializes a new instance of the <see cref="BuildingChecksPresetInfo" /> class.
                /// </summary>
                /// <param name="buildingCheckOrder">The building check order preset.</param>
                public BuildingChecksPresetInfo(BuildingCheckOrder buildingCheckOrder)
                {
                    this.Identifier = buildingCheckOrder;
                    this.BuildingChecks = GetBuildingChecksParameters(Identifier);
                }
            }
        }
    }
}