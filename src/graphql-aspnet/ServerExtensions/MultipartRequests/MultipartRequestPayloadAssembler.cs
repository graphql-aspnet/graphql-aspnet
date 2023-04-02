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
    using System.Text.Json.Nodes;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Exceptions;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Model;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Web;

    /// <summary>
    /// An assembler that can take the raw values required by the spec and assembly a valid payload that
    /// can be executed against the runtime.
    /// </summary>
    /// <remarks>Spec: <see href="https://github.com/jaydenseric/graphql-multipart-request-spec" />.</remarks>
    public partial class MultipartRequestPayloadAssembler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartRequestPayloadAssembler"/> class.
        /// </summary>
        public MultipartRequestPayloadAssembler()
        {
        }

        /// <summary>
        /// Assembles a valid query payload from the constituent parts defined by the specification.
        /// </summary>
        /// <param name="operations">The json object representing operation(s) provided on the request.</param>
        /// <param name="map">A map to connect files to appropriate variables in the operations collection.</param>
        /// <param name="files">The files found on the request.</param>
        /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A Task&lt;MultiPartRequestGraphQLPayload&gt; representing the asynchronous operation.</returns>
        public virtual Task<MultiPartRequestGraphQLPayload> AssemblePayload(
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