// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating
{
    using System.Linq;
    using GraphQL.AspNet.Directives.Global;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Tests.Internal.Templating.ObjectTypeTests;
    using NUnit.Framework;

    [TestFixture]
    public class InputObjectOneOfAttributeTemplatingTests
    {
        [Test]
        public void InputObjectTemplate_WithOneOfAttribute_ParsesOneOfAppliedDirective()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(SimpleObjectWithOneOfAttribute));
            template.Parse();
            template.ValidateOrThrow();

            var oneOfTemplate = template.AppliedDirectives.FirstOrDefault();
            Assert.That(oneOfTemplate, Is.Not.Null);
            Assert.That(oneOfTemplate.DirectiveType, Is.EqualTo(typeof(OneOfDirective)));
            Assert.That(oneOfTemplate.Arguments.Length, Is.EqualTo(0));
        }

        [Test]
        public void InputObjectTemplate_WithOneOfAttribute_AndCustomName_ParsesOneOfAppliedDirective()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(SimpleObjectWithOneOfNameAttribute));
            template.Parse();
            template.ValidateOrThrow();

            var oneOfTemplate = template.AppliedDirectives.FirstOrDefault();
            Assert.That(oneOfTemplate, Is.Not.Null);
            Assert.That(oneOfTemplate.DirectiveType, Is.EqualTo(typeof(OneOfDirective)));
            Assert.That(oneOfTemplate.Arguments.Length, Is.EqualTo(0));
        }

        [Test]
        public void InputObjectTemplate_WithOutOneOfAttribute_WhenParsedAsInputObject_DoesNotParsesOneOfAppliedDirective()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(SimpleObjectWithoutOneOfAttribute));
            template.Parse();
            template.ValidateOrThrow();

            var oneOfTemplate = template.AppliedDirectives.FirstOrDefault();
            Assert.That(oneOfTemplate, Is.Null);
        }
    }
}