using ICities;
using System;
using System.Linq;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// The serializer.
    /// </summary>
    /// <seealso cref="ICities.SerializableDataExtensionBase" />
    public class SerializableDataExtension : SerializableDataExtensionBase
    {
        /// <summary>
        /// The serializable data.
        /// </summary>
        private ISerializableData serializableData;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableDataExtension"/> class.
        /// </summary>
        public SerializableDataExtension() : base()
        {
            Log.Debug(this, "Constructed");
        }

        /// <summary>
        /// Called when created.
        /// </summary>
        /// <param name="serializableData">The serializable data interface object.</param>
        public override void OnCreated(ISerializableData serializableData)
        {
            Log.Debug(this, "OnCreated");
            this.serializableData = serializableData;
            base.OnCreated(serializableData);
        }

        /// <summary>
        /// Called when loading data.
        /// </summary>
        public override void OnLoadData()
        {
            Log.Debug(this, "OnLoadData", "Begin");

            // Logging to output panel here crashed the game. :-(
            bool logToDebugOutputPanel = Log.LogToDebugOutputPanel;

            try
            {
                Log.LogToDebugOutputPanel = false;

                if (this.serializableData != null && Global.Settings != null)
                {
                    if (Global.Settings.LoadSettingsPerCity || !Global.Settings.Loaded)
                    {
                        Log.Info(this, "OnLoadData", "LoadSettings");

                        SerializableSettings.BinarySettings.Deseralize(this.LoadSerializedData("Settings"));
                    }

                    if (Global.Buildings != null)
                    {
                        Log.Info(this, "OnLoadData", "LoadBuildingStates");

                        Global.Buildings.DeserializeAutoEmptying(this.LoadSerializedData("AutoEmptying"));
                        Global.Buildings.DeserializeTargetAssignments(this.LoadSerializedData("TargetAssignments"));
                        Global.Buildings.DeserializeDesolateBuildings(this.LoadSerializedData("DesolateBuildings"));
                    }

                    if (Global.Vehicles != null)
                    {
                        Log.Info(this, "OnLoadData", "LoadVehicleStates");

                        Global.Vehicles.DeserializeStuckVehicles(this.LoadSerializedData("StuckVehicles"));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnLoadData", ex);
            }
            finally
            {
                Log.LogToDebugOutputPanel = logToDebugOutputPanel;
                base.OnLoadData();
            }

            Log.Debug(this, "OnLoadData", "End");
        }

        /// <summary>
        /// Called when released.
        /// </summary>
        public override void OnReleased()
        {
            Log.Debug(this, "OnReleased");
            this.serializableData = null;
            base.OnReleased();
        }

        /// <summary>
        /// Called when saving data.
        /// </summary>
        public override void OnSaveData()
        {
            Log.Debug(this, "OnSaveData", "Begin");

            try
            {
                if (Global.Settings != null)
                {
                    Log.Info(this, "OnSaveData", "SaveSettings");

                    this.SaveSerializedData(SerializableSettings.BinarySettings.Serialize(), "Settings");
                }

                if (Global.Buildings != null)
                {
                    Log.Info(this, "OnSaveData", "SaveBuildingStates");

                    this.SaveSerializedData(Global.Buildings.SerializeAutoEmptying(), "AutoEmptying");
                    this.SaveSerializedData(Global.Buildings.SerializeTargetAssignments(), "TargetAssignments");
                    this.SaveSerializedData(Global.Buildings.SerializeDesolateBuildings(), "DesolateBuildings");
                }

                if (Global.Vehicles != null)
                {
                    Log.Info(this, "OnSaveData", "SaveVehicleStates");

                    this.SaveSerializedData(Global.Vehicles.SerializeStuckVehicles(), "StuckVehicles");
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "OnSaveData", ex);
            }
            finally
            {
                base.OnSaveData();
            }

            Log.Debug(this, "OnSaveData", "End");
        }

        /// <summary>
        /// Gets the data identifier.
        /// </summary>
        /// <param name="subIds">The sub identifiers.</param>
        /// <returns>The data identifier.</returns>
        private string GetDataId(params string[] subIds)
        {
            return String.Join(".", (new string[] { Library.Name, Library.SteamWorkshopFileId.ToString() }).Union(subIds).ToArray());
        }

        /// <summary>
        /// Loads the serialized data.
        /// </summary>
        /// <param name="subIds">The sub identifiers.</param>
        /// <returns>The deserialized binary data.</returns>
        private SerializableSettings.BinaryData LoadSerializedData(params string[] subIds)
        {
            string id = this.GetDataId(subIds);

            Log.DevDebug(this, "LoadSerializedData", id);

            try
            {
                SerializableSettings.BinaryData data = new SerializableSettings.BinaryData(this.serializableData, id);
                if (data != null)
                {
                    Log.Debug(this, "LoadSerializedData", id, data.Length);
                }

                return data;
            }
            catch (Exception ex)
            {
                Log.Error(this, "LoadSerializedData", ex, id);
                return null;
            }
        }

        /// <summary>
        /// Saves the serialized data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="subIds">The sub identifiers.</param>
        private void SaveSerializedData(SerializableSettings.BinaryData data, params string[] subIds)
        {
            string id = this.GetDataId(subIds);

            if (data == null)
            {
                Log.DevDebug(this, "SaveSerializedData", id, "NoData");
                data = new SerializableSettings.BinaryData();
            }

            Log.Debug(this, "SaveSerializedData", id, data.Length);

            try
            {
                data.Save(this.serializableData, id);
            }
            catch (Exception ex)
            {
                Log.Error(this, "SaveSerializedData", ex, id);
            }
        }
    }
}