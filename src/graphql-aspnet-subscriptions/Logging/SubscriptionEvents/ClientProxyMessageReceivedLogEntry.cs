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
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging;

    /// <summary>
    /// Recorded when an client proxy receives a new message from its connected client.
    /// </summary>
    public class ClientProxyMessageReceivedLogEntry : GraphLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientProxyMessageReceivedLogEntry"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="message">The message.</param>
        public ClientProxyMessageReceivedLogEntry(ISubscriptionClientProxy client, ILoggableClientProxyMessage message)
            : base(SubscriptionLogEventIds.ClientMessageReceived)
        {
            this.ClientId = client?.Id.ToString();
            this.MessageType = message?.Type;
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
        /// Gets the type of the message that was received.
        /// </summary>
        /// <value>The type of the message.</value>
        public string MessageType
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.MESSAGE_TYPE);
            private set => this.SetProperty(SubscriptionLogPropertyNames.MESSAGE_TYPE, value);
        }

        /// <summary>
        /// Gets the id that was supplied by the client with the message, if any.
        /// </summary>
        /// <value>The message identifier.</value>
        public string MessageId
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.MESSAGE_ID);
            private set => this.SetProperty(SubscriptionLogPropertyNames.MESSAGE_ID, value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var idTruncated = this.ClientId?.Length > 8 ? this.ClientId.Substring(0, 8) : this.ClientId;
            if (this.MessageId == null)
                return $"Client Message Received | Client Id: {idTruncated} (Type: '{this.MessageType}')";
            else
                return $"Client Message Received | Client Id: {idTruncated}, Message Id: {this.MessageId} (Type: '{this.MessageType}')";
        }
    }
}