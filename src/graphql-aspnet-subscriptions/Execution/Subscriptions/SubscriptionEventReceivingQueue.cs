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
    /// A globally shared, intermediate queue of <see cref="SubscriptionEvent"/> items waiting to be consumed by
    /// receivers. When deserialized events are first communicated to the <see cref="ISubscriptionEventRouter"/>
    /// they are queued to this internal queue before being dispatched to any receivers
    /// to prevent resources overloading in high volume scenarios.
    /// </summary>
    public sealed class SubscriptionEventReceivingQueue : ConcurrentQueue<SubscriptionEvent>
    {
    }
}