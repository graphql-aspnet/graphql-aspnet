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
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class GeneralQueryExecutionRuntimeFieldTests
    {
        [Test]
        public async Task BasicMappedQuery_ExecutesMethod()
        {
            var ServerBuilder = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapQuery("/field1/field2", (int a, int b) =>
                    {
                        return a + b;
                    });
                });

            var server = ServerBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 5, b: 33 } } }");

            // supply no values, allowing the defaults to take overand returning the single
            // requested "Property1" with the default string defined on the method.
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
    }
}