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
    /// A response to a call to 'addOrUpdateDonut'.
    /// </summary>
    public class AddOrUpdateDonutResponse
    {
        /// <summary>
        /// Gets or sets the add or update donut.
        /// </summary>
        /// <value>The add or update donut.</value>
        public Donut AddOrUpdateDonut { get; set; }
    }
}