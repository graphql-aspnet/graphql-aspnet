// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Defaults.TypeMakers
{
    using System.Linq;
    using GraphQL.AspNet.Defaults.TypeMakers;
    using GraphQL.AspNet.Tests.Defaults.TypeMakers.TestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    public class FieldMaker_InputFieldTests : GraphTypeMakerTestBase
    {
        [Test]
        public void Parse_NotRequiredValueTypePropertyCheck()
        {
            var server = new TestServerBuilder().Build();
            var template = TemplateHelper.CreateInputObjectTemplate<InputTestObject>();

            var fieldTemplate = template
                .FieldTemplates
                .Values
                .Single(x => x.Name == nameof(InputTestObject.NotRequiredValueTypeField));

            var graphField = new GraphFieldMaker(server.Schema).CreateField(fieldTemplate).Field;

            Assert.AreEqual("notRequiredValueTypeField", graphField.Name);
            Assert.IsFalse(graphField.IsRequired);

            // even though its not required, its still "Int!" because its a
            // non-nullable value type
            Assert.AreEqual("Int!", graphField.TypeExpression.ToString());
            Assert.AreEqual("[type]/Input_InputTestObject/NotRequiredValueTypeField", graphField.Route.ToString());
            Assert.AreEqual("NotRequiredValueTypeField", graphField.InternalName);
            Assert.AreEqual(typeof(int), graphField.DeclaredReturnType);

            Assert.AreEqual(1, graphField.AppliedDirectives.Count);
            Assert.AreEqual("DirectiveForNotRequiredValueTypeField", graphField.AppliedDirectives.First().DirectiveName);
        }

        [Test]
        public void Parse_RequiredValueTypePropertyCheck()
        {
            var server = new TestServerBuilder().Build();
            var template = TemplateHelper.CreateInputObjectTemplate<InputTestObject>();

            var fieldTemplate = template
                .FieldTemplates
                .Values
                .Single(x => x.Name == nameof(InputTestObject.RequiredValueTypeField));

            var graphField = new GraphFieldMaker(server.Schema).CreateField(fieldTemplate).Field;

            Assert.AreEqual("requiredValueTypeField", graphField.Name);
            Assert.IsTrue(graphField.IsRequired);

            Assert.AreEqual("Int!", graphField.TypeExpression.ToString());
            Assert.AreEqual("[type]/Input_InputTestObject/RequiredValueTypeField", graphField.Route.ToString());
            Assert.AreEqual("RequiredValueTypeField", graphField.InternalName);
            Assert.AreEqual(typeof(int), graphField.DeclaredReturnType);

            Assert.AreEqual(0, graphField.AppliedDirectives.Count);
        }

        [Test]
        public void Parse_NotRequiredReferenceTypePropertyCheck()
        {
            var server = new TestServerBuilder().Build();
            var template = TemplateHelper.CreateInputObjectTemplate<InputTestObject>();

            var fieldTemplate = template
                .FieldTemplates
                .Values
                .Single(x => x.Name == nameof(InputTestObject.NotRequiredReferenceTypeField));

            var graphField = new GraphFieldMaker(server.Schema).CreateField(fieldTemplate).Field;

            Assert.AreEqual("notRequiredReferenceTypeField", graphField.Name);
            Assert.IsFalse(graphField.IsRequired);

            // teh field is not required and the type is a reference type (which is nullable)
            // meaning the type expression should also be "nullable"
            Assert.AreEqual("Input_TwoPropertyObject", graphField.TypeExpression.ToString());
            Assert.AreEqual("[type]/Input_InputTestObject/NotRequiredReferenceTypeField", graphField.Route.ToString());
            Assert.AreEqual("NotRequiredReferenceTypeField", graphField.InternalName);
            Assert.AreEqual(typeof(TwoPropertyObject), graphField.DeclaredReturnType);

            Assert.AreEqual(0, graphField.AppliedDirectives.Count);
        }

        [Test]
        public void Parse_RequiredReferenceTypePropertyCheck()
        {
            var server = new TestServerBuilder().Build();
            var template = TemplateHelper.CreateInputObjectTemplate<InputTestObject>();

            var fieldTemplate = template
                .FieldTemplates
                .Values
                .Single(x => x.Name == nameof(InputTestObject.RequiredReferenceTypeField));

            var graphField = new GraphFieldMaker(server.Schema).CreateField(fieldTemplate).Field;

            Assert.AreEqual("requiredReferenceTypeField", graphField.Name);
            Assert.IsTrue(graphField.IsRequired);

            // because its marked as required, even though its a reference type (which is nullable)
            // the type expression is automatically hoisted to be "non-nullable"
            Assert.AreEqual("Input_TwoPropertyObject!", graphField.TypeExpression.ToString());
            Assert.AreEqual("[type]/Input_InputTestObject/RequiredReferenceTypeField", graphField.Route.ToString());
            Assert.AreEqual("RequiredReferenceTypeField", graphField.InternalName);
            Assert.AreEqual(typeof(TwoPropertyObject), graphField.DeclaredReturnType);

            Assert.AreEqual(0, graphField.AppliedDirectives.Count);
        }
    }
}