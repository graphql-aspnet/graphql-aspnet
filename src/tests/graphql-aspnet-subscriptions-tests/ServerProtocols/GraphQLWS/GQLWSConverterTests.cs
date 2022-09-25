// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.ServerProtocols.GraphQLWS
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Common;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Converters;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.ServerMessages;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.Subscriptions.Tests.ServerProtocols.GraphQLWS.GraphQLWSData;
    using GraphQL.Subscriptions.Tests.TestServerExtensions;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class GQLWSConverterTests
    {
        [Test]
        public void CompleteMessage_WithId_SerializesCorrectly()
        {
            var message = new GQLWSSubscriptionCompleteMessage("abc123");

            var converter = new GQLWSServerCompleteMessageConverter();

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
            var message = new GQLWSSubscriptionCompleteMessage(null);
            var converter = new GQLWSServerCompleteMessageConverter();

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
                .AddGraphController<GQLWSDataMessageController>()
                .AddSubscriptionServer()
                .AddGraphQL(options =>
                {
                    options.ResponseOptions.TimeStampLocalizer = (time) => dt;
                })
                .Build();

            var converter = new GQLWSServerErrorMessageConverter(server.Schema);

            var message = new GQLWSServerErrorMessage(
                "an error occured",
                Constants.ErrorCodes.BAD_REQUEST,
                GraphMessageSeverity.Warning,
                "prev123",
                lastMessageType: GQLWSMessageType.SUBSCRIBE,
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
                .AddGraphController<GQLWSDataMessageController>()
                .AddSubscriptionServer()
                .AddGraphQL(options =>
                {
                    options.ResponseOptions.TimeStampLocalizer = (time) => dt;
                })
                .Build();

            var converter = new GQLWSServerErrorMessageConverter(server.Schema);

            var message = new GQLWSServerErrorMessage(
                "an error occured",
                Constants.ErrorCodes.BAD_REQUEST,
                GraphMessageSeverity.Warning,
                "prev123",
                lastMessageType: GQLWSMessageType.SUBSCRIBE);

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
                .AddGraphController<GQLWSDataMessageController>()
                .AddSubscriptionServer()
                .Build();

            var client = server.CreateSubscriptionClient();

            var context = server.CreateQueryContextBuilder()
                    .AddQueryText("query { getValue { property1 property2 } }")
                    .Build();
            await server.ExecuteQuery(context);

            var message = new GQLWSServerNextDataMessage("abc111", context.Result);

            var converter = new GQLWSServerDataMessageConverter(
                server.Schema,
                server.ServiceProvider.GetRequiredService<IGraphResponseWriter<GraphSchema>>());

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var response = JsonSerializer.Serialize((object)message, message.GetType(), options);

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
            var message = new GQLWSServerAckOperationMessage();
            message.Id = "abc";

            var converter = new GQLWSMessageConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var response = JsonSerializer.Serialize((object)message, typeof(GQLWSMessage), options);

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
            var message = new GQLWSServerAckOperationMessage();
            var converter = new GQLWSMessageConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);
            var response = JsonSerializer.Serialize((object)message, typeof(GQLWSMessage), options);

            var expected = @"
            {
                ""type"" : ""connection_ack""
            }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
        }
    }
}