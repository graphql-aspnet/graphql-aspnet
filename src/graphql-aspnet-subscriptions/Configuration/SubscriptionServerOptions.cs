// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration
{
    using System;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Configuration options relating to subscriptions for a given schema.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor is built for.</typeparam>
    public class SubscriptionServerOptions<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionServerOptions{TSchema}"/> class.
        /// </summary>
        public SubscriptionServerOptions()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the default subscription processing handler
        /// should be registered to the application. If disabled, the application will not register
        /// its middleware component to the asp.net pipeline for handling subscriptions. The application will need to handle
        /// websocket request manually. (Default: false).
        /// </summary>
        /// <value><c>true</c> if the default subscription route and middleware component should not be registered; otherwise, <c>false</c>.</value>
        public bool DisableDefaultRoute { get; set; } = false;

        /// <summary>
        /// Gets or sets the route path where any subscription clients will connect. use <see cref="SubscriptionConstants.Routing.SCHEMA_ROUTE_KEY"/>
        /// as a placeholder for the primary route for this schema. (Default: "{schemaRoute}/subscriptions").
        /// </summary>
        /// <value>The route path for a websocket to connect to.</value>
        public string Route { get; set; } = SubscriptionConstants.Routing.DEFAULT_SUBSCRIPTIONS_ROUTE;

        /// <summary>
        /// <para>Gets or sets an optional .NET type to use as the middleware component
        /// for Subscription requests. When set, this type should accept, as constructor arguments, accept
        /// a <see cref="RequestDelegate"/>, the <see cref="SubscriptionServerOptions{TSchema}"/> for the
        /// target schema and a <see cref="string"/> representing the configured route for the
        /// subscription as constructor parameters. When set to null, Apollo's graphql-over-websocket.
        ///
        /// </para>
        /// <para>It can be advantagous to extend <see cref="DefaultGraphQLHttpSubscriptionMiddleware{TSchema}" />.  See the documentation for further details.</para>
        /// </summary>
        /// <value>The type of the middleware component to use when processing WebSocket Requests received by the application for subscriptions.</value>
        public Type HttpMiddlewareComponentType { get; set; } = null;

        /// <summary>
        /// <para>Gets or sets the amount of time between GraphQL keep alive operation messages
        /// to a connected client for those protocols that support it.</para>
        /// <para>This keep alive is seperate from the socket level keep alive timer. (Default: 2 minutes).</para>
        /// </summary>
        /// <value>The keep alive interval.</value>
        public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromMinutes(2);

        /// <summary>
        /// Gets or sets the size of the message buffer (in bytes), at the application level, to extract and deserialize
        /// the grpahql message on the websocket.  This value is seperate from the socket level buffer size receiving from
        /// the connected size. (Default: 4kb).
        /// </summary>
        /// <value>The size of the message buffer.</value>
        public int MessageBufferSize { get; set; } = 4 * 1024;

        /// <summary>
        /// <para>
        /// Gets or sets the maximum number of connected clients the server will communicate with
        /// at any given time.  If the collected sum total of subscriptions set to receive any given event (or events) exceeds
        /// this value additional subscription communications are throttled (Default: 50 connections).
        /// </para>
        /// </summary>
        /// <value>The maximum concurrent client notifications.</value>
        public int MaxConcurrentClientNotifications { get; set; } = 50;
    }
}