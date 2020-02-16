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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A subscription server proxy that executes in process and will raise events directly to the
    /// configured <see cref="ISubscriptionEventListener"/>. This object only raises events
    /// to the locally attached graphql subscription server and DOES NOT scale.
    /// See demo projects for scalable subscription configurations.
    /// </summary>
    public class InProcessSubscriptionPublisher : ISubscriptionEventPublisher
    {
        private readonly ISubscriptionEventListener _eventListener;

        /// <summary>
        /// Initializes a new instance of the <see cref="InProcessSubscriptionPublisher"/> class.
        /// </summary>
        /// <param name="eventListener">The event listener.</param>
        public InProcessSubscriptionPublisher(ISubscriptionEventListener eventListener)
        {
            _eventListener = Validation.ThrowIfNullOrReturn(eventListener, nameof(eventListener));
        }

        /// <summary>
        /// Raises a new event in a manner such that a compatible <see cref="ISubscriptionEventListener" /> could
        /// receive it for processing.
        /// </summary>
        /// <param name="eventData">The event to publish.</param>
        /// <returns>Task.</returns>
        public Task PublishEvent(SubscriptionEvent eventData)
        {
            return _eventListener.RaiseEvent(eventData);
        }
    }
}