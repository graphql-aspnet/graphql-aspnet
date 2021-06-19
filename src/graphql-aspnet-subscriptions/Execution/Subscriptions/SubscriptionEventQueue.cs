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

    /// <summary>
    /// A shared queue of <see cref="SubscriptionEvent"/> loaded when controllers
    /// publish events and dispatched via a background service.
    /// </summary>
    public class SubscriptionEventQueue : ConcurrentQueue<SubscriptionEvent>
    {
    }
}