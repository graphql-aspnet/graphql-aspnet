// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.SyntaxNodes
{
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;

    /// <summary>
    /// A root document node, to which operations and fragments are appended.
    /// </summary>
    [DebuggerDisplay("Count: {Children.Count}")]
    public class DocumentNode : SyntaxNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentNode" /> class.
        /// </summary>
        /// <param name="nodeList">The node list this node references.</param>
        public DocumentNode()
            : base(new SourceLocation(0, 0, 0))
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Document";
        }
    }
}