using System;
using System.Collections.Generic;
using System.Reflection;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Object method reflective caller base class.
    /// </summary>
    internal abstract class ObjectMethods
    {
        /// <summary>
        /// The methods.
        /// </summary>
        private Dictionary<Type, MethodInfo> methods = new Dictionary<Type, MethodInfo>();

        /// <summary>
        /// The unhandled classes.
        /// </summary>
        private HashSet<Type> unhandledClasses = new HashSet<Type>();

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        /// <value>
        /// The name of the method.
        /// </value>
        protected abstract string MethodName
        {
            get;
        }

        /// <summary>
        /// Gets the name of the signature method.
        /// </summary>
        /// <value>
        /// The name of the signature method.
        /// </value>
        protected abstract string SignatureMethodName
        {
            get;
        }

        /// <summary>
        /// Gets the <see cref="MethodInfo"/> for the specified source class.
        /// </summary>
        /// <value>
        /// The <see cref="MethodInfo"/>.
        /// </value>
        /// <param name="sourceClass">The source class.</param>
        /// <returns>The method info for the source class.</returns>
        public MethodInfo this[Type sourceClass]
        {
            get
            {
                return this.GetMethodInfo(sourceClass);
            }
        }

        /// <summary>
        /// Determines whether this instance can call the method in the specified source class.
        /// </summary>
        /// <param name="sourceClass">The source class.</param>
        /// <returns>True if the method in the specified source class can be called.</returns>
        public bool CanCall(Type sourceClass)
        {
            return this.GetMethodInfo(sourceClass) != null;
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            this.methods.Clear();
            this.unhandledClasses.Clear();
        }

        /// <summary>
        /// Fails the class.
        /// </summary>
        /// <param name="sourceClass">The source class.</param>
        public void FailClass(Type sourceClass)
        {
            this.methods[sourceClass] = null;
        }

        /// <summary>
        /// Tries to the get method information.
        /// </summary>
        /// <param name="sourceClass">The source class.</param>
        /// <param name="methodInfo">The method information.</param>
        /// <returns>True if success.</returns>
        public bool TryGetMethodInfo(Type sourceClass, out MethodInfo methodInfo)
        {
            methodInfo = this.GetMethodInfo(sourceClass);
            return methodInfo != null;
        }

        /// <summary>
        /// Check if method applies to class.
        /// </summary>
        /// <param name="sourceClass">The source class.</param>
        /// <returns>True if method applies to class.</returns>
        protected abstract bool AppliesToCLass(Type sourceClass);

        /// <summary>
        /// Gets the information for the method in the source class.
        /// </summary>
        /// <param name="sourceClass">The source class.</param>
        /// <returns>The method information.</returns>
        /// <exception cref="System.NullReferenceException">Method info not returned.</exception>
        private MethodInfo GetMethodInfo(Type sourceClass)
        {
            if (!Global.Settings.UseReflection)
            {
                return null;
            }

            if (this.AppliesToCLass(sourceClass))
            {
                MethodInfo methodInfo;
                if (this.methods.TryGetValue(sourceClass, out methodInfo))
                {
                    return methodInfo;
                }

                try
                {
                    Log.DevDebug(this, "GetMethodInfo", sourceClass);
                    methodInfo = MonoDetour.FindMethod(sourceClass, this.MethodName, this.GetType(), this.SignatureMethodName);

                    if (methodInfo == null)
                    {
                        throw new NullReferenceException("Method info not returned");
                    }

                    this.methods[sourceClass] = methodInfo;
                    return methodInfo;
                }
                catch (Exception ex)
                {
                    Log.Warning(this, "GetMethodInfo", "Failed", sourceClass, ex.GetType(), ex.Message);
                    this.methods[sourceClass] = null;

                    return null;
                }
            }
            if (!this.unhandledClasses.Contains(sourceClass))
            {
                this.unhandledClasses.Add(sourceClass);
                Log.Warning(this, "GetMethodInfo", "Failed", sourceClass);
            }

            return null;
        }
    }
}