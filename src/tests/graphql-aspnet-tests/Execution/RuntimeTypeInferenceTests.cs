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
    using GraphQL.AspNet.Tests.Execution.ExecutionPlanTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class RuntimeTypeInferenceTests
    {
        [Test]
        public async Task ExtendedTypeOfToAKnownGraphType_ShouldBeProcessed()
        {
            var server = new TestServerBuilder()
                    .AddGraphType<MixedReturnTypeController>()
                    .Build();

            // controller returns a MixedReturnTypeB, but is declared in the schema as MixedReturnTypeA
            // (the return type of the controller method signature)
            // MixedB should be able to masquerade as MixedA
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query  { createReturnObject { field1 }}");

            // the returned object should be carried forward to produce a result
            var result = await server.RenderResult(builder);

            var expectedOutput =
            @"{
                ""data"": {
                    ""createReturnObject"" : {
                        ""field1"": ""FieldValue1""
                    }
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task IndeterminateUnionType_ShouldCallIntoUnionProxyResolveMethod()
        {
            MixedTypeUnion.TotalCallCount = 0;
            var server = new TestServerBuilder()
                     .AddGraphType<MixedReturnTypeController>()
                     .AddSchemaBuilderAction(a =>
                     {
                         a.Options.ResponseOptions.ExposeExceptions = true;
                     })
                     .Build();

            // controller returns a MixedReturnTypeC, but is declared in the schema as MixedUnionType
            // (of A and B)
            // MixedC inherits from both A and B and could be either
            // the library should call into MixedUnionType and ask it to resolve the relationship
            //
            // mixedUnion should return TypeA, thus rendering field 1
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                @"query  { 
                    createIndeterminateReturn { 
                        ... on MixedReturnTypeB {
                                field2
                        }
                        ... on MixedReturnTypeA {
                                field1
                        }
                }}");

            // the returned object should be carried forward to produce a result
            var result = await server.RenderResult(builder);

            var expectedOutput =
            @"{
                ""data"": {
                    ""createIndeterminateReturn"" : {
                        ""field1"": ""FieldValue1""
                    }
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
            Assert.AreEqual(1, MixedTypeUnion.TotalCallCount);
        }
    }
}