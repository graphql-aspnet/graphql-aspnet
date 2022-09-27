// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.TestServerExtensions.GraphqlWsLegacyMessaging
{
    using GraphQL.AspNet.GraphqlWsLegacy.Messages;
    using GraphQL.AspNet.GraphqlWsLegacy.Messages.Common;

    /// <summary>
    /// A general message to deserialize server sent messages into for inspection in testing.
    /// </summary>
    public class GraphqlWsLegacyResponseMessage : GraphqlWsLegacyMessage<string>
    {
        public GraphqlWsLegacyResponseMessage()
            : base(GraphqlWsLegacyMessageType.UNKNOWN)
        {
        }
    }
}