// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.ClientMessages
{
    using System.Diagnostics;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.Common;

    /// <summary>
    /// A partially deserialized operation message recieved from the client. Converts the actual paylaod
    /// into a collection of key/value variables for later parsing. Used as an intermediary to prevent
    /// double deserialization of a message via json deserializer.
    /// </summary>
    [DebuggerDisplay("Message Type: {Type}")]
    internal class GraphqlWsLegacyClientPartialMessage : GraphqlWsLegacyMessage<GraphQueryData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyClientPartialMessage"/> class.
        /// </summary>
        public GraphqlWsLegacyClientPartialMessage()
            : base(GraphqlWsLegacyMessageType.UNKNOWN)
        {
        }

        /// <summary>
        /// Converts this instance into its final, payload focused message.
        /// </summary>
        /// <returns>IGraphQLOperationMessage.</returns>
        public GraphqlWsLegacyMessage Convert()
        {
            switch (this.Type)
            {
                case GraphqlWsLegacyMessageType.CONNECTION_INIT:
                    return new GraphqlWsLegacyClientConnectionInitMessage()
                    {
                        Payload = null, // TODO: connection may have params, need to handle it
                    };

                case GraphqlWsLegacyMessageType.START:
                    return new GraphqlWsLegacyClientStartMessage()
                    {
                        Id = this.Id,
                        Payload = this.Payload,
                    };

                case GraphqlWsLegacyMessageType.STOP:
                    return new GraphqlWsLegacyClientStopMessage()
                    {
                        Id = this.Id,
                        Payload = null, // stop message has no expected payload
                    };

                case GraphqlWsLegacyMessageType.CONNECTION_TERMINATE:
                    return new GraphqlWsLegacyClientConnectionTerminateMessage()
                    {
                        Id = this.Id,
                        Payload = null, // terminate message has no expected
                    };

                default:
                    return new GraphqlWsLegacyUnknownMessage()
                    {
                        Id = this.Id,
                    };
            }
        }
    }
}