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
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Tests.Execution.BatchResolverTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using Moq;
    using NUnit.Framework;
    using Microsoft.Extensions.DependencyInjection;
    using GraphQL.AspNet.Common.Extensions;

    [TestFixture]
    public class TypeExtensionExecutionTests
    {
        public class InheritsFromDictionary : Dictionary<TwoPropertyObject, TwoPropertyObjectV2>
        {
        }

        [TestCase(typeof(Dictionary<TwoPropertyObject, TwoPropertyObjectV2>), typeof(TwoPropertyObject), typeof(TwoPropertyObject), false)]
        [TestCase(typeof(IDictionary<TwoPropertyObject, TwoPropertyObjectV2>), typeof(TwoPropertyObject), typeof(TwoPropertyObject), false)]
        [TestCase(typeof(IDictionary<TwoPropertyObject, TwoPropertyObjectV2>), typeof(TwoPropertyObjectV2), typeof(TwoPropertyObjectV2), false)]
        [TestCase(typeof(IDictionary<List<TwoPropertyObject>, TwoPropertyObjectV2>), typeof(TwoPropertyObject), typeof(TwoPropertyObjectV2), false)]
        [TestCase(typeof(IDictionary<TwoPropertyObject, List<TwoPropertyObject>>), typeof(TwoPropertyObject), typeof(TwoPropertyObjectV2), false)]
        [TestCase(null, typeof(TwoPropertyObject), typeof(TwoPropertyObjectV2), false)]
        [TestCase(typeof(IDictionary<TwoPropertyObject, List<TwoPropertyObject>>), null, typeof(TwoPropertyObjectV2), false)]
        [TestCase(typeof(IDictionary<TwoPropertyObject, List<TwoPropertyObject>>), typeof(TwoPropertyObject), null, false)]
        [TestCase(typeof(InheritsFromDictionary), typeof(TwoPropertyObject), typeof(TwoPropertyObject), false)]

        [TestCase(typeof(InheritsFromDictionary), typeof(TwoPropertyObject), typeof(TwoPropertyObjectV2), true)]
        [TestCase(typeof(IDictionary<TwoPropertyObject, TwoPropertyObjectV2>), typeof(TwoPropertyObject), typeof(TwoPropertyObjectV2), true)]
        [TestCase(typeof(IDictionary<TwoPropertyObject, List<TwoPropertyObjectV2>>), typeof(TwoPropertyObject), typeof(TwoPropertyObjectV2), true)]
        [TestCase(typeof(Dictionary<TwoPropertyObject, TwoPropertyObjectV2>), typeof(TwoPropertyObject), typeof(TwoPropertyObjectV2), true)]
        public void IsBatchDictionary(Type typeToTest, Type typeOfKey, Type typeOfResult, bool expectedResult)
        {
            var str = typeToTest?.FriendlyName();
            var isValid = BatchResultProcessor.IsBatchDictionaryType(typeToTest, typeOfKey, typeOfResult);
            Assert.AreEqual(expectedResult, isValid);
        }

        [Test]
        public async Task BatchExtension_ToAnObject_SingleQueryForMultipleChildren_ExecutesOnce_AndReturnsData()
        {
            var counter = new Dictionary<string, int>();
            var batchService = new Mock<IBatchCounterService>();
            batchService.Setup(x => x.CallCount).Returns(counter);

            var serverBuilder = new TestServerBuilder()
            .AddGraphType<BatchObjectController>();

            serverBuilder.AddSingleton(batchService.Object);
            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText("query { batch { fetchData { property1, property2, kids { parentId, name }}}}");

            var response = await server.RenderResult(builder);

            var expected = @"
                {
                  ""data"": {
                    ""batch"": {
                            ""fetchData"": [
                            {
                                ""property1"": ""object0"",
                                ""property2"": 0,
                                ""kids"": [
                                {
                                    ""parentId"": ""object0"",
                                    ""name"": ""object0_child_0""
                                },
                                {
                                    ""parentId"": ""object0"",
                                    ""name"": ""object0_child_1""
                                }]
                            },
                            {
                                ""property1"": ""object1"",
                                ""property2"": 1,
                                ""kids"": [
                                {
                                    ""parentId"": ""object1"",
                                    ""name"": ""object1_child_0""
                                },
                                {
                                    ""parentId"": ""object1"",
                                    ""name"": ""object1_child_1""
                                }]
                            },
                            {
                                ""property1"": ""object2"",
                                ""property2"": 2,
                                ""kids"": [
                                {
                                    ""parentId"": ""object2"",
                                    ""name"": ""object2_child_0""
                                },
                                {
                                    ""parentId"": ""object2"",
                                    ""name"": ""object2_child_1""
                                }]
                            }]
                        }
                    }
                }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
            Assert.AreEqual(1, counter[nameof(BatchObjectController.PrimaryDataFetch)]);
            Assert.AreEqual(1, counter[nameof(BatchObjectController.FetchChildren)]);
            Assert.IsFalse(counter.ContainsKey(nameof(BatchObjectController.FetchSibling)));
        }

        [Test]
        public async Task BatchExtension_ToAnObject_SingleQueryForSingleSybling_ExecutesOnce_AndReturnsData()
        {
            var counter = new Dictionary<string, int>();
            var batchService = new Mock<IBatchCounterService>();
            batchService.Setup(x => x.CallCount).Returns(counter);

            var serverBuilder = new TestServerBuilder()
                .AddGraphType<BatchObjectController>();
            serverBuilder.AddSingleton(batchService.Object);
            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText("query { batch { fetchData { property1, property2, sybling { syblingId, name }}}}");

            var response = await server.RenderResult(builder);

            var expected = @"
               {
                  ""data"": {
                    ""batch"" : {
                            ""fetchData"": [
                            {
                                ""property1"": ""object0"",
                                ""property2"": 0,
                                ""sybling"": {
                                    ""syblingId"": ""object0"",
                                    ""name"": ""object0_sybling""
                                }
                            },
                            {
                                ""property1"": ""object1"",
                                ""property2"": 1,
                                ""sybling"": {
                                    ""syblingId"": ""object1"",
                                    ""name"": ""object1_sybling""
                                }
                            },
                            {
                                ""property1"": ""object2"",
                                ""property2"": 2,
                                ""sybling"": {
                                    ""syblingId"": ""object2"",
                                    ""name"": ""object2_sybling""
                                }
                            }]
                        }
                    }
                }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
            Assert.AreEqual(1, counter[nameof(BatchObjectController.PrimaryDataFetch)]);
            Assert.IsFalse(counter.ContainsKey(nameof(BatchObjectController.FetchChildren)));
            Assert.AreEqual(1, counter[nameof(BatchObjectController.FetchSibling)]);
        }

        [Test]
        public async Task BatchExtension_ToAStruct_SingleQueryForMultipleChildren_ExecutesOnce_AndReturnsData()
        {
            var counter = new Dictionary<string, int>();
            var batchService = new Mock<IBatchCounterService>();
            batchService.Setup(x => x.CallCount).Returns(counter);

            var serverBuilder = new TestServerBuilder()
            .AddGraphType<BatchStructController>();

            serverBuilder.AddSingleton(batchService.Object);
            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText("query { batch { fetchData { property1, property2, kids { parentId, name }}}}");

            var response = await server.RenderResult(builder);

            var expected = @"
                {
                  ""data"": {
                    ""batch"": {
                            ""fetchData"": [
                            {
                                ""property1"": ""object0"",
                                ""property2"": 0,
                                ""kids"": [
                                {
                                    ""parentId"": ""object0"",
                                    ""name"": ""object0_child_0""
                                },
                                {
                                    ""parentId"": ""object0"",
                                    ""name"": ""object0_child_1""
                                }]
                            },
                            {
                                ""property1"": ""object1"",
                                ""property2"": 1,
                                ""kids"": [
                                {
                                    ""parentId"": ""object1"",
                                    ""name"": ""object1_child_0""
                                },
                                {
                                    ""parentId"": ""object1"",
                                    ""name"": ""object1_child_1""
                                }]
                            },
                            {
                                ""property1"": ""object2"",
                                ""property2"": 2,
                                ""kids"": [
                                {
                                    ""parentId"": ""object2"",
                                    ""name"": ""object2_child_0""
                                },
                                {
                                    ""parentId"": ""object2"",
                                    ""name"": ""object2_child_1""
                                }]
                            }]
                        }
                    }
                }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
            Assert.AreEqual(1, counter[nameof(BatchStructController.PrimaryDataFetch)]);
            Assert.AreEqual(1, counter[nameof(BatchStructController.FetchChildren)]);
            Assert.IsFalse(counter.ContainsKey(nameof(BatchStructController.FetchSibling)));
        }

        [Test]
        public async Task BatchExtension_ToAStruct_SingleQueryForSingleSybling_ExecutesOnce_AndReturnsData()
        {
            var counter = new Dictionary<string, int>();
            var batchService = new Mock<IBatchCounterService>();
            batchService.Setup(x => x.CallCount).Returns(counter);

            var serverBuilder = new TestServerBuilder()
                .AddGraphType<BatchStructController>();
            serverBuilder.AddSingleton(batchService.Object);
            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText("query { batch { fetchData { property1, property2, sybling { syblingId, name }}}}");

            var response = await server.RenderResult(builder);

            var expected = @"
               {
                  ""data"": {
                    ""batch"" : {
                            ""fetchData"": [
                            {
                                ""property1"": ""object0"",
                                ""property2"": 0,
                                ""sybling"": {
                                    ""syblingId"": ""object0"",
                                    ""name"": ""object0_sybling""
                                }
                            },
                            {
                                ""property1"": ""object1"",
                                ""property2"": 1,
                                ""sybling"": {
                                    ""syblingId"": ""object1"",
                                    ""name"": ""object1_sybling""
                                }
                            },
                            {
                                ""property1"": ""object2"",
                                ""property2"": 2,
                                ""sybling"": {
                                    ""syblingId"": ""object2"",
                                    ""name"": ""object2_sybling""
                                }
                            }]
                        }
                    }
                }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
            Assert.AreEqual(1, counter[nameof(BatchStructController.PrimaryDataFetch)]);
            Assert.IsFalse(counter.ContainsKey(nameof(BatchStructController.FetchChildren)));
            Assert.AreEqual(1, counter[nameof(BatchStructController.FetchSibling)]);
        }

        [Test]
        public async Task BatchExtension_ToAnInterface_SingleQueryForMultipleChildren_ExecutesOnce_AndReturnsData()
        {
            var counter = new Dictionary<string, int>();
            var batchService = new Mock<IBatchCounterService>();
            batchService.Setup(x => x.CallCount).Returns(counter);

            var serverBuilder = new TestServerBuilder()
                .AddGraphType<BatchInterfaceController>()
                .AddGraphQL(o =>
                {
                    o.ResponseOptions.ExposeExceptions = true;
                });

            serverBuilder.AddSingleton(batchService.Object);
            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText("query { batch { fetchData { property1, kids { parentId, name }}}}");

            var response = await server.RenderResult(builder);

            var expected = @"
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
                                }]
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
                                }]
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
                                }]
                            }]
                        }
                    }
                }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
            Assert.AreEqual(1, counter[nameof(BatchInterfaceController.PrimaryDataFetch)]);
            Assert.AreEqual(1, counter[nameof(BatchInterfaceController.FetchChildren)]);
            Assert.IsFalse(counter.ContainsKey(nameof(BatchObjectController.FetchSibling)));
        }

        [Test]
        public async Task BatchExtension_ToAInterface_SingleQueryForSingleSybling_ExecutesOnce_AndReturnsData()
        {
            var counter = new Dictionary<string, int>();
            var batchService = new Mock<IBatchCounterService>();
            batchService.Setup(x => x.CallCount).Returns(counter);

            var serverBuilder = new TestServerBuilder()
                .AddGraphType<BatchInterfaceController>();
            serverBuilder.AddSingleton(batchService.Object);
            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText("query { batch { fetchData { property1, sybling { syblingId, name }}}}");

            var response = await server.RenderResult(builder);

            var expected = @"
               {
                  ""data"": {
                    ""batch"" : {
                            ""fetchData"": [
                            {
                                ""property1"": ""object0"",
                                ""sybling"": {
                                    ""syblingId"": ""object0"",
                                    ""name"": ""object0_sybling""
                                }
                            },
                            {
                                ""property1"": ""object1"",
                                ""sybling"": {
                                    ""syblingId"": ""object1"",
                                    ""name"": ""object1_sybling""
                                }
                            },
                            {
                                ""property1"": ""object2"",
                                ""sybling"": {
                                    ""syblingId"": ""object2"",
                                    ""name"": ""object2_sybling""
                                }
                            }]
                        }
                    }
                }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
            Assert.AreEqual(1, counter[nameof(BatchStructController.PrimaryDataFetch)]);
            Assert.IsFalse(counter.ContainsKey(nameof(BatchStructController.FetchChildren)));
            Assert.AreEqual(1, counter[nameof(BatchStructController.FetchSibling)]);
        }
    }
}