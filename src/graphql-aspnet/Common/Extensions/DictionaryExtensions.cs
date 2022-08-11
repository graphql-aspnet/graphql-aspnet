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

    /// <summary>
    /// Helper methods for working with Extensions.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Invokes the supplied <paramref name="action"/> against each key/value pair in the dictionary
        /// and updates the key with the returned value.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TResult">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary to modify.</param>
        /// <param name="action">The action to invoke .</param>
        public static void ForEach<TKey, TResult>(this IDictionary<TKey, TResult> dictionary, Func<TKey, TResult, TResult> action)
        {
            Validation.ThrowIfNull(dictionary, nameof(dictionary));
            Validation.ThrowIfNull(action, nameof(action));

            var keys = new List<TKey>(dictionary.Keys);
            foreach (var key in keys)
            {
                var value = dictionary[key];
                dictionary[key] = action.Invoke(key, value);
            }
        }

        /// <summary>
        /// Invokes the supplied <paramref name="action"/> against each key/value pair of the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TResult">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary to modify.</param>
        /// <param name="action">The action to invoke .</param>
        public static void ForEach<TKey, TResult>(this IReadOnlyDictionary<TKey, TResult> dictionary, Action<TKey, TResult> action)
        {
            Validation.ThrowIfNull(dictionary, nameof(dictionary));
            Validation.ThrowIfNull(action, nameof(action));

            foreach (var key in dictionary.Keys)
            {
                var value = dictionary[key];
                action.Invoke(key, value);
            }
        }
    }
}