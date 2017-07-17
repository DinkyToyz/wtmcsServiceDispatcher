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
        /// <typeparam name="TSource">The item type.</typeparam>
        /// <param name="source">The enumerable items.</param>
        /// <returns>The distinct items.</returns>
        public static IEnumerable<TSource> DistinctInOrder<TSource>(this IList<TSource> source)
        {
            int count = source.Count;
            HashSet<TSource> seen = new HashSet<TSource>();

            for (int i = 0; i < count; i++)
            {
                TSource item = source[i];

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
        /// <typeparam name="TSource">The item type.</typeparam>
        /// <param name="source">The enumerable items.</param>
        /// <returns>The distinct items.</returns>
        public static IEnumerable<TSource> DistinctInOrder<TSource>(this TSource[] source)
        {
            HashSet<TSource> seen = new HashSet<TSource>();

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
        /// <typeparam name="TSource">The item type.</typeparam>
        /// <param name="source">The enumerable items.</param>
        /// <returns>The distinct items.</returns>
        public static IEnumerable<TSource> DistinctInOrder<TSource>(this IEnumerable<TSource> source)
        {
            HashSet<TSource> seen = new HashSet<TSource>();

            foreach (TSource item in source)
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
        /// <typeparam name="TSource">The type of the elements.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <returns>
        /// default(T) if the source sequence is empty; otherwise, the last element.
        /// </returns>
        public static TSource OrderByLastOrDefault<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.ToList().OrderByLastOrDefault(keySelector);
        }

        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key and returns the last element of the sorted sequence, or a default value if the sequence contains no elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <returns>
        /// default(T) if the source sequence is empty; otherwise, the last element.
        /// </returns>
        public static TSource OrderByLastOrDefault<TSource, TKey>(this IList<TSource> source, Func<TSource, TKey> keySelector)
        {
            int count = source.Count;
            if (count < 1)
            {
                return default(TSource);
            }

            if (count == 1)
            {
                return source[0];
            }

            TSource[] items = new TSource[count];
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
        /// <typeparam name="TSource">The type of the elements.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <returns>
        /// default(T) if the source sequence is empty; otherwise, the last element.
        /// </returns>
        public static TSource OrderByLastOrDefault<TSource, TKey>(this TSource[] source, Func<TSource, TKey> keySelector)
        {
            if (source.Length < 1)
            {
                return default(TSource);
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
        /// <typeparam name="TSource">The type of the elements.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="count">The count.</param>
        /// <returns>
        /// The specified number of elements.
        /// </returns>
        public static IEnumerable<TSource> OrderByTake<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, int count)
        {
            return source.ToList().OrderByTake(keySelector, count);
        }

        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key and returns a specified number if contiguous elements from the start of the sorted sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="count">The count.</param>
        /// <returns>
        /// The specified number of elements.
        /// </returns>
        public static IEnumerable<TSource> OrderByTake<TSource, TKey>(this IList<TSource> source, Func<TSource, TKey> keySelector, int count)
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

            TSource[] items = new TSource[countSource];
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
        /// <typeparam name="TSource">The type of the elements.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="count">The count.</param>
        /// <returns>
        /// The specified number of elements.
        /// </returns>
        public static IEnumerable<TSource> OrderByTake<TSource, TKey>(this TSource[] source, Func<TSource, TKey> keySelector, int count)
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
        /// Projects each element in a sequence into a new form.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements.</typeparam>
        /// <typeparam name="TResult">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The projected elements.
        /// </returns>
        public static IEnumerable<TResult> Select<TSource, TResult>(this TSource[] source, Func<TSource, TResult> selector)
        {
            for (int i = 0; i < source.Length; i++)
            {
                yield return selector(source[i]);
            }
        }

        /// <summary>
        /// Projects each element in a sequence into a new form.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements.</typeparam>
        /// <typeparam name="TResult">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The projected elements.
        /// </returns>
        public static IEnumerable<TResult> Select<TSource, TResult>(this IList<TSource> source, Func<TSource, TResult> selector)
        {
            int count = source.Count;

            for (int i = 0; i < count; i++)
            {
                yield return selector(source[i]);
            }
        }

        /// <summary>
        /// Projects each element in a sequence into a new form and returns in an array if <typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements.</typeparam>
        /// <typeparam name="TResult">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The projected elements.</returns>
        public static TResult[] SelectToArray<TSource, TResult>(this TSource[] source, Func<TSource, TResult> selector)
        {
            TResult[] dest = new TResult[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                dest[i] = selector(source[i]);
            }

            return dest;
        }

        /// <summary>
        /// Projects each element in a sequence into a new form and returns in an array of <typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements.</typeparam>
        /// <typeparam name="TResult">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The projected elements.</returns>
        public static TResult[] SelectToArray<TSource, TResult>(this IList<TSource> source, Func<TSource, TResult> selector)
        {
            int count = source.Count;
            TResult[] dest = new TResult[count];

            for (int i = 0; i < count; i++)
            {
                dest[i] = selector(source[i]);
            }

            return dest;
        }

        /// <summary>
        /// Projects each element in a sequence into a new form and returns in an array of <typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements.</typeparam>
        /// <typeparam name="TResult">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The projected elements.</returns>
        public static TResult[] SelectToArray<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return source.SelectToList(selector).ToArray();
        }

        /// <summary>
        /// Projects each element in a sequence into a new form and returns in a List<typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements.</typeparam>
        /// <typeparam name="TResult">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The projected elements.</returns>
        public static List<TResult> SelectToList<TSource, TResult>(this TSource[] source, Func<TSource, TResult> selector)
        {
            List<TResult> dest = new List<TResult>(source.Length);

            for (int i = 0; i < source.Length; i++)
            {
                dest.Add(selector(source[i]));
            }

            return dest;
        }

        /// <summary>
        /// Projects each element in a sequence into a new form and returns in a List<typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements.</typeparam>
        /// <typeparam name="TResult">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The projected elements.</returns>
        public static List<TResult> SelectToList<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            List<TResult> dest = new List<TResult>();

            foreach (TSource item in source)
            {
                dest.Add(selector(item));
            }

            return dest;
        }

        /// <summary>
        /// Projects each element in a sequence into a new form and returns in a List<typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements.</typeparam>
        /// <typeparam name="TResult">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The projected elements.</returns>
        public static List<TResult> SelectToList<TSource, TResult>(this IList<TSource> source, Func<TSource, TResult> selector)
        {
            int count = source.Count;
            List<TResult> dest = new List<TResult>(count);

            for (int i = 0; i < count; i++)
            {
                dest.Add(selector(source[i]));
            }

            return dest;
        }

        /// <summary>
        /// Projects each element in a sequence into a new form and filters the values based on a predicate.
        /// </summary>
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <typeparam name="TResult">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>
        /// The projected and filtered sequence.
        /// </returns>
        public static IEnumerable<TResult> SelectWhere<TSource, TResult>(this TSource[] source, Func<TSource, TResult> selector, Func<TResult, bool> predicate)
        {
            for (int i = 0; i < source.Length; i++)
            {
                TResult item = selector(source[i]);

                if (predicate(item))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Projects each element in a sequence into a new form and filters the values based on a predicate.
        /// </summary>
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <typeparam name="TResult">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>
        /// The projected and filtered sequence.
        /// </returns>
        public static IEnumerable<TResult> SelectWhere<TSource, TResult>(this IList<TSource> source, Func<TSource, TResult> selector, Func<TResult, bool> predicate)
        {
            int count = source.Count;

            for (int i = 0; i < count; i++)
            {
                TResult item = selector(source[i]);

                if (predicate(item))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Projects each element in a sequence into a new form and filters the values based on a predicate.
        /// </summary>
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <typeparam name="TResult">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>
        /// The projected and filtered sequence.
        /// </returns>
        public static IEnumerable<TResult> SelectWhere<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, Func<TResult, bool> predicate)
        {
            foreach (TSource sourceItem in source)
            {
                TResult destItem = selector(sourceItem);

                if (predicate(destItem))
                {
                    yield return destItem;
                }
            }
        }

        /// <summary>
        /// Returns a specified number if contiguous elements from the start of the sequence and returns in an array of <typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="count">The count.</param>
        /// <returns>The specified number of elements.</returns>
        public static TSource[] TakeToArray<TSource>(this IEnumerable<TSource> source, int count)
        {
            return source.TakeToList(count).ToArray();
        }

        /// <summary>
        /// Returns a specified number if contiguous elements from the start of the sequence and returns in an array of <typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="count">The count.</param>
        /// <returns>The specified number of elements.</returns>
        public static TSource[] TakeToArray<TSource>(this IList<TSource> source, int count)
        {
            count = Math.Min(count, source.Count);
            TSource[] array = new TSource[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = source[i];
            }

            return array;
        }

        /// <summary>
        /// Returns a specified number if contiguous elements from the start of the sequence and returns in an array of <typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="count">The count.</param>
        /// <returns>The specified number of elements.</returns>
        public static TSource[] TakeToArray<TSource>(this TSource[] source, int count)
        {
            count = Math.Min(count, source.Length);
            TSource[] array = new TSource[count];

            for (int i = 0; i < count; i++)
            {
                array[i] = source[i];
            }

            return array;
        }

        /// <summary>
        /// Returns a specified number if coniguous elements from the start of the sequence and returns in a List<typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="count">The count.</param>
        /// <returns>The specified number of elements</returns>
        public static List<TSource> TakeToList<TSource>(this IEnumerable<TSource> source, int count)
        {
            int added = 0;
            List<TSource> list = new List<TSource>(count);

            foreach (TSource item in source)
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
        /// Returns a specified number if coniguous elements from the start of the sequence and returns in a List<typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="count">The count.</param>
        /// <returns>The specified number of elements</returns>
        public static List<TSource> TakeToList<TSource>(this IList<TSource> source, int count)
        {
            count = Math.Min(count, source.Count);
            List<TSource> list = new List<TSource>(count);

            for (int i = 0; i < count; i++)
            {
                list.Add(source[i]);
            }

            return list;
        }

        /// <summary>
        /// Returns a specified number if coniguous elements from the start of the sequence and returns in a List<typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="count">The count.</param>
        /// <returns>The specified number of elements</returns>
        public static List<TSource> TakeToList<TSource>(this TSource[] source, int count)
        {
            count = Math.Min(count, source.Length);
            List<TSource> list = new List<TSource>(count);

            for (int i = 0; i < count; i++)
            {
                list.Add(source[i]);
            }

            return list;
        }

        /// <summary>
        /// Creates an array of <typeparamref name="TSource"/> from an array of <typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>The list.</returns>
        public static TSource[] ToArray<TSource>(this TSource[] source)
        {
            return (TSource[])source.Clone();
        }

        /// <summary>
        /// Creates an array of <typeparamref name="TSource"/> from a List<typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>The list.</returns>
        public static TSource[] ToArray<TSource>(this IList<TSource> source)
        {
            int count = source.Count;
            TSource[] dest = new TSource[count];

            for (int i = 0; i < count; i++)
            {
                dest[i] = source[i];
            }

            return dest;
        }

        /// <summary>
        /// Creates a List<typeparamref name="TSource"/> from an array of <typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>The list.</returns>
        public static List<TSource> ToList<TSource>(this TSource[] source)
        {
            List<TSource> dest = new List<TSource>(source.Length);

            for (int i = 0; i < source.Length; i++)
            {
                dest.Add(source[i]);
            }

            return dest;
        }

        /// <summary>
        /// Creates a List<typeparamref name="TSource"/> from a List<typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>The list.</returns>
        public static List<TSource> ToList<TSource>(this IList<TSource> source)
        {
            int count = source.Count;
            List<TSource> dest = new List<TSource>(count);

            for (int i = 0; i < count; i++)
            {
                dest.Add(source[i]);
            }

            return dest;
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The filtered sequence.</returns>
        public static IEnumerable<TSource> Where<TSource>(this TSource[] source, Func<TSource, bool> predicate)
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
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The filtered sequence.</returns>
        public static IEnumerable<TSource> Where<TSource>(this IList<TSource> source, Func<TSource, bool> predicate)
        {
            int count = source.Count;

            for (int i = 0; i < count; i++)
            {
                TSource item = source[i];

                if (predicate(item))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate and projects each returned element into a new form.
        /// </summary>
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <typeparam name="TResult">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The filtered and projected sequence.
        /// </returns>
        public static IEnumerable<TResult> WhereSelect<TSource, TResult>(this TSource[] source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
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
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <typeparam name="TResult">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The filtered and projected sequence.
        /// </returns>
        public static IEnumerable<TResult> WhereSelect<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
        {
            foreach (TSource item in source)
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
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <typeparam name="TResult">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The filtered and projected sequence.
        /// </returns>
        public static IEnumerable<TResult> WhereSelect<TSource, TResult>(this IList<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
        {
            int count = source.Count;

            for (int i = 0; i < count; i++)
            {
                TSource item = source[i];

                if (predicate(item))
                {
                    yield return selector(item);
                }
            }
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate and projects each returned element into a new form and returns in an array of <typeparamref name="TResult"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <typeparam name="TResult">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The filtered and projected sequence.
        /// </returns>
        public static TResult[] WhereSelectToArray<TSource, TResult>(this TSource[] source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
        {
            return source.WhereSelectToList(predicate, selector).ToArray();
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate and projects each returned element into a new form and returns in an array of <typeparamref name="TResult"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <typeparam name="TResult">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The filtered and projected sequence.
        /// </returns>
        public static TResult[] WhereSelectToArray<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
        {
            return source.WhereSelectToList(predicate, selector).ToArray();
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate and projects each returned element into a new form and returns in an array of <typeparamref name="TResult"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <typeparam name="TResult">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The filtered and projected sequence.
        /// </returns>
        public static TResult[] WhereSelectToArray<TSource, TResult>(this IList<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
        {
            return source.WhereSelectToList(predicate, selector).ToArray();
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate and projects each returned element into a new form and returns in a List<typeparamref name="TResult"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <typeparam name="TResult">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The filtered and projected sequence.
        /// </returns>
        public static List<TResult> WhereSelectToList<TSource, TResult>(this TSource[] source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
        {
            List<TResult> list = new List<TResult>(source.Length);

            for (int i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    list.Add(selector(source[i]));
                }
            }

            return list;
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate and projects each returned element into a new form and returns in a List<typeparamref name="TResult"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <typeparam name="TResult">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The filtered and projected sequence.
        /// </returns>
        public static List<TResult> WhereSelectToList<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
        {
            List<TResult> list = new List<TResult>();

            foreach (TSource item in source)
            {
                if (predicate(item))
                {
                    list.Add(selector(item));
                }
            }

            return list;
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate and projects each returned element into a new form and returns in a List<typeparamref name="TResult"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <typeparam name="TResult">The type to project into.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The filtered and projected sequence.
        /// </returns>
        public static List<TResult> WhereSelectToList<TSource, TResult>(this IList<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
        {
            int count = source.Count;
            List<TResult> list = new List<TResult>(count);

            for (int i = 0; i < count; i++)
            {
                TSource item = source[i];

                if (predicate(item))
                {
                    list.Add(selector(item));
                }
            }

            return list;
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate and computes the sum of the sequence of long values that are obtained by invoking a transform function on each element.
        /// </summary>
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The sum of the filtered and projected values from the sequence.
        /// </returns>
        public static long WhereSum<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, Func<TSource, long> selector)
        {
            long sum = 0;

            foreach (TSource item in source)
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
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The sum of the filtered and projected values from the sequence.
        /// </returns>
        public static long WhereSum<TSource>(this IList<TSource> source, Func<TSource, bool> predicate, Func<TSource, long> selector)
        {
            int count = source.Count;
            long sum = 0;

            for (int i = 0; i < count; i++)
            {
                TSource item = source[i];

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
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>
        /// The sum of the filtered and projected values from the sequence.
        /// </returns>
        public static long WhereSum<TSource>(this TSource[] source, Func<TSource, bool> predicate, Func<TSource, long> selector)
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

        /// <summary>
        /// Filters a sequence of values based on a predicate and returns elements in an array of <typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The filtered sequence.</returns>
        public static TSource[] WhereToArray<TSource>(this IList<TSource> source, Func<TSource, bool> predicate)
        {
            return source.WhereToList(predicate).ToArray();
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate and returns elements in an array of <typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The filtered sequence.</returns>
        public static TSource[] WhereToArray<TSource>(this TSource[] source, Func<TSource, bool> predicate)
        {
            return source.WhereToList(predicate).ToArray();
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate and returns elements in an array of <typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The filtered sequence.</returns>
        public static TSource[] WhereToArray<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return source.WhereToList(predicate).ToArray();
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate and returns elements in a List<typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The filtered sequence.</returns>
        public static List<TSource> WhereToList<TSource>(this IList<TSource> source, Func<TSource, bool> predicate)
        {
            int count = source.Count;
            List<TSource> list = new List<TSource>(count);

            for (int i = 0; i < count; i++)
            {
                TSource item = source[i];

                if (predicate(item))
                {
                    list.Add(item);
                }
            }

            return list;
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate and returns elements in a List<typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The filtered sequence.</returns>
        public static List<TSource> WhereToList<TSource>(this TSource[] source, Func<TSource, bool> predicate)
        {
            List<TSource> list = new List<TSource>(source.Length);

            for (int i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    list.Add(source[i]);
                }
            }

            return list;
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate and returns elements in a List<typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The filtered sequence.</returns>
        public static List<TSource> WhereToList<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            List<TSource> list = new List<TSource>();

            foreach (TSource item in source)
            {
                if (predicate(item))
                {
                    list.Add(item);
                }
            }

            return list;
        }
    }
}