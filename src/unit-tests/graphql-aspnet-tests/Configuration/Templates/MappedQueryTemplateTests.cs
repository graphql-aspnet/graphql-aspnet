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
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
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
    }
}