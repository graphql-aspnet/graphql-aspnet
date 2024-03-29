﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messaging.Messages
{
    using System.Diagnostics;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messaging.Common;

    /// <summary>
    /// A message sent by the client when it wants to start a new subscription operation.
    /// </summary>
    [DebuggerDisplay("graphql-ws: Subscription Start (Id: {Id})")]
    internal class GqltwsClientSubscribeMessage : GqltwsMessage<GraphQueryData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsClientSubscribeMessage"/> class.
        /// </summary>
        public GqltwsClientSubscribeMessage()
            : base(GqltwsMessageType.SUBSCRIBE)
        {
        }
    }
}