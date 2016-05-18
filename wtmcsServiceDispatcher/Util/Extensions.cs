using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Type extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Get only ASCII capitals.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The ASCII capitals.</returns>
        public static string ASCIICapitals(this string text)
        {
            return Regex.Replace(text, "[^A-Z]", "");
        }

        /// <summary>
        /// Invokes method in base class.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The return object.</returns>
        public static object BaseInvoke(this object instance, string methodName, object[] parameters)
        {
            Type baseType = instance.GetType().BaseType;

            if (baseType == null)
            {
                throw new MethodAccessException("Base type not found");
            }

            MethodInfo methodInfo = baseType.GetMethod(methodName);
            if (methodInfo == null)
            {
                throw new MethodAccessException("Base method not found");
            }

            return methodInfo.Invoke(instance, parameters);
        }

        /// <summary>
        /// Casts object to type.
        /// </summary>
        /// <typeparam name="T">Type to cast to.</typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>Cast object.</returns>
        public static object CastTo<T>(this object obj)
        {
            try
            {
                return (T)obj;
            }
            catch
            {
                return obj;
            }
        }

        /// <summary>
        /// Casts object to type.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="type">The type.</param>
        /// <returns>Cast object.</returns>
        public static object CastTo(this object obj, Type type)
        {
            MethodInfo castMethod = obj.GetType().GetMethod("CastTo").MakeGenericMethod(type);
            return castMethod.Invoke(null, new object[] { obj });
        }

        /// <summary>
        /// Casts object to base class.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Cast object.</returns>
        public static object CastToBase(this object obj)
        {
            return obj.CastTo(obj.GetType().BaseType);
        }

        /// <summary>
        /// Cleans the newlines.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The clean text.</returns>
        public static string CleanNewLines(this string text)
        {
            return Regex.Replace(text, "[\r\n]+", "\n");
        }

        /// <summary>
        /// Cleans the newlines.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The clean text.</returns>
        public static string CleanNewLines(this StringBuilder text)
        {
            return text.ToString().CleanNewLines();
        }

        /// <summary>
        /// Conforms the newlines to the environment.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The conforming text.</returns>
        public static string ConformNewlines(this string text)
        {
            return Regex.Replace(text, "[\r\n]+", Environment.NewLine);
        }

        /// <summary>
        /// Conforms the newlines to the environment.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The conforming text.</returns>
        public static string ConformNewlines(this StringBuilder text)
        {
            return text.ToString().ConformNewlines();
        }
    }
}