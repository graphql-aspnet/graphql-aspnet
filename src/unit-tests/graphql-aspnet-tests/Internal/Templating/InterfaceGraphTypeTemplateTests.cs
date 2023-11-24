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
    public class InterfaceGraphTypeTemplateTests
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
                var template = new InterfaceGraphTypeTemplate(typeof(InterfaceGraphTypeTemplateTests));
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
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            Assert.AreEqual(new object[] { 8, "big face" }, appliedDirective.Arguments);
        }

        [Test]
        public void Parse_ImplementedInterfaces_AreCaptured()
        {
            var template = new InterfaceGraphTypeTemplate(typeof(ITestableInterfaceImplementation));
            template.Parse();
            template.ValidateOrThrow();

            Assert.IsNotNull(template);
            Assert.AreEqual("[type]/ITestableInterfaceImplementation", template.Route.Path);
            Assert.AreEqual(typeof(ITestableInterfaceImplementation), template.ObjectType);
            Assert.AreEqual(5, template.DeclaredInterfaces.Count());

            Assert.IsTrue(template.DeclaredInterfaces.Contains(typeof(IInterface1)));
            Assert.IsTrue(template.DeclaredInterfaces.Contains(typeof(IInterface2)));
            Assert.IsTrue(template.DeclaredInterfaces.Contains(typeof(IInterface3)));
            Assert.IsTrue(template.DeclaredInterfaces.Contains(typeof(INestedInterface1)));
            Assert.IsTrue(template.DeclaredInterfaces.Contains(typeof(INestedInterface2)));
        }

        [Test]
        public void Parse_InheritedUndeclaredMethodField_IsNotIncluded()
        {
            var template = new InterfaceGraphTypeTemplate(typeof(InterfaceThatInheritsUndeclaredMethodField));
            template.Parse();
            template.ValidateOrThrow();

            // PropFieldOnInterface, MethodFieldOnInterface
            // base items are ignored
            Assert.AreEqual(2, template.FieldTemplates.Count);
            Assert.IsTrue(template.FieldTemplates.Any(x => string.Equals(x.Name, nameof(InterfaceThatInheritsUndeclaredMethodField.PropFieldOnInterface), System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(template.FieldTemplates.Any(x => string.Equals(x.Name, nameof(InterfaceThatInheritsUndeclaredMethodField.MethodFieldOnInterface), System.StringComparison.OrdinalIgnoreCase)));
            Assert.AreEqual(1, template.DeclaredInterfaces.Count());
            Assert.IsTrue(template.DeclaredInterfaces.Contains(typeof(InterfaceWithUndeclaredInterfaceField)));
        }

        [Test]
        public void Parse_InheritedDeclaredMethodField_IsNotIncluded()
        {
            var template = new InterfaceGraphTypeTemplate(typeof(InterfaceThatInheritsDeclaredMethodField));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(2, template.FieldTemplates.Count);
            Assert.IsTrue(template.FieldTemplates.Any(x => string.Equals(x.Name, nameof(InterfaceThatInheritsDeclaredMethodField.PropFieldOnInterface), System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(template.FieldTemplates.Any(x => string.Equals(x.Name, nameof(InterfaceThatInheritsDeclaredMethodField.MethodFieldOnInterface), System.StringComparison.OrdinalIgnoreCase)));

            Assert.AreEqual(1, template.DeclaredInterfaces.Count());
            Assert.IsTrue(template.DeclaredInterfaces.Contains(typeof(InterfaceWithDeclaredInterfaceField)));
        }

        [Test]
        public void Parse_InterfaceWithMethodOverloads_ShouldParseBothAndNotFail()
        {
            var template = new InterfaceGraphTypeTemplate(typeof(IInterfaceWithOverloads));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(2, template.FieldTemplates.Count);
            Assert.IsTrue(template.FieldTemplates.All(x => x.Name == nameof(IInterfaceWithOverloads.Method1)));
        }
    }
}