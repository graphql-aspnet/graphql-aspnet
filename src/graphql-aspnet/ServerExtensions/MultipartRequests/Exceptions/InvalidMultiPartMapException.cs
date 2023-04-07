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
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Web;
    using GraphQL.AspNet.Web.Exceptions;

    /// <summary>
    /// An exception thrown by the Multipart Request Server extension indicating that something unexpected occured
    /// with the provided map object.
    /// </summary>
    public class InvalidMultiPartMapException : HttpContextParsingException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMultiPartMapException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public InvalidMultiPartMapException(string message, Exception innerException = null)
            : base(System.Net.HttpStatusCode.BadRequest, message, innerException)
        {
        }
    }
}