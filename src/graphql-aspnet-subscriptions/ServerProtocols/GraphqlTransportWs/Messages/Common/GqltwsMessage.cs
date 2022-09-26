// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.Common
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// A common base class for all graphql-ws messages.
    /// </summary>
    public abstract class GqltwsMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsMessage"/> class.
        /// </summary>
        public GqltwsMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsMessage" /> class.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        protected GqltwsMessage(GqltwsMessageType messageType)
        {
            this.Type = messageType;
            this.Id = null;
        }

        /// <summary>
        /// Gets or sets the identifier for the scoped operation started by a client.
        /// </summary>
        /// <value>The identifier.</value>
        [JsonPropertyName(GqltwsConstants.Messaging.MESSAGE_ID)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the message, indicating expected payload types.
        /// </summary>
        /// <value>The type.</value>
        [JsonConverter(typeof(GqltwsMessageTypeConverter))]
        [JsonPropertyName(GqltwsConstants.Messaging.MESSAGE_TYPE)]
        public GqltwsMessageType Type { get; set; }

        /// <summary>
        /// Gets the payload of the message as a general object.
        /// </summary>
        /// <value>The payload object.</value>
        [JsonIgnore]
        public abstract object PayloadObject { get; }
    }
}