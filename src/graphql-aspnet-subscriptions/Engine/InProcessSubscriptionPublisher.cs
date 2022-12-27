// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.SubscriptionServer;

    /// <summary>
    /// A subscription server proxy that executes in process and will raise events directly to the
    /// configured <see cref="ISubscriptionEventRouter"/>. This object only raises events
    /// to the locally attached graphql subscription server and DOES NOT scale.
    /// See demo projects for scalable subscription configurations.
    /// </summary>
    public class InProcessSubscriptionPublisher : ISubscriptionEventPublisher
    {
        private readonly ISubscriptionEventRouter _eventRouter;

        /// <summary>
        /// Initializes a new instance of the <see cref="InProcessSubscriptionPublisher"/> class.
        /// </summary>
        /// <param name="eventRouter">The local event router to push messages to.</param>
        public InProcessSubscriptionPublisher(ISubscriptionEventRouter eventRouter)
        {
            _eventRouter = Validation.ThrowIfNullOrReturn(eventRouter, nameof(eventRouter));
        }

        /// <inheritdoc />
        public virtual ValueTask PublishEventAsync(SubscriptionEvent eventData, CancellationToken cancelToken = default)
        {
            // this publisher pushes events raised
            // by mutations and queries directly into the DI-configured router
            // for immediate dispatch within this local instance
            _eventRouter.RaisePublishedEvent(eventData);
            return default;
        }
    }
}