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
    using System.Net;
    using System.Text;
    using System.Text.Json.Nodes;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Configuration;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Exceptions;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Model;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Web;
    using GraphQL.AspNet.Web.Exceptions;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// An assembler that can take the raw values required by the spec and assembly a valid payload that
    /// can be executed against the runtime.
    /// </summary>
    /// <remarks>Spec: <see href="https://github.com/jaydenseric/graphql-multipart-request-spec" />.</remarks>
    public partial class MultiPartHttpFormPayloadParser
    {
        private readonly HttpContext _context;
        private readonly IFileUploadScalarValueMaker _fileUploadScalarMaker;
        private readonly IMultipartRequestConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiPartHttpFormPayloadParser" /> class.
        /// </summary>
        /// <param name="context">The context to be parsed.</param>
        /// <param name="fileUploadScalarMaker">A maker that can create individual file upload scalars usable
        /// by the graphql engine.</param>
        /// <param name="configuration">A configuration object to determine how the assembler
        /// will assembly a payload from its constituent parts.</param>
        public MultiPartHttpFormPayloadParser(
            HttpContext context,
            IFileUploadScalarValueMaker fileUploadScalarMaker,
            IMultipartRequestConfiguration configuration = null)
        {
            _context = Validation.ThrowIfNullOrReturn(context, nameof(context));
            _fileUploadScalarMaker = Validation.ThrowIfNullOrReturn(fileUploadScalarMaker, nameof(fileUploadScalarMaker));
            _config = configuration ?? new MultipartRequestConfiguration();

            if (!_context.IsMultipartFormRequest())
            {
                throw new HttpContextParsingException(
                    HttpStatusCode.BadRequest,
                    "Invalid request, expected a multi-part form submission.");
            }
        }

        /// <summary>
        /// Parses the contained http context and attempts to build out an appropriate payload
        /// that can be submitted to the graphql engine.
        /// </summary>
        /// <returns>&lt;MultiPartRequestGraphQLPayload&gt;</returns>
        public virtual async Task<MultiPartRequestGraphQLPayload> ParseAsync()
        {
            string operations = null;
            string fileMap = null;
            var files = new Dictionary<string, FileUpload>();

            // check the blobs of the form extracting the required keys
            // and storing any other keys as potential data blobs referenced as "files"
            // by any queries
            if (_context.Request.Form != null)
            {
                foreach (var item in _context.Request.Form)
                {
                    switch (item.Key)
                    {
                        case MultipartRequestConstants.Web.OPERATIONS_FORM_KEY:
                            if (operations != null)
                            {
                                throw new HttpContextParsingException(
                                    errorMessage: $"The '{MultipartRequestConstants.Web.OPERATIONS_FORM_KEY}' form field is defined " +
                                    $"more than once on the request. It must be unique.");
                            }

                            operations = item.Value.ToString();
                            break;

                        case MultipartRequestConstants.Web.MAP_FORM_KEY:
                            if (fileMap != null)
                            {
                                throw new HttpContextParsingException(
                                    errorMessage: $"The '{MultipartRequestConstants.Web.MAP_FORM_KEY}' form field is defined " +
                                    $"more than once on the request. It must be unique.");
                            }

                            fileMap = item.Value.ToString();
                            break;

                        default:

                            // treat other unknown form fields as just blobs of data that may be
                            // merged via a map
                            byte[] bytes = Encoding.UTF8.GetBytes(item.Value.ToString());
                            var file = await _fileUploadScalarMaker.CreateFileScalarAsync(item.Key, bytes);

                            this.ValidateAndAppendFileOrThrow(files, file);
                            break;
                    }
                }

                // also extract any files actually uploaded
                if (_context.Request.Form.Files != null)
                {
                    foreach (var uploadedFile in _context.Request.Form.Files)
                    {
                        var file = await _fileUploadScalarMaker.CreateFileScalarAsync(uploadedFile);
                        this.ValidateAndAppendFileOrThrow(files, file);
                    }
                }
            }

            var payload = await this.AssemblePayload(
               operations,
               fileMap,
               files,
               _context.RequestAborted)
               .ConfigureAwait(false);

            return payload;
        }

        /// <summary>
        /// Validates an assembled file reference for internal consistancy and add it to the
        /// collection of parsed files on the request. An exception should be thrown
        /// if the file is not correctly added.
        /// </summary>
        /// <param name="fileList">The file list to append the new file to.</param>
        /// <param name="newFile">The file to inspect.</param>
        protected virtual void ValidateAndAppendFileOrThrow(Dictionary<string, FileUpload> fileList, FileUpload newFile)
        {
            Validation.ThrowIfNull(fileList, nameof(fileList));
            if (newFile == null || string.IsNullOrWhiteSpace(newFile.MapKey))
            {
                throw new HttpContextParsingException(
                    HttpStatusCode.BadRequest,
                    $"A file or form field was encountered that contains no name. All form fields must " +
                    $"be uniquely named.");
            }

            if (fileList.ContainsKey(newFile.MapKey))
            {
                throw new HttpContextParsingException(
                    HttpStatusCode.BadRequest,
                    $"A file or form field '{newFile.MapKey}' was already parsed. All form fields and file references must " +
                    $"be uniquely named.");
            }

            fileList.Add(newFile.MapKey, newFile);
        }

        /// <summary>
        /// Assembles a valid query payload from the constituent parts defined by the specification.
        /// </summary>
        /// <param name="operations">The json object representing operation(s) provided on the request.</param>
        /// <param name="map">A map to connect files to appropriate variables in the operations collection.</param>
        /// <param name="files">The files found on the request.</param>
        /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A Task&lt;MultiPartRequestGraphQLPayload&gt; representing the asynchronous operation.</returns>
        protected virtual Task<MultiPartRequestGraphQLPayload> AssemblePayload(
            string operations,
            string map = null,
            IReadOnlyDictionary<string, FileUpload> files = null,
            CancellationToken cancellationToken = default)
        {
            operations = Validation.ThrowIfNullWhiteSpaceOrReturn(operations, nameof(operations));
            map = map?.Trim();

            JsonNode topNode;

            try
            {
                topNode = JsonNode.Parse(operations, _nodeOptions, _documentOptions);
            }
            catch (Exception ex)
            {
                throw new InvalidMultiPartOperationException(
                    $"Unable to parse the '{MultipartRequestConstants.Web.OPERATIONS_FORM_KEY}' form field. The provided value is not a " +
                    $"valid json string. See inner exception for details.",
                    ex);
            }

            this.InjectMappedFileMarkers(topNode, map);

            MultiPartRequestGraphQLPayload payload;

            if (!topNode.IsArray())
            {
                // single query, No Batch Processing
                var query = this.ConvertNodeToQueryData(topNode, files: files);
                payload = new MultiPartRequestGraphQLPayload(query);
            }
            else
            {
                // multiple queries, Batch Processing
                var queries = new List<GraphQueryData>();
                var i = 0;
                foreach (var node in topNode.AsArray())
                {
                    var query = this.ConvertNodeToQueryData(node, i++, files);
                    queries.Add(query);
                }

                payload = new MultiPartRequestGraphQLPayload(queries);
            }

            return Task.FromResult(payload);
        }
    }
}