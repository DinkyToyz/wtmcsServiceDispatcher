using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Generic building information interface.
    /// </summary>
    internal interface IBuildingInfo
    {
        /// <summary>
        /// Gets the CS building.
        /// </summary>
        /// <value>
        /// The CS building.
        /// </value>
        Building Building
        {
            get;
        }

        /// <summary>
        /// Gets the building identifier.
        /// </summary>
        ushort BuildingId
        {
            get;
        }

        /// <summary>
        /// Gets the CS building information.
        /// </summary>
        /// <value>
        /// The building CS information.
        /// </value>
        BuildingInfo BuildingInfo
        {
            get;
        }

        /// <summary>
        /// Gets the name of the building.
        /// </summary>
        /// <value>
        /// The name of the building.
        /// </value>
        string BuildingName
        {
            get;
        }

        /// <summary>
        /// Gets the district the building is in.
        /// </summary>
        byte District
        {
            get;
        }

        /// <summary>
        /// Gets the name of the district.
        /// </summary>
        /// <value>
        /// The name of the district.
        /// </value>
        string DistrictName
        {
            get;
        }

        /// <summary>
        /// Gets the position.
        /// </summary>
        Vector3 Position
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the building is updated.
        /// </summary>
        bool Updated
        {
            get;
        }

        /// <summary>
        /// Reinitializes this instance.
        /// </summary>
        void ReInitialize();

        /// <summary>
        /// Updates the building values.
        /// </summary>
        /// <param name="districtManager">The district manager.</param>
        /// <param name="building">The building.</param>
        /// <param name="ignoreInterval">If set to <c>true</c> ignore object update interval.</param>
        void UpdateValues(DistrictManager districtManager, ref Building building, bool ignoreInterval = true);
    }
}
