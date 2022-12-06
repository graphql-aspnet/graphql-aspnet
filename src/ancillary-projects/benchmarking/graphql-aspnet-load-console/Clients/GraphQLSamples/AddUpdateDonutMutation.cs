// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.GraphQLSamples
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL;
    using GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.Common;
    using GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.GraphQLSamples.Common;
    using GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.GraphQLSamples.Responses;
    using GraphQL.Client.Http;

    /// <summary>
    /// A client script that will execute a query to retrieve a donut from
    /// the server.
    /// </summary>
    public class AddUpdateDonutMutation : GraphQLScriptBase
    {
        private const string QUERY_TEXT = @"mutation {
                    addOrUpdateDonut(donut: { id: ""1""  name: ""BIG PINK"", flavor: STRAWBERRY }) {
                        id
                        name
                        flavor
                    }
                }";

        /// <summary>
        /// Initializes a new instance of the <see cref="AddUpdateDonutMutation" /> class.
        /// </summary>
        /// <param name="url">The url to query against.</param>
        /// <param name="scriptNumber">The individual script number of this instance.</param>
        /// <param name="categorySuffix">The execution category suffix
        /// to apply to this individual script instance.</param>
        public AddUpdateDonutMutation(string url, int scriptNumber, string categorySuffix = "")
            : base(url, scriptNumber, "UpdateDonut", categorySuffix)
        {
        }

        /// <inheritdoc />
        protected override async Task ExecuteSingleGraphQLQuery(
            ClientScriptIterationResults recordTo,
            GraphQLHttpClient client,
            CancellationToken cancelToken = default)
        {
            var request = new GraphQLRequest(QUERY_TEXT, null);

            try
            {
                var r = await client.SendQueryAsync<AddOrUpdateDonutResponse>(request, cancelToken);
                var result = r.AsGraphQLHttpResponse();

                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    recordTo.AddResultsByStatusCode((int)result.StatusCode);
                }
                else if (!string.IsNullOrWhiteSpace(r.Data?.AddOrUpdateDonut.Id)
                    && !string.IsNullOrWhiteSpace(r.Data?.AddOrUpdateDonut.Name)
                    && !string.IsNullOrWhiteSpace(r.Data?.AddOrUpdateDonut.Flavor))
                {
                    recordTo.AddResults("success");
                }
                else
                {
                    recordTo.AddResults("invalid");
                }
            }
            catch (Exception ex)
            {
                this.RecordException(recordTo, ex);
            }
        }
    }
}