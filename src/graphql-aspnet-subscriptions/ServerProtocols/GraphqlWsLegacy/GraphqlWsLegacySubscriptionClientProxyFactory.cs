// *************************************************************
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
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.Converters;
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
            var serverOptions = connection.ServiceProvider.GetService<SubscriptionServerOptions<TSchema>>();
            var logger = connection.ServiceProvider.GetService<IGraphEventLogger>();
            var converterFactory = connection.ServiceProvider.GetService<GraphqlWsLegacyMessageConverterFactory>();

            var client = new GraphqlWsLegacyClientProxy<TSchema>(
                connection,
                serverOptions,
                converterFactory,
                this.Protocol,
                logger,
                schema.Configuration.ExecutionOptions.EnableMetrics);

            return Task.FromResult((ISubscriptionClientProxy<TSchema>)client);
        }

        /// <inheritdoc />
        public virtual string Protocol => GraphqlWsLegacyConstants.PROTOCOL_NAME;
    }
}