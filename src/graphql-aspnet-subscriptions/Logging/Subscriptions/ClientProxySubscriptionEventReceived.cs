// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.Subscriptions
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// Recorded when an client proxy instance receives an event
    /// from its server component and begins processing said event against its subscription list.
    /// </summary>
    /// <typeparam name="TSchema">The type of schema the event is being raised against.</typeparam>
    public class ClientProxySubscriptionEventReceived<TSchema> : GraphLogEntry
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientProxySubscriptionEventReceived{TSchema}" /> class.
        /// </summary>
        /// <param name="client">The client proxy that received the event.</param>
        /// <param name="fieldPath">The field path of the event recieved.</param>
        /// <param name="subscriptionsToReceive">The filtered set of subscriptions for this client
        /// that will receive the event.</param>
        public ClientProxySubscriptionEventReceived(
            ISubscriptionClientProxy client,
            SchemaItemPath fieldPath,
            IReadOnlyList<ISubscription> subscriptionsToReceive)
            : base(SubscriptionLogEventIds.ClientSubscriptionEventRecieved)
        {
            this.SchemaTypeName = typeof(TSchema).FriendlyName(true);
            this.SubscriptionPath = fieldPath?.Path;
            this.SubscriptionCount = subscriptionsToReceive?.Count;
            this.SubscriptionIds = subscriptionsToReceive?.Select(x => x.Id).ToList();
            this.ClientId = client?.Id.ToString();
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
            get => this.GetProperty<int?>(SubscriptionLogPropertyNames.SUBSCRIPTION_COUNT);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_COUNT, value);
        }

        /// <summary>
        /// Gets the set of client provided ids for the subscriptions that will receive this event.
        /// </summary>
        /// <value>The subscription ids.</value>
        public IList<string> SubscriptionIds
        {
            get => this.GetProperty<IList<string>>(SubscriptionLogPropertyNames.SUBSCRIPTION_IDS);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_IDS, value);
        }

        /// <summary>
        /// Gets the path to the top level subscription field in the target schema.
        /// </summary>
        /// <value>The subscription path.</value>
        public string SubscriptionPath
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.SUBSCRIPTION_PATH);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_PATH, value);
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

        /// <inheritdoc />
        public override string ToString()
        {
            var clientId = this.ClientId?.Length > 8 ? this.ClientId.Substring(0, 8) : this.ClientId;
            return $"Client Event Received | Client: {clientId}, Route: '{this.SubscriptionPath}', Subscription Count: {this.SubscriptionCount}";
        }
    }
}