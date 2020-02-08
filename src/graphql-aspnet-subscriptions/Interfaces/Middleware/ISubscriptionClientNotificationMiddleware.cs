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
    using GraphQL.AspNet.Middleware.ClientNotification;

    /// <summary>
    /// A middleware component in the single client notification pipeline.
    /// </summary>
    public interface ISubscriptionClientNotificationMiddleware : IGraphMiddlewareComponent<ClientNotificationContext>
    {
    }
}