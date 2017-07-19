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

                Log.Info(this, "OnSaveData", "LoadStates");
                this.LoadStandardServiceStates();
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
                Log.Info(this, "OnSaveData", "SaveStates");
                this.SaveStandardServiceStates();
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
        private byte[] LoadSerializedData(params string[] subIds)
        {
            string id = this.GetDataId(subIds);

            Log.Debug(this, "LoadSerializedData", id);

            try
            {
                byte[] data = this.serializableData.LoadData(id);
                if (data != null)
                {
                    Log.Debug(this, "LoadStandardServiceStates", "Data", id, data.Length);
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
        /// Loads the serialized data.
        /// </summary>
        /// <param name="subIds">The sub identifiers.</param>
        /// <returns>The deserialized binary data.</returns>
        private ushort[] LoadSerializedUShortData(params string[] subIds)
        {
            byte[] data = this.LoadSerializedData(subIds);

            if (data == null)
            {
                return null;
            }

            ushort[] ushortData = new ushort[data.Length / 2];
            if (ushortData.Length > 0)
            {
                Buffer.BlockCopy(data, 0, ushortData, 0, ushortData.Length * 2);
            }

            return ushortData;
        }

        /// <summary>
        /// Loads the standard service states.
        /// </summary>
        private void LoadStandardServiceStates()
        {
            Log.Debug(this, "LoadStandardServiceStates", "Begin");

            try
            {
                if (this.serializableData != null && Global.Buildings != null)
                {
                    Log.Debug(this, "LoadStandardServiceStates", "AutoEmptying");
                    Global.Buildings.DeserializeAutoEmptying(this.LoadSerializedUShortData("AutoEmptying"));

                    Log.Debug(this, "LoadStandardServiceStates", "TargetAssignments");
                    Global.Buildings.DeserializeTargetAssignments(this.LoadSerializedUShortData("TargetAssignments"));
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "LoadStandardServiceStates", ex);
            }

            Log.Debug(this, "LoadStandardServiceStates", "End");
        }

        /// <summary>
        /// Saves the serialized data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="subIds">The sub identifiers.</param>
        private void SaveSerializedData(byte[] data, params string[] subIds)
        {
            string id = this.GetDataId(subIds);

            if (data == null)
            {
                Log.DevDebug(this, "SaveSerializedData", id, "NoData");
                return;
            }

            Log.Debug(this, "SaveSerializedData", id, data.Length);

            try
            {
                this.serializableData.SaveData(id, data);
            }
            catch (Exception ex)
            {
                Log.Error(this, "SaveSerializedData", ex, id);
            }
        }

        /// <summary>
        /// Saves the serialized data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="subIds">The sub identifiers.</param>
        private void SaveSerializedData(ushort[] data, params string[] subIds)
        {
            if (data == null)
            {
                Log.DevDebug(this, "SaveSerializedData", this.GetDataId(subIds), "NoData");
                return;
            }

            byte[] byteData = new byte[data.Length * 2];
            if (byteData.Length > 0)
            {
                Buffer.BlockCopy(data, 0, byteData, 0, byteData.Length);
            }

            this.SaveSerializedData(byteData, subIds);
        }

        /// <summary>
        /// Saves the standard service states.
        /// </summary>
        private void SaveStandardServiceStates()
        {
            Log.Debug(this, "SaveStandardServiceStates", "Begin");

            try
            {
                if (Global.Buildings != null)
                {
                    this.SaveSerializedData(Global.Buildings.SerializeAutoEmptying(), "AutoEmptying");
                    this.SaveSerializedData(Global.Buildings.SerializeTargetAssignments(), "TargetAssignments");
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "SaveStandardServiceStates", ex);
            }

            Log.Debug(this, "SaveStandardServiceStates", "End");
        }
    }
}