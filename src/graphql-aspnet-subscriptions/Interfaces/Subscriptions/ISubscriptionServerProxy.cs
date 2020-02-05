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
    /// An object that proxies the subscription server llowing your mutation/query operations to
    /// publish events that can then be sent to subscribed clients via graphql.
    /// </summary>
    public interface ISubscriptionServerProxy
    {
        /// <summary>
        /// Publishes the specified event with the given data to the subscription server
        /// where it is delivered to authorized clients.
        /// </summary>
        /// <param name="eventName">Unique name of the event.</param>
        /// <param name="data">The data package to send.</param>
        void Publish(string eventName, object data);
    }
}