using System;
using System.Collections.Generic;
using System.Linq;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Optimized Linq extensions.
    /// </summary>
    internal static class LinqExtensions
    {
        /// <summary>
        /// Returns distinct items in unchanged order.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="source">The enumerable items.</param>
        /// <returns>The distinct items.</returns>
        public static IEnumerable<T> DistinctInOrder<T>(this IList<T> source)
        {
            int count = source.Count;
            HashSet<T> seen = new HashSet<T>();

            for (int i = 0; i < count; i++)
            {
                T item = source[i];

                if (!seen.Contains(item))
                {
                    seen.Add(item);
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Returns distinct items in unchanged order.
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
        /// Returns distinct items in unchanged order.
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

        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key and returns the last element of the sorted sequence, or a default value if the sequence contains no elements.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <returns>
        /// default(T) if the source sequence is empty; otherwise, the last element.
        /// </returns>
        public static T OrderByLastOrDefault<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            return source.ToList().OrderByLastOrDefault(keySelector);
        }

        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key and returns the last element of the sorted sequence, or a default value if the sequence contains no elements.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <returns>
        /// default(T) if the source sequence is empty; otherwise, the last element.
        /// </returns>
        public static T OrderByLastOrDefault<T, TKey>(this IList<T> source, Func<T, TKey> keySelector)
        {
            int count = source.Count;
            if (count < 1)
            {
                return default(T);
            }

            if (count == 1)
            {
                return source[0];
            }

            T[] items = new T[count];
            TKey[] keys = new TKey[count];

            for (int i = 0; i < count; i++)
            {
                items[i] = source[i];
                keys[i] = keySelector(source[i]);
            }

            Array.Sort(keys, items);

            return source[count - 1];
        }

        /// <summary>
        /// Sorts the elements of a sequence in place in ascending order according to a key and returns the last element of the sorted sequence, or a default value if the sequence contains no elements.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <returns>
        /// default(T) if the source sequence is empty; otherwise, the last element.
        /// </returns>
        public static T OrderByLastOrDefault<T, TKey>(this T[] source, Func<T, TKey> keySelector)
        {
            if (source.Length < 1)
            {
                return default(T);
            }

            if (source.Length == 1)
            {
                return source[0];
            }

            TKey[] keys = new TKey[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                keys[i] = keySelector(source[i]);
            }

            Array.Sort(keys, source);

            return source[source.Length - 1];
        }

        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key and returns a specified number if contiguous elements from the start of the sorted sequence.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="count">The count.</param>
        /// <returns>
        /// The specified number of elements.
        /// </returns>
        public static IEnumerable<T> OrderByTake<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, int count)
        {
            return source.ToList().OrderByTake(keySelector, count);
        }

        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key and returns a specified number if contiguous elements from the start of the sorted sequence.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="count">The count.</param>
        /// <returns>
        /// The specified number of elements.
        /// </returns>
        public static IEnumerable<T> OrderByTake<T, TKey>(this IList<T> source, Func<T, TKey> keySelector, int count)
        {
            if (count < 1)
            {
                yield break;
            }

            int countSource = source.Count;
            if (countSource < 1)
            {
                yield break;
            }

            if (countSource == 1 || count == 1)
            {
                yield return source[0];
                yield break;
            }

            T[] items = new T[countSource];
            TKey[] keys = new TKey[countSource];

            for (int i = 0; i < countSource; i++)
            {
                items[i] = source[i];
                keys[i] = keySelector(source[i]);
            }

            Array.Sort(keys, items);

            count = Math.Min(count, countSource);
            for (int i = 0; i < count; i++)
            {
                yield return items[i];
            }
        }

        /// <summary>
        /// Sorts the elements of a sequence in place in ascending order according to a key and returns a specified number if contiguous elements from the start of the sorted sequence.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="count">The count.</param>
        /// <returns>
        /// The specified number of elements.
        /// </returns>
        public static IEnumerable<T> OrderByTake<T, TKey>(this T[] source, Func<T, TKey> keySelector, int count)
        {
            if (count < 1 || source.Length < 1)
            {
                yield break;
            }

            if (source.Length == 1 || count == 1)
            {
                yield return source[0];
                yield break;
            }

            TKey[] keys = new TKey[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                keys[i] = keySelector(source[i]);
            }

            Array.Sort(keys, source);

            count = Math.Min(count, source.Length);
            for (int i = 0; i < count; i++)
            {
                yield return source[i];
            }
        }

        /// <summary>
        /// Projects each element in a sequence into a new form and returns in an array if <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <typeparam name="TSelected">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The projected elements.</returns>
        public static TSelected[] SelectToArray<T, TSelected>(this T[] source, Func<T, TSelected> selector)
        {
            TSelected[] dest = new TSelected[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                dest[i] = selector(source[i]);
            }

            return dest;
        }

        /// <summary>
        /// Projects each element in a sequence into a new form and returns in an array of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <typeparam name="TSelected">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The projected elements.</returns>
        public static TSelected[] SelectToArray<T, TSelected>(this IList<T> source, Func<T, TSelected> selector)
        {
            int count = source.Count;
            TSelected[] dest = new TSelected[count];

            for (int i = 0; i < count; i++)
            {
                dest[i] = selector(source[i]);
            }

            return dest;
        }

        /// <summary>
        /// Projects each element in a sequence into a new form and returns in an array of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <typeparam name="TSelected">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The projected elements.</returns>
        public static TSelected[] SelectToArray<T, TSelected>(this IEnumerable<T> source, Func<T, TSelected> selector)
        {
            return source.SelectToList(selector).ToArray();
        }

        /// <summary>
        /// Projects each element in a sequence into a new form and returns in a IList<typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <typeparam name="TSelected">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The projected elements.</returns>
        public static IList<TSelected> SelectToList<T, TSelected>(this T[] source, Func<T, TSelected> selector)
        {
            List<TSelected> dest = new List<TSelected>(source.Length);

            for (int i = 0; i < source.Length; i++)
            {
                dest.Add(selector(source[i]));
            }

            return dest;
        }

        /// <summary>
        /// Projects each element in a sequence into a new form and returns in a IList<typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <typeparam name="TSelected">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The projected elements.</returns>
        public static IList<TSelected> SelectToList<T, TSelected>(this IEnumerable<T> source, Func<T, TSelected> selector)
        {
            List<TSelected> dest = new List<TSelected>();

            foreach (T item in source)
            {
                dest.Add(selector(item));
            }

            return dest;
        }

        /// <summary>
        /// Projects each element in a sequence into a new form and returns in a List<typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <typeparam name="TSelected">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The projected elements.</returns>
        public static IList<TSelected> SelectToList<T, TSelected>(this IList<T> source, Func<T, TSelected> selector)
        {
            int count = source.Count;
            List<TSelected> dest = new List<TSelected>(count);

            for (int i = 0; i < count; i++)
            {
                dest.Add(selector(source[i]));
            }

            return dest;
        }

        /// <summary>
        /// Projects each element in a sequence into a new form and filters the values based on a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <typeparam name="TSelected">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>
        /// The projected and filtered sequence.
        /// </returns>
        public static IEnumerable<TSelected> SelectWhere<T, TSelected>(this T[] source, Func<T, TSelected> selector, Func<TSelected, bool> predicate)
        {
            for (int i = 0; i < source.Length; i++)
            {
                TSelected item = selector(source[i]);

                if (predicate(item))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Projects each element in a sequence into a new form and filters the values based on a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <typeparam name="TSelected">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>
        /// The projected and filtered sequence.
        /// </returns>
        public static IEnumerable<TSelected> SelectWhere<T, TSelected>(this IList<T> source, Func<T, TSelected> selector, Func<TSelected, bool> predicate)
        {
            int count = source.Count;

            for (int i = 0; i < count; i++)
            {
                TSelected item = selector(source[i]);

                if (predicate(item))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Projects each element in a sequence into a new form and filters the values based on a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <typeparam name="TSelected">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>
        /// The projected and filtered sequence.
        /// </returns>
        public static IEnumerable<TSelected> SelectWhere<T, TSelected>(this IEnumerable<T> source, Func<T, TSelected> selector, Func<TSelected, bool> predicate)
        {
            foreach (T sourceItem in source)
            {
                TSelected destItem = selector(sourceItem);

                if (predicate(destItem))
                {
                    yield return destItem;
                }
            }
        }

        /// <summary>
        /// Returns a specified number if contiguous elements from the start of the sequence and returns in an array of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="count">The count.</param>
        /// <returns>The specified number of elements.</returns>
        public static T[] TakeToArray<T>(this IEnumerable<T> source, int count)
        {
            return source.TakeToList(count).ToArray();
        }

        /// <summary>
        /// Returns a specified number if contiguous elements from the start of the sequence and returns in an array of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="count">The count.</param>
        /// <returns>The specified number of elements.</returns>
        public static T[] TakeToArray<T>(this IList<T> source, int count)
        {
            count = Math.Min(count, source.Count);
            T[] array = new T[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = source[i];
            }

            return array;
        }

        /// <summary>
        /// Returns a specified number if contiguous elements from the start of the sequence and returns in an array of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="count">The count.</param>
        /// <returns>The specified number of elements.</returns>
        public static T[] TakeToArray<T>(this T[] source, int count)
        {
            count = Math.Min(count, source.Length);
            T[] array = new T[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = source[i];
            }

            return array;
        }

        /// <summary>
        /// Returns a specified number if coniguous elements from the start of the sequence and returns in a List<typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="count">The count.</param>
        /// <returns>The specified number of elements</returns>
        public static IList<T> TakeToList<T>(this IEnumerable<T> source, int count)
        {
            int added = 0;
            List<T> list = new List<T>(count);

            foreach (T item in source)
            {
                list.Add(item);

                added++;
                if (added >= count)
                {
                    break;
                }
            }

            return list;
        }

        /// <summary>
        /// Returns a specified number if coniguous elements from the start of the sequence and returns in a List<typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="count">The count.</param>
        /// <returns>The specified number of elements</returns>
        public static IList<T> TakeToList<T>(this IList<T> source, int count)
        {
            count = Math.Min(count, source.Count);
            List<T> list = new List<T>(count);

            for (int i = 0; i < count; i++)
            {
                list.Add(source[i]);
            }

            return list;
        }

        /// <summary>
        /// Returns a specified number if coniguous elements from the start of the sequence and returns in a List<typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="count">The count.</param>
        /// <returns>The specified number of elements</returns>
        public static IList<T> TakeToList<T>(this T[] source, int count)
        {
            count = Math.Min(count, source.Length);
            List<T> list = new List<T>(count);

            for (int i = 0; i < count; i++)
            {
                list.Add(source[i]);
            }

            return list;
        }

        /// <summary>
        /// Creates an array of <typeparamref name="T"/> from an array of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>The list.</returns>
        public static T[] ToArray<T>(this T[] source)
        {
            return (T[])source.Clone();
        }

        /// <summary>
        /// Creates an array of <typeparamref name="T"/> from a List<typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>The list.</returns>
        public static T[] ToArray<T>(this IList<T> source)
        {
            int count = source.Count;
            T[] dest = new T[count];

            for (int i = 0; i < count; i++)
            {
                dest[i] = source[i];
            }

            return dest;
        }

        /// <summary>
        /// Creates a List<typeparamref name="T"/> from an array of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>The list.</returns>
        public static IList<T> ToList<T>(this T[] source)
        {
            List<T> dest = new List<T>(source.Length);

            for (int i = 0; i < source.Length; i++)
            {
                dest.Add(source[i]);
            }

            return dest;
        }

        /// <summary>
        /// Creates a List<typeparamref name="T"/> from a List<typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>The list.</returns>
        public static IList<T> ToList<T>(this IList<T> source)
        {
            int count = source.Count;
            List<T> dest = new List<T>(count);

            for (int i = 0; i < count; i++)
            {
                dest.Add(source[i]);
            }

            return dest;
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The filtered sequence.</returns>
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

        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The filtered sequence.</returns>
        public static IEnumerable<T> Where<T>(this IList<T> source, Func<T, bool> predicate)
        {
            int count = source.Count;

            for (int i = 0; i < count; i++)
            {
                T item = source[i];

                if (predicate(item))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate and projects each returned element into a new form.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <typeparam name="TSelected">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The filtered and projected sequence.
        /// </returns>
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

        /// <summary>
        /// Filters a sequence of values based on a predicate and projects each returned element into a new form.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <typeparam name="TSelected">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The filtered and projected sequence.
        /// </returns>
        public static IEnumerable<TSelected> WhereSelect<T, TSelected>(this IEnumerable<T> source, Func<T, bool> predicate, Func<T, TSelected> selector)
        {
            foreach (T item in source)
            {
                if (predicate(item))
                {
                    yield return selector(item);
                }
            }
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate and projects each returned element into a new form.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <typeparam name="TSelected">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The filtered and projected sequence.
        /// </returns>
        public static IEnumerable<TSelected> WhereSelect<T, TSelected>(this IList<T> source, Func<T, bool> predicate, Func<T, TSelected> selector)
        {
            int count = source.Count;

            for (int i = 0; i < count; i++)
            {
                T item = source[i];

                if (predicate(item))
                {
                    yield return selector(item);
                }
            }
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate and computes the sum of the sequence of long values that are obtained by invoking a transform function on each element.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The sum of the filtered and projected values from the sequence.
        /// </returns>
        public static long WhereSum<T>(this IEnumerable<T> source, Func<T, bool> predicate, Func<T, long> selector)
        {
            long sum = 0;

            foreach (T item in source)
            {
                if (predicate(item))
                {
                    sum += selector(item);
                }
            }

            return sum;
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate and computes the sum of the sequence of long values that are obtained by invoking a transform function on each element.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The sum of the filtered and projected values from the sequence.
        /// </returns>
        public static long WhereSum<T>(this IList<T> source, Func<T, bool> predicate, Func<T, long> selector)
        {
            int count = source.Count;
            long sum = 0;

            for (int i = 0; i < count; i++)
            {
                T item = source[i];

                if (predicate(item))
                {
                    sum += selector(item);
                }
            }

            return sum;
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate and computes the sum of the sequence of long values that are obtained by invoking a transform function on each element.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The sum of the filtered and projected values from the sequence.
        /// </returns>
        public static long WhereSum<T>(this T[] source, Func<T, bool> predicate, Func<T, long> selector)
        {
            long sum = 0;

            for (int i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    sum += selector(source[i]);
                }
            }

            return sum;
        }
    }
}