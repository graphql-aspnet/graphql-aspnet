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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// A keyed collection that generates keys from a delegate rather than being explicitly provided.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <seealso cref="System.Collections.ObjectModel.KeyedCollection{TKey, TItem}" />
    public class DelegateKeyedCollection<TKey, TItem> : KeyedCollection<TKey, TItem>
    {
        private readonly Func<TItem, TKey> _keyDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateKeyedCollection{TKey,TItem}" /> class.
        /// </summary>
        /// <param name="keyDelegate">The delegate for extracting the key from an item.</param>
        public DelegateKeyedCollection(Func<TItem, TKey> keyDelegate)
        {
            _keyDelegate = Validation.ThrowIfNullOrReturn(keyDelegate, nameof(keyDelegate));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateKeyedCollection{TKey,TItem}" /> class.
        /// </summary>
        /// <param name="keyDelegate">The delegate for extracting the key from an item.</param>
        /// <param name="comparer">The comparer.</param>
        public DelegateKeyedCollection(Func<TItem, TKey> keyDelegate, IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
            _keyDelegate = Validation.ThrowIfNullOrReturn(keyDelegate, nameof(keyDelegate));
        }

        /// <summary>
        /// When implemented in a derived class, extracts the key from the specified element.
        /// </summary>
        /// <param name="item">The element from which to extract the key.</param>
        /// <returns>The key for the specified element.</returns>
        protected override TKey GetKeyForItem(TItem item)
        {
            return _keyDelegate(item);
        }

        /// <summary>
        /// Sorts collection by its keys.
        /// </summary>
        public void SortByKeys()
        {
            var comparer = Comparer<TKey>.Default;
            this.SortByKeys(comparer);
        }

        /// <summary>
        /// Sorts the items in the collection by its established keys using the comparer provided.
        /// </summary>
        /// <param name="keyComparer">The key comparer.</param>
        public void SortByKeys(IComparer<TKey> keyComparer)
        {
            var comparer = new ComparisonComparer<TItem>((x, y) => keyComparer.Compare(this.GetKeyForItem(x), this.GetKeyForItem(y)));
            this.Sort(comparer);
        }

        /// <summary>
        /// Sorts the items in the collection by its established keys using the comparer provided.
        /// </summary>
        /// <param name="keyComparison">The key comparison.</param>
        public void SortByKeys(Comparison<TKey> keyComparison)
        {
            var comparer = new ComparisonComparer<TItem>((x, y) => keyComparison(this.GetKeyForItem(x), this.GetKeyForItem(y)));
            this.Sort(comparer);
        }

        /// <summary>
        /// Sorts this instance by its default comparer.
        /// </summary>
        public void Sort()
        {
            var comparer = Comparer<TItem>.Default;
            this.Sort(comparer);
        }

        /// <summary>
        /// Sorts this instance by the provided comparison object.
        /// </summary>
        /// <param name="comparison">The comparison.</param>
        public void Sort(Comparison<TItem> comparison)
        {
            var newComparer = new ComparisonComparer<TItem>(comparison);
            this.Sort(newComparer);
        }

        /// <summary>
        /// Sorts this instance by the provided comparer.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public void Sort(IComparer<TItem> comparer)
        {
            if (this.Items is List<TItem> list)
            {
                list.Sort(comparer);
            }
        }

        /// <summary>
        /// Gets the keys contained in this dictionary.
        /// </summary>
        /// <value>The keys.</value>
        public ICollection<TKey> Keys => this.Dictionary?.Keys;

        /// <summary>
        /// Gets the values contained in this dictionary.
        /// </summary>
        /// <value>The values.</value>
        public ICollection<TItem> Values => this.Dictionary?.Values;
    }
}