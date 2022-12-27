// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Directives
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Directives.Global;
    using GraphQL.AspNet.Tests.Directives.DirectiveTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    internal class IncludeDirectiveTests
    {
        [TestCase(true, "{ \"data\" :{ \"simple\": {\"simpleQueryMethod\" : { \"property1\" : \"default string\", \"property2\": 5 } } } }")]
        [TestCase(false, "{ \"data\" :{ \"simple\": {\"simpleQueryMethod\" : { \"property1\" : \"default string\" } } } }")]
        public async Task IncludeDirective_ResponseAppropriately(bool includeValue, string expectedJson)
        {
            var server = new TestServerBuilder()
             .AddType<SimpleExecutionController>()
                 .AddType<IncludeDirective>()
                 .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    "query Operation1{  simple {  simpleQueryMethod { property1, property2 @include(if: " +
                    includeValue.ToString().ToLower() +
                    ") } } }")
                .AddOperationName("Operation1");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedJson,
                result);
        }

        [TestCase(true, false, true)]
        [TestCase(false, false, false)]
        [TestCase(false, true, false)]
        [TestCase(true, true, false)]
        public async Task IncludeDirective_ThenSkipDirective_OrderTest(bool includeIf, bool skipIf, bool fieldShouldBeIncluded)
        {
            var server = new TestServerBuilder()
         .AddType<SimpleExecutionController>()
             .AddType<IncludeDirective>()
             .AddType<SkipDirective>()
             .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    "query Operation1{  simple {  simpleQueryMethod { property1, property2 " +
                    "@include(if: " + includeIf.ToString().ToLower() + ") " +
                    "@skip(if: " + skipIf.ToString().ToLower() + ")" +
                    "} } }")
                .AddOperationName("Operation1");

            var expectedJson = @"
            {
            ""data"" : {
                ""simple"": {
                  ""simpleQueryMethod"": {
                    ""property1"": ""default string""" +
                    (fieldShouldBeIncluded ? ",\"property2\" : 5" : string.Empty) +
            @"
                  }
                }
              }
            }
            ";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedJson,
                result);
        }

        [TestCase(true, false, true)]
        [TestCase(false, true, false)]
        [TestCase(false, false, false)]
        [TestCase(true, true, false)]
        public async Task SkipDirective_ThenIncludeDirective_OrderTest(bool includeIf, bool skipIf, bool fieldShouldBeIncluded)
        {
            var server = new TestServerBuilder()
         .AddType<SimpleExecutionController>()
             .AddType<IncludeDirective>()
             .AddType<SkipDirective>()
             .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    "query Operation1{  simple {  simpleQueryMethod { property1, property2 " +
                    "@skip(if: " + skipIf.ToString().ToLower() + ")" +
                    "@include(if: " + includeIf.ToString().ToLower() + ") " +
                    "} } }")
                .AddOperationName("Operation1");

            var expectedJson = @"
            {
            ""data"" : {
                ""simple"": {
                  ""simpleQueryMethod"": {
                    ""property1"": ""default string""" +
                    (fieldShouldBeIncluded ? ",\"property2\" : 5" : string.Empty) +
            @"
                  }
                }
              }
            }
            ";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedJson,
                result);
        }
    }
}