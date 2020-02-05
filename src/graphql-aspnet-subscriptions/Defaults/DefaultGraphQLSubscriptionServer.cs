// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Defaults
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// The default implementation of graphql aspnet subscription proxy available to all 
    /// graph controllers so they can 
    /// </summary>
    public class DefaultGraphQLSubscriptionServerProxy : ISubscriptionServerProxy
    {
        /// <summary>
        /// Publishes the specified event with the given data to the subscription server
        /// where it is delivered to authorized clients.
        /// </summary>
        /// <param name="eventName">Unique name of the event.</param>
        /// <param name="data">The data package to send.</param>
        /// <returns>Task.</returns>
        public async Task Publish(string eventName, object data)
        {
            throw new System.NotImplementedException();
        }
    }
}