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
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Execution.ExecutionDirectiveTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class ExecutionDirectiveTests
    {
        [Test]
        public async Task DirectiveInputArgument_ResolvesCorrectlyAndIsReceviedByTheDirective()
        {
            var directiveInstance = new SampleDirective();
            var builder = new TestServerBuilder();
            builder.AddSingleton(directiveInstance);
            builder.AddGraphController<DirectiveTestController>()
                  .AddDirective<SampleDirective>();

            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(@"{
                    retrieveObject @sample(arg1: ""arg1Value"") {
                       property1
                    }
                }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""retrieveObject"" : {
                        ""property1"" : ""value1""
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);

            CommonAssertions.AreEqualJsonStrings(expectedJson, result);

            Assert.AreEqual(1, directiveInstance.ValuesReceived.Count);
            Assert.AreEqual(DirectiveLocation.FIELD, directiveInstance.ValuesReceived[0].Location);
            Assert.AreEqual("arg1Value", directiveInstance.ValuesReceived[0].Value);
        }

        [Test]
        public async Task VariableDirectiveInputArgument_ResolvesCorrectlyAndIsReceviedByTheDirective()
        {
            var directiveInstance = new SampleDirective();
            var builder = new TestServerBuilder();
            builder.AddSingleton(directiveInstance);
            builder.AddGraphController<DirectiveTestController>()
                  .AddDirective<SampleDirective>();

            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(@"query ($arg1: String!) {
                    retrieveObject @sample(arg1: $arg1) {
                       property1
                    }
                }");

            queryContext.AddVariableData(@"
                {
                ""arg1"" : ""variableProvidedValue""
                }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""retrieveObject"" : {
                        ""property1"" : ""value1""
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);

            CommonAssertions.AreEqualJsonStrings(expectedJson, result);

            Assert.AreEqual(1, directiveInstance.ValuesReceived.Count);
            Assert.AreEqual(DirectiveLocation.FIELD, directiveInstance.ValuesReceived[0].Location);
            Assert.AreEqual("variableProvidedValue", directiveInstance.ValuesReceived[0].Value);
        }
    }
}