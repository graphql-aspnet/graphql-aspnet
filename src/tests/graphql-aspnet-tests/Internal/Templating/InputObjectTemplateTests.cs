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
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Tests.Internal.Templating.ObjectTypeTests;
    using NUnit.Framework;

    [TestFixture]
    public class InputObjectTemplateTests
    {
        [Test]
        public void UnparsedTemplate_Object_ThrowsException()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(SimpleObjectNoMethods));

            Assert.Throws<InvalidOperationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void UnparsedTemplate_Struct_ThrowsException()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(SimpleStructNoMethods));

            Assert.Throws<InvalidOperationException>(() =>
            {
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void Parse_Object_GeneralPropertySettings_SetCorrectly()
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
        public void Parse_Struct_GeneralPropertySettings_SetCorrectly()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(SimpleStructNoMethods));
            template.Parse();
            template.ValidateOrThrow();

            Assert.IsNotNull(template);
            Assert.AreEqual("[type]/Input_SimpleStructNoMethods", template.Route.Path);
            Assert.AreEqual(null, template.Description);
            Assert.AreEqual(typeof(SimpleStructNoMethods), template.ObjectType);
            Assert.AreEqual(0, template.FieldTemplates.Count());
        }

        [Test]
        public void Parse_Object_DescriptionAttribute_SetsCorrectly()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(DescriptionObject));
            template.Parse();
            template.ValidateOrThrow();

            Assert.IsNotNull(template);
            Assert.AreEqual("A valid description", template.Description);
        }

        [Test]
        public void Parse_Struct_DescriptionAttribute_SetsCorrectly()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(DescriptionStruct));
            template.Parse();
            template.ValidateOrThrow();

            Assert.IsNotNull(template);
            Assert.AreEqual("A valid description struct", template.Description);
        }

        [Test]
        public void Parse_Object_MethodsAreCapturedAsExplictOrImplicitCorrectly()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(OneMarkedMethod));
            template.Parse();
            template.ValidateOrThrow();

            // an input object should parse no methods, ever
            Assert.AreEqual(0, template.FieldTemplates.Count);
        }

        [Test]
        public void Parse_Struct_MethodsAreCapturedAsExplictOrImplicitCorrectly()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(StructOneMarkedMethod));
            template.Parse();
            template.ValidateOrThrow();

            // an input object should parse no methods, ever
            Assert.AreEqual(0, template.FieldTemplates.Count);
        }

        [Test]
        public void Parse_Object_PropertiesAreCapturedAsExplictOrImplicitCorrectly()
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
        public void Parse_Struct_PropertiesAreCapturedAsExplictOrImplicitCorrectly()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(StructOneMarkedProperty));
            template.Parse();
            template.ValidateOrThrow();

            var totalProps = typeof(OneMarkedProperty).GetProperties().Length;

            Assert.IsTrue(totalProps == template.FieldTemplates.Count);
            Assert.AreEqual(1, template.FieldTemplates.Count(x => x.Value.IsExplicitDeclaration));
            Assert.AreEqual(1, template.FieldTemplates.Count(x => !x.Value.IsExplicitDeclaration));
        }

        [Test]
        public void Parse_Object_OverloadedMethodsWithNameClash_SkipsCorrectly()
        {
            // since methods should be skipped for INPUT_OBJECT templates
            // this should parse with no exceptions.
            var template = new InputObjectGraphTypeTemplate(typeof(TwoMethodsWithSameName));
            template.Parse();
            template.ValidateOrThrow();
        }

        [Test]
        public void Parse_Struct_OverloadedMethodsWithNameClash_SkipsCorrectly()
        {
            // since methods should be skipped for INPUT_OBJECT templates
            // this should parse with no exceptions.
            var template = new InputObjectGraphTypeTemplate(typeof(StructTwoMethodsWithSameName));
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
        public void Parse_FromKnownScalar_ThrowsException()
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
        public void Parse_Object_InvalidMethods_ThatAreNotExplicitlyDeclared_AreSkipped()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(ObjectWithInvalidNonDeclaredMethods));
            template.Parse();
            template.ValidateOrThrow();

            // should just have the property
            Assert.AreEqual(1, template.FieldTemplates.Count);
        }

        [Test]
        public void Parse_Struct_InvalidMethods_ThatAreNotExplicitlyDeclared_AreSkipped()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(StructWithInvalidNonDeclaredMethods));
            template.Parse();
            template.ValidateOrThrow();

            // should just have the property
            Assert.AreEqual(1, template.FieldTemplates.Count);
        }

        [Test]
        public void Parse_Object_PropertiesWithNoSetter_AreSkipped()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(ObjectWithNoSetters));
            template.Parse();
            template.ValidateOrThrow();

            // should just have the single property with a getter and setter
            Assert.AreEqual(1, template.FieldTemplates.Count);
        }

        [Test]
        public void Parse_Struct_PropertiesWithNoSetter_AreSkipped()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(StructWithNoSetters));
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

        [Test]
        public void Parse_Struct_FieldsAreFound()
        {
            var template = new InputObjectGraphTypeTemplate(typeof(CustomStruct));
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual(1, template.FieldTemplates.Count);

            var item = template.FieldTemplates.Values.ElementAt(0);
            Assert.AreEqual("Prop1", item.Name);
        }

        [Test]
        public void Parse_KeyValuePair_ExceptionIsThrown()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new InputObjectGraphTypeTemplate(typeof(KeyValuePair<string, int>));
                template.Parse();
                template.ValidateOrThrow();
            });
        }

        [Test]
        public void Interface_InputTemplate_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = new InputObjectGraphTypeTemplate(typeof(IInputObject));
                template.Parse();
                template.ValidateOrThrow();
            });
        }
    }
}