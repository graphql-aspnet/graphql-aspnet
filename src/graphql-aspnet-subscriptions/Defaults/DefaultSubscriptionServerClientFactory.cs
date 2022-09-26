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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Exceptions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// The default implementation of the client factory which can create clients for the
    /// built-in protocols supported by the library.
    /// </summary>
    public class DefaultSubscriptionServerClientFactory : ISubscriptionServerClientFactory
    {
        private readonly Dictionary<string, ISubscriptionClientProxyFactory> _clientFactories;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSubscriptionServerClientFactory"/> class.
        /// </summary>
        /// <param name="clientFactories">The client factories.</param>
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

            // validate the expected protocol
            var expectedProtocol = connection.RequestedProtocol;
            if (string.IsNullOrWhiteSpace(expectedProtocol))
                expectedProtocol = subscriptionOptions.DefaultProtocol;

            expectedProtocol = expectedProtocol?.Trim() ?? string.Empty;

            // ensure the protocol exists and is registered on the server
            if (!_clientFactories.ContainsKey(expectedProtocol))
                throw new UnknownClientProtocolException(expectedProtocol);

            // ensure the protocol is supported by the target schema.
            if (subscriptionOptions.SupportedProtocols != null)
            {
                if (!subscriptionOptions.SupportedProtocols.Contains(expectedProtocol))
                    throw new UnsupportedClientProtocolException(expectedProtocol);
            }

            // generate the appropriate client
            return await _clientFactories[expectedProtocol].CreateClient<TSchema>(connection);
        }
    }
}
