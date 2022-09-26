// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.ServerProtocols.GraphqlTransportWs
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.BidirectionalMessages;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.Common;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.Converters;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.ServerMessages;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.Subscriptions.Tests.ServerProtocols.GraphqlTransportWs.GraphqlTransportWsData;
    using GraphQL.Subscriptions.Tests.TestServerExtensions;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class GqltwsConverterTests
    {
        public class GQWSCustomMessage : GqltwsMessage<string>
        {
            public GQWSCustomMessage()
                : base(GqltwsMessageType.COMPLETE)
            {
            }
        }

        [Test]
        public void CompleteMessage_WithId_SerializesCorrectly()
        {
            var message = new GqltwsSubscriptionCompleteMessage("abc123");

            var converter = new GqltwsServerCompleteMessageConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var response = JsonSerializer.Serialize((object)message, message.GetType(), options);

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
            var message = new GqltwsSubscriptionCompleteMessage(null);
            var converter = new GqltwsServerCompleteMessageConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var response = JsonSerializer.Serialize((object)message, message.GetType(), options);

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
                .AddGraphController<GqltwsDataMessageController>()
                .AddSubscriptionServer()
                .AddGraphQL(options =>
                {
                    options.ResponseOptions.TimeStampLocalizer = (time) => dt;
                })
                .Build();

            var converter = new GqltwsServerErrorMessageConverter(server.Schema);

            var message = new GqltwsServerErrorMessage(
                "an error occured",
                Constants.ErrorCodes.BAD_REQUEST,
                GraphMessageSeverity.Warning,
                "prev123",
                lastMessageType: GqltwsMessageType.SUBSCRIBE,
                clientProvidedId: "abc123");

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var response = JsonSerializer.Serialize((object)message, message.GetType(), options);

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
                            ""lastMessage_type"":""subscribe""
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
                .AddGraphController<GqltwsDataMessageController>()
                .AddSubscriptionServer()
                .AddGraphQL(options =>
                {
                    options.ResponseOptions.TimeStampLocalizer = (time) => dt;
                })
                .Build();

            var converter = new GqltwsServerErrorMessageConverter(server.Schema);

            var message = new GqltwsServerErrorMessage(
                "an error occured",
                Constants.ErrorCodes.BAD_REQUEST,
                GraphMessageSeverity.Warning,
                "prev123",
                lastMessageType: GqltwsMessageType.SUBSCRIBE);

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var response = JsonSerializer.Serialize((object)message, message.GetType(), options);

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
                            ""lastMessage_type"":""subscribe""
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
                .AddGraphController<GqltwsDataMessageController>()
                .AddSubscriptionServer()
                .Build();

            var client = server.CreateSubscriptionClient();

            var context = server.CreateQueryContextBuilder()
                    .AddQueryText("query { getValue { property1 property2 } }")
                    .Build();
            await server.ExecuteQuery(context);

            var message = new GqltwsServerNextDataMessage("abc111", context.Result);

            var converter = new GqltwsServerDataMessageConverter(
                server.Schema,
                server.ServiceProvider.GetRequiredService<IGraphResponseWriter<GraphSchema>>());

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var response = JsonSerializer.Serialize((object)message, message.GetType(), options);

            var expected = @"
            {
                ""type"" : ""next"",
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
            var message = new GqltwsServerAckOperationMessage();
            message.Id = "abc";

            var converter = new GqltwsMessageConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var response = JsonSerializer.Serialize((object)message, typeof(GqltwsMessage), options);

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
            var message = new GqltwsServerAckOperationMessage();
            var converter = new GqltwsMessageConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var response = JsonSerializer.Serialize((object)message, typeof(GqltwsMessage), options);

            var expected = @"
            {
                ""type"" : ""connection_ack""
            }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
        }

        [Test]
        public void GeneralMessage_WithNoData_SerializesCorrectly()
        {
            var server = new TestServerBuilder()
                .AddGraphController<GqltwsDataMessageController>()
                .AddSubscriptionServer()
                .Build();

            var message = new GQWSCustomMessage();
            message.Payload = null;
            message.Id = "abc";

            var converter = new GqltwsMessageConverter();
            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);

            var response = JsonSerializer.Serialize(message, typeof(GqltwsMessage), options);

            // expect no payload attribute
            var expected = @"
            {
                ""id"": ""abc"",
                ""type"" : ""complete""
            }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
        }

        [Test]
        public void GeneralMessage_WithData_SerializesCorrectly()
        {
            var server = new TestServerBuilder()
                .AddGraphController<GqltwsDataMessageController>()
                .AddSubscriptionServer()
                .Build();

            var message = new GQWSCustomMessage();
            message.Payload = "mydata";
            message.Id = "abc";

            var converter = new GqltwsMessageConverter();
            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);

            var response = JsonSerializer.Serialize(message, typeof(GqltwsMessage), options);

            // expect no payload attribute
            var expected = @"
            {
                ""id"": ""abc"",
                ""type"" : ""complete"",
                ""payload"" : ""mydata""
            }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
        }
    }
}