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
    /// A message recieved from the client when it is notfiying the server that its dropping
    /// the connection and the server should dispose of it.
    /// </summary>
    [DebuggerDisplay("GraphqlWsLegacy Client Terminate")]
    internal class GraphqlWsLegacyClientConnectionTerminateMessage : GraphqlWsLegacyMessage<GraphqlWsLegacyNullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyClientConnectionTerminateMessage"/> class.
        /// </summary>
        public GraphqlWsLegacyClientConnectionTerminateMessage()
            : base(GraphqlWsLegacyMessageType.CONNECTION_TERMINATE)
        {
        }
    }
}