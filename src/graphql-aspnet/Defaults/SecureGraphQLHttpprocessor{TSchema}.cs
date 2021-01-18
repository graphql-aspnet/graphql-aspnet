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
    using GraphQL.AspNet.Interfaces.TypeSystem;

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
        /// <param name="runtime">The runtime.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="logger">The logger.</param>
        public SecureGraphQLHttpProcessor(
            TSchema schema,
            IGraphQLRuntime<TSchema> runtime,
            IGraphResponseWriter<TSchema> writer,
            IGraphEventLogger logger = null)
            : base(schema, runtime, writer, logger)
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

            await base
                .SubmitGraphQLQuery(queryData)
                .ConfigureAwait(false);
        }
    }
}