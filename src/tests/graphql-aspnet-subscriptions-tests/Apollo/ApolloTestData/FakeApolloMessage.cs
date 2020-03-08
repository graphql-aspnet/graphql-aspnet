// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Apollo.ApolloTestData
{
    using GraphQL.AspNet.Apollo.Messages.Common;

    public class FakeApolloMessage : ApolloMessage
    {
        public new string Type { get; set; }

        public override object PayloadObject => null;
    }
}