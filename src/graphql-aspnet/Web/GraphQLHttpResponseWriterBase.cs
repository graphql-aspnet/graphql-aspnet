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
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Response;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A base http response writer class that provides common error handling
    /// for multiple http response writers.
    /// </summary>
    public abstract class GraphQLHttpResponseWriterBase
    {
        /// <summary>
        /// When exceptions are exposed, this is the text phrase sent to the client as the complete response when no graphql
        /// writer is supplied to this action result.
        /// </summary>
        public const string NO_WRITER_WITH_DETAIL = "Invalid result. No " + nameof(IQueryResponseWriter) + " was " +
                                                    "provided so the resultant data could not be serialized.";

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
                                                    "the " + nameof(GraphQLHttpResponseWriter) + " was null.";

        /// <summary>
        /// When exceptions are NOT exposed, this is the text phrase sent to the client as the complete response when no graphql
        /// operation result is supplied to this action result.
        /// </summary>
        public const string NO_RESULT_NO_DETAIL = "An error occured processing your graphql query. Contact an administrator.";

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLHttpResponseWriterBase" /> class.
        /// </summary>
        /// <param name="documentWriter">The document writer to perform the serailization.</param>
        /// <param name="exposeMetrics">if set to <c>true</c> any metrics contained on the result will be exposed and sent to the requestor.</param>
        /// <param name="exposeExceptions">if set to <c>true</c> exceptions will be writen to the response stream; otherwise false.</param>
        public GraphQLHttpResponseWriterBase(
            IQueryResponseWriter documentWriter,
            bool exposeMetrics,
            bool exposeExceptions)
        {
            this.DocumentWriter = documentWriter;
            this.WriterOptions = new ResponseWriterOptions()
            {
                ExposeMetrics = exposeMetrics,
                ExposeExceptions = exposeExceptions,
            };
        }

        /// <summary>
        /// Writes the contained result(s) to the provided http context. This method is usually called by
        /// an http processor to serialize a completed query.
        /// </summary>
        /// <param name="context">The context to which the result should be written.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the asynchronous execute operation.</returns>
        public abstract Task WriteResultAsync(HttpContext context, CancellationToken cancelToken = default);

        /// <summary>
        /// Inspects this http writer instance to determine if writing can take place. If it can't, an appropriate
        /// error message is returned. Returns <c>null</c> when valid.
        /// </summary>
        /// <param name="errorMessage">When this method returns false, A human-readable
        /// error message will be written to this variable with the expectation that
        /// is is rendered to the caller.</param>
        /// <returns><c>true</c> if this instance is in a state such that it can
        /// process a graphql result; otherwise, <c>false</c>.</returns>
        protected virtual bool CanWriteResponse(out string errorMessage)
        {
            errorMessage = null;
            if (this.DocumentWriter == null)
            {
                errorMessage = this.WriterOptions.ExposeExceptions
                    ? NO_WRITER_WITH_DETAIL
                    : NO_WRITER_NO_DETAIL;

                return false;
            }

            if (!this.HasValidResults())
            {
                errorMessage = this.WriterOptions.ExposeExceptions
                    ? NO_RESULT_WITH_DETAIL
                    : NO_RESULT_NO_DETAIL;

                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether this instance has a set of results such that something meaningful could
        /// be written to a response stream.
        /// </summary>
        /// <returns><c>true</c> if this instance has valid results; otherwise, <c>false</c>.</returns>
        protected abstract bool HasValidResults();

        /// <summary>
        /// Gets a writer instance to which a query response can be written.
        /// </summary>
        /// <value>The document writer.</value>
        protected IQueryResponseWriter DocumentWriter { get; }

        /// <summary>
        /// Gets a collection of writer options that can be passed to <see cref="DocumentWriter"/>
        /// for configuring the output stream.
        /// </summary>
        /// <value>The writer options.</value>
        protected virtual ResponseWriterOptions WriterOptions { get; }
    }
}