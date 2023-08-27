// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.ServerExtensions.MutlipartRequests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Web;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using NSubstitute;
    using NUnit.Framework;
    using static System.Formats.Asn1.AsnWriter;

    [TestFixture]
    public class BatchGraphQLHttpResponseWriterTests
    {
        private HttpContext CreateContext()
        {
            var httpContext = new DefaultHttpContext()
            {
                Response =
                {
                    Body = new MemoryStream(),
                },
            };

            return httpContext;
        }

        private IQueryExecutionResult CreateTestResult(string errorMessage, string errorCode)
        {
            var result = new QueryExecutionResult(Substitute.For<IQueryExecutionRequest>());
            result.Messages.Add(GraphMessageSeverity.Critical, errorMessage, errorCode);

            return result;
        }

        [Test]
        public async Task SingleResult_IsWrittenAsSingleJsonObject()
        {
            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.ResponseOptions.ExposeExceptions = false;
                    o.ResponseOptions.ExposeMetrics = false;
                    o.ResponseOptions.IndentDocument = true;
                    o.ResponseOptions.TimeStampLocalizer = (DateTimeOffset offset) => new DateTime(2023, 3, 4, 4, 5, 6, DateTimeKind.Utc);
                })
                .Build();

            var scope = server.ServiceProvider.CreateScope();
            var jsonWriter = scope.ServiceProvider.GetService<IQueryResponseWriter<GraphSchema>>();
            var context = this.CreateContext();

            var singleResult = this.CreateTestResult("test message", "TEST");
            var writer = new BatchGraphQLHttpResponseWriter(
                server.Schema,
                singleResult,
                jsonWriter);

            await writer.WriteResultAsync(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var streamReader = new StreamReader(context.Response.Body);
            var text = streamReader.ReadToEnd();

            var expectedOutput = @"
            {
              ""errors"": [
                {
                  ""message"": ""test message"",
                  ""extensions"": {
                    ""code"": ""TEST"",
                    ""timestamp"": ""2023-03-04T04:05:06.000+00:00"",
                    ""severity"": ""CRITICAL""
                  }
                }
              ]
            }";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, text);
        }

        [Test]
        public async Task MultipleResults_IsWrittenAsAJsonArray()
        {
            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.ResponseOptions.ExposeExceptions = false;
                    o.ResponseOptions.ExposeMetrics = false;
                    o.ResponseOptions.IndentDocument = true;
                    o.ResponseOptions.TimeStampLocalizer = (DateTimeOffset offset) => new DateTime(2023, 3, 4, 4, 5, 6, DateTimeKind.Utc);
                })
                .Build();

            var scope = server.ServiceProvider.CreateScope();
            var jsonWriter = scope.ServiceProvider.GetService<IQueryResponseWriter<GraphSchema>>();
            var context = this.CreateContext();

            var result1 = this.CreateTestResult("test message", "TEST");
            var result2 = this.CreateTestResult("test message2", "TEST2");

            var results = result1.AsEnumerable().Concat(result2.AsEnumerable()).ToList();

            var writer = new BatchGraphQLHttpResponseWriter(
                server.Schema,
                results,
                jsonWriter);

            await writer.WriteResultAsync(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var streamReader = new StreamReader(context.Response.Body);
            var text = streamReader.ReadToEnd();

            var expectedOutput = @"
            [
                {
                  ""errors"": [
                    {
                      ""message"": ""test message"",
                      ""extensions"": {
                        ""code"": ""TEST"",
                        ""timestamp"": ""2023-03-04T04:05:06.000+00:00"",
                        ""severity"": ""CRITICAL""
                      }
                    }
                  ]
                },
                {
                  ""errors"": [
                    {
                      ""message"": ""test message2"",
                      ""extensions"": {
                        ""code"": ""TEST2"",
                        ""timestamp"": ""2023-03-04T04:05:06.000+00:00"",
                        ""severity"": ""CRITICAL""
                      }
                    }
                  ]
                }
            ]";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, text);
        }

        [TestCase(true, BatchGraphQLHttpResponseWriter.NO_WRITER_WITH_DETAIL)]
        [TestCase(false, BatchGraphQLHttpResponseWriter.NO_WRITER_NO_DETAIL)]
        public async Task GraphQlActionResult_NoWriterDeclared(bool exposeExceptions, string expectedText)
        {
            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.ResponseOptions.ExposeExceptions = exposeExceptions;
                    o.ResponseOptions.ExposeMetrics = false;
                    o.ResponseOptions.IndentDocument = true;
                    o.ResponseOptions.TimeStampLocalizer = (DateTimeOffset offset) => new DateTime(2023, 3, 4, 4, 5, 6, DateTimeKind.Utc);
                })
                .Build();

            var context = this.CreateContext();
            var httpContext = context;
            var response = httpContext.Response;

            var queryResult = Substitute.For<IQueryExecutionResult>();
            var result = new BatchGraphQLHttpResponseWriter(
                server.Schema,
                Substitute.For<IQueryExecutionResult>(),
                null);

            await result.WriteResultAsync(httpContext);

            response.Body.Seek(0, SeekOrigin.Begin);
            string allText;
            using (var reader = new StreamReader(response.Body))
                allText = reader.ReadToEnd();

            Assert.AreEqual(expectedText, allText);
            Assert.AreEqual(500, response.StatusCode);
        }

        [TestCase(true, BatchGraphQLHttpResponseWriter.NO_RESULT_WITH_DETAIL)]
        [TestCase(false, BatchGraphQLHttpResponseWriter.NO_RESULT_NO_DETAIL)]
        public async Task GraphQlActionResult_NoResultProvided(bool exposeExceptions, string expectedText)
        {
            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.ResponseOptions.ExposeExceptions = exposeExceptions;
                    o.ResponseOptions.ExposeMetrics = false;
                    o.ResponseOptions.IndentDocument = true;
                    o.ResponseOptions.TimeStampLocalizer = (DateTimeOffset offset) => new DateTime(2023, 3, 4, 4, 5, 6, DateTimeKind.Utc);
                })
                .Build();

            var context = this.CreateContext();

            var scope = server.ServiceProvider.CreateScope();
            var jsonWriter = scope.ServiceProvider.GetService<IQueryResponseWriter<GraphSchema>>();

            var httpContext = context;
            var response = httpContext.Response;

            var queryResult = Substitute.For<IQueryExecutionResult>();
            var result = new BatchGraphQLHttpResponseWriter(
                server.Schema,
                null as IQueryExecutionResult,
                jsonWriter);

            await result.WriteResultAsync(httpContext);

            response.Body.Seek(0, SeekOrigin.Begin);
            string allText;
            using (var reader = new StreamReader(response.Body))
                allText = reader.ReadToEnd();

            Assert.AreEqual(expectedText, allText);
            Assert.AreEqual(500, response.StatusCode);
        }
    }
}