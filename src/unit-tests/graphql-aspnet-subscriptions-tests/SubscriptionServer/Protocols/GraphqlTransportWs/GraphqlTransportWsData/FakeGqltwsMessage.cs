﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.SubscriptionServer.Protocols.GraphqlTransportWs.GraphqlTransportWsData
{
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messaging.Common;

    internal class FakeGqltwsMessage : GqltwsMessage
    {
        public new string Type { get; set; }

        public override object PayloadObject => null;
    }
}