﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.ServerMessages
{
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Common;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Payloads;

    /// <summary>
    /// A message sent by the server when a given subscription (indicated by its client provided id)
    /// will be dropped and no more data will be sent for it.
    /// </summary>
    public class GQLWSServerCompleteMessage : GQLWSMessage<GQLWSNullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GQLWSServerCompleteMessage"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public GQLWSServerCompleteMessage(string id)
            : base(GQLWSMessageType.COMPLETE)
        {
            this.Id = id;
        }
    }
}