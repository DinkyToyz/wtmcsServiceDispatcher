using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Method detour class for Mono.
    /// </summary>
    internal class MonoDetour
    {
        /// <summary>
        /// The calling conventions indicating existence of a "this" parameter.
        /// </summary>
        private static readonly CallingConventions TheseCallingConventions = CallingConventions.HasThis | CallingConventions.ExplicitThis;

        /// <summary>
        /// The calling conventions that must be the same when comparing two methods.
        /// </summary>
        private static readonly CallingConventions ValidateCallingConventions = CallingConventions.Standard | CallingConventions.VarArgs | CallingConventions.Any;

        /// <summary>
        /// The original call site.
        /// </summary>
        private CallSite originalCallSite = CallSite.Zero;

        /// <summary>
        /// The original call info.
        /// </summary>
        private CodeStart originalCodeStart = CodeStart.Zero;

        /// <summary>
        /// The original method's name.
        /// </summary>
        private string originalMethodName;

        /// <summary>
        /// The original method's class type.
        /// </summary>
        private Type originalType;

        /// <summary>
        /// The replacement call site.
        /// </summary>
        private CallSite replacementCallSite = CallSite.Zero;

        /// <summary>
        /// The replacement method's name.
        /// </summary>
        private string replacementMethodName;

        /// <summary>
        /// The replacement method's class type.
        /// </summary>
        private Type replacementType;

        /// <summary>
        /// Initializes a new instance of the <see cref="MonoDetour" /> class.
        /// </summary>
        /// <param name="originalClass">The original class.</param>
        /// <param name="replacementClass">The replacement class.</param>
        /// <param name="originalMethodName">Name of the original method.</param>
        /// <param name="replacementMethodName">Name of the replacement method.</param>
        /// <exception cref="System.ArgumentException">
        /// Original method name not defined
        /// or
        /// Replacement method name not defined.
        /// </exception>
        /// <exception cref="System.NullReferenceException">
        /// Replacement method not found
        /// or
        /// Original method not found
        /// or
        /// Original method address not found
        /// or
        /// Replacement method address not found
        /// or
        /// Original call info not found.
        /// </exception>
        public MonoDetour(Type originalClass, Type replacementClass, string originalMethodName, string replacementMethodName = null)
        {
            Log.Debug(this, "MonoDetour", "Construct", originalClass, replacementClass, originalMethodName, replacementMethodName);

            this.IsDetoured = false;
            this.originalType = originalClass;
            this.replacementType = replacementClass;
            this.originalMethodName = originalMethodName;
            this.replacementMethodName = (replacementMethodName == null) ? originalMethodName : replacementMethodName;

            if (String.IsNullOrEmpty(this.originalMethodName))
            {
                throw new ArgumentException("Original method name not defined");
            }

            if (String.IsNullOrEmpty(this.replacementMethodName))
            {
                throw new ArgumentException("Replacement method name not defined");
            }

            // Get info for the replacement method.
            MethodInfo replacementMethod = replacementClass.GetMethod(
                                            this.replacementMethodName,
                                            BindingFlags.Default | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (replacementMethod == null)
            {
                throw new NullReferenceException("Replacement method not found");
            }

            // Find and get info for the original method by comparing all methods with the specified name to the replacement method.
            MethodInfo originalMethod = FindMethod(originalClass, originalMethodName, replacementMethod);

            if (originalMethod == null)
            {
                throw new NullReferenceException("Original method not found");
            }

            this.originalCallSite = originalMethod.MethodHandle.GetFunctionPointer();
            if (this.originalCallSite == CallSite.Zero)
            {
                throw new NullReferenceException("Original method address not found");
            }

            this.replacementCallSite = replacementMethod.MethodHandle.GetFunctionPointer();
            if (this.replacementCallSite == CallSite.Zero)
            {
                throw new NullReferenceException("Replacement method address not found");
            }

            // Save call info to original code.
            this.originalCodeStart = new CodeStart(this.originalCallSite);
            if (this.originalCodeStart == CodeStart.Zero)
            {
                throw new NullReferenceException("Original call info not found");
            }

            if (Log.LogALot)
            {
                Log.DevDebug(this, "MonoDetour", "Constructed", originalClass, replacementClass, originalMethodName, replacementMethodName);
            }
        }

        /// <summary>
        /// Gets a value indicating whether methods can be detoured or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if methods can be detoured; otherwise, <c>false</c>.
        /// </value>
        public static bool CanDetour
        {
            get
            {
                try
                {
                    PortableExecutableKinds exeKind;
                    ImageFileMachine imgfMachine;
                    typeof(object).Module.GetPEKind(out exeKind, out imgfMachine);

                    bool canDetour = imgfMachine == ImageFileMachine.IA64 || imgfMachine == ImageFileMachine.AMD64 || imgfMachine == ImageFileMachine.I386;

                    if (Log.LogALot)
                    {
                        Log.DevDebug(typeof(MonoDetour), "CanDetour", canDetour, exeKind, imgfMachine);
                    }

                    return canDetour;
                }
                catch (Exception ex)
                {
                    Log.Error(typeof(MonoDetour), "CanDetour", ex);
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the method is detoured.
        /// </summary>
        /// <value>
        /// <c>True</c> if the method is detoured; otherwise, <c>false</c>.
        /// </value>
        public bool IsDetoured
        {
            get;
            private set;
        }

        /// <summary>
        /// Finds the method.
        /// </summary>
        /// <param name="sourceClass">The source class.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns>
        /// The method information.
        /// </returns>
        /// <exception cref="System.NullReferenceException">Signature method not found.</exception>
        public static MethodInfo FindMethod(Type sourceClass, string methodName)
        {
            return FindMethod(sourceClass, methodName, null);
        }

        /// <summary>
        /// Finds the method.
        /// </summary>
        /// <param name="sourceClass">The source class.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="signatureClass">The signature class.</param>
        /// <param name="signatureMethodName">Name of the signature method.</param>
        /// <returns>
        /// The method information.
        /// </returns>
        /// <exception cref="System.NullReferenceException">Signature method not found.</exception>
        public static MethodInfo FindMethod(Type sourceClass, string methodName, Type signatureClass, string signatureMethodName)
        {
            // Get info for the signature method.
            MethodInfo signatureMethod = signatureClass.GetMethod(
                                            signatureMethodName,
                                            BindingFlags.Default | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (signatureMethod == null)
            {
                throw new NullReferenceException("Signature method not found");
            }

            return FindMethod(sourceClass, methodName, signatureMethod);
        }

        /// <summary>
        /// Detours the method.
        /// </summary>
        /// <exception cref="System.NullReferenceException">
        /// Original call site not defined
        /// or
        /// Replacement call site not defined.
        /// </exception>
        public void Detour()
        {
            if (this.originalCallSite == CallSite.Zero)
            {
                throw new NullReferenceException("Original call site not defined");
            }

            if (this.replacementCallSite == CallSite.Zero)
            {
                throw new NullReferenceException("Replacement call site not defined");
            }

            this.PatchCallSiteWithJump(this.originalCallSite, this.replacementCallSite);

            this.IsDetoured = true;
        }

        /// <summary>
        /// Reverts the detour.
        /// </summary>
        /// <exception cref="System.NullReferenceException">
        /// Original call site not defined
        /// or
        /// Original call info not defined.
        /// </exception>
        public void Revert()
        {
            if (this.originalCallSite == CallSite.Zero)
            {
                throw new NullReferenceException("Original call site not defined");
            }

            if (this.originalCodeStart == CodeStart.Zero)
            {
                throw new NullReferenceException("Original call info not defined");
            }

            this.IsDetoured = false;
            this.PatchCallSiteWithCodeStart(this.originalCallSite, this.originalCodeStart);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return (new StringBuilder())
                    .Append("OC:").Append(this.originalType.ToString()).Append(", ")
                    .Append("RC:").Append(this.replacementType.ToString()).Append(", ")
                    .Append("OMN:").Append((this.originalMethodName == null) ? "~" : this.originalMethodName).Append(", ")
                    .Append("RMN:").Append((this.replacementMethodName == null) ? "~" : this.replacementMethodName).Append(", ")
                    .Append("OCS:").Append(this.originalCallSite).Append(", ")
                    .Append("RCS:").Append(this.replacementCallSite).Append(", ")
                    .Append("OCI:").Append(this.originalCodeStart.ToString()).Append(", ")
                    .Append("D:").Append(this.IsDetoured.ToString())
                    .ToString();
        }

        /// <summary>
        /// Checks if types are compatible.
        /// </summary>
        /// <param name="type1">The first type.</param>
        /// <param name="type2">The second type.</param>
        /// <returns>True if types are related or assignable.</returns>
        private static bool CompatibleTypes(Type type1, Type type2)
        {
            return RelatedTypes(type1, type2) || type1.IsAssignableFrom(type2) || type2.IsAssignableFrom(type1);
        }

        /// <summary>
        /// Finds the method.
        /// </summary>
        /// <param name="sourceClass">The source class.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="signatureMethod">The signature method.</param>
        /// <returns>The method information.</returns>
        private static MethodInfo FindMethod(Type sourceClass, string methodName, MethodInfo signatureMethod)
        {
            foreach (MethodInfo method in sourceClass.GetMethods(BindingFlags.Default | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                if (method.Name == methodName)
                {
                    if (ValidateSignatures(signatureMethod, method))
                    {
                        Log.DevDebug(typeof(MonoDetour), "FindMethod", "MethodFound", methodName, method.Name);

                        return method;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Check if types are related.
        /// </summary>
        /// <param name="type1">The first type.</param>
        /// <param name="type2">The second type.</param>
        /// <returns>True if types are related.</returns>
        private static bool RelatedTypes(Type type1, Type type2)
        {
            return type1 == type2 || type1.IsSubclassOf(type2) || type2.IsSubclassOf(type1);
        }

        /// <summary>
        /// Validates the method signatures.
        /// </summary>
        /// <param name="method1">The first method.</param>
        /// <param name="method2">The second method2.</param>
        /// <returns>True on success.</returns>
        private static bool ValidateSignatures(MethodInfo method1, MethodInfo method2)
        {
            // Validate method info.
            if ((method1.ReturnType != method2.ReturnType) ||
                (method1.IsGenericMethod != method2.IsGenericMethod) ||
                ((method1.CallingConvention & ValidateCallingConventions) != (method2.CallingConvention & ValidateCallingConventions)))
            {
                return false;
            }

            // Get parameters.
            List<ParameterInfo> params1 = method1.GetParameters().OrderBy(p => p.Position).ToList();
            List<ParameterInfo> params2 = method2.GetParameters().OrderBy(p => p.Position).ToList();

            int pos1Add = 0;

            if ((method1.CallingConvention & TheseCallingConventions) == CallingConventions.HasThis &&
                (method2.CallingConvention & TheseCallingConventions) != CallingConventions.HasThis &&
                params2[0].Position == 0 &&
                (RelatedTypes(params2[0].ParameterType, method1.DeclaringType) || RelatedTypes(params2[0].ParameterType, method1.ReflectedType)))
            {
                params2.RemoveAt(0);
                pos1Add = +1;
            }
            else if ((method2.CallingConvention & TheseCallingConventions) == CallingConventions.HasThis &&
                     (method1.CallingConvention & TheseCallingConventions) != CallingConventions.HasThis &&
                     params1[0].Position == 0 &&
                     (RelatedTypes(params1[0].ParameterType, method2.DeclaringType) || RelatedTypes(params1[0].ParameterType, method2.ReflectedType)))
            {
                params1.RemoveAt(0);
                pos1Add = -1;
            }

            if (method1.ReturnParameter != null)
            {
                params1.Add(method1.ReturnParameter);
            }

            if (method2.ReturnParameter != null)
            {
                params2.Add(method2.ReturnParameter);
            }

            // Validate parameter count.
            if (params1.Count != params2.Count)
            {
                return false;
            }

            // Validate parameters,
            for (int i = 0; i < params1.Count; i++)
            {
                // Validate parameter info.
                if ((params1[i].IsOut != params2[i].IsOut) ||
                    (params1[i].IsRetval != params2[i].IsRetval) ||
                    (params1[i].IsIn != params2[i].IsIn) ||
                    (params1[i].IsLcid != params2[i].IsLcid) ||
                    (params1[i].IsOptional != params2[i].IsOptional) ||
                    ((params1[i].DefaultValue == null) != (params2[i].DefaultValue == null)))
                {
                    return false;
                }

                // Validate parameter type.
                if (!CompatibleTypes(params1[i].ParameterType, params2[i].ParameterType))
                {
                    return false;
                }

                // Validate parameter default values.
                if ((params1[i].DefaultValue != null) && (params1[i].DefaultValue.ToString() != params2[i].DefaultValue.ToString()))
                {
                    return false;
                }

                // Validate parameter position.
                int pos1 = params1[i].Position + (params1[i].Position >= 0 ? pos1Add : 0);
                if (pos1 != params2[i].Position)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Patches the call site with whatever instructions are in code start.
        /// </summary>
        /// <param name="callSite">The call site.</param>
        /// <param name="codeStart">The call info.</param>
        private void PatchCallSiteWithCodeStart(IntPtr callSite, CodeStart codeStart)
        {
            codeStart.PatchCallSite(callSite);
        }

        /// <summary>
        /// Patches the call site with an assembly jump to pointer.
        /// </summary>
        /// <param name="callSite">The call site.</param>
        /// <param name="targetAddress">The target address pointer.</param>
        /// <exception cref="System.NullReferenceException">
        /// Call site not defined
        /// or
        /// Target address not defined.
        /// </exception>
        private void PatchCallSiteWithJump(IntPtr callSite, IntPtr targetAddress)
        {
            if (callSite == IntPtr.Zero)
            {
                throw new NullReferenceException("Call site not defined");
            }

            if (targetAddress == IntPtr.Zero)
            {
                throw new NullReferenceException("Target address not defined");
            }

            //Log.DevDebug(this, "PatchCallSite", callSite, targetAddress);

            unsafe
            {
                byte* rawPointer = (byte*)callSite.ToPointer();

                // Use temporary %r11 register.
                *rawPointer = 0x49; // movabs
                *(rawPointer + 1) = 0xBB; // %r11
                *((ulong*)(rawPointer + 2)) = (ulong)targetAddress.ToInt64(); // jump target address

                // jump
                *(rawPointer + 10) = 0x41; // encoded ...
                *(rawPointer + 11) = 0xFF; // ... uncondotional jump
                *(rawPointer + 12) = 0xE3; // %r11
            }
        }

        /// <summary>
        /// Integer pointer wrapper for call site.
        /// </summary>
        private struct CallSite
        {
            /// <summary>
            /// The zero, or null, call site.
            /// </summary>
            public static readonly CallSite Zero = new CallSite(IntPtr.Zero);

            /// <summary>
            /// The pointer.
            /// </summary>
            private IntPtr pointer;

            /// <summary>
            /// Initializes a new instance of the <see cref="CallSite"/> struct.
            /// </summary>
            /// <param name="pointer">The pointer.</param>
            public CallSite(IntPtr pointer)
            {
                this.pointer = pointer;
            }

            /// <summary>
            /// Performs an implicit conversion from <see cref="IntPtr"/> to <see cref="CallSite"/>.
            /// </summary>
            /// <param name="pointer">The pointer.</param>
            /// <returns>
            /// The result of the conversion.
            /// </returns>
            public static implicit operator CallSite(IntPtr pointer)
            {
                return new CallSite(pointer);
            }

            /// <summary>
            /// Performs an implicit conversion from <see cref="CallSite"/> to <see cref="IntPtr"/>.
            /// </summary>
            /// <param name="callSite">The call site.</param>
            /// <returns>
            /// The result of the conversion.
            /// </returns>
            public static implicit operator IntPtr(CallSite callSite)
            {
                return callSite.pointer;
            }

            /// <summary>
            /// Implements the operator !=.
            /// </summary>
            /// <param name="callSite1">The first call site.</param>
            /// <param name="callSite2">The second call site.</param>
            /// <returns>
            /// The result of the operator.
            /// </returns>
            public static bool operator !=(CallSite callSite1, CallSite callSite2)
            {
                return callSite1.pointer != callSite2.pointer;
            }

            /// <summary>
            /// Implements the operator ==.
            /// </summary>
            /// <param name="callSite1">The first call site.</param>
            /// <param name="callSite2">The second call site.</param>
            /// <returns>
            /// The result of the operator.
            /// </returns>
            public static bool operator ==(CallSite callSite1, CallSite callSite2)
            {
                return callSite1.pointer == callSite2.pointer;
            }

            /// <summary>
            /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
            /// </summary>
            /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
            /// <returns>
            ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
            /// </returns>
            public override bool Equals(object obj)
            {
                if (obj is CallSite)
                {
                    return ((CallSite)obj).pointer.Equals(this.pointer);
                }

                if (obj is IntPtr)
                {
                    return obj.Equals(this.pointer);
                }

                return false;
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
            /// </returns>
            public override int GetHashCode()
            {
                return this.pointer.ToString().GetHashCode();
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                if (this.pointer == IntPtr.Zero)
                {
                    return this.pointer.ToString("X16");
                }

                CodeStart codeStart = new CodeStart(this);
                if (codeStart == CodeStart.Zero)
                {
                    return this.pointer.ToString("X16");
                }

                return this.pointer.ToString("X16") + "(" + codeStart.ToString() + ")";
            }
        }

        /// <summary>
        /// Assembly code from start of call site.
        /// </summary>
        private struct CodeStart
        {
            /// <summary>
            /// The zero, or null, call info.
            /// </summary>
            public static readonly CodeStart Zero = new CodeStart(IntPtr.Zero);

            /// <summary>
            /// Byte at 0 bytes offset from the call site address.
            /// </summary>
            private readonly byte b00;

            /// <summary>
            /// Byte at 1 bytes offset from the call site address.
            /// </summary>
            private readonly byte b01;

            /// <summary>
            /// Byte at 10 bytes offset from the call site address.
            /// </summary>
            private readonly byte b10;

            /// <summary>
            /// Byte at 11 bytes offset from the call site address.
            /// </summary>
            private readonly byte b11;

            /// <summary>
            /// Byte at 12 bytes offset from the call site address.
            /// </summary>
            private readonly byte b12;

            /// <summary>
            /// Unsigned long at 2 bytes offset from the call site address.
            /// </summary>
            private readonly ulong ul02;

            /// <summary>
            /// Initializes a new instance of the <see cref="CodeStart" /> struct.
            /// </summary>
            /// <param name="callSite">The call site address.</param>
            public CodeStart(IntPtr callSite)
            {
                if (callSite == IntPtr.Zero)
                {
                    this.b00 = 0;
                    this.b01 = 0;
                    this.ul02 = 0;
                    this.b10 = 0;
                    this.b11 = 0;
                    this.b12 = 0;
                }
                else
                {
                    unsafe
                    {
                        byte* rawPointer = (byte*)callSite.ToPointer();

                        this.b00 = *rawPointer;
                        this.b01 = *(rawPointer + 1);
                        this.ul02 = *((ulong*)(rawPointer + 2));
                        this.b10 = *(rawPointer + 10);
                        this.b11 = *(rawPointer + 11);
                        this.b12 = *(rawPointer + 12);
                    }
                }
            }

            /// <summary>
            /// Implements the operator !=.
            /// </summary>
            /// <param name="codeStart1">The first call info.</param>
            /// <param name="codeStart2">The second call info.</param>
            /// <returns>
            /// The result of the operator.
            /// </returns>
            public static bool operator !=(CodeStart codeStart1, CodeStart codeStart2)
            {
                return !codeStart1.Equals(codeStart2);
            }

            /// <summary>
            /// Implements the operator ==.
            /// </summary>
            /// <param name="codeStart1">The first call info.</param>
            /// <param name="codeStart2">The second call info.</param>
            /// <returns>
            /// The result of the operator.
            /// </returns>
            public static bool operator ==(CodeStart codeStart1, CodeStart codeStart2)
            {
                return codeStart1.Equals(codeStart2);
            }

            /// <summary>
            /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
            /// </summary>
            /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
            /// <returns>
            ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
            /// </returns>
            public override bool Equals(object obj)
            {
                return
                    obj is CodeStart && obj != null &&
                    this.b00 == ((CodeStart)obj).b00 &&
                    this.b01 == ((CodeStart)obj).b01 &&
                    this.ul02 == ((CodeStart)obj).ul02 &&
                    this.b10 == ((CodeStart)obj).b10 &&
                    this.b11 == ((CodeStart)obj).b11 &&
                    this.b12 == ((CodeStart)obj).b12;
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
            /// </returns>
            public override int GetHashCode()
            {
                return this.ToString().GetHashCode();
            }

            /// <summary>
            /// Patches the call site with instructions.
            /// </summary>
            /// <param name="callSite">The call site.</param>
            /// <exception cref="System.NullReferenceException">
            /// Call info not defined
            /// or
            /// Call site not defined.
            /// </exception>
            public void PatchCallSite(IntPtr callSite)
            {
                if (this == CodeStart.Zero)
                {
                    throw new NullReferenceException("Call info not defined");
                }

                if (callSite == IntPtr.Zero)
                {
                    throw new NullReferenceException("Call site not defined");
                }

                unsafe
                {
                    byte* rawPointer = (byte*)callSite.ToPointer();

                    *rawPointer = this.b00;
                    *(rawPointer + 1) = this.b01;
                    *((ulong*)(rawPointer + 2)) = this.ul02;
                    *(rawPointer + 10) = this.b10;
                    *(rawPointer + 11) = this.b11;
                    *(rawPointer + 12) = this.b12;
                }
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return (new StringBuilder())
                        .Append(this.b00.ToString("X2"))
                        .Append(this.b01.ToString("X2"))
                        .Append(this.ul02.ToString("X16"))
                        .Append(this.b10.ToString("X2"))
                        .Append(this.b11.ToString("X2"))
                        .Append(this.b12.ToString("X2"))
                        .ToString();
            }
        }
    }
}