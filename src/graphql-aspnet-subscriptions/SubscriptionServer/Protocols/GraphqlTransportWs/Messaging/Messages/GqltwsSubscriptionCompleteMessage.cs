// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messaging.Messages
{
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messaging.Common;

    /// <summary>
    /// A message sent by the client or server when a given subscription (indicated by its client provided id)
    /// will be dropped and no more data will be sent for it.
    /// </summary>
    internal class GqltwsSubscriptionCompleteMessage : GqltwsMessage<GqltwsNullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsSubscriptionCompleteMessage"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public GqltwsSubscriptionCompleteMessage(string id = "")
            : base(GqltwsMessageType.COMPLETE)
        {
            this.Id = id;
        }
    }
}