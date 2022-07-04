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

    /// <summary>
    /// An empty syntax node, used a place holder when needed.
    /// </summary>
    [DebuggerDisplay("EMPTY")]
    public class EmptyNode : SyntaxNode
    {
        /// <summary>
        /// Gets the singleton instance of this node.
        /// </summary>
        /// <value>The instance.</value>
        public static EmptyNode Instance { get; } = new EmptyNode();

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyNode"/> class.
        /// </summary>
        private EmptyNode()
            : base(new SourceLocation(0, 0, 0))
        {
        }

        /// <inheritdoc />
        protected override bool CanHaveChild(SyntaxNode childNode)
        {
            return false;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"-Empty-";
        }
    }
}