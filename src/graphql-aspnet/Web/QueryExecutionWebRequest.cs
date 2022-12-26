// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Web
{
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Web;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A wrapper on a <see cref="IQueryExecutionRequest"/> to provide
    /// additional details related to an ASP.NET web request.
    /// </summary>
    [DebuggerDisplay("Query Length = {QueryLength} (Operation = {OperationName})")]
    public class QueryExecutionWebRequest : QueryExecutionRequest, IQueryExecutionWebRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryExecutionWebRequest" /> class.
        /// </summary>
        /// <param name="baseRequest">Another formed request from
        /// which this web request should be generated.</param>
        /// <param name="context">The http context that originated this operation request.</param>
        public QueryExecutionWebRequest(IQueryExecutionRequest baseRequest, HttpContext context)
            : base(baseRequest)
        {
            this.HttpContext = Validation.ThrowIfNullOrReturn(context, nameof(context));
        }

        /// <inheritdoc />
        public HttpContext HttpContext { get; }
    }
}