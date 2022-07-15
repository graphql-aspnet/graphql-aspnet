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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.Metrics;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// Creates a new query plan from a parsed syntax tree when required.
    /// </summary>
    /// <typeparam name="TSchema">The type of schema this middleware component works for.</typeparam>
    public class GenerateQueryPlanMiddleware<TSchema> : IQueryExecutionMiddleware
        where TSchema : class, ISchema
    {
        private readonly TSchema _schema;
        private readonly IGraphQueryDocumentGenerator<TSchema> _documentGenerator;
        private readonly IGraphQueryPlanGenerator<TSchema> _planGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateQueryPlanMiddleware{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The target schema.</param>
        /// <param name="documentGenerator">The document generator used to validate
        /// document instances.</param>
        /// <param name="planGenerator">The plan generator.</param>
        public GenerateQueryPlanMiddleware(
            TSchema schema,
            IGraphQueryDocumentGenerator<TSchema> documentGenerator,
            IGraphQueryPlanGenerator<TSchema> planGenerator)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _documentGenerator = Validation.ThrowIfNullOrReturn(documentGenerator, nameof(documentGenerator));
            _planGenerator = Validation.ThrowIfNullOrReturn(planGenerator, nameof(planGenerator));
        }

        /// <inheritdoc />
        public async Task InvokeAsync(GraphQueryExecutionContext context, GraphMiddlewareInvocationDelegate<GraphQueryExecutionContext> next, CancellationToken cancelToken)
        {
            if (context.IsValid && context.QueryPlan == null && context.Operation != null)
            {
                context.QueryPlan = await _planGenerator
                    .CreatePlan(context.Operation)
                    .ConfigureAwait(false);

                context.QueryPlan.IsCacheable = context.Operation.AllDirectives.Count == 0;
                context.Messages.AddRange(context.QueryPlan.Messages);

                if (context.QueryPlan.IsValid)
                    context.Logger?.QueryPlanGenerated(context.QueryPlan);
            }

            await next(context, cancelToken).ConfigureAwait(false);
        }
    }
}