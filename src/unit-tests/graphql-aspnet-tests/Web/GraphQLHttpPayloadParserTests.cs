// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Web
{
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using GraphQL.AspNet.Execution.Variables;
    using GraphQL.AspNet.Web;
    using GraphQL.AspNet.Web.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;
    using NUnit.Framework;

    [TestFixture]
    public class GraphQLHttpPayloadParserTests
    {
        public class HttpContextInputs
        {
            public bool IncludeQueryParameter { get; set; }

            public bool IncludeVariableParameter { get; set; }

            public bool IncludeOperationParameter { get; set; }

            public string QueryParameter { get; set; }

            public string VariableParameter { get; set; }

            public string OperationNameParameter { get; set; }

            public string Body { get; set; }

            public string Method { get; set; }

            public string ContentType { get; set; }
        }

        public static List<object[]> _testData;

        static GraphQLHttpPayloadParserTests()
        {
            _testData = new List<object[]>();

            // All in post body
            _testData.Add(new object[]
            {
                new HttpContextInputs()
                {
                    Method = "POST",
                    ContentType = "application/json",
                    Body = "{ \"query\": \"{a{b c}}\", \"operationName\": \"a\", \"variables\": { \"q\": \"w\" } }",
                },
                new GraphQueryData()
                {
                    Query = "{a{b c}}",
                    OperationName = "a",
                    Variables = InputVariableCollection.FromJsonDocument("{\"q\": \"w\"}"),
                },
            });

            // All in query string as GET
            _testData.Add(new object[]
            {
                new HttpContextInputs()
                {
                    Method = "GET",
                    IncludeQueryParameter = true,
                    IncludeOperationParameter = true,
                    IncludeVariableParameter = true,
                    QueryParameter = "{a{b c}}",
                    OperationNameParameter = "a",
                    VariableParameter = HttpUtility.UrlEncode("{\"q\": \"w\"}"),
                },

                new GraphQueryData()
                {
                    Query = "{a{b c}}",
                    OperationName = "a",
                    Variables = InputVariableCollection.FromJsonDocument("{\"q\": \"w\"}"),
                },
            });

            // post body with query string override
            _testData.Add(new object[]
            {
                new HttpContextInputs()
                {
                    IncludeQueryParameter = true,
                    QueryParameter = HttpUtility.UrlEncode("{a{b c}}"),
                    Method = "POST",
                    ContentType = "application/json",
                    Body = "{ \"query\": \"{b{a c}}\" }",
                },
                new GraphQueryData()
                {
                    Query = "{a{b c}}",
                },
            });

            // post body with operation name override
            _testData.Add(new object[]
            {
                new HttpContextInputs()
                {
                    IncludeOperationParameter = true,
                    OperationNameParameter = "a",
                    Method = "POST",
                    ContentType = "application/json",
                    Body = "{ \"query\": \"{b{a c}}\" , \"operationName\": \"b\"}",
                },
                new GraphQueryData()
                {
                    Query = "{b{a c}}",
                    OperationName = "a",
                },
            });

            // post body with variable set override
            _testData.Add(new object[]
            {
                new HttpContextInputs()
                {
                    Method = "POST",
                    ContentType = "application/json",
                    Body = "{ \"query\": \"{a{b c}}\", \"operationName\": \"a\", \"variables\": { \"q\": \"w\" } }",

                    IncludeVariableParameter = true,
                    VariableParameter = HttpUtility.UrlEncode("{\"q\": \"j\"}"),
                },
                new GraphQueryData()
                {
                    Query = "{a{b c}}",
                    OperationName = "a",
                    Variables = InputVariableCollection.FromJsonDocument("{\"q\": \"j\"}"),
                },
            });

            // as graphql content type
            _testData.Add(new object[]
            {
                new HttpContextInputs()
                {
                    Method = "POST",
                    ContentType = Constants.Web.GRAPHQL_CONTENT_TYPE_HEADER_VALUE,
                    Body = "{a{b c}}",
                },
                new GraphQueryData()
                {
                    Query = "{a{b c}}",
                },
            });

            // as graphql content type with qury string override
            _testData.Add(new object[]
            {
                new HttpContextInputs()
                {
                    Method = "POST",
                    ContentType = Constants.Web.GRAPHQL_CONTENT_TYPE_HEADER_VALUE,
                    Body = "{a{b c}}",
                    IncludeQueryParameter = true,
                    QueryParameter = "{c{d e}}",
                },
                new GraphQueryData()
                {
                    Query = "{c{d e}}",
                },
            });
        }

        [TestCaseSource(nameof(_testData))]
        public async Task HttpParserTest(HttpContextInputs inputs, GraphQueryData expectedOutput)
        {
            var context = new DefaultHttpContext();

            // setup the query string
            var queryParams = new List<string>();
            if (inputs.IncludeQueryParameter)
                queryParams.Add($"{Constants.Web.QUERYSTRING_QUERY_KEY}={inputs.QueryParameter}");
            if (inputs.IncludeOperationParameter)
                queryParams.Add($"{Constants.Web.QUERYSTRING_OPERATIONNAME_KEY}={inputs.OperationNameParameter}");
            if (inputs.IncludeVariableParameter)
                queryParams.Add($"{Constants.Web.QUERYSTRING_VARIABLES_KEY}={inputs.VariableParameter}");

            if (queryParams.Count > 0)
                context.Request.QueryString = new QueryString("?" + string.Join("&", queryParams));

            // setup request details
            context.Request.Method = inputs.Method;
            context.Request.ContentType = inputs.ContentType;
            if (inputs.Body != null)
            {
                context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(inputs.Body));
            }

            var parser = new GraphQLHttpPayloadParser(context);
            var result = await parser.ParseAsync();

            Assert.AreEqual(expectedOutput.Query, result.Query);
            Assert.AreEqual(expectedOutput.OperationName, result.OperationName);

            if (result.Variables == null)
            {
                Assert.AreEqual(expectedOutput.Variables, result.Variables);
                return;
            }

            Assert.AreEqual(expectedOutput.Variables.Count, result.Variables.Count);
            foreach (var expectedVar in expectedOutput.Variables)
            {
                var found = result.Variables.TryGetVariable(expectedVar.Key, out var val);
                Assert.IsTrue(found);
                Assert.AreEqual(expectedVar.Value.GetType(), val.GetType());
            }
        }

        [Test]
        public async Task JsonDeserialziationError_PostBody_ThrowsHttpParseException()
        {
            var context = new DefaultHttpContext();

            // invalid json
            var bodyText = "\"query\": \"{a{b c}}\", \"operationName\": \"a\", \"variables\": { \"q\": \"w\" } }";

            // setup request details
            context.Request.Method = "POST";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyText));

            var parser = new GraphQLHttpPayloadParser(context);

            try
            {
                await parser.ParseAsync();
            }
            catch (HttpContextParsingException ex)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.StatusCode);
                return;
            }

            Assert.Fail();
        }

        [Test]
        public async Task JsonDeserialziationError_VariablesQueryString_ThrowsHttpParseException()
        {
            var context = new DefaultHttpContext();

            // invalid json
            var bodyText = "{ \"query\": \"{a{b c}}\" }";

            // setup request details
            context.Request.Method = "POST";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyText));

            // not valid json in the variables parameter
            context.Request.QueryString = new QueryString($"?{Constants.Web.QUERYSTRING_VARIABLES_KEY}={HttpUtility.UrlEncode("\"a\":\"b\"}")}");

            var parser = new GraphQLHttpPayloadParser(context);

            try
            {
                await parser.ParseAsync();
            }
            catch (HttpContextParsingException ex)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.StatusCode);
                return;
            }

            Assert.Fail();
        }

        [Test]
        public async Task NotPostOrGet_ThrowsHttpParseException()
        {
            var context = new DefaultHttpContext();

            // invalid json
            var bodyText = "{ \"query\": \"{a{b c}}\", \"operationName\": \"a\", \"variables\": { \"q\": \"w\" } }";

            // setup request details
            context.Request.Method = "DELETE";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyText));

            var parser = new GraphQLHttpPayloadParser(context);

            try
            {
                await parser.ParseAsync();
            }
            catch (HttpContextParsingException ex)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ex.StatusCode);
                return;
            }

            Assert.Fail();
        }
    }
}