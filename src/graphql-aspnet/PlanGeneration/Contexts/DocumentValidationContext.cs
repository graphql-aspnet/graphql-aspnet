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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.ValidationRules.Interfaces;

    /// <summary>
    /// A context used to validate all the created parts of a document generated during construction.
    /// </summary>
    internal class DocumentValidationContext : DocumentGenerationContext<IDocumentPart>, IContextGenerator<DocumentValidationContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentValidationContext" /> class.
        /// </summary>
        /// <param name="docContext">The document context.</param>
        /// <param name="part">The currently scoped document part.</param>
        public DocumentValidationContext(DocumentContext docContext, IDocumentPart part)
            : base(docContext, part)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="DocumentValidationContext" /> class from being created.
        /// </summary>
        private DocumentValidationContext()
        {
        }

        /// <summary>
        /// Creates new contexts for any chilren of the currently active context that need to be processed
        /// against the same rule set.
        /// </summary>
        /// <returns>IEnumerable&lt;TContext&gt;.</returns>
        public IEnumerable<DocumentValidationContext> CreateChildContexts()
        {
            foreach (var docPart in this.ActivePart.Children)
            {
                // use the private constructor
                var newContext = new DocumentValidationContext
                {
                    DocumentContext = this.DocumentContext,
                    Item = docPart,
                    ContextItems = new Dictionary<Type, IDocumentPart>(this.ContextItems),
                    ParentContext = this,
                };

                // set the part of THIS context as something in scope on the child if its not the same type (shouldnt be)
                if (this.ActivePart.GetType() != docPart.GetType())
                    newContext.AddOrUpdateContextItem(this.ActivePart);

                yield return newContext;
            }
        }

        /// <summary>
        /// Gets the active node on this context.
        /// </summary>
        /// <value>The active node.</value>
        public IDocumentPart ActivePart => this.Item;

        /// <summary>
        /// Gets the parent context that created this child context, if any. Root contexts will
        /// have a null parent.
        /// </summary>
        /// <value>The parent context.</value>
        public DocumentValidationContext ParentContext { get; private set; }
    }
}