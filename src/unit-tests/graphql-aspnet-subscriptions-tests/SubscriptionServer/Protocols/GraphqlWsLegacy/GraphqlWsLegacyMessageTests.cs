// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.SubscriptionServer.Protocols.GraphqlWsLegacy
{
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy.Messaging;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy.Messaging.Common;
    using NUnit.Framework;

    [TestFixture]
    public class GraphqlWsLegacyMessageTests
    {
        private class TestNullPayloadMessage : GraphqlWsLegacyMessage<GraphqlWsLegacyNullPayload>
        {
            public TestNullPayloadMessage()
                : base(GraphqlWsLegacyMessageType.UNKNOWN)
            {
            }
        }

        [Test]
        public void PayloadTypeOfNull_AlwaysReturnsNull()
        {
            var message = new TestNullPayloadMessage();

            Assert.IsNull(message.Payload);
            message.Payload = new GraphqlWsLegacyNullPayload();
            Assert.IsNull(message.Payload);
        }
    }
}