﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.ServerProtocols.GraphqlTransportWs
{
    using System.Text;
    using System.Text.Json;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.Subscriptions.Tests.Mocks;
    using GraphQL.Subscriptions.Tests.ServerProtocols.GraphqlTransportWs.GraphqlTransportWsData;
    using NUnit.Framework;

    public static class GqltwsClientAsserts
    {
        /// <summary>
        /// Asserts that a response message from the server queued and that it is of the supplied type.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="type">The type of message to check for.</param>
        /// <param name="dequeue">if true, the message is removed from the queue.</param>
        internal static void AssertGqltwsResponse(
            this MockClientConnection connection,
            GqltwsMessageType type,
            bool dequeue = true)
        {
            connection.AssertGqltwsResponse(type, null, false, null, false, dequeue);
        }

        /// <summary>
        /// Asserts that a response message from the server queued and that it is of the supplied type.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="type">The type of message to check for.</param>
        /// <param name="id">The id returned by the server, if supplied.</param>
        /// <param name="dequeue">if true, the message is removed from the queue.</param>
        internal static void AssertGqltwsResponse(
            this MockClientConnection connection,
            GqltwsMessageType type,
            string id,
            bool dequeue = true)
        {
            connection.AssertGqltwsResponse(type, id, true, null, false, dequeue);
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
        internal static void AssertGqltwsResponse(
            this MockClientConnection connection,
            GqltwsMessageType type,
            string id,
            string expectedPayloadJson,
            bool dequeue = true)
        {
            connection.AssertGqltwsResponse(type, id, true, expectedPayloadJson, true, dequeue);
        }

        private static void AssertGqltwsResponse(
            this MockClientConnection connection,
            GqltwsMessageType type,
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
            options.Converters.Add(new GqltwsResponseMessageConverter());

            var convertedMessage = JsonSerializer.Deserialize<GqltwsResponseMessage>(str, options);

            Assert.IsNotNull(convertedMessage, "Could not deserialize response message");
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