// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.GraphqlWsLegacy.Messages.ClientMessages
{
    using System.Diagnostics;
    using GraphQL.AspNet.GraphqlWsLegacy.Messages.Common;
    using GraphQL.AspNet.GraphqlWsLegacy.Messages.Payloads;

    /// <summary>
    /// A message recieved from the client after the establishment of the websocket to initialize the graphql
    /// session on the socket.
    /// </summary>
    [DebuggerDisplay("GraphqlWsLegacy Client Initialized")]
    public class GraphqlWsLegacyClientConnectionInitMessage : GraphqlWsLegacyMessage<GraphqlWsLegacyNullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyClientConnectionInitMessage"/> class.
        /// </summary>
        public GraphqlWsLegacyClientConnectionInitMessage()
            : base(GraphqlWsLegacyMessageType.CONNECTION_INIT)
        {
        }
    }
}