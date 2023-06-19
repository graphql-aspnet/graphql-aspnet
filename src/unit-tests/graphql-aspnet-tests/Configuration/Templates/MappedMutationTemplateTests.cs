﻿// *************************************************************
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
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Configuration.Templates;
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
        public void MapMutation_FromSchemaOptions_WithNoDelegate_DoesNotAddFieldToSchema()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapMutation("/path1/path2");

            Assert.IsNotNull(field);
            Assert.AreEqual(SchemaItemCollections.Mutation, field.CreatePath().RootCollection);
            Assert.IsInstanceOf(typeof(IGraphQLRuntimeFieldDefinition), field);
            Assert.AreEqual(0, options.RuntimeTemplates.Count());
        }

        [Test]
        public void MapMutation_FromSchemaOptions_WithDelegate_DoesAddFieldToSchema()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var field = options.MapMutation("/path1/path2", TestDelegate);

            Assert.IsNotNull(field);
            Assert.AreEqual(SchemaItemCollections.Mutation, field.CreatePath().RootCollection);
            Assert.IsInstanceOf(typeof(IGraphQLRuntimeResolvedFieldDefinition), field);
            Assert.AreEqual(1, options.RuntimeTemplates.Count());
        }

        [Test]
        public void MapMutation_FromBuilder_WithNoDelegate_DoesNotAddFieldToSchema()
        {
            var services = new ServiceCollection();
            var options = new SchemaOptions<GraphSchema>(services);

            var builderMock = new Mock<ISchemaBuilder>();
            builderMock.Setup(x => x.Options).Returns(options);

            var field = builderMock.Object.MapMutation("/path1/path2");

            Assert.IsNotNull(field);
            Assert.AreEqual(SchemaItemCollections.Mutation, field.CreatePath().RootCollection);
            Assert.IsInstanceOf(typeof(IGraphQLRuntimeFieldDefinition), field);
            Assert.AreEqual(0, options.RuntimeTemplates.Count());
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
            Assert.AreEqual(SchemaItemCollections.Mutation, field.CreatePath().RootCollection);
            Assert.IsInstanceOf(typeof(IGraphQLRuntimeResolvedFieldDefinition), field);
            Assert.AreEqual(1, options.RuntimeTemplates.Count());
        }
    }
}