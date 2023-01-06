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
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Tests.Execution.TestData.ExecutionPlanTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public partial class GeneralQueryExecutionTests
    {
        [Test]
        public async Task SingleFieldResolution_ViaPipeline_YieldsCorrectResult()
        {
            var server = new TestServerBuilder()
                .AddType<SimpleExecutionController>()
                .Build();

            var builder = server.CreateGraphTypeFieldContextBuilder<SimpleExecutionController>(
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
                .AddType<SimpleExecutionController>()
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
                .AddType<SimpleExecutionController>()
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
        public async Task WhenNoLeafValuesAreRequested_ItemIsReturnedAsNullAndPropegated()
        {
            var server = new TestServerBuilder()
                        .AddType<UnionController>()
                        .Build();

            var builder = server.CreateQueryContextBuilder();

            // retrieveUnion only returns TwoPropertyObject
            // since the declaration does not declare what to do with the value
            // it should be dropped
            builder.AddQueryText(@"query Operation1
            {
                    retrieveUnion {
                        ... on TwoPropertyObjectV2 {
                             property1
                        }
                    }
            }");
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
                        .AddType<SimpleExecutionController>()
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
        public async Task UnhandledException_InUserCode_OnActionMethod_ResultsInErrorMessageOnResponse()
        {
            var server = new TestServerBuilder()
                    .AddType<SimpleExecutionController>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                    .AddQueryText("query {  simple {  throwFromController { itemProperty } } }");

            // the controller method returns an object with a single mapped method "ExecuteThrow" that throws an exception
            // this is executed through the graphmethod resolver (not the controller action resolver) allowing an internal
            // exception to fully bubble up (not captured by the action invoker)
            var result = await server.ExecuteQuery(builder);

            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, result.Messages.Severity);
            Assert.AreEqual(Constants.ErrorCodes.INTERNAL_SERVER_ERROR, result.Messages[0].Code);
            Assert.IsNotNull(result.Messages[0].Exception);
            Assert.AreEqual("Failure from Controller", result.Messages[0].Exception.Message);

            Assert.AreEqual("simple", result.Messages[0].Origin.Path[0].ToString());
            Assert.AreEqual("throwFromController", result.Messages[0].Origin.Path[1]);
        }

        [Test]
        public async Task UnhandledException_InUserCode_OnDirectGraphMethodCall_ResultsInMessageOnResponse()
        {
            var server = new TestServerBuilder()
                .AddType<SimpleExecutionController>()
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

        [Test]
        public async Task ObjectMethodAsField_Syncronously_ResolvesCorrectly()
        {
            var server = new TestServerBuilder()
                        .AddType<ObjectMethodController>()
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
                    .AddType<ObjectMethodController>()
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
                    .AddType<GraphIdController>()
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
                    .AddType<ListController>()
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
                    .AddType<ListController>()
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
                    .AddType<ListController>()
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

        [Test]
        public async Task InputComplexObjects_WithNoRequiredFields_Succeeds()
        {
            var server = new TestServerBuilder()
                    .AddType<ComplexInputObjectController>()
                    .Build();

            // controller accepts a complex input object with no required fields (just string values).
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("mutation  { " +
                "   addObject ( objectA: {property1: \"prop1\", property2: \"prop2\" } ) { " +
                "     property1 " +
                "     property2 " +
                "   } " +
                "}");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""addObject"" : {
                            ""property1"" : ""prop1"",
                            ""property2"" : ""prop2""
                        }
                    }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task InputWithNullableComplexChildObject_HasUndefinedForChildObject_YieldsNullChildObject()
        {
            var server = new TestServerBuilder()
                    .AddType<ComplexInputObjectController>()
                    .Build();

            // parentObj has a property called  'child' that is not passed on the query
            // the controller should recieve a default child object for the instance (not an empty one, not null)
            // and thus return the default property
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("mutation  { " +
                "      objectWithNullChild ( parentObj: {property1: \"prop1 supplied value\" } ) { " +
                "           property1 " +
                "           child {" +
                "               property2 " +
                "           }" +
                "      } " +
                "}");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""objectWithNullChild"" : {
                            ""property1"" : ""prop1 supplied value"",
                            ""child"" : {
                                ""property2"" : ""child default value""
                            }
                       }
                    }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task InputWithNullableComplexChildObject_HasNullPassedForChildObject_YieldsNullChildObject()
        {
            var server = new TestServerBuilder()
                    .AddType<ComplexInputObjectController>()
                    .Build();

            // parentObj has a property called  'child' that is passed on the query as null
            // the controller should recieve a <null> child object (not an empty one, not a default instance)
            // and thus return null
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("mutation  { " +
                "      objectWithNullChild ( parentObj: {property1: \"prop1\", child : null } ) { " +
                "           property1 " +
                "           child {" +
                "               property2 " +
                "           }" +
                "      } " +
                "}");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""objectWithNullChild"" : {
                            ""property1"" : ""prop1"",
                            ""child"" : null
                       }
                    }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task InputWithNullableComplexChildObject_WhenEmptyChildObjectSupplied_HasChildWithDefaultValues()
        {
            var server = new TestServerBuilder()
                    .AddType<ComplexInputObjectController>()
                    .Build();

            // child is passed as an empty object and has no required fields. All fields of child
            // should be initialized to null and returnable (as null)
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("mutation  { " +
                "      objectWithNullChild ( parentObj: {property1: \"prop1\", child :{} } ) { " +
                "           property1 " +
                "           child {" +
                "               property2 " +
                "           }" +
                "      } " +
                "}");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""objectWithNullChild"" : {
                            ""property1"" : ""prop1"",
                            ""child"" : {
                                 ""property2"" : null
                            }
                       }
                    }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task InputWithRequiredButNullableComplexChildObject_WhenNotSuppied_YieldsError()
        {
            var server = new TestServerBuilder()
                    .AddType<ComplexInputObjectController>()
                    .Build();

            // parentObj has a required property called  'child' that is not passed on the query
            // a error should be thrown as its required on a query
            // breaks rule 5.6.4
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("mutation  { " +
                "      objectWithRequiredButNullableChild ( parentObj: {property1: \"prop1\" } ) { " +
                "           property1 " +
                "           child {" +
                "               property2 " +
                "           }" +
                "      } " +
                "}");

            var result = await server.ExecuteQuery(builder);

            Assert.IsFalse(result.Messages.IsSucessful);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_DOCUMENT, result.Messages[0].Code);
            Assert.AreEqual("5.6.4", result.Messages[0].MetaData["Rule"]);
        }

        [Test]
        public async Task InputWithRequiredComplexChildObjectThatAllowsNull_SuppliesNullForChildObject_YieldsSuccess()
        {
            var server = new TestServerBuilder()
                    .AddType<ComplexInputObjectController>()
                    .Build();

            // parentObj has a property called  'child' that is nullable and
            // passed as null on the query
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("mutation  { " +
                "      objectWithRequiredButNullableChild ( parentObj: {property1: \"prop1\", child : null } ) { " +
                "           property1 " +
                "           child {" +
                "               property2 " +
                "           }" +
                "      } " +
                "}");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""objectWithRequiredButNullableChild"" : {
                            ""property1"" : ""prop1"",
                            ""child"" : null
                       }
                    }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task InputWithRequiredeComplexChildObject_SuppliesEmptyObjectForChild_YieldsSuccess()
        {
            var server = new TestServerBuilder()
                    .AddType<ComplexInputObjectController>()
                    .Build();

            // parentObj has a property called  'child' that is passed as with no params on the query
            // this is fine and all child properties should be set to their default values
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("mutation  { " +
                "      objectWithRequiredButNullableChild ( parentObj: {property1: \"prop1\", child : {} } ) { " +
                "           property1 " +
                "           child {" +
                "               property2 " +
                "           }" +
                "      } " +
                "}");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""objectWithRequiredButNullableChild"" : {
                            ""property1"" : ""prop1"",
                            ""child"" : {
                                 ""property2"" : null
                            }
                       }
                    }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task InputWithNonNullableComplexChildObject_HasEmptyForChildObject_YieldsSuccess()
        {
            var server = new TestServerBuilder()
                    .AddType<ComplexInputObjectController>()
                    .Build();

            // parentObj has a property called  'child' that is passed as empty on the query
            // should succeed, all child properties are optional
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("mutation  { " +
                "      objectWithNonNullChild ( parentObj: {property1: \"prop1\", child : {} } ) { " +
                "           property1 " +
                "           child {" +
                "               property2 " +
                "           }" +
                "      } " +
                "}");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""objectWithNonNullChild"" : {
                            ""property1"" : ""prop1"",
                            ""child"" : {
                                 ""property2"" : null
                            }
                       }
                    }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task InputWithNonNullableComplexChildObject_HasNullChildObject_YieldsError()
        {
            var server = new TestServerBuilder()
                    .AddType<ComplexInputObjectController>()
                    .Build();

            // parentObj has a property called  'child' that is passed as <null> on the query
            // but is marked as non-null in the graph
            // breaks rule 5.6.1   (cannot coerce <null> to Child!)
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("mutation  { " +
                "      objectWithNonNullChild ( parentObj: {property1: \"prop1\", child : null } ) { " +
                "           property1 " +
                "           child {" +
                "               property2 " +
                "           }" +
                "      } " +
                "}");

            var result = await server.ExecuteQuery(builder);

            Assert.IsFalse(result.Messages.IsSucessful);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_DOCUMENT, result.Messages[0].Code);
            Assert.AreEqual("5.6.1", result.Messages[0].MetaData["Rule"]);
        }

        [Test]
        public async Task ControllerReturnsAnarrayForAnEnumerableDeclarartion_YieldsCorrectResult()
        {
            var server = new TestServerBuilder()
                    .AddType<ArrayAsEnumerableController>()
                    .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query  { " +
                "      retrieveData () { " +
                "           property1 " +
                "           property2 " +
                "      } " +
                "}");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""retrieveData"" : [
                            {
                                ""property1"" : ""1A"",
                                ""property2"" : 2
                            },
                            {
                                ""property1"" : ""1B"",
                                ""property2"" : 3
                            }
                        ]
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task ControllerReturnsAnArrayForAnArrayThroughGraphActionDeclarartion_YieldsCorrectResult()
        {
            var server = new TestServerBuilder()
                    .AddType<ArrayThroughGraphActionAsEnumerableController>()
                    .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query  { " +
                "      retrieveData () { " +
                "           property1 " +
                "           property2 " +
                "      } " +
                "}");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""retrieveData"" : [
                            {
                                ""property1"" : ""1A"",
                                ""property2"" : 2
                            },
                            {
                                ""property1"" : ""1B"",
                                ""property2"" : 3
                            }
                        ]
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task ControllerReturnsAnArrayForAnArrayDeclaration_YieldsCorrectResult()
        {
            var server = new TestServerBuilder()
                    .AddType<ArrayThroughArrayDeclarationController>()
                    .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query  { " +
                "      retrieveData () { " +
                "           property1 " +
                "           property2 " +
                "      } " +
                "}");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""retrieveData"" : [
                            {
                                ""property1"" : ""1A"",
                                ""property2"" : 2
                            },
                            {
                                ""property1"" : ""1B"",
                                ""property2"" : 3
                            }
                        ]
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task FlatArray_AsProperty_ResolvesDataCorrectly()
        {
            var server = new TestServerBuilder()
                    .AddType<ArrayOnReturnObjectPropertyController>()
                    .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query  { " +
                "      retrieveData () { " +
                "           propertyA " +
                "           propertyB {" +
                "               property1" +
                "               property2" +
                "           }" +
                "      } " +
                "}");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""retrieveData"" : [
                            {
                                ""propertyA"" : ""AA"",
                                ""propertyB"" : [
                                    {
                                        ""property1"" : ""1A"",
                                        ""property2"" : 2
                                    },
                                    {
                                        ""property1"" : ""1B"",
                                        ""property2"" : 3
                                    }
                                ]
                            },
                        ]
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task FlatArray_AsObjectMethod_ResolvesDataCorrectly()
        {
            var server = new TestServerBuilder()
                    .AddType<ArrayOnReturnObjectMethodController>()
                    .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query  { " +
                "      retrieveData () { " +
                "           propertyA " +
                "           moreData() {" +
                "               property1" +
                "               property2" +
                "           }" +
                "      } " +
                "}");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""retrieveData"" : {
                            ""propertyA"" : ""AA"",
                            ""moreData"" : [
                                {
                                    ""property1"" : ""1A"",
                                    ""property2"" : 2
                                },
                                {
                                    ""property1"" : ""1B"",
                                    ""property2"" : 3
                                }
                            ]
                         }
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task FlatArray_OfScalars_ResolvesDataCorrectly()
        {
            var server = new TestServerBuilder()
                    .AddType<ArrayScalarController>()
                    .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query  { " +
                "      retrieveData () " +
                "}");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""retrieveData"" : [1, 3, 5]
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task KeyValuePair_ResolvesDataCorrectly()
        {
            var server = new TestServerBuilder()
                    .AddType<KeyValuePairController>()
                    .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query  { " +
                "      retrieveData () {" +
                "           key         " +
                "           value       " +
                "      }" +
                "}");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""retrieveData"" : [
                            {""key"" : ""key1"", ""value"" : 1 },
                            {""key"" : ""key2"", ""value"" : 2 }
                       ]
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task TypeExtension_OnValueType_ResolvesDataCorrectly()
        {
            var server = new TestServerBuilder()
         .AddType<TypeExtensionKeyValuePairController>()
         .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query  { " +
                "      retrieveData () {" +
                "           key         " +
                "           value       " +
                "           value2      " +
                "      }" +
                "}");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""retrieveData"" : [
                            {""key"" : ""key1"", ""value"" : 1, ""value2"" : ""1ABC"" },
                            {""key"" : ""key2"", ""value"" : 2, ""value2"" : ""2ABC"" }
                       ]
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task TypeExtension_OnGeneralObject_ResolvesDataCorrectly()
        {
            var server = new TestServerBuilder()
         .AddType<TypeExtensionOnTwoPropertyObjectController>()
         .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query  { " +
                "      retrieveData () {" +
                "           property1      " +
                "           property2      " +
                "           property3      " +
                "      }" +
                "}");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""retrieveData"" : [
                            {""property1"" : ""Prop1"", ""property2"" : 1, ""property3"" : ""1ABC"" },
                            {""property1"" : ""Prop2"", ""property2"" : 2, ""property3"" : ""2ABC"" }
                       ]
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }
    }
}