// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Apollo.Logging.ApolloEvents
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Logging.Common;

    /// <summary>
    /// Recorded when an Apollo Subscription Server instance receives an event
    /// from the router configured for this ASP.NET server instance.
    /// </summary>
    /// <typeparam name="TSchema">The type of schema the event is being raised against.</typeparam>
    public class ApolloServerSubscriptionEventReceived<TSchema> : GraphLogEntry
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloServerSubscriptionEventReceived{TSchema}" /> class.
        /// </summary>
        /// <param name="server">The server instance that received the event.</param>
        /// <param name="eventRecieved">The event that was recieved from the global listener.</param>
        /// <param name="clientsToReceive">The filtered list of clients that will receive the event
        /// from the server.</param>
        public ApolloServerSubscriptionEventReceived(
            ApolloSubscriptionServer<TSchema> server,
            SubscriptionEvent eventRecieved,
            IReadOnlyList<ApolloClientProxy<TSchema>> clientsToReceive)
            : base(ApolloLogEventIds.ServerSubcriptionEventReceived)
        {
            this.SchemaTypeName = eventRecieved.SchemaTypeName;
            this.SubscriptionEventName = eventRecieved.EventName;
            this.SubscriptionEventId = eventRecieved.Id;
            this.ClientCount = clientsToReceive.Count;
            this.ServerId = server.Id;
            this.ClientIds = clientsToReceive.Select(x => x.Id).ToList();
        }

        /// <summary>
        /// Gets the qualified name of the schema this log entry is reported against.
        /// </summary>
        /// <value>The name of the schema type.</value>
        public string SchemaTypeName
        {
            get => this.GetProperty<string>(LogPropertyNames.SCHEMA_TYPE_NAME);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_TYPE_NAME, value);
        }

        /// <summary>
        /// Gets the name of the event that was included in this log entry.
        /// </summary>
        /// <value>The name of the event.</value>
        public string SubscriptionEventName
        {
            get => this.GetProperty<string>(ApolloLogPropertyNames.SUBSCRIPTION_EVENT_NAME);
            private set => this.SetProperty(ApolloLogPropertyNames.SUBSCRIPTION_EVENT_NAME, value);
        }

        /// <summary>
        /// Gets the number of clients set to receive this event when it is dispatched from the
        /// Apollo server component.
        /// </summary>
        /// <value>The name of the schema type.</value>
        public int ClientCount
        {
            get => this.GetProperty<int>(ApolloLogPropertyNames.CLIENT_COUNT);
            private set => this.SetProperty(ApolloLogPropertyNames.CLIENT_COUNT, value);
        }

        /// <summary>
        /// Gets the set of ids of the clients that will receive this event when it is dispatched from the
        /// Apollo server component.
        /// </summary>
        /// <value>The name of the schema type.</value>
        public IList<string> ClientIds
        {
            get => this.GetProperty<IList<string>>(ApolloLogPropertyNames.CLIENT_IDS);
            private set => this.SetProperty(ApolloLogPropertyNames.CLIENT_IDS, value);
        }

        /// <summary>
        /// Gets the unique id of the event that was received.
        /// </summary>
        /// <value>The identifier.</value>
        public string SubscriptionEventId
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.SUBSCRIPTION_EVENT_ID);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_EVENT_ID, value);
        }

        /// <summary>
        /// Gets the unique id of the client that was created.
        /// </summary>
        /// <value>The identifier.</value>
        public string ServerId
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.SUBSCRIPTION_SERVER_ID);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_SERVER_ID, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var idTruncated = this.SubscriptionEventId?.Length > 8 ? this.SubscriptionEventId.Substring(0, 8) : this.SubscriptionEventId;
            var serverId = this.ServerId?.Length > 8 ? this.ServerId.Substring(0, 8) : this.ServerId;
            return $"Apollo Server Event Received | Server: {serverId}, EventName: '{this.SubscriptionEventName}' (Id: {idTruncated}), Subscribed Clients: {this.ClientCount}";
        }
    }
}