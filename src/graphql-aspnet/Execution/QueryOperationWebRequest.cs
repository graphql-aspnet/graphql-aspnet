// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A wrapper on a <see cref="IQueryOperationRequest"/> to provide
    /// additional details related to an ASP.NET web request.
    /// </summary>
    [DebuggerDisplay("Query Length = {QueryLength} (Operation = {OperationName})")]
    public class QueryOperationWebRequest : QueryOperationRequest, IQueryOperationWebRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryOperationWebRequest" /> class.
        /// </summary>
        /// <param name="baseRequest">Another formed request from
        /// which this web request should be generated.</param>
        /// <param name="context">The http context that originated this operation request.</param>
        public QueryOperationWebRequest(IQueryOperationRequest baseRequest, HttpContext context)
            : base(baseRequest)
        {
            this.HttpContext = Validation.ThrowIfNullOrReturn(context, nameof(context));
        }

        /// <inheritdoc />
        public HttpContext HttpContext { get; }
    }
}