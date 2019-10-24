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
    using System.Diagnostics;

    /// <summary>
    /// A simple list that is thread-safe.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    [DebuggerStepThrough]
    [DebuggerDisplay("Count = {Count}")]
    public class ConcurrentList<T> : IList<T>
    {
        private readonly List<T> _list;
        private readonly object _root;

        /// <summary>
        /// Occurs when a new item is added to this list. This event is thread safe.
        /// </summary>
        public event EventHandler<T> ItemAdded;

        /// <summary>
        /// Occurs when a new item is removed to this list. This event is thread safe.
        /// </summary>
        public event EventHandler<T> ItemRemoved;

        /// <summary>
        /// Occurs when the entirty of this list is removed and discarded.
        /// </summary>
        public event EventHandler<T> ListCleared;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentList{T}"/> class.
        /// </summary>
        public ConcurrentList()
        {
            _list = new List<T>();
            _root = ((ICollection)_list).SyncRoot;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentList{T}"/> class.
        /// </summary>
        /// <param name="list">The set of items to populate this list with.</param>
        public ConcurrentList(IEnumerable<T> list)
        {
            _list = new List<T>(list);
            _root = ((ICollection)_list).SyncRoot;
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="List{T}"></see>.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                lock (_root)
                    return _list.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="List{T}"></see> is read-only.
        /// </summary>
        /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
        public bool IsReadOnly
        {
            get
            {
                lock (_root)
                    return ((ICollection<T>)_list).IsReadOnly;
            }
        }

        /// <summary>
        /// Adds an item to the <see cref="List{T}"></see>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="List{T}"></see>.</param>
        public void Add(T item)
        {
            lock (_root)
            {
                _list.Add(item);
                this.ItemAdded?.Invoke(this, item);
            }
        }

        /// <summary>
        /// Removes all items from the <see cref="List{T}"></see>.
        /// </summary>
        public void Clear()
        {
            lock (_root)
            {
                _list.Clear();
                this.ListCleared?.Invoke(this, default);
            }
        }

        /// <summary>
        /// Determines whether the <see cref="List{T}"></see> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="List{T}"></see>.</param>
        /// <returns>true if <paramref name="item">item</paramref> is found in the <see cref="List{T}"></see>; otherwise, false.</returns>
        public bool Contains(T item)
        {
            lock (_root)
                return _list.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="List{T}"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="List{T}"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_root)
                _list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="List{T}"></see>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="List{T}"></see>.</param>
        /// <returns>true if <paramref name="item">item</paramref> was successfully removed from the <see cref="List{T}"></see>; otherwise, false. This method also returns false if <paramref name="item">item</paramref> is not found in the original <see cref="List{T}"></see>.</returns>
        public bool Remove(T item)
        {
            lock (_root)
            {
                var removed = _list.Remove(item);
                if (removed)
                    this.ItemRemoved?.Invoke(this, item);
                return removed;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            lock (_root)
                return new List<T>(_list).GetEnumerator();
        }

        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>T.</returns>
        public T this[int index]
        {
            get
            {
                lock (_root)
                    return _list[index];
            }

            set
            {
                lock (_root)
                    _list[index] = value;
            }
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="IList{T}"></see>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="IList{T}"></see>.</param>
        /// <returns>The index of <paramref name="item">item</paramref> if found in the list; otherwise, -1.</returns>
        public int IndexOf(T item)
        {
            lock (_root)
                return _list.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item to the <see cref="IList{T}"></see> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="IList{T}"></see>.</param>
        public void Insert(int index, T item)
        {
            lock (_root)
                _list.Insert(index, item);
        }

        /// <summary>
        /// Removes the <see cref="IList{T}"></see> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            lock (_root)
                _list.RemoveAt(index);
        }
    }
}