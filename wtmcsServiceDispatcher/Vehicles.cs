using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICities;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using UnityEngine;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    internal class Vehicles
    {
        List<ServiceVehicleInfo> hearseVehicles = new List<ServiceVehicleInfo>();

        public ServiceVehicleInfo[] HearseVehicles
        {
            get
            {
                return hearseVehicles.ToArray();
            }
        }

        public class ServiceVehicleInfo
        {
            public ushort VehicleId = 0;

            public ushort SourceBuilding = 0;

            public ushort TargetBuilding = 0;

            public bool Busy
            {
                get
                {
                    return TargetBuilding != 0;
                }
            }

            public Vector3 Position;

            public ServiceVehicleInfo()
            {
            }

            public ServiceVehicleInfo(ushort vehicleId, Vehicle vehicle)
            {
                this.VehicleId = vehicleId;
                this.SourceBuilding = vehicle.m_sourceBuilding;
                this.TargetBuilding = vehicle.m_targetBuilding;
                this.Position = vehicle.GetLastFramePosition();
            }
        }

        private bool isUpdated = false;

        public void NewRun()
        {
            isUpdated = false;
        }

        public void Update()
        {
            if (isUpdated)
            {
                return;
            }

            hearseVehicles.Clear();

            Vehicle[] vehicles  = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            for (ushort id = 0; id < vehicles.Length; id++)
            {
                if (vehicles[id].Info != null)
                {
                    CategorizeVehicles(id, vehicles[id]);
                }
            }
        }

        public void CategorizeVehicles(ushort vehicleId, Vehicle vehicle)
        {
            if (Global.Settings.HandleHearses)
            {
                if (vehicle.Info.m_vehicleAI is HearseAI && vehicle.m_targetBuilding == 0)
                {
                    hearseVehicles.Add(new ServiceVehicleInfo(vehicleId, vehicle));
                }
            }
        }
    }
}
