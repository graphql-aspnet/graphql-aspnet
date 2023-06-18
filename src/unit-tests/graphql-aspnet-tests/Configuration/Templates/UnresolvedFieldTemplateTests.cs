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
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Configuration.Templates;
    using GraphQL.AspNet.Schemas;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class UnresolvedFieldTemplateTests
    {
        [Test]
        public void UnresolvedField_WhenAllowAnonymousAdded_AddsAnonymousAttribute()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQuery("/path1/path2");

            field.AllowAnonymous();

            Assert.AreEqual(1, field.Attributes.Count);
            Assert.IsNotNull(field.Attributes.FirstOrDefault(x => x is AllowAnonymousAttribute));
        }

        [Test]
        public void UnresolvedField_WhenRequireAuthAdded_AddsAuthAttribute()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQuery("/path1/path2");

            field.RequireAuthorization("policy1", "roles1");

            Assert.AreEqual(1, field.Attributes.Count);
            var attrib = field.Attributes.FirstOrDefault(x => x is AuthorizeAttribute) as AuthorizeAttribute;
            Assert.IsNotNull(attrib);
            Assert.AreEqual("policy1", attrib.Policy);
            Assert.AreEqual("roles1", attrib.Roles);
        }

        [Test]
        public void UnresolvedField_WhenResolverAdded_BecomesResolvedField()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQuery("/path1/path2");

            Assert.IsInstanceOf(typeof(IGraphQLFieldTemplate), field);
            Assert.IsNotInstanceOf(typeof(IGraphQLRuntimeResolvedFieldTemplate), field);

            var field1 = field.AddResolver((string a) => 1);
            Assert.IsInstanceOf(typeof(IGraphQLRuntimeResolvedFieldTemplate), field1);
            Assert.IsNull(field1.ReturnType);
        }

        [Test]
        public void UnresolvedField_WhenResolverAddedWithSpecificType_BecomesResolvedField()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQuery("/path1/path2");

            Assert.IsInstanceOf(typeof(IGraphQLFieldTemplate), field);
            Assert.IsNotInstanceOf(typeof(IGraphQLRuntimeResolvedFieldTemplate), field);

            var field1 = field.AddResolver<decimal>((string a) => 1);
            Assert.IsInstanceOf(typeof(IGraphQLRuntimeResolvedFieldTemplate), field1);
            Assert.AreEqual(typeof(decimal), field1.ReturnType);
        }

        [Test]
        public void UnresolvedField_WhenUnresolvedChildFieldIsAdded_PathIsCorrect()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQuery("/path1/path2");

            var childField = field.MapField("/path3/path4");

            var path = childField.CreatePath();
            Assert.AreEqual("[query]/path1/path2/path3/path4", path.Path);
        }

        [Test]
        public void UnresolvedField_WhenResolvedChildFieldIsAdded_PathIsCorrect()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQuery("/path1/path2");

            var childField = field.MapField("/path3/path4", (string a) => 1);

            var path = childField.CreatePath();
            Assert.AreEqual("[query]/path1/path2/path3/path4", path.Path);
        }

        [Test]
        public void UnresolvedField_WhenResolvedChildFieldIsAddedToUnresolvedChildField_PathIsCorrect()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQuery("/path1/path2");
            var childField = field.MapField("/path3/path4");
            var resolvedField = childField.MapField("/path5/path6", (string a) => 1);

            var path = resolvedField.CreatePath();
            Assert.AreEqual("[query]/path1/path2/path3/path4/path5/path6", path.Path);
            Assert.IsNotNull(resolvedField.Resolver);
        }

        [Test]
        public void UnresolvedField_WhenResolvedChildFieldIsAdded_AndParentPathIsChanged_PathIsCorrect()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQuery("/path1/path2");

            var childField = field.MapField("/path3/path4", (string a) => 1);

            var path = childField.CreatePath();
            Assert.AreEqual("[query]/path1/path2/path3/path4", path.Path);
        }
    }
}