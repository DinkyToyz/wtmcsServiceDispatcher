using System;
using System.Reflection;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Single method reflective caller.
    /// </summary>
    internal abstract class SingleMethod
    {
        /// <summary>
        /// The method information.
        /// </summary>
        private MethodInfo methodInfo = null;

        /// <summary>
        /// Method information is initialized.
        /// </summary>
        private bool methodInfoInitialized = false;

        /// <summary>
        /// Gets a value indicating whether this instance can call the method.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can call the method; otherwise, <c>false</c>.
        /// </value>
        public bool CanCall
        {
            get
            {
                this.InitMethodInfo();
                return this.methodInfo != null;
            }
        }

        /// <summary>
        /// Gets the method information.
        /// </summary>
        /// <value>
        /// The method information.
        /// </value>
        public MethodInfo MethodInfo
        {
            get
            {
                this.InitMethodInfo();
                return this.methodInfo;
            }
        }

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
        /// Gets the source class.
        /// </summary>
        /// <value>
        /// The source class.
        /// </value>
        protected abstract Type SourceClass
        {
            get;
        }

        /// <summary>
        /// Fails this instance.
        /// </summary>
        public void Fail()
        {
            this.methodInfo = null;
            this.methodInfoInitialized = true;
        }

        /// <summary>
        /// Tries to get the method information.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// <returns>True if success.</returns>
        public bool TryGetMethodInfo(out MethodInfo methodInfo)
        {
            this.InitMethodInfo();
            methodInfo = this.methodInfo;
            return methodInfo != null;
        }

        /// <summary>
        /// Initializes the method information.
        /// </summary>
        /// <exception cref="System.NullReferenceException">Method info not returned.</exception>
        private void InitMethodInfo()
        {
            if (!this.methodInfoInitialized)
            {
                //Log.DevDebug(this, "InitMethodInfo");
                try
                {
                    this.methodInfo = MonoDetour.FindMethod(this.SourceClass, this.MethodName, this.GetType(), this.SignatureMethodName);

                    if (this.methodInfo == null)
                    {
                        throw new NullReferenceException("Method info not returned");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(this, "InitMethodInfo", ex, this.MethodName, this.GetType(), this.SignatureMethodName);
                    this.methodInfo = null;
                }
                finally
                {
                    this.methodInfoInitialized = true;
                }
            }
        }
    }
}