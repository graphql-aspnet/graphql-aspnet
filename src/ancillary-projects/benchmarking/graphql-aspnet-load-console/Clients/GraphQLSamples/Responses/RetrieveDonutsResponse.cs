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

using System.Collections.Generic;

/// <summary>
/// A response from retrieving a single donut.
/// </summary>
public class RetrieveDonutsResponse
{
    /// <summary>
    /// Gets or sets the retrieve donuts.
    /// </summary>
    /// <value>The retrieve donuts.</value>
    public List<Donut> RetrieveDonuts { get; set; }
}