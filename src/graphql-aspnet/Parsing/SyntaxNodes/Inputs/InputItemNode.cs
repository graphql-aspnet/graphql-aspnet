// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.SyntaxNodes.Inputs
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// A node representing a named value passed in to a field. Will generally contain one
    /// or more children containing hte <see cref="InputValueNode"/> that is the value assigned to this node.
    /// </summary>
    /// <seealso cref="SyntaxNode" />
    [DebuggerDisplay("{InputName}")]
    public class InputItemNode : SyntaxNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputItemNode"/> class.
        /// </summary>
        /// <param name="startLocation">The start location.</param>
        /// <param name="name">The name assigned to the item.</param>
        public InputItemNode(SourceLocation startLocation, ReadOnlyMemory<char> name)
            : base(startLocation)
        {
            this.InputName = name;
        }

        /// <summary>
        /// Determines whether this instance can contain the child being added.
        /// </summary>
        /// <param name="childNode">The child node.</param>
        /// <returns><c>true</c> if this instance can have the node as a child; otherwise, <c>false</c>.</returns>
        protected override bool CanHaveChild(SyntaxNode childNode)
        {
            return childNode is InputValueNode ||
                   childNode is DirectiveNode;
        }

        /// <summary>
        /// Gets the name of this input item.
        /// </summary>
        /// <value>The text that is the name of this input item.</value>
        public ReadOnlyMemory<char> InputName { get; }
    }
}