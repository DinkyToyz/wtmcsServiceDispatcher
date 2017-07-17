using ColossalFramework;
using System;
using System.Collections.Generic;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Keeps track of interesting buildings.
    /// </summary>
    internal class BuildingKeeper : IHandlerPart
    {
        /// <summary>
        /// The current/last update bucket.
        /// </summary>
        private uint bucket;

        /// <summary>
        /// The building object bucket manager.
        /// </summary>
        private Bucketeer bucketeer;

        /// <summary>
        /// The bucket factor.
        /// </summary>
        private uint bucketFactor = 192;

        /// <summary>
        /// The bucket mask.
        /// </summary>
        private uint bucketMask = 255;

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
        /// Logs a list of service building info for debug use.
        /// </summary>
        public void DebugListLogBuildings()
        {
            try
            {
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
        /// Gets the buidling categories for a building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <returns>The categories in which the buidling has been categorized.</returns>
        public IEnumerable<string> GetCategories(ushort buildingId)
        {
            List<string> categories = new List<string>();

            if (Global.DispatchServices != null)
            {
                categories.AddRange(Global.DispatchServices.GetCategories(buildingId));
            }

            if (this.DesolateBuildings != null && this.DesolateBuildings.ContainsKey(buildingId))
            {
                categories.Add("Desolate");
            }

            return categories;
        }

        /// <summary>
        /// Re-initialize the part.
        /// </summary>
        public void ReInitialize()
        {
            this.Initialize(false);
        }

        /// <summary>
        /// Updates data.
        /// </summary>
        /// <exception cref="System.Exception">Update bucket loop counter too high.</exception>
        public void Update()
        {
            if (Global.DispatchServices != null)
            {
                Global.DispatchServices.CategorizePrepare();
            }

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

                this.CategorizeBuildings();
            }

            if (Global.BuildingUpdateNeeded && Global.DispatchServices != null)
            {
                Global.DispatchServices.UpdateAllBuildings();
                Global.BuildingUpdateNeeded = false;
            }

            if (Global.DispatchServices != null)
            {
                Global.DispatchServices.CategorizeFinish();
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
                if (Global.DispatchServices != null)
                {
                    if (usableBuilding)
                    {
                        Global.DispatchServices.CategorizeBuilding(id, ref buildings[id]);
                    }
                    else
                    {
                        Global.DispatchServices.UnCategorizeBuilding(id);
                    }
                }

                // Handle abandonded and burned down buildings.
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
        }

        /// <summary>
        /// Initializes the data lists.
        /// </summary>
        /// <param name="constructing">If set to <c>true</c> object is being constructed.</param>
        private void Initialize(bool constructing)
        {
            Log.InfoList info = new Log.InfoList();
            info.Add("constructing", constructing);

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

            DistrictManager districtManager = null;
            if (Global.Settings.DispatchAnyByDistrict)
            {
                districtManager = Singleton<DistrictManager>.instance;
            }

            foreach (T building in infoBuildings)
            {
                building.ReInitialize();
                building.UpdateValues(ref buildings[building.BuildingId], true);
            }
        }
    }
}