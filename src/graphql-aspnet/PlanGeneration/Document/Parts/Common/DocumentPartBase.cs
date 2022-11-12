﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts.Common
{
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.PlanGeneration.Document;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.SuppliedValues;

    /// <summary>
    /// A base class with common functionality of all <see cref="IDocumentPart" />
    /// implementations.
    /// </summary>
    internal abstract class DocumentPartBase : IDocumentPart
    {
        private SourcePath _path = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentPartBase"/> class.
        /// </summary>
        /// <param name="parentPart">The parent document part that owns this instance.</param>
        /// <param name="sourceLocation">The location where this document part
        /// originated in the query.</param>
        protected DocumentPartBase(IDocumentPart parentPart, SourceLocation sourceLocation)
        {
            this.Parent = Validation.ThrowIfNullOrReturn(parentPart, nameof(Parent));
            this.SourceLocation = sourceLocation;

            this.Children = new DocumentPartsCollection(this);
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
        public SourceLocation SourceLocation { get; }

        /// <inheritdoc />
        public abstract string Description { get; }
    }
}