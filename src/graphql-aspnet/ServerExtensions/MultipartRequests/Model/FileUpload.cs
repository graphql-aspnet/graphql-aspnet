// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests.Model
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;

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
        private readonly IFileUploadStreamContainer _streamContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileUpload" /> class.
        /// </summary>
        /// <param name="mapKey">The key used to identify where in the supplied operations
        /// this instance should be placed.</param>
        /// <param name="streamContainer">The stream container providng access to the raw
        /// bits of the file when requested.</param>
        /// <param name="contentType">The 'Content-Type' header value supplied with the file.</param>
        /// <param name="fileName">Name of the file that was supplied.</param>
        /// <param name="headers">The headers.</param>
        public FileUpload(
            string mapKey,
            IFileUploadStreamContainer streamContainer,
            string contentType = null,
            string fileName = null,
            IReadOnlyDictionary<string, StringValues> headers = null)
        {
            this.MapKey = Validation.ThrowIfNullWhiteSpaceOrReturn(mapKey, nameof(mapKey));
            _streamContainer = Validation.ThrowIfNullOrReturn(streamContainer, nameof(streamContainer));

            this.ContentType = contentType;
            this.FileName = fileName;
            this.Headers = headers;
        }

        /// <summary>
        /// Opens the low level stream containing the file contents.
        /// </summary>
        /// <returns>Task&lt;Stream&gt;.</returns>
        public virtual async Task<Stream> OpenFileAsync()
        {
            return await _streamContainer.OpenStreamAsync();
        }

        /// <summary>
        /// Gets or sets the `Content-Type` header provided with the file data. May null if no
        /// content type was specified.
        /// </summary>
        /// <value>The 'Content-Type' header supplied with the file.</value>
        public virtual string ContentType { get; protected set; }

        /// <summary>
        /// Gets or sets the key value provided on the request which was used to map this
        /// file into a variable.
        /// </summary>
        /// <remarks>
        /// Generally speaking this is the '<c>Name</c>' property of the asp.net <see cref="IFormFile"/> object.
        /// </remarks>
        /// <value>The unique identifier of this file on the request.</value>
        public virtual string MapKey { get; protected set; }

        /// <summary>
        /// Gets or sets the filename of this file as it was provided on the request.
        /// </summary>
        /// <value>The name of the file.</value>
        public virtual string FileName { get; protected set; }

        /// <summary>
        /// Gets or sets the complete collection of headers that were supplied with this file.
        /// </summary>
        /// <value>The headers supplied on the request.</value>
        public virtual IReadOnlyDictionary<string, StringValues> Headers { get; protected set; }
    }
}