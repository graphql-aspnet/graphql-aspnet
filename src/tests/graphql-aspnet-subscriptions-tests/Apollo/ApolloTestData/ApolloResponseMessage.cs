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
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages.Common;

    /// <summary>
    /// A general message to deserialize server sent messages into for inspection in testing.
    /// </summary>
    public class ApolloResponseMessage : ApolloMessage
    {
        public override object PayloadObject { get; }
    }
}