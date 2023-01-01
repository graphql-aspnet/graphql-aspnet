﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.DocumentParts
{
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.QueryPlans.DocumentParts.SuppliedValues;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Schema;

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
            this.Parent = Validation.ThrowIfNullOrReturn(parentPart, nameof(this.Parent));
            this.SourceLocation = sourceLocation;

            this.Children = new DocumentPartsCollection(this);
        }

        /// <summary>
        /// When overriden in a child field, extends the provided, human readable,
        /// path to include this document part.
        /// </summary>
        /// <param name="pathToExtend">The path to extend.</param>
        /// <returns>SourcePath.</returns>
        protected virtual SourcePath ExtendPath(SourcePath pathToExtend)
        {
            return pathToExtend;
        }

        /// <inheritdoc />
        public virtual void AssignGraphType(IGraphType graphType)
        {
            this.GraphType = graphType;
        }

        /// <summary>
        /// When called, walks the document part chain from this part upwards forcing a refresh
        /// on all encountered <see cref="IReferenceDocumentPart"/> instances.
        /// </summary>
        /// <param name="refreshSelf">if set to <c>true</c> a refresh will be issued against
        /// this document part first, before the parents of this field.</param>
        protected void RefreshAllAscendantFields(bool refreshSelf = true)
        {
            var docPart = this as IDocumentPart;
            if (docPart != null && !refreshSelf)
                docPart = docPart.Parent;

            while (docPart != null)
            {
                docPart.Refresh();
                docPart = docPart.Parent;
            }
        }

        /// <inheritdoc />
        public virtual void Refresh()
        {
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
        public abstract string Description { get; }

        /// <inheritdoc />
        public SourceLocation SourceLocation { get; }

        /// <inheritdoc />
        public SourcePath Path
        {
            get
            {
                if (_path == null)
                    _path = this.ExtendPath(this.Parent.Path.Clone());

                return _path;
            }
        }

        /// <inheritdoc />
        public SourceOrigin Origin => new SourceOrigin(this.SourceLocation, this.Path);
    }
}