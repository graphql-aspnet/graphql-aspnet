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
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Internal.Templating.DirectiveTestData;
    using GraphQL.AspNet.Common.Extensions;
    using NUnit.Framework;

    [TestFixture]
    public class GraphDirectiveTemplateTests
    {
        [Test]
        public void Simpletemplate_AllDefaults_GeneralPropertyCheck()
        {
            var template = new GraphDirectiveTemplate(typeof(SimpleDirective));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual("Simple", template.Name);
            Assert.AreEqual(nameof(SimpleDirective), template.InternalName);
            Assert.AreEqual(typeof(SimpleDirective).FriendlyName(true), template.InternalFullName);
            Assert.AreEqual("Simple Description", template.Description);
            Assert.AreEqual(1, template.Methods.Count);
            Assert.IsTrue(template.Locations.HasFlag(DirectiveLocation.FIELD));
            Assert.AreEqual(typeof(SimpleDirective), template.ObjectType);
            Assert.AreEqual("[directive]/Simple", template.Route.Path);
            Assert.AreEqual(DirectiveLocation.FIELD, template.Locations);
            Assert.IsNotNull(template.Methods.FindMethod(DirectiveLifeCycle.BeforeResolution));
        }

        [Test]
        public void InvalidDirective_MethodWithInvalidSignature_ThrowsException()
        {
            var template = new GraphDirectiveTemplate(typeof(InvalidDirective));
            template.Parse();
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void InvalidDirective_MethodWithNoLocations_ThrowsException()
        {
            var template = new GraphDirectiveTemplate(typeof(NoLocationsDirective));
            template.Parse();
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void InvalidDirective_MismatchedSignatures_ThrowsException()
        {
            var template = new GraphDirectiveTemplate(typeof(MismatchedSignaturesDirective));
            template.Parse();
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }
    }
}