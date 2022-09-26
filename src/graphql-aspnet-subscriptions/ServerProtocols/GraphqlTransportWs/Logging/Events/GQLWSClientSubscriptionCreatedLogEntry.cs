﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphQLWS.Logging.Events
{
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Logging.Common;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Logging;

    /// <summary>
    /// Recorded whenever an graphql-ws client proxy registers a new subscription
    /// and can send data to the connected client when events are raised.
    /// </summary>
    internal class GqltwsClientSubscriptionCreatedLogEntry : GraphLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsClientSubscriptionCreatedLogEntry" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="subscription">The subscription that was created.</param>
        public GqltwsClientSubscriptionCreatedLogEntry(ISubscriptionClientProxy client, ISubscription subscription)
            : base(GqltwsLogEventIds.ClientSubscriptionStarted)
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
            get => this.GetProperty<string>(GqltwsLogPropertyNames.SUBSCRIPTION_ID);
            private set => this.SetProperty(GqltwsLogPropertyNames.SUBSCRIPTION_ID, value);
        }

        /// <summary>
        /// Gets the id that was supplied by the client with the graphql-ws message, if any.
        /// </summary>
        /// <value>The message identifier.</value>
        public string Route
        {
            get => this.GetProperty<string>(GqltwsLogPropertyNames.SUBSCRIPTION_ROUTE);
            private set => this.SetProperty(GqltwsLogPropertyNames.SUBSCRIPTION_ROUTE, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var idTruncated = this.ClientId?.Length > 8 ? this.ClientId.Substring(0, 8) : this.ClientId;
            return $"Subscription Started | Client Id: {idTruncated}, Sub Id: {this.SubscriptionId}, Field: {this.Route}";
        }
    }
}