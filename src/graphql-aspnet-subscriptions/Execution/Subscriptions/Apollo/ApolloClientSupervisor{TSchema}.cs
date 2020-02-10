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
    using System.IO;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages.ClientMessages;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages.Common;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages.ServerMessages;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// An intermediary between an apollo client and the apollo server instance. This object
    /// acts as a liason to hold client connections, respond to some house-keeping events and filter
    /// data level events to ensure proper routing to any known clients.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this supervisor's clients are built for.</typeparam>
    public class ApolloClientSupervisor<TSchema>
        where TSchema : class, ISchema
    {
        private readonly HashSet<ApolloClientProxy<TSchema>> _clients;

        // a collection of subscriptions this supervisor is watching for
        // keyed on teh full field path within the target schema (not against the alternate, short event name)
        private readonly Dictionary<string, ConcurrentHashSet<ClientSubscription<TSchema>>> _activeSubscriptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloClientSupervisor{TSchema}"/> class.
        /// </summary>
        public ApolloClientSupervisor()
        {
            _clients = new HashSet<ApolloClientProxy<TSchema>>();
            _activeSubscriptions = new Dictionary<string, ConcurrentHashSet<ClientSubscription<TSchema>>>();
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
        /// Retrieves the set of subscriptions.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <returns>IEnumerable&lt;ClientSubscription&lt;TSchema&gt;&gt;.</returns>
        public IEnumerable<ClientSubscription<TSchema>> RetrieveSubscriptions(string eventName)
        {
            if (!_activeSubscriptions.ContainsKey(eventName))
                return new List<ClientSubscription<TSchema>>();

            return _activeSubscriptions[eventName];
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
                    break;
            }

            return Task.CompletedTask;
        }

        private Task StopSubscriptionForClient(ApolloClientProxy<TSchema> client, ApolloSubscriptionStopMessage message)
        {
            throw new NotImplementedException();
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
                if (!_activeSubscriptions.ContainsKey(subscription.Route.Path))
                {
                    lock (_activeSubscriptions)
                    {
                        if (!_activeSubscriptions.ContainsKey(subscription.Route.Path))
                            _activeSubscriptions.Add(subscription.Route.Path, new ConcurrentHashSet<ClientSubscription<TSchema>>());
                    }
                }

                _activeSubscriptions[subscription.Route.Path].Add(subscription);
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
    }
}