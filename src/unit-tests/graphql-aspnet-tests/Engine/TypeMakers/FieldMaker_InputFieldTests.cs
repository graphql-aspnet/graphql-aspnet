// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine.TypeMakers
{
    using System.Linq;
    using GraphQL.AspNet.Engine.TypeMakers;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Generation;
    using GraphQL.AspNet.Schemas.Generation.TypeMakers;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Engine.TypeMakers.TestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    public class FieldMaker_InputFieldTests : GraphTypeMakerTestBase
    {
        [Test]
        public void Parse_NotRequiredValueTypePropertyCheck()
        {
            var server = new TestServerBuilder().Build();
            var template = GraphQLTemplateHelper.CreateInputObjectTemplate<InputTestObject>();

            var fieldTemplate = template
                .FieldTemplates
                .Values
                .Single(x => x.Name == nameof(InputTestObject.NotRequiredValueTypeField));

            var factory = server.CreateMakerFactory();

            var graphField = new GraphFieldMaker(server.Schema, factory).CreateField(fieldTemplate).Field;

            Assert.AreEqual("notRequiredValueTypeField", graphField.Name);
            Assert.IsFalse((bool)graphField.IsRequired);

            // even though its not required, its still "Int!" because its a
            // non-nullable value type
            Assert.AreEqual("Int!", graphField.TypeExpression.ToString());
            Assert.AreEqual("[type]/Input_InputTestObject/NotRequiredValueTypeField", graphField.Route.ToString());
            Assert.AreEqual("NotRequiredValueTypeField", graphField.InternalFullName);
            Assert.AreEqual(typeof(int), graphField.DeclaredReturnType);

            Assert.AreEqual(1, graphField.AppliedDirectives.Count);
            Assert.AreEqual("DirectiveForNotRequiredValueTypeField", Enumerable.First(graphField.AppliedDirectives).DirectiveName);
        }

        [Test]
        public void Parse_RequiredValueTypePropertyCheck()
        {
            var server = new TestServerBuilder().Build();
            var template = GraphQLTemplateHelper.CreateInputObjectTemplate<InputTestObject>();

            var fieldTemplate = template
                .FieldTemplates
                .Values
                .Single(x => x.Name == nameof(InputTestObject.RequiredValueTypeField));

            var factory = server.CreateMakerFactory();

            var graphField = new GraphFieldMaker(server.Schema, factory).CreateField(fieldTemplate).Field;

            Assert.AreEqual("requiredValueTypeField", graphField.Name);
            Assert.IsTrue((bool)graphField.IsRequired);

            Assert.AreEqual("Int!", graphField.TypeExpression.ToString());
            Assert.AreEqual("[type]/Input_InputTestObject/RequiredValueTypeField", graphField.Route.ToString());
            Assert.AreEqual("RequiredValueTypeField", graphField.InternalFullName);
            Assert.AreEqual(typeof(int), graphField.DeclaredReturnType);

            Assert.AreEqual(0, graphField.AppliedDirectives.Count);
        }

        [Test]
        public void Parse_NotRequiredReferenceTypePropertyCheck()
        {
            var server = new TestServerBuilder().Build();
            var template = GraphQLTemplateHelper.CreateInputObjectTemplate<InputTestObject>();

            var fieldTemplate = template
                .FieldTemplates
                .Values
                .Single(x => x.Name == nameof(InputTestObject.NotRequiredReferenceTypeField));

            var factory = server.CreateMakerFactory();

            var graphField = new GraphFieldMaker(server.Schema, factory).CreateField(fieldTemplate).Field;

            Assert.AreEqual("notRequiredReferenceTypeField", graphField.Name);
            Assert.IsFalse((bool)graphField.IsRequired);

            // teh field is not required and the type is a reference type (which is nullable)
            // meaning the type expression should also be "nullable"
            Assert.AreEqual("Input_TwoPropertyObject", graphField.TypeExpression.ToString());
            Assert.AreEqual("[type]/Input_InputTestObject/NotRequiredReferenceTypeField", graphField.Route.ToString());
            Assert.AreEqual("NotRequiredReferenceTypeField", graphField.InternalFullName);
            Assert.AreEqual(typeof(TwoPropertyObject), graphField.DeclaredReturnType);

            Assert.AreEqual(0, graphField.AppliedDirectives.Count);
        }

        [Test]
        public void Parse_RequiredReferenceTypePropertyCheck()
        {
            var server = new TestServerBuilder().Build();
            var template = GraphQLTemplateHelper.CreateInputObjectTemplate<InputTestObject>();

            var fieldTemplate = template
                .FieldTemplates
                .Values
                .Single(x => x.Name == nameof(InputTestObject.RequiredReferenceTypeField));

            var factory = server.CreateMakerFactory();

            var graphField = new GraphFieldMaker(server.Schema, factory).CreateField(fieldTemplate).Field;

            Assert.AreEqual("requiredReferenceTypeField", graphField.Name);

            // a nullable type expression can never be "required"
            Assert.IsFalse((bool)graphField.IsRequired);

            // because its marked as required, even though its a reference type (which is nullable)
            // the type expression is automatically hoisted to be "non-nullable"
            Assert.AreEqual("Input_TwoPropertyObject", graphField.TypeExpression.ToString());
            Assert.AreEqual("[type]/Input_InputTestObject/RequiredReferenceTypeField", graphField.Route.ToString());
            Assert.AreEqual("RequiredReferenceTypeField", graphField.InternalFullName);
            Assert.AreEqual(typeof(TwoPropertyObject), graphField.DeclaredReturnType);

            Assert.AreEqual(0, graphField.AppliedDirectives.Count);
        }

        [Test]
        public void Parse_RequiredNonNullableReferenceTypePropertyCheck()
        {
            var server = new TestServerBuilder().Build();
            var template = GraphQLTemplateHelper.CreateInputObjectTemplate<InputTestObject>();

            var fieldTemplate = template
                .FieldTemplates
                .Values
                .Single(x => x.Name == nameof(InputTestObject.RequiredReferenceExplicitNonNullTypeField));

            var factory = server.CreateMakerFactory();
            var graphField = new GraphFieldMaker(server.Schema, factory).CreateField(fieldTemplate).Field;

            Assert.AreEqual("requiredReferenceExplicitNonNullTypeField", graphField.Name);
            Assert.IsTrue((bool)graphField.IsRequired);

            // because its marked as required, even though its a reference type (which is nullable)
            // the type expression is automatically hoisted to be "non-nullable"
            Assert.AreEqual("Input_TwoPropertyObject!", graphField.TypeExpression.ToString());
            Assert.AreEqual("[type]/Input_InputTestObject/RequiredReferenceExplicitNonNullTypeField", graphField.Route.ToString());
            Assert.AreEqual("RequiredReferenceExplicitNonNullTypeField", graphField.InternalFullName);
            Assert.AreEqual(typeof(TwoPropertyObject), graphField.DeclaredReturnType);

            Assert.AreEqual(0, graphField.AppliedDirectives.Count);
        }

        [Test]
        public void Parse_RequiredGraphIdPropertyCheck()
        {
            var server = new TestServerBuilder().Build();
            var template = GraphQLTemplateHelper.CreateInputObjectTemplate<InputTestObject>();

            var fieldTemplate = template
                .FieldTemplates
                .Values
                .Single(x => x.Name == nameof(InputTestObject.GraphIdRequired));

            var factory = server.CreateMakerFactory();

            var graphField = new GraphFieldMaker(server.Schema, factory).CreateField(fieldTemplate).Field;

            Assert.AreEqual("graphIdRequired", graphField.Name);
            Assert.IsTrue((bool)graphField.IsRequired);

            // scalar is required by default
            Assert.AreEqual("ID!", graphField.TypeExpression.ToString());
            Assert.AreEqual("[type]/Input_InputTestObject/GraphIdRequired", graphField.Route.ToString());
            Assert.AreEqual("GraphIdRequired", graphField.InternalFullName);
            Assert.AreEqual(typeof(GraphId), graphField.DeclaredReturnType);

            Assert.AreEqual(0, graphField.AppliedDirectives.Count);
        }

        [Test]
        public void Parse_NotRequiredGraphIdPropertyCheck()
        {
            var server = new TestServerBuilder().Build();
            var template = GraphQLTemplateHelper.CreateInputObjectTemplate<InputTestObject>();

            var fieldTemplate = template
                .FieldTemplates
                .Values
                .Single(x => x.Name == nameof(InputTestObject.GraphIdNotRequired));

            var factory = server.CreateMakerFactory();

            var graphField = new GraphFieldMaker(server.Schema, factory).CreateField(fieldTemplate).Field;

            Assert.AreEqual("graphIdNotRequired", graphField.Name);
            Assert.IsFalse((bool)graphField.IsRequired);

            Assert.AreEqual("ID!", graphField.TypeExpression.ToString());
            Assert.AreEqual("[type]/Input_InputTestObject/GraphIdNotRequired", graphField.Route.ToString());
            Assert.AreEqual("GraphIdNotRequired", graphField.InternalFullName);
            Assert.AreEqual(typeof(GraphId), graphField.DeclaredReturnType);

            Assert.AreEqual(0, graphField.AppliedDirectives.Count);
        }

        [Test]
        public void Parse_NotRequiredNullableGraphIdPropertyCheck()
        {
            var server = new TestServerBuilder().Build();
            var template = GraphQLTemplateHelper.CreateInputObjectTemplate<InputTestObject>();

            var fieldTemplate = template
                .FieldTemplates
                .Values
                .Single(x => x.Name == nameof(InputTestObject.GraphIdNullable));

            var factory = server.CreateMakerFactory();

            var graphField = new GraphFieldMaker(server.Schema, factory).CreateField(fieldTemplate).Field;

            Assert.AreEqual("graphIdNullable", graphField.Name);
            Assert.IsFalse((bool)graphField.IsRequired);

            Assert.AreEqual("ID", graphField.TypeExpression.ToString());
            Assert.AreEqual("[type]/Input_InputTestObject/GraphIdNullable", graphField.Route.ToString());
            Assert.AreEqual("GraphIdNullable", graphField.InternalFullName);
            Assert.AreEqual(typeof(GraphId?), graphField.DeclaredReturnType);

            Assert.AreEqual(0, graphField.AppliedDirectives.Count);
        }
    }
}