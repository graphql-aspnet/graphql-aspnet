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
    using System;
    using System.Linq;
    using GraphQL.AspNet.Directives.Global;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Tests.Internal.Templating.ObjectTypeTests;
    using NUnit.Framework;

    [TestFixture]
    public class InputObjectOneOfAttributeTemplatingTests
    {
        [TestCase(typeof(ValidOneOfObjectWithStringProp))]
        [TestCase(typeof(ValidOneOfObjectWithNullableIntProp))]
        [TestCase(typeof(ValidOneOfTemplateWithNullableStructField))]
        public void ValidOneOfTemplate_ShouldParseOneOfAppliedDirective(Type typeToCheck)
        {
            var template = new InputObjectGraphTypeTemplate(typeof(ValidOneOfObjectWithStringProp));
            template.Parse();
            template.ValidateOrThrow();

            var oneOfTemplate = template.AppliedDirectives.FirstOrDefault();
            Assert.That(oneOfTemplate, Is.Not.Null);
            Assert.That(oneOfTemplate.DirectiveType, Is.EqualTo(typeof(OneOfDirective)));
            Assert.That(oneOfTemplate.Arguments.Length, Is.EqualTo(0));
        }

        [TestCase(typeof(InvalidOneOfTemplateWithNonNullFieldType))]
        [TestCase(typeof(InvalidOneOfTemplateNonNullTypeExpression))]
        [TestCase(typeof(InvalidOneOfTemplateDefaultValueOnNullableField))]
        [TestCase(typeof(InvalidOneOfTemplateDefaultValueOnNonNullField))]
        [TestCase(typeof(InvalidOneOfTemplateDefaultValueOnStringField))]
        [TestCase(typeof(InvalidOneOfTemplateWithStructField))]
        [TestCase(typeof(InvalidOneOfTemplateWithNullableStructFieldAndDeclaredDefaultvalue))]
        public void InvalidOneOfTemplates_ShouldFailParsing(Type typeToCheck)
        {
            var template = new InputObjectGraphTypeTemplate(typeToCheck);
            template.Parse();

            Assert.Throws<GraphTypeDeclarationException>(() => template.ValidateOrThrow());
        }

        [TestCase(typeof(ValidOneOfObjectWithStringProp))]
        [TestCase(typeof(ValidOneOfObjectWithNullableIntProp))]
        [TestCase(typeof(ValidOneOfTemplateWithNullableStructField))]
        [TestCase(typeof(InvalidOneOfTemplateWithNonNullFieldType))]
        [TestCase(typeof(InvalidOneOfTemplateNonNullTypeExpression))]
        [TestCase(typeof(InvalidOneOfTemplateDefaultValueOnNullableField))]
        [TestCase(typeof(InvalidOneOfTemplateDefaultValueOnNonNullField))]
        [TestCase(typeof(InvalidOneOfTemplateDefaultValueOnStringField))]
        [TestCase(typeof(InvalidOneOfTemplateWithStructField))]
        [TestCase(typeof(InvalidOneOfTemplateWithNullableStructFieldAndDeclaredDefaultvalue))]
        public void AnyOneOfTemplates_WhenParsedAsObject_ShouldParseSuccessfully(Type typeToCheck)
        {
            // should successfully parse without throwing
            var template = new ObjectGraphTypeTemplate(typeToCheck);
            template.Parse();
            template.ValidateOrThrow();

            // should not have any one of directives
            var oneOfTemplate = template.AppliedDirectives.FirstOrDefault();
            Assert.That(oneOfTemplate, Is.Null);
        }

        [Test]
        public void InputObjectTemplate_WithOutOneOfAttribute_ShouldNotParseAnyOneOfAppliedDirective()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(SimpleObjectWithStringProp));
            template.Parse();
            template.ValidateOrThrow();

            var oneOfTemplate = template.AppliedDirectives.FirstOrDefault();
            Assert.That(oneOfTemplate, Is.Null);
        }
    }
}