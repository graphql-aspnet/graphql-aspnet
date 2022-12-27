// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.SubscriptionServer.Protocols.GraphqlWsLegacy
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.AspNet.Common.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using GraphQL.AspNet.Tests.Mocks;
    using GraphQL.AspNet.Tests.SubscriptionServer.Protocols.GraphqlWsLegacy.GraphqlWsLegacyData;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy.Messaging.Messages;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy.Messaging.Converters;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy.Messaging;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy.Messaging.Common;

    [TestFixture]
    public class GraphqlWsLegacyConverterTests
    {
        [Test]
        public void CompleteMessage_WithId_SerializesCorrectly()
        {
            var message = new GraphqlWsLegacyServerCompleteMessage("abc123");

            var converter = new GraphqlWsLegacyServerCompleteMessageConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var response = JsonSerializer.Serialize(message, message.GetType(), options);

            var expected = @"
            {
                ""type"" : ""complete"",
                ""id"": ""abc123""
            }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
        }

        [Test]
        public void CompleteMessage_WithoutId_SerializesWithNoIdParameter()
        {
            var message = new GraphqlWsLegacyServerCompleteMessage(null);
            var converter = new GraphqlWsLegacyServerCompleteMessageConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var response = JsonSerializer.Serialize(message, message.GetType(), options);

            var expected = @"
            {
                ""type"" : ""complete"",
            }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
        }

        [Test]
        public void ErrorMessage_WithId_SerializesWithIdParameter()
        {
            var dt = DateTime.UtcNow;
            var server = new TestServerBuilder()
                .AddGraphController<GraphqlWsLegacyDataMessageController>()
                .AddSubscriptionServer()
                .AddGraphQL(options =>
                {
                    options.ResponseOptions.TimeStampLocalizer = (time) => dt;
                })
                .Build();

            var converter = new GraphqlWsLegacyServerErrorMessageConverter(server.Schema);

            var message = new GraphqlWsLegacyServerErrorMessage(
                "an error occured",
                Constants.ErrorCodes.BAD_REQUEST,
                GraphMessageSeverity.Warning,
                "prev123",
                lastMessageType: GraphqlWsLegacyMessageType.START,
                clientProvidedId: "abc123");

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var response = JsonSerializer.Serialize(message, message.GetType(), options);

            // error message should render a single IGraphMessage
            // that is normally part of the errors collection on a standard response
            var expected = @"
            {
                ""id"": ""abc123"",
                ""type"":""error"",
                ""payload"":{
                    ""message"":""an error occured"",
                    ""extensions"":{
                        ""code"":""BAD_REQUEST"",
                        ""timestamp"":""{dateString}"",
                        ""severity"":""WARNING"",
                        ""metaData"":{
                            ""lastMessage_id"":""prev123"",
                            ""lastMessage_type"":""start""
                        }
                    }
                }
            }";

            expected = expected.Replace("{dateString}", dt.ToRfc3339String());

            CommonAssertions.AreEqualJsonStrings(expected, response);
        }

        [Test]
        public void ErrorMessage_WithoutId_SerializesWithoutIdParameter()
        {
            var dt = DateTime.UtcNow;
            var server = new TestServerBuilder()
                .AddGraphController<GraphqlWsLegacyDataMessageController>()
                .AddSubscriptionServer()
                .AddGraphQL(options =>
                {
                    options.ResponseOptions.TimeStampLocalizer = (time) => dt;
                })
                .Build();

            var converter = new GraphqlWsLegacyServerErrorMessageConverter(server.Schema);

            var message = new GraphqlWsLegacyServerErrorMessage(
                "an error occured",
                Constants.ErrorCodes.BAD_REQUEST,
                GraphMessageSeverity.Warning,
                "prev123",
                lastMessageType: GraphqlWsLegacyMessageType.START);

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var response = JsonSerializer.Serialize(message, message.GetType(), options);

            // error message should render a single IGraphMessage
            // that is normally part of the errors collection on a standard response
            var expected = @"
            {
                ""type"":""error"",
                ""payload"":{
                    ""message"":""an error occured"",
                    ""extensions"":{
                        ""code"":""BAD_REQUEST"",
                        ""timestamp"":""{dateString}"",
                        ""severity"":""WARNING"",
                        ""metaData"":{
                            ""lastMessage_id"":""prev123"",
                            ""lastMessage_type"":""start""
                        }
                    }
                }
            }";

            expected = expected.Replace("{dateString}", dt.ToRfc3339String());

            CommonAssertions.AreEqualJsonStrings(expected, response);
        }

        [Test]
        public async Task DataMessage_WithData_SerializesCorrectly()
        {
            var server = new TestServerBuilder()
                .AddGraphController<GraphqlWsLegacyDataMessageController>()
                .AddSubscriptionServer()
                .Build();

            var client = server.CreateSubscriptionClient();

            var context = server.CreateQueryContextBuilder()
                    .AddQueryText("query { getValue { property1 property2 } }")
                    .Build();
            await server.ExecuteQuery(context);

            var message = new GraphqlWsLegacyServerDataMessage("abc111", context.Result);

            var converter = new GraphqlWsLegacyServerDataMessageConverter(
                server.Schema,
                server.ServiceProvider.GetRequiredService<IQueryResponseWriter<GraphSchema>>());

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var response = JsonSerializer.Serialize(message, message.GetType(), options);

            var expected = @"
            {
                ""type"" : ""data"",
                ""id"" : ""abc111"",
                ""payload"": {
                    ""data"": {
                        ""getValue"" : {
                            ""property1"": ""abc123"",
                            ""property2"" : 15
                        }
                    }
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
        }

        [Test]
        public void GeneralMessage_WithId_SerializesCorrectly()
        {
            var message = new GraphqlWsLegacyServerAckOperationMessage();
            message.Id = "abc";

            var converter = new GraphqlWsLegacyMessageConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var response = JsonSerializer.Serialize(message, typeof(GraphqlWsLegacyMessage), options);

            var expected = @"
            {
                ""id"": ""abc"",
                ""type"" : ""connection_ack""
            }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
        }

        [Test]
        public void GeneralMessage_WithoutId_SerializesCorrectly()
        {
            var message = new GraphqlWsLegacyServerAckOperationMessage();
            var converter = new GraphqlWsLegacyMessageConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var response = JsonSerializer.Serialize(message, typeof(GraphqlWsLegacyMessage), options);

            var expected = @"
            {
                ""type"" : ""connection_ack""
            }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
        }
    }
}