// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Middleware
{
    using System.Runtime.Caching;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Middleware.QueryExecution;
    using GraphQL.AspNet.Parsing;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Middleware.QueryPipelineIntegrationTestData;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class QueryPipelineIntegrationTests
    {
        [Test]
        public void NullRequest_YieldsResponseIndicatingNull()
        {
            var server = new TestServerBuilder()
              .AddGraphType<SimpleExecutionController>()
              .Build();

            // standard pipeline should throuhg a GraphExecutionException from
            // the first component in the series
            Assert.ThrowsAsync<GraphExecutionException>(async () =>
            {
                await server.ExecuteQuery(null as GraphQueryExecutionContext);
            });
        }

        [Test]
        public async Task WithAttachedQueryCache_RendersPlanToCache()
        {
            var keyManager = new DefaultQueryPlanCacheKeyManager(new GraphQLParser());

            var cacheInstance = new MemoryCache(nameof(WithAttachedQueryCache_RendersPlanToCache));
            var cache = new DefaultQueryPlanCacheProvider(cacheInstance);
            var builder = new TestServerBuilder()
              .AddGraphType<SimpleExecutionController>();
            builder.AddSingleton<IGraphQueryPlanCacheProvider>(cache);
            builder.AddSingleton<IGraphQueryPlanCacheKeyManager>(keyManager);

             // configure an absolute expriation of a few seconds to ensure the plan remains in cache
             // long enough to be fetched by tis expected key
            builder.AddGraphQL(o =>
            {
                o.CacheOptions.TimeToLiveInMilliseconds = 10000;
            });

            var server = builder.Build();
            var queryText = "query { simple{ simpleQueryMethod { property1} } }";
            var expectedCacheKey = keyManager.CreateKey<GraphSchema>(queryText);

            var queryBuilder = server.CreateQueryContextBuilder();
            queryBuilder.AddQueryText(queryText);
            var result = await server.ExecuteQuery(queryBuilder);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, cache.Count);

            // ensure we can pull the plan directly from the cache instance
            var cachedPlan = cacheInstance.GetCacheItem(expectedCacheKey);
            Assert.IsNotNull(cachedPlan);

            // attempt eviction through the provider
            await cache.EvictAsync(expectedCacheKey);

            // ensure underlying cache instance was evicted
            cachedPlan = cacheInstance.GetCacheItem(expectedCacheKey);
            Assert.IsNull(cachedPlan);
        }
    }
}