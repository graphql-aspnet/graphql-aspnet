// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.SubscriptionServer.Protocols.GraphqlTransportWs
{
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messaging;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messaging.Common;
    using NUnit.Framework;

    [TestFixture]
    public class GqltwsMessageTests
    {
        private class TestNullPayloadMessage : GqltwsMessage<GqltwsNullPayload>
        {
            public TestNullPayloadMessage()
                : base(GqltwsMessageType.UNKNOWN)
            {
            }
        }

        [Test]
        public void PayloadTypeOfNull_AlwaysReturnsNull()
        {
            var message = new TestNullPayloadMessage();

            Assert.IsNull(message.Payload);
            message.Payload = new GqltwsNullPayload();
            Assert.IsNull(message.Payload);
        }
    }
}