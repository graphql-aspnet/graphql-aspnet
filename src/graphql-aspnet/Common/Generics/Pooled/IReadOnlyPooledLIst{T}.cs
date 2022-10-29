// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

// *****************************************************************************
// This PooledList has been adapted from jtmueller's implementation: <br/>
// https://github.com/jtmueller/Collections.Pooled/tree/master/Collections.Pooled <br/>
// *****************************************************************************

namespace GraphQL.AspNet.Common.Generics.Pooled
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a read-only collection of pooled elements that can be accessed by index
    /// </summary>
    /// <typeparam name="T">The type of elements in the read-only pooled list.</typeparam>

    public interface IReadOnlyPooledList<T> : IReadOnlyList<T>
    {
        /// <summary>
        /// Gets a <see cref="System.ReadOnlySpan{T}" /> for the items currently in the collection.
        /// </summary>
        /// <value>The span.</value>
        ReadOnlySpan<T> Span { get; }
    }
}