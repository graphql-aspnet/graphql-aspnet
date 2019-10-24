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
    using System.Reflection;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Tests.Internal.Templating.ObjectTypeTests;
    using NUnit.Framework;

    [TestFixture]
    public class GraphObjectTemplateTests
    {
        [Test]
        public void UnparsedTemplate_ThrowsException()
        {
            var template = new ObjectGraphTypeTemplate(typeof(SimpleObjectNoMethods));

            Assert.Throws<InvalidOperationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void Parse_GeneralPropertySettings_SetCorrectly()
        {
            var template = new ObjectGraphTypeTemplate(typeof(SimpleObjectNoMethods));
            template.Parse();
            template.ValidateOrThrow();

            Assert.IsNotNull(template);
            Assert.AreEqual("[type]/SimpleObjectNoMethods", template.Route.Path);
            Assert.AreEqual(null, template.Description);
            Assert.AreEqual(typeof(SimpleObjectNoMethods), template.ObjectType);
            Assert.AreEqual(0, template.FieldTemplates.Count());
            Assert.AreEqual("SimpleObjectNoMethods", template.Name);
        }

        [Test]
        public void Parse_DescriptionAttribute_SetsCorrectly()
        {
            var template = new ObjectGraphTypeTemplate(typeof(DescriptionObject));
            template.Parse();
            template.ValidateOrThrow();

            Assert.IsNotNull(template);
            Assert.AreEqual("A valid description", template.Description);
        }

        [Test]
        public void Parse_MethodsAreCapturedAsExplictOrImplicitCorrectly()
        {
            var template = new ObjectGraphTypeTemplate(typeof(OneMarkedMethod));
            template.Parse();
            template.ValidateOrThrow();

            var totalMethods = typeof(OneMarkedMethod)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).Count(x => !x.IsSpecialName);

            Assert.IsTrue(totalMethods == template.FieldTemplates.Count);
            Assert.AreEqual(1, template.FieldTemplates.Count(x => x.Value.IsExplicitDeclaration));
            Assert.AreEqual(1, template.FieldTemplates.Count(x => !x.Value.IsExplicitDeclaration));
        }

        [Test]
        public void Parse_PropertiesAreCapturedAsExplictOrImplicitCorrectly()
        {
            var template = new ObjectGraphTypeTemplate(typeof(OneMarkedProperty));
            template.Parse();
            template.ValidateOrThrow();

            var totalProps = typeof(OneMarkedProperty).GetProperties().Length;

            Assert.IsTrue(totalProps == template.FieldTemplates.Count);
            Assert.AreEqual(1, template.FieldTemplates.Count(x => x.Value.IsExplicitDeclaration));
            Assert.AreEqual(1, template.FieldTemplates.Count(x => !x.Value.IsExplicitDeclaration));
        }

        [Test]
        public void Parse_OverloadedMethodsWithNoNameClash_ParsesCorrectly()
        {
            var template = new ObjectGraphTypeTemplate(typeof(TwoMethodsWithSameNameWithAttributeDiff));
            template.Parse();
            template.ValidateOrThrow();

            Assert.IsNotNull(template);

            Assert.AreEqual(2, template.FieldTemplates.Count());
            Assert.IsTrue(template.FieldTemplates.ContainsKey($"[type]/{nameof(TwoMethodsWithSameNameWithAttributeDiff)}/{nameof(TwoMethodsWithSameNameWithAttributeDiff.Method1)}"));
            Assert.IsTrue(template.FieldTemplates.ContainsKey($"[type]/{nameof(TwoMethodsWithSameNameWithAttributeDiff)}/MethodA"));
        }

        [Test]
        public void Parse_OverloadedMethodsWithNameClash_ThrowsException()
        {
            var template = new ObjectGraphTypeTemplate(typeof(TwoMethodsWithSameName));
            template.Parse();

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void Parse_FromAnEnum_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new ObjectGraphTypeTemplate(typeof(GraphCollection));
            });
        }

        [Test]
        public void Parse_FromValueType_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new ObjectGraphTypeTemplate(typeof(int));
            });
        }

        [Test]
        public void Parse_FromString_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new ObjectGraphTypeTemplate(typeof(string));
            });
        }

        [Test]
        public void Parse_FromAbstractClass_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new ObjectGraphTypeTemplate(typeof(BaseItemTemplate));
                template.Parse();
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void Parse_WithClassLevelGraphSkipAttribute_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new ObjectGraphTypeTemplate(typeof(ForcedSkippedObject));
                template.Parse();
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void Parse_InvalidMethods_ThatAreNotExplicitlyDeclared_AreSkipped()
        {
            var template = new ObjectGraphTypeTemplate(typeof(ObjectWithInvalidNonDeclaredMethods));
            template.Parse();
            template.ValidateOrThrow();

            // should have the declared method, the undeclared but valid method, the decalred property
            // the invalid undeclared method should be dropped silently
            Assert.AreEqual(3, template.FieldTemplates.Count);
        }
    }
}