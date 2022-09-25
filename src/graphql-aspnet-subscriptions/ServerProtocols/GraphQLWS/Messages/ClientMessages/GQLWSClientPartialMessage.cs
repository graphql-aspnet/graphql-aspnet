// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.ClientMessages
{
    using System.Diagnostics;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Common;

    /// <summary>
    /// A partially deserialized operation message recieved from the client. Converts the actual paylaod
    /// into a collection of key/value variables for later parsing. Used as an intermediary to prevent
    /// double deserialization of a message via json deserializer.
    /// </summary>
    [DebuggerDisplay("Message Type: {Type}")]
    internal class GQLWSClientPartialMessage : GQLWSMessage<GraphQueryData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GQLWSClientPartialMessage"/> class.
        /// </summary>
        public GQLWSClientPartialMessage()
            : base(GQLWSMessageType.UNKNOWN)
        {
        }

        /// <summary>
        /// Converts this instance into its final, payload focused message.
        /// </summary>
        /// <returns>IGraphQLOperationMessage.</returns>
        public GQLWSMessage Convert()
        {
            switch (this.Type)
            {
                case GQLWSMessageType.CONNECTION_INIT:
                    return new GQLWSClientConnectionInitMessage()
                    {
                        Payload = null, // TODO: connection may have params, need to handle it
                    };

                case GQLWSMessageType.START:
                    return new GQLWSClientStartMessage()
                    {
                        Id = this.Id,
                        Payload = this.Payload,
                    };

                case GQLWSMessageType.STOP:
                    return new GQLWSClientStopMessage()
                    {
                        Id = this.Id,
                        Payload = null, // stop message has no expected payload
                    };

                case GQLWSMessageType.CONNECTION_TERMINATE:
                    return new GQLWSClientConnectionTerminateMessage()
                    {
                        Id = this.Id,
                        Payload = null, // terminate message has no expected
                    };

                default:
                    return new GQLWSUnknownMessage()
                    {
                        Id = this.Id,
                    };
            }
        }
    }
}