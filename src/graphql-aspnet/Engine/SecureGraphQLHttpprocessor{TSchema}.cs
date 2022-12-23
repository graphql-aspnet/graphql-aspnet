// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// An http processor that only allows authenticated requests through to the runtime.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor is built for.</typeparam>
    public class SecureGraphQLHttpProcessor<TSchema> : DefaultGraphQLHttpProcessor<TSchema>
            where TSchema : class, ISchema
    {
        /// <summary>
        /// An error message constant, in english, providing the text  to return to teh caller when a 401 occurs.
        /// </summary>
        protected const string ERROR_UNAUTHORIZED = "Unauthorized";

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureGraphQLHttpProcessor{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The singleton instance of <typeparamref name="TSchema"/> representing this processor works against.</param>
        /// <param name="runtime">The primary runtime instance in which GraphQL requests are processed for <typeparamref name="TSchema"/>.</param>
        /// <param name="writer">The result writer capable of converting a <see cref="IQueryOperationResult"/> into a serialized payload
        /// for the given <typeparamref name="TSchema"/>.</param>
        /// <param name="logger">A logger instance where this object can write and record log entries.</param>
        public SecureGraphQLHttpProcessor(
            TSchema schema,
            IGraphQLRuntime<TSchema> runtime,
            IQueryResponseWriter<TSchema> writer,
            IGraphEventLogger logger = null)
            : base(schema, runtime, writer, logger)
        {
        }

        /// <inheritdoc />
        protected override async Task SubmitQueryAsync(GraphQueryData queryData, CancellationToken cancelToken = default)
        {
            if (this.User?.Identity == null || !this.User.Identity.IsAuthenticated)
            {
                await this.WriteStatusCodeResponseAsync(HttpStatusCode.Unauthorized, ERROR_UNAUTHORIZED, cancelToken).ConfigureAwait(false);
                return;
            }

            await base
                .SubmitQueryAsync(queryData, cancelToken)
                .ConfigureAwait(false);
        }
    }
}