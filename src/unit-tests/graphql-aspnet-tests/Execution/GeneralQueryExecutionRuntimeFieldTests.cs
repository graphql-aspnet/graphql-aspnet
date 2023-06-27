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
    using GraphQL.AspNet.Tests.Execution.TestData.RuntimeFieldTest;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
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
    }
}