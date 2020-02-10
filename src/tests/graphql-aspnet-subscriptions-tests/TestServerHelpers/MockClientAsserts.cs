// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.CommonHelpers
{
    using System.Text;
    using System.Text.Json;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages;
    using GraphQL.Subscriptions.Tests.Apollo;
    using GraphQL.Subscrptions.Tests.CommonHelpers;
    using NUnit.Framework;

    public static class MockClientAsserts
    {
        /// <summary>
        /// Asserts that a response message from the server queued and that it is of the supplied type.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="type">The type to check for.</param>
        /// <param name="dequeue">if true, the message is removed from the queue.</param>
        internal static void AssertServerSentMessageType(this MockClientConnection connection, ApolloMessageType type, bool dequeue = true)
        {
            if (connection.ResponseMessageCount == 0)
                Assert.Fail("No messages queued.");

            var message = dequeue ? connection.DequeueNextReceivedMessage() : connection.PeekNextReceivedMessage();
            var str = Encoding.UTF8.GetString(message.Data);

            var options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            options.AllowTrailingCommas = true;

            var convertedMessage = System.Text.Json.JsonSerializer.Deserialize<ApolloResponseMessage>(str, options);

            Assert.IsNotNull(convertedMessage, "Could not deserialized response message");
            Assert.AreEqual(type, convertedMessage.Type, $"Expected message type of {type.ToString()} but got {convertedMessage.Type.ToString()}");
        }
    }
}