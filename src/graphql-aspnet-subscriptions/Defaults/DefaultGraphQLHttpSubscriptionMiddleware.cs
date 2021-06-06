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
    using System.Globalization;
    using System.Net;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Connections.WebSockets;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Logging.Extensions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A default implementation of the logic for handling a subscription request over a websocket.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor is built for.</typeparam>
    public class DefaultGraphQLHttpSubscriptionMiddleware<TSchema>
        where TSchema : class, ISchema
    {
        private readonly ISubscriptionServer<TSchema> _subscriptionServer;
        private readonly RequestDelegate _next;
        private readonly SubscriptionServerOptions<TSchema> _options;
        private readonly string _routePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGraphQLHttpSubscriptionMiddleware{TSchema}" /> class.
        /// </summary>
        /// <param name="next">The delegate pointing to the next middleware component
        /// in the pipeline.</param>
        /// <param name="subscriptionServer">The subscription server configured for
        /// this web host.</param>
        /// <param name="options">The options.</param>
        public DefaultGraphQLHttpSubscriptionMiddleware(
            RequestDelegate next,
            ISubscriptionServer<TSchema> subscriptionServer,
            SubscriptionServerOptions<TSchema> options)
        {
            _next = next;
            _options = Validation.ThrowIfNullOrReturn(options, nameof(options));
            _subscriptionServer = Validation.ThrowIfNullOrReturn(subscriptionServer, nameof(subscriptionServer));
            _routePath = Validation.ThrowIfNullOrReturn(_options.Route, nameof(_options.Route));
        }

        /// <summary>
        /// The invocation method that can process the current http context
        /// if it should be handled as a subscription request.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Task.</returns>
        public virtual async Task InvokeAsync(HttpContext context)
        {
            // immediate bypass if not aimed at this schema subscription route
            var isListeningToPath = string.Compare(
                context.Request.Path,
                _routePath,
                CultureInfo.InvariantCulture,
                CompareOptions.OrdinalIgnoreCase) == 0;

            if (isListeningToPath && context.WebSockets.IsWebSocketRequest)
            {
                var logger = context.RequestServices.GetService<IGraphEventLogger>();

                try
                {
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync(
                        SubscriptionConstants.WebSockets.DEFAULT_SUB_PROTOCOL)
                        .ConfigureAwait(false);

                    IClientConnection socketProxy = new WebSocketClientConnection(webSocket, context);
                    var subscriptionClient = await _subscriptionServer
                            .RegisterNewClient(socketProxy)
                            .ConfigureAwait(false);

                    if (subscriptionClient != null)
                    {
                        logger?.SubscriptionClientRegistered(_subscriptionServer, subscriptionClient);

                        // hold the client connection to keep the socket open
                        await subscriptionClient.StartConnection().ConfigureAwait(false);

                        logger?.SubscriptionClientDropped(subscriptionClient);
                    }
                }
                catch (Exception ex)
                {
                    logger?.UnhandledExceptionEvent(ex);
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        await context.Response.WriteAsync(
                            "An unexpected error occured attempting to configure the web socket connection. " +
                            "Check the server event logs for further details.")
                            .ConfigureAwait(false);
                    }
                }

                return;
            }

            await _next(context).ConfigureAwait(false);
        }
    }
}