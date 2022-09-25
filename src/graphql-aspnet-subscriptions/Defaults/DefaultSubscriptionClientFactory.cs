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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Converters;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// The default implementation of the client factory which can create clients for the
    /// built-in protocols supported by the library.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this factory registers clients for.</typeparam>
    /// <remarks>Inherit from this class and register your own instance to extend its functionality.</remarks>
    public class DefaultSubscriptionClientFactory<TSchema> : ISubscriptionServerClientFactory<TSchema>
        where TSchema : class, ISchema
    {
        private readonly TSchema _schema;
        private readonly SubscriptionServerOptions<TSchema> _serverOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSubscriptionClientFactory{TSchema}"/> class.
        /// </summary>
        /// <param name="schema">The schema this factory works for.</param>
        /// <param name="serverOptions">The subscription specific server options for the given schema.</param>
        public DefaultSubscriptionClientFactory(
            TSchema schema,
            SubscriptionServerOptions<TSchema> serverOptions)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _serverOptions = Validation.ThrowIfNullOrReturn(serverOptions, nameof(serverOptions));
        }

        /// <inheritdoc />
        public Task<ISubscriptionClientProxy<TSchema>> CreateSubscriptionClient(IClientConnection connection)
        {
            var expectedProtocol = connection.RequestedProtocol;
            if (string.IsNullOrWhiteSpace(expectedProtocol))
                expectedProtocol = SubscriptionConstants.WebSockets.DEFAULT_SUB_PROTOCOL;

            ISubscriptionClientProxy<TSchema> client = null;
            switch (expectedProtocol)
            {
                case SubscriptionConstants.WebSockets.GRAPHQL_WS_PROTOCOL:
                    var logger = connection.ServiceProvider.GetService<IGraphEventLogger>();
                    client = new GQLWSClientProxy<TSchema>(
                        connection,
                        _serverOptions,
                        new GQLWSMessageConverterFactory(),
                        logger,
                        _schema.Configuration.ExecutionOptions.EnableMetrics);
                    break;

                case SubscriptionConstants.WebSockets.APOLLO_SUBSCRIPTION_TRANSPORT:
                    break;

                // unsupported protocol
                default:
                    break;
            }

            return Task.FromResult(client);
        }
    }
}