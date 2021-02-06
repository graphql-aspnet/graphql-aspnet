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
    using System;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Directives.Global;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Execution.ExecutionPlanTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class GeneralQueryExecutionTests
    {
        [Test]
        public async Task SingleFieldResolution_ViaPipeline_YieldsCorrectResult()
        {
            var server = new TestServerBuilder()
                .AddGraphType<SimpleExecutionController>()
                .Build();

            var builder = server.CreateFieldContextBuilder<SimpleExecutionController>(
                nameof(SimpleExecutionController.SimpleQueryMethod));
            builder.AddInputArgument("arg1", "my value");
            builder.AddInputArgument("arg2", 15L);

            builder.AddSourceData(new object());

            var context = builder.CreateExecutionContext();
            await server.ExecuteField(context);

            var data = context.Result as TwoPropertyObject;
            Assert.IsNotNull(data);
            Assert.AreEqual("my value", data.Property1);
        }

        [Test]
        public async Task SingleFieldResolution_NoSuppliedVariables_ResolvePathThroughController_CallsResolutions_Correctly()
        {
            var server = new TestServerBuilder()
                .AddGraphType<SimpleExecutionController>()
                .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query Operation1($var1: String = ""not-default string"") {
                                simple{
                                    simpleQueryMethod (arg1: $var1) {
                                        property1} } }");
            builder.AddOperationName("Operation1");

            // supply no values, allowing the defaults to take overand returning the single
            // requested "Property1" with the default string defined on the method.
            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""simple"": {
                            ""simpleQueryMethod"" : {
                                ""property1"" : ""not-default string""
                            }
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task TypeNameMetaField_OnObject_returnsValue()
        {
            var server = new TestServerBuilder()
                .AddGraphType<SimpleExecutionController>()
                .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText("query Operation1{  simple {  simpleQueryMethod { property1 __typename} } }");
            builder.AddOperationName("Operation1");

            // supply no values, allowing the defaults to take overand returning the single
            // requested "Property1" with the default string defined on the method.
            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""simple"": {
                                ""simpleQueryMethod"" : {
                                        ""property1"" : ""default string"",
                                        ""__typename"" : ""TwoPropertyObject""
                                }
                        }
                    }
                 }",
                result);
        }

        [Test]
        public async Task SimpleResolution_WithDirective_CallsDirectiveCorrectly()
        {
            var server = new TestServerBuilder()
                   .AddGraphType<SimpleExecutionController>()
                   .AddGraphType<CallTestDirective>()
                   .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query Operation1{  simple {  simpleQueryMethod @callTest(arg: 3) { property1} } }")
                .AddOperationName("Operation1");

            // supply no values, allowing the defaults to take overand returning the single
            // requested "Property1" with the default string defined on the method.
            var result = await server.ExecuteQuery(builder);
            Assert.AreEqual(0, result.Messages.Count);
            Assert.AreEqual(1, CallTestDirective.TotalCalls);
        }

        [Test]
        public async Task DirectiveExecution_MaintainsRequestMetaDataCollection_CallsDirectiveCorrectly()
        {
            var server = new TestServerBuilder()
            .AddGraphType<SimpleExecutionController>()
                .AddGraphType<MetaDataShareDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText("query Operation1{  simple {  simpleQueryMethod @metaDataShare(arg: 3) { property1} } }");
            builder.AddOperationName("Operation1");

            // supply no values, allowing the defaults to take overand returning the single
            // requested "Property1" with the default string defined on the method.
            var context = builder.Build();
            await server.ExecuteQuery(context);
            Assert.AreEqual(0, context.Messages.Count);
            Assert.IsTrue(MetaDataShareDirective.FoundInAfterCompletion);
        }

        [Test]
        public async Task WhenNoLeafValuesAreRequested_ItemIsReturnedAsNullAndPropegated()
        {
            var server = new TestServerBuilder()
                        .AddGraphType<SimpleExecutionController>()
                        .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText("query Operation1{  simple {  simpleQueryMethod  } }");
            builder.AddOperationName("Operation1");

            var result = await server.RenderResult(builder);

            CommonAssertions.AreEqualJsonStrings(
                "{ \"data\" : null }",
                result);
        }

        [Test]
        public async Task IteratingACollection_CallsResolutions_Correctly()
        {
            var server = new TestServerBuilder()
                        .AddGraphType<SimpleExecutionController>()
                        .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText("query Operation1{  simple {  collectionQueryMethod { property1 property2 } } }");
            builder.AddOperationName("Operation1");

            // supply no values, allowing the defaults to take overand returning the single
            // requested "Property1" with the default string defined on the method.
            var result = await server.RenderResult(builder);
            var output = @"{
                ""data"" : {
                    ""simple"": {
                        ""collectionQueryMethod"": [
                            { ""property1"": ""string0"", ""property2"": 0 },
                            { ""property1"": ""string1"", ""property2"": 1 },
                            { ""property1"": ""string2"", ""property2"": 2 }
                            ]
                        }
                    }
                }";

            CommonAssertions.AreEqualJsonStrings(
                output,
                result);
        }

        [Test]
        public async Task RunnerTimeout_ErrorMessagesAreSetCorrect()
        {
            var serverBuilder = new TestServerBuilder()
                        .AddGraphType<SimpleExecutionController>();

            serverBuilder.AddGraphQL(o =>
            {
                o.ExecutionOptions.QueryTimeout = TimeSpan.FromMilliseconds(15);
            });

            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query {  simple {  timedOutMethod  } }");

            var result = await server.ExecuteQuery(builder);

            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, result.Messages.Severity);
            Assert.AreEqual(Constants.ErrorCodes.OPERATION_CANCELED, result.Messages[0].Code);
        }

        [Test]
        public async Task UnhandledException_InUserCode_OnActionMethod_ResultsInErrorMessageOnResponse()
        {
            var server = new TestServerBuilder()
                    .AddGraphType<SimpleExecutionController>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                    .AddQueryText("query {  simple {  throwFromController } }");

            // the controller method returns an object with a single mapped method "ExecuteThrow" that throws an exception
            // this is executed through the graphmethod resolver (not the controller action resolver) allowing an internal
            // exception to fully bubble up (not captured by the action invoker)
            var result = await server.ExecuteQuery(builder);

            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, result.Messages.Severity);
            Assert.AreEqual(Constants.ErrorCodes.UNHANDLED_EXCEPTION, result.Messages[0].Code);
            Assert.IsNotNull(result.Messages[0].Exception);
            Assert.AreEqual("Failure from Controller", result.Messages[0].Exception.Message);

            Assert.AreEqual("simple", result.Messages[0].Origin.Path[0].ToString());
            Assert.AreEqual("throwFromController", result.Messages[0].Origin.Path[1]);
        }

        [Test]
        public async Task UnhandledException_InUserCode_OnDirectGraphMethodCall_ResultsInMessageOnResponse()
        {
            var server = new TestServerBuilder()
                .AddGraphType<SimpleExecutionController>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query {  simple {  throwable  { executeThrow } } }");

            // the controller method returns an object with a single mapped method "ExecuteThrow" that throws an exception
            // this is executed through the graphmethod resolver (not the controller action resolver) allowing an internal
            // exception to fully bubble up (not captured by the action invoker)
            var result = await server.ExecuteQuery(builder);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, result.Messages.Severity);
            Assert.AreEqual(Constants.ErrorCodes.UNHANDLED_EXCEPTION, result.Messages[0].Code);
            Assert.IsNotNull(result.Messages[0].Exception);
            Assert.AreEqual("Failure from ObjectWithThrowMethod", result.Messages[0].Exception.Message);
        }

        [TestCase(true, "{ \"data\" : { \"simple\": {\"simpleQueryMethod\" : { \"property1\" : \"default string\" } } } }")]
        [TestCase(false, "{ \"data\" : { \"simple\": {\"simpleQueryMethod\" : { \"property1\" : \"default string\", \"property2\": 5 } } } }")]
        public async Task SkipDirective_ResponseAppropriately(bool skipValue, string expectedJson)
        {
            var server = new TestServerBuilder()
            .AddGraphType<SimpleExecutionController>()
                .AddGraphType<SkipDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    "query Operation1{  simple {  simpleQueryMethod { property1, property2 @skip(if: " +
                    skipValue.ToString().ToLower() +
                    ") } } }")
                .AddOperationName("Operation1");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedJson,
                result);
        }

        [TestCase(true, "{ \"data\" :{ \"simple\": {\"simpleQueryMethod\" : { \"property1\" : \"default string\", \"property2\": 5 } } } }")]
        [TestCase(false, "{ \"data\" :{ \"simple\": {\"simpleQueryMethod\" : { \"property1\" : \"default string\" } } } }")]
        public async Task IncludeDirective_ResponseAppropriately(bool includeValue, string expectedJson)
        {
            var server = new TestServerBuilder()
             .AddGraphType<SimpleExecutionController>()
                 .AddGraphType<IncludeDirective>()
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

        [Test]
        public async Task ObjectMethodAsField_Syncronously_ResolvesCorrectly()
        {
            var server = new TestServerBuilder()
                        .AddGraphType<ObjectMethodController>()
                        .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query  { objects {  retrieveObject { syncMethod} } }");

            // call the child SyncMethod (which is a syrconous method described as a field on an object, not a controller)
            // ensure it returns a value
            var result = await server.RenderResult(builder);

            var expectedOutput =
                        @"{
                            ""data"" : {
                                ""objects"" : {
                                    ""retrieveObject"" : {
                                        ""syncMethod"" : 5
                                    }
                                }
                            }
                        }";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task ObjectMethodAsField_Asyncronously_ResolvesCorrectly()
        {
            var server = new TestServerBuilder()
                    .AddGraphType<ObjectMethodController>()
                    .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query  { objects {  retrieveObject { asyncMethod} } }");

            // call the child AsyncMethod (which is a asycrnous method described as a field on an object, not a controller)
            // ensure it returns a value
            var result = await server.RenderResult(builder);

            var expectedOutput =
                @"{
                    ""data"": {
                       ""objects"" : {
                                ""retrieveObject"" : {
                                    ""asyncMethod"" : 8
                                }
                            }
                        }
                   }";
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task CustomScalar_BecomesValidCoreScalarValue()
        {
            var server = new TestServerBuilder()
                    .AddGraphType<GraphIdController>()
                    .Build();

            // retrieveid returns a "GraphId"
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query  { retrieveId }");

            // ensure its a string when printed
            var result = await server.RenderResult(builder);

            var expectedOutput =
                @"{
                    ""data"": {
                       ""retrieveId"" : ""abc123""
                    }
                  }";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task ListOfScalars_ProjectsIntoAListCorrectly()
        {
            // test the case where a leaf graph type is returne das a
            // list List<int>, List<string>, List<SomeEnum> etc.
            // ensure the data items are properly projected into a list
            // when rendered
            var server = new TestServerBuilder()
                    .AddGraphType<ListController>()
                    .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query  { createIntList }");

            // ensure its a string when printed
            var result = await server.RenderResult(builder);

            var expectedOutput =
                @"{
                    ""data"": {
                       ""createIntList"" : [5, 10, 15, 20, 25, 30]
                    }
                  }";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task ListOfEnums_ProjectsIntoAListCorrectly()
        {
            // test the case where a leaf graph type is returne das a
            // list List<int>, List<string>, List<SomeEnum> etc.
            // ensure the data items are properly projected into a list
            // when rendered
            var server = new TestServerBuilder()
                    .AddGraphType<ListController>()
                    .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query  { createEnumList }");

            // ensure its a string when printed
            var result = await server.RenderResult(builder);

            var expectedOutput =
                @"{
                    ""data"": {
                       ""createEnumList"" : [""TESTVALUE1"", ""TESTVALUE2""]
                    }
                  }";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task ListOfEnums_WithAnUnlabeledValue_Fails()
        {
            var server = new TestServerBuilder()
                    .AddGraphType<ListController>()
                    .Build();

            // controller returns a list of {Value1, -3}
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query  { createEnumListWithInValidValue }");

            // ensure hte -3 wasnt accepted as valid for the enum being
            // returned
            var result = await server.ExecuteQuery(builder);
            Assert.IsFalse(result.Messages.IsSucessful);
            Assert.AreEqual(1, result.Messages.Count);
        }
    }
}