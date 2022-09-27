// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.ClientProxyLogEntries
{
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Logging.Common;

    /// <summary>
    /// Recorded whenever an client proxy drops a subscription registration.
    /// </summary>
    internal class ClientProxySubscriptionStoppedLogEntry : GraphLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientProxySubscriptionStoppedLogEntry" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="subscription">The subscription that was created.</param>
        public ClientProxySubscriptionStoppedLogEntry(ISubscriptionClientProxy client, ISubscription subscription)
            : base(SubscriptionLogEventIds.ClientSubscriptionStopped)
        {
            this.ClientId = client?.Id;
            this.SubscriptionId = subscription?.Id;
            this.Route = subscription?.Route?.Path;
        }

        /// <summary>
        /// Gets the unique id of the client that was created.
        /// </summary>
        /// <value>The identifier.</value>
        public string ClientId
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.SUBSCRIPTION_CLIENT_ID);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_CLIENT_ID, value);
        }

        /// <summary>
        /// Gets the client supplied id for the subscription.
        /// </summary>
        /// <value>The subscription identifier.</value>
        public string SubscriptionId
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.SUBSCRIPTION_ID);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_ID, value);
        }

        /// <summary>
        /// Gets the id that was supplied by the client with the GraphqlWsLegacy message, if any.
        /// </summary>
        /// <value>The message identifier.</value>
        public string Route
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.SUBSCRIPTION_ROUTE);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_ROUTE, value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var idTruncated = this.ClientId?.Length > 8 ? this.ClientId.Substring(0, 8) : this.ClientId;
            return $"Client Subscription Stopped | Client Id: {idTruncated}, Sub Id: {this.SubscriptionId}, Field: {this.Route}";
        }
    }
}