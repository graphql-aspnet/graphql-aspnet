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
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Tests.Execution.TestData.ControllerIsolationTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class ControllerIsolationTests
    {
        [Test]
        public async Task IsolatedController_WithDownLevelInvocations_IsolatesCorrectly()
        {
            var query = @"query {
                    isolated {
                        extractInt
                        extractSecondInt
                    }
                    isolated2 {
                        extractInt
                        extractSecondInt
                    }
                }";

            var server = new TestServerBuilder()
                .AddGraphQL(options =>
                {
                    options.AddController<IsolatedController>();
                    options.AddController<Isolated2Controller>();
                    options.ExecutionOptions.ResolverIsolation = ResolverIsolationOptions.All;
                })

                .Build();

            var contextBuilder = server.CreateQueryContextBuilder();
            contextBuilder.AddQueryText(query);

            var result = await server.ExecuteQuery(contextBuilder);
            Assert.AreEqual(0, result.Messages.Count);

            var jsonData = await server.RenderResult(contextBuilder);

            var expectedOutput =
                @"{
                    ""data"": {
                       ""isolated"" : {
                               ""extractInt"": 3,
                               ""extractSecondInt"": 4
                        },
                        ""isolated2"" : {
                               ""extractInt"": 3,
                               ""extractSecondInt"": 4
                        }
                     }
                  }";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, jsonData);
        }

        [Test]
        public async Task IsolatedController_SameController_IsolatesCorrectly()
        {
            var query = @"query {
                    renamed: isolated {
                        extractInt
                        extractSecondInt
                    }

                    isolated {
                        extractInt
                        extractSecondInt
                    }
                }";

            var server = new TestServerBuilder()
                .AddGraphQL(options =>
                {
                    options.AddController<IsolatedController>();
                    options.ExecutionOptions.ResolverIsolation = ResolverIsolationOptions.All;
                })
                .Build();

            var contextBuilder = server.CreateQueryContextBuilder();
            contextBuilder.AddQueryText(query);

            var result = await server.ExecuteQuery(contextBuilder);
            Assert.AreEqual(0, result.Messages.Count);

            var jsonData = await server.RenderResult(contextBuilder);

            var expectedOutput =
                @"{
                    ""data"": {
                       ""isolated"" : {
                               ""extractInt"": 3,
                               ""extractSecondInt"": 4
                        },
                        ""renamed"" : {
                               ""extractInt"": 3,
                               ""extractSecondInt"": 4
                        }
                     }
                  }";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, jsonData);
        }
    }
}