// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine
{
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Execution.QueryPlans.ComplexityTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class DefaultOperationDepthCalculatorTests
    {
        private IQueryDocument CreateDocument(string text)
        {
            var builder = new TestServerBuilder()
                .AddType<ComplexityTestObject>()
                .AddType<NestedComplexityTestObject>()
                .AddType<TripleNestedComplexityObject>()
                .AddType<ComplexityTestController>();

            var server = builder.Build();

            return server.CreateDocument(text);
        }

        [Test]
        public void MaxDepthOfQuery_NoFragment_CalculatesAsExpected()
        {
            var doc = this.CreateDocument(@"query {                      # Level 1
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

            var calculator = new DefaultQueryOperationDepthCalculator<GraphSchema>();

            var result = calculator.Calculate(doc.Operations[0]);
            Assert.AreEqual(5, result);
        }

        [Test]
        public void MaxDepthOfQuery_WithFragment_CalculatesAsExpected()
        {
            var doc = this.CreateDocument(@"query {                      # Level 1
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

            var calculator = new DefaultQueryOperationDepthCalculator<GraphSchema>();
            var result = calculator.Calculate(doc.Operations[0]);
            Assert.AreEqual(4, result);
        }

        [Test]
        public void MaxDepthOfQuery_WithFragment_ButNoNesting_CalculatesAsExpected()
        {
            var doc = this.CreateDocument(@"query {                      # Level 1
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

            var calculator = new DefaultQueryOperationDepthCalculator<GraphSchema>();
            var result = calculator.Calculate(doc.Operations[0]);
            Assert.AreEqual(3, result);
        }
    }
}