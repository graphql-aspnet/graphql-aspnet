// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.ServerProtocols.GraphqlTransportWs.GraphqlTransportWsData
{
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.Common;

    /// <summary>
    /// A general message to deserialize server sent messages into for inspection in testing.
    /// </summary>
    public class GqltwsResponseMessage : GqltwsMessage<string>
    {
        public GqltwsResponseMessage()
            : base(GqltwsMessageType.UNKNOWN)
        {
        }
    }
}