// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Messaging.Messages
{
    using System;
    using System.Diagnostics;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Interfaces.Messaging;
    using GraphQL.AspNet.Messaging.Messages.Payloads;
    using GraphQL.AspNet.Variables;

    /// <summary>
    /// A partially deserialized operation message recieved from the client. Converts the actual paylaod
    /// into a collection of key/value variables for later parsing.
    /// </summary>
    [DebuggerDisplay("Message Type: {Type}")]
    public class AnonymousClientOperationMessage : ApolloMessage<GraphQueryData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousClientOperationMessage"/> class.
        /// </summary>
        public AnonymousClientOperationMessage()
            : base(ApolloMessageType.UNKNOWN)
        {
        }

        /// <summary>
        /// Converts this instance into its final, payload focused message.
        /// </summary>
        /// <returns>IGraphQLOperationMessage.</returns>
        public IGraphQLOperationMessage Convert()
        {
            switch (this.Type)
            {
                case ApolloMessageType.CONNECTION_INIT:
                    return new ConnectionInitOperationMessage()
                    {
                        Payload = null, // TODO: connection may have params, need to handle it
                    };

                case ApolloMessageType.START:
                    return new StartOperationMessage()
                    {
                        Id = this.Id,
                        Payload = this.Payload,
                    };

                case ApolloMessageType.STOP:
                    return new StopOperationMessage()
                    {
                        Id = this.Id,
                        Payload = null, // stop message has no expected payload
                    };

                case ApolloMessageType.CONNECTION_TERMINATE:
                    return new ConnectionInitOperationMessage()
                    {
                        Id = this.Id,
                        Payload = null, // terminate message has no expected
                    };

                default:
                    return new UnknownOperationMessage()
                    {
                        Id = this.Id,
                    };
            }
        }
    }
}