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
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Payloads;

    /// <summary>
    /// A message recieved from the client after the establishment of the websocket to initialize the graphql
    /// session on the socket.
    /// </summary>
    [DebuggerDisplay("graphql-ws: Client Initialized")]
    public class GQLWSClientConnectionInitMessage : GQLWSMessage<GQLWSNullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GQLWSClientConnectionInitMessage"/> class.
        /// </summary>
        public GQLWSClientConnectionInitMessage()
            : base(GQLWSMessageType.CONNECTION_INIT)
        {
        }
    }
}