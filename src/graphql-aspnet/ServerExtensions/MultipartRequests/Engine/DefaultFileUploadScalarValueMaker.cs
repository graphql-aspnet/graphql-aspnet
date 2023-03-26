// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests.Engine
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Model;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// A scalar maker that can convert a raw ASP.NET <see cref="IFormFile"/> or a raw byte array
    /// object into a <see cref="FileUpload"/> graphql scalar instance.
    /// </summary>
    public class DefaultFileUploadScalarValueMaker : IFileUploadScalarValueMaker
    {
        /// <inheritdoc />
        public virtual async Task<FileUpload> CreateFileScalarAsync(IFormFile aspNetFile)
        {
            if (aspNetFile == null)
                return null;

            var streamContainer = await this.CreateStreamContainer(aspNetFile);

            Dictionary<string, StringValues> headers = null;
            string contentType = null;
            if (aspNetFile.Headers != null)
            {
                // stand implementation of `FormFile` reads contenttype from the headers
                // property, if headers is null, contentType will throw an null ref exeception
                headers = new Dictionary<string, StringValues>(aspNetFile.Headers);
                contentType = aspNetFile.ContentType;
            }

            var file = new FileUpload(
                aspNetFile.Name,
                streamContainer,
                contentType,
                aspNetFile.FileName,
                headers);

            return file;
        }

        /// <inheritdoc />
        public virtual Task<FileUpload> CreateFileScalarAsync(string mapKey, byte[] blobData)
        {
            blobData = blobData ?? new byte[0];
            var streamContainer = new ByteArrayStreamContainer(blobData);

            var file = new FileUpload(
                mapKey,
                streamContainer);

            return Task.FromResult(file);
        }

        /// <summary>
        /// Creates a stream container that manages the opening and serving of the underlying
        /// <see cref="Stream"/> containing the file contents.
        /// </summary>
        /// <param name="aspNetFile">The ASP.NET file reference.</param>
        /// <returns>Task&lt;IFileUploadStream&gt;.</returns>
        protected virtual Task<IFileUploadStreamContainer> CreateStreamContainer(IFormFile aspNetFile)
        {
            var stream = new FormFileStreamContainer(aspNetFile);
            return Task.FromResult(stream as IFileUploadStreamContainer);
        }
    }
}