// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// A template interface representing an enumeration graph type.
    /// </summary>
    public interface IEnumGraphTypeTemplate : IGraphTypeTemplate
    {
        /// <summary>
        /// Gets the collected set of enumeration values that this template parsed.
        /// </summary>
        /// <value>The values.</value>
        IReadOnlyList<IEnumValueTemplate> Values { get; }
    }
}