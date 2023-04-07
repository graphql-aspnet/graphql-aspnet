// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Web.Exceptions
{
    using System;
    using System.Net;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// An exception thrown when the runtime is uanble to successfully extract the
    /// data from an <see cref="HttpContext"/> to generate a data package
    /// for the graphql runtime.
    /// </summary>
    public class HttpContextParsingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContextParsingException"/> class.
        /// </summary>
        /// <param name="statusCode">The status code taht should be set, indicating the reason for failure.</param>
        /// <param name="errorMessage">An end user friendly error message to include. This message will be
        /// written directly to the http response.</param>
        public HttpContextParsingException(HttpStatusCode statusCode = HttpStatusCode.BadRequest, string errorMessage = "")
            : this(statusCode, errorMessage, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContextParsingException" /> class.
        /// </summary>
        /// <param name="statusCode">The status code taht should be set, indicating the reason for failure.</param>
        /// <param name="errorMessage">An end user friendly error message to include. This message will be
        /// written directly to the http response.</param>
        /// <param name="innerException">An exception which caused this exception to be thrown.</param>
        public HttpContextParsingException(
            HttpStatusCode statusCode,
            string errorMessage,
            Exception innerException = null)
            : base(errorMessage, innerException)
        {
            this.StatusCode = statusCode;
        }

        /// <summary>
        /// Gets the status code that should be set on the http response, indicating the reason for the failure.
        /// </summary>
        /// <value>The status code.</value>
        public HttpStatusCode StatusCode { get; }
    }
}