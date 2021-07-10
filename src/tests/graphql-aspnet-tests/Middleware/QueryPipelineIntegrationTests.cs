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
    using System;
    using System.Runtime.Caching;
    using System.Security.Policy;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.Middleware.FieldExecution.Components;
    using GraphQL.AspNet.Middleware.QueryExecution;
    using GraphQL.AspNet.Parsing;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.AspNet.Tests.Middleware.QueryPipelineIntegrationTestData;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
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

        [Test]
        public async Task ExceptionThrownByChildFieldExecution_IsCapturedByParent()
        {
            var server = new TestServerBuilder()
                .AddGraphType<SimpleExecutionController>()
                .AddSchemaBuilderAction(a =>
                {
                    a.FieldExecutionPipeline.AddMiddleware(new ForceExceptionForProperty1Middlware());
                })
                .Build();

            // with the field execution pipeline addition
            // this query should run normally but resolving property results
            // in a random exception
            // the master query should capture and report this exception
            var query = server.CreateQueryContextBuilder()
                .AddQueryText("query  { simple {simpleQueryMethod { property1 }}}")
                .Build();

            await server.ExecuteQuery(query);

            Assert.IsFalse(query.Messages.IsSucessful);
            Assert.AreEqual(1, query.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.EXECUTION_ERROR, query.Messages[0].Code);
            Assert.IsNotNull(query.Messages[0].Exception);
            Assert.AreEqual(GraphMessageSeverity.Critical, query.Messages[0].Severity);
            Assert.AreEqual("Forced Exception for testing", query.Messages[0].Exception.Message);
        }
    }
}