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
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;

    /// <summary>
    /// A root document node, to which operations and fragments are childed.
    /// </summary>
    [DebuggerDisplay("Count: {Children.Count}")]
    public class DocumentNode : SyntaxNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentNode"/> class.
        /// </summary>
        public DocumentNode()
            : base(new SourceLocation(0, 0, 0))
        {
        }

        /// <inheritdoc />
        protected override bool CanHaveChild(SyntaxNode childNode)
        {
            return childNode is OperationNode || childNode is NamedFragmentNode;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Document";
        }
    }
}