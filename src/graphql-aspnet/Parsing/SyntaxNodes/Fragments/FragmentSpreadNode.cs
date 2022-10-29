// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.SyntaxNodes.Fragments
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;

    /// <summary>
    /// A node identifying a pointer to a named fragment in a query document.
    /// </summary>
    [DebuggerDisplay("Spread Fragment: {PointsToFragmentName}")]
    public class FragmentSpreadNode : SyntaxNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FragmentSpreadNode"/> class.
        /// </summary>
        /// <param name="startLocation">The start location.</param>
        /// <param name="pointsTo">The points to.</param>
        public FragmentSpreadNode(SourceLocation startLocation, ReadOnlyMemory<char> pointsTo)
            : base(startLocation)
        {
            this.PointsToFragmentName = pointsTo;
        }

        /// <summary>
        /// Gets the name of the fragement that this node points to.
        /// </summary>
        /// <value>The name of the points to fragment.</value>
        public ReadOnlyMemory<char> PointsToFragmentName { get; }

        /// <summary>
        /// Gets the friendly name of the node.
        /// </summary>
        /// <value>The name of the node.</value>
        protected override string NodeName => "FragmentSpread";

        /// <inheritdoc />
        public override string ToString()
        {
            return $"S-{this.PointsToFragmentName}";
        }
    }
}