﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Contexts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Parsing;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A context managing the translation of a <see cref="SyntaxNode"/> to a properly structured
    /// <see cref="IDocumentPart"/> with a <see cref="IQueryDocument"/>.
    /// </summary>
    [DebuggerDisplay("Node Type: {ActiveNode.NodeType}")]
    internal readonly ref struct DocumentConstructionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentConstructionContext" /> struct.
        /// </summary>
        /// <param name="syntaxTree">The syntax tree to process for filling the document.</param>
        /// <param name="sourceText">The source text that is referenced by the <paramref name="syntaxTree"/>.</param>
        /// <param name="documentToFill">The root document to fill from the syntax tree.</param>
        /// <param name="targetSchema">The target schema to which the document should be paired.</param>
        public DocumentConstructionContext(
            SyntaxTree syntaxTree,
            SourceText sourceText,
            IQueryDocument documentToFill,
            ISchema targetSchema)
        {
            this.SyntaxTree = syntaxTree;
            this.SourceText = sourceText;

            this.ActiveNode = syntaxTree.RootNode;
            this.ParentNode = SyntaxNode.None;

            this.ActivePart = Validation.ThrowIfNullOrReturn(documentToFill, nameof(documentToFill));
            this.ParentPart = null;

            this.Messages = Validation.ThrowIfNullOrReturn(documentToFill.Messages, $"{nameof(documentToFill)}.{nameof(documentToFill.Messages)}");

            this.Document = documentToFill;
            this.Schema = Validation.ThrowIfNullOrReturn(targetSchema, nameof(targetSchema));

            this.Depth = 0;

            this.Spreads = new List<IFragmentSpreadDocumentPart>();

            this.ActiveOperation = null;
        }

        private DocumentConstructionContext(
            SyntaxTree syntaxTree,
            SourceText sourceText,
            SyntaxNode activeNode,
            SyntaxNode parentNode,
            IDocumentPart activePart,
            IDocumentPart parentPart,
            IGraphMessageCollection messageCollection,
            ISchema schema,
            IQueryDocument rootDocumentPart,
            int depth,
            IList<IFragmentSpreadDocumentPart> fragSpreads,
            IOperationDocumentPart activeOperation)
        {
            this.SyntaxTree = syntaxTree;
            this.SourceText = sourceText;

            this.ActiveNode = activeNode;
            this.ParentNode = parentNode;

            this.ActivePart = activePart;
            this.ParentPart = parentPart;

            this.Messages = messageCollection;

            this.Document = rootDocumentPart;
            this.Schema = schema;

            this.Depth = depth;

            this.Spreads = fragSpreads;

            this.ActiveOperation = activeOperation;
        }

        /// <summary>
        /// Generates a context that flags this context as being a pass through
        /// through context in the document creation process.
        /// Its depth is skipped in a max depth calculation.
        /// </summary>
        /// <returns>DocumentConstructionContext.</returns>
        public DocumentConstructionContext Skip()
        {
            return new DocumentConstructionContext(
                this.SyntaxTree,
                this.SourceText,
                this.ActiveNode,
                this.ParentNode,
                this.ParentPart, // bring down the parent part to be the active part
                this.ParentPart,
                this.Messages,
                this.Schema,
                this.Document,
                this.Depth,
                this.Spreads,
                this.ActiveOperation);
        }

        /// <summary>
        /// Regenerates this context with the provided part as the
        /// newly created <see cref="ActivePart"/> for this context.
        /// </summary>
        /// <param name="docPart">The document part to assign.</param>
        /// <returns>The updated construction context.</returns>
        public DocumentConstructionContext AssignPart(IDocumentPart docPart)
        {
            if (this.ActivePart != null)
                throw new InvalidOperationException("This context already has a part assigned.");

            Validation.ThrowIfNull(docPart, nameof(docPart));
            var newDepth = this.Depth;
            var newOperation = this.ActiveOperation;

            this.ParentPart.Children.Add(docPart);

            if (docPart is IFieldDocumentPart)
                newDepth += 1;

            if (docPart is IFragmentSpreadDocumentPart lbdp)
                this.Spreads.Add(lbdp);

            if (docPart is IOperationDocumentPart odp)
                newOperation = odp;

            return new DocumentConstructionContext(
                this.SyntaxTree,
                this.SourceText,
                this.ActiveNode,
                this.ParentNode,
                docPart,   // new active part
                this.ParentPart,
                this.Messages,
                this.Schema,
                this.Document,
                newDepth,
                this.Spreads,
                newOperation);
        }

        /// <summary>
        /// Create a child construction context using the contents of this context
        /// along with the newly supplied child node.
        /// </summary>
        /// <param name="childNode">The node to incorporate in the
        /// new construction context.</param>
        /// <returns>DocumentConstructionContext.</returns>
        public DocumentConstructionContext CreateChildContext(SyntaxNode childNode)
        {
            return new DocumentConstructionContext(
               this.SyntaxTree,
               this.SourceText,
               childNode,
               this.ParentNode,
               null,   // no part set yet
               this.ActivePart,
               this.Messages,
               this.Schema,
               this.Document,
               this.Depth,
               this.Spreads,
               this.ActiveOperation);
        }

        /// <summary>
        /// Gets the query document being constructed.
        /// </summary>
        /// <value>The document.</value>
        public IQueryDocument Document { get; }

        /// <summary>
        /// Gets the active part that was constructed with this context. May be null
        /// if no part has been constructed yet.
        /// </summary>
        /// <value>The active part.</value>
        public IDocumentPart ActivePart { get; }

        /// <summary>
        /// Gets the parent document part which will own the part created
        /// by this context.
        /// </summary>
        /// <value>The parent part.</value>
        public IDocumentPart ParentPart { get; }

        /// <summary>
        /// Gets the syntax node this context is processing in order to create a
        /// document part.
        /// </summary>
        /// <value>The active node.</value>
        public SyntaxNode ActiveNode { get; }

        /// <summary>
        /// Gets the parent syntax node which generated this context.
        /// </summary>
        /// <value>The parent node.</value>
        public SyntaxNode ParentNode { get; }

        /// <summary>
        /// Gets the syntax tree this context is working with. This tree is referenced
        /// when generating child contexts for differnt active syntax nodes.
        /// </summary>
        /// <value>The syntax tree.</value>
        public SyntaxTree SyntaxTree { get; }

        /// <summary>
        /// Gets the source text this context is tracking against.
        /// </summary>
        /// <value>The source text.</value>
        public SourceText SourceText { get; }

        /// <summary>
        /// Gets the set of messages added during the use of this context.
        /// </summary>
        /// <value>The messages.</value>
        public IGraphMessageCollection Messages { get; }

        /// <summary>
        /// Gets the schema from which the document is constructed. This schema
        /// will be used for all graph type and directive lookups.
        /// </summary>
        /// <value>The schema.</value>
        public ISchema Schema { get; }

        /// <summary>
        /// Gets the current depth in the document of the part contained on this context.
        /// level.
        /// </summary>
        /// <value>The maximum depth.</value>
        public int Depth { get; }

        /// <summary>
        /// Gets the fragment spreads targeting named fragments.
        /// </summary>
        /// <value>The late bound parts of the document being built.</value>
        public IList<IFragmentSpreadDocumentPart> Spreads { get; }

        /// <summary>
        /// Gets the active operation being built, if any.
        /// </summary>
        /// <value>The active operation.</value>
        public IOperationDocumentPart ActiveOperation { get; }
    }
}