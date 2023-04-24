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
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// An assembler that can take the raw values required by the spec and assembly a valid payload that
    /// can be executed against the runtime.
    /// </summary>
    /// <remarks>Spec: <see href="https://github.com/jaydenseric/graphql-multipart-request-spec" />.</remarks>
    public partial class MultiPartHttpFormPayloadParser
    {
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        protected record PendingBlob(string MapKey, StringValues Data);
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter

        private readonly HttpContext _context;
        private readonly IFileUploadScalarValueMaker _fileUploadScalarMaker;
        private readonly IMultipartRequestConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiPartHttpFormPayloadParser"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="fileUploadScalarMaker">The file upload scalar maker.</param>
        /// <param name="configuration">The configuration.</param>
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
            Dictionary<string, FileUpload> files = null;

            var pendingBlobs = new List<PendingBlob>();

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
                            // store a reference to any incoming blobs but don't parse them just yet
                            // we need to ensure configuration maximums aren't exceeded
                            pendingBlobs.Add(new (item.Key, item.Value));
                            break;
                    }
                }

                this.ValidateFileDataOrThrow(pendingBlobs, _context.Request.Form.Files);
                files = await this.CreateFilesCollection(pendingBlobs, _context.Request.Form.Files);
            }

            var payload = await this.AssemblePayload(
               operations,
               fileMap,
               files,
               _context.RequestAborted).ConfigureAwait(false);

            return payload;
        }

        /// <summary>
        /// Validates the two collections of pending files meet the requirements of this schema before attempting to
        /// process and assemble the files. If this method does not throw an exception the file sets are valid.
        /// </summary>
        /// <param name="blobs">The blobs that will be converted to <see cref="FileUpload"/> objects.</param>
        /// <param name="files">The aspnet files that will be converted to <see cref="FileUpload"/> objects.</param>
        protected virtual void ValidateFileDataOrThrow(
            IReadOnlyList<PendingBlob> blobs,
            IReadOnlyList<IFormFile> files)
        {
            // are files present and are they allowed ?
            var hasBlobs = blobs != null && blobs.Count > 0;
            var hasFiles = files != null && files.Count > 0;

            if ((hasBlobs || hasFiles) && !_config.RequestMode.IsFileUploadEnabled())
            {
                throw new HttpContextParsingException(
               errorMessage: $"Unable to process the request. The target schema does not allow file uploads. Ensure " +
               $"no files or unexpected form fields are attached to the request and try again.");
            }

            // are we within the limits of each type of file ?
            var blobsExceeded = hasBlobs && _config.MaxBlobCount.HasValue && blobs.Count > _config.MaxBlobCount;
            var filesExceeded = hasFiles && _config.MaxFileCount.HasValue && files.Count > _config.MaxFileCount;

            if (blobsExceeded || filesExceeded)
            {
                throw new HttpContextParsingException(
                errorMessage: $"Maxium allowed files exceeeded. {blobs.Count} of {_config.MaxBlobCount} allowed blobs submitted and" +
                $"{files.Count} of {_config.MaxFileCount} allowed files were submitted.");
            }
        }

        /// <summary>
        /// Attempts to create a fully formed collection of <see cref="FileUpload"/> from the blobs and
        /// form files encountered on the primary request.
        /// </summary>
        /// <param name="pendingBlobs">The pending blobs read from the post body.</param>
        /// <param name="files">The files pulled from the request.</param>
        /// <returns>Dictionary&lt;System.String, FileUpload&gt;.</returns>
        protected virtual async Task<Dictionary<string, FileUpload>> CreateFilesCollection(
            IReadOnlyList<PendingBlob> pendingBlobs,
            IFormFileCollection files)
        {
            // send all files off to the maker for processing
            // this can be instant, but for large files it may take a while and can be done asyncronously
            var fileTasks = new List<Task<FileUpload>>((pendingBlobs?.Count ?? 0) + (files?.Count ?? 0));
            if (pendingBlobs != null)
            {
                foreach (var blob in pendingBlobs)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(blob.Data.ToString());
                    var task = _fileUploadScalarMaker.CreateFileScalarAsync(blob.MapKey, bytes);
                    fileTasks.Add(task);
                }
            }

            if (files != null)
            {
                foreach (var fileData in files)
                {
                    var task = _fileUploadScalarMaker.CreateFileScalarAsync(fileData);
                    fileTasks.Add(task);
                }
            }

            if (fileTasks.Count == 0)
                return null;

            await Task.WhenAll(fileTasks);

            // fill out the result object with all the indexed files
            var filesOut = new Dictionary<string, FileUpload>();
            foreach (var task in fileTasks)
            {
                if (task.IsFaulted)
                    await task;

                var newFile = task.Result;
                if (newFile == null || string.IsNullOrWhiteSpace(newFile.MapKey))
                {
                    throw new HttpContextParsingException(
                        HttpStatusCode.BadRequest,
                        $"A file or form field blob was encountered that contains no name. All values must " +
                        $"be uniquely named.");
                }

                if (filesOut.ContainsKey(newFile.MapKey))
                {
                    throw new HttpContextParsingException(
                        HttpStatusCode.BadRequest,
                        $"A file or form field '{newFile.MapKey}' was already parsed. All values must " +
                        $"be uniquely named.");
                }

                filesOut.Add(newFile.MapKey, newFile);
            }

            return filesOut;
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

            if (topNode.IsArray() && !_config.RequestMode.IsBatchProcessingEnabled())
            {
                throw new InvalidMultiPartOperationException(
                  $"Unable to parse the '{MultipartRequestConstants.Web.OPERATIONS_FORM_KEY}' form field. The target schema does not allow " +
                  $"batch operations.");
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