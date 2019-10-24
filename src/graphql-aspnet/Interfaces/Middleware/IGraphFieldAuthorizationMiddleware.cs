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
    using GraphQL.AspNet.Middleware.FieldAuthorization;

    /// <summary>
    /// A middleware component in the field authorization pipeline.
    /// </summary>
    public interface IGraphFieldAuthorizationMiddleware : IGraphMiddlewareComponent<GraphFieldAuthorizationContext>
    {
    }
}