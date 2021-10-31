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
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// Creates a new query plan from a parsed syntax tree when required.
    /// </summary>
    /// <typeparam name="TSchema">The type of schema this middleware component works for.</typeparam>
    public class GenerateQueryPlanMiddleware<TSchema> : IQueryExecutionMiddleware
        where TSchema : class, ISchema
    {
        private readonly IGraphQueryPlanGenerator<TSchema> _planGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateQueryPlanMiddleware{TSchema}"/> class.
        /// </summary>
        /// <param name="planGenerator">The plan generator.</param>
        public GenerateQueryPlanMiddleware(IGraphQueryPlanGenerator<TSchema> planGenerator)
        {
            _planGenerator = Validation.ThrowIfNullOrReturn(planGenerator, nameof(planGenerator));
        }

        /// <summary>
        /// Invokes this middleware component allowing it to perform its work against the supplied context.
        /// </summary>
        /// <param name="context">The context containing the request passed through the pipeline.</param>
        /// <param name="next">The delegate pointing to the next piece of middleware to be invoked.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>Task.</returns>
        public async Task InvokeAsync(GraphQueryExecutionContext context, GraphMiddlewareInvocationDelegate<GraphQueryExecutionContext> next, CancellationToken cancelToken)
        {
            if (context.IsValid && context.QueryPlan == null && context.SyntaxTree != null)
            {
                context.Metrics?.StartPhase(ApolloExecutionPhase.VALIDATION);
                context.QueryPlan = await _planGenerator.CreatePlan(context.SyntaxTree).ConfigureAwait(false);
                context.Messages.AddRange(context.QueryPlan.Messages);
                context.Metrics?.EndPhase(ApolloExecutionPhase.VALIDATION);

                if (context.QueryPlan.IsValid)
                    context.Logger?.QueryPlanGenerated(context.QueryPlan);
            }

            await next(context, cancelToken).ConfigureAwait(false);
        }
    }
}