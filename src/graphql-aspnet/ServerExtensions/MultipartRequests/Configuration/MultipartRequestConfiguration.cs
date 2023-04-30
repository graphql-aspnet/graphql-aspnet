// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests.Configuration
{
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Engine;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces;

    /// <summary>
    /// A configuration class to set runtime options related to <see cref="MultipartRequestServerExtension" />.
    /// </summary>
    public class MultipartRequestConfiguration : IMultipartRequestConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartRequestConfiguration"/> class.
        /// </summary>
        public MultipartRequestConfiguration()
        {
            this.RequestMode = MultipartRequestMode.Default;
            this.MaxFileCount = null;
            this.MaxBlobCount = null;
            this.MapMode = MultipartRequestMapHandlingMode.Default;
            this.RegisterMultipartRequestHttpProcessor = true;
            this.RequireMultipartRequestHttpProcessor = true;
        }

        /// <inheritdoc />
        public MultipartRequestMode RequestMode { get; set; }

        /// <inheritdoc />
        public int? MaxFileCount { get; set; }

        /// <inheritdoc />
        public int? MaxBlobCount { get; set; }

        /// <inheritdoc />
        public MultipartRequestMapHandlingMode MapMode { get; set; }

        /// <inheritdoc />
        public bool RegisterMultipartRequestHttpProcessor { get; set; }

        /// <inheritdoc />
        public bool RequireMultipartRequestHttpProcessor { get; set; }
    }
}