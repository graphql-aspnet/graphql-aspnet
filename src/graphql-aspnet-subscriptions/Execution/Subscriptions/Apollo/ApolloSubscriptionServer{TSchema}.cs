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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages.ClientMessages;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages.Common;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages.ServerMessages;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A baseline component acts to centralize the subscription server operations, regardless of if
    /// this server is in-process with the primary graphql runtime or out-of-process on a seperate instance.
    /// </summary>
    /// <typeparam name="TSchema">The schema type this server is registered to handle.</typeparam>
    public class ApolloSubscriptionServer<TSchema> : ISubscriptionServer<TSchema>
        where TSchema : class, ISchema
    {
        private readonly ISubscriptionEventListener<TSchema> _listener;
        private readonly HashSet<ApolloClientProxy<TSchema>> _clients;

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
        /// <param name="listener">The listener watching for new events that need to be communicated
        /// to clients managed by this server.</param>
        public ApolloSubscriptionServer(ISubscriptionEventListener<TSchema> listener)
        {
            _listener = Validation.ThrowIfNullOrReturn(listener, nameof(listener));
            _clients = new HashSet<ApolloClientProxy<TSchema>>();
            this.Subscriptions = new ClientSubscriptionCollection<TSchema>();

            _listener.NewSubscriptionEvent += this.Listener_HandleNewSubscriptionEvent;
        }

        /// <summary>
        /// Handles the new subscription event raised by the listener, dispatching it to any events
        /// as appropriate.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="SubscriptionEventEventArgs"/> instance containing the event data.</param>
        private void Listener_HandleNewSubscriptionEvent(object sender, SubscriptionEventEventArgs args)
        {
        }

        /// <summary>
        /// Register a newly connected subscription with the server so that it can start sending messages.
        /// </summary>
        /// <param name="client">The client.</param>
        public void RegisterNewClient(ISubscriptionClientProxy client)
        {
            Validation.ThrowIfNull(client, nameof(client));
            Validation.ThrowIfNotCastable<ApolloClientProxy<TSchema>>(client.GetType(), nameof(client));

            var apolloClient = client as ApolloClientProxy<TSchema>;
            _clients.Add(apolloClient);

            apolloClient.ConnectionOpening += this.ApolloClient_ConnectionOpening;
            apolloClient.ConnectionClosed += this.ApolloClient_ConnectionClosed;
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

            if (subscription.IsValid)
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