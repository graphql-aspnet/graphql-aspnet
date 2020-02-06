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
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// The default implementation of graphql aspnet subscription proxy available exposed via an extension
    /// method to all <see cref="GraphController"/> to publish events that will be transmitted
    /// to the subscription server at the end of the current request.
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