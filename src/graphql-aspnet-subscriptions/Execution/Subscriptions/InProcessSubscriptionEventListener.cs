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
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// The default listener for raised subscription events. This object only listens to the locally attached
    /// graphql server and DOES NOT scale. See demos for scalable subscription configurations.
    /// </summary>
    /// <typeparam name="TSchema">The type of the t schema.</typeparam>
    public class InProcessSubscriptionEventListener<TSchema> : ISubscriptionEventListener<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// An event raised whenever this listener recieves a new event from the source
        /// its monitoring.
        /// </summary>
        public event SubscriptionEventHandler NewSubscriptionEvent;

        /// <summary>
        /// Adds the event name to the list of events this listener should listen for. The listener
        /// will begin processing events of this type.
        /// </summary>
        /// <param name="name">The fully qualified name of the event.</param>
        public void AddEventType(string name)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Removes the event name from the list of events this listener should listen for. No more
        /// events of this type will be processed.
        /// </summary>
        /// <param name="name">The fully qualified name of the event.</param>
        public void RemoveEventType(string name)
        {
            throw new System.NotImplementedException();
        }
    }
}