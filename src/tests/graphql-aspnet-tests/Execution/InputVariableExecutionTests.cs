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
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Execution.InputVariableExecutionTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class InputVariableExecutionTests
    {
        [Test]
        public async Task SingleScalarValueVariable_IsUsedInsteadOfDefault()
        {
            var server = new TestServerBuilder()
                .AddGraphType<InputValueController>()
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
                .AddGraphType<InputValueController>()
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
                .AddGraphType<InputValueController>()
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
    }
}