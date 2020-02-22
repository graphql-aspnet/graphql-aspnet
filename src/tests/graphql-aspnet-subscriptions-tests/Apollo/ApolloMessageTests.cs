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
    using GraphQL.AspNet.Apollo.Messages;
    using GraphQL.AspNet.Apollo.Messages.Common;
    using GraphQL.AspNet.Apollo.Messages.Payloads;
    using NUnit.Framework;

    [TestFixture]
    public class ApolloMessageTests
    {
        private class TestNullPayloadMessage : ApolloMessage<ApolloNullPayload>
        {
            public TestNullPayloadMessage()
                : base(ApolloMessageType.UNKNOWN)
            {
            }
        }

        [Test]
        public void PayloadTypeOfNull_AlwaysReturnsNull()
        {
            var message = new TestNullPayloadMessage();

            Assert.IsNull(message.Payload);
            message.Payload = new ApolloNullPayload();
            Assert.IsNull(message.Payload);
        }
    }
}