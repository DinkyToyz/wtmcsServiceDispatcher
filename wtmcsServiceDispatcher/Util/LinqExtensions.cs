using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    internal static class LinqExtensions
    {
        public static List<T> ToList<T>(this T[] source)
        {
            List<T> dest = new List<T>(source.Length);

            for (int i = 0; i < source.Length; i++)
            {
                dest.Add(source[i]);
            }

            return dest;
        }

        public static List<T> ToList<T>(this List<T> source)
        {
            List<T> dest = new List<T>(source.Count);

            for (int i = 0; i < source.Count; i++)
            {
                dest.Add(source[i]);
            }

            return dest;
        }

        public static T[] ToArray<T>(this T[] source)
        {
            T[] dest = new T[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                dest[i] = source[i];
            }

            return dest;
        }

        public static T[] ToArray<T>(this List<T> source)
        {
            T[] dest = new T[source.Count];

            for (int i = 0; i < source.Count; i++)
            {
                dest[i] = source[i];
            }

            return dest;
        }

        public static IEnumerable<T> Where<T>(this T[] source, Func<T, bool> predicate)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    yield return source[i];
                }
            }
        }

        public static IEnumerable<T> Where<T>(this List<T> source, Func<T, bool> predicate)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (predicate(source[i]))
                {
                    yield return source[i];
                }
            }
        }

        public static IEnumerable<TSelected> WhereSelect<T, TSelected>(this T[] source, Func<T, bool> predicate, Func<T, TSelected> selector)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    yield return selector(source[i]);
                }
            }
        }

        public static IEnumerable<TSelected> WhereSelect<T, TSelected>(this List<T> source, Func<T, bool> predicate, Func<T, TSelected> selector)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (predicate(source[i]))
                {
                    yield return selector(source[i]);
                }
            }
        }

        public static TSelected[] SelectToArray<T, TSelected>(this T[] source, Func<T, TSelected> selector)
        {
            TSelected[] dest = new TSelected[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                dest[i] = selector(source[i]);
            }

            return dest;
        }

        public static TSelected[] SelectToArray<T, TSelected>(this List<T> source, Func<T, TSelected> selector)
        {
            TSelected[] dest = new TSelected[source.Count];

            for (int i = 0; i < source.Count; i++)
            {
                dest[i] = selector(source[i]);
            }

            return dest;
        }

        /// <summary>
        /// Returns distinct items in ucnhanged order.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="source">The enumerable items.</param>
        /// <returns>The distinct items.</returns>
        public static IEnumerable<T> DistinctInOrder<T>(this List<T> source)
        {
            HashSet<T> seen = new HashSet<T>();

            for (int i = 0; i < source.Count; i++)
            {
                if (!seen.Contains(source[i]))
                {
                    seen.Add(source[i]);
                    yield return source[i];
                }
            }
        }
        /// <summary>
        /// Returns distinct items in ucnhanged order.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="source">The enumerable items.</param>
        /// <returns>The distinct items.</returns>
        public static IEnumerable<T> DistinctInOrder<T>(this T[] source)
        {
            HashSet<T> seen = new HashSet<T>();

            for (int i = 0; i < source.Length; i++)
            {
                if (!seen.Contains(source[i]))
                {
                    seen.Add(source[i]);
                    yield return source[i];
                }
            }
        }

        /// <summary>
        /// Returns distinct items in ucnhanged order.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="source">The enumerable items.</param>
        /// <returns>The distinct items.</returns>
        public static IEnumerable<T> DistinctInOrder<T>(this IEnumerable<T> source)
        {
            HashSet<T> seen = new HashSet<T>();

            foreach (T item in source)
            {
                if (!seen.Contains(item))
                {
                    seen.Add(item);
                    yield return item;
                }
            }
        }
    }
}
