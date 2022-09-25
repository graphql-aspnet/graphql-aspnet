// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.TestServerExtensions.GQLWSMessaging
{
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Common;

    /// <summary>
    /// A general message to deserialize server sent messages into for inspection in testing.
    /// </summary>
    public class GQLWSResponseMessage : GQLWSMessage<string>
    {
        public GQLWSResponseMessage()
            : base(GQLWSMessageType.UNKNOWN)
        {
        }
    }
}