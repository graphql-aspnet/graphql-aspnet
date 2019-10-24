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
    using System;
    using System.Linq;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Tests.Internal.Templating.ObjectTypeTests;
    using NUnit.Framework;

    [TestFixture]
    public class InputObjectTemplateTests
    {
        [Test]
        public void UnparsedTemplate_ThrowsException()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(SimpleObjectNoMethods));

            Assert.Throws<InvalidOperationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void Parse_GeneralPropertySettings_SetCorrectly()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(SimpleObjectNoMethods));
            template.Parse();
            template.ValidateOrThrow();

            Assert.IsNotNull(template);
            Assert.AreEqual("[type]/Input_SimpleObjectNoMethods", template.Route.Path);
            Assert.AreEqual(null, template.Description);
            Assert.AreEqual(typeof(SimpleObjectNoMethods), template.ObjectType);
            Assert.AreEqual(0, template.FieldTemplates.Count());
        }

        [Test]
        public void Parse_DescriptionAttribute_SetsCorrectly()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(DescriptionObject));
            template.Parse();
            template.ValidateOrThrow();

            Assert.IsNotNull(template);
            Assert.AreEqual("A valid description", template.Description);
        }

        [Test]
        public void Parse_MethodsAreCapturedAsExplictOrImplicitCorrectly()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(OneMarkedMethod));
            template.Parse();
            template.ValidateOrThrow();

            // an input object should parse no methods, ever
            Assert.AreEqual(0, template.FieldTemplates.Count);
        }

        [Test]
        public void Parse_PropertiesAreCapturedAsExplictOrImplicitCorrectly()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(OneMarkedProperty));
            template.Parse();
            template.ValidateOrThrow();

            var totalProps = typeof(OneMarkedProperty).GetProperties().Length;

            Assert.IsTrue(totalProps == template.FieldTemplates.Count);
            Assert.AreEqual(1, template.FieldTemplates.Count(x => x.Value.IsExplicitDeclaration));
            Assert.AreEqual(1, template.FieldTemplates.Count(x => !x.Value.IsExplicitDeclaration));
        }

        [Test]
        public void Parse_OverloadedMethodsWithNameClash_ThrowsException()
        {
            // since methods should be skipped this object (with two methods of the same graph name)
            // should parse with no exceptions.
            var template = new InputObjectGraphTypeTemplate(typeof(TwoMethodsWithSameName));
            template.Parse();
            template.ValidateOrThrow();
        }

        [Test]
        public void Parse_FromAnEnum_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new InputObjectGraphTypeTemplate(typeof(GraphCollection));
            });
        }

        [Test]
        public void Parse_FromValueType_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new InputObjectGraphTypeTemplate(typeof(int));
            });
        }

        [Test]
        public void Parse_FromString_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new InputObjectGraphTypeTemplate(typeof(string));
            });
        }

        [Test]
        public void Parse_FromAbstractClass_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new InputObjectGraphTypeTemplate(typeof(BaseItemTemplate));
                template.Parse();
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void Parse_InvalidMethods_ThatAreNotExplicitlyDeclared_AreSkipped()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(ObjectWithInvalidNonDeclaredMethods));
            template.Parse();
            template.ValidateOrThrow();

            // should just have the property
            Assert.AreEqual(1, template.FieldTemplates.Count);
        }

        [Test]
        public void Parse_PropertiesWithNoSetter_AreSkipped()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(ObjectWithNoSetters));
            template.Parse();
            template.ValidateOrThrow();

            // should just have the single property with a getter and setter
            Assert.AreEqual(1, template.FieldTemplates.Count);
        }

        [Test]
        public void Parse_NoPublicParameterlessConstructor_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new InputObjectGraphTypeTemplate(typeof(RequiredConstructor));
                template.Parse();
                template.ValidateOrThrow();
            });
        }
    }
}