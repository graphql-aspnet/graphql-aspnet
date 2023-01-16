// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Web;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// An http query processor that supports processing http requests conforming to the
    /// <c>graphql-multipart-request</c> specification.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor will work with.</typeparam>
    public class MultipartRequestGraphQLHttpProcessor<TSchema> : GraphQLHttpProcessorBase<TSchema>
        where TSchema : class, ISchema
    {
        private readonly IQueryResponseWriter<TSchema> _writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartRequestGraphQLHttpProcessor{TSchema}"/> class.
        /// </summary>
        /// <param name="schema">The singleton instance of <typeparamref name="TSchema" /> representing this processor works against.</param>
        /// <param name="runtime">The primary runtime instance in which GraphQL requests are processed for <typeparamref name="TSchema" />.</param>
        /// <param name="writer">The result writer capable of converting a <see cref="T:GraphQL.AspNet.Interfaces.Execution.IQueryExecutionResult" /> into a serialized payload
        /// for the given <typeparamref name="TSchema" />.</param>
        /// <param name="logger">A logger instance where this object can write and record log entries.</param>
        public MultipartRequestGraphQLHttpProcessor(
            TSchema schema,
            IGraphQLRuntime<TSchema> runtime,
            IQueryResponseWriter<TSchema> writer,
            IGraphEventLogger logger = null)
            : base(schema, runtime, logger)
        {
            _writer = Validation.ThrowIfNullOrReturn(writer, nameof(writer));
        }

        /// <inheritdoc />
        public override Task InvokeAsync(HttpContext context)
        {
            return null;
        }
    }
}