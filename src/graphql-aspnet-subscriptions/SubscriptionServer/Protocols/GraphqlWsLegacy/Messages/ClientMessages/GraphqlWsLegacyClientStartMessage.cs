// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy.Messages.ClientMessages
{
    using System.Diagnostics;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy.Messages.Common;

    /// <summary>
    /// A message sent by the client when it wants to start a new subscription operation.
    /// </summary>
    [DebuggerDisplay("GraphqlWsLegacy Subscription Start (Id: {Id})")]
    public class GraphqlWsLegacyClientStartMessage : GraphqlWsLegacyMessage<GraphQueryData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyClientStartMessage"/> class.
        /// </summary>
        public GraphqlWsLegacyClientStartMessage()
            : base(GraphqlWsLegacyMessageType.START)
        {
        }
    }
}