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
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// A node indicating the invocation of a directive.
    /// </summary>
    [DebuggerDisplay("@{DirectiveName}")]
    public class DirectiveNode : SyntaxNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveNode"/> class.
        /// </summary>
        /// <param name="startLocation">The start location.</param>
        /// <param name="name">The name.</param>
        public DirectiveNode(SourceLocation startLocation, ReadOnlyMemory<char> name)
            : base(startLocation)
        {
            this.DirectiveName = name;
        }

        /// <summary>
        /// Determines whether this instance can contain the child being added.
        /// </summary>
        /// <param name="childNode">The child node.</param>
        /// <returns><c>true</c> if this instance can have the node as a child; otherwise, <c>false</c>.</returns>
        protected override bool CanHaveChild(SyntaxNode childNode)
        {
            return childNode is InputItemCollectionNode;
        }

        /// <summary>
        /// Gets the name of the directive. This name would match a directive supported
        /// by the server.
        /// </summary>
        /// <value>The name of the directive.</value>
        public ReadOnlyMemory<char> DirectiveName { get; }
    }
}