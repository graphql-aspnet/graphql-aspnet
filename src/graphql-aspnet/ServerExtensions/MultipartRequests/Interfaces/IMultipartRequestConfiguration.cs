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
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Configuration;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Engine;

    /// <summary>
    /// An interface detailing the options configured for an instance of <see cref="MultipartRequestServerExtension" />
    /// for a single schema.
    /// </summary>
    public interface IMultipartRequestConfiguration
    {
        /// <summary>
        /// Gets a value indicating which options of the multipart request specification are enabled.
        /// </summary>
        /// <remarks>
        /// (Default: All Options).
        /// </remarks>
        /// <value>The request mode configured for this schema.</value>
        public MultipartRequestMode RequestMode { get; }

        /// <summary>
        /// Gets a value that, when set, indicates the maximum number of files and blobs that can be included
        /// on a single POST request. When not set, an unlimited number of files per request is allowed.
        /// </summary>
        /// <remarks>
        /// (Default: Unlimited).
        /// </remarks>
        /// <value>The maximum file count.</value>
        public int? MaxFileCount { get; }

        /// <summary>
        /// Gets a value that, when set, indicates the maximum number of blobs (i.e. unspecified form fields)
        /// that can be included on a single POST request. When not set, an unlimited number of blobs per request is allowed.
        /// </summary>
        /// <remarks>
        /// (Default: Unlimited).
        /// </remarks>
        /// <value>The maximum blob count.</value>
        public int? MaxBlobCount { get; }

        /// <summary>
        /// Gets a value that indicates how the extension will handle values passed on the map variable.
        /// </summary>
        /// <value>The maximum object path depth.</value>
        public MultipartRequestMapHandlingMode MapMode { get; }

        /// <summary>
        /// Gets a value indicating whether the extension should register the default instance of the http
        /// processor that can parse multi-part form requests is required. Set this value to false if you want to extend
        /// the default functionality of the http processor.
        /// </summary>
        /// <remarks>
        /// (Default: <c>true</c>).</remarks>
        /// <value><c>true</c> if the default multi-part request http processor is required; otherwise, <c>false</c>.</value>
        public bool RegisterMultipartRequestHttpProcessor { get; }

        /// <summary>
        /// Gets a value indicating whether the custom http processor that can parse multi-part form requests is required.
        /// When <c>true</c>, at runtime, if the configured processor for this schema does not inherit
        /// from <see cref="MultipartRequestGraphQLHttpProcessor{TSchema}" /> an exception will be thrown.
        /// </summary>
        /// <remarks>
        /// (Default: <c>true</c>).</remarks>
        /// <value><c>true</c> if the default multi-part request http processor is required; otherwise, <c>false</c>.</value>
        public bool RequireMultipartRequestHttpProcessor { get; }
    }
}