// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.RuntimetypeExtDeclarations
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
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class MappedTypeExtensionTemplateTests
    {
        [Test]
        public void MapTypeExtension_ByOptions_AddsTypeExtensionToOptions()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("mytypeExt");
            Assert.IsInstanceOf(typeof(IGraphQLRuntimeTypeExtensionDefinition), typeExt);

            Assert.AreEqual(1, options.RuntimeTemplates.Count());
            Assert.IsNotNull(options.RuntimeTemplates.FirstOrDefault(x => x == typeExt));
            Assert.AreEqual("[type]/TwoPropertyObject/mytypeExt", typeExt.Route.Path);
            Assert.IsNull(typeExt.ReturnType);
            Assert.IsNull(typeExt.Resolver);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, typeExt.ExecutionMode);

            Assert.AreEqual(1, typeExt.Attributes.Count());

            var typeExtensionAttrib = typeExt.Attributes.FirstOrDefault(x => x.GetType() == typeof(TypeExtensionAttribute));
            Assert.IsNotNull(typeExtensionAttrib);
        }

        [Test]
        public void MapTypeExtension_ByOptions_AndTypeDeclaration_AddsTypeExtensionToOptions()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var typeExt = options.MapTypeExtension(typeof(TwoPropertyObject), "mytypeExt", (int x) => 0);
            Assert.IsInstanceOf(typeof(IGraphQLRuntimeTypeExtensionDefinition), typeExt);

            Assert.AreEqual(1, options.RuntimeTemplates.Count());
            Assert.IsNotNull(options.RuntimeTemplates.FirstOrDefault(x => x == typeExt));
            Assert.AreEqual("[type]/TwoPropertyObject/mytypeExt", typeExt.Route.Path);
            Assert.IsNull(typeExt.ReturnType);
            Assert.IsNotNull(typeExt.Resolver);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, typeExt.ExecutionMode);

            Assert.AreEqual(1, typeExt.Attributes.Count());

            var typeExtensionAttrib = typeExt.Attributes.FirstOrDefault(x => x.GetType() == typeof(TypeExtensionAttribute));
            Assert.IsNotNull(typeExtensionAttrib);
        }

        [Test]
        public void MapTypeExtension_ByOptions_WithTypeParameter_AddsTypeExtensionToOptions()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var typeExt = options.MapTypeExtension(typeof(TwoPropertyObject), "mytypeExt");
            Assert.IsInstanceOf(typeof(IGraphQLRuntimeTypeExtensionDefinition), typeExt);

            Assert.AreEqual(1, options.RuntimeTemplates.Count());
            Assert.IsNotNull(options.RuntimeTemplates.FirstOrDefault(x => x == typeExt));
            Assert.AreEqual("[type]/TwoPropertyObject/mytypeExt", typeExt.Route.Path);
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

            var builderMock = Substitute.For<ISchemaBuilder>();
            builderMock.Options.Returns(options);

            var typeExt = builderMock.MapTypeExtension<TwoPropertyObject>("mytypeExt")
                .WithInternalName("internaltypeExtName");

            Assert.AreEqual("internaltypeExtName", typeExt.InternalName);
        }

        [Test]
        public void MapTypeExtension_ByBuilder_AddsTypeExtensionToOptions()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var builderMock = Substitute.For<ISchemaBuilder>();
            builderMock.Options.Returns(options);

            var typeExt = builderMock.MapTypeExtension<TwoPropertyObject>("mytypeExt");
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

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("mytypeExt", (string a) => 1);
            Assert.IsInstanceOf(typeof(IGraphQLRuntimeTypeExtensionDefinition), typeExt);

            Assert.AreEqual(1, options.RuntimeTemplates.Count());
            Assert.IsNotNull(options.RuntimeTemplates.FirstOrDefault(x => x == typeExt));
            Assert.AreEqual("[type]/TwoPropertyObject/mytypeExt", typeExt.Route.Path);
            Assert.IsNull(typeExt.ReturnType);
            Assert.AreEqual(typeof(int), typeExt.Resolver.Method.ReturnType);
        }

        [Test]
        public void MapTypeExtension_ByBuilder_WithResolver_AddsTypeExtensionToOptions()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var builderMock = Substitute.For<ISchemaBuilder>();
            builderMock.Options.Returns(options);

            var typeExt = builderMock.MapTypeExtension<TwoPropertyObject>("mytypeExt", (string a) => 1);
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

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("mytypeExt", (string a) => 1);

            typeExt.AllowAnonymous();

            Assert.AreEqual(2, typeExt.Attributes.Count());
            Assert.IsNotNull(typeExt.Attributes.FirstOrDefault(x => x is AllowAnonymousAttribute));
        }

        [Test]
        public void MappedTypeExtension_WhenRequireAuthAdded_AddsAuthAttribute()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("mytypeExt", (string a) => 1);

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

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("mytypeExt", (string a) => 1);
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

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("mytypeExt", (string a) => 1);
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

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("mytypeExt", (string a) => 1);
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

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("mytypeExt", null, (string a) => 1);

            var attrib = typeExt.Attributes.OfType<UnionAttribute>().SingleOrDefault();

            Assert.IsNull(attrib);
        }

        [Test]
        public void MappedTypeExtension_WithUnionName0_AddsUnionNameToType()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("mytypeExt", "myUnion", (string a) => 1);

            var attrib = typeExt.Attributes.OfType<UnionAttribute>().SingleOrDefault();

            Assert.AreEqual("myUnion", attrib.UnionName);
        }

        [Test]
        public void MappedTypeExension_SwappingOutResolvers_RemovesUnion()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("mytypeExt", "myUnion", (string a) => 1);
            Assert.AreEqual(1, typeExt.Attributes.Count(x => x is UnionAttribute));

            // union is removed when resolver is re-declared
            typeExt.AddResolver((int a) => 0);
            Assert.AreEqual(0, typeExt.Attributes.Count(x => x is UnionAttribute));
        }

        [Test]
        public void MappedTypeExension_ViaOptions_WithUnion()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("mytypeExt", "myUnion", (string a) => 1);
            Assert.AreEqual(1, typeExt.Attributes.Count(x => x is UnionAttribute));
            Assert.IsNotNull(typeExt.Resolver);
        }

        [Test]
        public void MappedTypeExension_ViaBuilder_WithUnion()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var builderMock = Substitute.For<ISchemaBuilder>();
            builderMock.Options.Returns(options);

            var typeExt = builderMock.MapTypeExtension<TwoPropertyObject>("mytypeExt", "myUnion", (string a) => 1);
            Assert.AreEqual(1, typeExt.Attributes.Count(x => x is UnionAttribute));
            Assert.IsNotNull(typeExt.Resolver);
        }

        [Test]
        public void MappedTypeExension_NoResolver_CreatesField()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("mytypeExt");
            Assert.AreEqual(1, typeExt.Attributes.Count());

            // No Resolver is Set
            Assert.IsNull(typeExt.Resolver);
        }

        [Test]
        public void MappedTypeExension_AddingUnionViaAddResolver_UnionIsApplied()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            // no union
            var typeExt = options.MapTypeExtension<TwoPropertyObject>("path2", (string a) => (int?)1);

            // union added
            typeExt.AddResolver("myUnion", () => 0);
            Assert.AreEqual(1, typeExt.Attributes.Count(x => x is UnionAttribute));
            Assert.AreEqual("myUnion", typeExt.Attributes.OfType<UnionAttribute>().Single().UnionName);
        }

        [Test]
        public void MappedTypeExension_AddingResolver_WithExplicitReturnType_WithUnion_UnionIsApplied()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            // no union
            var typeExt = options.MapTypeExtension<TwoPropertyObject>("path2", (string a) => (int?)1);

            // union added
            typeExt.AddResolver("myUnion", () => 0);
            Assert.AreEqual(1, typeExt.Attributes.Count(x => x is UnionAttribute));
            Assert.AreEqual("myUnion", typeExt.Attributes.OfType<UnionAttribute>().Single().UnionName);
        }

        [Test]
        public void MappedTypeExension_PossibleTypeSwapping()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var typeExt = options.MapTypeExtension<TwoPropertyObject>("mytypeExt", (string a) => 1);
            typeExt.AddPossibleTypes(typeof(TwoPropertyObject), typeof(TwoPropertyObjectV2), typeof(TwoPropertyObjectV3));
            typeExt.ClearPossibleTypes();
            typeExt.AddPossibleTypes(typeof(TwoPropertyObject), typeof(TwoPropertyObjectV2));
            typeExt.ClearPossibleTypes();
            typeExt.AddPossibleTypes(typeof(TwoPropertyObject));

            Assert.AreEqual(2, typeExt.Attributes.Count());
            var possibleTypesAttrib = typeExt.Attributes.SingleOrDefault(x => x is PossibleTypesAttribute) as PossibleTypesAttribute;

            Assert.AreEqual(1, possibleTypesAttrib.PossibleTypes.Count);
            Assert.IsNotNull(possibleTypesAttrib.PossibleTypes.FirstOrDefault(x => x == typeof(TwoPropertyObject)));
        }
    }
}