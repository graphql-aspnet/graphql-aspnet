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
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Document;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;

    /// <summary>
    /// A context allowing the sharing of data between validation rules against a cohesive data set.
    /// </summary>
    internal class DocumentContext : IDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentContext" /> class.
        /// </summary>
        /// <param name="schema">The schema in scope during this validation run.</param>
        public DocumentContext(ISchema schema)
        {
            this.Schema = schema;
            this.Messages = new GraphMessageCollection();
            this.Fragments = new QueryFragmentCollection();
            this.Operations = new QueryOperationCollection();
        }

        /// <summary>
        /// Constructs the final document from the completed parts.
        /// </summary>
        /// <returns>IGraphQueryDocument.</returns>
        public IGraphQueryDocument ConstructDocument()
        {
            return new QueryDocument(
                this.Messages,
                this.Operations?.Values,
                this.MaxDepth);
        }

        /// <summary>
        /// Updates the maximum depth of this document if it is deeper than the currently tracked
        /// depth.
        /// </summary>
        /// <param name="depthAchieved">The depth achieved.</param>
        public void UpdateMaxDepth(int depthAchieved)
        {
            if (this.MaxDepth < depthAchieved)
                this.MaxDepth = depthAchieved;
        }

        /// <summary>
        /// Creates a new child context for the given document level operation definition.
        /// </summary>
        /// <param name="node">The operation to create a context for.</param>
        /// <returns>DocumentFieldContext.</returns>
        public DocumentConstructionContext ForTopLevelNode(SyntaxNode node)
        {
            Validation.ThrowIfNull(node, nameof(node));
            return new DocumentConstructionContext(this, node);
        }

        /// <summary>
        /// Gets the reference schema used to validate the document.
        /// </summary>
        /// <value>The schema.</value>
        public ISchema Schema { get; }

        /// <summary>
        /// Gets a master collection of messages generated during the construction of this document.
        /// </summary>
        /// <value>The messages.</value>
        public IGraphMessageCollection Messages { get; }

        /// <summary>
        /// Gets a collection of all the named fragments declared on the document.
        /// </summary>
        /// <value>The fragments.</value>
        public QueryFragmentCollection Fragments { get; }

        /// <summary>
        /// Gets a collection of all the defined operations declared on the document.
        /// </summary>
        /// <value>The operations.</value>
        public QueryOperationCollection Operations { get; }

        /// <summary>
        /// Gets the child parts declared in this instance.
        /// </summary>
        /// <value>The children.</value>
        public IEnumerable<IDocumentPart> Children
        {
            get
            {
                foreach (var fragment in this.Fragments.Values)
                    yield return fragment;

                foreach (var operation in this.Operations.Values)
                    yield return operation;
            }
        }

        /// <summary>
        /// Gets the maximum field depth acheived by the document.
        /// </summary>
        /// <value>The maximum depth.</value>
        public int MaxDepth { get; private set; }
    }
}