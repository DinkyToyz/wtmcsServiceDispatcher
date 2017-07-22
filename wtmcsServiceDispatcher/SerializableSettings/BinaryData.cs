using ICities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.SerializableSettings
{
    /// <summary>
    /// Binary data container for serialization.
    /// </summary>
    internal class BinaryData
    {
        /// <summary>
        /// The block size.
        /// </summary>
        private int blockSize = 1024;

        /// <summary>
        /// The index.
        /// </summary>
        private int index = 0;

        /// <summary>
        /// Whether this instance is writeable or readable.
        /// </summary>
        private bool isWriteable;

        /// <summary>
        /// The local checksum index.
        /// </summary>
        private int localCheckSumIndex = 0;

        /// <summary>
        /// The serialized data.
        /// </summary>
        private byte[] serializedData = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryData"/> class.
        /// </summary>
        /// <param name="serializableData">The serializable data interface object.</param>
        /// <param name="id">The identifier.</param>
        public BinaryData(ISerializableData serializableData, string id)
        {
            this.isWriteable = false;
            this.serializedData = Load(serializableData, id);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryData" /> class.
        /// </summary>
        /// <param name="initialSize">The initial size.</param>
        /// <param name="blockSize">The size.</param>
        public BinaryData(int initialSize, int blockSize)
        {
            this.blockSize = blockSize;
            this.isWriteable = true;

            if (initialSize > 0)
            {
                this.serializedData = new byte[initialSize];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryData" /> class.
        /// </summary>
        /// <param name="initialSize">The initial size.</param>
        public BinaryData(int initialSize)
        {
            this.isWriteable = true;

            if (initialSize > 0)
            {
                this.serializedData = new byte[initialSize];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryData" /> class.
        /// </summary>
        public BinaryData()
        {
            this.isWriteable = true;
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public IList<byte> Data => Array.AsReadOnly((this.serializedData == null) ? new byte[0] : this.serializedData);

        /// <summary>
        /// Gets the count of bytes left.
        /// </summary>
        /// <value>
        /// The bytes left.
        /// </value>
        public int Left => (this.serializedData == null) ? 0 : this.serializedData.Length - this.index;

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length => (this.serializedData == null) ? 0 : this.isWriteable ? this.index : this.serializedData.Length;

        /// <summary>
        /// Gets a value indicating whether this <see cref="BinaryData"/> is readable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if readable; otherwise, <c>false</c>.
        /// </value>
        public bool Readable => !this.isWriteable;

        /// <summary>
        /// Gets a value indicating whether this <see cref="BinaryData"/> is read-only.
        /// </summary>
        /// <value>
        ///   <c>true</c> if read-only; otherwise, <c>false</c>.
        /// </value>
        public bool ReadOnly => !this.isWriteable;

        /// <summary>
        /// Gets a value indicating whether this <see cref="BinaryData"/> is writable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if writable; otherwise, <c>false</c>.
        /// </value>
        public bool Writable => this.isWriteable;

        /// <summary>
        /// Gets a value indicating whether this <see cref="BinaryData"/> is write-only.
        /// </summary>
        /// <value>
        ///   <c>true</c> if write only; otherwise, <c>false</c>.
        /// </value>
        public bool WriteOnly => this.isWriteable;

        /// <summary>
        /// Gets the <see cref="System.Byte"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="System.Byte"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The byte.</returns>
        public byte this[int index]
        {
            get
            {
                if (this.serializedData == null ||
                    (this.isWriteable && index >= this.index) ||
                    (!this.isWriteable && index >= this.serializedData.Length))
                {
                    throw new IndexOutOfRangeException("Index is out of range for the serialized data");
                }

                return this.serializedData[index];
            }
        }

        /// <summary>
        /// Adds the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Add(byte data)
        {
            this.AssureSize(this.index + 1);

            this.serializedData[this.index] = data;
            this.index++;
        }

        /// <summary>
        /// Adds the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Add(Dispatcher.DispatcherTypes data)
        {
            this.Add((byte)data);
        }

        /// <summary>
        /// Adds the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Add(ServiceDispatcherSettings.ModCompatibilityMode data)
        {
            this.Add((byte)data);
        }

        /// <summary>
        /// Adds the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Add(SettingsType data)
        {
            this.Add((byte)data);
        }

        /// <summary>
        /// Adds the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Add(ServiceType data)
        {
            this.Add((byte)data);
        }

        /// <summary>
        /// Adds the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Add(ServiceDispatcherSettings.SpareVehiclesCreation data)
        {
            this.Add((byte)data);
        }

        /// <summary>
        /// Adds the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Add(BinaryData data)
        {
            this.Add(data.serializedData);
        }

        /// <summary>
        /// Adds the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Add(ServiceDispatcherSettings.BuildingCheckOrder data)
        {
            this.Add((byte)data);
        }

        /// <summary>
        /// Adds the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Add(ServiceDispatcherSettings.Allowance data)
        {
            this.Add((byte)data);
        }

        /// <summary>
        /// Adds the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Add(ServiceDispatcherSettings.BuildingCheckParameters[] data)
        {
            this.Add(data.SelectToArray(p => (byte)p));
        }

        /// <summary>
        /// Adds the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Add(Boolean data)
        {
            this.Add(data ? (byte)1 : (byte)0);
        }

        /// <summary>
        /// Adds the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Add(Vehicle.Flags data)
        {
            this.Add((ulong)data);
        }

        /// <summary>
        /// Adds the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Add(byte[] data)
        {
            this.AssureSize(this.index + data.Length);

            Buffer.BlockCopy(data, 0, this.serializedData, this.index, data.Length);
            this.index += data.Length;
        }

        /// <summary>
        /// Adds the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Add(UInt16[] data)
        {
            this.AssureSize(this.index + data.Length * 2);

            Buffer.BlockCopy(data, 0, this.serializedData, this.index, data.Length * 2);
            this.index += data.Length * 2;
        }

        /// <summary>
        /// Adds the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        public void Add(UInt16 source)
        {
            this.AssureSize(this.index + 2);

            Buffer.BlockCopy(BitConverter.GetBytes(source), 0, this.serializedData, this.index, 2);
            this.index += 2;
        }

        /// <summary>
        /// Adds the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        public void Add(UInt32 source)
        {
            this.AssureSize(this.index + 4);

            Buffer.BlockCopy(BitConverter.GetBytes(source), 0, this.serializedData, this.index, 4);
            this.index += 4;
        }

        /// <summary>
        /// Adds the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        public void Add(Vector3 source)
        {
            this.Add(source.x);
            this.Add(source.y);
            this.Add(source.z);
        }

        /// <summary>
        /// Adds the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        public void Add(UInt64 source)
        {
            this.AssureSize(this.index + 8);

            Buffer.BlockCopy(BitConverter.GetBytes(source), 0, this.serializedData, this.index, 8);
            this.index += 8;
        }

        /// <summary>
        /// Adds the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        public void Add(Double source)
        {
            this.AssureSize(this.index + 8);

            Buffer.BlockCopy(BitConverter.GetBytes(source), 0, this.serializedData, this.index, 8);
            this.index += 8;
        }

        /// <summary>
        /// Adds the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        public void Add(Single source)
        {
            this.AssureSize(this.index + 4);

            Buffer.BlockCopy(BitConverter.GetBytes(source), 0, this.serializedData, this.index, 4);
            this.index += 4;
        }

        /// <summary>
        /// Adds a local checksum to the data.
        /// </summary>
        public void AddLocalCheckSum()
        {
            this.AssureSize(this.index + 2);

            FletcherChecksum.ControlBytes control = FletcherChecksum.GetControlBytes(this.serializedData, this.localCheckSumIndex, this.index);

            this.serializedData[this.index] = control.First;
            this.serializedData[this.index + 1] = control.Second;
            this.index += 2;

            this.localCheckSumIndex = this.index;
        }

        /// <summary>
        /// Checks a local checksum using error-correction check-bytes.
        /// </summary>
        /// <returns>True if the checksum is valid.</returns>
        public void CheckLocalCheckSum()
        {
            this.AssureLeft(2);
            this.index += 2;

            if (!FletcherChecksum.Validate(this.serializedData, this.localCheckSumIndex, this.index))
            {
                throw new InvalidOperationException("Serialized data corruption!");
            }
        }

        /// <summary>
        /// Gets the next ServiceDispatcherSettings.BuildingCheckOrder.
        /// </summary>
        /// <returns>The ServiceDispatcherSettings.BuildingCheckOrder.</returns>
        public ServiceDispatcherSettings.Allowance GetAllowance()
        {
            return (ServiceDispatcherSettings.Allowance)this.GetByte();
        }

        /// <summary>
        /// Gets the next bool.
        /// </summary>
        /// <returns>The bool.</returns>
        public Boolean GetBool()
        {
            byte b = this.GetByte();

            switch (b)
            {
                case 0:
                    return false;

                case 1:
                    return true;

                default:
                    throw new InvalidOperationException("Error in serialized data");
            }
        }

        /// <summary>
        /// Gets the next ServiceDispatcherSettings.BuildingCheckOrder.
        /// </summary>
        /// <returns>The ServiceDispatcherSettings.BuildingCheckOrder.</returns>
        public ServiceDispatcherSettings.BuildingCheckOrder GetBuildingCheckOrder()
        {
            return (ServiceDispatcherSettings.BuildingCheckOrder)this.GetByte();
        }

        /// <summary>
        /// Gets the next array of ServiceDispatcherSettings.BuildingCheckParameters.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns>The data.</returns>
        public ServiceDispatcherSettings.BuildingCheckParameters[] GetBuildingCheckParametersArray(int length)
        {
            return this.GetByteArray(length).SelectToArray(b => (ServiceDispatcherSettings.BuildingCheckParameters)b);
        }

        /// <summary>
        /// Gets the next byte.
        /// </summary>
        /// <returns>The byte.</returns>
        public byte GetByte()
        {
            this.AssureLeft(1);

            int i = this.index;
            this.index++;

            return this.serializedData[i];
        }

        /// <summary>
        /// Gets the next byte array.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns>The data.</returns>
        public byte[] GetByteArray(int length)
        {
            this.AssureLeft(length);

            int i = this.index;
            this.index += length;

            byte[] data = new byte[length];

            if (length > 0)
            {
                Buffer.BlockCopy(this.serializedData, i, data, 0, length);
            }

            return data;
        }

        /// <summary>
        /// Gets the next Dispatcher.DispatcherTypes.
        /// </summary>
        /// <returns>The Dispatcher.DispatcherTypes.</returns>
        public Dispatcher.DispatcherTypes GetDispatcherType()
        {
            return (Dispatcher.DispatcherTypes)this.GetByte();
        }

        /// <summary>
        /// Gets the next double.
        /// </summary>
        /// <returns>The double.</returns>
        public Double GetDouble()
        {
            this.AssureLeft(8);

            int i = this.index;
            this.index += 8;
            return BitConverter.ToDouble(this.serializedData, i);
        }

        /// <summary>
        /// Gets the next float.
        /// </summary>
        /// <returns>The float.</returns>
        public Single GetFloat()
        {
            this.AssureLeft(4);

            int i = this.index;
            this.index += 4;
            return BitConverter.ToSingle(this.serializedData, i);
        }

        /// <summary>
        /// Gets the next ServiceDispatcherSettings.BuildingCheckOrder.
        /// </summary>
        /// <returns>The ServiceDispatcherSettings.BuildingCheckOrder.</returns>
        public ServiceDispatcherSettings.ModCompatibilityMode GetModCompatibilityMode()
        {
            return (ServiceDispatcherSettings.ModCompatibilityMode)this.GetByte();
        }

        /// <summary>
        /// Gets the next BinarySettings.ServiceType.
        /// </summary>
        /// <returns>The BinarySettings.ServiceType.</returns>
        public ServiceType GetServiceType()
        {
            return (ServiceType)this.GetByte();
        }

        /// <summary>
        /// Gets the next BinarySettings.SettingsType.
        /// </summary>
        /// <returns>The BinarySettings.SettingsType.</returns>
        public SettingsType GetSettingsType()
        {
            return (SettingsType)this.GetByte();
        }

        /// <summary>
        /// Gets the next ServiceDispatcherSettings.SpareVehiclesCreation.
        /// </summary>
        /// <returns>The ServiceDispatcherSettings.SpareVehiclesCreation.</returns>
        public ServiceDispatcherSettings.SpareVehiclesCreation GetSpareVehiclesCreation()
        {
            return (ServiceDispatcherSettings.SpareVehiclesCreation)this.GetByte();
        }

        /// <summary>
        /// Gets the next uint.
        /// </summary>
        /// <returns>the uint.</returns>
        public UInt32 GetUint()
        {
            this.AssureLeft(4);

            int i = this.index;
            this.index += 4;
            return BitConverter.ToUInt32(this.serializedData, i);
        }

        /// <summary>
        /// Gets the next ulong.
        /// </summary>
        /// <returns>The ulong.</returns>
        public UInt64 GetUlong()
        {
            this.AssureLeft(8);

            int i = this.index;
            this.index += 8;
            return BitConverter.ToUInt64(this.serializedData, i);
        }

        /// <summary>
        /// Gets the next ushort.
        /// </summary>
        /// <returns>The ushort.</returns>
        public UInt16 GetUshort()
        {
            this.AssureLeft(2);

            int i = this.index;
            this.index += 2;
            return BitConverter.ToUInt16(this.serializedData, i);
        }

        /// <summary>
        /// Gets the next ushort array.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns>The data.</returns>
        public UInt16[] GetUshortArray(int length)
        {
            int l = length * 2;
            this.AssureLeft(l);

            int i = this.index;
            this.index += l;

            ushort[] data = new ushort[length];

            if (l > 0)
            {
                Buffer.BlockCopy(this.serializedData, i, data, 0, l);
            }

            return data;
        }

        /// <summary>
        /// Gets the next ushort array, to the end of data.
        /// </summary>
        /// <returns>The data.</returns>
        public UInt16[] GetUshortArray()
        {
            int l = this.Left / 2;

            if (l == 0 && this.Left > 0)
            {
                this.AssureLeft(0);
                this.index = this.Length;
                return new ushort[0];
            }

            return this.GetUshortArray(l);
        }

        /// <summary>
        /// Gets the next Vector3.
        /// </summary>
        /// <returns>The Vector3.</returns>
        public Vector3 GetVector3()
        {
            Vector3 vector = new Vector3();
            vector.x = this.GetFloat();
            vector.y = this.GetFloat();
            vector.z = this.GetFloat();

            return vector;
        }

        /// <summary>
        /// Gets the next Vehicle.Flags.
        /// </summary>
        /// <returns>The Vehicle.Flags.</returns>
        public Vehicle.Flags GetVehicleFlags()
        {
            return (Vehicle.Flags)this.GetUlong();
        }

        /// <summary>
        /// Gets the next byte without moving forwards.
        /// </summary>
        /// <value>
        /// The byte.
        /// </value>
        public byte PeekByte()
        {
            this.AssureLeft(1);

            return this.serializedData[this.index];
        }

        /// <summary>
        /// Gets the next byte without moving forwards.
        /// </summary>
        /// <value>
        /// The byte.
        /// </value>
        public SerializableSettings.SettingsType PeekSettingsType()
        {
            return (SerializableSettings.SettingsType)this.PeekByte();
        }

        /// <summary>
        /// Resets the local checksum index to the current index.
        /// </summary>
        public void ResetLocalCheckSum()
        {
            this.localCheckSumIndex = this.index;
        }

        /// <summary>
        /// Saves the serialized data.
        /// </summary>
        /// <param name="serializableData">The serializable data inteface object.</param>
        /// <param name="id">The identifier.</param>
        public void Save(ISerializableData serializableData, string id)
        {
            if (!this.isWriteable)
            {
                throw new InvalidOperationException("Container is readonly");
            }

            if (this.serializedData != null)
            {
                FletcherChecksum.ControlBytes control = FletcherChecksum.GetControlBytes(this.serializedData, 0, this.index);

                byte[] data = new byte[this.index + 2];
                if (this.index > 0)
                {
                    Buffer.BlockCopy(this.serializedData, 0, data, 0, this.index);
                }

                data[this.index] = control.First;
                data[this.index + 1] = control.Second;

                Log.Debug(this, "Save", id, data.Length);
                serializableData.SaveData(id, data);
            }
        }

        /// <summary>
        /// Loads the data for the specified identifier using the specified serializable data inteface object.
        /// </summary>
        /// <param name="serializableData">The serializable data.</param>
        /// <param name="id">The identifier.</param>
        private static byte[] Load(ISerializableData serializableData, string id)
        {
            byte[] data = serializableData.LoadData(id);
            Log.Debug(typeof(BinaryData), "Load", id, data.Length);

            if (data.Length > 0)
            {
                if (data.Length < 2)
                {
                    throw new InvalidOperationException("Serialized data corruption");
                }

                if (!FletcherChecksum.Validate(data, 0, data.Length))
                {
                    throw new InvalidOperationException("Serialized data corruption");
                }

                Array.Resize(ref data, data.Length - 2);
            }

            return data;
        }

        /// <summary>
        /// Assures the bytes left for reading.
        /// </summary>
        /// <param name="size">The size.</param>
        private void AssureLeft(int size)
        {
            if (this.isWriteable)
            {
                throw new InvalidOperationException("Container is write-only");
            }

            if (this.Left < size)
            {
                throw new IndexOutOfRangeException("Not enough bytes left");
            }
        }

        /// <summary>
        /// Assures the size of the array for writing.
        /// </summary>
        /// <param name="size">The size.</param>
        private void AssureSize(int size)
        {
            if (!this.isWriteable)
            {
                throw new InvalidOperationException("Container is read-only");
            }

            if (this.serializedData != null && this.serializedData.Length >= size)
            {
                return;
            }

            int newSize = (size / this.blockSize) * this.blockSize;
            if (newSize < size)
            {
                newSize += blockSize;
            }

            if (Log.LogALot)
            {
                Log.DevDebug(this, "AssureSize", "SerializedData", (this.serializedData == null) ? 0 : this.serializedData.Length, size, newSize);
            }

            Array.Resize(ref this.serializedData, newSize);
        }
    }
}