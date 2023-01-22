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
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces;
    using GraphQL.AspNet.Web;
    using GraphQL.AspNet.Web.Exceptions;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// An http query processor that supports processing http requests conforming to the
    /// <c>graphql-multipart-request</c> specification.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor will work with.</typeparam>
    public class MultipartRequestGraphQLHttpProcessor<TSchema> : GraphQLHttpProcessorBase<TSchema>
        where TSchema : class, ISchema
    {
        private static readonly MultipartRequestPayloadAssembler _assembler = new MultipartRequestPayloadAssembler();

        private readonly IFileUploadScalarValueMaker _fileUploadMaker;
        private readonly IQueryResponseWriter<TSchema> _writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartRequestGraphQLHttpProcessor{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The singleton instance of <typeparamref name="TSchema" /> representing this processor works against.</param>
        /// <param name="runtime">The primary runtime instance in which GraphQL requests are processed for <typeparamref name="TSchema" />.</param>
        /// <param name="writer">The result writer capable of converting a <see cref="T:GraphQL.AspNet.Interfaces.Execution.IQueryExecutionResult" /> into a serialized payload
        /// for the given <typeparamref name="TSchema" />.</param>
        /// <param name="fileUploadMaker">A custom maker registered by the server extension
        /// to build file upload scalars directly from a multi-part form.</param>
        /// <param name="logger">A logger instance where this object can write and record log entries.</param>
        public MultipartRequestGraphQLHttpProcessor(
            TSchema schema,
            IGraphQLRuntime<TSchema> runtime,
            IQueryResponseWriter<TSchema> writer,
            IFileUploadScalarValueMaker fileUploadMaker,
            IGraphEventLogger logger = null)
            : base(schema, runtime, logger)
        {
            _fileUploadMaker = Validation.ThrowIfNullOrReturn(fileUploadMaker, nameof(fileUploadMaker));
            _writer = Validation.ThrowIfNullOrReturn(writer, nameof(writer));
        }

        /// <inheritdoc />
        public override async Task InvokeAsync(HttpContext context)
        {
            this.HttpContext = Validation.ThrowIfNullOrReturn(context, nameof(context));

            MultiPartRequestGraphQLPayload payload = null;

            try
            {
                payload = await this.ParseHttpContextAsync();
            }
            catch (HttpContextParsingException ex)
            {
                await this.WriteStatusCodeResponseAsync(ex.StatusCode, ex.Message, context.RequestAborted).ConfigureAwait(false);
                return;
            }

            await this.SubmitQueryAsync(payload, context.RequestAborted).ConfigureAwait(false);
        }

        /// <summary>
        /// Submits the request data to the GraphQL runtime for processing. When overloading in a child class, allows the class
        /// to interject and alter the <paramref name="payload" /> just prior to it being executed by the graphql runtime.
        /// </summary>
        /// <param name="payload">The query data payload parsed from an <see cref="HttpRequest" />; may be null.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        protected virtual async Task SubmitQueryAsync(MultiPartRequestGraphQLPayload payload, CancellationToken cancelToken = default)
        {

        }

        /// <summary>
        /// When overriden in a child class, allows for the alteration of the method by which the various query
        /// parameters are extracted from the <see cref="HttpContext"/> for input to the graphql runtime.
        /// </summary>
        /// <remarks>
        /// Throw an <see cref="HttpContextParsingException"/> to stop execution and quickly write
        /// an error back to the requestor.
        /// </remarks>
        /// <returns>A parsed query data object containing the input parameters for the
        /// graphql runtime or <c>null</c>.</returns>
        protected virtual async Task<MultiPartRequestGraphQLPayload> ParseHttpContextAsync()
        {
            var isPostRequest = string.Equals(this.HttpContext.Request.Method, nameof(HttpMethod.Post), StringComparison.OrdinalIgnoreCase);
            var isMultiPartForm = isPostRequest && this.HttpContext.Request.HasFormContentType;

            if (!isMultiPartForm)
            {
                var singleParser = new GraphQLHttpPayloadParser(this.HttpContext);
                var queryData = await singleParser.ParseAsync();

                return new MultiPartRequestGraphQLPayload(queryData);
            }

            var parts = await this.ExtractMultiPartFormDataAsync().ConfigureAwait(false);
            var payload = await _assembler.AssemblePayload(
                                            parts.Operations,
                                            parts.FileMap,
                                            parts.Files,
                                            this.HttpContext.RequestAborted)
                                    .ConfigureAwait(false);

            return payload;
    }

        /// <summary>
        /// A method that can inspect a form on the <see cref="HttpContext"/> and extract constituent parts in a
        /// manner consistant with the grapql-multipart spec.
        /// </summary>
        /// <remarks>
        /// Spec: <see href="https://github.com/jaydenseric/graphql-multipart-request-spec" />.
        /// </remarks>
        /// <returns>System.ValueTuple&lt;System.String, System.String, List&lt;FileUpload&gt;&gt;.</returns>
        protected virtual async Task<(string Operations, string FileMap, IReadOnlyDictionary<string, FileUpload> Files)> ExtractMultiPartFormDataAsync()
        {
            string operations = null;
            string fileMap = null;
            var files = new Dictionary<string, FileUpload>();

            // check the blobs of the form extracting the required keys
            // and storing any other keys as potential data blobs referenced as "files"
            // by any queries
            foreach (var item in this.HttpContext.Request.Form)
            {
                switch (item.Key)
                {
                    case MultipartRequestConstants.Web.OPERATIONS_FORM_KEY:
                        if (operations != null)
                            throw new InvalidOperationException("Cant define it twice!");

                        operations = item.Value.ToString();
                        break;

                    case MultipartRequestConstants.Web.MAP_FORM_KEY:
                        if (fileMap != null)
                            throw new InvalidOperationException("Cant define it twice!");

                        fileMap = item.Value.ToString();
                        break;

                    default:

                        // treat other unknown blocks as just blobs of data
                        byte[] bytes = Encoding.UTF8.GetBytes(item.Value.ToString());
                        var file = await _fileUploadMaker.CreateFileScalar(item.Key, bytes);

                        files.Add(file.MapKey, file);
                        break;
                }
            }

            // also extract any files actually uploaded
            foreach (var uploadedFile in this.HttpContext.Request.Form.Files)
            {
                var file = await _fileUploadMaker.CreateFileScalar(uploadedFile);
                files.Add(file.MapKey, file);
            }

            return (operations, fileMap, files);
        }
    }
}