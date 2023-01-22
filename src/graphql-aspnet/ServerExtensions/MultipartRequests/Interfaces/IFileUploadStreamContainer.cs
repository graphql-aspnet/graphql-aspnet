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
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// A container that wraps the logic necessary to open and serve a raw <see cref="Stream"/>
    /// containing the file contents. This stream may originate from various locations depending on the implementation.
    /// </summary>
    public interface IFileUploadStreamContainer
    {
        /// <summary>
        /// Opens the file stream in a manner appropriate with stream container.
        /// </summary>
        /// <returns>Task&lt;Stream&gt;.</returns>
        Task<Stream> OpenStreamAsync();
    }
}