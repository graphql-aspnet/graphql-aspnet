// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.Document
{
    using System;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Execution.QueryPlans.Document.Parts.SuppliedValues;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A document representing the query text as supplied by the user matched against a schema.
    /// </summary>
    internal class QueryDocument : IQueryDocument, IDescendentDocumentPartSubscriber
    {
        private readonly DocumentOperationCollection _operations;
        private readonly DocumentNamedFragmentCollection _fragmentCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryDocument" /> class.
        /// </summary>
        public QueryDocument()
        {
            this.Messages = new GraphMessageCollection();
            this.Children = new DocumentPartsCollection(this);

            this.Path = new SourcePath();
            this.Origin = new SourceOrigin(new SourceLocation(0, 0, 0));
            _fragmentCollection = new DocumentNamedFragmentCollection(this);
            _operations = new DocumentOperationCollection(this);
        }

        /// <inheritdoc cref="IDescendentDocumentPartSubscriber.OnDescendentPartAdded" />
        void IDescendentDocumentPartSubscriber.OnDescendentPartAdded(IDocumentPart decendentPart, int relativeDepth)
        {
            if (decendentPart is INamedFragmentDocumentPart nf)
                _fragmentCollection.AddFragment(nf);
            else if (decendentPart is IOperationDocumentPart od)
                _operations.AddOperation(od);
        }

        /// <inheritdoc />
        public void AssignGraphType(IGraphType graphType)
        {
            throw new NotSupportedException("No graph type exists that can be used for the root document");
        }

        /// <inheritdoc />
        public IGraphMessageCollection Messages { get; }

        /// <inheritdoc />
        public IDocumentPartsCollection Children { get; }

        /// <inheritdoc />
        public IGraphType GraphType => null;

        /// <inheritdoc />
        public DocumentPartType PartType => DocumentPartType.Document;

        /// <inheritdoc />
        public IDocumentPart Parent => null;

        /// <inheritdoc />
        public SourceLocation SourceLocation => SourceLocation.None;

        /// <inheritdoc />
        public SourcePath Path { get; }

        /// <inheritdoc />
        public IOperationCollectionDocumentPart Operations => _operations;

        /// <inheritdoc />
        public INamedFragmentCollectionDocumentPart NamedFragments => _fragmentCollection;

        /// <inheritdoc />
        public string Description => "Document Root";

        /// <inheritdoc />
        public SourceOrigin Origin { get; }
    }
}