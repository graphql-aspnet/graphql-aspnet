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
    using System;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Payloads;

    /// <summary>
    /// An implementation of the required operation message interface.
    /// </summary>
    /// <typeparam name="TPayloadType">The type of the payload this message expects.</typeparam>
    public abstract class GQLWSMessage<TPayloadType> : GQLWSMessage
        where TPayloadType : class
    {
        private TPayloadType _payload;

        /// <summary>
        /// Initializes a new instance of the <see cref="GQLWSMessage{TPayload}"/> class.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        protected GQLWSMessage(GQLWSMessageType messageType)
            : base(messageType)
        {
        }

        /// <summary>
        /// Gets or sets the payload of the message.
        /// </summary>
        /// <value>The payload.</value>
        [JsonPropertyName(GQLWSConstants.Messaging.MESSAGE_PAYLOAD)]
        public TPayloadType Payload
        {
            get
            {
                if (typeof(TPayloadType) == typeof(GQLWSNullPayload))
                    return null;

                return _payload;
            }

            set
            {
                _payload = value;
            }
        }

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
        public override object PayloadObject => this.Payload;
    }
}