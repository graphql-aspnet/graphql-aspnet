// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy.Messages.ServerMessages
{
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy.Messages.Common;

    /// <summary>
    /// A keep alive message sent periodically by the server to keep the connection
    /// open a the application level.
    /// </summary>
    public class GraphqlWsLegacyKeepAliveOperationMessage : GraphqlWsLegacyMessage<GraphqlWsLegacyNullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyKeepAliveOperationMessage"/> class.
        /// </summary>
        public GraphqlWsLegacyKeepAliveOperationMessage()
            : base(GraphqlWsLegacyMessageType.CONNECTION_KEEP_ALIVE)
        {
        }
    }
}