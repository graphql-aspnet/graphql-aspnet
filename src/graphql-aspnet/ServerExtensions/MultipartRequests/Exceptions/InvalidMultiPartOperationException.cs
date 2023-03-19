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
    /// An exception thrown when there is an issue parsing the form data received on a request.
    /// </summary>
    public class InvalidMultiPartOperationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMultiPartOperationException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public InvalidMultiPartOperationException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}