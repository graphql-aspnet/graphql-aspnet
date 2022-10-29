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
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;

    /// <summary>
    /// An empty syntax node, used a place holder when needed.
    /// </summary>
    [DebuggerDisplay("EMPTY")]
    public class EmptyNode : SyntaxNode
    {
        /// <summary>
        /// Gets the singleton instance of this empty node.
        /// </summary>
        /// <value>The instance.</value>
        public static EmptyNode Instance { get; } = new EmptyNode();

        /// <summary>
        /// Creates a new empty node that indicates its existance at a specific location.
        /// </summary>
        /// <param name="absoluteLocation">The absolute location in the query document.</param>
        /// <param name="line">The line of the query document.</param>
        /// <param name="position">The position on the line of the query document.</param>
        /// <returns>EmptyNode.</returns>
        internal static EmptyNode AtLocation(int absoluteLocation, int line, int position)
        {
            return new EmptyNode(new SourceLocation(absoluteLocation, line, position));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyNode"/> class.
        /// </summary>
        private EmptyNode()
            : base(new SourceLocation(0, 0, 0))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyNode"/> class.
        /// </summary>
        /// <param name="location">The location.</param>
        private EmptyNode(SourceLocation location)
           : base(location)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"-Empty-";
        }
    }
}