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
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Execution.TestData.ExecutionPlanTestData;
    using GraphQL.AspNet.Tests.Framework;
    using Newtonsoft.Json.Linq;
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
                        o.AddType<InputArrayScalarController>();
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
                        o.AddType<InputStructController>();
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
                        o.AddType<InputStructController>();
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
                        o.AddType<InputObjectArrayController>();
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

        [Test]
        public async Task TypeNameOnAUnionReturn_YieldsResults()
        {
            var server = new TestServerBuilder()
                  .AddGraphQL(o =>
                  {
                      o.AddType<UnionController>();
                      o.ResponseOptions.ExposeExceptions = true;
                  })
                  .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(@"query {
                      retrieveUnion  {
                         ... on TwoPropertyObject {
                                property1
                                property2
                         }
                         __typename
                      }
                }");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""retrieveUnion"" : {
                            ""property1"" : ""prop1"",
                            ""property2"" : 5,
                            ""__typename"" : ""TwoPropertyObject""
                       }
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task FragmentSpreadOnOperationLevel_YieldsResults()
        {
            var server = new TestServerBuilder()
                  .AddGraphQL(o =>
                  {
                      o.AddType<SimpleExecutionController>();
                      o.ResponseOptions.ExposeExceptions = true;
                  })
                  .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(@"query {
                      ... frag1
                }

                fragment frag1 on Query
                {
                    simple  {
                        simpleQueryMethod {
                            property1
                        }
                        __typename
                    }
                }");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""simple"" : {
                            ""simpleQueryMethod"" :{
                                ""property1"" : ""default string"",
                           },
                           ""__typename"" : ""Query_Simple""
                        }
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task InlineFragmentOnOperationLevel_YieldsResults()
        {
            var server = new TestServerBuilder()
                  .AddGraphQL(o =>
                  {
                      o.AddType<SimpleExecutionController>();
                      o.ResponseOptions.ExposeExceptions = true;
                  })
                  .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(@"query {
                      ... {
                          simple  {
                              simpleQueryMethod {
                                    property1
                              }
                              __typename
                          }
                     }
                }

                ");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""simple"" : {
                            ""simpleQueryMethod"" :{
                                ""property1"" : ""default string"",
                           },
                           ""__typename"" : ""Query_Simple""
                        }
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task NonNullableArgumentWithDefaultValue_WhenNotSupplied_UsesDefaultValue()
        {
            var server = new TestServerBuilder()
                  .AddGraphQL(o =>
                  {
                      o.AddType<SimpleExecutionController>();
                      o.ResponseOptions.ExposeExceptions = true;
                  })
                  .Build();

            // simple.nonNullableIntArg has a argument of int with a default
            // value of 3
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(@"query {
                      ... {
                          simple  {
                              nonNullableIntArg
                          }
                     }
                }");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""simple"" : {
                            ""nonNullableIntArg"": 22
                        }
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task NonNullableInputFieldWithDefaultValue_WhenVariableSuppliedViaDefaultNull_UsesDefaultValueOfField()
        {
            // Direct Test of:
            // https://spec.graphql.org/October2021/#sec-All-Variable-Usages-are-Allowed.Allowing-optional-variables-when-default-values-exist
            var server = new TestServerBuilder()
                  .AddGraphQL(o =>
                  {
                      o.AddType<SimpleExecutionController>();
                      o.ResponseOptions.ExposeExceptions = true;
                  })
                  .Build();

            // simple.nonNullableInputField.inputFIeld.id has a argument of type Int!
            // but receives null via the default value of the variable
            // however, since the field has a default value defined (because its not required)
            // the value resolves correctly
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(@"query ($var1: Int = null){
                      ... {
                          simple  {
                              nonNullableInputField(inputField: {id: $var1 })
                          }
                     }
                }");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""simple"" : {
                            ""nonNullableInputField"": 22
                        }
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task ResolveEnumFromVariable_YieldsData()
        {
            var server = new TestServerBuilder()
                  .AddGraphQL(o =>
                  {
                      o.AddType<SimpleExecutionController>();
                      o.ResponseOptions.ExposeExceptions = true;
                  })
                  .Build();

            // simple.nonNullableInputField.inputFIeld.id has a argument of type Int!
            // but receives null via the variable
            // however, since the field has a default value defined (because its not required)
            // the value resolves correctly
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(@"query ($enumValue: ReturnValueTypeEnum!){
                    simple  {
                        numberFromEnum(numberType: $enumValue)
                    }
                }")
                .AddVariableData("{\"enumValue\" : \"SEVEN\" }");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""simple"" : {
                            ""numberFromEnum"": 7
                        }
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task SingleValueVariableInDeclaredArray_IsCoercableToArrayOfValidType()
        {
            var server = new TestServerBuilder()
             .AddGraphQL(o =>
             {
                 o.AddType<ArrayOfGraphIdController>();
                 o.ResponseOptions.ExposeExceptions = true;
             })
             .Build();

            // simple.nonNullableInputField.inputFIeld.id has a argument of type Int!
            // but receives null via the variable
            // however, since the field has a default value defined (because its not required)
            // the value resolves correctly
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                @"query ($additionalId: ID!){
                        idsAccepted(ids: [""1"", ""2"", $additionalId])
                }")
                .AddVariableData(
                @"{
                    ""additionalId"" : ""3""
                  }");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""idsAccepted"" : true
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [TestCase("Int", "3")]
        [TestCase("Int!", "3")]
        [TestCase("String", "\"3\"")]
        public async Task SingleValueVariableInDeclaredArray_OfWrongType_IsNotCoercableToArrayOfValidType(
            string variableType,
            string value)
        {
            var server = new TestServerBuilder()
             .AddGraphQL(o =>
             {
                 o.AddType<ArrayOfGraphIdController>();
                 o.ResponseOptions.ExposeExceptions = true;
             })
             .Build();

            // simple.nonNullableInputField.inputFIeld.id has a argument of type Int!
            // but receives null via the variable
            // however, since the field has a default value defined (because its not required)
            // the value resolves correctly
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                "query ($additionalId: " + variableType + @"){
                        idsAccepted(ids: [""1"", ""2"", $additionalId])
                }")
                .AddVariableData(
                @"{
                    ""additionalId"" : " + value + @"
                  }");

            var result = await server.ExecuteQuery(builder);

            Assert.IsFalse(result.Data != null);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_DOCUMENT, result.Messages[0].Code);
        }

        [Test]
        public async Task SingleValueVariableInDeclaredArray_OfWrongValue_IsNotCoercableToArrayOfValidType()
        {
            var server = new TestServerBuilder()
             .AddGraphQL(o =>
             {
                 o.AddType<ArrayOfGraphIdController>();
                 o.ResponseOptions.ExposeExceptions = true;
             })
             .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                @"query ($additionalId: ID!){ 
                        idsAccepted(ids: [""1"", ""2"", $additionalId])
                }")
                .AddVariableData(
                @"{
                    ""additionalId"" : true
                  }");

            var result = await server.ExecuteQuery(builder);

            Assert.IsFalse(result.Data != null);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_VARIABLE_VALUE, result.Messages[0].Code);
        }

        [Test]
        public async Task ResolveNullForSuppliedNumber()
        {
            var server = new TestServerBuilder()
                  .AddGraphQL(o =>
                  {
                      o.AddType<SimpleExecutionController>();
                      o.ResponseOptions.ExposeExceptions = true;
                  })
                  .Build();

            // simple.nonNullableInputField.inputFIeld.id has a argument of type Int!
            // but receives null via the variable
            // however, since the field has a default value defined (because its not required)
            // the value resolves correctly
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(@"query {
                    simple  {
                        nullableInputIntValue(number: null)
                    }
                }");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""simple"" : {
                            ""nullableInputIntValue"": null
                        }
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [TestCase(
            @" query { receiveNullableInt }",
            @"{ ""data"": { ""receiveNullableInt"": ""object null"" } }")]
        [TestCase(
            @" query { receiveNullableInt(obj: null) }",
            @"{ ""data"": { ""receiveNullableInt"": ""object null"" } }")]
        [TestCase(
            @" query { receiveNullableInt(obj: 1) }",
            @"{ ""data"": { ""receiveNullableInt"": ""object supplied"" } }")]
        [TestCase(
            @" query { receiveNullableIntWithDefaultValue }",
            @"{ ""data"": { ""receiveNullableIntWithDefaultValue"": ""object null"" } }")]
        [TestCase(
            @" query { receiveNullableIntWithDefaultValue(obj: null) }",
            @"{ ""data"": { ""receiveNullableIntWithDefaultValue"": ""object null"" } }")]
        [TestCase(
            @" query { receiveNullableIntWithDefaultValue(obj: 1) }",
            @"{ ""data"": { ""receiveNullableIntWithDefaultValue"": ""object supplied"" } }")]
        [TestCase(
            @" query { receiveNullableEnum }",
            @"{ ""data"": { ""receiveNullableEnum"": ""object null"" } }")]
        [TestCase(
            @" query { receiveNullableEnum(obj: null) }",
            @"{ ""data"": { ""receiveNullableEnum"": ""object null"" } }")]
        [TestCase(
            @" query { receiveNullableEnum(obj: VALUE1) }",
            @"{ ""data"": { ""receiveNullableEnum"": ""object supplied"" } }")]
        [TestCase(
            @" query { receiveNullableEnumWithDefaultValue }",
            @"{ ""data"": { ""receiveNullableEnumWithDefaultValue"": ""object null"" } }")]
        [TestCase(
            @" query { receiveNullableEnumWithDefaultValue(obj: null) }",
            @"{ ""data"": { ""receiveNullableEnumWithDefaultValue"": ""object null"" } }")]
        [TestCase(
            @" query { receiveNullableEnumWithDefaultValue(obj: VALUE1) }",
            @"{ ""data"": { ""receiveNullableEnumWithDefaultValue"": ""object supplied"" } }")]
        [TestCase(
            @" query { receiveObject }",
            @"{ ""data"": { ""receiveObject"": ""object null"" } }")]
        [TestCase(
            @" query { receiveObject(obj: null) }",
            @"{ ""data"": { ""receiveObject"": ""object null"" } }")]
        [TestCase(
            @" query { receiveObject(obj: {}) }",
            @"{ ""data"": { ""receiveObject"": ""object supplied"" } }")]
        [TestCase(
            @" query { receiveObjectWithDefaultValue }",
            @"{ ""data"": { ""receiveObjectWithDefaultValue"": ""object null"" } }")]
        [TestCase(
            @" query { receiveObjectWithDefaultValue(obj: null) }",
            @"{ ""data"": { ""receiveObjectWithDefaultValue"": ""object null"" } }")]
        [TestCase(
            @" query { receiveObjectWithDefaultValue(obj: {}) }",
            @"{ ""data"": { ""receiveObjectWithDefaultValue"": ""object supplied"" } }")]
        [TestCase(
            @" query { receiveNullableObject }",
            @"{ ""data"": { ""receiveNullableObject"": ""object null"" } }")]
        [TestCase(
            @" query { receiveNullableObject(obj: null) }",
            @"{ ""data"": { ""receiveNullableObject"": ""object null"" } }")]
        [TestCase(
            @" query { receiveNullableObject(obj: {}) }",
            @"{ ""data"": { ""receiveNullableObject"": ""object supplied"" } }")]
        [TestCase(
            @" query { receiveNullableObjectWithDefaultValue }",
            @"{ ""data"": { ""receiveNullableObjectWithDefaultValue"": ""object null"" } }")]
        [TestCase(
            @" query { receiveNullableObjectWithDefaultValue(obj: null) }",
            @"{ ""data"": { ""receiveNullableObjectWithDefaultValue"": ""object null"" } }")]
        [TestCase(
            @" query { receiveNullableObjectWithDefaultValue(obj: {}) }",
            @"{ ""data"": { ""receiveNullableObjectWithDefaultValue"": ""object supplied"" } }")]
        [TestCase(
            @" query { receiveNullableStruct }",
            @"{ ""data"": { ""receiveNullableStruct"": ""object null"" } }")]
        [TestCase(
            @" query { receiveNullableStruct(obj: null) }",
            @"{ ""data"": { ""receiveNullableStruct"": ""object null"" } }")]
        [TestCase(
            @" query { receiveNullableStruct(obj: {field1: ""bob""}) }",
            @"{ ""data"": { ""receiveNullableStruct"": ""object supplied"" } }")]
        [TestCase(
            @" query { receiveNullableStructWithDefaultValue }",
            @"{ ""data"": { ""receiveNullableStructWithDefaultValue"": ""object null"" } }")]
        [TestCase(
            @" query { receiveNullableStructWithDefaultValue(obj: null) }",
            @"{ ""data"": { ""receiveNullableStructWithDefaultValue"": ""object null"" } }")]
        [TestCase(
            @" query { receiveNullableStructWithDefaultValue(obj: {field1: ""bob""}) }",
            @"{ ""data"": { ""receiveNullableStructWithDefaultValue"": ""object supplied"" } }")]
        public async Task NullableFieldArgument_DefaultValueTests(string query, string expectedResults)
        {
            var server = new TestServerBuilder()
                .AddGraphController<NullableFieldArgumentTestController>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(query);
            var result = await server.RenderResult(builder);

            CommonAssertions.AreEqualJsonStrings(expectedResults, result);
        }

        [TestCase(
            @" query { receiveNullableInt }",
            @"{ ""data"": { ""receiveNullableInt"": null } }")]
        [TestCase(
            @" query { receiveNullableInt(obj:null) }",
            @"{ ""data"": { ""receiveNullableInt"": null } }")]
        [TestCase(
            @" query { receiveNullableInt(obj: 3) }",
            @"{ ""data"": { ""receiveNullableInt"": ""3"" } }")]
        [TestCase(
            @" query { receiveNullableIntWithDefaultValue }",
            @"{ ""data"": { ""receiveNullableIntWithDefaultValue"": ""5"" } }")]
        [TestCase(
            @" query { receiveNullableIntWithDefaultValue(obj:null) }",
            @"{ ""data"": { ""receiveNullableIntWithDefaultValue"": null } }")]
        [TestCase(
            @" query { receiveNullableIntWithDefaultValue(obj: 3) }",
            @"{ ""data"": { ""receiveNullableIntWithDefaultValue"": ""3"" } }")]
        [TestCase(
            @" query { receiveNullableEnum }",
            @"{ ""data"": { ""receiveNullableEnum"": null } }")]
        [TestCase(
            @" query { receiveNullableEnum(obj:null) }",
            @"{ ""data"": { ""receiveNullableEnum"": null } }")]
        [TestCase(
            @" query { receiveNullableEnum(obj: VALUE1) }",
            @"{ ""data"": { ""receiveNullableEnum"": ""Value1"" } }")]
        [TestCase(
            @" query { receiveNullableEnumWithDefaultValue }",
            @"{ ""data"": { ""receiveNullableEnumWithDefaultValue"": ""Value2"" } }")]
        [TestCase(
            @" query { receiveNullableEnumWithDefaultValue(obj:null) }",
            @"{ ""data"": { ""receiveNullableEnumWithDefaultValue"": null } }")]
        [TestCase(
            @" query { receiveNullableEnumWithDefaultValue(obj: VALUE1) }",
            @"{ ""data"": { ""receiveNullableEnumWithDefaultValue"": ""Value1"" } }")]
        public async Task NullableFieldArgument_NonNullDefaultValueTests(string query, string expectedResults)
        {
            var server = new TestServerBuilder()
                .AddGraphController<DefaultValueCheckerController>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(query);
            var result = await server.RenderResult(builder);

            CommonAssertions.AreEqualJsonStrings(expectedResults, result);
        }

        [Test]
        public async Task Inherited_ControllerAction_IsMappedCorrectly()
        {
            var server = new TestServerBuilder()
                  .AddGraphQL(o =>
                  {
                      o.AutoRegisterLocalEntities = false;
                      o.AddType<EmployeeController>();
                  })
                  .Build();

            // totalPeople exists on  base controller
            // totalEmployees exists on  the added EmployeeController
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(@"query {
                    employees  {
                        totalEmployees
                        totalPeople
                    }
                }");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""employees"" : {
                            ""totalEmployees"": 5,
                            ""totalPeople"": 99
                        }
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task Inherited_ControllerAction_OnAbstractController_IsRegistered()
        {
            var server = new TestServerBuilder()
                  .AddGraphQL(o =>
                  {
                      o.AutoRegisterLocalEntities = false;
                      o.AddType<AppleController>();
                  })
                  .Build();

            // totalPeople exists on  base controller
            // totalEmployees exists on  the added EmployeeController
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(@"query {
                    apples  {
                        totalFruit
                        totalApples
                    }
                }");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""apples"" : {
                            ""totalFruit"": 32,
                            ""totalApples"": 85
                        }
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task Inherited_NonAbstractControllerAction_IsRegistered_WhenInheritedControllerIsAlsoRegistered_ActionIsRegisteredTwice()
        {
            var server = new TestServerBuilder()
                  .AddGraphQL(o =>
                  {
                      o.AutoRegisterLocalEntities = false;
                      o.AddType<AppleController>();
                      o.AddType<FruitController>();
                  })
                  .Build();

            // totalPeople exists on  base controller
            // totalEmployees exists on  the added EmployeeController
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(@"query {
                    apples  {
                        totalFruit   # should be a route on appleController
                        totalApples
                    }
                    fruit {
                        totalFruit   # should be registered to the base controller as itself
                    }
                }");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""apples"" : {
                            ""totalFruit"": 32,
                            ""totalApples"": 85
                        },
                        ""fruit"" : {
                            ""totalFruit"": 32
                        }
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task WhenBaseControllerDefinesGraphRoute_ParentControllerCanOverrideTheValueToRegisterItsOwnRouteKey()
        {
            var server = new TestServerBuilder()
                  .AddGraphQL(o =>
                  {
                      o.AutoRegisterLocalEntities = false;
                      o.AddType<RoomController>();
                      o.AddType<HouseController>();
                  })
                  .Build();

            // totalPeople exists on  base controller
            // totalEmployees exists on  the added EmployeeController
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(@"query {
                    houseFloorPlan  {
                        totalFloors
                        totalRooms
                    }
                    roomsAvailable {
                        totalRooms
                    }
                }");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""houseFloorPlan"" : {
                            ""totalFloors"": 2,
                            ""totalRooms"": 5
                        },
                        ""roomsAvailable"" : {
                            ""totalRooms"": 5
                        }
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }
    }
}