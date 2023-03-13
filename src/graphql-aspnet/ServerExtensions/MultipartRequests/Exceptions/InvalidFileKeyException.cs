// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests.Exceptions
{
    using System;

    /// <summary>
    /// An exception thrown when attempting to resolve a file map, but the indicated file key (i.e. field name)
    /// was not found on the parsed multi-part form.
    /// </summary>
    public class InvalidFileKeyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidFileKeyException"/> class.
        /// </summary>
        /// <param name="fileMapKey">The file map key that was missing.</param>
        public InvalidFileKeyException(string fileMapKey)
            : base($"The file map key '{fileMapKey}' was not found in the supplied form")
        {
        }
    }
}