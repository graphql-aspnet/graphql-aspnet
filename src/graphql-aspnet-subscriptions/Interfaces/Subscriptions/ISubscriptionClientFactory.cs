// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Subscriptions
{
    using System.Net.WebSockets;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// An interface representing a factory that can generate appropiate client proxies to communicate
    /// with for any given configuration of subscriptions for a given schema. At runtime whenever a schema
    /// accepts a websocket request that connected socket is wrapped in an approrpiate client proxy
    /// to facilitate communications according to its required messaging protocol. This object creates those clients.
    /// </summary>
    /// <typeparam name="TSchema">The type of the t schema.</typeparam>
    public interface ISubscriptionClientFactory<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Creates the client proxy using the underlying <see cref="T:Microsoft.AspNetCore.Http.HttpContext" /> and connected
        /// <see cref="T:System.Net.WebSockets.WebSocket" />.
        /// </summary>
        /// <param name="context">The original http context that initiated the web socket.</param>
        /// <param name="connection">The connected socket resolved by the aspnet runtime.</param>
        /// <param name="options">The configured options for subscriptions for the target schema.</param>
        /// <returns>ISubscriptionClientProxy&lt;TSchema&gt;.</returns>
        ISubscriptionClientProxy CreateClientProxy(HttpContext context, IClientConnection connection, SchemaSubscriptionOptions<TSchema> options);
    }
}