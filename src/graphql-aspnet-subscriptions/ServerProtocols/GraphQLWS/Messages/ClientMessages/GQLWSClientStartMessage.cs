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

    /// <summary>
    /// A message sent by the client when it wants to start a new subscription operation.
    /// </summary>
    [DebuggerDisplay("graphql-ws: Subscription Start (Id: {Id})")]
    public class GQLWSClientStartMessage : GQLWSMessage<GraphQueryData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GQLWSClientStartMessage"/> class.
        /// </summary>
        public GQLWSClientStartMessage()
            : base(GQLWSMessageType.START)
        {
        }
    }
}