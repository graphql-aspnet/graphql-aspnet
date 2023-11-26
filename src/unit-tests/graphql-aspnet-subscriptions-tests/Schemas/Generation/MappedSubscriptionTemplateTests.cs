// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation
{
    using System.Linq;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;
    using GraphQL.AspNet.Schemas;
    using Microsoft.Extensions.DependencyInjection;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class MappedSubscriptionTemplateTests
    {
        public int TestDelegate(string a)
        {
            return 0;
        }

        [Test]
        public void MapSubscription_FromSchemaOptions_WithDelegate_DoesAddFieldToSchema()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapSubscription("/path1/path2", TestDelegate);

            Assert.IsNotNull(field);
            Assert.AreEqual("[subscription]/path1/path2", field.ItemPath.Path);
            Assert.IsInstanceOf(typeof(IGraphQLRuntimeResolvedFieldDefinition), field);
            Assert.AreEqual(1, options.RuntimeTemplates.Count());

            Assert.AreEqual(1, field.Attributes.Count());
            var SubscriptionRootAttrib = field.Attributes.FirstOrDefault(x => x.GetType() == typeof(SubscriptionRootAttribute));
            Assert.IsNotNull(SubscriptionRootAttrib);
        }

        [Test]
        public void MapSubscription_FromBuilder_WithDelegate_DoesAddFieldToSchema()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var builderMock = Substitute.For<ISchemaBuilder>();
            builderMock.Options.Returns(options);

            var field = builderMock.MapSubscription("/path1/path2", TestDelegate);

            Assert.IsNotNull(field);
            Assert.AreEqual("[subscription]/path1/path2", field.ItemPath.Path);
            Assert.IsInstanceOf(typeof(IGraphQLRuntimeResolvedFieldDefinition), field);
            Assert.AreEqual(1, options.RuntimeTemplates.Count());
            Assert.IsNotNull(field.Resolver);

            Assert.AreEqual(1, field.Attributes.Count());
            var SubscriptionRootAttrib = field.Attributes.FirstOrDefault(x => x.GetType() == typeof(SubscriptionRootAttribute));
            Assert.IsNotNull(SubscriptionRootAttrib);
        }

        [Test]
        public void MapSubscription_FromBuilder_WithNoDelegate_DoesAddFieldToSchema()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var builderMock = Substitute.For<ISchemaBuilder>();
            builderMock.Options.Returns(options);

            var field = builderMock.MapSubscription("/path1/path2");

            Assert.IsNotNull(field);
            Assert.AreEqual("[subscription]/path1/path2", field.ItemPath.Path);
            Assert.IsInstanceOf(typeof(IGraphQLRuntimeResolvedFieldDefinition), field);
            Assert.AreEqual(1, options.RuntimeTemplates.Count());

            Assert.AreEqual(1, field.Attributes.Count());
            var SubscriptionRootAttrib = field.Attributes.FirstOrDefault(x => x.GetType() == typeof(SubscriptionRootAttribute));
            Assert.IsNotNull(SubscriptionRootAttrib);
        }

        [Test]
        public void MapSubscription_FromBuilder_WithUnionName0_AddsUnionNameToType()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var builderMock = Substitute.For<ISchemaBuilder>();
            builderMock.Options.Returns(options);

            var field = builderMock.MapSubscription("myField", "myUnion", (string a) => 1);

            var attrib = field.Attributes.OfType<UnionAttribute>().SingleOrDefault();

            Assert.AreEqual("myUnion", attrib.UnionName);
            Assert.AreEqual(1, field.Attributes.Count(x => x is SubscriptionRootAttribute));
        }

        [Test]
        public void MapSubscription_WithUnionNameSetToNull_AddsUnionNameToType()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapSubscription("myField", null, (string a) => 1);

            var attrib = field.Attributes.OfType<UnionAttribute>().SingleOrDefault();

            Assert.IsNull(attrib);
            Assert.AreEqual(1, field.Attributes.Count(x => x is SubscriptionRootAttribute));
        }

        [Test]
        public void MapSubscription_WithUnionName0_AddsUnionNameToType()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapSubscription("myField", "myUnion", (string a) => 1);

            var attrib = field.Attributes.OfType<UnionAttribute>().SingleOrDefault();

            Assert.AreEqual("myUnion", attrib.UnionName);
            Assert.AreEqual(1, field.Attributes.Count(x => x is SubscriptionRootAttribute));
        }

        [Test]
        public void MapSubscription_WithNoResolver_IsCreated()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapSubscription("myField");

            Assert.IsNull(field.Resolver);
            Assert.IsNull(field.ReturnType);

            Assert.IsTrue(options.RuntimeTemplates.Contains(field));
            Assert.AreEqual(1, field.Attributes.Count(x => x is SubscriptionRootAttribute));
        }

        [Test]
        public void MapSubscription_WithResolver_AndUnion_IsCreated()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapSubscription("myField", "myUnion", () => 1);

            Assert.IsNotNull(field.Resolver);

            Assert.AreEqual(1, field.Attributes.OfType<UnionAttribute>().Count());
            Assert.AreEqual("myUnion", field.Attributes.OfType<UnionAttribute>().Single().UnionName);
            Assert.IsTrue(options.RuntimeTemplates.Contains(field));
            Assert.AreEqual(1, field.Attributes.Count(x => x is SubscriptionRootAttribute));
        }
    }
}