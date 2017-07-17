using System;
using System.Collections.Generic;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher.Services
{
    internal class RecoveryCrews : HiddenVehicleService
    {
        /// <summary>
        /// The stuck vehicles.
        /// </summary>
        protected Dictionary<ushort, StuckVehicleInfo> stuckVehicles;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecoveryCrews"/> class.
        /// </summary>
        public RecoveryCrews() : base(ServiceHelper.ServiceType.RecoveryCrewDispatcher, Global.Settings.RecoveryCrews)
        { }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Services.IService" /> is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public override bool Enabled => (this.Settings.DispatchVehicles || Global.Settings.DispatchAnyVehicles) && this.HasVehicles;

        /// <summary>
        /// Gets a value indicating whether this instance has vehicles.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has vehicles; otherwise, <c>false</c>.
        /// </value>
        public override bool HasVehicles => this.stuckVehicles != null;

        /// <summary>
        /// Gets the service category.
        /// </summary>
        /// <value>
        /// The service category.
        /// </value>
        public override string ServiceCategory => "RecoveryCrews";

        /// <summary>
        /// Gets the service log pre/suffix.
        /// </summary>
        /// <value>
        /// The service log pre/suffix.
        /// </value>
        public override string ServiceLogFix => "HRC";

        /// <summary>
        /// Gets the target category.
        /// </summary>
        /// <value>
        /// The target category.
        /// </value>
        public override string TargetCategory => "StuckVehicle";

        /// <summary>
        /// Gets the target log pre/suffix.
        /// </summary>
        /// <value>
        /// The target log pre/suffix.
        /// </value>
        public override string TargetLogFix => "TVS";

        /// <summary>
        /// Checks the vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="vehicle">The vehicle.</param>
        public override void CheckVehicle(ushort vehicleId, ref Vehicle vehicle)
        {
            // Try to fix stuck vehicles.
            if (this.stuckVehicles != null)
            {
                if (StuckVehicleInfo.HasProblem(vehicleId, ref vehicle))
                {
                    StuckVehicleInfo stuckVehicle;
                    if (this.stuckVehicles.TryGetValue(vehicleId, out stuckVehicle))
                    {
                        stuckVehicle.Update(ref vehicle);
                    }
                    else
                    {
                        if (Log.LogALot)
                        {
                            Log.DevDebug(this, "CheckVehicle", "New", vehicleId, vehicle.m_flags, vehicle.m_flags & StuckVehicleInfo.FlagsToCheck, ConfusionHelper.VehicleIsConfused(ref vehicle));
                        }
                        stuckVehicle = new StuckVehicleInfo(vehicleId, ref vehicle);
                        this.stuckVehicles[vehicleId] = stuckVehicle;
                    }

                    if (stuckVehicle.HandleProblem())
                    {
                        if (Log.LogALot)
                        {
                            Log.DevDebug(this, "CheckVehicle", "Act", vehicleId);
                        }

                        this.stuckVehicles.Remove(vehicleId);
                    }
                }
                else if (this.stuckVehicles.ContainsKey(vehicleId))
                {
                    if (Log.LogALot)
                    {
                        Log.DevDebug(this, "CheckVehicle", "Del", vehicleId, vehicle.m_flags, vehicle.m_flags & StuckVehicleInfo.FlagsToCheck, ConfusionHelper.VehicleIsConfused(ref vehicle));
                    }

                    this.stuckVehicles.Remove(vehicleId);
                }
            }
        }

        /// <summary>
        /// Logs a list of vehicle info for debug use.
        /// </summary>
        public override void DebugListLogVehicles()
        {
            try
            {
                if (this.stuckVehicles != null)
                {
                    foreach (StuckVehicleInfo vehicle in this.stuckVehicles.Values)
                    {
                        Log.InfoList info = new Log.InfoList();
                        vehicle.AddDebugInfoData(info);
                        Log.DevDebug(this, "DebugListLog", "StuckVehicle", info.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "DebugListLogVehicles", ex);
            }
        }

        /// <summary>
        /// Remove categorized vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        public override void RemoveVehicle(ushort vehicleId)
        {
            if (this.stuckVehicles != null && this.stuckVehicles.ContainsKey(vehicleId))
            {
                this.stuckVehicles.Remove(vehicleId);
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
            StuckVehicleInfo stuckVehicle;
            if (this.stuckVehicles != null && this.stuckVehicles.TryGetValue(vehicleId, out stuckVehicle))
            {
                vehicle = stuckVehicle;
                return true;
            }

            vehicle = null;
            return false;
        }

        protected override void Initialize(bool constructing)
        {
            base.Initialize(constructing);

            if (this.Settings.DispatchVehicles || Global.Settings.DispatchAnyVehicles)
            {
                if (constructing || this.stuckVehicles == null)
                {
                    this.stuckVehicles = new Dictionary<ushort, StuckVehicleInfo>();
                }
            }
            else
            {
                this.stuckVehicles = null;
            }

            // Forget stuck vehicles that are no longer the dispatcher's responcibility.
            if (this.stuckVehicles != null && !Global.Settings.RecoveryCrews.DispatchVehicles)
            {
                ushort[] vehicleIds = this.stuckVehicles.WhereSelectToArray(kvp => !kvp.Value.RecoveryCrewsResponsibility, kvp => kvp.Key);

                for (int i = 0; i < vehicleIds.Length; i++)
                {
                    this.stuckVehicles.Remove(vehicleIds[i]);
                }
            }
        }
    }
}