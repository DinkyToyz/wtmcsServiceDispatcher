using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Transfer manager methods.
    /// </summary>
    internal static class TransferManagerHelper
    {
        /// <summary>
        /// The clean transfer offers maximum game version.
        /// </summary>
        private static uint cleanTransferOffersMaxGameVersion = Settings.AboveMaxTestedGameVersion;

        /// <summary>
        /// The clean transfer offers minimum game version.
        /// </summary>
        private static uint cleanTransferOffersMinGameVersion = BuildConfig.MakeVersionNumber(1, 3, 0, BuildConfig.ReleaseType.Final, 0, BuildConfig.BuildType.Unknown);

        /// <summary>
        /// There has been transfer manager helper errors.
        /// </summary>
        private static bool error = false;

        /// <summary>
        /// The incoming amount value.
        /// </summary>
        private static int[] incomingAmountValue = null;

        /// <summary>
        /// The incoming count value.
        /// </summary>
        private static ushort[] incomingCountValue = null;

        /// <summary>
        /// The incoming offers value.
        /// </summary>
        private static TransferManager.TransferOffer[] incomingOffersValue;

        /// <summary>
        /// The outgoing amount value.
        /// </summary>
        private static int[] outgoingAmountValue = null;

        /// <summary>
        /// The outgoing count value.
        /// </summary>
        private static ushort[] outgoingCountValue = null;

        /// <summary>
        /// The outgoing offers value.
        /// </summary>
        private static TransferManager.TransferOffer[] outgoingOffersValue;

        /// <summary>
        /// The transfer manager instance identifier.
        /// </summary>
        private static int? TransferManagerInstanceId = null;

        /// <summary>
        /// Gets the incoming amount.
        /// </summary>
        /// <value>
        /// The incoming amount.
        /// </value>
        public static int[] IncomingAmount => incomingAmountValue;

        /// <summary>
        /// Gets the incoming count.
        /// </summary>
        /// <value>
        /// The incoming count.
        /// </value>
        public static ushort[] IncomingCount => incomingCountValue;

        /// <summary>
        /// Gets the incoming offers.
        /// </summary>
        /// <value>
        /// The incoming offers.
        /// </value>
        public static TransferManager.TransferOffer[] IncomingOffers => incomingOffersValue;

        /// <summary>
        /// Gets the outgoing amount.
        /// </summary>
        /// <value>
        /// The outgoing amount.
        /// </value>
        public static int[] OutgoingAmount => outgoingAmountValue;

        /// <summary>
        /// Gets the outgoing count.
        /// </summary>
        /// <value>
        /// The outgoing count.
        /// </value>
        public static ushort[] OutgoingCount => outgoingCountValue;

        /// <summary>
        /// Gets the outgoing offers.
        /// </summary>
        /// <value>
        /// The outgoing offers.
        /// </value>
        public static TransferManager.TransferOffer[] OutgoingOffers => outgoingOffersValue;

        /// <summary>
        /// Checks the instance, and resaves references if necessary.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public static void CheckInstance(TransferManager instance)
        {
            if (TransferManagerInstanceId == null || !TransferManagerInstanceId.HasValue)
            {
                Initialize(instance);
            }
            else if (instance.GetInstanceID() != TransferManagerInstanceId)
            {
                DeInitialize();
                Initialize(instance);
            }
        }

        /// <summary>
        /// Cleans the transfer offers for the handled materials.
        /// This might not be a good idea.
        /// </summary>
        public static void CleanTransferOffers()
        {
            if (!error && Global.Settings.AllowReflection(cleanTransferOffersMinGameVersion, cleanTransferOffersMaxGameVersion))
            {
                try
                {
                    TransferManager transferManager = Singleton<TransferManager>.instance;

                    // Get private data.
                    CheckInstance(transferManager);

                    // Clean for hearses.
                    if (Global.CleanHearseTransferOffers)
                    {
                        CleanTransferOffers(TransferManager.TransferReason.Dead);
                    }

                    // Clean for garbage trucks.
                    if (Global.CleanGarbageTruckTransferOffers)
                    {
                        CleanTransferOffers(TransferManager.TransferReason.Garbage);
                    }

                    // Clean for hearses.
                    if (Global.CleanAmbulanceTransferOffers)
                    {
                        CleanTransferOffers(TransferManager.TransferReason.Sick);
                    }
                }
                catch (Exception ex)
                {
                    error = true;
                    Log.Error(typeof(TransferManagerHelper), "CleanTransferOffers", ex);
                }
            }
        }

        /// <summary>
        /// Logs the transfer offers.
        /// </summary>
        /// <param name="material">The material.</param>
        public static void DebugListLog(TransferManager.TransferReason material)
        {
            try
            {
                TransferManager transferManager = Singleton<TransferManager>.instance;

                // Get private data.
                TransferManager.TransferOffer[] outgoingOffers = GetOutgoingOffers(transferManager);
                TransferManager.TransferOffer[] incomingOffers = GetIncomingOffers(transferManager);
                ushort[] outgoingCount = GetOutgoingCount(transferManager);
                ushort[] incomingCount = GetIncomingCount(transferManager);
                int[] outgoingAmount = GetOutgoingAmount(transferManager);
                int[] incomingAmount = GetIncomingAmount(transferManager);
                Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

                // Log
                DebugListLog("Outgoing", outgoingOffers, outgoingCount, outgoingAmount, buildings, material);
                DebugListLog("Incoming", incomingOffers, incomingCount, incomingAmount, buildings, material);
            }
            catch (Exception ex)
            {
                error = true;
                Log.Error(typeof(TransferManagerHelper), "DebugListLog", ex, material);
            }
        }

        /// <summary>
        /// Logs the transfer offers.
        /// </summary>
        /// <param name="allMaterials">if set to <c>true</c> log for all materials.</param>
        public static void DebugListLog(bool allMaterials = false)
        {
            try
            {
                TransferManager transferManager = Singleton<TransferManager>.instance;

                // Get private data.
                TransferManager.TransferOffer[] outgoingOffers = GetOutgoingOffers(transferManager);
                TransferManager.TransferOffer[] incomingOffers = GetIncomingOffers(transferManager);
                ushort[] outgoingCount = GetOutgoingCount(transferManager);
                ushort[] incomingCount = GetIncomingCount(transferManager);
                int[] outgoingAmount = GetOutgoingAmount(transferManager);
                int[] incomingAmount = GetIncomingAmount(transferManager);
                Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

                List<TransferManager.TransferReason> materials;

                if (allMaterials)
                {
                    materials = new List<TransferManager.TransferReason>(
                        ((IEnumerable<TransferManager.TransferReason>)Enum.GetValues(typeof(TransferManager.TransferReason)))
                        .Where(m => m != TransferManager.TransferReason.None));
                }
                else
                {
                    materials = new List<TransferManager.TransferReason>();

                    if (Global.Settings.Garbage.DispatchVehicles)
                    {
                        materials.Add(TransferManager.TransferReason.Garbage);
                        materials.Add(TransferManager.TransferReason.GarbageMove);
                    }

                    if (Global.Settings.DeathCare.DispatchVehicles)
                    {
                        materials.Add(TransferManager.TransferReason.Dead);
                        materials.Add(TransferManager.TransferReason.DeadMove);
                    }

                    if (Global.Settings.HealthCare.DispatchVehicles)
                    {
                        materials.Add(TransferManager.TransferReason.Sick);
                        materials.Add(TransferManager.TransferReason.SickMove);
                    }
                }

                foreach (TransferManager.TransferReason material in materials)
                {
                    {
                        try
                        {
                            DebugListLog("Outgoing", outgoingOffers, outgoingCount, outgoingAmount, buildings, material);
                            DebugListLog("Incoming", incomingOffers, incomingCount, incomingAmount, buildings, material);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(typeof(TransferManagerHelper), "DebugListLog", ex, material);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                error = true;
                Log.Error(typeof(TransferManagerHelper), "DebugListLog", ex);
            }
        }

        /// <summary>
        /// Resets saved references.
        /// </summary>
        public static void DeInitialize()
        {
            if (Log.LogALot)
            {
                Log.DevDebug(typeof(TransferManagerHelper), "DeInitialize");
            }

            TransferManagerInstanceId = null;

            incomingAmountValue = null;
            incomingCountValue = null;
            incomingOffersValue = null;

            outgoingAmountValue = null;
            outgoingCountValue = null;
            outgoingOffersValue = null;
        }

        /// <summary>
        /// Fetches the incoming amount from a TransferManager instance.
        /// </summary>
        /// <param name="instance">The TransferManager instance.</param>
        /// <returns>The incoming amount.</returns>
        public static int[] GetIncomingAmount(TransferManager instance)
        {
            return (int[])GetFieldValue(instance, "m_incomingAmount");
        }

        /// <summary>
        /// Fetches the incoming count from a TransferManager instance.
        /// </summary>
        /// <param name="instance">The TransferManager instance.</param>
        /// <returns>The incoming count.</returns>
        public static ushort[] GetIncomingCount(TransferManager instance)
        {
            return (ushort[])GetFieldValue(instance, "m_incomingCount");
        }

        /// <summary>
        /// Fetches the incoming offers from a TransferManager instance.
        /// </summary>
        /// <param name="instance">The TransferManager instance.</param>
        /// <returns>The incoming offers.</returns>
        public static TransferManager.TransferOffer[] GetIncomingOffers(TransferManager instance)
        {
            return (TransferManager.TransferOffer[])GetFieldValue(instance, "m_incomingOffers");
        }

        /// <summary>
        /// Fetches the outgoing amount from a TransferManager instance.
        /// </summary>
        /// <param name="instance">The TransferManager instance.</param>
        /// <returns>The outgoing amount.</returns>
        public static int[] GetOutgoingAmount(TransferManager instance)
        {
            return (int[])GetFieldValue(instance, "m_outgoingAmount");
        }

        /// <summary>
        /// Fetches the outgoing count from a TransferManager instance.
        /// </summary>
        /// <param name="instance">The TransferManager instance.</param>
        /// <returns>The outgoing count.</returns>
        public static ushort[] GetOutgoingCount(TransferManager instance)
        {
            return (ushort[])GetFieldValue(instance, "m_outgoingCount");
        }

        /// <summary>
        /// Fetches the outgoing offers from a TransferManager instance.
        /// </summary>
        /// <param name="instance">The TransferManager instance.</param>
        /// <returns>The outgoing offers.</returns>
        public static TransferManager.TransferOffer[] GetOutgoingOffers(TransferManager instance)
        {
            return (TransferManager.TransferOffer[])GetFieldValue(instance, "m_outgoingOffers");
        }

        /// <summary>
        /// Logs some information.
        /// </summary>
        public static void LogInfo()
        {
            Log.Info(typeof(TransferManagerHelper), "LogInfo", "AllowCleanTransferOffers", Global.Settings.ReflectionAllowanceText(cleanTransferOffersMinGameVersion, cleanTransferOffersMaxGameVersion));
        }

        /// <summary>
        /// Makes an offer.
        /// </summary>
        /// <param name="targetBuildingId">The target building identifier.</param>
        /// <param name="targetCitizenId">The target citizen identifier.</param>
        /// <returns>The offer.</returns>
        public static TransferManager.TransferOffer MakeOffer(ushort targetBuildingId, uint targetCitizenId)
        {
            TransferManager.TransferOffer offer = new TransferManager.TransferOffer();
            if (targetCitizenId == 0)
            {
                offer.Building = targetBuildingId;
            }
            else
            {
                offer.Citizen = targetCitizenId;
            }

            return offer;
        }

        /// <summary>
        /// Cleans the transfer offers for the specified material.
        /// </summary>
        /// <param name="outgoingOffers">The outgoing offers.</param>
        /// <param name="incomingOffers">The incoming offers.</param>
        /// <param name="outgoingCount">The outgoing count.</param>
        /// <param name="incomingCount">The incoming count.</param>
        /// <param name="outgoingAmount">The outgoing amount.</param>
        /// <param name="incomingAmount">The incoming amount.</param>
        /// <param name="material">The material.</param>
        private static void CleanTransferOffers(
            TransferManager.TransferOffer[] outgoingOffers,
            TransferManager.TransferOffer[] incomingOffers,
            ushort[] outgoingCount,
            ushort[] incomingCount,
            int[] outgoingAmount,
            int[] incomingAmount,
            TransferManager.TransferReason material)
        {
            // Zero counts.
            for (int priority = 0; priority < 8; priority++)
            {
                int index = ((int)material * 8) + priority;

                incomingCount[index] = 0;
                outgoingCount[index] = 0;
            }

            // Zero amounts.
            incomingAmount[(int)material] = 0;
            outgoingAmount[(int)material] = 0;
        }

        /// <summary>
        /// Cleans the transfer offers for the specified material.
        /// </summary>
        /// <param name="material">The material.</param>
        private static void CleanTransferOffers(TransferManager.TransferReason material)
        {
            // Zero counts.
            for (int priority = 0; priority < 8; priority++)
            {
                int index = ((int)material * 8) + priority;

                incomingCountValue[index] = 0;
                outgoingCountValue[index] = 0;
            }

            // Zero amounts.
            incomingAmountValue[(int)material] = 0;
            outgoingAmountValue[(int)material] = 0;
        }

        /// <summary>
        /// Logs the transfer offers.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="offers">The offers.</param>
        /// <param name="count">The count.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="buildings">The buildings.</param>
        /// <param name="material">The material.</param>
        private static void DebugListLog(
            string direction,
            TransferManager.TransferOffer[] offers,
            ushort[] count,
            int[] amount,
            Building[] buildings,
            TransferManager.TransferReason material)
        {
            for (int priority = 0; priority < 8; priority++)
            {
                int index = ((int)material * 8) + priority;
                for (int i = 0; i < count[index]; i++)
                {
                    Log.InfoList info = new Log.InfoList();
                    TransferManager.TransferOffer offer = offers[(index * 256) + i];

                    info.Add("Active", offer.Active);
                    info.Add("Amount", offer.Amount);
                    info.Add("Priority", offer.Priority);
                    info.Add("Vehicle", offer.Vehicle, VehicleHelper.GetVehicleName(offer.Vehicle));
                    info.Add("Citizen", offer.Citizen, CitizenHelper.GetCitizenName(offer.Citizen));
                    info.Add("TransportLine", offer.TransportLine, TransportLineHelper.GetLineName(offer.TransportLine));
                    info.Add("Building", offer.Building, BuildingHelper.GetBuildingName(offer.Building), BuildingHelper.GetDistrictName(offer.Building));

                    if (buildings != null && offer.Building > 0 && buildings[offer.Building].Info != null && (buildings[offer.Building].m_flags & Building.Flags.Created) == Building.Flags.Created)
                    {
                        info.Add("Garbage", buildings[offer.Building].m_garbageBuffer);
                        info.Add("Dead", buildings[offer.Building].m_deathProblemTimer);
                    }

                    Log.DevDebug(typeof(TransferManagerHelper), "DebugListLog", direction, material, info);
                }
            }
        }

        /// <summary>
        /// Fetches the field value.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The field value.</returns>
        private static object GetFieldValue(TransferManager instance, string fieldName)
        {
            FieldInfo fieldInfo = instance.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            return fieldInfo.GetValue(instance);
        }

        /// <summary>
        /// Saves references from specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        private static void Initialize(TransferManager instance)
        {
            if (Log.LogALot)
            {
                Log.DevDebug(typeof(TransferManagerHelper), "Initialize");
            }

            incomingAmountValue = GetIncomingAmount(instance);
            incomingCountValue = GetIncomingCount(instance);
            incomingOffersValue = GetIncomingOffers(instance);

            outgoingAmountValue = GetOutgoingAmount(instance);
            outgoingCountValue = GetOutgoingCount(instance);
            outgoingOffersValue = GetOutgoingOffers(instance);

            TransferManagerInstanceId = instance.GetInstanceID();
        }
    }
}