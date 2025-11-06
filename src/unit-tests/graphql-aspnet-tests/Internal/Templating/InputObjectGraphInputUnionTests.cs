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
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Internal.Templating.ObjectTypeTests;
    using NUnit.Framework;

    [TestFixture]
    public class InputObjectGraphInputUnionTests
    {
        [Test]
        public void InputObjectTemplate_WhenInheritsFromGraphUnionType_HasAppliedDirectiveAdded()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(SimpleObjectFromGraphInputUnion));
            template.Parse();
            template.ValidateOrThrow();

            var oneOfTemplate = template.AppliedDirectives.FirstOrDefault();
            Assert.That(oneOfTemplate, Is.Not.Null);
            Assert.That(oneOfTemplate.DirectiveType, Is.EqualTo(typeof(OneOfDirective)));
            Assert.That(oneOfTemplate.Arguments.Length, Is.EqualTo(0));
        }

        [Test]
        public void InputObjectTemplate_WhenIsGraphInputUnion_UnWrapsBaseType_AndHasAppliedDirectiveAdded()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(GraphInputUnion<SimpleObjectOneProp>));
            template.Parse();
            template.ValidateOrThrow();

            var oneOfTemplate = template.AppliedDirectives.FirstOrDefault();
            Assert.That(oneOfTemplate, Is.Not.Null);
            Assert.That(oneOfTemplate.DirectiveType, Is.EqualTo(typeof(OneOfDirective)));
            Assert.That(template.ObjectType, Is.EqualTo(typeof(SimpleObjectOneProp)));
            Assert.That(oneOfTemplate.Arguments.Length, Is.EqualTo(0));
        }

        [Test]
        public void ObjectTemplate_WhenObjectInheritsFromGraphUnionType_ThrowsException()
        {
            var template = new ObjectGraphTypeTemplate(typeof(SimpleObjectFromGraphInputUnion));
            template.Parse();

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void ObjectTemplate_WhenObjectIsGraphInputUnion_ThrowsException()
        {
            var template = new ObjectGraphTypeTemplate(typeof(GraphInputUnion<SimpleObjectOneProp>));
            template.Parse();

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }
    }
}