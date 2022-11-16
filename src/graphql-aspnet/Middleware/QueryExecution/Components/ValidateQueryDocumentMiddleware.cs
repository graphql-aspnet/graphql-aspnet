// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.QueryExecution.Components
{
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Metrics;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// Validates that a parsed query document is internally consistant
    /// with itself and the target schema.
    /// </summary>
    /// <typeparam name="TSchema">The type of schema this middleware component works for.</typeparam>
    internal class ValidateQueryDocumentMiddleware<TSchema> : IQueryExecutionMiddleware
        where TSchema : class, ISchema
    {
        private readonly IGraphQueryDocumentGenerator<TSchema> _documentGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateQueryDocumentMiddleware{TSchema}"/> class.
        /// </summary>
        /// <param name="documentGenerator">The document generator for the schema
        /// that can validate a document.</param>
        public ValidateQueryDocumentMiddleware(IGraphQueryDocumentGenerator<TSchema> documentGenerator)
        {
            _documentGenerator = Validation.ThrowIfNullOrReturn(documentGenerator, nameof(documentGenerator));
        }

        /// <inheritdoc />
        public Task InvokeAsync(GraphQueryExecutionContext context, GraphMiddlewareInvocationDelegate<GraphQueryExecutionContext> next, CancellationToken cancelToken)
        {
            context.Metrics?.StartPhase(ApolloExecutionPhase.VALIDATION);

            if (context.IsValid && context.QueryPlan == null && context.QueryDocument != null)
            {
                _documentGenerator.ValidateDocument(context.QueryDocument);
                if (!context.QueryDocument.Messages.IsSucessful)
                {
                    context.Messages.AddRange(context.QueryDocument.Messages);
                    context.Cancel();
                }
            }

            context.Metrics?.EndPhase(ApolloExecutionPhase.VALIDATION);

            return next(context, cancelToken);
        }
    }
}