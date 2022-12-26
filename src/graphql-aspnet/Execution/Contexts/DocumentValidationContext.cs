// *************************************************************
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
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution.RulesEngine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A context used to validate all the created parts of a document generated during construction.
    /// </summary>
    [DebuggerDisplay("Part: {ActivePart.PartType}")]
    public class DocumentValidationContext : IRuleProcessorChildContextGenerator<DocumentValidationContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentValidationContext" /> class.
        /// </summary>
        /// <param name="targetSchema">The target schema the query document is validated against.</param>
        /// <param name="queryDocument">The query document to validate.</param>
        public DocumentValidationContext(ISchema targetSchema, IQueryDocument queryDocument)
        {
            this.Schema = Validation.ThrowIfNullOrReturn(targetSchema, nameof(targetSchema));
            this.ActivePart = Validation.ThrowIfNullOrReturn(queryDocument, nameof(queryDocument));
            this.Document = queryDocument;
            this.Messages = queryDocument.Messages;
            this.RuleMetaData = new Dictionary<Guid, object>();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="DocumentValidationContext" /> class from being created.
        /// </summary>
        private DocumentValidationContext(DocumentValidationContext parentContext, IDocumentPart partToValidate)
        {
            this.ActivePart = Validation.ThrowIfNullOrReturn(partToValidate, nameof(partToValidate));
            this.ParentContext = parentContext;
            this.Document = parentContext.Document;
            this.Messages = parentContext.Messages;
            this.RuleMetaData = parentContext.RuleMetaData;
            this.Schema = parentContext.Schema;
        }

        /// <summary>
        /// Creates new contexts for any chilren of the currently active context that need to be processed
        /// against the same rule set.
        /// </summary>
        /// <returns>IEnumerable&lt;TContext&gt;.</returns>
        public IEnumerable<DocumentValidationContext> CreateChildContexts()
        {
            if (this.ActivePart.Children.Count == 0)
                return Enumerable.Empty<DocumentValidationContext>();

            var children = new List<DocumentValidationContext>(this.ActivePart.Children.Count);
            foreach (var docPart in this.ActivePart.Children)
            {
                // use the private constructor
                var newContext = new DocumentValidationContext(this, docPart);
                children.Add(newContext);
            }

            return children;
        }

        /// <summary>
        /// Gets the schema the document is being validated against.
        /// </summary>
        /// <value>The active schema.</value>
        public ISchema Schema { get; }

        /// <summary>
        /// Gets the part being validated on this context.
        /// </summary>
        /// <value>The active part.</value>
        public IDocumentPart ActivePart { get; private set; }

        /// <summary>
        /// Gets the part that owns the <see cref="ActivePart" /> on this context.
        /// </summary>
        /// <value>The parent part.</value>
        public IDocumentPart ParentPart => this.ParentContext?.ActivePart;

        /// <summary>
        /// Gets the parent context that created this child context, if any. Root contexts will
        /// have a null parent.
        /// </summary>
        /// <value>The parent context.</value>
        public DocumentValidationContext ParentContext { get; private set; }

        /// <summary>
        /// Gets the message collection where validation errors should be saved.
        /// </summary>
        /// <value>The messages.</value>
        public IGraphMessageCollection Messages { get; }

        /// <summary>
        /// Gets a reference to the document being validated.
        /// </summary>
        /// <value>The document.</value>
        public IQueryDocument Document { get; }

        /// <summary>
        /// Gets a metadata object (by rule id) to carry information
        /// between rule invocations.
        /// </summary>
        /// <value>The shared metadata for this request invocation.</value>
        public Dictionary<Guid, object> RuleMetaData { get; }
    }
}