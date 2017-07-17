using System.Collections.Generic;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.Services
{
    internal abstract class HiddenVehicleService : IService
    {
        /// <summary>
        /// The settings.
        /// </summary>
        public readonly HiddenServiceSettings Settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="HiddenVehicleService"/> class.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="settings">The settings.</param>
        public HiddenVehicleService(ServiceHelper.ServiceType serviceType, HiddenServiceSettings settings)
        {
            this.Settings = settings;
            this.ServiceType = serviceType;
        }

        /// <summary>
        /// Gets a value indicating whether this dispatcher creates vehicles.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the dispatcher creates vehicles; otherwise, <c>false</c>.
        /// </value>
        public bool CreatesVehicles => false;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Services.IService" /> is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public abstract bool Enabled { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has buildings.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has buildings; otherwise, <c>false</c>.
        /// </value>
        public bool HasBuildings => false;

        /// <summary>
        /// Gets a value indicating whether this instance has vehicles.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has vehicles; otherwise, <c>false</c>.
        /// </value>
        public abstract bool HasVehicles { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is dispatching.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is dispatching; otherwise, <c>false</c>.
        /// </value>
        public bool IsDispatching => false;

        /// <summary>
        /// Gets a value indicating whether to remove unusable buildings.
        /// </summary>
        /// <value>
        ///   <c>true</c> if unusable buildings should be removed; otherwise, <c>false</c>.
        /// </value>
        public bool RemoveUnusableBuildings => false;

        /// <summary>
        /// Gets the service buildings.
        /// </summary>
        /// <value>
        /// The service buildings.
        /// </value>
        public IEnumerable<ServiceBuildingInfo> ServiceBuildings
        {
            get
            {
                yield break;
            }
        }

        /// <summary>
        /// Gets the service category.
        /// </summary>
        /// <value>
        /// The service category.
        /// </value>
        public abstract string ServiceCategory { get; }

        /// <summary>
        /// Gets the service log pre/suffix.
        /// </summary>
        /// <value>
        /// The service log pre/suffix.
        /// </value>
        public abstract string ServiceLogFix { get; }

        /// <summary>
        /// The dispatcher type.
        /// </summary>
        public ServiceHelper.ServiceType ServiceType { get; private set; }

        /// <summary>
        /// Gets the target category.
        /// </summary>
        /// <value>
        /// The target category.
        /// </value>
        public abstract string TargetCategory { get; }

        /// <summary>
        /// Gets the target log pre/suffix.
        /// </summary>
        /// <value>
        /// The target log pre/suffix.
        /// </value>
        public abstract string TargetLogFix { get; }

        /// <summary>
        /// Gets the transfer reason.
        /// </summary>
        /// <value>
        /// The transfer reason.
        /// </value>
        public TransferManager.TransferReason TransferReason => TransferManager.TransferReason.None;

        /// <summary>
        /// Gets or sets the type of the transfer.
        /// </summary>
        /// <value>
        /// The type of the transfer.
        /// </value>
        public byte TransferType => (byte)this.TransferReason;

        /// <summary>
        /// Categorizes the building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        public void CheckBuilding(ushort buildingId, ref Building building)
        { }

        /// <summary>
        /// Finish the categorization.
        /// </summary>
        public void CheckBuildingsFinish()
        { }

        /// <summary>
        /// Prepare for categorization.
        /// </summary>
        public void CheckBuildingsPrepare()
        { }

        /// <summary>
        /// Checks the vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        public abstract void CheckVehicle(ushort vehicleId, ref Vehicle vehicle);

        /// <summary>
        /// Checks the vehicle target.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        public void CheckVehicleTarget(ushort vehicleId, ref Vehicle vehicle)
        { }

        /// <summary>
        /// Logs a list of service building info for debug use.
        /// </summary>
        public void DebugListLogBuildings()
        { }

        /// <summary>
        /// Logs a list of vehicle info for debug use.
        /// </summary>
        public abstract void DebugListLogVehicles();

        /// <summary>
        /// Dispatches vehicles to targets.
        /// </summary>
        public void Dispatch()
        { }

        /// <summary>
        /// Gets the building categories for a building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <returns>
        /// The categories in which the building has been categorized.
        /// </returns>
        public IEnumerable<string> GetCategories(ushort buildingId)
        {
            yield break;
        }

        /// <summary>
        /// Re-initialize the part.
        /// </summary>
        public void ReInitialize()
        {
            this.Initialize(false);
        }

        /// <summary>
        /// Reinitializes the dispatcher.
        /// </summary>
        public void ReinitializeDispatcher()
        { }

        /// <summary>
        /// Remove categorized building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        public void RemoveBuilding(ushort buildingId)
        { }

        /// <summary>
        /// Remove categorized vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        public abstract void RemoveVehicle(ushort vehicleId);

        /// <summary>
        /// Gets the service building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <returns>
        /// True if building found.
        /// </returns>
        public bool TryGetServiceBuilding(ushort buildingId, out ServiceBuildingInfo building)
        {
            building = null;
            return false;
        }

        /// <summary>
        /// Gets the target building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <returns>
        /// True if building found.
        /// </returns>
        public bool TryGetTargetBuilding(ushort buildingId, out TargetBuildingInfo building)
        {
            building = null;
            return false;
        }

        /// <summary>
        /// Gets the service building.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>
        /// True if building found.
        /// </returns>
        public abstract bool TryGetVehicle(ushort vehicleId, out IVehicleInfo vehicle);

        /// <summary>
        /// Update all buildings.
        /// </summary>
        public void UpdateAllBuildings()
        { }

        /// <summary>
        /// Update all vehicles.
        /// </summary>
        public void UpdateAllVehicles()
        { }

        /// <summary>
        /// Finish the update.
        /// </summary>
        public void UpdateBuildingsFinish()
        { }

        /// <summary>
        /// Prepare for categorization.
        /// </summary>
        public void UpdateBuildingsPrepare()
        { }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="constructing">if set to <c>true</c> the instance is beeing constructed.</param>
        protected virtual void Initialize(bool constructing)
        { }
    }
}