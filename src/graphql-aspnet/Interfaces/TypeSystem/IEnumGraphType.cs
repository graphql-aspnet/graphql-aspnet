// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.TypeSystem
{
    using System.Collections.Generic;

    /// <summary>
    /// A representation of an enumeration, a fixed set of possible values.
    /// </summary>
    /// <seealso cref="IGraphType" />
    public interface IEnumGraphType : IGraphType, ITypedItem
    {
        /// <summary>
        /// Gets the values that can be supplied to this enum.
        /// </summary>
        /// <value>The values.</value>
        IReadOnlyDictionary<string, IEnumOption> Values { get; }
    }
}