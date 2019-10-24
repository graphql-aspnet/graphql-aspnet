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
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;

    /// <summary>
    /// A document representing the query text as supplied by the user matched against a schema.
    /// </summary>
    internal class QueryDocument : IGraphQueryDocument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryDocument" /> class.
        /// </summary>
        /// <param name="messages">The messages to preload to the document.</param>
        /// <param name="operations">the operations to preload to the document.</param>
        /// <param name="maxDepth">The maximum depth achived by the document.</param>
        public QueryDocument(IGraphMessageCollection messages = null, IEnumerable<QueryOperation> operations = null, int maxDepth = 0)
        {
            this.MaxDepth = maxDepth;
            this.Operations = new QueryOperationCollection();
            this.Messages = new GraphMessageCollection();

            this.Operations.AddRange(operations);
            this.Messages.AddRange(messages);
        }

        /// <summary>
        /// Gets the set of operations parsed from a user's query text.
        /// </summary>
        /// <value>The operations.</value>
        public IQueryOperationCollection Operations { get; }

        /// <summary>
        /// Gets the messages generated during the validation run, if any.
        /// </summary>
        /// <value>The messages.</value>
        public IGraphMessageCollection Messages { get; }

        /// <summary>
        /// Gets the field maximum depth of any given operation of this document.
        /// </summary>
        /// <value>The maximum depth.</value>
        public int MaxDepth { get; }
    }
}