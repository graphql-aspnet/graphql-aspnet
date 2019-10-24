// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values
{
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;

    /// <summary>
    /// Represents a collection of input values on a given field.
    /// </summary>
    /// <seealso cref="SyntaxNode" />
    [DebuggerDisplay("Count = {Children.Count}")]
    public class InputItemCollectionNode : SyntaxNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputItemCollectionNode"/> class.
        /// </summary>
        /// <param name="startLocation">The start location.</param>
        public InputItemCollectionNode(SourceLocation startLocation)
            : base(startLocation)
        {
        }

        /// <summary>
        /// Determines whether this instance can contain the child being added.
        /// </summary>
        /// <param name="childNode">The child node.</param>
        /// <returns><c>true</c> if this instance can have the node as a child; otherwise, <c>false</c>.</returns>
        protected override bool CanHaveChild(SyntaxNode childNode)
        {
            return childNode is InputItemNode;
        }
    }
}