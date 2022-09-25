// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Common
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// A common base class for all graphql-ws messages.
    /// </summary>
    public abstract class GQLWSMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GQLWSMessage"/> class.
        /// </summary>
        public GQLWSMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GQLWSMessage" /> class.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        protected GQLWSMessage(GQLWSMessageType messageType)
        {
            this.Type = messageType;
            this.Id = null;
        }

        /// <summary>
        /// Gets or sets the identifier for the scoped operation started by a client.
        /// </summary>
        /// <value>The identifier.</value>
        [JsonPropertyName(GQLWSConstants.Messaging.MESSAGE_ID)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the message, indicating expected payload types.
        /// </summary>
        /// <value>The type.</value>
        [JsonConverter(typeof(GQLWSMessageTypeConverter))]
        [JsonPropertyName(GQLWSConstants.Messaging.MESSAGE_TYPE)]
        public GQLWSMessageType Type { get; set; }

        /// <summary>
        /// Gets the payload of the message as a general object.
        /// </summary>
        /// <value>The payload object.</value>
        [JsonIgnore]
        public abstract object PayloadObject { get; }
    }
}