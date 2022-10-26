// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.ServerProtocols.GraphqlWsLegacy.GraphqlWsLegacyData
{
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.Common;

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