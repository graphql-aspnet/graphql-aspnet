﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphQLWS
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Converters;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A factory for generating instance of <see cref="ISubscriptionClientProxy{TSchema}"/>
    /// that supports the 'graphql-transport-ws' protocol.
    /// </summary>
    internal class GqltwsSubscriptionClientProxyFactory : ISubscriptionClientProxyFactory
    {
        /// <inheritdoc />
        public Task<ISubscriptionClientProxy<TSchema>> CreateClient<TSchema>(IClientConnection connection)
            where TSchema : class, ISchema
        {
            var schema = connection.ServiceProvider.GetService<TSchema>();
            var serverOptions = connection.ServiceProvider.GetService<SubscriptionServerOptions<TSchema>>();
            var logger = connection.ServiceProvider.GetService<IGraphEventLogger>();
            var converterFactory = connection.ServiceProvider.GetService<GqltwsMessageConverterFactory>();

            var client = new GqltwsClientProxy<TSchema>(
                connection,
                serverOptions,
                converterFactory,
                logger,
                schema.Configuration.ExecutionOptions.EnableMetrics);

            return Task.FromResult((ISubscriptionClientProxy<TSchema>)client);
        }

        /// <inheritdoc />
        public string Protocol => GqltwsConstants.PROTOCOL_NAME;
    }
}