// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ApolloClient
{
    using System.Net.WebSockets;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Clients;
    using GraphQL.AspNet.Interfaces.Messaging;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Messaging;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A abstract factory for generating instances of apollo client proxies to communicate
    /// using the apollo graphql-over-websocket protocol.
    /// </summary>
    /// <typeparam name="TSchema">The type of the t schema.</typeparam>
    public class ApolloClientFactory<TSchema> : ISubscriptionClientFactory<TSchema>
            where TSchema : class, ISchema
    {
        /// <summary>
        /// Creates the client proxy using the underlying <see cref="T:Microsoft.AspNetCore.Http.HttpContext" /> and connected
        /// <see cref="T:System.Net.WebSockets.WebSocket" />.
        /// </summary>
        /// <param name="context">The original http context that initiated the web socket.</param>
        /// <param name="connectedSocket">The connected socket resolved by the aspnet runtime.</param>
        /// <param name="options">The configured options for subscriptions for the target schema.</param>
        /// <returns>ISubscriptionClientProxy&lt;TSchema&gt;.</returns>
        public ISubscriptionClientProxy CreateClientProxy(HttpContext context, WebSocket connectedSocket, SchemaSubscriptionOptions<TSchema> options)
        {
            return new ApolloClientProxy<TSchema>(context, connectedSocket, options);
        }
    }
}