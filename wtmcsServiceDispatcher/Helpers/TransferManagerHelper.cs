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
        /// There has been transfer manager helper errors.
        /// </summary>
        private static bool error = false;

        /// <summary>
        /// Cleans the transfer offers for the handled materials.
        /// </summary>
        public static void CleanTransferOffers()
        {
            if (!error)
            {
                try
                {
                    if ((Global.Settings.DispatchHearses && Global.Settings.CreateSpareHearses != Settings.SpareVehiclesCreation.Never) ||
                        (Global.Settings.DispatchGarbageTrucks && Global.Settings.CreateSpareGarbageTrucks != Settings.SpareVehiclesCreation.Never))
                    {
                        TransferManager transferManager = Singleton<TransferManager>.instance;

                        // Get private data.
                        TransferManager.TransferOffer[] outgoingOffers = OutgoingOffers(transferManager);
                        TransferManager.TransferOffer[] incomingOffers = IncomingOffers(transferManager);
                        ushort[] outgoingCount = OutgoingCount(transferManager);
                        ushort[] incomingCount = IncomingCount(transferManager);
                        int[] outgoingAmount = OutgoingAmount(transferManager);
                        int[] incomingAmount = IncomingAmount(transferManager);

                        // Clean for hearses.
                        if (Global.Settings.DispatchHearses && Global.Settings.CreateSpareHearses != Settings.SpareVehiclesCreation.Never)
                        {
                            CleanTransferOffers(outgoingOffers, incomingOffers, outgoingCount, incomingCount, outgoingAmount, incomingAmount, TransferManager.TransferReason.Dead);
                        }

                        // Clean for garbage trucks.
                        if (Global.Settings.DispatchGarbageTrucks && Global.Settings.CreateSpareGarbageTrucks != Settings.SpareVehiclesCreation.Never)
                        {
                            CleanTransferOffers(outgoingOffers, incomingOffers, outgoingCount, incomingCount, outgoingAmount, incomingAmount, TransferManager.TransferReason.Garbage);
                        }
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
        /// Logs the transfer offers.
        /// </summary>
        /// <param name="material">The material.</param>
        public static void LogTransferOffers(TransferManager.TransferReason material)
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

                // Log
                LogTransferOffers("Outgoing", outgoingOffers, outgoingCount, outgoingAmount, material);
                LogTransferOffers("Incoming", incomingOffers, incomingCount, incomingAmount, material);
            }
            catch (Exception ex)
            {
                error = true;
                Log.Error(typeof(TransferManagerHelper), "LogTransferOffers", ex, material);
            }
        }

        /// <summary>
        /// Logs the transfer offers.
        /// </summary>
        public static void LogTransferOffers()
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

                // Log for all materials
                foreach (TransferManager.TransferReason material in Enum.GetValues(typeof(TransferManager.TransferReason)))
                {
                    LogTransferOffers("Outgoing", outgoingOffers, outgoingCount, outgoingAmount, material);
                    LogTransferOffers("Incoming", incomingOffers, incomingCount, incomingAmount, material);
                }
            }
            catch (Exception ex)
            {
                error = true;
                Log.Error(typeof(TransferManagerHelper), "LogTransferOffers", ex);
            }
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
        /// Logs the transfer offers.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="offers">The offers.</param>
        /// <param name="count">The count.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="material">The material.</param>
        private static void LogTransferOffers(
            string direction,
            TransferManager.TransferOffer[] offers,
            ushort[] count,
            int[] amount,
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
                    info.Add("Building", offer.Building, BuildingHelper.GetBuildingName(offer.Building), BuildingHelper.GetDistrictName(offer.Building));
                    info.Add("Vehicle", offer.Vehicle, VehicleHelper.GetVehicleName(offer.Vehicle));
                    info.Add("Citizen", offer.Citizen, CitizenHelper.GetCitizenName(offer.Citizen));
                    info.Add("TransportLine", offer.TransportLine, TransportLineHelper.GetLineName(offer.TransportLine));

                    Log.Debug(typeof(TransferManagerHelper), "LogTransferOffers", direction, material, info);
                }
            }
        }
    }
}