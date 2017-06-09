﻿using System;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    internal class TransferManagerAddIncomingOfferDetour : MethodDetoursBase
    {
        /// <summary>
        /// The number of calls that were blocked.
        /// </summary>
        public static UInt64 Blocked = 0;

        /// <summary>
        /// The number of calls to the detoured method.
        /// </summary>
        public static UInt64 Calls = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransferManagerAddIncomingOfferDetour"/> class.
        /// </summary>
        public TransferManagerAddIncomingOfferDetour()
        {
            Calls = 0;
            Blocked = 0;
        }

        /// <summary>
        /// The original class type.
        /// </summary>
        public override Type OriginalClassType
        {
            get
            {
                return typeof(TransferManager);
            }
        }

        /// <summary>
        /// The maximum game version for detouring.
        /// </summary>
        protected override uint MaxGameVersion => Settings.AboveMaxTestedGameVersion;

        /// <summary>
        /// The minimum game version for detouring.
        /// </summary>
        protected override uint MinGameVersion
        {
            get
            {
                return BuildConfig.MakeVersionNumber(1, 7, 0, BuildConfig.ReleaseType.Final, 0, BuildConfig.BuildType.Unknown);
            }
        }

        /// <summary>
        /// The original method name.
        /// </summary>
        protected override string OriginalMethodName
        {
            get
            {
                return "AddIncomingOffer";
            }
        }

        /// <summary>
        /// The replacement method name.
        /// </summary>
        protected override string ReplacementMethodName
        {
            get
            {
                return "TransferManager_AddIncomingOffer_Override";
            }
        }

        /// <summary>
        /// Copied from original game code at game version 1.7.n-xn.
        /// </summary>
        public static void TransferManager_AddIncomingOffer_Original(TransferManager transferManager, TransferManager.TransferReason material, TransferManager.TransferOffer offer)
        {
            TransferManager.TransferOffer[] incomingOffers = TransferManagerHelper.IncomingOffers(transferManager);
            ushort[] incomingCount = TransferManagerHelper.IncomingCount(transferManager);
            int[] incomingAmount = TransferManagerHelper.IncomingAmount(transferManager);

            for (int priority = offer.Priority; priority >= 0; --priority)
            {
                int index = (int)material * 8 + priority;
                int num = (int)incomingCount[index];
                if (num < 256)
                {
                    incomingOffers[index * 256 + num] = offer;
                    incomingCount[index] = (ushort)(num + 1);
                    incomingAmount[(int)material] += offer.Amount;
                    break;
                }
            }
        }

        /// <summary>
        /// Blocks offers that should be handled by the mod.
        /// </summary>
        /// <param name="transferManager">The transfer manager.</param>
        /// <param name="material">The material.</param>
        /// <param name="offer">The offer.</param>
        public static void TransferManager_AddIncomingOffer_Override(TransferManager transferManager, TransferManager.TransferReason material, TransferManager.TransferOffer offer)
        {
            Calls++;

            if ((material == TransferManager.TransferReason.Dead && Global.Settings.DeathCare.DispatchVehicles) ||
                (material == TransferManager.TransferReason.Garbage && Global.Settings.Garbage.DispatchVehicles) ||
                (material == TransferManager.TransferReason.Sick && Global.Settings.HealthCare.DispatchVehicles))
            {
                return;
            }

            TransferManager_AddIncomingOffer_Original(transferManager, material, offer);
        }

        /// <summary>
        /// Logs the counts.
        /// </summary>
        public override void LogCounts()
        {
            Log.Debug(this, "Counts", Calls, Blocked);
        }
    }
}