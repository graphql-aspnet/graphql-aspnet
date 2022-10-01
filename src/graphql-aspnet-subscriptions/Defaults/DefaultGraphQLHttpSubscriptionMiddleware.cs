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
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Connections.WebSockets;
    using GraphQL.AspNet.Exceptions;
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
    /// <typeparam name="TSchema">The type of the schema this middleware component is built for.</typeparam>
    public class DefaultGraphQLHttpSubscriptionMiddleware<TSchema>
        where TSchema : class, ISchema
    {
        private readonly ISubscriptionServerClientFactory _clientFactory;
        private readonly RequestDelegate _next;
        private readonly TSchema _schema;
        private readonly SubscriptionServerOptions<TSchema> _options;
        private readonly string _routePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGraphQLHttpSubscriptionMiddleware{TSchema}" /> class.
        /// </summary>
        /// <param name="next">The delegate pointing to the next middleware component
        /// in the pipeline.</param>
        /// <param name="schema">The schema targeted by this middleware component.</param>
        /// <param name="clientFactory">The client factory used to instantiate
        /// client proxies.</param>
        /// <param name="options">The configuration options the developer
        /// assigned for this schema.</param>
        public DefaultGraphQLHttpSubscriptionMiddleware(
            RequestDelegate next,
            TSchema schema,
            ISubscriptionServerClientFactory clientFactory,
            SubscriptionServerOptions<TSchema> options)
        {
            _next = next;
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _options = Validation.ThrowIfNullOrReturn(options, nameof(options));
            _clientFactory = Validation.ThrowIfNullOrReturn(clientFactory, nameof(clientFactory));
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
            var isListeningToPath = context?.Request?.Path == null || string.Compare(
                context.Request.Path,
                _routePath,
                CultureInfo.InvariantCulture,
                CompareOptions.OrdinalIgnoreCase) == 0;

            if (isListeningToPath && context.WebSockets.IsWebSocketRequest)
            {
                var logger = context.RequestServices.GetService<IGraphEventLogger>();
                ISubscriptionClientProxy<TSchema> subscriptionClient = null;
                IClientConnection clientConnection = null;
                try
                {
                    clientConnection = new WebSocketClientConnection(context);

                    // if this schema instance only allows pre-authenticaed clients
                    // do a hard exit
                    var isAuthenticated = clientConnection.SecurityContext?.DefaultUser != null &&
                                          clientConnection.SecurityContext
                                            .DefaultUser
                                            .Identities
                                            .Any(x => x.IsAuthenticated);

                    if (_options.AuthenticatedRequestsOnly && !isAuthenticated)
                        throw new UnauthenticatedClientConnectionException(clientConnection);

                    subscriptionClient = await _clientFactory.CreateSubscriptionClient<TSchema>(clientConnection);
                    if (subscriptionClient != null)
                    {
                        logger?.SubscriptionClientRegistered<TSchema>(subscriptionClient);

                        // hold the client connection to keep the socket open
                        await subscriptionClient.StartConnection(
                            _options.ConnectionKeepAliveInterval,
                            _options.ConnectionInitializationTimeout,
                            context.RequestAborted).ConfigureAwait(false);

                        logger?.SubscriptionClientDropped(subscriptionClient);
                    }
                }
                catch (UnauthenticatedClientConnectionException)
                {
                    if (clientConnection != null)
                    {
                        await clientConnection.CloseAsync(
                                ConnectionCloseStatus.ProtocolError,
                                "Unauthorized Request",
                                context.RequestAborted)
                            .ConfigureAwait(false);
                    }
                }
                catch (UnsupportedClientProtocolException uspe)
                {
                    logger?.UnsupportedClientProtocol(_schema, uspe.Protocol);
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        await context.Response.WriteAsync(
                            $"The requested messaging protocol(s) '{uspe.Protocol}' are not supported " +
                            $"by the target schema.").ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    logger?.UnhandledExceptionEvent(ex);
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        await context.Response.WriteAsync(
                            "An unexpected error occured attempting to configure the web socket " +
                            "connection. Check the server event logs for further details.")
                            .ConfigureAwait(false);
                    }
                }
                finally
                {
                    subscriptionClient?.Dispose();
                    subscriptionClient = null;
                }

                return;
            }

            await _next(context).ConfigureAwait(false);
        }
    }
}