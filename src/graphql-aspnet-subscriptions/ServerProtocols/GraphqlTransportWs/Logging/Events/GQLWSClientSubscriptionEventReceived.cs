// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphQLWS.Logging.Events
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Logging.Common;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// Recorded when an graphql-ws client proxy instance receives an event
    /// from its server component and begins processing said event against its subscription list.
    /// </summary>
    /// <typeparam name="TSchema">The type of schema the event is being raised against.</typeparam>
    internal class GqltwsClientSubscriptionEventReceived<TSchema> : GraphLogEntry
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsClientSubscriptionEventReceived{TSchema}" /> class.
        /// </summary>
        /// <param name="client">The client proxy that received the event.</param>
        /// <param name="fieldPath">The field path of the event recieved.</param>
        /// <param name="subscriptionsToReceive">The filtered set of subscriptions for this client
        /// that will receive the event.</param>
        public GqltwsClientSubscriptionEventReceived(
            GqltwsClientProxy<TSchema> client,
            SchemaItemPath fieldPath,
            IReadOnlyList<ISubscription> subscriptionsToReceive)
            : base(GqltwsLogEventIds.ClientSubscriptionEventRecieved)
        {
            this.SchemaTypeName = typeof(TSchema).FriendlyName(true);
            this.SubscriptionRoute = fieldPath?.Path;
            this.SubscriptionCount = subscriptionsToReceive?.Count;
            this.SubscriptionIds = subscriptionsToReceive?.Select(x => x.Id).ToList();
            this.ClientId = client?.Id;
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
        /// Gets the number of subscriptions on this client that set to receive this event.
        /// </summary>
        /// <value>The subscription count.</value>
        public int? SubscriptionCount
        {
            get => this.GetProperty<int?>(GqltwsLogPropertyNames.SUBSCRIPTION_COUNT);
            private set => this.SetProperty(GqltwsLogPropertyNames.SUBSCRIPTION_COUNT, value);
        }

        /// <summary>
        /// Gets the set of client provided ids for the subscriptions that will receive this event.
        /// </summary>
        /// <value>The subscription ids.</value>
        public IList<string> SubscriptionIds
        {
            get => this.GetProperty<IList<string>>(GqltwsLogPropertyNames.SUBSCRIPTION_IDS);
            private set => this.SetProperty(GqltwsLogPropertyNames.SUBSCRIPTION_IDS, value);
        }

        /// <summary>
        /// Gets the unique id of the event that was received.
        /// </summary>
        /// <value>The identifier.</value>
        public string SubscriptionRoute
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.SUBSCRIPTION_ROUTE);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_ROUTE, value);
        }

        /// <summary>
        /// Gets the unique id of the client receieving the event.
        /// </summary>
        /// <value>The identifier.</value>
        public string ClientId
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.SUBSCRIPTION_CLIENT_ID);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_CLIENT_ID, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var clientId = this.ClientId?.Length > 8 ? this.ClientId.Substring(0, 8) : this.ClientId;
            return $"GraphQL-WS Client Event Received | Client: {clientId}, Route: '{this.SubscriptionRoute}', Subscription Count: {this.SubscriptionCount}";
        }
    }
}