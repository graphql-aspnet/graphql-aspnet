// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Useful extension methods for linq queries.
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// Returns the single source object as a single item enumerable collection.
        /// </summary>
        /// <typeparam name="TSource">The type of the source item.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>IEnumerable&lt;TSource&gt;.</returns>
        [DebuggerStepThrough]
        public static IEnumerable<TSource> AsEnumerable<TSource>(this TSource source)
        {
            yield return source;
        }

        /// <summary>
        /// Skips the last n number of elements in the sequence.
        /// </summary>
        /// <typeparam name="T">The type of item being iterated.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="n">The n.</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        [DebuggerStepThrough]
        public static IEnumerable<T> SkipLastN<T>(this IEnumerable<T> source, int n)
        {
            if (n < 0)
                throw new ArgumentException("Number of item skip from the end (N) must be greater than or equal to 0", nameof(n));

            return SkipLastNSet();
            IEnumerable<T> SkipLastNSet()
            {
                if (source == null)
                {
                    yield break;
                }

                var it = source.GetEnumerator();
                try
                {
                    bool hasRemainingItems = false;
                    var cache = new Queue<T>(n + 1);

                    do
                    {
                        hasRemainingItems = it.MoveNext();
                        if (hasRemainingItems)
                        {
                            cache.Enqueue(it.Current);
                            if (cache.Count > n)
                                yield return cache.Dequeue();
                        }
                    }
                    while (hasRemainingItems);
                }
                finally
                {
                    it.Dispose();
                }
            }
        }

        /// <summary>
        /// Returns an enumeration of the items that are not castable to the supplied type.
        /// </summary>
        /// <typeparam name="TOfType">Include all items of this type.</typeparam>
        /// <typeparam name="TButNotType">Exclude items of this type.</typeparam>
        /// <param name="list">The list.</param>
        /// <returns>IEnumerable&lt;TOfType&gt;.</returns>
        public static IEnumerable<TOfType> OfTypeButNotType<TOfType, TButNotType>(this IEnumerable<TOfType> list)
        {
            if (list == null)
                return Enumerable.Empty<TOfType>();

            return list.Where(x => !(x is TButNotType));
        }

        /// <summary>
        /// Returns true if there are at least <paramref name="count"/> number of items in the enumeration.
        /// </summary>
        /// <typeparam name="TType">The type of item in the list.</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="count">The count.</param>
        /// <returns><c>true</c> if true, if at least the provided amount of items exist, <c>false</c> otherwise.</returns>
        public static bool AnyCount<TType>(this IEnumerable<TType> list, int count)
        {
            if (count <= 0)
                return true;

            if (count == 1)
                return list.Any();

            return list.Skip(count - 1).Any();
        }
    }
}