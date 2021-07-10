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

        [Test]
        public async Task WhenSourceObjectIsAValidProxy_ChildFieldResolutionValidatesSourceProxyAsValid()
        {
            // blog controller returns a BlogProxy and PostProxy
            // for the root object and child object set
            var server = new TestServerBuilder()
                    .AddGraphType<BlogController>()
                    .AddSchemaBuilderAction(a =>
                    {
                        a.Options.ResponseOptions.ExposeExceptions = true;
                    })
                    .Build();

            // blogs returns a BlogProxyObject
            // the [type]/Blog/posts field should
            // validate the Type BlogProxy as being a valid Blog that it can
            // use as a source
            // same with BlogPost/BlogPostProxy and BlogPostComment/BlogPostCommentProxy
            var builder = server.CreateQueryContextBuilder()
            .AddQueryText(
            @"query  {
                        blogs {
                            blogId,
                            url,
                            posts {
                                postId,
                                title,
                                comments {
                                    commentId,
                                    comment
                                }
                            }
                    }}");

            var expectedResult = @"
            {
              ""data"": {
                ""blogs"": [
                  {
                    ""blogId"": 1,
                    ""url"": ""http://blog.com"",
                    ""posts"": [
                      {
                        ""postId"": 1,
                        ""title"": ""Title 1"",
                        ""comments"": [
                          {
                            ""commentId"": 30,
                            ""comment"": ""Comment 30""
                          },
                          {
                            ""commentId"": 31,
                            ""comment"": ""Comment 31""
                          }
                        ]
                      },
                      {
                        ""postId"": 2,
                        ""title"": ""Title 2"",
                        ""comments"": [
                          {
                            ""commentId"": 32,
                            ""comment"": ""Comment 32""
                          },
                          {
                            ""commentId"": 33,
                            ""comment"": ""Comment 33""
                          }
                        ]
                      }
                    ]
                  }
                ]
              }
                }";

            var outputJson = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedResult, outputJson);
        }
    }
}