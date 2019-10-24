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
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;

    /// <summary>
    /// A value node that is a container for a collection of other input values.
    /// </summary>
    /// <seealso cref="InputValueNode" />
    [DebuggerDisplay("Count = {Children.Count}")]
    public class ListValueNode : InputValueNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListValueNode" /> class.
        /// </summary>
        /// <param name="startLocation">The start location.</param>
        public ListValueNode(SourceLocation startLocation)
            : base(startLocation, ReadOnlyMemory<char>.Empty)
        {
        }

        /// <summary>
        /// Determines whether this instance can contain the child being added.
        /// </summary>
        /// <param name="childNode">The child node.</param>
        /// <returns><c>true</c> if this instance can have the node as a child; otherwise, <c>false</c>.</returns>
        protected override bool CanHaveChild(SyntaxNode childNode)
        {
            return childNode is InputValueNode;
        }
    }
}