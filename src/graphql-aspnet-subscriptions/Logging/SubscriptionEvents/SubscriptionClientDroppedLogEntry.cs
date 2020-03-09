// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.SubscriptionEvents
{
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging.Common;

    /// <summary>
    /// Recorded when a subscription client is no longer connected or otherwise dropped
    /// by ASP.NET. The server will process no more messages from the client.
    /// </summary>
    public class SubscriptionClientDroppedLogEntry : GraphLogEntry
    {
        private readonly string _clientTypeShortName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionClientDroppedLogEntry"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public SubscriptionClientDroppedLogEntry(ISubscriptionClientProxy client)
            : base(SubscriptionLogEventIds.SubscriptionClientDropped)
        {
            _clientTypeShortName = client.GetType().FriendlyName();
            this.ClientTypeName = client.GetType().FriendlyName(true);
            this.ClientId = client.Id;
        }

        /// <summary>
        /// Gets the <see cref="Type"/> name of the subscription client that was registered.
        /// </summary>
        /// <value>The name of the client type.</value>
        public string ClientTypeName
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.SUBSCRIPTION_CLIENT_TYPE_NAME);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_CLIENT_TYPE_NAME, value);
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
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var idTruncated = this.ClientId?.Length > 8 ? this.ClientId.Substring(0, 8) : this.ClientId;
            return $"Client Dropped | Type: '{_clientTypeShortName}' (Id: {idTruncated})";
        }
    }
}