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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Tests.Execution.TestData.ExecutionPlanTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class MemoryBasedQueryCacheTests
    {
        private IQueryExecutionPlan CreatePlan(string queryText)
        {
            var server = new TestServerBuilder()
                .AddType<SimpleExecutionController>()
                .Build();

            var task = server.CreateQueryPlan(queryText);
            task.Wait();
            return task.Result;
        }

        [Test]
        public async Task NotInCache_ReturnsFalse()
        {
            using var cache = new DefaultQueryExecutionPlanCacheProvider();
            var hash = Guid.NewGuid().ToString("N");

            var foundPlan = await cache.TryGetPlanAsync(hash);
            Assert.IsNull(foundPlan);
        }

        [Test]
        public async Task AddPlanToCache_BecomesInCache()
        {
            var cache = new DefaultQueryExecutionPlanCacheProvider();
            var text = "query Operation1{  simple {  simpleQueryMethod { property1 __typename} } }";

            var plan = this.CreatePlan(text);
            var hash = Guid.NewGuid().ToString("N");

            var wasCached = await cache.TryCachePlanAsync(hash, plan);
            Assert.IsTrue(wasCached);

            var foundPlan = await cache.TryGetPlanAsync(hash);
            Assert.AreEqual(plan, foundPlan);
        }

        [Test]
        public async Task ForceEvict_RemovesFromCache()
        {
            var cache = new DefaultQueryExecutionPlanCacheProvider();
            var text = "query Operation1{  simple {  simpleQueryMethod { property1 __typename} } }";

            var plan = this.CreatePlan(text);
            var hash = Guid.NewGuid().ToString("N");

            var wasCached = await cache.TryCachePlanAsync(hash, plan);
            Assert.IsTrue(wasCached);

            var evicted = await cache.EvictAsync(hash);
            Assert.IsTrue(evicted);

            var foundPlan = await cache.TryGetPlanAsync(hash);
            Assert.IsNull(foundPlan);
        }
    }
}