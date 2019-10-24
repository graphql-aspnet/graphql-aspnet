// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Defaults
{
    using System.Net;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.QueryExecution;

    /// <summary>
    /// An http processor that only allows authenticated requests through to the runtime.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor is built for.</typeparam>
    public class SecureGraphQLHttpProcessor<TSchema> : DefaultGraphQLHttpProcessor<TSchema>
            where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecureGraphQLHttpProcessor{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="queryPipeline">The query pipeline.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="metricsFactory">The metrics factory.</param>
        /// <param name="logger">The logger.</param>
        public SecureGraphQLHttpProcessor(
            TSchema schema,
            ISchemaPipeline<TSchema, GraphQueryExecutionContext> queryPipeline,
            IGraphResponseWriter<TSchema> writer,
            IGraphQueryExecutionMetricsFactory<TSchema> metricsFactory,
            IGraphEventLogger logger = null)
            : base(schema, queryPipeline, writer, metricsFactory, logger)
        {
        }

        /// <summary>
        /// Submits the GraphQL query for processing.
        /// </summary>
        /// <param name="queryData">The query data.</param>
        /// <returns>Task&lt;IActionResult&gt;.</returns>
        public override async Task SubmitGraphQLQuery(GraphQueryData queryData)
        {
            if (this.User?.Identity == null || !this.User.Identity.IsAuthenticated)
            {
                await this.WriteStatusCodeResponse(HttpStatusCode.Unauthorized, ERROR_UNAUTHORIZED).ConfigureAwait(false);
                return;
            }

            await base.SubmitGraphQLQuery(queryData);
        }
    }
}