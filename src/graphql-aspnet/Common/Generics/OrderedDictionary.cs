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
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common.Generics.Interfaces;

    /// <summary>
    /// A dictionary object that allows rapid hash lookups using keys, but also
    /// maintains the key insertion order so that values can be retrieved by
    /// key index.
    /// Adapted from:
    /// https://stackoverflow.com/a/9844528
    /// https://github.com/mattmc3/dotmore/blob/master/dotmore/Collections/Generic/OrderedDictionary.cs .
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(OrderedDictionaryDebugBrowser))]
    public class OrderedDictionary<TKey, TValue> : IOrderedDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        private DelegateKeyedCollection<TKey, KeyValuePair<TKey, TValue>> _keyedCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}"/> class.
        /// </summary>
        public OrderedDictionary()
        {
            this.Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public OrderedDictionary(IEqualityComparer<TKey> comparer)
        {
            this.Initialize(comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        public OrderedDictionary(IOrderedDictionary<TKey, TValue> dictionary)
        {
            this.Initialize();
            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                _keyedCollection.Add(pair);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="comparer">The comparer.</param>
        public OrderedDictionary(IOrderedDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            this.Initialize(comparer);
            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                _keyedCollection.Add(pair);
            }
        }

        /// <summary>
        /// Initializes the dictionary with the given comparer.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        private void Initialize(IEqualityComparer<TKey> comparer = null)
        {
            this.Comparer = comparer;
            if (comparer != null)
            {
                _keyedCollection = new DelegateKeyedCollection<TKey, KeyValuePair<TKey, TValue>>(x => x.Key, comparer);
            }
            else
            {
                _keyedCollection = new DelegateKeyedCollection<TKey, KeyValuePair<TKey, TValue>>(x => x.Key);
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key associated with the value to get or set.</param>
        public TValue this[TKey key]
        {
            get
            {
                return this.GetValue(key);
            }

            set
            {
                this.SetValue(key, value);
            }
        }

        /// <summary>
        /// Gets the collection of keys in this dictionary.
        /// </summary>
        /// <value>The keys.</value>
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => this.Keys;

        /// <summary>
        /// Gets the collection of values in this dictionary.
        /// </summary>
        /// <value>The values.</value>
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => this.Values;

        /// <summary>
        /// Gets or sets the value at the specified index.
        /// </summary>
        /// <param name="index">The index of the value to get or set.</param>
        public TValue this[int index]
        {
            get
            {
                return this.GetItem(index).Value;
            }

            set
            {
                this.SetItem(index, value);
            }
        }

        /// <summary>
        /// Gets the count of items in the dictionary.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _keyedCollection.Count;

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets the keys of the dictionary as a collection.
        /// </summary>
        /// <value>The keys.</value>
        public ICollection<TKey> Keys => _keyedCollection.Keys ?? new TKey[0];

        /// <summary>
        /// Gets the values of the dictionary as a read only collection.
        /// </summary>
        /// <value>The values.</value>
        public ICollection<TValue> Values => _keyedCollection.Values?.Select(x => x.Value).ToList() as ICollection<TValue> ?? new TValue[0];

        /// <summary>
        /// Adds an element with the provided key and value to the dictionary.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        public void Add(TKey key, TValue value)
        {
            _keyedCollection.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            _keyedCollection.Clear();
        }

        /// <summary>
        /// Inserts an item at the indexed location with the provided key.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Insert(int index, TKey key, TValue value)
        {
            _keyedCollection.Insert(index, new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>
        /// Retrieves the index of the given key. Returns -1 if the key is not found.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>System.Int32.</returns>
        public int IndexOf(TKey key)
        {
            if (_keyedCollection.Contains(key))
            {
                return _keyedCollection.IndexOf(_keyedCollection[key]);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Determines whether the specified value is contained in this dictionary.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <returns><c>true</c> if the specified value contains value; otherwise, <c>false</c>.</returns>
        public bool ContainsValue(TValue value)
        {
            return this.Values.Contains(value);
        }

        /// <summary>
        /// Determines whether the specified value is contained in this dictionary using the provied comparer.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns><c>true</c> if the specified value contains value; otherwise, <c>false</c>.</returns>
        public bool ContainsValue(TValue value, IEqualityComparer<TValue> comparer)
        {
            return this.Values.Contains(value, comparer);
        }

        /// <summary>
        /// Determines whether the dictionary contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the dictionary.</param>
        /// <returns>true if the dictionary contains an element with the key; otherwise, false.</returns>
        public bool ContainsKey(TKey key)
        {
            return _keyedCollection.Contains(key);
        }

        /// <summary>
        /// Gets the item found at the provided index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>KeyValuePair&lt;TKey, TValue&gt;.</returns>
        public KeyValuePair<TKey, TValue> GetItem(int index)
        {
            if (index < 0 || index >= _keyedCollection.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "The index was outside the bounds of the dictionary");
            }

            return _keyedCollection[index];
        }

        /// <summary>
        /// Sets the value at the index specified.
        /// </summary>
        /// <param name="index">The index of the value desired.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the index specified does not refer to a KeyValuePair in this object.
        /// </exception>
        public void SetItem(int index, TValue value)
        {
            if (index < 0 || index >= _keyedCollection.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "The index was outside the bounds of the dictionary");
            }

            var kvp = new KeyValuePair<TKey, TValue>(_keyedCollection[index].Key, value);
            _keyedCollection[index] = kvp;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _keyedCollection.GetEnumerator();
        }

        /// <summary>
        /// Removes the element with the specified key from the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>true if the element is successfully removed; otherwise, false.
        /// This method also returns false if <paramref name="key">key</paramref> was not found in the
        /// original dictionary.</returns>
        public bool Remove(TKey key)
        {
            return _keyedCollection.Remove(key);
        }

        /// <summary>
        /// Removes the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _keyedCollection.Count)
            {
                throw new ArgumentException($"The index was outside the bounds of the dictionary: {index}");
            }

            _keyedCollection.RemoveAt(index);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key associated with the value to get.</param>
        /// <returns>TValue.</returns>
        public TValue GetValue(TKey key)
        {
            if (!_keyedCollection.Contains(key))
            {
                throw new ArgumentException($"The given key is not present in the dictionary: {key}");
            }

            var kvp = _keyedCollection[key];
            return kvp.Value;
        }

        /// <summary>
        /// Sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key associated with the value to set.</param>
        /// <param name="value">The the value to set.</param>
        public void SetValue(TKey key, TValue value)
        {
            var kvp = new KeyValuePair<TKey, TValue>(key, value);
            var idx = this.IndexOf(key);
            if (idx > -1)
            {
                _keyedCollection[idx] = kvp;
            }
            else
            {
                _keyedCollection.Add(kvp);
            }
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key,
        /// if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns>true if the object that implements dictionary contains an element
        /// with the specified key; otherwise, false.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_keyedCollection.Contains(key))
            {
                value = _keyedCollection[key].Value;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        /// <summary>
        /// Adds an element with the provided key and value to the dictionary.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        void IDictionary<TKey, TValue>.Add(TKey key, TValue value) => this.Add(key, value);

        /// <summary>
        /// Determines whether the dictionary contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the dictionary.</param>
        /// <returns>true if the dictionary contains an element with the key; otherwise, false.</returns>
        bool IDictionary<TKey, TValue>.ContainsKey(TKey key) => this.ContainsKey(key);

        /// <summary>
        /// Gets the collection of keys in this dictionary.
        /// </summary>
        /// <value>The keys.</value>
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => this.Keys;

        /// <summary>
        /// Removes the element with the specified key from the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key">key</paramref> was not found in the original dictionary.</returns>
        bool IDictionary<TKey, TValue>.Remove(TKey key) => this.Remove(key);

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns>true if the object that implements dictionary contains an element with the specified key; otherwise, false.</returns>
        bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value) => this.TryGetValue(key, out value);

        /// <summary>
        /// Gets the collection of values in this dictionary.
        /// </summary>
        /// <value>The values.</value>
        ICollection<TValue> IDictionary<TKey, TValue>.Values => this.Values;

        /// <summary>
        /// Gets or sets the value with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>TValue.</returns>
        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get => this[key];
            set => this[key] = value;
        }

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The object to add to the collection.</param>
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => _keyedCollection.Add(item);

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        void ICollection<KeyValuePair<TKey, TValue>>.Clear() => _keyedCollection.Clear();

        /// <summary>
        /// Determines whether the collection contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the collection.</param>
        /// <returns>true if <paramref name="item">item</paramref> is found in the collection; otherwise, false.</returns>
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
            => _keyedCollection.Contains(item);

        /// <summary>
        /// Copies the elements of the collection to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from collection. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            => _keyedCollection.CopyTo(array, arrayIndex);

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        int ICollection<KeyValuePair<TKey, TValue>>.Count => _keyedCollection.Count;

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => this.IsReadOnly;

        /// <summary>
        /// Removes the first occurrence of a specific object from the collection.
        /// </summary>
        /// <param name="item">The object to remove from the collection.</param>
        /// <returns>true if <paramref name="item">item</paramref> was successfully removed from the collection; otherwise, false. This method also returns false if <paramref name="item">item</paramref> is not found in the original collection.</returns>
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => _keyedCollection.Remove(item);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the ordered dictionary collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IDictionaryEnumerator"></see> for the entire ordered dictionary collection.</returns>
        IDictionaryEnumerator IOrderedDictionary.GetEnumerator() => new DictionaryEnumerator<TKey, TValue>(this);

        /// <summary>
        /// Inserts a key/value pair into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the key/value pair should be inserted.</param>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.  The value can be null.</param>
        void IOrderedDictionary.Insert(int index, object key, object value)
            => this.Insert(index, (TKey)key, (TValue)value);

        /// <summary>
        /// Removes the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        void IOrderedDictionary.RemoveAt(int index) => this.RemoveAt(index);

        /// <summary>
        /// Gets or sets the object at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>System.Object.</returns>
        object IOrderedDictionary.this[int index]
        {
            get => this[index];
            set => this[index] = (TValue)value;
        }

        /// <summary>
        /// Adds an element with the provided key and value to the dictionary object.
        /// </summary>
        /// <param name="key">The <see cref="T:System.Object"></see> to use as the key of the element to add.</param>
        /// <param name="value">The <see cref="T:System.Object"></see> to use as the value of the element to add.</param>
        void IDictionary.Add(object key, object value) => this.Add((TKey)key, (TValue)value);

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        void IDictionary.Clear() => this.Clear();

        /// <summary>
        /// Determines whether the dictionary object contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the dictionary object.</param>
        /// <returns>true if the dictionary contains an element with the key; otherwise, false.</returns>
        bool IDictionary.Contains(object key) => _keyedCollection.Contains((TKey)key);

        /// <summary>
        /// Returns an enumerator that iterates through the ordered dictionary collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IDictionaryEnumerator"></see> for the entire ordered dictionary collection.</returns>
        IDictionaryEnumerator IDictionary.GetEnumerator() => new DictionaryEnumerator<TKey, TValue>(this);

        /// <summary>
        /// Gets a value indicating whether the dictionary object has a fixed size.
        /// </summary>
        /// <value><c>true</c> if this instance is fixed size; otherwise, <c>false</c>.</value>
        bool IDictionary.IsFixedSize => false;

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
        bool IDictionary.IsReadOnly => false;

        /// <summary>
        /// Gets the collection of keys in this dictionary.
        /// </summary>
        /// <value>The keys.</value>
        ICollection IDictionary.Keys => (ICollection)this.Keys;

        /// <summary>
        /// Removes the element with the specified key from the dictionary object.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        void IDictionary.Remove(object key) => this.Remove((TKey)key);

        /// <summary>
        /// Gets the collection of values in this dictionary.
        /// </summary>
        /// <value>The values.</value>
        ICollection IDictionary.Values => (ICollection)this.Values;

        /// <summary>
        /// Gets or sets the object with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>System.Object.</returns>
        object IDictionary.this[object key]
        {
            get => this[(TKey)key];
            set => this[(TKey)key] = (TValue)value;
        }

        /// <summary>
        /// Copies the elements of the collection to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from collection. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        void ICollection.CopyTo(Array array, int index) => ((ICollection)_keyedCollection).CopyTo(array, index);

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        int ICollection.Count => ((ICollection)_keyedCollection).Count;

        /// <summary>
        /// Gets a value indicating whether access to the collection is synchronized (thread safe).
        /// </summary>
        /// <value><c>true</c> if this instance is synchronized; otherwise, <c>false</c>.</value>
        bool ICollection.IsSynchronized => ((ICollection)_keyedCollection).IsSynchronized;

        /// <summary>
        /// Gets an object that can be used to synchronize access to the collection.
        /// </summary>
        /// <value>The synchronize root.</value>
        object ICollection.SyncRoot => ((ICollection)_keyedCollection).SyncRoot;

        /// <summary>
        /// Sorts the values by the default <see cref="Comparer"/>.
        /// </summary>
        public void SortKeys() => _keyedCollection.SortByKeys();

        /// <summary>
        /// Sorts and reorders the values of this dictionary by the provided comparer item.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public void SortKeys(IComparer<TKey> comparer) => _keyedCollection.SortByKeys(comparer);

        /// <summary>
        /// Sorts and reorders the keys of this dictionary by the provided comparison item.
        /// </summary>
        /// <param name="comparison">The comparison.</param>
        public void SortKeys(Comparison<TKey> comparison) => _keyedCollection.SortByKeys(comparison);

        /// <summary>
        /// Sorts the values by the default <see cref="Comparer"/>.
        /// </summary>
        public void SortValues() => this.SortValues(Comparer<TValue>.Default);

        /// <summary>
        /// Sorts and reorders the values of this dictionary by the provided comparer item.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public void SortValues(IComparer<TValue> comparer) => _keyedCollection.Sort((x, y) => comparer.Compare(x.Value, y.Value));

        /// <summary>
        /// Sorts and reorders the values of this dictionary by the provided comparison item.
        /// </summary>
        /// <param name="comparison">The comparison.</param>
        public void SortValues(Comparison<TValue> comparison) => _keyedCollection.Sort((x, y) => comparison(x.Value, y.Value));

        /// <summary>
        /// Gets the comparer used in this dictionary.
        /// </summary>
        /// <value>The comparer.</value>
        public IEqualityComparer<TKey> Comparer { get; private set; }
    }
}