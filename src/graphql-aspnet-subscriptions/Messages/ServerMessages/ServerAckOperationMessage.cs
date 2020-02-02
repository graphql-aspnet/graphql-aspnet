// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Messaging.ServerMessages
{
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Messaging.Messages.Payloads;

    /// <summary>
    /// A message sent by the server to a client to acknowledge receipt of a message when no other
    /// specific message is warranted.
    /// </summary>
    public class ServerAckOperationMessage : GraphQLOperationMessage<NullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerAckOperationMessage"/> class.
        /// </summary>
        public ServerAckOperationMessage()
            : base(GraphQLOperationMessageType.CONNECTION_ACK)
        {
        }
    }
}