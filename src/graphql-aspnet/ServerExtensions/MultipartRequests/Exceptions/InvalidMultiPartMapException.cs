// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Web;

    /// <summary>
    /// An exception thrown by the Multipart Request Server extension indicating that something unexpected occured
    /// with the provided map object.
    /// </summary>
    public class InvalidMultiPartMapException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMultiPartMapException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidMultiPartMapException(string message)
            : base(message)
        {
            this.SegmentPath = null;
            this.FileMapKey = null;
            this.Segments = null;
            this.Index = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMultiPartMapException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="fileMapKey">The file map key that was being parsed.</param>
        /// <param name="segments">The set of segments that made of the invalid path.</param>
        /// <param name="failedIndex">The index within <paramref name="segments" /> when traversal failed.</param>
        public InvalidMultiPartMapException(
            string message,
            string fileMapKey,
            IReadOnlyList<MultipartObjectPathSegment> segments,
            int failedIndex)
            : base(message)
        {
            this.Segments = segments;
            this.Index = failedIndex;
            this.FileMapKey = fileMapKey;
            if (segments != null)
            {
                this.SegmentPath = string.Join(".", segments.Select(x => $"\"{x.PropertyName}\""));
            }
        }

        /// <summary>
        /// Gets the path string that was pointed to in the path segment collection.
        /// </summary>
        /// <value>The segment path.</value>
        public string SegmentPath { get; }

        /// <summary>
        /// Gets the actual parsed segments that were being navigated when the path traversal failed.
        /// </summary>
        /// <value>The segments.</value>
        public IReadOnlyList<MultipartObjectPathSegment> Segments { get; }

        /// <summary>
        /// Gets the index into the <see cref="Segments"/> that failed to be traversed.
        /// </summary>
        /// <value>The index.</value>
        public int Index { get; }

        /// <summary>
        /// Gets the key assigned to the file that was being placed when object-path traversal failed.
        /// </summary>
        /// <value>The file map key.</value>
        public string FileMapKey { get; }
    }
}