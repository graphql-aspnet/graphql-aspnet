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
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Internal.Templating.DirectiveTestData;
    using NUnit.Framework;

    [TestFixture]
    public class DirectiveTemplateTests
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
            Assert.IsNotNull(template.Methods.FindMetaData(DirectiveLocation.FIELD));
            Assert.IsFalse(template.IsRepeatable);
        }

        [Test]
        public void AppliedDirectives_OnDirective_ThrowsException()
        {
            var template = new GraphDirectiveTemplate(typeof(DirectiveWithDeclaredDirectives));
            template.Parse();

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void AppliedDirectives_OnDirectiveMethod_ThrowsException()
        {
            var template = new GraphDirectiveTemplate(typeof(DirectiveWithDeclaredDirectiveOnMethod));
            template.Parse();

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
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

        [Test]
        public void RepeatableAttribute_SetsRepeatableProperty()
        {
            var template = new GraphDirectiveTemplate(typeof(RepeatableDirective));
            template.Parse();
            template.ValidateOrThrow();

            Assert.IsTrue(template.IsRepeatable);
        }

        [Test]
        public void SecurityPolicices_AreParsedCorrectly()
        {
            var template = new GraphDirectiveTemplate(typeof(DirectiveWithSecurityRequirements));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(2, template.SecurityPolicies.Count);

            Assert.IsFalse(template.SecurityPolicies.AllowAnonymous);

            Assert.IsTrue(template.SecurityPolicies.ElementAt(0).IsNamedPolicy);
            Assert.AreEqual("CustomPolicy", template.SecurityPolicies.ElementAt(0).PolicyName);

            Assert.IsFalse(template.SecurityPolicies.ElementAt(1).IsNamedPolicy);
            CollectionAssert.AreEquivalent(new string[] { "CustomRole1", "CustomRole2" }, template.SecurityPolicies.ElementAt(1).AllowedRoles);
        }
    }
}