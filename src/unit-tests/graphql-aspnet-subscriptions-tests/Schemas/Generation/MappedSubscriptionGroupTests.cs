﻿// *************************************************************
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
    using GraphQL.AspNet.Schemas;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class MappedSubscriptionGroupTests
    {
        [Test]
        public void MapSubscriptionGroup_WhenAllowAnonymousAdded_AddsAnonymousAttribute()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapSubscriptionGroup("/path1/path2");

            field.AllowAnonymous();

            Assert.AreEqual(1, field.Attributes.Count());
            Assert.IsNotNull(field.Attributes.FirstOrDefault(x => x is AllowAnonymousAttribute));
        }

        [Test]
        public void MapSubscriptionGroup_WhenRequireAuthAdded_AddsAuthAttribute()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapSubscriptionGroup("/path1/path2");

            field.RequireAuthorization("policy1", "roles1");

            Assert.AreEqual(1, field.Attributes.Count());
            var attrib = field.Attributes.FirstOrDefault(x => x is AuthorizeAttribute) as AuthorizeAttribute;
            Assert.IsNotNull(attrib);
            Assert.AreEqual("policy1", attrib.Policy);
            Assert.AreEqual("roles1", attrib.Roles);
        }

        [Test]
        public void MapSubscriptionGroup_WhenUnresolvedChildFieldIsAdded_PathIsCorrect()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapSubscriptionGroup("/path1/path2");

            var childField = field.MapChildGroup("/path3/path4");

            Assert.AreEqual("[subscription]/path1/path2/path3/path4", childField.Route.Path);
        }

        [Test]
        public void MapSubscriptionGroup_WhenAllowAnonymousAdded_ThenResolvedField_AddsAnonymousAttributeToField()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapSubscriptionGroup("/path1/path2");

            field.AllowAnonymous();

            var chidlField = field.MapField("/path3/path4", (string a) => 1);

            Assert.AreEqual(2, chidlField.Attributes.Count());
            Assert.IsNotNull(chidlField.Attributes.FirstOrDefault(x => x is AllowAnonymousAttribute));
            Assert.IsNotNull(chidlField.Attributes.FirstOrDefault(x => x is SubscriptionRootAttribute));
        }

        [Test]
        public void MapSubscriptionGroup_WhenResolvedChildFieldIsAdded_PathIsCorrect()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapSubscriptionGroup("/path1/path2");

            var childField = field.MapField("/path3/path4", (string a) => 1);

            Assert.AreEqual("[subscription]/path1/path2/path3/path4", childField.Route.Path);
            Assert.AreEqual(1, childField.Attributes.Count(x => x is SubscriptionRootAttribute));
        }

        [Test]
        public void MapSubscriptionGroup_WhenResolvedChildFieldIsAddedToUnresolvedChildField_PathIsCorrect()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapSubscriptionGroup("/path1/path2");
            var childField = field.MapChildGroup("/path3/path4");
            var resolvedField = childField.MapField("/path5/path6", (string a) => 1);

            Assert.AreEqual("[subscription]/path1/path2/path3/path4/path5/path6", resolvedField.Route.Path);
            Assert.AreEqual(1, resolvedField.Attributes.Count(x => x is SubscriptionRootAttribute));
            Assert.IsNotNull(resolvedField.Resolver);
        }

        [Test]
        public void MapSubscriptionGroup_WhenResolvedChildFieldIsAdded_AndParentPathIsChanged_PathIsCorrect()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapSubscriptionGroup("/path1/path2");

            var childField = field.MapField("/path3/path4", (string a) => 1);

            Assert.AreEqual("[subscription]/path1/path2/path3/path4", childField.Route.Path);
            Assert.AreEqual(1, childField.Attributes.Count(x => x is SubscriptionRootAttribute));
        }

        [Test]
        public void MapField_FromSchemaBuilder_WithNoResovler_FieldIsMade_ResolverIsNotSet()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var builderMock = new Mock<ISchemaBuilder>();
            builderMock.Setup(x => x.Options).Returns(options);

            var childField = builderMock.Object.MapSubscriptionGroup("/path1/path2")
                               .MapField("myField");

            Assert.IsNull(childField.Resolver);
            Assert.AreEqual(1, childField.Attributes.Count(x => x is SubscriptionRootAttribute));
        }
    }
}