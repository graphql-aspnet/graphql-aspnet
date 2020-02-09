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
    using GraphQL.AspNet.Middleware.SubscriptionEventExecution;

    /// <summary>
    /// A middleware component in the subscription event processing pipeline.
    /// </summary>
    public interface ISubscriptionExecutionMiddleware : IGraphMiddlewareComponent<GraphSubscriptionExecutionContext>
    {
    }
}