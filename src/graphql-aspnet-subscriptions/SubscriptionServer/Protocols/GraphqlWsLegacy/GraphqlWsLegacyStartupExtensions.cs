﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy
{
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Startup extensions for registering the necessary components to support
    /// GraphqlWsLegacy's legacy graphql-ws messaging protocol.
    /// </summary>
    public static class GraphqlWsLegacyStartupExtensions
    {
        /// <summary>
        /// Adds the necessary components to properly handle the legacy 'graphql-ws' protocol
        /// owned by GraphqlWsLegacy on this server.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddGraphqlWsLegacysProtocol(this IServiceCollection serviceCollection)
        {
            if (serviceCollection != null)
            {
                serviceCollection.Add(
                    new ServiceDescriptor(
                        typeof(ISubscriptionClientProxyFactory),
                        typeof(GraphqlWsLegacySubscriptionClientProxyFactory),
                        ServiceLifetime.Singleton));

                serviceCollection.Add(
                    new ServiceDescriptor(
                        typeof(ISubscriptionClientProxyFactory),
                        typeof(GraphqlWsLegacySubscriptionClientProxyFactoryAlternate),
                        ServiceLifetime.Singleton));
            }

            return serviceCollection;
        }
    }
}