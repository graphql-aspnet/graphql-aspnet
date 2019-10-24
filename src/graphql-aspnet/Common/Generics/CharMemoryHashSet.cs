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

    /// <summary>
    /// A hashset with a built in comparer for processing readonly memory types.
    /// </summary>
    /// <seealso cref="System.Collections.Generic.HashSet{ReadOnlyMemory}" />
    public class CharMemoryHashSet : HashSet<ReadOnlyMemory<char>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CharMemoryHashSet"/> class.
        /// </summary>
        public CharMemoryHashSet()
        : base(new MemoryOfCharComparer())
        {
        }

        /// <summary>
        /// Attempts to add the range of entities, one by one, to the hashset.
        /// </summary>
        /// <param name="items">The items.</param>
        public void AddRange(IEnumerable<ReadOnlyMemory<char>> items)
        {
            if (items == null)
                return;

            foreach (var item in items)
                this.Add(item);
        }
    }
}