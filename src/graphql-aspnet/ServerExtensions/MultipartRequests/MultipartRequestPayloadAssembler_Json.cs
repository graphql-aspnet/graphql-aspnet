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

    /// <summary>
    /// An assembler that can take the raw values required by the spec and assembly a valid payload that
    /// can be executed against the runtime.
    /// </summary>
    /// <remarks>Spec: <see href="https://github.com/jaydenseric/graphql-multipart-request-spec" />.</remarks>
    public partial class MultipartRequestPayloadAssembler
    {
        protected class FileObjectMap
        {
            public FileObjectMap(string segment)
            {
                if (int.TryParse(segment, out var index))
                    this.Index = index;
                else
                    this.PropertyName = segment;
            }

            /// <summary>
            /// Gets the index of the array pointed at by this segment. When null, this segment does
            /// not point to an array index.
            /// </summary>
            /// <value>The index.</value>
            public int? Index { get;  }

            /// <summary>
            /// Gets the name of the property this segment points to. When null, this segment does not
            /// point to a property.
            /// </summary>
            /// <value>The name of the property.</value>
            public string PropertyName { get;  }

            /// <summary>
            /// Gets or sets the next segment int he chain pointed at by this segment. If null, this segment
            /// represents a terminal segment.
            /// </summary>
            /// <value>The next segment.</value>
            public FileObjectMapSegment Next { get; set; }
        }

        private List<KeyValuePair<string, FileObjectMapSegment>> CreateMap(string map)
        {
            var segments = new List<KeyValuePair<string, FileObjectMapSegment>>();
            var doc = JsonDocument.Parse(map, _options);

            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                throw new InvalidOperationException("Expected Object");

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                var mapKey = prop.Name;

                if (prop.Value.ValueKind != JsonValueKind.Array)
                    throw new InvalidOperationException("Expected array");

                FileObjectMapSegment parentSegment = null;
                FileObjectMapSegment lastSegment = null;
                foreach (var element in prop.Value.EnumerateArray())
                {
                    var segment = new FileObjectMapSegment(element.GetString());

                    if (parentSegment == null)
                        parentSegment = segment;
                    if (lastSegment != null)
                        lastSegment.Next = segment;

                    lastSegment = segment;
                }

                segments.Add(new KeyValuePair<string, FileObjectMapSegment>(mapKey, parentSegment));
            }

            return segments;
        }
    }
}