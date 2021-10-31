// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution
{
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A context object representing a single request, by a single user through the query execution.
    /// Requests implementing this interface declare that they originate from an http request and
    /// contain a valid <see cref="Microsoft.AspNetCore.Http.HttpContext"/>.
    /// </summary>
    public interface IGraphOperationWebRequest : IGraphOperationRequest
    {
        /// <summary>
        /// Gets the <see cref="Microsoft.AspNetCore.Http.HttpContext"/> from which this operation request was generated.
        /// </summary>
        /// <value>The active http context.</value>
        HttpContext HttpContext { get; }
    }
}