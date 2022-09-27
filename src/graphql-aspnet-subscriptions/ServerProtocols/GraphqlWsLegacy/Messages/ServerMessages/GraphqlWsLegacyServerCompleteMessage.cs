// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.GraphqlWsLegacy.Messages.ServerMessages
{
    using GraphQL.AspNet.GraphqlWsLegacy.Messages.Common;
    using GraphQL.AspNet.GraphqlWsLegacy.Messages.Payloads;

    /// <summary>
    /// A message sent by the GraphqlWsLegacy server when a given subscription (indicated by its client provided id)
    /// will be dropped and no more data will be sent for it.
    /// </summary>
    public class GraphqlWsLegacyServerCompleteMessage : GraphqlWsLegacyMessage<GraphqlWsLegacyNullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyServerCompleteMessage"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public GraphqlWsLegacyServerCompleteMessage(string id)
            : base(GraphqlWsLegacyMessageType.COMPLETE)
        {
            this.Id = id;
        }
    }
}