﻿using System;
using System.Collections.Generic;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Detour collection.
    /// </summary>
    public static class Detours
    {
        /// <summary>
        /// A detour reinitialize is needed.
        /// </summary>
        public static bool InitNeeded = true;

        /// <summary>
        /// The methods detours.
        /// </summary>
        private static Dictionary<Methods, MethodDetoursBase> methodsDetours = null;

        /// <summary>
        /// Methods that can be detoured.
        /// </summary>
        public enum Methods
        {
            /// <summary>
            /// No method.
            /// </summary>
            None = 0,

            /// <summary>
            /// The GarbageTruckAI.TryCollectGarbage method.
            /// </summary>
            GarbageTruckAI_TryCollectGarbage = 1,

            /// <summary>
            /// The GarbageTruckAI.ShouldReturnToSource method.
            /// </summary>
            GarbageTruckAI_ShouldReturnToSource = 2,

            /// <summary>
            /// HearseAI.ShouldReturnToSource method.
            /// </summary>
            HearseAI_ShouldReturnToSource = 3,

            /// <summary>
            /// AmbulanceAI.ShouldReturnToSource method.
            /// </summary>
            AmbulanceAI_ShouldReturnToSource = 4,

            /// <summary>
            /// TransferManager.AddOutgoingOffer method.
            /// </summary>
            TransferManager_AddOutgoingOffer = 5,

            /// <summary>
            /// TransferManager.AddIncomingOffer method.
            /// </summary>
            TransferManager_AddIncomingOffer = 6
        }

        /// <summary>
        /// Aborts the specified method detour.
        /// </summary>
        /// <param name="method">The method.</param>
        public static void Abort(Methods method)
        {
            MethodDetoursBase detours;
            if (TryGetMethodDetours(method, out detours))
            {
                detours.Abort();
            }
        }

        /// <summary>
        /// Adds a class to detours.
        /// </summary>
        /// <param name="classType">The class type.</param>
        public static void AddClass(Type classType)
        {
            Assure();

            foreach (MethodDetoursBase detour in methodsDetours.Values)
            {
                if (detour != null && detour.CanDetourClass(classType))
                {
                    detour.AddClass(classType);
                }
            }
        }

        /// <summary>
        /// Determines whether this instance can detour the specified method.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>True if the method can be detoured.</returns>
        public static bool CanDetour(Methods method)
        {
            if (!Global.Settings.AllowReflection())
            {
                return false;
            }

            Assure();

            MethodDetoursBase detours;
            if (TryGetMethodDetours(method, out detours))
            {
                return detours.CanDetour;
            }

            return false;
        }

        /// <summary>
        /// Creates the detour objects.
        /// </summary>
        public static void Create()
        {
            Assure(true);
        }

        /// <summary>
        /// Disposes all detours.
        /// </summary>
        public static void Dispose()
        {
            if (methodsDetours != null)
            {
                foreach (MethodDetoursBase detour in methodsDetours.Values)
                {
                    if (detour != null)
                    {
                        detour.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the method detours.
        /// </summary>
        public static void Initialize()
        {
            Assure();

            if (Global.Settings.DeathCare.DispatchVehicles && CanDetour(Methods.HearseAI_ShouldReturnToSource))
            {
                Detour(Methods.HearseAI_ShouldReturnToSource);
            }
            else
            {
                Revert(Methods.HearseAI_ShouldReturnToSource);
            }

            if (Global.Settings.Garbage.DispatchVehicles && CanDetour(Methods.GarbageTruckAI_ShouldReturnToSource))
            {
                Detour(Methods.GarbageTruckAI_ShouldReturnToSource);
            }
            else
            {
                Revert(Methods.GarbageTruckAI_ShouldReturnToSource);
            }

            if (Global.Settings.Garbage.DispatchVehicles && Global.Settings.Garbage.LimitOpportunisticCollection && CanDetour(Methods.GarbageTruckAI_TryCollectGarbage))
            {
                Detour(Methods.GarbageTruckAI_TryCollectGarbage);
            }
            else
            {
                Revert(Methods.GarbageTruckAI_TryCollectGarbage);
            }

            if (Global.Settings.HealthCare.DispatchVehicles && CanDetour(Methods.AmbulanceAI_ShouldReturnToSource))
            {
                Detour(Methods.AmbulanceAI_ShouldReturnToSource);
            }
            else
            {
                Revert(Methods.AmbulanceAI_ShouldReturnToSource);
            }

            if (Global.Settings.DispatchAnyVehicles && Global.Settings.BlockTransferManagerOffers && Global.CleanTransferOffers)
            {
                if (CanDetour(Methods.TransferManager_AddIncomingOffer))
                {
                    Detour(Methods.TransferManager_AddIncomingOffer);
                }
                else
                {
                    Revert(Methods.TransferManager_AddIncomingOffer);
                }

                if (CanDetour(Methods.TransferManager_AddOutgoingOffer))
                {
                    Detour(Methods.TransferManager_AddOutgoingOffer);
                }
                else
                {
                    Revert(Methods.TransferManager_AddOutgoingOffer);
                }
            }
            else
            {
                Revert(Methods.TransferManager_AddIncomingOffer);
                Revert(Methods.TransferManager_AddOutgoingOffer);
            }

            InitNeeded = false;
        }

        /// <summary>
        /// Logs the counts.
        /// </summary>
        public static void LogCounts()
        {
            if (methodsDetours != null)
            {
                foreach (MethodDetoursBase detour in methodsDetours.Values)
                {
                    if (detour != null)
                    {
                        detour.LogCounts();
                    }
                }
            }
        }

        /// <summary>
        /// Logs the counts.
        /// </summary>
        /// <param name="method">The method.</param>
        public static void LogCounts(Methods method)
        {
            if (methodsDetours != null && methodsDetours[method] != null)
            {
                methodsDetours[method].LogCounts();
            }
        }

        /// <summary>
        /// Logs some information.
        /// </summary>
        public static void LogInfo()
        {
            if (methodsDetours != null)
            {
                foreach (MethodDetoursBase detour in methodsDetours.Values)
                {
                    if (detour != null)
                    {
                        detour.LogInfo();
                    }
                }
            }
        }

        /// <summary>
        /// Logs some information.
        /// </summary>
        /// <param name="method">The method.</param>
        public static void LogInfo(Methods method)
        {
            if (methodsDetours != null && methodsDetours[method] != null)
            {
                methodsDetours[method].LogInfo();
            }
        }

        /// <summary>
        /// Reverts all detours.
        /// </summary>
        public static void Revert()
        {
            if (methodsDetours != null)
            {
                foreach (MethodDetoursBase detour in methodsDetours.Values)
                {
                    if (detour != null)
                    {
                        detour.Revert();
                    }
                }
            }
        }

        /// <summary>
        /// Assures that the detour objects are created.
        /// </summary>
        /// <param name="create">If set to <c>true</c> re-create objects if they already exists.</param>
        private static void Assure(bool create = false)
        {
            try
            {
                if (create || methodsDetours == null)
                {
                    methodsDetours = new Dictionary<Methods, MethodDetoursBase>();

                    methodsDetours[Methods.HearseAI_ShouldReturnToSource] = new HearseAIShouldReturnToSourceDetour();
                    methodsDetours[Methods.GarbageTruckAI_TryCollectGarbage] = new GarbageTruckAITryCollectGarbageDetour();
                    methodsDetours[Methods.GarbageTruckAI_ShouldReturnToSource] = new GarbageTruckAIShouldReturnToSourceDetour();
                    methodsDetours[Methods.AmbulanceAI_ShouldReturnToSource] = new AmbulanceAIShouldReturnToSourceDetour();
                    methodsDetours[Methods.TransferManager_AddIncomingOffer] = new TransferManagerAddIncomingOfferDetour();
                    methodsDetours[Methods.TransferManager_AddOutgoingOffer] = new TransferManagerAddOutgoingOfferDetour();
                }
            }
            catch (Exception ex)
            {
                Log.Error(typeof(Detours), "Assure", ex);

                methodsDetours = null;
            }
        }

        /// <summary>
        /// Detours the specified method.
        /// </summary>
        /// <param name="method">The method.</param>
        private static void Detour(Methods method)
        {
            Assure();

            MethodDetoursBase detours;
            if (TryGetMethodDetours(method, out detours))
            {
                detours.Detour();
            }
        }

        /// <summary>
        /// Reverts the specified method.
        /// </summary>
        /// <param name="method">The method.</param>
        private static void Revert(Methods method)
        {
            MethodDetoursBase detours;
            if (TryGetMethodDetours(method, out detours))
            {
                detours.Revert();
            }
        }

        /// <summary>
        /// Tries to get the method detours for the specified method.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="detours">The method detours.</param>
        /// <returns>True if the method detours were retrieved.</returns>
        private static bool TryGetMethodDetours(Methods method, out MethodDetoursBase detours)
        {
            if (methodsDetours != null && methodsDetours.TryGetValue(method, out detours) && detours != null)
            {
                return true;
            }

            detours = null;
            return false;
        }
    }
}