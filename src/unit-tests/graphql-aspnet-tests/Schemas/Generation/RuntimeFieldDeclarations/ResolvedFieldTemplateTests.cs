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
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class ResolvedFieldTemplateTests
    {
        [Test]
        public void ResolvedField_WhenAllowAnonymousAdded_AddsAnonymousAttribute()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQuery("/path1/path2", (string a) => 1);

            field.AllowAnonymous();

            Assert.AreEqual(2, field.Attributes.Count());
            Assert.IsNotNull(field.Attributes.FirstOrDefault(x => x is AllowAnonymousAttribute));
        }

        [Test]
        public void ResolvedField_WhenNameApplied_NameIsAttchedToFieldDeclaration()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options
                .MapQuery("/path1/path2", (string a) => 1)
                .WithName("theName");

            Assert.AreEqual("theName", field.InternalName);
        }

        [Test]
        public void ResolvedField_WhenRequireAuthAdded_AddsAuthAttribute()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQuery("/path1/path2", (string a) => 1);

            field.RequireAuthorization("policy1", "roles1");

            Assert.AreEqual(2, field.Attributes.Count());

            var attrib = field.Attributes.FirstOrDefault(x => x is AuthorizeAttribute) as AuthorizeAttribute;
            Assert.IsNotNull(attrib);
            Assert.AreEqual("policy1", attrib.Policy);
            Assert.AreEqual("roles1", attrib.Roles);
        }

        [Test]
        public void ResolvedField_WhenResolverIsChanged_NewResolverIsUsed()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQuery("/path1/path2", (string a) => 1);
            Assert.AreEqual(typeof(int), field.Resolver.Method.ReturnType);

            field.AddResolver((string a) => "bob");
            Assert.AreEqual(typeof(string), field.Resolver.Method.ReturnType);
            Assert.IsNull(field.ReturnType);
        }

        [Test]
        public void ResolvedField_WhenResolverIsChangedWithExplicitType_NewResolverIsUsedAndTypeIsUsed()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQuery("/path1/path2", (string a) => 1);
            Assert.AreEqual(typeof(int), field.Resolver.Method.ReturnType);

            field.AddResolver<decimal>((string a) => "bob");
            Assert.AreEqual(typeof(string), field.Resolver.Method.ReturnType);
            Assert.AreEqual(typeof(decimal), field.ReturnType);
        }

        [Test]
        public void ResolvedField_AddPossibleTypes_AddsAppropriateAttribute()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapMutation("/path1/path2", (string a) => 1);
            field.AddPossibleTypes(typeof(TwoPropertyObject), typeof(TwoPropertyObjectV2));

            Assert.AreEqual(2, field.Attributes.Count());
            var possibleTypesAttrib = field.Attributes.FirstOrDefault(x => x is PossibleTypesAttribute) as PossibleTypesAttribute;

            Assert.AreEqual(2, possibleTypesAttrib.PossibleTypes.Count);
            Assert.IsNotNull(possibleTypesAttrib.PossibleTypes.FirstOrDefault(x => x == typeof(TwoPropertyObject)));
            Assert.IsNotNull(possibleTypesAttrib.PossibleTypes.FirstOrDefault(x => x == typeof(TwoPropertyObjectV2)));
        }

        [Test]
        public void ResolvedField_ResolverReturnsNullableT_ItsPreserved()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQuery("/path1/path2", (string a) => (int?)1);

            // nullable int
            Assert.AreEqual(typeof(int?), field.Resolver.Method.ReturnType);
        }

        [Test]
        public void ResolvedField_SwappingOutResolvers_RemovesUnion()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQuery("/path1/path2", "myUnion", (string a) => (int?)1);
            Assert.AreEqual(1, field.Attributes.Count(x => x is UnionAttribute));

            // union is removed when resolver is re-declared
            field.AddResolver((int a) => 0);
            Assert.AreEqual(0, field.Attributes.Count(x => x is UnionAttribute));
        }

        [Test]
        public void ResolvedField_PossibleTypeSwapping()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQuery("/path1/path2", (string a) => (int?)1);
            field.AddPossibleTypes(typeof(TwoPropertyObject), typeof(TwoPropertyObjectV2), typeof(TwoPropertyObjectV3));
            field.ClearPossibleTypes();
            field.AddPossibleTypes(typeof(TwoPropertyObject), typeof(TwoPropertyObjectV2));
            field.ClearPossibleTypes();
            field.AddPossibleTypes(typeof(TwoPropertyObject));

            Assert.AreEqual(2, field.Attributes.Count());
            var possibleTypesAttrib = field.Attributes.SingleOrDefault(x => x is PossibleTypesAttribute) as PossibleTypesAttribute;

            Assert.AreEqual(1, possibleTypesAttrib.PossibleTypes.Count);
            Assert.IsNotNull(possibleTypesAttrib.PossibleTypes.FirstOrDefault(x => x == typeof(TwoPropertyObject)));
        }

        [Test]
        public void ResolvedField_AddingUnionViaAddResolver_UnionIsApplied()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            // no union
            var field = options.MapQuery("/path1/path2", (string a) => (int?)1);

            // union added
            field.AddResolver("myUnion", () => 0);

            Assert.AreEqual(1, field.Attributes.Count(x => x is UnionAttribute));
            Assert.AreEqual("myUnion", field.Attributes.OfType<UnionAttribute>().Single().UnionName);
        }
    }
}