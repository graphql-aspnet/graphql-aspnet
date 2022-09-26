// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Logging.Events
{
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Logging.Common;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.Common;

    /// <summary>
    /// Recorded when an graphql-ws client proxy sends a message down to its connected client.
    /// </summary>
    internal class GqltwsClientMessageSentLogEntry : GraphLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsClientMessageSentLogEntry"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="message">The message.</param>
        public GqltwsClientMessageSentLogEntry(ISubscriptionClientProxy client, GqltwsMessage message)
            : base(GqltwsLogEventIds.ClientMessageSent)
        {
            this.ClientId = client?.Id;
            this.MessageType = message?.Type.ToString();
            this.MessageId = message?.Id;
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
        /// Gets the <see cref="GqltwsMessageType"/> of the message that was received.
        /// </summary>
        /// <value>The type of the message.</value>
        public string MessageType
        {
            get => this.GetProperty<string>(GqltwsLogPropertyNames.MESSAGE_TYPE);
            private set => this.SetProperty(GqltwsLogPropertyNames.MESSAGE_TYPE, value);
        }

        /// <summary>
        /// Gets the id that was supplied by the client with the graphql-ws message, if any.
        /// </summary>
        /// <value>The message identifier.</value>
        public string MessageId
        {
            get => this.GetProperty<string>(GqltwsLogPropertyNames.MESSAGE_ID);
            private set => this.SetProperty(GqltwsLogPropertyNames.MESSAGE_ID, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var idTruncated = this.ClientId?.Length > 8 ? this.ClientId.Substring(0, 8) : this.ClientId;
            if (this.MessageId == null)
                return $"GraphQL-WS Message Sent | Client Id: {idTruncated} (Type: '{this.MessageType}')";
            else
                return $"GraphQL-WS Message Sent | Client Id: {idTruncated}, Message Id: {this.MessageId} (Type: '{this.MessageType}')";
        }
    }
}