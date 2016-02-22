using System;
using System.Collections;
using ColossalFramework;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Bulldozer helper class.
    /// </summary>
    internal static class BulldozeHelper
    {
        /// <summary>
        /// The BulldozeTool.DeleteBuilding method.
        /// </summary>
        private static BulldozeToolDeleteBuilding deleteBuildingMethod;

        /// <summary>
        /// Gets a value indicating whether this instance can bulldoze.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can bulldoze; otherwise, <c>false</c>.
        /// </value>
        public static bool CanBulldoze
        {
            get
            {
                return deleteBuildingMethod == null || deleteBuildingMethod.CanCall;
            }
        }

        /// <summary>
        /// Bulldozes the building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        public static void BulldozeBuilding(ushort buildingId)
        {
            try
            {
                Singleton<SimulationManager>.instance.AddAction(deleteBuildingMethod.Call(buildingId));
            }
            catch (Exception ex)
            {
                Log.Error(typeof(BulldozeHelper), "BulldozeBuilding", ex, buildingId);
            }
        }

        /// <summary>
        /// De-initializes this instance.
        /// </summary>
        public static void DeInitialize()
        {
            deleteBuildingMethod = null;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public static void Initialize()
        {
            deleteBuildingMethod = new BulldozeToolDeleteBuilding();
        }

        /// <summary>
        /// BulldozeTool.DeleteBuilding caller.
        /// </summary>
        private class BulldozeToolDeleteBuilding : SingleMethod
        {
            /// <summary>
            /// The bulldoze tool.
            /// </summary>
            private BulldozeTool bulldozeTool;

            /// <summary>
            /// Initializes a new instance of the <see cref="BulldozeToolDeleteBuilding"/> class.
            /// </summary>
            public BulldozeToolDeleteBuilding()
            {
                this.bulldozeTool = GameObject.FindObjectOfType<BulldozeTool>();
            }

            /// <summary>
            /// Gets the name of the method.
            /// </summary>
            /// <value>
            /// The name of the method.
            /// </value>
            protected override string MethodName
            {
                get
                {
                    return "DeleteBuilding";
                }
            }

            /// <summary>
            /// Gets the name of the signature method.
            /// </summary>
            /// <value>
            /// The name of the signature method.
            /// </value>
            protected override string SignatureMethodName
            {
                get
                {
                    return "DeleteBuilding_Signature";
                }
            }

            /// <summary>
            /// Gets the source class.
            /// </summary>
            /// <value>
            /// The source class.
            /// </value>
            protected override Type SourceClass
            {
                get
                {
                    return (this.bulldozeTool == null) ? typeof(BulldozeTool) : this.bulldozeTool.GetType();
                }
            }

            /// <summary>
            /// Calls the method.
            /// </summary>
            /// <param name="buildingId">The building identifier.</param>
            /// <returns>Data suitable for simulation manager add action.</returns>
            public IEnumerator Call(ushort buildingId)
            {
                return (IEnumerator)this.MethodInfo.Invoke(this.bulldozeTool, new object[] { buildingId });
            }

            /// <summary>
            /// The BulldozeTool.DeleteBuilding method signature.
            /// </summary>
            /// <param name="bulldozeTool">The bulldoze tool.</param>
            /// <param name="building">The building.</param>
            /// <returns>No value.</returns>
            private static IEnumerator DeleteBuilding_Signature(BulldozeTool bulldozeTool, ushort building)
            {
                throw new NotImplementedException("Call to method signature");
            }
        }
    }
}