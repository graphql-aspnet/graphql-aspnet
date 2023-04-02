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
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces;

    /// <summary>
    /// An implementation of a file stream container which streams a string from
    /// a memory stream.
    /// </summary>
    public class ByteArrayStreamContainer : IFileUploadStreamContainer
    {
        private readonly byte[] _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArrayStreamContainer" /> class.
        /// </summary>
        /// <param name="blobData">The data block to serve as a stream.</param>
        public ByteArrayStreamContainer(byte[] blobData)
        {
            _data = blobData;
        }

        /// <inheritdoc />
        public Task<Stream> OpenFileStreamAsync()
        {
            MemoryStream memoryStream = _data != null
                ? new MemoryStream(_data)
                : new MemoryStream();

            return Task.FromResult(memoryStream as Stream);
        }
    }
}