﻿// *************************************************************
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
    /// Recorded whenever an client proxy registers a new subscription
    /// and can send data to the connected client when events are raised.
    /// </summary>
    internal class ClientProxySubscriptionCreatedLogEntry : GraphLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientProxySubscriptionCreatedLogEntry" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="subscription">The subscription that was created.</param>
        public ClientProxySubscriptionCreatedLogEntry(ISubscriptionClientProxy client, ISubscription subscription)
            : base(SubscriptionLogEventIds.ClientSubscriptionStarted)
        {
            this.ClientId = client?.Id;
            this.SubscriptionId = subscription?.Id;
            this.SubscriptionPath = subscription?.Route?.Path;
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
        /// Gets the path to the top level subscription field in the target schema.
        /// </summary>
        /// <value>The subscription path.</value>
        public string SubscriptionPath
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.SUBSCRIPTION_PATH);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_PATH, value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var idTruncated = this.ClientId?.Length > 8 ? this.ClientId.Substring(0, 8) : this.ClientId;
            return $"Client Subscription Started | Client Id: {idTruncated}, Sub Id: {this.SubscriptionId}, Field: {this.SubscriptionPath}";
        }
    }
}