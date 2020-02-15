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
    /// configured <see cref="ISubscriptionEventListener{TSchema}"/>. This object only raises events
    /// to the locally attached graphql subscription server and DOES NOT scale.
    /// See demo projects for scalable subscription configurations.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this proxy handles events for.</typeparam>
    public class InProcessSubscriptionServerProxy<TSchema> : ISubscriptionPublisher<TSchema>
            where TSchema : class, ISchema
    {
        private readonly ISubscriptionEventListener<TSchema> _eventListener;

        /// <summary>
        /// Initializes a new instance of the <see cref="InProcessSubscriptionServerProxy{TSchema}"/> class.
        /// </summary>
        /// <param name="eventListener">The event listener.</param>
        public InProcessSubscriptionServerProxy(ISubscriptionEventListener<TSchema> eventListener)
        {
            _eventListener = Validation.ThrowIfNullOrReturn(eventListener, nameof(eventListener));
        }

        /// <summary>
        /// Raises a new event in a manner such that a compatible <see cref="ISubscriptionEventListener{TSchema}"/> could
        /// receive it for processing.
        /// </summary>
        /// <typeparam name="TData">The type of the data being sent.</typeparam>
        /// <param name="eventName">The schema-unique name of the event.</param>
        /// <param name="dataObject">The data object to send.</param>
        /// <returns>Task.</returns>
        public async Task PublishEvent<TData>(string eventName, TData dataObject)
        {
            var eventData = new SubscriptionEvent()
            {
                Data = dataObject,
                DataTypeName = dataObject?.GetType().FullName,
                SchemaTypeName = typeof(TSchema).FullName,
                EventName = eventName,
            };

            await _eventListener.RaiseEvent(eventData);
        }
    }
}