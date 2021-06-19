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
        public async Task IndeterminateUnionType_ShouldCallIntoUnionProxyResolveMethod_AndProduceResult()
        {
            MixedTypeUnion.TotalCallCount = 0;
            var server = new TestServerBuilder()
                     .AddGraphType<MixedReturnTypeController>()
                     .AddSchemaBuilderAction(a =>
                     {
                         a.Options.ResponseOptions.ExposeExceptions = true;
                     })
                     .Build();

            // controller actually returns a MixedReturnTypeC, but is declared in the schema as
            // MixedUnionType (of A and B)
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

        [Test]
        public async Task IndeterminateUnionType_NullReturnType_Fails()
        {
            MixedTypeUnionNullReturn.TotalCallCount = 0;
            var server = new TestServerBuilder()
                     .AddGraphType<MixedReturnTypeController>()
                     .AddSchemaBuilderAction(a =>
                     {
                         a.Options.ResponseOptions.ExposeExceptions = true;
                     })
                     .Build();

            // controller actually returns a MixedReturnTypeC, but is declared in the schema as
            // MixedTypeUnionNullReturn (of A and B)
            // MixedC inherits from both A and B and could be either
            // the library should call into MixedUnionType and ask it to resolve the relationship
            // the union type (MixedTypeUnionNullReturn) will return -null-
            // and the query should fail
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                @"query  {
                    createNullIndeterminateReturn {
                        ... on MixedReturnTypeB {
                                field2
                        }
                        ... on MixedReturnTypeA {
                                field1
                        }
                }}");

            // the returned object should be carried forward to produce a result
            var result = await server.ExecuteQuery(builder);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Messages.IsSucessful);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.EXECUTION_ERROR, result.Messages[0].Code);
            Assert.AreEqual(1, MixedTypeUnionNullReturn.TotalCallCount);
        }

        [Test]
        public async Task IndeterminateUnionType_SourceReturnType_Fails()
        {
            MixedTypeUnionSourceReturn.TotalCallCount = 0;
            var server = new TestServerBuilder()
                     .AddGraphType<MixedReturnTypeController>()
                     .AddSchemaBuilderAction(a =>
                     {
                         a.Options.ResponseOptions.ExposeExceptions = true;
                     })
                     .Build();

            // controller actually returns a MixedReturnTypeC, but is declared in the schema as
            // MixedTypeUnionSourceReturn (of A and B)
            // MixedC inherits from both A and B and could be either
            // the library should call into MixedUnionType and ask it to resolve the relationship
            // the union type (MixedTypeUnionSourceReturn) will return the provided source object (MixedC)
            // and the query should fail
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                @"query  {
                    createSourceIndeterminateReturn {
                        ... on MixedReturnTypeB {
                                field2
                        }
                        ... on MixedReturnTypeA {
                                field1
                        }
                }}");

            // the returned object should be carried forward to produce a result
            var result = await server.ExecuteQuery(builder);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Messages.IsSucessful);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.EXECUTION_ERROR, result.Messages[0].Code);
            Assert.AreEqual(1, MixedTypeUnionSourceReturn.TotalCallCount);
        }
    }
}