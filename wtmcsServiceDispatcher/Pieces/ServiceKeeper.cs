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
        /// Gets a value indicating whether any dispatcher creates vehicles.
        /// </summary>
        /// <value>
        ///   <c>true</c> if any dispatcher creates vehicles; otherwise, <c>false</c>.
        /// </value>
        public bool AnyDispatcherCreatesVehicles
        {
            get
            {
                for (int i = 0; i < this.services.Length; i++)
                {
                    if (this.services[i] != null && this.services[i].CreatesVehicles)
                    {
                        return true;
                    }
                }

                return false;
            }
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
        /// Gets the transfer reasons for which a dispatcher creates vehicles.
        /// </summary>
        /// <value>
        /// The transfer reasons for which a dispatcher cerates vehicles.
        /// </value>
        public TransferManager.TransferReason[] TransferReasonsDispatcherCreatesVehiclesFor => this.services.WhereSelectToArray(s => s != null && s.CreatesVehicles, s => s.TransferReason);

        /// <summary>
        /// Gets the <see cref="DispatchService"/> with the specified dispatcher type.
        /// </summary>
        /// <value>
        /// The <see cref="DispatchService"/>.
        /// </value>
        /// <param name="serviceType">Type of the dispatcher.</param>
        /// <returns>The <see cref="DispatchService"/>.</returns>
        public DispatchService this[ServiceHelper.ServiceType serviceType] => this.services[(int)serviceType];

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
        /// Checks whether a dispatcher creates vehicles for a service.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns>True if vehicles are cerated by a dispatcher.</returns>
        public bool DispatcherCreatesVehicles(ServiceHelper.ServiceType serviceType)
        {
            return this.services[(int)serviceType] != null && this.services[(int)serviceType].CreatesVehicles;
        }

        /// <summary>
        /// Checks whether a dispatcher creates vehicles for a transfer reason.
        /// </summary>
        /// <param name="transferReason">The transfer reason.</param>
        /// <returns>
        /// True if vehicles are cerated by a dispatcher.
        /// </returns>
        public bool DispatcherCreatesVehicles(TransferManager.TransferReason transferReason)
        {
            return this.DispatcherCreatesVehicles(ServiceHelper.GetServiceType(transferReason));
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
        /// <param name="serviceType">Type of the dispatcher.</param>
        /// <returns>
        ///   <c>true</c> if the specified dispatcher service is dispatching; otherwise, <c>false</c>.
        /// </returns>
        public bool IsDispatching(ServiceHelper.ServiceType serviceType)
        {
            return this.services[(int)serviceType] != null && this.services[(int)serviceType].IsDispatching;
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
            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null && this.services[i].Enabled)
                {
                    this.services[i].UpdateAllBuildings();
                }
            }
        }

        /// <summary>
        /// Update all vehciles.
        /// </summary>
        public void UpdateAllVehicles()
        {
            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null && this.services[i].Enabled)
                {
                    this.services[i].UpdateAllVehicles();
                }
            }
        }

        /// <summary>
        /// Finish the update.
        /// </summary>
        public void UpdateFinish()
        {
            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null && this.services[i].Enabled)
                {
                    this.services[i].UpdateFinish();
                }
            }
        }

        /// <summary>
        /// Prepare for update.
        /// </summary>
        public void UpdatePrepare()
        {
            for (int i = 0; i < this.services.Length; i++)
            {
                if (this.services[i] != null)
                {
                    this.services[i].UpdatePrepare();
                }
            }
        }
    }
}