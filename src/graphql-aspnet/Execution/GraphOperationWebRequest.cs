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
    /// A context object representing a single request, by a single requestor, to use through the query execution process.
    /// This request originated as a result of an HTTP request.
    /// </summary>
    [DebuggerDisplay("Query Length = {QueryLength} (Operation = {OperationName})")]
    public class GraphOperationWebRequest : GraphOperationRequest, IGraphOperationWebRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphOperationWebRequest" /> class.
        /// </summary>
        /// <param name="baseRequest">Another formed request from
        /// which this web request should be generated.</param>
        /// <param name="context">The http context that originated this operation request.</param>
        public GraphOperationWebRequest(IGraphOperationRequest baseRequest, HttpContext context)
            : base(baseRequest)
        {
            this.HttpContext = Validation.ThrowIfNullOrReturn(context, nameof(context));
        }

        /// <inheritdoc />
        public HttpContext HttpContext { get; }
    }
}