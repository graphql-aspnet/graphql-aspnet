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
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Tests.Execution.ExecutionPlanTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class MemoryBasedQueryCacheTests
    {
        private IGraphQueryPlan CreatePlan(string queryText)
        {
            var server = new TestServerBuilder()
                .AddGraphType<SimpleExecutionController>()
                .Build();

            var task = server.CreateQueryPlan(queryText);
            task.Wait();
            return task.Result;
        }

        [Test]
        public async Task NotInCache_ReturnsFalse()
        {
            var cache = new DefaultQueryPlanCacheProvider();
            var hash = Guid.NewGuid().ToString("N");
            var found = await cache.TryGetPlanAsync(hash, out var plan);
            Assert.IsNull(plan);
            Assert.IsFalse(found);
        }

        [Test]
        public async Task AddPlanToCache_BecomesInCache()
        {
            var cache = new DefaultQueryPlanCacheProvider();
            var text = "query Operation1{  simple {  simpleQueryMethod { property1 __typename} } }";

            var plan = this.CreatePlan(text);
            var hash = Guid.NewGuid().ToString("N");

            var wasCached = await cache.TryCachePlanAsync(hash, plan);
            Assert.IsTrue(wasCached);

            var found = await cache.TryGetPlanAsync(hash, out var foundPlan);
            Assert.IsTrue(found);
            Assert.AreEqual(plan, foundPlan);
        }

        [Test]
        public async Task ForceEvict_RemovesFromCache()
        {
            var cache = new DefaultQueryPlanCacheProvider();
            var text = "query Operation1{  simple {  simpleQueryMethod { property1 __typename} } }";

            var plan = this.CreatePlan(text);
            var hash = Guid.NewGuid().ToString("N");

            var wasCached = await cache.TryCachePlanAsync(hash, plan);
            Assert.IsTrue(wasCached);

            var evicted = await cache.EvictAsync(hash);
            Assert.IsTrue(evicted);

            var found = await cache.TryGetPlanAsync(hash, out var foundPlan);
            Assert.IsFalse(found);
            Assert.IsNull(foundPlan);
        }
    }
}