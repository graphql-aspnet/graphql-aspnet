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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Execution.ExecutionPlanTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class FragmentExecutionTests
    {
        [Test]
        public async Task BasicBranch_ViaNamedFragments_RendersCorrectly()
        {
            var server = new TestServerBuilder()
                    .AddGraphType<FragmentProcessingController>()
                    .Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                        query {
                            fragTester {
                                makeHybridData {
                                    ...aData
                                    ...bData
                                }
                            }
                        }

                        fragment aData on FragmentDataA{
                           property1
                        }

                        fragment bData on FragmentDataB{
                           property2
                           property1
                        }");

            var r = await server.ExecuteQuery(builder);
            var result = await server.RenderResult(builder);

            // note the ordering on bData fragment
            var expectedReslt = @"
                                {
                                    ""data"" : {
                                          ""fragTester"": {
                                                ""makeHybridData"": [
                                                {
                                                    ""property1"": ""fragmentA_prop1_0""
                                                },
                                                {
                                                    ""property1"": ""fragmentA_prop1_1""
                                                },
                                                {
                                                    ""property2"": ""fragmentB_prop2_0"",
                                                    ""property1"": ""fragmentB_prop1_0""
                                                },
                                                {
                                                    ""property2"": ""fragmentB_prop2_1"",
                                                    ""property1"": ""fragmentB_prop1_1""
                                                }]
                                            }
                                    }
                                }";

            CommonAssertions.AreEqualJsonStrings(expectedReslt, result);
        }

        [Test]
        public async Task WhenASourceItemMatchesMultipleNamedFragments_WithDuplicatePropertyNames_EnsureDataResultsAreCorrect()
        {
            // concrete class FragmentDataC inherits from FragmentDataA
            // any items that match for C will also match for A rendering two "Property1" if a soft coerison is done (using the "is" operator)
            // this test ensures that explicit type checks are always done when executing fields with target type restrictions

            // before testing ensure the inheritance chain hasnt been altered
            Assert.IsTrue(Validation.IsCastable<FragmentDataA>(typeof(FragmentDataC)));
            var server = new TestServerBuilder()
                .AddGraphType<FragmentProcessingController>()
                .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                            query {
                                fragTester {
                                    sourceDataInheritance {
                                        ...aData
                                        ...bData
                                        ...cData
                                    }
                                }
                            }

                            fragment aData on FragmentDataA{
                               property1
                            }

                            fragment bData on FragmentDataB{
                               property1
                            }

                            fragment cData on FragmentDataC{
                               property2
                               property1
                            }");

            var expectedResult = @"
                                {
                                    ""data"" : {
                                      ""fragTester"": {
                                            ""sourceDataInheritance"": [
                                            {
                                                ""property1"": ""fragmentA_prop1_0""
                                            },
                                            {
                                                ""property1"": ""fragmentB_prop1_0""
                                            },
                                            {
                                                ""property2"": ""fragmentA_prop2_FromC"",
                                                ""property1"": ""fragmentA_prop1_FromC""
                                            }]
                                        }
                                      }
                                    }";

            var result = await server.RenderResult(builder);

            CommonAssertions.AreEqualJsonStrings(expectedResult, result);
        }

        [Test]
        public async Task WhenAnInterfaceIsSpreadInAUnion_TheUnionMembersThatImplementTheInterface_ShouldReturnInterfaceFields()
        {
            // sourceDataInheritance returns a union type (FragmentDataA | FragmentDataB | FragmentDataC)
            // All Three implement interface graph type FragmentDataItem and should include any fields in the "sampleFrag" fragment
            // which is typed to the interface
            var server = new TestServerBuilder()
                    .AddGraphType<FragmentProcessingController>()
                    .AddGraphType<IFragmentDataItem>()
                    .AddSchemaBuilderAction(o =>
                    {
                        o.Options.ResponseOptions.ExposeExceptions = true;
                    })
                    .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                            query {
                                fragTester {
                                    sourceDataInheritance {
                                        ...sampleFrag
                                    }
                                }
                            }

                            fragment sampleFrag on FragmentDataItem {
                               property1
                            }");

            var expectedResult = @"
                                    {
                                        ""data"" : {
                                              ""fragTester"": {
                                                    ""sourceDataInheritance"": [
                                                    {
                                                        ""property1"": ""fragmentA_prop1_0""
                                                    },
                                                    {
                                                        ""property1"": ""fragmentB_prop1_0""
                                                    },
                                                    {
                                                        ""property1"": ""fragmentA_prop1_FromC""
                                                    }]
                                                }
                                        }
                                    }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedResult, result);
        }
    }
}