// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Configuration
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Configuration.ConfigurationTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.ThirdPartyDll;
    using GraphQL.AspNet.Tests.ThirdPartyDll.Model;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class ConfigurationSetupTests
    {
        [Test]
        public void AddGraphQL_AddingDefaultSchema_WithOneController_GeneratesAllDefaultEngineParts()
        {
            var serviceCollection = new ServiceCollection();
            var returned = serviceCollection.AddGraphQL(options =>
            {
                options.AddType<FanController>();
            });

            var sp = serviceCollection.BuildServiceProvider();
            var controller = sp.GetService(typeof(FanController));
            Assert.IsNotNull(controller);

            Assert.IsNotNull(sp.GetService(typeof(IGraphQLHttpProcessor<GraphSchema>)));

            Assert.IsNotNull(sp.GetService(typeof(ISchemaPipeline<GraphSchema, GraphFieldExecutionContext>)));
            Assert.IsNotNull(sp.GetService(typeof(ISchemaPipeline<GraphSchema, SchemaItemSecurityChallengeContext>)));
            Assert.IsNotNull(sp.GetService(typeof(ISchemaPipeline<GraphSchema, QueryExecutionContext>)));

            // objects injected for by standard pipeline components
            Assert.IsNotNull(sp.GetService(typeof(IQueryExecutionPlanGenerator<GraphSchema>)) as DefaultQueryExecutionPlanGenerator<GraphSchema>);
            Assert.IsNotNull(sp.GetService(typeof(IQueryResponseWriter<GraphSchema>)) as DefaultQueryResponseWriter<GraphSchema>);
            Assert.IsNotNull(sp.GetService(typeof(IQueryExecutionMetricsFactory<GraphSchema>)) as DefaultQueryExecutionMetricsFactory<GraphSchema>);
        }

        [Test]
        public void AddGraphQL_AttemptingToAddDefautSchemaAsTyped_ThrowsException()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddGraphQL();

            Assert.Throws<InvalidOperationException>(() =>
            {
                serviceCollection.AddGraphQL<GraphSchema>();
            });
        }

        [Test]
        public void SchemaBuilder_AddAssembly_AddGraphAssembly()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddGraphQL(options =>
            {
                options.AddAssembly(Assembly.GetAssembly(typeof(CandleController)));
            });

            var provider = serviceCollection.BuildServiceProvider();
            provider.UseGraphQL();

            var sp = serviceCollection.BuildServiceProvider();
            sp.UseGraphQL();

            var schema = sp.GetService(typeof(GraphSchema)) as ISchema;
            Assert.IsNotNull(schema);

            // 2 objects: candle, customer
            // 1 enum: WaxType
            // 2 virtual: Query_candles, Query_customers
            // 3 scalars: int, string, boolean (from introspection)
            // 1 operation type: queryType
            // 8 introspection types
            // 4 built in directives (skip, include, deprecated, specifiedBy)
            // 1 built in, required graph type (VirtualResolveOdbject)
            Assert.AreEqual(22, schema.KnownTypes.Count);
        }

        [Test]
        public void SchemaBuilder_AddSchemaAssembly_AllControllersAddedToType()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddGraphQL<CandleSchema>(options =>
            {
                options.AddSchemaAssembly();
            });

            var provider = serviceCollection.BuildServiceProvider();
            provider.UseGraphQL();

            var sp = serviceCollection.BuildServiceProvider();
            var schema = sp.GetService(typeof(CandleSchema)) as ISchema;
            Assert.IsNotNull(schema);

            // 2 objects: candle, customer
            // 1 enum: WaxType
            // 2 virtual: Query_candles, Query_customers
            // 3 scalars: int, string, boolean (from introspection)
            // 1 operation type: queryType
            // 8 introspection types
            // 4 built in directives (skip, include, deprecated, specifiedBy)
            // 1 built in, required graph type (VirtualResolvedObject)
            Assert.AreEqual(22, schema.KnownTypes.Count);
        }

        [Test]
        public void SchemaBuilder_AddGraphController_AppearsInSchema()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddGraphQL(options =>
            {
                options.AddType<CandleController>();
            });

            var provider = serviceCollection.BuildServiceProvider();
            provider.UseGraphQL();

            var sp = serviceCollection.BuildServiceProvider();
            var schema = sp.GetService(typeof(GraphSchema)) as ISchema;
            Assert.IsNotNull(schema);

            // 2 objects: candle, candlewax
            // 1 virtual: queryType_candles
            // 3 scalars: int, string, boolean (from introspection)
            // 1 operation type: queryType
            // 8 introspection types
            // 4 built in directives (skip, include, deprecated,specifiedByDirective)
            // 1 built in, required graph type (VirtualResolveOdbject)
            Assert.AreEqual(20, schema.KnownTypes.Count);
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(Candle)));
            Assert.IsTrue(schema.KnownTypes.Contains($"{Constants.ReservedNames.QUERY_TYPE_NAME}_Candles"));
        }

        [Test]
        public void SchemaBuilder_AddGraphDirective_AppearsInSchema()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddGraphQL(options =>
            {
                options.AddType<Sample1Directive>();
            });

            var provider = serviceCollection.BuildServiceProvider();
            provider.UseGraphQL();

            // sample1Directive,
            // skipDirective, includeDirective, deprecatedDirective, specifiedByDirective
            var schema = provider.GetService(typeof(GraphSchema)) as GraphSchema;

            Assert.IsNotNull(schema);
            Assert.IsNotNull(schema.KnownTypes.FindGraphType(typeof(Sample1Directive), TypeKind.DIRECTIVE));
        }

        [Test]
        public void SchemaBuilder_AddGraphType_AppearsInSchema()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddGraphQL<CandleSchema>(options =>
            {
                options.AddGraphType<Candle>();
                options.AddGraphType<Customer>();
            });

            var sp = serviceCollection.BuildServiceProvider();
            sp.UseGraphQL();

            var schema = sp.GetService(typeof(CandleSchema)) as ISchema;
            Assert.IsNotNull(schema);

            Assert.IsTrue(schema.KnownTypes.Contains(typeof(Candle)));
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(Customer)));
        }

        [Test]
        public async Task SchemaBuilder_AddMiddleware_AsFunctionRegistration_AppearsInPipeline()
        {
            int counter = 0;
            var serverBuilder = new TestServerBuilder<CandleSchema>();
            var builder = serverBuilder.AddGraphQL<CandleSchema>(options =>
            {
                options.AddType<CandleController>();
            });

            builder.FieldExecutionPipeline.AddMiddleware((req, next, token) =>
                {
                    counter++;
                    return next(req, token);
                });

            var server = serverBuilder.Build();

            var queryBuilder = server.CreateQueryContextBuilder();
            queryBuilder.AddQueryText("{candles{ candle(id: 18){ name }}}");
            var result = await server.ExecuteQuery(queryBuilder);

            Assert.IsTrue(result.Data != null);

            // resolution of fields "queryType_candles", "candle" and "name"
            // means the middleware should be called 3x
            Assert.AreEqual(3, counter);
        }

        [Test]
        public async Task SchemaBuilder_AddMiddleware_AsTypeRegistration_AppearsInPipeline()
        {
            var serverBuilder = new TestServerBuilder<CandleSchema>();
            var schemaBuilder = serverBuilder.AddGraphQL<CandleSchema>(options =>
            {
                options.AddType<CandleController>();
            });

            schemaBuilder.FieldExecutionPipeline.AddMiddleware<CandleMiddleware>(ServiceLifetime.Singleton, "Candle middleware");

            var server = serverBuilder.Build();

            var queryBuilder = server.CreateQueryContextBuilder();
            queryBuilder.AddQueryText("{candles{ candle(id: 18){ name }}}");
            var result = await server.ExecuteQuery(queryBuilder);

            Assert.IsTrue(result.Data != null);

            // resolution of fields "queryType_candles", "candle" and "name"
            // means the middleware should be called 3x
            Assert.AreEqual(3, CandleMiddleware.Counter);
        }

        [Test]
        public void ChangingGlobalConfig_ChangesHowControllersAreRegistered()
        {
            // make sure the original setting is not what we hope to change it to
            // otherwise the test is inconclusive
            if (GraphQLServerSettings.ControllerServiceLifeTime == ServiceLifetime.Singleton)
            {
                Assert.Inconclusive("Unable to determine if the service lifetime was changed. Original and new settings are the same.");
            }

            GraphQLServerSettings.ControllerServiceLifeTime = ServiceLifetime.Singleton;

            var serverBuilder = new TestServerBuilder<CandleSchema>();
            var schemaBuilder = serverBuilder.AddGraphQL<CandleSchema>(options =>
            {
                options.AddType<CandleController>();
            });

            var descriptor = serverBuilder.SchemaOptions.ServiceCollection.SingleOrDefault(x => x.ServiceType == typeof(CandleController));

            Assert.AreEqual(ServiceLifetime.Singleton, descriptor.Lifetime);
        }
    }
}