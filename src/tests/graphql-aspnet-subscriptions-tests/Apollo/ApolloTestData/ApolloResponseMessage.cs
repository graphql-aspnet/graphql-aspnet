// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Apollo
{
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages.Common;
    using Newtonsoft.Json;

    /// <summary>
    /// A general message to deserialize server sent messages into for inspection in testing.
    /// </summary>
    public class ApolloResponseMessage : ApolloMessage
    {
        public override object PayloadObject { get; }
    }
}