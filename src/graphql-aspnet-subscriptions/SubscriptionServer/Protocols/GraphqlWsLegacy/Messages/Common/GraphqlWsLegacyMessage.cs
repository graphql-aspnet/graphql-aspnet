// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy.Messages.Common
{
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// A common base class for all GraphqlWsLegacy messages.
    /// </summary>
    public abstract class GraphqlWsLegacyMessage : ILoggableClientProxyMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyMessage"/> class.
        /// </summary>
        public GraphqlWsLegacyMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyMessage" /> class.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        protected GraphqlWsLegacyMessage(GraphqlWsLegacyMessageType messageType)
        {
            this.Type = messageType;
            this.Id = null;
        }

        /// <summary>
        /// Gets or sets the identifier for the scoped operation started by a client.
        /// </summary>
        /// <value>The identifier.</value>
        [JsonPropertyName(GraphqlWsLegacyConstants.Messaging.MESSAGE_ID)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the message, indicating expected payload types.
        /// </summary>
        /// <value>The type.</value>
        [JsonConverter(typeof(GraphqlWsLegacyMessageTypeConverter))]
        [JsonPropertyName(GraphqlWsLegacyConstants.Messaging.MESSAGE_TYPE)]
        public GraphqlWsLegacyMessageType Type { get; set; }

        /// <summary>
        /// Gets the payload of the message as a general object.
        /// </summary>
        /// <value>The payload object.</value>
        [JsonIgnore]
        public abstract object PayloadObject { get; }

        /// <inheritdoc />
        [JsonIgnore]
        string ILoggableClientProxyMessage.Type => this.Type.ToString();
    }
}