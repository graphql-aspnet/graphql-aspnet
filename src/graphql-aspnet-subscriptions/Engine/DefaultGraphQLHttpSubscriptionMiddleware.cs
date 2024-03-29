﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.SubscriptionServer.Exceptions;
    using GraphQL.AspNet.Web;
    using GraphQL.AspNet.Web.WebSockets;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A middleware component, injected into the ASP.NET HTTP pipeline to
    /// handling a subscription request over a websocket.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this middleware component is built for.</typeparam>
    public sealed class DefaultGraphQLHttpSubscriptionMiddleware<TSchema>
        where TSchema : class, ISchema
    {
        private readonly RequestDelegate _next;
        private readonly ISubscriptionServerClientFactory _clientMaker;
        private readonly TSchema _schema;
        private readonly string _routePath;
        private readonly IGlobalSubscriptionClientProxyCollection _clientTracker;
        private readonly SubscriptionServerOptions<TSchema> _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGraphQLHttpSubscriptionMiddleware{TSchema}" /> class.
        /// </summary>
        /// <param name="next">The delegate pointing to the next middleware component
        /// in the pipeline.</param>
        /// <param name="schema">The schema targeted by this middleware component.</param>
        /// <param name="clientMaker">The client abstract factory used to instantiate
        /// client proxies.</param>
        /// <param name="clientTracker">The global client tracker that monitors
        /// all active clients on this server instance.</param>
        /// <param name="options">The configuration options the developer
        /// assigned for this schema.</param>
        public DefaultGraphQLHttpSubscriptionMiddleware(
            RequestDelegate next,
            TSchema schema,
            ISubscriptionServerClientFactory clientMaker,
            IGlobalSubscriptionClientProxyCollection clientTracker,
            SubscriptionServerOptions<TSchema> options)
        {
            _next = next;
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _options = Validation.ThrowIfNullOrReturn(options, nameof(options));
            _clientMaker = Validation.ThrowIfNullOrReturn(clientMaker, nameof(clientMaker));
            _routePath = Validation.ThrowIfNullOrReturn(_options.Route, nameof(_options.Route));
            _clientTracker = Validation.ThrowIfNullOrReturn(clientTracker, nameof(clientTracker));
        }

        /// <summary>
        /// The invocation method that can process the current http context
        /// if it should be handled as a subscription request.
        /// </summary>
        /// <param name="context">The http context to process.</param>
        /// <returns>Task.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // ---------------------------------
            // Ensure this is a socket request targeted at the url
            // the schema is listening on
            // ---------------------------------
            var isListeningToPath =
                context.WebSockets != null
                && context.WebSockets.IsWebSocketRequest
                && context?.Request?.Path != null
                && string.Compare(
                    context.Request.Path,
                    _routePath,
                    CultureInfo.InvariantCulture,
                    CompareOptions.OrdinalIgnoreCase) == 0;

            if (!isListeningToPath)
            {
                await _next(context).ConfigureAwait(false);
                return;
            }

            var logger = context.RequestServices.GetService<IGraphEventLogger>();
            ISubscriptionClientProxy<TSchema> subscriptionClient = null;
            IClientConnection clientConnection = null;

            try
            {
                clientConnection = new WebSocketClientConnection(context);

                // if this schema instance only allows pre-authenticaed clients
                // do a hard exit
                // ----------------------------
                var isAuthenticated = clientConnection.SecurityContext?.DefaultUser != null &&
                                      clientConnection.SecurityContext
                                        .DefaultUser
                                        .Identities
                                        .Any(x => x.IsAuthenticated);

                if (_options.AuthenticatedRequestsOnly && !isAuthenticated)
                    throw new UnauthenticatedClientConnectionException(clientConnection);

                // wrap the connection in a proxy that abstracts graphql related
                // communications from the underlying communications protocols
                // ----------------------------
                subscriptionClient = await _clientMaker.CreateSubscriptionClientAsync<TSchema>(clientConnection);
                if (subscriptionClient == null)
                    throw new InvalidOperationException("No client proxy could be configred for the connection.");

                var clientAdded = _clientTracker.TryAddClient(subscriptionClient);
                if (!clientAdded)
                    throw new MaxConcurrentClientConnectionsReachedException();

                // begin listening for messages through the connection
                // hold the client connection until operations complete
                // ----------------------------
                logger?.SubscriptionClientRegistered<TSchema>(subscriptionClient);

                await subscriptionClient.StartConnectionAsync(
                    _options.ConnectionKeepAliveInterval,
                    _options.ConnectionInitializationTimeout,
                    context.RequestAborted).ConfigureAwait(false);

                logger?.SubscriptionClientDropped(subscriptionClient);
            }
            catch (MaxConcurrentClientConnectionsReachedException)
            {
                await this.WriteErrorToClientAndCloseAsync(
                            context,
                            clientConnection,
                            (int)ConnectionCloseStatus.InternalServerError,
                            (int)HttpStatusCode.InternalServerError,
                            "Maximum concurrent connections reached")
                    .ConfigureAwait(false);
            }
            catch (UnauthenticatedClientConnectionException)
            {
                await this.WriteErrorToClientAndCloseAsync(
                           context,
                           clientConnection,
                           (int)HttpStatusCode.Unauthorized,
                           (int)HttpStatusCode.Unauthorized,
                           "Unauthorized Request")
                   .ConfigureAwait(false);
            }
            catch (UnsupportedClientProtocolException uspe)
            {
                logger?.UnsupportedClientProtocol(_schema, uspe.Protocol);
                await this.WriteErrorToClientAndCloseAsync(
                         context,
                         clientConnection,
                         (int)ConnectionCloseStatus.ProtocolError,
                         (int)HttpStatusCode.BadRequest,
                         $"The requested messaging protocol(s) '{uspe.Protocol}' are not supported.")
                 .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger?.UnhandledExceptionEvent(ex);
                await this.WriteErrorToClientAndCloseAsync(
                        context,
                        clientConnection,
                        (int)ConnectionCloseStatus.InternalServerError,
                        (int)HttpStatusCode.InternalServerError,
                        "An unexpected error occured attempting to configure the web socket.")
                .ConfigureAwait(false);
            }
            finally
            {
                if (subscriptionClient != null)
                {
                    _clientTracker.TryRemoveClient(subscriptionClient.Id, out _);
                    subscriptionClient.Dispose();
                    subscriptionClient = null;
                }
            }
        }

        private async Task WriteErrorToClientAndCloseAsync(
            HttpContext context,
            IClientConnection clientConnection,
            int clientConnectionStatus,
            int httpStatus,
            string message)
        {
            if (clientConnection != null && clientConnection.State == ClientConnectionState.Open)
            {
                await clientConnection.CloseAsync(
                      (ConnectionCloseStatus)clientConnectionStatus,
                      message,
                      context.RequestAborted)
                  .ConfigureAwait(false);

                return;
            }

            if (context != null && !context.Response.HasStarted)
            {
                // write directly to the http context response when
                // no client connection was successfully created
                context.Response.StatusCode = httpStatus;
                await context.Response.WriteAsync(message)
                    .ConfigureAwait(false);

                return;
            }

            // no client connection intiated and an http response
            // is already started back to the user...this should be an impossible state
            // but there is no garuntee on the order of aspnet middleware components
            // on the server. Regardless, there is nothing we can do.
            //
            // ¯\_(ツ)_/¯
        }
    }
}