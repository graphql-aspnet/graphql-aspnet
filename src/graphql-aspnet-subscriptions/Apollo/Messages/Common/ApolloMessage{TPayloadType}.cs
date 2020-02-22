// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Apollo.Messages.Common
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// An implementation of the required operation message interface.
    /// </summary>
    /// <typeparam name="TPayloadType">The type of the payload this message expects.</typeparam>
    public abstract class ApolloMessage<TPayloadType> : ApolloMessage
        where TPayloadType : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloMessage{TPayload}"/> class.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        protected ApolloMessage(ApolloMessageType messageType)
            : base(messageType)
        {
        }

        /// <summary>
        /// Gets or sets the payload of the message.
        /// </summary>
        /// <value>The payload.</value>
        public TPayloadType Payload { get; set; }

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