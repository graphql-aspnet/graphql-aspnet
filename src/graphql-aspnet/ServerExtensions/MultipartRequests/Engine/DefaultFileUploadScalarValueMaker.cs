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
    /// A maker that can convert a raw ASP.NET <see cref="IFormFile"/> object into a <see cref="FileUpload"/>
    /// graphql scalar.
    /// </summary>
    public class DefaultFileUploadScalarValueMaker : IFileUploadScalarValueMaker
    {
        /// <inheritdoc />
        public virtual async Task<FileUpload> CreateFileScalar(IFormFile aspNetFile)
        {
            if (aspNetFile == null)
                return null;

            var streamContainer = await this.CreateStream(aspNetFile);

            var file = new FileUpload(
                aspNetFile.Name,
                streamContainer,
                aspNetFile.ContentType,
                aspNetFile.FileName,
                new Dictionary<string, StringValues>(aspNetFile.Headers));

            return file;
        }

        /// <inheritdoc />
        public Task<FileUpload> CreateFileScalar(string mapKey, byte[] blobData)
        {
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
        protected virtual Task<IFileUploadStreamContainer> CreateStream(IFormFile aspNetFile)
        {
            var stream = new FormFileStreamContainer(aspNetFile);
            return Task.FromResult(stream as IFileUploadStreamContainer);
        }
    }
}