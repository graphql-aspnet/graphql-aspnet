// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.SubscriptionTransportWs
{
    using GraphQL.AspNet.Apollo.Messages.Converters;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Startup extensions for registering the necessary components to support
    /// apollo's legacy graphql-ws messaging protocol.
    /// </summary>
    public static class ApolloStartupExtensions
    {
        /// <summary>
        /// Adds the necessary components to properly handle the legacy 'graphql-ws' protocol
        /// owned by apollo on this server.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddApollosProtocol(this IServiceCollection serviceCollection)
        {
            if (serviceCollection != null)
            {
                serviceCollection.Add(
                    new ServiceDescriptor(
                        typeof(ApolloMessageConverterFactory),
                        typeof(ApolloMessageConverterFactory),
                        ServiceLifetime.Singleton));

                serviceCollection.Add(
                    new ServiceDescriptor(
                        typeof(ISubscriptionClientProxyFactory),
                        typeof(ApolloSubscriptionClientProxyFactory),
                        ServiceLifetime.Singleton));
            }

            return serviceCollection;
        }
    }
}