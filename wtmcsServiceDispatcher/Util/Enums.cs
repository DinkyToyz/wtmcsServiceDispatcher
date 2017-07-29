using System;
using System.Text;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Enum conversion utils.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal static class Enums<T> where T : struct, IConvertible
    {
        /// <summary>
        /// Initializes the <see cref="Enums{T}"/> class, to check that T is an enum.
        /// </summary>
        static Enums()
        {
            if (!typeof(T).IsEnum)
            {
                throw new InvalidCastException("Type  (" + typeof(T).ToString() + ") if not an enum");
            }
        }

        /// <summary>
        /// Converts the specified value to the enum type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The enum value.</returns>
        public static T Convert(object value)
        {
            if (!typeof(T).IsEnum)
            {
                throw new InvalidCastException("Type  (" + typeof(T).ToString() + ") if not an enum");
            }

            if (value is StringBuilder)
            {
                value = value.ToString();
            }

            if (value is string)
            {
                long longValue;
                if (long.TryParse((string)value, out longValue))
                {
                    return Convert(longValue);
                }

                ulong ulongValue;
                if (ulong.TryParse((string)value, out ulongValue))
                {
                    return Convert(ulongValue);
                }

                return (T)Enum.Parse(typeof(T), (string)value, true);
            }

            if (IsDefined(value))
            {
                throw new InvalidCastException("Cannot convert value (" + value.ToString() + ") to enum (" + typeof(T).ToString() + ")");
            }

            return (T)value;
        }

        /// <summary>
        /// Determines whether the specified value is defined as an enum element.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is defined; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDefined(object value)
        {
            return Enum.IsDefined(typeof(T), value);
        }

        /// <summary>
        /// Determines whether the specified value is convertible to an enum element.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValid(object value)
        {
            if (!typeof(T).IsEnum)
            {
                throw new InvalidCastException("Type  (" + typeof(T).ToString() + ") if not an enum");
            }

            if (value is StringBuilder)
            {
                value = value.ToString();
            }

            if (value is string)
            {
                long longValue;
                if (long.TryParse((string)value, out longValue))
                {
                    return IsValid(longValue);
                }

                ulong ulongValue;
                if (ulong.TryParse((string)value, out ulongValue))
                {
                    return IsValid(ulongValue);
                }

                try
                {
                    Enum.Parse(typeof(T), (string)value, true);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return IsDefined(value);
        }

        /// <summary>
        /// Tries to convert the specified value to the enum type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="result">The result.</param>
        /// <returns>True on success.</returns>
        public static bool TryConvert(object value, out T result)
        {
            if (!typeof(T).IsEnum)
            {
                throw new InvalidCastException("Type  (" + typeof(T).ToString() + ") if not an enum");
            }

            if (value is StringBuilder)
            {
                value = value.ToString();
            }

            if (value is string)
            {
                long longValue;
                if (long.TryParse((string)value, out longValue))
                {
                    return TryConvert(longValue, out result);
                }

                ulong ulongValue;
                if (ulong.TryParse((string)value, out ulongValue))
                {
                    return TryConvert(ulongValue, out result);
                }

                try
                {
                    result = (T)Enum.Parse(typeof(T), (string)value, true);
                    return true;
                }
                catch
                {
                    result = default(T);
                    return false;
                }
            }

            if (!IsDefined(value))
            {
                result = default(T);
                return false;
            }

            try
            {
                result = (T)value;
                return true;
            }
            catch
            {
                result = default(T);
                return false;
            }
        }
    }
}