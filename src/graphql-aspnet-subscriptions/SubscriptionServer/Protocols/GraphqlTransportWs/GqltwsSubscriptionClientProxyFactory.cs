﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.Web;
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
            var router = connection.ServiceProvider.GetService<ISubscriptionEventRouter>();
            var logger = connection.ServiceProvider.GetService<IGraphEventLogger>();
            var writer = connection.ServiceProvider.GetService<IQueryResponseWriter<TSchema>>();

            var client = new GqltwsClientProxy<TSchema>(
                connection,
                schema,
                router,
                writer,
                logger,
                schema.Configuration.ExecutionOptions.EnableMetrics);

            return Task.FromResult((ISubscriptionClientProxy<TSchema>)client);
        }

        /// <inheritdoc />
        public string Protocol => GqltwsConstants.PROTOCOL_NAME;
    }
}