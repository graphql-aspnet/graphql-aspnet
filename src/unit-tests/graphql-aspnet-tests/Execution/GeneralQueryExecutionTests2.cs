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
    using GraphQL.AspNet.Tests.Execution.TestData.ExecutionPlanTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
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
        public async Task NonNullableInputFieldWithDefaultValue_WhenVariableSuppliedIsNull_UsesDefaultValue()
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

        [TestCase("ID!", "\"3\"")]
        [TestCase("ID", "\"3\"")]
        public async Task SingleValueVariableInDeclaredArray_IsCoercableToArrayOfValidType(
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

        [TestCase("ID", "3")]
        [TestCase("ID!", "3")]
        public async Task SingleValueVariableInDeclaredArray_OfWrongValue_IsNotCoercableToArrayOfValidType(
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
    }
}