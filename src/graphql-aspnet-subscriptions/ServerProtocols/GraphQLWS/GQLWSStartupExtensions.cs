// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphQLWS
{
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Converters;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extension methods used to register and startup the 'graphql-transport-ws'
    /// protocol.
    /// </summary>
    internal static class GQLWSStartupExtensions
    {
        /// <summary>
        /// Adds the necessary components to properly handle the 'graphql-transport-ws' protocol
        /// on this server.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddGQLWSProtocol(this IServiceCollection serviceCollection)
        {
            if (serviceCollection != null)
            {
                serviceCollection.Add(
                    new ServiceDescriptor(
                        typeof(GQLWSMessageConverterFactory),
                        typeof(GQLWSMessageConverterFactory),
                        ServiceLifetime.Singleton));
            }

            return serviceCollection;
        }
    }
}