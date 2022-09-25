// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphQLWS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Logging;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Converters;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.ServerMessages;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A baseline component acts to centralize the subscription server operations, regardless of if
    /// this server is in-process with the primary graphql runtime or out-of-process on a seperate instance.
    /// </summary>
    /// <typeparam name="TSchema">The schema type this server is registered to handle.</typeparam>
    public class GQLWSSubscriptionServer<TSchema> : ISubscriptionServer<TSchema>, ISubscriptionEventReceiver, IDisposable
        where TSchema : class, ISchema
    {
        private readonly ISubscriptionEventRouter _eventRouter;
        private readonly HashSet<GQLWSClientProxy<TSchema>> _clients;
        private readonly TSchema _schema;
        private readonly SubscriptionServerOptions<TSchema> _serverOptions;
        private readonly SemaphoreSlim _eventSendSemaphore;
        private readonly GQLWSServerEventLogger<TSchema> _logger;
        private readonly object _syncLock = new object();
        private readonly Dictionary<SubscriptionEventName, HashSet<GQLWSClientProxy<TSchema>>> _subCountByName;

        /// <summary>
        /// Initializes a new instance of the <see cref="GQLWSSubscriptionServer{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema instance this sever will use for various comparisons.</param>
        /// <param name="options">The user configured options for this server.</param>
        /// <param name="eventRouter">The listener watching for new events that need to be communicated
        /// to clients managed by this server.</param>
        /// <param name="logger">The logger to record server events to, if any.</param>
        public GQLWSSubscriptionServer(
            TSchema schema,
            SubscriptionServerOptions<TSchema> options,
            ISubscriptionEventRouter eventRouter,
            IGraphEventLogger logger = null)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _serverOptions = Validation.ThrowIfNullOrReturn(options, nameof(options));
            _eventRouter = Validation.ThrowIfNullOrReturn(eventRouter, nameof(eventRouter));
            _clients = new HashSet<GQLWSClientProxy<TSchema>>();
            _eventSendSemaphore = new SemaphoreSlim(_serverOptions.MaxConcurrentClientNotifications);

            _logger = logger != null ? new GQLWSServerEventLogger<TSchema>(this, logger) : null;
            _subCountByName = new Dictionary<SubscriptionEventName, HashSet<GQLWSClientProxy<TSchema>>>(
                SubscriptionEventNameEqualityComparer.Instance);

            this.Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _eventRouter?.RemoveReceiver(this);
                _eventSendSemaphore?.Dispose();
            }
        }

        /// <summary>
        /// Receives a new event that was raised by a listener.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <returns>Task.</returns>
        Task ISubscriptionEventReceiver.ReceiveEvent(SubscriptionEvent eventData)
        {
            return this.ReceiveEvent(eventData);
        }

        /// <summary>
        /// Dispatches the event to all registered clients wishing to receive it.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <returns>The total number of clients notified of the event.</returns>
        public async Task<int> ReceiveEvent(SubscriptionEvent eventData)
        {
            if (eventData == null)
                return 0;

            var eventRoute = _schema.RetrieveSubscriptionFieldPath(eventData.ToSubscriptionEventName());
            if (eventRoute == null)
                return 0;

            // grab a reference to all clients that need the event
            var clientList = new List<GQLWSClientProxy<TSchema>>();
            lock (_syncLock)
            {
                if (_subCountByName.TryGetValue(eventData.ToSubscriptionEventName(), out var clients))
                {
                    clientList.AddRange(clients);
                }
            }

            _logger?.EventReceived(eventData, clientList);
            if (clientList.Count == 0)
                return 0;

            var cancelSource = new CancellationTokenSource();

            // TODO: Add some timing wrappers with cancel token to ensure no spun out
            // comms.
            var allTasks = clientList.Select((client) =>
                this.ExecuteSubscriptionEvent(
                    client,
                    eventRoute,
                    eventData.Data,
                    cancelSource.Token)).ToList();

            await Task.WhenAll(allTasks).ConfigureAwait(false);

            // re-await any faulted tasks so tehy can unbubble any exceptions
            foreach (var task in allTasks.Where(x => x.IsFaulted))
                await task.ConfigureAwait(false);

            return allTasks.Count;
        }

        private async Task ExecuteSubscriptionEvent(
            ISubscriptionClientProxy client,
            SchemaItemPath route,
            object data,
            CancellationToken cancelToken = default)
        {
            try
            {
                // execute the request through the runtime
                await _eventSendSemaphore.WaitAsync().ConfigureAwait(false);
                await client.ReceiveEvent(route, data, cancelToken).ConfigureAwait(false);
            }
            finally
            {
                _eventSendSemaphore.Release();
            }
        }

        /// <inheritdoc />
        public async Task<ISubscriptionClientProxy> RegisterNewClient(IClientConnection connection)
        {
            Validation.ThrowIfNull(connection, nameof(connection));

            var logger = connection.ServiceProvider.GetService<IGraphEventLogger>();
            var gqlwsClient = new GQLWSClientProxy<TSchema>(
                connection,
                _serverOptions,
                new GQLWSMessageConverterFactory(),
                logger,
                _schema.Configuration.ExecutionOptions.EnableMetrics);

            var isAuthenticated = connection.SecurityContext.DefaultUser != null
                                && connection.SecurityContext
                                             .DefaultUser
                                             .Identities
                                             .Any(x => x.IsAuthenticated);

            if (_serverOptions.AuthenticatedRequestsOnly && !isAuthenticated)
            {
                await gqlwsClient.SendMessage(
                    new GQLWSServerErrorMessage(
                        "Unauthorized request.",
                        Constants.ErrorCodes.ACCESS_DENIED,
                        lastMessageId: null));

                await gqlwsClient.CloseConnection(
                    ClientConnectionCloseStatus.ProtocolError,
                    "Unauthorized Request",
                    default);

                return null;
            }

            gqlwsClient.ConnectionOpening += this.GQLWSClient_ConnectionOpening;
            gqlwsClient.ConnectionClosed += this.GQLWSClient_ConnectionClosed;
            gqlwsClient.SubscriptionRouteAdded += this.GQLWSClient_SubscriptionRouteAdded;
            gqlwsClient.SubscriptionRouteRemoved += this.GQLWSClient_SubscriptionRouteRemoved;

            return gqlwsClient;
        }

        private void GQLWSClient_SubscriptionRouteRemoved(object sender, GQLWSSubscriptionFieldEventArgs e)
        {
            var client = sender as GQLWSClientProxy<TSchema>;
            if (client == null)
                return;

            var names = SubscriptionEventName.FromGraphField(_schema, e.Field);
            foreach (var name in names)
            {
                lock (_syncLock)
                {
                    if (_subCountByName.TryGetValue(name, out var clients))
                    {
                        if (clients.Contains(client))
                        {
                            clients.Remove(client);
                            if (clients.Count == 0)
                            {
                                _subCountByName.Remove(name);
                                _eventRouter.RemoveReceiver(name, this);
                                _logger?.EventMonitorEnded(name);
                            }
                        }
                    }
                }
            }
        }

        private void GQLWSClient_SubscriptionRouteAdded(object sender, GQLWSSubscriptionFieldEventArgs e)
        {
            var client = sender as GQLWSClientProxy<TSchema>;
            if (client == null)
                return;

            var names = SubscriptionEventName.FromGraphField(_schema, e.Field);
            foreach (var name in names)
            {
                lock (_syncLock)
                {
                    if (!_subCountByName.ContainsKey(name))
                        _subCountByName.Add(name, new HashSet<GQLWSClientProxy<TSchema>>());

                    _subCountByName[name].Add(client);
                    if (_subCountByName[name].Count == 1)
                    {
                        _eventRouter.AddReceiver(name, this);
                        _logger?.EventMonitorStarted(name);
                    }
                }
            }
        }

        private void GQLWSClient_ConnectionOpening(object sender, EventArgs e)
        {
            var client = sender as GQLWSClientProxy<TSchema>;
            if (client == null)
                return;

            _clients.Add(client);
        }

        private void GQLWSClient_ConnectionClosed(object sender, EventArgs e)
        {
            var client = sender as GQLWSClientProxy<TSchema>;
            if (client == null)
                return;

            _clients.Remove(client);
            client.ConnectionClosed -= this.GQLWSClient_ConnectionClosed;
        }

        /// <summary>
        /// Gets an Id that uniquely identifies this server instance.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; }
    }
}