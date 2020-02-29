// *************************************************************
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
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Apollo.Messages;
    using GraphQL.AspNet.Apollo.Messages.ClientMessages;
    using GraphQL.AspNet.Apollo.Messages.Common;
    using GraphQL.AspNet.Apollo.Messages.ServerMessages;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Execution.Subscriptions.ClientConnections;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.QueryExecution;
    using GraphQL.AspNet.Middleware.SubscriptionQueryExecution;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Schemas;
    using Microsoft.Extensions.DependencyInjection;

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
        private readonly ClientTrackedMessageIdSet _trackedIds;

        /// <summary>
        /// Raised when the supervisor begins monitoring a new subscription.
        /// </summary>
        public event EventHandler<ClientSubscriptionEventArgs<TSchema>> SubscriptionRegistered;

        /// <summary>
        /// Raised when the supervisor stops monitoring a new subscription.
        /// </summary>
        public event EventHandler<ClientSubscriptionEventArgs<TSchema>> SubscriptionRemoved;

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
            _trackedIds = new ClientTrackedMessageIdSet();

            this.Subscriptions = new ClientSubscriptionCollection<TSchema>();
            this.Subscriptions.SubscriptionFieldRegistered += this.Subscriptions_EventRegistered;
            this.Subscriptions.SubscriptionFieldAbandoned += this.Subscriptions_EventAbandoned;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ApolloSubscriptionServer{TSchema}"/> class.
        /// </summary>
        ~ApolloSubscriptionServer()
        {
            _listener?.RemoveReceiver(this);
            this.Subscriptions.SubscriptionFieldRegistered -= this.Subscriptions_EventRegistered;
            this.Subscriptions.SubscriptionFieldAbandoned -= this.Subscriptions_EventAbandoned;
        }

        /// <summary>
        /// Unregister this server as a listener for a given event when the last client disconnects its
        /// subscription registration.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="ApolloTrackedFieldArgs"/> instance containing the event data.</param>
        private void Subscriptions_EventAbandoned(object sender, ApolloTrackedFieldArgs args)
        {
            foreach (var eventName in SubscriptionEventName.FromGraphField<TSchema>(args.Field))
                _listener.RemoveReceiver(eventName, this);
        }

        /// <summary>
        /// Register this server as a listener for the given event when a client first
        /// requests a subscription from it.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="ApolloTrackedFieldArgs"/> instance containing the event data.</param>
        private void Subscriptions_EventRegistered(object sender, ApolloTrackedFieldArgs args)
        {
            foreach (var eventName in SubscriptionEventName.FromGraphField<TSchema>(args.Field))
                _listener.AddReceiver(eventName, this);
        }

        /// <summary>
        /// Receives a new event that was raised by a listener.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <returns>Task.</returns>
        public async Task ReceiveEvent(SubscriptionEvent eventData)
        {
            if (eventData == null)
                return;

            var eventRoute = _schema.RetrieveSubscriptionFieldPath(eventData.ToSubscriptionEventName());
            if (eventRoute == null)
                return;

            var subscriptions = this.Subscriptions.RetrieveSubscriptions(eventRoute);
            if (subscriptions == null && !subscriptions.Any())
                return;

            var cancelSource = new CancellationTokenSource();

            // TODO: Add some timing wrappers with cancel token to ensure no spun out
            // comms.
            var allTasks = subscriptions.Select((sub) => this.ExecuteSubscriptionEvent(sub, eventData, cancelSource.Token));
            await Task.WhenAll(allTasks).ConfigureAwait(false);

            // reawait any faulted tasks so tehy can unbuble any exceptions
            foreach (var task in allTasks.Where(x => x.IsFaulted))
                await task.ConfigureAwait(false);
        }

        private async Task ExecuteSubscriptionEvent(
            ISubscription<TSchema> subscription,
            SubscriptionEvent eventData,
            CancellationToken cancelToken = default)
        {
            await Task.Yield();

            var serviceProvider = subscription.Client.ServiceProvider;
            var runtime = serviceProvider.GetRequiredService<IGraphQLRuntime<TSchema>>();
            var schema = serviceProvider.GetRequiredService<TSchema>();

            IGraphQueryExecutionMetrics metricsPackage = null;
            IGraphEventLogger logger = serviceProvider.GetService<IGraphEventLogger>();

            if (schema.Configuration.ExecutionOptions.EnableMetrics)
            {
                var factory = serviceProvider.GetRequiredService<IGraphQueryExecutionMetricsFactory<TSchema>>();
                metricsPackage = factory.CreateMetricsPackage();
            }

            var context = new GraphQueryExecutionContext(
                runtime.CreateRequest(subscription.QueryData),
                serviceProvider,
                subscription.Client.User,
                metricsPackage,
                logger);

            // register the event data as a source input for the target subscription field
            context.DefaultFieldSources.AddSource(subscription.Field, eventData.Data);
            context.QueryPlan = subscription.QueryPlan;
            context.QueryOperation = subscription.QueryOperation;

            try
            {
                // execute the request through the runtime
                await _eventSendSemaphore.WaitAsync().ConfigureAwait(false);
                await runtime.ExecuteRequest(context, cancelToken)
                    .ContinueWith(
                        task =>
                        {
                            if (task.IsFaulted)
                                return task;

                            // send the message with the resultant data package
                            var message = new ApolloServerDataMessage(subscription.Id, task.Result);
                            return subscription.Client.SendMessage(message);
                        },
                        cancelToken)
                    .ConfigureAwait(false);
            }
            finally
            {
                _eventSendSemaphore.Release();
            }
        }

        /// <summary>
        /// Adds a new subscription to this apollo server. If the subscription is invalid or otherwise
        /// not able to be added to the monitored subscription collection the subscription's client is automatically
        /// notified via a dispatched message.
        /// </summary>
        /// <param name="subscription">The subscription to add.</param>
        /// <returns>True if the subscription was successfully added to the tracked collection, otherwise false.</returns>
        public async Task<bool> AddSubscription(ISubscription subscription)
        {
            bool subAdded = false;
            if (_trackedIds.ReserveMessageId(subscription.Client, subscription.Id))
            {
                subAdded = await this.RegisterSubscriptionOrRespond(subscription)
                    .ConfigureAwait(false);

                if (!subAdded)
                    _trackedIds.ReleaseMessageId(subscription.Client, subscription.Id);
            }

            return subAdded;
        }

        /// <summary>
        /// Attempts to add the subscription to the server tracked collection. THis method
        /// does not validate in flight message ids.
        /// </summary>
        /// <param name="subscription">The subscription to add.</param>
        /// <returns>A value indicating if the subscription was successfully added.</returns>
        private async Task<bool> RegisterSubscriptionOrRespond(ISubscription subscription)
        {
            Validation.ThrowIfNull(subscription, nameof(subscription));
            Validation.ThrowIfNotCastable<ISubscription<TSchema>>(subscription.GetType(), nameof(subscription));

            var registrationComplete = false;
            if (this.Subscriptions.Contains(subscription.Client, subscription.Id))
            {
                await subscription.Client.SendMessage(
                    new ApolloServerErrorMessage(
                        $"A client subscription with id '{subscription.Id}' is already registered.",
                        Constants.ErrorCodes.BAD_REQUEST,
                        lastMessageId: subscription.Id,
                        lastMessageType: ApolloMessageType.START,
                        clientProvidedId: subscription.Id))
                    .ConfigureAwait(false);
            }
            else if (!subscription.IsValid)
            {
                if (subscription.Messages.Count == 1)
                {
                    await subscription.Client.SendMessage(
                        new ApolloServerErrorMessage(
                            subscription.Messages[0],
                            clientProvidedId: subscription.Id))
                        .ConfigureAwait(false);
                }
                else
                {
                    var response = GraphOperationRequest.FromMessages(subscription.Messages, subscription.QueryData);
                    await subscription.Client
                        .SendMessage(new ApolloServerDataMessage(subscription.Id, response))
                        .ConfigureAwait(false);
                }

                await subscription.Client
                    .SendMessage(new ApolloServerCompleteMessage(subscription.Id))
                    .ConfigureAwait(false);
            }
            else
            {
                var schemaSubscription = subscription as ISubscription<TSchema>;
                this.Subscriptions.Add(schemaSubscription);
                this.SubscriptionRegistered?.Invoke(this, new ClientSubscriptionEventArgs<TSchema>(schemaSubscription));
                registrationComplete = true;
            }

            return registrationComplete;
        }

        /// <summary>
        /// Register a newly connected subscription with the server so that it can start sending messages.
        /// </summary>
        /// <param name="client">The client.</param>
        public void RegisterNewClient(ISubscriptionClientProxy client)
        {
            Validation.ThrowIfNull(client, nameof(client));
            Validation.ThrowIfNotCastable<ApolloClientProxy<TSchema>>(client.GetType(), nameof(client));

            if (client is ApolloClientProxy<TSchema> apolloClient)
            {
                _clients.Add(apolloClient);

                apolloClient.ConnectionOpening += this.ApolloClient_ConnectionOpening;
                apolloClient.ConnectionClosed += this.ApolloClient_ConnectionClosed;
            }
        }

        /// <summary>
        /// Handles the ConnectionOpening event of the ApolloClient control. The client raises this event
        /// when its socket connection is fully realized and it starts listening for messages (and can send messages to)
        /// the client.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ApolloClient_ConnectionOpening(object sender, EventArgs e)
        {
            if (!(sender is ApolloClientProxy<TSchema> client))
                return;

            client.RegisterAsyncronousMessageDelegate(this.ApolloClient_MessageRecieved);
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
            client.ConnectionOpening -= this.ApolloClient_ConnectionOpening;

            this.Subscriptions.RemoveAllSubscriptions(client);
            _trackedIds.ReleaseClient(client);
        }

        /// <summary>
        /// Handles the MessageRecieved event of the ApolloClient control. The client raises this event
        /// whenever a message is recieved and successfully parsed from the under lying websocket.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="message">The message.</param>
        /// <returns>TaskMethodBuilder.</returns>
        private Task ApolloClient_MessageRecieved(object sender, ApolloMessage message)
        {
            var client = sender as ApolloClientProxy<TSchema>;
            if (client == null)
                return Task.CompletedTask;

            if (message == null)
                return Task.CompletedTask;

            switch (message.Type)
            {
                case ApolloMessageType.CONNECTION_INIT:
                    return this.AcknowledgeNewConnection(client);

                case ApolloMessageType.START:
                    return this.ExecuteStartRequest(client, message as ApolloClientStartMessage);

                case ApolloMessageType.STOP:
                    return this.ExecuteStopRequest(client, message as ApolloClientStopMessage);

                case ApolloMessageType.CONNECTION_TERMINATE:
                    return this.ShutDownConnection(client);

                default:
                    return this.UnknownMessageRecieved(client, message);
            }
        }

        /// <summary>
        /// Forceablly terminates all active subscriptions for the client and sends the close
        /// request to shut down the underlying connection.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>Task.</returns>
        private Task ShutDownConnection(ApolloClientProxy<TSchema> client)
        {
            // discontinue all client registered subscriptions
            // and acknowledge the terminate request
            this.Subscriptions.RemoveAllSubscriptions(client);

            return client.CloseConnection(
                ClientConnectionCloseStatus.NormalClosure,
                $"Recieved closure request via message '{ApolloMessageTypeExtensions.Serialize(ApolloMessageType.CONNECTION_TERMINATE)}'.");
        }

        /// <summary>
        /// Returns a generic error to the client indicating that the last message recieved was unknown or invalid.
        /// </summary>
        /// <param name="client">The client to return the error to.</param>
        /// <param name="lastMessage">The last message that was received that was unprocessable.</param>
        /// <returns>Task.</returns>
        private Task UnknownMessageRecieved(ApolloClientProxy<TSchema> client, ApolloMessage lastMessage)
        {
            var apolloError = new ApolloServerErrorMessage(
                    "The last message recieved was unknown or could not be processed " +
                    "by this server. This graph ql is configured to use Apollo's GraphQL over websockets " +
                    "message schema.",
                    Constants.ErrorCodes.BAD_REQUEST,
                    lastMessage: lastMessage,
                    clientProvidedId: lastMessage.Id);

            apolloError.Payload.MetaData.Add(
                Constants.Messaging.REFERENCE_RULE_NUMBER,
                "Unknown Message Type");

            apolloError.Payload.MetaData.Add(
                Constants.Messaging.REFERENCE_RULE_URL,
                "https://github.com/apollographql/subscriptions-transport-ws/blob/master/PROTOCOL.md");

            return client.SendMessage(apolloError);
        }

        /// <summary>
        /// Attempts to find and remove a subscription with the given client id on the message for the target subscription.
        /// </summary>
        /// <param name="client">The client to search.</param>
        /// <param name="message">The message containing the subscription id to stop.</param>
        /// <returns>Task.</returns>
        private async Task ExecuteStopRequest(ApolloClientProxy<TSchema> client, ApolloClientStopMessage message)
        {
            if (message?.Id != null)
            {
                var subFound = this.Subscriptions.TryRemoveSubscription(client, message.Id);

                if (subFound != null)
                {
                    _trackedIds.ReleaseMessageId(client, message.Id);
                    await client
                        .SendMessage(new ApolloServerCompleteMessage(subFound.Id))
                        .ConfigureAwait(false);

                    this.SubscriptionRemoved?.Invoke(this, new ClientSubscriptionEventArgs<TSchema>(subFound));
                }
                else
                {
                    var errorMessage = new ApolloServerErrorMessage(
                        $"No active subscription exists with id '{message.Id}'",
                        Constants.ErrorCodes.BAD_REQUEST,
                        lastMessage: message,
                        clientProvidedId: message.Id);

                    await client
                        .SendMessage(errorMessage)
                        .ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Parses the message contents to generate a valid client subscription and adds it to the watched
        /// set for this instance.
        /// </summary>
        /// <param name="client">The client requesting a subscription.</param>
        /// <param name="message">The message with the subscription details.</param>
        private async Task ExecuteStartRequest(ApolloClientProxy<TSchema> client, ApolloClientStartMessage message)
        {
            // ensure the id isnt already in use
            if (!_trackedIds.ReserveMessageId(client, message.Id))
            {
                await client
                    .SendMessage(new ApolloServerErrorMessage(
                        $"The message id {message.Id} is already reserved for an outstanding request and cannot " +
                        "be processed against. Allow the in-progress request to complete or stop the associated subscription.",
                        SubscriptionConstants.ErrorCodes.DUPLICATE_MESSAGE_ID,
                        lastMessage: message,
                        clientProvidedId: message.Id))
                    .ConfigureAwait(false);

                return;
            }

            var retainMessageId = false;

            var runtime = client.ServiceProvider.GetRequiredService(typeof(IGraphQLRuntime<TSchema>)) as IGraphQLRuntime<TSchema>;
            var request = runtime.CreateRequest(message.Payload);

            var metricsPackage = _schema.Configuration.ExecutionOptions.EnableMetrics ? runtime.CreateMetricsPackage() : null;

            var context = new ApolloQueryExecutionContext(
                client,
                request,
                message.Id,
                metricsPackage);

            var result = await runtime
                            .ExecuteRequest(context)
                            .ConfigureAwait(false);

            if (context.IsSubscriptionOperation)
            {
                retainMessageId = await this
                    .RegisterSubscriptionOrRespond(context.Subscription)
                    .ConfigureAwait(false);
            }
            else
            {
                // not a subscription, just send back the generated response and close out the id
                ApolloMessage responseMessage;

                // report syntax errors as error messages
                // allow others to bubble into a fully reslt (per apollo spec)
                if (result.Messages.Count == 1
                    && result.Messages[0].Code == Constants.ErrorCodes.SYNTAX_ERROR)
                {
                    responseMessage = new ApolloServerErrorMessage(
                          result.Messages[0],
                          message,
                          message.Id);
                }
                else
                {
                    responseMessage = new ApolloServerDataMessage(message.Id, result);
                }

                await client
                    .SendMessage(responseMessage)
                    .ConfigureAwait(false);
                await client
                    .SendMessage(new ApolloServerCompleteMessage(message.Id))
                    .ConfigureAwait(false);
            }

            if (!retainMessageId)
                _trackedIds.ReleaseMessageId(client, message.Id);
        }

        /// <summary>
        /// Sends the required startup messages down to the connected client to acknowledge the connection/protocol.
        /// </summary>
        /// <param name="client">The client.</param>
        private async Task AcknowledgeNewConnection(ApolloClientProxy<TSchema> client)
        {
            // protocol dictates the messages must be sent in this order
            // await each send before attempting the next one
            await client.SendMessage(new ApolloServerAckOperationMessage()).ConfigureAwait(false);
            await client.SendMessage(new ApolloKeepAliveOperationMessage()).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the collection of subscriptions this supervisor is managing.
        /// </summary>
        /// <value>The subscriptions.</value>
        public ClientSubscriptionCollection<TSchema> Subscriptions { get; }
    }
}