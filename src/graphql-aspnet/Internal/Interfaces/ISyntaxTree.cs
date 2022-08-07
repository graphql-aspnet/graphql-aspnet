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
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A set of context sensitive nodes that defines a recieved graphql document.
    /// This tree forms the foundation of the data used to fulfill a query request.
    /// </summary>
    public interface ISyntaxTree
    {
        /// <summary>
        /// Gets the root node of the AST, all other nodes are children of this root node.
        /// </summary>
        /// <value>The root node.</value>
        DocumentNode RootNode { get; }
    }
}