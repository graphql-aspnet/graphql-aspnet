// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating
{
    using System.Linq;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Tests.Internal.Templating.ControllerTestData;
    using NUnit.Framework;

    [TestFixture]
    public class GraphControllerTemplateTests
    {
        [Test]
        public void Parse_GraphRootController_UsesEmptyRoutePath()
        {
            var template = GraphQLProviders.TemplateProvider.ParseType<DeclaredGraphRootController>() as GraphControllerTemplate;
            Assert.IsNotNull(template);
            Assert.AreEqual(GraphFieldPath.Empty, template.Route);
        }

        [Test]
        public void Parse_MismatchedRouteFragmentConfiguration_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                GraphQLProviders.TemplateProvider.ParseType<InvalidRouteController>();
            });
        }

        [Test]
        public void Parse_OverloadedMethodsOnDifferentRoots_ParsesCorrectly()
        {
            var template = GraphQLProviders.TemplateProvider.ParseType<TwoMethodsDifferentRootsController>() as GraphControllerTemplate;
            Assert.IsNotNull(template);

            Assert.AreEqual(3, template.FieldTemplates.Count());
            Assert.AreEqual(2, template.Actions.Count());
            Assert.AreEqual(1, template.Extensions.Count());
            Assert.IsTrue(template.FieldTemplates.ContainsKey($"[mutation]/TwoMethodsDifferentRoots/{nameof(TwoMethodsDifferentRootsController.ActionMethodNoAttributes)}"));
            Assert.IsTrue(template.FieldTemplates.ContainsKey($"[query]/TwoMethodsDifferentRoots/{nameof(TwoMethodsDifferentRootsController.ActionMethodNoAttributes)}"));
            Assert.IsTrue(template.FieldTemplates.ContainsKey($"[type]/TwoPropertyObject/Property3"));
        }
    }
}