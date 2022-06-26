// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.Common;

    /// <summary>
    /// A base class with common functionality of all <see cref="IDocumentPart" />
    /// implementations.
    /// </summary>
    internal abstract class DocumentPartBase : IDocumentPart
    {
        private SourcePath _path = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentPartBase" /> class.
        /// </summary>
        /// <param name="parentPart">The parent document part, if any, that owns this instance.</param>
        /// <param name="node">The AST node from which this part was created.</param>
        protected DocumentPartBase(IDocumentPart parentPart, SyntaxNode node)
        {
            this.Parent = Validation.ThrowIfNullOrReturn(parentPart, nameof(Parent));
            this.Node = Validation.ThrowIfNullOrReturn(node, nameof(node));

            this.Children = new DocumentPartsCollection(this);

            // wire up local events
            this.Children.PartAdded += (o, e) => this.OnChildPartAdded(e.TargetDocumentPart);
            this.Children.BeforePartAdded += (o, e) =>
            {
                this.OnBeforeChildAdd(e.TargetDocumentPart);
                e.AllowAdd = this.OnBeforeChildAdd(e.TargetDocumentPart);
            };
        }

        /// <summary>
        /// When overriden in a child field, extends the human readable path into the query document
        /// to include this document part.
        /// </summary>
        /// <param name="path">The path to extend. This path should be cloned.</param>
        /// <returns>SourcePath.</returns>
        protected virtual SourcePath CreatePath(SourcePath path)
        {
            return path.Clone();
        }

        /// <summary>
        /// When overriden in a child class, this method is called
        /// when a child part is added to this instance.
        /// </summary>
        /// <param name="childPart">The child part.</param>
        protected virtual void OnChildPartAdded(IDocumentPart childPart)
        {
        }

        /// <summary>
        /// When overriden in a child class, allows for the inspection
        /// of the child part to determine if it can be added.
        /// </summary>
        /// <param name="childPart">The child part.</param>
        /// <returns><c>true</c> to indicate that the part should be added;
        /// otherwise false.</returns>
        protected virtual bool OnBeforeChildAdd(IDocumentPart childPart)
        {
            return true;
        }

        /// <inheritdoc />
        public virtual void AssignGraphType(IGraphType graphType)
        {
            this.GraphType = graphType;
        }

        /// <inheritdoc />
        public abstract DocumentPartType PartType { get; }

        /// <inheritdoc />
        public IDocumentPartsCollection Children { get; }

        /// <inheritdoc />
        public IDocumentPart Parent { get; }

        /// <inheritdoc />
        public IGraphType GraphType { get; private set; }

        /// <inheritdoc />
        public SourcePath Path
        {
            get
            {
                if (_path == null)
                {
                    _path = this.CreatePath(this.Parent.Path);
                }

                return _path;
            }
        }

        /// <inheritdoc />
        public SyntaxNode Node { get; }
    }
}