// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A parser that can inspect an <see cref="Microsoft.AspNetCore.Http.HttpContext"/> and parse out
    /// a set of operations and files that conform to the <c>graphql-multipart-request</c> specification.
    /// </summary>
    /// <remarks>
    /// Spec: <see href="https://github.com/jaydenseric/graphql-multipart-request-spec"/>.
    /// </remarks>
    public class MultipartRequestPayloadParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartRequestPayloadParser"/> class.
        /// </summary>
        /// <param name="context">The context to generate from.</param>
        public MultipartRequestPayloadParser(HttpContext context)
        {
            this.HttpContext = Validation.ThrowIfNullOrReturn(context, nameof(context));
        }

        /// <summary>
        /// Generates a payload containing the queries and files uploaded on the context.
        /// </summary>
        /// <returns>A Task&lt;MultipartGraphQlHttpPayload&gt; representing the asynchronous operation.</returns>
        public virtual async Task<MultipartGraphQlHttpPayload> ParseAsync()
        {
            return null;
        }

        /// <summary>
        /// Gets the context being parsed by this instance.
        /// </summary>
        /// <value>The context.</value>
        protected HttpContext HttpContext { get; }

        /// <summary>
        /// Gets a value indicating whether the watched http context is a GET request.
        /// </summary>
        /// <value>A value indicating if the context is a GET request.</value>
        public virtual bool IsGetRequest => string.Equals(this.HttpContext.Request.Method, nameof(HttpMethod.Get), StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets a value indicating whether the watched http context is a POST request.
        /// </summary>
        /// <value>A value indicating if the context is a post request.</value>
        public virtual bool IsPostRequest => string.Equals(this.HttpContext.Request.Method, nameof(HttpMethod.Post), StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets a value indicating whether the content-type of the http request is set to <c>application/graphql</c>.
        /// </summary>
        /// <value>A value indicating if the http post body content type represents a graphql query.</value>
        public virtual bool IsGraphQLBody => string.Equals(this.HttpContext.Request.ContentType, Constants.Web.GRAPHQL_CONTENT_TYPE_HEADER_VALUE, StringComparison.OrdinalIgnoreCase);
    }
}