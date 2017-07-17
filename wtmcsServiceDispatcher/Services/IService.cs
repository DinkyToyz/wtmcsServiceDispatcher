using System.Collections.Generic;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.Services
{
    /// <summary>
    /// Service data base class.
    /// </summary>
    /// <seealso cref="WhatThe.Mods.CitiesSkylines.ServiceDispatcher.IHandlerPart" />
    internal interface IService : IHandlerPart
    {
        /// <summary>
        /// Gets a value indicating whether this dispatcher creates vehicles.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the dispatcher creates vehicles; otherwise, <c>false</c>.
        /// </value>
        bool CreatesVehicles { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Services.IService"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        bool Enabled { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has buildings.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has buildings; otherwise, <c>false</c>.
        /// </value>
        bool HasBuildings { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has vehicles.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has vehicles; otherwise, <c>false</c>.
        /// </value>
        bool HasVehicles { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is dispatching.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is dispatching; otherwise, <c>false</c>.
        /// </value>
        bool IsDispatching { get; }

        /// <summary>
        /// Gets a value indicating whether to remove unusable buildings.
        /// </summary>
        /// <value>
        ///   <c>true</c> if unusable buildings should be removed; otherwise, <c>false</c>.
        /// </value>
        bool RemoveUnusableBuildings { get; }

        /// <summary>
        /// Gets the service buildings.
        /// </summary>
        /// <value>
        /// The service buildings.
        /// </value>
        IEnumerable<ServiceBuildingInfo> ServiceBuildings { get; }

        /// <summary>
        /// Gets the service category.
        /// </summary>
        /// <value>
        /// The service category.
        /// </value>
        string ServiceCategory { get; }

        /// <summary>
        /// Gets the service log pre/suffix.
        /// </summary>
        /// <value>
        /// The service log pre/suffix.
        /// </value>
        string ServiceLogFix { get; }

        /// <summary>
        /// The dispatcher type.
        /// </summary>
        ServiceHelper.ServiceType ServiceType { get; }

        /// <summary>
        /// Gets the target category.
        /// </summary>
        /// <value>
        /// The target category.
        /// </value>
        string TargetCategory { get; }

        /// <summary>
        /// Gets the target log pre/suffix.
        /// </summary>
        /// <value>
        /// The target log pre/suffix.
        /// </value>
        string TargetLogFix { get; }

        /// <summary>
        /// Gets the transfer reason.
        /// </summary>
        /// <value>
        /// The transfer reason.
        /// </value>
        TransferManager.TransferReason TransferReason { get; }

        /// <summary>
        /// Gets or sets the type of the transfer.
        /// </summary>
        /// <value>
        /// The type of the transfer.
        /// </value>
        byte TransferType { get; }

        /// <summary>
        /// Categorizes the building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        void CheckBuilding(ushort buildingId, ref Building building);

        /// <summary>
        /// Finish the categorization.
        /// </summary>
        void CheckBuildingsFinish();

        /// <summary>
        /// Prepare for categorization.
        /// </summary>
        void CheckBuildingsPrepare();

        /// <summary>
        /// Checks the vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        void CheckVehicle(ushort vehicleId, ref Vehicle vehicle);

        /// <summary>
        /// Checks the vehicle target.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        void CheckVehicleTarget(ushort vehicleId, ref Vehicle vehicle);

        /// <summary>
        /// Logs a list of service building info for debug use.
        /// </summary>
        void DebugListLogBuildings();

        /// <summary>
        /// Logs a list of vehicle info for debug use.
        /// </summary>
        void DebugListLogVehicles();

        /// <summary>
        /// Dispatches vehicles to targets.
        /// </summary>
        void Dispatch();

        /// <summary>
        /// Gets the building categories for a building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <returns>The categories in which the building has been categorized.</returns>
        IEnumerable<string> GetCategories(ushort buildingId);

        /// <summary>
        /// Reinitializes the dispatcher.
        /// </summary>
        void ReinitializeDispatcher();

        /// <summary>
        /// Remove categorized building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        void RemoveBuilding(ushort buildingId);

        /// <summary>
        /// Remove categorized vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        void RemoveVehicle(ushort vehicleId);

        /// <summary>
        /// Gets the service building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <returns>True if building found.</returns>
        bool TryGetServiceBuilding(ushort buildingId, out ServiceBuildingInfo building);

        /// <summary>
        /// Gets the target building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        /// <returns>True if building found.</returns>
        bool TryGetTargetBuilding(ushort buildingId, out TargetBuildingInfo building);

        /// <summary>
        /// Gets the service building.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>
        /// True if building found.
        /// </returns>
        bool TryGetVehicle(ushort vehicleId, out IVehicleInfo vehicle);

        /// <summary>
        /// Update all buildings.
        /// </summary>
        void UpdateAllBuildings();

        /// <summary>
        /// Update all vehicles.
        /// </summary>
        void UpdateAllVehicles();

        /// <summary>
        /// Finish the update.
        /// </summary>
        void UpdateBuildingsFinish();

        /// <summary>
        /// Prepare for categorization.
        /// </summary>
        void UpdateBuildingsPrepare();
    }
}