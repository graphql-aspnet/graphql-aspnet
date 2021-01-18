// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.TestServerExtensions
{
    using System;
    using System.Text;
    using System.Text.Json;
    using GraphQL.AspNet.Apollo.Messages;
    using GraphQL.AspNet.Apollo.Messages.ServerMessages;
    using GraphQL.AspNet.Tests.Framework.Clients;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.Subscriptions.Tests.Apollo.ApolloTestData;
    using GraphQL.Subscriptions.Tests.TestServerExtensions.ApolloMessaging;
    using NUnit.Framework;

    public static class MockClientAsserts
    {
        /// <summary>
        /// Asserts that a response message from the server queued and that it is of the supplied type.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="type">The type of message to check for.</param>
        /// <param name="dequeue">if true, the message is removed from the queue.</param>
        internal static void AssertApolloResponse(
            this MockClientConnection connection,
            ApolloMessageType type,
            bool dequeue = true)
        {
            AssertApolloResponse(connection, type, null, false, null, false, dequeue);
        }

        /// <summary>
        /// Asserts that a response message from the server queued and that it is of the supplied type.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="type">The type of message to check for.</param>
        /// <param name="id">The id returned by the server, if supplied.</param>
        /// <param name="dequeue">if true, the message is removed from the queue.</param>
        internal static void AssertApolloResponse(
            this MockClientConnection connection,
            ApolloMessageType type,
            string id,
            bool dequeue = true)
        {
            AssertApolloResponse(connection, type, id, true, null, false, dequeue);
        }

        /// <summary>
        /// Asserts that the next message on the received queue is a DATA message and that the payload
        /// matches the provided json string.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="type">The expected type of the message.</param>
        /// <param name="id">The expected identifier of the subscription that rendered the data.</param>
        /// <param name="expectedPayloadJson">The expected payload of the message, converted to a json string.</param>
        /// <param name="dequeue">if set to <c>true</c> if the message should be removed from the queue.</param>
        internal static void AssertApolloResponse(
            this MockClientConnection connection,
            ApolloMessageType type,
            string id,
            string expectedPayloadJson,
            bool dequeue = true)
        {
            AssertApolloResponse(connection, type, id, true, expectedPayloadJson, true, dequeue);
        }

        private static void AssertApolloResponse(
            this MockClientConnection connection,
            ApolloMessageType type,
            string id,
            bool compareId,
            string expectedPayloadJson,
            bool compareJson,
            bool dequeue = true)
        {
            if (connection.ResponseMessageCount == 0)
                Assert.Fail("No messages queued.");

            var message = dequeue ? connection.DequeueNextReceivedMessage() : connection.PeekNextReceivedMessage();
            var str = Encoding.UTF8.GetString(message.Data);

            var options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            options.AllowTrailingCommas = true;
            options.Converters.Add(new ApolloResponseMessageConverter());

            var convertedMessage = System.Text.Json.JsonSerializer.Deserialize<ApolloResponseMessage>(str, options);

            Assert.IsNotNull(convertedMessage, "Could not deserialized response message");
            Assert.AreEqual(type, convertedMessage.Type, $"Expected message type of {type.ToString()} but got {convertedMessage.Type.ToString()}");

            if (compareJson)
            {
                if (expectedPayloadJson == null)
                    Assert.IsNull(convertedMessage.Payload);
                else
                    CommonAssertions.AreEqualJsonStrings(expectedPayloadJson, convertedMessage.Payload);
            }

            if (compareId)
                Assert.AreEqual(id, convertedMessage.Id);
        }
    }
}