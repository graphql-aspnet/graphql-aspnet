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
    using System.Collections.Generic;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Interfaces.Schema;

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
        /// its middleware component to the ASP.NET pipeline for handling subscriptions. The application will need to handle
        /// websocket requests manually. (Default: false, "do include the default route").
        /// </summary>
        /// <value><c>true</c> if the default subscription route and middleware component should not be registered; otherwise, <c>false</c>.</value>
        public bool DisableDefaultRoute { get; set; } = false;

        /// <summary>
        /// Gets or sets the route path where any subscription clients will connect. use <see cref="SubscriptionConstants.Routing.SCHEMA_ROUTE_KEY"/>
        /// as a placeholder for the primary route for this schema. (Default: "{schemaRoute}/graphql").
        /// </summary>
        /// <value>The route path for a websocket to connect to.</value>
        public string Route { get; set; } = SubscriptionConstants.Routing.DEFAULT_SUBSCRIPTIONS_ROUTE;

        /// <summary>
        /// <para>
        /// Gets or sets an optional <see cref="Type"/> to use as the ASP.NET pipeline component to intercept graphql requests.
        /// When set to null, the <see cref="DefaultGraphQLHttpSubscriptionMiddleware{TSchema}"/>
        /// middleware component is used.
        /// </para>
        /// <para>It can be advantagous to extend <see cref="DefaultGraphQLHttpSubscriptionMiddleware{TSchema}" />.
        /// See the documentation for further details.
        /// </para>
        /// <para>
        /// (Default: null).
        /// </para>
        /// </summary>
        /// <value>The type of the middleware component to use when processing WebSocket Requests received by the application for subscriptions.</value>
        public Type HttpMiddlewareComponentType { get; set; } = null;

        /// <summary>
        /// <para>
        /// Gets or sets the amount of time between GraphQL keep alive operation messages
        /// sent to a connected client for those protocols that support it.
        /// </para>
        /// <para>
        /// This keep alive is seperate from any low level commonucation protocol keep alive timer (such as websockets).
        /// </para>
        /// <para>(Default: 2 minutes). Specify <c>null</c> to disable.</para>
        /// </summary>
        /// <value>The keep alive interval to use for this schema.</value>
        public TimeSpan? ConnectionKeepAliveInterval { get; set; } = TimeSpan.FromMinutes(2);

        /// <summary>
        /// <para>
        /// Gets or sets the amount of time to wait between opening a connection to a
        /// subscription client and the receipt of an initialization sequence. When set,
        /// if the client has not transmitted a protocol appropriate initialization message
        /// within the given timeframe the connection is immediately closed.
        /// </para>
        /// <para>(Default: 30 seconds). Specify <c>null</c> to disable.</para>
        /// </summary>
        /// <value>The initialization timeout.</value>
        public TimeSpan? ConnectionInitializationTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// <para>
        /// Gets or sets a value indicating whether web socket clients must be authenticated in order
        /// for their connection to be accepted. When true, unauthenticated clients are immediately rejected.
        /// </para>
        /// <para>In general, this means supplying a bearer token, cookie or other authentication measure
        /// that is validated by ASP.NET prior to the acceptance of a web socket.  Different ASP.NET server configurations will
        /// expect different authentication requirements.
        /// </para>
        /// <para>
        /// (Default: <c>false</c>).</para>
        /// </summary>
        /// <value><c>true</c> if an authenticated request is required to complete
        /// a subscription client connection; otherwise, <c>false</c>.</value>
        public bool AuthenticatedRequestsOnly { get; set; } = false;

        /// <summary>
        /// <para>Gets or sets the string representing the default messaging protocol to use
        /// when a connected client does not specify which protocol they wish to communicate with.
        /// </para>
        /// <para>(Default: null).</para>
        /// </summary>
        /// <value>The default communication protocol for this schema.</value>
        public string DefaultMessageProtocol { get; set; } = null;

        /// <summary>
        /// <para>
        /// Gets or sets a list of communication protocols supported by this schema when set. If
        /// this value is null, all protocols registered with the server instance are allowed.
        /// </para>
        /// <para>(Default: null).</para>
        /// </summary>
        ///
        /// <value>The supported protocols.</value>
        public ISet<string> SupportedMessageProtocols { get; set; }
    }
}