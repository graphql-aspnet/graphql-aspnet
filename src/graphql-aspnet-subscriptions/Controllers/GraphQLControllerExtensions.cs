// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Controllers
{
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods to expose subscription to graph controllers.
    /// </summary>
    public static class GraphQLControllerExtensions
    {
        /// <summary>
        /// Publishes an event to the subscription server so it can be sent to
        /// the appropriate subscribed clients.
        /// </summary>
        /// <param name="controller">The controller initiating the event.</param>
        /// <param name="eventName">Name schema-unique name of the event being raised .</param>
        /// <param name="dataObject">The data object being published.</param>
        /// <returns>Task.</returns>
        public static Task PublishSubscriptionEvent(this GraphController controller, string eventName, object dataObject)
        {
            var schema = controller.sc
            return Task.CompletedTask;
        }
    }
}