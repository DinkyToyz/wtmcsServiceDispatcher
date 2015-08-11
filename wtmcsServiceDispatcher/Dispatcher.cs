using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using ICities;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    internal class Dispatcher
    {
        private Dictionary<ushort, Buildings.serviceBuildingInfo> serviceBuildings = new Dictionary<ushort,Buildings.serviceBuildingInfo>();

            private uint cleaned = 0;

        protected FrameStamps Handled = new FrameStamps(240);
        protected FrameStamps Checked = new FrameStamps(60);

        protected class FrameStamps
        {
            private uint timeout;

            private Dictionary<ushort, uint> stamps = new Dictionary<ushort,uint>();

            public FrameStamps(uint timeout)
            {
                this.timeout = timeout;
            }

            public bool this[ushort targetId]
            {
                get
                {
                    if (!stamps.ContainsKey(targetId))
                    {
                        return false;
                    }

                    if (Global.CurrentFrame - stamps[targetId] < timeout)
                    {
                        return true;
                    }

                    return false;
                }
                set
                {
                    if (value)
                    {
                        stamps[targetId] = Global.CurrentFrame;
                    }
                    else
                    {
                        stamps.Remove(targetId);
                    }
                }
            }

            public void Clean()
            {
                KeyValuePair<ushort, uint>[] stampkvs = stamps.ToArray();
                foreach (KeyValuePair<ushort, uint> stamp in stampkvs)
                {
                    if (Global.CurrentFrame - stamp.Value > timeout * 10)
                    {
                        stamps.Remove(stamp.Key);
                    }
                }
            }
        }

        public void Clean()
        {
            if (Global.CurrentFrame - cleaned < 300)
            {
                return;
            }

            Handled.Clean();
            Checked.Clean();

            cleaned = Global.CurrentFrame;
        }

        protected void Initialize(ushort[] serviceBuildings, Vehicles.ServiceVehicleInfo[] serviceVehicles)
        {
            DistrictManager districtManager = Singleton<DistrictManager>.instance;
            Building[] buildingBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            this.serviceBuildings.Clear();

            foreach (ushort serviceBuildingId in serviceBuildings)
            {
                this.serviceBuildings.Add(serviceBuildingId, new Buildings.serviceBuildingInfo(districtManager, serviceBuildingId, buildingBuffer[serviceBuildingId]));
            }

            foreach (Vehicles.ServiceVehicleInfo vehicle in serviceVehicles)
            {
                if (serviceBuildings.Contains(vehicle.SourceBuilding))
                {
                    this.serviceBuildings[vehicle.SourceBuilding].Vehicles.Add(vehicle);
                }
            }
        }

        protected void AssignVehicle(ushort targetBuildingId, Building targetBuilding)
        {
            DistrictManager districtManager = null;
            byte targetDistrict = 0;
            if (Global.Settings.DispatchByDistrict && districtManager != null)
            {
                districtManager = Singleton<DistrictManager>.instance;
                targetDistrict = districtManager.GetDistrict(targetBuilding.m_position);
            }

            foreach (Buildings.serviceBuildingInfo serviceBuilding in serviceBuildings.Values)
            {
                serviceBuilding.SetTargetInfo(districtManager, targetBuildingId, targetBuilding);
            }

            foreach (Buildings.serviceBuildingInfo serviceBuilding in serviceBuildings.Values.OrderBy(i => i, new Buildings.serviceBuildingInfoComparer()))
            {
                Vehicles.ServiceVehicleInfo vehicleInfo = null;
                float vehicleDistance = float.PositiveInfinity;

                foreach (Vehicles.ServiceVehicleInfo vehicle in serviceBuilding.Vehicles)
                {
                    if (!vehicle.Busy)
                    {
                        float distance = (targetBuilding.m_position - vehicle.Position).sqrMagnitude;

                        if (vehicleInfo== null || distance < vehicleDistance)
                        {
                            vehicleInfo = vehicle;
                            vehicleDistance = distance;
                        }
                    }
                }

                if (vehicleInfo != null)
                {
                    vehicleInfo.TargetBuilding = targetBuildingId;
                    Handled[targetBuildingId] = true;

                    Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
                    vehicles[vehicleInfo.VehicleId].m_targetBuilding = targetBuildingId;
                }
            }
        }

        public void Dispatch(ushort[] targets)
        {
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            foreach (ushort target in targets)
            {
                AssignVehicle(target, buildings[target]);
            }
        }
    }
}
