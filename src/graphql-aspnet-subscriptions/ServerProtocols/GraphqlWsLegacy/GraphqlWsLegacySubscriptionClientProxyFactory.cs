﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A factory for generating instance of <see cref="ISubscriptionClientProxy{TSchema}"/>
    /// that supports the 'graphql-ws' protocol.
    /// </summary>
    internal class GraphqlWsLegacySubscriptionClientProxyFactory : ISubscriptionClientProxyFactory
    {
        /// <inheritdoc />
        public Task<ISubscriptionClientProxy<TSchema>> CreateClient<TSchema>(IClientConnection connection)
            where TSchema : class, ISchema
        {
            var schema = connection.ServiceProvider.GetService<TSchema>();
            var logger = connection.ServiceProvider.GetService<IGraphEventLogger>();

            var client = new GraphqlWsLegacyClientProxy<TSchema>(
                connection,
                this.Protocol,
                logger,
                schema.Configuration.ExecutionOptions.EnableMetrics);

            return Task.FromResult((ISubscriptionClientProxy<TSchema>)client);
        }

        /// <inheritdoc />
        public virtual string Protocol => GraphqlWsLegacyConstants.PROTOCOL_NAME;
    }
}