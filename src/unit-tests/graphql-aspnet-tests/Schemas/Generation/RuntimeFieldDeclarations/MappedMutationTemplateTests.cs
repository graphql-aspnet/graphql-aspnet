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
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;
    using GraphQL.AspNet.Schemas;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class MappedMutationTemplateTests
    {
        public int TestDelegate(string a)
        {
            return 0;
        }

        [Test]
        public void MapMutation_FromSchemaOptions_WithDelegate_DoesAddFieldToSchema()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapMutation("/path1/path2", TestDelegate);

            Assert.IsNotNull(field);
            Assert.AreEqual("[mutation]/path1/path2", field.Route.Path);
            Assert.IsInstanceOf(typeof(IGraphQLRuntimeResolvedFieldDefinition), field);
            Assert.AreEqual(1, options.RuntimeTemplates.Count());

            Assert.AreEqual(1, field.Attributes.Count());
            var mutationRootAttrib = field.Attributes.FirstOrDefault(x => x.GetType() == typeof(MutationRootAttribute));
            Assert.IsNotNull(mutationRootAttrib);
        }

        [Test]
        public void MapMutation_FromBuilder_WithDelegate_DoesAddFieldToSchema()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var builderMock = new Mock<ISchemaBuilder>();
            builderMock.Setup(x => x.Options).Returns(options);

            var field = builderMock.Object.MapMutation("/path1/path2", TestDelegate);

            Assert.IsNotNull(field);
            Assert.AreEqual("[mutation]/path1/path2", field.Route.Path);
            Assert.IsInstanceOf(typeof(IGraphQLRuntimeResolvedFieldDefinition), field);
            Assert.AreEqual(1, options.RuntimeTemplates.Count());

            Assert.AreEqual(1, field.Attributes.Count());
            var mutationRootAttrib = field.Attributes.FirstOrDefault(x => x.GetType() == typeof(MutationRootAttribute));
            Assert.IsNotNull(mutationRootAttrib);
        }

        [Test]
        public void MapMutation_WithUnionNameSetToNull_AddsUnionNameToType()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapMutation("myField", null, (string a) => 1);

            var attrib = field.Attributes.OfType<UnionAttribute>().SingleOrDefault();

            Assert.IsNull(attrib);
        }

        [Test]
        public void MapMutation_WithUnionName0_AddsUnionNameToType()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapMutation("myField", "myUnion", (string a) => 1);

            var attrib = field.Attributes.OfType<UnionAttribute>().SingleOrDefault();

            Assert.AreEqual("myUnion", attrib.UnionName);
        }

        [Test]
        public void MapMutation_WithNoResolver_IsCreated()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapMutation("myField");

            Assert.IsNull(field.Resolver);
            Assert.IsNull(field.ReturnType);

            Assert.IsTrue(options.RuntimeTemplates.Contains(field));
        }

        [Test]
        public void MapMutation_WithResolver_AndUnion_IsCreated()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapMutation("myField", "myUnion", () => 1);

            Assert.IsNotNull(field.Resolver);

            Assert.AreEqual(1, field.Attributes.OfType<UnionAttribute>().Count());
            Assert.AreEqual("myUnion", field.Attributes.OfType<UnionAttribute>().Single().UnionName);
            Assert.IsTrue(options.RuntimeTemplates.Contains(field));
        }
    }
}