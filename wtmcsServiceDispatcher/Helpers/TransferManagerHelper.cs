using System;
using System.Reflection;
using ColossalFramework;

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
        private static uint cleanTransferOffersMaxGameVersion = BuildConfig.MakeVersionNumber(1, 6, 0, BuildConfig.ReleaseType.Final, 0, BuildConfig.BuildType.Unknown);

        /// <summary>
        /// The clean transfer offers minimum game version.
        /// </summary>
        private static uint cleanTransferOffersMinGameVersion = BuildConfig.MakeVersionNumber(1, 3, 0, BuildConfig.ReleaseType.Final, 0, BuildConfig.BuildType.Unknown);

        /// <summary>
        /// There has been transfer manager helper errors.
        /// </summary>
        private static bool error = false;

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
                    if (Log.LogALot)
                    {
                        Log.DevDebug(typeof(TransferManagerHelper), "CleanTransferOffers");
                    }

                    TransferManager transferManager = Singleton<TransferManager>.instance;

                    // Get private data.
                    TransferManager.TransferOffer[] outgoingOffers = OutgoingOffers(transferManager);
                    TransferManager.TransferOffer[] incomingOffers = IncomingOffers(transferManager);
                    ushort[] outgoingCount = OutgoingCount(transferManager);
                    ushort[] incomingCount = IncomingCount(transferManager);
                    int[] outgoingAmount = OutgoingAmount(transferManager);
                    int[] incomingAmount = IncomingAmount(transferManager);

                    // Clean for hearses.
                    if (Global.CleanHearseTransferOffers)
                    {
                        if (Log.LogALot)
                        {
                            Log.DevDebug(typeof(TransferManagerHelper), "CleanTransferOffers", "TransferManager.TransferReason.Dead");
                        }

                        CleanTransferOffers(outgoingOffers, incomingOffers, outgoingCount, incomingCount, outgoingAmount, incomingAmount, TransferManager.TransferReason.Dead);
                    }

                    // Clean for garbage trucks.
                    if (Global.CleanGarbageTruckTransferOffers)
                    {
                        if (Log.LogALot)
                        {
                            Log.DevDebug(typeof(TransferManagerHelper), "CleanTransferOffers", "TransferManager.TransferReason.Garbage");
                        }

                        CleanTransferOffers(outgoingOffers, incomingOffers, outgoingCount, incomingCount, outgoingAmount, incomingAmount, TransferManager.TransferReason.Garbage);
                    }

                    // Clean for hearses.
                    if (Global.CleanAmbulanceTransferOffers)
                    {
                        if (Log.LogALot)
                        {
                            Log.DevDebug(typeof(TransferManagerHelper), "CleanTransferOffers", "TransferManager.TransferReason.Sick");
                        }

                        CleanTransferOffers(outgoingOffers, incomingOffers, outgoingCount, incomingCount, outgoingAmount, incomingAmount, TransferManager.TransferReason.Sick);
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
                TransferManager.TransferOffer[] outgoingOffers = OutgoingOffers(transferManager);
                TransferManager.TransferOffer[] incomingOffers = IncomingOffers(transferManager);
                ushort[] outgoingCount = OutgoingCount(transferManager);
                ushort[] incomingCount = IncomingCount(transferManager);
                int[] outgoingAmount = OutgoingAmount(transferManager);
                int[] incomingAmount = IncomingAmount(transferManager);
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
        public static void DebugListLog()
        {
            try
            {
                TransferManager transferManager = Singleton<TransferManager>.instance;

                // Get private data.
                TransferManager.TransferOffer[] outgoingOffers = OutgoingOffers(transferManager);
                TransferManager.TransferOffer[] incomingOffers = IncomingOffers(transferManager);
                ushort[] outgoingCount = OutgoingCount(transferManager);
                ushort[] incomingCount = IncomingCount(transferManager);
                int[] outgoingAmount = OutgoingAmount(transferManager);
                int[] incomingAmount = IncomingAmount(transferManager);
                Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

                // Log for garbage.
                DebugListLog("Outgoing", outgoingOffers, outgoingCount, outgoingAmount, buildings, TransferManager.TransferReason.Garbage);
                DebugListLog("Incoming", incomingOffers, incomingCount, incomingAmount, buildings, TransferManager.TransferReason.Garbage);
                DebugListLog("Outgoing", outgoingOffers, outgoingCount, outgoingAmount, buildings, TransferManager.TransferReason.GarbageMove);
                DebugListLog("Incoming", incomingOffers, incomingCount, incomingAmount, buildings, TransferManager.TransferReason.GarbageMove);

                // Log for dead people.
                DebugListLog("Outgoing", outgoingOffers, outgoingCount, outgoingAmount, buildings, TransferManager.TransferReason.Dead);
                DebugListLog("Incoming", incomingOffers, incomingCount, incomingAmount, buildings, TransferManager.TransferReason.Dead);
                DebugListLog("Outgoing", outgoingOffers, outgoingCount, outgoingAmount, buildings, TransferManager.TransferReason.DeadMove);
                DebugListLog("Incoming", incomingOffers, incomingCount, incomingAmount, buildings, TransferManager.TransferReason.DeadMove);
            }
            catch (Exception ex)
            {
                error = true;
                Log.Error(typeof(TransferManagerHelper), "DebugListLog", ex);
            }
        }

        /// <summary>
        /// Fetches the incoming amount from a TransferManager instance.
        /// </summary>
        /// <param name="instance">The TransferManager instance.</param>
        /// <returns>The incoming amount.</returns>
        public static int[] IncomingAmount(TransferManager instance)
        {
            return (int[])GetFieldValue(instance, "m_incomingAmount");
        }

        /// <summary>
        /// Fetches the incoming count from a TransferManager instance.
        /// </summary>
        /// <param name="instance">The TransferManager instance.</param>
        /// <returns>The incoming count.</returns>
        public static ushort[] IncomingCount(TransferManager instance)
        {
            return (ushort[])GetFieldValue(instance, "m_incomingCount");
        }

        /// <summary>
        /// Fetches the incoming offers from a TransferManager instance.
        /// </summary>
        /// <param name="instance">The TransferManager instance.</param>
        /// <returns>The incoming offers.</returns>
        public static TransferManager.TransferOffer[] IncomingOffers(TransferManager instance)
        {
            return (TransferManager.TransferOffer[])GetFieldValue(instance, "m_incomingOffers");
        }

        /// <summary>
        /// Logs some information.
        /// </summary>
        public static void LogInfo()
        {
            Log.Info(typeof(TransferManagerHelper), "LogInfo", "AllowCleanTransferOffers", Global.Settings.ReflectionAllowanceText(cleanTransferOffersMinGameVersion, cleanTransferOffersMaxGameVersion));
        }

        /// <summary>
        /// Fetches the outgoing amount from a TransferManager instance.
        /// </summary>
        /// <param name="instance">The TransferManager instance.</param>
        /// <returns>The outgoing amount.</returns>
        public static int[] OutgoingAmount(TransferManager instance)
        {
            return (int[])GetFieldValue(instance, "m_outgoingAmount");
        }

        /// <summary>
        /// Fetches the outgoing count from a TransferManager instance.
        /// </summary>
        /// <param name="instance">The TransferManager instance.</param>
        /// <returns>The outgoing count.</returns>
        public static ushort[] OutgoingCount(TransferManager instance)
        {
            return (ushort[])GetFieldValue(instance, "m_outgoingCount");
        }

        /// <summary>
        /// Fetches the outgoing offers from a TransferManager instance.
        /// </summary>
        /// <param name="instance">The TransferManager instance.</param>
        /// <returns>The outgoing offers.</returns>
        public static TransferManager.TransferOffer[] OutgoingOffers(TransferManager instance)
        {
            return (TransferManager.TransferOffer[])GetFieldValue(instance, "m_outgoingOffers");
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
            ////if (Log.LogALot && (incomingAmount[(int)material] != 0 || outgoingAmount[(int)material] != 0))
            ////{
            ////    Log.DevDebug(typeof(TransferManagerHelper), "CleanTransferOffers", "Amounts", incomingAmount[(int)material], outgoingAmount[(int)material]);
            ////}

            // Zero counts.
            for (int priority = 0; priority < 8; priority++)
            {
                int index = ((int)material * 8) + priority;

                ////if (Log.LogALot && (incomingCount[index] != 0 || outgoingCount[index] != 0))
                ////{
                ////    Log.DevDebug(typeof(TransferManagerHelper), "CleanTransferOffers", "Counts", index, incomingCount[index], outgoingCount[index]);
                ////}

                incomingCount[index] = 0;
                outgoingCount[index] = 0;
            }

            // Zero amounts.
            incomingAmount[(int)material] = 0;
            outgoingAmount[(int)material] = 0;
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
    }
}