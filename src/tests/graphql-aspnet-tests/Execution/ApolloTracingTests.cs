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
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution.Metrics;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Execution.ExecutionPlanTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class ApolloTracingTests
    {
        [Test]
        public async Task StandardExcution_EnsureResultsMakeSense()
        {
            var serverBuilder = new TestServerBuilder(TestOptions.IncludeMetrics);
            serverBuilder.AddGraphType<SimpleExecutionController>();
            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query Operation1{  simple {  simpleQueryMethod { property1 } } }")
                .AddOperationName("Operation1");
            builder.AddMetrics(new ApolloTracingMetricsV1(server.Schema));
            var context = builder.Build();

            await server.ExecuteQuery(context);
            var metrics = context.Metrics as ApolloTracingMetricsV1;

            Assert.AreEqual(0, context.Messages.Count);

            // parse, validation, execution
            Assert.AreEqual(3, metrics.PhaseEntries.Count);

            // simple, SimpleQueryMethod, Property1
            Assert.AreEqual(3, metrics.ResolverEntries.Count);

            // ensure phase ordering follows natural progression
            var parsePhase = metrics.PhaseEntries[ApolloExecutionPhase.PARSING];
            var validationPhase = metrics.PhaseEntries[ApolloExecutionPhase.VALIDATION];
            var executionPhase = metrics.PhaseEntries[ApolloExecutionPhase.EXECUTION];

            Assert.IsTrue(parsePhase.StartOffsetTicks < parsePhase.EndOffsetTicks);
            Assert.IsTrue(parsePhase.EndOffsetTicks < validationPhase.StartOffsetTicks);

            Assert.IsTrue(validationPhase.StartOffsetTicks < validationPhase.EndOffsetTicks);
            Assert.IsTrue(validationPhase.EndOffsetTicks < executionPhase.StartOffsetTicks);

            Assert.IsTrue(executionPhase.StartOffsetTicks < executionPhase.EndOffsetTicks);
            Assert.IsTrue(executionPhase.EndOffsetTicks < metrics.TotalTicks);

            // ensure resolver tree follows natural progresion
            var simple = metrics.ResolverEntries.First(x => x.Key.Field.Name == "simple").Value;
            var queryMethod = metrics.ResolverEntries.First(x => x.Key.Field.Name == "simpleQueryMethod").Value;
            var property1 = metrics.ResolverEntries.First(x => x.Key.Field.Name == "property1").Value;

            Assert.IsTrue(executionPhase.StartOffsetTicks < simple.StartOffsetTicks);

            Assert.IsTrue(simple.StartOffsetTicks < simple.EndOffsetTicks);
            Assert.IsTrue(simple.EndOffsetTicks < queryMethod.StartOffsetTicks);

            Assert.IsTrue(queryMethod.StartOffsetTicks < queryMethod.EndOffsetTicks);
            Assert.IsTrue(queryMethod.EndOffsetTicks < property1.StartOffsetTicks);

            Assert.IsTrue(property1.StartOffsetTicks < property1.EndOffsetTicks);
            Assert.IsTrue(property1.EndOffsetTicks < executionPhase.EndOffsetTicks);
        }

        [Test]
        public async Task StandardExcution_OutputTest()
        {
            var serverBuilder = new TestServerBuilder(TestOptions.IncludeMetrics);
            serverBuilder.AddGraphType<SimpleExecutionController>();

            serverBuilder.AddGraphQL(o =>
            {
                o.ResponseOptions.ExposeMetrics = true;
            });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query Operation1{  simple {  simpleQueryMethod { property1} } }")
                .AddOperationName("Operation1");

            var metrics = new ApolloTracingMetricsV1(server.Schema);
            builder.AddMetrics(metrics);

            var result = await server.RenderResult(builder);

            var parsePhase = metrics.PhaseEntries[ApolloExecutionPhase.PARSING];
            var validationPhase = metrics.PhaseEntries[ApolloExecutionPhase.VALIDATION];
            var simple = metrics.ResolverEntries.First(x => x.Key.Field.Name == "simple").Value;
            var queryMethod = metrics.ResolverEntries.First(x => x.Key.Field.Name == "simpleQueryMethod").Value;
            var property1 = metrics.ResolverEntries.First(x => x.Key.Field.Name == "property1").Value;

            var expectedResult = @"
                        {
                          ""data"": {
                                    ""simple"": {
                                        ""simpleQueryMethod"": {
                                            ""property1"": ""default string""
                                        }
                                    }
                                },
                                ""extensions"": {
                                    ""tracing"": {
                                        ""version"": 1,
                                        ""startTime"": ""[StartTime]"",
                                        ""endTime"": ""[EndTime]"",
                                        ""duration"": [Duration],
                                        ""parsing"": {
                                            ""startOffset"": [ParsingOffset],
                                            ""duration"": [ParsingDuration]
                                        },
                                        ""validation"": {
                                            ""startOffset"":[ValidationOffset],
                                            ""duration"": [ValidationDuration]
                                        },
                                        ""execution"": {
                                            ""resolvers"": [
                                            {
                                                ""path"": [""simple""],
                                                ""fieldName"": ""simple"",
                                                ""parentType"" : ""Query"",
                                                ""returnType"": ""Query_Simple"",
                                                ""startOffset"": [simpleStartOffset],
                                                ""duration"": [simpleDuration]
                                            },
                                            {
                                                ""path"": [""simple"", ""simpleQueryMethod""],
                                                ""fieldName"": ""simpleQueryMethod"",
                                                ""parentType"": ""Query_Simple"",
                                                ""returnType"": ""TwoPropertyObject"",
                                                ""startOffset"": [SimpleQueryMethodOffset],
                                                ""duration"": [SimpleQueryMethodDuration]
                                            },
                                            {
                                                ""path"": [""simple"", ""simpleQueryMethod"", ""property1""],
                                                ""fieldName"": ""property1"",
                                                ""parentType"": ""TwoPropertyObject"",
                                                ""returnType"": ""String"",
                                                ""startOffset"": [Property1Offset],
                                                ""duration"": [Property1Duration]
                                            }
                                            ]
                                        }
                                    }
                                }
                            }";

            expectedResult = expectedResult.Replace("[StartTime]", metrics.StartDate.ToRfc3339String())
                .Replace("[EndTime]", metrics.StartDate.AddTicks(metrics.TotalTicks).ToRfc3339String())
                .Replace("[Duration]", metrics.TotalTicks.ToString())
                .Replace("[ParsingOffset]", parsePhase.StartOffsetNanoseconds.ToString())
                .Replace("[ParsingDuration]", parsePhase.DurationNanoSeconds.ToString())
                .Replace("[ValidationOffset]", validationPhase.StartOffsetNanoseconds.ToString())
                .Replace("[ValidationDuration]", validationPhase.DurationNanoSeconds.ToString())
                .Replace("[simpleStartOffset]", simple.StartOffsetNanoseconds.ToString())
                .Replace("[simpleDuration]", simple.DurationNanoSeconds.ToString())
                .Replace("[SimpleQueryMethodOffset]", queryMethod.StartOffsetNanoseconds.ToString())
                .Replace("[SimpleQueryMethodDuration]", queryMethod.DurationNanoSeconds.ToString())
                .Replace("[Property1Offset]", property1.StartOffsetNanoseconds.ToString())
                .Replace("[Property1Duration]", property1.DurationNanoSeconds.ToString());

            CommonAssertions.AreEqualJsonStrings(expectedResult, result);
        }

        [Test]
        public void DefaultMetricsFactory_Create()
        {
            var schema = new GraphSchema();
            var factory = new DefaultGraphQueryExecutionMetricsFactory<GraphSchema>(schema);

            var instance = factory.CreateMetricsPackage();
            Assert.IsNotNull(instance);
        }
    }
}