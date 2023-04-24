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
    /// <summary>
    /// Extension methods to provide helpful context to the <see cref="MultipartRequestMode"/>
    /// enumeration.
    /// </summary>
    public static class MultipartRequestModeExtensions
    {
        /// <summary>
        /// Determines whether file uploads are enabled for a given set of request mode flags.
        /// </summary>
        /// <param name="requestMode">The request mode.</param>
        /// <returns><c>true</c> if file uploads are allowed; otherwise, <c>false</c>.</returns>
        public static bool IsFileUploadEnabled(this MultipartRequestMode requestMode)
        {
            return requestMode.HasFlag(MultipartRequestMode.FileUploads);
        }

        /// <summary>
        /// Determines whether batch queries are enabled for a given set of request mode flags.
        /// </summary>
        /// <param name="requestMode">The request mode.</param>
        /// <returns><c>true</c> if batch processing is allowed; otherwise, <c>false</c>.</returns>
        public static bool IsBatchProcessingEnabled(this MultipartRequestMode requestMode)
        {
            return requestMode.HasFlag(MultipartRequestMode.BatchQueries);
        }
    }
}