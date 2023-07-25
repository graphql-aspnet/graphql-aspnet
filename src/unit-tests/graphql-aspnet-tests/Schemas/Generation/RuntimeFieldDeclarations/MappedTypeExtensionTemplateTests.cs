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
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class MappedTypeExtensionTemplateTests
    {
        [Test]
        public void MapTypeExtension_ByOptions_AddsTypeExtensionToOptions()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("myField");
            Assert.IsInstanceOf(typeof(IGraphQLRuntimeTypeExtensionDefinition), typeExt);

            Assert.AreEqual(1, options.RuntimeTemplates.Count());
            Assert.IsNotNull(options.RuntimeTemplates.FirstOrDefault(x => x == typeExt));
            Assert.AreEqual("[type]/TwoPropertyObject/myField", typeExt.Route.Path);
            Assert.IsNull(typeExt.ReturnType);
            Assert.IsNull(typeExt.Resolver);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, typeExt.ExecutionMode);

            Assert.AreEqual(1, typeExt.Attributes.Count());

            var typeExtensionAttrib = typeExt.Attributes.FirstOrDefault(x => x.GetType() == typeof(TypeExtensionAttribute));
            Assert.IsNotNull(typeExtensionAttrib);
        }

        [Test]
        public void MapTypeExtension_WithName_AddsInternalName()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var builderMock = new Mock<ISchemaBuilder>();
            builderMock.Setup(x => x.Options).Returns(options);

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("myField")
                .WithName("internalFieldName");

            Assert.AreEqual("internalFieldName", typeExt.InternalName);
        }

        [Test]
        public void MapTypeExtension_ByBuilder_AddsTypeExtensionToOptions()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var builderMock = new Mock<ISchemaBuilder>();
            builderMock.Setup(x => x.Options).Returns(options);

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("myField");
            Assert.IsInstanceOf(typeof(IGraphQLRuntimeTypeExtensionDefinition), typeExt);

            Assert.AreEqual(1, options.RuntimeTemplates.Count());
            Assert.IsNotNull(options.RuntimeTemplates.FirstOrDefault(x => x == typeExt));
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, typeExt.ExecutionMode);
        }

        [Test]
        public void MapTypeExtension_ByOptions_WithResolver_AddsTypeExtensionToOptions()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("myField", (string a) => 1);
            Assert.IsInstanceOf(typeof(IGraphQLRuntimeTypeExtensionDefinition), typeExt);

            Assert.AreEqual(1, options.RuntimeTemplates.Count());
            Assert.IsNotNull(options.RuntimeTemplates.FirstOrDefault(x => x == typeExt));
            Assert.AreEqual("[type]/TwoPropertyObject/myField", typeExt.Route.Path);
            Assert.IsNull(typeExt.ReturnType);
            Assert.AreEqual(typeof(int), typeExt.Resolver.Method.ReturnType);
        }

        [Test]
        public void MapTypeExtension_ByBuilder_WithResolver_AddsTypeExtensionToOptions()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var builderMock = new Mock<ISchemaBuilder>();
            builderMock.Setup(x => x.Options).Returns(options);

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("myField", (string a) => 1);
            Assert.IsInstanceOf(typeof(IGraphQLRuntimeTypeExtensionDefinition), typeExt);

            Assert.AreEqual(1, options.RuntimeTemplates.Count());
            Assert.IsNotNull(options.RuntimeTemplates.FirstOrDefault(x => x == typeExt));
            Assert.IsNull(typeExt.ReturnType);
            Assert.AreEqual(typeof(int), typeExt.Resolver.Method.ReturnType);
        }

        [Test]
        public void MappedTypeExtension_WhenAllowAnonymousAdded_AddsAnonymousAttribute()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("myField", (string a) => 1);

            typeExt.AllowAnonymous();

            Assert.AreEqual(2, typeExt.Attributes.Count());
            Assert.IsNotNull(typeExt.Attributes.FirstOrDefault(x => x is AllowAnonymousAttribute));
        }

        [Test]
        public void MappedTypeExtension_WhenRequireAuthAdded_AddsAuthAttribute()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("myField", (string a) => 1);

            typeExt.RequireAuthorization("policy1", "roles1");

            Assert.AreEqual(2, typeExt.Attributes.Count());
            var attrib = typeExt.Attributes.FirstOrDefault(x => x is AuthorizeAttribute) as AuthorizeAttribute;
            Assert.IsNotNull(attrib);
            Assert.AreEqual("policy1", attrib.Policy);
            Assert.AreEqual("roles1", attrib.Roles);
        }

        [Test]
        public void MappedTypeExtension_WhenResolverIsChangedWithExplicitType_NewResolverIsUsedAndTypeIsUsed()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("myField", (string a) => 1);
            Assert.AreEqual(typeof(int), typeExt.Resolver.Method.ReturnType);

            typeExt.AddResolver<decimal>((string a) => "bob");
            Assert.AreEqual(typeof(string), typeExt.Resolver.Method.ReturnType);
            Assert.AreEqual(typeof(decimal), typeExt.ReturnType);
        }

        [Test]
        public void MappedTypeExtension_WithBatchProcessing_ChangesExecutionMode()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("myField", (string a) => 1);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, typeExt.ExecutionMode);

            typeExt.WithBatchProcessing();
            Assert.AreEqual(FieldResolutionMode.Batch, typeExt.ExecutionMode);

            var typeExtensionAttrib = typeExt.Attributes.FirstOrDefault(x => x.GetType() == typeof(BatchTypeExtensionAttribute));
            Assert.IsNotNull(typeExtensionAttrib);
        }

        [Test]
        public void MappedTypeExtension_AddPossibleTypes_AddsAppropriateAttribute()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("myField", (string a) => 1);
            typeExt.AddPossibleTypes(typeof(TwoPropertyObjectV2), typeof(TwoPropertyObjectV3));

            Assert.AreEqual(2, typeExt.Attributes.Count());
            var attrib = typeExt.Attributes.FirstOrDefault(x => x is PossibleTypesAttribute) as PossibleTypesAttribute;

            Assert.AreEqual(2, attrib.PossibleTypes.Count);
            Assert.IsNotNull(attrib.PossibleTypes.FirstOrDefault(x => x == typeof(TwoPropertyObjectV2)));
            Assert.IsNotNull(attrib.PossibleTypes.FirstOrDefault(x => x == typeof(TwoPropertyObjectV3)));
        }

        [Test]
        public void MappedTypeExtension_WithoutUnionName_AddsUnionNameToType()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("myField", null, (string a) => 1);

            var attrib = typeExt.Attributes.OfType<UnionAttribute>().SingleOrDefault();

            Assert.IsNull(attrib);
        }

        [Test]
        public void MappedTypeExtension_WithUnionName0_AddsUnionNameToType()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("myField", "myUnion", (string a) => 1);

            var attrib = typeExt.Attributes.OfType<UnionAttribute>().SingleOrDefault();

            Assert.AreEqual("myUnion", attrib.UnionName);
        }

        [Test]
        public void ResolvedField_SwappingOutResolvers_RemovesUnion()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("myField", "myUnion", (string a) => 1);
            Assert.AreEqual(1, typeExt.Attributes.Count(x => x is UnionAttribute));

            // union is removed when resolver is re-declared
            typeExt.AddResolver((int a) => 0);
            Assert.AreEqual(0, typeExt.Attributes.Count(x => x is UnionAttribute));
        }
    }
}