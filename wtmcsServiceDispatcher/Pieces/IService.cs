using System.Collections.Generic;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Service data base class.
    /// </summary>
    /// <seealso cref="WhatThe.Mods.CitiesSkylines.ServiceDispatcher.IHandlerPart" />
    internal interface IBuildingService : IService
    {
        /// <summary>
        /// Categorizes the building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="building">The building.</param>
        void CategorizeBuilding(ushort buildingId, ref Building building);

        /// <summary>
        /// Gets the building categories for a building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <returns>The categories in which the building has been categorized.</returns>
        IEnumerable<string> GetCategories(ushort buildingId);

        /// <summary>
        /// Update all buildings.
        /// </summary>
        /// <summary>
        /// Remove categorized buidling.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        void UnCategorizeBuilding(ushort buildingId);

        /// <summary>
        /// Update all buildings.
        /// </summary>
        void UpdateAllBuildings();
    }

    /// <summary>
    /// Service data base class.
    /// </summary>
    /// <seealso cref="WhatThe.Mods.CitiesSkylines.ServiceDispatcher.IHandlerPart" />
    internal interface IService : IHandlerPart
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="DispatchService"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        bool Enabled { get; }

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
        /// Finish the categorization.
        /// </summary>
        void CategorizeFinish();

        /// <summary>
        /// Prepare for categorization.
        /// </summary>
        void CategorizePrepare();
    }
}