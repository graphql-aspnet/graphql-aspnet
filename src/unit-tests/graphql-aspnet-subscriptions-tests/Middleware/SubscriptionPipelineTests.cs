// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Middleware;
    using GraphQL.AspNet.Middleware.QueryExecution;
    using GraphQL.AspNet.Middleware.QueryExecution.Components;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Security;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    using ExecutionPipelineBuilder = GraphQL.AspNet.Interfaces.Configuration.ISchemaPipelineBuilder<GraphQL.AspNet.Schemas.GraphSchema, GraphQL.AspNet.Interfaces.Middleware.IQueryExecutionMiddleware, GraphQL.AspNet.Execution.Contexts.QueryExecutionContext>;

    [TestFixture]
    public class SubscriptionPipelineTests
    {
        private class FakePipelineBuilder : ExecutionPipelineBuilder
        {
            public FakePipelineBuilder()
            {
                this.ComponentsAdded = new List<Type>();
            }

            public ExecutionPipelineBuilder AddMiddleware(IQueryExecutionMiddleware middlewareInstance, string name = null)
            {
                this.ComponentsAdded.Add(middlewareInstance.GetType());
                return this;
            }

            public ExecutionPipelineBuilder AddMiddleware(Func<QueryExecutionContext, GraphMiddlewareInvocationDelegate<QueryExecutionContext>, CancellationToken, Task> operation, string name = null)
                => throw new NotImplementedException();

            public ISchemaPipeline<GraphSchema, QueryExecutionContext> Build() => throw new NotImplementedException();

            public ExecutionPipelineBuilder Clear()
                => throw new NotImplementedException();

            ExecutionPipelineBuilder ExecutionPipelineBuilder.AddMiddleware<TComponent>(ServiceLifetime lifetime, string name)
            {
                this.ComponentsAdded.Add(typeof(TComponent));
                return this;
            }

            ExecutionPipelineBuilder ExecutionPipelineBuilder.AddMiddleware<TComponent>(Func<IServiceProvider, IQueryExecutionMiddleware> instanceFactory, ServiceLifetime lifetime, string name)
                => throw new NotImplementedException();

            public List<Type> ComponentsAdded { get; }

            public int Count => this.ComponentsAdded.Count;
        }

        /// <summary>
        /// A sanity check test to ensure that the base execution pipeline helper
        /// and the subscription helper differ by only the subscription middleware component.
        /// </summary>
        [Test]
        public void BasePipelineAndSubscriptionPipeline_DifferByOnlySubscriptionComponent()
        {
            var options = new SchemaOptions<GraphSchema>(new ServiceCollection());
            options.AuthorizationOptions.Method = AuthorizationMethod.PerRequest;

            var basePipeline = new FakePipelineBuilder();
            var baselineHelper = new QueryExecutionPipelineHelper<GraphSchema>(basePipeline);

            var subscriptionPipeline = new FakePipelineBuilder();
            var subscriptiohHelper = new SubscriptionQueryExecutionPipelineHelper<GraphSchema>(subscriptionPipeline);

            // populate some fake builders with the default components of each helper
            baselineHelper.AddDefaultMiddlewareComponents(options);
            subscriptiohHelper.AddDefaultMiddlewareComponents(options);

            // ensure that they different in type and order by only the extra subscription
            // component that gets added
            Assert.AreEqual(basePipeline.Count + 1, subscriptionPipeline.Count);
            var indexofBaseLine = 0;
            for (var i = 0; i < subscriptionPipeline.Count; i++)
            {
                var subscriptionComponent = subscriptionPipeline.ComponentsAdded[i];

                if (subscriptionComponent == typeof(SubscriptionCreationMiddleware<GraphSchema>))
                    continue;

                var baseComponent = basePipeline.ComponentsAdded[indexofBaseLine++];

                Assert.AreEqual(baseComponent, subscriptionComponent);
            }
        }
    }
}