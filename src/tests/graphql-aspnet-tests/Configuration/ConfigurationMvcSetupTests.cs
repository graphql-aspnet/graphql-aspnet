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
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration.Mvc;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.Middleware.QueryExecution;
    using GraphQL.AspNet.Parsing;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Configuration.ConfigurationTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.ThirdPartyDll;
    using GraphQL.AspNet.Tests.ThirdPartyDll.Model;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class ConfigurationMvcSetupTests
    {
        [SetUp]
        public void Setup()
        {
            GraphQLProviders.TemplateProvider.CacheTemplates = true;
        }

        [TearDown]
        public void TearDown()
        {
            GraphQLMvcSchemaBuilderExtensions.Clear();
            GraphQLProviders.TemplateProvider.Clear();
            GraphQLProviders.TemplateProvider.CacheTemplates = false;
        }

        [Test]
        public void AddGraphQL_AddingDefaultSchema_WithOneController_GeneratesAllDefaultEngineParts()
        {
            var serviceCollection = new ServiceCollection();
            var returned = serviceCollection.AddGraphQL(options =>
            {
                options.AddGraphType<FanController>();
            });

            var sp = serviceCollection.BuildServiceProvider();
            var controller = sp.GetService(typeof(FanController));
            Assert.IsNotNull(controller);

            Assert.IsNotNull(sp.GetService(typeof(IGraphQLHttpProcessor<GraphSchema>)));

            Assert.IsNotNull(sp.GetService(typeof(ISchemaPipeline<GraphSchema, GraphFieldExecutionContext>)));
            Assert.IsNotNull(sp.GetService(typeof(ISchemaPipeline<GraphSchema, GraphFieldSecurityContext>)));
            Assert.IsNotNull(sp.GetService(typeof(ISchemaPipeline<GraphSchema, GraphQueryExecutionContext>)));

            // objects injected for by standard pipeline components
            Assert.IsNotNull(sp.GetService(typeof(IGraphQueryPlanGenerator<GraphSchema>)) as DefaultGraphQueryPlanGenerator<GraphSchema>);
            Assert.IsNotNull(sp.GetService(typeof(IGraphResponseWriter<GraphSchema>)) as DefaultResponseWriter<GraphSchema>);
            Assert.IsNotNull(sp.GetService(typeof(IGraphQueryExecutionMetricsFactory<GraphSchema>)) as DefaultGraphQueryExecutionMetricsFactory<GraphSchema>);
            Assert.IsNotNull(sp.GetService(typeof(IGraphQLDocumentParser)) as GraphQLParser);

            GraphQLProviders.TemplateProvider.Clear();
        }

        [Test]
        public void AddGraphQL_AttemptingToAddDefautSchemaAsTyped_ThrowsException()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddGraphQL();

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                serviceCollection.AddGraphQL<GraphSchema>();
            });

            GraphQLProviders.TemplateProvider.Clear();
        }

        [Test]
        public void UseGraphQL_PerformsPreparseAndSetupRoutines()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddGraphQL(options =>
            {
                options.AddGraphType<FanController>();
            });

            var sp = serviceCollection.BuildServiceProvider();
            sp.UseGraphQL();

            // FanController, FanItem, FanSpeed, skipDirective, includeDirective
            Assert.AreEqual(5, GraphQLProviders.TemplateProvider.Count);
        }

        [Test]
        public void SchemaBuilder_AddAssembly_AddGraphAssembly()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddGraphQL(options =>
            {
                options.AddGraphAssembly(Assembly.GetAssembly(typeof(CandleController)));
            });

            var provider = serviceCollection.BuildServiceProvider();
            provider.UseGraphQL();

            // virtualResolvedObject, candleController, candle, waxType,
            // customerController, customer, skipDirective, includeDirective
            Assert.AreEqual(8, GraphQLProviders.TemplateProvider.Count);

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
            // 2 built in directives (skip, include)
            // 1 built in, required graph type (VirtualResolveOdbject)
            Assert.AreEqual(20, schema.KnownTypes.Count);
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

            // virtualResolvedObject, candleController, candle, waxType,
            // customerController, customer, skipDirective, includeDirective
            Assert.AreEqual(8, GraphQLProviders.TemplateProvider.Count);

            var sp = serviceCollection.BuildServiceProvider();
            var schema = sp.GetService(typeof(CandleSchema)) as ISchema;
            Assert.IsNotNull(schema);

            // 2 objects: candle, customer
            // 1 enum: WaxType
            // 2 virtual: Query_candles, Query_customers
            // 3 scalars: int, string, boolean (from introspection)
            // 1 operation type: queryType
            // 8 introspection types
            // 2 built in directives (skip, include)
            // 1 built in, required graph type (VirtualResolvedObject)
            Assert.AreEqual(20, schema.KnownTypes.Count);
        }

        [Test]
        public void SchemaBuilder_AddGraphController_AppearsInSchema()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddGraphQL(options =>
            {
                options.AddGraphType<CandleController>();
            });

            var provider = serviceCollection.BuildServiceProvider();
            provider.UseGraphQL();

            // virtualResolvedObject, candleController, candle, waxType, skipDirective, includeDirective
            Assert.AreEqual(6, GraphQLProviders.TemplateProvider.Count);

            var sp = serviceCollection.BuildServiceProvider();
            var schema = sp.GetService(typeof(GraphSchema)) as ISchema;
            Assert.IsNotNull(schema);

            // 2 objects: candle, candlewax
            // 1 virtual: queryType_candles
            // 3 scalars: int, string, boolean (from introspection)
            // 1 operation type: queryType
            // 8 introspection types
            // 2 built in directives (skip, include)
            // 1 built in, required graph type (VirtualResolveOdbject)
            Assert.AreEqual(18, schema.KnownTypes.Count);
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(Candle)));
            Assert.IsTrue(schema.KnownTypes.Contains($"{Constants.ReservedNames.QUERY_TYPE_NAME}_Candles"));
        }

        [Test]
        public void SchemaBuilder_AddGraphDirective_AppearsInSchema()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddGraphQL(options =>
            {
                options.AddGraphType<Sample1Directive>();
            });

            var provider = serviceCollection.BuildServiceProvider();
            provider.UseGraphQL();

            // sample1Directive, skipDirective, includeDirective
            Assert.AreEqual(3, GraphQLProviders.TemplateProvider.Count);
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
                options.AddGraphType<CandleController>();
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
                options.AddGraphType<CandleController>();
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
            using var restorePoint = new GraphQLProviderRestorePoint();

            GraphQLProviders.GlobalConfiguration = new DefaultGraphQLGLobalConfiguration();

            var originalSetting = GraphQLProviders.GlobalConfiguration.ControllerServiceLifeTime;

            // make sure the original setting is not what we hope to change it to
            // otherwise the test is inconclusive
            if (originalSetting == ServiceLifetime.Singleton)
            {
                Assert.Inconclusive("Unable to determine if the service lifetime was changed. Original and new settings are the same.");
            }

            GraphQLProviders.GlobalConfiguration.ControllerServiceLifeTime = ServiceLifetime.Singleton;

            var serverBuilder = new TestServerBuilder<CandleSchema>();
            var schemaBuilder = serverBuilder.AddGraphQL<CandleSchema>(options =>
            {
                options.AddGraphType<CandleController>();
            });

            var descriptor = serverBuilder.SchemaOptions.ServiceCollection.SingleOrDefault(x => x.ServiceType == typeof(CandleController));

            Assert.AreEqual(ServiceLifetime.Singleton, descriptor.Lifetime);
        }
    }
}