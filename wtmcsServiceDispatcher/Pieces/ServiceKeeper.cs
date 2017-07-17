using ColossalFramework;
using System;
using System.Collections.Generic;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Keeps track of dispatch services.
    /// </summary>
    internal class DispatchServiceKeeper : IHandlerPart
    {
        /// <summary>
        /// The services.
        /// </summary>
        private DispatchService[] services = null;

        /// <summary>
        /// The dispatcher types.
        /// </summary>
        public enum ServiceTypes
        {
            /// <summary>
            /// Dispatches hearses.
            /// </summary>
            HearseDispatcher = 0,

            /// <summary>
            /// Dispatches garbage trucks.
            /// </summary>
            GarbageTruckDispatcher = 1,

            /// <summary>
            /// Dispatches ambulances.
            /// </summary>
            AmbulanceDispatcher = 2,

            /// <summary>
            /// Dispatches wrecking crews.
            /// </summary>
            WreckingCrewDispatcher = 3,

            /// <summary>
            /// Dispatches recovery crews.
            /// </summary>
            RecoveryCrewDispatcher = 4,

            /// <summary>
            /// Not a dispatcher.
            /// </summary>
            None = 3
        }

        /// <summary>
        /// Gets the dispatching services.
        /// </summary>
        /// <value>
        /// The dispatching services.
        /// </value>
        public IEnumerable<DispatchService> DispatchingServices => this.services.Where(s => s != null && s.IsDispatching);

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <value>
        /// The services.
        /// </value>
        public IEnumerable<DispatchService> EnabledServices => this.services.Where(s => s != null && s.Enabled);

        /// <summary>
        /// Gets the <see cref="DispatchService"/> with the specified dispatcher type.
        /// </summary>
        /// <value>
        /// The <see cref="DispatchService"/>.
        /// </value>
        /// <param name="DispatcherType">Type of the dispatcher.</param>
        /// <returns>The <see cref="DispatchService"/>.</returns>
        public DispatchService this[Dispatcher.DispatcherTypes DispatcherType] => this.services[(int)DispatcherType];

        /// <summary>
        /// Categorizes the building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        public void CategorizeBuilding(ushort buildingId, ref Building building)
        {
            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null && this.services[i].Enabled)
                {
                    this.services[i].CategorizeBuilding(buildingId, ref building);
                }
            }
        }

        /// <summary>
        /// Finish the categorization.
        /// </summary>
        public void CategorizeFinish()
        {
            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null && this.services[i].Enabled)
                {
                    this.services[i].CategorizeFinish();
                }
            }
        }

        /// <summary>
        /// Prepare for categorization.
        /// </summary>
        public void CategorizePrepare()
        {
            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null)
                {
                    this.services[i].CategorizePrepare();
                }
            }
        }

        /// <summary>
        /// Checks the vehicle target.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        public void CheckVehicleTarget(ushort vehicleId, ref Vehicle vehicle)
        {
            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null && this.services[i].IsDispatching && vehicle.m_transferType == this.services[i].TransferType)
                {
                    this.services[i].CheckVehicleTarget(vehicleId, ref vehicle);
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
                for (int i = 0; i < this.services.Length; i++)
                {
                    if (this.services[i] != null)
                    {
                        this.services[i].DebugListLogBuildings();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "DebugListLogBuildings", ex);
            }
        }

        /// <summary>
        /// Dispatches vehicles to targets.
        /// </summary>
        public void Dispatch()
        {
            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null && this.services[i].IsDispatching)
                {
                    this.services[i].Dispatch();
                }
            }
        }

        /// <summary>
        /// Gets the building categories for a building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <returns>The categories in which the building has been categorized.</returns>
        public IEnumerable<string> GetCategories(ushort buildingId)
        {
            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null)
                {
                    foreach (string category in this.services[i].GetCategories(buildingId))
                    {
                        yield return category;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the service building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <returns>A service building with the id, or null.</returns>
        public ServiceBuildingInfo GetServiceBuilding(ushort buildingId)
        {
            ServiceBuildingInfo building;

            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null & this.services[i].TryGetServiceBuilding(buildingId, out building))
                {
                    return building;
                }
            }

            return null;
        }

        /// <summary>
        /// Initializes the data lists.
        /// </summary>
        /// <param name="constructing">If set to <c>true</c> object is being constructed.</param>
        public void Initialize(bool constructing)
        {
            Log.Debug(this, "Initialize", constructing);

            if (constructing || this.services == null)
            {
                this.services = new DispatchService[]
                   {
                        new DispatchService.DeathCare(),
                        new DispatchService.Garbage(),
                        (Global.EnableDevExperiments ? new DispatchService.HealthCare() : null)
                   };
            }
            else
            {
                foreach (DispatchService service in this.services)
                {
                    if (service != null)
                    {
                        service.ReInitialize();
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the specified disapatcher service is dispatching.
        /// </summary>
        /// <param name="dispatcherType">Type of the dispatcher.</param>
        /// <returns>
        ///   <c>true</c> if the specified dispatcher service is dispatching; otherwise, <c>false</c>.
        /// </returns>
        public bool IsDispatching(Dispatcher.DispatcherTypes dispatcherType)
        {
            return this.services[(int)dispatcherType] != null && this.services[(int)dispatcherType].IsDispatching;
        }

        /// <summary>
        /// Re-initialize the part.
        /// </summary>
        public void ReInitialize()
        {
            this.Initialize(false);
        }

        /// <summary>
        /// Remove categorized buidling.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        public void UnCategorizeBuilding(ushort buildingId)
        {
            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null)
                {
                    this.UnCategorizeBuilding(buildingId);
                }
            }
        }

        /// <summary>
        /// Update all buildings.
        /// </summary>
        public void UpdateAllBuildings()
        {
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            DistrictManager districtManager = null;
            if (Global.Settings.DispatchAnyByDistrict)
            {
                districtManager = Singleton<DistrictManager>.instance;
            }

            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null && this.services[i].Enabled)
                {
                    this.services[i].UpdateAllBuildings();
                }
            }
        }
    }
}