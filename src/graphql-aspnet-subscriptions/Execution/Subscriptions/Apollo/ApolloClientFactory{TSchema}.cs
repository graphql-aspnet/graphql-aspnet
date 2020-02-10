// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Subscriptions.Apollo
{
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A abstract factory for generating instances of apollo client proxies to communicate
    /// using the apollo graphql-over-websocket protocol.
    /// </summary>
    /// <typeparam name="TSchema">The type of the t schema.</typeparam>
    public class ApolloClientFactory<TSchema> : ISubscriptionClientFactory<TSchema>
            where TSchema : class, ISchema
    {
        private readonly ApolloClientSupervisor<TSchema> _clientSupervisor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloClientFactory{TSchema}" /> class.
        /// </summary>
        /// <param name="clientSupervisor">The master supervisor to which any created client proxy
        /// will automatically be registered.</param>
        public ApolloClientFactory(ApolloClientSupervisor<TSchema> clientSupervisor)
        {
            _clientSupervisor = Validation.ThrowIfNullOrReturn(clientSupervisor, nameof(clientSupervisor));
        }

        /// <summary>
        /// Creates the client proxy using the underlying <see cref="T:Microsoft.AspNetCore.Http.HttpContext" /> and connected
        /// <see cref="T:System.Net.WebSockets.WebSocket" />.
        /// </summary>
        /// <param name="context">The original http context that initiated the web socket.</param>
        /// <param name="socketProxy">The connected socket resolved by the aspnet runtime.</param>
        /// <param name="options">The configured options for subscriptions for the target schema.</param>
        /// <returns>ISubscriptionClientProxy&lt;TSchema&gt;.</returns>
        public ISubscriptionClientProxy CreateClientProxy(HttpContext context, IClientConnection socketProxy, SchemaSubscriptionOptions<TSchema> options)
        {
            var client = new ApolloClientProxy<TSchema>(context.RequestServices, context.User, socketProxy, options);
            _clientSupervisor.RegisterNewClient(client);

            return client;
        }
    }
}