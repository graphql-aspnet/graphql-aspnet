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
    /// A message sent by the client when it wants to stop an inflight subscription operation.
    /// </summary>
    ///
    [DebuggerDisplay("graphql-ws: Subscription Stop (Id: {Id})")]
    public class GQLWSClientStopMessage : GQLWSMessage<GQLWSNullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GQLWSClientStopMessage"/> class.
        /// </summary>
        public GQLWSClientStopMessage()
            : base(GQLWSMessageType.STOP)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GQLWSClientStopMessage" /> class.
        /// </summary>
        /// <param name="id">The identifier of the subscription to stop.</param>
        public GQLWSClientStopMessage(string id)
            : this()
        {
            this.Id = id;
        }
    }
}