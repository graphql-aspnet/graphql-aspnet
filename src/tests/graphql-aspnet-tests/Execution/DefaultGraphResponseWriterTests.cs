// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution
{
    using System;
    using System.IO;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Response;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Execution.ExecutionPlanTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DefaultGraphResponseWriterTests
    {
        private async Task<string> WriteResponse(IGraphQueryResponseWriter writer, IGraphOperationResult result, GraphQLResponseOptions options = null)
        {
            var stream = new MemoryStream();
            options = options ?? new GraphQLResponseOptions()
            {
                ExposeExceptions = true,
                ExposeMetrics = true,
            };

            await writer.WriteAsync(stream, result, options);
            stream.Seek(0, SeekOrigin.Begin);

            using (var reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }

        [Test]
        public async Task DefaultWriteOperation_InvalidSyntax_RendersJsonWithError()
        {
            var fixedDate = DateTimeOffset.UtcNow.UtcDateTime;

            var serverBuilder = new TestServerBuilder()
                .AddGraphType<SimpleExecutionController>();

            serverBuilder.AddGraphQL(o =>
            {
                o.ResponseOptions.TimeStampLocalizer = (_) => fixedDate;
            });

            var server = serverBuilder.Build();
            var queryBuilder = server.CreateQueryContextBuilder();

            queryBuilder.AddQueryText("query Operation1{  simple \n {{  simpleQueryMethod { property1} } }");

            var response = await server.ExecuteQuery(queryBuilder);
            var writer = new DefaultResponseWriter<GraphSchema>(server.Schema);
            var result = await this.WriteResponse(writer, response);

            var expectedData = @"
                {
                  ""errors"": [
                            {
                                ""message"": ""Invalid query. Expected 'NameToken' but received 'ControlToken'"",
                                ""locations"": [
                                {
                                    ""line"": 2,
                                    ""column"": 3
                                }],
                                ""extensions"": {
                                    ""code"": ""SYNTAX_ERROR"",
                                    ""timestamp"": """ + fixedDate.ToRfc3339String() + @""",
                                    ""severity"": ""CRITICAL""
                                }
                            }]
                }";

            // no errors collection generated because no errors occured.
            CommonAssertions.AreEqualJsonStrings(
                expectedData,
                result);
        }

        [Test]
        public async Task MetaDataOnMessage_RendersAsExpected()
        {
            var fixedDate = DateTimeOffset.UtcNow.UtcDateTime;

            var builder = new TestServerBuilder()
                .AddGraphType<SimpleExecutionController>();

            builder.AddGraphQL(o =>
            {
                o.ResponseOptions.TimeStampLocalizer = (_) => fixedDate;
            });

            var server = builder.Build();
            var queryBuilder = server.CreateQueryContextBuilder();

            queryBuilder.AddQueryText("query Operation1{  simple  {  customMessage  } }");

            var response = await server.ExecuteQuery(queryBuilder);
            var writer = new DefaultResponseWriter<GraphSchema>(server.Schema);
            var result = await this.WriteResponse(writer, response);

            var expectedData = @"
                {
                  ""errors"": [
                    {
                      ""message"": ""fail text"",
                      ""extensions"": {
                        ""code"": ""fail code"",
                        ""timestamp"": """ + fixedDate.ToRfc3339String() + @""",

                        ""severity"": ""CRITICAL"",
                        ""metaData"" : {
                            ""customKey1"": ""customValue1""
                        }
                      }
                    }
                  ]
                }";

            // no errors collection generated because no errors occured.
            CommonAssertions.AreEqualJsonStrings(
                expectedData,
                result);
        }

        [Test]
        public async Task MetaDataKeyMatchesInternalKeyOnMessage_RendersWithIndexValue()
        {
            var fixedDate = DateTimeOffset.UtcNow.UtcDateTime;

            var builder = new TestServerBuilder()
                .AddGraphType<SimpleExecutionController>();

            builder.AddGraphQL(o =>
            {
                o.ResponseOptions.TimeStampLocalizer = (_) => fixedDate;
            });

            var server = builder.Build();
            var queryBuilder = server.CreateQueryContextBuilder();

            queryBuilder.AddQueryText("query Operation1{  simple  {  customMessageKeyClash  } }");

            var response = await server.ExecuteQuery(queryBuilder);
            var writer = new DefaultResponseWriter<GraphSchema>(server.Schema);
            var result = await this.WriteResponse(writer, response);

            var expectedData = @"
                {
                  ""errors"": [
                    {
                      ""message"": ""fail text"",
                      ""extensions"": {
                        ""code"": ""fail code"",
                        ""timestamp"": """ + fixedDate.ToRfc3339String() + @""",
                        ""severity"": ""CRITICAL"",
                        ""metaData"" : {
                            ""severity"": ""gleam""
                        }
                      }
                    }
                  ]
                }";

            // no errors collection generated because no errors occured.
            CommonAssertions.AreEqualJsonStrings(
                expectedData,
                result);
        }

        [Test]
        public async Task ExceptionOnMessage_RendersAsExpected()
        {
            var fixedDate = DateTimeOffset.UtcNow.UtcDateTime;

            var builder = new TestServerBuilder()
                .AddGraphType<SimpleExecutionController>();

            builder.AddGraphQL(o =>
            {
                o.ResponseOptions.TimeStampLocalizer = (_) => fixedDate;
            });

            var server = builder.Build();
            var queryBuilder = server.CreateQueryContextBuilder();

            queryBuilder.AddQueryText("query Operation1{  simple  {  throwsException  } }");

            var response = await server.ExecuteQuery(queryBuilder);
            var writer = new DefaultResponseWriter<GraphSchema>(server.Schema);
            var result = await this.WriteResponse(writer, response);

            var exceptionStackTrace = JsonEncodedText.Encode(response.Messages[0].Exception.StackTrace, JavaScriptEncoder.UnsafeRelaxedJsonEscaping).ToString();

            var expectedData = @"
                                {
                                  ""errors"": [
                                    {
                                      ""message"": ""Operation failed."",
                                      ""locations"": [
                                        {
                                          ""line"": 1,
                                          ""column"": 31
                                        }
                                      ],
                                      ""path"": [
                                        ""simple"",
                                        ""throwsException""
                                      ],
                                      ""extensions"": {
                                        ""code"": ""UNHANDLED_EXCEPTION"",
                                        ""timestamp"": """ + fixedDate.ToRfc3339String() + @""",
                                        ""severity"": ""CRITICAL"",
                                        ""exception"": {
                                          ""type"": ""InvalidOperationException"",
                                          ""message"": ""This is an invalid message"",
                                          ""stacktrace"": """ + exceptionStackTrace + @""",
                                        }
                                      }
                                    }
                                  ]
                                }";

            CommonAssertions.AreEqualJsonStrings(
                expectedData,
                result);
        }

        [TestCase("abc", "abc")]
        [TestCase("this is 'some' text", "this is 'some' text")]
        [TestCase("This is 'some text' with '다이아몬드' characters", "This is 'some text' with '다이아몬드' characters")]
        [TestCase("This is 'some text' with a \nnewline '다이아몬드' characters", "This is 'some text' with a \\nnewline '다이아몬드' characters")]
        public async Task EnsureProperStringEscapement(string testValue, string expectedValue)
        {
            var fixedDate = DateTimeOffset.UtcNow.UtcDateTime;

            var builder = new TestServerBuilder()
                .AddGraphType<SimpleExecutionController>();

            builder.AddGraphQL(o =>
            {
                o.ResponseOptions.TimeStampLocalizer = (_) => fixedDate;
            });

            var server = builder.Build();
            var queryBuilder = server.CreateQueryContextBuilder();

            var dic = new ResponseFieldSet();
            dic.Add("testKey", new ResponseSingleValue(testValue));

            var mockResult = new Mock<IGraphOperationResult>();
            mockResult.Setup(x => x.Data).Returns(dic);
            mockResult.Setup(x => x.Messages).Returns(new GraphMessageCollection());
            mockResult.Setup(x => x.Request).Returns(queryBuilder.OperationRequest);

            var writer = new DefaultResponseWriter<GraphSchema>(server.Schema);
            var result = await this.WriteResponse(writer, mockResult.Object);

            var expected = @"
                            {
                                ""data"" : {
                                    ""testKey"" : """ + expectedValue + @""",
                                }
                            }";

            CommonAssertions.AreEqualJsonStrings(expected, result);
        }
    }
}