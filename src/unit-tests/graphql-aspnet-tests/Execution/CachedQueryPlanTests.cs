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
    using GraphQL.AspNet.Configuration.Mvc;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Directives.DirectiveTestData;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class CachedQueryPlanTests
    {
        [Test]
        public async Task PlanWithNoExecutionDirectives_IsCached()
        {
            var serverBuilder = new TestServerBuilder<GraphSchema>()
                .AddType<SimpleExecutionController>();

            serverBuilder.AddGraphQLLocalQueryCache();
            var server = serverBuilder.Build();

            var cacheProvider = server.ServiceProvider.GetRequiredService<IGraphQueryPlanCacheProvider>();
            var cacheKeyProvider = server.ServiceProvider.GetRequiredService<IGraphQueryPlanCacheKeyManager>();

            var builder = server.CreateQueryContextBuilder();

            var query = @"
                query Operation1{
                    simple{
                        simpleQueryMethod (arg1: ""a string"") {
                            property1
                        }
                    }
                }";

            var operation = "Operation1";

            builder.AddQueryText(query);
            builder.AddOperationName(operation);

            // supply no values, allowing the defaults to take overand returning the single
            // requested "Property1" with the default string defined on the method.
            var result = await server.ExecuteQuery(builder);

            Assert.IsNotNull(cacheProvider);
            Assert.IsNotNull(cacheKeyProvider);
            Assert.AreEqual(0, result.Messages.Count);
            Assert.IsNotNull(result.Data);

            var key = cacheKeyProvider.CreateKey<GraphSchema>(query, operation);
            var found = await cacheProvider.TryGetPlanAsync(key, out var plan);

            Assert.IsTrue(found);
            Assert.IsNotNull(plan);
        }

        [Test]
        public async Task PlanWithExecutionDirectives_IsNotCached()
        {
            var serverBuilder = new TestServerBuilder<GraphSchema>()
                .AddType<SimpleExecutionController>();

            serverBuilder.AddGraphQLLocalQueryCache();
            var server = serverBuilder.Build();

            var cacheProvider = server.ServiceProvider.GetRequiredService<IGraphQueryPlanCacheProvider>();
            var cacheKeyProvider = server.ServiceProvider.GetRequiredService<IGraphQueryPlanCacheKeyManager>();

            var builder = server.CreateQueryContextBuilder();

            var query = @"
                query Operation1{
                    simple{
                        simpleQueryMethod (arg1: ""a string"") {
                            property1 @include(if: true)
                        }
                    }
                }";

            var operation = "Operation1";

            builder.AddQueryText(query);
            builder.AddOperationName(operation);

            // supply no values, allowing the defaults to take overand returning the single
            // requested "Property1" with the default string defined on the method.
            var result = await server.ExecuteQuery(builder);

            Assert.IsNotNull(cacheProvider);
            Assert.IsNotNull(cacheKeyProvider);
            Assert.AreEqual(0, result.Messages.Count);
            Assert.IsNotNull(result.Data);

            var key = cacheKeyProvider.CreateKey<GraphSchema>(query, operation);
            var found = await cacheProvider.TryGetPlanAsync(key, out var plan);

            Assert.IsFalse(found);
            Assert.IsNull(plan);
        }
    }
}