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
            try
            {
                Type baseType = instance.GetType().BaseType;

                if (baseType == null)
                {
                    return null;
                }

                MethodInfo methodInfo = baseType.GetMethod(methodName);
                if (methodInfo == null)
                {
                    return null;
                }

                return methodInfo.Invoke(instance, parameters);
            }
            catch (Exception ex)
            {
                Log.Error(instance, "BaseInvoke", ex, methodName);
                return null;
            }
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
        /// <returns>The comforming text.</returns>
        public static string ConformNewlines(this string text)
        {
            return Regex.Replace(text, "[\r\n]+", Environment.NewLine);
        }

        /// <summary>
        /// Conforms the newlines to the environment.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The comforming text.</returns>
        public static string ConformNewlines(this StringBuilder text)
        {
            return text.ToString().ConformNewlines();
        }

        /// <summary>
        /// Get the nets name.
        /// </summary>
        /// <param name="netInfo">The net information.</param>
        /// <returns>The name.</returns>
        public static string NetName(this NetInfo netInfo)
        {
            string name = netInfo.m_class.name;

            if (name == "Highway" && netInfo.name.Contains("Ramp"))
            {
                return "Highway Ramp";
            }

            if (name == "Highway Tunnel" && netInfo.name.Contains("Ramp"))
            {
                return "Highway Ramp Tunnel";
            }

            return name;
        }
    }
}