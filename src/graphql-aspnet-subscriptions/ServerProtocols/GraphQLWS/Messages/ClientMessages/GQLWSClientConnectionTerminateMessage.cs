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
    /// A message recieved from the client when it is notfiying the server that its dropping
    /// the connection and the server should dispose of it.
    /// </summary>
    [DebuggerDisplay("graphql-ws: Client Terminate")]
    internal class GQLWSClientConnectionTerminateMessage : GQLWSMessage<GQLWSNullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GQLWSClientConnectionTerminateMessage"/> class.
        /// </summary>
        public GQLWSClientConnectionTerminateMessage()
            : base(GQLWSMessageType.CONNECTION_TERMINATE)
        {
        }
    }
}