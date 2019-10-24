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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Tokens;

    /// <summary>
    /// The <see cref="SyntaxNode"/> is a single element in a syntax tree generated from the
    /// lexed source document. It is a represention of a "thing" in a graphql source document.
    /// Groups of <see cref="LexicalToken"/> together when parsed become syntax nodes. Syntax nodes
    /// create a foundational framework from which an execution plan can be generated and ran against a schema.
    /// </summary>
    [Serializable]
    public abstract class SyntaxNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxNode" /> class.
        /// </summary>
        /// <param name="startLocation">The start location.</param>
        protected SyntaxNode(SourceLocation startLocation)
        {
            this.Children = new NodeCollection();
            this.Location = Validation.ThrowIfNullOrReturn(startLocation, nameof(startLocation));
        }

        /// <summary>
        /// Adds the child to the node, incorporating it into the syntax tree. If this
        /// node cannot accept this node an exception is thrown.
        /// </summary>
        /// <param name="childNode">The child node.</param>
        public void AddChild(SyntaxNode childNode)
        {
            Validation.ThrowIfNull(childNode, nameof(childNode));
            if (this.CanHaveChild(childNode))
            {
                this.Children.Add(childNode);
                childNode.ParentNode = this;
            }
            else
            {
                throw new GraphQLSyntaxException(
                    childNode.Location,
                    $"Unexpected node, {childNode.NodeName} is invalid at this location.");
            }
        }

        /// <summary>
        /// Gets the owner of this node in the syntax tree.
        /// </summary>
        /// <value>The parent node.</value>
        public SyntaxNode ParentNode { get; private set; }

        /// <summary>
        /// Determines whether this instance can contain the child being added.
        /// </summary>
        /// <param name="childNode">The child node.</param>
        /// <returns><c>true</c> if this instance can have the node as a child; otherwise, <c>false</c>.</returns>
        protected abstract bool CanHaveChild(SyntaxNode childNode);

        /// <summary>
        /// Gets the friendly name of the node.
        /// </summary>
        /// <value>The name of the node.</value>
        protected virtual string NodeName => this.GetType().FriendlyName();

        /// <summary>
        /// Gets the child nodes owned by this instance, if any.
        /// </summary>
        /// <value>The children.</value>
        public NodeCollection Children { get; }

        /// <summary>
        /// Gets the location in the source text where this node originated.
        /// </summary>
        /// <value>The location.</value>
        public SourceLocation Location { get; }
    }
}