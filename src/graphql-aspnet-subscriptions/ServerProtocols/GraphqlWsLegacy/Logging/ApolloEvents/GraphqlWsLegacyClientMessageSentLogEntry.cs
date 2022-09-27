// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.GraphqlWsLegacy.Logging.GraphqlWsLegacyEvents
{
    using GraphQL.AspNet.GraphqlWsLegacy.Messages;
    using GraphQL.AspNet.GraphqlWsLegacy.Messages.Common;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Logging.Common;

    /// <summary>
    /// Recorded when an GraphqlWsLegacy client proxy sends a message down to its connected client.
    /// </summary>
    public class GraphqlWsLegacyClientMessageSentLogEntry : GraphLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyClientMessageSentLogEntry"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="message">The message.</param>
        public GraphqlWsLegacyClientMessageSentLogEntry(ISubscriptionClientProxy client, GraphqlWsLegacyMessage message)
            : base(GraphqlWsLegacyLogEventIds.ClientMessageSent)
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
        /// Gets the <see cref="GraphqlWsLegacyMessageType"/> of the message that was received.
        /// </summary>
        /// <value>The type of the message.</value>
        public string MessageType
        {
            get => this.GetProperty<string>(GraphqlWsLegacyLogPropertyNames.MESSAGE_TYPE);
            private set => this.SetProperty(GraphqlWsLegacyLogPropertyNames.MESSAGE_TYPE, value);
        }

        /// <summary>
        /// Gets the id that was supplied by the client with the GraphqlWsLegacy message, if any.
        /// </summary>
        /// <value>The message identifier.</value>
        public string MessageId
        {
            get => this.GetProperty<string>(GraphqlWsLegacyLogPropertyNames.MESSAGE_ID);
            private set => this.SetProperty(GraphqlWsLegacyLogPropertyNames.MESSAGE_ID, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var idTruncated = this.ClientId?.Length > 8 ? this.ClientId.Substring(0, 8) : this.ClientId;
            if (this.MessageId == null)
                return $"GraphqlWsLegacy Message Sent | Client Id: {idTruncated} (Type: '{this.MessageType}')";
            else
                return $"GraphqlWsLegacy Message Sent | Client Id: {idTruncated}, Message Id: {this.MessageId} (Type: '{this.MessageType}')";
        }
    }
}