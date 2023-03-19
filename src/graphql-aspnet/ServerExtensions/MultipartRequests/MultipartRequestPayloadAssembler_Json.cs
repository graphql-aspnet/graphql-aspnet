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
    using System.Diagnostics;
    using System.Linq;
    using System.Text.Json;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
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
        /// Parses the string on the "map" key in the multipart content form and converts it into a
        /// a list of file map keys and paths in the payload.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns>List&lt;KeyValuePair&lt;System.String, List&lt;ObjectPathSegment&gt;&gt;&gt;.</returns>
        protected virtual List<KeyValuePair<string, List<MultipartObjectPathSegment>>> CreateMap(string map)
        {
            var allSegments = new List<KeyValuePair<string, List<MultipartObjectPathSegment>>>();
            if (string.IsNullOrWhiteSpace(map))
                return allSegments;

            JsonDocument doc;

            try
            {
                doc = JsonDocument.Parse(map, _options);
            }
            catch (Exception ex)
            {
                throw new InvalidMultiPartMapException(
                    $"Unable to parse the '{MultipartRequestConstants.Web.MAP_FORM_KEY}' form field. The provided value is not a " +
                    $"valid json document. {ex.Message}");
            }

            using (doc)
            {
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    throw new InvalidMultiPartMapException(
                           $"Expected a valid json object for the multi-part form field '{MultipartRequestConstants.Web.MAP_FORM_KEY}' but received a(n) {doc.RootElement.ValueKind}.");
                }

                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    var mapKey = prop.Name;
                    List<MultipartObjectPathSegment> segments = null;

                    switch (prop.Value.ValueKind)
                    {
                        case JsonValueKind.Array:
                            segments = this.BuildSegmentsFromPathArray(mapKey, prop.Value);
                            break;

                        case JsonValueKind.String:
                            segments = this.BuildSegmentsFromStringPath(mapKey, prop.Value.GetString());
                            break;

                        case JsonValueKind.Number:
                            segments = this.BuildSegmentsFromStringPath(mapKey, prop.Value.GetInt64().ToString());
                            break;

                        default:
                            throw new InvalidMultiPartMapException(
                                $"Expected an array, a number or a string for the map value of key '{mapKey}' " +
                                $"but instead received a(n) {prop.Value.ValueKind}.",
                                mapKey,
                                null,
                                -1);
                    }

                    allSegments.Add(new (mapKey, segments));
                }
            }

            return allSegments;
        }

        /// <summary>
        /// For the given key and path string (e.g. "path1.0.path2", builds out the map segments pointed at by the string.
        /// </summary>
        /// <param name="mapKey">The map key to build for.</param>
        /// <param name="mapPath">The map path as a string (e.g. "path1.0.path2.1").</param>
        /// <returns>List&lt;MultipartObjectPathSegment&gt;.</returns>
        protected virtual List<MultipartObjectPathSegment> BuildSegmentsFromStringPath(string mapKey, string mapPath)
        {
            if (string.IsNullOrWhiteSpace(mapPath))
                return new List<MultipartObjectPathSegment>();

            var split = mapPath.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            return split.Select(x => new MultipartObjectPathSegment(x)).ToList();
        }

        /// <summary>
        /// For the given key and json array, builds out the map segments within the array. Expects that the jsonElement
        /// array contains only strings or numbers or an exception will be thrown.
        /// </summary>
        /// <param name="mapKey">The map key to build for.</param>
        /// <param name="arrayElement">The array element to parse.</param>
        /// <returns>List&lt;MultipartObjectPathSegment&gt;.</returns>
        protected virtual List<MultipartObjectPathSegment> BuildSegmentsFromPathArray(string mapKey, JsonElement arrayElement)
        {
            var propSegments = new List<MultipartObjectPathSegment>();

            int index = 0;
            foreach (var element in arrayElement.EnumerateArray())
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
                        throw new InvalidMultiPartMapException(
                            $"Unable to parse the object-path for file key '{mapKey}', Expected each element of the array " +
                            $"to be a string or a number but received a '{element.ValueKind}'.",
                            mapKey,
                            propSegments,
                            index);
                }

                propSegments.Add(segment);
                index++;
            }

            return propSegments;
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
                {
                    throw new InvalidMultiPartMapException(
                        $"An expected file key named '{map.Key}' was not found amongst the request.",
                        map.Key,
                        null,
                        -1);
                }

                var segments = map.Value;
                if (segments.Count == 0)
                    throw new InvalidMultiPartMapException("The supplied object-path points to nothing", map.Key, segments, -1);

                var file = files[map.Key];

                GraphQueryData queryData = null;
                var index = 0;
                if (payload.IsBatch)
                {
                    if (!segments[0].Index.HasValue)
                    {
                        throw new InvalidMultiPartMapException(
                            "The multi-part form indicated a batch operation, however; the mapped object-path " +
                            $"for file '{map.Key}' does not point to a member of the batch.",
                            map.Key,
                            segments,
                            -1);
                    }

                    if (segments[0].Index.Value < 0 || segments[0].Index.Value >= payload.QueriesToExecute.Count)
                    {
                        throw new InvalidMultiPartMapException(
                          $"Index out of range, map for file '{map.Key}' points to an operation not in the provided batch",
                          map.Key,
                          segments,
                          0);
                    }

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

            // segments might be, at a minimum, ["variables", "Anything"]
            if ((segments.Count - index) < 2)
            {
                throw new InvalidMultiPartMapException(
                    $"Unexpected object-path for the mapped file '{file?.Key}'. Expected at least 2 path segments, but received {segments.Count}.",
                    file?.Key,
                    segments,
                    -1);
            }

            var segment = segments[index];
            if (segment.Index.HasValue)
            {
                throw new InvalidMultiPartMapException(
                    $"Unexpected array indexer at position {index} in the object-path for file '{file?.Key}'.",
                    file?.Key,
                    segments,
                    index);
            }

            if (string.Compare(Constants.Web.QUERYSTRING_VARIABLES_KEY, segment.PropertyName, StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new InvalidMultiPartMapException(
                    $"Expected object-path segment to point to '{Constants.Web.QUERYSTRING_QUERY_KEY}' but got '{segment.PropertyName}'.",
                    file?.Key,
                    segments,
                    index);
            }

            if (queryData.Variables == null || queryData.Variables.Count == 0)
            {
                throw new InvalidMultiPartMapException(
                    $"The variables collection pointed to by the map for file '{file?.Key}' is null or empty. No files " +
                    $"can be assigned to it.",
                    file?.Key,
                    segments,
                    index);
            }

            var variableCollection = queryData.Variables;
            segment = segments[index + 1];

            // variables is an object with named properties (by definition), the first item must
            // be a declared property within it
            if (!variableCollection.TryGetVariable(segment.PropertyName, out var variable))
            {
                throw new InvalidMultiPartMapException(
                    $"Unknown top-level variable. A variable named '{segment.PropertyName}' does not exist " +
                    $"in the variables collection pointed to by the map for file '{file?.Key}'.",
                    file?.Key,
                    segments,
                    index + 1);
            }

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
                {
                    throw new InvalidMultiPartMapException(
                      $"Unknown object-path segment Path segment '{segment.PropertyName}' in the path for the file '{file?.Key}' " +
                      $"was not found and could not be created.",
                      file?.Key,
                      segments,
                      i);
                }

                variable = foundChild;
            }

            // the terminating value MUST be a single value variable (not a list or object)
            if (!(variable is IInputSingleValueVariable isvv))
            {
                throw new InvalidMultiPartMapException(
                  $"The variable value pointed to by the path for file '{file?.Key}' does not " +
                  $"point to a single-valued variable. Unable to allocate the requested file upload scalar.",
                  file?.Key,
                  segments,
                  segments.Count - 1);
            }

            // the terminating value MUST currently point to nothing
            if (isvv.Value != null)
            {
                throw new InvalidMultiPartMapException(
                  $"The variable value pointed to by the path for file '{file?.Key}' is not null. " +
                  $"You must define <null> as the supplied variable value in order for the file upload scalar to be " +
                  $"correctly applied.",
                  file?.Key,
                  segments,
                  segments.Count - 1);
            }

            if (parent is IWritableInputVariableCollection wivc)
            {
                // if the owner of the variable is the top level collection
                // then we need to update the named variable in the collection with the file
                wivc.Replace(segment.PropertyName, file);
                return;
            }

            if (parent is IWritableInputListVariable wilv && segment.Index.HasValue)
            {
                // if the owner of the variable is an array then we need to
                // replace the array index with the file
                wilv.Replace(segment.Index.Value, file);
                return;
            }

            if (parent is IWritableInputFieldSetVariable wifsv)
            {
                // if the owner of the variable is a field set (i.e. an object) then we need to
                // replace the named field value with the file
                wifsv.Replace(segment.PropertyName, file);
                return;
            }

            // fail safe
            // don't know what the parent was (or it wasn't writable) so we can't update the value within in
            throw new InvalidMultiPartMapException(
                 $"Unable to insert the file '{file?.Key}' into the target variable collection. The variable collection may be read only " +
                 $"or an unexpected variable type was encountered.",
                 file?.Key,
                 segments,
                 segments.Count - 1);
        }
    }
}