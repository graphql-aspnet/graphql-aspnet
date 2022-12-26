﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy.Messaging.Messages
{
    using System.Diagnostics;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy.Messaging.Common;

    /// <summary>
    /// A message recieved from the client after the establishment of the websocket to initialize the graphql
    /// session on the socket.
    /// </summary>
    [DebuggerDisplay("GraphqlWsLegacy Client Initialized")]
    internal class GraphqlWsLegacyClientConnectionInitMessage : GraphqlWsLegacyMessage<GraphqlWsLegacyNullPayload>
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