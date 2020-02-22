// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Apollo.Messages.ClientMessages
{
    using System.Diagnostics;
    using GraphQL.AspNet.Apollo.Messages.Common;

    /// <summary>
    /// A partially deserialized operation message recieved from the client. Converts the actual paylaod
    /// into a collection of key/value variables for later parsing. Used as an intermediary to prevent
    /// double deserialization of a message via json deserializer.
    /// </summary>
    [DebuggerDisplay("Message Type: {Type}")]
    internal class ApolloPartialClientMessage : ApolloMessage<GraphQueryData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloPartialClientMessage"/> class.
        /// </summary>
        public ApolloPartialClientMessage()
            : base(ApolloMessageType.UNKNOWN)
        {
        }

        /// <summary>
        /// Converts this instance into its final, payload focused message.
        /// </summary>
        /// <returns>IGraphQLOperationMessage.</returns>
        public ApolloMessage Convert()
        {
            switch (this.Type)
            {
                case ApolloMessageType.CONNECTION_INIT:
                    return new ApolloConnectionInitMessage()
                    {
                        Payload = null, // TODO: connection may have params, need to handle it
                    };

                case ApolloMessageType.START:
                    return new ApolloSubscriptionStartMessage()
                    {
                        Id = this.Id,
                        Payload = this.Payload,
                    };

                case ApolloMessageType.STOP:
                    return new ApolloSubscriptionStopMessage()
                    {
                        Id = this.Id,
                        Payload = null, // stop message has no expected payload
                    };

                case ApolloMessageType.CONNECTION_TERMINATE:
                    return new ApolloConnectionInitMessage()
                    {
                        Id = this.Id,
                        Payload = null, // terminate message has no expected
                    };

                default:
                    return new ApolloUnknownMessage()
                    {
                        Id = this.Id,
                    };
            }
        }
    }
}