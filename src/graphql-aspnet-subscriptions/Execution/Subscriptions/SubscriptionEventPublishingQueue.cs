// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Subscriptions
{
    using System.Collections.Concurrent;
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// A globally shared, intermediate queue of <see cref="SubscriptionEvent"/> items waiting to be published. When
    /// controllers publish events they are initially staged to this queue where an additional
    /// service dequeue's them and publishes them using the server's configured <see cref="ISubscriptionEventPublisher"/>.
    /// </summary>
    public sealed class SubscriptionEventPublishingQueue : ConcurrentQueue<SubscriptionEvent>
    {
    }
}