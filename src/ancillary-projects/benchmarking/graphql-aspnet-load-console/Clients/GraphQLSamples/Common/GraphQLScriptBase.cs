// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.GraphQLSamples.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.Common;
    using GraphQL.Client.Http;
    using GraphQL.Client.Serializer.SystemTextJson;


    /// <summary>
    /// A client script with base components for managing a graphql connection.
    /// </summary>
    public abstract class GraphQLScriptBase : ClientScriptBase
    {
        private static readonly SystemTextJsonSerializer _serializer =
            new SystemTextJsonSerializer(new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
            });

        private readonly string _url;
        private readonly ConcurrentDictionary<ClientScriptIterationResults, GraphQLHttpClient> _clients;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLScriptBase" /> class.
        /// </summary>
        /// <param name="url">The url to send graphql requests to.</param>
        /// <param name="scriptNumber">The overall script number assigned to this instance.</param>
        /// <param name="executionCategory">The execution catgory this script falls under.</param>
        /// <param name="categorySuffix">The category suffix to apply to this individual script.</param>
        protected GraphQLScriptBase(
            string url,
            int scriptNumber,
            string executionCategory,
            string categorySuffix = "")
            : base(scriptNumber, executionCategory, categorySuffix)
        {
            _url = url;
            _clients = new ConcurrentDictionary<ClientScriptIterationResults, GraphQLHttpClient>();
        }

        /// <inheritdoc />
        protected override Task ExecuteSingleClientIteration(
            ScriptProfileClientOptions options,
            ClientScriptIterationResults recordTo,
            CancellationToken cancelToken = default)
        {
            var client = this.CreateGraphQLClient();
            _clients.TryAdd(recordTo, client);

            return base.ExecuteSingleClientIteration(options, recordTo, cancelToken);
        }

        /// <inheritdoc />
        protected override async Task ExecuteSingleQuery(
            ClientScriptIterationResults recordTo,
            CancellationToken cancelToken = default)
        {
            GraphQLHttpClient client = null;
            if (!_clients.TryGetValue(recordTo, out client))
            {
                recordTo.AddResults("noGraphQLClient");
                return;
            }

            try
            {
                await this.ExecuteSingleGraphQLQuery(recordTo, client, cancelToken);
            }
            catch (Exception ex)
            {
                this.RecordException(recordTo, ex);
            }
        }

        /// <summary>
        /// When overriden in a child class, executes a single graphql query
        /// and records the results.
        /// </summary>
        /// <param name="recordTo">The results set to record to.</param>
        /// <param name="client">The client to execute the request against.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        protected abstract Task ExecuteSingleGraphQLQuery(
            ClientScriptIterationResults recordTo,
            GraphQLHttpClient client,
            CancellationToken cancelToken);

        /// <summary>
        /// Gets the graphql client to use for this script.
        /// </summary>
        /// <returns>The graphql client.</returns>
        protected GraphQLHttpClient CreateGraphQLClient()
        {
            return new GraphQLHttpClient(_url, _serializer);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var kvp in _clients)
                    kvp.Value.Dispose();

                _clients.Clear();
            }

            base.Dispose(disposing);
        }
    }
}