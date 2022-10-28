// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Contexts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.RulesEngine.Interfaces;

    /// <summary>
    /// A subset of a document context dealing with a single operation definition within a document.
    /// </summary>
    [DebuggerDisplay("Node Type: {ActiveNode.NodeName}")]
    internal class DocumentConstructionContext : IContextGenerator<DocumentConstructionContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentConstructionContext" /> class.
        /// </summary>
        /// <param name="syntaxTree">The syntax tree to process for filling the document.</param>
        /// <param name="rootDocument">The root document to fill from the syntax tree.</param>
        /// <param name="targetSchema">The target schema to which the document should be paired.</param>
        public DocumentConstructionContext(
            ISyntaxTree syntaxTree,
            IGraphQueryDocument rootDocument,
            ISchema targetSchema)
        {
            this.ActivePart = Validation.ThrowIfNullOrReturn(rootDocument, nameof(rootDocument));
            this.ActiveNode = Validation.ThrowIfNullOrReturn(syntaxTree, nameof(syntaxTree)).RootNode;
            this.Messages = Validation.ThrowIfNullOrReturn(rootDocument.Messages, $"{nameof(rootDocument)}.{nameof(rootDocument.Messages)}");
            this.Schema = Validation.ThrowIfNullOrReturn(targetSchema, nameof(targetSchema));
            this.ParentContext = null;
            this.Document = rootDocument;
            this.Depth = 0;
            this.Spreads = new List<IFragmentSpreadDocumentPart>();
            this.ActiveOperation = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentConstructionContext" /> class.
        /// </summary>
        /// <param name="newNode">The current AST node being processed.</param>
        /// <param name="parentContext">The parent context form which this
        /// one inherits, if any.</param>
        private DocumentConstructionContext(
            SyntaxNode newNode,
            DocumentConstructionContext parentContext)
        {
            if (parentContext?.ActivePart == null)
            {
                throw new InvalidOperationException(
                     "A parent's active part must be set. Did you forget " +
                    "to mark a node as skipped?");
            }

            this.ActivePart = null;
            this.ActiveNode = Validation.ThrowIfNullOrReturn(newNode, nameof(newNode));
            this.ParentContext = parentContext;
            this.Document = parentContext.Document;
            this.Schema = parentContext.Schema;
            this.Messages = parentContext.Messages;
            this.Depth = parentContext.Depth;
            this.Spreads = parentContext.Spreads;
            parentContext.ActiveOperation = parentContext.ActiveOperation;
        }

        /// <summary>
        /// Creates new contexts for any chilren of the currently active context that need to be processed
        /// against the same rule set.
        /// </summary>
        /// <returns>IEnumerable&lt;TContext&gt;.</returns>
        public IEnumerable<DocumentConstructionContext> CreateChildContexts()
        {
            if (this.ActiveNode.Children != null)
            {
                foreach (var child in this.ActiveNode.Children)
                {
                    yield return new DocumentConstructionContext(child, this);
                }
            }
        }

        /// <summary>
        /// Flags this context as being a path through context in the document
        /// creation process. Its depth is skipped in a max depth calculation.
        /// </summary>
        public void Skip()
        {
            this.ActivePart = this.ParentPart;
        }

        /// <summary>
        /// Assigns a new created document part to this context. This part is
        /// automatically set as the <see cref="ActivePart"/> of this context.
        /// </summary>
        /// <param name="docPart">The document part to assign.</param>
        public void AssignPart(IDocumentPart docPart)
        {
            if (this.ActivePart != null)
                throw new InvalidOperationException("This context already has a part assigned.");

            this.ActivePart = Validation.ThrowIfNullOrReturn(docPart, nameof(docPart));

            this.ParentPart.Children.Add(this.ActivePart);

            if (this.ActivePart is IFieldDocumentPart)
                this.Depth += 1;

            if (this.Document.MaxDepth < this.Depth)
                this.Document.MaxDepth = this.Depth;

            if (docPart is IFragmentSpreadDocumentPart lbdp)
                this.Spreads.Add(lbdp);

            if (docPart is IOperationDocumentPart odp)
                this.ActiveOperation = odp;
        }

        /// <summary>
        /// Gets the parent context that created this child context, if any. Root contexts will
        /// have a null parent.
        /// </summary>
        /// <value>The parent context.</value>
        public DocumentConstructionContext ParentContext { get; private set; }

        /// <summary>
        /// Gets the query document being constructed.
        /// </summary>
        /// <value>The document.</value>
        public IGraphQueryDocument Document { get; }

        /// <summary>
        /// Gets the syntax node this context is processing in order to create a
        /// document part.
        /// </summary>
        /// <value>The active node.</value>
        public SyntaxNode ActiveNode { get; }

        /// <summary>
        /// Gets the active part that was constructed with this context. May be null
        /// if no part has been constructed yet.
        /// </summary>
        /// <value>The active part.</value>
        public IDocumentPart ActivePart { get; private set; }

        /// <summary>
        /// Gets the parent document part which will own the part created
        /// by this context.
        /// </summary>
        /// <value>The parent part.</value>
        public IDocumentPart ParentPart => this.ParentContext?.ActivePart;

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
        /// Gets a helpful path to determine where this context is in the document.
        /// Debug only.
        /// </summary>
        /// <value>The path.</value>
        public string Path
        {
            get
            {
                if (Debugger.IsAttached)
                {
                    var str = this.ParentContext?.Path ?? string.Empty;
                    if (this.ParentContext != null)
                        str += "|";

                    str += this.ActiveNode.ToString();

                    return str;
                }
                else
                {
                    return "-Path Inspection Disabled-";
                }
            }
        }

        /// <summary>
        /// Gets the current depth in the document of the part contained on this context.
        /// level.
        /// </summary>
        /// <value>The maximum depth.</value>
        public int Depth { get; private set; }

        /// <summary>
        /// Gets the fragment spreads targeting named fragments. Since the named fragments
        /// may not exist when the spread is encountered a second pass must be done
        /// to link them to their associated named fragment.
        /// </summary>
        /// <value>The late bound parts of the document being built.</value>
        public IList<IFragmentSpreadDocumentPart> Spreads { get; private set; }

        /// <summary>
        /// Gets the active operation being built, if any.
        /// </summary>
        /// <value>The active operation.</value>
        public IOperationDocumentPart ActiveOperation { get; private set; }
    }
}