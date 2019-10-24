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
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A set of context sensitive nodes that defines a recieved graph ql document.
    /// This tree forms the foundation of the data used to fulfill a query request.
    /// </summary>
    public interface ISyntaxTree : IEnumerable<SyntaxNode>
    {
        /// <summary>
        /// Gets a collection of the nodes contained in this document.
        /// </summary>
        /// <value>The dictionary, keyed on operation name, of the operations on the recieved query.</value>
        IReadOnlyList<SyntaxNode> Nodes { get; }
    }
}