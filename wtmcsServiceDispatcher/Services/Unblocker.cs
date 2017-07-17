using ColossalFramework;
using System.Collections.Generic;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.Services
{
    internal class Unblocker : HiddenVehicleService
    {
        /// <summary>
        /// The vehicles that have been removed from grid.
        /// </summary>
        private HashSet<ushort> removedFromGrid = new HashSet<ushort>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Unblocker" /> class.
        /// </summary>
        public Unblocker() : base(ServiceHelper.ServiceType.Unblocker, null)
        { }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Services.IService" /> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public override bool Enabled => Global.Settings.DeathCare.RemoveFromGrid || Global.Settings.HealthCare.RemoveFromGrid;

        /// <summary>
        /// Gets a value indicating whether this instance has vehicles.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has vehicles; otherwise, <c>false</c>.
        /// </value>
        public override bool HasVehicles => this.removedFromGrid != null;

        /// <summary>
        /// Gets the service category.
        /// </summary>
        /// <value>
        /// The service category.
        /// </value>
        public override string ServiceCategory => "Unblocker";

        /// <summary>
        /// Gets the service log pre/suffix.
        /// </summary>
        /// <value>
        /// The service log pre/suffix.
        /// </value>
        public override string ServiceLogFix => "HU";

        /// <summary>
        /// Gets the target category.
        /// </summary>
        /// <value>
        /// The target category.
        /// </value>
        public override string TargetCategory => "BlockingServiceVehicles";

        /// <summary>
        /// Gets the target log pre/suffix.
        /// </summary>
        /// <value>
        /// The target log pre/suffix.
        /// </value>
        public override string TargetLogFix => "TVBSV";

        public override void CheckVehicle(ushort vehicleId, ref Vehicle vehicle)
        {
            // Handle grid removals.
            if (this.removedFromGrid != null)
            {
                if ((vehicle.m_flags & Vehicle.Flags.Stopped) == ~VehicleHelper.VehicleAll &&
                    (vehicle.Info.m_vehicleAI is HearseAI || vehicle.Info.m_vehicleAI is AmbulanceAI))
                {
                    if (this.removedFromGrid.Contains(vehicleId))
                    {
                        if (Log.LogALot)
                        {
                            Log.Debug(this, "CheckVehicle", "Rem", vehicleId, vehicle.m_targetBuilding, vehicle.Info.name, VehicleHelper.GetVehicleName(vehicleId), vehicle.m_flags);
                        }

                        this.removedFromGrid.Remove(vehicleId);
                    }
                }
                else if ((Global.Settings.DeathCare.RemoveFromGrid && vehicle.Info.m_vehicleAI is HearseAI) ||
                          (Global.Settings.HealthCare.RemoveFromGrid && vehicle.Info.m_vehicleAI is AmbulanceAI))
                {
                    if (!this.removedFromGrid.Contains(vehicleId))
                    {
                        if (Log.LogALot)
                        {
                            Log.Debug(this, "CheckVehicle", "Mov", vehicleId, vehicle.m_targetBuilding, vehicle.Info.name, VehicleHelper.GetVehicleName(vehicleId), vehicle.m_flags);
                        }

                        Singleton<VehicleManager>.instance.RemoveFromGrid(vehicleId, ref vehicle, false);
                        this.removedFromGrid.Add(vehicleId);
                    }
                }
            }
        }

        /// <summary>
        /// Logs a list of vehicle info for debug use.
        /// </summary>
        public override void DebugListLogVehicles()
        {
            if (this.removedFromGrid != null)
            {
                VehicleHelper.DebugListLog(this.removedFromGrid, this.TargetCategory);
            }
        }

        /// <summary>
        /// Remove categorized vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        public override void RemoveVehicle(ushort vehicleId)
        {
            if (this.removedFromGrid != null && this.removedFromGrid.Contains(vehicleId))
            {
                this.removedFromGrid.Remove(vehicleId);
            }
        }

        /// <summary>
        /// Gets the service building.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>
        /// True if building found.
        /// </returns>
        public override bool TryGetVehicle(ushort vehicleId, out IVehicleInfo vehicle)
        {
            if (this.removedFromGrid != null && this.removedFromGrid.Contains(vehicleId))
            {
                vehicle = new BlockingVehicleInfo(vehicleId);
                return true;
            }

            vehicle = null;
            return false;
        }

        public class BlockingVehicleInfo : IVehicleInfo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="BlockingVehicleInfo"/> class.
            /// </summary>
            /// <param name="vehicleId">The vehicle identifier.</param>
            public BlockingVehicleInfo(ushort vehicleId)
            {
                this.VehicleId = vehicleId;
            }

            /// <summary>
            /// Gets the vehicle identifier.
            /// </summary>
            /// <value>
            /// The vehicle identifier.
            /// </value>
            public ushort VehicleId { get; private set; }

            public void AddDebugInfoData(Log.InfoList info)
            {
                info.Add("VehicleInfo", "Blocking");
            }
        }
    }
}