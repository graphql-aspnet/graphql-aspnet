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
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Tests.Execution.TestData.InputVariableExecutionTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class VariableExecutionTests
    {
        [Test]
        public async Task SingleScalarValueVariable_IsUsedInsteadOfDefault()
        {
            var server = new TestServerBuilder()
                .AddType<InputValueController>()
                .Build();

            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(
                @"query($variable1: String) {
                    scalarValue(arg1: $variable1)
                  }");

            builder.AddVariableData("{ \"variable1\" : \"test string 86\" }");

            var result = await server.RenderResult(builder);

            var expected = @"
                            {
                                ""data"" : {
                                  ""scalarValue"" : ""test string 86""
                                }
                            }";

            CommonAssertions.AreEqualJsonStrings(expected, result);
        }

        [Test]
        public async Task SingleComplexValueVariable_IsUsedInsteadOfDefault()
        {
            var server = new TestServerBuilder()
                .AddType<InputValueController>()
                .Build();

            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(
                @"query($variable1: Input_TwoPropertyObject) {
                    complexValue(arg1: $variable1) {
                        property1
                        property2
                    }
                }");

            builder.AddVariableData(
                @"{
                    ""variable1"" : {
                            ""property1"" : ""value1"",
                            ""property2"": 15
                    }
                  }");

            var result = await server.RenderResult(builder);

            var expected = @"{
                                ""data"" : {
                                  ""complexValue"" : {
                                        ""property1"" : ""value1"",
                                        ""property2"" : 15
                                    }
                                }
                            }";

            CommonAssertions.AreEqualJsonStrings(expected, result);
        }

        [Test]
        public async Task ScalarVariable_NestedInInputObject_IsUsedInsteadOfDefault()
        {
            var server = new TestServerBuilder()
                .AddType<InputValueController>()
                .AddGraphQL(o =>
                {
                    o.ResponseOptions.ExposeExceptions = true;
                })
                .Build();

            // variable passed is just 1 value of hte input object (not the whole thing)
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(
                @"query($variable1: String){
                        complexValue(arg1: { property1: $variable1, property2: 15} ) {
                            property1
                            property2
                        }
                }");

            builder.AddVariableData("{ \"variable1\" : \"stringPassedValue\" }");

            var result = await server.RenderResult(builder);

            var expected = @"{
                                ""data"" : {
                                  ""complexValue"" : {
                                        ""property1"" : ""stringPassedValue"",
                                        ""property2"" : 15
                                    }
                                }
                            }";

            CommonAssertions.AreEqualJsonStrings(expected, result);
        }

        [Test]
        public async Task VariableAsAListItem_ExecutesCorrectly()
        {
            var server = new TestServerBuilder()
                .AddType<InputValueController>()
                .AddGraphQL(o =>
                {
                    o.ResponseOptions.ExposeExceptions = true;
                })
                .Build();

            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(
                @"query($variable1: Int!){
                   sumListValues(arg1: [1,2,$variable1])
                }");

            builder.AddVariableData("{ \"variable1\" : 4 }");

            var result = await server.RenderResult(builder);

            var expected = @"{
                                ""data"" : {
                                  ""sumListValues"" : 7
                                }
                            }";

            CommonAssertions.AreEqualJsonStrings(expected, result);
        }

        [Test]
        public async Task VariableAsAListOfListItem_ExecutesCorrectly()
        {
            var server = new TestServerBuilder()
                .AddType<InputValueController>()
                .AddGraphQL(o =>
                {
                    o.ResponseOptions.ExposeExceptions = true;
                })
                .Build();

            var builder = server.CreateQueryContextBuilder();

            builder.AddVariableData("{ \"variable1\" : 86 }");

            builder.AddQueryText(
                @"query($variable1: Int!){
                   sumListListValues(arg1: [[1,2],[$variable1, 4]])
                }");

            var result = await server.RenderResult(builder);

            var expected = @"{
                                ""data"" : {
                                  ""sumListListValues"" : 93
                                }
                            }";

            CommonAssertions.AreEqualJsonStrings(expected, result);
        }

        [Test]
        public async Task DeepNestedListOfListItem_ExecutesCorrectly()
        {
            var server = new TestServerBuilder()
                .AddType<InputValueController>()
                .AddGraphQL(o =>
                {
                    o.ResponseOptions.ExposeExceptions = true;
                })
                .Build();

            var builder = server.CreateQueryContextBuilder();

            builder.AddVariableData("{ \"variable1\" : 74, \"variable2\" : 99 }");

            builder.AddQueryText(
                @"query($variable1: Int!,$variable2: Int!){
                   stupidDeepListValues(arg1: [[[[[[[1,2],[$variable1, 4]],[[$variable2, 6]]]]]]])
                }");

            var result = await server.RenderResult(builder);

            var expected = @"{
                                ""data"" : {
                                  ""stupidDeepListValues"" : 186
                                }
                            }";

            CommonAssertions.AreEqualJsonStrings(expected, result);
        }

        [Test]
        public async Task NullableInputObject_WhenSuppliedOnVariableAsNull_IsTreatedAsNull()
        {
            // github issue 95
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Input_ModelWithGuid){
                    createWithModelGuid(param: $arg1)  {
                       property1
                        property2
                    }
                }");

            queryContext.AddVariableData(@"{ ""arg1"": null }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithModelGuid"" : {
                        ""property1"" : null,
                        ""property2"": 5
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task NullableInputObject_WhenPartiallySuppliedOnVariableAsNull_IsTreatedAsNull()
        {
            // github issue 95
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Input_ModelWithGuid){
                    createWithModelGuidNullId(param: $arg1)  {
                       property1
                        property2
                    }
                }");

            queryContext.AddVariableData(@"{ ""arg1"": { ""id"": null }  }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithModelGuidNullId"" : {
                        ""property1"" : null,
                        ""property2"": 5
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task NullableInputObject_WhenNotSuppliedOnVariable_IsTreatedAsNull()
        {
            // github issue 95
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Input_ModelWithGuid){
                    createWithModelGuid(param: $arg1)  {
                       property1
                        property2
                    }
                }");

            var result = await server.ExecuteQuery(queryContext);
            Assert.IsTrue(result.Messages.Severity.IsCritical());
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_VARIABLE_VALUE, result.Messages[0].Code);
        }

        [Test]
        public async Task NullableInputObject_WhenSuppliedOnVariable_IsParsedCorrectly()
        {
            // github issue 95
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Input_ModelWithGuid){
                    createWithModelGuid(param: $arg1)  {
                       property1
                        property2
                    }
                }");

            queryContext.AddVariableData(@"{ ""arg1"": { ""id"": ""e4b693dd-8a89-4565-af17-769daa93452d"" } }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithModelGuid"" : {
                        ""property1"" : ""e4b693dd-8a89-4565-af17-769daa93452d"",
                        ""property2"": 5
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task NullableGuid_AsInputField_WhenSupplied_IsCoerced()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            // id is a nullable guid (Guid?)
            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation {
                    createWithModelGuid(param: {id: ""E4B693DD-8A89-4565-AF17-769DAA93452D""})  {
                       property1
                        property2
                    }
                }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithModelGuid"" : {
                        ""property1"" : ""e4b693dd-8a89-4565-af17-769daa93452d"",
                        ""property2"": 5
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task NullableGuid_AsInputField_WhenNull_IsNull()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation {
                    createWithModelGuid(param: {id: null})  {
                       property1
                        property2
                    }
                }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithModelGuid"" : {
                        ""property1"" : null,
                        ""property2"": 5
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task NullableGuid_AsInputField_WhenSuppliedOnVariable_IsSupplied()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Guid){
                    createWithModelGuid(param: {id: $arg1})  {
                       property1
                        property2
                    }
                }");

            queryContext.AddVariableData(@"{ ""arg1"": ""E4B693DD-8A89-4565-AF17-769DAA93452D"" }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithModelGuid"" : {
                        ""property1"" : ""e4b693dd-8a89-4565-af17-769daa93452d"",
                        ""property2"": 5
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task NullableModelGuid_AsInputField_WhenSuppliedOnVariableAsNull_IsNull()
        {
            // github issue 95
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Guid){
                    createWithModelGuid(param: {id: $arg1})  {
                       property1
                        property2
                    }
                }");

            queryContext.AddVariableData(@"{ ""arg1"":  null }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithModelGuid"" : {
                        ""property1"" : null,
                        ""property2"": 5
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task NullableModelGuid_AsInputField_WhenNotSuppliedOnVariable_RecordsError()
        {
            // github issue 95
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Guid){
                    createWithModelGuid(param: {id: $arg1})  {
                       property1
                        property2
                    }
                }");

            queryContext.AddVariableData(@"{ }");

            var result = await server.ExecuteQuery(queryContext);
            Assert.IsTrue(result.Messages.Severity.IsCritical());
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_VARIABLE_VALUE, result.Messages[0].Code);
        }

        [Test]
        public async Task NullableModelGuid_AsInputField_WhenIdValueSuppliedOnVariableAsNull_IsNull()
        {
            // github issue 95
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Input_ModelWithGuid){
                    createWithModelGuidNullId(param: $arg1)  {
                       property1
                        property2
                    }
                }");

            queryContext.AddVariableData(@"{ ""arg1"":  { ""id"": null }  }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithModelGuidNullId"" : {
                        ""property1"" : null,
                        ""property2"": 5
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task NullableModelInt_AsInputField_WhenSuppliedOnVariableAsNull_IsNull()
        {
            // github issue 95
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Int){
                    createWithModelInt(param: {id: $arg1})  {
                       property1
                       property2
                    }
                }");

            queryContext.AddVariableData(@"{ ""arg1"":  null }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithModelInt"" : {
                        ""property1"" : ""some value"",
                        ""property2"": -1
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task NullableModelInt_AsInputField_WhenNoVariableSupplied_IsTreatedAsNullBeingSupplied()
        {
            // github issue 95
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Int){
                    createWithModelInt(param: {id: $arg1})  {
                       property1
                       property2
                    }
                }");

            queryContext.AddVariableData(@"{ }");

            var result = await server.ExecuteQuery(queryContext);
            Assert.IsTrue(result.Messages.Severity.IsCritical());
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_VARIABLE_VALUE, result.Messages[0].Code);
        }

        [Test]
        public async Task NullableGraphId_AsInputField_WhenSuppliedOnVariableAsNull_IsNull()
        {
            // github issue 95
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: ID){
                    createWithNullableId(param: $arg1)  {
                       property1
                       property2
                    }
                }");

            queryContext.AddVariableData(@"{ ""arg1"":  null }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithNullableId"" : {
                        ""property1"" : null,
                        ""property2"": 5
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task NullableGraphId_AsInputField_WhenNotSuppliedOnVariable_IsTreatedAsNull()
        {
            // github issue 95
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: ID){
                    createWithNullableId(param: $arg1)  {
                       property1
                        property2
                    }
                }");

            queryContext.AddVariableData(@"{ }");

            var result = await server.ExecuteQuery(queryContext);
            Assert.IsTrue(result.Messages.Severity.IsCritical());
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_VARIABLE_VALUE, result.Messages[0].Code);
        }

        [Test]
        public async Task GraphId_AsInputField_WhenSuppliedOnVariableAsNull_IsNull()
        {
            // github issue 95
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: ID!){
                    createWithId(param: $arg1)  {
                       property1
                       property2
                    }
                }");

            queryContext.AddVariableData(@"{ ""arg1"":  null }");

            var result = await server.ExecuteQuery(queryContext);
            Assert.IsTrue(result.Messages.Severity.IsCritical());
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_VARIABLE_VALUE, result.Messages[0].Code);
        }

        [Test]
        public async Task GraphId_AsInputField_WhenNotSuppliedOnVariable_IsTreatedAsNull()
        {
            // github issue 95
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: ID!){
                    createWithId(param: $arg1)  {
                       property1
                       property2
                    }
                }");

            queryContext.AddVariableData(@"{  }");

            var result = await server.ExecuteQuery(queryContext);
            Assert.IsTrue(result.Messages.Severity.IsCritical());
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_VARIABLE_VALUE, result.Messages[0].Code);
        }

        [Test]
        public async Task NullableEnum_AsInputField_WhenSuppliedOnVariableAsNull_IsNull()
        {
            // github issue 95
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: TestEnum){
                    createWithEnum(param: $arg1)  {
                       property1
                       property2
                    }
                }");

            queryContext.AddVariableData(@"{ ""arg1"":  null }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithEnum"" : {
                        ""property1"" : null,
                        ""property2"": 5
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task NullableEnum_AsInputField_WhenNotSuppliedOnVariable_ReturnsError()
        {
            // github issue 95
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: TestEnum){
                    createWithEnum(param: $arg1)  {
                       property1
                        property2
                    }
                }");

            queryContext.AddVariableData(@"{ }");

            var result = await server.ExecuteQuery(queryContext);
            Assert.IsTrue(result.Messages.Severity.IsCritical());
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_VARIABLE_VALUE, result.Messages[0].Code);
        }

        [Test]
        public async Task NullableInt_AsInputField_WhenSuppliedOnVariableAsNull_IsNull()
        {
            // github issue 95
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Int){
                    createWithNullableInt(param: $arg1)  {
                       property1
                       property2
                    }
                }");

            queryContext.AddVariableData(@"{ ""arg1"":  null }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithNullableInt"" : {
                        ""property1"" : null,
                        ""property2"": 5
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task NullableInt_AsInputField_WhenNotSuppliedOnVariable_IsTreatedAsNull()
        {
            // github issue 95
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Int){
                    createWithNullableInt(param: $arg1)  {
                       property1
                        property2
                    }
                }");

            queryContext.AddVariableData(@"{ }");

            var result = await server.ExecuteQuery(queryContext);
            Assert.IsTrue(result.Messages.Severity.IsCritical());
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_VARIABLE_VALUE, result.Messages[0].Code);
        }

        [Test]
        public async Task NonNullableInt_AsInputField_WhenNotSuppliedOnVariable_IsTreatedAsNull()
        {
            // github issue 95
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Int!){
                    createWithInt(param: $arg1)  {
                       property1
                        property2
                    }
                }");

            queryContext.AddVariableData(@"{ }");

            var result = await server.ExecuteQuery(queryContext);
            Assert.IsTrue(result.Messages.Severity.IsCritical());
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_VARIABLE_VALUE, result.Messages[0].Code);
        }

        [Test]
        public async Task NonNullableInt_AsInputField_WhenSuppliedOnVariableAsNull_IsTreatedAsNull()
        {
            // github issue 95
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Int!){
                    createWithInt(param: $arg1)  {
                       property1
                        property2
                    }
                }");

            queryContext.AddVariableData(@"{ ""arg1"": null }");

            var result = await server.ExecuteQuery(queryContext);
            Assert.IsTrue(result.Messages.Severity.IsCritical());
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_VARIABLE_VALUE, result.Messages[0].Code);
        }

        [Test]
        public async Task NonNullableInt_AsInputField_WhenNotSuppliedOnVariable_ButWithDefaultValue_IsTreatedAsDefaultValue()
        {
            // github issue 95
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Int! = 34){
                    createWithInt(param: $arg1)  {
                       property1
                        property2
                    }
                }");

            queryContext.AddVariableData(@"{ }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithInt"" : {
                        ""property1"" : ""34"",
                        ""property2"": 5
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task NonNullableInt_AsInputField_WhenSuppliedOnVariableAsNull_ButWithDefaultValue_IsTreatedAsNullValue()
        {
            // github issue 95
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Int! = 34){
                    createWithInt(param: $arg1)  {
                       property1
                        property2
                    }
                }");

            // supplied null value will take prescendents over the 34 (default value)
            // and fail the query
            queryContext.AddVariableData(@"{ ""arg1"" : null }");

            var result = await server.ExecuteQuery(queryContext);
            Assert.IsTrue(result.Messages.Severity.IsCritical());
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_VARIABLE_VALUE, result.Messages[0].Code);
        }

        [Test]
        public async Task InputObject_WithNotRequiredNonNullableInt_WhenVariableNotSupplied_DefaultValueIsUsed()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Int = null){
                    createWithModelWithIntWithDefaultValue(param: {id: $arg1} )  {
                       property1
                        property2
                    }
                }");

            // No Variable data is supplied to a variable with a default value of null
            // meaning the default value of param.id should take effect
            queryContext.AddVariableData(@"{ }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithModelWithIntWithDefaultValue"" : {
                        ""property1"" : ""98"",
                        ""property2"": 5
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task InputObject_WithNotRequiredNonNullableInt_WhenVariableNotSupplied_VariableDefaultValueIsUsed()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Int = 409){
                    createWithModelWithIntWithDefaultValue(param: {id: $arg1} )  {
                       property1
                        property2
                    }
                }");

            // No Variable data is supplied meaning the default value of param.id
            // shoudl take effect
            queryContext.AddVariableData(@"{ }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithModelWithIntWithDefaultValue"" : {
                        ""property1"" : ""409"",
                        ""property2"": 5
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task InputObject_WithNotRequiredNonNullableInt_WhenVariableSupplied_VariableValueIsUsed()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Int = 409){
                    createWithModelWithIntWithDefaultValue(param: {id: $arg1} )  {
                       property1
                        property2
                    }
                }");

            // No Variable data is supplied meaning the default value of param.id
            // shoudl take effect
            queryContext.AddVariableData(@"{ ""arg1"" : 65 }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithModelWithIntWithDefaultValue"" : {
                        ""property1"" : ""65"",
                        ""property2"": 5
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task InputObject_WithNotRequiredNonNullableInt_WhenVariableIsSuppliedAsNull_FieldErrorOccurs()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Int = null){
                    createWithModelWithIntWithDefaultValue(param: {id: $arg1} )  {
                       property1
                        property2
                    }
                }");

            // Variable data is EXPLICITLY supplied as null, meaning the default value of param.id
            // cannot take effect and a field error should occur
            queryContext.AddVariableData(@"{ ""arg1"" : null }");

            var result = await server.ExecuteQuery(queryContext);
            Assert.IsTrue(result.Messages.Severity.IsCritical());
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_ARGUMENT_VALUE, result.Messages[0].Code);
        }

        [Test]
        public async Task SingleArgumentScalar_WithNotRequiredNonNullableInt_WhenVariableIsNotSupplied_DefaultValueOfVariableIsUsed()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Int = 11){
                    createWithIntWithDefaultValue(param: $arg1)  {
                       property1
                        property2
                    }
                }");

            // No Variable data is supplied for $arg1 meaning the default value of the
            // variable should take effect since it was defined
            queryContext.AddVariableData(@"{ }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithIntWithDefaultValue"" : {
                        ""property1"" : ""11"",
                        ""property2"": 5
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }


        [Test]
        public async Task SingleArgumentScalar_WithNotRequiredNonNullableInt_WhenVariableIsNotSupplied_DefaultValueOfVariableIsUsed_CausesErrorWhenDefaultIsInvalid()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Int = 11.23){
                    createWithIntWithDefaultValue(param: $arg1)  {
                       property1
                        property2
                    }
                }");

            // No Variable data is supplied for $arg1 meaning the default value of the
            // variable should take effect since it was defined
            queryContext.AddVariableData(@"{ }");

            var result = await server.ExecuteQuery(queryContext);
            Assert.IsTrue(result.Messages.Severity.IsCritical());
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_VARIABLE_VALUE, result.Messages[0].Code);
        }

        [Test]
        public async Task SingleArgumentScalar_WithNotRequiredNonNullableInt_WhenVariableIsIsSupplied_ButHasInvalidDefaultValue_DefaultValueIsIgnored()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Int = 11.23){
                    createWithIntWithDefaultValue(param: $arg1)  {
                       property1
                        property2
                    }
                }");

            // No Variable data is supplied for $arg1 meaning the default value of the
            // variable should take effect since it was defined
            queryContext.AddVariableData(@"{ ""arg1"": 31 }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithIntWithDefaultValue"" : {
                        ""property1"" : ""31"",
                        ""property2"": 5
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task SingleArgumentScalar_WithNotRequiredNonNullableInt_WhenVariableIsNotSupplied_ArgumentDefaultValueIsUsed()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Int = null){
                    createWithIntWithDefaultValue(param: $arg1)  {
                       property1
                        property2
                    }
                }");

            // No Variable data is supplied for $arg1 meaning the default value of param
            // should take effect
            queryContext.AddVariableData(@"{ }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithIntWithDefaultValue"" : {
                        ""property1"" : ""33"",
                        ""property2"": 5
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task SingleArgumentScalar_WithNotRequiredNonNullableInt_WhenVariableExplicitlySuppliedAsNull_FieldErrorOccurs()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Int = null){
                    createWithIntWithDefaultValue(param: $arg1)  {
                       property1
                        property2
                    }
                }");

            queryContext.AddVariableData(@"{ ""arg1"" : null }");

            // Variable data is EXPLICITLY supplied as null, meaning the default value of param
            // cannot take effect and thus the value supplied cannot be used.
            var result = await server.ExecuteQuery(queryContext);
            Assert.IsTrue(result.Messages.Severity.IsCritical());
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_ARGUMENT_VALUE, result.Messages[0].Code);
        }

        [Test]
        public async Task SingleArgumentScalar_WithNotRequiredNonNullableInt_WhenVariableIsSupplied_VariableValueIsUsed()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Int = null){
                    createWithIntWithDefaultValue(param: $arg1)  {
                       property1
                        property2
                    }
                }");

            // Actual value supplied for arg1...should be used
            queryContext.AddVariableData(@"{ ""arg1"" : 2020 }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithIntWithDefaultValue"" : {
                        ""property1"" : ""2020"",
                        ""property2"": 5
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task SingleArgumentEnum_WithNotRequiredNonNullableEnum_WhenVariableIsNotSupplied_DefaultValueOfVariableIsUsed()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: TestEnum = VALUE1){
                    createWithEnumWithDefaultValue(param: $arg1)  {
                       property1
                        property2
                    }
                }");

            // No Variable data is supplied for $arg1 meaning the default value of the
            // variable should take effect since it was defined
            queryContext.AddVariableData(@"{ }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithEnumWithDefaultValue"" : {
                        ""property1"" : ""Value1"",
                        ""property2"": 5
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task SingleArgumentEnum_WithNotRequiredNonNullableEnum_WhenVariableIsNotSupplied_ArgumentDefaultValueIsUsed()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: TestEnum = null){
                    createWithEnumWithDefaultValue(param: $arg1)  {
                       property1
                        property2
                    }
                }");

            // No Variable data is supplied for $arg1 meaning the default value of param
            // should take effect
            queryContext.AddVariableData(@"{ }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithEnumWithDefaultValue"" : {
                        ""property1"" : ""Value2"",
                        ""property2"": 5
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task SingleArgumentEnum_WithNotRequiredNonNullableEnum_WhenVariableExplicitlySuppliedAsNull_FieldErrorOccurs()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: TestEnum = null){
                    createWithEnumWithDefaultValue(param: $arg1)  {
                       property1
                        property2
                    }
                }");

            queryContext.AddVariableData(@"{ ""arg1"" : null }");

            // Variable data is EXPLICITLY supplied as null, meaning the default value of param
            // cannot take effect and thus the value supplied cannot be used.
            var result = await server.ExecuteQuery(queryContext);
            Assert.IsTrue(result.Messages.Severity.IsCritical());
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_ARGUMENT_VALUE, result.Messages[0].Code);
        }

        [Test]
        public async Task SingleArgumentEnum_WithNotRequiredNonNullableEnum_WhenVariableIsSupplied_VariableValueIsUsed()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: TestEnum = null){
                    createWithEnumWithDefaultValue(param: $arg1)  {
                       property1
                        property2
                    }
                }");

            // Actual value supplied for arg1...should be used
            queryContext.AddVariableData(@"{ ""arg1"" : ""Value3"" }");

            var expectedJson = @"
            {
                ""data"" : {
                    ""createWithEnumWithDefaultValue"" : {
                        ""property1"" : ""Value3"",
                        ""property2"": 5
                    }
                }
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }
    }
}