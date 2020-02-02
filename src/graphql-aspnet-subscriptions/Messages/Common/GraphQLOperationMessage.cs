// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Messaging
{
    using System;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Interfaces.Messaging;

    /// <summary>
    /// An implementation of the required operation message interface.
    /// </summary>
    /// <typeparam name="TPayloadType">The type of the payload this message expects.</typeparam>
    public abstract class GraphQLOperationMessage<TPayloadType> : IGraphQLOperationMessage<TPayloadType>
        where TPayloadType : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLOperationMessage{TPayload}"/> class.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        protected GraphQLOperationMessage(GraphQLOperationMessageType messageType)
        {
            this.Type = messageType;
            this.Payload = null;
            this.Id = null;
        }

        /// <summary>
        /// Gets or sets the payload of the message.
        /// </summary>
        /// <value>The payload.</value>
        public TPayloadType Payload { get; set; }

        /// <summary>
        /// Gets or sets the identifier for the scoped operation started by a client.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the message, indicating expected payload types.
        /// </summary>
        /// <value>The type.</value>
        [JsonConverter(typeof(GraphQLOperationMessageTypeConverter))]
        public GraphQLOperationMessageType Type { get; set; }

        /// <summary>
        /// Gets the type of the payload handled by this message.
        /// </summary>
        /// <value>The type of the payload.</value>
        [JsonIgnore]
        public Type PayloadType => typeof(TPayloadType);

        /// <summary>
        /// Gets the payload of the message as a general object.
        /// </summary>
        /// <value>The payload object.</value>
        [JsonIgnore]
        public object PayloadObject => this.Payload;
    }
}