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
    /// A message sent by the client when it wants to stop an inflight subscription operation.
    /// </summary>
    ///
    [DebuggerDisplay("GraphqlWsLegacy Subscription Stop (Id: {Id})")]
    internal class GraphqlWsLegacyClientStopMessage : GraphqlWsLegacyMessage<GraphqlWsLegacyNullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyClientStopMessage"/> class.
        /// </summary>
        public GraphqlWsLegacyClientStopMessage()
            : base(GraphqlWsLegacyMessageType.STOP)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyClientStopMessage" /> class.
        /// </summary>
        /// <param name="id">The identifier of the subscription to stop.</param>
        public GraphqlWsLegacyClientStopMessage(string id)
            : this()
        {
            this.Id = id;
        }
    }
}