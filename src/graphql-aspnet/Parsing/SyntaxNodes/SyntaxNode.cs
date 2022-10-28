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
    using System.Collections.Generic;
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
    public abstract class SyntaxNode : IDisposable
    {
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxNode" /> class.
        /// </summary>
        /// <param name="startLocation">The start location.</param>
        protected SyntaxNode(SourceLocation startLocation)
        {
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
                this.Children = this.Children ?? new NodeCollection();
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
        /// Adds a collection of children to this instance.
        /// </summary>
        /// <param name="childNodes">The child nodes to add.</param>
        public void AddChildren(IEnumerable<SyntaxNode> childNodes)
        {
            if (childNodes == null)
                return;

            foreach (var child in childNodes)
                this.AddChild(child);
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
        public NodeCollection Children { get; private set; }

        /// <summary>
        /// Gets the location in the source text where this node originated.
        /// </summary>
        /// <value>The location.</value>
        public SourceLocation Location { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return "Unknown";
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (this.Children != null)
                        this.Children.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _isDisposed = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}