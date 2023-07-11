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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Controllers.ActionResults;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Tests.Execution.TestData.RuntimeFieldTest;
    using GraphQL.AspNet.Tests.Execution.TestData.RuntimeFieldTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class GeneralQueryExecutionRuntimeFieldTests
    {
        [Test]
        public async Task BasicMappedQuery_ExecutesMethod()
        {
            var serverBuilder = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapQuery("/field1/field2", (int a, int b) =>
                    {
                        return a + b;
                    });
                });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 5, b: 33 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 38
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task BasicMappedQuery_AddingResolverAfter_ExecutesMethod()
        {
            var serverBuilder = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapQuery("/field1/field2")
                        .AddResolver((int a, int b) =>
                        {
                            return a + b;
                        });
                });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 5, b: 33 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 38
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task MappedQuery_ViaGroup_ExecutesMethod()
        {
            var serverBuilder = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    var group = o.MapQueryGroup("/field1");
                    group.MapField("/field2", (int a, int b) =>
                    {
                        return a + b;
                    });
                });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 5, b: 33 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 38
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task BasicMappedMutation_ExecutesMethod()
        {
            var serverBuilder = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapMutation("/field1/field2", (int a, int b) =>
                    {
                        return a + b;
                    });
                });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"mutation { field1 { field2(a: 4, b: 33 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 37
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task BasicMappedMutation_AddingResolverAfter_ExecutesMethod()
        {
            var serverBuilder = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapMutation("/field1/field2")
                    .AddResolver((int a, int b) =>
                    {
                        return a + b;
                    });
                });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"mutation { field1 { field2(a: 4, b: 33 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 37
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task StaticMethodMappedDelegate_ThrowsValidationException()
        {
            var serverBuilder = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapQuery("/field1/field2", SampleDelegatesForMinimalApi.StaticMethod);
                });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 4, b: 37 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 41
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task InstanceMethodMappedDelegate_ExecutesMethod()
        {
            var data = new SampleDelegatesForMinimalApi();
            var serverBuilder = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapQuery("/field1/field2", data.InstanceMethod);
                });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 4, b: 37 } } }");

            // supply no values, allowing the defaults to take overand returning the single
            // requested "Property1" with the default string defined on the method.
            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 41
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task BasicMappedQuery_ReturningActionResult_ResolvesCorrectly()
        {
            var serverBuilder = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapQuery("/field1/field2")
                        .AddResolver<int>((int a, int b) =>
                        {
                            return GraphActionResult.Ok(a + b);
                        });
                });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 5, b: 33 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 38
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task BasicMappedQuery_WithExplicitlyDeclaredInjectedService_ReturningValueResult_ResolvesCorrectly()
        {
            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddTransient<IInjectedService, InjectedService>();

            serverBuilder.AddGraphQL(o =>
            {
                o.MapQuery("/field1/field2")
                    .AddResolver((int a, int b, [FromServices] IInjectedService injectedService) =>
                    {
                        // injected srvice returns 23
                        return a + b + injectedService.FetchValue();
                    });
            });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 6, b: 10 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 39
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task BasicMappedQuery_WithExplicitlyDeclaredInjectedService_ReturningActionResult_ResolvesCorrectly()
        {
            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddTransient<IInjectedService, InjectedService>();

            serverBuilder.AddGraphQL(o =>
            {
                o.MapQuery("/field1/field2")
                    .AddResolver<int>((int a, int b, [FromServices] IInjectedService injectedService) =>
                    {
                        // injected srvice returns 23
                        return GraphActionResult.Ok(a + b + injectedService.FetchValue());
                    });
            });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 5, b: 33 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 61
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task BasicMappedQuery_WithImplicitlyDeclaredInjectedService_ReturningValueResult_ResolvesCorrectly()
        {
            Assert.Inconclusive("Need to finish this test");
            return;

            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddTransient<IInjectedService, InjectedService>();

            serverBuilder.AddGraphQL(o =>
            {
                o.MapQuery("/field1/field2")
                    .AddResolver((int a, int b, IInjectedService injectedService) =>
                    {
                        // injected srvice returns 23
                        return a + b + injectedService.FetchValue();
                    });
            });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 6, b: 10 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 39
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task BasicMappedQuery_WithImplicitlyDeclaredInjectedService_ReturningActionResult_ResolvesCorrectly()
        {
            Assert.Inconclusive("Need to finish this test");
            return;

            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddTransient<IInjectedService, InjectedService>();

            serverBuilder.AddGraphQL(o =>
            {
                o.MapQuery("/field1/field2")
                    .AddResolver<int>((int a, int b, IInjectedService injectedService) =>
                    {
                        // injected srvice returns 23
                        return GraphActionResult.Ok(a + b + injectedService.FetchValue());
                    });
            });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 5, b: 33 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 61
                        }
                    }
                }",
                result);
        }
    }
}