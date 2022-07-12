// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.Common;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.SuppliedValues;

    /// <summary>
    /// A document representing the query text as supplied by the user matched against a schema.
    /// </summary>
    internal class QueryDocument : IGraphQueryDocument
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
            _fragmentCollection = new DocumentNamedFragmentCollection(this);
            _operations = new DocumentOperationCollection(this);

            this.Children.ChildPartAdded += this.Children_PartAdded;
            this.Attributes = new HashSet<string>();
        }

        private void Children_PartAdded(object sender, DocumentPartEventArgs eventArgs)
        {
            if (eventArgs.TargetDocumentPart is INamedFragmentDocumentPart nf)
                _fragmentCollection.AddFragment(nf);
            else if (eventArgs.TargetDocumentPart is IOperationDocumentPart od)
                _operations.AddOperation(od);
        }

        /// <inheritdoc />
        public void AssignGraphType(IGraphType graphType)
        {
            throw new System.NotSupportedException("No graph type exists that can used for the document");
        }

        /// <inheritdoc />
        public IGraphMessageCollection Messages { get; }

        /// <inheritdoc />
        public int MaxDepth { get; set; }

        /// <inheritdoc />
        public IDocumentPartsCollection Children { get; }

        /// <inheritdoc />
        public IGraphType GraphType => null;

        /// <inheritdoc />
        public DocumentPartType PartType => DocumentPartType.Document;

        /// <inheritdoc />
        public IDocumentPart Parent => null;

        /// <inheritdoc />
        public SyntaxNode Node => EmptyNode.Instance;

        /// <inheritdoc />
        public SourcePath Path { get; }

        /// <inheritdoc />
        public IOperationCollectionDocumentPart Operations => _operations;

        /// <inheritdoc />
        public INamedFragmentCollectionDocumentPart NamedFragments => _fragmentCollection;

        /// <inheritdoc />
        public ISet<string> Attributes { get; }

        /// <inheritdoc />
        public string Description => "Document Root";
    }
}