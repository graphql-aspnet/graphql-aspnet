﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.SubscriptionServer.Protocols.GraphqlWsLegacy.GraphqlWsLegacyData
{
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy.Messages.Common;

    public class FakeGraphqlWsLegacyMessage : GraphqlWsLegacyMessage
    {
        public new string Type { get; set; }

        public override object PayloadObject => null;
    }
}