// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.PlanGeneration
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.PlanGeneration.ComplexityTestData;
    using NUnit.Framework;

    [TestFixture]
    internal class ComplexityCalculationTests
    {
        private TestServer<GraphSchema> MakeServer(int? allowedMaxDepth = null, float? allowedMaxComplexity = null)
        {
            var builder = new TestServerBuilder()
                .AddGraphType<ComplexityTestObject>()
                .AddGraphType<NestedComplexityTestObject>()
                .AddGraphType<TripleNestedComplexityObject>()
                .AddGraphType<ComplexityTestController>();

            builder.AddGraphQL(o =>
            {
                o.ExecutionOptions.MaxQueryDepth = allowedMaxDepth;
                o.ExecutionOptions.MaxQueryComplexity = allowedMaxComplexity;
            });

            return builder.Build();
        }

        [Test]
        public void MaxDepthOfQuery_NoFragment_CalculatesAsExpected()
        {
            var server = this.MakeServer();
            var doc = server.CreateDocument(@"query {                      # Level 1
                                        complexityTest {                   # Level 2
                                            retrieveTripleNestedObject {   # Level 3
                                                property4
                                                object2 {                  # Level 4
                                                    property3
                                                    object1 {              # Level 5
                                                        property2
                                                        property1
                                                    }
                                                }
                                            }
                                        }
                                    }");

            Assert.AreEqual(5, doc.MaxDepth);
        }

        [Test]
        public void MaxDepthOfQuery_WithFragment_CalculatesAsExpected()
        {
            var server = this.MakeServer();
            var doc = server.CreateDocument(@"query {                      # Level 1
                                        complexityTest {                   # Level 2
                                            retrieveTripleNestedObject {   # Level 3
                                                property4
                                                ...ComplexSpread
                                            }
                                        }
                                    }
                                    fragment ComplexSpread on TriplenEstedComplexityObject  {  # Not A Level
                                        object2 {                  # Level 4
                                            property3
                                        }
                                    }");

            Assert.AreEqual(4, doc.MaxDepth);
        }

        [Test]
        public void MaxDepthOfQuery_WithFragment_ButNoNesting_CalculatesAsExpected()
        {
            var server = this.MakeServer();
            var doc = server.CreateDocument(@"query {                      # Level 1
                                        complexityTest {                   # Level 2
                                            retrieveTripleNestedObject {   # Level 3
                                                property4
                                                ...ComplexSpread
                                            }
                                        }
                                    }
                                    fragment ComplexSpread on TriplenEstedComplexityObject  {  # Not A Level
                                        property5  # Not a level
                                    }");

            Assert.AreEqual(3, doc.MaxDepth);
        }

        [Test]
        public void MaxDepthOfQuery_MultipleOperations_MaxDepthFromDeepestOperation_RepresentsDocument()
        {
            var server = this.MakeServer();
            var doc = server.CreateDocument(@"
                                    query Operation1{                      # Level 1
                                        complexityTest {                   # Level 2
                                            retrieveTripleNestedObject {   # Level 3
                                                property4
                                                object2 {                  # Level 4
                                                    property3
                                                    object1 {              # Level 5
                                                        property2
                                                        property1
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    query Operation2{                      # Level 1
                                        complexityTest {                   # Level 2
                                            retrieveTripleNestedObject {   # Level 3
                                                property4
                                            }
                                        }
                                    } ");

            Assert.AreEqual(5, doc.MaxDepth);
        }

        [Test]
        public async Task AllowedMaxDepthSetLowerThanDocument_DocumentIsDeniedExecution()
        {
            var server = this.MakeServer(allowedMaxDepth: 2);
            var queryBuilder = server.CreateQueryContextBuilder();

            // max depth of 5 should be rejected
            queryBuilder.AddQueryText(@"query {                      # Level 1
                                        complexityTest {                   # Level 2
                                            retrieveTripleNestedObject {   # Level 3
                                                property4
                                                object2 {                  # Level 4
                                                    property3
                                                    object1 {              # Level 5
                                                        property2
                                                        property1
                                                    }
                                                }
                                            }
                                        }
                                    }");

            var result = await server.ExecuteQuery(queryBuilder);

            Assert.AreEqual(1, result.Messages.Count);
            Assert.IsFalse(result.Messages.IsSucessful);
            Assert.AreEqual(Constants.ErrorCodes.REQUEST_ABORTED, result.Messages[0].Code);
        }

        [Test]
        public async Task ComplexityOfBasicQuery_CalculatesAsExpected()
        {
            var server = this.MakeServer();
            var queryPlan = await server.CreateQueryPlan(@"query {                      # Level 1
                                        complexityTest {                   # Level 2
                                            retrieveTripleNestedObject {   # Level 3
                                                property4
                                                object2 {                  # Level 4
                                                    property3
                                                    object1 {              # Level 5
                                                        property2
                                                        property1
                                                    }
                                                }
                                            }
                                        }
                                    }");

            Assert.That(18.377f, Is.EqualTo(queryPlan.EstimatedComplexity).Within(0.0003f));
        }

        [Test]
        public async Task AllowedComplexitySetLowerThanDocument_IsRejected()
        {
            var server = this.MakeServer(allowedMaxComplexity: 3f);
            var queryBuilder = server.CreateQueryContextBuilder();

            // should calculate to 18.377-ish
            queryBuilder.AddQueryText(@"query {                      # Level 1
                                        complexityTest {                   # Level 2
                                            retrieveTripleNestedObject {   # Level 3
                                                property4
                                                object2 {                  # Level 4
                                                    property3
                                                    object1 {              # Level 5
                                                        property2
                                                        property1
                                                    }
                                                }
                                            }
                                        }
                                    }");

            var result = await server.ExecuteQuery(queryBuilder);

            Assert.AreEqual(1, result.Messages.Count);
            Assert.IsFalse(result.Messages.IsSucessful);
            Assert.AreEqual(Constants.ErrorCodes.REQUEST_ABORTED, result.Messages[0].Code);
        }
    }
}