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
    using GraphQL.AspNet.Tests.Execution.ExecutionPlanTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    // So many general integration tests this is a second file
    [TestFixture]
    public partial class GeneralQueryExecutionTests
    {
        [Test]
        public async Task ArrayofScalarOnInput_ResolvesCorrectly()
        {
            var server = new TestServerBuilder()
                    .AddGraphQL(o =>
                    {
                        o.AddGraphType<InputArrayScalarController>();
                        o.ResponseOptions.ExposeExceptions = true;
                    })
                    .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query { " +
                "      sumArray (items: [1, 2, 3])" +
                "}");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""sumArray"" : 6
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task Struct_OnInputParmaeter_ResolvesCorrectly()
        {
            var server = new TestServerBuilder()
                    .AddGraphQL(o =>
                    {
                        o.AddGraphType<InputStructController>();
                        o.ResponseOptions.ExposeExceptions = true;
                    })
                    .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query { " +
                "      parsePerson (person: {firstName: \"Bob\", lastName: \"Smith\"})" +
                "}");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""parsePerson"" : true
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task Struct_ArrayOnInputParameter_ResolvesCorrectly()
        {
            var server = new TestServerBuilder()
                    .AddGraphQL(o =>
                    {
                        o.AddGraphType<InputStructController>();
                        o.ResponseOptions.ExposeExceptions = true;
                    })
                    .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query { " +
                "      parsePersonArray (items: [{firstName: \"first0\", lastName: \"last0\"},{firstName: \"first1\", lastName: \"last1\"},{firstName: \"first2\", lastName: \"last2\"}])" +
                "}");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""parsePersonArray"" : true
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task InputObjectArray_ResolvesCorrectly()
        {
            var server = new TestServerBuilder()
                    .AddGraphQL(o =>
                    {
                        o.AddGraphType<InputObjectArrayController>();
                        o.ResponseOptions.ExposeExceptions = true;
                    })
                    .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query { " +
                "      parseArray (items: [{property1: \"key1\", property2: 1}, {property1: \"key2\", property2: 2}])" +
                "}");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""parseArray"" : true
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }
    }
}