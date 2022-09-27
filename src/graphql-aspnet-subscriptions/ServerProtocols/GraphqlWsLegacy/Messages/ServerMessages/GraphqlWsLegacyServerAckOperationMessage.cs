// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.GraphqlWsLegacy.Messages.ServerMessages
{
    using GraphQL.AspNet.GraphqlWsLegacy.Messages.Common;
    using GraphQL.AspNet.GraphqlWsLegacy.Messages.Payloads;

    /// <summary>
    /// A message sent by the server to a client to acknowledge receipt of a message when no other
    /// specific message is warranted.
    /// </summary>
    public class GraphqlWsLegacyServerAckOperationMessage : GraphqlWsLegacyMessage<GraphqlWsLegacyNullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyServerAckOperationMessage"/> class.
        /// </summary>
        public GraphqlWsLegacyServerAckOperationMessage()
            : base(GraphqlWsLegacyMessageType.CONNECTION_ACK)
        {
        }
    }
}