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
    using System.IO;
    using GraphQL.AspNet.Attributes;

    /// <summary>
    /// An input object representing a single file uploaded to a query or mutation.
    /// </summary>
    /// <remarks>
    /// Attempting to use this class without registering the <see cref="MultipartRequestServerExtension"/>
    /// will result in a schema failure. This class cannot be used as a regular INPUT_OBJECT. See documentation
    /// for details.
    /// </remarks>
    [GraphSkip]
    [GraphType(PreventAutoInclusion = true)]
    public class FileUpload
    {
        /// <summary>
        /// Gets the `Content-Type` header provided with the file data. May null if no
        /// content type was specified.
        /// </summary>
        /// <value>The 'Content-Type' header supplied with the file.</value>
        public string ContentType { get; }

        /// <summary>
        /// Gets the stream representing the uploaded file.
        /// </summary>
        /// <value>The stream of bits representing the uploaded file.</value>
        public Stream FileStream { get; }

        /// <summary>
        /// Gets the identifier provided or created for this file in the multi-part form data.
        /// </summary>
        /// <value>The unique identifier of this file on the request.</value>
        public string Id { get; }
    }
}