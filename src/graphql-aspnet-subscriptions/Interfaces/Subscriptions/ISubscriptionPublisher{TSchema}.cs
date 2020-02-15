// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Subscriptions
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A proxy object used to publish events to a subscription server.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this publisher is sending events for.</typeparam>
    public interface ISubscriptionPublisher<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Raises a new event in a manner such that a compatible <see cref="ISubscriptionEventListener{TSchema}"/> could
        /// receive it for processing.
        /// </summary>
        /// <typeparam name="TData">The type of the data being sent.</typeparam>
        /// <param name="eventName">The schema-unique name of the event.</param>
        /// <param name="dataObject">The data object to send.</param>
        /// <returns>Task.</returns>
        Task PublishEvent<TData>(string eventName, TData dataObject);
    }
}