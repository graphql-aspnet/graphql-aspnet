// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Web
{
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A helper class that will translate <see cref="IQueryExecutionResult"/> into a properly structured <see cref="HttpResponse"/>
    /// using the provided flags and DI retrieved <see cref="IQueryResponseWriter"/>.
    /// </summary>
    public class GraphQLHttpResponseWriter : GraphQLHttpResponseWriterBase
    {
        private readonly IQueryExecutionResult _result;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLHttpResponseWriter" /> class.
        /// </summary>
        /// <param name="result">The graphql result to serialize.</param>
        /// <param name="documentWriter">The document writer to perform the serailization.</param>
        /// <param name="exposeMetrics">if set to <c>true</c> any metrics contained on the result will be exposed and sent to the requestor.</param>
        /// <param name="exposeExceptions">if set to <c>true</c> exceptions will be writen to the response stream; otherwise false.</param>
        public GraphQLHttpResponseWriter(
            IQueryExecutionResult result,
            IQueryResponseWriter documentWriter,
            bool exposeMetrics,
            bool exposeExceptions)
            : base(documentWriter, exposeMetrics, exposeExceptions)
        {
            _result = result;
        }

        /// <inheritdoc />
        public override async Task WriteResultAsync(HttpContext context, CancellationToken cancelToken = default)
        {
            var canWrite = this.CanWriteResponse(out var validationError);
            if (!canWrite)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync(validationError, cancelToken).ConfigureAwait(false);
                return;
            }

            await this.DocumentWriter.WriteAsync(
                context.Response.Body,
                _result,
                this.WriterOptions,
                cancelToken);
        }

        /// <inheritdoc />
        protected override bool HasValidResults()
        {
            return _result != null;
        }
    }
}