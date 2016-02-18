using System;
using System.Collections.Generic;
using System.Linq;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Method detours.
    /// </summary>
    internal abstract class MethodDetours : IDisposable
    {
        /// <summary>
        /// Error when detouring.
        /// </summary>
        protected bool error = false;

        /// <summary>
        /// The detours.
        /// </summary>
        private Dictionary<Type, DetourInfo> detours = new Dictionary<Type, DetourInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodDetours"/> class.
        /// </summary>
        public MethodDetours()
        {
            this.AddClass(this.OriginalClassType);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="MethodDetours"/> class.
        /// </summary>
        ~MethodDetours()
        {
            this.Dispose();
        }

        /// <summary>
        /// Gets a value indicating whether the method can be detoured.
        /// </summary>
        /// <value>
        /// <c>true</c> if the method can be detoured.; otherwise, <c>false</c>.
        /// </value>
        public bool CanDetour
        {
            get
            {
                return !this.error &&
                       MonoDetour.CanDetour &&
                       BuildConfig.APPLICATION_VERSION >= this.MinGameVersion &&
                       BuildConfig.APPLICATION_VERSION < this.MaxGameVersion;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the method is detoured.
        /// </summary>
        /// <value>
        /// <c>true</c> if the method is detoured; otherwise, <c>false</c>.
        /// </value>
        public bool IsDetoured
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the maximum game version for detouring.
        /// </summary>
        public abstract uint MaxGameVersion
        {
            get;
        }

        /// <summary>
        /// Gets the minimum game version for detouring.
        /// </summary>
        public abstract uint MinGameVersion
        {
            get;
        }

        /// <summary>
        /// Gets the original class type.
        /// </summary>
        protected abstract Type OriginalClassType
        {
            get;
        }

        /// <summary>
        /// Gets the original method name.
        /// </summary>
        protected abstract string OriginalMethodName
        {
            get;
        }

        /// <summary>
        /// Gets the replacement method name.
        /// </summary>
        protected abstract string ReplacementMethodName
        {
            get;
        }

        /// <summary>
        /// Adds a class for which the method will be detoured.
        /// </summary>
        /// <param name="originalClass">The original class.</param>
        public void AddClass(Type originalClass)
        {
            if (!this.detours.ContainsKey(originalClass))
            {
                this.detours[originalClass] = new DetourInfo();

                if (this.IsDetoured)
                {
                    this.Detour();
                }
            }
        }

        /// <summary>
        /// Detours the method.
        /// </summary>
        public void Detour()
        {
            if (this.CanDetour)
            {
                bool revert = false;

                foreach (Type originalClass in this.detours.Keys)
                {
                    if (this.CanDetourClass(originalClass) && !this.detours[originalClass].Error)
                    {
                        MonoDetour detour = null;

                        try
                        {
                            detour = this.detours[originalClass].Detour;
                            Log.Debug(this, "Detour", originalClass, detour);

                            if (detour == null)
                            {
                                detour = new MonoDetour(originalClass, this.GetType(), this.OriginalMethodName, this.ReplacementMethodName);
                                this.detours[originalClass].Detour = detour;
                                Log.Debug(this, "Detour", "Created", originalClass, detour);
                            }

                            if (!detour.IsDetoured)
                            {
                                detour.Detour();
                                Log.Info(this, "Detour", "Detoured", originalClass, this.OriginalMethodName, this.ReplacementMethodName);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (detour != null)
                            {
                                try
                                {
                                    detour.Revert();
                                }
                                catch
                                {
                                }
                            }

                            this.detours[originalClass].Detour = null;
                            this.detours[originalClass].Error = true;

                            if (originalClass == this.OriginalClassType)
                            {
                                this.error = true;
                                revert = true;
                                Log.Error(this, "Detour", ex, originalClass);
                            }
                            else
                            {
                                Log.Debug(this, "Detour", "NoDetour", ex.GetType(), ex.Message);
                            }
                        }
                    }
                }

                if (revert)
                {
                    this.Revert();
                }
            }
        }

        /// <summary>
        /// Reverts all detours and releases the detour objects.
        /// </summary>
        public void Dispose()
        {
            this.Revert(true);
        }

        /// <summary>
        /// Reverts all detours, releases the detour objects and sets the object to error state.
        /// </summary>
        public void Abort()
        {
            this.error = true;
            this.Revert(true);
        }

        /// <summary>
        /// Reverts all detours.
        /// </summary>
        public void Revert()
        {
            this.Revert(false);
        }

        /// <summary>
        /// Determines whether this instance can detour the method for the specified original class.
        /// </summary>
        /// <param name="originalClass">The original class.</param>
        /// <returns>
        ///   <c>true</c> if the method can be detoured for the class.; otherwise, <c>false</c>.
        /// </returns>
        protected abstract bool CanDetourClass(Type originalClass);

        /// <summary>
        /// Reverts all detours.
        /// </summary>
        /// <param name="dispose">If set to <c>true</c> release the detour objects.</param>
        private void Revert(bool dispose = false)
        {
            Type[] originalClasses = this.detours.Keys.ToArray();
            foreach (Type originalClass in originalClasses)
            {
                this.Revert(originalClass);
            }

            if (dispose)
            {
                Log.Debug(this, "Clear");
                this.detours.Clear();
            }
        }

        /// <summary>
        /// Reverts a detours.
        /// </summary>
        /// <param name="originalClass">The original class.</param>
        /// <param name="dispose">If set to <c>true</c> release the detour object.</param>
        private void Revert(Type originalClass, bool dispose = false)
        {
            if (this.detours.ContainsKey(originalClass))
            {
                if (this.detours[originalClass].Detour != null && this.detours[originalClass].Detour.IsDetoured)
                {
                    Log.Info(this, "Revert", originalClass, this.OriginalMethodName, this.ReplacementMethodName);
                    try
                    {
                        this.detours[originalClass].Detour.Revert();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(this, "Revert", ex, originalClass, this.detours[originalClass].Detour);

                        this.detours[originalClass].Detour = null;
                        this.detours[originalClass].Error = true;
                    }
                    Log.Debug(this, "Reverted", originalClass, this.detours[originalClass]);
                }

                if (dispose)
                {
                    Log.Debug(this, "Dispose", originalClass);
                    this.detours[originalClass].Detour = null;
                    this.detours.Remove(originalClass);
                }
            }
        }

        /// <summary>
        /// Detour info.
        /// </summary>
        private class DetourInfo
        {
            /// <summary>
            /// The detour.
            /// </summary>
            public MonoDetour Detour = null;

            /// <summary>
            /// The error.
            /// </summary>
            public bool Error = false;
        }
    }
}