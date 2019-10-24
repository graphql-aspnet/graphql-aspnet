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
    /// A base type for common data shared between all
    /// values passed as input for execution of a graph operation.
    /// </summary>
    [DebuggerDisplay("{Value}")]
    public abstract class InputValueNode : SyntaxNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputValueNode"/> class.
        /// </summary>
        /// <param name="startLocation">The start location.</param>
        /// <param name="value">The value.</param>
        protected InputValueNode(SourceLocation startLocation, ReadOnlyMemory<char> value)
            : base(startLocation)
        {
            this.Value = value;
        }

        /// <summary>
        /// Determines whether this instance can contain the child being added.
        /// </summary>
        /// <param name="childNode">The child node.</param>
        /// <returns><c>true</c> if this instance can have the node as a child; otherwise, <c>false</c>.</returns>
        protected override bool CanHaveChild(SyntaxNode childNode)
        {
            return false;
        }

        /// <summary>
        /// Gets an explicit, single value, set for this node.
        /// </summary>
        /// <value>The value.</value>
        public ReadOnlyMemory<char> Value { get; }
    }
}