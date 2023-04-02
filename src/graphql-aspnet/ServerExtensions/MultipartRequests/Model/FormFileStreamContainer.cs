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
    using System.IO;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// An implementation of a file stream container which streams file contents
    /// directly from the <see cref="IFormFile"/> reference into a graphql query.
    /// </summary>
    public class FormFileStreamContainer : IFileUploadStreamContainer
    {
        private readonly IFormFile _file;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormFileStreamContainer"/> class.
        /// </summary>
        /// <param name="aspNetFile">The ASP net file to capture and serve the stream from.</param>
        public FormFileStreamContainer(IFormFile aspNetFile)
        {
            _file = Validation.ThrowIfNullOrReturn(aspNetFile, nameof(aspNetFile));
        }

        /// <inheritdoc />
        public Task<Stream> OpenFileStreamAsync()
        {
            var stream = _file.OpenReadStream();
            return Task.FromResult(stream);
        }
    }
}