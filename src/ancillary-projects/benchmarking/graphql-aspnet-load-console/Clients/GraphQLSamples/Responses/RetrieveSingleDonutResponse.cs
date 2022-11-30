// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

using GraphQL.AspNet.SubscriberLoadTest.Models.Models.ClientModels;

namespace GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.GraphQLSamples.Responses;

/// <summary>
/// A response model for a single donut query.
/// </summary>
public class RetrieveSingleDonutResponse
{
    /// <summary>
    /// Gets or sets the donut retrieved.
    /// </summary>
    /// <value>The retrieve donut.</value>
    public Donut RetrieveDonut { get; set; }
}