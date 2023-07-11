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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.AspNet.Tests.Internal.Templating.DirectiveTestData;
    using GraphQL.AspNet.Tests.Internal.Templating.ObjectTypeTests;
    using NUnit.Framework;

    [TestFixture]
    public class ObjectGraphTypeTemplateTests
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
        public void Parse_Object_GeneralPropertySettings_SetCorrectly()
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
        public void Parse_Struct_GeneralPropertySettings_SetCorrectly()
        {
            var template = new ObjectGraphTypeTemplate(typeof(SimpleStructNoMethods));
            template.Parse();
            template.ValidateOrThrow();

            Assert.IsNotNull(template);
            Assert.AreEqual("[type]/SimpleStructNoMethods", template.Route.Path);
            Assert.AreEqual(null, template.Description);
            Assert.AreEqual(typeof(SimpleStructNoMethods), template.ObjectType);
            Assert.AreEqual(0, template.FieldTemplates.Count());
            Assert.AreEqual("SimpleStructNoMethods", template.Name);
        }

        [Test]
        public void Parse_Object_DescriptionAttribute_SetsCorrectly()
        {
            var template = new ObjectGraphTypeTemplate(typeof(DescriptionObject));
            template.Parse();
            template.ValidateOrThrow();

            Assert.IsNotNull(template);
            Assert.AreEqual("A valid description", template.Description);
        }

        [Test]
        public void Parse_Struct_DescriptionAttribute_SetsCorrectly()
        {
            var template = new ObjectGraphTypeTemplate(typeof(DescriptionStruct));
            template.Parse();
            template.ValidateOrThrow();

            Assert.IsNotNull(template);
            Assert.AreEqual("A valid description struct", template.Description);
        }

        [Test]
        public void Parse_Object_MethodsAreCapturedAsExplictOrImplicitCorrectly()
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
        public void Parse_Struct_MethodsAreCapturedAsExplictOrImplicitCorrectly()
        {
            var template = new ObjectGraphTypeTemplate(typeof(StructOneMarkedMethod));
            template.Parse();
            template.ValidateOrThrow();

            var totalMethods = typeof(StructOneMarkedMethod)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).Count(x => !x.IsSpecialName);

            Assert.IsTrue(totalMethods == template.FieldTemplates.Count);
            Assert.AreEqual(1, template.FieldTemplates.Count(x => x.Value.IsExplicitDeclaration));
            Assert.AreEqual(1, template.FieldTemplates.Count(x => !x.Value.IsExplicitDeclaration));
        }

        [Test]
        public void Parse_Object_PropertiesAreCapturedAsExplictOrImplicitCorrectly()
        {
            var template = new ObjectGraphTypeTemplate(typeof(OneMarkedProperty));
            template.Parse();
            template.ValidateOrThrow();

            var totalProps = typeof(OneMarkedProperty).GetProperties().Length;

            Assert.IsTrue(totalProps == template.FieldTemplates.Count);
            Assert.AreEqual(1, template.FieldTemplates.Count(x => x.Value.IsExplicitDeclaration));
            Assert.AreEqual(1, template.FieldTemplates.Count(x => !x.Value.IsExplicitDeclaration));
        }

        public void Parse_Struct_PropertiesAreCapturedAsExplictOrImplicitCorrectly()
        {
            var template = new ObjectGraphTypeTemplate(typeof(StructOneMarkedProperty));
            template.Parse();
            template.ValidateOrThrow();

            var totalProps = typeof(StructOneMarkedProperty).GetProperties().Length;

            Assert.IsTrue(totalProps == template.FieldTemplates.Count);
            Assert.AreEqual(1, template.FieldTemplates.Count(x => x.Value.IsExplicitDeclaration));
            Assert.AreEqual(1, template.FieldTemplates.Count(x => !x.Value.IsExplicitDeclaration));
        }

        [Test]
        public void Parse_Object_OverloadedMethodsWithNoNameClash_ParsesCorrectly()
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
        public void Parse_Struct_OverloadedMethodsWithNoNameClash_ParsesCorrectly()
        {
            var template = new ObjectGraphTypeTemplate(typeof(StructTwoMethodsWithSameNameWithAttributeDiff));
            template.Parse();
            template.ValidateOrThrow();

            Assert.IsNotNull(template);

            Assert.AreEqual(2, template.FieldTemplates.Count());
            Assert.IsTrue(template.FieldTemplates.ContainsKey($"[type]/{nameof(StructTwoMethodsWithSameNameWithAttributeDiff)}/{nameof(TwoMethodsWithSameNameWithAttributeDiff.Method1)}"));
            Assert.IsTrue(template.FieldTemplates.ContainsKey($"[type]/{nameof(StructTwoMethodsWithSameNameWithAttributeDiff)}/MethodA"));
        }

        [Test]
        public void Parse_Object_OverloadedMethodsWithNameClash_ThrowsException()
        {
            var template = new ObjectGraphTypeTemplate(typeof(TwoMethodsWithSameName));
            template.Parse();

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void Parse_Struct_OverloadedMethodsWithNameClash_ThrowsException()
        {
            var template = new ObjectGraphTypeTemplate(typeof(StructTwoMethodsWithSameName));
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
                var template = new ObjectGraphTypeTemplate(typeof(SchemaItemCollections));
            });
        }

        [Test]
        public void Parse_FromKnownScalar_ThrowsException()
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
                var template = new ObjectGraphTypeTemplate(typeof(SchemaItemTemplateBase));
                template.Parse();
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void Parse_Object_WithClassLevelGraphSkipAttribute_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new ObjectGraphTypeTemplate(typeof(ForcedSkippedObject));
                template.Parse();
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void Parse_Struct_WithClassLevelGraphSkipAttribute_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new ObjectGraphTypeTemplate(typeof(ForceSkippedStruct));
                template.Parse();
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void Parse_Object_InvalidMethods_ThatAreNotExplicitlyDeclared_AreSkipped()
        {
            var template = new ObjectGraphTypeTemplate(typeof(ObjectWithInvalidNonDeclaredMethods));
            template.Parse();
            template.ValidateOrThrow();

            // should have the declared method, the undeclared but valid method, the decalred property
            // the invalid undeclared method should be dropped silently
            Assert.AreEqual(3, template.FieldTemplates.Count);
        }

        [Test]
        public void Parse_Struct_InvalidMethods_ThatAreNotExplicitlyDeclared_AreSkipped()
        {
            var template = new ObjectGraphTypeTemplate(typeof(StructWithInvalidNonDeclaredMethods));
            template.Parse();
            template.ValidateOrThrow();

            // should have the declared method, the undeclared but valid method, the decalred property
            // the invalid undeclared method should be dropped silently
            Assert.AreEqual(3, template.FieldTemplates.Count);
        }

        [Test]
        public void Parse_ArrayProperty_ExtractsListOfCoreType()
        {
            var template = new ObjectGraphTypeTemplate(typeof(TypeWithArrayProperty));
            template.Parse();
            template.ValidateOrThrow();

            // should have the declared method, the undeclared but valid method, the decalred property
            // the invalid undeclared method should be dropped silently
            Assert.AreEqual(1, template.FieldTemplates.Count);

            var expectedTypeExpression = new GraphTypeExpression(
                "Type",
                MetaGraphTypes.IsList);

            Assert.AreEqual(1, template.FieldTemplates.Count());
            var fieldTemplate = template.FieldTemplates.ElementAt(0).Value;
            Assert.AreEqual(typeof(TwoPropertyObject[]), fieldTemplate.DeclaredReturnType);
            Assert.AreEqual(typeof(TwoPropertyObject), fieldTemplate.ObjectType);
            Assert.AreEqual(expectedTypeExpression, fieldTemplate.TypeExpression);
        }

        [Test]
        public void Parse_KeyValuePair_GeneratesValidTemplate()
        {
            var template = new ObjectGraphTypeTemplate(typeof(KeyValuePair<string, int>));
            template.Parse();
            template.ValidateOrThrow();

            // Key, Value
            Assert.AreEqual(2, template.FieldTemplates.Count());
            var fieldTemplate0 = template.FieldTemplates.ElementAt(0).Value;
            var fieldTemplate1 = template.FieldTemplates.ElementAt(1).Value;

            Assert.AreEqual(typeof(string), fieldTemplate0.DeclaredReturnType);
            Assert.AreEqual(typeof(string), fieldTemplate0.ObjectType);
            Assert.AreEqual("Key", fieldTemplate0.Name);
            Assert.AreEqual(typeof(int), fieldTemplate1.ObjectType);
            Assert.AreEqual(typeof(int), fieldTemplate1.DeclaredReturnType);
            Assert.AreEqual("Value", fieldTemplate1.Name);
        }

        [Test]
        public void Parse_InheritedProperties_AreValidFields()
        {
            var template = new ObjectGraphTypeTemplate(typeof(InheritedTwoPropertyObject));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(3, template.FieldTemplates.Count());
            var fieldTemplate0 = template.FieldTemplates.ElementAt(0).Value;
            var fieldTemplate1 = template.FieldTemplates.ElementAt(1).Value;
            var fieldTemplate2 = template.FieldTemplates.ElementAt(2).Value;

            Assert.AreEqual("Property3", fieldTemplate0.Name);
            Assert.AreEqual("Property1", fieldTemplate1.Name);
            Assert.AreEqual("Property2", fieldTemplate2.Name);
        }

        [Test]
        public void Parse_AssignedDirective_IsTemplatized()
        {
            var template = new ObjectGraphTypeTemplate(typeof(ObjectWithDirective));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(1, template.AppliedDirectives.Count());

            var appliedDirective = template.AppliedDirectives.First();
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            Assert.AreEqual(new object[] { 1, "object arg" }, appliedDirective.Arguments);
        }

        [Test]
        public void Parse_DefinedMethodsAreMethodsAreIgnored()
        {
            var template = new ObjectGraphTypeTemplate(typeof(ObjectWithDeconstructor));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(1, template.FieldTemplates.Count);
            Assert.AreEqual(nameof(ObjectWithDeconstructor.Property1), template.FieldTemplates.First().Value.Name);
        }

        [Test]
        public void Parse_ExplicitInheritedMethodBasedField_IsSeenAsAGraphField()
        {
            var template = new ObjectGraphTypeTemplate(typeof(ObjectThatInheritsExplicitMethodField));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(2, template.FieldTemplates.Count);
            Assert.IsTrue(template.FieldTemplates.Any(x => x.Value.InternalName == nameof(ObjectThatInheritsExplicitMethodField.FieldOnObject)));
            Assert.IsTrue(template.FieldTemplates.Any(x => x.Value.InternalName == nameof(ObjectWithExplicitMethodField.FieldOnBaseObject)));
        }

        [Test]
        public void Parse_NonExplicitMethodBasedField_IsSeenAsTemplatefield()
        {
            var template = new ObjectGraphTypeTemplate(typeof(ObjectThatInheritsNonExplicitMethodField));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(2, template.FieldTemplates.Count);
            Assert.IsTrue(template.FieldTemplates.Any(x => x.Value.InternalName == nameof(ObjectThatInheritsNonExplicitMethodField.FieldOnObject)));
            Assert.IsTrue(template.FieldTemplates.Any(x => x.Value.InternalName == nameof(ObjectWithNonExplicitMethodField.FieldOnBaseObject)));
        }

        [Test]
        public void Parse_StaticMethodsWithProperGraphFieldDeclarations_AreSkipped()
        {
            var template = new ObjectGraphTypeTemplate(typeof(ObjectWithStatics));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(2, template.FieldTemplates.Count());

            Assert.IsNotNull(template.FieldTemplates.Values.SingleOrDefault(x => x.Route.Name == nameof(ObjectWithStatics.InstanceProperty)));
            Assert.IsNotNull(template.FieldTemplates.Values.SingleOrDefault(x => x.Route.Name == nameof(ObjectWithStatics.InstanceMethod)));
        }
    }
}