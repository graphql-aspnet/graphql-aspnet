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
                    var segment = new MultipartObjectPathSegment(element.GetString());
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
                if (segments[0].Index.HasValue)
                {
                    if (!payload.IsBatch)
                        throw new InvalidOperationException("Single operation provided, map points to a batch");

                    if (segments[0].Index.Value > (payload.QueriesToExecute.Count - 1))
                        throw new InvalidOperationException("Index out of range, map points to an operation not in the provided batch");

                    queryData = payload.QueriesToExecute[segments[0].Index.Value];
                    segments = segments.Skip(1).ToList();
                }
                else if (payload.IsBatch)
                {
                    throw new InvalidOperationException("Batch provided, map points to a single operation");
                }
                else
                {
                    queryData = payload.QueriesToExecute[0];
                }

                this.PlaceFileInQueryData(queryData, file, segments);
            }
        }

        /// <summary>
        /// Atttempts to place the provided file into the query data object using the object path supplied.
        /// </summary>
        /// <param name="queryData">The query data in which to place the file.</param>
        /// <param name="file">The file to be placed.</param>
        /// <param name="segments">The segments that point into the provided <paramref name="queryData"/>.</param>
        protected virtual void PlaceFileInQueryData(GraphQueryData queryData, FileUpload file, IEnumerable<MultipartObjectPathSegment> segments)
        {
            var propGetters = InstanceFactory.CreatePropertyGetterInvokerCollection(typeof(GraphQueryData));


        }
    }
}