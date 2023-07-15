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
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class MappedQueryGroupTests
    {
        [Test]
        public void MapQueryGroup_WhenAllowAnonymousAdded_AddsAnonymousAttribute()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQueryGroup("/path1/path2");

            field.AllowAnonymous();

            Assert.AreEqual(1, field.Attributes.Count());
            Assert.IsNotNull(field.Attributes.FirstOrDefault(x => x is AllowAnonymousAttribute));
        }

        [Test]
        public void MapQueryGroup_WhenRequireAuthAdded_AddsAuthAttribute()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQueryGroup("/path1/path2");

            field.RequireAuthorization("policy1", "roles1");

            Assert.AreEqual(1, field.Attributes.Count());
            var attrib = field.Attributes.FirstOrDefault(x => x is AuthorizeAttribute) as AuthorizeAttribute;
            Assert.IsNotNull(attrib);
            Assert.AreEqual("policy1", attrib.Policy);
            Assert.AreEqual("roles1", attrib.Roles);
        }

        [Test]
        public void MapQueryGroup_WhenUnresolvedChildFieldIsAdded_PathIsCorrect()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQueryGroup("/path1/path2");

            var childField = field.MapSubGroup("/path3/path4");

            Assert.AreEqual("[query]/path1/path2/path3/path4", childField.Route.Path);
        }

        [Test]
        public void MapQueryGroup_WhenResolvedChildFieldIsAdded_PathIsCorrect()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQueryGroup("/path1/path2");

            var childField = field.MapField("/path3/path4", (string a) => 1);

            Assert.AreEqual("[query]/path1/path2/path3/path4", childField.Route.Path);
        }

        [Test]
        public void MapMutationGroup_WhenAllowAnonymousAdded_ThenResolvedField_AddsAnonymousAttributeToField()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQueryGroup("/path1/path2");

            field.AllowAnonymous();

            var chidlField = field.MapField("/path3/path4", (string a) => 1);

            Assert.AreEqual(2, chidlField.Attributes.Count());
            Assert.IsNotNull(chidlField.Attributes.FirstOrDefault(x => x is AllowAnonymousAttribute));
            Assert.IsNotNull(chidlField.Attributes.FirstOrDefault(x => x is QueryRootAttribute));
        }

        [Test]
        public void MapQueryGroup_WhenResolvedChildFieldIsAddedToUnresolvedChildField_PathIsCorrect()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQueryGroup("/path1/path2");
            var childField = field.MapSubGroup("/path3/path4");
            var resolvedField = childField.MapField("/path5/path6", (string a) => 1);

            Assert.AreEqual("[query]/path1/path2/path3/path4/path5/path6", resolvedField.Route.Path);
            Assert.IsNotNull(resolvedField.Resolver);
        }

        [Test]
        public void MapQueryGroup_WhenResolvedChildFieldIsAdded_AndParentPathIsChanged_PathIsCorrect()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQueryGroup("/path1/path2");

            var childField = field.MapField("/path3/path4", (string a) => 1);

            Assert.AreEqual("[query]/path1/path2/path3/path4", childField.Route.Path);
        }

        [Test]
        public void MapQueryGroup_SingleCopyAttribute_AppliedToGroupAndField_IsOnlyAppliedOnce()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQueryGroup("/path1/path2")
                               .AllowAnonymous();

            var childField = field.MapField("/path3/path4", (string a) => 1)
                                  .AllowAnonymous();

            var anonCount = childField.Attributes.Where(x => x is AllowAnonymousAttribute).Count();
            Assert.AreEqual(1, anonCount);
        }

        [Test]
        public void MapQueryGroup_MultipleCopyAttribute_AppliedToGroupAndField_IsAppliedMultipleTimes()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapQueryGroup("/path1/path2")
                               .RequireAuthorization("policy1");

            var childField = field.MapField("/path3/path4", (string a) => 1)
                                  .RequireAuthorization("policy2");

            Assert.AreEqual(3, childField.Attributes.Count());
            var anonCount = childField.Attributes.Where(x => x is AuthorizeAttribute).Count();

            Assert.AreEqual(2, anonCount);
            Assert.IsNotNull(childField.Attributes.SingleOrDefault(x => x is AuthorizeAttribute a && a.Policy == "policy1"));
            Assert.IsNotNull(childField.Attributes.SingleOrDefault(x => x is AuthorizeAttribute a && a.Policy == "policy2"));

            // ensure the order of applied attributes is parent field then child field
            var i = 0;
            foreach (var attrib in childField.Attributes)
            {
                if (attrib is AuthorizeAttribute a && a.Policy == "policy1")
                {
                    Assert.AreEqual(0, i);
                    i++;
                }
                else if (attrib is AuthorizeAttribute b && b.Policy == "policy2")
                {
                    Assert.AreEqual(1, i);
                    i++;
                }
            }

            Assert.AreEqual(2, i);
        }
    }
}