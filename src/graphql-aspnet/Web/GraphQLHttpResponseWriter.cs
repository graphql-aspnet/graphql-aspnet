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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Response;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A helper class that will translate <see cref="IGraphOperationResult"/> into a properly structured <see cref="HttpResponse"/>
    /// using the provided flags and DI retrieved <see cref="IGraphQueryResponseWriter"/>.
    /// </summary>
    public class GraphQLHttpResponseWriter
    {
        /// <summary>
        /// When exceptions are exposed, this is the text phrase sent to the client as the complete response when no graphql
        /// writer is supplied to this action result.
        /// </summary>
        public const string NO_WRITER_WITH_DETAIL = "Invalid result. No " + nameof(IGraphQueryResponseWriter) + " was " +
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
        public const string NO_RESULT_WITH_DETAIL = "Invalid result. The " + nameof(IGraphOperationResult) + " passed to the " +
                                                    "the " + nameof(GraphQLHttpResponseWriter) + " was null.";

        /// <summary>
        /// When exceptions are NOT exposed, this is the text phrase sent to the client as the complete response when no graphql
        /// operation result is supplied to this action result.
        /// </summary>
        public const string NO_RESULT_NO_DETAIL = "An error occured processing your graphql query. Contact an administrator.";

        private readonly IGraphOperationResult _result;
        private readonly IGraphQueryResponseWriter _documentWriter;
        private readonly GraphQLResponseOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLHttpResponseWriter" /> class.
        /// </summary>
        /// <param name="result">The graphql result to serialize.</param>
        /// <param name="documentWriter">The document writer to perform the serailization.</param>
        /// <param name="exposeMetrics">if set to <c>true</c> any metrics contained on the result will be exposed and sent to the requestor.</param>
        /// <param name="exposeExceptions">if set to <c>true</c> exceptions will be writen to the response stream; otherwise false.</param>
        public GraphQLHttpResponseWriter(
            IGraphOperationResult result,
            IGraphQueryResponseWriter documentWriter,
            bool exposeMetrics,
            bool exposeExceptions)
        {
            _result = result;
            _documentWriter = documentWriter;
            _options = new GraphQLResponseOptions()
            {
                ExposeExceptions = exposeExceptions,
                ExposeMetrics = exposeMetrics,
            };
        }

        /// <summary>
        /// Executes the result operation of the action method asynchronously. This method is called by MVC to process
        /// the result of an action method.
        /// </summary>
        /// <param name="context">The context in which the result is executed. The context information includes
        /// information about the action that was executed and request information.</param>
        /// <returns>A task that represents the asynchronous execute operation.</returns>
        public async Task WriteResultAsync(HttpContext context)
        {
            if (_documentWriter == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                if (_options.ExposeExceptions)
                   await context.Response.WriteAsync(NO_WRITER_WITH_DETAIL).ConfigureAwait(false);
                else
                    await context.Response.WriteAsync(NO_WRITER_NO_DETAIL).ConfigureAwait(false);
            }
            else if (_result == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                if (_options.ExposeExceptions)
                    await context.Response.WriteAsync(NO_RESULT_WITH_DETAIL).ConfigureAwait(false);
                else
                    await context.Response.WriteAsync(NO_RESULT_NO_DETAIL).ConfigureAwait(false);
            }
            else
            {
                await _documentWriter.WriteAsync(context.Response.Body, _result, _options).ConfigureAwait(false);
            }
        }
    }
}