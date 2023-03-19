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
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A GraphQL response writer that can take in a collection of results and render them as a single json array
    /// to a stream.
    /// </summary>
    public class BatchGraphQLHttpResponseWriter
    {
        /// <summary>
        /// When exceptions are exposed, this is the text phrase sent to the client as the complete response when no graphql
        /// writer is supplied to this action result.
        /// </summary>
        public const string NO_WRITER_WITH_DETAIL = "Invalid result. No " + nameof(IQueryResponseWriter) + " was " +
                                                    "provided. The resultant data could not be serialized.";

        /// <summary>
        /// When exceptions are NOT exposed, this is the text phrase sent to the client as the complete response when no graphql
        /// writer is supplied to this action result.
        /// </summary>
        public const string NO_WRITER_NO_DETAIL = "An error occured processing your graphql query. Contact an administrator.";

        /// <summary>
        /// When exceptions are exposed, this is the text phrase sent to the client as the complete response when no graphql
        /// operation result is supplied to this action result.
        /// </summary>
        public const string NO_RESULT_WITH_DETAIL = "Invalid result. The " + nameof(IQueryExecutionResult) + " passed to the " +
                                                    "the " + nameof(BatchGraphQLHttpResponseWriter) + " was null.";

        /// <summary>
        /// When exceptions are NOT exposed, this is the text phrase sent to the client as the complete response when no graphql
        /// operation result is supplied to this action result.
        /// </summary>
        public const string NO_RESULT_NO_DETAIL = "An error occured processing your graphql query. Contact an administrator.";

        private readonly IReadOnlyList<IQueryExecutionResult> _results;
        private readonly IQueryExecutionResult _singleResult;
        private readonly IQueryResponseWriter _documentWriter;
        private readonly ResponseWriterOptions _options;
        private readonly JsonWriterOptions _writerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchGraphQLHttpResponseWriter" /> class.
        /// </summary>
        /// <param name="schema">The schema governing the use of this writer.</param>
        /// <param name="results">The collection of graphql result to serialize as a batch.</param>
        /// <param name="documentWriter">The document writer to perform the serailization.</param>
        /// <param name="exposeMetrics">if set to <c>true</c> any metrics contained on the result will be exposed and sent to the requestor.</param>
        /// <param name="exposeExceptions">if set to <c>true</c> exceptions will be writen to the response stream; otherwise false.</param>
        public BatchGraphQLHttpResponseWriter(
            ISchema schema,
            IReadOnlyList<IQueryExecutionResult> results,
            IQueryResponseWriter documentWriter,
            bool exposeMetrics,
            bool exposeExceptions)
            : this(schema, documentWriter, exposeMetrics, exposeExceptions)
        {
            _results = Validation.ThrowIfNullOrReturn(results, nameof(results));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchGraphQLHttpResponseWriter" /> class.
        /// </summary>
        /// <param name="schema">The schema governing the use of this writer.</param>
        /// <param name="result">The single graphql result to serialize.</param>
        /// <param name="documentWriter">The document writer to perform the serailization.</param>
        /// <param name="exposeMetrics">if set to <c>true</c> any metrics contained on the result will be exposed and sent to the requestor.</param>
        /// <param name="exposeExceptions">if set to <c>true</c> exceptions will be writen to the response stream; otherwise false.</param>
        public BatchGraphQLHttpResponseWriter(
            ISchema schema,
            IQueryExecutionResult result,
            IQueryResponseWriter documentWriter,
            bool exposeMetrics,
            bool exposeExceptions)
            : this(schema, documentWriter, exposeMetrics, exposeExceptions)
        {
            _singleResult = Validation.ThrowIfNullOrReturn(result, nameof(result));
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="BatchGraphQLHttpResponseWriter" /> class from being created.
        /// </summary>
        /// <param name="schema">The schema governing the use of this writer.</param>
        /// <param name="documentWriter">The document writer to perform the serailization.</param>
        /// <param name="exposeMetrics">if set to <c>true</c> any metrics contained on the result will be exposed and sent to the requestor.</param>
        /// <param name="exposeExceptions">if set to <c>true</c> exceptions will be writen to the response stream; otherwise false.</param>
        private BatchGraphQLHttpResponseWriter(
            ISchema schema,
            IQueryResponseWriter documentWriter,
            bool exposeMetrics,
            bool exposeExceptions)
        {
            _documentWriter = documentWriter;
            _options = new ResponseWriterOptions()
            {
                ExposeExceptions = exposeExceptions,
                ExposeMetrics = exposeMetrics,
            };

            _writerOptions = new JsonWriterOptions()
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
        public virtual async Task WriteResultAsync(HttpContext context, CancellationToken cancelToken = default)
        {
            if (_documentWriter == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                if (_options.ExposeExceptions)
                    await context.Response.WriteAsync(NO_WRITER_WITH_DETAIL, cancelToken).ConfigureAwait(false);
                else
                    await context.Response.WriteAsync(NO_WRITER_NO_DETAIL, cancelToken).ConfigureAwait(false);

                return;
            }

            if (_results == null && _singleResult == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                if (_options.ExposeExceptions)
                    await context.Response.WriteAsync(NO_RESULT_WITH_DETAIL, cancelToken).ConfigureAwait(false);
                else
                    await context.Response.WriteAsync(NO_RESULT_NO_DETAIL, cancelToken).ConfigureAwait(false);

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
                    writer = new Utf8JsonWriter(context.Response.Body, _writerOptions);

                    writer.WriteStartArray();

                    foreach (var result in _results)
                        _documentWriter.Write(writer, result, _options);

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
            await _documentWriter.WriteAsync(context.Response.Body, _singleResult, _options, cancelToken: cancelToken);
        }
    }
}