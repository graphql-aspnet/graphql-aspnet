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
    using System.Diagnostics;

    /// <summary>
    /// A single segment from the array pointed to by an entry in the "map" section of the multi-part
    /// form.
    /// </summary>
    [DebuggerDisplay("{Raw}")]
    public class MultipartObjectPathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartObjectPathSegment"/> class.
        /// </summary>
        /// <param name="segment">The segment parsed from a map.</param>
        public MultipartObjectPathSegment(string segment)
        {
            if (int.TryParse(segment, out var index))
                this.Index = index;

            this.PropertyName = segment;
            this.Raw = segment;
        }

        /// <summary>
        /// Gets the index of the array pointed at by this segment. When null, this segment does
        /// not point to an array index.
        /// </summary>
        /// <value>The index.</value>
        public int? Index { get; }

        /// <summary>
        /// Gets the name of the property this segment points to. When null, this segment does not
        /// point to a property.
        /// </summary>
        /// <value>The name of the property.</value>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the raw value provided for this segment.
        /// </summary>
        /// <value>The raw segment value.</value>
        public string Raw { get; }
    }
}