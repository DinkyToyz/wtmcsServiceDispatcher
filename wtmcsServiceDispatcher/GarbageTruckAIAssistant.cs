using ColossalFramework;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Garbage truck AI inheritor.
    /// </summary>
    internal class GarbageTruckAIAssistant : GarbageTruckAI
    {
        /// <summary>
        /// Recalls the specified vehicle to source building.
        /// </summary>
        /// <param name="vehicleID">The vehicle identifier.</param>
        /// <param name="data">The vehicle data.</param>
        public void Recall(ushort vehicleID, ref Vehicle data)
        {
            // From original GarbageTruckAI.SetTarget and GarbageTruckAI.RemoveTarget code at game version 1.2.2 f3.
            Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)data.m_targetBuilding].RemoveGuestVehicle(vehicleID, ref data);
            data.m_targetBuilding = 0;
            data.m_flags &= ~Vehicle.Flags.WaitingTarget;
            data.m_waitCounter = (byte)0;
            data.m_flags |= Vehicle.Flags.GoingBack;

            if (this.StartPathFind(vehicleID, ref data))
            {
                return;
            }

            data.Unspawn(vehicleID);
        }
    }
}