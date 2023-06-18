// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Configuration.Templates
{
    using System.Linq;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Configuration.Templates;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class MappedDirectiveTemplateTests
    {
        [Test]
        public void MapDirective_ByOptions_AddsDirectiveToOptions()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var directive = options.MapDirective("@myDirective");
            Assert.IsInstanceOf(typeof(IGraphQLDirectiveTemplate), directive);

            Assert.AreEqual(1, options.RuntimeTemplates.Count());
            Assert.IsNotNull(options.RuntimeTemplates.FirstOrDefault(x => x == directive));
            Assert.AreEqual("@myDirective", directive.Template);
            Assert.IsNull(directive.ReturnType);
            Assert.IsNull(directive.Resolver);
        }

        [Test]
        public void MapDirective_ByBuilder_AddsDirectiveToOptions()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var builderMock = new Mock<ISchemaBuilder>();
            builderMock.Setup(x => x.Options).Returns(options);

            var directive = builderMock.Object.MapDirective("@myDirective");
            Assert.IsInstanceOf(typeof(IGraphQLDirectiveTemplate), directive);

            Assert.AreEqual(1, options.RuntimeTemplates.Count());
            Assert.IsNotNull(options.RuntimeTemplates.FirstOrDefault(x => x == directive));
        }

        [Test]
        public void MapDirective_ByOptions_WithResolver_AddsDirectiveToOptions()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var directive = options.MapDirective("@myDirective", (string a) => 1);
            Assert.IsInstanceOf(typeof(IGraphQLDirectiveTemplate), directive);

            Assert.AreEqual(1, options.RuntimeTemplates.Count());
            Assert.IsNotNull(options.RuntimeTemplates.FirstOrDefault(x => x == directive));
            Assert.AreEqual("@myDirective", directive.Template);
            Assert.IsNull(directive.ReturnType);
            Assert.AreEqual(typeof(int), directive.Resolver.Method.ReturnType);
        }

        [Test]
        public void MapDirective_ByBuilder_WithResolver_AddsDirectiveToOptions()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var builderMock = new Mock<ISchemaBuilder>();
            builderMock.Setup(x => x.Options).Returns(options);

            var directive = builderMock.Object.MapDirective("@myDirective", (string a) => 1);
            Assert.IsInstanceOf(typeof(IGraphQLDirectiveTemplate), directive);

            Assert.AreEqual(1, options.RuntimeTemplates.Count());
            Assert.IsNotNull(options.RuntimeTemplates.FirstOrDefault(x => x == directive));
            Assert.IsNull(directive.ReturnType);
            Assert.AreEqual(typeof(int), directive.Resolver.Method.ReturnType);
        }

        [Test]
        public void MappedDirective_WhenAllowAnonymousAdded_AddsAnonymousAttribute()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var directive = options.MapDirective("@mydirective", (string a) => 1);

            directive.AllowAnonymous();

            Assert.AreEqual(1, directive.Attributes.Count);
            Assert.IsNotNull(directive.Attributes.FirstOrDefault(x => x is AllowAnonymousAttribute));
        }

        [Test]
        public void MappedDirective_WhenRequireAuthAdded_AddsAuthAttribute()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var directive = options.MapDirective("@mydirective", (string a) => 1);

            directive.RequireAuthorization("policy1", "roles1");

            Assert.AreEqual(1, directive.Attributes.Count);
            var attrib = directive.Attributes.FirstOrDefault(x => x is AuthorizeAttribute) as AuthorizeAttribute;
            Assert.IsNotNull(attrib);
            Assert.AreEqual("policy1", attrib.Policy);
            Assert.AreEqual("roles1", attrib.Roles);
        }

        [Test]
        public void MappedDirective_WhenLocationRestricted_AddsAttribute()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var directive = options.MapDirective("@mydirective", (string a) => 1);

            directive.RestrictLocations(DirectiveLocation.QUERY | DirectiveLocation.MUTATION);

            Assert.AreEqual(1, directive.Attributes.Count);
            var attrib = directive.Attributes.FirstOrDefault(x => x is DirectiveLocationsAttribute) as DirectiveLocationsAttribute;
            Assert.IsNotNull(attrib);
            Assert.IsTrue(attrib.Locations.HasFlag(DirectiveLocation.QUERY));
            Assert.IsTrue(attrib.Locations.HasFlag(DirectiveLocation.MUTATION));
        }

        [Test]
        public void MappedDirective_WhenRepeatableAdded_AddsAttribute()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var directive = options.MapDirective("@mydirective", (string a) => 1);

            directive.IsRepeatable();

            Assert.AreEqual(1, directive.Attributes.Count);
            var attrib = directive.Attributes.FirstOrDefault(x => x is RepeatableAttribute) as RepeatableAttribute;
            Assert.IsNotNull(attrib);
        }

        [Test]
        public void MappedDirective_WhenResolverIsChangedWithExplicitType_NewResolverIsUsedAndTypeIsUsed()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var directive = options.MapDirective("@mydirective", (string a) => 1);
            Assert.AreEqual(typeof(int), directive.Resolver.Method.ReturnType);

            directive.AddResolver<decimal>((string a) => "bob");
            Assert.AreEqual(typeof(string), directive.Resolver.Method.ReturnType);
            Assert.AreEqual(typeof(decimal), directive.ReturnType);
        }
    }
}