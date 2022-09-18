﻿// *************************************************************
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
    using System.Threading.Tasks;
    using System.Web;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.AspNet.Tests.Web.CancelTokenTestData;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Internal;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class GetRequestTests
    {
        public static List<object[]> _queryStringTests;

        static GetRequestTests()
        {
            _queryStringTests = new List<object[]>();

            var queryText = "{add(a: 5, b: 3)}";
            var queryString = $"?{Constants.Web.QUERYSTRING_QUERY_KEY}={queryText}";
            _queryStringTests.Add(new object[] { queryString, 8 });

            queryText = HttpUtility.UrlEncode("{add(a: 5, b: 3)}");
            queryString = $"?{Constants.Web.QUERYSTRING_QUERY_KEY}={queryText}";
            _queryStringTests.Add(new object[] { queryString, 8 });

            queryText = "query ($i: Int!){add(a: 5, b: $i)}";
            var variables = "{\"i\": 3}";
            queryString = $"?{Constants.Web.QUERYSTRING_QUERY_KEY}={queryText}&{Constants.Web.QUERYSTRING_VARIABLES_KEY}={variables}";
            _queryStringTests.Add(new object[] { queryString, 8 });

            queryText = HttpUtility.UrlEncode("query ($i: Int!){add(a: 5, b: $i)}");
            variables = HttpUtility.UrlEncode("{\"i\": 3}");
            queryString = $"?{Constants.Web.QUERYSTRING_QUERY_KEY}={queryText}&{Constants.Web.QUERYSTRING_VARIABLES_KEY}={variables}";
            _queryStringTests.Add(new object[] { queryString, 8 });

            queryText = HttpUtility.UrlEncode("query op1($i: Int!){add(a: 5, b: $i)} query op2{add(a: 5, b:5)}");
            var operation = "op1";
            variables = HttpUtility.UrlEncode("{\"i\": 3}");
            queryString = $"?{Constants.Web.QUERYSTRING_QUERY_KEY}={queryText}&{Constants.Web.QUERYSTRING_VARIABLES_KEY}={variables}&{Constants.Web.QUERYSTRING_OPERATIONNAME_KEY}={operation}";
            _queryStringTests.Add(new object[] { queryString, 8 });

            queryText = HttpUtility.UrlEncode("query op1_1($i: Int!){add(a: 5, b: $i)} query op2{add(a: 5, b:5)}");
            operation = HttpUtility.UrlEncode("op1_1");
            variables = HttpUtility.UrlEncode("{\"i\": 3}");
            queryString = $"?{Constants.Web.QUERYSTRING_QUERY_KEY}={queryText}&{Constants.Web.QUERYSTRING_VARIABLES_KEY}={variables}&{Constants.Web.QUERYSTRING_OPERATIONNAME_KEY}={operation}";
            _queryStringTests.Add(new object[] { queryString, 8 });
        }

        [TestCaseSource(nameof(_queryStringTests))]
        public async Task GETRequest_QueryStringTests(string queryText, int result)
        {
            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddGraphController<MathController>();
            serverBuilder.AddTransient<DefaultGraphQLHttpProcessor<GraphSchema>>();
            serverBuilder.AddGraphQL((o) =>
            {
                o.ExecutionOptions.QueryTimeout = null;
            });

            var server = serverBuilder.Build();

            using var scope = server.ServiceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<DefaultGraphQLHttpProcessor<GraphSchema>>();

            var httpContext = new DefaultHttpContext()
            {
                Response =
                {
                    Body = new MemoryStream(),
                },
                RequestServices = scope.ServiceProvider,
            };

            var request = httpContext.Request as DefaultHttpRequest;
            request.Method = "GET";
            request.QueryString = new QueryString(queryText);

            await processor.Invoke(httpContext);
            await httpContext.Response.Body.FlushAsync();

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(httpContext.Response.Body);
            var text = reader.ReadToEnd();

            var expectedResult = @"
                {
                    ""data"" : {
                        ""add"" : " + result + @"
                    }
                }";

            CommonAssertions.AreEqualJsonStrings(expectedResult, text);
            Assert.AreEqual(200, httpContext.Response.StatusCode);
        }
    }
}