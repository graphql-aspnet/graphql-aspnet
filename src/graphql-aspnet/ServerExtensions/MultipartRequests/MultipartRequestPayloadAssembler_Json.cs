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
    using System.Text.Json.Nodes;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Exceptions;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Model;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Model.Json;

    /// <summary>
    /// An assembler that can take the raw values required by the spec and assembly a valid payload that
    /// can be executed against the runtime.
    /// </summary>
    /// <remarks>Spec: <see href="https://github.com/jaydenseric/graphql-multipart-request-spec" />.</remarks>
    public partial class MultipartRequestPayloadAssembler
    {
        private static readonly JsonDocumentOptions _documentOptions;
        private static readonly JsonNodeOptions _nodeOptions;
        private static readonly JsonSerializerOptions _serializerOptions;

        /// <summary>
        /// Gets a singleton, default instance of the assembler.
        /// </summary>
        /// <value>The default instance.</value>
        public static MultipartRequestPayloadAssembler Default { get; }

        static MultipartRequestPayloadAssembler()
        {
            _documentOptions = new JsonDocumentOptions()
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            };

            _nodeOptions = new JsonNodeOptions()
            {
                PropertyNameCaseInsensitive = true,
            };

            _serializerOptions = new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                PropertyNameCaseInsensitive = true,
            };

            _serializerOptions.Converters.Add(new FileMappedIInputVariableCollectionConverter());
            _serializerOptions.Converters.Add(new FileMappedInputVariableCollectionConverter());

            Default = new MultipartRequestPayloadAssembler();
        }

        /// <summary>
        /// Converts the node representing a query into an actual untyped query data element
        /// containing the query text and the passed variables (if any).
        /// </summary>
        /// <param name="node">The node to convert.</param>
        /// <param name="index">The index of the element within a parent array, if any. This index will be populated only if
        /// the query is a batch query.</param>
        /// <param name="files">A collection of parsed files used to inject file references
        /// into the input variables on the created query data objects.</param>
        /// <returns>GraphQueryData.</returns>
        protected virtual GraphQueryData ConvertNodeToQueryData(JsonNode node, int? index = null, IReadOnlyDictionary<string, FileUpload> files = null)
        {
            if (!node.IsObject())
            {
                throw new InvalidMultiPartOperationException(
                    $"The value provided for the form key {MultipartRequestConstants.Web.OPERATIONS_FORM_KEY} " +
                    (index.HasValue ? $"at index {index.Value} " : string.Empty) +
                    "was not a valid json object. It cannot be converted into a query");
            }

            GraphQueryData data;
            try
            {
                data = node.Deserialize<GraphQueryData>(_serializerOptions);
            }
            catch (Exception ex)
            {
                throw new InvalidMultiPartOperationException(
                    $"The value provided for the form key {MultipartRequestConstants.Web.OPERATIONS_FORM_KEY} " +
                    (index.HasValue ? $"at index {index.Value} " : string.Empty) +
                    "could not be deserialzied into a valid graphql payload. See inner exception for details.", ex);
            }

            if (files != null && data.Variables is FileMappedInputVariableCollection fmivc)
            {
                foreach (var fileVariable in fmivc.FileUploadVariables)
                {
                    if (files.ContainsKey(fileVariable.MapKey))
                    {
                        fileVariable.Value = files[fileVariable.MapKey];
                    }
                    else
                    {
                        throw new InvalidMultiPartMapException(
                            $"An expected file with map key '{fileVariable.MapKey}' was not found on " +
                            $"the provided request.");
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Using a technique similar to the `object-path` npm package to inject
        /// marked string values into the json node indicating where files should be placed when deserializing
        /// a variable collection.
        /// </summary>
        /// <param name="operationsNode">The operations node where file markers will be injected.</param>
        /// <param name="map">The map recieved on the request indicating where files will be injected.</param>
        protected virtual void InjectMappedFileMarkers(JsonNode operationsNode, string map)
        {
            if (map == null || operationsNode == null)
                return;

            JsonNode mapNode;

            try
            {
                mapNode = JsonNode.Parse(map, documentOptions: _documentOptions);
            }
            catch (Exception ex)
            {
                throw new InvalidMultiPartMapException(
                  $"Unable to parse the '{MultipartRequestConstants.Web.MAP_FORM_KEY}' form field. The provided value is not a " +
                  $"valid json string. {ex.Message}",
                  ex);
            }

            if (!mapNode.IsObject())
            {
                throw new InvalidMultiPartMapException(
                    $"The value provided for the form key {MultipartRequestConstants.Web.MAP_FORM_KEY} " +
                    "was not a valid json object. It cannot be converted into a set of mapping instructions.");
            }

            foreach (var kvp in mapNode.AsObject())
            {
                var fileMapKey = kvp.Key;

                if (fileMapKey.Contains(MultipartRequestConstants.Protected.FILE_MARKER_PREFIX, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidMultiPartMapException("Map keys cannot contain the global file marker delimiter key.");

                var markerValue = JsonValue.Create($"{MultipartRequestConstants.Protected.FILE_MARKER_PREFIX}{fileMapKey}");

                try
                {
                    if (kvp.Value.IsArray())
                    {
                        var arr = kvp.Value.AsArray();
                        if (arr.Count == 1)
                        {
                            if (arr[0].IsValue() && arr[0].AsValue().TryGetValue<string>(out var mapString))
                            {
                                operationsNode.SetJsonNode(mapString, markerValue);
                                continue;
                            }
                        }
                        else
                        {
                            operationsNode.SetJsonNode(kvp.Value.AsArray(), markerValue);
                            continue;
                        }
                    }

                    if (kvp.Value.IsValue())
                    {
                        if (kvp.Value.AsValue().TryGetValue<string>(out var mapString))
                        {
                            operationsNode.SetJsonNode(mapString, markerValue);
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidMultiPartMapException(
                        $"The map value for key '{fileMapKey}' " +
                        $"did not point to a valid location in the {MultipartRequestConstants.Web.OPERATIONS_FORM_KEY} " +
                        $"object. See inner exception for details.", ex);
                }

                throw new InvalidMultiPartMapException(
                    $"Invalid value for map key '{fileMapKey}'. Expected a dot delimited string or " +
                    $"an array of path values.");
            }
        }
    }
}