// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.DirectiveTestData;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.InterfaceTestData;
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
            Assert.AreEqual(2, Enumerable.Count<KeyValuePair<string, IGraphFieldTemplate>>(template.FieldTemplates));
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

            Assert.AreEqual(1, Enumerable.Count<IAppliedDirectiveTemplate>(template.AppliedDirectives));

            var appliedDirective = Enumerable.First<IAppliedDirectiveTemplate>(template.AppliedDirectives);
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
            Assert.AreEqual(5, Enumerable.Count<Type>(template.DeclaredInterfaces));

            Assert.IsTrue(Enumerable.Contains(template.DeclaredInterfaces, typeof(IInterface1)));
            Assert.IsTrue(Enumerable.Contains(template.DeclaredInterfaces, typeof(IInterface2)));
            Assert.IsTrue(Enumerable.Contains(template.DeclaredInterfaces, typeof(IInterface3)));
            Assert.IsTrue(Enumerable.Contains(template.DeclaredInterfaces, typeof(INestedInterface1)));
            Assert.IsTrue(Enumerable.Contains(template.DeclaredInterfaces, typeof(INestedInterface2)));
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
            Assert.IsTrue(Enumerable.Any<KeyValuePair<string, IGraphFieldTemplate>>(template.FieldTemplates, x => string.Equals(x.Value.Name, nameof(InterfaceThatInheritsUndeclaredMethodField.PropFieldOnInterface), System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(Enumerable.Any<KeyValuePair<string, IGraphFieldTemplate>>(template.FieldTemplates, x => string.Equals(x.Value.Name, nameof(InterfaceThatInheritsUndeclaredMethodField.MethodFieldOnInterface), System.StringComparison.OrdinalIgnoreCase)));
            Assert.AreEqual(1, Enumerable.Count<Type>(template.DeclaredInterfaces));
            Assert.IsTrue(Enumerable.Contains(template.DeclaredInterfaces, typeof(InterfaceWithUndeclaredInterfaceField)));
        }

        [Test]
        public void Parse_InheritedDeclaredMethodField_IsNotIncluded()
        {
            var template = new InterfaceGraphTypeTemplate(typeof(InterfaceThatInheritsDeclaredMethodField));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(2, template.FieldTemplates.Count);
            Assert.IsTrue(Enumerable.Any<KeyValuePair<string, IGraphFieldTemplate>>(template.FieldTemplates, x => string.Equals(x.Value.Name, nameof(InterfaceThatInheritsDeclaredMethodField.PropFieldOnInterface), System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(Enumerable.Any<KeyValuePair<string, IGraphFieldTemplate>>(template.FieldTemplates, x => string.Equals(x.Value.Name, nameof(InterfaceThatInheritsDeclaredMethodField.MethodFieldOnInterface), System.StringComparison.OrdinalIgnoreCase)));

            Assert.AreEqual(1, Enumerable.Count<Type>(template.DeclaredInterfaces));
            Assert.IsTrue(Enumerable.Contains(template.DeclaredInterfaces, typeof(InterfaceWithDeclaredInterfaceField)));
        }
    }
}