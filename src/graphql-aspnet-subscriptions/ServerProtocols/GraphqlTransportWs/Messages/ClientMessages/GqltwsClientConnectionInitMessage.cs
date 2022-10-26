// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.ClientMessages
{
    using System.Diagnostics;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.Common;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.Payloads;

    /// <summary>
    /// A message recieved from the client after the establishment of the websocket to initialize the graphql
    /// session on the socket.
    /// </summary>
    [DebuggerDisplay("graphql-ws: Client Initialized")]
    public class GqltwsClientConnectionInitMessage : GqltwsMessage<GqltwsNullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsClientConnectionInitMessage"/> class.
        /// </summary>
        public GqltwsClientConnectionInitMessage()
            : base(GqltwsMessageType.CONNECTION_INIT)
        {
        }
    }
}