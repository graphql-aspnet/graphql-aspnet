// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests.Web
{
    using System.Collections.Generic;
    using System.Net;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Response;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Web;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A GraphQL response writer that can take in a collection of results and render them as a single json array
    /// to a stream.
    /// </summary>
    public class BatchGraphQLHttpResponseWriter : GraphQLHttpResponseWriterBase
    {
        private readonly IReadOnlyList<IQueryExecutionResult> _results;
        private readonly IQueryExecutionResult _singleResult;
        private readonly JsonWriterOptions _utfWriterOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchGraphQLHttpResponseWriter" /> class.
        /// </summary>
        /// <param name="schema">The schema governing the use of this writer.</param>
        /// <param name="results">The collection of graphql result to serialize as a batch.</param>
        /// <param name="documentWriter">The document writer to perform the serailization.</param>
        public BatchGraphQLHttpResponseWriter(
            ISchema schema,
            IReadOnlyList<IQueryExecutionResult> results,
            IQueryResponseWriter documentWriter)
            : this(schema, documentWriter)
        {
            _results = results;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchGraphQLHttpResponseWriter" /> class.
        /// </summary>
        /// <param name="schema">The schema governing the use of this writer.</param>
        /// <param name="result">The single graphql result to serialize.</param>
        /// <param name="documentWriter">The document writer to perform the serailization.</param>
        public BatchGraphQLHttpResponseWriter(
            ISchema schema,
            IQueryExecutionResult result,
            IQueryResponseWriter documentWriter)
            : this(schema, documentWriter)
        {
            _singleResult = result;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="BatchGraphQLHttpResponseWriter" /> class from being created.
        /// </summary>
        /// <param name="schema">The schema governing the use of this writer.</param>
        /// <param name="documentWriter">The document writer to perform the serailization.</param>
        private BatchGraphQLHttpResponseWriter(
            ISchema schema,
            IQueryResponseWriter documentWriter)
            : base(
                  documentWriter,
                  schema?.Configuration?.ResponseOptions?.ExposeMetrics ?? false,
                  schema?.Configuration?.ResponseOptions?.ExposeExceptions ?? false)
        {
            Validation.ThrowIfNull(schema, nameof(schema));

            _utfWriterOptions = new JsonWriterOptions()
            {
                Indented = schema.Configuration.ResponseOptions.IndentDocument,
            };
        }

        /// <summary>
        /// Executes the result operation of the action method asynchronously. This method is called by ASP.NET to process
        /// the result of an action method.
        /// </summary>
        /// <param name="context">The context in which the result is executed. The context information includes
        /// information about the action that was executed and request information.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the asynchronous execute operation.</returns>
        public override async Task WriteResultAsync(HttpContext context, CancellationToken cancelToken = default)
        {
            var canWrite = this.CanWriteResponse(out var validationError);
            if (!canWrite)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync(validationError, cancelToken).ConfigureAwait(false);
                return;
            }

            // ************************************
            //  Write a Batch inside an array
            // ************************************
            if (_results != null)
            {
                // generate and control the utf writer so we can append
                // the encapsulating array if need be.
                Utf8JsonWriter writer = null;
                try
                {
                    writer = new Utf8JsonWriter(context.Response.Body, _utfWriterOptions);

                    writer.WriteStartArray();

                    foreach (var result in _results)
                        this.DocumentWriter.Write(writer, result, this.WriterOptions);

                    writer.WriteEndArray();
                }
                finally
                {
                    if (writer != null)
                    {
                        await writer.FlushAsync().ConfigureAwait(false);
                        await writer.DisposeAsync().ConfigureAwait(false);
                    }
                }

                return;
            }

            // ************************************
            //  Write a single non-batched result
            // ************************************
            await this.DocumentWriter.WriteAsync(context.Response.Body, _singleResult, this.WriterOptions, cancelToken: cancelToken);
        }

        /// <inheritdoc />
        protected override bool HasValidResults()
        {
            return _results != null || _singleResult != null;
        }
    }
}