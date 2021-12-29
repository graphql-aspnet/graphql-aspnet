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
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Internal.Templating.DirectiveTestData;
    using NUnit.Framework;

    [TestFixture]
    public class GraphDirectiveTemplateTests
    {
        [Test]
        public void Simpletemplate_AllDefaults_GeneralPropertyCheck()
        {
            var template = new GraphDirectiveTemplate(typeof(SimpleExecutableDirective));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual("SimpleExecutable", template.Name);
            Assert.AreEqual(nameof(SimpleExecutableDirective), template.InternalName);
            Assert.AreEqual(typeof(SimpleExecutableDirective).FriendlyName(true), template.InternalFullName);
            Assert.AreEqual("Simple Description", template.Description);
            Assert.AreEqual(1, template.Methods.Count);
            Assert.IsTrue(template.Locations.HasFlag(DirectiveLocation.FIELD));
            Assert.AreEqual(typeof(SimpleExecutableDirective), template.ObjectType);
            Assert.AreEqual("[directive]/SimpleExecutable", template.Route.Path);
            Assert.AreEqual(DirectiveLocation.FIELD, template.Locations);
            Assert.IsNotNull(template.Methods.FindMethod(DirectiveLocation.FIELD));
        }

        [Test]
        public void InvalidDirective_NoLocationsDefined_ThrowsException()
        {
            var template = new GraphDirectiveTemplate(typeof(NoLocationsDirective));
            template.Parse();
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void InvalidDirective_MismatchedExecutionSignatures_ThrowsException()
        {
            var template = new GraphDirectiveTemplate(typeof(MismatchedSignaturesDirective));
            template.Parse();
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void OverlappingDirectiveLocations_AreCountedCorrectlyAndNotDropped()
        {
            var expectedLocations = DirectiveLocation.FIELD | DirectiveLocation.MUTATION;

            var template = new GraphDirectiveTemplate(typeof(OverlappingLocationsDirective));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(expectedLocations, template.Locations);
        }
    }
}