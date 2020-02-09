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
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A proxy object used by <see cref="GraphController" /> to submit new events to the
    /// attached subscription server.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this server proxy is sending events for.</typeparam>
    public interface ISubscriptionServerProxy<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Raises a new event to the subscription server so that it may send the supplied
        /// data to listening clients.
        /// </summary>
        /// <typeparam name="TData">The type of the data being sent.</typeparam>
        /// <param name="eventName">The schema-unique name of the event.</param>
        /// <param name="dataObject">The data object to send.</param>
        /// <returns>Task.</returns>
        Task PublishEvent<TData>(string eventName, TData dataObject);
    }
}