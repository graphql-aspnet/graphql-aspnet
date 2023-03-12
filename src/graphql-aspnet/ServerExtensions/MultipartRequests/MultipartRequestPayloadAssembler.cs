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
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Variables;

    /// <summary>
    /// An assembler that can take the raw values required by the spec and assembly a valid payload that
    /// can be executed against the runtime.
    /// </summary>
    /// <remarks>Spec: <see href="https://github.com/jaydenseric/graphql-multipart-request-spec" />.</remarks>
    public partial class MultipartRequestPayloadAssembler
    {
        private static readonly JsonDocumentOptions _options;

        static MultipartRequestPayloadAssembler()
        {
            _options = new JsonDocumentOptions()
            {
                CommentHandling = JsonCommentHandling.Skip,
                MaxDepth = 3,
                AllowTrailingCommas = true,
            };
        }

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

            using var doc = JsonDocument.Parse(operations, _options);
            var isBatch = doc.RootElement.ValueKind == JsonValueKind.Array;

            MultiPartRequestGraphQLPayload payload = null;
            if (!isBatch)
            {
                // single query object (not an array of queries in a batch)
                var query = this.ConvertElementToQueryData(doc.RootElement);
                payload = new MultiPartRequestGraphQLPayload(query);
            }
            else
            {
                var queries = new List<GraphQueryData>();
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    var query = this.ConvertElementToQueryData(element);
                    queries.Add(query);
                }

                payload = new MultiPartRequestGraphQLPayload(queries);
            }

            if (files != null && files.Count > 0)
            {
                var fileMap = this.CreateMap(map);
                this.MapFilesToPayload(payload, files, fileMap);
            }

            return Task.FromResult(payload);
        }

        /// <summary>
        /// Converts the node representing a query into an actual untyped query data element
        /// containing the query text and the passed variables (if any).
        /// </summary>
        /// <param name="element">The element to convert.</param>
        /// <returns>GraphQueryData.</returns>
        protected virtual GraphQueryData ConvertElementToQueryData(JsonElement element)
        {
            // extract query text
            var foundQuery = element.TryGetProperty(MultipartRequestConstants.QueryPayloadKeywords.QUERY_KEY, out var queryElement);
            if (!foundQuery)
            {
                throw new InvalidOperationException("No query part");
            }

            var foundVariables = element.TryGetProperty(MultipartRequestConstants.QueryPayloadKeywords.VARIABLE_KEY, out var variablesElement);
            var foundOperation = element.TryGetProperty(MultipartRequestConstants.QueryPayloadKeywords.OPERATION_KEY, out var operationElement);

            var queryText = queryElement.GetString();
            InputVariableCollection variables = null;
            if (foundVariables)
            {
                variables = InputVariableCollection.FromJsonElement(variablesElement);
            }

            variables = variables ?? InputVariableCollection.Empty;
            var operationName = foundOperation ? operationElement.GetString() : null;

            return new GraphQueryData()
            {
                Query = queryText,
                Variables = variables,
                OperationName = operationName,
            };
        }
    }
}