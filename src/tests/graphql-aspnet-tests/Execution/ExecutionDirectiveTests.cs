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

        [Test]
        public async Task VariableBooleanDirectiveInputArgument_ResolvesCorrectlyAndIsReceviedByTheDirective()
        {
            var server = new TestServerBuilder()
                .AddGraphController<DirectiveTestController>()
                .AddDirective<ToSarcasticCaseExecutionDirective>()
                .Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(@"query ($startOnLower: Boolean!) {
                    retrieveObject {
                       property1 @toSarcasticCase(startOnLowerCase: $startOnLower)
                    }
                }");

            queryContext.AddVariableData(@"
                {
                ""startOnLower"" : true
                }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""retrieveObject"" : {
                        ""property1"" : ""vAlUe1""
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);

            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task ExecutionDirectiveAddsAFieldPostProcessor_ForSingleField_ProcessorIsExecutedAsExpected()
        {
            var directiveInstance = new SampleDirective();
            var builder = new TestServerBuilder();
            builder.AddSingleton(directiveInstance);
            builder.AddGraphController<DirectiveTestController>()
                  .AddDirective<ToUpperCaseExecutionDirective>();

            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(@"query {
                    retrieveObject {
                       property1 @toUpperCaseExecution
                    }
                }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""retrieveObject"" : {
                        ""property1"" : ""VALUE1""
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);

            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task ExecutionDirectiveAddsAFieldPostProcessor_ForTypeExtensionField_ProcessorIsExecutedAsExpected()
        {
            var directiveInstance = new SampleDirective();
            var builder = new TestServerBuilder();
            builder.AddSingleton(directiveInstance);
            builder.AddGraphController<DirectiveTestController>()
                  .AddDirective<ToUpperCaseExecutionDirective>();

            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(@"query {
                    retrieveObject {
                       property3 @toUpperCaseExecution
                    }
                }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""retrieveObject"" : {
                        ""property3"" : ""VALUE1 PROP 3""
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);

            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task ExecutionDirectiveAddsAFieldPostProcessor_ForBatchExtensionField_ProcessorIsExecutedAsExpected()
        {
            var directiveInstance = new SampleDirective();
            var builder = new TestServerBuilder();
            builder.AddSingleton(directiveInstance);
            builder.AddGraphController<DirectiveTestController>()
                  .AddDirective<AdjustBatchDataDirective>();

            var server = builder.Build();

            // field "child" is executed as a batch extension
            // @adjustBatchData will upper case any child items found
            // via a batch
            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(@"query {
                    retrieveObjects {
                       child @adjustBatchData {
                            property1
                        }
                    }
                }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""retrieveObjects"" : [
                        {  ""child"" : { ""property1"" : ""CHILD PROP VALUE0"" } },
                        {  ""child"" : { ""property1"" : ""CHILD PROP VALUE1"" } },
                        {  ""child"" : { ""property1"" : ""CHILD PROP VALUE2"" } },
                    ]
                }
            }";

            var result = await server.RenderResult(queryContext);

            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [TestCase("query @recordLocation  { retrieveObject { property1 } }", null, DirectiveLocation.QUERY)]
        [TestCase("mutation @recordLocation  { mutateObject { property1 } }", null, DirectiveLocation.MUTATION)]
        [TestCase("query { retrieveObject { property1 @recordLocation } }", null, DirectiveLocation.FIELD)]
        [TestCase("query { retrieveObject { ... frag1 } } fragment frag1 on TwoPropertyObject @recordLocation { property1 } ", null, DirectiveLocation.FRAGMENT_DEFINITION)]
        [TestCase("query { retrieveObject { ... frag1 } } fragment frag1 on TwoPropertyObject { property1 @recordLocation } ", null, DirectiveLocation.FIELD)]
        [TestCase("query { retrieveObject { ... frag1 @recordLocation } } fragment frag1 on TwoPropertyObject { property1 } ", null, DirectiveLocation.FRAGMENT_SPREAD)]
        [TestCase("query { retrieveObject { ... @recordLocation { property1 } } }", null, DirectiveLocation.INLINE_FRAGMENT)]
        [TestCase("query ($id: String! @recordLocation ){ retrieveSingleObject (id: $id) {  property1  } }", @"{ ""id"" : ""bob"" }", DirectiveLocation.VARIABLE_DEFINITION)]
        [TestCase("query ($id: String = \"jane\" @recordLocation ){ retrieveSingleObject (id: $id) {  property1  } }", @"{ ""id"" : ""bob"" }", DirectiveLocation.VARIABLE_DEFINITION)]

        public async Task ExpectedDirectiveLocationIsAnExpectedValue(string queryText, string variables, DirectiveLocation expectedLocation)
        {
            var directiveInstance = new DirectiveLocationRecorderDirective();

            var builder = new TestServerBuilder();
            builder.AddSingleton(directiveInstance);
            builder.AddGraphController<DirectiveTestController>()
                  .AddDirective<DirectiveLocationRecorderDirective>();

            var server = builder.Build();

            // field "child" is executed as a batch extension
            // @adjustBatchData will upper case any child items found
            // via a batch
            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(queryText);
            if (variables != null)
                queryContext.AddVariableData(variables);

            var result = await server.ExecuteQuery(queryContext);

            Assert.IsNotNull(result.Data);
            Assert.AreEqual(0, result.Messages.Count);

            Assert.AreEqual(expectedLocation, directiveInstance.RecordedLocation);
        }
    }
}