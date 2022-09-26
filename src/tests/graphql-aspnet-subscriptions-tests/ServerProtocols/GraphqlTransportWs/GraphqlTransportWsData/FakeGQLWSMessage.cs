// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.ServerProtocols.GraphQLWS.GraphQLWSData
{
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Common;

    public class FakeGqltwsMessage : GqltwsMessage
    {
        public new string Type { get; set; }

        public override object PayloadObject => null;
    }
}