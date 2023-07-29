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
    using GraphQL.AspNet.Configuration.Startup;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Configuration.SchemaBuildTestData;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class GraphSchemaBuilderTests
    {
        [Test]
        public void SimpleSchema_IsRenderedOut()
        {
            var collection = new ServiceCollection();
            collection.AddSingleton<GraphSchema>();

            var provider = collection.BuildServiceProvider();
            var scope = provider.CreateScope();

            var schema = GraphSchemaBuilder.BuildSchema<GraphSchema>(scope.ServiceProvider);

            Assert.IsNotNull(schema);
        }

        [Test]
        public void SchemaWithOneAvailableService_IsRenderedOut()
        {
            var collection = new ServiceCollection();
            collection.AddSingleton<OneServiceSchema>();
            collection.AddSingleton<TestService1>();

            var provider = collection.BuildServiceProvider();
            var scope = provider.CreateScope();

            var schema = GraphSchemaBuilder.BuildSchema<OneServiceSchema>(scope.ServiceProvider);
            Assert.IsNotNull(schema);
        }

        [Test]
        public void MultipleConstructors_ButOnlyOneMatches_CorrectConstructorIsUsed()
        {
            var collection = new ServiceCollection();
            collection.AddSingleton<MultiConstructorSchema>();
            collection.AddSingleton<TestService1>();

            var provider = collection.BuildServiceProvider();
            var scope = provider.CreateScope();

            var schema = GraphSchemaBuilder.BuildSchema<MultiConstructorSchema>(scope.ServiceProvider);
            Assert.IsNotNull(schema);
            Assert.AreEqual(1, schema.PropValue);
        }

        [Test]
        public void MultipleConstructors_ButOnlyOneMatches_ButHasDefaultValues_CorrectConstructorIsUsed()
        {
            var collection = new ServiceCollection();
            collection.AddSingleton<MultiConstructorSchemWithDefaultValues>();
            collection.AddSingleton<TestService1>();

            var provider = collection.BuildServiceProvider();
            var scope = provider.CreateScope();

            var schema = GraphSchemaBuilder.BuildSchema<MultiConstructorSchemWithDefaultValues>(scope.ServiceProvider);
            Assert.IsNotNull(schema);
            Assert.AreEqual(3, schema.PropValue);
        }
    }
}