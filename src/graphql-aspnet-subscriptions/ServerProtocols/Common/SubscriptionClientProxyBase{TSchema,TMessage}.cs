﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Middleware.SubcriptionExecution;
    using GraphQL.AspNet.Schemas;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A common base class encapsulating a wide variety of common operations
    /// that any client proxy communicating with a distinct messaging subprotocol, would implement.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this proxy targets.</typeparam>
    /// <typeparam name="TMessage">A common base type representing the messages this proxy
    /// communicates in with its client connection.</typeparam>
    public abstract class SubscriptionClientProxyBase<TSchema, TMessage> : ISubscriptionClientProxy<TSchema>
        where TSchema : class, ISchema
        where TMessage : class, ILoggableClientProxyMessage
    {
        private readonly SubscriptionCollection<TSchema> _subscriptions;
        private readonly ClientTrackedMessageIdSet _reservedSubscriptionIds;
        private readonly TSchema _schema;
        private readonly ISubscriptionEventRouter _router;
        private IClientConnection _clientConnection;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionClientProxyBase{TSchema, TMessageBase}" /> class.
        /// </summary>
        /// <param name="id">The globally unique id assigned to this instance.</param>
        /// <param name="schema">The schema this client listens for.</param>
        /// <param name="clientConnection">The underlying client connection that this proxy communicates with.</param>
        /// <param name="router">The router component that will send this client event data.</param>
        /// <param name="logger">The primary logger object to record events to.</param>
        protected SubscriptionClientProxyBase(
            string id,
            TSchema schema,
            IClientConnection clientConnection,
            ISubscriptionEventRouter router,
            IGraphEventLogger logger = null)
        {
            this.Id = Validation.ThrowIfNullWhiteSpaceOrReturn(id, nameof(id));
            this.Logger = new ClientProxyEventLogger<TSchema>(this, logger);

            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _clientConnection = Validation.ThrowIfNullOrReturn(clientConnection, nameof(clientConnection));
            _router = Validation.ThrowIfNullOrReturn(router, nameof(router));
            _reservedSubscriptionIds = new ClientTrackedMessageIdSet();
            _subscriptions = new SubscriptionCollection<TSchema>();
        }

        /// <summary>
        /// When overriden in a child class, this method fires when the server provided timeframe
        /// for client initailization has expired. This method will always fire when the timeout
        /// completes, it is up to the class implementing this method to appropriate if and
        /// when needed.
        /// </summary>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        protected virtual Task InitializationWindowExpired(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// When overridden in a child class, this method is called on an interval set by the
        /// time provided during <see cref="StartConnection"/>. When called this method should immediately initiate
        /// a keep alive heartbeat sequence with the connected client.
        /// </summary>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        protected virtual Task ExecuteKeepAlive(CancellationToken cancelToken = default)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Deserializes a received message (represented as a UTF-8 encoded byte array) into an
        /// appropriate <typeparamref name="TMessage" /> consistant with the messaging protocol
        /// used by this proxy.
        /// </summary>
        /// <param name="stream">The stream containing the bytes to deserialize.</param>
        /// <returns>TMessage.</returns>
        protected abstract TMessage DeserializeMessage(Stream stream);

        /// <summary>
        /// Serializes the message into an array of UTF-8 encoded bytes that can be transmitted
        /// to the connected client.
        /// </summary>
        /// <param name="message">The message to serialize.</param>
        /// <returns>System.Byte[].</returns>
        protected abstract byte[] SerializeMessage(TMessage message);

        /// <summary>
        /// Called when a client has sent a message to this proxy in a manner appropriate with
        /// this client's protocol.
        /// </summary>
        /// <param name="message">The message that was sent by the connected client.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        protected abstract Task ClientMessageReceived(TMessage message, CancellationToken cancelToken = default);

        /// <summary>
        /// Creates a message, consistant with this proxy's underlying protocol, that
        /// can transmit a completd query operation result. This method is commonly called
        /// via the processing of a newly received subscription event.
        /// </summary>
        /// <param name="subscriptionId">The unqiue identifier provided by the client
        /// that identifies what series of operations this result belongs to.</param>
        /// <param name="operationResult">The data result to transmit.</param>
        /// <returns>TMessage.</returns>
        protected abstract TMessage CreateDataMessage(string subscriptionId, IGraphOperationResult operationResult);

        /// <summary>
        /// Creates a message consistant with this proxy's underlying protocol
        /// that indicates a subscription is done and is no longer subscribed server side.
        /// </summary>
        /// <remarks>
        /// Return <c>null</c> if this protocol supports no such message.
        /// </remarks>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <returns>TMessage.</returns>
        protected abstract TMessage CreateCompleteMessage(string subscriptionId);

        /// <inheritdoc />
        public virtual async Task StartConnection(
            TimeSpan? keepAliveInterval = null,
            TimeSpan? initializationTimeout = null,
            CancellationToken cancelToken = default)
        {
            if (_clientConnection == null || _clientConnection.ClosedForever)
            {
                throw new InvalidOperationException(
                    $"Unable to start this client proxy (id: {this.Id}). It has already " +
                    "been previously closed and cannot be reopened.");
            }

            // accept the connection and begin lisening
            // for messages related to the protocol known to this specific client type
            await _clientConnection.OpenAsync(this.Protocol, cancelToken);

            TimerAsync keepAliveTimer = null;
            TimerAsync initialziationTimer = null;

            try
            {
                if (initializationTimeout.HasValue)
                {
                    initialziationTimer = new TimerAsync(
                        async (token) =>
                        {
                            await this.InitializationWindowExpired(token);
                            await initialziationTimer.Stop();
                        },
                        initializationTimeout.Value,
                        TimeSpan.MaxValue,
                        false);

                    initialziationTimer?.Start();
                }

                if (keepAliveInterval.HasValue)
                {
                    this.IsKeepAliveEnabled = true;
                    keepAliveTimer = new TimerAsync(
                        (token) => this.ExecuteKeepAlive(token),
                        keepAliveInterval.Value,
                        keepAliveInterval.Value,
                        false);

                    keepAliveTimer.Start();
                }

                // message loop
                IClientConnectionReceiveResult result = null;
                if (_clientConnection.State == ClientConnectionState.Open)
                {
                    do
                    {
                        var stream = new MemoryStream();
                        result = await _clientConnection
                                .ReceiveFullMessage(stream)
                                .ConfigureAwait(false);

                        if (result.MessageType == ClientMessageType.Text)
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            var message = this.DeserializeMessage(stream);
                            await this.ClientMessageReceived(message)
                                .ConfigureAwait(false);
                        }
                    }
                    while (!result.CloseStatus.HasValue && _clientConnection.State == ClientConnectionState.Open);
                }

                // immediately kill any timers once we stop
                // listening for messages
                keepAliveTimer?.Stop();
                keepAliveTimer?.Dispose();
                keepAliveTimer = null;
                this.IsKeepAliveEnabled = false;

                initialziationTimer?.Stop();
                initialziationTimer?.Dispose();
                initialziationTimer = null;

                // the connection and resources may already be closed
                // but in case it wasnt formally close it
                if (_clientConnection?.State == ClientConnectionState.Open)
                {
                    await this.CloseConnection(
                        result?.CloseStatus ?? ConnectionCloseStatus.Unknown,
                        result?.CloseStatusDescription,
                        cancelToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    this.ReleaseClientResources();
                }

                _clientConnection = null;
            }
            finally
            {
                // ensure timers are stopped and released
                // even when a server exception may be thrown
                // during message receiving
                keepAliveTimer?.Stop();
                keepAliveTimer?.Dispose();
                keepAliveTimer = null;
                this.IsKeepAliveEnabled = false;

                initialziationTimer?.Stop();
                initialziationTimer?.Dispose();
                initialziationTimer = null;
            }
        }

        /// <inheritdoc />
        public async ValueTask ReceiveEvent(SubscriptionEvent eventData, CancellationToken cancelToken = default)
        {
            // its possible that an event was scheduled by the router
            // when this connection was closed (but before it could be delivered)
            // double check to make sure this client is able to receive events before processing them
            if (_isDisposed
                || _clientConnection == null
                || _clientConnection.State != ClientConnectionState.Open)
            {
                return;
            }

            // force asyncronicity
            await Task.Yield();

            var field = _schema.RetrieveSubscriptionFieldPath(eventData.ToSubscriptionEventName());
            if (field == null)
                return;

            // find the subscriptions that are registered for the received data
            var targetSubscriptions = _subscriptions.RetreiveByRoute(field);
            if (targetSubscriptions.Count == 0)
                return;

            this.Logger.SubscriptionEventReceived(field, targetSubscriptions);

            // execute the individual subscription queries
            // using the provided source data as an input
            var tasks = new List<Task>();
            foreach (var subscription in targetSubscriptions)
            {
                var executionTask = this.ExecuteSubscriptionEvent(subscription, eventData, cancelToken);
                tasks.Add(executionTask);
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the recevied event data against an individual, active subscription.
        /// </summary>
        /// <param name="subscription">The subscription to execute against.</param>
        /// <param name="eventData">The event data to process.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        protected virtual async Task ExecuteSubscriptionEvent(ISubscription<TSchema> subscription, SubscriptionEvent eventData, CancellationToken cancelToken = default)
        {
            var processor = new SubscriptionEventProcessor<TSchema>(_clientConnection.ServiceProvider);

            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(cancelToken, _clientConnection.RequestAborted);

            var context = await processor.ProcessEvent(
                    _clientConnection.SecurityContext,
                    eventData,
                    subscription,
                    combinedToken.Token);

            // ------------------------------
            // send the message with the resultant data package
            // ------------------------------
            var shouldSkip = context.Session.Items.ContainsKey(SubscriptionConstants.ContextDataKeys.SKIP_EVENT);
            if (!shouldSkip)
            {
                var message = this.CreateDataMessage(subscription.Id, context.Result);
                await this.SendMessage(message, combinedToken.Token);
            }

            // ------------------------------
            // stop the subscription if requested
            // ------------------------------
            var shouldComplete = context.Session.Items.ContainsKey(SubscriptionConstants.ContextDataKeys.COMPLETE_SUBSCRIPTION);
            if (shouldComplete)
            {
                var completeMessage = this.CreateCompleteMessage(subscription.Id);
                if (completeMessage != null)
                    await this.SendMessage(completeMessage, combinedToken.Token);
                this.ReleaseSubscription(subscription.Id);
            }
        }

        /// <summary>
        /// Sends a given message down to the connected client.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        protected virtual async Task SendMessage(TMessage message, CancellationToken cancelToken = default)
        {
            Validation.ThrowIfNull(message, nameof(message));

            if (_clientConnection?.State == ClientConnectionState.Open)
            {
                var bytes = this.SerializeMessage(message);
                await _clientConnection.SendAsync(
                    bytes,
                    ClientMessageType.Text,
                    true,
                    cancelToken);
                this.Logger.MessageSent(message);
            }
        }

        /// <summary>
        /// <para>Executes the received query. If the query represents a subscription, and executes correctly,
        /// the subscription is automatically registered with the server and listening
        /// begins.
        /// </para>
        /// <para>
        /// If the query is a single operation (e.g. mutation, query) it is immediately executed
        /// and the results returned.
        /// </para>
        /// </summary>
        /// <param name="subscriptionId">The subscription id.</param>
        /// <param name="queryData">The query data representing the graph query.</param>
        /// <param name="enableMetrics">if set to <c>true</c> metrics will be added
        /// to the generatd query.</param>
        /// <returns>SubscriptionDataExecutionResult&lt;TSchema&gt;.</returns>
        protected virtual async Task<SubscriptionDataExecutionResult<TSchema>> ExecuteQuery(
            string subscriptionId,
            GraphQueryData queryData,
            bool enableMetrics = false)
        {
            subscriptionId = Validation.ThrowIfNullWhiteSpaceOrReturn(subscriptionId, nameof(subscriptionId));
            Validation.ThrowIfNull(queryData, nameof(queryData));

            // ensure the id isnt already in use
            if (!_reservedSubscriptionIds.ReserveMessageId(subscriptionId))
                return SubscriptionDataExecutionResult<TSchema>.DuplicateId(subscriptionId);

            var runtime = _clientConnection.ServiceProvider.GetRequiredService(typeof(IGraphQLRuntime<TSchema>)) as IGraphQLRuntime<TSchema>;
            var logger = _clientConnection.ServiceProvider.GetService<IGraphEventLogger>();
            var request = runtime.CreateRequest(queryData);
            var metricsPackage = enableMetrics ? runtime.CreateMetricsPackage() : null;
            var session = new QuerySession();

            var context = new SubcriptionExecutionContext(
                request,
                this,
                _clientConnection.ServiceProvider,
                session,
                _clientConnection.SecurityContext,
                subscriptionId,
                metrics: metricsPackage,
                logger: logger);

            var result = await runtime.ExecuteRequest(context).ConfigureAwait(false);

            if (context.IsSubscriptionOperation)
            {
                var subscription = context.Subscription as ISubscription<TSchema>;
                if (subscription.IsValid)
                {
                    var totalTracked = _subscriptions.Add(subscription);
                    if (totalTracked == 1)
                    {
                        var eventName = SubscriptionEventName.FromGraphField<TSchema>(subscription.Field);
                        _router.AddReceiver(this, eventName);
                    }

                    this.Logger?.SubscriptionCreated(subscription);
                    return SubscriptionDataExecutionResult<TSchema>.SubscriptionRegistered(subscription);
                }

                _reservedSubscriptionIds.ReleaseMessageId(subscriptionId);
                return SubscriptionDataExecutionResult<TSchema>.OperationFailure(subscriptionId, subscription.Messages);
            }

            // not a subscription, just send back the generated response and close out the id
            _reservedSubscriptionIds.ReleaseMessageId(subscriptionId);
            return SubscriptionDataExecutionResult<TSchema>.SingleOperationCompleted(subscriptionId, result);
        }

        /// <summary>
        /// Ends the subscription such that no further events will be raised and releases
        /// the subscription id from the tracked set.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription.</param>
        /// <returns><c>true</c> if the subscription was located and released, <c>false</c> otherwise.</returns>
        protected virtual bool ReleaseSubscription(string subscriptionId)
        {
            var totalRemaining = _subscriptions.Remove(subscriptionId, out var subFound);

            if (subFound == null)
                return false;

            // unregister the subscription from the router as appropriate
            _reservedSubscriptionIds.ReleaseMessageId(subFound.Id);
            if (totalRemaining == 0)
            {
                var eventName = SubscriptionEventName.FromGraphField<TSchema>(subFound.Field);
                _router.RemoveReceiver(this, eventName);
            }

            this.Logger?.SubscriptionStopped(subFound);
            return true;
        }

        /// <inheritdoc />
        public virtual async Task CloseConnection(
            ConnectionCloseStatus reason,
            string message = null,
            CancellationToken cancelToken = default)
        {
            if (_clientConnection.State == ClientConnectionState.Open)
            {
                await _clientConnection.CloseAsync(reason, message, cancelToken);
            }

            this.ReleaseClientResources();
        }

        /// <summary>
        /// Instructs this client proxy to release any and all resources its currently
        /// tracking. The connection is permnantly closed and will never be reopened.
        /// </summary>
        protected virtual void ReleaseClientResources()
        {
            // discontinue all client registered subscriptions
            // and acknowledge the terminate request
            _router.RemoveReceiver(this);
            _subscriptions.Clear();
            _reservedSubscriptionIds.Clear();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // no resources in base to dispose
                    // client connection should be disposed individually
                }

                _isDisposed = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets a decorated <see cref="IGraphEventLogger"/> with specialized
        /// events for client proxies.
        /// </summary>
        /// <value>The event logger for this instance.</value>
        protected ClientProxyEventLogger<TSchema> Logger { get; }

        /// <summary>
        /// Gets the subscriptions currnetly registered to this istance, keyed on their client
        /// supplied unique id.
        /// </summary>
        /// <value>The subscriptions collection.</value>
        public IReadOnlyDictionary<string, ISubscription<TSchema>> Subscriptions => _subscriptions;

        /// <inheritdoc />
        public string Id { get; }

        /// <inheritdoc />
        public abstract string Protocol { get; }

        /// <summary>
        /// Gets a value indicating whether keep alive pings are currently
        /// enabled on this client.
        /// </summary>
        /// <value><c>true</c> if this instance is sending keep alives to the client; otherwise, <c>false</c>.</value>
        protected bool IsKeepAliveEnabled { get; private set; }
    }
}