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
    using System.IO;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Web.CancelTokenTestData;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class HttpContextInjectionTests
    {
        [Test]
        public async Task WhenHttpContextIsFound_ItsInjectedAsExpected()
        {
            DefaultHttpContext httpContext = null;
            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddGraphController<MathController>();
            serverBuilder.AddTransient<DefaultGraphQLHttpProcessor<GraphSchema>>();
            serverBuilder.AddGraphQL((o) =>
            {
                o.ExecutionOptions.QueryTimeout = null;
                o.MapQuery("field", (HttpContext context) =>
                {
                    if (context != null
                    && context.Items.ContainsKey("Test1")
                    && context.Items["Test1"].ToString() == "Value1")
                    {
                        return 1;
                    }

                    return 0;
                });
            });

            var server = serverBuilder.Build();

            using var scope = server.ServiceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<DefaultGraphQLHttpProcessor<GraphSchema>>();

            httpContext = new DefaultHttpContext()
            {
                Response =
                {
                    Body = new MemoryStream(),
                },
                RequestServices = scope.ServiceProvider,
            };

            var request = httpContext.Request;
            request.Method = "GET";
            request.QueryString = new QueryString("?query=query { field }");

            httpContext.Items.Add("Test1", "Value1");

            await processor.InvokeAsync(httpContext);
            await httpContext.Response.Body.FlushAsync();

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(httpContext.Response.Body);
            var text = reader.ReadToEnd();

            var expectedResult = @"
                {
                    ""data"" : {
                        ""field"" : 1
                    }
                }";

            CommonAssertions.AreEqualJsonStrings(expectedResult, text);
            Assert.AreEqual(200, httpContext.Response.StatusCode);
        }
    }
}