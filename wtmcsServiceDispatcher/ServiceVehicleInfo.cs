using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Info about a service vehicle.
    /// </summary>
    internal class ServiceVehicleInfo
    {
        /// <summary>
        /// The free capacity.
        /// </summary>
        public int CapacityFree;

        /// <summary>
        /// The used capacity in percent.
        /// </summary>
        public float CapacityUsed;

        /// <summary>
        /// The vehicle is free.
        /// </summary>
        public bool FreeToCollect;

        /// <summary>
        /// The last seen stamp.
        /// </summary>
        public uint LastSeen;

        /// <summary>
        /// The position.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The target.
        /// </summary>
        public ushort Target;

        /// <summary>
        /// The vehicle identifier.
        /// </summary>
        public ushort VehicleId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceVehicleInfo" /> class.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="freeToCollect">If set to <c>true</c> the vehicle is free.</param>
        public ServiceVehicleInfo(ushort vehicleId, ref Vehicle vehicle, bool freeToCollect)
        {
            this.VehicleId = vehicleId;

            this.Update(ref vehicle, freeToCollect);
        }

        /// <summary>
        /// Gets the name of the vehicle.
        /// </summary>
        /// <value>
        /// The name of the vehicle.
        /// </value>
        public string VehicleName
        {
            get
            {
                return VehicleHelper.GetVehicleName(this.VehicleId);
            }
        }

        /// <summary>
        /// Updates the specified vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="freeToCollect">If set to <c>true</c> the vehicle is free.</param>
        public void Update(ref Vehicle vehicle, bool freeToCollect)
        {
            this.LastSeen = Global.CurrentFrame;
            this.Position = vehicle.GetLastFramePosition();
            this.Target = vehicle.m_targetBuilding;
            this.FreeToCollect = freeToCollect;

            string localeKey;
            int bufCur, bufMax;
            vehicle.Info.m_vehicleAI.GetBufferStatus(this.VehicleId, ref vehicle, out localeKey, out bufCur, out bufMax);
            this.CapacityFree = bufMax - bufCur;
            this.CapacityUsed = (float)bufCur / (float)bufMax;
        }
    }
}
