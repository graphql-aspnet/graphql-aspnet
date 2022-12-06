// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.GraphQLSamples.Responses
{
    using GraphQL.AspNet.SubscriberLoadTest.Models.Models.ClientModels;

    /// <summary>
    /// A response model to the donut update subscription.
    /// </summary>
    public class OnDonutUpdatedResponse
    {
        /// <summary>
        /// Gets or sets the blah blah blah.
        /// </summary>
        /// <value>The on donut updated.</value>
        public Donut OnDonutUpdated { get; set; }
    }
}