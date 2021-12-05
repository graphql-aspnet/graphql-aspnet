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
    using GraphQL.AspNet.Execution.Contexts;

    /// <summary>
    /// A middleware component in the field authorization pipeline.
    /// </summary>
    public interface IGraphFieldSecurityMiddleware : IGraphMiddlewareComponent<GraphFieldSecurityContext>
    {
    }
}