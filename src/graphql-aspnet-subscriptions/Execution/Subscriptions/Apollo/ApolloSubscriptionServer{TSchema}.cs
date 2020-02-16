// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Subscriptions.Apollo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages.ClientMessages;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages.Common;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages.ServerMessages;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.QueryExecution;
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
        /// <param name="listener">The listener watching for new events that need to be communicated
        /// to clients managed by this server.</param>
        public ApolloSubscriptionServer(TSchema schema, ISubscriptionEventListener listener)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _listener = Validation.ThrowIfNullOrReturn(listener, nameof(listener));
            _clients = new HashSet<ApolloClientProxy<TSchema>>();

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

            var pipelineExecutions = new List<Task>();
            var cancelSource = new CancellationTokenSource();

            foreach (var subscription in subscriptions)
            {
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

                // execute the request through the runtime
                var task = runtime.ExecuteRequest(context)
                    .ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                            return task;

                        // send the message with the data package
                        var message = new ApolloServerDataMessage(subscription.ClientProvidedId, task.Result);
                        return subscription.Client.SendMessage(message);
                    }, cancelSource.Token);

                pipelineExecutions.Add(task);
            }

            await Task.WhenAll(pipelineExecutions);

            // reawait any faulted tasks so tehy can unbuble any exceptions
            foreach (var task in pipelineExecutions.Where(x => x.IsFaulted))
                await task;
        }

        /// <summary>
        /// Adds a new subscription to this server to be monitored.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        public void AddSubscription(ISubscription<TSchema> subscription)
        {
            if (subscription != null && subscription.IsValid)
            {
                this.Subscriptions.Add(subscription);
                this.SubscriptionRegistered?.Invoke(this, new ClientSubscriptionEventArgs<TSchema>(subscription));
            }
            else
            {
                // TODO: Send error if the document failed to parse or the subscription
                //       couldnt otherwise we registered with the supervisor
            }
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
            var client = sender as ApolloClientProxy<TSchema>;
            if (client == null)
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
                    return this.StartNewSubscriptionForClient(client, message as ApolloSubscriptionStartMessage);

                case ApolloMessageType.STOP:
                    return this.StopSubscriptionForClient(client, message as ApolloSubscriptionStopMessage);

                case ApolloMessageType.CONNECTION_TERMINATE:
                    break;

                default:
                    return this.UnknownMessageRecieved(client);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns a generic error to the client indicating that the last message recieved was unknown or invalid.
        /// </summary>
        /// <param name="client">The client to return the error to.</param>
        /// <returns>Task.</returns>
        private Task UnknownMessageRecieved(ApolloClientProxy<TSchema> client)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Attempts to find and remove a subscription with the given client id on the message for the target subscription.
        /// </summary>
        /// <param name="client">The client to search.</param>
        /// <param name="message">The message containing the subscription id to stop.</param>
        /// <returns>Task.</returns>
        private Task StopSubscriptionForClient(ApolloClientProxy<TSchema> client, ApolloSubscriptionStopMessage message)
        {
            if (message?.Id != null)
            {
                var subFound = this.Subscriptions.TryRemoveSubscription(client, message.Id);

                if (subFound != null)
                    this.SubscriptionRemoved?.Invoke(this, new ClientSubscriptionEventArgs<TSchema>(subFound));
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Parses the message contents to generate a valid client subscription and adds it to the watched
        /// set for this instance.
        /// </summary>
        /// <param name="client">The client requesting a subscription.</param>
        /// <param name="message">The message with the subscription details.</param>
        private async Task StartNewSubscriptionForClient(ApolloClientProxy<TSchema> client, ApolloSubscriptionStartMessage message)
        {
            var maker = client.ServiceProvider.GetRequiredService(typeof(IClientSubscriptionMaker<TSchema>)) as IClientSubscriptionMaker<TSchema>;
            var subscription = await maker.Create(client, message.Payload, message.Id);
            this.AddSubscription(subscription);
        }

        /// <summary>
        /// Sends the required startup messages down to the connected client to acknowledge the connection/protocol.
        /// </summary>
        /// <param name="client">The client.</param>
        private async Task AcknowledgeNewConnection(ApolloClientProxy<TSchema> client)
        {
            await client.SendMessage(new ApolloServerAckOperationMessage());
            await client.SendMessage(new ApolloKeepAliveOperationMessage());
        }

        /// <summary>
        /// Gets the collection of subscriptions this supervisor is managing.
        /// </summary>
        /// <value>The subscriptions.</value>
        public ClientSubscriptionCollection<TSchema> Subscriptions { get; }
    }
}