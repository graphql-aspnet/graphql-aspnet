// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Controllers
{
    using System.IO;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Controllers.GraphQueryControllerData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Web;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Net.Http.Headers;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GraphQueryProcessorTests
    {
        private (IGraphQLHttpProcessor<GraphSchema> Processor, HttpContext Context) CreateQueryArtifacts(GraphQueryData data = null)
        {
            var builder = new TestServerBuilder(TestOptions.CodeDeclaredNames);
            builder.AddGraphType<CandyController>();
            var server = builder.Build();

            return (server.CreateHttpQueryProcessor(), server.CreateHttpContext(data));
        }

        [TestCase(true, GraphQLHttpResponseWriter.NO_WRITER_WITH_DETAIL)]
        [TestCase(false, GraphQLHttpResponseWriter.NO_WRITER_NO_DETAIL)]
        public async Task GraphQlActionResult_NoWriterDeclared(bool exposeExceptions, string expectedText)
        {
            var (_, context) = this.CreateQueryArtifacts();
            var httpContext = context;
            var response = httpContext.Response;

            var operationResult = new Mock<IGraphOperationResult>();
            var result = new GraphQLHttpResponseWriter(operationResult.Object, null, false, exposeExceptions);

            await result.WriteResultAsync(httpContext);

            response.Body.Seek(0, SeekOrigin.Begin);
            string allText;
            using (var reader = new StreamReader(response.Body))
                allText = reader.ReadToEnd();

            Assert.AreEqual(expectedText, allText);
            Assert.AreEqual(500, response.StatusCode);
        }

        [TestCase(true, GraphQLHttpResponseWriter.NO_RESULT_WITH_DETAIL)]
        [TestCase(false, GraphQLHttpResponseWriter.NO_RESULT_NO_DETAIL)]
        public async Task GraphQlActionResult_NoResultProvided(bool exposeExceptions, string expectedText)
        {
            var (_, context) = this.CreateQueryArtifacts();
            var httpContext = context;
            var response = httpContext.Response;

            var writer = new Mock<IGraphQueryResponseWriter>();
            var result = new GraphQLHttpResponseWriter(null, writer.Object, false, exposeExceptions);

            await result.WriteResultAsync(httpContext);

            response.Body.Seek(0, SeekOrigin.Begin);
            string allText;
            using (var reader = new StreamReader(response.Body))
                allText = reader.ReadToEnd();

            Assert.AreEqual(expectedText, allText);
            Assert.AreEqual(500, response.StatusCode);
        }

        [Test]
        public async Task Execution_GeneralReturnCheck()
        {
            var (processor, httpContext) = this.CreateQueryArtifacts(new GraphQueryData()
            {
                Query = "{ candy { count } }",
                OperationName = null,
            });

            var response = httpContext.Response;
            await processor.Invoke(httpContext);

            // headers that should be set
            Assert.IsTrue(response.Headers.ContainsKey(HeaderNames.ContentType));
            Assert.IsTrue(response.Headers.ContainsKey(Constants.ServerInformation.SERVER_INFORMATION_HEADER));

            // check the response body that was written
            var expectedOutput = @"
                {
                    ""data"" : {
                        ""candy"" : {
                              ""count"" : 5
                        }
                    }
                }";

            string responseText;

            response.Body.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(response.Body))
                responseText = reader.ReadToEnd();

            CommonAssertions.AreEqualJsonStrings(expectedOutput, responseText);
        }
    }
}