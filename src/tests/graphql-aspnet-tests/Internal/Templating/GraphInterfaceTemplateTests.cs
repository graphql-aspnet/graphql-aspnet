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
    using GraphQL.AspNet.Tests.Internal.Templating.DirectiveTestData;
    using GraphQL.AspNet.Tests.Internal.Templating.InterfaceTestData;
    using NUnit.Framework;

    [TestFixture]
    public class GraphInterfaceTemplateTests
    {
        [Test]
        public void Parse_FromInterface_GeneralPropertySettings_SetCorrectly()
        {
            var template = new InterfaceGraphTypeTemplate(typeof(ISimpleInterface));
            template.Parse();
            template.ValidateOrThrow();

            Assert.IsNotNull(template);
            Assert.AreEqual("[type]/ISimpleInterface", template.Route.Path);
            Assert.AreEqual(null, template.Description);
            Assert.AreEqual(typeof(ISimpleInterface), template.ObjectType);
            Assert.AreEqual(2, template.FieldTemplates.Count());
        }

        [Test]
        public void Parse_FromClass_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new InterfaceGraphTypeTemplate(typeof(GraphInterfaceTemplateTests));
            });
        }

        [Test]
        public void Parse_AssignedDirective_IsTemplatized()
        {
            var template = new InterfaceGraphTypeTemplate(typeof(IInterfaceWithDirective));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(1, template.AppliedDirectives.Count());

            var appliedDirective = template.AppliedDirectives.First();
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.Directive);
            Assert.AreEqual(new object[] { 8, "big face" }, appliedDirective.Arguments);
        }
    }
}