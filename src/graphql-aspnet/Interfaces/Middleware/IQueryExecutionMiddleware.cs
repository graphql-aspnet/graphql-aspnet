// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Middleware
{
    using GraphQL.AspNet.Middleware.QueryExecution;

    /// <summary>
    /// A middleware component in the primary query processing pipeline.
    /// </summary>
    public interface IQueryExecutionMiddleware : IGraphMiddlewareComponent<GraphQueryExecutionContext>
    {
    }
}