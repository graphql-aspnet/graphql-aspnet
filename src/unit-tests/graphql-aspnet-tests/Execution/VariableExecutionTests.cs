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
            builder.AddVariableData("{ \"variable1\" : \"test string 86\" }");
            builder.AddQueryText("query($variable1: String){ scalarValue(arg1: $variable1) }");
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
            builder.AddVariableData("{ \"variable1\" : { \"property1\" : \"value1\", \"property2\": 15 } }");
            builder.AddQueryText("query($variable1: Input_TwoPropertyObject){ complexValue(arg1: $variable1) { property1 property2 } }");
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
        public async Task NestedScalarVariable_IsUsedInsteadOfDefault()
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
            builder.AddVariableData("{ \"variable1\" : \"stringPassedValue\" }");
            builder.AddQueryText("query($variable1: String){ " +
                "complexValue(arg1: { property1: $variable1, property2: 15} ) " +
                "{ property1 property2 } }");
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

            builder.AddVariableData("{ \"variable1\" : 4 }");

            builder.AddQueryText("query($variable1: Int!){ " +
                "   sumListValues(arg1: [1,2,$variable1]) " +
                "}");

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

            builder.AddQueryText("query($variable1: Int!){ " +
                "   sumListListValues(arg1: [[1,2],[$variable1, 4]]) " +
                "}");

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

            builder.AddQueryText("query($variable1: Int!,$variable2: Int!){ " +
                "   stupidDeepListValues(arg1: [[[[[[[1,2],[$variable1, 4]],[[$variable2, 6]]]]]]]) " +
                "}");

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

            queryContext.AddVariableData(@"{ }");

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
        public async Task NullableModelGuid_AsInputField_WhenNotSuppliedOnVariable_IsTreatedAsNull()
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
        public async Task NullableModelGuid_AsInputField_WhenIdValueSuppliedOnVariableAsNull_IsNull()
        {
            // github issue 95
            var builder = new TestServerBuilder();
            builder.AddGraphController<NullableVariableObjectController>();
            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"mutation ($arg1: Guid){
                    createWithModelGuidNullId(param: {id: $arg1})  {
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
        public async Task NullableEnum_AsInputField_WhenNotSuppliedOnVariable_IsTreatedAsNull()
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
    }
}