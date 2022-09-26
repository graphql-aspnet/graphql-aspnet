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
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.ServerMessages;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// The standard implementation of the subscription server component.
    /// </summary>
    /// <typeparam name="TSchema">The type of the t schema.</typeparam>
    public class DefaultSubscriptionServer<TSchema> : ISubscriptionServer<TSchema>, IDisposable
        where TSchema : class, ISchema
    {
        private readonly TSchema _schema;
        private readonly ISubscriptionEventRouter _eventRouter;
        private readonly SubscriptionServerOptions<TSchema> _serverOptions;
        private readonly HashSet<ISubscriptionClientProxy<TSchema>> _activeClients;
        private readonly SubscriptionServerEventLogger<TSchema> _logger;

        private readonly object _syncLock = new object();
        private readonly SemaphoreSlim _eventSendSemaphore;
        private readonly Dictionary<SubscriptionEventName, HashSet<ISubscriptionClientProxy<TSchema>>> _subCountByName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSubscriptionServer{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema this server is registered for.</param>
        /// <param name="serverOptions">The subscription options used to configure the server instnace.</param>
        /// <param name="eventRouter">The event listener through which
        /// raised subscription events will be communicated to this instance for dispatch to its connected clients.</param>
        /// <param name="logger">The logger instance to record entries against.</param>
        public DefaultSubscriptionServer(
            TSchema schema,
            SubscriptionServerOptions<TSchema> serverOptions,
            ISubscriptionEventRouter eventRouter,
            IGraphEventLogger logger = null)
        {

            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _serverOptions = Validation.ThrowIfNullOrReturn(serverOptions, nameof(serverOptions));
            _eventRouter = Validation.ThrowIfNullOrReturn(eventRouter, nameof(eventRouter));

            _activeClients = new HashSet<ISubscriptionClientProxy<TSchema>>();
            _eventSendSemaphore = new SemaphoreSlim(_serverOptions.MaxConcurrentClientNotifications);
            _subCountByName = new Dictionary<SubscriptionEventName, HashSet<ISubscriptionClientProxy<TSchema>>>(
                SubscriptionEventNameEqualityComparer.Instance);

            _logger = logger != null ? new SubscriptionServerEventLogger<TSchema>(this, logger) : null;

            this.Id = "default-subscription-server-" + _schema.Name;
        }

        /// <inheritdoc />
        public async Task<bool> RegisterNewClient(ISubscriptionClientProxy<TSchema> clientProxy)
        {
            Validation.ThrowIfNull(clientProxy, nameof(clientProxy));

            var isAuthenticated = clientProxy.SecurityContext.DefaultUser != null &&
                                  clientProxy.SecurityContext
                                    .DefaultUser
                                    .Identities
                                    .Any(x => x.IsAuthenticated);

            if (_serverOptions.AuthenticatedRequestsOnly && !isAuthenticated)
            {
                await clientProxy.SendErrorMessage(
                    new GraphExecutionMessage(
                        GraphMessageSeverity.Critical,
                        message: "Unauthorized request.",
                        code: Constants.ErrorCodes.ACCESS_DENIED));

                await clientProxy.CloseConnection(
                    ClientConnectionCloseStatus.ProtocolError,
                    "Unauthorized Request",
                    default);

                return false;
            }

            clientProxy.ConnectionOpening += this.SubscriptionClient_ConnectionOpening;
            clientProxy.ConnectionClosed += this.SubscriptionClient_ConnectionClosed;
            clientProxy.SubscriptionRouteAdded += this.SubscriptionClient_SubscriptionRouteAdded;
            clientProxy.SubscriptionRouteRemoved += this.SubscriptionClient_SubscriptionRouteRemoved;
            return true;
        }

        private void SubscriptionClient_SubscriptionRouteRemoved(object sender, SubscriptionFieldEventArgs e)
        {
            var client = sender as ISubscriptionClientProxy<TSchema>;
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
                                _eventRouter.RemoveReceiver(this, name);
                                _logger?.EventMonitorEnded(name);
                            }
                        }
                    }
                }
            }
        }

        private void SubscriptionClient_SubscriptionRouteAdded(object sender, SubscriptionFieldEventArgs e)
        {
            var client = sender as ISubscriptionClientProxy<TSchema>;
            if (client == null)
                return;

            var names = SubscriptionEventName.FromGraphField(_schema, e.Field);
            foreach (var name in names)
            {
                lock (_syncLock)
                {
                    if (!_subCountByName.ContainsKey(name))
                        _subCountByName.Add(name, new HashSet<ISubscriptionClientProxy<TSchema>>());

                    _subCountByName[name].Add(client);
                    if (_subCountByName[name].Count == 1)
                    {
                        _eventRouter.AddReceiver(this, name);
                        _logger?.EventMonitorStarted(name);
                    }
                }
            }
        }

        private void SubscriptionClient_ConnectionOpening(object sender, EventArgs e)
        {
            var client = sender as ISubscriptionClientProxy<TSchema>;
            if (client == null)
                return;

            _activeClients.Add(client);
        }

        private void SubscriptionClient_ConnectionClosed(object sender, EventArgs e)
        {
            var client = sender as ISubscriptionClientProxy<TSchema>;
            if (client == null)
                return;

            _activeClients.Remove(client);
            client.ConnectionClosed -= this.SubscriptionClient_ConnectionClosed;
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
            var clientList = new List<ISubscriptionClientProxy<TSchema>>();
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

        /// <inheritdoc />
        public string Id { get; }
    }
}