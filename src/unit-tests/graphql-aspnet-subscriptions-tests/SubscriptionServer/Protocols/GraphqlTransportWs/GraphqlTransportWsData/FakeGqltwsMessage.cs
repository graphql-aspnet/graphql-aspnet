// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.SubscriptionServer.Protocols.GraphqlTransportWs.GraphqlTransportWsData
{
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messages.Common;

    public class FakeGqltwsMessage : GqltwsMessage
    {
        public new string Type { get; set; }

        public override object PayloadObject => null;
    }
}