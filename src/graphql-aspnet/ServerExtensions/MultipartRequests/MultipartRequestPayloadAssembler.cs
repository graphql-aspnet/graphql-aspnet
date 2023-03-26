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
        private static readonly JsonDocumentOptions _options;

        /// <summary>
        /// Gets a singleton, default instance of the assembler.
        /// </summary>
        /// <value>The default instance.</value>
        public static MultipartRequestPayloadAssembler Default { get; }

        static MultipartRequestPayloadAssembler()
        {
            _options = new JsonDocumentOptions()
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            };

            Default = new MultipartRequestPayloadAssembler();
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

            JsonDocument doc = null;

            try
            {
                doc = JsonDocument.Parse(operations, _options);
            }
            catch (Exception ex)
            {
                throw new InvalidMultiPartOperationException(
                    $"Unable to parse the '{MultipartRequestConstants.Web.OPERATIONS_FORM_KEY}' form field. The provided value is not a " +
                    $"valid json document. {ex.Message}",
                    ex);
            }

            MultiPartRequestGraphQLPayload payload = null;
            using (doc)
            {
                var isBatch = doc.RootElement.ValueKind == JsonValueKind.Array;

                if (!isBatch)
                {
                    // single query object (not an array of queries in a batch)
                    var query = this.ConvertElementToQueryData(doc.RootElement);
                    payload = new MultiPartRequestGraphQLPayload(query);
                }
                else
                {
                    var queries = new List<GraphQueryData>();
                    var i = 0;
                    foreach (var element in doc.RootElement.EnumerateArray())
                    {
                        var query = this.ConvertElementToQueryData(element, i++);
                        queries.Add(query);
                    }

                    payload = new MultiPartRequestGraphQLPayload(queries);
                }

                if (files != null && files.Count > 0)
                {
                    var fileMap = this.CreateMap(map);
                    this.MapFilesToPayload(payload, files, fileMap);
                }
            }

            return Task.FromResult(payload);
        }

        /// <summary>
        /// Converts the node representing a query into an actual untyped query data element
        /// containing the query text and the passed variables (if any).
        /// </summary>
        /// <param name="element">The element to convert.</param>
        /// <param name="index">The index of the element within a parent array, if any. This index will be populated only if
        /// the query is a batch query.</param>
        /// <returns>GraphQueryData.</returns>
        protected virtual GraphQueryData ConvertElementToQueryData(JsonElement element, int? index = null)
        {
            // extract query text
            var foundQuery = element.TryGetProperty(MultipartRequestConstants.QueryPayloadKeywords.QUERY_KEY, out var queryElement);
            if (!foundQuery)
            {
                if (!index.HasValue)
                {
                    throw new InvalidMultiPartOperationException($"A json property named '{MultipartRequestConstants.QueryPayloadKeywords.QUERY_KEY}' " +
                        $"is required on the object passed as the value of {MultipartRequestConstants.Web.OPERATIONS_FORM_KEY} form field.");
                }
                else
                {
                    throw new InvalidMultiPartOperationException($"A json property named '{MultipartRequestConstants.QueryPayloadKeywords.QUERY_KEY}' " +
                        $"is required at index {index.Value} on the array passed as the value of {MultipartRequestConstants.Web.OPERATIONS_FORM_KEY} form field.");
                }
            }

            var foundVariables = element.TryGetProperty(MultipartRequestConstants.QueryPayloadKeywords.VARIABLE_KEY, out var variablesElement);
            var foundOperationName = element.TryGetProperty(MultipartRequestConstants.QueryPayloadKeywords.OPERATION_KEY, out var operationElement);

            var queryText = queryElement.GetString();
            InputVariableCollection variables = null;
            if (foundVariables)
            {
                variables = InputVariableCollection.FromJsonElement(variablesElement);
            }

            variables = variables ?? InputVariableCollection.Empty;
            string operationName = null;

            if (foundOperationName)
            {
                if (operationElement.ValueKind != JsonValueKind.String)
                {
                    if (!index.HasValue)
                    {
                        throw new InvalidMultiPartOperationException($"The property named '{MultipartRequestConstants.QueryPayloadKeywords.OPERATION_KEY}' " +
                            $"on the object passed as the value of {MultipartRequestConstants.Web.OPERATIONS_FORM_KEY} form field must be a string or null.");
                    }
                    else
                    {
                        throw new InvalidMultiPartOperationException($"The json property named '{MultipartRequestConstants.QueryPayloadKeywords.QUERY_KEY}' " +
                            $"at index {index.Value} on the array passed as the value of {MultipartRequestConstants.Web.OPERATIONS_FORM_KEY} form field must be a string or null.");
                    }
                }

                operationName = operationElement.GetString();
            }

            return new GraphQueryData()
            {
                Query = queryText,
                Variables = variables,
                OperationName = operationName,
            };
        }
    }
}