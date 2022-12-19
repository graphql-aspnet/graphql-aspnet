// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.SubscriptionServer.Exceptions;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// The default implementation of the "server owned" abstract factory which uses all registered
    /// client factories to create an appropriate proxy through which the server can speak with
    /// a connected client.
    /// </summary>
    internal sealed class DefaultSubscriptionServerClientFactory : ISubscriptionServerClientFactory
    {
        private readonly Dictionary<string, ISubscriptionClientProxyFactory> _clientFactories;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSubscriptionServerClientFactory"/> class.
        /// </summary>
        /// <param name="clientFactories">The client factories registered with this application.</param>
        public DefaultSubscriptionServerClientFactory(IEnumerable<ISubscriptionClientProxyFactory> clientFactories)
        {
            Validation.ThrowIfNull(clientFactories, nameof(clientFactories));
            if (!clientFactories.Any())
            {
                throw new InvalidOperationException($"At least one {nameof(ISubscriptionClientProxyFactory)} must be " +
                    $"registered to the DI container.");
            }

            _clientFactories = new Dictionary<string, ISubscriptionClientProxyFactory>(StringComparer.OrdinalIgnoreCase);
            foreach (var factory in clientFactories)
                _clientFactories.Add(factory.Protocol, factory);
        }

        /// <inheritdoc />
        public async Task<ISubscriptionClientProxy<TSchema>> CreateSubscriptionClient<TSchema>(IClientConnection connection)
        where TSchema : class, ISchema
        {
            Validation.ThrowIfNull(connection, nameof(connection));
            Validation.ThrowIfNull(connection.ServiceProvider, $"{nameof(connection)}.{nameof(connection.ServiceProvider)}");

            var subscriptionOptions = connection.ServiceProvider.GetService<SubscriptionServerOptions<TSchema>>();

            // if the client doesn't specify a protocol they can accept
            // use the default for the schema (assuming one exists)
            var protocolData = connection.RequestedProtocols;
            if (string.IsNullOrWhiteSpace(protocolData))
                protocolData = subscriptionOptions?.DefaultMessageProtocol;

            protocolData = protocolData?.Trim();
            if (string.IsNullOrWhiteSpace(protocolData))
                throw new UnsupportedClientProtocolException("~none~");

            // a client may submit multiple protocoles as a comma seperated list
            // try each protocol key, in order received, until a match is found
            var requestedProtocols = protocolData.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            string protocolToUse = null;
            HashSet<string> unsupportedProtocols = null;
            foreach (var protocol in requestedProtocols)
            {
                var testProtocol = protocol.Trim();

                // ensure the protocol exists and is registered on the server
                if (!_clientFactories.ContainsKey(testProtocol))
                {
                    unsupportedProtocols = unsupportedProtocols ?? new HashSet<string>();
                    unsupportedProtocols.Add(testProtocol);
                    continue;
                }

                // ensure the protocol is allowed by the target schema
                if (subscriptionOptions?.SupportedMessageProtocols != null)
                {
                    if (!subscriptionOptions.SupportedMessageProtocols.Contains(testProtocol))
                    {
                        unsupportedProtocols = unsupportedProtocols ?? new HashSet<string>();
                        unsupportedProtocols.Add(testProtocol);
                        continue;
                    }
                }

                // match detected!
                protocolToUse = testProtocol;
                break;
            }

            // generate the appropriate client
            if (string.IsNullOrWhiteSpace(protocolToUse))
                throw new UnsupportedClientProtocolException(string.Join(", ", unsupportedProtocols));

            return await _clientFactories[protocolToUse].CreateClient<TSchema>(connection);
        }
    }
}