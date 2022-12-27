// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;

    /// <summary>
    /// An arbitrary dictionary of items.
    /// </summary>
    public sealed class MetaDataCollection : IEnumerable<KeyValuePair<string, object>>
    {
        // Authors Note:
        //
        // Kevin Carroll (11/2022)
        //
        // This class was originally a wrapper on ConcurrentDictionary
        // however, after a performance analysis, the speed at which this
        // object is created (at least 1 per request) performs 2 orders of magnitude
        // more allocations than any other object while mostly containing 0 objects
        // this was causing an enormous amount of GC pressure under load.
        //
        // It was decided to push this object back to a standard
        // dictionary with a locking mechanism.
        //
        // Given its scope (with a single request and only in user code)
        // its unlikely object will be under heavy pressure need the
        // speed of lock free reads provided by ConcurrentDictionary
        // ------
        private Dictionary<string, object> _localDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaDataCollection"/> class.
        /// </summary>
        public MetaDataCollection()
        {
            _localDictionary = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaDataCollection" /> class.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity.</param>
        public MetaDataCollection(int initialCapacity)
        {
            _localDictionary = new Dictionary<string, object>(initialCapacity);
        }

        /// <summary>
        /// Merges the provided collection into this one. Any existing keys in this instance are updated with their new values
        /// and non-existant keys are added to this instance.
        /// </summary>
        /// <param name="otherCollection">The other collection.</param>
        private void Merge(MetaDataCollection otherCollection)
        {
            if (otherCollection == null)
                return;

            lock (_localDictionary)
            {
                foreach (var kvp in otherCollection)
                {
                    this[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>
        /// Clones this instance into a new set of items.
        /// </summary>
        /// <returns>MetaDataCollection.</returns>
        public MetaDataCollection Clone()
        {
            var newCollection = new MetaDataCollection(this.Count);
            newCollection.Merge(this);
            return newCollection;
        }

        /// <summary>
        /// Adds the object to the collection with the specified key.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to associate with the <paramref name="key"/>.</param>
        public void Add(string key, object value)
        {
            lock (_localDictionary)
                _localDictionary.Add(key, value);
        }

        /// <summary>
        /// Determines whether the specified key exists in the collection.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the instance contains the specified key; otherwise, <c>false</c>.</returns>
        public bool ContainsKey(string key)
        {
            lock (_localDictionary)
                return _localDictionary.ContainsKey(key);
        }

        /// <summary>
        /// Removes the item specified by the given key from the collection.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the item was removed, <c>false</c> otherwise.</returns>
        public bool Remove(string key)
        {
            lock (_localDictionary)
                return _localDictionary.Remove(key);
        }

        /// <summary>
        /// Tries to retrieve a single value from the collection.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <param name="value">This parameter will be populated with the value if its found.</param>
        /// <returns><c>true</c> if the value was found, <c>false</c> otherwise.</returns>
        public bool TryGetValue(string key, out object value)
        {
            lock (_localDictionary)
                return _localDictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Tries to add the key with the specified value to the collection.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to associate with the <paramref name="key"/>.</param>
        /// <returns><c>true</c> if the item was successfully added, <c>false</c> otherwise.</returns>
        public bool TryAdd(string key, object value)
        {
            lock (_localDictionary)
            {
                if (_localDictionary.ContainsKey(key))
                    return false;

                _localDictionary.Add(key, value);
                return true;
            }
        }

        /// <summary>
        /// If the key already exists, its current value is returned. When the
        /// key does not exist, it is added with the supplied value and the supplied
        /// value is returned.
        /// </summary>
        /// <param name="key">The key to add or retrieve.</param>
        /// <param name="value">The value to associate with the key if
        /// it is added to the collection..</param>
        /// <returns>System.Object.</returns>
        public object GetOrAdd(string key, object value)
        {
            lock (_localDictionary)
            {
                if (_localDictionary.ContainsKey(key))
                    return _localDictionary[key];

                _localDictionary.Add(key, value);
                return value;
            }
        }

        /// <summary>
        /// If the key already exists, its current value is returned. When the
        /// key does not exist, the <paramref name="valueFactory" /> is invoked
        /// to create a new value. That created value is associated with the key
        /// then returned.
        /// </summary>
        /// <param name="key">The key to add or retrieve.</param>
        /// <param name="valueFactory">The value factory used to
        /// create a new value when it is to be added.</param>
        /// <returns>System.Object.</returns>
        public object GetOrAdd(string key, Func<string, object> valueFactory)
        {
            Validation.ThrowIfNull(valueFactory, nameof(valueFactory));

            lock (_localDictionary)
            {
                if (_localDictionary.ContainsKey(key))
                    return _localDictionary[key];

                var value = valueFactory(key);
                _localDictionary.Add(key, value);
                return value;
            }
        }

        /// <summary>
        /// Clears the items in this colleciton.
        /// </summary>
        public void Clear()
        {
            lock (_localDictionary)
                _localDictionary.Clear();
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            lock (_localDictionary)
            {
                return _localDictionary.GetEnumerator();
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Gets the number of entries in this collection.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                lock (_localDictionary)
                    return _localDictionary.Count;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="object"/> with the specified key.
        /// </summary>
        /// <param name="key">The key to associate to or look up.</param>
        /// <returns>System.Object.</returns>
        public object this[string key]
        {
            get
            {
                lock (_localDictionary)
                    return _localDictionary[key];
            }

            set
            {
                lock (_localDictionary)
                    _localDictionary[key] = value;
            }
        }
    }
}