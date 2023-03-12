// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

/// <summary>
/// The MultipartRequests namespace.
/// </summary>
namespace GraphQL.AspNet.ServerExtensions.MultipartRequests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.Json;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Model;
    using Microsoft.AspNetCore.Http.Features;

    /// <summary>
    /// An assembler that can take the raw values required by the spec and assembly a valid payload that
    /// can be executed against the runtime.
    /// </summary>
    /// <remarks>Spec: <see href="https://github.com/jaydenseric/graphql-multipart-request-spec" />.</remarks>
    [DebuggerDisplay("{Raw}")]
    public partial class MultipartRequestPayloadAssembler
    {


        /// <summary>
        /// Parses the string on the "map" key in the multipart content form and converts it into a
        /// a list of file map keys and paths in the payload.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns>List&lt;KeyValuePair&lt;System.String, List&lt;ObjectPathSegment&gt;&gt;&gt;.</returns>
        protected virtual List<KeyValuePair<string, List<MultipartObjectPathSegment>>> CreateMap(string map)
        {
            var allSegments = new List<KeyValuePair<string, List<MultipartObjectPathSegment>>>();
            var doc = JsonDocument.Parse(map, _options);

            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                throw new InvalidOperationException("Expected Object");

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                var mapKey = prop.Name;

                if (prop.Value.ValueKind != JsonValueKind.Array)
                    throw new InvalidOperationException("Expected array");

                var propSegments = new List<MultipartObjectPathSegment>();
                foreach (var element in prop.Value.EnumerateArray())
                {
                    MultipartObjectPathSegment segment = null;
                    switch (element.ValueKind)
                    {
                        case JsonValueKind.Number:
                            segment = new MultipartObjectPathSegment(element.GetRawText());
                            break;

                        case JsonValueKind.String:
                            segment = new MultipartObjectPathSegment(element.GetString());
                            break;
                        default:
                            throw new InvalidOperationException($"Invalid object-path segment. Expected string or number, got '{element.ValueKind}'");
                    }

                    propSegments.Add(segment);
                }

                allSegments.Add(new (mapKey, propSegments));
            }

            return allSegments;
        }

        /// <summary>
        /// Attempts to place each file into the payload according to the path pointed at by the provided map.
        /// </summary>
        /// <param name="payload">The payload into which files will be inserted.</param>
        /// <param name="files">The collection of files read from the multi-part form.</param>
        /// <param name="fileMap">The file map created from the json string found on the multi-part form.</param>
        protected virtual void MapFilesToPayload(
            MultiPartRequestGraphQLPayload payload,
            IReadOnlyDictionary<string, FileUpload> files,
            List<KeyValuePair<string, List<MultipartObjectPathSegment>>> fileMap)
        {
            if (payload == null || fileMap == null || fileMap.Count == 0 || files == null || files.Count == 0)
                return;

            foreach (var map in fileMap)
            {
                if (!files.ContainsKey(map.Key))
                    throw new InvalidOperationException("File Key not found in provided file set");

                var segments = map.Value;
                if (segments.Count == 0)
                    throw new InvalidOperationException("Object Path points to nothing");

                var file = files[map.Key];

                GraphQueryData queryData = null;
                var index = 0;
                if (payload.IsBatch)
                {
                    if (!segments[0].Index.HasValue)
                        throw new InvalidOperationException("Batch operation provided, map does not point to a member of the batch");

                    if (segments[0].Index.Value < 0 || segments[0].Index.Value >= payload.QueriesToExecute.Count)
                        throw new InvalidOperationException("Index out of range, map points to an operation not in the provided batch");

                    queryData = payload.QueriesToExecute[segments[0].Index.Value];
                    index = 1;
                }
                else
                {
                    queryData = payload.QueriesToExecute[0];
                }

                var fileVariable = new InputFileUploadVariable(map.Key, file);
                this.PlaceFileInQueryData(queryData, fileVariable, segments, index);
            }
        }

        /// <summary>
        /// Atttempts to place the provided file into the query data object using the object path supplied.
        /// </summary>
        /// <param name="queryData">The query data in which to place the file.</param>
        /// <param name="file">The file to be placed.</param>
        /// <param name="segments">The segments that point into the provided <paramref name="queryData" />.</param>
        /// <param name="index">The index within <paramref name="segments"/> to start from.</param>
        protected virtual void PlaceFileInQueryData(GraphQueryData queryData, InputFileUploadVariable file, IReadOnlyList<MultipartObjectPathSegment> segments, int index)
        {
            Validation.ThrowIfNull(queryData, nameof(queryData));
            Validation.ThrowIfNull(segments, nameof(segments));

            // segments st be, at a minimum, ["variables", "Anything"]
            if (segments.Count < 2)
                throw new InvalidOperationException("Unexpected path segment");

            var segment = segments[index];
            if (segment.Index.HasValue)
                throw new InvalidOperationException("Unexpected Array Indexer");

            if (string.Compare(Constants.Web.QUERYSTRING_VARIABLES_KEY, segment.PropertyName, true) != 0)
                throw new InvalidOperationException("expected to start with variables");

            if (queryData.Variables == null || queryData.Variables.Count == 0)
                throw new InvalidOperationException("No variables defined");

            var variableCollection = queryData.Variables;
            segment = segments[index + 1];

            // variables is an object with named properties (by definition), the first item must
            // be a declared property within it
            if (!variableCollection.TryGetVariable(segment.PropertyName, out var variable))
                throw new InvalidOperationException("unknown variable");

            object parent = variableCollection;
            for (var i = index + 2; i < segments.Count; i++)
            {
                parent = variable;
                segment = segments[i];

                IInputVariable foundChild = null;
                if (segment.Index.HasValue && variable is IInputListVariable ilv)
                {
                    foundChild = ilv.Items[segment.Index.Value];
                }
                else if (variable is IInputFieldSetVariable fsv)
                {
                    if (fsv.Fields.ContainsKey(segment.PropertyName))
                        foundChild = fsv.Fields[segment.PropertyName];
                }

                if (foundChild == null)
                    throw new InvalidOperationException("Unknown segment");

                variable = foundChild;
            }

            // the terminating value MUST be a single value variable (not a list or object)
            if (!(variable is IInputSingleValueVariable isvv))
                throw new InvalidOperationException("Invalid Path, final variable does not represent a single value");

            // the terminating value MUST point to null
            if (isvv.Value != null)
                throw new InvalidOperationException($"Expected null value but got '{isvv.Value}'");

            // if the owner of the variable is the top level collection
            // then we need to update the named variable in the collection with the file
            if (parent is IWritableInputVariableCollection wivc)
            {
                wivc.Replace(segment.PropertyName, file);
                return;
            }

            // if the owner of the variable is an array then we need to
            // replace the array index with the file
            if (parent is IWritableInputListVariable wilv && segment.Index.HasValue)
            {
                wilv.Replace(segment.Index.Value, file);
                return;
            }

            // if the owner of the variable is a field set (i.e. an object) then we need to
            // replace the named field with the file
            if (parent is IWritableInputFieldSetVariable wifsv)
            {
                wifsv.Replace(segment.PropertyName, file);
                return;
            }

            // don't know what the parent was so we can't update the value within in
            throw new InvalidOperationException("Unable to write file to variable collection. Collection is not modifiable");
        }
    }
}