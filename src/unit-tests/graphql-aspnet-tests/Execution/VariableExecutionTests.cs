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
    }
}