using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Keeps track of interesting buildings.
    /// </summary>
    internal class BuildingKeeper : IHandlerPart
    {
        /// <summary>
        /// The death care building lists.
        /// </summary>
        public StandardServiceBuildings DeathCare = new StandardServiceBuildings(Dispatcher.DispatcherTypes.HearseDispatcher);

        /// <summary>
        /// The garbage building lists.
        /// </summary>
        public StandardServiceBuildings Garbage = new StandardServiceBuildings(Dispatcher.DispatcherTypes.GarbageTruckDispatcher);

        /// <summary>
        /// The health care building lists.
        /// </summary>
        public StandardServiceBuildings HealthCare = new StandardServiceBuildings(Dispatcher.DispatcherTypes.AmbulanceDispatcher);

        /// <summary>
        /// The current/last update bucket.
        /// </summary>
        private uint bucket;

        /// <summary>
        /// The building object bucket manager.
        /// </summary>
        private Bucketeer bucketeer = null;

        /// <summary>
        /// The bucket factor.
        /// </summary>
        private uint bucketFactor = 192;

        /// <summary>
        /// The bucket mask.
        /// </summary>
        private uint bucketMask = 255;

        /// <summary>
        /// Gets the serialized target assignments.
        /// </summary>
        /// <value>
        /// The serialized target assignments.
        /// </value>
        private Dictionary<ushort, ushort> serializedTargetAssignments = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingKeeper"/> class.
        /// </summary>
        public BuildingKeeper()
        {
            this.Initialize(true);
            Log.Debug(this, "Constructed");
        }

        /// <summary>
        /// Gets the desolate buildings.
        /// </summary>
        public Dictionary<ushort, Double> DesolateBuildings
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether emptying is considerd as being automatic for new buildings.
        /// </summary>
        /// <value>
        ///   <c>true</c> if emptying is considered automatic; otherwise, <c>false</c>.
        /// </value>
        public bool EmptyingIsAutoEmptying { get; private set; }

        /// <summary>
        /// The serialized automatic emptying building list.
        /// </summary>
        public HashSet<ushort> SerializedAutoEmptying { get; private set; }

        /// <summary>
        /// Gets the service building lists.
        /// </summary>
        /// <value>
        /// The service building lists.
        /// </value>
        public IEnumerable<Dictionary<ushort, ServiceBuildingInfo>> ServiceBuildingLists
        {
            get
            {
                if (this.Garbage.ServiceBuildings != null)
                {
                    yield return this.Garbage.ServiceBuildings;
                }

                if (this.DeathCare.ServiceBuildings != null)
                {
                    yield return this.DeathCare.ServiceBuildings;
                }

                if (this.HealthCare.ServiceBuildings != null)
                {
                    yield return this.HealthCare.ServiceBuildings;
                }
            }
        }

        /// <summary>
        /// Gets the building list pairs.
        /// </summary>
        /// <value>
        /// The building list pairs.
        /// </value>
        public IEnumerable<StandardServiceBuildings> StandardServices
        {
            get
            {
                if (this.Garbage.ServiceBuildings != null && this.Garbage.TargetBuildings != null)
                {
                    yield return this.Garbage;
                }

                if (this.DeathCare.ServiceBuildings != null && this.DeathCare.TargetBuildings != null)
                {
                    yield return this.DeathCare;
                }

                if (this.HealthCare.ServiceBuildings != null && this.HealthCare.TargetBuildings != null)
                {
                    yield return this.HealthCare;
                }
            }
        }

        /// <summary>
        /// Gets the target building lists.
        /// </summary>
        /// <value>
        /// The target building lists.
        /// </value>
        public IEnumerable<Dictionary<ushort, TargetBuildingInfo>> TargetBuildingLists
        {
            get
            {
                if (this.Garbage.TargetBuildings != null)
                {
                    yield return this.Garbage.TargetBuildings;
                }

                if (this.DeathCare.TargetBuildings != null)
                {
                    yield return this.DeathCare.TargetBuildings;
                }

                if (this.HealthCare.TargetBuildings != null)
                {
                    yield return this.HealthCare.TargetBuildings;
                }
            }
        }

        /// <summary>
        /// Logs a list of service building info for debug use.
        /// </summary>
        public void DebugListLogBuildings()
        {
            try
            {
                if (this.DeathCare.TargetBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.DeathCare.TargetBuildings.Values);
                }

                if (this.DeathCare.ServiceBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.DeathCare.ServiceBuildings.Values);
                }

                if (this.Garbage.TargetBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.Garbage.TargetBuildings.Values);
                }

                if (this.Garbage.ServiceBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.Garbage.ServiceBuildings.Values);
                }

                if (this.HealthCare.TargetBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.HealthCare.TargetBuildings.Values);
                }

                if (this.HealthCare.ServiceBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.HealthCare.ServiceBuildings.Values);
                }

                if (this.DesolateBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.DesolateBuildings, "DesolateBuildings");
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "DebugListLogBuildings", ex);
            }
        }

        /// <summary>
        /// Logs a list of desolate building info for debug use.
        /// </summary>
        public void DebugListLogDesolateBuildings()
        {
            try
            {
                if (Global.Settings.WreckingCrews.DispatchVehicles && this.DesolateBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.DesolateBuildings, "DesolateBuildings");
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "DebugListLogDesolateBuildings", ex);
            }
        }

        /// <summary>
        /// Logs a list of service building info for debug use.
        /// </summary>
        public void DebugListLogServiceBuildings()
        {
            try
            {
                if (this.DeathCare.ServiceBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.DeathCare.ServiceBuildings.Values);
                }

                if (this.Garbage.ServiceBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.Garbage.ServiceBuildings.Values);
                }

                if (this.HealthCare.ServiceBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.HealthCare.ServiceBuildings.Values);
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "DebugListLogServiceBuildings", ex);
            }
        }

        /// <summary>
        /// Logs a list of service building info for debug use.
        /// </summary>
        public void DebugListLogTargetBuildings()
        {
            try
            {
                if (this.DeathCare.TargetBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.DeathCare.TargetBuildings.Values);
                }

                if (this.Garbage.TargetBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.Garbage.TargetBuildings.Values);
                }

                if (this.HealthCare.TargetBuildings != null)
                {
                    BuildingHelper.DebugListLog(this.HealthCare.TargetBuildings.Values);
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "DebugListLogTargetBuildings", ex);
            }
        }

        /// <summary>
        /// Deserializes the automatic emptying building list.
        /// </summary>
        /// <param name="serializedData">The serialized data.</param>
        public void DeserializeAutoEmptying(SerializableSettings.BinaryData serializedData)
        {
            this.EmptyingIsAutoEmptying = serializedData == null;

            if (serializedData == null || serializedData.Left == 0)
            {
                this.SerializedAutoEmptying = null;
                return;
            }

            try
            {
                byte version = serializedData.GetByte();
                if (version > 0)
                {
                    Log.Warning(this, "DeserializeAutoEmptying", "Serialized data version too high!", version, 0);
                    this.SerializedAutoEmptying = null;
                    return;
                }

                if (serializedData.Left == 0)
                {
                    this.SerializedAutoEmptying = null;
                    return;
                }

                ushort[] data = serializedData.GetUshortArray();

                if (Log.LogALot)
                {
                    Log.Debug(this, "DeserializeAutoEmptying", serializedData.Length, data.Length, String.Join(" ", data.OrderBy(us => us).SelectToArray(us => us.ToString())));
                }

                this.SerializedAutoEmptying = new HashSet<ushort>(data);

                if (Log.LogALot)
                {
                    Log.DevDebug(this, "DeserializeAutoEmptying", this.SerializedAutoEmptying.Count, String.Join(", ", this.SerializedAutoEmptying.OrderBy(id => id).SelectToArray(id => id.ToString())));
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "DeserializeAutoEmptying", ex);
                this.SerializedAutoEmptying = null;
            }
        }

        /// <summary>
        /// Deserializes the desolate buildings.
        /// </summary>
        /// <param name="serializedData">The serialized data.</param>
        public void DeserializeDesolateBuildings(SerializableSettings.BinaryData serializedData)
        {
            if (serializedData == null || serializedData.Left == 0)
            {
                return;
            }

            try
            {
                byte version = serializedData.GetByte();
                if (version > 0)
                {
                    Log.Warning(this, "DeserializeDesolateBuildings", "Serialized data version too high!", version, 0);
                    return;
                }

                if (serializedData.Left == 0 || this.DesolateBuildings == null)
                {
                    return;
                }

                while (serializedData.Left >= 10)
                {
                    ushort id = serializedData.GetUshort();
                    double stamp = serializedData.GetDouble();

                    this.DesolateBuildings[id] = stamp;
                }

                if (Log.LogALot)
                {
                    Log.DevDebug(this, "DeserializeDesolateBuildings", this.DesolateBuildings.Count, String.Join(", ", this.DesolateBuildings.OrderBy(db => db.Key).SelectToArray(db => db.Key.ToString() + ":" + db.Value.ToString())));
                }

                this.DesolateBuildings.Clear();
            }
            catch (Exception ex)
            {
                Log.Error(this, "DeserializeDesolateBuildings", ex);

                if (this.DesolateBuildings != null)
                {
                    this.DesolateBuildings.Clear();
                }
            }
        }

        /// <summary>
        /// Deserializes the target assignment list.
        /// </summary>
        /// <param name="serializedData">The serialized data.</param>
        public void DeserializeTargetAssignments(SerializableSettings.BinaryData serializedData)
        {
            if (serializedData == null || serializedData.Left == 0)
            {
                this.serializedTargetAssignments = null;
                return;
            }

            try
            {
                byte version = serializedData.GetByte();
                if (version > 0)
                {
                    Log.Warning(this, "DeserializeTargetAssignments", "Serialized data version too high!", version, 0);
                    this.serializedTargetAssignments = null;
                    return;
                }

                if (serializedData.Left == 0)
                {
                    this.serializedTargetAssignments = null;
                    return;
                }

                ushort[] data = serializedData.GetUshortArray();

                if (Log.LogALot)
                {
                    Log.Debug(this, "DeserializeTargetAssignments", serializedData.Length, data.Length, String.Join(" ", data.SelectToArray(us => us.ToString())));
                }

                this.serializedTargetAssignments = new Dictionary<ushort, ushort>(data.Length / 2);

                for (int i = 0; i < data.Length - 1; i += 2)
                {
                    this.serializedTargetAssignments[data[i]] = data[i + 1];
                }

                if (Log.LogALot)
                {
                    Log.DevDebug(this, "DeserializeTargetAssignments", this.serializedTargetAssignments.Count, String.Join(", ", this.serializedTargetAssignments.OrderBy(ta => ta.Key).SelectToArray(ta => ta.Key.ToString() + ":" + ta.Value.ToString())));
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "DeserializeTargetAssignments", ex);
                this.serializedTargetAssignments = null;
            }
        }

        /// <summary>
        /// Gets the buidling categories for a building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <returns>The categories in which the buidling has been categorized.</returns>
        public IEnumerable<string> GetCategories(ushort buildingId)
        {
            if (this.Garbage.ServiceBuildings != null && this.Garbage.ServiceBuildings.ContainsKey(buildingId))
            {
                yield return "Garbage";
            }

            if (this.DeathCare.ServiceBuildings != null && this.DeathCare.ServiceBuildings.ContainsKey(buildingId))
            {
                yield return "DeathCare";
            }

            if (this.HealthCare.ServiceBuildings != null && this.HealthCare.ServiceBuildings.ContainsKey(buildingId))
            {
                yield return "HealthCare";
            }

            if (this.Garbage.TargetBuildings != null && this.Garbage.TargetBuildings.ContainsKey(buildingId))
            {
                yield return "Dirty";
            }

            if (this.DeathCare.TargetBuildings != null && this.DeathCare.TargetBuildings.ContainsKey(buildingId))
            {
                yield return "DeadPeople";
            }

            if (this.HealthCare.TargetBuildings != null && this.HealthCare.TargetBuildings.ContainsKey(buildingId))
            {
                yield return "SickPeople";
            }

            if (this.DesolateBuildings != null && this.DesolateBuildings.ContainsKey(buildingId))
            {
                yield return "Desolate";
            }
        }

        /// <summary>
        /// Gets the service building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <returns>A list of service buildings with the id.</returns>
        public ServiceBuildingInfo GetServiceBuilding(ushort buildingId)
        {
            ServiceBuildingInfo building;

            if (this.Garbage.ServiceBuildings != null && this.Garbage.ServiceBuildings.TryGetValue(buildingId, out building))
            {
                return building;
            }

            if (this.DeathCare.ServiceBuildings != null && this.DeathCare.ServiceBuildings.TryGetValue(buildingId, out building))
            {
                return building;
            }

            if (this.HealthCare.ServiceBuildings != null && this.HealthCare.ServiceBuildings.TryGetValue(buildingId, out building))
            {
                return building;
            }

            return null;
        }

        /// <summary>
        /// Re-initialize the part.
        /// </summary>
        public void ReInitialize()
        {
            this.Initialize(false);
        }

        /// <summary>
        /// Serializes the automatic emptying bulding list.
        /// </summary>
        /// <returns>Serialized data.</returns>
        public SerializableSettings.BinaryData SerializeAutoEmptying()
        {
            if (this.StandardServices == null)
            {
                return null;
            }

            if (Log.LogALot)
            {
                Log.DevDebug(this, "SerializeAutoEmptying", String.Join(", ", this.StandardServices.Where(ss => ss != null && ss.ServiceBuildings != null).SelectMany(ss => ss.ServiceBuildings.Values).Where(sb => sb.IsAutoEmptying).OrderBy(sb => sb.BuildingId).SelectToArray(sb => sb.BuildingId.ToString())));
            }

            ushort[] sourceData = this.StandardServices
                    .Where(s => s != null && s.ServiceBuildings != null)
                    .SelectMany(s => s.ServiceBuildings.Values)
                    .Where(b => b.IsAutoEmptying)
                    .SelectToArray(b => b.BuildingId);

            SerializableSettings.BinaryData serializedData = new SerializableSettings.BinaryData(sourceData.Length * 2 + 1);

            // Version.
            serializedData.Add((byte)0);

            // Data.
            serializedData.Add(sourceData);

            if (Log.LogALot)
            {
                Log.DevDebug(this, "SerializeAutoEmptying", serializedData.Length);
            }

            return serializedData;
        }

        public SerializableSettings.BinaryData SerializeDesolateBuildings()
        {
            if (this.DesolateBuildings == null)
            {
                return null;
            }

            if (Log.LogALot)
            {
                Log.DevDebug(this, "SerializeDesolateBuildings", this.DesolateBuildings.Count, String.Join(", ", this.DesolateBuildings.OrderBy(db => db.Key).SelectToArray(db => db.Key.ToString() + ":" + db.Value.ToString("#0.##"))));
            }

            SerializableSettings.BinaryData serializedData = new SerializableSettings.BinaryData(this.DesolateBuildings.Count * 10 + 1);

            // Version.
            serializedData.Add((byte)0);

            // Data.
            foreach (KeyValuePair<ushort, double> desolateBuilding in this.DesolateBuildings)
            {
                serializedData.Add(desolateBuilding.Key);
                serializedData.Add(desolateBuilding.Value);
            }

            if (Log.LogALot)
            {
                Log.DevDebug(this, "SerializeDesolateBuildings", serializedData.Length);
            }

            return serializedData;
        }

        /// <summary>
        /// Serializes the target assignments.
        /// </summary>
        /// <returns>Serialized data.</returns>
        public SerializableSettings.BinaryData SerializeTargetAssignments()
        {
            if (this.StandardServices == null)
            {
                return null;
            }

            if (Log.LogALot)
            {
                Log.DevDebug(this, "SerializeTargetAssignments", String.Join(", ", this.StandardServices.Where(ss => ss != null && ss.ServiceBuildings != null).SelectMany(ss => ss.ServiceBuildings.Values).SelectMany(b => b.Vehicles.Values).Where(v => v.Target != 0).OrderBy(v => v.VehicleId).SelectToArray(v => v.VehicleId.ToString() + ":" + v.Target.ToString())));
            }

            ushort[] sourceData = this.StandardServices
                    .Where(s => s != null && s.ServiceBuildings != null)
                    .SelectMany(s => s.ServiceBuildings.Values)
                    .SelectMany(b => b.Vehicles.Values)
                    .Where(v => v.Target != 0)
                    .SelectMany(v => new ushort[] { v.VehicleId, v.Target })
                    .ToArray();

            SerializableSettings.BinaryData serializedData = new SerializableSettings.BinaryData(sourceData.Length * 2 + 1);

            // Version.
            serializedData.Add((byte)0);

            // Data.
            serializedData.Add(sourceData);

            if (Log.LogALot)
            {
                Log.DevDebug(this, "SerializeTargetAssignments", serializedData.Length);
            }

            return serializedData;
        }

        /// <summary>
        /// Updates data.
        /// </summary>
        public void Update()
        {
            // Get and categorize buildings.
            this.DeathCare.HasBuildingsToCheck = false;
            this.Garbage.HasBuildingsToCheck = false;
            this.HealthCare.HasBuildingsToCheck = false;

            // First update?
            if (this.bucketeer != null)
            {
                // Data is initialized. Just check buildings for this frame.
                uint endBucket = this.bucketeer.GetEnd();

                uint counter = 0;
                while (this.bucket != endBucket)
                {
                    if (counter > 256)
                    {
                        throw new Exception("Update bucket loop counter too high");
                    }
                    counter++;

                    this.bucket = this.bucketeer.GetNext(this.bucket + 1);
                    Bucketeer.Boundaries bounds = this.bucketeer.GetBoundaries(this.bucket);

                    this.CategorizeBuildings(bounds.FirstId, bounds.LastId);
                }
            }
            else
            {
                if (Global.BucketedUpdates)
                {
                    uint length = (uint)Singleton<BuildingManager>.instance.m_buildings.m_buffer.Length;
                    this.bucketFactor = length / (this.bucketMask + 1);
                    //Log.DevDebug(this, "Update", "bucketFactor", length, this.bucketMask, this.bucketFactor);

                    this.bucketeer = new Bucketeer(this.bucketMask, this.bucketFactor);
                    this.bucket = this.bucketeer.GetEnd();
                }

                if (Log.LogALot)
                {
                    Log.DevDebug(this, "Update", "SerializedAutoEmptying", this.EmptyingIsAutoEmptying, (this.SerializedAutoEmptying == null) ? 0 : this.SerializedAutoEmptying.Count);
                }

                this.CategorizeBuildings();
                this.SerializedAutoEmptying = null;
                this.EmptyingIsAutoEmptying = false;

                if (this.serializedTargetAssignments != null)
                {
                    this.CollectVehicles();
                    this.serializedTargetAssignments = null;
                }
            }

            if (Global.BuildingUpdateNeeded)
            {
                if (this.DeathCare.ServiceBuildings != null)
                {
                    this.UpdateValues(this.DeathCare.ServiceBuildings.Values);
                }

                if (this.DeathCare.TargetBuildings != null)
                {
                    this.UpdateValues(this.DeathCare.TargetBuildings.Values);
                }

                if (this.Garbage.ServiceBuildings != null)
                {
                    this.UpdateValues(this.Garbage.ServiceBuildings.Values);
                }

                if (this.Garbage.TargetBuildings != null)
                {
                    this.UpdateValues(this.Garbage.TargetBuildings.Values);
                }

                if (this.HealthCare.ServiceBuildings != null)
                {
                    this.UpdateValues(this.HealthCare.ServiceBuildings.Values);
                }

                if (this.HealthCare.TargetBuildings != null)
                {
                    this.UpdateValues(this.HealthCare.TargetBuildings.Values);
                }

                Global.BuildingUpdateNeeded = false;
            }
        }

        /// <summary>
        /// Categorizes the buildings.
        /// </summary>
        private void CategorizeBuildings()
        {
            this.CategorizeBuildings(0, Singleton<BuildingManager>.instance.m_buildings.m_buffer.Length);
        }

        /// <summary>
        /// Categorizes the buildings.
        /// </summary>
        /// <param name="firstBuildingId">The first building identifier.</param>
        /// <param name="lastBuildingId">The last building identifier.</param>
        private void CategorizeBuildings(ushort firstBuildingId, int lastBuildingId)
        {
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            bool desolateBuilding;
            bool usableBuilding;

            if (this.DeathCare.BuildingsInNeedOfEmptyingChange != null)
            {
                this.DeathCare.BuildingsInNeedOfEmptyingChange.Clear();
            }

            if (this.Garbage.BuildingsInNeedOfEmptyingChange != null)
            {
                this.Garbage.BuildingsInNeedOfEmptyingChange.Clear();
            }

            for (ushort id = firstBuildingId; id < lastBuildingId; id++)
            {
                if (buildings[id].Info == null)
                {
                    usableBuilding = false;
                    desolateBuilding = false;
                }
                else
                {
                    desolateBuilding = (buildings[id].m_flags & (Building.Flags.Abandoned | Building.Flags.BurnedDown)) != Building.Flags.None;
                    usableBuilding = (buildings[id].m_flags & Building.Flags.Created) == Building.Flags.Created && (buildings[id].m_flags & (Building.Flags.Abandoned | Building.Flags.BurnedDown | Building.Flags.Deleted | Building.Flags.Hidden)) == Building.Flags.None;
                }

                // Handle buildings needing services.
                if (usableBuilding)
                {
                    if (Global.Settings.DeathCare.DispatchVehicles || Global.Settings.DeathCare.AutoEmpty)
                    {
                        this.CategorizeDeathCareBuilding(id, ref buildings[id]);
                    }

                    if (Global.Settings.Garbage.DispatchVehicles || Global.Settings.Garbage.AutoEmpty)
                    {
                        this.CategorizeGarbageBuilding(id, ref buildings[id]);
                    }

                    if (Global.Settings.HealthCare.DispatchVehicles)
                    {
                        this.CategorizeHealthCareBuilding(id, ref buildings[id]);
                    }
                }
                else
                {
                    if (this.DeathCare.ServiceBuildings != null && this.DeathCare.ServiceBuildings.ContainsKey(id))
                    {
                        this.DeathCare.ServiceBuildings.Remove(id);
                        Log.Debug(this, "CategorizeBuildings", "Not Cemetary Building", id);
                    }

                    if (this.DeathCare.TargetBuildings != null && this.DeathCare.TargetBuildings.ContainsKey(id))
                    {
                        this.DeathCare.TargetBuildings.Remove(id);
                        Log.Debug(this, "CategorizeBuildings", "Not Dead People Building", id);
                    }

                    if (this.Garbage.ServiceBuildings != null && this.Garbage.ServiceBuildings.ContainsKey(id))
                    {
                        this.Garbage.ServiceBuildings.Remove(id);
                        Log.Debug(this, "CategorizeBuildings", "Not Landfill Building", id);
                    }

                    if (this.Garbage.TargetBuildings != null && this.Garbage.TargetBuildings.ContainsKey(id))
                    {
                        this.Garbage.TargetBuildings.Remove(id);
                        Log.Debug(this, "CategorizeBuildings", "Not Dirty Building", id);
                    }

                    if (this.HealthCare.ServiceBuildings != null && this.HealthCare.ServiceBuildings.ContainsKey(id))
                    {
                        this.HealthCare.ServiceBuildings.Remove(id);
                        Log.Debug(this, "CategorizeBuildings", "Not Medical Building", id);
                    }

                    if (this.HealthCare.TargetBuildings != null && this.HealthCare.TargetBuildings.ContainsKey(id))
                    {
                        this.HealthCare.TargetBuildings.Remove(id);
                        Log.Debug(this, "CategorizeBuildings", "Not Sick People Building", id);
                    }
                }

                // Handle abandonded and burnet down buildings.
                if (this.DesolateBuildings != null)
                {
                    double stamp;
                    bool known = this.DesolateBuildings.TryGetValue(id, out stamp);

                    if (desolateBuilding)
                    {
                        double delta;

                        if (known)
                        {
                            delta = Global.SimulationTime - stamp;
                        }
                        else
                        {
                            delta = 0.0;
                            this.DesolateBuildings[id] = Global.SimulationTime;
                            Log.Debug(this, "CategorizeBuildings", "Desolate Building", id);
                        }

                        if (delta >= Global.Settings.WreckingCrews.DelaySeconds)
                        {
                            Log.Debug(this, "CategorizeBuildings", "Bulldoze Building", id);
                            BulldozeHelper.BulldozeBuilding(id);
                        }
                    }
                    else if (known)
                    {
                        this.DesolateBuildings.Remove(id);
                        Log.Debug(this, "CategorizeBuildings", "Not Desolate Building", id);
                    }
                }
            }

            if (Global.Settings.DeathCare.AutoEmpty && this.DeathCare.BuildingsInNeedOfEmptyingChange.Count > 0)
            {
                this.ChangeAutoEmptying(this.DeathCare);
            }

            if (Global.Settings.Garbage.AutoEmpty && this.Garbage.BuildingsInNeedOfEmptyingChange.Count > 0)
            {
                this.ChangeAutoEmptying(this.Garbage);
            }
        }

        /// <summary>
        /// Categorizes the building for death-care.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        private void CategorizeDeathCareBuilding(ushort buildingId, ref Building building)
        {
            // Check cemetaries and crematoriums.
            if (building.Info.m_buildingAI is CemeteryAI)
            {
                ServiceBuildingInfo hearseBuilding;

                if (!this.DeathCare.ServiceBuildings.TryGetValue(buildingId, out hearseBuilding))
                {
                    hearseBuilding = new ServiceBuildingInfo(buildingId, ref building, Dispatcher.DispatcherTypes.HearseDispatcher);
                    Log.Debug(this, "CategorizeBuilding", "Cemetary", buildingId, building.Info.name, hearseBuilding.BuildingName, hearseBuilding.Range, hearseBuilding.District);

                    this.DeathCare.ServiceBuildings[buildingId] = hearseBuilding;
                }
                else
                {
                    hearseBuilding.Update(ref building);
                }

                if (Global.Settings.DeathCare.AutoEmpty && (hearseBuilding.NeedsEmptying || hearseBuilding.EmptyingIsDone))
                {
                    this.DeathCare.BuildingsInNeedOfEmptyingChange.Add(hearseBuilding);
                }
            }
            else if (this.DeathCare.ServiceBuildings.ContainsKey(buildingId))
            {
                Log.Debug(this, "CategorizeBuilding", "Not Cemetary", buildingId);

                this.DeathCare.ServiceBuildings.Remove(buildingId);
            }

            // Check dead people.
            if (Global.Settings.DeathCare.DispatchVehicles)
            {
                if (building.m_deathProblemTimer > 0)
                {
                    if (!this.DeathCare.TargetBuildings.ContainsKey(buildingId))
                    {
                        TargetBuildingInfo deadPeopleBuilding = new TargetBuildingInfo(buildingId, ref building, Dispatcher.DispatcherTypes.HearseDispatcher, TargetBuildingInfo.ServiceDemand.NeedsService);
                        if (Log.LogToFile)
                        {
                            Log.Debug(this, "CategorizeBuilding", "Dead People", buildingId, building.Info.name, deadPeopleBuilding.BuildingName, building.m_deathProblemTimer, deadPeopleBuilding.ProblemValue, deadPeopleBuilding.HasProblem, deadPeopleBuilding.District);
                        }

                        this.DeathCare.TargetBuildings[buildingId] = deadPeopleBuilding;
                        this.DeathCare.HasBuildingsToCheck = true;
                    }
                    else
                    {
                        this.DeathCare.TargetBuildings[buildingId].Update(ref building, TargetBuildingInfo.ServiceDemand.NeedsService);
                        this.DeathCare.HasBuildingsToCheck = this.DeathCare.HasBuildingsToCheck || this.DeathCare.TargetBuildings[buildingId].CheckThis;
                    }
                }
                else if (this.DeathCare.TargetBuildings.ContainsKey(buildingId))
                {
                    if (this.DeathCare.TargetBuildings[buildingId].WantedService)
                    {
                        this.DeathCare.TargetBuildings[buildingId].Update(ref building, TargetBuildingInfo.ServiceDemand.None);
                    }
                    else
                    {
                        if (Log.LogToFile)
                        {
                            Log.Debug(this, "CategorizeBuilding", "No Dead People", buildingId);
                        }

                        this.DeathCare.TargetBuildings.Remove(buildingId);
                    }
                }
            }
        }

        /// <summary>
        /// Categorizes the building for garbage.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        private void CategorizeGarbageBuilding(ushort buildingId, ref Building building)
        {
            // Check landfills and incinerators.
            if (building.Info.m_buildingAI is LandfillSiteAI)
            {
                ServiceBuildingInfo garbageBuilding;

                if (!this.Garbage.ServiceBuildings.TryGetValue(buildingId, out garbageBuilding))
                {
                    garbageBuilding = new ServiceBuildingInfo(buildingId, ref building, Dispatcher.DispatcherTypes.GarbageTruckDispatcher);
                    Log.Debug(this, "CategorizeBuilding", "Landfill", buildingId, building.Info.name, garbageBuilding.BuildingName, garbageBuilding.Range, garbageBuilding.District);

                    this.Garbage.ServiceBuildings[buildingId] = garbageBuilding;
                }
                else
                {
                    garbageBuilding.Update(ref building);
                }

                if (Global.Settings.Garbage.AutoEmpty && (garbageBuilding.NeedsEmptying || garbageBuilding.EmptyingIsDone))
                {
                    this.Garbage.BuildingsInNeedOfEmptyingChange.Add(garbageBuilding);
                }
            }
            else if (this.Garbage.ServiceBuildings.ContainsKey(buildingId))
            {
                Log.Debug(this, "CategorizeBuilding", "Not Landfill", buildingId);

                this.Garbage.ServiceBuildings.Remove(buildingId);
            }

            // Check garbage.
            if (Global.Settings.Garbage.DispatchVehicles)
            {
                TargetBuildingInfo.ServiceDemand demand;
                if (building.m_garbageBuffer >= Global.Settings.Garbage.MinimumAmountForDispatch)
                {
                    demand = TargetBuildingInfo.ServiceDemand.NeedsService;
                }
                else if (building.m_garbageBuffer >= Global.Settings.Garbage.MinimumAmountForPatrol)
                {
                    demand = TargetBuildingInfo.ServiceDemand.WantsService;
                }
                else
                {
                    demand = TargetBuildingInfo.ServiceDemand.None;
                }

                if (demand != TargetBuildingInfo.ServiceDemand.None && !(building.Info.m_buildingAI is LandfillSiteAI))
                {
                    if (!this.Garbage.TargetBuildings.ContainsKey(buildingId))
                    {
                        TargetBuildingInfo dirtyBuilding = new TargetBuildingInfo(buildingId, ref building, Dispatcher.DispatcherTypes.GarbageTruckDispatcher, demand);
                        if (Log.LogToFile)
                        {
                            Log.Debug(this, "CategorizeBuilding", "Dirty", demand, buildingId, building.Info.name, dirtyBuilding.BuildingName, building.m_garbageBuffer, dirtyBuilding.ProblemValue, dirtyBuilding.HasProblem, dirtyBuilding.District);
                        }

                        this.Garbage.TargetBuildings[buildingId] = dirtyBuilding;
                        this.Garbage.HasBuildingsToCheck = true;
                    }
                    else
                    {
                        this.Garbage.TargetBuildings[buildingId].Update(ref building, demand);
                        this.Garbage.HasBuildingsToCheck = this.Garbage.HasBuildingsToCheck || this.Garbage.TargetBuildings[buildingId].CheckThis;
                    }
                }
                else if (this.Garbage.TargetBuildings.ContainsKey(buildingId))
                {
                    if ((building.m_garbageBuffer > 10 && (building.m_garbageBuffer >= Global.Settings.Garbage.MinimumAmountForDispatch / 10 || building.m_garbageBuffer >= Global.Settings.Garbage.MinimumAmountForPatrol / 2)) || this.Garbage.TargetBuildings[buildingId].WantedService)
                    {
                        this.Garbage.TargetBuildings[buildingId].Update(ref building, TargetBuildingInfo.ServiceDemand.None);
                    }
                    else
                    {
                        if (Log.LogToFile)
                        {
                            Log.Debug(this, "CategorizeBuilding", "Not Dirty", buildingId);
                        }

                        this.Garbage.TargetBuildings.Remove(buildingId);
                    }
                }
            }
        }

        /// <summary>
        /// Categorizes the building for health-care.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        private void CategorizeHealthCareBuilding(ushort buildingId, ref Building building)
        {
            // Check hospitals and clinics.
            if (building.Info.m_buildingAI is HospitalAI)
            {
                if (!this.HealthCare.ServiceBuildings.ContainsKey(buildingId))
                {
                    ServiceBuildingInfo ambulanceBuilding = new ServiceBuildingInfo(buildingId, ref building, Dispatcher.DispatcherTypes.AmbulanceDispatcher);
                    Log.Debug(this, "CategorizeBuilding", "Medical", buildingId, building.Info.name, ambulanceBuilding.BuildingName, ambulanceBuilding.Range, ambulanceBuilding.District);

                    this.HealthCare.ServiceBuildings[buildingId] = ambulanceBuilding;
                }
                else
                {
                    this.HealthCare.ServiceBuildings[buildingId].Update(ref building);
                }
            }
            else if (this.HealthCare.ServiceBuildings.ContainsKey(buildingId))
            {
                Log.Debug(this, "CategorizeBuilding", "Not Medical", buildingId);

                this.HealthCare.ServiceBuildings.Remove(buildingId);
            }

            // Check sick people.
            if (building.m_healthProblemTimer > 0)
            {
                if (!this.HealthCare.TargetBuildings.ContainsKey(buildingId))
                {
                    TargetBuildingInfo sickPeopleBuilding = new TargetBuildingInfo(buildingId, ref building, Dispatcher.DispatcherTypes.AmbulanceDispatcher, TargetBuildingInfo.ServiceDemand.NeedsService);
                    if (Log.LogToFile)
                    {
                        Log.Debug(this, "CategorizeBuilding", "Sick People", buildingId, building.Info.name, sickPeopleBuilding.BuildingName, building.m_healthProblemTimer, sickPeopleBuilding.ProblemValue, sickPeopleBuilding.HasProblem, sickPeopleBuilding.District);
                    }

                    this.HealthCare.TargetBuildings[buildingId] = sickPeopleBuilding;
                    this.HealthCare.HasBuildingsToCheck = true;
                }
                else
                {
                    this.HealthCare.TargetBuildings[buildingId].Update(ref building, TargetBuildingInfo.ServiceDemand.NeedsService);
                    this.HealthCare.HasBuildingsToCheck = this.HealthCare.HasBuildingsToCheck || this.HealthCare.TargetBuildings[buildingId].CheckThis;
                }
            }
            else if (this.HealthCare.TargetBuildings.ContainsKey(buildingId))
            {
                if (this.HealthCare.TargetBuildings[buildingId].WantedService)
                {
                    this.HealthCare.TargetBuildings[buildingId].Update(ref building, TargetBuildingInfo.ServiceDemand.None);
                }
                else
                {
                    if (Log.LogToFile)
                    {
                        Log.Debug(this, "CategorizeBuilding", "No Sick People", buildingId);
                    }

                    this.HealthCare.TargetBuildings.Remove(buildingId);
                }
            }
        }

        /// <summary>
        /// Changes the automatic emptying.
        /// </summary>
        /// <param name="service">The service buidling lists.</param>
        private void ChangeAutoEmptying(StandardServiceBuildings service)
        {
            uint isEmptying = 0;
            uint canEmptyOther = 0;

            foreach (ServiceBuildingInfo building in service.ServiceBuildings.Values)
            {
                if (building.IsAutoEmptying)
                {
                    isEmptying++;
                }

                if (building.CanEmptyOther)
                {
                    canEmptyOther++;
                }
            }

            foreach (ServiceBuildingInfo building in service.BuildingsInNeedOfEmptyingChange)
            {
                if (building.EmptyingIsDone)
                {
                    building.AutoEmptyStop();
                    isEmptying--;
                }
                else if (building.NeedsEmptying && canEmptyOther > isEmptying)
                {
                    building.AutoEmptyStart();
                    isEmptying++;
                }
            }
        }

        /// <summary>
        /// Changes the automatic emptying.
        /// </summary>
        /// <param name="allServiceBuildings">The service buildings.</param>
        /// <param name="serviceBuildingsInNeedOfEmptyingChange">The service buildings in need of emptying change.</param>
        private void ChangeAutoEmptying(Dictionary<ushort, ServiceBuildingInfo> allServiceBuildings, List<ServiceBuildingInfo> serviceBuildingsInNeedOfEmptyingChange)
        {
            uint isEmptying = 0;
            uint canEmptyOther = 0;

            foreach (ServiceBuildingInfo building in allServiceBuildings.Values)
            {
                if (building.IsAutoEmptying)
                {
                    isEmptying++;
                }

                if (building.CanEmptyOther)
                {
                    canEmptyOther++;
                }
            }

            foreach (ServiceBuildingInfo building in serviceBuildingsInNeedOfEmptyingChange)
            {
                if (building.EmptyingIsDone)
                {
                    building.AutoEmptyStop();
                    isEmptying--;
                }
                else if (building.NeedsEmptying && canEmptyOther > isEmptying)
                {
                    building.AutoEmptyStart();
                    isEmptying++;
                }
            }
        }

        /// <summary>
        /// Collects the vehicles from deserialization.
        /// </summary>
        private void CollectVehicles()
        {
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

            if (Log.LogALot)
            {
                Log.DevDebug(this, "CollectVehicles", "SerializedTargetAssignments", this.serializedTargetAssignments.Count);
            }

            int countServices = 0;
            int countDispathchingServices = 0;
            int countBuildings = 0;
            int countUsableBuildings = 0;
            int countVehicles = 0;
            int countUsableVehicles = 0;
            int countNewVehicles = 0;
            int countTargetingVehicles = 0;

            foreach (StandardServiceBuildings service in this.StandardServices)
            {
                countServices++;

                if (service.ServiceBuildings != null)
                {
                    countDispathchingServices++;

                    foreach (ServiceBuildingInfo serviceBuilding in service.ServiceBuildings.Values)
                    {
                        countBuildings++;

                        if (buildings[serviceBuilding.BuildingId].Info != null && (buildings[serviceBuilding.BuildingId].m_flags & Building.Flags.Created) != Building.Flags.None || (buildings[serviceBuilding.BuildingId].m_flags & (Building.Flags.Abandoned | Building.Flags.BurnedDown | Building.Flags.Deleted | Building.Flags.Hidden)) == Building.Flags.None)
                        {
                            countUsableBuildings++;

                            serviceBuilding.FirstOwnVehicleId = buildings[serviceBuilding.BuildingId].m_ownVehicles;

                            int count = 0;
                            ushort vehicleId = serviceBuilding.FirstOwnVehicleId;
                            while (vehicleId != 0)
                            {
                                if (count >= ushort.MaxValue)
                                {
                                    throw new Exception("Loop counter too high");
                                }
                                count++;
                                countVehicles++;

                                if (vehicles[vehicleId].m_transferType == service.TransferType && vehicles[vehicleId].m_targetBuilding != 0)
                                {
                                    // Add status for relevant vehicles.
                                    if (vehicles[vehicleId].Info != null &&
                                        (vehicles[vehicleId].m_flags & Vehicle.Flags.Created) == Vehicle.Flags.Created &&
                                        (vehicles[vehicleId].m_flags & VehicleHelper.VehicleExists) != ~VehicleHelper.VehicleAll)
                                    {
                                        countUsableVehicles++;

                                        if (!serviceBuilding.Vehicles.ContainsKey(vehicleId))
                                        {
                                            countNewVehicles++;

                                            ushort serializedTarget;
                                            if (this.serializedTargetAssignments.TryGetValue(vehicleId, out serializedTarget) && serializedTarget == vehicles[vehicleId].m_targetBuilding)
                                            {
                                                countTargetingVehicles++;
                                                serviceBuilding.Vehicles[vehicleId] = new ServiceVehicleInfo(vehicleId, ref vehicles[vehicleId], service.DispatcherType, serializedTarget);
                                            }
                                        }
                                    }
                                }

                                vehicleId = vehicles[vehicleId].m_nextOwnVehicle;
                                if (vehicleId == serviceBuilding.FirstOwnVehicleId)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (Log.LogALot)
            {
                Log.DevDebug(this, "CollectVehicles", "SerializedTargetAssignments", this.serializedTargetAssignments.Count, countServices, countDispathchingServices, countBuildings, countUsableBuildings, countVehicles, countUsableVehicles, countNewVehicles, countTargetingVehicles);
            }
        }

        /// <summary>
        /// Initializes the data lists.
        /// </summary>
        /// <param name="constructing">If set to <c>true</c> object is being constructed.</param>
        private void Initialize(bool constructing)
        {
            Log.InfoList info = new Log.InfoList();
            info.Add("constructing", constructing);

            if (constructing)
            {
                this.bucketeer = null;
                this.SerializedAutoEmptying = null;
                this.EmptyingIsAutoEmptying = true;
                this.serializedTargetAssignments = null;
            }

            this.DeathCare.Intialize(constructing, Global.Settings.DeathCare, info);
            this.Garbage.Intialize(constructing, Global.Settings.Garbage, info);

            if (!Global.Settings.Garbage.DispatchVehicles && !Global.Settings.Garbage.AutoEmpty)
            {
                this.Garbage.ServiceBuildings = null;
            }
            else if (constructing || this.Garbage.ServiceBuildings == null)
            {
                info.Add("GarbageBuildings", "new");
                this.Garbage.HasBuildingsToCheck = false;
                this.Garbage.ServiceBuildings = new Dictionary<ushort, ServiceBuildingInfo>();
            }

            info.Add("DispatchAmbulances", Global.Settings.HealthCare.DispatchVehicles);
            this.HealthCare.Intialize(constructing, Global.Settings.HealthCare, info);

            info.Add("AutoBulldozeBuildings", Global.Settings.WreckingCrews.DispatchVehicles);
            info.Add("CanBulldoze", BulldozeHelper.CanBulldoze);

            if (Global.Settings.WreckingCrews.DispatchVehicles && BulldozeHelper.CanBulldoze)
            {
                if (this.DesolateBuildings == null)
                {
                    info.Add("Desolate", "new");
                    this.DesolateBuildings = new Dictionary<ushort, double>();
                }
            }
            else
            {
                this.DesolateBuildings = null;
            }

            Log.Debug(this, "Initialize", info);
        }

        /// <summary>
        /// Updates the building values.
        /// </summary>
        /// <typeparam name="T">The building class type.</typeparam>
        /// <param name="infoBuildings">The buildings.</param>
        private void UpdateValues<T>(IEnumerable<T> infoBuildings) where T : IBuildingInfo
        {
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            foreach (T building in infoBuildings)
            {
                building.ReInitialize();
                building.UpdateValues(ref buildings[building.BuildingId], true);
            }
        }

        /// <summary>
        /// Standard service building lists.
        /// </summary>
        public class StandardServiceBuildings
        {
            /// <summary>
            /// The buildings in need of emptying change.
            /// </summary>
            public List<ServiceBuildingInfo> BuildingsInNeedOfEmptyingChange = null;

            /// <summary>
            /// Whether this service has buildings to check.
            /// </summary>
            public bool HasBuildingsToCheck = false;

            /// <summary>
            /// The service buildings.
            /// </summary>
            public Dictionary<ushort, ServiceBuildingInfo> ServiceBuildings = null;

            /// <summary>
            /// The target buildings.
            /// </summary>
            public Dictionary<ushort, TargetBuildingInfo> TargetBuildings = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="StandardServiceBuildings" /> class.
            /// </summary>
            /// <param name="dispatcherType">Type of the dispatcher.</param>
            public StandardServiceBuildings(Dispatcher.DispatcherTypes dispatcherType)
            {
                this.DispatcherType = dispatcherType;

                switch (dispatcherType)
                {
                    case Dispatcher.DispatcherTypes.HearseDispatcher:
                        this.Service = "DeathCare";
                        this.TransferType = (byte)TransferManager.TransferReason.Dead;
                        break;

                    case Dispatcher.DispatcherTypes.GarbageTruckDispatcher:
                        this.Service = "Garbage";
                        this.TransferType = (byte)TransferManager.TransferReason.Garbage;
                        break;

                    case Dispatcher.DispatcherTypes.AmbulanceDispatcher:
                        this.Service = "HelathCare";
                        this.TransferType = (byte)TransferManager.TransferReason.Sick;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("Unknown dispatcher type: " + dispatcherType.ToString());
                }
            }

            /// <summary>
            /// Gets the type of the dispatcher.
            /// </summary>
            /// <value>
            /// The type of the dispatcher.
            /// </value>
            public Dispatcher.DispatcherTypes DispatcherType { get; private set; }

            /// <summary>
            /// The material.
            /// </summary>
            public string Service { get; set; }

            /// <summary>
            /// Gets the type of the transfer.
            /// </summary>
            /// <value>
            /// The type of the transfer.
            /// </value>
            public byte TransferType { get; private set; }

            /// <summary>
            /// Intializes this instance.
            /// </summary>
            /// <param name="constructing">if set to <c>true</c> the isntance is beeing constructed.</param>
            /// <param name="settings">The settings.</param>
            /// <param name="info">The debug information list.</param>
            public void Intialize(bool constructing, StandardServiceSettings settings, Log.InfoList info)
            {
                info.Add(this.Service, "DispatchVehicles", Global.Settings.DeathCare.DispatchVehicles);
                info.Add(this.Service, "AutoEmpty", Global.Settings.DeathCare.AutoEmpty);

                if (!settings.DispatchVehicles)
                {
                    this.HasBuildingsToCheck = false;
                    this.TargetBuildings = null;
                }
                else if (constructing || this.TargetBuildings == null)
                {
                    info.Add(this.Service, "New", "TargetBuildings");
                    this.HasBuildingsToCheck = false;
                    this.TargetBuildings = new Dictionary<ushort, TargetBuildingInfo>();
                }

                if (!settings.AutoEmpty)
                {
                    this.BuildingsInNeedOfEmptyingChange = null;
                }
                else if (constructing || this.BuildingsInNeedOfEmptyingChange == null)
                {
                    info.Add(this.Service, "New", "BuildingsInNeedOfEmptying");
                    this.BuildingsInNeedOfEmptyingChange = new List<ServiceBuildingInfo>();
                }

                if (!settings.DispatchVehicles && !settings.AutoEmpty)
                {
                    this.ServiceBuildings = null;
                }
                else if (constructing || this.ServiceBuildings == null)
                {
                    info.Add(this.Service, "New", "ServiceCareBuildings");
                    this.ServiceBuildings = new Dictionary<ushort, ServiceBuildingInfo>();
                }
            }
        }
    }
}