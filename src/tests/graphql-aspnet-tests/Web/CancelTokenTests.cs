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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.AspNet.Tests.Web.CancelTokenTestData;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class CancelTokenTests
    {
        [Test]
        public async Task ControllerActionWithCancelToken_WithNoRuntimeTimout_RecievesTheHttpToken()
        {
            var mathController = new MathController();

            var serverBuilder = new TestServerBuilder();

            serverBuilder.AddSingleton(mathController);
            serverBuilder.AddGraphController<MathController>();
            serverBuilder.AddTransient<DefaultGraphQLHttpProcessor<GraphSchema>>();
            serverBuilder.AddGraphQL((o) =>
            {
                o.ExecutionOptions.QueryTimeout = null;
            });

            var server = serverBuilder.Build();

            using var scope = server.ServiceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<DefaultGraphQLHttpProcessor<GraphSchema>>();

            var requestData = new Dictionary<string, string>()
            {
                { "query", "{ add(a: 5, b: 3) }" },
            };

            var json = JsonSerializer.Serialize(requestData);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var httpContext = new DefaultHttpContext()
            {
                Request =
                {
                    Body = stream,
                    ContentLength = stream.Length,
                },
                Response =
                {
                    Body = new MemoryStream(),
                },
            };

            var cancelSource = new CancellationTokenSource();

            httpContext.RequestAborted = cancelSource.Token;
            httpContext.Request.Method = "POST";
            httpContext.RequestServices = scope.ServiceProvider;

            await processor.Invoke(httpContext);
            await httpContext.Response.Body.FlushAsync();

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(httpContext.Response.Body);
            var text = reader.ReadToEnd();

            var expectedResult = @"
                {
                    ""data"" : {
                        ""add"" : 8
                    }
                }";

            Assert.AreEqual(cancelSource.Token, mathController.ReceivedToken);
            CommonAssertions.AreEqualJsonStrings(expectedResult, text);
            Assert.AreEqual(200, httpContext.Response.StatusCode);
        }

        [Test]
        public async Task ControllerWithCancelToken_WithRuntimeTimeout_ReceivesARealCancelToken_ThatIsNotTheHttpToken()
        {
            var mathController = new MathController();

            var serverBuilder = new TestServerBuilder();

            serverBuilder.AddSingleton(mathController);
            serverBuilder.AddGraphController<MathController>();
            serverBuilder.AddTransient<DefaultGraphQLHttpProcessor<GraphSchema>>();
            serverBuilder.AddGraphQL((o) =>
            {
                o.ExecutionOptions.QueryTimeout = TimeSpan.FromSeconds(5);
            });

            var server = serverBuilder.Build();

            using var scope = server.ServiceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<DefaultGraphQLHttpProcessor<GraphSchema>>();

            var requestData = new Dictionary<string, string>()
            {
                { "query", "{ add(a: 5, b: 3) }" },
            };

            var json = JsonSerializer.Serialize(requestData);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var httpContext = new DefaultHttpContext()
            {
                Request =
                {
                    Body = stream,
                    ContentLength = stream.Length,
                },
                Response =
                {
                    Body = new MemoryStream(),
                },
            };

            var cancelSource = new CancellationTokenSource();

            httpContext.RequestAborted = cancelSource.Token;
            httpContext.Request.Method = "POST";
            httpContext.RequestServices = scope.ServiceProvider;

            await processor.Invoke(httpContext);
            await httpContext.Response.Body.FlushAsync();

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(httpContext.Response.Body);
            var text = reader.ReadToEnd();

            var expectedResult = @"
                {
                    ""data"" : {
                        ""add"" : 8
                    }
                }";

            Assert.AreNotEqual(default(CancellationToken), mathController.ReceivedToken);
            Assert.AreNotEqual(cancelSource.Token, mathController.ReceivedToken);
            CommonAssertions.AreEqualJsonStrings(expectedResult, text);
            Assert.AreEqual(200, httpContext.Response.StatusCode);
        }
    }
}