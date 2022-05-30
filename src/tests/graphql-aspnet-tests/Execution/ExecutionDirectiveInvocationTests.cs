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
    using GraphQL.AspNet.Tests.Execution.ExecutionDirectiveInvocationTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class ExecutionDirectiveInvocationTests
    {
        [Test]
        public async Task WhenDirectiveReplacesResolvedResult_NewValueIsUsed()
        {
            var server = new TestServerBuilder()
                .AddGraphController<ExecutionDirectiveTestController>()
                .AddDirective<StringValueReplacementDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query { " +
                "      retrieveObject { property1 @stringValueReplacement } " +
                "}");

            // the controller sets "Prop one value" to property 1
            // the directive should replace it with the REPLACED STIRNG
            var expectedOutput =
                @"{
                    ""data"": {
                       ""retrieveObject"" : {
                            ""property1"" : ""REPLACED STRING""
                        }
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }
    }
}