// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Generics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// An enumerator capable of iterating over a generic dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the t key.</typeparam>
    /// <typeparam name="TValue">The type of the t value.</typeparam>
    public sealed class DictionaryEnumerator<TKey, TValue> : IDictionaryEnumerator, IDisposable
    {
        private readonly IEnumerator<KeyValuePair<TKey, TValue>> _impl;

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryEnumerator{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public DictionaryEnumerator(IDictionary<TKey, TValue> value)
        {
            _impl = value.GetEnumerator();
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public void Reset() => _impl.Reset();

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
        public bool MoveNext() => _impl.MoveNext();

        /// <summary>
        /// Gets both the key and the value of the current dictionary entry.
        /// </summary>
        /// <value>The entry.</value>
        public DictionaryEntry Entry
        {
            get
            {
                var pair = _impl.Current;
                return new DictionaryEntry(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Gets the key of the current dictionary entry.
        /// </summary>
        /// <value>The key.</value>
        public object Key => _impl.Current.Key;

        /// <summary>
        /// Gets the value of the current dictionary entry.
        /// </summary>
        /// <value>The value.</value>
        public object Value => _impl.Current.Value;

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <value>The current.</value>
        public object Current => this.Entry;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _impl?.Dispose();
        }
    }
}