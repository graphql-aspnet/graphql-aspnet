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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution.Metrics;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Execution.BatchResolverTestData;
    using GraphQL.AspNet.Tests.Execution.ExecutionPlanTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
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

        [Test]
        public async Task Tracing_ThroughBatchTypeExtension_WithSingleObjectPerSourceResult_YieldsCorrectResults()
        {
            var counter = new Dictionary<string, int>();

            var serverBuilder = new TestServerBuilder(TestOptions.IncludeMetrics);
            serverBuilder.AddGraphType<BatchController>();

            serverBuilder.AddGraphQL(o =>
            {
                o.ResponseOptions.ExposeMetrics = true;
            });

            var batchService = new Mock<IBatchCounterService>();
            batchService.Setup(x => x.CallCount).Returns(counter);

            serverBuilder.AddSingleton(batchService.Object);
            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText("query { batch { fetchData { property1, sybling { syblingId }}}}");

            var metrics = new ApolloTracingMetricsV1(server.Schema);
            builder.AddMetrics(metrics);

            var result = await server.RenderResult(builder);

            var expectedResult = @"
	            {
	              ""data"": {
		            ""batch"": {
					            ""fetchData"": [
					              {
						            ""property1"": ""object0"",
						            ""sybling"": {""syblingId"": ""object0""}
					              },
					              {
						            ""property1"": ""object1"",
						            ""sybling"": {""syblingId"": ""object1""}
					              },
					              {
						            ""property1"": ""object2"",
						            ""sybling"": {""syblingId"": ""object2""}
					              }
					            ]
		            }
	              },
	              ""extensions"": {
		            ""tracing"": {
		              ""version"": 1,
		              ""startTime"": ""<anyValue>"",
		              ""endTime"": ""<anyValue>"",
		              ""duration"": ""<anyValue>"",
		              ""parsing"": {
                            ""startOffset"": ""<anyValue>"",
                            ""duration"": ""<anyValue>""
                      },
                      ""validation"": {
                            ""startOffset"":""<anyValue>"",
                            ""duration"": ""<anyValue>""
                      },
		              ""execution"": {
			            ""resolvers"": [
			              {
				            ""path"": [ ""batch""],
				            ""parentType"": ""Query"",
				            ""fieldName"": ""batch"",
				            ""returnType"": ""Query_Batch"",
				            ""startOffset"": ""<anyValue>"",
				            ""duration"": ""<anyValue>""
			              },
			              {
				            ""path"": [""batch"",""fetchData""],
				            ""parentType"": ""Query_Batch"",
				            ""fieldName"": ""fetchData"",
				            ""returnType"": ""[TwoPropertyObject]"",
				            ""startOffset"": ""<anyValue>"",
				            ""duration"": ""<anyValue>""
			              },
			              {
				            ""path"": [""batch"",""fetchData"",0,""property1""],
				            ""parentType"": ""TwoPropertyObject"",
				            ""fieldName"": ""property1"",
				            ""returnType"": ""String"",
				            ""startOffset"": ""<anyValue>"",
				            ""duration"": ""<anyValue>""
			              },
			              {
				            ""path"": [""batch"",""fetchData"",1,""property1""],
				            ""parentType"": ""TwoPropertyObject"",
				            ""fieldName"": ""property1"",
				            ""returnType"": ""String"",
				            ""startOffset"": ""<anyValue>"",
				            ""duration"": ""<anyValue>""
			              },
			              {
				            ""path"": [""batch"",""fetchData"",2,""property1""],
				            ""parentType"": ""TwoPropertyObject"",
				            ""fieldName"": ""property1"",
				            ""returnType"": ""String"",
				            ""startOffset"": ""<anyValue>"",
				            ""duration"": ""<anyValue>""
			              },
			              {
				            ""path"": [""batch"",""fetchData"",""sybling""],
				            ""parentType"": ""TwoPropertyObject"",
				            ""fieldName"": ""sybling"",
				            ""returnType"": ""SyblingTestObject"",
				            ""startOffset"": ""<anyValue>"",
				            ""duration"": ""<anyValue>""
			              },
			              {
				            ""path"": [""batch"",""fetchData"",0,""sybling"",""syblingId""],
				            ""parentType"": ""SyblingTestObject"",
				            ""fieldName"": ""syblingId"",
				            ""returnType"": ""String"",
				            ""startOffset"": ""<anyValue>"",
				            ""duration"": ""<anyValue>""
			              },
			              {
				            ""path"": [""batch"",""fetchData"",1,""sybling"",""syblingId""],
				            ""parentType"": ""SyblingTestObject"",
				            ""fieldName"": ""syblingId"",
				            ""returnType"": ""String"",
				            ""startOffset"": ""<anyValue>"",
				            ""duration"": ""<anyValue>""
			              },
			              {
				            ""path"": [""batch"",""fetchData"",2,""sybling"",""syblingId""],
				            ""parentType"": ""SyblingTestObject"",
				            ""fieldName"": ""syblingId"",
				            ""returnType"": ""String"",
				            ""startOffset"": ""<anyValue>"",
				            ""duration"": ""<anyValue>""
			              }
			            ]
		              }
		            }
	              }
	            }";

            CommonAssertions.AreEqualJsonStrings(expectedResult, result);
        }

        [Test]
        public async Task Tracing_ThroughBatchTypeExtension_WithMultiObjectPerSourceResult_YieldsCorrectResults()
        {
            var counter = new Dictionary<string, int>();

            var serverBuilder = new TestServerBuilder(TestOptions.IncludeMetrics);
            serverBuilder.AddGraphType<BatchController>();

            serverBuilder.AddGraphQL(o =>
            {
                o.ResponseOptions.ExposeMetrics = true;
            });

            var batchService = new Mock<IBatchCounterService>();
            batchService.Setup(x => x.CallCount).Returns(counter);

            serverBuilder.AddSingleton(batchService.Object);
            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText("query { batch { fetchData { property1, kids { parentId, name }}}}");

            var metrics = new ApolloTracingMetricsV1(server.Schema);
            builder.AddMetrics(metrics);

            var result = await server.RenderResult(builder);

            var expectedResult = @"
	          {
              ""data"": {
                ""batch"": {
                ""fetchData"": [
                    {
                        ""property1"": ""object0"",
                        ""kids"": [
                        {
                            ""parentId"": ""object0"",
                            ""name"": ""object0_child_0""
                        },
                        {
                            ""parentId"": ""object0"",
                            ""name"": ""object0_child_1""
                        }
                        ]
                    },
                    {
                        ""property1"": ""object1"",
                        ""kids"": [
                        {
                            ""parentId"": ""object1"",
                            ""name"": ""object1_child_0""
                        },
                        {
                            ""parentId"": ""object1"",
                            ""name"": ""object1_child_1""
                        }
                        ]
                    },
                    {
                        ""property1"": ""object2"",
                        ""kids"": [
                        {
                            ""parentId"": ""object2"",
                            ""name"": ""object2_child_0""
                        },
                        {
                            ""parentId"": ""object2"",
                            ""name"": ""object2_child_1""
                        }
                        ]
                    }
                    ]
                }
              },
              ""extensions"": {
                ""tracing"": {
                  ""version"": 1,
                  ""startTime"": ""<anyValue>"",
                  ""endTime"": ""<anyValue>"",
                  ""duration"": ""<anyValue>"",
                  ""parsing"": {
                    ""startOffset"": ""<anyValue>"",
                    ""duration"": ""<anyValue>""
                  },
                  ""validation"": {
                    ""startOffset"": ""<anyValue>"",
                    ""duration"": ""<anyValue>""
                  },
                  ""execution"": {
                    ""resolvers"": [
                      {
                        ""path"": [""batch""],
                        ""parentType"": ""Query"",
                        ""fieldName"": ""batch"",
                        ""returnType"": ""Query_Batch"",
                        ""startOffset"": ""<anyValue>"",
                        ""duration"": ""<anyValue>""
                      },
                      {
                        ""path"": [""batch"", ""fetchData""],
                        ""parentType"": ""Query_Batch"",
                        ""fieldName"": ""fetchData"",
                        ""returnType"": ""[TwoPropertyObject]"",
                        ""startOffset"": ""<anyValue>"",
                        ""duration"": ""<anyValue>""
                      },
                      {
                        ""path"": [""batch"", ""fetchData"", 0, ""property1""],
                        ""parentType"": ""TwoPropertyObject"",
                        ""fieldName"": ""property1"",
                        ""returnType"": ""String"",
                        ""startOffset"": ""<anyValue>"",
                        ""duration"": ""<anyValue>""
                      },
                      {
                        ""path"": [""batch"", ""fetchData"", 1, ""property1""],
                        ""parentType"": ""TwoPropertyObject"",
                        ""fieldName"": ""property1"",
                        ""returnType"": ""String"",
                        ""startOffset"": ""<anyValue>"",
                        ""duration"": ""<anyValue>""
                      },
                      {
                        ""path"": [""batch"", ""fetchData"", 2, ""property1""],
                        ""parentType"": ""TwoPropertyObject"",
                        ""fieldName"": ""property1"",
                        ""returnType"": ""String"",
                        ""startOffset"": ""<anyValue>"",
                        ""duration"": ""<anyValue>""
                      },
                      {
                        ""path"": [""batch"", ""fetchData"", ""kids""],
                        ""parentType"": ""TwoPropertyObject"",
                        ""fieldName"": ""kids"",
                        ""returnType"": ""[ChildTestObject]"",
                        ""startOffset"": ""<anyValue>"",
                        ""duration"": ""<anyValue>""
                      },
                      {
                        ""path"": [""batch"", ""fetchData"", 0, ""kids"", 0, ""parentId""],
                        ""parentType"": ""ChildTestObject"",
                        ""fieldName"": ""parentId"",
                        ""returnType"": ""String"",
                        ""startOffset"": ""<anyValue>"",
                        ""duration"": ""<anyValue>""
                      },
                      {
                      ""path"": [""batch"", ""fetchData"", 0, ""kids"", 1, ""parentId""],
                        ""parentType"": ""ChildTestObject"",
                        ""fieldName"": ""parentId"",
                        ""returnType"": ""String"",
                        ""startOffset"": ""<anyValue>"",
                        ""duration"": ""<anyValue>""
                      },
                      {
                      ""path"": [""batch"", ""fetchData"", 1, ""kids"", 0, ""parentId""],
                        ""parentType"": ""ChildTestObject"",
                        ""fieldName"": ""parentId"",
                        ""returnType"": ""String"",
                        ""startOffset"": ""<anyValue>"",
                        ""duration"": ""<anyValue>""
                      },
                      {
                      ""path"": [""batch"", ""fetchData"", 1, ""kids"", 1, ""parentId""],
                        ""parentType"": ""ChildTestObject"",
                        ""fieldName"": ""parentId"",
                        ""returnType"": ""String"",
                        ""startOffset"": ""<anyValue>"",
                        ""duration"": ""<anyValue>""
                      },
                      {
                        ""path"": [""batch"", ""fetchData"", 2, ""kids"", 0, ""parentId""],
                        ""parentType"": ""ChildTestObject"",
                        ""fieldName"": ""parentId"",
                        ""returnType"": ""String"",
                        ""startOffset"": ""<anyValue>"",
                        ""duration"": ""<anyValue>""
                      },
                      {
                        ""path"": [""batch"", ""fetchData"", 2, ""kids"", 1, ""parentId""],
                        ""parentType"": ""ChildTestObject"",
                        ""fieldName"": ""parentId"",
                        ""returnType"": ""String"",
                        ""startOffset"": ""<anyValue>"",
                        ""duration"": ""<anyValue>""
                      },
                      {
                        ""path"": [""batch"", ""fetchData"", 0, ""kids"", 0, ""name""],
                        ""parentType"": ""ChildTestObject"",
                        ""fieldName"": ""name"",
                        ""returnType"": ""String"",
                        ""startOffset"": ""<anyValue>"",
                        ""duration"": ""<anyValue>""
                      },
                      {
                         ""path"": [""batch"", ""fetchData"", 0, ""kids"", 1, ""name""],
                        ""parentType"": ""ChildTestObject"",
                        ""fieldName"": ""name"",
                        ""returnType"": ""String"",
                        ""startOffset"": ""<anyValue>"",
                        ""duration"": ""<anyValue>""
                      },
                      {
                        ""path"": [""batch"", ""fetchData"", 1, ""kids"", 0, ""name""],
                        ""parentType"": ""ChildTestObject"",
                        ""fieldName"": ""name"",
                        ""returnType"": ""String"",
                        ""startOffset"": ""<anyValue>"",
                        ""duration"": ""<anyValue>""
                      },
                      {
                        ""path"": [""batch"", ""fetchData"", 1, ""kids"", 1, ""name""],
                        ""parentType"": ""ChildTestObject"",
                        ""fieldName"": ""name"",
                        ""returnType"": ""String"",
                        ""startOffset"": ""<anyValue>"",
                        ""duration"": ""<anyValue>""
                      },
                      {
                        ""path"": [""batch"", ""fetchData"", 2, ""kids"", 0, ""name""],
                        ""parentType"": ""ChildTestObject"",
                        ""fieldName"": ""name"",
                        ""returnType"": ""String"",
                        ""startOffset"": ""<anyValue>"",
                        ""duration"": ""<anyValue>""
                      },
                      {
                        ""path"": [""batch"", ""fetchData"", 2, ""kids"", 1, ""name""],
                        ""parentType"": ""ChildTestObject"",
                        ""fieldName"": ""name"",
                        ""returnType"": ""String"",
                        ""startOffset"": ""<anyValue>"",
                        ""duration"": ""<anyValue>""
                      }
                    ]
                  }
                }
              }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedResult, result);
        }
    }
}