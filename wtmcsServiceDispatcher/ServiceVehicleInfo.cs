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
        public int CapacityFree
        {
            get;
            private set;
        }

        /// <summary>
        /// The used capacity in percent.
        /// </summary>
        public float CapacityUsed
        {
            get;
            private set;
        }

        /// <summary>
        /// The vehicle is free.
        /// </summary>
        public bool FreeToCollect
        {
            get;
            private set;
        }

        /// <summary>
        /// The last seen stamp.
        /// </summary>
        public uint LastSeen
        {
            get;
            private set;
        }

        /// <summary>
        /// The position.
        /// </summary>
        public Vector3 Position
        {
            get;
            private set;
        }

        /// <summary>
        /// The target.
        /// </summary>
        public ushort Target
        {
            get;
            private set;
        }

        /// <summary>
        /// The vehicle identifier.
        /// </summary>
        public ushort VehicleId
        {
            get;
            private set;
        }

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

        private uint lastTargetStamp = 0;

        public void SetTarget(ushort targetBuildingId, ref Vehicle vehicle)
        {
            if (targetBuildingId != vehicle.m_targetBuilding)
            {
                vehicle.Info.m_vehicleAI.SetTarget(this.VehicleId, ref vehicle, targetBuildingId);
            }

            this.SetTarget(targetBuildingId);
        }

        public void SetTarget(ushort targetBuildingId)
        {
            if (targetBuildingId != 0)
            {
                lastTargetStamp = Global.CurrentFrame;
                this.FreeToCollect = false;
            }

            this.Target = targetBuildingId;
        }

        public bool DeAssign(ref Vehicle vehicle)
        {
            if (Global.CurrentFrame - this.lastTargetStamp > Global.TargetLingerDelay)
            {
                this.SetTarget(0, ref vehicle);
                return true;
            }

            return this.Target == 0;
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
            this.FreeToCollect = freeToCollect;

            this.SetTarget(vehicle.m_targetBuilding);

            string localeKey;
            int bufCur, bufMax;
            vehicle.Info.m_vehicleAI.GetBufferStatus(this.VehicleId, ref vehicle, out localeKey, out bufCur, out bufMax);
            this.CapacityFree = bufMax - bufCur;
            this.CapacityUsed = (float)bufCur / (float)bufMax;
        }
    }
}