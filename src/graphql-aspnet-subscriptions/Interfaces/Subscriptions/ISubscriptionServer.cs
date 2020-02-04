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
    /// <summary>
    /// An interface representing a pub/sub server contract to which events can be published
    /// and automatically, and securely, routed to subscribers via graphql subscriptions.
    /// </summary>
    public interface ISubscriptionServer
    {
        /// <summary>
        /// Publishes the specified event with the supplied data to the subscription server
        /// where it is delivered to authorized clients.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="data">The data package to send.</param>
        void Publish(string eventName, object data);

        void Register();
    }
}