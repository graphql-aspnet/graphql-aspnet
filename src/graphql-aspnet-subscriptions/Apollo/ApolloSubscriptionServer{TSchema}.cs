﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Apollo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Apollo.Messages.Converters;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A baseline component acts to centralize the subscription server operations, regardless of if
    /// this server is in-process with the primary graphql runtime or out-of-process on a seperate instance.
    /// </summary>
    /// <typeparam name="TSchema">The schema type this server is registered to handle.</typeparam>
    public class ApolloSubscriptionServer<TSchema> : ISubscriptionServer<TSchema>, ISubscriptionEventReceiver
        where TSchema : class, ISchema
    {
        private readonly ISubscriptionEventListener _listener;
        private readonly HashSet<ApolloClientProxy<TSchema>> _clients;
        private readonly TSchema _schema;
        private readonly SubscriptionServerOptions<TSchema> _serverOptions;
        private readonly SemaphoreSlim _eventSendSemaphore;

        private readonly object _syncLock = new object();
        private readonly Dictionary<SubscriptionEventName, HashSet<ISubscriptionClientProxy>> _subCountByName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloSubscriptionServer{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema instance this sever will use for various comparisons.</param>
        /// <param name="options">The user configured options for this server.</param>
        /// <param name="listener">The listener watching for new events that need to be communicated
        /// to clients managed by this server.</param>
        public ApolloSubscriptionServer(
            TSchema schema,
            SubscriptionServerOptions<TSchema> options,
            ISubscriptionEventListener listener)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _serverOptions = Validation.ThrowIfNullOrReturn(options, nameof(options));
            _listener = Validation.ThrowIfNullOrReturn(listener, nameof(listener));
            _clients = new HashSet<ApolloClientProxy<TSchema>>();
            _eventSendSemaphore = new SemaphoreSlim(_serverOptions.MaxConcurrentClientNotifications);

            _subCountByName = new Dictionary<SubscriptionEventName, HashSet<ISubscriptionClientProxy>>(
                SubscriptionEventNameEqualityComparer.Instance);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ApolloSubscriptionServer{TSchema}"/> class.
        /// </summary>
        ~ApolloSubscriptionServer()
        {
            _listener?.RemoveReceiver(this);
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

            // grab a reference to all clients that need the event
            var clientList = new List<ISubscriptionClientProxy>();
            lock (_syncLock)
            {
                if (!_subCountByName.TryGetValue(eventData.ToSubscriptionEventName(), out var clients))
                    return 0;

                clientList.AddRange(clients);
            }

            var eventRoute = _schema.RetrieveSubscriptionFieldPath(eventData.ToSubscriptionEventName());
            if (eventRoute == null)
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

            // re-await any faulted tasks so tehy can unbuble any exceptions
            foreach (var task in allTasks.Where(x => x.IsFaulted))
                await task.ConfigureAwait(false);

            return allTasks.Count;
        }

        private async Task ExecuteSubscriptionEvent(
            ISubscriptionClientProxy client,
            GraphFieldPath route,
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
        /// Attempts to add the subscription to the server tracked collection. THis method
        /// does not validate in flight message ids.
        /// </summary>
        /// <param name="connection">The connection representing the newly connected client.</param>
        /// <returns>A value indicating if the subscription was successfully added.</returns>
        public Task<ISubscriptionClientProxy> RegisterNewClient(IClientConnection connection)
        {
            Validation.ThrowIfNull(connection, nameof(connection));

            var apolloClient = new ApolloClientProxy<TSchema>(
                connection,
                _serverOptions,
                new ApolloMessageConverterFactory(),
                _schema.Configuration.ExecutionOptions.EnableMetrics);

            apolloClient.ConnectionOpening += this.ApolloClient_ConnectionOpening;
            apolloClient.ConnectionClosed += this.ApolloClient_ConnectionClosed;
            apolloClient.SubscriptionRouteAdded += this.ApolloClient_SubscriptionRouteAdded;
            apolloClient.SubscriptionRouteRemoved += this.ApolloClient_SubscriptionRouteRemoved;

            return apolloClient.AsCompletedTask<ISubscriptionClientProxy>();
        }

        private void ApolloClient_SubscriptionRouteRemoved(object sender, ApolloSubscriptionFieldEventArgs e)
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
                                _listener.RemoveReceiver(name, this);
                            }
                        }
                    }
                }
            }
        }

        private void ApolloClient_SubscriptionRouteAdded(object sender, ApolloSubscriptionFieldEventArgs e)
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
                        _subCountByName.Add(name, new HashSet<ISubscriptionClientProxy>());

                    _subCountByName[name].Add(client);
                    if (_subCountByName[name].Count == 1)
                    {
                        _listener.AddReceiver(name, this);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ConnectionOpening event of the ApolloClient control. The client raises this event
        /// when its message pump begins receiving messages. The server keeps track of the client until it is shut down.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ApolloClient_ConnectionOpening(object sender, EventArgs e)
        {
            var client = sender as ApolloClientProxy<TSchema>;
            if (client == null)
                return;

            _clients.Add(client);
        }

        /// <summary>
        /// Handles the ConnectionClosed event of the ApolloClient control. The client raises this event
        /// when its underlying websocket is no longer maintained and has shutdown.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ApolloClient_ConnectionClosed(object sender, EventArgs e)
        {
            var client = sender as ApolloClientProxy<TSchema>;
            if (client == null)
                return;

            _clients.Remove(client);
            client.ConnectionClosed -= this.ApolloClient_ConnectionClosed;
        }
    }
}