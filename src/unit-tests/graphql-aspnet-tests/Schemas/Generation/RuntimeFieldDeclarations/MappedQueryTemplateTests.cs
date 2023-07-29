// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.RuntimeFieldDeclarations
{
    using System.Linq;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class MappedQueryTemplateTests
    {
        public int TestDelegate(string a)
        {
            return 0;
        }

        [Test]
        public void MapQuery_FromSchemaOptions_WithDelegate_DoesAddFieldToSchema()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQuery("/path1/path2", TestDelegate);

            Assert.IsNotNull(field);
            Assert.AreEqual(SchemaItemCollections.Query, field.Route.RootCollection);
            Assert.IsInstanceOf(typeof(IGraphQLRuntimeResolvedFieldDefinition), field);
            Assert.AreEqual(1, options.RuntimeTemplates.Count());

            var queryRootAttrib = field.Attributes.FirstOrDefault(x => x.GetType() == typeof(QueryRootAttribute));
            Assert.IsNotNull(queryRootAttrib);
        }

        [Test]
        public void MapQuery_FromBuilder_WithDelegate_DoesAddFieldToSchema()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var builderMock = new Mock<ISchemaBuilder>();
            builderMock.Setup(x => x.Options).Returns(options);

            var field = builderMock.Object.MapQuery("/path1/path2", TestDelegate);

            Assert.IsNotNull(field);
            Assert.AreEqual(SchemaItemCollections.Query, field.Route.RootCollection);

            Assert.IsInstanceOf(typeof(IGraphQLRuntimeResolvedFieldDefinition), field);
            Assert.AreEqual(1, options.RuntimeTemplates.Count());

            var queryRootAttrib = field.Attributes.FirstOrDefault(x => x.GetType() == typeof(QueryRootAttribute));
            Assert.IsNotNull(queryRootAttrib);
        }

        [Test]
        public void MapQuery_WithUnionNameSetToNull_AddsUnionNameToType()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var typeExt = options.MapQuery("myField", null, (string a) => 1);

            var attrib = typeExt.Attributes.OfType<UnionAttribute>().SingleOrDefault();

            Assert.IsNull(attrib);
        }

        [Test]
        public void MapQuery_WithUnionName0_AddsUnionNameToType()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var typeExt = options.MapQuery("myField", "myUnion", (string a) => 1);

            var attrib = typeExt.Attributes.OfType<UnionAttribute>().SingleOrDefault();

            Assert.AreEqual("myUnion", attrib.UnionName);
        }

        [Test]
        public void MapMutation_FromBuilder_WithNoDelegate_DoesAddFieldToSchema()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var builderMock = new Mock<ISchemaBuilder>();
            builderMock.Setup(x => x.Options).Returns(options);

            var field = builderMock.Object.MapQuery("/path1/path2");

            Assert.IsNotNull(field);
            Assert.AreEqual("[query]/path1/path2", field.Route.Path);
            Assert.IsInstanceOf(typeof(IGraphQLRuntimeResolvedFieldDefinition), field);
            Assert.AreEqual(1, options.RuntimeTemplates.Count());

            Assert.AreEqual(1, field.Attributes.Count());
            var rootAttrib = field.Attributes.FirstOrDefault(x => x.GetType() == typeof(QueryRootAttribute));
            Assert.IsNotNull(rootAttrib);
        }

        [Test]
        public void MapMutation_FromBuilder_WithUnionName0_AddsUnionNameToType()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var builderMock = new Mock<ISchemaBuilder>();
            builderMock.Setup(x => x.Options).Returns(options);

            var field = builderMock.Object.MapQuery("myField", "myUnion", (string a) => 1);

            var attrib = field.Attributes.OfType<UnionAttribute>().SingleOrDefault();

            Assert.AreEqual("myUnion", attrib.UnionName);

            var rootAttrib = field.Attributes.FirstOrDefault(x => x.GetType() == typeof(QueryRootAttribute));
            Assert.IsNotNull(rootAttrib);
        }
    }
}