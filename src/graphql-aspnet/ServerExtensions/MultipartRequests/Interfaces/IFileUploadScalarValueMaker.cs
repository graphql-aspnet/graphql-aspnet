// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Model;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// An object that that can generate a valid <see cref="FileUpload"/> scalar value
    /// from an ASP.NET <see cref="IFormFile"/> received on a request.
    /// </summary>
    public interface IFileUploadScalarValueMaker
    {
        /// <summary>
        /// Creates an instance of the file upload scalar from an asp.net file.
        /// </summary>
        /// <param name="aspNetFile">The ASP.NET file object created by the runtime.</param>
        /// <returns>Task&lt;FileUpload&gt;.</returns>
        Task<FileUpload> CreateFileScalarAsync(IFormFile aspNetFile);

        /// <summary>
        /// Creates an instance of the file upload scalar from a key/value blob.
        /// </summary>
        /// <param name="mapKey">The map key provided on the request to identify the blob.</param>
        /// <param name="blobData">A data block to serve as a file.</param>
        /// <returns>Task&lt;FileUpload&gt;.</returns>
        Task<FileUpload> CreateFileScalarAsync(string mapKey, byte[] blobData);
    }
}