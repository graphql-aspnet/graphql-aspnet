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
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Logging.Extensions;
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
        private readonly ISubscriptionServerClientFactory _clientFactory;
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
        /// <param name="clientFactory">The client factory used to instantiate
        /// client proxies.</param>
        /// <param name="clientTracker">The global client tracker that monitors
        /// all active clients on this server instance.</param>
        /// <param name="options">The configuration options the developer
        /// assigned for this schema.</param>
        public DefaultGraphQLHttpSubscriptionMiddleware(
            RequestDelegate next,
            TSchema schema,
            ISubscriptionServerClientFactory clientFactory,
            IGlobalSubscriptionClientProxyCollection clientTracker,
            SubscriptionServerOptions<TSchema> options)
        {
            _next = next;
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _options = Validation.ThrowIfNullOrReturn(options, nameof(options));
            _clientFactory = Validation.ThrowIfNullOrReturn(clientFactory, nameof(clientFactory));
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
            var isListeningToPath = context?.Request?.Path != null && string.Compare(
                context.Request.Path,
                _routePath,
                CultureInfo.InvariantCulture,
                CompareOptions.OrdinalIgnoreCase) == 0;

            if (!isListeningToPath || !context.WebSockets.IsWebSocketRequest)
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
                subscriptionClient = await _clientFactory.CreateSubscriptionClient<TSchema>(clientConnection);
                if (subscriptionClient == null)
                    throw new InvalidOperationException("No client proxy could be configred for the connection.");

                var clientAdded = _clientTracker.TryAddClient(subscriptionClient);
                if (!clientAdded)
                    throw new MaxConcurrentClientConnectionsReachedException();

                // begin listening for messages through the connection
                // hold the client connection until operations complete
                // ----------------------------
                logger?.SubscriptionClientRegistered<TSchema>(subscriptionClient);

                await subscriptionClient.StartConnection(
                    _options.ConnectionKeepAliveInterval,
                    _options.ConnectionInitializationTimeout,
                    context.RequestAborted).ConfigureAwait(false);

                logger?.SubscriptionClientDropped(subscriptionClient);
            }
            catch (MaxConcurrentClientConnectionsReachedException)
            {
                await this.WriteErrorToClientAndClose(
                            context,
                            clientConnection,
                            (int)ConnectionCloseStatus.InternalServerError,
                            (int)HttpStatusCode.InternalServerError,
                            "Maximum concurrent connections reached")
                    .ConfigureAwait(false);
            }
            catch (UnauthenticatedClientConnectionException)
            {
                await this.WriteErrorToClientAndClose(
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
                await this.WriteErrorToClientAndClose(
                         context,
                         clientConnection,
                         (int)ConnectionCloseStatus.ProtocolError,
                         (int)HttpStatusCode.BadRequest,
                         $"The requested messaging protocol(s) '{uspe.Protocol}' are not supported " +
                          $"by the target schema.")
                 .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger?.UnhandledExceptionEvent(ex);
                await this.WriteErrorToClientAndClose(
                        context,
                        clientConnection,
                        (int)ConnectionCloseStatus.InternalServerError,
                        (int)HttpStatusCode.InternalServerError,
                        "An unexpected error occured attempting to configure the web socket " +
                          "connection. Check the server event logs for further details.")
                .ConfigureAwait(false);
            }
            finally
            {
                if (subscriptionClient != null)
                {
                    _clientTracker.RemoveClient(subscriptionClient);
                    subscriptionClient.Dispose();
                    subscriptionClient = null;
                }
            }
        }

        private async Task WriteErrorToClientAndClose(
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
            }
            else if (context != null && !context.Response.HasStarted)
            {
                context.Response.StatusCode = httpStatus;
                await context.Response.WriteAsync(message)
                    .ConfigureAwait(false);
            }
        }
    }
}